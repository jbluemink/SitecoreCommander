# SXA Site Structure - Migration Scope

Typical source structure:

- `/sitecore/content/{tenant}/{site}/Home` : pages and local datasource folders.
- `/sitecore/content/{tenant}/{site}/Data` : shared datasource items.
- `/sitecore/content/{tenant}/{site}/Media` : media inventory (migration can be deferred).
- `/sitecore/content/{tenant}/{site}/Presentation` : platform configuration (usually out of scope).
- `/sitecore/content/{tenant}/{site}/Settings` : platform/site settings (scope-dependent).

## Migration Priority
1. Shared datasource tree under `/Data`.
2. Page tree under `/Home` including local `/Data` subfolders.
3. Media inventory and optional later media migration.

## Item Classification
- Page items: usually have layout values.
- Datasource items: usually no layout, referenced from renderings.
- Platform config items: generally skipped unless explicitly mapped.
