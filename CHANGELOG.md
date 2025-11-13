# Changelog

All notable changes to Rune & Rust will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

---

## [0.19.7] - 2025-11-13 - Jötun-Reader Specialization (Complete Implementation)

### Updated

#### Specialization: Jötun-Reader (System Analyst)
- **Archetype:** Adept
- **Path Type:** Coherent (updated from Heretical)
- **Mechanical Role:** Controller / Utility Specialist
- **Primary Attribute:** WITS
- **Secondary Attribute:** FINESSE
- **Resource System:** Stamina + Psychic Stress
- **Trauma Risk:** High
- **Unlock Requirements:** Legend 3+
- **Icon:** 🔍 (updated from 📜)

#### Core Philosophy
The Jötun-Reader is a forensic pathologist of the apocalypse who reads crash logs of a dead civilization. They observe and document system failures with clinical precision, analyze enemy weaknesses, translate runic inscriptions, apply tactical debuffs, and weaponize forbidden knowledge. High Trauma Risk creates dependency on Bone-Setter support. Non-combat specialist who enables party success through pure knowledge.

#### 9 Updated Abilities (30 PP Total)

**Tier 1 (3 PP each):**
1. **Scholarly Acumen I** (Passive, 3 ranks) - Investigation bonuses
   - +2d10 to +4d10 bonus to WITS-based Investigate and System Bypass checks
   - Auto-upgrade Success → Critical Success at Rank 3
2. **Analyze Weakness** (Active, 3 ranks) - Enemy analysis with Psychic Stress cost
   - Reveal 1-2 Resistances/Vulnerabilities, costs 5-0 Psychic Stress
   - Free Action at Rank 3, auto-applies [Analyzed] on Critical
3. **Runic Linguistics** (Passive, 3 ranks) - Translation system
   - Translate Elder Futhark inscriptions, handle corrupted text
   - Extrapolate 70-80% missing sections at Rank 3

**Tier 2 (4 PP each, requires 8 PP in tree):**
4. **Exploit Design Flaw** (Active, 3 ranks) - Tactical debuffs
   - Apply [Analyzed] debuff: +2 to +4 Accuracy for all allies
   - +1d10 bonus damage at Rank 3, no prior analysis required
5. **Navigational Bypass** (Active, 3 ranks) - Hazard navigation
   - Grant party +1d10 to +3d10 to bypass trap checks
   - Can use in combat at Rank 3
6. **Structural Insight** (Passive, 3 ranks) - Environmental detection
   - Auto-detect hazards, cover quality, structural features
   - Controlled collapse and +1 Defense in analyzed areas at Rank 3

**Tier 3 (5 PP each, requires 16 PP in tree):**
7. **Calculated Triage** (Passive, 3 ranks) - Healing optimization
   - +25% to +50% healing effectiveness for nearby allies
   - Field Hospital zone at Rank 3: +75% healing, +2 Resolve
8. **The Unspoken Truth** (Active, 3 ranks) - Psychological attacks
   - Opposed WITS vs WILL check, inflict [Disoriented]
   - Add 5-12 Psychic Stress to target at Rank 3, may trigger narrative consequences

**Capstone (6 PP, requires 24 PP + both Tier 3):**
9. **Architect of the Silence** (Active, 3 ranks, Once Per Combat) - Command-line seizure
   - Speak original command-line code to apply [Seized] status
   - Total paralysis for 1-2 rounds on Jötun-Forged/Undying enemies
   - Costs 15-20 Psychic Stress (reduced to 10-15 at Rank 3)
   - Auto-Critical analyze ALL Jötun-Forged/Undying at combat start (Rank 3 passive)

### Technical

- **Updated Files:**
  - `RuneAndRust.Persistence/DataSeeder.cs` - Updated Jötun-Reader seeding (SpecializationID: 2, AbilityIDs: 201-209)
  - `RuneAndRust.Engine/SpecializationFactory.cs` - Updated ability initialization with full rank progression
  - `RuneAndRust.Tests/SpecializationIntegrationTests.cs` - Validation test already exists

### Design Philosophy
- **Information Warfare:** Analyze Weakness reveals defenses, Exploit Design Flaw grants party accuracy
- **Linguistic Archaeology:** Runic Linguistics translates ancient inscriptions, Architect uses command syntax
- **Trauma Economy Integration:** Most abilities cost Psychic Stress, creating high-risk/high-reward gameplay
- **Force Multiplier:** Contributes zero direct damage but increases party effectiveness by 30-40%

### Balance Considerations
- **Psychic Stress Management:** Budget 5 Stress per Analyze, 15-20 per Architect use
- **Party Accuracy Boost:** [Analyzed] debuff increases hit rate by 10-15%
- **Runic Translation:** Gates 20-30% of dungeon secrets
- **Strategic Weakness:** Weak solo play, requires Bone-Setter support, no combat-only dungeon viability

### Synergies
- **Analyze Weakness → Exploit Design Flaw:** Full enemy intel + +4 Accuracy for party
- **Scholarly Acumen + Runic Linguistics:** Perfect translation with massive investigation bonuses
- **Calculated Triage + Bone-Setter:** Combined 75%+ healing boost
- **Architect (Rank 3 passive) + Analyze Weakness:** Auto-analyze all machines at combat start

---

## [0.19.5] - 2025-11-13 - Scrap-Tinker Specialization

### Added

#### New Specialization: Scrap-Tinker (Master Artificer)
- **Archetype:** Adept
- **Path Type:** Coherent
- **Mechanical Role:** Crafter / Pet Controller
- **Primary Attribute:** WITS
- **Secondary Attribute:** FINESSE
- **Resource System:** Stamina + Scrap Materials
- **Trauma Risk:** None
- **Unlock Requirements:** Legend 3+
- **Icon:** 🔧

#### Core Mechanics
- **Scrap Material Economy:** Track and manage Scrap Materials for crafting
- **Gadget Crafting:** Create Flash Bombs, Shock Mines, Repair Kits at workbenches
- **Quality System:** Masterwork and Prototype quality rolls for superior gadgets
- **Weapon Modifications:** Permanent enhancements to ally weapons (Elemental, Precision, Reinforced)
- **Pet System:** Deploy Scout Drone and Scrap Golem with controllable commands
- **Trap Placement:** Shock Mines trigger on enemy movement

#### 9 New Abilities (30 PP Total)

**Tier 1 (3 PP each):**
1. **Master Scavenger** (Passive) - Enhanced Scrap Material gathering
   - Find 50% more Scrap from mechanical enemies and containers
   - Start expeditions with 20 Scrap at Rank 3
2. **Deploy Flash Bomb** (Active) - AoE attack applying [Blinded] status
   - 3x3 area, WILL save DC 13-17
   - Masterwork bombs deal 2d6 damage at Rank 3
3. **Salvage Expertise** (Passive) - Crafting bonuses and quality improvements
   - +3d10 crafting bonus at Rank 3
   - 40% Masterwork chance, 10% Prototype chance

**Tier 2 (4 PP each, requires 8 PP in tree):**
4. **Deploy Scout Drone** (Active) - Deploy reconnaissance drone
   - Provides vision, reveals hidden enemies and traps
   - Can self-destruct for 4d6 AoE damage at Rank 3
5. **Deploy Shock Mine** (Active) - Place trap mine
   - 3d8-5d8 Lightning damage, STURDINESS save DC 14-18
   - Applies [Stunned] and [Slowed] effects
6. **Weapon Modification** (Active) - Permanently enhance ally weapons
   - Add Elemental damage (+1d6-2d6), Precision (+1-2 to hit), or Reinforced (+50-100% durability)
   - Can stack 2 modifications at Rank 3

**Tier 3 (5 PP each, requires 16 PP in tree):**
7. **Automated Scavenging** (Passive) - Auto-collect Scrap after combat
   - 5-15 Scrap per combat, no action required
   - 25% chance for rare components at Rank 3
8. **Efficient Assembly** (Passive) - Reduced crafting costs and time
   - 25-50% less Scrap Materials required
   - Craft 3 gadgets simultaneously at Rank 3

**Capstone (6 PP, requires 24 PP + both Tier 3):**
9. **Deploy Scrap Golem** (Active, Once Per Expedition) - Deploy powerful combat pet
   - 40-80 HP, 6-10 Armor, immune to psychic effects
   - Slam attack: 3d10-5d10 Physical damage
   - Defend command: Grant +3 Soak to adjacent ally
   - Can self-destruct for 8d10 AoE damage at Rank 3

#### Design Philosophy
- **Salvage and Innovation:** Reverse-engineer corrupted technology and repurpose scrap
- **Force Multiplier:** Enhance party capabilities through weapon mods and gadgets
- **Preparation Gameplay:** Craft during downtime, deploy strategic tools in combat
- **Utility Specialist:** Provide vision (Scout Drone), control (Flash Bombs), and traps (Shock Mines)
- **Pet-Based Gameplay:** Control drones and golems for reconnaissance and combat

### Technical
- **Updated Files:**
  - `RuneAndRust.Persistence/DataSeeder.cs` - Added Scrap-Tinker specialization (280+ lines)
    - SpecializationID: 14
    - AbilityIDs: 1401-1409
  - `RuneAndRust.Tests/SpecializationIntegrationTests.cs` - Added validation test for Scrap-Tinker
- **Test Coverage:**
  - Specialization seeding tests
  - Ability structure validation (3-3-2-1 tier distribution)
  - PP cost validation (30 PP total)
  - Prerequisite validation (Capstone requires both Tier 3 abilities)
  - Unlock requirement tests

### Balance Considerations
- **Weapon Modifications:** Target 25-40% party DPS increase when fully invested
- **Scrap Economy:** Requires 2-3 combats worth of Scrap per gadget (sustainable)
- **Pet Survivability:** Scout Drone 3-5 turn survival, Scrap Golem tanks comparable to Warrior
- **Role Differentiation:** Strong utility/support, minimal direct combat contribution
- **Strategic Weakness:** Weak solo play (crafting/mods wasted), requires long expeditions for crafting time

### Synergies
- **Master Scavenger + Efficient Assembly:** Craft twice as many gadgets with doubled Scrap income
- **Scout Drone + Shock Mines:** Vision reveals enemy paths for optimal mine placement
- **Weapon Modification + Salvage Expertise:** Prototype mods have doubled bonuses
- **Scrap Golem + Automated Scavenging:** Golem scavenges additional 10 Scrap per combat

---

## [0.19.3] - 2025-11-13 - Atgeir-wielder Specialization

### Added

#### New Specialization: Atgeir-wielder (Formation Master)
- **Archetype:** Warrior
- **Path Type:** Coherent
- **Mechanical Role:** Battlefield Controller / Formation Anchor
- **Primary Attribute:** MIGHT
- **Secondary Attribute:** WITS
- **Resource System:** Stamina
- **Trauma Risk:** None
- **Unlock Requirements:** Legend 3+
- **Icon:** ⚔️

#### Core Mechanics
- **[Reach] System:** Attack front-row enemies from back-row safety
- **[Push] Mechanic:** Drive enemies backward with STURDINESS opposed checks
- **[Pull] Mechanic:** Drag priority targets forward into kill zone
- **Stance System:** Trade mobility for massive defensive bonuses and forced movement immunity
- **Aura Effects:** Provide Soak bonuses and Stamina regeneration to adjacent allies

#### 9 New Abilities (30 PP Total)

**Tier 1 (3 PP each):**
1. **Formal Training** (Passive) - Enhanced Stamina regeneration and resistance to disorientation effects
   - Rank 3 grants +1 WITS
2. **Skewer** (Active) - MIGHT-based Physical attack with [Reach]
   - Can attack front row from back row
   - Applies [Bleeding] on critical hit at Rank 3
3. **Disciplined Stance** (Bonus Action) - Defensive stance with massive Soak
   - Immune to [Push]/[Pull] at Rank 3
   - Cannot move while active

**Tier 2 (4 PP each, requires 8 PP in tree):**
4. **Hook and Drag** (Active) - Physical attack with [Pull] effect
   - Drag back-row enemies to front row
   - Applies [Stunned] on successful Pull at Rank 3
5. **Line Breaker** (Active) - AoE attack with [Push] on all front-row enemies
   - Apply [Off-Balance] to pushed enemies at Rank 3
6. **Guarding Presence** (Passive) - Aura providing Soak and bonuses to adjacent allies
   - Rank 3 grants +3 Stamina regeneration to allies

**Tier 3 (5 PP each, requires 16 PP in tree):**
7. **Brace for Charge** (Active, Once Per Combat) - Defensive stance with counter-damage
   - Deals 6d8 Physical damage to melee attackers at Rank 3
   - Auto-stuns Mechanical/Undying attackers
8. **Unstoppable Phalanx** (Active) - Line-piercing attack hitting two enemies
   - Doubles secondary damage if primary target dies at Rank 3

**Capstone (6 PP, requires 24 PP + both Tier 3):**
9. **Living Fortress** (Passive) - Ultimate formation anchor
   - Permanent immunity to [Push] and [Pull]
   - Brace for Charge becomes reactive (twice per combat at Rank 3)
   - Zone of Control at Rank 3: Enemies opposite you have -1 to hit and cannot move freely

#### Design Philosophy
- **Tactical Discipline:** Control battlefield through positioning, reach, and forced movement
- **Formation Anchor:** Become immovable defensive core for party
- **Thinking Warrior:** WITS as secondary attribute rewards tactical play
- **Party Synergy:** High value in team play, excels at protecting fragile allies
- **Strategic Weakness:** Lower solo viability, weak against high-mobility enemies

### Technical
- **New Files:**
  - `RuneAndRust.Tests/AtgeirWielderSpecializationTests.cs` - Comprehensive unit tests (40+ tests)
- **Updated Files:**
  - `RuneAndRust.Persistence/DataSeeder.cs` - Added Atgeir-wielder specialization (300+ lines)
    - SpecializationID: 12
    - AbilityIDs: 1201-1209
- **Test Coverage:**
  - Specialization seeding tests
  - Ability structure validation (3-3-2-1 tier distribution)
  - PP cost validation (30 PP total)
  - Prerequisite validation
  - Unlock requirement tests
  - Ability mechanics tests

### Balance Considerations
- **Target Success Rates:** Push/Pull effects balanced for 60-70% success vs average STURDINESS
- **Defensive Parity:** Disciplined Stance rivals Skjaldmær for tankiness with different playstyle
- **Safety Tax:** Back-row Skewer damage ~80% of front-row attacks (balanced for safety)
- **Role Differentiation:** Strong party controller/tank, intentionally weak solo performance

---

## [0.3.0] - 2025-11-11 - Equipment & Loot Update

### Added

#### Equipment System
- **36 unique items** across 3 weapon types per class and 12 armor pieces
- **5 quality tiers:** Jury-Rigged → Scavenged → Clan-Forged → Optimized → Myth-Forged
- **Equipment slots:** Weapon + Armor
- **Starting equipment:** All classes now begin with class-appropriate gear
  - Warrior: Rusty Hatchet + Scrap Plating
  - Scavenger: Makeshift Spear + Tattered Leathers
  - Mystic: Crude Staff + Tattered Leathers

#### Inventory System
- **5-slot inventory** with pickup/drop mechanics
- **Equipment commands:**
  - `inventory` - View equipped gear and inventory
  - `equip <item>` - Equip weapon or armor
  - `unequip <slot>` - Unequip weapon or armor
  - `pickup <item>` - Pick up item from ground
  - `drop <item>` - Drop item to ground
  - `compare <item>` - Compare unequipped item with equipped gear
- **Inventory capacity management** with full inventory warnings

#### Loot System
- **Dynamic loot generation** from defeated enemies
- **Smart loot tables:**
  - Corrupted Servitor: 60% Jury-Rigged, 30% Scavenged, 10% nothing
  - Blight-Drone: 40% Scavenged, 40% Clan-Forged, 20% Optimized
  - Ruin-Warden (Boss): 30% Optimized, 70% Myth-Forged
- **Class-appropriate drops:** 60% chance for weapons to match player class
- **Puzzle reward:** Guaranteed Clan-Forged weapon for solving puzzle
- **Ground items:** Loot appears in rooms after combat

#### Equipment Features
- **Stat bonuses:** Weapons and armor provide attribute bonuses (MIGHT, FINESSE, WILL, etc.)
- **Combat integration:** Equipment stats affect damage, accuracy, HP, and defense
- **Special effects:** Myth-Forged items have unique abilities
  - Warden's Greatsword: Ignores 50% armor, grants [Fortified] on kill
  - Shadow's Edge: Inflicts [Bleeding] (2 damage/turn)
  - Void Lens: -2 Aether cost, Aether regen on hit
  - Warden's Aegis: Can block attacks
  - Shadowweave Cloak: +50% evasion chance

#### Persistence
- **Equipment save/load:** Full persistence for equipped gear and inventory
- **Room item restoration:** Ground loot persists across save/load
- **Database migration:** Backward compatible with v0.1/v0.2 saves
- **Auto-save on transitions:** Equipment state saved automatically

#### UI Enhancements
- **Equipment display:** Rich UI for inventory with quality tier colors
- **Comparison view:** Side-by-side stat comparison when equipping
- **Ground item notifications:** Visual indicators for loot in rooms
- **Character sheet update:** Shows equipped weapon and armor

### Changed
- Combat damage now uses equipped weapon stats instead of fixed base damage
- Character stats recalculate when equipping/unequipping armor
- Room descriptions now show ground items
- Character creation flow now includes starting equipment assignment

### Technical
- New files:
  - `RuneAndRust.Core/Equipment.cs` - Equipment data model with quality tiers
  - `RuneAndRust.Engine/EquipmentDatabase.cs` - Hard-coded database of 36 items
  - `RuneAndRust.Engine/EquipmentService.cs` - Equipment management service
  - `RuneAndRust.Engine/LootService.cs` - Loot generation with drop tables
- Updated files:
  - `PlayerCharacter.cs` - Added equipment slots and inventory
  - `Room.cs` - Added ItemsOnGround list
  - `CombatEngine.cs` - Uses equipment stats in damage calculations
  - `SaveRepository.cs` - Serializes equipment to JSON
  - `SaveData.cs` - Added equipment fields
  - `Program.cs` - Added 6 new equipment command handlers
  - `UIHelper.cs` - Added equipment display methods (~500 lines)
  - `CharacterFactory.cs` - Assigns starting equipment on creation
- Total new code: ~2,500 lines of C#
- Test coverage: 40+ new unit tests for equipment and loot systems

### Documentation
- `IMPLEMENTATION_V03.md` - Technical implementation notes
- `BALANCE_REVIEW_V03.md` - Comprehensive balance analysis (rating: 9/10)
- Updated `README.md` with v0.3 features

---

## [0.2.0] - 2025-11-10 - Expanded Edition

### Added

#### Progression System
- **XP and leveling:** Players can level up from 1 to 5
- **XP rewards:** 10 XP per Corrupted Servitor, 25 XP per Blight-Drone, 100 XP for Ruin-Warden
- **Level-up rewards:**
  - +10 HP
  - +5 Stamina
  - +1 Attribute Point (player choice)
  - Full heal (HP and Stamina restored)
- **XP thresholds:** 50/100/150/200 XP required for levels 2-5
- **Attribute caps:** Maximum attribute value of 6 for strategic growth

#### New Abilities
- **6 additional abilities** (2 per class) unlock at levels 3 and 5:
  - **Warrior:**
    - Cleaving Strike (Lv3): AOE attack hitting all enemies
    - Battle Rage (Lv5): Gain bonus damage when below 50% HP
  - **Scavenger:**
    - Precision Strike (Lv3): High-accuracy single-target attack
    - Survivalist (Lv5): Passive HP regeneration
  - **Mystic:**
    - Aetheric Shield (Lv3): Absorbs damage
    - Chain Lightning (Lv5): AOE ability that chains between enemies

#### Save/Load System
- **SQLite persistence:** Game state saved to local database
- **Auto-save:** Automatic save on room transitions
- **Load game menu:** Resume any saved character
- **Save management:** List, load, and delete saved games
- **Persistence coverage:**
  - Character stats (HP, Stamina, XP, Level, Attributes)
  - World state (current room, cleared rooms, puzzle status)
  - Boss defeat status

### Changed
- Combat now grants XP on enemy defeat
- Victory screen shows final level and XP earned
- Character sheet displays current level and XP progress

### Technical
- New files:
  - `RuneAndRust.Core/WorldState.cs`
  - `RuneAndRust.Engine/ProgressionService.cs`
  - `RuneAndRust.Persistence/SaveRepository.cs`
  - `RuneAndRust.Persistence/SaveData.cs`
- Test coverage: Save/load integration tests
- Total code: ~5,000 lines of C#

### Documentation
- `CODE_REVIEW_V02.md` - Technical code review and analysis
- Updated `README.md` with v0.2 features

---

## [0.1.0] - 2025-11-09 - Vertical Slice

### Added

#### Core Game Loop
- **Character creation:** 3 playable classes with unique abilities
- **Exploration:** Navigate 5 interconnected rooms
- **Turn-based combat:** Dice pool combat system with initiative
- **Environmental puzzle:** WITS-based challenge
- **Boss fight:** Ruin-Warden with phase-based AI
- **Victory/defeat screens:** Endgame narrative

#### Classes and Abilities
- **Warrior:** Melee combat specialist
  - Starting abilities: Power Strike, Shield Wall
  - Starting stats: 50 HP, 30 Stamina
- **Scavenger:** Balanced tactical fighter
  - Starting abilities: Exploit Weakness, Quick Dodge
  - Starting stats: 40 HP, 40 Stamina
- **Mystic:** Ranged caster with control abilities
  - Starting abilities: Aetheric Bolt, Disrupt
  - Starting stats: 30 HP, 50 Stamina

#### Dice Pool Combat System
- Roll d6 equal to (Attribute + bonuses)
- Count 5-6 as successes
- Net successes = damage dealt or effect strength
- Used for attacks, defenses, ability checks, and puzzles

#### World and Enemies
- **5 rooms:** Entrance, Corridor, Combat Arena, Puzzle Chamber, Boss Sanctum
- **3 enemy types:**
  - Corrupted Servitor: Basic enemy (10 HP)
  - Blight-Drone: Mid-tier threat (25 HP)
  - Ruin-Warden: Boss with phase-based AI (75 HP)

#### Commands
- **Exploration:** look, stats, move [direction], help, restart, quit
- **Combat:** attack, defend, ability [name], flee
- **Puzzle:** attempt [attribute]

### Technical
- **Clean architecture:** Separated Core, Engine, Persistence, and ConsoleApp layers
- **Spectre.Console:** Rich terminal UI with HP/Stamina bars
- **Dice service:** Deterministic dice rolling for testing
- **Enemy AI:** Weighted decision trees
- **Status effects:** Defense buffs, stuns, dodge charges
- Total code: ~3,000 lines of C#

### Documentation
- `BALANCE.md` - Balance analysis and design decisions
- `README.md` - Project overview and getting started guide

---

## Development Milestones

- **Week 1 (2025-11-02):** Foundation - Project setup, core models, dice service
- **Week 2 (2025-11-05):** Character creation, exploration, puzzle system
- **Week 3 (2025-11-07):** Combat system, enemy AI, abilities
- **Week 4 (2025-11-09):** Polish, balance, v0.1 release
- **Week 5 (2025-11-10):** Progression system, new abilities, save/load, v0.2 release
- **Week 6 (2025-11-11):** Equipment system, loot generation, inventory, v0.3 release

---

## Versioning Notes

- **v0.1:** Vertical slice - complete playthrough from start to finish
- **v0.2:** Expanded edition - progression systems and persistence
- **v0.3:** Equipment & loot - itemization and build diversity
- **v0.4 (Planned):** TBD - Community feedback will shape next update

---

*For detailed technical implementation notes, see IMPLEMENTATION_V03.md*
*For balance analysis, see BALANCE_REVIEW_V03.md*
