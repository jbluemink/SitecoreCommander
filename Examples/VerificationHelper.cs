using SitecoreCommander.Utils;
using System.Text.Json;

namespace SitecoreCommander.Examples
{
    /// <summary>
    /// Helper class for verifying API responses, handling errors, and validating configuration.
    /// </summary>
    public static class VerificationHelper
    {
        public class VerificationResult<T>
        {
            public bool Success { get; set; }
            public string Message { get; set; } = string.Empty;
            public T? Data { get; set; }
            public string? ErrorDetails { get; set; }
        }

        /// <summary>
        /// Verify an API response and return structured result with success/failure info.
        /// </summary>
        public static async Task<VerificationResult<T>> VerifyResponseAsync<T>(
            T? response,
            string operationName,
            Func<T, bool>? additionalValidation = null) where T : class
        {
            try
            {
                if (response == null)
                {
                    await LogAsync($"❌ {operationName}: Response is null", ConsoleColor.Red);
                    return new VerificationResult<T>
                    {
                        Success = false,
                        Message = $"{operationName} returned null response",
                        ErrorDetails = "The API returned no data. Check if the resource exists or if authentication failed."
                    };
                }

                // Check for GraphQL error response pattern
                if (response is JsonElement jsonElement)
                {
                    if (jsonElement.TryGetProperty("errors", out var errors))
                    {
                        var errorList = errors.EnumerateArray()
                            .Select(e => e.GetProperty("message").GetString())
                            .ToList();

                        await LogAsync($"❌ {operationName}: GraphQL errors", ConsoleColor.Red);
                        return new VerificationResult<T>
                        {
                            Success = false,
                            Message = $"{operationName} returned GraphQL errors",
                            ErrorDetails = string.Join("; ", errorList)
                        };
                    }
                }

                // Run additional validation if provided
                if (additionalValidation != null && !additionalValidation(response))
                {
                    await LogAsync($"❌ {operationName}: Validation failed", ConsoleColor.Red);
                    return new VerificationResult<T>
                    {
                        Success = false,
                        Message = $"{operationName} validation failed",
                        ErrorDetails = "Response data does not meet validation criteria"
                    };
                }

                await LogAsync($"✅ {operationName}: Success", ConsoleColor.Green);
                return new VerificationResult<T>
                {
                    Success = true,
                    Message = $"{operationName} completed successfully",
                    Data = response
                };
            }
            catch (Exception ex)
            {
                await LogAsync($"❌ {operationName}: Exception - {ex.Message}", ConsoleColor.Red);
                return new VerificationResult<T>
                {
                    Success = false,
                    Message = $"{operationName} threw an exception",
                    ErrorDetails = ex.Message
                };
            }
        }

        /// <summary>
        /// Validate required configuration values before running examples.
        /// </summary>
        public static async Task<bool> ValidateConfigAsync()
        {
            var configPath = Path.Combine(AppContext.BaseDirectory, "appsettings.Local.json");
            var projectRoot = FindProjectRoot();
            if (projectRoot != null)
            {
                configPath = Path.Combine(projectRoot, "appsettings.Local.json");
            }

            var missingKeys = new List<string>();
            var hasJwtClientId = !string.IsNullOrWhiteSpace(Config.JwtClientId);
            var hasJwtClientSecret = !string.IsNullOrWhiteSpace(Config.JwtClientSecret);
            var hasJwtConfig = hasJwtClientId && hasJwtClientSecret;

            var hasUserJsonPath = !string.IsNullOrWhiteSpace(Config.XMCloudUserJsonPath);
            var userJsonExists = hasUserJsonPath && File.Exists(Config.XMCloudUserJsonPath);
            var hasUserJsonConfig = userJsonExists;

            if (hasJwtClientId ^ hasJwtClientSecret)
                missingKeys.Add("Complete JWT config: both SitecoreCommander:JwtClientId and SitecoreCommander:JwtClientSecret are required");

            if (string.IsNullOrWhiteSpace(Config.apikey))
                missingKeys.Add("SitecoreCommander:ApiKey");

            if (!hasJwtConfig && !hasUserJsonConfig)
            {
                missingKeys.Add("Authentication: configure JWT credentials OR a valid XMCloudUserJsonPath");
            }

            if (missingKeys.Count > 0)
            {
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("❌ CONFIGURATION ERROR");
                Console.ResetColor();
                Console.WriteLine();
                Console.WriteLine($"📁 Configuration file: {configPath}");
                Console.WriteLine();
                Console.WriteLine("Missing required settings:");
                foreach (var key in missingKeys)
                {
                    Console.WriteLine($"   - {key}");
                }
                Console.WriteLine();
                Console.WriteLine("ApiKey guidance:");
                Console.WriteLine("   - Use the Sitecore item API key from /sitecore/system/Settings/Services/API Keys");
                Console.WriteLine("   - Configure it in appsettings.local.json under SitecoreCommander:ApiKey");
                Console.WriteLine();
                Console.WriteLine("Example appsettings.local.json structure:");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(@"{
  ""SitecoreCommander"": {
    ""EnvironmentName"": ""your-environment-name"",
        ""XMCloudUserJsonPath"": ""C:\\Users\\you\\.sitecore\\user.json"",
    ""JwtClientId"": ""your-jwt-client-id"",
    ""JwtClientSecret"": ""your-jwt-client-secret"",
    ""ApiKey"": ""your-api-key"",
    ""DefaultLanguage"": ""en"",
    ""RestFullApiHostname"": ""https://xmcloudcm.localhost"",
    ""RestFullSitecoreUser"": ""admin"",
    ""RestFullSitecorePassword"": """"
  }
}");
                Console.ResetColor();
                Console.WriteLine();
                Console.WriteLine("Sitecore CLI user.json setup:");
                Console.WriteLine("   1. Install Sitecore CLI if needed: dotnet tool install -g Sitecore.CLI");
                Console.WriteLine("   2. Authenticate: dotnet sitecore cloud login");
                Console.WriteLine("   3. Verify user.json exists under ~/.sitecore/user.json");
                return false;
            }

            if (!hasJwtConfig)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("ℹ️  JWT credentials not fully configured. Examples will use Sitecore CLI user.json where supported.");
                Console.ResetColor();
            }

            if (!hasUserJsonConfig)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("ℹ️  Sitecore CLI user.json not found. JWT mode remains available.");
                Console.ResetColor();
            }

            return true;
        }

        /// <summary>
        /// Log a message with timestamp and optional color.
        /// </summary>
        public static async Task LogAsync(string message, ConsoleColor? color = null)
        {
            if (color.HasValue)
            {
                Console.ForegroundColor = color.Value;
            }

            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] {message}");

            if (color.HasValue)
            {
                Console.ResetColor();
            }

            await SimpleLogger.LogAsync(message);
        }

        /// <summary>
        /// Display a section header for examples.
        /// </summary>
        public static void PrintSectionHeader(string title)
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"═══════════════════════════════════════════════════════");
            Console.WriteLine($"  {title}");
            Console.WriteLine($"═══════════════════════════════════════════════════════");
            Console.ResetColor();
            Console.WriteLine();
        }

        /// <summary>
        /// Display a subsection header for individual examples.
        /// </summary>
        public static void PrintExampleHeader(string name, string description)
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"📝 {name}");
            Console.ResetColor();
            Console.WriteLine($"   {description}");
            Console.WriteLine();
        }

        /// <summary>
        /// Display success result with details.
        /// </summary>
        public static void PrintSuccess(string operation, string? details = null)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"✅ {operation}");
            Console.ResetColor();
            if (!string.IsNullOrEmpty(details))
            {
                Console.WriteLine($"   {details}");
            }
        }

        /// <summary>
        /// Display failure result with error details.
        /// </summary>
        public static void PrintFailure(string operation, string? errorDetails = null)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"❌ {operation}");
            Console.ResetColor();
            if (!string.IsNullOrEmpty(errorDetails))
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine($"   Error: {errorDetails}");
                Console.ResetColor();
            }
        }

        /// <summary>
        /// Prompt user for confirmation (writes should be cautious).
        /// </summary>
        public static bool PromptConfirmation(string message)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine();
            Console.WriteLine($"⚠️  {message}");
            Console.ResetColor();
            Console.Write("Proceed? (y/n): ");

            var response = Console.ReadLine()?.Trim().ToLower();
            return response == "y" || response == "yes";
        }

        /// <summary>
        /// Find the project root directory by looking for .sln or .csproj files.
        /// </summary>
        private static string? FindProjectRoot()
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            while (dir != null)
            {
                if (dir.GetFiles("*.sln").Length > 0 || dir.GetFiles("*.csproj").Length > 0)
                {
                    return dir.FullName;
                }
                dir = dir.Parent;
            }
            return null;
        }
    }
}
