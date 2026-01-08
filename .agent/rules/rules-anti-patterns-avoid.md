---
trigger: always_on
---

# Anti-Patterns to Avoid

## 1.1 Hardcoded Terminology
```csharp
// ❌ BAD: Hardcoded
Console.WriteLine("You gained 50 XP!");

// ✅ GOOD: Configurable
var xpTerm = _terminology.Get("progression.experience.name");
Console.WriteLine($"You gained 50 {xpTerm}!");
```

## 1.2 Enum-Based Type Systems
```csharp
// ❌ BAD: Enum prevents extension
public enum DamageType { Fire, Ice, Poison, Lightning }

// ✅ GOOD: String ID allows config-based extension
public string DamageTypeId { get; set; } // Loaded from damage-types.json
```

## 1.3 Single-Phrase Outputs
```csharp
// ❌ BAD: Always same text
return "You strike the goblin!";

// ✅ GOOD: Varied, contextual
var verb = _lexicon.GetTerm("attack", "combat");
var target = monster.Name;
return $"You {verb} the {target}!";
```

## 1.4 Theme-Specific Code
```csharp
// ❌ BAD: Assumes dark fantasy
var desc = "Shadows dance at the edge of your vision";

// ✅ GOOD: Theme-aware service
var desc = _descriptorService.Get("environmental.lighting", _themeConfig.ActiveTheme);
```