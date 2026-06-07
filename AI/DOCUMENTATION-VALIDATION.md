# Documentation Validation Notes

Date: 2026-06-07

## Scope
This note records validation status for AI and skill documentation in this repository.

## Validation Sources
- MCP server configured in `.vscode/mcp.json`: `sitecore-documentation-docs` (`https://sitecore.mcp.kapa.ai`)
- Internet sources:
  - `https://doc.sitecore.com/`
  - `https://doc.sitecore.com/xp`
  - `https://developers.sitecore.com/`

## Validation Outcome
1. MCP endpoint was reachable as configured, but content extraction through available tooling did not return usable documentation chunks.
2. Sitecore documentation root pages were reachable.
3. Multiple deep links for historical XP/XM pages returned 404 in this environment.
4. Because of (1) and (3), some detailed claims remain partially verified and should be treated as implementation guidance pending targeted source confirmation.

## Claims Considered Stable
- Shared and final layout need distinct handling in migration/analysis workflows.
- Placeholder identity is based on path semantics, not ordering attributes.
- Dynamic placeholder parsing requires robust parameter decoding and case-insensitive key handling.
- `defaultParameters` encoding quality has direct runtime impact.

## Claims Marked As Needs-Confirmation
- Exact field/attribute serialization variants across all Sitecore versions and environments.
- Strict mapping expectations between dynamic path GUID segments and parent rendering uid in every implementation.
- Version-specific behavior around Page Design / Partial Design resolution edge cases.

## Unclear Areas To Document Better
1. Shared vs final classification when snippets contain mixed-style clues.
2. Expected behavior when `dynamicplaceholderid` is absent but placeholder path carries GUID suffixes.
3. Clear fail/warn policy for unknown placeholder roots in dry-run versus apply mode.
4. Version scoping (XM Cloud vs XP 10.x) for each rule section.

## Recommended Additions
1. Add one explicit shared example and one explicit final example with field origin declared.
2. Add a matrix of required input fields for reliable XML analysis.
3. Add a compact rule table: hard-fail, soft-fail, warn-only.
4. Add contributor checklist for anonymizing datasource IDs before sharing outside private channels.

## Open Questions
1. Should docs enforce a canonical example style (final-style only) or always provide both shared and final examples?
2. Should migration rules default to strict mode (fail fast) or advisory mode (warn and continue)?
3. Which Sitecore versions should be treated as in-scope baseline behavior?

## Is It Useful To Provide XML Examples?
Yes. XML examples are highly useful and reduce ambiguity significantly.

Most useful sample package:
1. Source field name (`__Renderings` or `__Final Renderings`).
2. Full `<d>` node for one device with all `<r>` children.
3. Raw attributes for `uid`, `id`/`s:id`, `ph`/`s:ph`, `par`/`s:par`, `ds`/`s:ds`.
4. Expected interpretation and target outcome.
5. One edge case (missing datasource, empty params, mixed casing, unknown placeholder root).
