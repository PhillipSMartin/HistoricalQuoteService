using System;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using HistoricalQuoteClientLib;
using Microsoft.SqlServer.Server;

public partial class StoredProcedures
{
    [Microsoft.SqlServer.Server.SqlProcedure]
    public static void CallQuoteService(string ticker, string quoteType, DateTime? date)
    {
        SqlDataRecord record = new SqlDataRecord(
           new SqlMetaData("Ticker", SqlDbType.VarChar, 20),
           new SqlMetaData("Date", SqlDbType.DateTime, -1),
           new SqlMetaData("Open", SqlDbType.Decimal, -1),
           new SqlMetaData("Close", SqlDbType.Decimal, -1),
           new SqlMetaData("High", SqlDbType.Decimal, -1),
           new SqlMetaData("Low", SqlDbType.Decimal, -1),
           new SqlMetaData("Wap", SqlDbType.Decimal, -1),
           new SqlMetaData("Volume", SqlDbType.BigInt, -1),
            new SqlMetaData("ErrorMessage", SqlDbType.VarChar, -1)
         );

        try
        {
            SqlPipe sqlPip = SqlContext.Pipe;
            var result = ClientUtilities.GetQuote(ticker, quoteType, date.Value);
            record.SetString(0, result.Ticker);
            record.SetDateTime(1, result.Date);
            record.SetDecimal(2, (decimal)result.Open);
            record.SetDecimal(3, (decimal)result.Close);
            record.SetDecimal(4, (decimal)result.High);
            record.SetDecimal(5, (decimal)result.Low);
            record.SetDecimal(6, (decimal)result.Wap);
            record.SetInt64(7, result.Volume);
            record.SetString(8, result.ErrorMessage);
            SqlContext.Pipe.Send(record);
        }
        catch (Exception ex)
        {
            record.SetString(8, ex.Message);
            SqlContext.Pipe.Send(record);
        }
    }
}
