# Rune & Rust - Core System Specifications

> **Version:** 0.3.3c
> **Last Updated:** 2025-12-21
> **Maintained By:** The Architect

This document serves as the master index for all game system specifications. Each specification provides comprehensive documentation including behaviors, restrictions, limitations, use cases, and cross-system dependencies.

---

## System Specification Index

| Spec ID | System | Document | Primary Service(s) |
|---------|--------|----------|-------------------|
| SPEC-DICE-001 | Dice Pool System | [SPEC-DICE-001.md](./SPEC-DICE-001.md) | `DiceService` |
| SPEC-COMBAT-001 | Combat System | [SPEC-COMBAT-001.md](./SPEC-COMBAT-001.md) | `CombatService`, `AttackResolutionService`, `InitiativeService` |
| SPEC-ABILITY-001 | Ability System | [SPEC-ABILITY-001.md](./SPEC-ABILITY-001.md) | `AbilityService`, `EffectScriptExecutor` |
| SPEC-CHAR-001 | Character & Progression | [SPEC-CHAR-001.md](./SPEC-CHAR-001.md) | `StatCalculationService`, `CharacterFactory` |
| SPEC-TRAUMA-001 | Trauma & Stress System | [SPEC-TRAUMA-001.md](./SPEC-TRAUMA-001.md) | `TraumaService`, `TraumaRegistry` |
| SPEC-CORRUPT-001 | Corruption System | [SPEC-CORRUPT-001.md](./SPEC-CORRUPT-001.md) | `TraumaService` |
| SPEC-REST-001 | Rest & Recovery System | [SPEC-REST-001.md](./SPEC-REST-001.md) | `RestService`, `AmbushService` |
| SPEC-INV-001 | Inventory & Equipment | [SPEC-INV-001.md](./SPEC-INV-001.md) | `InventoryService`, `LootService` |
| SPEC-HAZARD-001 | Dynamic Hazard System | [SPEC-HAZARD-001.md](./SPEC-HAZARD-001.md) | `HazardService`, `EffectScriptExecutor` |
| SPEC-COND-001 | Ambient Condition System | [SPEC-COND-001.md](./SPEC-COND-001.md) | `ConditionService` |
| SPEC-STATUS-001 | Status Effect System | [SPEC-STATUS-001.md](./SPEC-STATUS-001.md) | `StatusEffectService` |

---

## Architecture Overview

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                              TERMINAL LAYER                                  │
│  ┌─────────────────┐  ┌───────────────────┐  ┌─────────────────────────┐   │
│  │ CombatRenderer  │  │ RestScreenRenderer │  │ CharacterCreationCtrl   │   │
│  └────────┬────────┘  └─────────┬─────────┘  └───────────┬─────────────┘   │
└───────────┼─────────────────────┼────────────────────────┼─────────────────┘
            │                     │                        │
┌───────────┼─────────────────────┼────────────────────────┼─────────────────┐
│           ▼                     ▼                        ▼   ENGINE LAYER  │
│  ┌────────────────────────────────────────────────────────────────────┐    │
│  │                          GameService                                │    │
│  │  (Main Orchestrator - Manages GameState, Phase Transitions)         │    │
│  └────────────────────────────────────────────────────────────────────┘    │
│           │                     │                        │                  │
│  ┌────────┴──────┐     ┌───────┴───────┐      ┌─────────┴─────────┐       │
│  │ CombatService │     │  RestService  │      │  NavigationService │       │
│  └───────┬───────┘     └───────┬───────┘      └─────────┬─────────┘       │
│          │                     │                        │                  │
│  ┌───────┴───────────────────────────────────────────────────────────┐    │
│  │  SUPPORTING SERVICES                                               │    │
│  │  ┌───────────────┐ ┌─────────────┐ ┌─────────────┐ ┌────────────┐ │    │
│  │  │ TraumaService │ │ HazardSvc   │ │ ConditionSvc│ │ AbilitySvc │ │    │
│  │  └───────────────┘ └─────────────┘ └─────────────┘ └────────────┘ │    │
│  │  ┌───────────────┐ ┌─────────────┐ ┌─────────────┐ ┌────────────┐ │    │
│  │  │ InventoryService│ │ LootService │ │ AmbushSvc  │ │ ResourceSvc│ │    │
│  │  └───────────────┘ └─────────────┘ └─────────────┘ └────────────┘ │    │
│  └───────────────────────────────────────────────────────────────────┘    │
│                                    │                                        │
│  ┌─────────────────────────────────┴─────────────────────────────────┐    │
│  │  FOUNDATIONAL SERVICES                                             │    │
│  │  ┌───────────────┐ ┌─────────────────────┐ ┌───────────────────┐  │    │
│  │  │  DiceService  │ │ StatCalculationSvc  │ │ StatusEffectService│  │    │
│  │  └───────────────┘ └─────────────────────┘ └───────────────────┘  │    │
│  │  ┌───────────────────────────────────────────────────────────────┐│    │
│  │  │           EffectScriptExecutor (Shared Utility)               ││    │
│  │  └───────────────────────────────────────────────────────────────┘│    │
│  └───────────────────────────────────────────────────────────────────┘    │
└────────────────────────────────────────────────────────────────────────────┘
            │
┌───────────┼────────────────────────────────────────────────────────────────┐
│           ▼                      PERSISTENCE LAYER                          │
│  ┌─────────────────────────────────────────────────────────────────────┐   │
│  │  REPOSITORIES                                                        │   │
│  │  CharacterRepo │ RoomRepo │ ItemRepo │ InventoryRepo │ SaveGameRepo  │   │
│  └─────────────────────────────────────────────────────────────────────┘   │
│                                    │                                        │
│  ┌─────────────────────────────────┴───────────────────────────────────┐   │
│  │                     PostgreSQL 16 + EF Core 8.0                      │   │
│  └─────────────────────────────────────────────────────────────────────┘   │
└────────────────────────────────────────────────────────────────────────────┘
```

---

## Cross-System Dependency Matrix

| System | Depends On | Depended By |
|--------|-----------|-------------|
| **DiceService** | (none) | Combat, Trauma, Ability, Rest, Hazard, Condition |
| **StatCalculationService** | (none) | Character, Combat, Inventory |
| **StatusEffectService** | (none) | Combat, Ability, Condition |
| **TraumaService** | DiceService | Combat, Character |
| **CombatService** | DiceService, TraumaService, HazardService, ConditionService, AbilityService, StatusEffectService | GameService |
| **RestService** | InventoryService, AmbushService | GameService |
| **AmbushService** | DiceService | RestService |
| **InventoryService** | InventoryRepository | Rest, Combat, Character |
| **HazardService** | EffectScriptExecutor | Combat, Navigation |
| **ConditionService** | EffectScriptExecutor, DiceService | Combat |
| **AbilityService** | ResourceService, EffectScriptExecutor | Combat |

---

## Specification Document Format

Each specification follows this standard format:

```markdown
# SPEC-{SYSTEM}-{NUMBER}: {System Name}

## Overview
Brief description of the system's purpose.

## Behaviors
### Primary Behaviors
What the system does in normal operation.

### Edge Case Behaviors
How the system handles boundary conditions.

## Restrictions
Things the system explicitly cannot or should not do.

## Limitations
Known bounds, caps, or technical constraints.

## Use Cases
Common scenarios with code examples.

## Cross-Links
### Dependencies (Consumes)
Services this system relies on.

### Dependents (Provides To)
Services that rely on this system.

## Related Services
Primary service implementations with file paths.

## Data Models
Key entities and DTOs used by this system.

## Configuration
Configurable constants and settings.

## Testing
Test coverage and testing strategies.
```

---

## Quick Reference: Formulas

| Calculation | Formula | Source |
|-------------|---------|--------|
| Max HP | `50 + (Sturdiness × 10)` | StatCalculationService |
| Max Stamina | `20 + (Finesse × 5) + (Sturdiness × 3)` | StatCalculationService |
| Max Aether (Mystic) | `10 + (Will × 5)` | StatCalculationService |
| Action Points | `2 + (Wits ÷ 4)` | StatCalculationService |
| Defense Score | `10 + Finesse - (Stress ÷ 20)` | AttackResolutionService |
| Success Threshold | `Defense ÷ 5` | AttackResolutionService |
| Carry Capacity | `Might × 10,000g` | InventoryService |
| HP Recovery (Wilderness) | `10 + (Sturdiness × 2)` | RestService |
| Stress Recovery | `Will × 5` | RestService |
| Stress Defense Penalty | `Stress ÷ 20` (max 5) | TraumaService |

---

## Version History

| Version | Date | Changes |
|---------|------|---------|
| 0.3.3c | 2025-12-21 | Initial specification documentation |
| 0.3.3b | 2025-12-21 | Ambient Condition system added |
| 0.3.3a | 2025-12-21 | Dynamic Hazard system added |
| 0.3.2b | 2025-12-20 | Rest & Ambush system implemented |
| 0.3.0c | 2025-12-20 | Trauma system with Breaking Points |
