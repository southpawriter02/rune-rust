# Aethelgard Setting Compliance Guide

Parent item: Specification Writing Guide (Specification%20Writing%20Guide%202ba55eb312da8032b14de6752422e93e.md)

> Layer 0: Specifications - Setting Governance
> 
> 
> This document ensures all specifications comply with Aethelgard's canonical setting fundamentals. Use during specification writing, review, and implementation to catch setting contradictions.
> 

---

## Table of Contents

1. [Purpose & Integration](Aethelgard%20Setting%20Compliance%20Guide%202ba55eb312da8014b792e98207480e2b.md)
2. [Quick Compliance Check](Aethelgard%20Setting%20Compliance%20Guide%202ba55eb312da8014b792e98207480e2b.md)
3. [Domain Validation Checklists](Aethelgard%20Setting%20Compliance%20Guide%202ba55eb312da8014b792e98207480e2b.md)
4. [Voice & Tone Standards](Aethelgard%20Setting%20Compliance%20Guide%202ba55eb312da8014b792e98207480e2b.md)
5. [Common Violations & Fixes](Aethelgard%20Setting%20Compliance%20Guide%202ba55eb312da8014b792e98207480e2b.md)
6. [Implementation Validation](Aethelgard%20Setting%20Compliance%20Guide%202ba55eb312da8014b792e98207480e2b.md)

---

## Purpose & Integration

### What is Setting Compliance?

**Setting Compliance** ensures that all specifications, implementations, and content adhere to Aethelgard's canonical ground truth across 9 setting domains. This prevents contradictions from entering the codebase and maintains narrative consistency.

### Why It Matters

**For Specifications**:

- Catches lore contradictions before implementation
- Ensures design philosophy aligns with setting constraints
- Maintains immersion through consistent worldbuilding

**For Implementation**:

- Prevents AI from generating setting-breaking content
- Guides flavor text, descriptions, and naming
- Ensures data (enemies, items, locations) respects canon

**For Players**:

- Consistent, believable world
- No immersion-breaking contradictions
- Coherent narrative experience

### When to Use This Guide

**During Specification Writing**:

- Check "Design Constraints" section against relevant domains
- Validate examples use canonical terminology
- Ensure mechanics don't contradict setting rules

**During Specification Review**:

- Run through Quick Compliance Check
- Validate against applicable domain checklists
- Flag contradictions for resolution before approval

**During Implementation**:

- Reference when writing flavor text, descriptions
- Validate generated content (item names, enemy types, location descriptions)
- Check AI-generated content against Voice & Tone standards

---

## Quick Compliance Check

**Before approving any specification, answer these questions**:

### Critical Questions (Must Answer "No" to All)

- [ ]  **Does this contradict the current year (783 PG)?**
- [ ]  **Does this reference "Galdr" or "Unraveled" as entity types?** (Galdr = evocation discipline only)
- [ ]  **Does this describe pre-Glitch magic users?**
- [ ]  **Does this allow creating new Data-Slates or programming Pre-Glitch systems?**
- [ ]  **Does this present Jötun-Readers as having precision measurement tools?**
- [ ]  **Does this describe traditional spell-casting without runic focal points?**
- [ ]  **Does this position Vanaheim/Alfheim directly overhead Midgard?** (They're adjacent)
- [ ]  **Does this resolve the Counter-Rune paradox?** (Must remain Apocrypha)
- [ ]  **Does this describe only humans as sentient?** (Gorge-Maws and Rune-Lupins exist)
- [ ]  **Does this describe pristine/reliable Pre-Glitch systems?** (800 years = decay)

### Setting References (Should Use Canonical Terms)

- [ ]  **Uses "Aether" instead of "mana/magic energy"**
- [ ]  **Uses "Legend" instead of "XP/experience"**
- [ ]  **Uses "Iron Hearts" for Draugr/Haugbui power sources**
- [ ]  **Uses "Weaving" for Aetheric manipulation (not spell-casting)**
- [ ]  **References "Ginnungagap Glitch" for Year 0 PG**
- [ ]  **Uses "Layer 2 Diagnostic Voice" for technical/machine aesthetic**

---

## Domain Validation Checklists

> Usage: Identify which domains apply to your specification (see table below), then validate against those domain checklists.
> 

### Domain Applicability Table

| Specification Type | Primary Domains to Check |
| --- | --- |
| Combat Systems | Reality Rules (7), Entity Types (6), Magic System (3) |
| Progression Systems | Timeline (2), Magic System (3) |
| Enemy/NPC Systems | Entity Types (6), Species (5), Technology (4) |
| Equipment/Loot | Technology (4), Magic System (3) |
| Procedural Generation | Geography (8), Cosmology (1) |
| Narrative/Dialogue | Timeline (2), Species (5), Voice/Tone |
| Trauma Economy | Reality Rules (7), Magic System (3) |
| Crafting Systems | Technology (4), Magic System (3) |
| World/Exploration | Geography (8), Cosmology (1), Technology (4) |

---

### 🔮 Domain 1: Cosmology & Spatial Layout

**When to Check**: Geography, exploration, procedural generation, environmental descriptions

**Canonical Statements**:

- ✅ Midgard (habitation deck) has clear-ish view of sun through ash-filled atmosphere
- ✅ Vanaheim (massive forest) borders Midgard as **adjacent continent** (not overhead)
- ✅ Alfheim (research facility) hidden atop Vanaheim or connected to Asgard
- ✅ Asgard (orbital command station) **overhead**, causing Asgardian Scar
- ✅ Sub-Midgard realms (Jötunheim, Svartalfheim, Helheim, Niflheim, Muspelheim) form infrastructure layers
- ✅ Realms are continent-sized structures, **stacked by design** (not crash-landed randomly)

**Forbidden Contradictions**:

- ❌ All nine realms crash-landed and stacked randomly
- ❌ Vanaheim/Alfheim positioned directly overhead Midgard
- ❌ Canopy Sea blocks sun visibility from Midgard
- ❌ Vertical stack without intentional design

**Validation Questions**:

- [ ]  Does this specification describe realm spatial arrangement?
- [ ]  Does it reference sun/light visibility?
- [ ]  Does it use "crash-landed orbital bodies" language?
- [ ]  Does it contradict adjacent vs. overhead positioning?
- [ ]  Does it describe realms as randomly stacked vs. designed?

**Example Compliance**:

```markdown
✅ GOOD: "The Muspelheim Forge biome features geothermal vents from the
infrastructure layer beneath Midgard, their heat radiating upward through
intentionally designed thermal shafts."

❌ BAD: "The Muspelheim orbital fragment crashed into the Aethelgard stack,
its random positioning creating unpredictable thermal zones."

```

---

### 📅 Domain 2: Historical Timeline

**When to Check**: Lore references, NPC dialogue, progression milestones, quest narratives

**Canonical Statements**:

- ✅ **Current year: 783 PG (Post-Glitch)**
- ✅ Ginnungagap Glitch: **Year 0 PG** (system-wide cascade failure)
- ✅ Five ages:
    - Forging (pre-Glitch)
    - Silence (0-122 PG)
    - Wandering (122-287 PG)
    - Walls (287-518 PG)
    - Creeds (518-783 PG, current)
- ✅ Glitch mechanics: YGGDRASIL network core failure, recursive logic error in ODIN protocol, 23-minute propagation

**Forbidden Contradictions**:

- ❌ Alternate Glitch dates or mechanisms
- ❌ Different age boundaries
- ❌ Conflicting current year

**Validation Questions**:

- [ ]  Does this specification reference historical dates?
- [ ]  Does it describe Glitch mechanics?
- [ ]  Does it use age terminology (e.g., "Age of Creeds")?
- [ ]  Are all dates consistent with 783 PG current year?

**Example Compliance**:

```markdown
✅ GOOD: "Milestone 3 represents a character's transition from novice to
veteran within the Age of Creeds (current era, 518-783 PG)."

❌ BAD: "Milestone 3 marks 800 years since the Glitch" (contradicts 783 PG)

```

---

### ✨ Domain 3: Magic/Aetheric System

**When to Check**: Abilities, progression, combat mechanics, NPC descriptions, equipment enchantments

**Canonical Statements**:

- ✅ **Pre-Glitch: No "magic"** beyond runic surface effects; runes imbued power on host surfaces only
- ✅ **Post-Glitch: Glitch "created" magic systems** by enabling rune-based energy manipulation
- ✅ **Weaving disciplines**: Runes act as focal points/conduits; weavers sense, tap into, and manipulate Aetheric energy through runic channels
- ✅ **Nordic-themed variations**: Rune-Wardens, Shamans, Blot-Priests, Galdr-Casters (evokers only, not entities)
- ✅ **No traditional spellcasting**: No wizards, sorcerers, or fireball-throwing disciplines

**Forbidden Contradictions**:

- ❌ Pre-Glitch magic users
- ❌ Traditional spell-casting without runic focal points
- ❌ "Galdr" as entity type (Galdr = evocation discipline only)
- ❌ Unrestricted Aetheric manipulation without runes

**Validation Questions**:

- [ ]  Does this specification describe magic/Aetheric manipulation?
- [ ]  Does it reference pre-Glitch magical capabilities?
- [ ]  Does it use "Galdr" as entity type (should be discipline only)?
- [ ]  Does it describe weaving disciplines correctly (runic focal points)?
- [ ]  Does it use "spell-casting" terminology (forbidden)?

**Example Compliance**:

```markdown
✅ GOOD: "Mystic archetype weavers channel Aether through runic glyphs
etched on their armor, using the runes as focal points for energy
manipulation."

❌ BAD: "Mystics cast spells by drawing on their internal mana pool and
speaking arcane words of power."

✅ GOOD: "Galdr-Casters specialize in evocation weaving, using vocal
resonance to channel Aether through throat-runes."

❌ BAD: "Galdr enemies are corrupted spellcasters who haunt the ruins."

```

---

### ⚙️ Domain 4: Technology Constraints

**When to Check**: Equipment, crafting, NPCs (especially Jötun-Readers), Data-Slates, Pre-Glitch systems, Dvergr capabilities

**Canonical Statements**:

- ✅ **800 years of decay**: Pre-Glitch systems persist but are malfunctioning, hazardous, unreliable; operating in corrupted/unintended capacity
- ✅ **Semi-habitable at best**: Can make areas livable through improvisation but cannot restore Pre-Glitch control levels
- ✅ **Operational use without comprehension**: Can activate/use systems (lockpick consoles, trigger mechanisms, read Data-Slates) but cannot create, program, or modify them
- ✅ **Lost manufacturing capability**: No complex assembly for ~800 years; **cannot create new Data-Slates or reprogram/rewrite existing content on slates**
- ✅ **Lost precision measurement**: No capability to measure technical data (CPS thresholds, pH levels, Richter scales, diagnostic telemetry); Jötun-Readers lack comprehensive tools for truly scientific study
- ✅ **Scavenger/tinkerer culture**: Jury-riggers and bodge artists; keep systems semi-functional through improvisation, not mastery
- ✅ **Dvergr exceptionalism (limited)**: More advanced due to discipline/lineage; **skilled imitators (not innovators)**; Aether-blindness limits advancement
- ✅ **Cargo cult pathology**: Jötun-Readers are pathologists without proper diagnostic tools; study decay, not systems; oral tradition (Skaldic myths) replaces technical literacy

**Forbidden Contradictions**:

- ❌ Jötun-Readers with precision measurement/diagnostic tools
- ❌ Programming, modification, or manufacturing of Pre-Glitch systems
- ❌ Creating new Data-Slates or rewriting existing slate content
- ❌ Technical comprehension beyond operational use
- ❌ Dvergr as innovators (they're skilled imitators only)
- ❌ Pristine/reliable Pre-Glitch systems (800 years = decay/malfunction/repurposing)
- ❌ Jötun-Readers as "debuggers" or "programmers of reality"

**Validation Questions**:

- [ ]  Does this specification describe Jötun-Reader measurement/diagnostic capabilities?
- [ ]  Does it reference programming, modification, or manufacturing?
- [ ]  Does it describe creating/rewriting Data-Slates?
- [ ]  Does it imply technical comprehension beyond operational use?
- [ ]  Does it present Dvergr as innovators rather than imitators?
- [ ]  Does it describe Pre-Glitch systems as pristine/reliable (should be decayed/malfunctioning)?
- [ ]  Does it present Jötun-Readers as debuggers/programmers?
- [ ]  Does it describe precision measurements (pH, CPS thresholds, etc.)?

**Example Compliance**:

```markdown
✅ GOOD: "Jötun-Reader NPCs can identify Blight-corrupted zones through
qualitative observation (visual distortions, psychic discomfort) but cannot
measure exact CPS thresholds."

❌ BAD: "Jötun-Readers use Field Cards to measure precise CPS levels and
calibrate their instruments accordingly."

✅ GOOD: "Equipment can be scavenged from malfunctioning Pre-Glitch armories,
their targeting systems erratic and power cells unreliable after 800 years."

❌ BAD: "Pristine Pre-Glitch weapons are discovered in sealed vaults,
functioning perfectly as designed."

✅ GOOD: "Dvergr craftsmen can replicate the surface patterns of runic armor
with exceptional precision, though they cannot comprehend the underlying
Aetheric principles."

❌ BAD: "Dvergr engineers innovate new runic configurations, advancing beyond
Pre-Glitch designs."

```

**Critical Note**: This domain has highest remediation volume in v4.0 content. Be especially vigilant with equipment descriptions, crafting mechanics, and Jötun-Reader capabilities.

---

### 🐺 Domain 5: Sentient Species

**When to Check**: NPC systems, faction systems, dialogue, species lists, character creation

**Canonical Statements**:

- ✅ **Humans**: Primary inhabitants, post-Glitch survivors
- ✅ **Gorge-Maws**: Giant gorilla-shaped, sloth-like temperament; no eyes, tooth-riddled mouth; rumbling ent-like language; tremor sensing; long arms with huge clawed hands; **friendly but reclusive**; culturally/societally advanced but not economically
- ✅ **Rune-Lupins**: Man-sized telepathic wolves; glowing runes etched on skin; **friendly but reclusive**; culturally/societally advanced but not economically
- ✅ **No sentient status**: Undying (automata), constructs, AI systems

**Forbidden Contradictions**:

- ❌ Only humans as sentient species
- ❌ Gorge-Maws or Rune-Lupins absent from setting
- ❌ Hostile Gorge-Maws/Rune-Lupins
- ❌ Economically advanced non-human species

**Validation Questions**:

- [ ]  Does this specification list sentient species?
- [ ]  Does it describe non-human sapients?
- [ ]  Does it contradict Gorge-Maw/Rune-Lupin descriptions (friendly, reclusive)?
- [ ]  Does it present non-human species as economically integrated?
- [ ]  Does it describe Undying/constructs/AI as sentient?

**Example Compliance**:

```markdown
✅ GOOD: "NPCs include human faction members, with rare encounters with
Gorge-Maws (friendly if approached peacefully) and Rune-Lupins (telepathic
communication available to high-WITS characters)."

❌ BAD: "Sentient species: Humans only. Gorge-Maws and Rune-Lupins are
bestial creatures without intelligence."

✅ GOOD: "Gorge-Maw traders are culturally sophisticated but operate on
barter systems, avoiding human economic structures."

❌ BAD: "Gorge-Maw merchants run a thriving business empire across Midgard."

```

---

### 🤖 Domain 6: Entity Types

**When to Check**: Enemy design, combat encounters, NPC types, loot drops (Iron Hearts)

**Canonical Statements**:

- ✅ **Draugr-Pattern (Military/Security Automata)**: Civil security units; patrol-loop corrupted; pacification tools became lethal; **powered by Iron Hearts**; predictable routes
- ✅ **Haugbui-Class (Heavy Labor Automata)**: Construction/demolition units; task-loop corrupted (STACK/CLEAR/DECONSTRUCT); **powered by Iron Hearts**; obsessive task execution
- ✅ **Symbiotic Plate ("Iron Husk" AI)**: **DIFFERENT mechanism** — bio-mechanical neural interface; bonds to living host; slowly subjugates consciousness; creates "Husks" (living puppets); corrupted AI assumes control
- ✅ **Data-Wraith Consolidation**: Remove "Galdr" and "Unraveled" as separate entity types; "Galdr" = evocation discipline only
- ✅ **Distinction preserved**: Draugr/Haugbui = automata with Iron Hearts; Symbiotic Plate = parasitic AI interface

**Forbidden Contradictions**:

- ❌ Draugr/Haugbui terminology blurred or merged
- ❌ Symbiotic Plate as Iron Heart-powered automaton
- ❌ "Galdr" or "Unraveled" as entity types
- ❌ Husk-corruption mechanism confused with Iron Heart mechanics

**Validation Questions**:

- [ ]  Does this specification describe Draugr or Haugbui?
- [ ]  Does it describe Symbiotic Plate mechanics correctly?
- [ ]  Does it use "Galdr"/"Unraveled" as entity types?
- [ ]  Does it blur Draugr/Haugbui/Husk distinctions?
- [ ]  Does it correctly attribute Iron Hearts to Draugr/Haugbui (not Husks)?

**Example Compliance**:

```markdown
✅ GOOD: "Draugr-Pattern enemies patrol fixed routes, their Iron Hearts
providing centuries of corrupted operation. Defeating them grants Iron Heart
crafting components."

❌ BAD: "Draugr and Haugbui are similar enemy types, both heavy labor units."

✅ GOOD: "Symbiotic Plate enemies are living humans subjugated by parasitic
AI interfaces, distinct from Iron Heart-powered automata."

❌ BAD: "Husks are powered by corrupted Iron Hearts that control their bodies."

✅ GOOD: "Galdr-Casters (NPC specialization) use evocation weaving to
channel Aether through vocal runes."

❌ BAD: "Galdr enemies haunt the ruins, their spectral forms flickering."

```

---

### 🌀 Domain 7: Reality/Logic Rules

**When to Check**: Trauma economy, status effects, environmental hazards, Blight mechanics, paradox exposure

**Canonical Statements**:

- ✅ **Runic Blight**: Infinitely recursive paradoxical code; "ghost process" of All-Rune's failed execution
- ✅ **CPS (Cognitive Paradox Syndrome)**: Neurological degradation from exposure to paradoxical information
- ✅ **Paradox constraints**: Direct contradiction exposure causes degradation; thresholds documented per exposure type
- ✅ **Aetheric substrate**: Unified quantum energy field; planet's operating system

**Forbidden Contradictions**:

- ❌ Alternate Blight origin theories presented as fact
- ❌ CPS as purely psychological
- ❌ Paradox exposure without consequences

**Validation Questions**:

- [ ]  Does this specification describe Runic Blight mechanics?
- [ ]  Does it reference CPS?
- [ ]  Does it describe paradox exposure without consequences?
- [ ]  Does it contradict Aetheric substrate as "operating system" metaphor?

**Example Compliance**:

```markdown
✅ GOOD: "Corruption accumulation represents proximity to Runic Blight's
paradoxical code, manifesting as neurological degradation (CPS symptoms)."

❌ BAD: "Corruption is purely psychological stress from living in a harsh
world."

✅ GOOD: "Encountering Forlorn enemies exposes players to low-level paradox,
triggering Psychic Stress accumulation as the brain rejects contradictory
information."

❌ BAD: "Paradox exposure has no mechanical effect; it's purely narrative."

```

---

### 🗺️ Domain 8: Geographic Fundamentals

**When to Check**: Biome design, procedural generation, dungeon themes, location descriptions

**Canonical Statements**:

- ✅ **Nine realms baseline**:
    - Midgard (habitation)
    - Asgard (orbital)
    - Alfheim (research)
    - Vanaheim (forest)
    - Jötunheim (industrial)
    - Svartalfheim (fabrication)
    - Helheim (waste)
    - Niflheim (cryo)
    - Muspelheim (geothermal)
- ✅ **Spatial relationships**: See Domain 1 (Cosmology)
- ✅ **Key locations**: Scriptorium-Primus (Midgard), Asgardian Scar (impact site), Bifröst Bridge remnants

**Forbidden Contradictions**:

- ❌ Realm count other than nine
- ❌ Alternate realm purposes
- ❌ Contradictory spatial relationships

**Validation Questions**:

- [ ]  Does this specification describe realm geography?
- [ ]  Does it reference key locations?
- [ ]  Does it contradict realm purposes?
- [ ]  Does it describe biomes that don't match realm themes?

**Example Compliance**:

```markdown
✅ GOOD: "Muspelheim biome features geothermal hazards from the realm's
original purpose as climate regulation infrastructure, now corrupted into
deadly heat vents."

❌ BAD: "The frozen wastes of Muspelheim provide cryo-preservation challenges."

✅ GOOD: "Jötunheim dungeons feature industrial machinery from the realm's
manufacturing purpose, now malfunctioning after 800 years."

❌ BAD: "Jötunheim is a residential district with apartment complexes."

```

---

### ⚠️ Domain 9: Counter-Rune Paradox (Apocrypha-Level)

**When to Check**: Late-game content, Apocrypha-tier lore, endgame mechanics, faction motivations

**Canonical Statements**:

- ✅ **Dual nature (BOTH true simultaneously)**:
    1. **Parasitic (Self-Licking Wound)**: Counter-Rune averts collapse by feeding Blight with "living noise"; ensures sickness never ends; JORM signatures surge when Blight dips below threshold
    2. **World-Ending (Mirror Key)**: Counter-Rune stabilizes reality ONLY by restoring Aesir perfection; full activation reinstalls pre-Glitch control logic with genocidal failsafes; "reformats" Aethelgard and wipes all life
- ✅ **Mystery status**: Apocrypha-level (deliberately undefined resolution)
- ✅ **Foreshadowing permitted**: Field telemetry showing negative correlation, JORM_MODE flags, Tiwaz legal subroutines

**Forbidden Contradictions**:

- ❌ Counter-Rune as purely beneficial
- ❌ Counter-Rune as purely malevolent
- ❌ Resolution of paradox (must remain Apocrypha)
- ❌ Single-nature explanations

**Validation Questions**:

- [ ]  Does this specification describe Counter-Rune mechanics?
- [ ]  Does it resolve the parasitic/world-ending paradox?
- [ ]  Does it contradict dual-nature canon?
- [ ]  Does it present the Counter-Rune as definitively good or evil?

**Example Compliance**:

```markdown
✅ GOOD: "Late-game Data-Slates reference JORM_MODE protocols with conflicting
telemetry: some logs show Blight stabilization, others hint at extinction-level
failsafes. The contradiction is never resolved."

❌ BAD: "The Counter-Rune is revealed to be a benevolent system designed to
save Aethelgard from the Blight."

✅ GOOD: "Faction questlines present conflicting theories: Jötun-Readers believe
activation prevents collapse; Godsleepers fear it triggers genocide. Neither
is proven definitively."

❌ BAD: "Research confirms the Counter-Rune is purely parasitic, feeding the
Blight to ensure eternal suffering."

```

**Critical Note**: This domain is Apocrypha-level. Foreshadowing and ambiguity are encouraged; resolution is forbidden.

---

## Voice & Tone Standards

### Layer 2 Diagnostic Voice

**When to Use**: Technical descriptions, Pre-Glitch systems, machine/automaton dialogue, Data-Slate excerpts, flavor text for corrupted technology

**Characteristics**:

- Clinical, technical terminology
- Avoids flowery prose
- Uses machine/diagnostic language
- Emphasizes decay and malfunction
- Post-apocalyptic technical aesthetic

**Examples**:

```markdown
✅ GOOD (Layer 2 Diagnostic Voice):
"COMBAT DIAGNOSTICS: Initiative protocol executing. Dice pool resolution:
FINESSE attribute cross-referenced with corruption factors. Turn order
sequence calculated. Combatant priority queue established."

❌ BAD (Too Fantasy/Flowery):
"As the battle begins, your keen reflexes allow you to strike with lightning
speed, outmaneuvering your foes with graceful precision."

✅ GOOD (Flavor Text for Corrupted System):
"SYSTEM_STATUS: DEGRADED. Targeting array coherence: 37%. Recommended action:
Maintenance protocol. Override: COMBAT_MODE_ACTIVE. Error: Pacification tools
repurposed for lethal application. Advisory: Approach with extreme caution."

❌ BAD (Generic Fantasy):
"The ancient guardian awakens, its eyes glowing with malevolent power."

```

### Canonical Terminology

**Use These Terms (Not Alternatives)**:

| Use This | NOT This | Context |
| --- | --- | --- |
| Aether | Mana, magic energy, arcane power | Energy manipulation |
| Legend | XP, experience points, skill points | Character progression |
| Weaving | Spell-casting, magic use | Aetheric manipulation |
| Iron Hearts | Power cores, energy cells | Draugr/Haugbui power source |
| Ginnungagap Glitch | The Cataclysm, The Fall | Year 0 PG event |
| Post-Glitch | Post-apocalyptic | Era descriptor |
| 783 PG | Current year, present day | Timeline reference |
| Runic Blight | Corruption, the Darkness | Paradoxical corruption |
| CPS | Madness, insanity | Cognitive Paradox Syndrome |
| Data-Slates | Tablets, scrolls, books | Pre-Glitch information storage |
| Jury-riggers | Engineers, technicians | Tech tinkerers |

### Nordic-Themed Naming

**Preferred Patterns**:

- Use Old Norse-inspired names: Draugr, Haugbui, Forlorn, Einherjar
- Compound descriptors: Rune-Warden, Bone-Setter, Jötun-Reader, Rust-Witch
- Avoid generic fantasy: No "Dark Wizard," use "Blight-Touched Weaver"
- Avoid modern tech: No "Cybernetic Soldier," use "Draugr-Pattern Enforcer"

**Examples**:

```markdown
✅ GOOD:
- Ability: "Galdr's Wrath" (evocation weaving)
- Enemy: "Forlorn Husk" (Symbiotic Plate victim)
- Item: "Seidr-Woven Cloak" (Aether-channeling garment)
- NPC: "Ragna Ironbraid, Rune-Warden of the Ashen Halls"

❌ BAD:
- Ability: "Fireball" (generic fantasy)
- Enemy: "Cyborg Zombie" (modern tech language)
- Item: "Enchanted Robe" (generic fantasy)
- NPC: "Commander Johnson, Security Officer" (modern naming)

```

### Flavor Text Guidelines

**Combat Flavor**:

```markdown
✅ GOOD:
"Your blade strikes true, parting corrupted plating with a screech of
tortured metal. The Draugr's Iron Heart flickers, ancient subroutines
misfiring in their death throes."

❌ BAD:
"You deal 15 damage. The enemy is defeated."

✅ GOOD:
"Psychic Stress surges as the Forlorn's presence warps perception. Your
mind rejects the contradictions radiating from its fractured form."

❌ BAD:
"The enemy scares you. You take stress damage."

```

**Environmental Flavor**:

```markdown
✅ GOOD:
"The Muspelheim Forge exhales superheated air through cracked vents, its
climate regulators repurposed by 800 years of decay. Diagnostic runes flicker
amber, warning of imminent thermal cascade."

❌ BAD:
"The fire dungeon is hot. Lava flows through the area."

✅ GOOD:
"Niflheim's cryo-stasis chambers hum with corrupted purpose, preserving
nothing, freezing everything. Frost creeps across failed biometric scanners."

❌ BAD:
"The ice level has frozen enemies and slippery floors."

```

---

## Common Violations & Fixes

### Violation 1: Pre-Glitch Magic Users

**Violation**:

```markdown
❌ "Before the Ginnungagap Glitch, powerful sorcerers wielded magic to
protect Aethelgard from threats."

```

**Fix**:

```markdown
✅ "Before the Ginnungagap Glitch, runes provided only surface-level effects
when etched on materials—reinforced hulls, illuminated corridors. The Glitch's
recursive cascade transformed runes into focal points for Aetheric
manipulation, creating the 'magic' systems post-Glitch survivors now wield."

```

**Reference**: Domain 3 (Magic/Aetheric System)

---

### Violation 2: Jötun-Readers as Precision Scientists

**Violation**:

```markdown
❌ "Jötun-Readers measure CPS thresholds with Field Cards, calibrating their
instruments to detect exact paradox levels (measured in micro-CPS units)."

```

**Fix**:

```markdown
✅ "Jötun-Readers assess Blight corruption through qualitative observation—
visual distortions, psychic discomfort intensity, duration of exposure before
symptom onset. Without precision measurement tools, their Field Cards document
subjective impressions and oral tradition, not calibrated data."

```

**Reference**: Domain 4 (Technology Constraints)

---

### Violation 3: Creating New Data-Slates

**Violation**:

```markdown
❌ "The crafting system allows players to create new Data-Slates by programming
Pre-Glitch terminals with discovered lore."

```

**Fix**:

```markdown
✅ "The crafting system allows players to *repair* damaged Data-Slates found
in ruins, restoring corrupted sections to legibility. Manufacturing new slates
or rewriting existing content is beyond post-Glitch capabilities."

```

**Reference**: Domain 4 (Technology Constraints)

---

### Violation 4: "Galdr" as Enemy Type

**Violation**:

```markdown
❌ "Galdr enemies are spectral entities formed from corrupted Aetheric energy,
haunting the ruins of Alfheim."

```

**Fix**:

```markdown
✅ "Data-Wraiths (consolidated entity type, not 'Galdr' or 'Unraveled') are
corrupted Aetheric echoes haunting Alfheim's ruins. Galdr-Casters are living
NPCs who specialize in evocation weaving."

```

**Reference**: Domain 6 (Entity Types), Domain 3 (Magic/Aetheric System)

---

### Violation 5: Pristine Pre-Glitch Systems

**Violation**:

```markdown
❌ "The sealed vault contains pristine Pre-Glitch weapons, functioning
perfectly as designed with no signs of decay."

```

**Fix**:

```markdown
✅ "The vault's environmental seals failed centuries ago. Weapons within show
signs of 800 years: targeting arrays drift unpredictably, power cells leak
residual charge, grips crumble to dust. Semi-functional at best, deadly
unreliable at worst."

```

**Reference**: Domain 4 (Technology Constraints)

---

### Violation 6: Resolving Counter-Rune Paradox

**Violation**:

```markdown
❌ "Research conclusively proves the Counter-Rune is parasitic, designed to
feed the Blight eternally. The 'world-ending' theory is disproven."

```

**Fix**:

```markdown
✅ "Conflicting telemetry persists: some JORM logs show Blight stabilization
correlating with Counter-Rune activity; others reveal extinction-level
failsafes embedded in Tiwaz subroutines. Jötun-Readers debate endlessly,
unable to reconcile the paradox."

```

**Reference**: Domain 9 (Counter-Rune Paradox - Apocrypha-Level)

---

### Violation 7: Traditional Spell-Casting

**Violation**:

```markdown
❌ "Mystics cast Fireball by speaking the ancient words 'Ignis Maximus' and
channeling their internal mana reserves."

```

**Fix**:

```markdown
✅ "Mystics channel Aether through fire-aspect runes etched on gauntlets,
using the glyphs as focal points. Resonance between weaver and rune enables
thermal energy manipulation, manifesting as projected flame."

```

**Reference**: Domain 3 (Magic/Aetheric System)

---

### Violation 8: Only Humans as Sentient

**Violation**:

```markdown
❌ "Sentient species in Aethelgard: Humans. All other life forms are bestial
or mechanical."

```

**Fix**:

```markdown
✅ "Sentient species: Humans (primary), Gorge-Maws (giant, friendly, reclusive,
tremor-sensing), Rune-Lupins (telepathic wolves, friendly, reclusive). Undying
automata and constructs are non-sentient."

```

**Reference**: Domain 5 (Sentient Species)

---

## Implementation Validation

### For AI Implementers

**Before generating content**, validate against these quick checks:

**Flavor Text Generation**:

- [ ]  Uses Layer 2 Diagnostic Voice for technical/machine content
- [ ]  Uses canonical terminology (Aether, not mana; Legend, not XP)
- [ ]  Avoids traditional fantasy tropes (wizards, spellbooks, magic missiles)
- [ ]  Uses Nordic-themed naming conventions
- [ ]  References decay/malfunction for Pre-Glitch systems

**Enemy Design**:

- [ ]  Entity types match Domain 6 (Draugr, Haugbui, Symbiotic Plate)
- [ ]  No "Galdr" or "Unraveled" enemy types
- [ ]  Draugr/Haugbui drop Iron Hearts
- [ ]  Symbiotic Plate enemies are living puppets (not automata)

**Equipment/Loot**:

- [ ]  Pre-Glitch tech shows 800 years of decay
- [ ]  No creating new Data-Slates or programming existing ones
- [ ]  Runic equipment uses weaving focal point language
- [ ]  Scavenged quality tiers reflect improvisation culture

**NPC Dialogue**:

- [ ]  Jötun-Readers lack precision measurement tools
- [ ]  No pre-Glitch magic user references
- [ ]  Uses current year 783 PG in timeline references
- [ ]  Gorge-Maws and Rune-Lupins described as friendly/reclusive

**Procedural Generation**:

- [ ]  Biome themes match realm purposes (Domain 8)
- [ ]  Sun visibility preserved in Midgard zones
- [ ]  Vanaheim/Alfheim positioned adjacent (not overhead)
- [ ]  Realms described as designed structures (not random crashes)

---

## Specification Sections Affected

### Design Constraints (ALWAYS CHECK)

Every specification should include setting constraints in the "Design Constraints" section:

```markdown
## Design Philosophy

### Design Constraints

**Technical**: [Platform, engine limitations]
**Gameplay**: [Genre conventions, accessibility]
**Narrative**: [Lore consistency, thematic requirements]
**Scope**: [Time, budget, team constraints]

**Setting Compliance** (see SETTING_COMPLIANCE.md):
- [Domain X]: [Specific constraint from canonical statements]
- [Domain Y]: [Specific constraint from canonical statements]

Example:
**Setting Compliance**:
- Domain 3 (Magic): All Aetheric manipulation requires runic focal points
- Domain 4 (Technology): Pre-Glitch systems are decayed/malfunctioning (800 years)
- Domain 7 (Reality): Paradox exposure triggers Psychic Stress accumulation

```

### Functional Requirements (VALIDATE EXAMPLES)

Check all FR examples for setting compliance:

```markdown
**Example Scenarios**:
1. **Scenario**: Player uses Aetheric ability
   - **Input**: Player activates "Seidr Bolt" ability
   - **Expected Output**: Aether channeled through runic focal point ✅ (Domain 3)
   - **Success Condition**: Damage dealt, Stamina consumed

   ❌ BAD: "Player casts Fireball spell using internal mana"
   ✅ GOOD: "Player channels Aether through fire-rune focal point"

```

### System Mechanics (VALIDATE FORMULAS AND FLAVOR)

Ensure flavor text in mechanics sections uses canonical terminology:

```markdown
## System Mechanics

### Mechanic 1: Initiative Rolling

**Overview**:
At combat start, all participants roll initiative using FINESSE. ✅ Layer 2 voice ok here

**How It Works**:
1. COMBAT_DIAGNOSTICS: Roll FINESSE dice pool ✅ (Layer 2 Diagnostic Voice)
2. Count successes (5-6 results)
3. Sort descending by success count
4. Establish turn priority queue ✅ (technical language)

```

### Appendices (TERMINOLOGY VALIDATION)

Include setting-specific terminology in appendices:

```markdown
## Appendix A: Terminology

| Term | Definition |
|------|------------|
| **Aether** | Unified quantum energy field; planet's operating system |
| **Weaving** | Aetheric manipulation via runic focal points |
| **Iron Hearts** | Power source for Draugr/Haugbui automata |
| **783 PG** | Current year (Post-Glitch) |
| **Layer 2 Diagnostic Voice** | Technical/machine aesthetic for Pre-Glitch systems |

```

---

## Quick Reference Card

**Print this and keep handy during specification writing**:

### Critical "Must Not" Rules

- ❌ Pre-Glitch magic users
- ❌ "Galdr"/"Unraveled" as entity types
- ❌ Creating/programming Data-Slates
- ❌ Jötun-Readers with precision tools
- ❌ Traditional spell-casting (no wizards)
- ❌ Pristine Pre-Glitch systems
- ❌ Resolving Counter-Rune paradox
- ❌ Vanaheim/Alfheim overhead
- ❌ Only humans as sentient
- ❌ Generic fantasy terminology

### Must Use Terminology

- ✅ Aether (not mana)
- ✅ Legend (not XP)
- ✅ Weaving (not spell-casting)
- ✅ 783 PG (current year)
- ✅ Layer 2 Diagnostic Voice (technical)
- ✅ Iron Hearts (Draugr/Haugbui power)
- ✅ Nordic naming (Rune-Warden, not Wizard)

### Current Canon

- **Year**: 783 PG
- **Sentient Species**: Humans, Gorge-Maws (friendly), Rune-Lupins (friendly)
- **Entity Types**: Draugr (security automata), Haugbui (labor automata), Symbiotic Plate (AI parasites)
- **Magic**: Runic focal points + Aether weaving (NOT traditional spells)
- **Tech Level**: Operational use without comprehension; 800 years of decay

---

**Version**: 1.0
**Last Updated**: 2025-11-19
**Status**: Active

**Related Documents**:

- Main Specification Guide: `README.md`
- Specification Template: `TEMPLATE.md`
- Setting Fundamentals (Source): External Aethelgard Archives

---

**End of Setting Compliance Guide**