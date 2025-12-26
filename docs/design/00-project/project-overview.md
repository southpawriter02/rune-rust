---
id: SPEC-PROJECT-OVERVIEW
title: "Rune & Rust — Game Overview"
version: 1.0
status: draft
last-updated: 2025-12-07
---

# Rune & Rust — Game Overview

## 1. Vision

**Rune & Rust** is a text-based fantasy dungeon crawler RPG that combines deep tactical combat with rich character customization. Players explore procedurally generated dungeons, master one of 30+ specializations, and engage in a complex combat system emphasizing positioning, stances, and environmental awareness.

### 1.1 Design Pillars

| Pillar | Description |
|--------|-------------|
| **Tactical Depth** | Combat rewards positioning, stance selection, and ability synergies |
| **Character Identity** | 30+ specializations with unique mechanics and progression paths |
| **Exploration** | Vertical and horizontal movement, puzzles, interactive environments |
| **Persistence** | PostgreSQL-backed saves with full state preservation |
| **Dual Interface** | Terminal for testing/development, AvaloniaUI for players |

### 1.2 Core Fantasy

You are an adventurer delving into corrupted dungeons where reality itself has begun to rust and glitch. Ancient runes hold power but also danger—magic is both salvation and risk. Master your specialization, control the battlefield, and survive.

---

## 2. Technical Stack

| Component | Technology |
|-----------|------------|
| Language | C# (.NET 8+) |
| Database | PostgreSQL |
| Terminal UI | Text-based command interface |
| GUI | AvaloniaUI (cross-platform) |
| Architecture | Clean Architecture with Domain-Driven Design |

---

## 3. Core Systems Overview

### 3.1 Character System
- **Archetypes**: Warrior, Mage, Rogue, Divine, Hybrid, Specialist
- **Specializations**: 30+ unique classes with tiered ability trees
- **Attributes**: MIGHT, WITS, STURDINESS, WILL, AGILITY, PRESENCE
- **Resources**: HP, Stamina, Mana (varies by archetype)

### 3.2 Combat System
- **Positioning**: Front/Back row system with [Reach], [Push], [Pull] mechanics
- **Stances**: Offensive, Defensive, and situational stances
- **Actions**: Standard, Bonus, Reaction, Free actions per turn
- **Damage**: Soak, armor penetration, damage types, critical hits

### 3.3 Progression
- **Legend**: Primary advancement metric
- **PP (Profession Points)**: Spent to unlock specialization abilities
- **Ranks**: Abilities scale 1→2→3 based on tree progression

### 3.4 Environment
- **Dungeon Structure**: Floors, rooms, corridors
- **Movement**: Horizontal exploration + vertical (stairs, ladders, pits)
- **Interactables**: Doors, chests, levers, traps, puzzles

---

## 4. Development Priorities

1. **Documentation First**: Complete specifications before implementation
2. **Test-Driven**: Unit and integration tests for all systems
3. **Modular Design**: Loosely coupled systems for maintainability
4. **Data-Driven**: Game content defined in database, not code

---

## 5. Related Documents

- [GLOSSARY.md](./GLOSSARY.md) — Terminology definitions
- [CONVENTIONS.md](./CONVENTIONS.md) — Naming and documentation standards
- [ARCHITECTURE.md](./ARCHITECTURE.md) — Technical architecture details
