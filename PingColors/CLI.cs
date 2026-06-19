using System.Net;
using System.Net.Sockets;

namespace PingColors
{
    internal class CLI
    {
        private readonly string sTitle;
        public int iWarningResponseTime = 80;       // Default warning threshold in milliseconds
        public int iCriticalResponseTime = 200;     // Default critical threshold in milliseconds
        public int iTimeout = 5000;                 // Default timeout for ping in milliseconds
        public bool bSpeedMode = false;             // Default: speed mode off (reduces logging/pauses)
        public IPAddress oHost = IPAddress.None;    // Host to ping; set by CLI parsing

        public CLI(string pTitle)
        {
            this.sTitle = pTitle;
            Console.CancelKeyPress += delegate { Console.ResetColor(); Console.WriteLine("Exiting..."); };
            Console.Title = "PingColors";
        }

        internal void ParseArguments(string[] pArgs)
        {
            try
            {
                if (pArgs.Length == 0) { Custom.ShowHelp(); }
                for (int i = 0; i < pArgs.Length; i++)
                {
                    switch (pArgs[i])
                    {
                        case "--warning":
                        case "-w":
                            if (i + 1 < pArgs.Length) { iWarningResponseTime = int.Parse(pArgs[++i]); }
                            break;

                        case "--critical":
                        case "-c":
                            if (i + 1 < pArgs.Length) { iCriticalResponseTime = int.Parse(pArgs[++i]); }
                            break;
                        case "--timeout":
                        case "-t":
                            if (i + 1 < pArgs.Length) { iTimeout = int.Parse(pArgs[++i]); }
                            break;
                        case "--speedmode":
                        case "-s":
                            bSpeedMode = true;
                            break;
                        case "--help":
                        case "-h":
                        case "/?":
                            Custom.ShowHelp();
                            break;
                        default:
                            if (Uri.CheckHostName(pArgs[i]) != UriHostNameType.Unknown)
                            {
                                oHost = Dns.GetHostAddresses(pArgs[i]).First(address => address.AddressFamily == AddressFamily.InterNetwork);
                            }
                            else
                            {
                                Custom.Error($"Unknown argument: {pArgs[i]}");
                                Custom.ShowHelp();
                            }
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Custom.Error($"Error parsing arguments: {ex.Message}");
                Custom.ShowHelp();
            }
        }
    }
}
