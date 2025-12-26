# System Documentation

Sub-item: Attribute System (Attribute%20System%202ba55eb312da80938125ca091586f612.md), Accuracy & Evasion System (Accuracy%20&%20Evasion%20System%202ba55eb312da8066a03edb0798b7e3b1.md), Combat Resolution System (Combat%20Resolution%20System%202ba55eb312da80599ee2eb4b959ed453.md), Resource Pools (HP, Stamina, AP) (Resource%20Pools%20(HP,%20Stamina,%20AP)%202ba55eb312da8081ba73de96323af8dc.md), Status Effects System (Status%20Effects%20System%202ba55eb312da8038bccadce128382f6f.md), Psychic Stress & Corruption System (Psychic%20Stress%20&%20Corruption%20System%202ba55eb312da80cf9214e60d92546cbc.md), Breaking Points & Permanent Traumas (Breaking%20Points%20&%20Permanent%20Traumas%202ba55eb312da802e83f7c91ad79238ee.md)

This directory contains comprehensive documentation for all game systems using the **5-layer documentation structure**:

1. **Functional** - What it does (player perspective)
2. **Statistical** - The numbers (formulas, values, ranges)
3. **Technical** - How it works (code, database, architecture)
4. **Testing** - How we verify (test coverage, QA checklists)
5. **Balance** - Why these numbers (design intent, tuning rationale)

---

## Documentation Index

### Tier 1: Core Combat Systems

- [x]  [**Combat Resolution**](https://www.notion.so/combat-resolution.md) - Turn order, initiative, action economy
- [x]  [**Accuracy & Evasion**](https://www.notion.so/accuracy-evasion.md) - Attack rolls, Defense Score, hit calculation
- [x]  [**Damage Calculation**](https://www.notion.so/damage-calculation.md) - Damage formulas, Soak, mitigation
- [x]  [**Status Effects**](https://www.notion.so/status-effects.md) - All status effects with durations and mechanics

### Tier 2: Progression & Resources

- [ ]  [**Legend & Leveling**](https://www.notion.so/legend-leveling.md) - XP system, level thresholds, rewards
- [ ]  [**Progression Points (PP)**](https://www.notion.so/progression-points.md) - PP earning, spending, budgets
- [ ]  [**Attribute System**](https://www.notion.so/attribute-system.md) - 5 attributes, scaling, costs
- [ ]  [**Resource Pools**](https://www.notion.so/resource-pools.md) - HP, Stamina calculation and regeneration
- [ ]  [**Trauma Economy**](https://www.notion.so/trauma-economy.md) - Stress, Corruption, Breaking Points, Traumas

### Tier 3: Character Systems

- [ ]  [**Specializations**](https://www.notion.so/specializations.md) - All 6 specializations, requirements, features
- [ ]  [**Ability System**](https://www.notion.so/ability-system.md) - Ability acquisition, costs, cooldowns
- [ ]  [**Passive Abilities**](https://www.notion.so/passive-abilities.md) - Always-on effects and modifiers

### Tier 4: Equipment & Items

- [ ]  [**Equipment System**](https://www.notion.so/equipment-system.md) - Slots, equipping, requirements
- [ ]  [**Quality Tiers**](https://www.notion.so/quality-tiers.md) - 5 quality tiers with modifiers
- [ ]  [**Weapons**](https://www.notion.so/weapons.md) - Weapon types, damage dice, properties
- [ ]  [**Armor**](https://www.notion.so/armor.md) - Armor types, Soak values, weight classes
- [ ]  [**Accessories**](https://www.notion.so/accessories.md) - Accessory slot and effects
- [ ]  [**Consumables**](https://www.notion.so/consumables.md) - Consumable items and effects

### Tier 5: Enemies & Encounters

- [ ]  [**Enemy System**](https://www.notion.so/enemy-system.md) - Enemy types, stats, scaling
- [ ]  [**Enemy AI**](https://www.notion.so/enemy-ai.md) - AI behavior, targeting, tactics
- [ ]  [**Encounter Design**](https://www.notion.so/encounter-design.md) - Group composition, difficulty
- [ ]  [**Boss Mechanics**](https://www.notion.so/boss-mechanics.md) - Special boss abilities and phases

### Tier 6: Procedural Generation

- [ ]  [**Wave Function Collapse**](https://www.notion.so/wave-function-collapse.md) - WFC algorithm implementation
- [ ]  [**Sector Generation**](https://www.notion.so/sector-generation.md) - Sector creation pipeline
- [ ]  [**Room Templates**](https://www.notion.so/room-templates.md) - 30+ room templates
- [ ]  [**Quest Anchors**](https://www.notion.so/quest-anchors.md) - Quest objective placement
- [ ]  [**Biome System**](https://www.notion.so/biome-system.md) - Biome definitions ([The Roots])
- [ ]  [**Population**](https://www.notion.so/population.md) - Enemy and loot spawning

### Tier 7: World State & Persistence

- [ ]  [**World State Changes**](https://www.notion.so/world-state-changes.md) - State modification tracking
- [ ]  [**Destructible Environments**](https://www.notion.so/destructible-environments.md) - Terrain destruction
- [ ]  [**Save/Load System**](https://www.notion.so/save-load-system.md) - Persistence implementation
- [ ]  [**State Restoration**](https://www.notion.so/state-restoration.md) - Loading and applying changes

### Tier 8: Quest & Dialogue Systems

- [ ]  [**Quest System**](https://www.notion.so/quest-system.md) - Quest generation, types, objectives
- [ ]  [**Quest Objectives**](https://www.notion.so/quest-objectives.md) - Kill, Explore, Interact objectives
- [ ]  [**Dialogue System**](https://www.notion.so/dialogue-system.md) - Dialogue trees, choices
- [ ]  [**Skill Checks**](https://www.notion.so/skill-checks.md) - Attribute-based checks in dialogue

---

## Template Usage

Each system document follows this template: [System Documentation Template](https://www.notion.so/templates/system-documentation-template.md)

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