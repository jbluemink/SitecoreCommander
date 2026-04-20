using SitecoreCommander.Command;

namespace SitecoreCommander.Examples
{
    /// <summary>
    /// Examples for Sitecore Command API operations - Bulk/batch operations on item subtrees.
    /// ⚠️  CAUTION: These are destructive operations that modify multiple items at once.
    /// All examples use the test data folder to ensure safety.
    /// See: /AI/AGENT_API_PLAYBOOK.md
    /// </summary>
    public class CommandApiExamples : ExampleBase
    {
        public override async Task RunAsync()
        {
            if (!EnsureEnvironmentExists())
                return;

            VerificationHelper.PrintSectionHeader("Sitecore Command API Examples (BULK OPERATIONS)");

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("⚠️  WARNING: These examples perform batch operations on multiple items.");
            Console.WriteLine("   They only operate on test items under /sitecore/content/SitecoreCommander for safety.");
            Console.ResetColor();
            Console.WriteLine();

            await RunExampleAsync(
                "Replace Field (Subtree)",
                "Replace a specific field value across multiple items.",
                ReplaceFieldExample);

            await RunExampleAsync(
                "Unpublish Language (Subtree)",
                "⚠️  Unpublish a language from all items in a folder tree.",
                UnpublishLanguageExample);

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("📝 Note: Additional command examples (Delete, Move) require more setup.");
            Console.WriteLine("   Refer to the Command API documentation for these operations.");
            Console.ResetColor();
        }

        /// <summary>
        /// Example: Replace a field value across multiple items in a subtree.
        /// </summary>
        private async Task ReplaceFieldExample()
        {
            var targetPath = TestDataSetup.TestRootPath;
            Console.WriteLine($"   Target: {targetPath}");
            Console.WriteLine($"   Field: Title");
            Console.WriteLine($"   Find: \"example\" (case-insensitive)");
            Console.WriteLine($"   Replace with: \"UPDATED\"");
            Console.WriteLine();

            if (!VerificationHelper.PromptConfirmation("Replace field values across all items?"))
            {
                Console.WriteLine("   Skipped.");
                return;
            }

            try
            {
                var result = await ReplaceFieldFromSubtree.ReplaceAsync(
                    env: CreateEnvironmentConfig(),
                    path: targetPath,
                    language: "en",
                    fieldname: "Title",
                    replaceOld: "example",
                    replaceNew: "UPDATED");

                if (result)
                {
                    VerificationHelper.PrintSuccess(
                        "Replace Field in Subtree",
                        $"Successfully replaced field values");
                    await VerificationHelper.LogAsync("Field replacement completed", ConsoleColor.Green);
                }
                else
                {
                    VerificationHelper.PrintFailure("Replace Field in Subtree", "No items matched the criteria");
                }
            }
            catch (Exception ex)
            {
                VerificationHelper.PrintFailure("Replace Field in Subtree", ex.Message);
            }
        }

        /// <summary>
        /// Example: Unpublish a language from all items in a subtree.
        /// </summary>
        private async Task UnpublishLanguageExample()
        {
            var targetPath = TestDataSetup.TestRootPath;
            var language = "en";
            Console.WriteLine($"   Target: {targetPath}");
            Console.WriteLine($"   Language: {language}");
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"⚠️  This will UNPUBLISH all items in '{language}' under {targetPath}");
            Console.ResetColor();
            Console.WriteLine();

            if (!VerificationHelper.PromptConfirmation("Proceed with unpublishing?"))
            {
                Console.WriteLine("   Skipped.");
                return;
            }

            try
            {
                var result = await UnpublishLanguageFromSubtree.EditAsync(
                    env: CreateEnvironmentConfig(),
                    path: targetPath,
                    language: language);

                if (result)
                {
                    VerificationHelper.PrintSuccess(
                        "Unpublish Language from Subtree",
                        $"Successfully unpublished items");
                    await VerificationHelper.LogAsync("Unpublish operation completed", ConsoleColor.Yellow);
                }
                else
                {
                    VerificationHelper.PrintFailure("Unpublish Language from Subtree", "No items affected");
                }
            }
            catch (Exception ex)
            {
                VerificationHelper.PrintFailure("Unpublish Language from Subtree", ex.Message);
            }
        }

    }
}
