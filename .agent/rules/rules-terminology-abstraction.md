---
trigger: always_on
---

# Terminology Abstraction Rules

## 1.1 Use Terminology Services

```csharp
// ✅ DO: Use terminology service
var termName = _terminologyService.GetStatName("health"); // Returns "Vitality"
_renderer.Display($"Your {termName} is low!");

// ❌ DON'T: Hardcode terminology
_renderer.Display("Your Health is low!");
```

## 1.2 Configuration-Driven Labels
All these should come from `config/terminology.json`:
- Stat names (Health, Attack, Defense)
- Progression terms (Level, XP, Experience)
- Currency names (Gold, Coins, Runes)
- UI labels (Inventory, Equipment, Save Game)
- Achievement/milestone text

## 1.3 Abbreviations and Symbols
Always define alongside full names:

```json
{
  "health": {
    "name": "Vitality",
    "abbreviation": "VIT",
    "symbol": "❤",
    "description": "Your life force..."
  }
}
```