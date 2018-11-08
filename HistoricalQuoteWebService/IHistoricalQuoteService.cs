using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

namespace HistoricalQuoteWebService
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IService1" in both code and config file together.
    [ServiceContract]
    public interface IHistoricalQuoteService
    {
        [OperationContract]
        HistoricalQuote GetQuote(string ticker, string quoteType, DateTime date);

        // TODO: Add your service operations here
    }

    // Use a data contract as illustrated in the sample below to add composite types to service operations.
    // You can add XSD files into the project. After building the project, you can directly use the data types defined there, with the namespace "HistoricalQuoteServiceLib.ContractType".
    [DataContract]
    public class HistoricalQuote
    {
        private string ticker;
        private DateTime date;
        private double open;
        private double high;
        private double low;
        private double close;
        private double wap;
        private long volume;
        private string errorMessage;

        [DataMember]
        public string Ticker
        {
            get { return ticker; }
            set { ticker = value; }
        }

        [DataMember]
        public DateTime Date
        {
            get { return date; }
            set { date = value; }
        }

        [DataMember]
        public double Open
        {
            get { return open; }
            set { open = value; }
        }

        [DataMember]
        public double High
        {
            get { return high; }
            set { high = value; }
        }

        [DataMember]
        public double Low
        {
            get { return low; }
            set { low = value; }
        }

        [DataMember]
        public double Close
        {
            get { return close; }
            set { close = value; }
        }

        [DataMember]
        public double Wap
        {
            get { return wap; }
            set { wap = value; }
        }

        [DataMember]
        public long Volume
        {
            get { return volume; }
            set { volume = value; }
        }

        [DataMember]
        public string ErrorMessage
        {
            get { return errorMessage; }
            set { errorMessage = value; }
        }
    }
}
