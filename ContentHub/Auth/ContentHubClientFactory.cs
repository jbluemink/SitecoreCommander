using SitecoreCommander.ContentHub.Model;
using Stylelabs.M.Sdk.WebClient;
using Stylelabs.M.Sdk.WebClient.Authentication;
using System.Reflection;

namespace SitecoreCommander.ContentHub.Auth
{
    internal static class ContentHubClientFactory
    {
        internal static IWebMClient CreateClient(ContentHubOptions options)
        {
            ContentHubOptions.ValidateRequired(options);

            var endpoint = new Uri(options.Endpoint);
            var usePasswordGrant = !string.IsNullOrWhiteSpace(options.UserName)
                && !string.IsNullOrWhiteSpace(options.Password);

            if (usePasswordGrant)
            {
                var oauth = new OAuthPasswordGrant
                {
                    ClientId = options.ClientId,
                    ClientSecret = options.ClientSecret,
                    UserName = options.UserName,
                    Password = options.Password
                };

                return MClientFactory.CreateMClient(endpoint, oauth);
            }

            var clientCredentialsGrant = TryCreateClientCredentialsGrant(options)
                ?? throw new InvalidOperationException(
                    "Content Hub client credentials authentication is not supported by this SDK version. " +
                    "Configure ContentHub:UserName and ContentHub:Password, or upgrade the Stylelabs SDK.");

            return CreateClientWithAuth(endpoint, clientCredentialsGrant);
        }

        private static object? TryCreateClientCredentialsGrant(ContentHubOptions options)
        {
            var authAssembly = typeof(OAuthPasswordGrant).Assembly;
            var candidateTypeNames = new[]
            {
                "Stylelabs.M.Sdk.WebClient.Authentication.OAuthClientCredentialsGrant",
                "Stylelabs.M.Sdk.WebClient.Authentication.OAuthClientCredentials",
                "Stylelabs.M.Sdk.WebClient.Authentication.ClientCredentialsGrant"
            };

            foreach (var typeName in candidateTypeNames)
            {
                var type = authAssembly.GetType(typeName, throwOnError: false, ignoreCase: false);
                if (type == null)
                    continue;

                var instance = CreateAuthInstance(type, options.ClientId, options.ClientSecret);
                if (instance == null)
                    continue;

                SetIfExists(instance, "ClientId", options.ClientId);
                SetIfExists(instance, "ClientSecret", options.ClientSecret);
                return instance;
            }

            return null;
        }

        private static object? CreateAuthInstance(Type type, string clientId, string clientSecret)
        {
            try
            {
                return Activator.CreateInstance(type);
            }
            catch
            {
                // Try known constructor shape: (string clientId, string clientSecret)
            }

            var ctor = type
                .GetConstructors(BindingFlags.Public | BindingFlags.Instance)
                .FirstOrDefault(c =>
                {
                    var parameters = c.GetParameters();
                    return parameters.Length == 2
                        && parameters[0].ParameterType == typeof(string)
                        && parameters[1].ParameterType == typeof(string);
                });

            if (ctor == null)
                return null;

            return ctor.Invoke(new object[] { clientId, clientSecret });
        }

        private static void SetIfExists(object target, string propertyName, string value)
        {
            var property = target.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
            if (property == null || !property.CanWrite || property.PropertyType != typeof(string))
                return;

            property.SetValue(target, value);
        }

        private static IWebMClient CreateClientWithAuth(Uri endpoint, object auth)
        {
            var method = typeof(MClientFactory)
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .FirstOrDefault(m =>
                {
                    if (!string.Equals(m.Name, "CreateMClient", StringComparison.Ordinal))
                        return false;

                    var parameters = m.GetParameters();
                    return parameters.Length == 2
                        && parameters[0].ParameterType == typeof(Uri)
                        && parameters[1].ParameterType.IsInstanceOfType(auth);
                });

            if (method == null)
                throw new InvalidOperationException("Unable to resolve MClientFactory.CreateMClient overload for client credentials authentication.");

            var result = method.Invoke(null, new[] { endpoint, auth });
            return result as IWebMClient
                ?? throw new InvalidOperationException("MClientFactory.CreateMClient did not return an IWebMClient instance.");
        }
    }
}
