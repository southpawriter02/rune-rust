# Death & Resurrection System — Core System Specification v5.0

Type: Core System
Priority: Must-Have
Status: Review
Balance Validated: No
Document ID: AAM-SPEC-CORESYSTEM-DEATHANDRESURRECTION-v5.0
Proof-of-Concept Flag: No
Sub-Type: Core
Template Compliance: v5.0 Three-Tier Template
Template Validated: No
Voice Layer: Layer 2 (Diagnostic)
Voice Validated: No

## **I. Core Philosophy: Death as System Crash**

The Death & Resurrection System resolves the question: **What happens when a character's life signs terminate, and how do they return from the void?** This is not gentle fading—it is a **catastrophic system crash**, a violent de-compilation of the soul followed by a traumatic reboot sequence at the last stable kernel.

In Aethelgard, death is the ultimate expression of the world's fatal error. When HP reaches zero, the character's "silver cord" severs—a haunting echo of the original Ginnungagap Glitch. The soul is **violently ejected into the corrupted Aether**, the raw paradoxical chaos of the Great Silence. Resurrection is not a simple respawn: it is a **forced recompilation** at the character's attuned Runic Anchor, a painful reboot that leaves permanent scars in the form of accumulated Corruption.

**Design Pillars:**

- **Death is Significant:** Not a trivial setback, but a profound consequence with permanent penalties
- **Resurrection is Traumatic:** Characters return damaged, both temporarily (System Shock) and permanently (Corrupted File)
- **Ironman Persistence:** Auto-save on death prevents save-scumming; death is irreversible
- **Corpse Run Risk:** Recovering gear requires returning to dangerous death site (Glitched Remnant)
- **Corruption Accumulation:** Each death increases base Corruption (+1-2 per death, permanent)
- **Runic Anchor Dependency:** Respawn location determined by last attunement (see Sanctuary Rest System)

**Player Experience Statement:**

Death is **terrifying and consequential**. Each death makes your character a little more monstrous, a little more unstable. The system creates genuine tension: death is not a reload button, it's a scar that never fully heals.

---

## **II. System Components**

### **Core Subsystems**

**1. System Crashing State (0 HP)**

- Trigger: Current HP reduced to 0 or below
- Character unconscious, cannot act
- Death Saves begin (WILL-based Resolve Checks)
- Must succeed 3 saves before 3 failures
- *See: System Crashing mechanics*

**2. Death Saves (Stabilization Checks)**

- Made at start of each downed character's turn
- WILL attribute - HP Debt = dice pool
- Roll vs DC based on HP Debt severity
- 3 successes = stabilize, 3 failures = permanent death
- *See: Death Save resolution pipeline*

**3. The Echoing Path (Resurrection Journey)**

- Triggered when entire party dies or character fails 3 Death Saves
- Soul dragged through corrupted Aether (narrative experience)
- Recompiled at last attuned Runic Anchor
- Inflicts System Shock debuff + Corrupted File stack
- *See: Resurrection sequence*

**4. System Shock Debuff (Temporary Penalty)**

- Severe temporary debuff after resurrection
- Duration: 24 in-game hours or until Sanctuary Rest
- Effects: -2 to all attributes, -25% Max HP/Stamina, slowed regen
- Prevents "zerg rushing" encounters
- *See: System Shock specifications*

**5. Corrupted File Debuff (Permanent Penalty)**

- Permanent stacking debuff, gained on each death
- +1 or +2 base Runic Blight Corruption per death
- Irreversible (except via rare high-level quests to Nexus Anchors)
- Death spiral: More Corruption → worse penalties → more likely to die
- *See: Corrupted File mechanics*

**6. Glitched Remnant (Corpse Run)**

- Tear in reality left at death site
- Contains character's lost inventory and equipped gear (now [Broken])
- High chance of attracting powerful Forlorn enemy
- Party must return to recover items
- *See: Corpse Run mechanics*

**7. Forced Reboot (In-Combat Revival)**

- Rare abilities (Bone-Setter's Miracle Worker) can revive at 0 HP
- Bypasses full death sequence
- Still inflicts partial penalties
- *See: In-combat revival mechanics*

---

## **III. Calculation Formulas**

### **HP Debt & Death Save DC**

```jsx
HP Debt = |CurrentHP| (absolute value of negative HP)
```

**Example:**

- Character at 0 HP: HP Debt = 0 (just reached threshold)
- Character at -5 HP: HP Debt = 5 (took 5 overkill damage)
- Character at -15 HP: HP Debt = 15 (massive overkill)

**Death Save Severity Categories:**

```jsx
if (HP Debt === 0) {
  Severity = "Moderate" (DC 3)
} else if (HP Debt <= 5) {
  Severity = "Serious" (DC 6)
} else if (HP Debt <= 10) {
  Severity = "Critical" (DC 9)
} else {
  Severity = "Fatal" (DC 12)
}
```

**Death Save Dice Pool:**

```jsx
Dice Pool = WILL - HP Debt
```

**Example:**

- WILL 7, HP Debt 0: Roll 7d10 vs DC 3
- WILL 7, HP Debt 3: Roll 4d10 vs DC 6
- WILL 7, HP Debt 8: Roll -1d10 (floor at 1d10) vs DC 9

**Floor:** Minimum 1d10 (even with massive debt)

---

### **Death Save Outcomes**

```jsx
if (Net Successes >= DC) {
  Outcome = "Success" (mark 1 success)
} else if (Net Successes < DC && Net Successes >= -5) {
  Outcome = "Failure" (mark 1 failure)
} else if (Net Successes < -5) {
  Outcome = "Critical Failure" (mark 2 failures)
}
```

**Stabilization:**

- 3 Successes before 3 Failures → Stabilize at 0 HP (unconscious but stable)
- Stabilized character regains consciousness after combat ends

**Permanent Death:**

- 3 Failures before 3 Successes → Character dies permanently
- Triggers Echoing Path (resurrection at Anchor)

---

### **System Shock Penalties**

```jsx
System Shock Duration = 24 in-game hours OR next Sanctuary Rest (whichever first)
```

**Penalties While Active:**

```jsx
All Attributes: -2 (MIGHT, FINESSE, WITS, WILL, RESOLVE, STURDINESS)
Max HP: -25%
Max Stamina: -25%
Max AP: -25% (Mystics only)
Regen Rates: -50% (HP/Stamina/AP per-turn regen)
```

**Example:**

- Base WILL 8 → Debuffed WILL 6
- Base Max HP 80 → Debuffed Max HP 60
- Character revives at 60 HP out of 60 (full of new max)

---

### **Corrupted File Accumulation**

```jsx
Corrupted File Stacks = Number of Deaths
Permanent Corruption Increase = Stacks × Corruption_Per_Death
```

**Corruption Per Death:**

- Standard Death: +2 Runic Blight Corruption (permanent)
- Death in [Blighted] Zone: +3 Corruption (environmental modifier)
- Death to [Psychic Resonance] enemy: +3 Corruption (traumatic)

**Example Progression:**

- 1st Death: +2 Corruption (Base Corruption now 2)
- 2nd Death: +2 Corruption (Base Corruption now 4)
- 5th Death: +2 Corruption (Base Corruption now 10)
- 10th Death: +2 Corruption (Base Corruption now 20)

**At 50+ Corruption:**

- Max HP: -25% (from Corruption penalties)
- Max AP: -25% (Mystics)
- All Resolve Checks: -2d10
- Character becoming monstrous

**At 100 Corruption:**

- Character transformation into [Forlorn] (loss of player control)
- Permanent, irreversible
- Character becomes NPC enemy

---

### **Glitched Remnant Encounter Chance**

```jsx
Encounter Chance = 60% (base)
```

**Encounter Type (if triggered):**

- 50%: Forlorn (tier appropriate to party level)
- 30%: Draugr pack (2-4 units)
- 20%: Environmental hazard (collapsing structure, Blight surge)

**Remnant Lifespan:**

- Persists for 48 in-game hours
- After 48 hours, items are permanently lost

---

## **IV. Integration Points**

**Primary Dependencies:**

- **Resource Systems** — Monitors Current HP, triggers at 0 HP
- **Attributes System** — WILL attribute governs Death Save dice pool
- **Trauma Economy System** — Adds permanent Corruption via Corrupted File
- **Sanctuary Rest System** — Reads attuned Runic Anchor for respawn location
- **Dice Pool System** — Resolves Death Saves via d10 pool
- **Status Effect System** — Applies System Shock debuff
- **World Time System** — Tracks System Shock duration (24 hours)

**Referenced By:**

- **Combat System** — Triggers Death Saves when character reaches 0 HP
- **Sanctuary Rest System** — System Shock removed on Sanctuary Rest
- **Encounter System** — Glitched Remnant spawns enemies
- **Inventory System** — Gear moved to Glitched Remnant, marked [Broken]
- **Equipment System** — [Broken] gear requires repairs before use
- **Save System** — Auto-save triggered on death (Ironman)
- **Quest System** — Rare quests offer Corruption cleansing

**Modifies:**

- **Current HP** — 0 HP → triggers System Crashing state
- **Base Corruption** — Permanently increased by Corrupted File
- **Attributes** — Temporarily reduced by System Shock
- **Max HP/Stamina/AP** — Temporarily reduced by System Shock
- **Character Position** — Moved to attuned Runic Anchor
- **Inventory** — Items moved to Glitched Remnant
- **Equipment Durability** — All equipped gear becomes [Broken]

**Triggers:**

- **HP reaches 0** → System Crashing state, Death Saves begin
- **3 Death Save failures** → Echoing Path resurrection sequence
- **Entire party defeated** → Echoing Path for all characters
- **Resurrection complete** → Apply System Shock + Corrupted File, auto-save

---

## **V. Database Schema Requirements**

### **Death_History Table (Analytics & Corruption Tracking)**

**Purpose:** Tracks all character deaths for Corrupted File calculation and analytics

**Schema:**

```sql
CREATE TABLE Death_History (
  death_id INT PRIMARY KEY AUTO_INCREMENT,
  character_id INT NOT NULL,
  death_number INT NOT NULL,
  location_id INT NOT NULL,
  cause_of_death VARCHAR(200),
  hp_debt INT NOT NULL,
  world_time INT NOT NULL,
  corruption_gained INT NOT NULL,
  respawn_anchor_id INT NOT NULL,
  glitched_remnant_id INT,
  timestamp TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
  
  FOREIGN KEY (character_id) REFERENCES Characters(character_id)
    ON DELETE CASCADE,
  FOREIGN KEY (location_id) REFERENCES Locations(location_id)
    ON DELETE RESTRICT,
  FOREIGN KEY (respawn_anchor_id) REFERENCES Runic_Anchors(anchor_id)
    ON DELETE RESTRICT,
  
  INDEX idx_character (character_id),
  INDEX idx_death_number (character_id, death_number)
);
```

**Persistence:** Permanent historical record

---

### **Glitched_Remnants Table (Corpse Locations)**

**Purpose:** Tracks active corpse locations requiring recovery

**Schema:**

```sql
CREATE TABLE Glitched_Remnants (
  remnant_id INT PRIMARY KEY AUTO_INCREMENT,
  character_id INT NOT NULL,
  location_id INT NOT NULL,
  death_id INT NOT NULL,
  world_time_created INT NOT NULL,
  expiration_time INT NOT NULL,
  is_recovered BOOLEAN NOT NULL DEFAULT FALSE,
  encounter_spawned BOOLEAN DEFAULT FALSE,
  inventory_snapshot JSON NOT NULL,
  equipment_snapshot JSON NOT NULL,
  timestamp TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
  
  FOREIGN KEY (character_id) REFERENCES Characters(character_id)
    ON DELETE CASCADE,
  FOREIGN KEY (location_id) REFERENCES Locations(location_id)
    ON DELETE RESTRICT,
  FOREIGN KEY (death_id) REFERENCES Death_History(death_id)
    ON DELETE CASCADE,
  
  INDEX idx_character_active (character_id, is_recovered),
  INDEX idx_expiration (expiration_time)
);
```

**Persistence:** Temporary (48 hours in-world time, then purged)

---

### **System_Shock_Debuffs Table**

**Purpose:** Tracks active System Shock debuffs and their expiration

**Schema:**

```sql
CREATE TABLE System_Shock_Debuffs (
  debuff_id INT PRIMARY KEY AUTO_INCREMENT,
  character_id INT NOT NULL,
  applied_at_world_time INT NOT NULL,
  expires_at_world_time INT NOT NULL,
  applied_at_timestamp TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
  
  FOREIGN KEY (character_id) REFERENCES Characters(character_id)
    ON DELETE CASCADE,
  
  UNIQUE KEY uk_character (character_id),
  INDEX idx_expiration (expires_at_world_time)
);
```

**Persistence:** Temporary (auto-removed after 24 in-game hours OR Sanctuary Rest)

---

## **VI. Service Integration Architecture**

### **Core Service: DeathService**

**Responsibilities:**

- Monitor HP, trigger System Crashing state
- Manage Death Save sequence
- Execute resurrection sequence
- Apply System Shock and Corrupted File debuffs
- Create Glitched Remnants
- Trigger auto-save (Ironman)

**Key Methods:**

```jsx
OnHPReachesZero(characterID) → void
MakeDeathSave(characterID) → DeathSaveResult
Stabilize(characterID) → void
InitiateResurrection(characterID) → void
ResurrectionSequence(characterID) → ResurrectionResult
CreateGlitchedRemnant(characterID, locationID) → int remnantID
RecoverGlitchedRemnant(characterID, remnantID) → RecoveryResult
ApplySystemShock(characterID, duration_hours) → void
RemoveSystemShock(characterID) → void
ApplyCorruptedFile(characterID) → void
```

---

## **VII. UI/Feedback Requirements**

### **System Crashing UI**

```
=== SYSTEM CRASHING ===

[Character]'s HP reaches 0!
Her silver cord frays. The world's code fragments around her.
She is unconscious and dying.

[STATUS: SYSTEM CRASHING]
Death Saves required: 3 successes before 3 failures
```

### **Death Save UI**

```
=== DEATH SAVE ===

Death Saves: Successes ☑ ☐ ☐ | Failures ☑ ☐ ☐

Rolling Death Save...
WILL 7 - HP Debt 3 (Serious) = 4d10 vs. DC 6

[8, 4, 7, 2]

SUCCESS (1 success)
```

### **Resurrection Sequence UI**

```
=====================================
     THE ECHOING PATH
=====================================

The world fragments.
You are cast into the void—the eternal scream of paradox.

A signal. Stable. Clean.
The Runic Anchor: your last system restore point.

You are PULLED toward it. Violently. Painfully.
A forced recompilation. A dirty reboot.

=====================================
     RESURRECTION COMPLETE
=====================================

[RESURRECTION PENALTIES]
• System Shock (24 hours): All Attributes -2, Max HP/Stamina -25%
• Corrupted File (PERMANENT): Base Corruption +2

The Saga is saved. Death is permanent.
```

---

## **VIII. Edge Cases & Special Conditions**

- **Party Wipe:** Auto-trigger Echoing Path for all characters (skip Death Saves)
- **Death Save Interruption:** Ally healing above 0 HP ends Death Saves
- **Overkill:** Massive HP Debt = nearly impossible to survive (intentional)
- **Multiple Glitched Remnants:** Can exist simultaneously, each expires independently
- **Remnant Expiration:** Items permanently lost after 48 hours
- **System Shock During Combat:** Makes subsequent encounters harder
- **Corrupted File at High Corruption:** At 100 Corruption → Forlorn Transformation
- **In-Combat Revival:** Bypasses Echoing Path, still applies System Shock (not Corrupted File)

---

## **IX. Design Philosophy**

### **Balance Philosophy**

**Death Must Be Meaningful:** Not a simple reload, but a profound consequence with permanent penalties creating genuine fear.

**Death Spiral is Intentional:** More Corruption → worse penalties → more likely to die → more Corruption. Forces players to adapt strategy.

**Ironman Saves Prevent Abuse:** Auto-save on death cannot be undone. No save-scumming.

**Corpse Run Creates Tension:** 60% Forlorn chance makes recovery dangerous. 48-hour timer adds urgency.

**System Shock Prevents Zerg Rush:** Discourages immediate re-attempts, forces tactical retreat.

---

*This specification consolidates all death, resurrection, and post-death penalty mechanics into a unified system that creates meaningful consequences for character defeat.*