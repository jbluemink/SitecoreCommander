using System;
using System.IO;
using System.Threading.Tasks;

namespace SitecoreCommander.Utils
{
    public static class SimpleLogger
    {
        private static string _logFilePath = string.Empty;

        public static string GetLogFilePath()
        {
            return _logFilePath;
        }

        // Initialization can remain synchronous because it runs once.
        public static void InitializeLogFile()
        {
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string logFileName = $"Log_{timestamp}.txt";
            string logDirectory = "Logs";

            Directory.CreateDirectory(logDirectory);

            _logFilePath = Path.Combine(logDirectory, logFileName);

            // Let op: Log-aanroep moet nu ook async/await-compatibel zijn (zie main-methode)
            // Keep initialization lightweight; callers can log immediately after initialization.
        }

        /// <summary>
        /// Writes a message to the log file asynchronously (non-blocking).
        /// </summary>
        /// <param name="message">The message content to log.</param>
        public static async Task LogAsync(string message)
        {
            if (string.IsNullOrEmpty(_logFilePath))
            {
                // Initialize on first use. This synchronous path may block briefly.
                // Beter is om dit altijd bij de start te doen.
                InitializeLogFile();
            }

            string logLine = $"{DateTime.Now:HH:mm:ss.fff} | {message}";

            try
            {
                // Use async file IO and await completion.
                await File.AppendAllTextAsync(_logFilePath, logLine + Environment.NewLine);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error writing to log file: {ex.Message}");
            }
        }
    }
}
