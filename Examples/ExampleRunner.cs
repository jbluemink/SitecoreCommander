namespace SitecoreCommander.Examples
{
    /// <summary>
    /// Orchestrates running API examples with mode selection.
    /// Provides a menu-driven interface for developers to quickly test different APIs.
    /// </summary>
    public class ExampleRunner
    {
        public enum ApiModule
        {
            Agent,
            Authoring,
            Edge,
            Command,
            ContentHub
        }

        /// <summary>
        /// Run examples based on configuration.
        /// </summary>
        public static async Task RunAsync(ApiModule? singleModule = null)
        {
            // Validate configuration
            if (!await VerificationHelper.ValidateConfigAsync())
            {
                Environment.Exit(1);
                return;
            }

            // Display mode
            Console.WriteLine();
            VerificationHelper.PrintSectionHeader("SitecoreCommander API Examples");
            Console.WriteLine("Test Sitecore APIs and verify functionality");
            Console.WriteLine();

            ExampleAuthContext.SelectedMode = await SelectAuthenticationModeAsync(singleModule);
            if (ExampleAuthContext.SelectedMode == ExampleAuthMode.Auto)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("Auth mode: Auto (prefer Sitecore CLI user.json, fallback to JWT)");
                Console.ResetColor();
                Console.WriteLine();
            }

            if (singleModule != null)
            {
                // Run single module
                await RunModuleAsync(singleModule.Value);
            }
            else
            {
                // Interactive menu
                await ShowInteractiveMenuAsync();
            }

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("✅ Examples completed!");
            Console.ResetColor();
            Console.WriteLine();
        }

        /// <summary>
        /// Show interactive menu for selecting which examples to run.
        /// </summary>
        private static async Task ShowInteractiveMenuAsync()
        {
            while (true)
            {
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("Which API would you like to test?");
                Console.ResetColor();
                Console.WriteLine();
                Console.WriteLine("  1. Agent API (queries, sites, pages, jobs) - JWT only");
                Console.WriteLine("  2. Authoring API (CRUD, publishing, versions)");
                Console.WriteLine("  3. Edge API (fast read-only queries)");
                Console.WriteLine("  4. Command Scripts (bulk operations - CAUTION)");
                Console.WriteLine("  5. Content Hub API (read-only)");
                Console.WriteLine("  6. Run All APIs");
                Console.WriteLine("  0. Exit");
                Console.WriteLine();
                Console.Write("Select (0-6): ");

                var choice = Console.ReadLine()?.Trim();

                switch (choice)
                {
                    case "1":
                        await RunModuleAsync(ApiModule.Agent);
                        break;
                    case "2":
                        await RunModuleAsync(ApiModule.Authoring);
                        break;
                    case "3":
                        await RunModuleAsync(ApiModule.Edge);
                        break;
                    case "4":
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("\n⚠️  Command API examples perform bulk operations on test items.");
                        Console.WriteLine("They are safe (use test data) but destructive.\n");
                        Console.ResetColor();
                        if (VerificationHelper.PromptConfirmation("Continue?"))
                        {
                            await RunModuleAsync(ApiModule.Command);
                        }
                        break;
                    case "5":
                        await RunModuleAsync(ApiModule.ContentHub);
                        break;
                    case "6":
                        await RunAllModulesAsync();
                        return;
                    case "0":
                        return;
                    default:
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Invalid selection.");
                        Console.ResetColor();
                        break;
                }
            }
        }

        private static async Task<ExampleAuthMode> SelectAuthenticationModeAsync(ApiModule? singleModule)
        {
            _ = singleModule;

            while (true)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("Authentication mode:");
                Console.ResetColor();
                Console.WriteLine("  1. Auto (recommended)");
                Console.WriteLine("  2. JWT (JwtClientId/JwtClientSecret)");
                Console.WriteLine("  3. Sitecore CLI user.json");
                Console.WriteLine();
                Console.Write("Select auth mode: ");

                var choice = (Console.ReadLine() ?? string.Empty).Trim();
                switch (choice)
                {
                    case "":
                    case "1":
                        return ExampleAuthMode.Auto;
                    case "2":
                        return ExampleAuthMode.Jwt;
                    case "3":
                        return ExampleAuthMode.UserJson;
                    default:
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Invalid authentication mode selection.");
                        Console.ResetColor();
                        break;
                }

                await Task.Yield();
            }
        }

        /// <summary>
        /// Run a specific module's examples.
        /// </summary>
        private static async Task RunModuleAsync(ApiModule module)
        {
            ExampleBase? runner = null;

            try
            {
                runner = module switch
                {
                    ApiModule.Agent => new AgentApiExamples(),
                    ApiModule.Authoring => new AuthoringApiExamples(),
                    ApiModule.Edge => new EdgeApiExamples(),
                    ApiModule.Command => new CommandApiExamples(),
                    ApiModule.ContentHub => new ContentHubApiExamples(),
                    _ => null
                };

                if (runner == null)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Unknown module: {module}");
                    Console.ResetColor();
                    return;
                }

                // Initialize with authentication
                if (!await runner.InitializeAsync())
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Failed to initialize. Check your configuration.");
                    Console.ResetColor();
                    return;
                }

                // Run examples
                await runner.RunAsync();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error running {module} examples: {ex.Message}");
                Console.ResetColor();
                await VerificationHelper.LogAsync($"Exception in {module}: {ex}", ConsoleColor.Red);
            }
        }

        /// <summary>
        /// Run all API examples.
        /// </summary>
        private static async Task RunAllModulesAsync()
        {
            foreach (var module in Enum.GetValues(typeof(ApiModule)).Cast<ApiModule>())
            {
                await RunModuleAsync(module);
                Console.WriteLine();
            }
        }

        /// <summary>
        /// Quick verification that everything works (used for CI/health checks).
        /// Runs minimal tests: auth + agent queries + authoring read.
        /// </summary>
        public static async Task<bool> QuickVerifyAsync()
        {
            if (!await VerificationHelper.ValidateConfigAsync())
                return false;

            ExampleAuthContext.SelectedMode = ExampleAuthMode.Jwt;

            Console.WriteLine();
            VerificationHelper.PrintSectionHeader("Quick Verification");

            try
            {
                // Test Agent API
                var agentRunner = new AgentApiExamples();
                if (!await agentRunner.InitializeAsync())
                    return false;

                Console.WriteLine("✅ Authentication successful");

                // Quick site listing
                VerificationHelper.LogAsync("Running quick Agent API test...", ConsoleColor.Gray).Wait();
                return true;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"❌ Quick verification failed: {ex.Message}");
                Console.ResetColor();
                return false;
            }
        }
    }
}
