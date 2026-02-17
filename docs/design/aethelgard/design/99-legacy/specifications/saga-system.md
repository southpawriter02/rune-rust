# Saga System — Core System Specification v5.0

Type: Core System
Priority: Must-Have
Status: Proposed
Balance Validated: No
Document ID: AAM-SPEC-CORESYSTEM-SAGA-v5.0
Proof-of-Concept Flag: No
Sub-Type: Core
Sub-item: Archetype & Specialization System — Mechanic Specification v5.0 (Archetype%20&%20Specialization%20System%20%E2%80%94%20Mechanic%20Speci%2064145bff162b4bfa8df76a982f572f18.md), Ability Rank Advancement System — Mechanic Specification v5.0 (Ability%20Rank%20Advancement%20System%20%E2%80%94%20Mechanic%20Specifi%20f63ba4f1354e484a80081c4a6b118160.md)
Template Compliance: v5.0 Three-Tier Template
Template Validated: No
Voice Validated: No

## I. Core Philosophy: The Living Chronicle

The Saga System is the mechanical and narrative framework that governs all character progression in Aethelgard. Its core philosophy is that a character's growth is not a simple accumulation of power, but the **writing of a living chronicle**. It is the process of transforming the raw, chaotic data of experience into a coherent, legendary saga.

This system is designed to move beyond traditional "leveling up." It is an in-world, diegetic system that tracks a character's accomplishments, rewards them for their deeds, and provides a flexible, player-driven path for evolving their capabilities.

**Design Pillars:**

- **Narrative Weight:** Experience is not abstract—it represents saga-worthy deeds
- **Deliberate Growth:** Progression happens through reflection, not constant incremental gain
- **Player Agency:** Characters evolve through player choices, not automatic leveling
- **Thematic Integration:** Sanctuary Rest requirement reinforces coherence vs. chaos theme

---

## II. System Components

### **Component 1: Legend System (Passive Accumulation)**

- Tracks saga-worthy accomplishments and converts them to progression potential
- Passive resource (cannot be spent directly)
- Automatically fills a progression bar as character performs worthy deeds

### **Component 2: Progression Points (PP) System (Active Currency)**

- Spendable currency for purchasing character upgrades
- Awarded in discrete chunks when Legend bar fills (Milestone events)

### **Component 3: Saga Menu (UI Interface)**

- Player interface for spending Progression Points
- **Branches:** Attributes, Skills, Specializations

### **The Two-Currency Model**

- **Earning (Legend):** Continuous during gameplay, scaled by deed significance
- **Spending (PP):** Discrete milestone events, requires Sanctuary Rest

---

## III. Calculation Formulas

### **Milestone Progression Formula**

```jsx
LegendToNextMilestone = (CurrentMilestoneLevel × 100) + 500
```

**Value Ranges:**

- Milestone 0→1: 500 Legend required
- Milestone 5→6: 1,000 Legend required
- Milestone 10→11: 1,500 Legend required
- Milestone 20→21: 2,500 Legend required

**Design Intent:**

- Early game feels fast (500 Legend for first milestone)
- Mid game smoothly increases effort
- Late game becomes long-term goal

---

## IV. Integration Points

### **Primary Dependencies:**

- **Legend System** — Monitors LegendPoints threshold, triggers Milestones
- **Sanctuary Rest System** — Enforces PP spending restriction ("Golden Rule")
- **Attributes System** — Saga Menu writes attribute increases
- **Skill System** — Saga Menu writes skill rank increases
- **Specialization System** — Saga Menu manages specialization unlocks

### **Referenced By:**

- All Specialization Specs (207 abilities with PP costs)
- Character Creation System
- UI Systems

### **Modifies:**

- ProgressionPoints, CurrentMilestoneLevel
- Attribute Values, Skill Ranks, Specialization State

---

## V. Database Schema Requirements

### **Characters Table (Progression Fields)**

```sql
ALTER TABLE Characters ADD COLUMN (
  legend_points INT NOT NULL DEFAULT 0,
  progression_points INT NOT NULL DEFAULT 0,
  current_milestone_level INT NOT NULL DEFAULT 0,
  legend_to_next_milestone INT NOT NULL DEFAULT 500
);
```

### **Saga_Upgrades Table (Transaction History)**

```sql
CREATE TABLE Saga_Upgrades (
  upgrade_id INT PRIMARY KEY AUTO_INCREMENT,
  character_id INT NOT NULL,
  upgrade_type ENUM('attribute', 'skill', 'specialization', 'ability') NOT NULL,
  upgrade_target VARCHAR(100) NOT NULL,
  pp_cost INT NOT NULL,
  milestone_level_at_purchase INT NOT NULL,
  purchased_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
  
  FOREIGN KEY (character_id) REFERENCES Characters(character_id)
    ON DELETE CASCADE
);
```

---

## VI. Service Integration Architecture

### **Core Service: SagaService**

**Key Methods:**

- `AddLegend(character, amount)` — Adds Legend, checks Milestone threshold
- `TriggerMilestone(character)` — Awards PP, recalculates threshold
- `SpendProgressionPoint(character, upgradeTarget)` — Validates and executes PP spending
- `CalculateNextMilestoneThreshold(currentLevel)` — Formula: (N × 100) + 500

---

## VII. UI Requirements

### **Legend Progress Display**

```
━━━━━━━━━━━░░░░░░░░░░ 750 / 1,500 Legend
Milestone 10
```

### **Saga Menu Interface**

- **Tab 1: Attributes** — Core attribute increases
- **Tab 2: Skills** — Non-combat skill progression
- **Tab 3: Specializations** — Ability tree navigation

---

## VIII. Design Philosophy

### **Why the Saga System Exists**

- **Coherence vs. Chaos:** Diegetic system tied to narrative
- **Player Agency:** Every upgrade is deliberate choice
- **Narrative Weight:** Only saga-worthy deeds grant Legend

### **Why Two Currencies?**

- Preserves "milestone moment" feeling
- Provides granular spending control

### **Why Sanctuary Rest Restriction?**

- Narrative: Mind must be coherent to reflect
- Gameplay: Creates natural pacing, rewards preparation

---

## IX. Implementation Notes

### **Critical Requirements**

1. **Atomic Transactions:** Milestone events must update all fields atomically
2. **Legend Overflow Handling:** Preserve excess Legend after milestone
3. **Multiple Milestone Triggers:** Loop until legend_points < threshold
4. **Sanctuary Rest Validation:** Re-validate on every PP spend attempt

---

*This specification has been fully migrated from v2.0 Saga System specification.*