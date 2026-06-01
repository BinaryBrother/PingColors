using System.Net;

namespace PingColors
{
    internal class Program
    {
        static void Main(string[] args)
        {
            int iWarningResponseTime = 80;       // Default warning threshold in milliseconds
            int iCriticalResponseTime = 200;     // Default critical threshold in milliseconds
            int iTimeout = 5000;                 // Default timeout for ping in milliseconds
            bool bSpeedMode = false;             // Default speed mode is off
            IPAddress oHost = IPAddress.None;    // Default host is none, will be set by CLI

            CLI commandLineInterface = new();
            Custom customMethods = new();

            Console.CancelKeyPress += delegate { Console.ResetColor(); Console.WriteLine("Exiting..."); };
            Console.Title = "PingColors";

            commandLineInterface.HandleArgs(ref iWarningResponseTime, ref iCriticalResponseTime, ref iTimeout, ref oHost, ref bSpeedMode, args);
            customMethods.ErrorChecking(iWarningResponseTime, iCriticalResponseTime, iTimeout, oHost);
            customMethods.Ping(oHost, iTimeout, iWarningResponseTime, iCriticalResponseTime, bSpeedMode);
        }
    }
}
