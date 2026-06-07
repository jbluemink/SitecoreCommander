# C# Implementation Guide

## Objectives
- Parse layout XML safely.
- Resolve supported placeholders for each rendering.
- Parse `dynamicplaceholderid` robustly.
- Validate placement with deterministic diagnostics.

## Namespace and `p:p="1"` Semantics (Important)
- `__Renderings` (Shared Layout) may be serialized without `xmlns:p`, `xmlns:s`, and without `p:p="1"`.
- `__Final Renderings` often includes `xmlns:p="p"`, `xmlns:s="s"`, and root flag `p:p="1"`.
- Prefixes are not a stable contract. Read attributes by `LocalName` (`id`, `ph`, `par`, `uid`, `ds`, `before`, `after`) instead of hardcoding prefix-based lookups.
- Use the field context as source of truth for layout type:
    - parsed from `__Renderings` => shared
    - parsed from `__Final Renderings` => final
- `p:p="1"` is a useful hint for delta/final serialization style, but should not be the only discriminator.

## Shared + Final Merge Model
- Merge key is rendering `uid`.
- If a final rendering has same `uid` as shared, final overrides shared instance.
- Final layout can contain delta/partial nodes (for example ordering-only updates) that have `uid` but no `id`/`ph`.
- Do not treat uid-only delta nodes as mappable renderings.
- Preserve/apply ordering semantics (`p:before`, `p:after`) during composition.

## Suggested Data Contracts
```csharp
public sealed record RenderingPlacement(
    string Uid,
    string RenderingId,
    string PlaceholderPath,
    string ParametersRaw,
    bool IsFinalLayout
);

public sealed record PlaceholderSupportInfo(
    string RenderingId,
    IReadOnlyList<string> SupportedPlaceholderIds,
    IReadOnlyList<string> SupportedPlaceholderKeys,
    IReadOnlyList<string> UnresolvedPlaceholderIds
);

public sealed record PlacementValidationResult(
    string Uid,
    bool IsSupported,
    string ReasonCode,
    string PlaceholderPath,
    IReadOnlyList<string> SupportedPlaceholderKeys
);
```

## Parsing Layout XML
```csharp
using System;
using System.Linq;
using System.Xml.Linq;

public static IReadOnlyList<RenderingPlacement> ParseRenderings(string xml, bool isFinalLayout)
{
    var doc = XDocument.Parse(xml, LoadOptions.PreserveWhitespace);

    // Use LocalName instead of prefix-based XNamespace lookups.
    // Sitecore can serialize attributes as "ph" or "s:ph" (same LocalName).
    // This handles both shared XML without namespaces and final XML with p/s namespaces.
    var nodes = doc.Descendants().Where(x => x.Name.LocalName == "r");
    var results = new List<RenderingPlacement>();

    foreach (var node in nodes)
    {
        var uid = node.Attributes().FirstOrDefault(a =>
            string.Equals(a.Name.LocalName, "uid", StringComparison.OrdinalIgnoreCase))?.Value ?? string.Empty;

        var id = node.Attributes().FirstOrDefault(a =>
            string.Equals(a.Name.LocalName, "id", StringComparison.OrdinalIgnoreCase))?.Value ?? string.Empty;

        // Final layout can contain delta nodes (uid-only ordering overrides) without id/ph.
        // Skip those here; they are not renderings to map.
        if (string.IsNullOrWhiteSpace(id))
            continue;

        var placeholder = node.Attributes().FirstOrDefault(a =>
            string.Equals(a.Name.LocalName, "ph", StringComparison.OrdinalIgnoreCase))?.Value ?? string.Empty;

        var par = node.Attributes().FirstOrDefault(a =>
            string.Equals(a.Name.LocalName, "par", StringComparison.OrdinalIgnoreCase))?.Value ?? string.Empty;

        results.Add(new RenderingPlacement(
            uid,
            NormalizeGuid(id),
            NormalizePlaceholderPath(placeholder),
            par,
            isFinalLayout
        ));
    }

    return results;
}
```

## Parsing dynamicplaceholderid
```csharp
public static int ExtractDynamicPlaceholderId(string? par)
{
    if (string.IsNullOrWhiteSpace(par)) return 0;

    var normalized = par.Replace("&amp;", "&", StringComparison.OrdinalIgnoreCase);

    foreach (var token in normalized.Split('&', StringSplitOptions.None))
    {
        if (string.IsNullOrWhiteSpace(token))
            continue;

        var eqIndex = token.IndexOf('=');
        if (eqIndex < 0)
            continue;

        var rawKey = token.Substring(0, eqIndex);
        var rawValue = token.Substring(eqIndex + 1);
        var decodedKey = Uri.UnescapeDataString(rawKey.Replace("+", "%20", StringComparison.Ordinal));

        if (!string.Equals(decodedKey, "DynamicPlaceholderId", StringComparison.OrdinalIgnoreCase))
            continue;

        var decodedValue = Uri.UnescapeDataString(rawValue.Replace("+", "%20", StringComparison.Ordinal));
        return int.TryParse(decodedValue, out var parsed) && parsed > 0 ? parsed : 0;
    }

    return 0;
}
```

## Resolving Supported Placeholders
```csharp
public static PlaceholderSupportInfo ResolveSupportedPlaceholders(
    string renderingId,
    IReadOnlyDictionary<string, IReadOnlyList<string>> renderingToPlaceholderIds,
    IReadOnlyDictionary<string, string> placeholderIdToKey)
{
    var ids = renderingToPlaceholderIds.TryGetValue(renderingId, out var value)
        ? value.Where(v => !string.IsNullOrWhiteSpace(v)).Select(NormalizeGuid).Distinct().ToArray()
        : Array.Empty<string>();

    var keys = new List<string>();
    var unresolved = new List<string>();

    foreach (var id in ids)
    {
        if (placeholderIdToKey.TryGetValue(id, out var key) && !string.IsNullOrWhiteSpace(key))
        {
            keys.Add(key.Trim());
        }
        else
        {
            unresolved.Add(id);
        }
    }

    return new PlaceholderSupportInfo(renderingId, ids, keys.Distinct().ToArray(), unresolved);
}
```

## Placement Validation
```csharp
public static PlacementValidationResult ValidatePlacement(
    RenderingPlacement placement,
    PlaceholderSupportInfo support,
    bool fullPathMode)
{
    if (support.SupportedPlaceholderKeys.Count == 0 && support.UnresolvedPlaceholderIds.Count == 0)
    {
        return new PlacementValidationResult(
            placement.Uid,
            false,
            "metadata-missing",
            placement.PlaceholderPath,
            support.SupportedPlaceholderKeys
        );
    }

    var candidate = fullPathMode
        ? placement.PlaceholderPath
        : RootPlaceholder(placement.PlaceholderPath);

    var allow = new HashSet<string>(support.SupportedPlaceholderKeys, StringComparer.OrdinalIgnoreCase);

    var isSupported = allow.Contains(candidate) || allow.Contains(RootPlaceholder(candidate));

    return new PlacementValidationResult(
        placement.Uid,
        isSupported,
        isSupported ? "supported" : "unsupported",
        placement.PlaceholderPath,
        support.SupportedPlaceholderKeys
    );
}
```

## Reusable Helpers
```csharp
public static string NormalizeGuid(string? input)
{
    if (string.IsNullOrWhiteSpace(input))
        return string.Empty;

    var trimmed = input.Trim();
    return Guid.TryParse(trimmed, out var parsed)
        ? parsed.ToString("B").ToUpperInvariant() // {XXXXXXXX-XXXX-XXXX-XXXX-XXXXXXXXXXXX}
        : trimmed;
}

public static string NormalizePlaceholderPath(string value)
{
    var trimmed = (value ?? string.Empty).Trim();
    if (trimmed.Length == 0) return string.Empty;
    return trimmed.StartsWith("/", StringComparison.Ordinal) ? trimmed : "/" + trimmed;
}

public static string RootPlaceholder(string path)
{
    var normalized = NormalizePlaceholderPath(path);
    if (normalized.Length == 0) return string.Empty;

    var nextSlash = normalized.IndexOf('/', 1);
    return nextSlash < 0 ? normalized : normalized[..nextSlash];
}
```

## Edge-Case Matrix
- Missing `ph` and `s:ph`: report `missing-placeholder-path`.
- Missing rendering id in Final layout delta node (uid-only override): skip without warning.
- Missing rendering id on a normal rendering node: report `missing-rendering-id` and skip support lookup.
- `dynamicplaceholderid` missing: warn only when convention requires it.
- Placeholder ids unresolved: reason `unresolved-placeholder-ids`.
- Duplicate placeholders in metadata: deduplicate before validation.
- Malformed XML: fail fast with parser exception and include context fragment.
- Root `p:p="1"` missing in shared layout: expected and valid.
- Prefixed (`s:ph`) vs unprefixed (`ph`) attributes: treat as equivalent via `LocalName`.

## Query Batching Guidance
- For this repository's Authoring GraphQL usage, current practical default is 250 for paged descendant discovery.
- Keep batch size configurable, because complexity depends on query shape and selected fields.
- If complexity/timeout errors occur, retry with smaller batches (for example 100, 50, 20).
- Query complexity depends on the exact query shape and selected fields.
- On complexity failures (for example "too complex"), retry with smaller batches.

## Testing Recommendations
- Unit tests for helper methods (`NormalizeGuid`, `ExtractDynamicPlaceholderId`, `RootPlaceholder`).
- Parameterized tests for casing and malformed query strings.
- Integration-style tests with sample layout XML covering shared/final variants.
- Golden tests validating reason codes for known invalid placements.
