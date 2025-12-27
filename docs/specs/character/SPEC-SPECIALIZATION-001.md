---
id: SPEC-SPECIALIZATION-001
title: Specialization & Ability Tree System
version: 1.0.0
status: Draft
last_updated: 2025-01-20
related_specs: [SPEC-XP-001, SPEC-ABILITY-001, SPEC-CHAR-001]
---

# SPEC-SPECIALIZATION-001: Specialization & Ability Tree System

> **Version:** 1.0.0
> **Status:** Draft
> **Service:** `SpecializationService` (planned)
> **Location:** `RuneAndRust.Engine/Services/`

---

## 1. Overview

**Goal:** Implement a deep specialization system where players spend Progression Points (PP) to unlock abilities within distinct trees. This replaces the linear "unlock by level" model with a choice-driven system.

**Key Features:**
- **Specializations:** Distinct from Archetypes (e.g., a Warrior can specialize as a *Berserkr* or *Guardian*).
- **Ability Trees:** Visual and logical structures of dependent abilities (Tier 1 → Tier 2 → Tier 3 → Capstone).
- **Progression Points (PP):** A new currency earned via Saga/XP milestones, used to purchase nodes.
- **Tree Visualization:** A TUI screen to view and interact with the tree.

---

## 2. Core Concepts

### 2.1 Specialization
A **Specialization** is a subclass or focused path available to specific Archetypes.
- **Id:** Unique identifier (e.g., `spec_berserkr`).
- **Name:** Display name (e.g., "Berserkr").
- **ArchetypeRequirement:** The base archetype required (e.g., Warrior).
- **Description:** Flavor text.
- **Tree:** The collection of unlockable nodes.

### 2.2 Ability Tree & Nodes
Each specialization has a **Tree** composed of **Nodes**.
- **NodeId:** Unique ID within the tree.
- **AbilityId:** Reference to the `ActiveAbility` granted.
- **Tier:** The vertical depth (1, 2, 3, 4).
- **CostPP:** Cost in Progression Points (Standard: T1=1, T2=2, T3=3, Capstone=5).
- **Prerequisites:** List of Parent Node IDs that must be unlocked first.
- **Position:** (Row, Col) for UI rendering.

### 2.3 Unlocking Logic
- To unlock a Node:
    1. The player must have the Specialization unlocked (or auto-unlocked by Archetype).
    2. The player must have sufficient **PP**.
    3. The player must have unlocked all **Prerequisite Nodes**.
    4. The player must meet any level requirements (optional, but Tier implies level correlation).

---

## 3. Data Models

### 3.1 Specialization Definition
```csharp
public class SpecializationDefinition
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public ArchetypeType RequiredArchetype { get; set; }
    public List<SpecializationNode> Nodes { get; set; } = new();
}
```

### 3.2 Specialization Node
```csharp
public class SpecializationNode
{
    public string Id { get; set; } // e.g., "berserkr_rage"
    public Guid AbilityId { get; set; }
    public int Tier { get; set; } // 1, 2, 3, 4
    public int CostPP { get; set; }
    public List<string> PrerequisiteNodeIds { get; set; } = new();

    // UI Layout
    public int GridRow { get; set; }
    public int GridColumn { get; set; }
}
```

### 3.3 Character State Extensions
The `Character` entity needs to track progress.
```csharp
public class Character
{
    // ... existing properties
    public int ProgressionPoints { get; set; }
    public List<string> UnlockedSpecializationIds { get; set; } = new();
    public List<string> UnlockedAbilityNodeIds { get; set; } = new();
}
```

---

## 4. Behaviors & Service Logic

### 4.1 SpecializationService
`ISpecializationService` handles the business logic.

#### `GetAvailableSpecializations(Character character)`
Returns specializations matching the character's Archetype.

#### `CanUnlockNode(Character character, SpecializationDefinition spec, string nodeId)`
Checks:
- `character.ProgressionPoints >= node.CostPP`
- `node.PrerequisiteNodeIds` are all in `character.UnlockedAbilityNodeIds`
- `nodeId` is not already unlocked.

#### `UnlockNode(Character character, SpecializationDefinition spec, string nodeId)`
- Deducts PP.
- Adds `nodeId` to `UnlockedAbilityNodeIds`.
- Adds the associated `ActiveAbility` to the character's active ability list (or makes it available for loadout).
- Persists changes.

### 4.2 Integration with Combat
`CombatService` needs to load abilities from the specialization tree.
- Currently, it loads abilities by Archetype + Tier.
- **Update:** It must also load abilities where `AbilityId` corresponds to an unlocked node in `UnlockedAbilityNodeIds`.

---

## 5. UI Design (The Grid)

The UI will be a TUI screen, likely a new tab in the Character Menu or a specific "Saga" station interaction.

**Layout:**
```
[ Specialization: Berserkr ]  [ PP: 2 ]

   [X] Rage (T1)  -->  [ ] Cleave (T2)
        |
        +--> [ ] Ignore Pain (T2) --> [ ] Undying (T3)

[X] = Unlocked
[ ] = Available (Affordable)
[#] = Locked (Prereqs missing)
```

**Controls:**
- Arrow keys to navigate nodes.
- `Enter` to purchase.
- `Esc` to exit.

---

## 6. Seeding Content (Initial Pass)

### 6.1 Berserkr (Warrior)
- **T1:** `Rage` (Buff: +Dmg, -Def)
- **T2:** `Cleave` (AoE Attack), `Ignore Pain` (Temp HP)
- **T3:** `Bloodthirst` (Heal on Hit)
- **Capstone:** `Undying` (Prevent death 1/combat)

### 6.2 Skald (Investigator/Support)
- **T1:** `Battle Cry` (Buff Ally Dmg)
- **T2:** `Demoralize` (Debuff Enemy Hit), `Inpsire` (Heal Stamina)
- **T3:** `Echoing Shout` (AoE Stun)
- **Capstone:** `Heroic Saga` (Massive Buff)

---

## 7. Risks & Constraints

- **Dependency Loops:** Data validation must ensure no circular dependencies in prerequisites.
- **Respec:** Respec logic (refunding PP) should be considered for future versions (`v0.4.x` polish?).
- **Display Space:** TUI grids are limited. Keep trees relatively simple (max width 3-4, max depth 4).
