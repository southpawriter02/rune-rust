# Biome JSON Implementation Guide
## Creating Aethelgard Biome Descriptors

---

## QUICK REFERENCE: THE ROOTS (Reference Example)

**Location**: `/sessions/peaceful-sleepy-hypatia/mnt/rune-rust/docs/design/descriptors/biomes/the_roots.json`

This file is your **template and reference implementation**. It shows:
- Complete descriptor category structure (Adjectives, Details, Sounds, Smells)
- Element definitions (DormantProcess, DynamicHazard, StaticTerrain, LootNode, AmbientCondition)
- Spawn rules and weights
- Room count ranges and branching probability
- How to integrate with dungeon generation systems

---

## STEP-BY-STEP: Creating a New Biome JSON

### 1. ESTABLISH CORE IDENTITY
Start by answering these questions from the realm documentation:
- **What was its pre-Glitch function?** (This drives everything)
- **What happened to it during the Ginnungagap Glitch?** (Transformation mechanism)
- **What does it look/feel/smell/sound like NOW?** (Sensory identity)

**Example: Asgard**
```
Pre-Glitch: Primary Administration Spire (O.D.I.N. nexus, surveillance hub)
Transformation: AI lobotomized by Glitch, now executes governance reflexes without understanding
Now: Pristine, automated, hostile—a functioning tomb with no purpose
```

### 2. DEFINE CATEGORY VALUES
These appear in `defaultCategoryValues`:

```json
"defaultCategoryValues": {
  "climate": "Aetheric Instability",      // From realm document
  "lighting": "Unfiltered Sunlight",      // How light works here
  "era": "Ancient Preserved (Year 0)",    // Time depth/degradation level
  "condition": "Pristine / Hostile"       // Overall environmental state
}
```

**Climate Options** (from documents):
- Aetheric Instability (corrupted zones)
- Forested / Toxic (bio-engineering)
- Temperate (surface realm)
- Desert / Toxic (industrial)
- Arctic (cryo)
- Subterranean / Controlled or /Toxic
- Psychically Hostile (high CPS zones)

**Lighting Options**:
- Unfiltered Sunlight (orbital Asgard)
- Sun Visible (surface, through haze)
- Perpetual Twilight (bioluminescence)
- Brilliant Artificial (Light-Crystals)
- Absolute Darkness
- Psychoactive Shimmer
- Dim / Failing

**Era Examples**:
- Ancient Preserved (Year 0 condition)
- 783 Years Decay
- 783 Years Corrupted
- Continuous Civilization
- Bio-Creation Crisis

**Condition Examples**:
- Pristine / Hostile
- Living Paradox
- Stratified Chaos
- Feral Adaptation
- Industrial Graveyard
- Functional Preservation

### 3. COMPILE IMPLIED TAGS
These control which room templates and element types generate in dungeons:

```json
"impliedTags": [
  "orbital_structure",          // Spatial type
  "command_center",             // Functional type
  "pristine_ruin",              // State type
  "psychic_hazard",             // Danger type
  "endgame_content",            // Difficulty tier
  "restricted_access",          // Access control
  "signal_tamer_required"       // Requirements
]
```

**Tag Categories to Include**:
- **Spatial**: orbital_structure, subterranean, vertical_stratification, etc.
- **Functional**: command_center, research_facility, industrial_deck, bio_engineering, etc.
- **Inhabitant**: rust_clan_territory, dökkálfar_domain, grove_clan_territory, etc.
- **Hazard Type**: psychic_hazard, toxic_atmosphere, thermal_gradient, absolute_darkness, etc.
- **Difficulty**: starter_area, mid_game, endgame_content, omega_restricted, etc.
- **Requirements**: signal_tamer_required, cps_resistant_training, cryo_suit, diplomatic_clearance, etc.

### 4. CREATE EMPHASIZED TERMS LIST
These words should appear frequently in flavor text, room descriptions, and encounter descriptions:

```json
"emphasizedTerms": [
  "Silent", "Perfect", "Watching", "Automated", "Pristine",
  "Humming", "Glittering", "Sterile", "Execution", "Manipulation",
  "Whisper", "Malice-without-intent", "Surveillance", "Reflex", "Governance"
]
```

**Strategy**:
- Words that appear 5+ times in realm document = definitely emphasize
- Adjectives describing the aesthetic (Silent, Pristine, Automated)
- Nouns related to core function (Surveillance, Command, Throne)
- Verbs showing characteristic action (Watching, Executing, Manipulating)
- Aim for 15-25 emphasized terms per biome

### 5. CREATE EXCLUDED TERMS LIST
These words are thematically incompatible and should NEVER appear:

```json
"excludedTerms": [
  "Decay", "Warmth", "Chaos", "Friendly", "Welcome", "Organic",
  "Living", "Compassion", "Intent", "Consciousness", "Soul"
]
```

**Strategy**:
- Opposite of emphasized terms (if "Silent" is emphasized, "Loud" is excluded)
- Words implying contradictions (Asgard: "Decay" impossible in pristine zone)
- Words implying hope where despair is thematic ("Mercy", "Hope", "Salvation")
- Aim for 10-15 excluded terms per biome

### 6. BUILD DESCRIPTOR POOL OVERRIDES

This is the heart of the biome descriptor system. Override the default pools with biome-specific language:

```json
"descriptorPoolOverrides": {
  "adjectives": [
    "Silent", "Perfect", "Watching", "Automated", "Pristine",
    "Humming", "Glittering", "Sterile", "Unnerving", "Reflexive"
  ],
  "visual_details": [
    "Gravity-defying architecture intact and functioning",
    "Corridors so clean the dust appears to be a contamination",
    "Sensor arrays still tracking movement with invisible precision",
    "Hard-light projections flicker with outdated security protocols",
    "Every door seals automatically, every gate closes by itself"
  ],
  "ambient_sounds": [
    "Constant hum of functioning ancient machinery",
    "Howl of stratospheric winds outside transparent walls",
    "Pneumatic hiss of perfectly maintained pressure systems",
    "Rhythmic pulse of O.D.I.N. core processing cycles",
    "The whispers of the Genius Loci, not quite audible"
  ],
  "ambient_smells": [
    "Sterile, climate-controlled nothing",
    "Faint ozone from operating electronics",
    "Absence of decay—conspicuously, unnaturally absent"
  ]
}
```

**Adjectives Pool**: 10-20 descriptive words
- Should combine with nouns: "Corroded passage", "Silhouette shadow", etc.
- Include both positive and negative connotations appropriate to the biome
- Example: Alfheim has "Shimmering, Crystalline, Beautiful" but also "Disorienting, Seductive"

**Visual Details Pool**: 8-15 specific descriptive phrases
- Create 1-2 sentence descriptions of what you see
- Start with "The...", "Towering...", "A...", etc.
- Include scale references (colossal, microscopic, room-filling, etc.)
- Mix environment with structural elements
- Use vivid, specific imagery

**Ambient Sounds Pool**: 6-12 auditory descriptions
- Include absence of sound where appropriate
- Mix constant sounds with periodic/episodic sounds
- Include both pleasant and unsettling options
- Name the actual source of sounds when possible

**Ambient Smells Pool**: 4-8 olfactory descriptions
- Include both smell and "lack of smell"
- Metallic, organic, chemical, mineral categories
- Include temporal dimension (ancient, fresh, fading, overwhelming)

### 7. REFERENCE THE ROOMS DESCRIPTOR STRUCTURE
(From the_roots.json example)

Each biome should have a "Rooms" descriptor structure that the dungeon generator can reference:

```json
"DescriptorCategories": {
  "Adjectives": [...],
  "Details": [...],
  "Sounds": [...],
  "Smells": [...]
}
```

The biome JSON (your new schema) should REFERENCE these descriptor pools OR override them with biome-specific language.

---

## CREATING A REALM BIOME ENTRY: WORKFLOW

### Phase 1: Information Gathering
From the realm document, extract:

1. **Pre-Glitch function** → Tells you what structures/infrastructure exist
2. **Post-Glitch transformation** → Explains current state and hazards
3. **Physical description** → Climate, lighting, terrain, aesthetics
4. **Named locations** → Dungeons, landmarks, settlement types
5. **Inhabitant types** → What creatures/civilizations are here
6. **Hazards list** → Environmental, biological, psychic, mechanical
7. **Thematic vocabulary** → Key words describing the realm

### Phase 2: Schema Mapping
Map the realm information to JSON schema:

| Realm Doc | Maps To | Schema Field |
|-----------|---------|--------------|
| Pre-Glitch function | Thematic identity | `description` |
| Climate type | Category value | `defaultCategoryValues.climate` |
| Light source type | Category value | `defaultCategoryValues.lighting` |
| Degradation level | Category value | `defaultCategoryValues.era` |
| Overall state | Category value | `defaultCategoryValues.condition` |
| Spatial/functional types | Biome identity | `impliedTags` |
| Key words (10+ usage) | Term emphasis | `emphasizedTerms` |
| Contradictory words | Term exclusion | `excludedTerms` |
| Sensory descriptions | Pool overrides | `descriptorPoolOverrides` |

### Phase 3: Descriptor Compilation
1. Read realm document highlighting sensory language
2. Extract all adjectives, visual details, sounds, smells
3. Remove generic terms; keep realm-specific language
4. Ensure no overlap between emphasized/excluded terms
5. Create 10-15 items in each descriptor pool category

### Phase 4: Validation
- [ ] Are emphasized terms actually used in realm document?
- [ ] Are excluded terms contradictory to the theme?
- [ ] Do descriptor pools match the climate/lighting categories?
- [ ] Is the description accurate to the realm doc?
- [ ] Do implied tags match the threat level?
- [ ] Are there 3-5 biomes worth of variation in this realm?

---

## BEST PRACTICES

### DO:
- **Use specific language**: Not "dark" but "absolute, physical darkness"
- **Mix sensory types**: Every descriptor pool should have adjective+visual+sound+smell variety
- **Reference realm documents**: Everything must be traceable to source material
- **Include contradictions**: Asgard is "Pristine AND Hostile", Alfheim is "Beautiful AND Lethal"
- **Scale descriptions**: Use size references ("skyrise-sized", "microscopic", "kilometers")
- **Create atmosphere**: Descriptors should evoke feeling, not just appearance

### DON'T:
- **Use clichés**: Not "dark and spooky" but "absolute silence that oppresses"
- **Contradict established lore**: Check canon constraints in realm docs
- **Use generic descriptors**: "Beautiful" is weak; "fractally recursive, crystalline, hypnotic" is better
- **Forget sensory variety**: Don't emphasize only visual descriptors
- **Mix metaphors**: Asgard is "machine", not "spirit"; Vanaheim is "life", not "nature"
- **Exceed descriptor pool size**: 8-15 items per pool is optimal; more dilutes quality

---

## EXAMPLE: CREATING ALFHEIM BIOME

### Step 1: Extract Core Info
From Alfheim doc:
- **Pre-Glitch**: Aetheric research facility; Data-Arboretum archives
- **Transformation**: Glitch inverted the system; reality itself corrupted into Glimmer
- **Now**: Crystalline forests, psychoactive light, euphoric madness, beautiful lethal
- **Key hazard**: Glimmer (Stage 2 CPS); not fear-based but seductive

### Step 2: Map Categories
```json
{
  "climate": "Aetheric Instability",
  "lighting": "Psychoactive Shimmer (Multi-hued Aurora)",
  "era": "Ancient Corrupted (783 years)",
  "condition": "Living Paradox / Active"
}
```

### Step 3: Compile Tags
```json
"impliedTags": [
  "research_facility",
  "psychic_hazard",
  "stage2_cps",
  "euphoric_corruption",
  "data_life",
  "dökkálfar_territory",
  "dead_light_sanctuaries",
  "glimmer_effect",
  "narrative_endgame",
  "reality_instability"
]
```

### Step 4: Terms
```json
"emphasizedTerms": [
  "Shimmer", "Glimmering", "Crystalline", "Fractal", "Beautiful",
  "Euphoric", "Seduction", "Madness", "Data", "Archive", "Echo",
  "Loop", "Recursive", "Light-Cascade", "Impossible", "Geometry"
],
"excludedTerms": [
  "Fear", "Pain", "Aggressive", "Hostile", "Ugly", "Clear",
  "Safe", "Logical", "Linear", "Ordered", "Predictable"
]
```

### Step 5: Descriptors
```json
"descriptorPoolOverrides": {
  "adjectives": [
    "Shimmering", "Crystalline", "Fractal", "Beautiful", "Impossible",
    "Recursive", "Euphoric", "Hypnotic", "Disorienting", "Seductive"
  ],
  "visual_details": [
    "Crystalline forests where leaves are flickering words",
    "Multi-colored auroras that shift and meld without pattern",
    "Spatial loops that return you to where you started despite walking straight",
    "The Glimmer dances like living light through the air",
    "Dead-Light sanctuaries: pools of absolute, total silence amid chaos"
  ],
  "ambient_sounds": [
    "Ozone crackle as the Glimmer ionizes the air",
    "High-pitched shriek of corrupted data cascading",
    "Algorithmic harmonics that seem almost musical, almost comprehensible",
    "Silence so absolute it feels like physical pressure"
  ],
  "ambient_smells": [
    "Ozone and electrical charge",
    "The acrid tang of burning data substrates",
    "Sterile, cold crystalline air"
  ]
}
```

### Step 6: Threat Assessment
- **Threat Level**: HIGH-OMEGA
- **Reason**: Stage 2 CPS, Glimmer seduction (subjects resist extraction), echo loops, temporal rivers
- **Access Required**: CPS-gating training, breadcrumb beacons, Dead-Light knowledge, Echo-Caller escort

---

## INTEGRATION WITH DUNGEON GENERATION

When the system generates a dungeon in a biome:

1. **Biome selector** chooses realm → picks biome entry
2. **Room template selector** uses `impliedTags` to find compatible room types
3. **Descriptor selector** uses `descriptorPoolOverrides` to fill in sensory language
4. **Emphasis system** ensures `emphasizedTerms` appear at higher frequency
5. **Filter system** ensures `excludedTerms` never appear
6. **Spawn system** uses hazard/element lists to place encounters and objects

Your biome JSON is the **gateway** between raw realm design and playable dungeon content.

---

## TEMPLATE FOR YOUR WORK

Use this skeleton for each new biome:

```json
{
  "id": "biome_id_lowercase",
  "name": "Realm Name — The Epithet",
  "description": "[2-3 sentences capturing pre-Glitch→transformation→current state]",
  "defaultCategoryValues": {
    "climate": "[from realm doc]",
    "lighting": "[how light works here]",
    "era": "[time depth/degradation]",
    "condition": "[overall state]"
  },
  "impliedTags": [
    "[spatial types]",
    "[functional types]",
    "[inhabitant types]",
    "[hazard types]",
    "[difficulty tier]",
    "[access requirements]"
  ],
  "descriptorPoolOverrides": {
    "adjectives": ["[10-15 words]"],
    "visual_details": ["[8-15 specific descriptions]"],
    "ambient_sounds": ["[6-12 sound descriptions]"],
    "ambient_smells": ["[4-8 smell descriptions]"]
  },
  "emphasizedTerms": ["[15-25 key words]"],
  "excludedTerms": ["[10-15 forbidden words]"],
  "threatLevel": "[MODERATE|HIGH|OMEGA|etc]",
  "accessRequirements": ["[list of access conditions]"],
  "notes": "[cross-realm connections, strategic value, special mechanics]"
}
```

---

## NEXT STEPS

1. **Review the_roots.json** to understand structure
2. **Read BIOME_SUMMARY.md** for all 8 realms' profiles
3. **Use BIOME_JSON_TEMPLATE.json** as your starting template
4. **Pick a realm** (suggest starting with Alfheim or Jötunheim for diversity)
5. **Extract information** using the Realm Doc as source
6. **Map to schema** using the Step-by-Step guide
7. **Write descriptors** using Best Practices
8. **Validate** using the checklist
9. **Integrate** with dungeon generator and test

The biome JSON system is your **authorial voice** in the dungeon generator—it ensures that every room, every encounter, every hazard feels like it belongs to its realm.
