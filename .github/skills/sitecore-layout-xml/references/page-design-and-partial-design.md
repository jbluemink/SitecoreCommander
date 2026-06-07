# Page Design and Partial Design Resolution

## Why It Matters
Final page composition can include renderings from:
- Page item layout fields.
- Page Design assigned to the page.
- Partial Designs referenced by the Page Design.

Placeholder and datasource analysis should account for all relevant sources.

## Resolution Flow
1. Get page item (`itemId`, `path`, `template id`, `language`).
2. Read page-level `Page Design` field.
3. If `Page Design` is empty:
- Build candidate paths ending with `Presentation/Page Designs` from nearest site root ancestry.
- Read `TemplatesMapping` from those candidate items.
- Resolve page design by page template id.
4. Load resolved Page Design item (`itemId`, `name`).
5. Read `PartialDesigns` references.
6. Load each Partial Design item in batches and read:
- `__Renderings`
- `__Final Renderings`

## Normalization and Robustness
- Decode encoded ids repeatedly before GUID extraction.
- Accept GUIDs with and without braces.
- Deduplicate partial design ids before querying.
- Preserve order only where business logic requires it.

## Analysis Recommendations
- Label each rendering source: `page-layout`, `page-final-layout`, `partial-layout`, `partial-final-layout`.
- Keep per-source diagnostics so conflicts are traceable.
- When showing merged results, include source count and source names.

## Common Outcomes
- No page design linked: informational, not an error.
- Page design exists but no partial designs configured: informational.
- Partial design id unresolved: warning with unresolved id list.

## C# Integration Pattern
- Build a resolver service with methods:
- `ResolvePageDesignId(...)`
- `ResolvePartialDesignIds(...)`
- `FetchPartialDesignLayouts(...)`
- Use a default GraphQL batch size of 20 items.
- Keep batch size configurable because complexity limits depend on the exact query shape.
- If a query fails with "too complex", retry with a smaller batch size.
