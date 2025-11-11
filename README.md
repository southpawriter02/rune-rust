# Rune & Rust

A text-based dungeon crawler set in the twilight of a broken world. Corrupted machines guard ancient ruins. Only the bold survive.

**Current Version:** v0.3 Equipment & Loot ✅ **COMPLETE**

## Overview

Rune & Rust is a turn-based dungeon crawler where you explore a ruined facility, fight corrupted machines, solve environmental puzzles, and defeat a powerful boss. The game features a comprehensive equipment and loot system with 36 unique items across 5 quality tiers, progression systems with leveling and abilities, and save/load functionality for extended play sessions.

### Core Loop
1. **Character Creation** → Choose from 3 classes, each with starting equipment and 4 unique abilities
2. **Exploration** → Navigate 5 interconnected rooms, discover loot
3. **Combat** → Fight corrupted machines, earn XP, level up (1-5), collect equipment drops
4. **Loot & Gear** → Find 36 unique weapons and armor pieces across 5 quality tiers
5. **Inventory** → Manage equipment, compare stats, optimize your build
6. **Progression** → Unlock new abilities at levels 3 and 5
7. **Puzzle** → Solve environmental challenge for guaranteed reward
8. **Boss Fight** → Defeat the Ruin-Warden for legendary loot
9. **Save/Load** → Continue your journey across multiple sessions with equipment persistence
10. **Victory** → Survive to see the ending

## Project Structure

```
RuneAndRust/
├── RuneAndRust.sln                # Visual Studio solution
├── RuneAndRust.Core/              # Data models (POCOs)
│   ├── Attributes.cs
│   ├── CharacterClass.cs
│   ├── PlayerCharacter.cs
│   ├── Enemy.cs
│   ├── Room.cs
│   ├── Ability.cs
│   ├── CombatState.cs
│   ├── WorldState.cs              # [v0.2] Game progression state
│   └── Equipment.cs               # [NEW v0.3] Equipment & quality tiers
├── RuneAndRust.Engine/            # Game logic and services
│   ├── DiceService.cs
│   ├── DiceResult.cs
│   ├── ProgressionService.cs      # [v0.2] XP and leveling
│   ├── CombatEngine.cs
│   ├── EnemyAI.cs
│   ├── CharacterFactory.cs
│   ├── EnemyFactory.cs
│   ├── CommandParser.cs
│   ├── GameState.cs
│   ├── GameWorld.cs
│   ├── EquipmentDatabase.cs       # [NEW v0.3] 36 unique items
│   ├── EquipmentService.cs        # [NEW v0.3] Equip/unequip logic
│   └── LootService.cs             # [NEW v0.3] Drop tables & generation
├── RuneAndRust.Persistence/       # [v0.2] Save/Load system
│   ├── SaveRepository.cs          # SQLite-based persistence
│   └── SaveData.cs                # Serialization DTOs
└── RuneAndRust.ConsoleApp/        # UI and main game loop
    ├── Program.cs
    └── UIHelper.cs
```

### Architecture Principles
- **Separation of Concerns**: Core models, game logic, and UI are in separate projects
- **No Logic in Models**: Core project contains only POCOs
- **No UI in Engine**: Engine project has no console/display code
- **Clean Dependencies**: ConsoleApp → Engine → Core (one direction only)

## Building and Running

### Prerequisites
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or later
- Visual Studio 2022, VS Code, or Rider (optional)

### Build
```bash
# Restore dependencies
dotnet restore

# Build the solution
dotnet build

# Or build in Release mode
dotnet build -c Release
```

### Run
```bash
# Run from the ConsoleApp directory
cd RuneAndRust.ConsoleApp
dotnet run

# Or run directly from solution root
dotnet run --project RuneAndRust.ConsoleApp
```

## Development Progress

### Week 1: Foundation ✅ COMPLETE
- [x] Set up Visual Studio solution with 3 projects
- [x] Create core data models (PlayerCharacter, Enemy, Room, Ability, CombatState)
- [x] Create DiceService with dice pool mechanics
- [x] Create basic ConsoleApp with welcome screen
- [x] Install Spectre.Console for UI

### Week 2: Character Creation + Exploration ✅ COMPLETE
- [x] Implement character creation flow
- [x] Create all 5 Room objects with descriptions
- [x] Implement exploration commands (look, move, stats)
- [x] Create CommandParser service
- [x] Implement puzzle system with WITS checks
- [x] Implement victory/defeat screens

### Week 3: Combat System ✅ COMPLETE
- [x] Create CombatEngine service with initiative system
- [x] Implement all player combat commands (attack, defend, ability, flee)
- [x] Implement enemy AI with decision trees
- [x] Create rich combat UI with Spectre.Console
- [x] Implement all 6 class abilities
- [x] Boss fight with phase-based AI

### Week 4: Polish & Balance ✅ COMPLETE
- [x] Add restart option after game over/victory
- [x] Add ASCII art for boss encounter
- [x] Add tutorial hints in starting room
- [x] Improve victory screen with player stats
- [x] Playtest and balance (see BALANCE.md)
- [x] Adjust puzzle difficulty based on playtesting

### 🎉 v0.1 Vertical Slice: SHIPPED!

**The complete game is playable from start to finish!**
- Total development time: 4 weeks
- Total code written: ~3,000 lines of C#
- Playtime: 30-45 minutes per playthrough
- All core systems implemented and balanced

### 🚀 v0.2 Expanded Edition: SHIPPED!

**New features expand the core gameplay loop!**
- Phase 1: XP System & Leveling (Levels 1-5)
- Phase 2: New Abilities (2 per class, unlock at levels 3 & 5)
- Phase 3: Save/Load System with SQLite
- Total code: ~5,000 lines of C#
- Supports longer play sessions with persistence

### ⚔️ v0.3 Equipment & Loot: SHIPPED!

**The Equipment Update brings itemization and loot!**
- Phase 1-3: Core Equipment & Loot Systems (36 unique items)
- Phase 4: Combat Integration (equipment stats used in combat)
- Phase 5: Save/Load Persistence (equipment survives restarts)
- Total code: ~7,500 lines of C#
- Build diversity and progression through gear
- 5 Quality Tiers: Jury-Rigged → Scavenged → Clan-Forged → Optimized → Myth-Forged

## What's New in v0.3

### New Features (Equipment & Loot)
✅ **36 Unique Items** - 8 weapons per class, 12 armor pieces across quality tiers
✅ **5 Quality Tiers** - Jury-Rigged → Scavenged → Clan-Forged → Optimized → Myth-Forged
✅ **Equipment System** - Equip weapons and armor with stat bonuses
✅ **Inventory Management** - 5-slot inventory with pickup/drop/compare commands
✅ **Dynamic Loot Generation** - Smart loot tables with 60% class-appropriate drops
✅ **Combat Integration** - Equipment stats affect damage, defense, and attributes
✅ **Starting Equipment** - All classes begin with class-appropriate gear
✅ **Puzzle Rewards** - Guaranteed Clan-Forged weapon for solving puzzle
✅ **Boss Loot** - Ruin-Warden drops legendary Myth-Forged equipment
✅ **Equipment Persistence** - Full save/load support for gear and inventory
✅ **Equipment Commands** - inventory, equip, unequip, pickup, drop, compare
✅ **Attribute Bonuses** - Higher-tier equipment provides stat boosts

### Quality Tier System
- **Jury-Rigged (Tier 0):** Scrap held together with hope and wire
- **Scavenged (Tier 1):** Salvaged from ruins, functional but worn
- **Clan-Forged (Tier 2):** Crafted by survivor communities, solid work
- **Optimized (Tier 3):** Pre-Glitch tech, carefully maintained
- **Myth-Forged (Tier 4):** Legendary artifacts with special effects

### Equipment Highlights
- **Warrior:** Rusty Hatchet → Warden's Greatsword (ignores 50% armor)
- **Scavenger:** Makeshift Spear → Shadow's Edge (inflicts bleeding)
- **Mystic:** Crude Staff → Void Lens (Aether regeneration)
- **Armor:** Scrap Plating → Warden's Aegis (can block attacks)

## What's New in v0.2

### New Features (Expanded Edition)
✅ **XP & Leveling System** - Gain XP from defeating enemies, level up 1-5
✅ **Level-Up Rewards** - +10 HP, +5 Stamina, +1 Attribute Point, full heal
✅ **12 Total Abilities** - 4 per class (2 starting, 2 unlock at levels 3 & 5)
✅ **New Combat Mechanics** - Bleeding, Battle Rage, AOE attacks, Shield absorption
✅ **Save/Load System** - SQLite persistence with auto-save on room transitions
✅ **Load Game Menu** - Resume from any saved character
✅ **Enhanced Progression** - XP thresholds: 50/100/150/200 for levels 2-5
✅ **Attribute Caps** - Strategic growth with max attribute value of 6

### Complete Features from v0.1
✅ **3 Playable Classes** - Warrior, Scavenger, Mystic
✅ **5 Interconnected Rooms** - Fully explored dungeon with atmospheric descriptions
✅ **Turn-Based Combat** - Initiative system with dice pool mechanics
✅ **3 Enemy Types** - Servitors (10 XP), Drones (25 XP), Boss (100 XP)
✅ **Environmental Puzzle** - WITS-based challenge with multiple attempts
✅ **Boss Fight** - Phase-based AI with escalating difficulty
✅ **Rich Terminal UI** - Powered by Spectre.Console with HP/Stamina bars, combat logs
✅ **Restart System** - Play again without relaunching
✅ **Tutorial System** - In-game hints for new players

### Technical Highlights
- Clean architecture with separated concerns
- Full dice pool combat system (roll d6s, count 5-6 as successes)
- Enemy AI with weighted decision trees
- Status effect system (defense buffs, stuns, dodge charges)
- Initiative-based turn order
- Combat log with action history
- Comprehensive balance analysis

## Game Systems

### The Three Classes

| Class | HP | Stamina | Focus | Starting Abilities | Unlocked Abilities |
|-------|-----|---------|-------|--------------------|--------------------|
| **Warrior** | 50 | 30 | Melee combat, high survivability | Power Strike, Shield Wall | Cleaving Strike (Lv3), Battle Rage (Lv5) |
| **Scavenger** | 40 | 40 | Balanced, tactical | Exploit Weakness, Quick Dodge | Precision Strike (Lv3), Survivalist (Lv5) |
| **Mystic** | 30 | 50 | Ranged abilities, control | Aetheric Bolt, Disrupt | Aetheric Shield (Lv3), Chain Lightning (Lv5) |

### Dice Pool System
- Roll d6 equal to (Attribute + bonuses)
- Count 5-6 as successes
- Net successes = damage dealt or effect strength
- Used for attacks, defenses, ability checks, and puzzles

### The Five Rooms
1. **Entrance** - Safe zone, tutorial
2. **Corridor** - First combat (2x Corrupted Servitor)
3. **Combat Arena** - Main combat (3x Blight-Drone)
4. **Puzzle Chamber** - Environmental puzzle (WITS check)
5. **Boss Sanctum** - Final fight (Ruin-Warden)

## Technology Stack

- **Language**: C# 12
- **Framework**: .NET 8.0
- **UI Library**: [Spectre.Console](https://spectreconsole.net/) for rich terminal UI
- **Architecture**: Clean Architecture with separated concerns

## Scope: What's IN v0.3

✅ 3 character classes with 4 abilities each
✅ XP and leveling system (5 levels)
✅ Progression rewards (HP, Stamina, Attributes)
✅ **36 unique equipment items** (NEW)
✅ **Equipment slots: weapon + armor** (NEW)
✅ **5-slot inventory system** (NEW)
✅ **Quality tier progression (5 tiers)** (NEW)
✅ **Dynamic loot generation** (NEW)
✅ **Class-appropriate smart loot** (NEW)
✅ **Equipment persistence** (NEW)
✅ 5 attributes (MIGHT, FINESSE, WITS, WILL, STURDINESS)
✅ Turn-based combat with dice pool mechanics
✅ 3 enemy types with XP rewards
✅ 5 rooms in a linear path
✅ One environmental puzzle with reward
✅ Boss fight with phase-based AI and legendary loot
✅ Save/load system with SQLite
✅ Auto-save on room transitions
✅ Victory/defeat screens

## Scope: What's OUT of v0.3

❌ Multiple difficulty settings
❌ Specializations and advanced classes
❌ Trauma/Corruption mechanics
❌ Rune inscription system
❌ Factions and reputation
❌ Procedural dungeon generation
❌ Crafting and enchanting
❌ Equipment set bonuses
❌ Character appearance customization

*These features are planned for future versions*

## Contributing

This is a solo development project for the vertical slice. Feature requests and suggestions are welcome, but please note that v0.1 scope is locked.

## License

See [LICENSE](LICENSE) file for details.

## Development Philosophy

> "Ship first, expand later. v0.1 is DONE when you can play from start to finish without touching code."

The vertical slice prioritizes a complete, playable experience over feature breadth. Every system is implemented at minimum viable depth to support the core 30-minute gameplay loop.
