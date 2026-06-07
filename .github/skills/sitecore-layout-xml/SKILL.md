---
name: sitecore-layout-xml
description: 'Sitecore layout XML and placeholder mechanics for SXA and headless solutions. Use for placeholder path construction, supported placeholders per rendering, dynamicplaceholderid behavior, layout XML parsing, rendering parameters, and C# implementation/debugging of placeholder logic.'
argument-hint: 'Describe your Sitecore placeholder question or scenario, optionally include layout XML and rendering parameters.'
user-invocable: true
---

# Sitecore Layout XML and Placeholder Mechanics

## When to Use
- You need to explain how Sitecore placeholder paths are built in shared or final layout.
- You need to determine which placeholders are supported by a rendering.
- You need to analyze or debug dynamic placeholders (`dynamicplaceholderid`).
- You need to analyze Page Design and Partial Design contributions to rendered layout.
- You need to determine whether a rendering requires a datasource and why.
- You need practical C# guidance to read layout XML, rendering parameters, and template metadata.
- You need a repeatable checklist to validate rendering placement rules.

## Source Of Truth For This Repository
- Runtime implementation is leading, not pseudo-code docs.
- This repository does not currently include the `XmToAi` content migrator implementation.
- `XmToAi` can be used as an external reference implementation for layout migration patterns.
- Use this skill to understand, validate, and implement layout migration logic in this repository or external migrator tooling.

## What This Skill Covers
- Placeholder key and placeholder path semantics.
- Shared (`ph`) vs final (`s:ph`) layout behavior.
- Supported placeholder discovery from rendering metadata and placeholder settings/template items.
- `dynamicplaceholderid` extraction and normalization from rendering parameters.
- Partial Design resolution via Page Design (`Page Design` and `PartialDesigns` mapping flow).
- Datasource required logic and datasource validation rules.
- C# helper patterns for robust parsing and validation.

## Content Migrator Rules (Reference Pattern)
- Shared and Final should be processed separately and persisted to `__Renderings` and `__Final Renderings`.
- Parsing should be prefix-agnostic (local-name based attr reads).
- Dynamic placeholder behavior, ordering rebuild, datasource handling, and fail/warn policy should follow the deterministic checklist below.
- For concrete edge-case mechanics, use the `references/` docs as the canonical detail level.

## Procedure
1. Identify layout context.
- Determine whether the value comes from shared layout (`ph`) or final layout (`s:ph`).
- Confirm XML namespace usage (`xmlns:s="s"`, `xmlns:p="p"`).

2. Parse renderings.
- Read each rendering node and capture `uid`, rendering item id (`id` or `s:id`), placeholder value (`ph` or `s:ph`), and parameters (`par` or `s:par`).
- Keep raw values for diagnostics and normalized values for matching.

3. Resolve supported placeholders for each rendering.
- Fetch rendering item metadata and resolve its placeholder references to placeholder keys.
- If references are GUID-based, resolve GUID to placeholder key/name.
- Build an allow-list per rendering and compare it with actual placeholder path usage.

4. Handle dynamic placeholder logic.
- Parse rendering parameters and extract `dynamicplaceholderid` (case-insensitive).
- Normalize HTML encoded separators (`&amp;` -> `&`) before parsing.
- Dynamic ID assignment in this migrator is driven by target `defaultParameters` containing `DynamicPlaceholderId`.
- Reuse ID by rendering `uid` across Shared/Final when same rendering instance is encountered.
- Keep a source->target dynamic-id map for placeholder path suffix remapping.

5. Handle implicit dynamic placeholder keys (important).
- If XML does not explicitly define dynamic placeholder metadata for a child path segment, infer dynamic suffix from the parent rendering instance.
- In practice, placeholder segments are often in shape:
	- `{placeholder-base}-{RENDERING-UID}-0`
- The GUID part corresponds to the parent rendering `uid`.
- Example pattern (do not hardcode literal values):
	- `deck-container-{PARENT-RENDERING-UID}-0`
- For analysis and migration, treat that GUID as the rendering instance anchor and preserve/remap consistently with UID-based ordering and dynamic ID tracking.

6. Validate placement.
- Check if the target placeholder root or full path is in the rendering allow-list.
- Distinguish hard violations (not supported) from weak signals (metadata missing).
- Return deterministic diagnostics with reason codes.

7. Evaluate datasource requirements.
- Read rendering definition metadata (`DataSource Location` and `Datasource Template`).
- Treat datasource as required only when both fields are filled.
- Validate datasource value for required renderings (`empty`, `not-found`, `template-mismatch`, `valid`).
- Include template match fallbacks (base templates, originator/source, branch first-child template) when available.

8. Resolve Page Design and Partial Designs.
- Read page-level `Page Design` field first.
- If empty, resolve from `TemplatesMapping` on nearest `Presentation/Page Designs` candidate path.
- Read `PartialDesigns` references from the resolved Page Design and load each partial design layout.
- Merge diagnostics to show whether placement/validation is from page layout, partial design layout, or both.

9. Apply C# implementation patterns.
- Use safe XML and query parsing utilities.
- Keep GUID normalization and parameter parsing in reusable helpers.
- Add edge-case tests (missing params, malformed query strings, unknown GUIDs, mixed casing).

## Deterministic Processing Checklist
1. Parse device nodes and enforce allowed device scope.
2. Remove personalization and stale ordering attributes.
3. Build parent-first rendering order.
4. Resolve target component(s), placeholders, parameters, datasource behavior.
5. Allocate/reuse dynamic IDs (UID-aware), then apply par defaults.
6. Rewrite placeholder root + segment mappings + dynamic suffix remap.
7. Add generated child renderings for placeholder references and repeat patterns.
8. Re-annotate final ordering (`p:before`/`p:after`).
9. Validate unknown placeholders and datasource misses according to fail/warn policy.

## References
- [Placeholder Fundamentals](./references/placeholder-fundamentals.md)
- [Supported Placeholders and Template Retrieval](./references/supported-placeholders-and-template-retrieval.md)
- [Dynamic Placeholder ID Mechanics](./references/dynamic-placeholder-id.md)
- [Datasource Required and Validation Rules](./references/datasource-required-and-validation.md)
- [Page Design and Partial Design Resolution](./references/page-design-and-partial-design.md)
- [Query Complexity and Batching](./references/query-complexity-and-batching.md)
- [C# Implementation Guide](./references/csharp-implementation-guide.md)

## Related AI Playbooks In This Repository
- `AI/AGENT_API_PLAYBOOK.md` for API family and authentication workflow.
- `AI/AGENT_PROMPT_TEMPLATE.md` for planner output format and execution planning structure.
- `AI/placeholder-implementation-guide.md` and `AI/placeholder-dynamics.md` for concise implementation notes.
- `AI/DEFAULTPARAMETERS-CHECKLIST.md` for `defaultParameters` URL-encoding validation.

## Output Expectations
- Explain both conceptual behavior and concrete retrieval steps.
- Provide C#-ready pseudocode or code snippets when implementation is requested.
- Include edge-case handling and validation checks, not only happy-path examples.
- Explicitly state whether datasource is required and which rule triggered that outcome.