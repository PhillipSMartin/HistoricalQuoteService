using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

namespace HistoricalQuoteWebService
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service1" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select Service1.svc or Service1.svc.cs at the Solution Explorer and start debugging.
    public class HistoricalQuoteService : IHistoricalQuoteService
    {
        public static IHistoricalQuoteProvider Provider = new TWSProvider("172.18.0.6", null);

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
                    Provider.ReportError("Exception in HistoricalQuoteService.GetQuote()", ex);
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
