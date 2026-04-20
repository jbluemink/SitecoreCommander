using SitecoreCommander.Login;

namespace SitecoreCommander.Authoring
{
    internal sealed class AuthoringApiContext
    {
        internal string AccessToken { get; }
        internal Uri GraphQlEndpoint { get; }

        private AuthoringApiContext(string host, string accessToken)
        {
            if (string.IsNullOrWhiteSpace(host))
            {
                throw new ArgumentException("Host is required.", nameof(host));
            }

            if (string.IsNullOrWhiteSpace(accessToken))
            {
                throw new ArgumentException("Access token is required.", nameof(accessToken));
            }

            var normalizedHost = host.EndsWith("/") ? host : host + "/";
            AccessToken = accessToken;
            GraphQlEndpoint = new Uri(normalizedHost + "sitecore/api/authoring/graphql/v1/");
        }

        internal static AuthoringApiContext FromEnvironment(EnvironmentConfiguration env)
        {
            if (env == null)
            {
                throw new ArgumentNullException(nameof(env));
            }

            return new AuthoringApiContext(env.Host, env.AccessToken);
        }

        internal static AuthoringApiContext FromJwt(JwtTokenResponse token, string host)
        {
            if (token == null)
            {
                throw new ArgumentNullException(nameof(token));
            }

            return new AuthoringApiContext(host, token.access_token);
        }
    }

    internal static class AuthoringGraphQl
    {
        internal static Task<Request.GraphQLResponse<TResponse>> ExecuteAsync<TResponse>(
            AuthoringApiContext context,
            string query,
            object variables,
            CancellationToken cancellationToken)
            where TResponse : class
        {
            return Request.CallGraphQLAsync<TResponse>(
                context.GraphQlEndpoint,
                HttpMethod.Post,
                context.AccessToken,
                string.Empty,
                query,
                variables,
                cancellationToken);
        }
    }
}