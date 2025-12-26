# Sanctuary Rest System — Core System Specification v5.0

Type: Core System
Priority: Must-Have
Status: Proposed
Balance Validated: No
Document ID: AAM-SPEC-CORESYSTEM-SANCTUARYREST-v5.0
Proof-of-Concept Flag: No
Sub-Type: Core
Template Compliance: v5.0 Three-Tier Template
Template Validated: No
Voice Layer: Layer 2 (Diagnostic)
Voice Validated: No

## **I. Core Philosophy: The Clean Boot**

The Sanctuary Rest System resolves the question: **How do characters recover from the cumulative physical and psychic damage of surviving in Aethelgard?** This is not simple sleep—it is a **system defragmentation**, a deliberate act of synchronizing one's corrupted personal code with the last remaining nodes of stable, uncorrupted reality.

In a world where the operating system has crashed, **Runic Anchors** are islands of coherence—fragments of the world's original kernel, humming with clean logic. Resting at an Anchor is the only way to achieve a **full reboot**: complete restoration of HP, Stamina, Aether Pool, and the critical reset of Psychic Stress to zero.

**Design Pillars:**

- **Sanctuaries as Strategic Hubs:** Settlements with Runic Anchors are safe havens for full recovery, progression, and respawn
- **Attunement as System Restore Point:** Binding to an Anchor sets your resurrection location
- **Wilderness Rest as Gamble:** Partial recovery, resource consumption, and ambush risk
- **Time as Cost:** All rest advances the world clock by 8 hours
- **Trauma Economy Gate:** Only Sanctuary Rest fully resets Psychic Stress
- **Progression Gate:** Saga menu, attunement changes, artifact reconfiguration only available during Sanctuary Rest

---

## **II. System Components**

### **Core Subsystems**

**1. Runic Anchors (Physical Locations)**

- Ancient waystones or pre-Glitch server monoliths
- Emit calming hum, resist Runic Blight, provide Metaphysical Cover
- Found in settlements (Sanctuary Anchors) or rarely in wilderness (Nexus Anchors)

**2. Sanctuary Rest (Full Recovery)**

- Available only at Runic Anchors in friendly/neutral settlements
- Full restoration: HP, Stamina, AP, Psychic Stress reset to zero
- Unlocks Saga menu, attunement changes, artifact reconfiguration
- Time cost: 8 hours

**3. Wilderness Rest (Partial Recovery)**

- Available in any cleared, non-hostile room outside settlements
- Partial restoration: HP/Stamina to 75%, Stress recovery via WILL check
- Requires Rations and Water (1 per character)
- Ambush risk determined by Wasteland Survival Camp Craft check
- Time cost: 8 hours

**4. Attunement System**

- Process of binding to Runic Anchor (sets respawn point)
- Can only change attunement during Sanctuary Rest
- Also governs artifact/Myth-Forged gear attunement (3 slots)

---

## **III. Calculation Formulas**

### **Wilderness Rest Recovery**

```jsx
HP Recovery = MaxHP × 0.75
Stamina Recovery = MaxStamina × 0.75
AP Recovery = MaxAP × 0.75 (Mystics only)
```

**Psychic Stress Recovery (Wilderness):**

```jsx
Stress Recovered = WILL dice pool result
```

---

### **Ambush Chance Calculation**

```jsx
Base Chance = Region Danger DC (varies by location)
```

**Modified by Camp Craft check:**

- Critical Success: -40% chance
- Success: -20% chance
- Failure: +0% chance
- Fumble: +20% chance

---

### **Resource Consumption**

```jsx
Rations Required = Party Size × 1
Water Required = Party Size × 1
```

**Failure to Provide:** Characters gain [Exhausted] debuff (-2d10 to all checks)

---

## **IV. Integration Points**

**Primary Dependencies:**

- **Resource Systems** — Restores HP, Stamina, AP
- **Trauma Economy System** — Resets Psychic Stress (Sanctuary) or partial (Wilderness)
- **Skills System** — Wasteland Survival governs Camp Craft check
- **World Time System** — Advances clock by 8 hours
- **Inventory System** — Deducts Rations and Water

**Referenced By:**

- **Death & Resurrection System** — Characters respawn at last attuned Runic Anchor
- **Saga System** — PP spending only available during Sanctuary Rest
- **Equipment System** — Artifact attunement changes only during Sanctuary Rest
- **Encounter System** — Ambush checks trigger combat

---

## **V. Database Schema Requirements**

### **Runic_Anchors Table**

```sql
CREATE TABLE Runic_Anchors (
  anchor_id INT PRIMARY KEY AUTO_INCREMENT,
  anchor_name VARCHAR(100) NOT NULL,
  location_id INT NOT NULL,
  anchor_type ENUM('sanctuary', 'nexus') NOT NULL,
  is_active BOOLEAN NOT NULL DEFAULT TRUE,
  metaphysical_cover_bonus INT NOT NULL DEFAULT 2,
  
  FOREIGN KEY (location_id) REFERENCES Locations(location_id)
);
```

### **Character_Attunements Table**

```sql
CREATE TABLE Character_Attunements (
  character_id INT PRIMARY KEY,
  attuned_anchor_id INT NOT NULL,
  last_attunement_time TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
  
  FOREIGN KEY (character_id) REFERENCES Characters(character_id),
  FOREIGN KEY (attuned_anchor_id) REFERENCES Runic_Anchors(anchor_id)
);
```

### **Artifact_Attunements Table**

```sql
CREATE TABLE Artifact_Attunements (
  attunement_id INT PRIMARY KEY AUTO_INCREMENT,
  character_id INT NOT NULL,
  item_instance_id INT NOT NULL,
  slot_number INT NOT NULL,
  
  UNIQUE KEY uk_character_slot (character_id, slot_number),
  CONSTRAINT chk_slot CHECK (slot_number >= 1 AND slot_number <= 3)
);
```

---

## **VI. Service Integration Architecture**

### **Core Service: RestService**

**Key Methods:**

```jsx
InitiateRest(characterID) → RestResult
SanctuaryRestPipeline(characterID) → RestResult
WildernessRestPipeline(characterID) → RestResult
AttuneToAnchor(characterID, anchorID) → AttunementResult
AttuneToArtifact(characterID, itemInstanceID, slotNumber) → AttunementResult
UnattuneFromArtifact(characterID, itemInstanceID) → AttunementResult
```

---

## **VII. UI/Feedback Requirements**

### **Sanctuary Rest Feedback**

```
You rest under the protection of the Runic Anchor.

[FULL RECOVERY]
  HP: 80/80 (+50)
  Stamina: 100/100 (+60)
  Aether Pool: 120/120 (+70)
  Psychic Stress: 0/100 (-65)

The Saga is saved.

You may now use 'saga' to spend Progression Points or 'attune' to bind artifacts.
```

### **Wilderness Rest Feedback**

```
[PARTIAL RECOVERY]
  HP: 60/80 (+30, 75% max)
  Stamina: 75/100 (+35, 75% max)
  Psychic Stress: 52/100 (-13, WILL checks)

Consumed: 4 Rations, 4 Clean Water
```

---

## **VIII. Edge Cases & Special Conditions**

- **Insufficient Resources:** Characters gain [Exhausted] debuff
- **Ambush During Rest:** Combat initiated, no recovery, resources lost
- **Multiple Anchor Attunements:** New attunement replaces previous
- **Artifact Attunement Limits:** Maximum 3 slots per character (never increases)
- **Time-Sensitive Quests:** Warning if resting will cause quest failure
- **Nexus Anchors:** Rare wilderness Sanctuaries that function as full rest

---

## **IX. Design Philosophy**

### **Balance Philosophy**

**Sanctuaries as Earned Victories:** Reaching Sanctuary after dangerous expedition = major accomplishment

**Wilderness Rest as Calculated Risk:** Partial recovery, ambush risk creates tension

**Time as Strategic Resource:** 8-hour cost makes resting non-trivial decision

**Attunement as Commitment:** Forces pre-expedition planning

---

*This specification consolidates Runic Anchors, Sanctuary Rest, Wilderness Rest, and Attunement mechanics into a unified recovery system.*