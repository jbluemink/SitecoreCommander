using SitecoreCommander.Authoring;

namespace SitecoreCommander.Examples
{
    /// <summary>
    /// Examples for Sitecore Authoring API operations (GraphQL).
    /// Includes CRUD operations, publishing, and item management.
    /// ⚠️  CAUTION: Write operations included - test data is created under /sitecore/content/SitecoreCommander
    /// See: /AI/placeholder-implementation-guide.md
    /// </summary>
    public class AuthoringApiExamples : ExampleBase
    {
        private TestDataSetup? _testDataSetup;

        public override async Task RunAsync()
        {
            if (!EnsureEnvironmentExists())
                return;

            _testDataSetup = new TestDataSetup(CreateEnvironmentConfig(), CancellationToken.None);

            VerificationHelper.PrintSectionHeader("Sitecore Authoring API Examples");

            // Setup test data first
            await RunExampleAsync(
                "Setup Test Data",
                "Create test items under /sitecore/content/SitecoreCommander for examples.",
                SetupTestDataExample);

            await RunExampleAsync(
                "Read Item",
                "Retrieve an item with all its fields using the Authoring API.",
                ReadItemExample);

            await RunExampleAsync(
                "Update Item",
                "Modify item fields and save changes.",
                UpdateItemExample);

            await RunExampleAsync(
                "Add Item Version",
                "Create a new language version of an existing item.",
                AddItemVersionExample);

            await RunExampleAsync(
                "Get Item Children",
                "List all child items of a parent item.",
                GetItemChildrenExample);

            await RunExampleAsync(
                "Publish Item",
                "Publish an item to the web database.",
                PublishItemExample);

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("ℹ️  Test items created under: /sitecore/content/SitecoreCommander");
            Console.WriteLine("   Run CleanupTestData to remove them later.");
            Console.ResetColor();
        }

        /// <summary>
        /// Example: Setup test data for other examples.
        /// </summary>
        private async Task SetupTestDataExample()
        {
            if (_testDataSetup == null)
            {
                VerificationHelper.PrintFailure("Setup Test Data", "TestDataSetup not initialized");
                return;
            }

            var testItemId = await _testDataSetup.CreateTestItemAsync(
                itemName: "ExampleTestItem",
                title: "Authoring API Test Item",
                textContent: "This is a test item created by the Authoring API Examples. Feel free to delete it.");

            if (testItemId != null)
            {
                VerificationHelper.PrintSuccess("Setup Test Data", $"Test item created (ID: {testItemId})");
                await VerificationHelper.LogAsync($"Test data is ready for examples", ConsoleColor.Green);
            }
            else
            {
                VerificationHelper.PrintFailure("Setup Test Data", "Failed to create test item");
            }
        }

        /// <summary>
        /// Example: Read an item with all its fields.
        /// </summary>
        private async Task ReadItemExample()
        {
            var itemPath = $"{TestDataSetup.TestRootPath}/ExampleTestItem";
            Console.WriteLine($"   Reading item: {itemPath}");
            Console.WriteLine();

            var result = await GetItemWithAllFields.Get(
                env: CreateEnvironmentConfig(),
                cancellationToken: CancellationToken.None,
                itemPath: itemPath,
                language: "en");

            if (result?.itemId == null)
            {
                VerificationHelper.PrintFailure("Read Item", "Could not find item");
                return;
            }

            VerificationHelper.PrintSuccess("Read Item", $"Retrieved item: {result.name}");

            Console.WriteLine($"   ID: {result.itemId}");
            Console.WriteLine($"   Path: {result.path}");
            Console.WriteLine($"   Version: {result.version}");
            Console.WriteLine($"   Template: {result.template?.name ?? "(unknown)"}");

            if (result.fields?.Count > 0)
            {
                Console.WriteLine($"   Fields ({result.fields.Count}):");
                foreach (var field in result.fields.Take(5))
                {
                    var value = field.Value?.value ?? "(empty)";
                    var displayValue = value.Length > 40 ? value.Substring(0, 40) + "..." : value;
                    Console.WriteLine($"      - {field.Key}: {displayValue}");
                }
                if (result.fields.Count > 5)
                {
                    Console.WriteLine($"      ... and {result.fields.Count - 5} more");
                }
            }
        }

        /// <summary>
        /// Example: Update item fields.
        /// </summary>
        private async Task UpdateItemExample()
        {
            // First get the item to find its ID
            var itemPath = $"{TestDataSetup.TestRootPath}/ExampleTestItem";
            var getResult = await GetItemWithAllFields.Get(
                env: CreateEnvironmentConfig(),
                cancellationToken: CancellationToken.None,
                itemPath: itemPath,
                language: "en");

            if (getResult?.itemId == null)
            {
                VerificationHelper.PrintFailure("Update Item", "Could not find test item");
                return;
            }

            Console.WriteLine($"   Updating item: {itemPath}");
            Console.WriteLine();

            if (!VerificationHelper.PromptConfirmation($"Update '{itemPath}' with new values?"))
            {
                Console.WriteLine("   Skipped.");
                return;
            }

            var updates = new Dictionary<string, string>
            {
                { "Title", $"Updated Title - {DateTime.Now:yyyy-MM-dd HH:mm:ss}" },
                { "Text", "This item was updated by the Authoring API Examples." }
            };

            var result = await UpdateItem.Update(
                token: GetJwtTokenForLegacyWrappers(),
                host: CreateEnvironmentConfig().Host,
                cancellationToken: CancellationToken.None,
                itemId: getResult.itemId,
                language: "en",
                fields: updates);

            if (result?.itemId == null)
            {
                VerificationHelper.PrintFailure("Update Item", "Update failed or returned invalid response");
                return;
            }

            VerificationHelper.PrintSuccess("Update Item", $"Item updated successfully");
            Console.WriteLine($"   Updated fields: {string.Join(", ", updates.Keys)}");
        }

        /// <summary>
        /// Example: Add a language version to an item.
        /// </summary>
        private async Task AddItemVersionExample()
        {
            // First get the item to find its ID
            var itemPath = $"{TestDataSetup.TestRootPath}/ExampleTestItem";
            var getResult = await GetItemWithAllFields.Get(
                env: CreateEnvironmentConfig(),
                cancellationToken: CancellationToken.None,
                itemPath: itemPath,
                language: "en");

            if (getResult?.itemId == null)
            {
                VerificationHelper.PrintFailure("Add Item Version", "Could not find test item");
                return;
            }

            Console.WriteLine($"   Adding language version to: {itemPath}");
            Console.WriteLine();

            if (!VerificationHelper.PromptConfirmation($"Create a 'da' (Danish) version of '{itemPath}'?"))
            {
                Console.WriteLine("   Skipped.");
                return;
            }

            var result = await AddItemVersion.Add(
                token: GetJwtTokenForLegacyWrappers(),
                host: CreateEnvironmentConfig().Host,
                cancellationToken: CancellationToken.None,
                itemId: getResult.itemId,
                language: "da");

            if (result?.itemId == null)
            {
                VerificationHelper.PrintFailure("Add Item Version", "Failed to create language version");
                return;
            }

            VerificationHelper.PrintSuccess("Add Item Version", $"Danish version created successfully");
        }

        /// <summary>
        /// Example: Get child items of a parent.
        /// </summary>
        private async Task GetItemChildrenExample()
        {
            var parentPath = TestDataSetup.TestRootPath;
            Console.WriteLine($"   Getting children of: {parentPath}");
            Console.WriteLine();

            var result = await GetItemChildren.GetAll(
                token: GetJwtTokenForLegacyWrappers(),
                host: CreateEnvironmentConfig().Host,
                cancellationToken: CancellationToken.None,
                itemPath: parentPath);

            if (result == null || result.Count == 0)
            {
                VerificationHelper.PrintSuccess("Get Item Children", "No child items found");
                return;
            }

            VerificationHelper.PrintSuccess("Get Item Children", $"Found {result.Count} child item(s)");

            foreach (var child in result.Take(5))
            {
                Console.WriteLine($"   • {child.name} ({child.itemId})");
            }

            if (result.Count > 5)
            {
                Console.WriteLine($"   ... and {result.Count - 5} more");
            }
        }

        /// <summary>
        /// Example: Publish an item to the web database.
        /// </summary>
        private async Task PublishItemExample()
        {
            // First get the item to find its ID
            var itemPath = $"{TestDataSetup.TestRootPath}/ExampleTestItem";
            var getResult = await GetItemWithAllFields.Get(
                env: CreateEnvironmentConfig(),
                cancellationToken: CancellationToken.None,
                itemPath: itemPath,
                language: "en");

            if (getResult?.itemId == null)
            {
                VerificationHelper.PrintFailure("Publish Item", "Could not find test item");
                return;
            }

            Console.WriteLine($"   Publishing item: {itemPath}");
            Console.WriteLine();

            if (!VerificationHelper.PromptConfirmation($"Publish '{itemPath}' to web database?"))
            {
                Console.WriteLine("   Skipped.");
                return;
            }

            var result = await Publish.PublishItem(
                token: GetJwtTokenForLegacyWrappers(),
                host: CreateEnvironmentConfig().Host,
                cancellationToken: CancellationToken.None,
                itemId: getResult.itemId,
                languages: new[] { "en" });

            if (result == null)
            {
                VerificationHelper.PrintFailure("Publish Item", "Publish operation failed");
                return;
            }

            VerificationHelper.PrintSuccess("Publish Item", $"Item published successfully");
        }

    }
}
