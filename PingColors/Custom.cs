using System.Net;
using System.Net.NetworkInformation;

namespace PingColors
{
    internal class Custom
    {
        public static void DrawStatusBar(string message, ConsoleColor backgroundColor = ConsoleColor.Black, ConsoleColor foregroundColor = ConsoleColor.White)
        {
            // Save current cursor position and colors
            int savedLeft = Console.CursorLeft;
            int savedTop = Console.CursorTop;
            ConsoleColor savedBg = Console.BackgroundColor;
            ConsoleColor savedFg = Console.ForegroundColor;

            // Move to the last line of the console window
            int statusRow = Console.WindowHeight - 1;
            Console.SetCursorPosition(0, statusRow);

            // Apply colors
            Console.BackgroundColor = backgroundColor;
            Console.ForegroundColor = foregroundColor;

            // Pad or truncate the message to fill the full width
            int width = Console.WindowWidth;
            string statusText = message.Length >= width
                ? message[..(width - 1)]          // truncate if too long (leave 1 char margin)
                : message.PadRight(width);         // pad to fill the bar

            Console.Write(statusText);

            // Restore cursor position and colors
            Console.SetCursorPosition(savedLeft, savedTop);
            Console.BackgroundColor = savedBg;
            Console.ForegroundColor = savedFg;
        }
    
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
        internal void Ping(IPAddress pHost, int pTimeout, int pWarningResponseTime, int pCriticalResponseTime, bool pSpeedMode)
        {
            Ping oPingSender = new Ping();
            int _PingPass = 0;
            int _PingFail = 0;
            while (true)
            {
                try
                {
                    if (_PingPass > 20 || _PingFail > 20) { _PingPass = 0; _PingFail = 0; }

                    PingReply reply = oPingSender.Send(pHost, pTimeout); // Cannot use the Buffer param here, because Ubuntu requires elevated permissions.
                    if (reply.Status == IPStatus.Success)
                    {
                        _PingPass++;
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
                        DrawStatusBar("Packet Loss: " + Math.Round(((double)_PingFail / (_PingFail + _PingPass)) * 100.0, 1) + "% | Time: " + DateTime.Now.ToString("hh:mm:ss tt"));
                    }
                    else
                    {
                        _PingFail++;
                        Custom.Error($"Ping failed: {reply.Status}"); // This will catch cases where the ping request was sent but did not receive a successful response, such as timeouts or unreachable hosts.
                        DrawStatusBar("Packet Loss: " + Math.Round(((double)_PingFail / (_PingFail + _PingPass)) * 100.0, 1) + "% | Time: " + DateTime.Now.ToString("hh:mm:ss tt"));
                    }
                }
                catch (PingException e)
                {
                    _PingFail++;
                    Custom.Error($"Ping error: {e.Message}"); // This will catch exceptions related to the ping operation, such as network errors or invalid host.
                    DrawStatusBar("Packet Loss: " + Math.Round(((double)_PingFail / (_PingFail + _PingPass)) * 100.0, 1) + "% | Time: " + DateTime.Now.ToString("hh:mm:ss tt"));
                }
                if (!pSpeedMode) { Thread.Sleep(1000); } // Wait for 1 second before the next ping.
            }
        }
    }
}
