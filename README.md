# Rune & Rust

A text-based dungeon crawler set in the twilight of a broken world. Corrupted machines guard ancient ruins. Only the bold survive.

**Current Version:** v0.1 Vertical Slice (In Development)

## Overview

Rune & Rust is a turn-based dungeon crawler where you explore a ruined facility, fight corrupted machines, solve environmental puzzles, and defeat a powerful boss. The vertical slice (v0.1) delivers a complete 30-45 minute gameplay experience.

### Core Loop
1. **Character Creation** → Choose from 3 classes with unique abilities
2. **Exploration** → Navigate 5 interconnected rooms
3. **Combat** → Fight corrupted machines using a dice pool system
4. **Puzzle** → Solve one environmental challenge
5. **Boss Fight** → Defeat the Ruin-Warden
6. **Victory** → Survive to see the ending

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
│   └── CombatState.cs
├── RuneAndRust.Engine/            # Game logic and services
│   ├── DiceService.cs
│   └── DiceResult.cs
└── RuneAndRust.ConsoleApp/        # UI and main game loop
    └── Program.cs
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

### Week 1: Foundation ✓
- [x] Set up Visual Studio solution with 3 projects
- [x] Create core data models (PlayerCharacter, Enemy, Room, Ability, CombatState)
- [x] Create DiceService with dice pool mechanics
- [x] Create basic ConsoleApp with welcome screen
- [x] Install Spectre.Console for UI

### Week 2: Character Creation + Exploration (Next)
- [ ] Implement character creation flow
- [ ] Create all 5 Room objects with descriptions
- [ ] Implement exploration commands (look, move, stats)
- [ ] Create CommandParser service

### Week 3: Combat System
- [ ] Create CombatEngine service
- [ ] Implement player combat commands
- [ ] Implement enemy AI
- [ ] Create combat UI with Spectre.Console

### Week 4: Puzzle + Boss + Polish
- [ ] Implement puzzle mechanics
- [ ] Implement boss fight with phase-based AI
- [ ] Create victory/defeat screens
- [ ] Playtesting and balance

## Game Systems

### The Three Classes

| Class | HP | Stamina | Focus | Abilities |
|-------|-----|---------|-------|-----------|
| **Warrior** | 50 | 30 | Melee combat, high survivability | Power Strike, Shield Wall |
| **Scavenger** | 40 | 40 | Balanced, tactical | Exploit Weakness, Quick Dodge |
| **Mystic** | 30 | 50 | Ranged abilities, control | Aetheric Bolt, Disrupt |

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

## Scope: What's IN v0.1

✅ 3 character classes with 2 abilities each
✅ 5 attributes (MIGHT, FINESSE, WITS, WILL, STURDINESS)
✅ Turn-based combat with dice pool mechanics
✅ 3 enemy types (Servitor, Drone, Boss)
✅ 5 rooms in a linear path
✅ One environmental puzzle
✅ Boss fight with phase-based AI
✅ Victory/defeat screens

## Scope: What's OUT of v0.1

❌ Progression system (no XP, no leveling)
❌ Equipment system (no loot, no gear upgrades)
❌ Inventory system
❌ Save/load functionality
❌ Multiple difficulty settings
❌ Specializations and advanced classes
❌ Trauma/Corruption mechanics
❌ Rune inscription system
❌ Factions and reputation

*These features are planned for v0.2 and beyond*

## Contributing

This is a solo development project for the vertical slice. Feature requests and suggestions are welcome, but please note that v0.1 scope is locked.

## License

See [LICENSE](LICENSE) file for details.

## Development Philosophy

> "Ship first, expand later. v0.1 is DONE when you can play from start to finish without touching code."

The vertical slice prioritizes a complete, playable experience over feature breadth. Every system is implemented at minimum viable depth to support the core 30-minute gameplay loop.
