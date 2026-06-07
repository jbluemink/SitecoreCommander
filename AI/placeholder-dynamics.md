# Placeholder Dynamics

## Purpose
This note describes how `DynamicPlaceholderId` values and placeholder paths are transformed during layout migration.

## Canonical Source
- Canonical parsing and dynamic placeholder rules live in `.github/skills/sitecore-layout-xml/SKILL.md` and `.github/skills/sitecore-layout-xml/references/dynamic-placeholder-id.md`.
- This note summarizes expected behavior for implementation and review.

## Core Behavior
- Process renderings in parent-first order.
- Assign `DynamicPlaceholderId` for renderings that declare it in default parameters.
- Rewrite placeholder root and mapped placeholder segments.
- Remap dynamic placeholder suffixes to match target parent relationships.

## Practical Rule
Placeholder path transformation should preserve structural intent while updating:
- placeholder names
- dynamic suffix values
- nested path consistency
