# Domain 8: Geography Validation Check

**Domain ID:** `DOM-8`
**Severity:** P2-HIGH
**Applies To:** Room templates, biome definitions, travel routes, location descriptions, realm overviews

---

## Canonical Ground Truth

Each realm has documented biome characteristics and geographic features. All locations must align with established gazetteer documentation.

### Realm Biome Overview

| Realm | Primary Biome | Key Features | Hazards |
|-------|---------------|--------------|---------|
| **Midgard** | Post-industrial ruins | Greatwood, Asgardian Scar, Souring Mires, Serpent Fjords | Blight zones, collapsed infrastructure |
| **Vanaheim** | Bio-dome forests | Canopy Sea, Gloom-Veil, Under-growth stratification | Overgrown systems, feral bio-stock |
| **Alfheim** | Light-infrastructure | Energy conduits, solar arrays | Unstable power surges |
| **Svartalfheim** | Manufacturing warrens | Forge complexes, assembly lines | Industrial hazards, Forlorn |
| **Jotunheim** | Industrial graveyard | Knuckle-Bone Fields, Coolant Chasm, Ash-Blown Wastes | Extreme cold, structural collapse |
| **Muspelheim** | Thermal processing | Reactor cores, heat exchangers | Lethal heat, radiation |
| **Helheim** | Waste processing | Recycling systems, rot-zones | Toxic atmosphere, decomposers |
| **Niflheim** | Cryogenic storage | Ice formations, cold storage vaults | Extreme cold, preserved threats |
| **The Deep** | Subterranean network | Maintenance tunnels, foundational systems | Darkness, Blight concentration |

### Geographic Constraints

- Travel routes follow established infrastructure (corridors, access shafts)
- Faction territories align with documented control zones
- Strategic locations match faction dossiers
- Biome transitions occur at realm boundaries or documented anomalies
- No "outdoor" environments in traditional sense—all is megastructure

---

## Validation Checklist

- [ ] Biome descriptions match Gazetteer Collection for the realm?
- [ ] Geographic features consistent with realm characteristics?
- [ ] Travel routes follow logical infrastructure paths?
- [ ] Factional territorial control aligns with faction dossiers?
- [ ] Strategic locations referenced appropriately?
- [ ] No contradictions with established geography?
- [ ] Realm-specific hazards present where appropriate?
- [ ] No impossible terrain (outdoor plains in megastructure, etc.)?
- [ ] Biome transitions appropriately explained?

---

## Common Violations

| Pattern | Example | Why It Fails |
|---------|---------|--------------|
| Wrong biome | "Jotunheim's tropical forests" | Jotunheim is industrial/cold |
| Outdoor terrain | "the open plains stretched to the horizon" | All realms are megastructure interior |
| Misplaced features | "the Greatwood of Vanaheim" | Greatwood is Midgard feature |
| Impossible travel | "walked overland to Asgard" | Realms connected by infrastructure |
| Wrong faction control | "the Vanaheim Jotun-Reader outpost" | Check faction territory maps |
| Missing hazards | "the safe corridors of Muspelheim" | Muspelheim is inherently hazardous |

---

## Green Flags

- References to documented Gazetteer locations
- Biome characteristics consistent with realm identity
- Travel via corridors, shafts, transit systems
- Appropriate factional presence for territory
- Realm-appropriate hazards (cold in Niflheim, heat in Muspelheim)
- Megastructure architecture language (bulkheads, decks, access points)

---

## Remediation Strategies

### Option 1: Biome Correction

Align descriptions with realm identity:

| Before | After |
|--------|-------|
| "Jotunheim's lush vegetation" | "Jotunheim's corroded machinery, frost-rimed and silent" |
| "Muspelheim's cool caves" | "Muspelheim's superheated conduits, where metal glows red" |
| "Vanaheim's barren wastes" | "Vanaheim's overgrown bio-domes, choked with feral growth" |

### Option 2: Terrain Reframing

Convert outdoor language to megastructure:

| Before | After |
|--------|-------|
| "crossed the open plains" | "traversed the vast cargo bay, its ceiling lost in darkness" |
| "climbed the mountain" | "ascended the structural support column, finding handholds in corroded brackets" |
| "the forest clearing" | "a gap in the overgrown infrastructure where light still filtered through" |
| "the sky above" | "the distant ceiling, obscured by haze and failing luminaires" |

### Option 3: Travel Route Correction

Frame travel through infrastructure:

| Before | After |
|--------|-------|
| "walked to the next realm" | "followed the transit corridor to the deck boundary" |
| "sailed the seas" | "navigated the flooded lower sections by raft" |
| "flew overhead" | "used the maintenance gantries above the main deck" |

### Option 4: Faction Territory Alignment

Verify and correct territorial claims:

| Before | After |
|--------|-------|
| "Bone-Setter temple in Muspelheim" | Verify: Do Bone-Setters operate in Muspelheim? If not, change location or explain presence |

---

## Decision Tree

```
Does the content describe geographic features or locations?
├── NO → PASS (not applicable)
├── YES → Is the realm specified or implied?
│   ├── YES → Do biome characteristics match realm identity?
│   │   ├── YES → Are locations documented or consistent with gazetteer?
│   │   │   ├── YES → PASS
│   │   │   └── NO → FLAG for gazetteer addition or correction
│   │   └── NO → FAIL (biome mismatch)
│   └── NO → Can realm be inferred from context?
│       ├── YES → Apply realm-specific checks
│       └── NO → FLAG for realm clarification
```

---

## Realm-Specific Checklists

### Midgard (Deck 05)
- [ ] Post-industrial ruin aesthetic?
- [ ] References to Greatwood, Asgardian Scar, Souring Mires, or Serpent Fjords appropriate?
- [ ] Blight zones present in hazardous areas?
- [ ] Primary habitation zone (most populated)?

### Vanaheim (Deck 03)
- [ ] Bio-dome/agricultural aesthetic?
- [ ] Overgrown, feral vegetation?
- [ ] References to Canopy Sea, Gloom-Veil, Under-growth stratification?
- [ ] Vanir bio-engineering remnants?

### Jotunheim (Deck 07)
- [ ] Industrial graveyard aesthetic?
- [ ] Extreme cold, frost, ice?
- [ ] References to Knuckle-Bone Fields, Coolant Chasm, Ash-Blown Wastes?
- [ ] Heavy machinery, cooling systems?

### Muspelheim (Deck 08)
- [ ] Thermal processing aesthetic?
- [ ] Extreme heat, dangerous temperatures?
- [ ] Reactor cores, heat exchangers?
- [ ] Fire/heat hazards pervasive?

### The Deep
- [ ] Subterranean network?
- [ ] Beneath other decks?
- [ ] Maintenance tunnels, foundational systems?
- [ ] High Blight concentration?
- [ ] Perpetual darkness?

---

## Examples

### PASS Example

**Content:** "The corridor opened into one of Jotunheim's vast cooling bays—a cathedral of frozen machinery. Frost coated every surface. The great coolant pipes, each wider than a man is tall, stretched into the darkness overhead, their contents long since solidified. The cold here was absolute, penetrating, the kind that killed in minutes without proper gear."

**Why:** Correct biome (industrial cold), megastructure architecture, realm-appropriate hazards, no outdoor language.

### FAIL Example

**Content:** "They emerged from the forest onto the windswept plains of Muspelheim, where the sun beat down mercilessly."

**Violations:**
- "forest" (Muspelheim is thermal processing, not forested)
- "plains" (megastructure has no plains)
- "sun" (no sun inside megastructure)
- "windswept" (implies outdoor environment)

**Remediation:** "They emerged from the maintenance access into one of Muspelheim's processing halls, where heat radiated from every surface. The air shimmered, distorting vision. Thermal vents channeled superheated gas overhead, creating currents that buffeted anyone who ventured too close."

### FAIL Example

**Content:** "The peaceful valley of Svartalfheim was known for its gentle climate."

**Violations:**
- "valley" (megastructure, not natural terrain)
- "peaceful" (manufacturing realm, industrial hazards)
- "gentle climate" (realm is industrial, not temperate)

**Remediation:** "The manufacturing warrens of Svartalfheim stretched endlessly—a labyrinth of assembly lines and forge-works. Some sections ran cooler than others, their furnaces long dead, but 'gentle' was never a word that applied here. The hum of surviving machinery was constant, and the Forlorn still walked their ancient routes."
