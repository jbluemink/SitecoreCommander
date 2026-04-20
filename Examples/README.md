# SitecoreCommander Examples & Quick Start

Welcome to **SitecoreCommander**! This guide will help you quickly get started testing Sitecore APIs.

## 🚀 Quick Start (5 minutes)

### 1. Configure Authentication (Choose One)

#### Option A: JWT client credentials

Update `appsettings.Local.json`:

```json
{
  "SitecoreCommander": {
    "EnvironmentName": "your-environment-name",
    "JwtClientId": "your-jwt-client-id",
    "JwtClientSecret": "your-jwt-client-secret",
    "ApiKey": "your-api-key",
    "DefaultLanguage": "en",
    "RestFullApiHostname": "https://xmcloudcm.localhost",
    "RestFullSitecoreUser": "admin",
    "RestFullSitecorePassword": "your-password"
  }
}
```

> **Where to find your credentials?**
> - **JWT Client ID/Secret**: See https://deploy.sitecorecloud.io/credentials/environment
> - **Environment Name**: Your XM Cloud environment identifier
> - **API Key**: This must be the Sitecore item API key from `/sitecore/system/Settings/Services/API Keys`
> - **API Key config**: Set it in `appsettings.local.json` as `SitecoreCommander:ApiKey`

#### Option B: Sitecore CLI `user.json`

1. Install Sitecore CLI if needed:

```bash
dotnet tool install -g Sitecore.CLI
```

2. Login with Sitecore CLI:

```bash
dotnet sitecore cloud login
```

3. Set `XMCloudUserJsonPath` in `appsettings.Local.json`:

```json
{
  "SitecoreCommander": {
    "XMCloudUserJsonPath": "C:\\Users\\you\\.sitecore\\user.json",
    "EnvironmentName": "default",
    "ApiKey": "your-api-key"
  }
}
```

4. At runtime, choose auth mode `Sitecore CLI user.json` in the auth menu.

### 2. Enable Examples in Program.cs

In `Program.cs`, set:

```csharp
const bool ENABLE_EXAMPLES = true;
```

### 3. Run the Application

```bash
dotnet run
```

You'll see an interactive menu:

```
Authentication mode:

  1. Auto (recommended)
  2. JWT (JwtClientId/JwtClientSecret)
  3. Sitecore CLI user.json
```

Then the API module menu:

```
Which API would you like to test?

  1. Agent API (queries, sites, pages, jobs)
  2. Authoring API (CRUD, publishing, versions)
  3. Edge API (fast read-only queries)
  4. Command Scripts (bulk operations - CAUTION)
  5. Content Hub API (read-only)
  6. Run All APIs
  0. Exit

Select (0-6): _
```

---

## 📋 Available Examples

### 1️⃣ **Agent API** (Read-Only Queries)
Fast, efficient queries for content and site information.

> **Cloud note**: Agent API wrappers call the Sitecore cloud endpoint (`edge-platform.sitecorecloud.io`).
> A local-only Sitecore context is not sufficient by itself; your token/tenant must be valid for the cloud Agent endpoint.

| Example | Purpose |
|---------|---------|
| List All Sites | Get all Sitecore sites |
| Get Site Pages | Retrieve pages from a specific site |
| Get Item Details | Read detailed item information |
| List Job Operations | Query job management operations |

**Use Case**: Querying content structure, checking what exists before making changes.

**Documentation**: See [AGENT_API_PLAYBOOK.md](../AI/AGENT_API_PLAYBOOK.md)

### 2️⃣ **Authoring API** (CRUD + Publishing)
Create, read, update, delete items with full control.

| Example | Purpose |
|---------|---------|
| Setup Test Data | Create test items automatically |
| Read Item | Retrieve item with all fields |
| Update Item | Modify item fields |
| Add Item Version | Create language versions |
| Get Item Children | List child items |
| Publish Item | Publish to web database |

> **⚠️ CAUTION**: These examples create and modify items.
> - All changes are made to test data only: `/sitecore/content/SitecoreCommander/`
> - You'll be asked to confirm before any write operation
> - Test items remain after examples (good for inspection)

**Use Case**: Content migration, item creation, field updates, publishing workflows.

**Documentation**: See [placeholder-implementation-guide.md](../AI/placeholder-implementation-guide.md)

### 3️⃣ **Edge API** (Fast Read-Only)
GraphQL queries for efficient content retrieval.

| Example | Purpose |
|---------|---------|
| Get Sites from Edge | List sites (optimized) |
| Get Item from Edge | Retrieve item (optimized) |
| Get Item Versions | Get all language versions |
| Get Children from Edge | List child items (optimized) |

**Use Case**: High-performance read operations, headless content delivery.

### 4️⃣ **Command API** (Bulk Operations)
Batch operations on multiple items at once.

> **🚨 HIGHLY CAUTION**: Destructive bulk operations.
> - All examples use test data only
> - You'll see a warning before running
> - You must confirm each operation

| Example | Purpose |
|---------|---------|
| Delete Items | Remove items matching criteria |
| Replace Field | Update field across subtree |
| Move Items | Relocate items to new folder |
| Unpublish Language | Unpublish from language/region |

**Use Case**: Content cleanup, reorganization, bulk field updates, unpublishing.

### 5️⃣ **Content Hub API** (Read-Only)
Read-only checks against Sitecore Content Hub for migration validation.

| Example | Purpose |
|---------|---------|
| List Assets by Legacy Sitecore ID | Verify asset mappings without changing Content Hub data |

**Use Case**: Validate Content Hub connectivity and migration mappings safely.

**Required config (appsettings.local.json)**:

```json
{
  "ContentHub": {
    "Endpoint": "https://your-tenant.stylelabs.cloud",
    "ClientId": "your-client-id",
    "ClientSecret": "your-client-secret",
    "UserName": "your-username",
    "Password": "your-password"
  }
}
```

---

## 🧪 Test Data Structure

Examples automatically create test data under:

```
/sitecore/content
└── SitecoreCommander
    └── ExampleTestItem (created during Authoring API examples)
```

### Used Template IDs:
- **Folder**: `{A87A00B1-E6DB-45AB-8B54-636FEC3B5523}`
- **Sample Item**: `{AB86861A-6030-46C5-B394-E8F99E8B87DB}` (has Title and Text fields)

### Cleanup

To remove test items after examples:

1. Run examples again
2. When prompted, select "Cleanup Test Data" option
3. Confirm deletion

Or manually delete: `/sitecore/content/SitecoreCommander` folder

---

## 🎯 Running Specific Modules

### Run Only Agent API

Edit `Program.cs`:

```csharp
const bool ENABLE_EXAMPLES = true;
ExampleRunner.ApiModule? singleModule = ExampleRunner.ApiModule.Agent;
```

Then: `dotnet run`

### Run Only Authoring API

```csharp
ExampleRunner.ApiModule? singleModule = ExampleRunner.ApiModule.Authoring;
```

### Quick Verification (30 seconds)

For CI/automated testing:

```csharp
const bool QUICK_VERIFY = true;
const bool ENABLE_EXAMPLES = false;
```

This runs: Auth validation → Agent API basic query → Returns success/fail

---

## 🔍 Troubleshooting

### Error: "Config Error: Missing 'SitecoreCommander:JwtClientId'"

**Solution**: Update `appsettings.local.json` with missing value.

The error message shows:
- 📁 Exact config file path
- ✋ Which setting is missing
- 📋 Example JSON structure to copy

### Error: "Failed to obtain JWT token"

**Possible causes**:
1. JWT credentials are invalid
2. Environment name is wrong
3. Network connectivity issue
4. XM Cloud environment is offline

**Solution**:
- Verify credentials at https://deploy.sitecorecloud.io/credentials/environment
- Check `EnvironmentName` matches your environment
- Test network connectivity

### Error: "user.json not found"

**Possible causes**:
1. `XMCloudUserJsonPath` is not set
2. Path points to a non-existing file
3. Sitecore CLI login was not executed

**Solution**:
1. Run `dotnet sitecore cloud login`
2. Verify `%USERPROFILE%/.sitecore/user.json` exists
3. Set the absolute path in `SitecoreCommander:XMCloudUserJsonPath`

### Agent API with user.json vs JWT

Agent wrappers send a bearer token to the cloud Agent endpoint. Both auth modes can work, but it depends on token audience/scope.

**Recommended**:
- Use `JWT` (or `Auto`) for Agent API examples.

**Also possible**:
- `Sitecore CLI user.json` mode can work when the token in user.json is valid for the Sitecore cloud Agent endpoint.

If user.json mode fails with authorization errors, switch to JWT mode.

### Error: "Test item not found"

**Solution**: Run "Setup Test Data" example first.

### Error: "Confirmation prompt not showing"

Some CI environments don't support interactive input.

**Solution**: Use `QUICK_VERIFY = true` or set individual examples to skip confirmation.

---

## 💡 Examples & LLM Integration

Each example is designed to be interpretable by LLMs:

- **Clear structure**: One example per method
- **Descriptive names**: `ListAllSitesExample`, `UpdateItemExample`
- **Comments**: Explain what each example does
- **Documentation links**: Point to detailed guides in `/AI/`
- **Verification patterns**: Show expected response structure
- **Error handling**: Clear failure messages

### Using with AI

When working with an LLM (Claude, ChatGPT, etc.):

```
"Generate a script that:
1. Lists all sites using Agent API (see Examples/AgentApiExamples.cs)
2. For each site, gets pages
3. Exports to CSV"
```

The LLM can:
- Reference example code directly
- See how APIs are called
- Copy patterns for custom scripts
- Understand error handling

---

## 📚 Documentation Structure

- **`Examples/README.md`** (this file)
  - How to run examples
  - Troubleshooting
  
- **`Examples/AgentApiExamples.cs`**
  - Read-only query examples
  
- **`Examples/AuthoringApiExamples.cs`**
  - Create/update/delete examples
  
- **`Examples/EdgeApiExamples.cs`**
  - Fast read-only queries
  
- **`Examples/CommandApiExamples.cs`**
  - Bulk operations
  
- **`Examples/TestDataSetup.cs`**
  - Test data creation and cleanup
  
- **`Examples/VerificationHelper.cs`**
  - Response validation patterns
  
- **`AI/AGENT_API_PLAYBOOK.md`**
  - Detailed Agent API guide
  
- **`AI/placeholder-implementation-guide.md`**
  - Authoring API reference

---

## 🛠️ Common Patterns

### Pattern 1: Verify API Response

```csharp
var result = await ListSites.GetSites(token, cancellationToken, "");
var verification = await VerificationHelper.VerifyResponseAsync(
    result,
    "List Sites",
    r => r?.Sites != null && r.Sites.Count > 0);

if (verification.Success)
    Console.WriteLine($"✅ Found {verification.Data.Sites.Count} sites");
else
    Console.WriteLine($"❌ {verification.ErrorDetails}");
```

### Pattern 2: Create Test Item

```csharp
var testSetup = new TestDataSetup(token, cancellationToken);
var itemId = await testSetup.CreateTestItemAsync(
    itemName: "MyTestItem",
    title: "Test Title",
    textContent: "Test content");
```

### Pattern 3: Confirm Write Operation

```csharp
if (VerificationHelper.PromptConfirmation("Publish this item?"))
{
    var result = await Publish.PublishItem(token, cancellationToken, path, language);
}
```

---

## 📊 Exit Codes

| Code | Meaning |
|------|---------|
| 0 | Success |
| 1 | Configuration error or API failure |

---

## ✅ Checklist for New Developers

- [ ] Clone/download SitecoreCommander
- [ ] Create `appsettings.local.json` with credentials
- [ ] Set `ENABLE_EXAMPLES = true` in Program.cs
- [ ] Run `dotnet run`
- [ ] Test Agent API (read-only, safe)
- [ ] Test Authoring API (confirms CRUD works)
- [ ] Review example code structure
- [ ] Understand VerificationHelper patterns
- [ ] Ready to write custom code!

---

## 🤝 Contributing New Examples

To add a new example:

1. Create method in appropriate `*Examples.cs` class
2. Follow pattern:
   ```csharp
   await RunExampleAsync(
       "Example Name",
       "Description of what it does",
       YourExampleMethodAsync);
   ```

3. Use VerificationHelper for consistent output
4. Document with XML comments
5. Link to relevant docs in `/AI/` folder

---

## 📞 Need Help?

- Review example code in `Examples/` folder
- Check `/AI/` documentation files
- Look at error messages (they're detailed!)
- Run with specific module to isolate issues
- Check `Logs/` folder for detailed logs

---

**Happy coding! 🚀**
