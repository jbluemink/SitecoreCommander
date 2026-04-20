using System.Text.Json;

namespace SitecoreCommander.Lib
{
    public class Login
    {
        public static EnvironmentConfiguration GetSitecoreEnvironment()
        {
            using (StreamReader r = new StreamReader(Config.XMCloudUserJsonPath))
            {
                string json = r.ReadToEnd();
                var source = JsonSerializer.Deserialize<UserJson>(json);
                return GetEnvironmentConfiguration(source ?? new UserJson(), "");
            }
        }
        public static EnvironmentConfiguration GetSitecoreEnvironment(string endpointName)
        {
            using (StreamReader r = new StreamReader(Config.XMCloudUserJsonPath))
            {
                string json = r.ReadToEnd();
                var source = JsonSerializer.Deserialize<UserJson>(json);
                return GetEnvironmentConfiguration(source ?? new UserJson(), endpointName);
            }
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
