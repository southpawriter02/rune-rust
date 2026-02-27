# Aethelgard Biome Resources Index

## CRITICAL REFERENCE FILES

### 1. Existing Biome Descriptor (Your Template)
**File**: `/sessions/peaceful-sleepy-hypatia/mnt/rune-rust/docs/design/descriptors/biomes/the_roots.json`

This is the REFERENCE IMPLEMENTATION showing:
- Complete JSON structure for biome descriptors
- Element definitions (DormantProcess, DynamicHazard, StaticTerrain, LootNode, AmbientCondition)
- Room count ranges, branching probability, secret room mechanics
- Complete descriptor category pools (Adjectives, Details, Sounds, Smells)
- Spawn weights and rules

**USE THIS FILE AS**: Your structural template and validation example

---

## DOCUMENTATION CREATED FOR THIS PROJECT

### 2. BIOME_SUMMARY.md
**Location**: `/sessions/peaceful-sleepy-hypatia/mnt/rune-rust/BIOME_SUMMARY.md`

**Contains**:
- Comprehensive profile for each of 8 realms (Asgard, Alfheim, Vanaheim, Midgard, Jötunheim, Niflheim, Svartalfheim, The Roots)
- For each realm:
  - Thematic identity statement
  - Complete description
  - Pre-Glitch function → Post-Glitch transformation
  - Climate, Lighting, Era, Condition category values
  - Emphasized terms (15-25 words)
  - Excluded terms (10-15 words)
  - Implied tags for dungeon generation
  - Threat level and access requirements
  - Strategic importance notes
  - Cross-realm connections
  - Distinctive hazards and encounters
  - Special mechanics and constraints

**USE THIS FILE AS**: Your primary reference for realm-specific information during biome JSON creation

**Key Table**: Summary table comparing all 8 biomes across Climate/Threat/Emphasized Terms/Core Hazard

---

### 3. BIOME_JSON_TEMPLATE.json
**Location**: `/sessions/peaceful-sleepy-hypatia/mnt/rune-rust/BIOME_JSON_TEMPLATE.json`

**Contains**:
- Complete JSON entries for 7 realms (Asgard, Alfheim, Vanaheim, Midgard, Jötunheim, Niflheim, Svartalfheim)
- Each entry shows:
  ```json
  {
    "id": "biome_id",
    "name": "Realm Name — The Epithet",
    "description": "...",
    "defaultCategoryValues": { climate, lighting, era, condition },
    "impliedTags": [...],
    "descriptorPoolOverrides": {
      "adjectives": [...],
      "visual_details": [...],
      "ambient_sounds": [...],
      "ambient_smells": [...]
    },
    "emphasizedTerms": [...],
    "excludedTerms": [...],
    "threatLevel": "...",
    "accessRequirements": [...],
    "notes": "..."
  }
  ```
- Ready-to-use format that can be directly integrated into the biome system
- All language extracted from and validated against realm documentation

**USE THIS FILE AS**: Copy-paste template for creating new biome entries; reference for exact formatting

---

### 4. BIOME_IMPLEMENTATION_GUIDE.md
**Location**: `/sessions/peaceful-sleepy-hypatia/mnt/rune-rust/BIOME_IMPLEMENTATION_GUIDE.md`

**Contains**:
- Step-by-step workflow for creating new biome JSON entries
- Phase 1: Information Gathering (what to extract from realm docs)
- Phase 2: Schema Mapping (how to map realm info to JSON fields)
- Phase 3: Descriptor Compilation (how to write sensory language pools)
- Phase 4: Validation (checklist to ensure quality)
- Best Practices (DO's and DON'Ts)
- Complete worked example (Alfheim biome creation walkthrough)
- Integration notes (how biomes feed into dungeon generation)
- Template skeleton for quick implementation
- Tag categories and classification system

**USE THIS FILE AS**: Your process guide when creating new biomes; reference for making difficult decisions

---

## SOURCE DOCUMENTS (From Aethelgard Repository)

### Realm Geography Documents
Location: `/sessions/peaceful-sleepy-hypatia/mnt/rune-rust/docs/design/aethelgard/design/resources/01-geography/`

**Available realms with detailed documentation**:
1. `00-connections.md` - Spatial relationships between realms
2. `01-asgard.md` - The Shattered Spire (orbital command center)
3. `02-alfheim.md` - The Supercomputer of Madness (research zone)
4. `03-vanaheim.md` - The Verdant Hell (bio-engineering)
5. `04-midgard.md` - The Tamed Ruin (civilization hub)
6. `05-jötunheim.md` - The Industrial Graveyard (assembly deck)
7. `06-niflheim.md` - The Frozen Tomb (cryo-preservation)
8. `07-helheim.md` - Not yet read (decay specialist)
9. `07-svartalfheim.md` - The Kingdom of Controlled Light (resource extraction)
10. `08-muspelheim.md` - Not yet read (fire/heat counterpart)
11. `10-the-deep.md` - Not yet read (aquatic realms)

**Also available**:
- `bifrost-bridge.md` - Inter-realm connection system
- `1001-1007` series - Specialized locations (Vanir-Wyrd Network, Gleipnir Containment, etc.)

**USE THESE FILES AS**: Primary source material; always verify biome entries against actual realm documentation

---

## KEY INFORMATION EXTRACTED

### Climate Categories (for defaultCategoryValues)
- Aetheric Instability (corrupted zones with Runic Blight)
- Forested / Toxic (bio-engineering regions)
- Temperate / Forested (surface realm)
- Desert / Toxic (industrial graveyards)
- Arctic (cryo-preservation)
- Subterranean / Controlled (functional infrastructure)
- Subterranean / Toxic (hazardous depths)
- Psychically Hostile (high CPS/cognitive hazard areas)

### Lighting Categories
- Unfiltered Sunlight (orbital positions)
- Sun Visible (surface, through atmosphere)
- Perpetual Twilight (bioluminescent flora)
- Brilliant Artificial (Light-Crystal arrays)
- Psychoactive Shimmer (corrupted light)
- Absolute Darkness (unlit depths)
- Dim / Failing (degraded infrastructure)

### Threat Levels
- **Moderate**: Introductory content; avoidable hazards
- **High**: Mid-game; environmental/creature threats; escape possible
- **High-Omega**: Late mid-game; significant challenge
- **Omega**: Endgame; expedition-class; restricted access; extreme lethality

### Core Hazard Types by Realm
| Realm | Primary Hazard | Secondary Hazards |
|-------|--------|-----------|
| Asgard | Genius Loci (Type Delta CPS) + Heimdallr Signal (Type Omega CPS) | Undying Guardians, Structural Instability |
| Alfheim | Glimmer (Stage 2 CPS, euphoric) | Lys-Alfar swarms, Echo loops, Temporal Rivers |
| Vanaheim | Un-Womb bio-creation + Golden Plague | Gene-Storm bloom lashes, Predators, Weaponized flora |
| Midgard | Asgardian Scar (surface Blight epicenter) | Environmental biomes (Greatwood, Souring Mires, Fjords) |
| Jötunheim | Scream of Steel (Type Alpha Psychic) + Type Beta CPS | Undying threats, Coolant toxicity, Live power conduits |
| Niflheim | Slumbering Echoes (Stage-4 CPS) + Fimbulwinter | Hrimthursar-Pattern Cryo-formers, Whiteout, Crevasses |
| Svartalfheim | Absolute Darkness (Black Veins) | Deep-Stalkers, Silent Folk defensive systems, Structural collapse |
| The Roots | Steam vents + Live Power Conduits | Psychic Resonance, Corroded Atmosphere, Unstable ceilings |

---

## STRUCTURE QUICK REFERENCE

### defaultCategoryValues Object
```json
"defaultCategoryValues": {
  "climate": "[Aetheric Instability|Forested/Toxic|Temperate|Desert/Toxic|Arctic|Subterranean/Controlled|etc]",
  "lighting": "[Unfiltered Sunlight|Sun Visible|Perpetual Twilight|Brilliant Artificial|Psychoactive Shimmer|Absolute Darkness|Dim/Failing]",
  "era": "[Ancient Preserved|783 Years Decay|Continuous Civilization|Bio-Creation Crisis|etc]",
  "condition": "[Pristine/Hostile|Living Paradox|Stratified Chaos|Feral Adaptation|Functional Preservation|etc]"
}
```

### impliedTags Array (Sample Categories)
- **Spatial**: orbital_structure, subterranean, vertical_stratification, lateral_adjacency
- **Functional**: command_center, research_facility, industrial_deck, bio_engineering, cryo_preservation
- **Inhabitants**: rust_clan_territory, dökkálfar_domain, grove_clan_territory, silent_folk_territory
- **Hazards**: psychic_hazard, toxic_atmosphere, thermal_gradient, absolute_darkness, stage_X_cps
- **Difficulty**: starter_area, mid_game, endgame_content, omega_restricted
- **Requirements**: signal_tamer_required, cps_resistant_training, cryo_suit, diplomatic_clearance

### descriptorPoolOverrides Object
```json
"descriptorPoolOverrides": {
  "adjectives": [10-15 words describing the biome],
  "visual_details": [8-15 specific 1-2 sentence descriptions],
  "ambient_sounds": [6-12 auditory descriptions],
  "ambient_smells": [4-8 olfactory descriptions]
}
```

### Terms Lists
- **emphasizedTerms**: 15-25 words that should appear frequently
- **excludedTerms**: 10-15 words that are thematically incompatible

---

## WORKFLOW CHECKLIST FOR NEW BIOMES

When creating a biome JSON:

### Information Gathering Phase
- [ ] Read the complete realm geography document
- [ ] Extract pre-Glitch function and post-Glitch transformation
- [ ] Identify 3-4 distinct biomes within the realm
- [ ] List all hazards (environmental, biological, psychic, mechanical)
- [ ] Identify key locations/settlements
- [ ] Extract all sensory language (adjectives, details, sounds, smells)

### Schema Mapping Phase
- [ ] Assign climate category value
- [ ] Assign lighting category value
- [ ] Assign era category value (time depth)
- [ ] Assign condition category value (overall state)
- [ ] Compile 15-25 implied tags (spatial, functional, hazard, difficulty, requirements)

### Descriptor Compilation Phase
- [ ] Extract 10-15 unique adjectives from realm language
- [ ] Write 8-15 specific visual detail descriptions
- [ ] Describe 6-12 ambient sounds for the realm
- [ ] Describe 4-8 ambient smells
- [ ] Ensure each pool has variety and specificity

### Validation Phase
- [ ] Emphasized terms appear 5+ times in realm document
- [ ] Excluded terms contradict the theme
- [ ] No overlap between emphasized and excluded terms
- [ ] Descriptor language matches climate/lighting values
- [ ] All information traceable to realm documentation
- [ ] Threat level appropriate to hazards
- [ ] Access requirements reflect strategic value
- [ ] Cross-realm connections documented

---

## QUICK ACCESS GUIDE

### To understand the complete schema:
1. Read: `/sessions/peaceful-sleepy-hypatia/mnt/rune-rust/docs/design/descriptors/biomes/the_roots.json`
2. Compare with: `/sessions/peaceful-sleepy-hypatia/mnt/rune-rust/BIOME_JSON_TEMPLATE.json`
3. Learn the process: `/sessions/peaceful-sleepy-hypatia/mnt/rune-rust/BIOME_IMPLEMENTATION_GUIDE.md`

### To get realm information:
1. Quick reference: `/sessions/peaceful-sleepy-hypatia/mnt/rune-rust/BIOME_SUMMARY.md`
2. Detailed source: `/sessions/peaceful-sleepy-hypatia/mnt/rune-rust/docs/design/aethelgard/design/resources/01-geography/[XX-realm].md`

### To create a new biome:
1. Gather information from realm document
2. Follow Phase 1-4 in BIOME_IMPLEMENTATION_GUIDE.md
3. Use BIOME_JSON_TEMPLATE.json as copy-paste base
4. Validate against BIOME_SUMMARY.md and realm documentation
5. Integrate and test

---

## KEY INSIGHTS FROM ANALYSIS

### Thematic Patterns
- Each realm has a distinctive aesthetic tied to its pre-Glitch function
- Pre-Glitch→Post-Glitch transformation is the KEY to understanding theme
- Example: Asgard went from "perfect surveillance" to "blind reflex execution"
- Example: Alfheim went from "research facility" to "psychoactive madness"

### Descriptor Strategy
- Emphasis = what appears frequently in realm documents
- Exclusion = what contradicts the core theme
- Pools = sensory language should vary (not just visual, include sound/smell)
- Specificity > Generality (not "dark" but "absolute physical darkness")

### Hazard Classification
- **Environmental**: Climate, terrain, atmosphere hazards
- **Biological**: Creatures, flora, fauna
- **Psychic**: CPS (Cognitive Paradox Syndrome) stages, mental hazards
- **Mechanical**: Automation, industrial systems, infrastructure
- **Temporal**: Time distortion, aging, preservation

### Access Requirements Pattern
- Omega-threat realms require expedition credentials
- Restricted areas need specific equipment (cryo-suits, signal-tamers, etc.)
- Some realms require diplomatic clearance (Grove-Clans, Dvergr, Silent Folk)
- All require understanding of local hazards

---

## FILE LOCATIONS SUMMARY

| File | Location | Purpose |
|------|----------|---------|
| the_roots.json | docs/design/descriptors/biomes/ | Reference implementation |
| BIOME_SUMMARY.md | mnt/rune-rust/ | Quick reference; all 8 realms |
| BIOME_JSON_TEMPLATE.json | mnt/rune-rust/ | Copy-paste template |
| BIOME_IMPLEMENTATION_GUIDE.md | mnt/rune-rust/ | Process guide |
| BIOME_RESOURCES_INDEX.md | mnt/rune-rust/ | This file |
| Realm geography docs | docs/design/aethelgard/design/resources/01-geography/ | Primary source material |

---

## SUCCESS METRICS

A good biome JSON:
- [ ] Has 3+ emphasized terms appearing in room descriptions
- [ ] Has 0 excluded terms appearing in generated content
- [ ] Shows clear thematic consistency with realm documentation
- [ ] Produces atmospherically appropriate room descriptions
- [ ] Has descriptors that vary (not just adjectives)
- [ ] Matches threat level to encounter/hazard density
- [ ] Can be traced back to source documentation
- [ ] Integrates smoothly with dungeon generator
- [ ] Creates distinctive "feel" for each realm's dungeons

---

## ADDITIONAL REALMS TO DOCUMENT

Once 7 initial biomes are complete, document remaining realms:

### High Priority
- **Helheim** (Deck 07): The Rot-Realm; decay and corruption specialist
- **Muspelheim** (Deck 08): The Eternal Forge; fire/heat counterpart to Niflheim

### Medium Priority
- **The Deep** (Deck 10): Aquatic/unknown realms

### Specialized Locations
- **Vanir-Wyrd Network** (Interconnected; location 1001)
- **Gleipnir Containment Pens** (Jötunheim; location 1002)
- **Knuckle-Bone Fields** (Jötunheim; location 1003)
- **Great Looms** (Jötunheim; location 1004)
- **Jötun's Fall** (Niflheim; location 1005)
- **Crossroads** (Midgard; location 1006)
- **Dreadnoughts** (Niflheim; location 1007)
- **Bifröst Bridge** (Inter-realm connection)

These special locations may be sub-biomes within larger realms or deserve their own biome entries depending on gameplay scope.

---

## CONCLUSION

You now have:
1. **A complete schema** (the_roots.json)
2. **All realm information** (BIOME_SUMMARY.md)
3. **Ready-to-use templates** (BIOME_JSON_TEMPLATE.json)
4. **Process guidance** (BIOME_IMPLEMENTATION_GUIDE.md)
5. **This index** for navigation

**Next step**: Pick a realm and create its biome JSON using these materials as reference.
