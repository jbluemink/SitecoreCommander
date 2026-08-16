using System.Text.Json;

namespace SitecoreCommander.Lib
{
    public class Login
    {
        private const int UserJsonReadRetryCount = 5;
        private const int UserJsonReadRetryDelayMs = 200;

        public static EnvironmentConfiguration GetSitecoreEnvironment()
        {
            return ReadEnvironmentConfigurationWithRetry("");
        }
        public static EnvironmentConfiguration GetSitecoreEnvironment(string endpointName)
        {
            return ReadEnvironmentConfigurationWithRetry(endpointName);
        }

        private static EnvironmentConfiguration ReadEnvironmentConfigurationWithRetry(string endpointName)
        {
            Exception? lastException = null;

            for (var attempt = 1; attempt <= UserJsonReadRetryCount; attempt++)
            {
                try
                {
                    using var stream = new FileStream(
                        Config.XMCloudUserJsonPath,
                        FileMode.Open,
                        FileAccess.Read,
                        FileShare.ReadWrite | FileShare.Delete);
                    using var reader = new StreamReader(stream);
                    string json = reader.ReadToEnd();
                    var source = JsonSerializer.Deserialize<UserJson>(json);
                    return GetEnvironmentConfiguration(source ?? new UserJson(), endpointName);
                }
                catch (IOException ex) when (attempt < UserJsonReadRetryCount)
                {
                    lastException = ex;
                    Console.WriteLine($"[Login] user.json locked, retry {attempt}/{UserJsonReadRetryCount} in {UserJsonReadRetryDelayMs}ms: {ex.Message}");
                    Thread.Sleep(UserJsonReadRetryDelayMs);
                }
                catch (UnauthorizedAccessException ex) when (attempt < UserJsonReadRetryCount)
                {
                    lastException = ex;
                    Console.WriteLine($"[Login] user.json access denied, retry {attempt}/{UserJsonReadRetryCount} in {UserJsonReadRetryDelayMs}ms: {ex.Message}");
                    Thread.Sleep(UserJsonReadRetryDelayMs);
                }
            }

            if (lastException != null)
            {
                throw new IOException($"Failed to read '{Config.XMCloudUserJsonPath}' after {UserJsonReadRetryCount} attempts.", lastException);
            }

            throw new IOException($"Failed to read '{Config.XMCloudUserJsonPath}'.");
        }

        internal static EnvironmentConfiguration GetEnvironmentConfiguration(UserJson userJson, string endpointName)
        {
            if (string.IsNullOrEmpty(endpointName))
            {
                endpointName = Config.EnvironmentName;
            }

            if (string.IsNullOrEmpty(endpointName))
            {
                endpointName = userJson.DefaultEndpoint;
            }

            if (userJson.Endpoints.TryGetValue(endpointName, out var endpointConfig) && endpointConfig != null)
            {
                Console.WriteLine($"Connection {endpointName} Ref: {endpointConfig.Ref} Host: {endpointConfig.Host}");
                if (!string.IsNullOrEmpty(endpointConfig.Ref) && string.IsNullOrEmpty(endpointConfig.AccessToken))
                {
                    return GetEnvironmentConfiguration(userJson, "xmCloud");
                }
                return endpointConfig;
            }

            Console.WriteLine($"Endpoint '{endpointName}' not found.");
            throw new InvalidOperationException($"Endpoint '{endpointName}' could not be resolved from user.json.");
        }
    }
}
