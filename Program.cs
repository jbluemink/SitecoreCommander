using SitecoreCommander;
using SitecoreCommander.Examples;
using SitecoreCommander.Utils;

// ============================================================================
// SitecoreCommander - Quick Start Guide
// ============================================================================
// 
// This application provides examples and utilities for working with Sitecore APIs.
// 
// CONFIGURATION:
//   Required: appsettings.Local.json with either JWT credentials OR XMCloud user.json path
//   Example:  See /Examples/README.md
//
// QUICK START:
//   1. Update appsettings.local.json with your credentials
//   2. Set ENABLE_EXAMPLES = true (below) to run API examples
//   3. dotnet run
//   4. Follow the interactive menu
//
// ============================================================================

// Initialize logging
SimpleLogger.InitializeLogFile();
await VerificationHelper.LogAsync("SitecoreCommander started", ConsoleColor.Gray);

// ============================================================================
// CONFIGURATION: Toggle Example Modes
// ============================================================================

// Set ENABLE_EXAMPLES = true to run the examples framework with interactive menu
const bool ENABLE_EXAMPLES = true;

// Optional: Run only a specific API module (null = all modules)
ExampleRunner.ApiModule? singleModule = null;
// Examples:
// ExampleRunner.ApiModule singleModule = ExampleRunner.ApiModule.Agent;     // Only Agent API
// ExampleRunner.ApiModule singleModule = ExampleRunner.ApiModule.Authoring; // Only Authoring API

// Optional: Quick verification mode (fast health check)
const bool QUICK_VERIFY = false;

// ============================================================================
// MAIN ENTRY POINT
// ============================================================================

try
{
    if (QUICK_VERIFY)
    {
        // Fast health check
        var success = await ExampleRunner.QuickVerifyAsync();
        Environment.Exit(success ? 0 : 1);
    }
    else if (ENABLE_EXAMPLES)
    {
        // Run examples with interactive menu
        await ExampleRunner.RunAsync(singleModule);
    }
    else
    {
        // Default: Your custom code here
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("📝 SitecoreCommander initialized");
        Console.WriteLine("   To run examples, set ENABLE_EXAMPLES = true in Program.cs");
        Console.ResetColor();
        Console.WriteLine();
    }

    Environment.ExitCode = 0;
}
catch (Exception ex)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"Fatal error: {ex.Message}");
    Console.ResetColor();
    await VerificationHelper.LogAsync($"Fatal error: {ex}", ConsoleColor.Red);
    Environment.ExitCode = 1;
}

//Console.ReadKey();
