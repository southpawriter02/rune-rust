---
trigger: always_on
---

# Behavior & Functionality Customization

## 1.1 Formula Configuration
Combat/progression formulas should be configurable:
```json
{
  "derivedStats": {
    "meleeDamageBonus": "floor((might - 10) / 2)",
    "maxMana": "will * 5"
  }
}
```

## 1.2 AI Behavior Patterns
Monster behaviors should be tags/traits, not hardcoded:
- `aggressive`, `defensive`, `fleeing`
- `pack-tactics`, `solo-hunter`
- Traits loaded from configuration

## 1.3 Progression Curves
Support configurable level-up curves:
- Linear: `baseXP * level`
- Exponential: `baseXP * (growthFactor ^ level)`
- Custom: Explicit XP thresholds per level