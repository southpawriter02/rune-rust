# Statistical Registry

Parent item: Technical Reference (Technical%20Reference%202ba55eb312da8079a291e020980301c1.md)

This directory contains the **complete statistical database** for all game content. Every ability, enemy, equipment piece, status effect, and formula is catalogued here with **exact values**.

---

## Purpose

The Statistical Registry serves as the **single source of truth** for all game numbers. Before making balance changes, consult this registry to understand current values and relationships.

### Use Cases

- **Balance Design:** Compare abilities, find outliers, identify power gaps
- **Content Creation:** Reference templates and value ranges for new content
- **Development:** Lookup exact values instead of searching code
- **Testing:** Verify implementations match specifications

---

## Registry Index

### 📖 [Abilities Registry](https://www.notion.so/abilities-registry.md)

Complete catalogue of all 45+ abilities with exact stats.

**Entries Include:**

- Ability ID, name, specialization
- Tier, type (Active/Passive), category
- Resource costs (Stamina, Stress, Corruption, Aether)
- Effect values (damage, healing, durations)
- Rank progression (Rank 1-3)
- Synergies and anti-synergies
- Balance data (DPS, efficiency)

**Total Abilities:** 45+ (as of v0.16)

---

### ⚔️ [Equipment Registry](https://www.notion.so/equipment-registry.md)

Complete catalogue of all 60+ equipment pieces with stats.

**Entries Include:**

- Item ID, name, slot, type
- Quality tier (Jury-Rigged → Masterwork)
- Stats (damage dice, Soak, attribute bonuses)
- Special properties
- Acquisition sources
- Synergies and build recommendations
- Balance data (DPS, survivability)

**Total Equipment:** 60+ (as of v0.16)

**Breakdown:**

- Weapons: 30+
- Armor: 20+
- Accessories: 10+

---

### 👾 [Enemy Registry](https://www.notion.so/enemy-registry.md)

Complete bestiary of all 20+ enemy types with stat blocks.

**Entries Include:**

- Enemy ID, name, type
- Threat level (Trivial → Lethal)
- Base stats (HP, Defense, Soak, Initiative)
- Attributes (MIGHT, FINESSE, WITS, WILL, STURDINESS)
- Resistances, vulnerabilities, immunities
- Abilities (3-5 per enemy)
- AI behavior patterns
- Loot tables
- Scaling formulas
- Trauma impact (Stress, Corruption)

**Total Enemies:** 20+ (as of v0.16)

**Breakdown by Type:**

- Draugr-Pattern: 8
- Symbiotic Plate: 7
- Haugbui-Class: 3
- Jötun-Reader: 2
- Forlorn: 1+

---

### 🎯 [Status Effects Registry](https://www.notion.so/status-effects-registry.md)

Complete list of all 12+ status effects with mechanics.

**Entries Include:**

- Status effect ID, name, category
- Duration (turns, conditions)
- Effects (exact values)
- Stack behavior (if applicable)
- Application conditions
- Removal conditions
- Interactions with other effects

**Total Status Effects:** 12+ (as of v0.16)

**Categories:**

- Buffs: 4
- Debuffs: 5
- Damage-over-Time: 3
- Control: 3

---

### 📐 [Formulas Registry](https://www.notion.so/formulas-registry.md)

Complete collection of all calculation formulas.

**Formulas Include:**

- Combat formulas (damage, accuracy, initiative)
- Progression formulas (XP, PP, attributes)
- Resource formulas (HP, Stamina, Stress, Corruption)
- Scaling formulas (enemy stats, loot quality)
- Procedural generation formulas (room placement, weights)

**Total Formulas:** 50+ (as of v0.16)

---

### 💰 [Resource Costs Registry](https://www.notion.so/resource-costs-registry.md)

Complete breakdown of all resource costs and rates.

**Cost Tables:**

- Ability costs (Stamina, Stress, Corruption per ability)
- Attribute increase costs (PP per point)
- Action costs (Stamina per action type)
- Regeneration rates (HP, Stamina, Stress, Corruption)

---

### 🏆 [Loot Tables Registry](https://www.notion.so/loot-tables-registry.md)

Complete loot drop tables for all sources.

**Loot Tables:**

- Enemy drops (by enemy type)
- Room loot (by room archetype)
- Quest rewards (by quest tier)
- Boss loot (guaranteed + rare drops)

---

## Template Usage

Each registry entry follows a specific template:

- **Abilities:** [Ability Registry Entry Template](https://www.notion.so/templates/ability-registry-entry.md)
- **Equipment:** [Equipment Registry Entry Template](https://www.notion.so/templates/equipment-registry-entry.md)
- **Enemies:** [Enemy Bestiary Entry Template](https://www.notion.so/templates/enemy-bestiary-entry.md)

---

## Registry Format

### Ability Entry Example

```markdown
## Corrosive Curse

**Ability ID:** `42`
**Specialization:** Rust-Witch
**Tier:** 1
**Type:** Active

**Cost:**
- Stamina: 0
- Aether: 25
- Corruption: +2

**Effect:**
- Damage: 2d6 Arcane
- Status: [Corroded] for 3 turns
  - DoT: 1d6/turn
  - Armor Reduction: -2
  - Max Stacks: 5
- Special: 70% [Stunned] vs Mechanical/Undying (1 turn)

```

### Equipment Entry Example

```markdown
## Thunder Hammer

**Item ID:** `weapon_007`
**Type:** Weapon (HeavyBlunt)
**Quality:** MythForged (Tier 4)

**Stats:**
- Damage: 5d10 + 8 (Lightning)
- Range: Melee
- Hands: 2

**Special:**
- AOE: Hits all enemies in front row
- Knockdown: 80% chance, 1 turn

```

### Enemy Entry Example

```markdown
## Sentinel Prime

**Enemy ID:** `enemy_010`
**Type:** Draugr-Pattern
**Threat:** Lethal

**Stats:**
- HP: 90 (base at Legend 1)
- Defense: 16
- Soak: 8

**Abilities:**
1. Plasma Rifle (3d10+10 Lightning, Long range)
2. Tactical Analysis (AOE debuff, -2 Defense)
3. Emergency Protocols (Self-heal 30 HP, once per fight)

```

---

## Data Validation

All registry entries must be validated against code:

### Validation Checklist

- [ ]  Stat values match code implementation
- [ ]  Formulas produce correct outputs
- [ ]  All variables defined with ranges
- [ ]  Edge cases documented
- [ ]  Cross-references accurate

### Validation Tests

Tests should verify registry accuracy:

```csharp
[TestMethod]
public void Registry_AbilityStats_MatchCodeImplementation()
{
    // Load ability from registry
    var registryAbility = LoadFromRegistry("Corrosive Curse");

    // Load ability from code
    var codeAbility = AbilityDatabase.GetCorrosiveCurse();

    // Assert values match
    Assert.AreEqual(registryAbility.Damage, codeAbility.BaseDamage);
    Assert.AreEqual(registryAbility.Cost.Aether, codeAbility.AetherCost);
}

```

---

## Usage Examples

### Finding Overpowered Abilities

```
1. Open abilities-registry.md
2. Search for DPS values
3. Compare across same tier
4. Identify outliers (>20% above average)

```

### Comparing Equipment

```
1. Open equipment-registry.md
2. Filter by quality tier
3. Compare stat values
4. Identify best-in-slot for each build

```

### Enemy Weakness Analysis

```
1. Open enemy-registry.md
2. Check resistances/vulnerabilities section
3. Cross-reference with ability damage types
4. Build effectiveness matrix

```

---

## Maintenance

### When to Update Registry

- **After every balance change** - Update affected entries immediately
- **After adding new content** - Create new registry entries
- **After bug fixes** - Verify values still match
- **Monthly review** - Spot-check random entries vs code

### Update Process

1. Make code change
2. Update registry entry
3. Run validation tests
4. Update changelog in entry
5. Commit both code and registry together

---

## Export Formats

Registry data can be exported to:

### CSV Format

For spreadsheet analysis:

```
ability_id,name,tier,stamina_cost,damage,dps_at_legend_5
42,Corrosive Curse,1,0,2d6,18.5

```

### JSON Format

For programmatic access:

```json
{
  "ability_id": 42,
  "name": "Corrosive Curse",
  "tier": 1,
  "cost": {
    "stamina": 0,
    "aether": 25,
    "corruption": 2
  }
}

```

---

## Progress Tracking

**Registry Status:**

- Abilities: 0 / 45+ documented
- Equipment: 0 / 60+ documented
- Enemies: 0 / 20+ documented
- Status Effects: 0 / 12+ documented
- Formulas: 0 / 50+ documented

**Overall Progress:** 0%

---

**Last Updated:** 2025-11-12
**Documentation Version:** v0.17