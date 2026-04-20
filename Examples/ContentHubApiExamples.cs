using SitecoreCommander.ContentHub.Auth;
using SitecoreCommander.ContentHub.Model;
using SitecoreCommander.ContentHub.Services;

namespace SitecoreCommander.Examples
{
    /// <summary>
    /// Read-only examples for Content Hub API operations.
    /// This module only performs queries and does not create or update entities.
    /// </summary>
    public sealed class ContentHubApiExamples : ExampleBase
    {
        private ContentHubAssetService? _assetService;

        public override async Task<bool> InitializeAsync()
        {
            try
            {
                await VerificationHelper.LogAsync("Initializing Content Hub client...", ConsoleColor.Gray);

                var options = ContentHubOptions.FromEnvironment();
                ContentHubOptions.ValidateRequired(options);

                var client = ContentHubClientFactory.CreateClient(options);
                _assetService = new ContentHubAssetService(client, options);

                await VerificationHelper.LogAsync("✅ Content Hub client initialized", ConsoleColor.Green);
                return true;
            }
            catch (Exception ex)
            {
                await VerificationHelper.LogAsync(
                    $"Content Hub configuration missing or invalid: {ex.Message}",
                    ConsoleColor.Yellow);
                await VerificationHelper.LogAsync(
                    "Skipping Content Hub examples. Add ContentHub settings in appsettings.Local.json to enable this module.",
                    ConsoleColor.Yellow);
                return false;
            }
        }

        public override async Task RunAsync()
        {
            if (_assetService == null)
                return;

            VerificationHelper.PrintSectionHeader("Content Hub API Examples (Read-Only)");

            await RunExampleAsync(
                "List Assets",
                "Query Content Hub assets and print a small sample without modifying data.",
                ListAssetsReadOnlyExample);
        }

        private async Task ListAssetsReadOnlyExample()
        {
            var assets = await _assetService!.GetAssetsAsync(maxCount: 10);

            VerificationHelper.PrintSuccess(
                "List Assets",
                $"Retrieved {assets.Count} asset(s)");

            if (assets.Count == 0)
            {
                Console.WriteLine("   No assets found.");
                return;
            }

            foreach (var asset in assets)
            {
                Console.WriteLine($"   • id={asset.Id}");
            }
        }
    }
}
