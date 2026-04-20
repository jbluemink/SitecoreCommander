using SitecoreCommander.Authoring;
using SitecoreCommander.Authoring.Model;
using SitecoreCommander.Edge;

namespace SitecoreCommander.Examples
{
    /// <summary>
    /// Test data setup for examples. Creates and manages test items under /sitecore/content/SitecoreCommander
    /// Uses template IDs: 
    /// - Folder: {A87A00B1-E6DB-45AB-8B54-636FEC3B5523}
    /// - Sample Item: {AB86861A-6030-46C5-B394-E8F99E8B87DB}
    /// </summary>
    public class TestDataSetup
    {
        public const string TestRootPath = "/sitecore/content/SitecoreCommander";
        public const string FolderTemplateId = "A87A00B1-E6DB-45AB-8B54-636FEC3B5523";
        public const string SampleItemTemplateId = "76036F5E-CBCE-46D1-AF0A-4143F9B557AA";
        public const string ContentRootPath = "/sitecore/content";

        private readonly EnvironmentConfiguration _environment;
        private readonly CancellationToken _cancellationToken;

        public TestDataSetup(EnvironmentConfiguration environment, CancellationToken cancellationToken = default)
        {
            _environment = environment ?? throw new ArgumentNullException(nameof(environment));
            _cancellationToken = cancellationToken;
        }

        /// <summary>
        /// Ensure test root folder exists. Returns the folder item or null if creation failed.
        /// </summary>
        public async Task<string?> EnsureTestRootFolderAsync()
        {
            try
            {
                await VerificationHelper.LogAsync($"Checking if test root folder exists: {TestRootPath}", ConsoleColor.Gray);

                // Try to get existing test folder using Edge API (read-only)
                var existingFolder = await GetItem.GetSitecoreItem(CreateEnvironmentConfig(), _cancellationToken, TestRootPath, "en");

                if (existingFolder != null)
                {
                    await VerificationHelper.LogAsync($"✅ Test root folder already exists: {existingFolder.id}", ConsoleColor.Green);
                    return existingFolder.id;
                }

                // Create test root folder if it doesn't exist
                await VerificationHelper.LogAsync($"Creating test root folder...", ConsoleColor.Yellow);

                var parentItem = await GetItem.GetSitecoreItem(CreateEnvironmentConfig(), _cancellationToken, ContentRootPath, "en");
                if (parentItem == null)
                {
                    await VerificationHelper.LogAsync($"❌ Could not find parent path: {ContentRootPath}", ConsoleColor.Red);
                    return null;
                }

                var folderResult = await CreateFolderItem.CreateMap(
                    env: CreateEnvironmentConfig(),
                    cancellationToken: _cancellationToken,
                    itemname: "SitecoreCommander",
                    templateId: FolderTemplateId,
                    parentID: parentItem.id,
                    language: "en",
                    additionalLanguages: []);

                if (folderResult?.itemId != null)
                {
                    await VerificationHelper.LogAsync($"✅ Test root folder created: {folderResult.itemId}", ConsoleColor.Green);
                    return folderResult.itemId;
                }

                await VerificationHelper.LogAsync($"❌ Failed to create test root folder", ConsoleColor.Red);
                return null;
            }
            catch (Exception ex)
            {
                await VerificationHelper.LogAsync($"❌ Error ensuring test folder: {ex.Message}", ConsoleColor.Red);
                return null;
            }
        }

        /// <summary>
        /// Create a test sample item under the test root folder.
        /// </summary>
        public async Task<string?> CreateTestItemAsync(string itemName, string title, string textContent)
        {
            try
            {
                var rootFolderId = await EnsureTestRootFolderAsync();
                if (rootFolderId == null)
                {
                    return null;
                }

                // Check if item already exists
                var itemPath = $"{TestRootPath}/{itemName}";
                var existingItem = await GetItem.GetSitecoreItem(CreateEnvironmentConfig(), _cancellationToken, itemPath, "en");

                if (existingItem != null)
                {
                    await VerificationHelper.LogAsync($"Test item already exists: {itemPath}", ConsoleColor.Yellow);
                    return existingItem.id;
                }

                // Create the sample item
                var fieldValues = new Dictionary<string, string>
                {
                    { "Title", title },
                    { "Text", textContent }
                };

                var newItem = await AddItem.Create(
                    env: CreateEnvironmentConfig(),
                    cancellationToken: _cancellationToken,
                    itemname: itemName,
                    templateId: SampleItemTemplateId,
                    parentID: rootFolderId,
                    language: "en",
                    fields: fieldValues);

                if (newItem?.itemId != null)
                {
                    await VerificationHelper.LogAsync($"✅ Test item created: {itemPath} (ID: {newItem.itemId})", ConsoleColor.Green);
                    return newItem.itemId;
                }

                await VerificationHelper.LogAsync($"❌ Failed to create test item: {itemName}", ConsoleColor.Red);
                return null;
            }
            catch (Exception ex)
            {
                await VerificationHelper.LogAsync($"❌ Error creating test item: {ex.Message}", ConsoleColor.Red);
                return null;
            }
        }

        /// <summary>
        /// Delete the test root folder and all its contents (with confirmation).
        /// </summary>
        public async Task<bool> CleanupTestDataAsync()
        {
            try
            {
                if (!VerificationHelper.PromptConfirmation($"Delete test folder and all items under {TestRootPath}?"))
                {
                    return false;
                }

                var rootFolder = await GetItem.GetSitecoreItem(CreateEnvironmentConfig(), _cancellationToken, TestRootPath, "en");
                if (rootFolder == null)
                {
                    await VerificationHelper.LogAsync($"Test folder not found, nothing to delete", ConsoleColor.Yellow);
                    return true;
                }

                var deleteResult = await DeleteItem.Delete(
                    env: CreateEnvironmentConfig(),
                    cancellationToken: _cancellationToken,
                    itemPath: TestRootPath);

                if (deleteResult)
                {
                    await VerificationHelper.LogAsync($"✅ Test folder deleted successfully", ConsoleColor.Green);
                    return true;
                }

                await VerificationHelper.LogAsync($"❌ Failed to delete test folder", ConsoleColor.Red);
                return false;
            }
            catch (Exception ex)
            {
                await VerificationHelper.LogAsync($"❌ Error during cleanup: {ex.Message}", ConsoleColor.Red);
                return false;
            }
        }

        /// <summary>
        /// Create an EnvironmentConfiguration from JWT token.
        /// </summary>
        private EnvironmentConfiguration CreateEnvironmentConfig()
        {
            return _environment;
        }
    }
}
