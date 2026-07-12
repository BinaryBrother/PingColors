using System.Net;
using System.Net.NetworkInformation;

namespace PingColors
{
    internal class Custom
    {
        public static void Error(string pData)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(pData.PadRight(Console.WindowWidth - 1));
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

        public static void ErrorChecking(int pWarningResponseTime, int pCriticalResponseTime, int pTimeout, IPAddress pHost)
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
        private static void DrawStatusBar(int sent, int lost, int warningThreshold, int criticalThreshold, long? roundtripTime = 0)
        {
            if (Console.WindowHeight < 3) { return; }
            string bar1 = "";
            int firstLineRow = Console.WindowHeight - 2;
            int secondLineRow = Console.WindowHeight - 1;
            int consoleWidth = Console.WindowWidth - 1;

            int savedTop = Console.CursorTop;
            int savedLeft = Console.CursorLeft;
            double lossPercent = sent == 0 ? 0.0 : (lost * 100.0 / sent);

            // ==========================================
            // FIX: MANUAL LOG SCROLL ENGINE
            // ==========================================
            // If the active text log hits our status bar boundary, shift the entire 
            // console screen area up by 1 row to simulate natural terminal scrolling.
            if (savedTop >= firstLineRow)
            {
                // Shift everything from row 1 down to the line above the status bar UP by 1 row
                Console.MoveBufferArea(0, 1, Console.WindowWidth, firstLineRow - 1, 0, 0);

                // Push our tracking pointer back up to the newly cleared line
                savedTop = firstLineRow - 1;
            }
            if (roundtripTime.HasValue)
            {
                if (roundtripTime.Value < warningThreshold)
                {
                    bar1 += "\e[32m███\e[0m";
                }
                else if (roundtripTime.Value >= warningThreshold && roundtripTime.Value < criticalThreshold)
                {
                    bar1 += "\e[33m██████\e[0m";
                }
                else if (roundtripTime.Value >= criticalThreshold)
                {
                    bar1 += "\e[31m██████████\e[0m";
                }
                else
                {
                    bar1 += "\e[31m█████████████\e[0m";
                }
                // bar1 = $"███ Last RTT: {roundtripTime.Value} ms █";
            }
            //string rttText = roundtripTime.HasValue ? $"{roundtripTime.Value} ms" : "Timeout";
            //string bar1 = $"█ Last RTT: {rttText} █";
            string bar2 = $"█ Packets: {sent} █ Lost: {lost} █ Loss: {lossPercent:F1}% █";

            // --- LINE 1 ---
            Console.SetCursorPosition(0, firstLineRow);
            Console.Write(bar1.PadRight(consoleWidth));

            // --- LINE 2 ---
            Console.SetCursorPosition(0, secondLineRow);
            Console.Write(bar2.PadRight(consoleWidth - 1)); // -1 keeps the bottom-right corner empty to prevent glitch scrolling

            // --- RESTORE CURSOR SAFELY ---
            Console.SetCursorPosition(savedLeft, savedTop);
        }
        private static void DrawSpecialEffects(string pText)
        {
            int width = pText.Length + 4; // Padding of 2 spaces on each side

            // Top border
            Console.WriteLine("┌" + new string('─', width) + "┐");

            // Text line with padding
            Console.Write("│  ");

            // Bottom border
            
            for (int i = 0; i < pText.Length; i++)
            {
                int r = (int)(128 + 127 * Math.Sin(i * 0.3));
                int g = (int)(128 + 127 * Math.Sin(i * 0.3 + 2));
                int b = (int)(128 + 127 * Math.Sin(i * 0.3 + 4));
                // Explicitly inject the ANSI color block
                Console.Write($"\e[38;2;{r};{g};{b}m{pText[i]}");
            }
            Console.WriteLine("  │");
            Console.WriteLine("└" + new string('─', width) + "┘");
            Console.Write("\e[0m"); // Reset colors
            //Console.ResetColor();
        }
        public static async Task PingLoop(IPAddress pHost, int pTimeout, int pWarningResponseTime, int pCriticalResponseTime, bool pSpeedMode)
        {
            Ping oPingSender = new Ping();
            PingReply reply = null;
            int sentPackets = 0;
            int lostPackets = 0;
            DrawSpecialEffects($"=== Ping, With Color! ===");

            while (true)
            {
                try
                {
                    reply = await oPingSender.SendPingAsync(pHost, pTimeout); // Cannot use the Buffer param here, because Ubuntu requires elevated permissions.

                    sentPackets++;

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
                        //Console.WriteLine(lReturn);
                        Console.WriteLine(lReturn.PadRight(Console.WindowWidth - 1));
                    }
                    else
                    {
                        lostPackets++;
                        Custom.Error($"Ping failed: {reply.Status}"); // This will catch cases where the ping request was sent but did not receive a successful response, such as timeouts or unreachable hosts.
                    }
                }
                catch (PingException e)
                {
                    lostPackets++;
                    Custom.Error($"Ping error: {e.Message}"); // This will catch exceptions related to the ping operation, such as network errors or invalid host.
                }
                DrawStatusBar(sentPackets,lostPackets, pWarningResponseTime, pCriticalResponseTime, reply.RoundtripTime); // Placeholder values for sent and lost packets. You can implement a proper counter to track these values.
                if (!pSpeedMode) { await Task.Delay(1000); } // Wait for 1 second before the next ping.
            }
        }
    }
}
