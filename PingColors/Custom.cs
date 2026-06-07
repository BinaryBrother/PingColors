using System.Net;
using System.Net.NetworkInformation;
using static System.Net.Mime.MediaTypeNames;

namespace PingColors
{
    internal class Custom
    {
        public static void Error(string pData)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            //Console.Write(pData.PadRight(Console.BufferWidth));
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
        public static void Update(string text)
        {
            int left = Console.CursorLeft;
            int top = Console.CursorTop;

            int statusLine = Console.WindowHeight - 1;

            Console.SetCursorPosition(0, statusLine);

            //Console.BackgroundColor = ConsoleColor.DarkBlue;
            Console.ForegroundColor = ConsoleColor.White;

            Console.Write(text.PadRight(Console.WindowWidth));

            Console.ResetColor();

            Console.SetCursorPosition(left, top);
        }
        internal void Ping(IPAddress pHost, int pTimeout, int pWarningResponseTime, int pCriticalResponseTime, bool pSpeedMode)
        {
            Ping pPingSender = new Ping();

            while (true)
            {
                try
                {
                    // Send a basic ping request
                    PingReply reply = pPingSender.Send(pHost, pTimeout); // Cannot use the Buffer param here, because Ubuntu requires elevated permissions.
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
                        if (reply.Options != null) { lReturn += $" TTL={reply.Options.Ttl}"; } // Ubuntu's PingReply does not include the Options property.
                        //Console.Write(lReturn.PadRight(Console.BufferWidth));
                        Console.WriteLine(lReturn);
                        //Update("Last successful ping: " + DateTime.Now.ToString("hh:mm:ss tt"));
                    }
                    else
                    {
                        Custom.Error($"Ping failed: {reply.Status}"); // This will catch cases where the ping request was sent but did not receive a successful response, such as timeouts or unreachable hosts.
                    }
                }
                catch (PingException e)
                {
                    Custom.Error($"Ping error: {e.Message}"); // This will catch exceptions related to the ping operation, such as network errors or invalid host.
                }
                if (!pSpeedMode) { Thread.Sleep(1000); } // Wait for 1 second before the next ping.
            }
        }
    }
}
