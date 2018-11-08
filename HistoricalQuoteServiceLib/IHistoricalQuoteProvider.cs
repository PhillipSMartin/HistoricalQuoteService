using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HistoricalQuoteServiceLib
{
    public interface IHistoricalQuoteProvider : IDisposable
    {
        HistoricalQuote ProvideQuote(string ticker, string quoteType, DateTime date);

        void ReportError(string msg, Exception ex);
    }
}
