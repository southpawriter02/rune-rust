---
id: SPEC-CORE-LOGGING-MATRIX
title: "Logging Matrix — System Coverage Reference"
version: 1.0
status: draft
last-updated: 2025-12-11
parent: logging.md
---

# Logging Matrix

> *Quick reference for what each system logs and at what level.*

**Reference:** [logging.md](logging.md) — Full logging specification

---

## 1. Overview

This matrix consolidates logging requirements across all core systems. Use this as a quick reference when:

- Implementing a new system
- Debugging production issues
- Reviewing log output for specific features
- Setting up log filtering per sink

---

## 2. Core Systems Matrix

### 2.1 Critical Path Systems

| System | Information | Warning | Error | Debug | Verbose |
|--------|-------------|---------|-------|-------|---------|
| **Dice System** | Critical/Fumble | — | Roll failure | Roll results | Every roll |
| **Game Loop** | Phase start/end | Slow tick (>16ms) | Phase transition fail | State changes | Per-tick timing |
| **Persistence** | Save/Load complete | Slow query (>100ms) | Save/Load failure | Query execution | Entity tracking |
| **Events** | — | Unhandled event | Handler exception | Event published | Handler invocation |

### 2.2 Character Systems

| System | Information | Warning | Error | Debug | Verbose |
|--------|-------------|---------|-------|-------|---------|
| **Death/Resurrection** | Death, Respawn | Low HP threshold | Resurrection fail | Death save rolls | — |
| **Trauma Economy** | Trauma acquired | High stress (80+) | — | Stress changes | — |
| **Resources (HP/Stamina)** | — | Low resource | — | Resource changes | Regen ticks |
| **Character Creation** | Character created | — | Validation failure | Attribute allocation | — |

### 2.3 Combat Systems

| System | Information | Warning | Error | Debug | Verbose |
|--------|-------------|---------|-------|-------|---------|
| **Combat Engine** | Combat start/end | — | Turn processing fail | Turn transitions | Action queue |
| **Status Effects** | — | — | Application fail | Apply/Remove/Expire | Tick processing |
| **Damage Calculation** | — | — | — | Damage dealt | Soak calculation |

### 2.4 World Systems

| System | Information | Warning | Error | Debug | Verbose |
|--------|-------------|---------|-------|-------|---------|
| **Room Navigation** | Room entered | Missing room data | Load failure | Transition details | — |
| **Object Interaction** | Puzzle solved | — | Interaction fail | State changes | — |
| **Loot Generation** | — | — | Generation fail | Items generated | Roll details |

### 2.5 Crafting Systems

| System | Information | Warning | Error | Debug | Verbose |
|--------|-------------|---------|-------|-------|---------|
| **Field Medicine** | Treatment complete | — | Treatment fail | Healing applied | — |
| **Runeforging** | Rune created | Blight exposure | Inscription fail | Quality rolls | — |
| **Alchemy** | Potion brewed | — | Brewing fail | Ingredient consumption | — |
| **Bodging** | Repair complete | Suboptimal result | Repair fail | Material usage | — |

---

## 3. Status Effects Logging

All status effects follow a standard pattern:

| Event | Level | Template |
|-------|-------|----------|
| Applied | Debug | "{EffectName} applied to {CharacterId}, {StackCount} stacks, {Duration} duration" |
| Stack added | Verbose | "{EffectName} stack added to {CharacterId}, now {StackCount}" |
| Tick processed | Verbose | "{EffectName} tick on {CharacterId}: {Effect}" |
| Expired | Debug | "{EffectName} expired on {CharacterId}" |
| Cleansed | Debug | "{EffectName} cleansed from {CharacterId} via {Method}" |
| Resisted | Debug | "{CharacterId} resisted {EffectName} (DC {DC}, rolled {Roll})" |

### 3.1 Status Effect Specific Logging

| Effect | Additional Logging |
|--------|-------------------|
| **[Bleeding]** | Damage per tick, Soak bypass noted |
| **[Poisoned]** | Damage per tick, defense penalty |
| **[Burning]** | Spread events |
| **[Stunned]** | Actions prevented |
| **[Feared]** | Movement direction |

---

## 4. Specialization Logging

| Specialization | Key Log Events |
|----------------|----------------|
| **Berserkr** | Fury gained (Debug), Fury spent (Debug), WILL penalty active (Debug) |
| **Bone-Setter** | Treatment performed (Info), Healing applied (Debug) |
| **Ruin Stalker** | Trap detected (Debug), Corridor created (Info) |
| **Rúnasmiðr** | Rune inscribed (Info), Blight exposure (Warning) |

---

## 5. Log Properties by System

### 5.1 Always-Present Properties (via GameStateEnricher)

| Property | Type | Description |
|----------|------|-------------|
| `SaveGameId` | Guid | Current save game |
| `CharacterId` | Guid | Active character |
| `RoomId` | Guid | Current room |
| `TurnNumber` | long | Current turn |
| `GamePhase` | string | Current phase |

### 5.2 System-Specific Properties

| System | Property | Type |
|--------|----------|------|
| **Dice** | `PoolSize`, `NetSuccesses`, `Context` | int, int, string |
| **Combat** | `CombatId`, `RoundNumber`, `ActorId` | Guid, int, Guid |
| **Status Effects** | `EffectName`, `StackCount`, `Duration` | string, int, int |
| **Persistence** | `SlotNumber`, `DurationMs` | int, long |
| **Resources** | `ResourceType`, `PreviousValue`, `NewValue` | string, int, int |

---

## 6. Production Log Filtering

### 6.1 Recommended Minimum Levels

| Sink | Minimum Level | Rationale |
|------|---------------|-----------|
| Console (TUI) | Warning | Avoid clutter during gameplay |
| Console (Dev) | Debug | Full visibility for development |
| File (Production) | Information | Capture significant events |
| File (Debug) | Debug | Detailed troubleshooting |
| Remote (Seq) | Warning | Minimize network traffic |

### 6.2 Per-Namespace Overrides

```csharp
.MinimumLevel.Override("RuneAndRust.Engine.Dice", LogEventLevel.Verbose)
.MinimumLevel.Override("RuneAndRust.Engine.Combat", LogEventLevel.Debug)
.MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
```

---

## 7. Troubleshooting Common Scenarios

| Scenario | Enable Logging For | Level |
|----------|-------------------|-------|
| Combat not resolving correctly | Combat, Dice, StatusEffects | Debug |
| Save corruption | Persistence | Debug |
| Performance issues | GameLoop | Verbose |
| Status effect not applying | StatusEffects | Debug |
| Dice results seem wrong | Dice | Verbose |
| Room not loading | World, Persistence | Debug |

---

## 8. Implementation Status

| System | Logging Documented | Logging Implemented |
|--------|-------------------|---------------------|
| Dice System | ✅ | ❌ Planned |
| Game Loop | ❌ | ❌ Planned |
| Persistence | ❌ | ❌ Planned |
| Events | ❌ | ❌ Planned |
| Status Effects | ✅ (Bleeding) | ❌ Planned |
| Combat | ❌ | ❌ Planned |
| Character Creation | ✅ | ❌ Planned |
| Death/Resurrection | ✅ | ❌ Planned |

---

## 9. Changelog

| Version | Date | Changes |
|---------|------|---------|
| 1.0 | 2025-12-11 | Initial matrix with core systems, status effects, and troubleshooting guide |
