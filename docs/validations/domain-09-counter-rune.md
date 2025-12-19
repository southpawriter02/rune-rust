# Domain 9: Counter-Rune Validation Check

**Domain ID:** `DOM-9`
**Severity:** P1-CRITICAL
**Applies To:** All content, especially ability specs, runic references, lore documentation, dialogue trees

---

## Canonical Ground Truth

The Counter-Rune (Eihwaz ᛇ) is **restricted Apocrypha-level content**. Its existence, nature, and mechanics are protected mysteries not freely referenced in standard canon.

### Counter-Rune Overview

| Aspect | Constraint |
|--------|------------|
| **Name** | Eihwaz ᛇ (Counter-Rune) |
| **Status** | Restricted Apocrypha-level content |
| **Nature** | BOTH Parasitic AND World-ending (see below) |
| **Knowledge** | NOT widespread; closely guarded secret |
| **Appropriate Context** | Academic research, containment protocols only |

### THE DUAL NATURE PARADOX (Critical Canon)

**The Counter-Rune is SIMULTANEOUSLY:**

| Aspect | Function | Implication |
|--------|----------|-------------|
| **PARASITIC** | Feeds Blight with "living noise" | PREVENTS Blight collapse — necessary evil |
| **WORLD-ENDING** | Full activation wipes all life | Genocidal reset mechanism |

**BOTH are true at the same time.** This is not a contradiction to resolve — it is the nature of the Counter-Rune.

- If the Counter-Rune stops feeding the Blight → Blight collapses catastrophically
- If the Counter-Rune is fully activated → All life ends (genocidal reset)
- This paradox is **deliberately undefined** — do not resolve it
- Apocrypha-level mystery with no clean answers

### Propagation Rules

- Counter-Rune knowledge restricted to Apocrypha Catalogue
- References require appropriate context (academic study, containment research)
- No casual mentions in standard content
- Jotun-Reader containment research is the primary appropriate context
- Mystery protection protocols apply—do not over-explain
- **Do NOT resolve the dual nature** — both aspects must remain true

### Mystery Protection

The Counter-Rune's power partly comes from its mystery. Content should:
- Preserve uncertainty about its nature
- Avoid definitive explanations of its mechanics
- Maintain the sense that knowledge itself is dangerous
- Treat references as inherently significant, never casual
- **Never choose one aspect over the other** (parasitic OR world-ending — always BOTH)

---

## Validation Checklist

- [ ] Counter-Rune (Eihwaz) absent from standard canon content?
- [ ] If present, is context appropriate (research/containment)?
- [ ] No casual mentions or widespread knowledge implied?
- [ ] Mystery protection maintained (not over-explained)?
- [ ] **Dual nature preserved** — BOTH parasitic AND world-ending?
- [ ] Dual nature NOT resolved (both aspects remain true simultaneously)?
- [ ] Restricted to Apocrypha-level documentation if detailed?
- [ ] Characters don't have unexplained Counter-Rune knowledge?
- [ ] No "easy" Counter-Rune use or understanding depicted?
- [ ] No claim that Counter-Rune is "just" parasitic or "just" world-ending?

---

## Common Violations

| Pattern | Example | Why It Fails |
|---------|---------|--------------|
| Casual mention | "She carved the Eihwaz rune" | Implies common knowledge |
| Widespread knowledge | "Everyone knew about the Counter-Rune" | Contradicts restricted status |
| Over-explanation | "The Counter-Rune works by..." (detailed mechanics) | Violates mystery protection |
| Resolved duality | "The Counter-Rune is really just parasitic" | BOTH natures must remain true |
| Single-nature claim | "It's world-ending, not parasitic" | BOTH natures must remain true |
| Clean solution | "If we use the Counter-Rune correctly..." | No clean answers exist |
| Easy use | "He activated the Counter-Rune effortlessly" | Implies understood mechanics |
| Standard canon inclusion | Detailed Eihwaz reference in regular gazetteer | Should be Apocrypha only |

---

## Green Flags

- Zero Counter-Rune content in most entries (appropriate absence)
- Restricted to Apocrypha Catalogue when present
- Research/containment context only
- Mystery protection maintained
- Vague, ominous references if any
- Characters treating the topic with appropriate fear/caution

---

## Appropriate vs. Inappropriate Contexts

### APPROPRIATE Contexts

| Context | Example | Why Appropriate |
|---------|---------|-----------------|
| Apocrypha research logs | "The containment team documented observations of Eihwaz manifestation..." | Restricted documentation |
| Jotun-Reader containment protocols | "Protocols for suspected Counter-Rune exposure..." | Professional containment context |
| Academic speculation | "Some theorize a 'counter-rune' exists, but evidence is fragmentary..." | Uncertainty preserved |
| Horror/dread framing | "She recognized the symbol and felt cold terror—this should not exist here" | Emotional weight, no explanation |

### INAPPROPRIATE Contexts

| Context | Example | Why Inappropriate |
|---------|---------|-------------------|
| Casual conversation | "Pass me that Eihwaz-inscribed tool" | Implies everyday familiarity |
| Standard ability text | "This ability uses the Counter-Rune" | Standard content, not Apocrypha |
| Educational material | "Eihwaz is the 13th rune of Elder Futhark and functions as..." | Over-explains, removes mystery |
| Player-accessible power | "At level 10, you can wield the Counter-Rune" | Makes restricted content common |

---

## Remediation Strategies

### Option 1: Complete Removal

If Counter-Rune appears inappropriately in standard content, remove entirely:

| Before | After |
|--------|-------|
| "She used the Eihwaz rune to disrupt the paradox" | "She used a dispersal Galdr to disrupt the paradox" |
| "Counter-Rune knowledge was common" | Remove entirely, or replace with different forbidden knowledge |

### Option 2: Context Restriction

Move Counter-Rune content to appropriate Apocrypha context:

| Before | After |
|--------|-------|
| Eihwaz details in standard ability spec | Move to restricted Apocrypha document, reference obliquely in standard spec |
| Counter-Rune explanation in gazetteer | "Some ruins bear symbols the Jotun-Readers refuse to document" (no specifics) |

### Option 3: Mystery Restoration

If reference must remain, restore mystery:

| Before | After |
|--------|-------|
| "The Counter-Rune negates other runes by..." | "The symbol defied understanding. Those who studied it too long stopped speaking." |
| "Eihwaz functions through parasitic Aetheric resonance" | "Something about Eihwaz—no one wanted to say what they suspected" |

### Option 4: Vague Reference Conversion

Convert specific references to atmospheric hints:

| Before | After |
|--------|-------|
| "the Eihwaz inscription" | "a symbol she had been warned never to describe" |
| "Counter-Rune exposure" | "exposure to something that shouldn't exist" |
| "using Eihwaz" | "forbidden methods no sane Runasmidr would attempt" |

---

## Decision Tree

```
Does the content reference Eihwaz / Counter-Rune?
├── NO → PASS (appropriate absence)
├── YES → Is this Apocrypha-level documentation?
│   ├── YES → Is context appropriate (research/containment)?
│   │   ├── YES → Is mystery protection maintained (not over-explained)?
│   │   │   ├── YES → PASS
│   │   │   └── NO → FAIL (over-explanation)
│   │   └── NO → FAIL (inappropriate Apocrypha use)
│   └── NO → Is this a vague, atmospheric reference?
│       ├── YES → Does it imply widespread knowledge?
│       │   ├── NO → PASS (acceptable oblique reference)
│       │   └── YES → FAIL (implies common knowledge)
│       └── NO → FAIL (specific Counter-Rune in standard content)
```

---

## Severity Assessment

| Violation Type | Severity | Action |
|----------------|----------|--------|
| Detailed mechanics in standard content | P1-CRITICAL | Immediate removal or relocation |
| Casual mention implying common knowledge | P1-CRITICAL | Removal or mystery restoration |
| Vague reference with too much specificity | P2-HIGH | Mystery restoration |
| Atmospheric hint that's slightly too clear | P3-MEDIUM | Minor adjustment |

---

## Examples

### PASS Example (Appropriate Absence)

**Content:** "The Runasmidr's work used the traditional Elder Futhark inscriptions—Fehu for wealth-flow, Uruz for strength, Thurisaz for protection. Each rune carefully selected, carefully placed."

**Why:** Standard runic content without Counter-Rune reference. This is the expected default.

### PASS Example (Appropriate Apocrypha Context)

**Content:** [In Apocrypha document, marked RESTRICTED]
"Containment Protocol 7-E: Suspected Eihwaz Manifestation

Field teams encountering inscriptions matching Pattern Eihwaz-7 are to:
1. Immediately evacuate the area
2. Avoid visual study of the inscription
3. Report to containment division
4. Submit to cognitive screening within 6 hours

Note: The nature of Eihwaz remains... uncertain. What we know: looking too long is dangerous. What we don't know: almost everything else."

**Why:** Restricted context, appropriate professional framing, mystery preserved (admits uncertainty), no casual treatment.

### FAIL Example

**Content:** "Every apprentice Runasmidr learned about the Counter-Rune, Eihwaz, which could negate other runic effects through parasitic Aetheric resonance. It was dangerous but useful."

**Violations:**
- "Every apprentice" implies common knowledge (restricted)
- "learned about" implies educational curriculum (inappropriate)
- Mechanical explanation (violates mystery protection)
- "dangerous but useful" implies routine consideration (too casual)

**Remediation:** Remove entirely from standard content. If Counter-Rune must be referenced in advanced content, use: "There were symbols that senior Runasmidr warned against. One in particular—they wouldn't even name it. 'Some knowledge,' the old inscriptionist said, 'wants to be known. And that wanting is the danger.'"

### FAIL Example

**Content:** "She drew the Eihwaz rune, activating its counter-effect."

**Violation:** Casual use of specific Counter-Rune with implied mechanical understanding.

**Remediation:** "She drew a dispersal rune, its design harsh and angular—something she'd learned from a source she didn't speak of."

Or, if Counter-Rune use is plot-critical (Apocrypha context only): "She drew the symbol—the one she'd sworn never to use. Her hand shook. The rune seemed to want to complete itself, and that terrified her more than anything."
