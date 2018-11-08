using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Gargoyle.Common;
using Gargoyle.Messaging.Common;
using TWSLib;

namespace HistoricalQuoteWebService
{
    public class TWSProvider : IHistoricalQuoteProvider, IDisposable
    {
        private bool m_bConnected;
        private TWSUtilities m_twsUtilities = new TWSUtilities();
        private string m_ipAddress;
        private int? m_port;
        private Dictionary<int, TWSQuoteObject> m_quoteMap = new Dictionary<int, TWSQuoteObject>();
        private object m_quoteMapLock = new object();

        public TWSProvider(string ipAddress, int? port)
        {
            m_ipAddress = ipAddress;
            m_port = port;

            m_twsUtilities.OnReaderStopped += new ServiceStoppedEventHandler(OnServiceStopped);
            m_twsUtilities.OnHistoricalData += new EventHandler<HistoricalDataEventArgs>(OnHistoricalData);
        }

        private void OnHistoricalData(object sender, HistoricalDataEventArgs e)
        {
            // ignore 'end' notification
            if ((e.Bar != null) || (e.ErrorMessage != null))
            {
                lock (m_quoteMapLock)
                {
                    TWSQuoteObject quoteObject;
                    if (m_quoteMap.TryGetValue(e.RequestId, out quoteObject))
                    {
                        quoteObject.Quote = e;
                        quoteObject.WaitHandle.Set();
                        m_quoteMap.Remove(e.RequestId);
                    }
                }
            }
        }

        private void OnServiceStopped(object sender, ServiceStoppedEventArgs e)
        {
            m_bConnected = false;
        }

        #region IHistoricalQuoteProvider Members

        public HistoricalQuote ProvideQuote(string ticker, string quoteType, DateTime date)
        {
            HistoricalQuote quote = new HistoricalQuote()
            {
                Ticker = ticker,
                Date = date
            };
            TWSQuoteObject quoteObject = null;

            try
            {
                if (!Connect())
                {
                    quote.ErrorMessage = "Unable to connect to TWS";
                }
                else
                {
                    lock (m_quoteMapLock)
                    {
                        QuoteType quoteTypeEnum = (QuoteType)Enum.Parse(typeof(QuoteType), quoteType);
                        int requestId = m_twsUtilities.GetHistoricalData(ticker, quoteTypeEnum, null, date);
                        if (requestId < 0)
                        {
                            quote.ErrorMessage = "Unable to get historical quote";
                        }
                        else
                        {
                            quoteObject = new TWSQuoteObject(requestId);
                            m_quoteMap.Add(requestId, quoteObject);
                        }
                    }
                }

                if (quoteObject != null)
                {
                    if (!WaitAny(null, quoteObject.WaitHandle))
                    {
                        quote.ErrorMessage = "Request timed out";
                    }
                    else
                    {
                        quoteObject.FillQuote(quote);
                    }
                }
            }
            catch (Exception ex)
            {
                quote.ErrorMessage = ex.Message;
            }

            return quote;
        }

        public void ReportError(string msg, Exception ex)
        {
        }

        private bool Connect()
        {
            if (!m_bConnected)
            {
                if (m_twsUtilities.Init(TWSUtilities.HISTORICAL_READER, m_ipAddress, m_port))
                    m_bConnected = m_twsUtilities.Connect();

                if (m_bConnected)
                    m_bConnected = m_twsUtilities.StartHistoricalReader();
            }
            return m_bConnected;
        }

        public bool WaitAny(int? millisecondsTimeout, params System.Threading.WaitHandle[] successConditionHandles)
        {
            int n;
            if (!millisecondsTimeout.HasValue)
                millisecondsTimeout = 10000;

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

        #endregion

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected void Dispose(bool disposing)
        {
            if (m_bConnected)
            {
                m_bConnected = false;
                m_twsUtilities.StopHistoricalReader();
                m_twsUtilities.Dispose();

                if (m_quoteMap != null)
                {
                    TWSQuoteObject[] quoteObjects = m_quoteMap.Values.ToArray();
                    foreach (TWSQuoteObject quoteObject in quoteObjects)
                    {
                        m_quoteMap.Remove(quoteObject.RequestId);
                        quoteObject.Dispose();
                    }
                }
            }
        }
    }
}