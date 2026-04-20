# SitecoreCommander Agent API Playbook

This document helps AI agents choose the correct API wrapper and authentication profile.

## API Family Selection
1. Use Authoring wrappers (`Authoring/`) by default for complete content operations.
2. Use Agent wrappers (`Agent/`) for site/page listings, job workflows, and stream-style operations.
3. Use Edge wrappers for read-focused GraphQL queries.

## Authentication Profiles
- `authoring-cli`: Use `EnvironmentConfiguration` from Sitecore CLI `user.json`.
- `jwt`: Use `JwtTokenResponse`; pass `host` for Authoring calls.

## Authentication Decision Matrix
1. Prefer `authoring-cli` for interactive developer workflows where Sitecore CLI is already logged in.
2. Use `jwt` for unattended automation and as the default for Agent wrapper operations.
3. If the user asks for "user.json" auth, assume `authoring-cli` and verify `XMCloudUserJsonPath` exists.
4. For `Agent/` wrappers, `authoring-cli` may still work when the user.json access token has valid audience/scope for the cloud Agent endpoint.

## Sitecore CLI user.json Preconditions
Before selecting `authoring-cli`, confirm:
1. `dotnet sitecore cloud login` has been executed.
2. `SitecoreCommander:XMCloudUserJsonPath` points to a valid file.
3. The requested endpoint exists in `user.json` (`EnvironmentName` or default endpoint).

## Operational Rules
- Keep source systems read-only in migration scenarios.
- Do not auto-publish after write operations.
- Use publish as an explicit, planned step.
- Job-id logging is optional.

## Typical Agent Workflow
1. Resolve auth profile.
2. Run prechecks (token validity, host, item/site existence).
3. Execute wrapper methods.
4. Validate response/errors.
5. Emit logs for observability and reruns.

## Output Contract for Planning Agents
- `api_family`
- `auth_profile`
- `operation`
- `input_summary`
- `prechecks`
- `execution_steps`
- `rollback_strategy`
- `observability`
