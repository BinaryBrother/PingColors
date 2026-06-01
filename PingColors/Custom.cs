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

        internal void ErrorChecking(int warningResponseTime, int criticalResponseTime, int timeout, IPAddress host)
        { 
            if (warningResponseTime <= 0)
            {
                Custom.Error("Warning threshold must be a positive integer.");
                Custom.ShowHelp();
            }
            if (criticalResponseTime <= 0)
            {
                Custom.Error("Critical threshold must be a positive integer.");
                Custom.ShowHelp();
            }
            if (timeout <= 0)
            {
                Custom.Error("Timeout must be a positive integer.");
                Custom.ShowHelp();
            }
            if (host == null)
            {
                Custom.Error("No valid host specified.");
                Custom.ShowHelp();
            }
            if (warningResponseTime >= criticalResponseTime)
            {
                Custom.Error("Warning threshold must be less than critical threshold.");
                Custom.ShowHelp();
            }
            if (criticalResponseTime <= warningResponseTime)
            {
                Custom.Error("Critical threshold must be greater than warning threshold.");
                Custom.ShowHelp();
            }
            if (timeout < warningResponseTime || timeout < criticalResponseTime)
            {
                Custom.Error("Timeout must be greater than both warning and critical thresholds.");
                Custom.ShowHelp();
            }
        }

        internal void Ping(IPAddress pHost, int pTimeout, int pWarningResponseTime, int pCriticalResponseTime, bool pSpeedMode)
        {
            Ping pPingSender = new Ping();
            byte[] pBuffer = new byte[32];

            while (true)
            {
                try
                {
                    // Send a basic ping request
                    PingReply reply = pPingSender.Send(pHost, pTimeout, pBuffer);

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
                        Console.WriteLine($"Reply from {reply.Address} bytes={pBuffer.Length} time={reply.RoundtripTime}ms TTL={reply.Options?.Ttl}");
                    }
                    else
                    {
                        Custom.Error($"Ping failed: {reply.Status}");
                    }
                }
                catch (PingException e)
                {
                    Custom.Error($"Ping error: {e.Message}");
                }
                if (!pSpeedMode) { Thread.Sleep(1000); } // Wait for 1 second before the next ping
            }
        }
    }
}
