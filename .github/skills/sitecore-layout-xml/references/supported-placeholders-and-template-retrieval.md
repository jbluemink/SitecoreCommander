# Supported Placeholders and Template Retrieval

## Goal
Determine which placeholders are supported by a rendering and compare that against actual usage.

## Typical Retrieval Flow
1. Read rendering definition item id from layout XML (`id` or `s:id`).
2. Fetch rendering metadata (GraphQL, Content API, or Sitecore item API).
3. Read placeholder references from rendering fields (often GUID list in a placeholders field).
4. Resolve referenced placeholder items to their keys or names.
5. Build an allow-list per rendering.
6. Compare rendering placement (`ph` or `s:ph`) to the allow-list.

## Data Sources to Inspect
- Rendering item fields (including placeholders or compatible placeholder references).
- Placeholder Settings items for key names.
- Template and base template metadata when placeholder behavior is inherited or constrained through templates.

## Batch and Fallback Strategy
- Deduplicate GUIDs before lookup.
- Resolve in batches to reduce API calls.
- Use a default batch size of 20 items for GraphQL lookups.
- If the query fails with a complexity error (for example "too complex"), retry with a smaller batch size.
- Complexity risk depends on the exact query payload and selected fields, so keep batch size configurable.
- When a GUID cannot be resolved, keep GUID as fallback token and mark quality as degraded.
- Return both `resolvedKeys` and `unresolvedIds` for diagnostics.

## Output Model (Suggested)
- `renderingId`: GUID
- `supportedPlaceholderIds`: string[]
- `supportedPlaceholderKeys`: string[]
- `placementPlaceholderPath`: string
- `isSupported`: boolean
- `reasonCode`: `supported | unsupported | metadata-missing | unresolved-placeholder-ids`

## Template-Aware Checks
- If rendering metadata includes template constraints, evaluate those after placeholder allow-list checks.
- If only template metadata is available, use template + base template traversal to infer compatible placeholders.
- Keep inferred results separate from explicit rendering placeholder metadata.

## Recommended Diagnostics
- Include source of truth used (`rendering-field`, `placeholder-settings`, `template-inference`).
- Include count of unresolved references.
- Include final comparison mode (`root-only` or `full-path`).
