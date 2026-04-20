using SitecoreCommander.Edge;
using SitecoreCommander.Lib;

namespace SitecoreCommander.Examples
{
    /// <summary>
    /// Examples for Sitecore Edge API operations (GraphQL - read-only).
    /// Edge API is optimized for fast, read-only content queries.
    /// See: /AI/AGENT_API_PLAYBOOK.md
    /// </summary>
    public class EdgeApiExamples : ExampleBase
    {
        public override async Task RunAsync()
        {
            if (!EnsureEnvironmentExists())
                return;

            VerificationHelper.PrintSectionHeader("Sitecore Edge API Examples");

            await RunExampleAsync(
                "Get Sites from Edge",
                "List all sites using the Edge API (fast, read-only).",
                GetSitesFromEdgeExample);

            await RunExampleAsync(
                "Get Item from Edge",
                "Retrieve an item using the Edge API.",
                GetItemFromEdgeExample);

            await RunExampleAsync(
                "Get Children from Edge",
                "List child items using the Edge API.",
                GetChildrenFromEdgeExample);
        }

        /// <summary>
        /// Example: Get all sites from Edge API.
        /// </summary>
        private async Task GetSitesFromEdgeExample()
        {
            Console.WriteLine($"   Querying Sitecore sites...");
            Console.WriteLine();

            var result = await GetEdgeSites.Get(CreateEnvironmentConfig(), CancellationToken.None);

            if (result == null || result.Length == 0)
            {
                VerificationHelper.PrintFailure("Get Sites from Edge", "No sites found");
                return;
            }

            VerificationHelper.PrintSuccess(
                "Get Sites from Edge",
                $"Retrieved {result.Length} site(s)");

            foreach (var site in result.Take(5))
            {
                Console.WriteLine($"   • {site.name} ({site.hostname})");
            }

            if (result.Length > 5)
            {
                Console.WriteLine($"   ... and {result.Length - 5} more");
            }
        }

        /// <summary>
        /// Example: Get a specific item from Edge API.
        /// </summary>
        private async Task GetItemFromEdgeExample()
        {
            var itemPath = "/sitecore/content";
            Console.WriteLine($"   Retrieving item: {itemPath}");
            Console.WriteLine();

            var result = await GetItem.GetSitecoreItem(
                env: CreateEnvironmentConfig(),
                cancellationToken: CancellationToken.None,
                itemPath: itemPath,
                language: "en");

            if (result?.id == null)
            {
                VerificationHelper.PrintFailure("Get Item from Edge", "Could not find item");
                return;
            }

            VerificationHelper.PrintSuccess("Get Item from Edge", $"Retrieved: {result.name}");

            Console.WriteLine($"   ID: {result.id}");
            Console.WriteLine($"   Path: {result.path}");
            Console.WriteLine($"   Created: {result.created?.value ?? "(unknown)"}");
        }

        /// <summary>
        /// Example: Get child items from Edge API.
        /// </summary>
        private async Task GetChildrenFromEdgeExample()
        {
            var parentPath = "/sitecore/content";
            Console.WriteLine($"   Getting children of: {parentPath}");
            Console.WriteLine();

            var result = await GetChilderen.Get(
                env: CreateEnvironmentConfig(),
                cancellationToken: CancellationToken.None,
                itemPath: parentPath);

            if (result == null || result.Length == 0)
            {
                VerificationHelper.PrintFailure("Get Children from Edge", "No children found");
                return;
            }

            VerificationHelper.PrintSuccess(
                "Get Children from Edge",
                $"Retrieved {result.Length} child item(s)");

            foreach (var child in result.Take(5))
            {
                Console.WriteLine($"   • {child.name} ({child.id})");
            }

            if (result.Length > 5)
            {
                Console.WriteLine($"   ... and {result.Length - 5} more");
            }
        }

        protected override string GetResolvedHost()
        {
            if (ExampleAuthContext.SelectedMode == ExampleAuthMode.UserJson ||
                (ExampleAuthContext.SelectedMode == ExampleAuthMode.Auto && Environment != null && !string.IsNullOrWhiteSpace(Environment.Host)))
            {
                return Environment?.Host ?? GetHost();
            }

            var (host, source) = ResolveEdgeHost();
            Console.WriteLine($"   Edge host source: {source}");
            Console.WriteLine($"   Edge host: {host}");
            return host;
        }

        /// <summary>
        /// Resolve host for Edge API with explicit priority.
        /// </summary>
        private static (string Host, string Source) ResolveEdgeHost()
        {
            var configuredEdgeHost = Config.GetAppSetting("SitecoreCommander:EdgeApiHostname");
            if (!string.IsNullOrWhiteSpace(configuredEdgeHost))
            {
                return (configuredEdgeHost, "appsettings.Local.json (SitecoreCommander:EdgeApiHostname)");
            }

            try
            {
                if (!string.IsNullOrWhiteSpace(Config.XMCloudUserJsonPath) && File.Exists(Config.XMCloudUserJsonPath))
                {
                    var environment = SitecoreCommander.Lib.Login.GetSitecoreEnvironment();
                    if (!string.IsNullOrWhiteSpace(environment.Host))
                    {
                        return (environment.Host, $"{Config.XMCloudUserJsonPath} (endpoint '{Config.EnvironmentName}')");
                    }
                }
            }
            catch
            {
                // Fall through to the existing legacy/fallback host below.
            }

            return (Config.RestFullApiHostname, "appsettings.Local.json (SitecoreCommander:RestFullApiHostname fallback)");
        }
    }
}
