# System Documentation

This directory contains comprehensive documentation for all game systems using the **5-layer documentation structure**:

1. **Functional** - What it does (player perspective)
2. **Statistical** - The numbers (formulas, values, ranges)
3. **Technical** - How it works (code, database, architecture)
4. **Testing** - How we verify (test coverage, QA checklists)
5. **Balance** - Why these numbers (design intent, tuning rationale)

---

## Documentation Index

### Tier 1: Core Combat Systems
- [x] **[Combat Resolution](./combat-resolution.md)** - Turn order, initiative, action economy
- [x] **[Accuracy & Evasion](./accuracy-evasion.md)** - Attack rolls, Defense Score, hit calculation
- [x] **[Damage Calculation](./damage-calculation.md)** - Damage formulas, Soak, mitigation
- [x] **[Status Effects](./status-effects.md)** - All status effects with durations and mechanics

### Tier 2: Progression & Resources
- [ ] **[Legend & Leveling](./legend-leveling.md)** - XP system, level thresholds, rewards
- [ ] **[Progression Points (PP)](./progression-points.md)** - PP earning, spending, budgets
- [ ] **[Attribute System](./attribute-system.md)** - 5 attributes, scaling, costs
- [ ] **[Resource Pools](./resource-pools.md)** - HP, Stamina calculation and regeneration
- [ ] **[Trauma Economy](./trauma-economy.md)** - Stress, Corruption, Breaking Points, Traumas

### Tier 3: Character Systems
- [ ] **[Specializations](./specializations.md)** - All 6 specializations, requirements, features
- [ ] **[Ability System](./ability-system.md)** - Ability acquisition, costs, cooldowns
- [ ] **[Passive Abilities](./passive-abilities.md)** - Always-on effects and modifiers

### Tier 4: Equipment & Items
- [ ] **[Equipment System](./equipment-system.md)** - Slots, equipping, requirements
- [ ] **[Quality Tiers](./quality-tiers.md)** - 5 quality tiers with modifiers
- [ ] **[Weapons](./weapons.md)** - Weapon types, damage dice, properties
- [ ] **[Armor](./armor.md)** - Armor types, Soak values, weight classes
- [ ] **[Accessories](./accessories.md)** - Accessory slot and effects
- [ ] **[Consumables](./consumables.md)** - Consumable items and effects

### Tier 5: Enemies & Encounters
- [ ] **[Enemy System](./enemy-system.md)** - Enemy types, stats, scaling
- [ ] **[Enemy AI](./enemy-ai.md)** - AI behavior, targeting, tactics
- [ ] **[Encounter Design](./encounter-design.md)** - Group composition, difficulty
- [ ] **[Boss Mechanics](./boss-mechanics.md)** - Special boss abilities and phases

### Tier 6: Procedural Generation
- [ ] **[Wave Function Collapse](./wave-function-collapse.md)** - WFC algorithm implementation
- [ ] **[Sector Generation](./sector-generation.md)** - Sector creation pipeline
- [ ] **[Room Templates](./room-templates.md)** - 30+ room templates
- [ ] **[Quest Anchors](./quest-anchors.md)** - Quest objective placement
- [ ] **[Biome System](./biome-system.md)** - Biome definitions ([The Roots])
- [ ] **[Population](./population.md)** - Enemy and loot spawning

### Tier 7: World State & Persistence
- [ ] **[World State Changes](./world-state-changes.md)** - State modification tracking
- [ ] **[Destructible Environments](./destructible-environments.md)** - Terrain destruction
- [ ] **[Save/Load System](./save-load-system.md)** - Persistence implementation
- [ ] **[State Restoration](./state-restoration.md)** - Loading and applying changes

### Tier 8: Quest & Dialogue Systems
- [ ] **[Quest System](./quest-system.md)** - Quest generation, types, objectives
- [ ] **[Quest Objectives](./quest-objectives.md)** - Kill, Explore, Interact objectives
- [ ] **[Dialogue System](./dialogue-system.md)** - Dialogue trees, choices
- [ ] **[Skill Checks](./skill-checks.md)** - Attribute-based checks in dialogue

---

## Template Usage

Each system document follows this template: [System Documentation Template](../templates/system-documentation-template.md)

### Template Sections:
1. **Layer 1: Functional Overview** - Player experience, features, edge cases
2. **Layer 2: Statistical Reference** - Formulas, values, probability tables
3. **Layer 3: Technical Implementation** - Code, database, integration
4. **Layer 4: Testing Coverage** - Tests, coverage, QA checklists
5. **Layer 5: Balance Considerations** - Design intent, tuning rationale

---

## Documentation Guidelines

### Writing Standards
- ✅ Use exact numbers, not vague terms
- ✅ Include executable formulas with variable definitions
- ✅ Reference file paths and line numbers
- ✅ Document edge cases and special behaviors
- ✅ Cross-reference related systems

### Code References
When referencing code, use this format:
- File: `RuneAndRust.Engine/CombatEngine.cs:123`
- Method: `CombatEngine.CalculateDamage():45`
- Class: `RuneAndRust.Core/PlayerCharacter.cs`

### Formula Format
```
Formula_Name = Mathematical_Expression

Variables:
- variable_name: Description (range: min-max, default: value)
- variable_name: Description (range: min-max, default: value)

Example:
Input: A = 5, B = 10
Calculation: Result = (A × 2) + B = (5 × 2) + 10 = 20
Output: Result = 20
```

---

## Cross-References

Each system document should link to:
- **Related Systems** - Other systems it integrates with
- **Statistical Registry** - Specific entries (abilities, equipment, enemies)
- **Technical Reference** - Database schemas, service APIs
- **Testing Documentation** - Test specifications for this system

---

## Progress Tracking

**Systems Documented:** 4 / 30+
**Progress:** ~13%
**Status:** 🚧 In Progress

### Priority Order
1. ✅ Combat systems (foundation for everything else)
2. Progression & resources (character advancement)
3. Character systems (specializations, abilities)
4. Equipment & items (itemization)
5. Enemies & encounters (content)
6. Procedural generation (variety)
7. World state & persistence (saves)
8. Quest & dialogue (narrative)

---

**Last Updated:** 2025-11-12
**Documentation Version:** v0.17
