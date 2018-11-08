using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using TWSLib;

namespace HistoricalQuoteWebService
{
    public class TWSQuoteObject : IDisposable
    {
        public TWSQuoteObject(int requestId)
        {
            RequestId = requestId;
            WaitHandle = new AutoResetEvent(false);
            WaitHandle.Reset();
        }

        public int RequestId { get; private set; }
        public AutoResetEvent WaitHandle { get; private set; }
        public HistoricalDataEventArgs Quote { get; set; }

        public void FillQuote(HistoricalQuote quote)
        {
            if (Quote == null)
            {
                quote.ErrorMessage = "Internal error: HistoricalDataEventArgs is missing from TWSQuoteObject";
            }
            else
            {
                quote.Open = Quote.Open;
                quote.Close = Quote.Close;
                quote.High = Quote.High;
                quote.Low = Quote.Low;
                quote.Wap = Quote.WAP;
                quote.Volume = Quote.Volume;
                quote.ErrorMessage = Quote.ErrorMessage;
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (WaitHandle != null)
                {
                    WaitHandle.Dispose();
                    WaitHandle = null;
                }
            }
        }
        #endregion
    }
}