# Placeholder Dynamics

## Purpose
This note describes how `DynamicPlaceholderId` values and placeholder paths are transformed during layout migration.

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
