using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HistoricalQuoteServiceLib;

namespace HistoricalQuoteConsole
{
    public class QuoteProviderFactory
    {
        public static IHistoricalQuoteProvider GetProvider(string provider, params object[] list)
        {
            switch (provider)
            {
                case "Test":
                    return new TestProvider((Host)list[0]);
                case "TWS":
                    return new TWSProvider((Host)list[0], (string)list[1], (int?)list[2]);
                default:
                    return null;
            }
        }
    }
}
