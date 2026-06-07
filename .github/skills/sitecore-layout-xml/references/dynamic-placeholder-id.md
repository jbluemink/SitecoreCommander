# Dynamic Placeholder ID Mechanics

## What It Is
`dynamicplaceholderid` is a rendering parameter commonly used to make placeholder keys unique per rendering instance.

## Where to Read It
- Usually from `par` (shared) or `s:par` (final) on rendering nodes.
- Parameter strings can be URL query style and may contain HTML encoded separators.

## Parsing Rules
1. Normalize HTML encoding first (`&amp;` to `&`).
2. Parse key-value pairs using a query parser.
3. Match `dynamicplaceholderid` case-insensitively.
4. URL-decode the value.
5. Extract numeric token when required by your placeholder naming convention.

## Relation to Placeholder Path
- The final placeholder path may include a dynamic suffix or segment built from the id.
- Exact format is solution-specific (for example `content-3` or `container-{id}`).
- Keep this formatter isolated in one helper to avoid inconsistent behavior.

## Edge Cases
- Missing parameter key.
- Mixed casing (`DynamicPlaceholderId`, `dynamicplaceholderid`).
- Malformed query string.
- Duplicate parameter keys.
- Non-numeric ids in a numeric-only convention.

## Validation Guidance
- Do not fail hard only because id is absent, unless component convention requires it.
- Report whether parsing was exact, fallback, or failed.
- Keep raw parameter string in diagnostics for troubleshooting.
