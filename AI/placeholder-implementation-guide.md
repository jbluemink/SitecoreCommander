# Placeholder Implementation Guide

## Objective
Implement deterministic placeholder rewriting for source-to-target layout XML migration.

## Canonical Source
- Canonical runtime guidance for placeholder mechanics lives in `.github/skills/sitecore-layout-xml/SKILL.md` and its `references/` folder.
- This document is a concise implementation quick-start for this repository.

## Recommended Steps
1. Parse source renderings and collect placeholder paths.
2. Resolve component mapping for each rendering.
3. Rewrite placeholder roots and segment names.
4. Assign/remap `DynamicPlaceholderId` values.
5. Validate nested placeholder path consistency.

## Error Handling
- Log missing placeholder mappings as warnings.
- Stop the run on critical structural mismatches.
- Keep a trace log for source placeholder -> target placeholder mapping.

## XML Sample Intake (Recommended)
When contributors share XML for analysis, include:
1. Source field: `__Renderings` or `__Final Renderings`.
2. Full device node (`<d ...>`) with all child renderings.
3. Raw values for `uid`, `id`/`s:id`, `ph`/`s:ph`, `par`/`s:par`, and `ds`/`s:ds`.
4. Expected outcome: classification and desired target placeholder path(s).

## Example: JSS-style Layout Snippet
Given snippet characteristics:
- Namespaced attributes (`s:id`, `s:ph`, `s:ds`) are present.
- Placeholder paths include dynamic-looking suffixes such as `...-{GUID}-0`.
- Device id `{FE5D7FDF-89C0-4D99-9AA3-B5FBD009C9F3}` indicates Default device.

Interpretation guidance:
1. Treat the sample as final-style serialization unless field origin proves otherwise.
2. Do not infer shared vs final from naming alone; always confirm source field.
3. For paths like `/jss-main/jss-default-layout-{GUID}-0`, treat `{GUID}` as rendering-instance anchor candidate and validate against parent `uid`.
4. Keep `p:before`/`p:after` for ordering diagnostics only; they do not define placeholder identity.
