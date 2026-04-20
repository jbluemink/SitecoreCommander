using SitecoreCommander.Login;

namespace SitecoreCommander.Examples
{
    /// <summary>
    /// Base class for API example runners. Handles common setup like authentication and error handling.
    /// See: /AI/AGENT_API_PLAYBOOK.md, /AI/placeholder-implementation-guide.md
    /// </summary>
    public abstract class ExampleBase
    {
        protected JwtTokenResponse? Token { get; set; }
        protected EnvironmentConfiguration? Environment { get; set; }
        protected CancellationToken CancellationToken { get; set; }

        protected virtual bool SupportsJwtAuthentication => true;
        protected virtual bool SupportsUserJsonAuthentication => true;

        /// <summary>
        /// Initialize the example runner with authentication.
        /// Supports: JWT credentials or Sitecore CLI user.json.
        /// </summary>
        public virtual async Task<bool> InitializeAsync()
        {
            try
            {
                var requestedMode = ExampleAuthContext.SelectedMode;
                await VerificationHelper.LogAsync($"Authenticating with Sitecore ({requestedMode})...", ConsoleColor.Gray);

                if (requestedMode == ExampleAuthMode.Jwt)
                {
                    return await InitializeWithJwtAsync();
                }

                if (requestedMode == ExampleAuthMode.UserJson)
                {
                    return await InitializeWithUserJsonAsync();
                }

                if (SupportsUserJsonAuthentication && await InitializeWithUserJsonAsync(logAttempt: false))
                    return true;

                if (SupportsJwtAuthentication)
                    return await InitializeWithJwtAsync(logAttempt: false);

                await VerificationHelper.LogAsync("No supported authentication mode is available for this module.", ConsoleColor.Red);
                return false;
            }
            catch (Exception ex)
            {
                await VerificationHelper.LogAsync($"Authentication failed: {ex.Message}", ConsoleColor.Red);
                return false;
            }
        }

        /// <summary>
        /// Get the host for API calls from config.
        /// </summary>
        protected string GetHost()
        {
            return Config.RestFullApiHostname;
        }

        protected virtual string GetResolvedHost()
        {
            return GetHost();
        }

        protected EnvironmentConfiguration CreateEnvironmentConfig()
        {
            if (Environment == null)
                throw new InvalidOperationException("Environment is not initialized. Call InitializeAsync() first.");

            return Environment;
        }

        /// <summary>
        /// Run all examples in this group.
        /// </summary>
        public abstract Task RunAsync();

        /// <summary>
        /// Verify token is available before running examples.
        /// </summary>
        protected bool EnsureJwtTokenExists()
        {
            if (Token == null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("❌ JWT token is not initialized. Use JWT auth mode or call InitializeAsync() first.");
                Console.ResetColor();
                return false;
            }
            return true;
        }

        protected bool EnsureEnvironmentExists()
        {
            if (Environment == null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("❌ Environment configuration is not initialized. Call InitializeAsync() first.");
                Console.ResetColor();
                return false;
            }

            if (string.IsNullOrWhiteSpace(Environment.AccessToken))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("❌ Access token is missing in the active environment configuration.");
                Console.ResetColor();
                return false;
            }

            if (string.IsNullOrWhiteSpace(Environment.Host))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("❌ Host is missing in the active environment configuration.");
                Console.ResetColor();
                return false;
            }

            return true;
        }

        protected JwtTokenResponse GetJwtTokenForLegacyWrappers()
        {
            if (Token != null && !string.IsNullOrWhiteSpace(Token.access_token))
                return Token;

            if (Environment != null && !string.IsNullOrWhiteSpace(Environment.AccessToken))
            {
                return new JwtTokenResponse
                {
                    access_token = Environment.AccessToken,
                    token_type = "Bearer"
                };
            }

            throw new InvalidOperationException("No access token available for JWT-style wrapper calls.");
        }

        /// <summary>
        /// Run an example with error handling and verification.
        /// </summary>
        protected async Task RunExampleAsync(
            string name,
            string description,
            Func<Task> exampleFunc)
        {
            VerificationHelper.PrintExampleHeader(name, description);

            try
            {
                await exampleFunc();
            }
            catch (Exception ex)
            {
                VerificationHelper.PrintFailure(name, ex.Message);
                await VerificationHelper.LogAsync($"Exception in {name}: {ex}", ConsoleColor.Red);
            }

            Console.WriteLine();
        }

        private async Task<bool> InitializeWithJwtAsync(bool logAttempt = true)
        {
            if (!SupportsJwtAuthentication)
            {
                await VerificationHelper.LogAsync("This module does not support JWT authentication.", ConsoleColor.Red);
                return false;
            }

            if (logAttempt)
                await VerificationHelper.LogAsync("Using JWT credentials from appsettings.Local.json", ConsoleColor.Gray);

            Token = await SitecoreJwtClient.GetJwtAsync();

            if (Token == null || string.IsNullOrWhiteSpace(Token.access_token))
            {
                await VerificationHelper.LogAsync("Failed to obtain JWT token. Check JWT credentials in appsettings.Local.json", ConsoleColor.Red);
                return false;
            }

            Environment = new EnvironmentConfiguration
            {
                Host = GetResolvedHost(),
                AccessToken = Token.access_token
            };

            await VerificationHelper.LogAsync($"✅ Authentication successful (JWT) | Host: {Environment.Host}", ConsoleColor.Green);
            return true;
        }

        private async Task<bool> InitializeWithUserJsonAsync(bool logAttempt = true)
        {
            if (!SupportsUserJsonAuthentication)
            {
                await VerificationHelper.LogAsync("This module does not support user.json authentication.", ConsoleColor.Red);
                return false;
            }

            if (string.IsNullOrWhiteSpace(Config.XMCloudUserJsonPath) || !File.Exists(Config.XMCloudUserJsonPath))
            {
                await VerificationHelper.LogAsync("user.json not found. Configure SitecoreCommander:XMCloudUserJsonPath in appsettings.Local.json.", ConsoleColor.Yellow);
                return false;
            }

            if (logAttempt)
                await VerificationHelper.LogAsync($"Using Sitecore CLI user.json at: {Config.XMCloudUserJsonPath}", ConsoleColor.Gray);

            Environment = SitecoreCommander.Lib.Login.GetSitecoreEnvironment(Config.EnvironmentName);
            if (Environment == null || string.IsNullOrWhiteSpace(Environment.AccessToken))
            {
                await VerificationHelper.LogAsync("Failed to load a valid environment/token from user.json.", ConsoleColor.Red);
                return false;
            }

            Token = null;
            await VerificationHelper.LogAsync($"✅ Authentication successful (user.json) | Host: {Environment.Host}", ConsoleColor.Green);
            return true;
        }
    }
}
