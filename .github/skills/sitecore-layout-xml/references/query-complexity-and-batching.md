# Query Complexity and Batching

## Practical Rule
Use a maximum batch size of 20 items per GraphQL request by default.

## Why
When too many items are requested in one query, Sitecore GraphQL can return a complexity error (for example "query too complex").

## Important Nuance
The safe batch size depends on the exact query shape:
- Number of aliases/items in one operation.
- Number of requested fields per item.
- Nested fields and fragments.
- Server-side complexity limits and environment configuration.

Because of this, 20 is a practical default, not a guaranteed universal ceiling.

## Recommended Strategy
1. Start with batch size 20.
2. If complexity errors still occur, reduce batch size (for example 15 or 10).
3. Keep requested fields minimal for list/batch queries.
4. Prefer multi-step retrieval (first ids/basic fields, then details) for heavy payloads.
5. Keep batch size configurable per query type.

## Diagnostics to Return
- `batchSizeUsed`
- `queryType`
- `itemCount`
- `complexityError` (bool)
- `retryBatchSize` (if fallback was applied)
