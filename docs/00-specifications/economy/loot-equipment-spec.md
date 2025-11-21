# Loot & Equipment System Specification

> **Template Version**: 1.0
> **Last Updated**: 2025-11-21
> **Status**: Draft
> **Specification ID**: SPEC-ECONOMY-001

---

## Document Control

### Version History
| Version | Date | Author | Changes | Reviewers |
|---------|------|--------|---------|-----------|
| 1.0 | 2025-11-21 | AI | Initial specification | - |

### Approval Status
- [x] **Draft**: Initial authoring in progress
- [ ] **Review**: Ready for stakeholder review
- [ ] **Approved**: Approved for implementation
- [ ] **Active**: Currently implemented and maintained
- [ ] **Deprecated**: Superseded by [SPEC-ID]

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
- Archetype System: Class-appropriate loot generation (Warrior/Scavenger/Mystic) → `SPEC-PROGRESSION-002`
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
  - Lines 293-432: Scavenger weapons (Spears, Daggers)
  - Lines 437-605: Mystic weapons (Staves, Focuses)
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
   - **Rationale**: Weapon categories align with character archetypes (Warrior=MIGHT, Scavenger=FINESSE, Mystic=WILL), reinforcing class fantasy and ensuring loot feels appropriate for player build
   - **Examples**:
     - Warriors use Axes (balanced) and Greatswords (high damage, two-handed)
     - Scavengers use Spears (reach) and Daggers (fast, low stamina)
     - Mystics use Staves (Aether reduction) and Focuses (ability amplification, no melee)
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

> **Completeness Checklist**:
> - [x] All requirements have unique IDs (FR-001 through FR-008)
> - [x] All requirements have priority assigned
> - [x] All requirements have acceptance criteria
> - [x] All requirements have at least one example scenario
> - [x] All requirements trace to design goals
> - [x] All requirements are testable

### FR-001: Equipment Quality Tier System
**Priority**: Critical
**Status**: Implemented

**Description**:
The system defines five discrete quality tiers (Jury-Rigged, Scavenged, Clan-Forged, Optimized, Myth-Forged) where each tier represents progressively higher stats, better bonuses, and unique special effects. Equipment of higher quality provides superior combat effectiveness through increased damage, defense, and attribute modifiers.

**Rationale**:
Quality tiers create clear progression milestones; players instantly recognize Myth-Forged > Scavenged; tier system enables balanced loot tables (trash mobs drop low-tier, bosses drop high-tier).

**Acceptance Criteria**:
- [ ] Five quality tiers defined as enum: `QualityTier` (0=Jury-Rigged, 1=Scavenged, 2=Clan-Forged, 3=Optimized, 4=Myth-Forged)
- [ ] Each tier has human-readable name displayed in UI
- [ ] Equipment stats scale with quality (Tier 0: ~3.5 avg damage → Tier 4: ~11 avg damage for weapons)
- [ ] Attribute bonuses exclusive to Tier 2+ equipment (+1 at Tier 2, +2-3 at Tier 4)
- [ ] Special effects exclusive to Tier 4 (Myth-Forged) equipment
- [ ] Quality tier displayed in equipment name: "Rusty Hatchet (Jury-Rigged)"

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
Weapon categories reinforce class archetypes; Warriors use MIGHT weapons (Axe, Greatsword), Scavengers use FINESSE (Spear, Dagger), Mystics use WILL (Staff, Focus); ensures loot generation can create class-appropriate drops.

**Acceptance Criteria**:
- [ ] 11 weapon categories defined: Axe, Greatsword, Spear, Dagger, Staff, Focus, Blade, Blunt, EnergyMelee, Rifle, HeavyBlunt
- [ ] Each weapon has `WeaponAttribute` property ("MIGHT", "FINESSE", or "WILL")
- [ ] Attack rolls use weapon's attribute: MIGHT weapons roll MIGHTd6 for attack
- [ ] Weapon categories have distinct stat ranges:
  - Daggers: Low damage (1d6-1 to 2d6), low stamina cost (3-4)
  - Greatswords: High damage (1d6+2 to 4d6+4), high stamina cost (8-15)
  - Focuses: Zero melee damage, grant bonus dice to abilities
- [ ] Loot service can filter weapons by category for class-appropriate generation

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
- [ ] `GenerateLoot(Enemy)` method returns Equipment or null based on drop table
- [ ] Servitor drop table: 60% Tier 0, 30% Tier 1, 10% null
- [ ] Drone drop table: 40% Tier 1, 40% Tier 2, 20% Tier 3
- [ ] Boss (Warden) drop table: 30% Tier 3, 70% Tier 4
- [ ] Drop tables use weighted RNG (Random.Next(100) with threshold checks)
- [ ] Loot quality never exceeds enemy tier (trash mobs can't drop Myth-Forged)
- [ ] Combat log displays loot: "[yellow]💎 Clan-Forged Axe dropped![/]"

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
When generating loot, the system has a 60% chance to drop weapons appropriate for the player's character class (Warriors get Axes/Greatswords, Scavengers get Spears/Daggers, Mystics get Staves/Focuses). Boss loot is always class-appropriate. This ensures players receive useful equipment without forcing rigid class restrictions on all drops.

**Rationale**:
Class-appropriate loot reduces frustration of receiving unusable weapons (Warrior finding Staff); 60% chance balances targeted drops vs build experimentation; boss loot guarantees meaningful reward for class.

**Acceptance Criteria**:
- [ ] `GenerateRandomItem(quality, player)` checks player.Class
- [ ] 60% chance to filter EquipmentDatabase for class-appropriate weapons
- [ ] Class weapon mappings:
  - Warrior: Axe, Greatsword
  - Scavenger: Spear, Dagger
  - Mystic: Staff, Focus
- [ ] Boss loot (`GenerateClassAppropriateItem`) always filters by class (100% appropriate)
- [ ] 40% of non-boss drops can be off-class (enables build experimentation)
- [ ] Armor drops are not class-restricted (all classes can use all armor)

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
- [ ] Three armor categories: Light, Medium, Heavy
- [ ] Light Armor: +5 to +15 HP, +0 to +1 defense, +FINESSE bonuses, no penalties
- [ ] Medium Armor: +10 to +25 HP, +1 to +2 defense, balanced attribute bonuses
- [ ] Heavy Armor: +15 to +40 HP, +2 to +3 defense, +STURDINESS bonuses, -1 to -2 FINESSE penalty (Tier 2+)
- [ ] Armor category displayed in equipment description
- [ ] All classes can equip all armor (no class restrictions on armor)

**Example Scenarios**:
1. **Scenario**: Glass cannon Scavenger chooses armor
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
Equipment can grant attribute bonuses (+MIGHT, +FINESSE, +WILL, +STURDINESS) that stack additively with base attributes and exceed the normal attribute cap of 6. A character with MIGHT 6 (cap) + Myth-Forged weapon (+3 MIGHT) effectively has MIGHT 9 for attack rolls and damage calculations.

**Rationale**:
Equipment bonuses bypass attribute cap to enable late-game power scaling; Myth-Forged items provide ~50% power increase beyond base cap; creates incentive to find high-tier equipment even at max attribute investment.

**Acceptance Criteria**:
- [ ] Equipment has `List<EquipmentBonus>` with AttributeName and BonusValue
- [ ] Bonuses stack additively: Weapon +2 MIGHT + Armor +1 MIGHT = +3 total MIGHT
- [ ] Bonuses apply to all rolls using that attribute (attack rolls, skill checks)
- [ ] Bonuses displayed in character sheet: "MIGHT: 6 (+3 from equipment) = 9"
- [ ] Tier 2 equipment: +1 attribute typical
- [ ] Tier 4 equipment: +2 to +4 attributes possible
- [ ] No cap on equipment bonuses (can stack multiple +3 items theoretically)

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
- [ ] Equipment has `SpecialEffect` string property describing effect
- [ ] Equipment has `IgnoresArmor` boolean flag for armor-piercing
- [ ] Special effects exclusive to Tier 4 equipment (no Tier 0-3 items have effects)
- [ ] Effects displayed in equipment tooltip and description
- [ ] Common special effects:
  - Ignore Armor: Damage calculation skips defense bonus
  - Lifesteal: Heal 25% of damage dealt
  - Chain Lightning: Attack bounces to 2 additional targets
  - Regeneration: Heal X HP per turn
  - AoE: Damage all adjacent enemies
- [ ] Some effects have drawbacks: Heretical Blade grants +10 Corruption on equip

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
