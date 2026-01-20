---
trigger: always_on
---

# Theme Preset System

### 1.1 Supported Themes
Code should support at minimum:
- **Dark Fantasy** — Grim, atmospheric, horror elements
- **High Fantasy** — Epic, heroic, wondrous
- **Horror** — Terrifying, suspenseful
- **Custom** — User-defined theme presets

### 1.2 Theme Configuration Structure
```json
{
  "activeTheme": "dark_fantasy",
  "themes": {
    "dark_fantasy": {
      "descriptorOverrides": {...},
      "excludedTerms": ["cheerful", "bright"],
      "emphasizedTerms": ["shadows", "dread"]
    }
  }
}
```

### 1.3 Theme-Agnostic Code
```csharp
// ✅ DO: Use theme-agnostic descriptor service
var lighting = _descriptorService.Get("environmental.lighting");

// ❌ DON'T: Hardcode atmosphere
var lighting = "The room is dimly lit by flickering torches";
```