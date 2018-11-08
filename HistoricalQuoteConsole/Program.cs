using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Text;
using System.Threading.Tasks;
using HistoricalQuoteServiceLib;

namespace HistoricalQuoteConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            Host host = null;
            CommandLineParameters parms = new CommandLineParameters();

            string dirDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Gargoyle Strategic Investments\\HistoricalQuoteConsole";
            string appDataPath = dirDataPath + "\\TraceListener.log";
            DirectoryInfo dInfo = new DirectoryInfo(dirDataPath);
            if (!dInfo.Exists)
                dInfo.Create();

            TextWriterTraceListener trace = new TextWriterTraceListener(new StreamWriter(appDataPath, false));

            try
            {
                if (Gargoyle.Utilities.CommandLine.Utility.ParseCommandLineArguments(args, parms))
                {
                    host = new Host(parms);
                    if (host.Run())
                    {
                        trace.WriteLine("Historical Quote Service terminiated");
                    }
                    else
                    {
                        trace.WriteLine("Historical Quote Service failed - see error log");
                        if (parms.Console)
                            Console.ReadLine();
                    }
                }
                else
                {
                    // display usage message
                    string errorMessage = Gargoyle.Utilities.CommandLine.Utility.CommandLineArgumentsUsage(typeof(CommandLineParameters));

                    trace.WriteLine(errorMessage);
                }
            }
            catch (Exception ex)
            {
                trace.WriteLine(ex.ToString());
            }
            finally
            {
                trace.Flush();
                if (host != null)
                {
                    host.Dispose();
                    host = null;
                }
            }

            //using (var servicehost = new ServiceHost(typeof(HistoricalQuoteService)))
           // {

           //     servicehost.Open();

           //     Console.WriteLine("Your WCF service is running.");
           //     Console.WriteLine(
           //       "Your WCF service is running and is listening on below endpoints:");



           //     foreach (ServiceEndpoint endpoint in servicehost.Description.Endpoints)
           //     {
           //         Console.WriteLine("Address : {0} Binding Name : {1}",
           //           endpoint.Address.ToString(), endpoint.Binding.Name);
           //     }
           //     Console.WriteLine();
           //     Console.WriteLine("Press any key to stop your WCF Math service.");
           //     Console.ReadKey();


           //     servicehost.Close();
           // }
        }
    }
}
