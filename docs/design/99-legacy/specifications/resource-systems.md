# Resource Systems — Core System Specification v5.0

Type: Core System
Priority: Should-Have
Status: Review
Balance Validated: No
Document ID: AAM-SPEC-CORESYSTEM-RESOURCES-v5.0
Proof-of-Concept Flag: No
Resource System: Aether Pool, Corruption/Psychic Stress, Stamina
Sub-Type: Core
Sub-item: Health Pool (HP) System — Mechanic Specification v5.0 (Health%20Pool%20(HP)%20System%20%E2%80%94%20Mechanic%20Specification%20v%209e6e3be6b01e4d0bbfcf60921e38ac1f.md), Stamina System — Mechanic Specification v5.0 (Stamina%20System%20%E2%80%94%20Mechanic%20Specification%20v5%200%20e4c7bf5ba8394857a3c307eaf7ce222a.md), Aether Pool (AP) System — Mechanic Specification v5.0 (Aether%20Pool%20(AP)%20System%20%E2%80%94%20Mechanic%20Specification%20v%2050326f0c62b548a2b1479dc09034b6e1.md)
Template Compliance: v5.0 Three-Tier Template
Template Validated: No
Voice Layer: Layer 2 (Diagnostic)
Voice Validated: No

## **I. Core Philosophy: The Three Pillars of System Capacity**

The Resource Systems are the foundational framework for tracking character capacity across three distinct domains: **physical exertion (Stamina)**, **metaphysical power (Aether Pool)**, and **biological integrity (Health Pool)**. Each resource represents a different aspect of a character's ability to act, survive, and interface with the broken reality of Aethelgard.

- **Health Pool (HP)** measures **System Integrity** — the coherence of a character's physical form against data-loss
- **Stamina** measures **Action Readiness** — the fuel for coherent physical exertion in a chaotic world
- **Aether Pool (AP)** measures **Aetheric Cache Capacity** — a Mystic's ability to temporarily hold the tainted Aether without corruption

**Design Pillars:**

- **Universal Baseline + Archetype Divergence:** All characters have HP and Stamina. Only Mystics have AP.
- **Attribute-Driven Scaling:** Each resource scales from specific attributes (STURDINESS for HP, STURDINESS+FINESSE for Stamina, WILL for AP)
- **Corruption as Universal Penalty:** The Runic Blight Corruption system directly penalizes HP and AP (but not Stamina)
- **Regeneration Asymmetry:** Stamina regenerates rapidly (passive per-turn), HP regenerates slowly (rest + healing), AP regenerates very slowly (sanctuary rest + active focus)
- **Tactical Economy:** Resources are the currency of the action economy

---

## **II. System Components**

### **Core Resource Subsystems**

**1. Health Pool (HP) System**

- Measures physical integrity and survivability
- Universal resource (all characters)
- Penalized by Corruption (-5% per 10 Corruption)
- Triggers critical states: [Bloodied] at <50% HP, [System Crashing] at 0 HP
- *See: Health Pool (HP) System — Mechanic Specification v5.0*

**2. Stamina System**

- Measures physical and mental readiness for action
- Universal resource (all characters)
- NOT penalized by Corruption
- Regenerates rapidly (25% Max Stamina per turn)
- *See: Stamina System — Mechanic Specification v5.0*

**3. Aether Pool (AP) System**

- Measures Aetheric cache capacity (Mystic-specific)
- Mystic-exclusive resource (Base AP = 50 for Mystics, 0 for all others)
- Penalized by Corruption (-5% per 10 Corruption)
- Source of Corruption risk (casting spells accumulates Corruption)
- *See: Aether Pool (AP) System — Mechanic Specification v5.0*

---

## **III. Calculation Formulas**

### **Health Pool (HP) Calculation**

```jsx
Max HP = (Base HP + [STURDINESS × 10] + Gear/Ability Bonuses) × (1 - Corruption Penalty)
```

- **Base HP:** 50 (universal baseline)
- **STURDINESS:** Each point provides +10 Max HP
- **Corruption Penalty:** -5% per 10 Corruption

**Value Ranges:**

- Minimum: 50 HP | Typical: 100-200 HP | Maximum: ~350 HP

---

### **Stamina Calculation**

```jsx
Max Stamina = (Base Stamina + [STURDINESS × 5] + [FINESSE × 2]) + Gear/Ability Bonuses
```

- **Base Stamina:** 50 (universal baseline)
- NOT penalized by Corruption

**Value Ranges:**

- Minimum: 50 Stamina | Typical: 85-120 Stamina | Maximum: ~200 Stamina

---

### **Aether Pool (AP) Calculation**

```jsx
Max AP = (Base AP + [WILL × 10] + Gear/Ability Bonuses) × (1 - Corruption Penalty)
```

- **Base AP:** 50 for Mystics, 0 for all other archetypes
- **Corruption Penalty:** -5% per 10 Corruption

---

### **Passive Stamina Regeneration**

```jsx
Stamina Regen per Turn = Max Stamina × 0.25
```

- Triggers at Start of character's turn in combat
- Typical: 20-30 Stamina per turn

---

## **IV. Integration Points**

**Primary Dependencies:**

- **Attributes System** — Reads STURDINESS, FINESSE, WILL
- **Archetype System** — Determines Base AP
- **Equipment System** — Reads gear bonuses
- **Trauma Economy System** — Reads Corruption value for HP/AP penalties
- **Combat System** — Resources spent during combat actions

**Referenced By:**

- **All Specializations** — Every spec consumes Stamina or AP
- **Combat System** — Action costs, defensive action costs
- **Damage System** — HP reduction from damage
- **Death & Resurrection System** — Triggers when HP reaches 0

**Modifies:**

- **Character State** — Current HP, Current Stamina, Current AP values

**Triggers:**

- **Corruption Gain** → Recalculate Max HP and Max AP
- **Turn Start (Combat)** → Regenerate Stamina passively
- **Sanctuary Rest** → Fully restore all three resources

---

## **V. Database Schema Requirements**

### **Character Resources Table**

```sql
CREATE TABLE CharacterResources (
  character_id INT PRIMARY KEY,
  current_hp INT NOT NULL DEFAULT 50,
  max_hp INT NOT NULL DEFAULT 50,
  current_stamina INT NOT NULL DEFAULT 50,
  max_stamina INT NOT NULL DEFAULT 50,
  current_ap INT NOT NULL DEFAULT 0,
  max_ap INT NOT NULL DEFAULT 0,
  last_updated TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
  
  FOREIGN KEY (character_id) REFERENCES Characters(character_id)
    ON DELETE CASCADE,
  
  CONSTRAINT chk_hp CHECK (current_hp >= 0 AND current_hp <= max_hp),
  CONSTRAINT chk_stamina CHECK (current_stamina >= 0 AND current_stamina <= max_stamina),
  CONSTRAINT chk_ap CHECK (current_ap >= 0 AND current_ap <= max_ap)
);
```

---

## **VI. Service Integration Architecture**

### **Core Services**

**ResourceService**

- `CalculateMaxHP(characterID)` → maxHP (int)
- `CalculateMaxStamina(characterID)` → maxStamina (int)
- `CalculateMaxAP(characterID)` → maxAP (int)
- `GetCurrentResources(characterID)` → {currentHP, currentStamina, currentAP}
- `DeductStamina(characterID, cost)` → {success: bool, newValue: int}
- `DeductAP(characterID, cost)` → {success: bool, newValue: int}
- `ApplyHealing(characterID, healAmount)` → {oldHP, newHP}
- `RegenerateStamina(characterID)` → {oldStamina, newStamina, regenAmount}
- `ApplyDamage(characterID, damageAmount)` → {oldHP, newHP, bloodied: bool, crashing: bool}

---

## **VII. UI/Feedback Requirements**

### **Vitals Pane**

- **HP Bar:** Red bar, labeled "HP: [Current]/[Max]"
    - Bloodied (<50%): Pulsing red
    - System Crashing (0 HP): Gray/black fill
- **Stamina Bar:** Green/yellow bar
- **AP Bar:** Blue/purple bar (Mystics only)

### **Feedback Events**

- Resource deduction: "[Ability Name] costs 15 Stamina. (45 → 30)"
- Regeneration: "[Character] regenerates 25 Stamina. (30 → 55)"
- Corruption penalty: "Max HP and Max AP reduced by 15%."

---

## **VIII. Design Philosophy**

### **Balance Philosophy**

**Resource Management as Core Gameplay:**

- **Warriors/Skirmishers/Adepts:** High Stamina pools with rapid regeneration
- **Mystics:** Low Stamina, high AP creates different puzzle. Corruption penalty creates death spiral.

**Asymmetric Regeneration:**

- Stamina's rapid regeneration prevents "whiff turns"
- HP's slow regeneration makes damage meaningful
- AP's very slow regeneration makes Mystics conserve spells

### **Design Trade-offs**

**Why Stamina is NOT penalized by Corruption:**

- Represents immediate physical capacity, not long-term integrity
- Prevents Corruption from creating unwinnable combat states

**Why AP is penalized by Corruption:**

- Represents capacity to hold coherent Aetheric cache
- Creates escalating cost for Mystic power (death spiral by design)

---

*Parent specification complete. Child mechanic specifications (HP, Stamina, AP) to follow.*