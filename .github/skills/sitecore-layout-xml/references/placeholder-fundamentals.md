# Placeholder Fundamentals

## Core Concepts
- A placeholder key is the insertion point name a rendering can target.
- A placeholder path is a hierarchical address, for example `/main/row-1/column-2`.
- A placeholder path is typically built from parent to child as renderings nest.

## Shared vs Final Layout Attributes
- Shared layout usually stores placeholder in `ph`.
- Final layout usually stores placeholder in `s:ph`.
- Parse both and prefer final value when evaluating final presentation.

## Path Construction Rules
1. Start with the root placeholder key where the parent rendering is placed.
2. Append child placeholder keys as renderings nest.
3. Preserve separators and casing policy consistently in your implementation.
4. Normalize empty or whitespace values early.

## Practical Notes
- Some solutions only validate root placeholder keys.
- Other solutions validate full path segments.
- Treat this as a configurable rule and document the chosen mode.

## Positioning Context
- `p:before` and `p:after` affect ordering, not placeholder path identity.
- Placeholder path should remain stable across ordering changes unless parent container changes.

## Validation Checklist
- Confirm namespace declarations exist (`xmlns:p`, `xmlns:s`) before querying namespaced attributes.
- Confirm whether you are reading shared, final, or merged view.
- Confirm path comparison mode (root-only vs full-path).
- Confirm normalization policy (trim, casing, slash behavior).
