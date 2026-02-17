---
id: SPEC-AI-001
title: "Enemy AI System"
version: 1.0.0
status: Draft
last-updated: 2025-12-10
related-files:
  - path: "RuneAndRust.Engine/EnemyAI.cs"
    status: Exists
  - path: "RuneAndRust.Core/Enemy.cs"
    status: Exists
---

# Enemy AI System

> "The machine does not think as we do. It follows the paths etched into its rusted mind—loops of violence, defense, and calculated cruelty."

---

## 1. Overview

### 1.1 Identity Table

| Property | Value |
|----------|-------|
| Spec ID | `SPEC-AI-001` |
| Category | AI, Combat |
| Priority | High |
| Status | Draft |
| Domain | Artificial Intelligence, Enemy Design |

### 1.2 Core Philosophy

The **Enemy AI System** drives the tactical behavior of all non-player entities. It is designed to be **predictable enough to strategize against**, but **varied enough to prevent trivialization**. It uses a probability-based decision tree system modified by Enemy Archetypes and Traits.

**Design Pillars:**
1.  **Archetype-Driven Logic**: An enemy's role (Tank, DPS, Caster) dictates its primary behavior pattern.
2.  **Probability-Based Decision Making**: Actions are chosen via weighted rolls (e.g., 70% Attack, 30% Defend) rather than rigid scripts, preventing perfect predictability.
3.  **Trait Overrides**: Specific traits (e.g., `Cowardly`, `Berserk`) can override standard logic.
4.  **Phase Transitions**: Bosses and elite enemies shift behavior patterns based on HP thresholds.

---

## 2. AI Architecture

### 2.1 Decision Flow

```mermaid
flowchart TD
    A[Start Turn] --> B{Is Stunned?}
    B -->|Yes| C[Skip Turn]
    B -->|No| D{Has Override Trait?}
    D -->|Yes (Berserk/Flee)| E[Execute Trait Action]
    D -->|No| F[Determine Archetype Pattern]
    F --> G[Roll Probability (d100)]
    G --> H[Select Action]
    H --> I[Execute Action]
```

### 2.2 Decision Context

The AI considers the following inputs:
-   **Self State**: HP %, Active Status Effects, Traits.
-   **Target State**: Distance, Health, Status (Stunned, Bleeding).
-   **Environment**: Positioning, Allies nearby (for Swarm/Pack tactics).

---

## 3. Behavior Patterns

### 3.1 Aggressive Pattern (DPS, Glass Cannon, Swarm)
*Focus: Maximizing damage output.*

| Roll (d100) | Action | Context |
|-------------|--------|---------|
| **00-79** | Basic Attack / Skill | Attack closest or highest threat target. |
| **80-89** | Heavy Strike | High damage, lower accuracy (if available). |
| **90-99** | Utility / Reposition | Move to flank or use minor buff. |

*Examples: Corrupted Servitor, Scrap-Hound.*

### 3.2 Defensive Pattern (Tank)
*Focus: Survival, body-blocking, protecting allies.*

| Roll (d100) | Action | Context |
|-------------|--------|---------|
| **00-39** | Basic Attack | Standard attack to maintain threat. |
| **40-69** | Defensive Stance | Raise Defense or Soak. |
| **70-89** | Guard / Intercept | Move to protect ally or body block. |
| **90-99** | Self-Repair | Heal if HP < 50%. |

*Examples: Vault Custodian, Omega Sentinel (Phase 1).*

### 3.3 Tactical Pattern (Support, Caster)
*Focus: Buffs, debuffs, ranged control.*

| Roll (d100) | Action | Context |
|-------------|--------|---------|
| **00-39** | Ranged Attack | Magic/Psychic attack. |
| **40-69** | Buff Ally | Apply +Damage or Heal to ally. |
| **70-89** | Debuff Player | Apply Slow, Stun, or Stress. |
| **90-99** | Flee / Kite | Move away from melee threat. |

*Examples: Corrupted Engineer, Forlorn Scholar.*

### 3.4 Phase-Based Pattern (Bosses)
*Focus: Escalating difficulty based on HP.*

-   **Phase 1 (> 50% HP)**: Standard rotation (often Defensive or Testing).
-   **Phase 2 (< 50% HP)**: Aggressive/AoE focus. New abilities unlocked.
-   **Phase 3 (< 25% HP)**: Desperation. Ultimate attacks, high lethality.

*Example: Ruin-Warden.*

---

## 4. Trait Integration & Overrides

Traits from `SPEC-COMBAT-015` can modify or completely override standard AI logic.

| Trait | Effect on AI | Priority |
|-------|--------------|----------|
| **Cowardly** | If HP < 25%, force **Flee** action. | High (Override) |
| **Berserk** | If HP < 25%, attack **Random Target** (Friend/Foe). | High (Override) |
| **HealerHunter** | Target priority logic shifts to **Lowest HP** or **Support** units. | Targeting Modifier |
| **PackTactics** | Move logic prioritizes **Adjacency** to allies. | Movement Modifier |
| **HitAndFade** | After attack, move to **Max Range**. | Post-Action Modifier |
| **SelfDestructive**| If HP < 10%, move to Player and **Explode**. | High (Override) |

---

## 5. Target Prioritization

Default targeting logic uses a weighted "Threat Score":

1.  **Distance**: Closest target (Base weight).
2.  **Damage Dealt**: Who hurt me last turn? (+Threat).
3.  **Vulnerability**: Is target Stunned/Low HP? (+Threat).

**Modifiers:**
-   **Tank**: Prioritizes whoever has highest Aggro.
-   **Assassin**: Prioritizes Lowest HP or Highest WITS (Healers).
-   **Random**: Ignores logic (e.g., Confused status).

---

## 6. Voice Guidance

### 6.1 System Tone

**Layer 2 Diagnostic Voice**
The system analyzes enemy behavior patterns as "logic subroutines" or "feral instincts." It speaks with clinical detachment, identifying tactical intents.

### 6.2 Feedback Text Examples

| Event | Text |
|-------|------|
| **Aggressive AI** | "Logic core set to: EXTERMINATE. Hostile is committing fully to offense." |
| **Defensive AI** | "Target has engaged defensive protocols. Armor integrity reinforced." |
| **Fleeing** | "Morale failure detected. Hostile entity attempting retreat." |
| **Berserk** | "Error: Logic processing failed. Target is frenzied. Unpredictable behavior imminent." |
| **Tactical Flank** | "Warning: Pack tactics engaged. They are coordinating... clever machines." |

---

## 7. Technical Implementation

### 7.1 Data Model

```csharp
public enum EnemyPattern { Aggressive, Defensive, Tactical, PhaseBased }

public class EnemyAIProfile
{
    public EnemyPattern Pattern { get; set; }
    public List<Trait> Traits { get; set; }
    public Dictionary<EnemyAction, int> ActionWeights { get; set; }
}
```

### 7.2 Service Interface

```csharp
public interface IEnemyAIService
{
    EnemyAction DetermineAction(Enemy enemy, CombatState state);
    
    // Internal Logic
    bool CheckTraitOverrides(Enemy enemy, out EnemyAction action);
    EnemyPattern DetermineEffectPattern(Enemy enemy);
    EnemyAction SelectWeightedAction(Dictionary<EnemyAction, int> weights);
    CombatParticipant SelectTarget(Enemy enemy, CombatState state);
}
```

---

## 8. Phased Implementation Guide

### Phase 1: Core Logic
- [ ] **Data Model**: Implement `EnemyAIProfile` and `EnemyPattern`.
- [ ] **Service**: Implement `DetermineAction` stub and randomness generator.
- [ ] **Targets**: Implement basic "Closest Target" selection.

### Phase 2: Patterns
- [ ] **Aggressive**: Implement weighted table (80% Attack / 20% Skill).
- [ ] **Defensive**: Implement HP check logic (If Low -> Heal/Guard).
- [ ] **Tactical**: Implement Support/Debuff selection logic.

### Phase 3: Traits & Phases
- [ ] **Traits**: Implement `Cowardly` and `Berserk` overrides.
- [ ] **Phases**: Implement Boss Phase Transition triggers.
- [ ] **Complex Targeting**: Implement `HealerHunter` and `Aggro` tables.

### Phase 4: UI & Feedback
- [ ] **Feedback**: "Enemy looks angry!" flavor text on Berserk.
- [ ] **Log**: "Enemy is thinking..." debug logs for GM/Player insight.

---

## 9. Testing Requirements

### 9.1 Unit Tests
- [ ] **Distribution**: Run 1000 Aggressive iterations -> Verify ~800 Basic Attacks.
- [ ] **Override**: Set HP=10% + Cowardly -> Verify Action == Flee.
- [ ] **Targeting**: Healer at Range 5 vs Tank at Range 1 -> HealerHunter selects Healer.
- [ ] **Phase**: Boss HP < 50% -> Pattern switches to Phase 2.

### 9.2 Integration Tests
- [ ] **Combat**: Enemy A takes turn -> Selects Action -> Executes Action -> Valid State.
- [ ] **Berserk**: Enemy hits ally when Berserk -> Damage registered.
- [ ] **Stun**: Stunned Enemy -> Skips Turn immediately.

### 9.3 Manual QA
- [ ] **Log**: Check Combat Log for "Enemy decided to [Action]" messages if Debug enabled.

---

## 10. Logging Requirements

**Reference:** [logging.md](../logging.md)

### 10.1 Log Events

| Event | Level | Message Template | Properties |
|-------|-------|------------------|------------|
| AI Thinking | Debug | "{Enemy} evaluating. Pattern: {Pattern}. HP: {HP}%." | `Enemy`, `Pattern`, `HP` |
| AI Decision | Info | "{Enemy} decided to {Action} vs {Target} (Roll: {Roll})." | `Enemy`, `Action`, `Target`, `Roll` |
| Trait Trigger | Info | "{Enemy} trait {Trait} overrode logic -> {Action}." | `Enemy`, `Trait`, `Action` |

---

## 11. Related Specifications

| Spec ID | Relationship |
|---------|--------------|
| `SPEC-COMBAT-012` | Enemy Design System (Defines Archetypes) |
| `SPEC-COMBAT-015` | Creature Traits System (Defines Traits) |
| `SPEC-COMBAT-005` | Boss Encounter System |

---

## 12. Changelog

| Version | Date | Changes |
|---------|------|---------|
| 1.0.0 | 2025-12-10 | Initial specification synthesized from Enemy Design & Traits |
