using System.Net;
using System.Net.NetworkInformation;

namespace PingColors
{
    internal class Custom
    {
        public static void Error(string pData)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(pData);
            Console.ResetColor();
        }
        public static void ShowHelp()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Usage: PingColors [options] [host]");
            Console.WriteLine("Options:");
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine("  -w, --warning <ms>    Set the warning threshold in milliseconds (default: 80)");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("  -c, --critical <ms>   Set the critical threshold in milliseconds (default: 200)");
            Console.WriteLine("  -t, --timeout <ms>    Set the timeout for ping in milliseconds (default: 5000)");
            Console.ResetColor();
            Console.WriteLine("  -s, --speedmode       Super fast ping rate (default: false)");
            Console.WriteLine("  -h, --help            Show this help message");
            Environment.Exit(0);
        }

        internal void ErrorChecking(int pWarningResponseTime, int pCriticalResponseTime, int pTimeout, IPAddress pHost)
        {
            if (pWarningResponseTime <= 0)
            {
                Custom.Error("Warning threshold must be a positive integer.");
                Custom.ShowHelp();
            }
            if (pCriticalResponseTime <= 0)
            {
                Custom.Error("Critical threshold must be a positive integer.");
                Custom.ShowHelp();
            }
            if (pTimeout <= 0)
            {
                Custom.Error("Timeout must be a positive integer.");
                Custom.ShowHelp();
            }
            if (pHost == IPAddress.None)
            {
                Custom.Error("No valid host specified.");
                Custom.ShowHelp();
            }
            if (pWarningResponseTime >= pCriticalResponseTime)
            {
                Custom.Error("Warning threshold must be less than critical threshold.");
                Custom.ShowHelp();
            }
            if (pCriticalResponseTime <= pWarningResponseTime)
            {
                Custom.Error("Critical threshold must be greater than warning threshold.");
                Custom.ShowHelp();
            }
            if (pTimeout < pWarningResponseTime || pTimeout < pCriticalResponseTime)
            {
                Custom.Error("Timeout must be greater than both warning and critical thresholds.");
                Custom.ShowHelp();
            }
        }
        internal void Ping(IPAddress pHost, int pTimeout, int pWarningResponseTime, int pCriticalResponseTime, bool pSpeedMode)
        {
            Ping oPingSender = new Ping();

            while (true)
            {
                try
                {

                    PingReply reply = oPingSender.Send(pHost, pTimeout); // Cannot use the Buffer param here, because Ubuntu requires elevated permissions.
                    bool success = reply.Status == IPStatus.Success;
                    // Maintain the sliding window
                    if (reply.Status == IPStatus.Success)
                    {
                        if (reply.RoundtripTime < pWarningResponseTime)
                        {
                            Console.ForegroundColor = ConsoleColor.Green;
                        }
                        else if (reply.RoundtripTime >= pWarningResponseTime && reply.RoundtripTime < pCriticalResponseTime)
                        {
                            Console.ForegroundColor = ConsoleColor.DarkYellow;
                        }
                        else if (reply.RoundtripTime >= pCriticalResponseTime)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                        }
                        string lReturn = $"Reply from {reply.Address} bytes=32 time={reply.RoundtripTime}ms";
                        if (reply.Options != null) { lReturn += $" TTL={reply.Options.Ttl}"; } // Ubuntu's PingReply does not include the Options property
                        Console.WriteLine(lReturn);
                    }
                    else
                    {
                        Custom.Error($"Ping failed: {reply.Status}"); // This will catch cases where the ping request was sent but did not receive a successful response, such as timeouts or unreachable hosts.
                    }
                }
                catch (PingException e)
                {
                    Custom.Error($"Ping error: {e.Message}"); // This will catch exceptions related to the ping operation, such as network errors or invalid host.
                    //DrawStatusBar("Packet Loss: " + Math.Round(((double)_PingFail / (_PingFail + _PingPass)) * 100.0, 1) + "% | Time: " + DateTime.Now.ToString("hh:mm:ss tt"));
                }
                if (!pSpeedMode) { Thread.Sleep(1000); } // Wait for 1 second before the next ping.
            }
        }
    }
}
