# Agent Prompt Template

Use this template to keep AI-run execution consistent.

## Goal
[Describe the expected outcome in 1-3 sentences]

## Context
- Project: SitecoreCommander
- API families: Authoring (default), Agent (site/page/job workflows), Edge (read use cases)
- Auth profiles: `authoring-cli` or `jwt`

## Constraints
- No automatic publish after writes.
- Keep source systems read-only during migration.
- Use explicit rollback or compensation for mutations.

## Input
- `auth_profile`: [authoring-cli | jwt]
- `target_host`: [required when `auth_profile=jwt` for Authoring]
- `operation`: [exact wrapper method]
- `parameters`: [name/value list]

## Planner Rules
1. Prefer Authoring for full content operations.
2. Use Agent for site/page/job orchestration.
3. Add prechecks before execution.
4. Add rollback for mutating operations.
5. Select auth profile explicitly:
	- `jwt` for Agent wrappers and automation runs.
	- `authoring-cli` for Authoring/Edge operations when user context is intended.
6. For `authoring-cli`, include precheck that `XMCloudUserJsonPath` exists and endpoint can be resolved.

## Required Output
- `api_family`
- `auth_profile`
- `operation`
- `input_summary`
- `prechecks`
- `execution_steps`
- `rollback_strategy`
- `observability`
