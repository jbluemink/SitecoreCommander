using SitecoreCommander.Agent;
using SitecoreCommander.Login;
using SitecoreCommander.Utils;

namespace SitecoreCommander.Examples
{
    /// <summary>
    /// Examples for Sitecore Agent API operations.
    /// These are fast, read-only operations for querying content and managing jobs.
    /// See: /AI/AGENT_API_PLAYBOOK.md
    /// </summary>
    public class AgentApiExamples : ExampleBase
    {
        public override async Task RunAsync()
        {
            if (!EnsureEnvironmentExists())
                return;

            // Agent API requires JWT authentication (cloud endpoint requirement)
            // User.json tokens are for XM Cloud CM endpoints, not the Agent endpoint
            if (ExampleAuthContext.SelectedMode == ExampleAuthMode.UserJson)
            {
                await VerificationHelper.LogAsync(
                    "❌ Agent API requires JWT authentication",
                    ConsoleColor.Red);
                await VerificationHelper.LogAsync(
                    "User.json tokens are configured for XM Cloud content management endpoints, " +
                    "not the cloud Agent endpoint. Please select JWT mode to use Agent API.",
                    ConsoleColor.Yellow);
                return;
            }

            var token = GetJwtTokenForLegacyWrappers();

            VerificationHelper.PrintSectionHeader("Sitecore Agent API Examples");

            await RunExampleAsync(
                "List All Sites",
                "Retrieve all Sitecore sites from the environment.",
                () => ListAllSitesExample(token));

            await RunExampleAsync(
                "Get Site Pages",
                "List all pages under a specific site.",
                () => GetSitePagesExample(token));

            await RunExampleAsync(
                "Get Item Details",
                "Retrieve detailed information about a specific item/page.",
                () => GetItemDetailsExample(token));

            await RunExampleAsync(
                "List Job Operations",
                "Query job management operations and history.",
                () => ListJobOperationsExample(token));
        }

        /// <summary>
        /// Example: List all available sites in the Sitecore environment.
        /// </summary>
        private async Task ListAllSitesExample(JwtTokenResponse token)
        {
            var result = await ListSites.GetSites(token, CancellationToken.None, "");

            var verification = await VerificationHelper.VerifyResponseAsync(
                result,
                "List Sites",
                r => r?.Sites != null && r.Sites.Count > 0);

            if (!verification.Success)
            {
                VerificationHelper.PrintFailure("List Sites", verification.ErrorDetails);
                return;
            }

            VerificationHelper.PrintSuccess("List Sites", $"Found {verification.Data!.Sites.Count} site(s)");

            foreach (var site in verification.Data.Sites.Take(5))
            {
                Console.WriteLine($"   • {site.Name} ({site.TargetHostname})");
            }

            if (verification.Data.Sites.Count > 5)
            {
                Console.WriteLine($"   ... and {verification.Data.Sites.Count - 5} more");
            }

            await VerificationHelper.LogAsync(
                $"Successfully listed {verification.Data.Sites.Count} sites",
                ConsoleColor.Green);
        }

        /// <summary>
        /// Example: Get pages from the first available site.
        /// </summary>
        private async Task GetSitePagesExample(JwtTokenResponse token)
        {
            // First, get a site
            var sitesResult = await ListSites.GetSites(token, CancellationToken.None, "");
            if (sitesResult?.Sites == null || sitesResult.Sites.Count == 0)
            {
                VerificationHelper.PrintFailure("Get Site Pages", "No sites found");
                return;
            }

            var firstSite = sitesResult.Sites[0];
            Console.WriteLine($"   Using site: {firstSite.Name}");
            Console.WriteLine();

            // Now get pages
            var pagesResult = await ListSitListPagesOfASitees.GetPages(
                token,
                CancellationToken.None,
                firstSite.Name,
                language: "en",
                "");

            var verification = await VerificationHelper.VerifyResponseAsync(
                pagesResult,
                "Get Site Pages",
                r => r?.Items != null && r.Items.Count > 0);

            if (!verification.Success)
            {
                VerificationHelper.PrintFailure("Get Site Pages", verification.ErrorDetails);
                return;
            }

            VerificationHelper.PrintSuccess(
                "Get Site Pages",
                $"Found {verification.Data!.Items.Count} page(s) in {firstSite.Name}");

            foreach (var page in verification.Data.Items.Take(5))
            {
                Console.WriteLine($"   • {page.Path} ({page.Id})");
            }

            if (verification.Data.Items.Count > 5)
            {
                Console.WriteLine($"   ... and {verification.Data.Items.Count - 5} more");
            }
        }

        /// <summary>
        /// Example: Get detailed item information by ID.
        /// </summary>
        private async Task GetItemDetailsExample(JwtTokenResponse token)
        {
            // Get a page first to demonstrate retrieving its details
            var sitesResult = await ListSites.GetSites(token, CancellationToken.None, "");
            if (sitesResult?.Sites == null || sitesResult.Sites.Count == 0)
            {
                VerificationHelper.PrintFailure("Get Item Details", "No sites found");
                return;
            }

            var pagesResult = await ListSitListPagesOfASitees.GetPages(
                token,
                CancellationToken.None,
                sitesResult.Sites[0].Name,
                "en",
                "");

            if (pagesResult?.Items == null || pagesResult.Items.Count == 0)
            {
                VerificationHelper.PrintFailure("Get Item Details", "No pages found");
                return;
            }

            var firstPage = pagesResult.Items[0];
            Console.WriteLine($"   Retrieving details for: {firstPage.Path}");
            Console.WriteLine();

            var detailsResult = await RetrieveThePageDetails.GetItemById(
                token,
                CancellationToken.None,
                firstPage.Id);

            var verification = await VerificationHelper.VerifyResponseAsync(
                detailsResult,
                "Get Item Details",
                r => !string.IsNullOrWhiteSpace(r?.ItemId));

            if (!verification.Success)
            {
                VerificationHelper.PrintFailure("Get Item Details", verification.ErrorDetails);
                return;
            }

            VerificationHelper.PrintSuccess(
                "Get Item Details",
                $"Retrieved item: {verification.Data!.ItemId}");

            Console.WriteLine($"   Name: {verification.Data.Name}");
            Console.WriteLine($"   Path: {verification.Data.Path}");
            Console.WriteLine($"   Version: {verification.Data.Version}");

            var fields = verification.Data.Fields;
            if (fields == null || fields.Count == 0)
            {
                Console.WriteLine("   Fields: 0 (item details returned without field values)");
                Console.WriteLine("   Tip: this can happen for certain templates, field-level security, or API response shape differences.");
                return;
            }

            Console.WriteLine($"   Fields ({fields.Count}):");
            foreach (var field in fields.Take(5))
            {
                Console.WriteLine($"   • {field.Key}: {(string.IsNullOrEmpty(field.Value) ? "(empty)" : field.Value.Substring(0, Math.Min(50, field.Value.Length)))}");
            }

            if (fields.Count > 5)
            {
                Console.WriteLine($"   ... and {fields.Count - 5} more");
            }
        }

        /// <summary>
        /// Example: List job operations and history.
        /// </summary>
        private async Task ListJobOperationsExample(JwtTokenResponse token)
        {
            // Job operations require a job ID. For demo purposes, we'll try to query with empty jobId
            var result = await ListJobOperations.GetJob(token, CancellationToken.None, "demo");

            if (result == null)
            {
                VerificationHelper.PrintFailure("List Job Operations", "No job operations found (expected for demo)");
                Console.WriteLine("   Job operations are used to track long-running bulk operations.");
                Console.WriteLine("   You would provide a real job ID to retrieve operation history.");
                return;
            }

            VerificationHelper.PrintSuccess("List Job Operations", $"Retrieved {result.Count} operation(s)");
            await VerificationHelper.LogAsync(
                "Job operations can be used to track long-running operations",
                ConsoleColor.Gray);
        }


    }
}
