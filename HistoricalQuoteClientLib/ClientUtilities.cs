using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HistoricalQuoteClientLib.svcService;
using HistoricalQuoteWebService;

namespace HistoricalQuoteClientLib
{
    public class ClientUtilities
    {
        public static HistoricalQuote GetQuote(string ticker, string quoteType, DateTime date)
        {
            HistoricalQuote quote = null;
            using (Service1 proxy = new Service1())
            {
                quote = proxy.GetQuote(ticker, quoteType, date);
            }
            return quote;
        }
    }
}
