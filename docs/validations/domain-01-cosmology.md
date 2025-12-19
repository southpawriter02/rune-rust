# Domain 1: Cosmology Validation Check

**Domain ID:** `DOM-1`
**Severity:** P2-HIGH
**Applies To:** Room templates, biome definitions, world lore, location descriptions

---

## Canonical Ground Truth

The Nine Realms are **continent-sized structures** with intentional vertical and horizontal relationships. This is NOT a flat plane, but stacked mega-structures with specific spatial rules.

### Nine Realms Structure

| Realm | Position | Function |
|-------|----------|----------|
| **Asgard** | OVERHEAD of Midgard | Command/Administrative — causes the Asgardian Scar |
| **Vanaheim** | ADJACENT to Midgard | Forest/Biological — borders Midgard as continent |
| **Alfheim** | ADJACENT to Midgard | Energy/Light Systems — borders Midgard |
| **Midgard** | Central habitation | Primary Habitation — has clear-ish view of sun |
| **Svartalfheim** | Below/Adjacent | Manufacturing/Industrial |
| **Jotunheim** | Industrial sector | Heavy Industry/Cooling |
| **Muspelheim** | Thermal sector | Power Generation/Heat |
| **Helheim** | Waste sector | Waste Processing/Recycling |
| **Niflheim** | Cold sector | Cryogenic Storage/Cold* (See Note Below) |
| **The Deep** | Subterranean | Network beneath all structures |

*> **PENDING RESOLUTION:** The term "Cryogenic" is currently used in legacy docs, but there is an active debate on replacing it with "Ice-stasis" to better fit the medieval voice. For now, mark usage of "Cryogenic" as a potential voice violation if used in character dialogue.*

### Critical Spatial Constraints

- **Asgard IS overhead** of Midgard (this creates the Asgardian Scar)
- **Vanaheim/Alfheim are ADJACENT** to Midgard (continental borders, not overhead)
- **Midgard has sun visibility** through ash-filled atmosphere (clear-ish sky)
- Realms are continent-sized, stacked by original design
- No generic "upper/lower realms" terminology (use specific realm names)

---

## Validation Checklist

- [ ] Realm assignment matches canonical Nine Realms?
- [ ] Asgard correctly positioned as overhead of Midgard?
- [ ] Vanaheim/Alfheim positioned correctly (adjacent continents, not overhead)?
- [ ] Midgard sun visibility preserved (ash-filtered but visible)?
- [ ] Asgardian Scar referenced appropriately (caused by Asgard overhead)?
- [ ] Realm functions align with Setting Fundamentals?
- [ ] The Deep references appropriate (subterranean network context)?
- [ ] No non-canonical realm additions?
- [ ] No vague "upper/lower realms" (use specific realm names)?
- [ ] **NOTE:** Flag "Cryogenic" in dialogue as a potential Voice violation (prefer "Ice-stasis").

---

## Common Violations

| Pattern | Example | Why It Fails |
|---------|---------|--------------|
| Vanaheim/Alfheim overhead | "Alfheim's canopy stretches above Midgard" | Alfheim is ADJACENT, not overhead |
| Sunless Midgard | "the eternal darkness of Midgard" | Midgard HAS sun visibility (ash-filtered) |
| Wrong Asgard position | "Asgard, the distant realm" | Asgard is OVERHEAD, causes Asgardian Scar |
| Non-canonical realms | "the realm of Utgard" | Adds realms not in canonical nine |
| Vague hierarchy | "the upper realms" | Must specify which realm (e.g., "Asgard above") |
| Missing Asgardian Scar | Asgard mentioned without scar context | Asgard's position creates the Scar |

---

## Green Flags

- Asgard described as overhead/above Midgard
- Asgardian Scar attributed to Asgard's position
- Midgard sky/sun references (ash-filtered light)
- Vanaheim/Alfheim as adjacent continents/territories
- Subterranean Deep references ("beneath", "foundational systems")
- Canonical Nine Realms only
- Continent-scale descriptions for realms
- Megastructure elements within realms ("bulkhead", "access shaft")

---

## Remediation Strategies

### Option 1: Lateral Reframing

Replace vertical language with lateral/adjacent terminology:

| Before | After |
|--------|-------|
| "upper realms" | "distant sectors" |
| "descending to" | "traversing to" |
| "above Midgard" | "adjacent to Midgard" |
| "lower depths" | "The Deep" |

### Option 2: Deck Reference Conversion

Convert realm references to deck-based architecture:

| Before | After |
|--------|-------|
| "the realm of Jotunheim" | "Deck 07 (Jotunheim sector)" |
| "Vanaheim's canopy" | "Vanaheim's bio-dome structures" |
| "Asgard's heights" | "Deck 01's command infrastructure" |

---

## Decision Tree

```
Is the content referencing realm spatial relationships?
├── YES → Does it use vertical terminology (upper/lower/above/below)?
│   ├── YES → Is it referring to The Deep (subterranean)?
│   │   ├── YES → PASS (The Deep IS beneath other decks)
│   │   └── NO → FAIL (Domain 1 violation - vertical realm language)
│   └── NO → Does it use lateral terminology (adjacent/connected/sector)?
│       ├── YES → PASS
│       └── NO → Review for implicit vertical assumptions
└── NO → Continue to next check
```

---

## Examples

### PASS Example

**Content:** "The access corridor connects Midgard's eastern perimeter to the Vanaheim biodome sector through a series of pressure-sealed bulkheads."

**Why:** Uses lateral connection language, megastructure terminology, no vertical hierarchy implied.

### FAIL Example

**Content:** "Travelers ascending from Midgard can glimpse the verdant upper reaches of Vanaheim through the crystalline canopy."

**Violation:** Implies Vanaheim is above Midgard ("ascending", "upper reaches", "canopy" suggesting overhead)

**Remediation:** "Travelers crossing into the Vanaheim sector can glimpse verdant biodome structures through the reinforced viewing ports."
