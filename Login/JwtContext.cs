using System.Collections.Generic;

namespace SitecoreCommander.Login
{
    /// <summary>
    /// Encapsulates JWT token and automatically derives the API host from the token's tenant claim.
    /// Includes caching to avoid repeated JWT decoding for the same tenant.
    /// </summary>
    public sealed class JwtContext
    {
        private static readonly object _cacheLock = new object();
        private static readonly Dictionary<string, string> _tenantHostCache = new();

        /// <summary>
        /// The JWT access token.
        /// </summary>
        internal string AccessToken { get; }

        /// <summary>
        /// The API host URL derived from the JWT token's tenant claim.
        /// Automatically resolved and cached.
        /// </summary>
        internal string Host { get; }

        /// <summary>
        /// Creates a new JwtContext from an access token.
        /// The host is automatically derived from the token and cached by tenant.
        /// </summary>
        /// <param name="accessToken">JWT access token</param>
        internal JwtContext(string accessToken)
        {
            if (string.IsNullOrWhiteSpace(accessToken))
                throw new ArgumentException("Access token is required.", nameof(accessToken));

            AccessToken = accessToken;
            Host = GetOrCacheHostForTenant(accessToken);
        }

        /// <summary>
        /// Creates a new JwtContext from a JwtTokenResponse object.
        /// </summary>
        /// <param name="token">JWT token response containing access_token</param>
        internal JwtContext(JwtTokenResponse token)
            : this(token?.access_token ?? throw new ArgumentNullException(nameof(token)))
        {
        }

        /// <summary>
        /// Gets the cachedhost for a tenant, or derives and caches it from the token.
        /// </summary>
        private static string GetOrCacheHostForTenant(string accessToken)
        {
            var tenant = JwtTokenHelper.GetTenantNameFromJwt(accessToken);
            var cacheKey = tenant ?? "default";

            lock (_cacheLock)
            {
                if (!_tenantHostCache.ContainsKey(cacheKey))
                {
                    var host = JwtTokenHelper.GetInstanceUrlFromJwt(accessToken);
                    _tenantHostCache[cacheKey] = host;
                }

                return _tenantHostCache[cacheKey];
            }
        }

        /// <summary>
        /// Clears the tenant host cache. Useful for testing.
        /// </summary>
        internal static void ClearCache()
        {
            lock (_cacheLock)
            {
                _tenantHostCache.Clear();
            }
        }
    }
}
