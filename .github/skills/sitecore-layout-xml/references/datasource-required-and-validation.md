# Datasource Required and Validation Rules

## Required Logic
Datasource is required when both rendering definition fields are filled:
- `DataSource Location`
- `Datasource Template`

If one or both are empty, datasource is not required.

## Validation States (Recommended)
- `valid`
- `error-empty`
- `error-not-found`
- `error-template-mismatch`

## Validation Flow
1. Resolve rendering definition item from rendering id.
2. Read `DataSource Location` and `Datasource Template`.
3. Compute `datasourceRequired = hasLocation && hasTemplate`.
4. If not required, return `valid` without hard datasource checks.
5. If required:
- Empty datasource: `error-empty`.
- `local:` datasource: resolve local path relative to page path.
- `page:` datasource: treat as valid special reference.
- GUID datasource: lookup item and validate existence.
6. If datasource item exists and allowed templates are configured, run template matching.

## Template Matching Strategy
Template is valid if one of these matches succeeds:
- Direct template match.
- Base template match.
- Originator match (`__Originator`).
- Source match (`__Source`) fallback.
- Branch fallback: actual template equals first child template of allowed branch template.

## Template Reference Resolution
- Parse `Datasource Template` and optional additional compatible template references.
- Support GUID references and `/sitecore/...` path references.
- Resolve path references to template item ids before comparison.
- Normalize templates to uppercase without braces for comparison.

## Diagnostic Output
Return clear message with:
- Required decision (`true|false`) and why.
- Datasource mode (`local|page|guid|empty`).
- Match result (`direct|base-template|originator|branch-first-child|no-match`).
- Missing metadata and unresolved references count.

## Edge Cases
- Datasource required but allowed template list resolves to empty: warn but do not silently fail.
- Local datasource path resolves to missing item.
- Datasource item exists but template metadata missing.
- Mixed brace/casing formats in GUID values.
