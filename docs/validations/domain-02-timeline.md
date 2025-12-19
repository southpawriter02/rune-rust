# Domain 2: Timeline Validation Check

**Domain ID:** `DOM-2`
**Severity:** P2-HIGH
**Applies To:** Dialogue trees, NPC descriptions, world lore, historical references, event documentation

---

## Canonical Ground Truth

All temporal references must use the canonical 5-Age chronology with appropriate notation systems.

### 5-Age Chronology (Post-Glitch)

The current era (Post-Glitch) is divided into five distinct ages:

| Age | Period | Description |
|-----|--------|-------------|
| **Age of Forging** | Pre-Glitch | Technological era before the collapse (Cycle notation) |
| **Age of Silence** | ~0-100 PG | Immediate aftermath, chaos, system failures |
| **Age of Wandering** | ~100-300 PG | Nomadic survival, realm exploration |
| **Age of Walls** | ~300-500 PG | Settlement building, defensive structures |
| **Age of Creeds** | ~500-783 PG | Current age — factions, beliefs, organized society |

**Key Events:**
- **Ginnungagap Glitch** = Year 0 PG (system-wide cascade failure)
- **Current Year** = 783 PG (Age of Creeds)
- **Pre-Glitch** = "The Age Before" (umbrella term for Age of Forging)

### Timeline Rules

- **Ginnungagap Glitch = Year 0** (absolute reference point)
- **Current year: 783 PG** (Post-Glitch)
- All contemporary events use **PG notation**
- Pre-Glitch historical events use **Cycle notation**
- No absolute calendar dates (AD, CE, BCE, etc.)

---

## Validation Checklist

- [ ] All contemporary dates use PG notation?
- [ ] Pre-Glitch dates use Cycle notation (when specific)?
- [ ] No absolute calendar dates (e.g., "2347 AD", "1200 CE")?
- [ ] Event sequences align with 5-age chronology?
- [ ] Timeline compression avoided (Ages span appropriate durations)?
- [ ] Ginnungagap Glitch positioned as Year 0 pivot?
- [ ] Current year referenced as 783 PG when specific?
- [ ] "The Age Before" or "Pre-Glitch" used for umbrella historical references?

---

## Common Violations

| Pattern | Example | Why It Fails |
|---------|---------|--------------|
| Absolute dates | "in the year 2347 AD" | Uses real-world calendar system |
| Mixed notation | "Cycle 847 was 200 years before 450 PG" | Mathematically impossible |
| Missing notation | "three hundred years ago" | Ambiguous without PG reference |
| Wrong era assignment | "during the Glitch in 50 PG" | Glitch is Year 0, not PG era |
| Future PG dates | "by 900 PG" | Speculative beyond current 783 PG |

---

## Green Flags

- Consistent PG notation for current era events ("in 780 PG", "since 650 PG")
- Cycle notation for Age of Forging references ("Cycle 412")
- Year 0 = Ginnungagap Glitch
- 783 PG explicitly referenced as "present day" or "current year"
- "The Age Before" for general pre-Glitch references

---

## Remediation Strategies

### Option 1: PG Notation Conversion

Convert vague temporal references to PG notation:

| Before | After |
|--------|-------|
| "centuries ago" | "in the early centuries PG" or specific "around 200 PG" |
| "in ancient times" | "during the Age of Forging" or "in the Age Before" |
| "recently" | "within the last decade" or "since 775 PG" |

### Option 2: Relative Time Framing

Use relative references anchored to canonical events:

| Before | After |
|--------|-------|
| "500 years ago" | "five centuries after the Glitch" |
| "before the fall" | "before the Ginnungagap Glitch" |
| "in living memory" | "within the last two generations" |

### Option 3: Qualitative Duration

Replace specific spans with qualitative language:

| Before | After |
|--------|-------|
| "exactly 150 years" | "generations past" |
| "2,000 years before" | "deep in the Age of Forging" |

---

## Decision Tree

```
Does the content contain temporal references?
├── NO → PASS (not applicable)
├── YES → Is it a specific date/year?
│   ├── YES → Does it use PG or Cycle notation?
│   │   ├── YES → Is the notation appropriate for the era?
│   │   │   ├── YES → PASS
│   │   │   └── NO → FAIL (wrong notation for era)
│   │   └── NO → Does it use AD/CE/BCE or other real-world dates?
│   │       ├── YES → FAIL (absolute calendar violation)
│   │       └── NO → FAIL (missing required notation)
│   └── NO → Is it a relative time reference?
│       ├── YES → Is the implied era clear and consistent?
│       │   ├── YES → PASS
│       │   └── NO → FLAG for clarification
│       └── NO → Continue to next check
```

---

## Era-Specific Language Guide

### Pre-Glitch References (Age of Forging)

Appropriate terms:
- "During the Age of Forging..."
- "In Cycle [number]..."
- "Before the Glitch..."
- "The Age Before witnessed..."
- "Pre-Glitch systems..."

### Post-Glitch Age References

| Age | Appropriate Terms |
|-----|-------------------|
| **Age of Silence** | "the silent years", "the immediate aftermath", "the first century" |
| **Age of Wandering** | "the wandering years", "when the survivors roamed", "the nomadic period" |
| **Age of Walls** | "when the walls rose", "the fortress age", "the building years" |
| **Age of Creeds** | "the current age", "the age of factions", "in modern times" |

### General Current Era

Appropriate terms:
- "In [number] PG..."
- "Since the Glitch..."
- "In the seven centuries since..."
- "By the current year, 783 PG..."
- "Post-Glitch society..."
- "During the Age of Creeds..."

---

## Examples

### PASS Example

**Content:** "The Bone-Setters guild was established around 340 PG, nearly four and a half centuries after the Ginnungagap Glitch devastated the old systems."

**Why:** Uses PG notation, references Glitch as anchor point, timeline math is consistent.

### FAIL Example

**Content:** "Records from 2847 CE indicate the reactor was built during the height of the Asgardian Empire."

**Violation:** Uses real-world CE calendar system instead of Cycle notation.

**Remediation:** "Records from Cycle 847 indicate the reactor was built during the height of the Asgardian Empire."

### FAIL Example

**Content:** "The settlement was founded 1,500 years ago."

**Violation:** Ambiguous reference without era notation. 1,500 years would predate Year 0 but uses no Cycle notation.

**Remediation:** "The settlement was founded deep in the Age of Forging, its original purpose now lost to the Glitch."
