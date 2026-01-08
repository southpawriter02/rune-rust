---
trigger: always_on
---

# Lexicon System Rules

## 1.1 Synonym Pool Structure
Every action word should have:
- **Default** — Fallback term
- **Synonyms** — General alternatives
- **Contextual** — Situation-specific options
- **Weights** — Selection probability

```json
{
  "attack": {
    "default": "attack",
    "synonyms": ["strike", "slash", "swing", "thrust"],
    "contextual": {
      "combat": ["strike", "slash", "thrust"],
      "formal": ["engage", "assail"]
    },
    "weights": { "strike": 30, "slash": 25, "swing": 20 }
  }
}
```

## 1.2 Context-Aware Selection
Always pass context to lexicon lookups:
```csharp
// Use combat context during battle
var verb = _lexiconService.GetTerm("attack", context: "combat");

// Use exploration context for movement
var moveVerb = _lexiconService.GetTerm("move", context: "exploration");
```

## 1.3 Severity-Based Descriptors
For values that scale, define severity tiers:

| Category   | Tiers                                         |
|------------|-----------------------------------------------|
| Damage     | light, moderate, heavy, critical              |
| Quantities | none, one, few, many, horde                   |
| Condition  | healthy, wounded, bloodied, nearDeath         |
| Success    | failure, partialSuccess, success, critSuccess |