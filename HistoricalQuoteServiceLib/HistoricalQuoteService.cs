using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace HistoricalQuoteServiceLib
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class HistoricalQuoteService : IHistoricalQuoteService
    {
        public static IHistoricalQuoteProvider Provider { get; set; }

        #region IHistoricalQuoteService Members

        public HistoricalQuote GetQuote(string ticker, string quoteType, DateTime date)
        {
            try
            {
                return Provider.ProvideQuote(ticker, quoteType, date);
            }
            catch (Exception ex)
            {
                if (Provider != null)
                {
                    Provider.ReportError("Exception in SentoniService.GetQuote()", ex);
                    throw new FaultException(ex.Message);
                }
                else
                {
                    throw new FaultException("Provider was not set in HistoricalQuoteService instance");
                }
             }
        }

        #endregion
    }
}
