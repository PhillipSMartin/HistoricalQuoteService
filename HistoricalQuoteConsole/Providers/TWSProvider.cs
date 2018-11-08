using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gargoyle.Common;
using Gargoyle.Messaging.Common;
using HistoricalQuoteServiceLib;
using TWSLib;

namespace HistoricalQuoteConsole
{
    public class TWSProvider : IHistoricalQuoteProvider, IDisposable
    {
        private Host m_host;
        private bool m_bConnected;
        private TWSUtilities m_twsUtilities = new TWSUtilities();
        private string m_ipAddress;
        private int? m_port;
        private Dictionary<int, TWSQuoteObject> m_quoteMap = new Dictionary<int, TWSQuoteObject>();
        private object m_quoteMapLock = new object();

      public TWSProvider(Host host, string ipAddress, int? port)
        {
            m_host = host;
            m_ipAddress = ipAddress;
            m_port = port;

            m_twsUtilities.OnInfo += new LoggingEventHandler(m_host.Utilities_OnInfo);
            m_twsUtilities.OnError += new LoggingEventHandler(m_host.Utilities_OnError);
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
                  if (!m_quoteMap.TryGetValue(e.RequestId, out quoteObject))
                  {
                      m_host.OnInfo(String.Format("Unable to find request id {0} in map", e.RequestId), true);
                  }
                  else
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
                    if (!m_host.WaitAny(null, quoteObject.WaitHandle))
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
            if (m_host != null)
            {
                m_host.OnError(msg, ex);
            }
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
