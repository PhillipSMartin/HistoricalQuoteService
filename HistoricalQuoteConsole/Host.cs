using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Gargoyle.Common;
using Gargoyle.Utils.DBAccess;
using GargoyleTaskLib;
using HistoricalQuoteServiceLib;
using log4net;
using TWSLib;

namespace HistoricalQuoteConsole
{
    public class Host : IDisposable
    {
        private bool m_bTaskStarted;
        private bool m_bTaskFailed;
        private bool m_bWaiting;
        private TimeSpan m_stopTime;
        private string m_lastErrorMessage;
        private CommandLineParameters m_parms;
        private System.Data.SqlClient.SqlConnection m_hugoConnection;
 
        private ILog m_logger = LogManager.GetLogger(typeof(Host));
        private AutoResetEvent m_waitHandle = new AutoResetEvent(false);

        public Host(CommandLineParameters parms)
        {
            m_parms = parms;
        }

        public bool Run()
        {
            m_bTaskFailed = true;
            try
            {
                // initialize logging
                log4net.Config.XmlConfigurator.Configure();
  
                if (Initialize())
                {
                    using (var serviceHost = new ServiceHost(typeof(HistoricalQuoteService)))
                    {

                        var dataProvider = QuoteProviderFactory.GetProvider(m_parms.Provider, this, m_parms.QuoteServerHost, m_parms.GetQuoteServerPort);
                        if (dataProvider == null)
                        {
                            OnInfo(String.Format("Unable to instantiate provider {0}", m_parms.Provider), true);
                        }
                        else
                        {
                            HistoricalQuoteService.Provider = dataProvider;
                            serviceHost.Open();
                            OnInfo("Host started");

                            m_bTaskFailed = false;  // set up was successful - we now wait for the timer to expire or for a post from an event handler
                            m_bWaiting = true;

                            if (m_parms.StopTimeIsSpecified)
                            {
                                bool timedOut = WaitForCompletion(m_stopTime);
                            }
                            else
                            {
                                Console.WriteLine();
                                Console.WriteLine("Press any key to stop HistoricalQuoteService.");
                                Console.ReadKey();
                            }
                            m_bWaiting = false;

                            serviceHost.Close();
                            OnInfo("Host closed");
                            m_bTaskFailed = false;
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                m_bTaskFailed = true;
                OnError("Error in Run method", ex, true);
            }
            finally
            {
                if (m_bTaskStarted)
                {
                    m_bTaskStarted = !EndTask(m_parms.TaskName, !m_bTaskFailed);
                }
            }

            return !m_bTaskFailed;
        }

 
        // returns true if terminated because we reached stopTime, false if terminated early
        private bool WaitForCompletion(TimeSpan stopTime)
        {
            DateTime stopDateTime = DateTime.Today + stopTime;

            int tickTime = (int)(stopDateTime - DateTime.Now).TotalMilliseconds;
            if (tickTime <= 900000)
                tickTime = 900000;  // make sure stop time is at least 15 minutes from now

            OnInfo(String.Format("Waiting for {0} ms", tickTime));
            if (WaitAny(tickTime, m_waitHandle))
            {
                OnInfo("Job terminated early");
                return false;
            }
            else
            {
                OnInfo("Job terminated on schedule");
                return true;
            }
        }

        public bool WaitAny(int? millisecondsTimeout, params System.Threading.WaitHandle[] successConditionHandles)
        {
            int n;
            if (!millisecondsTimeout.HasValue)
                millisecondsTimeout = m_parms.Timeout;

            if (millisecondsTimeout == 0)
                n = System.Threading.WaitHandle.WaitAny(successConditionHandles);
            else
                n = System.Threading.WaitHandle.WaitAny(successConditionHandles, millisecondsTimeout.Value);
            if (n == System.Threading.WaitHandle.WaitTimeout)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        private bool Initialize()
        {
            if (m_parms.StopTimeIsSpecified) 
            {
                if (!m_parms.GetStopTime(out m_stopTime))
                {
                    OnInfo("Invalid stop time specified", true);
                    return false;
                }
            }

            DBAccess dbAccess = DBAccess.GetDBAccessOfTheCurrentUser(m_parms.ProgramName);
            m_hugoConnection = dbAccess.GetConnection("Hugo");
            if (m_hugoConnection == null)
            {
                OnInfo("Unable to connect to Hugo", true);
                return false;
            }

            if (!String.IsNullOrEmpty(m_parms.TaskName.Trim()))
            {
                switch (StartTask(m_parms.TaskName))
                {
                    case 0:
                        m_bTaskStarted = true;
                        break;

                    case 1:
                        m_bTaskStarted = false;
                        OnInfo("Task not started because it is a holiday");
                        return false;

                    default:
                        // if task wasn't started (which probably means taskname was not in the table), so be it - no need to abort
                        m_bTaskStarted = false;
                        break;
                }
            }

            return true;
        }

        #region Task Management
        // returns 0 if task is successfully started
        // returns 1 if we should not start the task because it should not be run today (because this task should not run on a holiday, for example)
        // returns 4 if we cannot find the task name
        // returns some number higher than 4 on an unexpected failure
        private int StartTask(string taskName)
        {
            try
            {
                using (TaskUtilities taskUtilities = new TaskUtilities(m_hugoConnection, m_parms.Timeout))
                {
                    return taskUtilities.TaskStarted(taskName, null);
                }
            }
            catch (Exception ex)
            {
                OnError("Unable to start task", ex);
                return 16;
            }
        }

        private bool EndTask(string taskName, bool succeeded)
        {
            try
            {
                using (TaskUtilities taskUtilities = new TaskUtilities(m_hugoConnection, m_parms.Timeout))
                {
                    if (succeeded)
                        return (0 != taskUtilities.TaskCompleted(taskName, ""));
                    else
                        return (0 != taskUtilities.TaskFailed(taskName, m_lastErrorMessage));
                }
            }
            catch (Exception ex)
            {
                OnError("Unable to stop task", ex);
                return false;
            }
        }
        #endregion

        #region Logging
        public void Utilities_OnError(object sender, LoggingEventArgs e)
        {
            OnError(e.Message, e.Exception);
        }

        public void Utilities_OnInfo(object sender, LoggingEventArgs e)
        {
            OnInfo(e.Message);
        }
        public void OnInfo(string msg, bool updateLastErrorMsg = false)
        {
             if (updateLastErrorMsg)
                m_lastErrorMessage = msg;
             if (m_parms.Console)
             {
                 Console.WriteLine(msg);
             }
             if (m_logger != null)
            {
                lock (m_logger)
                {
                    m_logger.Info(msg);
                }
            }
         }
        public void OnError(string msg, Exception e, bool updateLastErrorMsg = false)
        {
 
            if (updateLastErrorMsg && e != null)
                m_lastErrorMessage = msg + "->" + e.Message;
            else
                m_lastErrorMessage = msg;

            if (m_parms.Console)
            {
                Console.WriteLine(msg);
                if (e != null)
                {
                    Console.WriteLine(e.Message);
                }
            }
            if (m_logger != null)
            {
                lock (m_logger)
                {
                    m_logger.Error(msg, e);
                }
            }
        }
        #endregion

        #region IDisposable Members
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected void Dispose(bool disposing)
        {
            if (m_bWaiting)
            {
                m_waitHandle.Set();
                System.Threading.Thread.Sleep(m_parms.Timeout * 2);
            }

            if (m_bTaskStarted)
                m_bTaskStarted = !EndTask(m_parms.TaskName, !m_bTaskFailed);

            if (disposing)
            {
                if (m_waitHandle != null)
                {
                    m_waitHandle.Dispose();
                    m_waitHandle = null;
                }

                if (m_hugoConnection != null)
                {
                    m_hugoConnection.Dispose();
                    m_hugoConnection = null;
                }
            }
        }
        #endregion
    }
}
