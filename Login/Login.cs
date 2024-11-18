using System.Text.Json;

namespace SitecoreCommander.Lib
{
    internal class Login
    {
        internal static EnvironmentConfiguration GetSitecoreEnvironment()
        {
            using (StreamReader r = new StreamReader(Config.XMCloudUserJsonPath))
            {
                string json = r.ReadToEnd();
                var source = JsonSerializer.Deserialize<UserJson>(json);
                return GetEnvironmentConfiguration(source, "");
            }
        }
        internal static EnvironmentConfiguration GetSitecoreEnvironment(string endpointName)
        {
            using (StreamReader r = new StreamReader(Config.XMCloudUserJsonPath))
            {
                string json = r.ReadToEnd();
                var source = JsonSerializer.Deserialize<UserJson>(json);
                return GetEnvironmentConfiguration(source, endpointName);
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

            if (userJson.Endpoints.TryGetValue(endpointName, out EnvironmentConfiguration endpointConfig))
            {
                Console.WriteLine($"Connection {endpointName} Ref: {endpointConfig.Ref} Host: {endpointConfig.Host}");
                if (!string.IsNullOrEmpty(endpointConfig.Ref) && string.IsNullOrEmpty(endpointConfig.AccessToken))
                { 
                    return GetEnvironmentConfiguration(userJson, "xmCloud");
                }
                return endpointConfig;
            }
            else
            {
                Console.WriteLine($"Endpoint '{endpointName}' not found.");
            }
            return null;
        }
    }
}
