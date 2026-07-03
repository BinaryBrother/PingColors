using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace PingColors
{

    public partial class CLI
    {
        private const int STD_OUTPUT_HANDLE = -11;
        private const uint ENABLE_VIRTUAL_TERMINAL_PROCESSING = 0x0004;

        [LibraryImport("kernel32.dll", SetLastError = true)]
        public static partial IntPtr GetStdHandle(int nStdHandle);

        [LibraryImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

        [LibraryImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

        public int WarningResponseTime { get; set; }
        public int CriticalResponseTime { get; set; }
        public int Timeout { get; set; }
        public bool SpeedMode { get; set; }
        public IPAddress? Host { get; set; }

        public CLI(string pTitle)
        {
            Console.Title = pTitle;
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.CursorVisible = false;
            Console.CancelKeyPress += delegate 
            { 
                Console.ResetColor();
                Console.WriteLine(); // Move to a new line after Ctrl+C
                Console.WriteLine("Exiting..."); 
            };
            if (OperatingSystem.IsWindows())
            {
                IntPtr handle = GetStdHandle(STD_OUTPUT_HANDLE);
                if (GetConsoleMode(handle, out uint mode))
                {
                    SetConsoleMode(handle, mode | ENABLE_VIRTUAL_TERMINAL_PROCESSING);
                }
            }
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
                            if (i + 1 < pArgs.Length) { WarningResponseTime = int.Parse(pArgs[++i]); }
                            break;

                        case "--critical":
                        case "-c":
                            if (i + 1 < pArgs.Length) { CriticalResponseTime = int.Parse(pArgs[++i]); }
                            break;
                        case "--timeout":
                        case "-t":
                            if (i + 1 < pArgs.Length) { Timeout = int.Parse(pArgs[++i]); }
                            break;
                        case "--speedmode":
                        case "-s":
                            SpeedMode = true;
                            break;
                        case "--help":
                        case "-h":
                        case "/?":
                            Custom.ShowHelp();
                            break;
                        default:
                            if (Uri.CheckHostName(pArgs[i]) != UriHostNameType.Unknown)
                            {
                                Host = Dns.GetHostAddresses(pArgs[i]).First(address => address.AddressFamily == AddressFamily.InterNetwork);
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
