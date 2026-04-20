# defaultParameters – Encoding Reference

**Status:** MANDATORY for all component mappings in `component-map.json`

---

## What Are defaultParameters?

In Sitecore layout XML, each `<r>` (rendering) element has a `par` attribute that holds rendering parameters as URL-encoded key-value pairs:

```xml
<r id="{RENDERING-GUID}" ph="main" par="Styles%26CacheClearingBehavior=Clear+on+publish" ds="local:/Data/Item" uid="{UID}" />
```

- `par` — rendering parameters; stored as **URL-encoded key-value pairs**.
- `defaultParameters` in `component-map.json` is written verbatim into `par` at migration time.
- Sitecore reads `par` as URL-encoded; unencoded characters cause silent rendering failures at runtime.

---

## Golden Rule
**`defaultParameters` must be URL-encoded.** ⚠️

---

## Encoding Table

| Character | Plain | URL-Encoded | Example |
|-----------|-------|-------------|---------|
| `&` (parameter separator) | `&` | `%26` (or `\u0026` in JSON) | `a&b` → `a%26b` |
| ` ` (space) | ` ` | `+` (preferred) or `%20` | `Clear on publish` → `Clear+on+publish` |
| `,` (list separator) | `,` | `%2c` | `1,2` → `1%2c2` |
| `\|` (pipe/GUID list) | `\|` | `%7c` | `g1\|g2` → `g1%7cg2` |
| `{` (literal GUID) | `{` | `%7B` | `{GUID}` → `%7BGUID%7D` |
| `}` (literal GUID) | `}` | `%7D` | see above |
| `%` (literal percent) | `%` | `%25` | rare |
| `=` (key=value) | `=` | `=` (no encoding needed) | `Width=100` stays `Width=100` |

> **Sitecore tokens** like `{DYNAMIC_PLACEHOLDER_ID}` must **not** be encoded — the migrator replaces them at build time.

---

## Examples

### ❌ BROKEN

```json
"defaultParameters": "Styles5&Styles3&CacheClearingBehavior=Clear on publish&EnabledPlaceholders=1,2&DynamicPlaceholderId={DYNAMIC_PLACEHOLDER_ID}&SplitterSize=2"
```

Problems: unencoded `&`, space, and comma.

### ✅ CORRECT

```json
"defaultParameters": "Styles5%26Styles3%26CacheClearingBehavior=Clear+on+publish%26EnabledPlaceholders=1%2c2%26DynamicPlaceholderId={DYNAMIC_PLACEHOLDER_ID}%26SplitterSize=2"
```

### 🔄 ALTERNATIVE (JSON unicode escapes)

```json
"defaultParameters": "Styles5\u0026Styles3\u0026CacheClearingBehavior=Clear+on+publish\u0026EnabledPlaceholders=1%2c2\u0026DynamicPlaceholderId={DYNAMIC_PLACEHOLDER_ID}\u0026SplitterSize=2"
```

Both correct; pick one style and apply it consistently.

---

## Copy-Paste Workflow

1. Extract `s:par="..."` from Sitecore layout XML.
2. Re-encode all `&` separators as `%26` (or `\u0026`).
3. Encode spaces as `+`, commas as `%2c`.
4. Leave Sitecore token braces unencoded; encode literal GUID braces.
5. Paste as `"defaultParameters": "..."` in `component-map.json`.
6. After migration, compare output `s:par` in logs with your value.

---

## Common Mistakes

| Mistake | Impact | Fix |
|---------|--------|-----|
| Unencoded `&` between parameters | Rendering silently fails | Encode as `%26` or `\u0026` |
| Literal space in value | Attribute truncated or rejected | Replace with `+` |
| Unencoded comma in list | Ambiguous parameter parsing | Replace with `%2c` |
| Encoded `{DYNAMIC_PLACEHOLDER_ID}` | Token not recognized by migrator | Leave braces unencoded for tokens |
| Mixed encoding styles | Parser confusion | Pick one style throughout |
| Unencoded braces in literal GUIDs | XML parser error | Encode as `%7B` / `%7D` |

---

## Validation Checklist

Before committing a new `defaultParameters` value:

- [ ] All `&` separators encoded as `%26` (or `\u0026`)
- [ ] All spaces in values encoded as `+`
- [ ] All commas in lists encoded as `%2c`
- [ ] Sitecore tokens like `{DYNAMIC_PLACEHOLDER_ID}` left with unencoded braces
- [ ] Literal GUIDs encoded as `%7B...%7D`
- [ ] Output `s:par` in migration logs matches the expected value

---

## Runtime Symptom

- Migration completes without errors.
- Layout XML is written.
- **But:** Rendering appears broken or empty in Sitecore Content Editor.
- **Cause:** Malformed `par` encoding; Sitecore parameter parsing fails silently.

---

## References

- RFC 3986: https://tools.ietf.org/html/rfc3986
- Sitecore rendering parameters: https://doc.sitecore.com/xmc/en/content-management/121/digital-experience-manager/add-a-rendering.html
