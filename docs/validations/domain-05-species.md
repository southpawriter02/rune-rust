# Domain 5: Species Validation Check

**Domain ID:** `DOM-5`
**Severity:** P3-MEDIUM
**Applies To:** Room templates, biome definitions, encounter tables, creature descriptions, environmental flavor

---

## Canonical Ground Truth

All fauna and flora must be documented in the canonical Codex or have clear lineage to established species.

### Canonical Sentient Species

**Three sentient species exist in the setting:**

| Species | Description | Characteristics |
|---------|-------------|-----------------|
| **Humans** | Primary population | Dominant, varied cultures and factions |
| **Gorge-Maws** | Giant gorilla-shaped beings | Sloth-like temperament, friendly but reclusive |
| **Rune-Lupins** | Man-sized wolves | Telepathic, friendly but reclusive |

**Gorge-Maws:**
- Giant gorilla-like body shape
- Slow, deliberate, sloth-like behavior
- Culturally advanced but not economically integrated
- Friendly toward humans but prefer isolation
- Reclusive — rarely seen in human settlements

**Rune-Lupins:**
- Wolf-like, roughly human-sized
- Possess telepathic abilities
- Culturally advanced but not economically integrated
- Friendly toward humans but prefer isolation
- Reclusive — rarely travel in human lands

### Fauna/Flora Categories

| Category | Origin | Examples |
|----------|--------|----------|
| **Baseline Species** | Natural pre-Glitch organisms | Standard fauna adapted to realms |
| **Blight-Mutated** | Post-Glitch corrupted variants | Blight-touched wolves, corrupted flora |
| **Bio-Engineered** | Vanir/Aesir designed (pre-Glitch) | Purpose-built organisms, guardian species |
| **Automata** | Mechanoid entities | Forlorn, Draugr-Pattern, Haugbui-Class |

### Documentation Requirements

- All species should reference Codex entries or established canon
- Blight-mutated variants must identify their baseline species
- Bio-engineered species require PRE-GLITCH origin documentation
- New species introductions need appropriate worldbuilding context
- Gorge-Maws and Rune-Lupins are CANONICAL — do not treat as exotic/unknown

### Evolutionary/Mutation Constraints

- Blight mutations follow established corruption patterns
- Gene-Storm effects have documented mechanics
- Evolution occurs through Aetheric/Blight influence, not standard Darwinian processes

---

## Validation Checklist

**Sentient Species:**

- [ ] Only Humans, Gorge-Maws, and Rune-Lupins as sentient races?
- [ ] Gorge-Maws portrayed as gorilla-shaped, sloth-like, reclusive?
- [ ] Rune-Lupins portrayed as man-sized wolves, telepathic, reclusive?
- [ ] No other invented sentient races without explicit approval?
- [ ] No elves, dwarves, or traditional fantasy races?

**Fauna/Flora:**

- [ ] All fauna/flora referenced in Codex or established canon?
- [ ] Blight-mutated species reference canonical baseline?
- [ ] Bio-engineered species have PRE-GLITCH origin documentation?
- [ ] No unauthorized species additions without worldbuilding context?
- [ ] Species biology consistent with established mechanics?
- [ ] Evolutionary/mutation mechanics align with Blight/Gene-Storm lore?
- [ ] Automata/mechanoid entities follow established templates?
- [ ] Species abilities don't contradict setting physics?

---

## Common Violations

| Pattern | Example | Why It Fails |
|---------|---------|--------------|
| Undocumented species | "a pack of shadowcats" (no Codex entry) | No canonical basis |
| Origin-less bio-engineering | "engineered guardians" (no pre-Glitch context) | Implies POST-GLITCH bio-engineering capability |
| Unexplained abilities | "the creature teleported away" | Mechanism not grounded in Aetheric lore |
| Wrong evolution model | "they evolved over millennia" | 783 PG is insufficient time; use Blight mutation |
| Generic fantasy creatures | "dragons", "unicorns" | Not setting-appropriate |

---

## Green Flags

- Humans as primary population
- Gorge-Maws portrayed correctly (gorilla-like, sloth-paced, reclusive)
- Rune-Lupins portrayed correctly (wolf-like, telepathic, reclusive)
- References to documented Codex species
- Blight-mutation mechanics consistent ("Blight-touched", "corrupted variant of")
- Bio-engineering traced to Vanir/Aesir pre-Glitch programs
- Evolutionary lineages tied to Gene-Storm events
- Automata following Forlorn/Draugr-Pattern/Haugbui-Class templates

---

## Remediation Strategies

### Option 1: Codex Reference Addition

Add explicit lineage to canonical species:

| Before | After |
|--------|-------|
| "wolves prowl the ruins" | "a pack of Blight-touched wolves, their baseline forms barely recognizable beneath the corruption" |
| "strange flowers" | "specimens resembling corrupted variants of the Vanaheim stock-flora" |

### Option 2: Blight-Mutation Framing

Ground unfamiliar creatures in mutation mechanics:

| Before | After |
|--------|-------|
| "a massive insectoid predator" | "something that might once have been a maintenance drone, now grotesquely overgrown with chitinous Blight-growths" |
| "crystalline parasites" | "Blight-crystallization has consumed these organisms, leaving only mineral husks that still twitch with corrupted purpose" |

### Option 3: PRE-GLITCH Origin Documentation

For bio-engineered species, establish Age of Forging context:

| Before | After |
|--------|-------|
| "guardian beasts patrol the perimeter" | "Pre-Glitch records mention bio-engineered sentinel organisms; what remains of those programs now patrol on corrupted instinct" |

### Option 4: Generic → Specific Conversion

Replace generic fantasy creatures with setting-appropriate variants:

| Before | After |
|--------|-------|
| "dragon" | "a Blight-wyrm, once perhaps a maintenance serpent, now swollen with corrupted Aether" |
| "giant spider" | "web-spinners—Vanir-engineered silk-producers gone feral after the Glitch" |

---

## Decision Tree

```
Does the content reference fauna/flora?
├── NO → PASS (not applicable)
├── YES → Is the species documented in Codex/canon?
│   ├── YES → Do abilities/behaviors match established lore?
│   │   ├── YES → PASS
│   │   └── NO → FAIL (behavioral contradiction)
│   └── NO → Is it presented as a variant/mutation of known species?
│       ├── YES → Is the mutation mechanism Blight/Gene-Storm consistent?
│       │   ├── YES → FLAG for Codex documentation, soft PASS
│       │   └── NO → FAIL (unexplained mutation)
│       └── NO → Is it a generic fantasy creature?
│           ├── YES → FAIL (setting-inappropriate)
│           └── NO → FLAG for worldbuilding review
```

---

## Species Introduction Guidelines

When new species must be introduced:

1. **Establish Baseline:** What was this before the Glitch?
2. **Define Mutation Vector:** Blight exposure? Gene-Storm? Bio-engineering remnant?
3. **Ground Abilities:** All supernatural traits through Aetheric mechanics
4. **Document Habitat:** Which realm/biome? Why there?
5. **Consider Ecosystem:** What does it eat? What threatens it?

---

## Examples

### PASS Example

**Content:** "Blight-wolves prowl the perimeter—recognizable as canines only by their general shape. The corruption has elongated their limbs, split their jaws, and given them an unsettling, loping gait. They hunt in packs still, that instinct uncorrupted."

**Why:** Establishes baseline (wolves), explains mutation (Blight corruption), describes specific changes, preserves behavioral continuity.

### FAIL Example

**Content:** "A phoenix rose from the cooling vents, wings of pure flame lighting the darkness."

**Violation:** Generic fantasy creature with no setting grounding or Aetheric explanation.

**Remediation:** "Something rose from the cooling vents—perhaps a maintenance drone once, now a skeletal frame wreathed in unstable Aetheric discharge. The locals call them fire-wraiths, and flee when they appear."

### FAIL Example

**Content:** "The forests are home to many strange creatures that have evolved since the Glitch."

**Violation:** Implies Darwinian evolution over 783 years (insufficient time), no Blight/mutation context.

**Remediation:** "The forests teem with Blight-touched wildlife, baseline organisms warped by centuries of corrupted Aether exposure. Each generation seems stranger than the last."
