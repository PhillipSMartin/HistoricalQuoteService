using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HistoricalQuoteServiceLib;

namespace HistoricalQuoteConsole
{
    public class TestProvider : IHistoricalQuoteProvider
    {
        private Host m_host;

        public TestProvider(Host host)
        {
            m_host = host;
        }
        #region IHistoricalQuoteProvider Members

        public HistoricalQuote ProvideQuote(string ticker, string quoteType, DateTime date)
        {
            return new HistoricalQuote()
            {
                Ticker = ticker,
                Date = date,
                Open = 10,
                High = 20,
                Low = 5,
                Close = 15,
                Wap = 12.5,
                Volume = 100000
            };
        }

        public void ReportError(string msg, Exception ex)
        {
            if (m_host != null)
            {
                m_host.OnError(msg, ex);
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
        }
    }
}
