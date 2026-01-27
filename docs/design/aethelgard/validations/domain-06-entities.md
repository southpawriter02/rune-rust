# Domain 6: Entities Validation Check

**Domain ID:** `DOM-6`
**Severity:** P2-HIGH
**Applies To:** Dialogue trees, NPC descriptions, character references, faction documentation, historical figures

---

## Canonical Ground Truth

Named characters and significant entities must be established in canon or introduced with appropriate context.

### Entity Types

| Type | Definition | Requirements |
|------|------------|--------------|
| **Named NPCs** | Established characters with documented histories | Canon reference or proper introduction |
| **Data-Wraiths** | Psychic echoes of deceased individuals | Follow manifestation mechanics |
| **Faction Leaders** | Roles consistent with faction structure | Faction dossier alignment |
| **Historical Figures** | Characters from prior ages | Timeline consistency |

### Automata Types (Iron Heart-Powered)

| Type | Function | Power Source |
|------|----------|--------------|
| **Draugr-Pattern** | Military/security automata | Iron Heart |
| **Haugbui-Class** | Heavy labor automata | Iron Heart |
| **Forlorn** | Corrupted Einherjar with fragmented consciousness | Iron Heart (degraded) |

### Symbiotic Plate (Different Mechanism)

| Type | Function | Mechanism |
|------|----------|-----------|
| **Symbiotic Plate** | Bio-mechanical neural interface | Creates **Husks** when host dies |
| **Husks** | Bodies controlled by Symbiotic Plate after host death | Plate continues operating the corpse |

**IMPORTANT:** Symbiotic Plate uses a DIFFERENT mechanism than Iron Heart automata.

### REMOVED Entity Types (NO LONGER CANONICAL)

**The following are NOT valid entity types — remove if found:**

| Removed | Reason | Use Instead |
|---------|--------|-------------|
| ~~"Galdr"~~ | Galdr is a DISCIPLINE, not an entity | Galdr-singer (practitioner) |
| ~~"Unraveled"~~ | Not a canonical entity type | Remove or replace with appropriate type |

### Faction Affiliation Rules

- Characters must align with documented faction lore
- Faction roles must match established hierarchy
- Cross-faction affiliations need explanation
- Faction-specific terminology/titles must be accurate

### Entity Mechanics

**Data-Wraiths:**
- Psychic echoes, not literal ghosts
- Manifestation tied to paradox/Aetheric disturbance
- Retain fragmented memories/personalities
- Cannot be "destroyed," only dispersed

**Forlorn (Corrupted Einherjar):**
- Einherjar frames with corrupted/fragmented consciousness
- Original identity degraded or lost
- Mechanical bodies, confused purpose
- Range from near-lucid to completely feral
- Powered by degraded Iron Hearts

**Draugr-Pattern Automata:**
- Military/security function
- More aggressive behavior patterns
- Iron Heart powered
- May retain combat protocols

**Haugbui-Class Automata:**
- Heavy labor function
- Typically less aggressive
- Iron Heart powered
- May continue work routines

**Husks (Symbiotic Plate Victims):**
- Created when Symbiotic Plate host dies
- Plate continues operating the corpse
- NOT the same as Forlorn (different mechanism)
- Bio-mechanical rather than purely mechanical

---

## Validation Checklist

**Named Characters:**

- [ ] Named characters established in canon or properly introduced?
- [ ] Faction affiliations accurate and documented?
- [ ] Historical figures positioned correctly in timeline?
- [ ] Faction titles/ranks match documented hierarchy?
- [ ] New characters have sufficient worldbuilding context?

**Entity Types:**

- [ ] Data-Wraith manifestations consistent with canonical mechanics?
- [ ] Forlorn characterization aligns with Einherjar corruption lore?
- [ ] Draugr-Pattern used for military/security automata?
- [ ] Haugbui-Class used for labor automata?
- [ ] Husks properly distinguished from Forlorn (Symbiotic Plate mechanism)?
- [ ] Entity abilities/limitations consistent with established lore?

**Removed Types (MUST NOT APPEAR):**

- [ ] No "Galdr" used as entity type? (Galdr = discipline only)
- [ ] No "Unraveled" entities? (not canonical)
- [ ] No other invented entity categories without approval?

---

## Common Violations

| Pattern | Example | Why It Fails |
|---------|---------|--------------|
| Unestablished named characters | "Captain Erikson led the patrol" (no canon reference) | Character appears without introduction |
| Faction affiliation contradiction | "The Jotun-Reader priest blessed the..." | Jotun-Readers are technical, not religious |
| Data-Wraith as literal ghost | "the ghost attacked physically" | Data-Wraiths are psychic manifestations |
| Timeline placement error | "the hero of the Glitch" | No heroes during cataclysmic event |
| Wrong faction terminology | "the Guild Wizard" | "Wizard" forbidden; wrong faction title |
| "Galdr" as entity | "a Galdr emerged from the shadows" | Galdr is a discipline, not an entity type |
| "Unraveled" entities | "the Unraveled attacked" | Not a canonical entity type |
| Husk/Forlorn confusion | "the Forlorn Husk" | Different mechanisms — Forlorn are Iron Heart; Husks are Symbiotic Plate |

---

## Green Flags

- References to established canon characters
- Appropriate faction titles and roles
- Data-Wraith mechanics (psychic echoes, paradox-tied)
- Forlorn mechanics (corrupted Einherjar, identity degradation)
- New characters with clear introduction and context
- Historical figures with consistent timeline placement

---

## Remediation Strategies

### Option 1: Canon Reference Check

Before using a named character, verify:
1. Is this character documented in faction dossiers?
2. Does their role match established hierarchy?
3. Is their timeline placement consistent?

If not documented, either:
- Reference an established character instead
- Introduce as new character with proper context

### Option 2: Proper Introduction

For new characters:

| Before | After |
|--------|-------|
| "Captain Erikson ordered the retreat" | "The patrol leader—a scarred veteran the others called Erikson—ordered the retreat" |
| "High Priestess Mora performed the ritual" | "The senior Runasmidr of the settlement, Mora, led the Galdr-work" |

### Option 3: Entity Mechanic Correction

For Data-Wraiths:

| Before | After |
|--------|-------|
| "the ghost grabbed her arm" | "the Data-Wraith's presence pressed against her mind, phantom fingers of memory grasping" |
| "they destroyed the spirit" | "the Galdr disrupted the manifestation; the echo dispersed, though it would likely reform" |

For Forlorn:

| Before | After |
|--------|-------|
| "the robot attacked" | "the Forlorn lurched forward, its corroded frame jerking with fragmented purpose" |
| "it spoke clearly" | "words emerged from its vocalizer, broken and looping, a fragment of some long-dead greeting protocol" |

### Option 4: Faction Alignment Fix

| Before | After |
|--------|-------|
| "Jotun-Reader priest" | "Jotun-Reader diagnostician" or "the settlement's designated Galdr-singer" |
| "Guild Wizard" | "Guild-certified Runasmidr" or "the trade-hall's inscriptionist" |

---

## Decision Tree

```
Does the content reference named characters or significant entities?
├── NO → PASS (not applicable)
├── YES → Is this an established canon character?
│   ├── YES → Does characterization match canon?
│   │   ├── YES → PASS
│   │   └── NO → FAIL (characterization contradiction)
│   └── NO → Is this a new character?
│       ├── YES → Is there sufficient introduction/context?
│       │   ├── YES → PASS (new character properly introduced)
│       │   └── NO → FAIL (unestablished character)
│       └── NO → Is this a Data-Wraith or Forlorn?
│           ├── YES → Do mechanics match canon?
│           │   ├── YES → PASS
│           │   └── NO → FAIL (entity mechanics violation)
│           └── NO → Review for appropriate entity type
```

---

## Faction Quick Reference

When validating faction affiliations, check:

| Faction | Typical Roles | Forbidden Terms |
|---------|---------------|-----------------|
| Jotun-Readers | Diagnostician, Field Analyst, Archivist | Priest, Wizard, Mage |
| Runasmidr Guild | Inscriptionist, Forge-Master, Apprentice | Wizard, Sorcerer, Enchanter |
| Bone-Setters | Practitioner, Chiurgeon, Herbalist | Doctor, Surgeon, Nurse |
| [Others per dossiers] | [Per documentation] | [Per Domain 3] |

---

## Examples

### PASS Example

**Content:** "The settlement's Jotun-Reader—a weathered woman named Kara who'd spent thirty years reading the dying systems—studied the readout with practiced skepticism. 'The readings are degraded,' she said. 'But something's changed down there.'"

**Why:** New character properly introduced with role context, faction-appropriate title, behavior consistent with Jotun-Reader function.

### FAIL Example

**Content:** "High Wizard Thormund of the Jotun-Reader Order performed the ancient ritual to banish the ghost."

**Violations:**
- "Wizard" is forbidden terminology (Domain 3 & 6)
- "Jotun-Reader Order" sounds religious (they're technical)
- "ancient ritual" implies mystical rather than systematic
- "ghost" instead of Data-Wraith with proper mechanics

**Remediation:** "Thormund, a senior Jotun-Reader diagnostician, configured the dispersal array—Pre-Glitch equipment of uncertain reliability. The Galdr he intoned was technical, not mystical: frequencies calibrated to disrupt the Data-Wraith's paradox resonance."

### FAIL Example

**Content:** "The Forlorn greeted them warmly and offered directions to the settlement."

**Violation:** Forlorn have degraded consciousness; warm greeting implies full cognitive function.

**Remediation:** "The Forlorn turned toward them, its vocalizer crackling. 'Wel-welcome to—to—' The words looped, stuck in some ancient hospitality protocol. It gestured jerkily down the corridor, the motion repeated three times before it froze."
