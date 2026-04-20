# SitecoreCommander

SitecoreCommander is a .NET automation toolkit for Sitecore XM Cloud (Sitecore IA) and Sitecore 10.3+.

It is built for developers, technical content teams, and administrators who need to execute reliable API-based operations at scale. Instead of doing isolated calls in tools like Postman or GraphQL Playground, SitecoreCommander helps you orchestrate complete workflows across multiple APIs with reusable C# code.

The project is also designed for modern AI-assisted development workflows, including Vibe Coding and coding assistants such as GitHub Copilot. You can use it as a practical execution layer for AI-generated scripts, migration routines, and repeatable operational tasks.

## Overview
SitecoreCommander provides a scripting and automation approach inside Visual Studio and .NET, with strong debugging, logging, and version-controlled code. It is suitable for both experimentation and production-ready automation, especially for long-running or multi-step operations.

It covers:
- Sitecore Agent API (REST)
- Sitecore Authoring API (GraphQL)
- Sitecore Edge API (GraphQL)
- Sitecore ItemService (REST)
- WordPress XML import and transformation helpers
- Optional Content Hub integration helpers

## Why SitecoreCommander
- API-first automation: Execute complex multi-call flows that go beyond single request tools.
- Developer ergonomics: Use C#, debugging, and source control instead of ad-hoc scripts.
- Safer operations: Prefer Sitecore APIs over elevated in-instance scripting for sensitive environments.
- Scalable scripting: Handle long-running and resource-intensive tasks more reliably.
- AI-ready foundation: Great for AI assistants that generate or refine migration and automation code.

## Typical Use Cases
- Content migration and bulk content updates
- Automated creation, update, and deletion of items
- Publication and versioning workflows
- Repetitive maintenance and cleanup tasks
- Data import/export and system integration pipelines

## Important Note
This is an actively evolving toolkit. Some scenarios may require customization for your implementation model, templates, and governance rules. The codebase is intentionally extensible so you can adapt and grow it per project.

## What It Includes
- Sitecore Authoring API (GraphQL)
- Sitecore Agent API (REST)
- Sitecore Edge API (GraphQL)
- Sitecore ItemService (REST)
- WordPress XML import helpers
- Optional Content Hub integration helpers

## Quick Start
1. Create or update local settings in `appsettings.Local.json`.
2. Use `appsettings.example.json` as a template.
3. Choose an authentication method:
   - JWT client credentials (`JwtClientId` + `JwtClientSecret`)
   - Sitecore CLI `user.json` (`XMCloudUserJsonPath`)
4. Run:
   - `dotnet build`
   - `dotnet run`

## Authentication Options
SitecoreCommander examples support two authentication paths:

1. `JWT` (automation/client credentials)
   - Configure `SitecoreCommander:JwtClientId` and `SitecoreCommander:JwtClientSecret`.
   - Recommended for unattended automation and Agent API examples.

2. `Sitecore CLI user.json` (developer context)
   - Configure `SitecoreCommander:XMCloudUserJsonPath`.
   - Recommended for interactive Authoring/Edge work with your CLI session context.

### Create a valid user.json with Sitecore CLI
1. Install Sitecore CLI (if not already installed):
   - `dotnet tool install -g Sitecore.CLI`
2. Authenticate:
   - `dotnet sitecore cloud login`
3. Confirm the file exists at:
   - `%USERPROFILE%/.sitecore/user.json`
4. Point `SitecoreCommander:XMCloudUserJsonPath` to that file.

## Configuration Priority
1. `appsettings.Local.json`
2. Environment variables (if a key is not set in local settings)

## Notes
- `appsettings.Local.json` is intended for local values and should not be committed.
- Publish operations should be executed explicitly; avoid automatic publish after write operations.

## AI Agent Docs
- `AI/AGENT_API_PLAYBOOK.md`
- `AI/AGENT_PROMPT_TEMPLATE.md`
- `AI/agent-api-index.json`

## Troubleshooting
If Authoring API search returns Solr schema/index errors, verify that the relevant index is populated and up to date.
