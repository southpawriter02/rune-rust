# Shared Layer Guidelines

**Version:** 0.13.5e  
**Location:** `src/Presentation/RuneAndRust.Presentation.Shared/`

---

## What Belongs in Presentation.Shared

### ✓ Include

| Category | Examples |
|----------|----------|
| Platform-agnostic utilities | `FormatUtilities`, `GridUtilities`, `ValidationUtilities` |
| Shared interfaces | `IThemeService`, `IComponentLifecycle` |
| Shared enums | `ColorKey`, `IconKey`, `Direction`, `StatusType` |
| Shared value objects | `ThemeColor`, `RenderErrorContext` |
| Extension methods | `UiLoggingExtensions` |
| Constants | Unicode/ASCII fallback characters |

### ✗ Exclude

| Category | Reason |
|----------|--------|
| TUI-specific code | `ConsoleColor`, Spectre.Console components |
| GUI-specific code | Avalonia controls, Brushes |
| Platform implementations | Terminal adapters, window managers |
| UI component classes | Should remain in Tui/Gui projects |

---

## Design Principles

1. **Prefer static utility classes** for stateless operations
2. **Use interfaces** for dependencies requiring implementation
3. **Keep shared code minimal** and focused
4. **Avoid platform dependencies** — no Spectre.Console, no Avalonia
5. **Test utilities independently** with comprehensive unit tests

---

## Naming Conventions

| Type | Suffix | Example |
|------|--------|---------|
| Utilities | `*Utilities` | `FormatUtilities`, `GridUtilities` |
| Extensions | `*Extensions` | `UiLoggingExtensions` |
| Interfaces | `I*` | `IThemeService` |
| Enums | Descriptive name | `ColorKey`, `Direction` |
| Value Objects | Descriptive name | `ThemeColor` |

---

## Extraction Criteria

### Extract to Presentation.Shared when:

- ✓ Same logic exists in both TUI and GUI
- ✓ Logic is platform-agnostic
- ✓ Logic is general-purpose (not component-specific)
- ✓ Multiple components benefit from sharing

### Keep in TUI/GUI layer when:

- ✓ Logic is platform-specific
- ✓ Logic is component-specific
- ✓ Only one layer uses the logic
- ✓ Logic requires platform-specific dependencies

---

## Dependency Rules

### Presentation.Shared MAY depend on:

- .NET Standard / .NET libraries
- `Microsoft.Extensions.Logging.Abstractions`
- Domain layer (for shared domain types like `DamageType`)

### Presentation.Shared MUST NOT depend on:

- `Spectre.Console`
- `Avalonia`
- Any TUI-specific library
- Any GUI-specific library
- `Presentation.Tui`
- `Presentation.Gui`

---

## Current Shared Utilities

| Utility | Purpose | Key Methods |
|---------|---------|-------------|
| `FormatUtilities` | Formatting values | `FormatPercentage`, `FormatDuration`, `FormatDelta` |
| `GridUtilities` | Grid math | `CalculateGridPosition`, `IsValidPosition`, `GetAdjacentPositions` |
| `IconUtilities` | Icon lookup | `GetDirectionIcon`, `GetDamageTypeIcon`, `GetStatusIcon` |
| `ValidationUtilities` | Validation | `ValidatePercentage`, `ClampValue`, `NormalizeValue` |
| `RenderErrorHandler` | Error handling | `HandleRenderError`, `GetFallbackContent` |

---

## Usage Example

```csharp
using RuneAndRust.Presentation.Shared.Utilities;

public class HealthBarComponent
{
    public string FormatHealth(int current, int max)
    {
        // Use shared utility instead of local method
        return FormatUtilities.FormatPercentage(current, max);
    }
    
    public bool IsPositionValid(int x, int y, int width, int height)
    {
        return GridUtilities.IsValidPosition(x, y, width, height);
    }
}
```
