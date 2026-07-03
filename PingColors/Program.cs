using System.Net;
using System.Runtime.InteropServices;

namespace PingColors
{
    internal class Program
    {
        /// <summary>
        /// Application entry point.
        /// Parses and validates command-line arguments, sets up console behavior, and starts the ping loop.
        /// </summary>
        /// <param name="args">Command-line arguments passed to the application. The <see cref="CLI.ParseArguments(string[])"/> method interprets these to set host, thresholds and modes.</param>
        static void Main(string[] args)
        {
            // Create the CLI helper which holds configuration populated from defaults and command-line args.
            CLI CLI = new CLI("PingColors");

            // Default warning threshold (milliseconds).
            // Ping responses >= this value but < Critical will typically display a "warning" color/state. [Yellow]
            CLI.WarningResponseTime = 80;

            // Default critical threshold (milliseconds).
            // Ping responses >= this value will typically display a "critical" color/state [Red].
            CLI.CriticalResponseTime = 200;

            // Default per-ping timeout (milliseconds).
            // If a ping does not receive a reply within this interval it will be treated as a timeout/failure and displayed in [Red].
            CLI.Timeout = 5000;

            // Default operation mode:
            // false = normal mode (1 second pause between pings)
            // true  = speed mode (Pings as fast as the server returns a ping)
            CLI.SpeedMode = false;

            // Host to ping. Initialized to a sentinel value (no host) and set by argument parsing.
            CLI.Host = IPAddress.None;

            // Parse and apply command-line arguments. This may override the defaults above.
            // Expected to populate CLI.oHost and may adjust thresholds, timeout and modes.
            CLI.ParseArguments(args);

            // Validate the final configuration (thresholds, timeout and host). Throws or exits on invalid input.
            Custom Custom = new Custom();
            Custom.ErrorChecking(CLI.WarningResponseTime, CLI.CriticalResponseTime, CLI.Timeout, CLI.Host);

            // Start the ping loop:
            // - target: CLI.Host
            // - timeout per ping: CLI.Timeout (ms)
            // - warning threshold: CLI.WarningResponseTime (ms)
            // - critical threshold: CLI.CriticalResponseTime (ms)
            // - speed mode: CLI.SpeedMode (true reduces return delay)
            Custom.Ping(CLI.Host, CLI.Timeout, CLI.WarningResponseTime, CLI.CriticalResponseTime, CLI.SpeedMode);
        }
    }
}