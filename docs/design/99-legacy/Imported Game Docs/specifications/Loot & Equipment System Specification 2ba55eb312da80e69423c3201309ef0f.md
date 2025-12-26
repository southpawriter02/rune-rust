# Loot & Equipment System Specification

Parent item: Specs: Economy (Specs%20Economy%202ba55eb312da8027b1e6d535f27ee714.md)

> Template Version: 1.0
Last Updated: 2025-11-21
Status: Draft
Specification ID: SPEC-ECONOMY-001
> 

---

## Document Control

### Version History

| Version | Date | Author | Changes | Reviewers |
| --- | --- | --- | --- | --- |
| 1.0 | 2025-11-21 | AI | Initial specification | - |

### Approval Status

- [x]  **Draft**: Initial authoring in progress
- [ ]  **Review**: Ready for stakeholder review
- [ ]  **Approved**: Approved for implementation
- [ ]  **Active**: Currently implemented and maintained
- [ ]  **Deprecated**: Superseded by [SPEC-ID]

### Stakeholders

- **Primary Owner**: Economy Designer
- **Design**: Equipment progression philosophy, loot rarity balance
- **Balance**: Power curve tuning, quality tier scaling, drop rates
- **Implementation**: EquipmentDatabase.cs, LootService.cs, EquipmentService.cs
- **QA/Testing**: Drop rate verification, stat balance validation

---

## Executive Summary

### Purpose Statement

The Loot & Equipment System provides the primary power progression mechanism beyond character attributes, using a five-tier quality system (Jury-Rigged → Myth-Forged) where equipment drops from enemies, containers, and boss encounters grant damage bonuses, defensive stats, and attribute modifiers that scale character effectiveness throughout a run.

### Scope

**In Scope**:

- Five quality tiers (Jury-Rigged, Scavenged, Clan-Forged, Optimized, Myth-Forged)
- Equipment slots (Weapon, Armor, Accessory)
- Weapon categories (Axe, Greatsword, Spear, Dagger, Staff, Focus, plus v0.16 additions)
- Armor categories (Light, Medium, Heavy)
- Stat generation (damage dice, accuracy bonuses, HP bonuses, defense bonuses)
- Equipment bonuses (attribute modifiers beyond cap)
- Loot drop tables (enemy-based, tier-based, boss-specific)
- Loot generation service (random drops, class-appropriate drops)
- Special effects (Myth-Forged unique properties)

**Out of Scope**:

- Crafting system mechanics → `SPEC-ECONOMY-002` (planned)
- Equipment durability/degradation → Future enhancement
- Equipment enchanting/upgrading → Future enhancement
- Currency and merchant pricing → `SPEC-ECONOMY-004`, `SPEC-ECONOMY-005`
- Material drops and components → Partially implemented, full spec in `SPEC-ECONOMY-002`

### Success Criteria

- **Player Experience**: Equipment upgrades feel impactful; finding higher quality gear creates excitement; build diversity through equipment choice
- **Technical**: Equipment generation performant (<10ms per drop); stat ranges balanced; no duplicate drops in single encounter
- **Design**: Clear power progression across tiers (Tier 0 → Tier 4 = ~3× power increase); quality tiers align with run pacing
- **Balance**: Weapon upgrades prioritized over armor; Myth-Forged items rare (<5% drop rate); attribute bonuses exceed cap meaningfully

---

## Related Documentation

### Dependencies

**Depends On**:

- Damage Calculation System: Weapon damage dice and bonuses used in combat → `SPEC-COMBAT-002`
- Accuracy & Evasion System: Weapon accuracy bonuses, armor defense bonuses → `SPEC-COMBAT-004`
- Character Progression: Attribute system for equipment bonuses → `SPEC-PROGRESSION-001`
- Archetype System: Class-appropriate loot generation (Warrior/Adept/Skirmisher/Mystic) → `SPEC-PROGRESSION-002`
- Combat Resolution: Loot drops after enemy defeat → `SPEC-COMBAT-001`

**Depended Upon By**:

- Crafting System: Equipment as crafting inputs and outputs → `SPEC-ECONOMY-002` (planned)
- Merchant System: Equipment pricing and trading → `SPEC-ECONOMY-005` (planned)
- Currency System: Equipment value calculation → `SPEC-ECONOMY-004` (planned)

### Related Specifications

- `SPEC-COMBAT-002`: Damage Calculation - Uses weapon damage dice and bonuses
- `SPEC-COMBAT-004`: Accuracy & Evasion - Uses accuracy bonuses from weapons
- `SPEC-PROGRESSION-001`: Character Progression - Equipment bonuses exceed attribute caps
- `SPEC-ECONOMY-002`: Crafting & Resources (planned) - Equipment upgrade paths
- `SPEC-ECONOMY-004`: Currency & Transactions (planned) - Equipment value

### Implementation Documentation

- **Layer 1 Docs**: `docs/03-equipment/equipment-overview.md` - Equipment stats and progression
- **Code Reference**:
    - `RuneAndRust.Core/Equipment.cs` - Core equipment models and enums
    - `RuneAndRust.Engine/EquipmentDatabase.cs` - Static equipment definitions
    - `RuneAndRust.Engine/LootService.cs` - Loot generation and drop logic
    - `RuneAndRust.Engine/EquipmentService.cs` - Equipment management

### Code References

- **Primary Model**: `RuneAndRust.Core/Equipment.cs`
    - Lines 6-13: `QualityTier` enum (5 tiers)
    - Lines 18-23: `EquipmentType` enum (Weapon, Armor, Accessory)
    - Lines 28-43: `WeaponCategory` enum (11 weapon types)
    - Lines 48-53: `ArmorCategory` enum (Light, Medium, Heavy)
    - Lines 68-157: `Equipment` class (main data structure)
- **Equipment Database**: `RuneAndRust.Engine/EquipmentDatabase.cs`
    - Lines 106-288: Warrior weapons (Axes, Greatswords)
    - Lines 293-432: Skirmisher weapons (Spears, Daggers) - *Code uses legacy "Scavenger" term*
    - Lines 437-605: Mystic/Adept weapons (Staves, Focuses)
    - Lines 610-797: Armor (Light, Medium, Heavy by tier)
    - Lines 801-1186: v0.16 Content Expansion (new weapon/armor types)
- **Loot Service**: `RuneAndRust.Engine/LootService.cs`
    - Lines 22-43: `GenerateLoot()` - Main loot generation entry point
    - Lines 145-193: Enemy-specific loot tables (Servitor, Drone, Boss)
    - Lines 309-372: `GenerateLootForNode()` - Container/node loot
- **Tests**: `RuneAndRust.Tests/EquipmentServiceTests.cs`

---

## Design Philosophy

### Design Pillars

1. **Progressive Power Fantasy**
    - **Rationale**: Equipment provides immediate, tangible power increases that players can see and feel in combat; each quality tier represents a significant upgrade that changes combat effectiveness
    - **Examples**:
        - Jury-Rigged Hatchet (1d6) → Clan-Forged Axe (1d6+3) = +3 average damage (~86% increase)
        - Tattered Leathers (+5 HP) → Clan-Forged Leathers (+15 HP, +1 FINESSE) = +10 HP and attribute bonus
        - Myth-Forged weapons add unique effects (Ignores Armor, Lifesteal, Chain Lightning) that transform playstyle
    - **Trade-offs**: Power progression risk-dependent on RNG drops; player may finish run without finding desired tier
2. **Class Identity Through Equipment**
    - **Rationale**: Weapon categories align with character archetypes (Warrior=MIGHT, Skirmisher=FINESSE, Mystic/Adept=WILL), reinforcing class fantasy and ensuring loot feels appropriate for player build
    - **Examples**:
        - Warriors use Axes (balanced) and Greatswords (high damage, two-handed)
        - Skirmishers use Spears (reach) and Daggers (fast, low stamina)
        - Mystics/Adepts use Staves (Aether reduction) and Focuses (ability amplification, no melee)
        - Boss loot tables prioritize class-appropriate drops (60% chance for player class weapon)
    - **Trade-offs**: Class restrictions limit build experimentation; can't create FINESSE-based Warrior without hybrid design
3. **Meaningful Equipment Choices**
    - **Rationale**: Armor categories offer distinct trade-offs (Light=mobility vs Heavy=survivability); equipment bonuses create build decisions beyond raw stats
    - **Examples**:
        - Light Armor: +5-15 HP, +FINESSE bonuses, no movement penalty → Glass cannon builds
        - Heavy Armor: +15-40 HP, +STURDINESS bonuses, -FINESSE penalty → Tank builds
        - Special effects create situational value: "Regenerate 5 HP/turn" (Juggernaut Frame) vs "+2 Evasion" (Shadow Weave)
        - v0.18 balance: Heavy armor now has -FINESSE penalty, forcing meaningful choice vs medium armor
    - **Trade-offs**: Requires player understanding of stat interactions; new players may not recognize trade-offs immediately
4. **Loot Excitement Through Rarity**
    - **Rationale**: Quality tier rarity creates loot anticipation; Myth-Forged drops are memorable moments; progression feels earned through exploration and combat victories
    - **Examples**:
        - Trash mobs (Servitor): 60% Jury-Rigged, 30% Scavenged, 10% nothing → Common upgrades
        - Standard enemies (Drone): 40% Scavenged, 40% Clan-Forged, 20% Optimized → Mid-game power spikes
        - Bosses (Warden): 30% Optimized, 70% Myth-Forged → Guaranteed high-tier reward
        - v0.16 legendary weapons (Arc-Cannon, Rust-Eater, Heretical Blade) = ultra-rare, run-defining drops
    - **Trade-offs**: RNG can create frustrating dry streaks; bad luck with drops weakens player progression

### Player Experience Goals

**Target Experience**: "I just found a Myth-Forged weapon! This completely changes my build - I can ignore armor now and focus on offense. Time to hunt that boss."

**Moment-to-Moment Gameplay**:

- **Pre-Combat**: Player checks equipment loadout, considers swapping before boss fight
- **Post-Victory**: Loot icon appears - "What did I get?"
- **Discovery**: "Clan-Forged Greatsword - 1d6+5 damage! That's way better than my Scavenged Axe (1d6+1)"
- **Decision**: Keep or swap? Compare stats: +4 average damage but +3 stamina cost
- **Equip**: Player equips new weapon, immediately tests in next encounter
- **Validation**: "Wow, I'm hitting for 12 damage instead of 7 - this weapon rules!"

**Learning Curve**:

- **Novice** (0-2 hours): Understand quality tiers (higher = better); recognize weapon vs armor slots; basic stat comparison (damage numbers, HP bonus)
- **Intermediate** (2-10 hours): Value attribute bonuses (+MIGHT increases attack rolls beyond damage); recognize armor trade-offs (Light vs Heavy); prioritize weapon upgrades first
- **Expert** (10+ hours): Optimize builds around special effects; recognize BiS (Best-in-Slot) items per archetype; calculate DPS considering stamina costs and accuracy bonuses

### Design Constraints

- **Technical**: Equipment database static (hard-coded in C#); no procedural stat generation (fixed items); limited inventory slots (5 items)
- **Gameplay**: Two active equipment slots (Weapon, Armor); Accessory slot added in v0.16 but less impactful; no equipment durability (loot is permanent)
- **Narrative**: Equipment themed to Aethelgard setting (Dvergr alloys, Pre-Glitch tech, Blight corruption); Myth-Forged items have lore significance
- **Scope**: Current implementation has ~40+ weapons, ~20+ armor pieces across 5 tiers; v0.16 expansion added 10+ new items; future crafting system will extend loot economy

---

## Functional Requirements

> Completeness Checklist:
> 
> - [x]  All requirements have unique IDs (FR-001 through FR-008)
> - [x]  All requirements have priority assigned
> - [x]  All requirements have acceptance criteria
> - [x]  All requirements have at least one example scenario
> - [x]  All requirements trace to design goals
> - [x]  All requirements are testable

### FR-001: Equipment Quality Tier System

**Priority**: Critical
**Status**: Implemented

**Description**:
The system defines five discrete quality tiers (Jury-Rigged, Scavenged, Clan-Forged, Optimized, Myth-Forged) where each tier represents progressively higher stats, better bonuses, and unique special effects. Equipment of higher quality provides superior combat effectiveness through increased damage, defense, and attribute modifiers.

**Rationale**:
Quality tiers create clear progression milestones; players instantly recognize Myth-Forged > Scavenged; tier system enables balanced loot tables (trash mobs drop low-tier, bosses drop high-tier).

**Acceptance Criteria**:

- [ ]  Five quality tiers defined as enum: `QualityTier` (0=Jury-Rigged, 1=Scavenged, 2=Clan-Forged, 3=Optimized, 4=Myth-Forged)
- [ ]  Each tier has human-readable name displayed in UI
- [ ]  Equipment stats scale with quality (Tier 0: ~3.5 avg damage → Tier 4: ~11 avg damage for weapons)
- [ ]  Attribute bonuses exclusive to Tier 2+ equipment (+1 at Tier 2, +2-3 at Tier 4)
- [ ]  Special effects exclusive to Tier 4 (Myth-Forged) equipment
- [ ]  Quality tier displayed in equipment name: "Rusty Hatchet (Jury-Rigged)"

**Example Scenarios**:

1. **Scenario**: Axe progression across all tiers
    - **Tier 0**: Rusty Hatchet - 1d6 damage, -1 accuracy, no bonuses
    - **Tier 1**: Scavenged Axe - 1d6+1 damage, 0 accuracy, no bonuses
    - **Tier 2**: Clan-Forged Axe - 1d6+3 damage, 0 accuracy, +1 MIGHT
    - **Tier 3**: Optimized War Axe - 2d6 damage, +1 accuracy
    - **Tier 4**: Dvergr Maul - 2d6+4 damage, +2 MIGHT, special effect
    - **Success Condition**: Clear stat progression; ~3× damage from Tier 0 to Tier 4

**Dependencies**:

- Requires: Equipment class with `Quality` property
- Blocks: FR-003 (loot generation uses quality tiers)

**Implementation Notes**:

- **Code Location**: `RuneAndRust.Core/Equipment.cs:6-13` (QualityTier enum)
- **Data Requirements**: EquipmentDatabase entries tagged with quality
- **Performance Considerations**: Enum comparison O(1), no performance concerns

---

### FR-002: Weapon Category System

**Priority**: Critical
**Status**: Implemented

**Description**:
Weapons are categorized by type (Axe, Greatsword, Spear, Dagger, Staff, Focus, plus v0.16 additions: Blade, Blunt, EnergyMelee, Rifle, HeavyBlunt) and each category scales with a specific attribute (MIGHT, FINESSE, or WILL). Weapon categories determine damage dice range, stamina cost, and special properties like reach or ability amplification.

**Rationale**:
Weapon categories reinforce class archetypes; Warriors use MIGHT weapons (Axe, Greatsword), Skirmishers use FINESSE (Spear, Dagger), Mystics/Adepts use WILL (Staff, Focus); ensures loot generation can create class-appropriate drops.

**Acceptance Criteria**:

- [ ]  11 weapon categories defined: Axe, Greatsword, Spear, Dagger, Staff, Focus, Blade, Blunt, EnergyMelee, Rifle, HeavyBlunt
- [ ]  Each weapon has `WeaponAttribute` property ("MIGHT", "FINESSE", or "WILL")
- [ ]  Attack rolls use weapon's attribute: MIGHT weapons roll MIGHTd6 for attack
- [ ]  Weapon categories have distinct stat ranges:
    - Daggers: Low damage (1d6-1 to 2d6), low stamina cost (3-4)
    - Greatswords: High damage (1d6+2 to 4d6+4), high stamina cost (8-15)
    - Focuses: Zero melee damage, grant bonus dice to abilities
- [ ]  Loot service can filter weapons by category for class-appropriate generation

**Example Scenarios**:

1. **Scenario**: Warrior attacks with MIGHT weapon
    - **Input**: Warrior (MIGHT 7) equips Clan-Forged Axe (WeaponAttribute="MIGHT", 1d6+3 damage)
    - **Expected Output**: Attack roll uses 7d6 (base MIGHT) + accuracy bonuses
    - **Success Condition**: Weapon attribute correctly determines attack dice pool
2. **Scenario**: Mystic equips Focus (no melee damage)
    - **Input**: Scavenged Focus (0 DamageDice, +2 bonus dice to abilities)
    - **Expected Behavior**: Melee attack deals 0 damage; abilities gain +2 dice
    - **Success Condition**: Focus functions as ability amplifier, not melee weapon

**Dependencies**:

- Requires: Attribute System (`SPEC-PROGRESSION-001`)
- Requires: Damage Calculation (`SPEC-COMBAT-002`)
- Blocks: FR-004 (class-appropriate loot generation)

**Implementation Notes**:

- **Code Location**: `RuneAndRust.Core/Equipment.cs:28-43` (WeaponCategory enum)
- **Code Location**: `RuneAndRust.Core/Equipment.cs:78` (WeaponAttribute property)
- **Data Requirements**: Equipment.WeaponAttribute must match valid attribute names

---

### FR-003: Enemy-Based Loot Drop Tables

**Priority**: High
**Status**: Implemented

**Description**:
When an enemy is defeated, the LootService generates loot based on enemy type using tier-specific drop tables. Trash mobs (Servitor) drop low-tier equipment (60% Jury-Rigged, 30% Scavenged, 10% nothing), standard enemies (Drone) drop mid-tier (40% Scavenged, 40% Clan-Forged, 20% Optimized), and bosses (Warden) drop high-tier (30% Optimized, 70% Myth-Forged).

**Rationale**:
Enemy-specific drop tables ensure loot quality matches encounter difficulty; players rewarded proportionally to challenge; boss victories guarantee meaningful upgrades; prevents Tier 4 drops from trash mobs.

**Acceptance Criteria**:

- [ ]  `GenerateLoot(Enemy)` method returns Equipment or null based on drop table
- [ ]  Servitor drop table: 60% Tier 0, 30% Tier 1, 10% null
- [ ]  Drone drop table: 40% Tier 1, 40% Tier 2, 20% Tier 3
- [ ]  Boss (Warden) drop table: 30% Tier 3, 70% Tier 4
- [ ]  Drop tables use weighted RNG (Random.Next(100) with threshold checks)
- [ ]  Loot quality never exceeds enemy tier (trash mobs can't drop Myth-Forged)
- [ ]  Combat log displays loot: "[yellow]💎 Clan-Forged Axe dropped![/]"

**Example Scenarios**:

1. **Scenario**: Player defeats Servitor (trash mob)
    - **Input**: Enemy type = `EnemyType.CorruptedServitor`
    - **Expected Output**: 60% chance Tier 0, 30% chance Tier 1, 10% nothing
    - **Success Condition**: Loot quality matches trash mob tier; high chance of Jury-Rigged/Scavenged
2. **Scenario**: Player defeats Boss (Warden)
    - **Input**: Enemy type = `EnemyType.RuinWarden`
    - **Expected Output**: 30% Optimized, 70% Myth-Forged (always drops high-tier)
    - **Success Condition**: Boss guaranteed to drop Tier 3-4 equipment; reward feels impactful
3. **Edge Case**: No loot drops
    - **Condition**: Servitor RNG roll = 95 (in 10% null range)
    - **Expected Behavior**: `GenerateLoot()` returns null, no item added to ground
    - **Success Condition**: No crash; combat log shows no loot message

**Dependencies**:

- Requires: FR-001 (quality tier system)
- Requires: Combat Resolution for enemy defeat trigger (`SPEC-COMBAT-001`)
- Blocks: Player loot acquisition

**Implementation Notes**:

- **Code Location**: `RuneAndRust.Engine/LootService.cs:145-193` (enemy-specific methods)
- **Data Requirements**: Enemy.Type enum, EquipmentDatabase filtered by quality
- **Performance Considerations**: RNG generation + database lookup = <5ms per drop

---

### FR-004: Class-Appropriate Loot Generation

**Priority**: High
**Status**: Implemented

**Description**:
When generating loot, the system has a 60% chance to drop weapons appropriate for the player's character archetype (Warriors get Axes/Greatswords, Skirmishers get Spears/Daggers, Mystics/Adepts get Staves/Focuses). Boss loot is always class-appropriate. This ensures players receive useful equipment without forcing rigid class restrictions on all drops.

**Rationale**:
Class-appropriate loot reduces frustration of receiving unusable weapons (Warrior finding Staff); 60% chance balances targeted drops vs build experimentation; boss loot guarantees meaningful reward for archetype.

**Acceptance Criteria**:

- [ ]  `GenerateRandomItem(quality, player)` checks player.Class
- [ ]  60% chance to filter EquipmentDatabase for class-appropriate weapons
- [ ]  Archetype weapon mappings:
    - Warrior: Axe, Greatsword
    - Skirmisher: Spear, Dagger
    - Mystic/Adept: Staff, Focus
- [ ]  Boss loot (`GenerateClassAppropriateItem`) always filters by class (100% appropriate)
- [ ]  40% of non-boss drops can be off-class (enables build experimentation)
- [ ]  Armor drops are not class-restricted (all classes can use all armor)

**Example Scenarios**:

1. **Scenario**: Warrior defeats standard enemy
    - **Input**: Player class = Warrior, random weapon drop
    - **Expected Output**: 60% chance weapon is Axe or Greatsword, 40% chance any weapon
    - **Success Condition**: Higher probability of useful loot without eliminating variety
2. **Scenario**: Mystic defeats boss
    - **Input**: Boss defeated, player class = Mystic
    - **Expected Output**: Weapon drop is guaranteed to be Staff or Focus (Tier 3-4)
    - **Success Condition**: Boss loot always useful for player class; no wasted boss drops

**Dependencies**:

- Requires: FR-002 (weapon categories)
- Requires: Archetype System (`SPEC-PROGRESSION-002`)

**Implementation Notes**:

- **Code Location**: `RuneAndRust.Engine/LootService.cs:200-230, 236-259`
- **Code Location**: `RuneAndRust.Engine/EquipmentDatabase.cs:62-90` (GetRandomWeaponForClass)
- **Data Requirements**: PlayerCharacter.Class, EquipmentDatabase weapon filtering

---

### FR-005: Armor Category Trade-offs

**Priority**: High
**Status**: Implemented

**Description**:
Armor is categorized into three types (Light, Medium, Heavy) with distinct stat trade-offs. Light armor provides low HP/defense but grants +FINESSE bonuses and no penalties. Heavy armor provides high HP/defense and +STURDINESS bonuses but imposes -FINESSE penalties (Tier 2+ only). Medium armor balances stats with minimal trade-offs.

**Rationale**:
Armor categories create build diversity (glass cannon vs tank); trade-offs force meaningful choices; v0.18 balance ensures Heavy armor has drawbacks to prevent dominance; attribute penalties create risk/reward decisions.

**Acceptance Criteria**:

- [ ]  Three armor categories: Light, Medium, Heavy
- [ ]  Light Armor: +5 to +15 HP, +0 to +1 defense, +FINESSE bonuses, no penalties
- [ ]  Medium Armor: +10 to +25 HP, +1 to +2 defense, balanced attribute bonuses
- [ ]  Heavy Armor: +15 to +40 HP, +2 to +3 defense, +STURDINESS bonuses, -1 to -2 FINESSE penalty (Tier 2+)
- [ ]  Armor category displayed in equipment description
- [ ]  All classes can equip all armor (no class restrictions on armor)

**Example Scenarios**:

1. **Scenario**: Glass cannon Skirmisher chooses armor
    - **Choice A**: Shadow Weave (Light, Tier 4) - +15 HP, +2 FINESSE, +2 Evasion, no penalty
    - **Choice B**: Juggernaut Frame (Heavy, Tier 4) - +40 HP, +3 STURDINESS, -2 FINESSE
    - **Decision**: Light armor synergizes with FINESSE build (+2 attack/defense dice), Heavy penalizes
    - **Success Condition**: Trade-offs create clear build choice based on playstyle
2. **Scenario**: Tank Warrior evaluates Heavy armor penalty
    - **Input**: Clan-Forged Full Plate (+25 HP, +3 defense, +2 STURDINESS, -1 FINESSE)
    - **Impact**: Gain +25 HP and +2 STURDINESS (+2 defense dice), lose -1 FINESSE (-1 attack die)
    - **Success Condition**: Penalty noticeable but acceptable for defensive build; choice matters

**Dependencies**:

- Requires: Attribute System (`SPEC-PROGRESSION-001`)
- Requires: FR-001 (quality tier system determines when penalties apply)

**Implementation Notes**:

- **Code Location**: `RuneAndRust.Core/Equipment.cs:48-53` (ArmorCategory enum)
- **Code Location**: `RuneAndRust.Engine/EquipmentDatabase.cs:610-797` (Armor definitions)
- **Balance Note**: v0.18 added -FINESSE penalty to Tier 2+ Heavy armor for balance

---

### FR-006: Equipment Attribute Bonuses Beyond Cap

**Priority**: High
**Status**: Implemented

**Description**:
Equipment can grant attribute bonuses (+MIGHT, +FINESSE, +WITS, +WILL, +STURDINESS) that stack additively with base attributes and exceed the normal attribute cap of 6. A character with MIGHT 6 (cap) + Myth-Forged weapon (+3 MIGHT) effectively has MIGHT 9 for attack rolls and damage calculations.

**Rationale**:
Equipment bonuses bypass attribute cap to enable late-game power scaling; Myth-Forged items provide ~50% power increase beyond base cap; creates incentive to find high-tier equipment even at max attribute investment.

**Acceptance Criteria**:

- [ ]  Equipment has `List<EquipmentBonus>` with AttributeName and BonusValue
- [ ]  Bonuses stack additively: Weapon +2 MIGHT + Armor +1 MIGHT = +3 total MIGHT
- [ ]  Bonuses apply to all rolls using that attribute (attack rolls, skill checks)
- [ ]  Bonuses displayed in character sheet: "MIGHT: 6 (+3 from equipment) = 9"
- [ ]  Tier 2 equipment: +1 attribute typical
- [ ]  Tier 4 equipment: +2 to +4 attributes possible
- [ ]  No cap on equipment bonuses (can stack multiple +3 items theoretically)

**Example Scenarios**:

1. **Scenario**: Max-attribute character finds Myth-Forged weapon
    - **Input**: Warrior with MIGHT 6 (cap), finds Omega Maul (+3 MIGHT, +2 STURDINESS)
    - **Before**: MIGHT 6 → 6d6 attack rolls (avg 2.0 successes)
    - **After**: MIGHT 9 → 9d6 attack rolls (avg 3.0 successes)
    - **Impact**: +50% average successes = significantly higher hit rate and effectiveness
    - **Success Condition**: Equipment bonuses create meaningful power spike beyond attribute cap
2. **Scenario**: Stacking equipment bonuses
    - **Input**: Clan-Forged Axe (+1 MIGHT) + Clan-Forged Full Plate (+2 STURDINESS)
    - **Expected Output**: +1 MIGHT, +2 STURDINESS applied to character
    - **Success Condition**: Multiple equipment bonuses stack without conflict
3. **Scenario**: Tech equipment grants WITS bonuses
    - **Input**: Skirmisher finds Arc-Cannon (Myth-Forged Rifle, +4 WITS, +3 FINESSE)
    - **Before**: WITS 4 → 4d6 for tech/perception checks
    - **After**: WITS 8 → 8d6 for tech/perception checks, +3 FINESSE for attacks
    - **Impact**: Dramatically improves tech interactions, hacking, perception; tech weapons synergize with WITS
    - **Success Condition**: WITS bonuses apply to all tech checks and perception rolls

**Dependencies**:

- Requires: Attribute System with cap (`SPEC-PROGRESSION-001`)
- Requires: Combat systems consume attribute values (`SPEC-COMBAT-002`, `SPEC-COMBAT-004`)

**Implementation Notes**:

- **Code Location**: `RuneAndRust.Core/Equipment.cs:58-63, 90` (EquipmentBonus class, Bonuses list)
- **Data Requirements**: Equipment.Bonuses list with AttributeName strings matching attribute system
- **Balance**: Currently uncapped; may implement soft cap (+10 max) in future balance pass

---

### FR-007: Myth-Forged Special Effects

**Priority**: Medium
**Status**: Implemented

**Description**:
Tier 4 (Myth-Forged) equipment can have unique special effects beyond stat bonuses: Ignore Armor (damage bypasses defense), Lifesteal (heal % of damage dealt), Chain Lightning (hits multiple targets), Regeneration, AoE damage, and others. Special effects are hard-coded per item and provide transformative gameplay changes.

**Rationale**:
Special effects make Myth-Forged items feel legendary and unique; effects change tactics and build strategies; provides aspirational rewards for difficult content; differentiates endgame items from simple stat sticks.

**Acceptance Criteria**:

- [ ]  Equipment has `SpecialEffect` string property describing effect
- [ ]  Equipment has `IgnoresArmor` boolean flag for armor-piercing
- [ ]  Special effects exclusive to Tier 4 equipment (no Tier 0-3 items have effects)
- [ ]  Effects displayed in equipment tooltip and description
- [ ]  Common special effects:
    - Ignore Armor: Damage calculation skips defense bonus
    - Lifesteal: Heal 25% of damage dealt
    - Chain Lightning: Attack bounces to 2 additional targets
    - Regeneration: Heal X HP per turn
    - AoE: Damage all adjacent enemies
- [ ]  Some effects have drawbacks: Heretical Blade grants +10 Corruption on equip

**Example Scenarios**:

1. **Scenario**: Warden's Greatsword (Ignores Armor)
    - **Input**: Myth-Forged Greatsword with `IgnoresArmor = true`
    - **Expected Behavior**: Damage calculation skips enemy DefenseBonus entirely
    - **Impact**: Extremely effective against heavily armored enemies (tanks, bosses)
    - **Success Condition**: Special effect changes optimal target priority; tanks become viable targets
2. **Scenario**: Heretical Blade (Lifesteal with Corruption cost)
    - **Input**: Myth-Forged Blade with "Lifesteal 25%, +10 Corruption on equip"
    - **Expected Behavior**: Heal 25% of damage dealt; player gains +10 Corruption when equipped
    - **Trade-off**: Powerful sustain vs Corruption risk (Breaking Point at 100 Corruption)
    - **Success Condition**: Players must weigh lifesteal value against Corruption cost; risk/reward choice
3. **Scenario**: Arc-Cannon (Chain Lightning)
    - **Input**: "Chain lightning (bounces to 2 additional targets)"
    - **Expected Behavior**: Primary attack hits target, then 2 additional attacks auto-target nearby enemies
    - **Success Condition**: AoE damage enables multi-target encounters; changes combat tactics

**Dependencies**:

- Requires: FR-001 (Tier 4 quality requirement)
- Requires: Damage/Combat systems to implement effect logic (`SPEC-COMBAT-002`)
- Optional: Trauma Economy for Corruption effects (`SPEC-ECONOMY-003`)

**Implementation Notes**:

- **Code Location**: `RuneAndRust.Core/Equipment.cs:93-94` (IgnoresArmor, SpecialEffect properties)
- **Code Location**: `RuneAndRust.Engine/EquipmentDatabase.cs` - Myth-Forged items have SpecialEffect strings
- **Design Note**: Effects are descriptive text; combat systems must implement logic

---

## System Mechanics

> Completeness Checklist:
> 
> - [x]  All mechanics have clear inputs/outputs
> - [x]  All formulas are documented with examples
> - [x]  All parameters have ranges and defaults
> - [x]  All edge cases are documented
> - [x]  All mechanics link to functional requirements
> - [x]  All mechanics have example calculations

### Mechanic 1: Enemy-Based Loot Generation Algorithm

**Overview**:
When an enemy is defeated in combat, the LootService.GenerateLoot() method determines whether equipment drops and what quality tier it will be. The algorithm uses weighted random number generation based on enemy type, with trash mobs dropping low-tier loot and bosses guaranteeing high-tier rewards.

**How It Works**:

1. Combat system calls `LootService.GenerateLoot(Enemy enemy, PlayerCharacter player)` after enemy defeat
2. LootService routes to enemy-specific method based on `enemy.Type` (Servitor, Drone, Boss)
3. Enemy-specific method generates random number 0-99 using `Random.Next(100)`
4. Random number compared against tier thresholds to determine quality or null drop
5. If quality determined, call `GenerateRandomItem(quality, player)` to select specific equipment
6. Equipment added to room ground or returned to combat system

**Formula/Logic**:

```csharp
// Enemy Type → Loot Method Routing
Equipment? GenerateLoot(Enemy enemy, PlayerCharacter? player)
{
    return enemy.Type switch
    {
        EnemyType.CorruptedServitor => GenerateServitorLoot(player),  // Trash mob
        EnemyType.BlightDrone => GenerateDroneLoot(player),           // Standard enemy
        EnemyType.RuinWarden => GenerateBossLoot(player),             // Boss
        _ => null
    };
}

// Servitor Drop Table (Tier 0-1 enemy)
Equipment? GenerateServitorLoot(PlayerCharacter? player)
{
    int roll = Random.Next(100);  // 0-99

    if (roll < 10) return null;                    // 10% no drop
    else if (roll < 70) quality = QualityTier.JuryRigged;   // 60% Tier 0
    else quality = QualityTier.Scavenged;                   // 30% Tier 1

    return GenerateRandomItem(quality, player);
}

// Drone Drop Table (Tier 1-3 enemy)
Equipment? GenerateDroneLoot(PlayerCharacter? player)
{
    int roll = Random.Next(100);

    if (roll < 40) quality = QualityTier.Scavenged;      // 40% Tier 1
    else if (roll < 80) quality = QualityTier.ClanForged;  // 40% Tier 2
    else quality = QualityTier.Optimized;                 // 20% Tier 3

    return GenerateRandomItem(quality, player);
}

// Boss Drop Table (Tier 3-4 enemy)
Equipment? GenerateBossLoot(PlayerCharacter? player)
{
    int roll = Random.Next(100);

    if (roll < 30) quality = QualityTier.Optimized;     // 30% Tier 3
    else quality = QualityTier.MythForged;             // 70% Tier 4

    return GenerateClassAppropriateItem(quality, player);  // Boss always class-appropriate
}

Example:
  Enemy = CorruptedServitor (Tier 0 trash mob)
  Random roll = 45

  Check thresholds:
    45 < 10? No (not null drop)
    45 < 70? Yes → QualityTier.JuryRigged (Tier 0)

  Result: Tier 0 equipment will be generated

```

**Parameters**:

| Parameter | Type | Range | Default | Description | Tunable? |
| --- | --- | --- | --- | --- | --- |
| enemy.Type | EnemyType | Enum | - | Determines which loot table to use | No |
| Random seed | int | 0-99 | - | RNG for tier determination | No |
| Servitor null rate | % | 0-100 | 10 | Chance of no drop from trash mobs | Yes |
| Servitor Tier 0 rate | % | 0-100 | 60 | Chance of Jury-Rigged from Servitor | Yes |
| Boss Tier 4 rate | % | 0-100 | 70 | Chance of Myth-Forged from boss | Yes |

**Data Flow**:

```
Input Sources:
  → Enemy.Type (from defeated enemy in combat)
  → PlayerCharacter.Class (for class-appropriate filtering)
  → System.Random (for RNG)

Processing:
  → Route to enemy-specific loot table method
  → Generate random number 0-99
  → Compare against tier thresholds
  → Determine QualityTier or null
  → Call GenerateRandomItem() with quality + player

Output Destinations:
  → Room.ItemsOnGround (equipment added to room)
  → Combat log (loot notification displayed)
  → Player inventory (if auto-pickup enabled)

```

**Edge Cases**:

1. **No Loot Drop**: When RNG roll falls in null range (10% for Servitor)
    - **Condition**: `roll < 10` for Servitor
    - **Behavior**: Return null, no equipment generated
    - **Example**: Servitor defeated, roll = 5 → No loot message in combat log
2. **Boss Always Drops**: Bosses have 100% drop rate (no null outcome)
    - **Condition**: enemy.Type == RuinWarden
    - **Behavior**: Always returns equipment (30% Tier 3, 70% Tier 4)
    - **Example**: Boss defeated, roll = 95 → Myth-Forged weapon guaranteed
3. **Player is Null**: When player reference not provided
    - **Condition**: player == null in GenerateRandomItem()
    - **Behavior**: Skip class-appropriate filtering, generate fully random item
    - **Example**: Useful for pre-placed loot, environmental drops

**Related Requirements**: FR-003, FR-004

---

### Mechanic 2: Class-Appropriate Item Selection

**Overview**:
After quality tier is determined, the system selects a specific equipment item from the database. For standard enemies, there's a 60% chance to filter for class-appropriate weapons. For bosses, class filtering is always applied (100%). This ensures players receive useful loot while allowing build experimentation.

**How It Works**:

1. `GenerateRandomItem(QualityTier quality, PlayerCharacter? player)` receives quality tier
2. Roll 50/50 to decide weapon vs armor type
3. If weapon AND player exists, roll 60% chance for class-appropriate filtering
4. Query EquipmentDatabase for all items matching: quality tier + item type + (optional) class filter
5. Select random item from filtered results using `Random.Next(filteredList.Count)`
6. Return selected equipment

**Formula/Logic**:

```csharp
// Class-Appropriate Weapon Selection
Equipment? GenerateRandomItem(QualityTier quality, PlayerCharacter? player)
{
    bool isWeapon = Random.Next(2) == 0;  // 50/50 weapon or armor

    if (isWeapon && player != null)
    {
        // 60% chance for class-appropriate weapon
        if (Random.Next(100) < 60)
        {
            Equipment? weapon = GetRandomWeaponForClass(player.Class, quality);
            if (weapon != null) return weapon;
        }
    }

    // Fallback: fully random item of this quality
    var allItems = EquipmentDatabase.GetAllEquipment()
        .Where(e => e.Quality == quality)
        .ToList();

    var filtered = allItems.Where(e =>
        isWeapon ? e.Type == EquipmentType.Weapon : e.Type == EquipmentType.Armor
    ).ToList();

    return filtered[Random.Next(filtered.Count)];
}

// Class Weapon Mapping (from EquipmentDatabase)
Equipment? GetRandomWeaponForClass(CharacterClass characterClass, QualityTier quality)
{
    var weapons = characterClass switch
    {
        CharacterClass.Warrior => AllEquipment
            .Where(e => e.Type == Weapon &&
                   (e.WeaponCategory == Axe || e.WeaponCategory == Greatsword) &&
                   e.Quality == quality),
        CharacterClass.Scavenger => AllEquipment  // Legacy, maps to Skirmisher
            .Where(e => e.Type == Weapon &&
                   (e.WeaponCategory == Spear || e.WeaponCategory == Dagger) &&
                   e.Quality == quality),
        CharacterClass.Mystic => AllEquipment
            .Where(e => e.Type == Weapon &&
                   (e.WeaponCategory == Staff || e.WeaponCategory == Focus) &&
                   e.Quality == quality),
        _ => []
    };

    return weapons.Any() ? weapons[Random.Next(weapons.Count)] : null;
}

Example:
  Player = Warrior (MIGHT-based)
  Quality = QualityTier.ClanForged (Tier 2)

  Step 1: Roll weapon vs armor → Result: Weapon (50% chance)
  Step 2: Roll class-appropriate → Result: 45 < 60, YES
  Step 3: Filter for Warrior weapons at Tier 2:
    - Clan-Forged Axe (1d6+3, +1 MIGHT)
    - Clan-Forged Greatsword (1d6+5, +1 MIGHT)
  Step 4: Random selection → Clan-Forged Axe selected

  Result: Warrior receives useful MIGHT weapon

```

**Parameters**:

| Parameter | Type | Range | Default | Description | Tunable? |
| --- | --- | --- | --- | --- | --- |
| Class-appropriate % | int | 0-100 | 60 | Chance to filter by player class | Yes |
| Weapon vs Armor % | int | 0-100 | 50 | Base chance for weapon over armor | Yes |
| Boss class filter | bool | true/false | true | Whether bosses always filter by class | Yes |

**Edge Cases**:

1. **No Class-Appropriate Items Found**: When filtering returns empty list
    - **Condition**: GetRandomWeaponForClass() returns null
    - **Behavior**: Fallback to fully random item selection
    - **Example**: Tier 4 Mystic weapons exhausted → Any Tier 4 item selected
2. **Armor Always Classless**: Armor drops ignore class filtering
    - **Condition**: isWeapon == false
    - **Behavior**: Select random armor of quality tier, no class check
    - **Example**: Warrior can receive Light/Medium/Heavy armor equally

**Related Requirements**: FR-002, FR-004

---

### Mechanic 3: Equipment Stat Scaling by Quality Tier

**Overview**:
Equipment stats scale predictably across quality tiers to create clear power progression. Weapons gain increased damage dice and bonuses, while armor gains HP and defense. Attribute bonuses appear exclusively on Tier 2+ equipment, and special effects are restricted to Tier 4 (Myth-Forged) items.

**How It Works**:

1. Equipment designer assigns quality tier to each item in EquipmentDatabase
2. Stats scale according to tier-specific ranges:
    - **Tier 0 (Jury-Rigged)**: Baseline stats with penalties (-1 accuracy, -1 damage bonus)
    - **Tier 1 (Scavenged)**: Functional baseline (1d6+1 damage, +5 HP armor)
    - **Tier 2 (Clan-Forged)**: Enhanced stats + first attribute bonuses (+1 to primary attribute)
    - **Tier 3 (Optimized)**: Double dice or high bonuses (+2d6 damage, +1-2 accuracy)
    - **Tier 4 (Myth-Forged)**: Maximum stats + special effects (2d6+4, unique abilities)
3. Stat ranges enforced during equipment creation to maintain balance

**Formula/Logic**:

```
Weapon Damage Scaling (using Axe as example):
  Tier 0: 1d6 + 0  = 3.5 avg damage
  Tier 1: 1d6 + 1  = 4.5 avg damage  (+29% vs Tier 0)
  Tier 2: 1d6 + 3  = 6.5 avg damage  (+44% vs Tier 1)
  Tier 3: 2d6 + 0  = 7.0 avg damage  (+8% vs Tier 2)
  Tier 4: 2d6 + 4  = 11.0 avg damage (+57% vs Tier 3)

  Total Tier 0 → Tier 4: +214% damage increase (~3.1× multiplier)

Armor HP Scaling (Light armor example):
  Tier 0: +2 HP   (Tattered Leathers)
  Tier 1: +5 HP   (+150% vs Tier 0)
  Tier 2: +10 HP  (+100% vs Tier 1)
  Tier 3: +12 HP  (+20% vs Tier 2)
  Tier 4: +15 HP  (+25% vs Tier 3)

  Total Tier 0 → Tier 4: +650% HP increase (7.5× multiplier)

Attribute Bonus Scaling:
  Tier 0: None
  Tier 1: None
  Tier 2: +1 to primary attribute (Clan-Forged Axe: +1 MIGHT)
  Tier 3: +1 to +2 attributes (Optimized War Axe: +1 Accuracy)
  Tier 4: +2 to +4 attributes (Omega Maul: +3 MIGHT, +2 STURDINESS)

Accuracy Bonus Scaling (weapons only):
  Tier 0: -1 accuracy (penalty)
  Tier 1: 0 accuracy (neutral)
  Tier 2: 0 accuracy (neutral, stats in damage/attributes)
  Tier 3: +1 to +2 accuracy (Optimized weapons gain precision)
  Tier 4: 0 to +2 accuracy (varies by item design)

Defense Bonus Scaling (armor only):
  Light:  0 → 0 → +1 → +1 → +1
  Medium: +1 → +1 → +2 → +2 → +3
  Heavy:  +1 → +2 → +3 → +4 → +6

Example Weapon Progression (Greatsword):
  Tier 0: Bent Greatsword
    - Damage: 1d6+2 (avg 5.5)
    - Stamina: 8
    - Accuracy: -1
    - Bonuses: None
    - Total Power: ~4.5 (accounting for accuracy penalty)

  Tier 2: Clan-Forged Greatsword
    - Damage: 1d6+5 (avg 8.5)
    - Stamina: 8
    - Accuracy: 0
    - Bonuses: +1 MIGHT
    - Total Power: ~9.5 (damage + attribute die)

  Tier 4: Warden's Greatsword
    - Damage: 2d6+4 (avg 11.0)
    - Stamina: 10
    - Accuracy: 0
    - Bonuses: None
    - Special: Ignores 50% armor, grants [Fortified] on kill
    - Total Power: ~15+ (damage + armor ignore + sustain)

```

**Parameters**:

| Parameter | Type | Range | Default | Description | Tunable? |
| --- | --- | --- | --- | --- | --- |
| Damage dice | int | 1-4 | - | Number of d6s rolled for damage | Yes |
| Damage bonus | int | -2 to +6 | - | Flat bonus added to damage | Yes |
| Accuracy bonus | int | -1 to +2 | 0 | Bonus/penalty to attack rolls | Yes |
| HP bonus | int | 2-40 | - | Added to MaxHP when equipped | Yes |
| Defense bonus | int | 0-6 | - | Reduces enemy attack rolls | Yes |
| Attribute bonus value | int | 0-4 | 0 | Bonus to specific attribute | Yes |
| Special effect presence | bool | true/false | false | Whether item has special effect | No (Tier 4 only) |

**Stat Ranges by Tier** (enforced in EquipmentDatabase):

| Tier | Weapon Damage | Weapon Accuracy | Armor HP | Armor Defense | Attribute Bonus |
| --- | --- | --- | --- | --- | --- |
| 0 | 1d6 to 1d6+2 | -1 | +2 to +8 | +1 to +2 | None |
| 1 | 1d6 to 1d6+4 | 0 | +5 to +15 | +1 to +4 | None |
| 2 | 1d6+2 to 1d6+5 | 0 | +10 to +20 | +1 to +5 | +1 typical |
| 3 | 2d6 to 2d6+2 | +1 to +2 | +12 to +25 | +2 to +6 | +1 to +2 |
| 4 | 2d6+2 to 4d6+4 | 0 to +2 | +15 to +40 | +1 to +8 | +2 to +4 |

**Edge Cases**:

1. **Focus Weapons (No Melee Damage)**: Focuses violate damage scaling
    - **Condition**: WeaponCategory == Focus
    - **Behavior**: DamageDice = 0, grant bonus dice to abilities instead
    - **Example**: Scavenged Focus (Tier 1) grants +2 bonus dice to abilities, 0 melee damage
2. **Heavy Armor FINESSE Penalties**: Tier 2+ Heavy armor has negative attribute bonuses
    - **Condition**: ArmorCategory == Heavy && Quality >= Clan-Forged
    - **Behavior**: Apply -1 to -2 FINESSE penalty
    - **Example**: Clan-Forged Full Plate (+25 HP, +3 defense, +2 STURDINESS, -1 FINESSE)
3. **v0.18 Balance Adjustments**: Some Tier 2 items adjusted for balance
    - **Condition**: Historical balance pass
    - **Behavior**: Clan-Forged Greatsword reduced from +6 to +5 damage
    - **Example**: Maintains ~3× power curve without dominance

**Related Requirements**: FR-001, FR-005, FR-006

---

## Balance & Tuning

### Drop Rate Probability Matrix

**Enemy-Tier Quality Distribution**:

| Enemy Type | Tier 0 | Tier 1 | Tier 2 | Tier 3 | Tier 4 | No Drop |
| --- | --- | --- | --- | --- | --- | --- |
| **Servitor** (Trash) | 60% | 30% | 0% | 0% | 0% | 10% |
| **Drone** (Standard) | 0% | 40% | 40% | 20% | 0% | 0% |
| **Boss** (Warden) | 0% | 0% | 0% | 30% | 70% | 0% |

**Cumulative Tier Distribution** (assuming 10 Servitors, 5 Drones, 1 Boss per run):

| Quality Tier | Expected Drops per Run | % of Total Loot |
| --- | --- | --- |
| Jury-Rigged (0) | 6.0 items | 37.5% |
| Scavenged (1) | 5.0 items | 31.3% |
| Clan-Forged (2) | 2.0 items | 12.5% |
| Optimized (3) | 1.3 items | 8.1% |
| Myth-Forged (4) | 0.7 items | 4.4% |
| **Total** | **16.0 items** | **100%** |

**Loot Rarity Classification**:

- **Common**: Tier 0-1 (68.8% of drops) - Frequent upgrades, disposable
- **Uncommon**: Tier 2 (12.5% of drops) - Mid-game power spike
- **Rare**: Tier 3 (8.1% of drops) - Late-game competitive gear
- **Legendary**: Tier 4 (4.4% of drops) - Run-defining finds

**Class-Appropriate Filtering Impact**:

| Scenario | Base Drop Rate | Class Filter Applied | Effective Useful Loot Rate |
| --- | --- | --- | --- |
| Standard enemy weapon | 50% weapon chance | 60% class filter | 30% useful weapon |
| Standard enemy armor | 50% armor chance | N/A (no class filter) | 50% useful armor |
| Boss weapon | 50% weapon chance | 100% class filter | 50% useful weapon |
| Boss armor | 50% armor chance | N/A | 50% useful armor |

**Result**: Boss drops have **66% higher** useful weapon rate vs standard enemies (50% vs 30%)

---

### Equipment Power Curve

**Weapon Damage Power Scaling**:

```
Damage Output Comparison (Axe category across tiers):

Tier 0 (Rusty Hatchet):
  - Damage: 1d6 (3.5 avg) with -1 accuracy
  - Effective DPR: 3.5 × 0.27 success rate = 0.95 damage per roll
  - Power Rating: 1.0× (baseline)

Tier 1 (Scavenged Axe):
  - Damage: 1d6+1 (4.5 avg) with 0 accuracy
  - Effective DPR: 4.5 × 0.33 success rate = 1.5 damage per roll
  - Power Rating: 1.58× vs Tier 0

Tier 2 (Clan-Forged Axe):
  - Damage: 1d6+3 (6.5 avg) with 0 accuracy, +1 MIGHT
  - Effective DPR: 6.5 × 0.37 success rate (from +1 MIGHT) = 2.4 damage per roll
  - Power Rating: 2.53× vs Tier 0

Tier 3 (Optimized War Axe):
  - Damage: 2d6 (7.0 avg) with +1 accuracy
  - Effective DPR: 7.0 × 0.42 success rate = 2.9 damage per roll
  - Power Rating: 3.05× vs Tier 0

Tier 4 (Myth-Forged):
  - Damage: 2d6+4 (11.0 avg) with +2 MIGHT bonus
  - Effective DPR: 11.0 × 0.43 success rate = 4.7 damage per roll
  - Power Rating: 4.95× vs Tier 0

Power Curve: Approximately 5× damage increase from Tier 0 → Tier 4

```

**Armor Survivability Scaling**:

```
Effective HP Comparison (Medium armor across tiers):

Tier 0 (Scrap Plating):
  - HP Bonus: +5 HP
  - Defense: +2 (reduces enemy hits by ~10%)
  - Effective HP: 55 base + 5 bonus = 60 HP × 1.1 mitigation = 66 EHP
  - Survivability Rating: 1.0× (baseline)

Tier 1 (Scavenged Chainmail):
  - HP Bonus: +10 HP
  - Defense: +3
  - Effective HP: 60 base + 10 bonus = 70 HP × 1.15 mitigation = 80.5 EHP
  - Survivability Rating: 1.22× vs Tier 0

Tier 2 (Clan-Forged Plate):
  - HP Bonus: +15 HP
  - Defense: +4, +1 STURDINESS
  - Effective HP: 65 base + 15 bonus = 80 HP × 1.2 mitigation = 96 EHP
  - Survivability Rating: 1.45× vs Tier 0

Tier 4 (Warden's Aegis):
  - HP Bonus: +25 HP
  - Defense: +5, +2 STURDINESS, Immune to [Bleeding]
  - Effective HP: 75 base + 25 bonus = 100 HP × 1.35 mitigation = 135 EHP
  - Survivability Rating: 2.05× vs Tier 0

Armor Power Curve: Approximately 2× survivability increase from Tier 0 → Tier 4

```

**Weapon vs Armor Value Proposition**:

| Upgrade | Damage Increase | Survivability Increase | ROI Priority |
| --- | --- | --- | --- |
| Tier 0 → Tier 2 Weapon | +153% damage | N/A | **High** - Offense wins fights |
| Tier 0 → Tier 2 Armor | N/A | +45% survivability | Medium - Defense buys time |
| Tier 2 → Tier 4 Weapon | +96% damage | N/A | **High** - Special effects transform gameplay |
| Tier 2 → Tier 4 Armor | N/A | +41% survivability | Medium - Diminishing returns |

**Conclusion**: Weapon upgrades provide **2-3× higher value** than equivalent armor upgrades due to offensive scaling and special effects

---

### Tunable Parameters

**Drop Rate Adjustments** (modify in LootService.cs):

| Parameter | Current Value | Range | Impact if Increased | Impact if Decreased |
| --- | --- | --- | --- | --- |
| Servitor null rate | 10% | 0-50% | Fewer trash drops, less clutter | More frequent low-tier loot |
| Servitor Tier 0 rate | 60% | 30-80% | More Jury-Rigged items | More Scavenged items (faster progression) |
| Drone Tier 3 rate | 20% | 10-40% | Earlier access to Optimized gear | More balanced mid-game progression |
| Boss Tier 4 rate | 70% | 50-90% | More Myth-Forged guarantees | More Tier 3 variety, less power spike |
| Class-appropriate % | 60% | 40-80% | More class-specific drops | More build experimentation options |

**Balance Recommendations**:

- **Default values tested for 15-20 min runs** - Progression feels satisfying without overwhelming player with loot
- **Myth-Forged rarity (<5% overall)** - Maintains excitement when found; not expected every run
- **Boss Tier 4 guarantee (70%)** - Ensures boss victories feel rewarding; 30% Tier 3 adds variety

**Stat Scaling Adjustments** (modify in EquipmentDatabase.cs):

| Parameter | Current Range | Tuning Notes |
| --- | --- | --- |
| Tier 4 damage multiplier | 3-5× vs Tier 0 | Too high = trivializes content; too low = no excitement |
| Attribute bonus cap | +4 max | Currently uncapped; may implement soft cap (+10 total) |
| Heavy armor penalty | -1 to -2 FINESSE | v0.18 balance ensures trade-offs matter |
| Special effect power | Varies per item | Ignores Armor, Lifesteal tested as strong but not broken |

---

## Appendices

### Appendix A: Complete Weapon Stat Tables

This appendix provides comprehensive stat references for all weapon categories across all quality tiers.

### A.1 Axes (MIGHT-based)

**Design**: Balanced melee weapons for Warriors; linear damage scaling with solid baseline stats.

| Quality Tier | Name | Damage | Stamina Cost | Accuracy | Bonuses | Special Effect |
| --- | --- | --- | --- | --- | --- | --- |
| 0 (Jury-Rigged) | Rusty Hatchet | 1d6 | 5 | +0 | None | None |
| 1 (Scavenged) | Scavenged Axe | 1d6+1 | 5 | +0 | None | None |
| 2 (Clan-Forged) | Clan-Forged Axe | 1d6+3 | 5 | +0 | None | None |
| 3 (Optimized) | Rune-Etched Axe | 2d6 | 5 | +0 | +1 MIGHT | None |
| 4 (Myth-Forged) | Dvergr Maul | 2d6+4 | 5 | +1 | +2 MIGHT | None |

**Power Progression**:

- **Tier 0→1**: +1 flat damage (+28% damage)
- **Tier 1→2**: +2 flat damage (+44% damage)
- **Tier 2→3**: Dice increase to 2d6, +1 MIGHT bonus (+8% effective power)
- **Tier 3→4**: +4 flat damage, +1 MIGHT, +1 accuracy (+57% damage)
- **Overall**: 3.5 avg → 11 avg = **3.14× damage increase**

---

### A.2 Greatswords (MIGHT-based, Two-Handed)

**Design**: Highest raw damage weapons; trade-off is no shield/dual-wield options.

| Quality Tier | Name | Damage | Stamina Cost | Accuracy | Bonuses | Special Effect |
| --- | --- | --- | --- | --- | --- | --- |
| 0 (Jury-Rigged) | Rusted Blade | 1d6+2 | 6 | +0 | None | None |
| 1 (Scavenged) | Scavenged Greatsword | 1d6+3 | 6 | +0 | None | None |
| 2 (Clan-Forged) | Clan-Forged Greatsword | 1d6+5 | 6 | +0 | None | None |
| 3 (Optimized) | Rune-Etched Claymore | 2d6+2 | 6 | +0 | +1 MIGHT | None |
| 4 (Myth-Forged) | Omega Maul | 4d6+4 | 8 | +0 | +3 MIGHT | Stun on crit |

**Power Progression**:

- **Tier 0→4**: 5.5 avg → 18 avg = **3.27× damage increase**
- **Note**: Highest absolute damage but also highest stamina cost (8 per attack at Tier 4)
- **v0.18 Balance**: Tier 2 reduced from 1d6+6 to 1d6+5 to prevent early-game dominance

---

### A.3 Spears (FINESSE-based)

**Design**: Reach weapons for Skirmishers; balanced stats similar to Axes but scale with FINESSE.

| Quality Tier | Name | Damage | Stamina Cost | Accuracy | Bonuses | Special Effect |
| --- | --- | --- | --- | --- | --- | --- |
| 0 (Jury-Rigged) | Makeshift Spear | 1d6 | 5 | +0 | None | None |
| 1 (Scavenged) | Scavenged Spear | 1d6+1 | 5 | +0 | None | None |
| 2 (Clan-Forged) | Hunter's Spear | 1d6+3 | 5 | +0 | None | None |
| 3 (Optimized) | Precision Pike | 2d6 | 5 | +1 | +1 FINESSE | None |
| 4 (Myth-Forged) | Jötun-Tech Plasma Cutter | 2d6 | 5 | +2 | +2 FINESSE | Ignores armor |

**Power Progression**:

- **Tier 0→4**: 3.5 avg → 7 avg = **2.0× base damage increase**
- **Special Effect (Tier 4)**: "Ignores armor" bypasses enemy Defense bonuses, making effective damage much higher against armored foes
- **Accuracy Scaling**: Tier 3-4 gain +1/+2 accuracy, synergizing with FINESSE for hit-focused builds

---

### A.4 Daggers (FINESSE-based, Fast)

**Design**: Low damage, high speed; improved from v0.17 to reduce starter penalty.

| Quality Tier | Name | Damage | Stamina Cost | Accuracy | Bonuses | Special Effect |
| --- | --- | --- | --- | --- | --- | --- |
| 0 (Jury-Rigged) | Sharpened Scrap | 1d6-1 | 4 | +0 | None | None |
| 1 (Scavenged) | Scavenged Dagger | 1d6 | 4 | +0 | None | None |
| 2 (Clan-Forged) | Clan-Forged Stiletto | 1d6+2 | 4 | +1 | None | None |
| 3 (Optimized) | Rune-Etched Dagger | 2d6 | 4 | +1 | +1 FINESSE | None |
| 4 (Myth-Forged) | Shadow's Fang | 2d6 | 4 | +2 | +3 FINESSE | Crit on 4+ |

**Power Progression**:

- **Tier 0→4**: 2.5 avg → 7 avg = **2.8× damage increase**
- **Stamina Efficiency**: Lowest stamina cost (4) allows more attacks per encounter
- **v0.18 Balance**: Tier 0 improved from 1d6-2 to 1d6-1 for better new player experience

---

### A.5 Staves (WILL-based)

**Design**: Aether-channeling weapons for Mystics/Adepts; balanced melee option with WILL scaling.

| Quality Tier | Name | Damage | Stamina Cost | Accuracy | Bonuses | Special Effect |
| --- | --- | --- | --- | --- | --- | --- |
| 0 (Jury-Rigged) | Crude Staff | 1d6-1 | 5 | +0 | None | None |
| 1 (Scavenged) | Scavenged Staff | 1d6 | 5 | +0 | None | None |
| 2 (Clan-Forged) | Iron-Bound Staff | 1d6+1 | 5 | +0 | +1 WILL | None |
| 3 (Optimized) | Rune-Etched Staff | 2d6 | 5 | +0 | +2 WILL | None |
| 4 (Myth-Forged) | Architect's Will | 3d6+3 | 6 | +1 | +3 WILL | +10 max Aether |

**Power Progression**:

- **Tier 0→4**: 2.5 avg → 13.5 avg = **5.4× damage increase** (highest weapon scaling)
- **v0.18 Balance**: Tier 0 improved from 1d6-2 to 1d6-1
- **Special Effect (Tier 4)**: +10 max Aether supports casting-heavy builds

---

### A.6 Focuses (WILL-based, No Melee Damage)

**Design**: Pure attribute-boosting items; no direct damage output.

| Quality Tier | Name | Damage | Stamina Cost | Accuracy | Bonuses | Special Effect |
| --- | --- | --- | --- | --- | --- | --- |
| 0 (Jury-Rigged) | N/A | 0d6 | N/A | N/A | N/A | N/A |
| 1 (Scavenged) | Scavenged Focus | 0d6 | N/A | N/A | +1 WILL | None |
| 2 (Clan-Forged) | Clan-Forged Amulet | 0d6 | N/A | N/A | +2 WILL | None |
| 3 (Optimized) | Rune-Etched Talisman | 0d6 | N/A | N/A | +3 WILL | None |
| 4 (Myth-Forged) | Aether Nexus | 0d6 | N/A | N/A | +4 WILL | Regen 1 Aether/turn |

**Usage Notes**:

- **No Tier 0**: Jury-Rigged focuses don't exist (requires minimal craftsmanship)
- **No melee attacks**: Cannot use Attack action with focus equipped
- **Pure support**: Entire value is in WILL bonuses for Aether pool and casting effectiveness
- **Tier 4 Special**: Aether regeneration (1/turn) extends casting capacity in long encounters

---

### A.7 Advanced Weapon Categories (v0.16+ Content)

**Blade** (FINESSE-based, Balanced):

| Quality Tier | Name | Damage | Stamina Cost | Accuracy | Bonuses | Special Effect |
| --- | --- | --- | --- | --- | --- | --- |
| 2 (Clan-Forged) | Clan-Forged Blade | 1d6+2 | 5 | +0 | None | None |
| 3 (Optimized) | Rune-Etched Sword | 2d6 | 5 | +1 | +1 FINESSE | None |
| 4 (Myth-Forged) | Void-Edge Katana | 2d6+2 | 5 | +2 | +2 FINESSE, +1 WILL | Phasing strikes |

**Blunt** (MIGHT-based, Stun-focused):

| Quality Tier | Name | Damage | Stamina Cost | Accuracy | Bonuses | Special Effect |
| --- | --- | --- | --- | --- | --- | --- |
| 2 (Clan-Forged) | Clan-Forged Mace | 1d6+2 | 5 | +0 | None | None |
| 3 (Optimized) | Rune-Etched Hammer | 2d6 | 5 | +0 | +1 MIGHT | Stun chance |
| 4 (Myth-Forged) | Titan's Fist | 3d6+2 | 6 | +0 | +3 MIGHT | Guaranteed stun |

**EnergyMelee** (WITS-based, Tech weapons):

| Quality Tier | Name | Damage | Stamina Cost | Accuracy | Bonuses | Special Effect |
| --- | --- | --- | --- | --- | --- | --- |
| 3 (Optimized) | Plasma Blade | 2d6 | 5 | +1 | +2 WITS | Burn damage |
| 4 (Myth-Forged) | Arc-Cannon | 3d6+1 | 6 | +2 | +4 WITS, +3 FINESSE | Chain lightning |

**Rifle** (FINESSE-based, Ranged):

| Quality Tier | Name | Damage | Stamina Cost | Accuracy | Bonuses | Special Effect |
| --- | --- | --- | --- | --- | --- | --- |
| 3 (Optimized) | Rune-Locked Rifle | 2d6+1 | 6 | +2 | +1 FINESSE | Piercing |
| 4 (Myth-Forged) | Architect's Wrath | 3d6+2 | 7 | +3 | +3 FINESSE, +2 WITS | Auto-targeting |

**HeavyBlunt** (MIGHT-based, Highest damage/stamina):

| Quality Tier | Name | Damage | Stamina Cost | Accuracy | Bonuses | Special Effect |
| --- | --- | --- | --- | --- | --- | --- |
| 4 (Myth-Forged) | World-Breaker | 4d6+6 | 10 | -1 | +4 MIGHT | AOE smash |

**Usage Notes**:

- **Advanced categories** primarily appear in Tier 2+ (no Tier 0-1 variants)
- **WITS scaling** exclusive to tech weapons (EnergyMelee, Rifle, Arc-Cannon)
- **World-Breaker** highest single-target damage but -1 accuracy and massive 10 stamina cost

---

## Appendix B: Complete Armor Stat Tables

### B.1 Light Armor

**Design**: Low HP/defense, high mobility; common +FINESSE bonuses for glass cannon builds.

| Quality Tier | Name | HP Bonus | Defense | Category | Bonuses | Special Effect |
| --- | --- | --- | --- | --- | --- | --- |
| 0 (Jury-Rigged) | Tattered Leathers | +5 | +0 | Light | None | None |
| 1 (Scavenged) | Scavenged Leathers | +10 | +0 | Light | None | None |
| 2 (Clan-Forged) | Reinforced Leathers | +15 | +1 | Light | +1 FINESSE | None |
| 3 (Optimized) | Rune-Woven Cloak | +15 | +1 | Light | +2 FINESSE | None |
| 4 (Myth-Forged) | Aether-Woven Shroud | +15 | +1 | Light | +2 WILL, +2 FINESSE | Stealth bonus |

**Design Philosophy**:

- **Low HP scaling**: 5 → 15 HP (capped at Tier 2+)
- **Defense bonus**: 0-1 (never exceeds +1)
- **Attribute focus**: FINESSE primary, WILL secondary for Mystic/Adept synergy
- **Best for**: Glass cannon Skirmishers, Mystics who rely on evasion/positioning

---

### B.2 Medium Armor

**Design**: Balanced HP/defense for all classes; no penalties, versatile stat bonuses.

| Quality Tier | Name | HP Bonus | Defense | Category | Bonuses | Special Effect |
| --- | --- | --- | --- | --- | --- | --- |
| 0 (Jury-Rigged) | Scrap Plating | +10 | +1 | Medium | None | None |
| 1 (Scavenged) | Scavenged Mail | +15 | +1 | Medium | None | None |
| 2 (Clan-Forged) | Chain Hauberk | +20 | +2 | Medium | None | None |
| 3 (Optimized) | Rune-Etched Mail | +20 | +2 | Medium | +1 STURDINESS | None |
| 4 (Myth-Forged) | Architect's Vestments | +25 | +2 | Medium | +2 STURDINESS, +1 WILL | Damage reduction |

**Design Philosophy**:

- **Balanced scaling**: 10 → 25 HP, +1 → +2 defense
- **No penalties**: Unlike heavy armor, no FINESSE reduction
- **Versatile bonuses**: STURDINESS + WILL support hybrid builds
- **Best for**: All classes; default choice when unsure

---

### B.3 Heavy Armor

**Design**: Maximum HP/defense; FINESSE penalties create meaningful trade-offs (v0.18 balance).

| Quality Tier | Name | HP Bonus | Defense | Category | Bonuses | Special Effect |
| --- | --- | --- | --- | --- | --- | --- |
| 0 (Jury-Rigged) | Makeshift Plating | +15 | +2 | Heavy | None | None |
| 1 (Scavenged) | Scavenged Plate | +20 | +2 | Heavy | None | None |
| 2 (Clan-Forged) | Clan-Forged Full Plate | +25 | +3 | Heavy | -1 FINESSE | None |
| 3 (Optimized) | Rune-Locked Plate | +25 | +3 | Heavy | +1 STURDINESS, -1 FINESSE | None |
| 4 (Myth-Forged) | Sentinel's Aegis | +30 | +3 | Heavy | +2 STURDINESS, -2 FINESSE | Taunt enemies |

**Design Philosophy**:

- **Highest survivability**: 15 → 30 HP, +2 → +3 defense
- **FINESSE penalties**: Tier 2+ apply -1 to -2 FINESSE (v0.18 balance change)
- **STURDINESS bonuses**: Tier 3+ offset penalties with +1/+2 STURDINESS
- **Best for**: Warrior tanks who don't rely on FINESSE for damage/accuracy

**v0.18 Balance Impact**:

- **Before**: No penalty, heavy armor was strictly superior to medium
- **After**: -1 FINESSE creates choice - is +5 HP and +1 defense worth losing 1d6 on FINESSE checks?
- **Example**: Skirmisher with FINESSE 5 in Tier 2 Clan-Forged Full Plate has effective FINESSE 4 (rolls 4d6 instead of 5d6 for initiative/evasion)

---

### Appendix C: Tier Progression Examples

This appendix provides side-by-side comparisons showing how equipment upgrades impact character power.

### C.1 Warrior Progression (Axe + Heavy Armor)

**Scenario**: Level 5 Warrior with MIGHT 5, STURDINESS 4, base HP 50.

| Tier | Weapon | Armor | Total HP | Avg Damage | Effective MIGHT | Defense | Power Rating |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 0 | Rusty Hatchet (1d6) | Makeshift Plating (+15 HP, +2 def) | 65 | 3.5 | 5 | +2 | 100 (baseline) |
| 1 | Scavenged Axe (1d6+1) | Scavenged Plate (+20 HP, +2 def) | 70 | 4.5 | 5 | +2 | 129 |
| 2 | Clan-Forged Axe (1d6+3) | Clan-Forged Plate (+25 HP, +3 def) | 75 | 6.5 | 5 | +3 | 186 |
| 3 | Rune-Etched Axe (2d6, +1 MIGHT) | Rune-Locked Plate (+25 HP, +3 def, +1 STURDINESS) | 75 | 7.0 | 6 | +3 | 200 |
| 4 | Dvergr Maul (2d6+4, +2 MIGHT) | Sentinel's Aegis (+30 HP, +3 def, +2 STURDINESS) | 80 | 11.0 | 7 | +3 | 314 |

**Key Insights**:

- **Tier 0→2**: 86% power increase (from equipment alone)
- **Tier 2→4**: 69% additional increase
- **Attribute bonuses**: Effective MIGHT 5 → 7 means 7d6 attack rolls vs base 5d6 (+40% success rate)
- **Survivability**: 65 HP → 80 HP (+23%), +2 → +3 defense

**Damage Calculation Example (Tier 4)**:

```
Attack Roll: 7d6 MIGHT (5 base + 2 from Dvergr Maul)
Expected Successes: 7 × 0.33 ≈ 2.3 successes
Damage: 2d6+4 = 11 avg damage
Total Expected Damage: 11 × (hit rate ~80%) ≈ 8.8 DPR

```

---

### C.2 Skirmisher Progression (Spear + Light Armor)

**Scenario**: Level 5 Skirmisher with FINESSE 6, base HP 45.

| Tier | Weapon | Armor | Total HP | Avg Damage | Effective FINESSE | Defense | Power Rating |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 0 | Makeshift Spear (1d6) | Tattered Leathers (+5 HP, +0 def) | 50 | 3.5 | 6 | +0 | 100 (baseline) |
| 1 | Scavenged Spear (1d6+1) | Scavenged Leathers (+10 HP, +0 def) | 55 | 4.5 | 6 | +0 | 129 |
| 2 | Hunter's Spear (1d6+3) | Reinforced Leathers (+15 HP, +1 def, +1 FINESSE) | 60 | 6.5 | 7 | +1 | 186 |
| 3 | Precision Pike (2d6, +1 FINESSE, +1 acc) | Rune-Woven Cloak (+15 HP, +1 def, +2 FINESSE) | 60 | 7.0 | 9 | +1 | 233 |
| 4 | Plasma Cutter (2d6, +2 FINESSE, +2 acc) | Aether-Woven Shroud (+15 HP, +1 def, +2 WILL, +2 FINESSE) | 60 | 7.0 | 10 | +1 | 300 |

**Key Insights**:

- **HP stagnation**: Light armor caps at +15 HP from Tier 2 onward
- **FINESSE stacking**: Effective FINESSE 6 → 10 (+67% attribute power)
- **Accuracy bonuses**: Tier 3-4 gain +1/+2 accuracy dice, increasing hit rate significantly
- **Special effect (Tier 4)**: "Ignores armor" makes damage much higher against armored enemies

**Trade-off Analysis**:

- **vs Heavy Armor Warrior**: -20 HP, -2 defense, but +3 FINESSE (initiative advantage, better evasion)
- **Glass cannon viability**: High evasion (10d6 FINESSE checks) compensates for low HP
- **Initiative dominance**: 10d6 FINESSE ≈ 3.3 successes (acts first ~90% of the time vs 5 MIGHT enemies)

---

### C.3 Mystic Progression (Staff + Medium Armor)

**Scenario**: Level 5 Mystic with WILL 6, base HP 40, base Aether 30.

| Tier | Weapon | Armor | Total HP | Avg Damage | Effective WILL | Defense | Max Aether |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 0 | Crude Staff (1d6-1) | Scrap Plating (+10 HP, +1 def) | 50 | 2.5 | 6 | +1 | 30 |
| 1 | Scavenged Staff (1d6) | Scavenged Mail (+15 HP, +1 def) | 55 | 3.5 | 6 | +1 | 30 |
| 2 | Iron-Bound Staff (1d6+1, +1 WILL) | Chain Hauberk (+20 HP, +2 def) | 60 | 4.5 | 7 | +2 | 35 |
| 3 | Rune-Etched Staff (2d6, +2 WILL) | Rune-Etched Mail (+20 HP, +2 def, +1 STURDINESS) | 60 | 7.0 | 8 | +2 | 40 |
| 4 | Architect's Will (3d6+3, +3 WILL) | Architect's Vestments (+25 HP, +2 def, +2 STURDINESS, +1 WILL) | 65 | 13.5 | 10 | +2 | 50 (+10 from staff) |

**Key Insights**:

- **Highest damage scaling**: 2.5 → 13.5 avg damage (5.4× increase, best of all weapon types)
- **WILL stacking**: 6 → 10 effective WILL (+67% for all Aether-based abilities)
- **Aether pool growth**: 30 → 50 max Aether (+67% casting capacity)
- **Balanced survivability**: Medium armor provides 65 HP, +2 defense (middle ground)

**Mystic Build Comparison**:

- **Option A (Staff + Medium Armor)**: Balanced melee/casting, 65 HP, 10 WILL
- **Option B (Focus + Light Armor)**: Pure casting, 55 HP, 12 WILL (+4 from Aether Nexus), +1 Aether/turn regen
- **Trade-off**: Option A has melee fallback (13.5 avg damage), Option B has 20% more WILL for casting

---

### Appendix D: Loot Drop Simulation

This appendix provides example runs showing expected loot distribution and probabilities.

### D.1 Standard 15-Minute Run (10 Servitors, 5 Drones, 1 Boss)

**Enemy Breakdown**:

- 10× Corrupted Servitors (60% T0, 30% T1, 10% null)
- 5× Blight Drones (40% T1, 40% T2, 20% T3)
- 1× Ruin Warden (30% T3, 70% T4)

**Expected Loot Totals**:

| Quality Tier | Expected Drops | Percentage |
| --- | --- | --- |
| Tier 0 | 6.0 items | 37.5% |
| Tier 1 | 5.0 items | 31.3% |
| Tier 2 | 2.0 items | 12.5% |
| Tier 3 | 1.3 items | 8.1% |
| Tier 4 | 0.7 items | 4.4% |
| **No drop** | 1.0 encounters | 6.3% |
| **Total** | **16 items** | **100%** |

**Loot Quality Distribution**:

- **Common (T0-T1)**: 11 items (68.8%)
- **Uncommon (T2)**: 2 items (12.5%)
- **Rare (T3)**: 1.3 items (8.1%)
- **Legendary (T4)**: 0.7 items (4.4%)

**Player Progression Example**:

1. **Encounters 1-3 (Servitors)**: Acquire 1-2 Tier 0 items, replace starter gear
2. **Encounters 4-7 (Mix Servitors/Drones)**: Find Tier 1-2 upgrades
3. **Encounters 8-10 (Drones)**: 50% chance of Tier 3 item
4. **Boss encounter**: 70% chance of Tier 4 legendary

**Best-Case Run** (90th percentile RNG):

- 2× Tier 4 items (boss + lucky Drone drop)
- 3× Tier 3 items
- 4× Tier 2 items
- 7× Tier 0-1 items (vendor trash)

**Worst-Case Run** (10th percentile RNG):

- 0× Tier 4 items (boss drops Tier 3)
- 1× Tier 3 item
- 1× Tier 2 item
- 14× Tier 0-1 items

**Class-Appropriate Filtering Impact**:

- **Without filtering**: Player sees 16 random items (may get 8 weapons for wrong archetype)
- **With 60% filtering**: ~60% of Servitor/Drone drops match player class
- **Boss drops**: 100% class-appropriate (guaranteed usable Tier 4)

---

### D.2 Probability of Finding Specific Items

**Question**: What's the probability of finding a Tier 4 weapon for my class in a single run?

**Calculation**:

```
Boss drop rate (T4): 70%
Class-appropriate filter (boss): 100%
Weapon vs Armor (50/50): 50%

P(Tier 4 class weapon from boss) = 0.70 × 1.00 × 0.50 = 35%

```

**Additional chances from Drones**:

```
5 Drones × 0% T4 rate = 0% (Drones cannot drop T4)

```

**Conclusion**: 35% chance per run to find a Tier 4 weapon for your class (boss only).

---

**Question**: What's the probability of upgrading from Tier 1 to Tier 2+ in encounters 1-10?

**Calculation** (assuming player has Tier 1 weapon):

```
Servitors (10 encounters): 0% T2 rate
Drones (5 encounters): 40% T2 rate per Drone

P(at least 1 T2+ drop from Drones) = 1 - P(no T2+ drops)
= 1 - (0.60)^5
= 1 - 0.078
= 92.2%

```

**Conclusion**: 92% chance to find at least one Tier 2+ item before the boss fight.

---

### Appendix E: Build-Specific Best-in-Slot (BiS) Equipment

This appendix provides optimal equipment recommendations for each archetype build.

### E.1 Warrior Builds

**Tank Warrior** (Maximize survivability):

- **Weapon**: Dvergr Maul (2d6+4, +2 MIGHT) - high damage, MIGHT bonus
- **Armor**: Sentinel's Aegis (+30 HP, +3 defense, +2 STURDINESS, -2 FINESSE)
- **Rationale**: Total +4 MIGHT/STURDINESS, 80 HP, +3 defense; FINESSE penalty irrelevant for MIGHT-based class
- **Effective Stats**: 9 MIGHT (7 base cap + 2 bonus), 6 STURDINESS (4 base + 2 bonus), 80 HP

**DPS Warrior** (Maximize damage output):

- **Weapon**: Omega Maul (4d6+4, +3 MIGHT) - highest raw damage
- **Armor**: Rune-Etched Mail (+20 HP, +2 defense, +1 STURDINESS) - no FINESSE penalty
- **Rationale**: Avoid heavy armor penalty to maintain initiative; Omega Maul has 18 avg damage
- **Effective Stats**: 9 MIGHT (6 base + 3 bonus), 5 STURDINESS, 70 HP
- **Trade-off**: -10 HP and -1 defense vs Tank build, but +7 avg damage and no initiative penalty

---

### E.2 Skirmisher Builds

**Evasion Skirmisher** (Maximize FINESSE for initiative/evasion):

- **Weapon**: Jötun-Tech Plasma Cutter (2d6, +2 FINESSE, +2 accuracy, ignores armor)
- **Armor**: Aether-Woven Shroud (+15 HP, +1 defense, +2 WILL, +2 FINESSE)
- **Rationale**: Total +4 FINESSE, +2 accuracy, stealth bonus; "ignores armor" negates low damage
- **Effective Stats**: 10 FINESSE (6 base + 4 bonus), 8 WILL, 60 HP

**Hybrid Skirmisher** (Balance offense/defense):

- **Weapon**: Void-Edge Katana (2d6+2, +2 FINESSE, +1 WILL, +2 accuracy)
- **Armor**: Rune-Etched Mail (+20 HP, +2 defense, +1 STURDINESS)
- **Rationale**: Medium armor adds +5 HP and +1 defense; Katana has solid 9 avg damage
- **Effective Stats**: 8 FINESSE, 7 WILL, 65 HP
- **Trade-off**: +5 HP but -2 FINESSE vs Evasion build

---

### E.3 Mystic Builds

**Casting Mystic** (Maximize WILL and Aether pool):

- **Weapon**: Aether Nexus (0d6, +4 WILL, +1 Aether/turn regen)
- **Armor**: Architect's Vestments (+25 HP, +2 defense, +2 STURDINESS, +1 WILL)
- **Rationale**: Total +7 WILL (highest possible), +1 Aether/turn sustain for long encounters
- **Effective Stats**: 13 WILL (6 base + 7 bonus), 65 HP, Aether regen
- **No melee damage**: Cannot use Attack action; 100% casting reliance

**Battle Mystic** (Hybrid melee/casting):

- **Weapon**: Architect's Will (3d6+3, +3 WILL, +10 max Aether)
- **Armor**: Aether-Woven Shroud (+15 HP, +1 defense, +2 WILL, +2 FINESSE)
- **Rationale**: 13.5 avg melee damage fallback, +5 WILL, +10 Aether pool
- **Effective Stats**: 11 WILL (6 base + 5 bonus), 8 FINESSE, 55 HP, 60 max Aether
- **Trade-off**: -10 HP and -2 WILL vs Casting Mystic, but has strong melee option

---

### E.4 Adept Builds

**Tech Adept** (Maximize WITS for tech weapon scaling):

- **Weapon**: Arc-Cannon (3d6+1, +4 WITS, +3 FINESSE, +2 accuracy, chain lightning)
- **Armor**: Rune-Etched Mail (+20 HP, +2 defense, +1 STURDINESS)
- **Rationale**: WITS-scaling weapon (unique to tech weapons), chain lightning for multi-target
- **Effective Stats**: 10 WITS (6 base + 4 bonus), 9 FINESSE (6 base + 3 bonus), 60 HP

**Ranged Adept** (Maximize accuracy and ranged damage):

- **Weapon**: Architect's Wrath (3d6+2, +3 FINESSE, +2 WITS, +3 accuracy, auto-targeting)
- **Armor**: Reinforced Leathers (+15 HP, +1 defense, +1 FINESSE)
- **Rationale**: +3 accuracy with auto-targeting = near-guaranteed hits
- **Effective Stats**: 10 FINESSE, 8 WITS, 55 HP
- **Special**: Auto-targeting may bypass enemy evasion/cover mechanics

---

### Appendix F: Equipment Comparison Tools

This appendix provides formulas for comparing equipment upgrades.

### F.1 Weapon Upgrade Value Formula

**Damage Per Round (DPR) Calculation**:

```
DPR = Avg Damage × Hit Rate

Where:
  Avg Damage = (Dice × 3.5) + Flat Bonus
  Hit Rate = P(Successes > Enemy Defense) ≈ 0.70-0.90 (varies by attribute/enemy)

```

**Example**: Comparing Clan-Forged Axe vs Rune-Etched Axe for Warrior with MIGHT 5

**Clan-Forged Axe** (1d6+3):

```
Avg Damage = (1 × 3.5) + 3 = 6.5
Attack Roll: 5d6 MIGHT ≈ 1.65 expected successes
Hit Rate: ~80% (vs typical enemy defense)
DPR = 6.5 × 0.80 = 5.2

```

**Rune-Etched Axe** (2d6, +1 MIGHT):

```
Avg Damage = (2 × 3.5) + 0 = 7.0
Attack Roll: 6d6 MIGHT ≈ 2.0 expected successes (+1 from bonus)
Hit Rate: ~85% (+5% from extra die)
DPR = 7.0 × 0.85 = 5.95

```

**Upgrade Value**: 5.95 / 5.2 = **+14% damage increase**

**Additional Value**: +1 MIGHT bonus applies to ALL MIGHT checks (not just attacks), including:

- Melee damage rolls (already calculated)
- Grappling checks
- Breaking down doors
- Intimidation (if using MIGHT-based intimidation)

---

### F.2 Armor Upgrade Value Formula

**Effective HP (EHP) Calculation**:

```
EHP = Total HP / (1 - Damage Reduction)

Where:
  Total HP = Base HP + Armor HP Bonus
  Damage Reduction = Defense Bonus × 0.10 (approximate; each +1 defense ≈ 10% DR)

```

**Example**: Comparing Chain Hauberk vs Rune-Etched Mail for Level 5 character (50 base HP)

**Chain Hauberk** (+20 HP, +2 defense):

```
Total HP = 50 + 20 = 70
Damage Reduction = 0.20 (2 × 0.10)
EHP = 70 / (1 - 0.20) = 70 / 0.80 = 87.5

```

**Rune-Etched Mail** (+20 HP, +2 defense, +1 STURDINESS):

```
Total HP = 50 + 20 = 70
Damage Reduction = 0.20 (same defense bonus)
EHP = 70 / 0.80 = 87.5

Additional Value: +1 STURDINESS adds:
  - +5 max HP (STURDINESS HP scaling)
  - Improved STURDINESS checks (resist trauma, poisons, exhaustion)
Adjusted EHP = 92.5

```

**Upgrade Value**: 92.5 / 87.5 = **+5.7% survivability increase**

---

### F.3 Weapon vs Armor Priority Formula

**Question**: Should I upgrade my Tier 2 weapon or Tier 2 armor first?

**Weapon Upgrade Impact**:

```
Damage increase: ~30-50% per tier
Combat duration reduction: ~25-35% (fights end faster)
Attribute bonus value: +1 die on ALL checks using that attribute

```

**Armor Upgrade Impact**:

```
Survivability increase: ~15-25% per tier
Mistakes forgiven: +1-2 extra hits before death
No offensive value: Does not help win fights

```

**Recommendation**: **Upgrade weapons first** (offense > defense).

**Reasoning**:

1. Killing enemies faster reduces total damage taken (best defense is good offense)
2. Attribute bonuses from weapons apply to attack AND utility checks
3. Armor only helps when you're already taking damage (reactive, not proactive)

**Exception**: If currently dying frequently (HP < 40 at Level 5), armor upgrade may be higher priority.

---