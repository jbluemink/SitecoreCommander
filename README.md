# SitecoreCommander

SitecoreCommander is a .NET/C# automation toolkit for SitecoreAI and Sitecore 10.3+.

It is built for developers, technical content teams, and administrators who need reliable API automation for real projects, not one-off requests. Instead of isolated calls in Postman or GraphQL Playground, SitecoreCommander helps you compose reusable command chains that execute multiple API steps safely and consistently.

The toolkit is designed for a productive programming workflow in Visual Studio and Visual Studio Code: strong typing, refactoring support, debugging, and source-controlled automation code. Visual Studio Code offers excellent GitHub Copilot support for fast iteration, while Visual Studio provides a more advanced debugging experience for complex flows.

## Overview
SitecoreCommander provides a script-like automation approach in C#, with production-oriented engineering practices. It is well suited for multi-step and high-volume API workflows where reliability, diagnostics, and maintainability matter.

Typical workflow shape:
1. Run one operation to discover context (for example sites/items/pages).
2. Feed results into follow-up operations (update, move, publish, verify).
3. Validate outcomes and rerun safely when needed.

It covers:
- Sitecore Agent API (REST)
- Sitecore Authoring API (GraphQL)
- Sitecore Edge API (GraphQL)
- Sitecore ItemService (REST)
- WordPress XML import and transformation helpers
- Optional Content Hub integration helpers

## Why SitecoreCommander
- Not-from-scratch advantage: start from existing wrappers, examples, and operational patterns instead of rebuilding plumbing.
- Reliability for larger runs: designed for pagination, batch-oriented processing, and limit-aware API usage patterns.
- Better debugging and maintenance: C# tooling in Visual Studio and Visual Studio Code makes multi-step flows easier to inspect, refactor, and test, with the deepest debugger capabilities in Visual Studio.
- Safer automation: encourage explicit prechecks, controlled mutations, and explicit publish steps.
- AI-ready execution layer: AI can generate tasks quickly, while SitecoreCommander provides structure and guardrails for real execution.

## Why Not Start From Scratch
A blank script can work for small one-off tasks, but larger Sitecore automation often fails on details such as pagination, query limits, inconsistent retry/error handling, and partial result interpretation.

SitecoreCommander reduces that risk by providing:
1. Reusable API wrappers and examples.
2. Repeatable workflow patterns for multi-step operations.
3. A typed C# codebase that is easier to review and maintain over time.
4. A stronger debugging experience than ad-hoc request collections.

## AI-Assisted Workflow
SitecoreCommander works well with GitHub Copilot and other AI assistants when you want fast iteration without losing operational discipline.

Recommended flow:
1. Ask AI to draft a workflow goal and required operations.
2. Map each step to existing wrappers/examples.
3. Add prechecks and rollback/compensation for mutating operations.
4. Execute in controlled batches and validate output.
5. Keep the result as reusable C# code in source control.

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
- Optional Content Hub integration helpers (Content Hub API; service-based integration layer)
- Optional Bynder DAM integration helpers (Bynder API v4 REST)

## Documentation Structure For Contributors
- `README.md`: product overview, setup, and quick operational guidance.
- `AI/`: human-facing playbooks, templates, and checklists for implementation and review.
- `.github/skills/`: agent-invocable deep technical rules (placeholder/layout mechanics).
- `Examples/`: executable examples aligned with playbooks.

Start points:
- `AI/README.md` for the documentation map.
- `.github/skills/sitecore-layout-xml/SKILL.md` for layout XML and placeholder rules.

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
- `AI/README.md`
- `AI/DOCUMENTATION-VALIDATION.md`

## Troubleshooting
If Authoring API search returns Solr schema/index errors, verify that the relevant index is populated and up to date.
