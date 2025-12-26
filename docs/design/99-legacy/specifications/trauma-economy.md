# Trauma Economy System — Core System Specification v5.0

Type: Core System
Priority: Should-Have
Status: Review
Balance Validated: No
Document ID: AAM-SPEC-CORESYSTEM-TRAUMAECONOMY-v5.0
Proof-of-Concept Flag: No
Resource System: Corruption/Psychic Stress
Session Handoffs: Consolidation Work — Phase 1A Core Systems Audit Complete (https://www.notion.so/Consolidation-Work-Phase-1A-Core-Systems-Audit-Complete-0c51f0058f3a478fb7bc6a2c192cac2a?pvs=21)
Sub-Type: Core
Sub-item: Psychic Stress System — Mechanic Specification v5.0 (Psychic%20Stress%20System%20%E2%80%94%20Mechanic%20Specification%20v5%20%209e3a6f448b7940d295fb021d97b274fb.md), Runic Blight Corruption System — Mechanic Specification v5.0 (Runic%20Blight%20Corruption%20System%20%E2%80%94%20Mechanic%20Specific%2076c14a2cdec64a759c0e843cd08662b2.md)
Template Compliance: v5.0 Three-Tier Template
Template Validated: No
Voice Layer: Layer 2 (Diagnostic)
Voice Validated: No

## **I. Core Philosophy: The Price of Knowing**

<aside>
🔧

**IMPLEMENTATION GAPS — Code Needs Update (Resolved 2025-11-29)**

The following discrepancies were reviewed and confirmed — DB01-GS spec is correct, code needs to be updated:

| Mechanic | Spec (Correct) | Current Code | Resolution |
| --- | --- | --- | --- |
| **Dice System** | d10 with DC | d6 (5-6 = success) | ✅ Update code to d10 |
| **Breaking Point Reset** | Success=75%, Failure=50% | Fixed at 60 | ✅ Update code to variable reset |
| **Corruption Recovery** | Rare removal methods | No recovery | ✅ Add minimal recovery (see below) |

**Corruption Recovery Design Intent:** Recovery methods exist but are intentionally punishing — reduce Corruption by only 1-3 points, available only once or twice total. Designed to be barely worth pursuing.

</aside>

The Trauma Economy is a foundational design pillar of Aethelgard, built upon a single, grim principle: **in a broken world, the most profound forms of power and knowledge are inextricably linked to the world's psychic wound, and accessing them requires a direct, quantifiable sacrifice of a character's sanity and spiritual integrity.**

It is not merely a "sanity system"; it is an active, player-driven economy where the currencies are **Psychic Stress** and **Runic Blight Corruption**, and the goods are forbidden secrets, heretical power, and the unnatural advantages needed to survive.

**Design Pillars:**

- **Diegetic Horror:** Mechanizes the lore of the Great Silence into gameplay
- **Strategic Resource Management:** Trauma currencies are active, spendable resources
- **Two-Currency Depth:** Psychic Stress (volatile, recoverable) and Corruption (insidious, permanent)
- **Ludonarrative Harmony:** UI distortion and reality glitches translate broken reality into player experience
- **Specialization Identity:** Different builds have fundamentally different relationships with trauma

---

## **II. System Components**

### **Core Subsystems**

**1. Psychic Stress System**

- Volatile, short-term mental strain from Blight exposure
- Tracks accumulation from environmental sources, combat, voluntary trauma
- Drives UI distortion mechanics and Breaking Point thresholds

**2. Runic Blight Corruption System**

- Insidious, long-term infection of character's "source code"
- Tracks accumulation from Mystic spellcasting, heretical abilities, artifacts
- Drives Reality Glitch mechanics and Terminal Error conditions

**3. Trauma System**

- Permanent psychological scars from Breaking Point failures
- Catalogs Trauma types and their mechanical effects

**4. Resolve Check System**

- Universal resistance mechanic for mental fortitude tests
- WILL-based dice pool checks against variable DCs

---

## **III. Calculation Formulas**

### **The Two Currencies**

**Psychic Stress Range: 0-100%**

- 0-25%: Baseline discomfort
- 26-50%: Moderate distress, minor UI distortion
- 51-75%: Severe mental strain, major UI distortion
- 76-99%: Critical condition
- 100%: Breaking Point

**Runic Blight Corruption Range: 0-100**

- 0-20: Minimal infection
- 21-40: Low Corruption, occasional Glitches
- 41-60: Moderate Corruption, frequent Glitches
- 61-80: High Corruption, constant Glitch risk + Wild Magic (Mystics)
- 81-99: Extreme Corruption
- 100: Terminal Error — Transformation risk

### **Currency Interaction**

```jsx
Psychic Stress Resistance Penalty = -1 die per 20 Corruption
```

### **Breaking Point**

```jsx
Trigger = Psychic Stress >= 100%

Success: Stress resets to 75%, gain [Disoriented] for 2 turns
Failure: Stress resets to 50%, gain [Stunned] for 1 turn, gain permanent Trauma
```

### **Terminal Error**

```jsx
Transformation Probability = Base 50% + (Corruption Over 100 × 5%)
```

---

## **IV. Integration Points**

**Primary Dependencies:**

- **Attributes System** — WILL determines base resistance
- **Dice Pool System** — Resolve Checks use d10 success-counting
- **Combat System** — Abilities inflict Stress as cost
- **Status Effect System** — [Disoriented], [Stunned], [Feared]

**Referenced By:**

- All Mystic Specializations (spells generate Corruption)
- All Heretical Specializations (abilities cost Stress/Corruption)
- Environmental Systems ([Psychic Resonance] zones)
- Equipment System ([Glitched Artifacts])

---

## **V. Database Schema Requirements**

### **PsychicStressState Table**

```sql
CREATE TABLE PsychicStressState (
  character_id INT PRIMARY KEY,
  current_stress INT NOT NULL DEFAULT 0,
  distortion_tier INT NOT NULL DEFAULT 0,
  breaking_point_count INT NOT NULL DEFAULT 0,
  
  CHECK (current_stress >= 0 AND current_stress <= 100),
  CHECK (distortion_tier >= 0 AND distortion_tier <= 4)
);
```

### **CorruptionState Table**

```sql
CREATE TABLE CorruptionState (
  character_id INT PRIMARY KEY,
  current_corruption INT NOT NULL DEFAULT 0,
  glitch_tier INT NOT NULL DEFAULT 0,
  wild_magic_enabled BOOLEAN NOT NULL DEFAULT FALSE,
  
  CHECK (current_corruption >= 0 AND current_corruption <= 100)
);
```

### **CharacterTraumas Table**

```sql
CREATE TABLE CharacterTraumas (
  trauma_instance_id INT PRIMARY KEY AUTO_INCREMENT,
  character_id INT NOT NULL,
  trauma_type VARCHAR(100) NOT NULL,
  acquisition_timestamp TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
  source_description TEXT,
  suppressed_until TIMESTAMP NULL
);
```

---

## **VI. Service Integration Architecture**

### **Core Services**

**TraumaEconomyService**

- `AddPsychicStress(characterID, amount, source)`
- `AddCorruption(characterID, amount, source)`
- `TriggerBreakingPoint(characterID)`
- `TriggerTerminalError(characterID)`
- `GetUIDistortionTier(characterID)`

---

## **VII. UI/Feedback Requirements**

### **UI Distortion Tiers**

- **Tier 0 (0-25%):** Normal text
- **Tier 1 (26-50%):** Leetspeak substitutions (e→3, o→0)
- **Tier 2 (51-75%):** Unicode intrusion, formatting errors
- **Tier 3 (76-99%):** Heavy static, data log intrusions
- **Tier 4 (100%):** Complete breakdown, Breaking Point notification

### **Feedback Events**

- **Stress Gain:** "+15 Psychic Stress (Psychic Lash ability)"
- **Corruption Gain:** "+5 Corruption (Hagalaz's Storm spell)"
- **Breaking Point:** Full-screen modal with Resolve Check
- **Trauma Acquired:** Trauma card display

---

## **VIII. Design Philosophy**

### **The Marketplace Metaphor**

The Trauma Economy is framed as an **economy** to emphasize player agency:

- **Purchasing Power:** Heretical abilities trade sanity for power
- **Purchasing Knowledge:** Forbidden lore locked behind trauma gates
- **Purchasing Advantage:** Glitched Artifacts provide power at Corruption cost

### **The Two-Currency Design**

- **Strategic Depth:** Managing both volatile (Stress) and permanent (Corruption) creates layered decisions
- **Archetype Differentiation:** Heretical warriors pay Stress, Mystics pay Corruption
- **Recovery Asymmetry:** Stress recovers at rest, Corruption requires rare reagents

### **Balance Considerations**

- **Legend Incentive:** Trauma Modifier rewards risky behavior
- **Mystic's Burden:** Corruption spiral creates urgency
- **Coherent Advantage:** Resistance to trauma, but slower progression

---

*Migration Complete. All 10 sections migrated from v2.0 source material to v5.0 Three-Tier Template format.*