# Domain 4: Technology Validation Check

**Domain ID:** `DOM-4`
**Severity:** P1-CRITICAL
**Applies To:** Room templates, biome definitions, operational protocols, hazard documentation, flavor text, all descriptive content

---

## Canonical Ground Truth

**Core Principle: "Technology works, knowledge doesn't."**

After 800 years of decay, POST-GLITCH societies can **operate** Pre-Glitch technology but cannot **understand, create, program, or modify** it.

### The Technology Paradox

| Capability | Status | Example |
|------------|--------|---------|
| **Operational Use** | YES | Can activate a Data-Slate, use a runic device |
| **Comprehension** | NO | Cannot understand WHY it works |
| **Creation** | NO | Cannot manufacture new Data-Slates |
| **Programming** | NO | Cannot reprogram or modify existing systems |
| **Repair** | LIMITED | Can replace parts, cannot redesign |

### Medieval Baseline (POST-GLITCH)

- Technology level: ~1200-1400 CE equivalent for NEW creations
- **Lost precision measurement capability**
- Scavenged PRE-GLITCH artifacts (non-replicable, degraded, unreliable)
- Systems are malfunctioning, hazardous, unpredictable
- **Vocabulary:** Inhabitants use archaic/mystical terms for scientific concepts.

### Specialist Limitations

| Role | What They Can Do | What They Cannot Do |
|------|------------------|---------------------|
| **Dvergr** | Skilled imitators, can copy/adapt | NOT innovators — cannot create new designs |
| **Jotun-Readers** | Pathologists — diagnose system states | Lack proper diagnostic tools — work by intuition |

---

## Validation Checklist

### PRIMARY VIOLATION SOURCE — CHECK CAREFULLY

**Precision Measurements (POST-GLITCH context):**
*Exception: Precision measurements are allowed within blocks marked as `> **GM Note:**` or similar OOC (Out Of Character) mechanical notes.*

- [ ] No exact percentages (e.g., "O₂ 19-20%", "95%+ casualties")?
- [ ] No decimal precision (e.g., "18-24°C", "0.3-1.2 lux")?
- [ ] No exact distances to meter precision (e.g., "800-1,200 meters")?
- [ ] No exact population counts (e.g., "10,000+ residents")?
- [ ] No precise gas concentrations (e.g., "H₂S 6-12 ppm")?
- [ ] No frequency specifications (e.g., "18-22 Hz hum")?
- [ ] No systematic measurement capabilities described?

**Scientific Vocabulary (Post-Glitch Voice):**
- [ ] **No "Cellular" / "Molecular" / "Atomic"?** (Use "Living Weave", "Essence", "Smallest Part")
- [ ] **No "Polymer" / "Plastic" / "Synthetic"?** (Use "Resin", "Old World Shell", "False-Bone")
- [ ] **No "Electricity" / "Voltage"?** (Use "Lightning", "Spark", "Power-Flow")
- [ ] **No "Radiation"?** (Use "Invisible Fire", "Sickness-Light", "Blight-Heat")

**Cargo Cult Verbs (Action & Interaction):**
- [ ] **No "Programming" / "Coding"?** (Use "Inscribing", "Writing the Laws", "Ritualizing")
- [ ] **No "Calibrating" / "Optimizing"?** (Use "Attuning", "Appeasing", "Balancing")
- [ ] **No "Repairing" (in a modern sense)?** (Use "Mending", "Healing the Machine", "Patching")
- [ ] **No "Downloading" / "Uploading"?** (Use "Communing", "Drawing Knowledge", "Offering Memories")

**Appropriate Content:**

- [ ] PRE-GLITCH technical specs describing LOST capabilities?
- [ ] Qualitative descriptions ("hot", "crowded", "dangerous")?
- [ ] Approximations with explicit uncertainty ("several days", "dozens")?
- [ ] Operational use of technology without comprehension?
- [ ] Frustration with unreliable/malfunctioning systems?

---

## Common Violations

| Category | Example | Why It Fails | Remediation |
|----------|---------|--------------|-------------|
| **Precision** | "O₂ 19-20%" | Requires gas analyzers | "Breathable but thin" |
| **Scientific** | "The cellular structure is degrading" | Too modern/scientific | "The living weave is rotting away" |
| **Scientific** | "Polymer coating" | Too modern | "Smooth, hard resin shell" |
| **Verb** | "She programmed the door to open" | Implies engineering knowledge | "She performed the opening ritual" |
| **Verb** | "He calibrated the sensor" | Implies precision tools | "He attuned the eye until it saw clearly" |

---

## Remediation Strategies

### Option 1: Qualitative Conversion (Measurements)

Replace precision with descriptive bands (unless inside `> **GM Note:**`):

| Before | After |
|--------|-------|
| "O₂ 19-20%" | "breathable but thin air" |
| "800-950°C" | "lethal heat, metal-melting temperatures" |

### Option 2: Archaic Replacement (Vocabulary)

Replace scientific terms with medieval/mystical equivalents:

| Scientific | Archaic / Setting-Appropriate |
|------------|-------------------------------|
| Cellular | Living Weave, Flesh-Pattern |
| Polymer | Resin, False-Bone, Smooth-Shell |
| Radiation | Invisible Fire, Poison-Light |
| Electricity | Lightning, Spark, Power-Blood |
| Molecule | Mote, Grain, Essence-Part |
| DNA | Blood-Record, Ancestor-Script |

### Option 3: Ritualistic Reframing (Verbs)

Replace engineering actions with ritualistic ones:

| Engineering | Cargo Cult / Ritual |
|-------------|---------------------|
| Program | Inscribe, Rite, Command |
| Calibrate | Attune, Balance, Appease |
| Repair | Mend, Heal, Bodge |
| Activate | Awaken, Spark, Rouse |
| Reboot | Kill and Revive, cycle the death-sleep |

---

## Decision Tree

```
Is the content in a POST-GLITCH context?
├── NO (PRE-GLITCH/Age of Forging documentation)
│   └── Is it describing LOST capabilities?
│       ├── YES → PASS (historical documentation appropriate)
│       └── NO → Continue to POST-GLITCH check
├── YES → Check Content Category:
│   ├── Is it a GM Note / Mechanic Block?
│   │   ├── YES → PASS (Precision/Technical terms allowed for mechanics)
│   │   └── NO → Continue checks below:
│   ├── Does it contain precision measurements?
│   │   ├── YES → FAIL (Domain 4 violation - Precision)
│   │   └── NO → Check Vocabulary
│   ├── Does it use scientific terms (cellular, polymer)?
│   │   ├── YES → FAIL (Domain 4 violation - Voice)
│   │   └── NO → Check Verbs
│   └── Does it use modern engineering verbs (program, calibrate)?
│       ├── YES → FAIL (Domain 4 violation - Cargo Cult)
│       └── NO → PASS
```
