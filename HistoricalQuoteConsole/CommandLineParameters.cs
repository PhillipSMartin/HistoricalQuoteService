using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gargoyle.Utilities.CommandLine;

namespace HistoricalQuoteConsole
{
    public class CommandLineParameters
    {
        [CommandLineArgumentAttribute(CommandLineArgumentType.AtMostOnce, ShortName = "pname", Description = "Name of program to specify to DBAccess")]
        public string ProgramName = "HistoricalQuoteConsole";

        [CommandLineArgumentAttribute(CommandLineArgumentType.AtMostOnce, ShortName = "tname", Description = "Task name for reporting completion - single space to ignore")]
        public string TaskName = "HistoricalQuoteConsole";

        [CommandLineArgumentAttribute(CommandLineArgumentType.AtMostOnce, ShortName = "p", Description = "Historical quote provider")]
        public string Provider = "TWS";

        [CommandLineArgumentAttribute(CommandLineArgumentType.AtMostOnce, Description = "Time app should automatically stop, expressed as a string hh:mm - single space to accept stop from console")]
        public string StopTime = " ";

        [CommandLineArgumentAttribute(CommandLineArgumentType.AtMostOnce, ShortName = "qh", Description = "Host to connect to for quotes - single space if we don't want to connect")]
        public string QuoteServerHost = "gargoyle-mw20";

        [CommandLineArgumentAttribute(CommandLineArgumentType.AtMostOnce, ShortName = "qp", Description = "Port to connect to for quotes - -1 for default")]
        public int QuoteServerPort = -1;

        [CommandLineArgumentAttribute(CommandLineArgumentType.AtMostOnce, Description = "Time in milleseconds before events time out")]
        public int Timeout = 10000;

        [CommandLineArgumentAttribute(CommandLineArgumentType.AtMostOnce, Description = "Write to console if true")]
        public bool Console = true;

        public bool GetStopTime(out TimeSpan stopTime)
        {
            return TimeSpan.TryParse(StopTime, out stopTime);
        }

        public bool StopTimeIsSpecified
        {
            get { return StopTime != " "; }
        }

        public int? GetQuoteServerPort
        {
            get { return (QuoteServerPort < 0) ? (int?)null : QuoteServerPort; }
        }
    }
}
