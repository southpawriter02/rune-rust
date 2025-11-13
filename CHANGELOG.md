# Changelog

All notable changes to Rune & Rust will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

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
