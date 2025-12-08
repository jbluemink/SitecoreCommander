using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SitecoreCommander.Utils
{

    using System;
    using System.IO;
    using System.Threading.Tasks; // 👈 Nodig voor Task en async/await

    public static class SimpleLogger
    {
        private static string _logFilePath;

        // De initialisatiemethode kan synchroon blijven, omdat deze eenmalig wordt aangeroepen.
        public static void InitializeLogFile()
        {
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string logFileName = $"Log_{timestamp}.txt";
            string logDirectory = "Logs";

            Directory.CreateDirectory(logDirectory);

            _logFilePath = Path.Combine(logDirectory, logFileName);

            // Let op: Log-aanroep moet nu ook async/await-compatibel zijn (zie main-methode)
            // Voor een snelle initialisatie houden we deze Log call hier even weg, 
            // of we laten de caller deze direct na InitializeLogFile aanroepen.
        }

        /// <summary>
        /// Writes a message to the log file asynchronously (non-blocking).
        /// </summary>
        /// <param name="message">The message content to log.</param>
        public static async Task Log(string message) // 👈 Maak de methode async en return Task
        {
            if (string.IsNullOrEmpty(_logFilePath))
            {
                // Initialiseer indien nodig. Omdat dit synchroon is, blokkeert het even. 
                // Beter is om dit altijd bij de start te doen.
                InitializeLogFile();
            }

            string logLine = $"{DateTime.Now:HH:mm:ss.fff} | {message}";

            try
            {
                // 🚀 Gebruik de asynchrone versie en 'await' de bewerking
                await File.AppendAllTextAsync(_logFilePath, logLine + Environment.NewLine);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error writing to log file: {ex.Message}");
            }
        }
    }
}
