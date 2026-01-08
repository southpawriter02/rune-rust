---
trigger: always_on
---

# Magic System Agnosticism

## 1.1 Generic Resource Model
Don't assume "mana" — support configurable resources:
- Mana, Arcane Power, Spirit
- Rage, Fury, Battle Spirit
- Energy, Stamina, Ki
- Faith, Divine Power, Holy Light

## 1.2 Ability Effect Abstraction
Effects should be data-driven, not hardcoded:
```json
{
  "effects": [
    {
      "effectType": "Damage",
      "value": 25,
      "damageTypeId": "fire",
      "scalingStat": "will",
      "scalingMultiplier": 0.5
    }
  ]
}
```

## 1.3 Casting Terminology
Use lexicon for casting descriptions:
- "channels energy" / "incants ancient words" / "focuses power"
- Allow themes to override: primal (communes with nature), dark (draws from shadows)