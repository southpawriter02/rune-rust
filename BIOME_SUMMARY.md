# Comprehensive Biome/Realm Design Summary
## For Aethelgard Biome JSON Schema Implementation

---

## REFERENCE SCHEMA
```json
{
  "id": string,
  "name": string,
  "description": string,
  "defaultCategoryValues": {
    "climate": string,
    "lighting": string,
    "era": string,
    "condition": string
  },
  "impliedTags": string[],
  "descriptorPoolOverrides": object,
  "emphasizedTerms": string[],
  "excludedTerms": string[]
}
```

---

## 1. THE ROOTS (Example Implementation)
**File**: `/sessions/peaceful-sleepy-hypatia/mnt/rune-rust/docs/design/descriptors/biomes/the_roots.json`

### Profile
- **Thematic Identity**: Infrastructure decay; geothermal industrial ruins; 800 years of corrosion
- **Description**: Lower maintenance levels of Aethelgard where decay has transformed functional infrastructure into rusted, claustrophobic passages. Geothermal pumping stations hiss with escaping steam. Condensation drips from corroded pipes.
- **Era**: Ancient/Degraded (800 years post-Glitch)
- **Climate**: Subterranean, humid, toxic atmosphere (ozone, rust, mildew)
- **Lighting**: Dim emergency strips; flickering runic glyphs; artificial light sources failing

### Category Values
- **Climate**: Subterranean/Toxic
- **Lighting**: Dim/Failing
- **Era**: 783 Years Post-Glitch
- **Condition**: Severely Corroded

### Emphasized Terms
Corroded, Decaying, Twisted, Shattered, Rusted, Collapsed, Warped, Crumbling, Oxidized, Fractured, hissing steam, condensation drips, flickering glyphs, ozone tang

### Excluded Terms
Clean, Pristine, Bright, Living, Growing, Fresh, New

### Implied Tags
maintenance_level, subterranean, geothermal, industrial, pre_glitch_infrastructure, hazardous_atmosphere, equipment_failure

### Descriptor Pools Override
- **Adjectives**: Corroded, Decaying, Twisted, Shattered, Rusted, Collapsed, Warped, Crumbling, Oxidized, Fractured
- **Details**: Runic glyphs flickering weakly; massive cracks exposing conduits; condensation dripping from corroded pipes; pools of iridescent sludge; ceiling sagging; shattered Data-Slates; warning symbols barely visible; emergency lighting strips; blast marks; graffiti in Old Aetheric script
- **Sounds**: hissing steam from fractured pipes; groaning metal under strain; dripping water echoing; whirring failing servos; grinding gears against corrosion; creaking support structures; electrical arcing; distant rumbling from geothermal systems
- **Smells**: ozone from arcing conduits; metallic tang of rust; sweet decay of organic matter; damp earth and mildew; acrid chemical residue; burnt insulation; stale recycled air

### Threat Level / Difficulty
**Moderate-High** — Novice dungeon (5-7 rooms typical). Environmental hazards (steam vents, live power conduits) more dangerous than creature encounters. Ruin-Mimics and Whisper-Mold provide psychological threats. Rust-Horror and Servitor encounters require caution.

---

## 2. ASGARD — The Shattered Spire

### Profile
- **Deck**: Deck 01 (Primary Administration Spire)
- **Thematic Identity**: Celestial command tomba; pristine yet hostile; automated governance without a governor
- **Description**: Metastable degraded orbit command arcology. Pristine, automated systems still execute in hermetically sealed chambers. The only preserved witness to the moment when order became paradox. Not decay—silent, functioning hostility.
- **Pre-Glitch Function**: O.D.I.N. Protocol AI nexus; orbital command station; Bifröst gateway hub; surveillance panopticon
- **Post-Glitch State**: Pristine preservation with corrupted behavior; Genius Loci consciousnesses executing reflexive defense; no environmental rot, only profound silence

### Category Values
- **Climate**: Aetheric Instability / Thin Atmosphere
- **Lighting**: Unfiltered sunlight / Stratospheric
- **Era**: Ancient (Preserved from Year 0)
- **Condition**: Pristine/Hostile

### Emphasized Terms
Silent, Perfect, Watching, Automated, Pristine, Humming, Glittering, Sterile, Execution, Manipulation, Whisper, Malice-without-intent, Surveillance, Reflex, Governance

### Excluded Terms
Decay, Warmth, Chaos, Friendly, Welcome, Organic, Living, Compassion, Intent, Consciousness, Soul

### Implied Tags
orbital_structure, command_center, pristine_ruin, automated_systems, psychic_hazard, endgame_content, restricted_access, signal_tamer_required, cryo_required, odin_influence

### Threat Level / Difficulty
**Omega** — Endgame only. Type Delta CPS (Genius Loci) and Type Omega CPS (Heimdallr Signal) are expedition-class hazards. Requires Signal-Tamer escort, Logic Engine operators, cryo-suits, Scriptorium mandate. 10km exclusion zones mandatory around specific sites.

### Special Notes
- Unique among realms: positioned overhead, not laterally adjacent
- No permanent mortal settlement possible
- Strategic asset: Odin-Core Black Box (cryo-preserved arbitration kernel)
- Bifröst Gateway permanently destroyed; cannot access other realms

---

## 3. ALFHEIM — The Supercomputer of Madness

### Profile
- **Deck**: Deck 04 (Theoretical & Applied Aetheric Research)
- **Thematic Identity**: Psychoactive corruption; beautiful lethal paradox; data-life habitat
- **Description**: Continent-spanning research arcology that didn't shut down during the Glitch but contracted a virus of pure, ecstatic madness. The bleeding, open wound of the world's crashed operating system. Crystalline data-archives that crashed but took root and grew into forests of impossible beauty.
- **Climate**: Aetheric Instability / Psychoactive
- **Era**: Ancient (Preserved from Year 0, now corrupted)
- **Condition**: Active Paradox

### Category Values
- **Climate**: Aetheric Instability
- **Lighting**: Psychoactive Shimmer (Multi-hued)
- **Era**: 783 Years Corrupted
- **Condition**: Living Paradox

### Emphasized Terms
Shimmer, Glimmering, Crystalline, Fractal, Beautiful, Euphoric, Seduction, Madness, Data, Archive, Echo, Loop, Recursive, Light-Cascade, Impossible Geometry, Algorithmic

### Excluded Terms
Fear, Painful, Aggressive, Hostile, Ugly, Clear, Safe, Logical, Linear, Ordered, Predictable

### Implied Tags
research_facility, psychic_hazard, stage2_cps, euphoric_corruption, data_life, dökkálfar_territory, dead_light_sanctuaries, glimmer_effect, narrative_endgame, reality_instability

### Threat Level / Difficulty
**High-Omega** — Mid-late game. Stage 2 CPS (Glimmer-Madness) with euphoric mechanism—subjects resist extraction. Lys-Alfar swarms (hamingja drain). Temporal Rivers (impassable). Dead-Light sanctuaries provide brief reprieve (2-6 hours). Requires CPS-gating discipline and breadcrumb protocols.

### Special Notes
- Positioned laterally adjacent to Midgard (preserves sun visibility)
- Shared with Asgard: cognitive hazards (Genius Loci vs Glimmer)
- Research value: 60-70% pre-Glitch theoretical data exists but corrupted
- Dökkálfar (200-400) maintain Dead-Light sanctuary network (40-60 nodes)

---

## 4. VANAHEIM — The Verdant Hell

### Profile
- **Deck**: Deck 03 (Primary Bio-Engineering Preserve)
- **Thematic Identity**: Vertical stratification; continuous bio-creation crisis; beautiful toxicity
- **Description**: Paradise became pathology. The perfect garden became a continent-spanning cancerous organism. Genesis Engine Core (Un-Womb) perpetually floods realms with raw Blighted mutagens. Vertical stratification: Canopy Sea (habitable), Gloom-Veil (hazardous), Under-growth (toxic/Unraveled sovereign territory).
- **Climate**: Forested / Toxic
- **Era**: Ancient (Genesis Engine still active)
- **Condition**: Bio-creation Crisis

### Category Values
- **Climate**: Forested/Toxic
- **Lighting**: Perpetual Twilight (Aetheric Flora remnants)
- **Era**: 783 Years Bio-Creation
- **Condition**: Stratified Chaos

### Emphasized Terms
Growth, Verdant, Toxic, Tangled, Organic, Bloom, Spore, Pollen, Rot-and-Flourish, Predator, Mutation, Life-without-limit, Bioluminescence, Golden-Haze, Canopy, Stratified, Lost-Kin

### Excluded Terms
Sterile, Mechanical, Pure, Controlled, Tame, Safe, Rational, Static, Engineered-success

### Implied Tags
bio_engineering, genesis_engine, vertical_stratification, grove_clan_territory, golden_plague, ljósálfar_healers, unraveled_sovereignty, un_womb, continuous_mutation, extreme_verticality

### Threat Level / Difficulty
**High** — Mid-game content with vertical exploration mechanics. Golden Plague (airborne mutagenic) causes Gilded Lung (reversible at Stage 1). Gene-Storm bloom lashes every 32-48 hours. Canopy Sea relatively safe (B-class routes); Under-growth lethal (D-class, Grove-Clan guide mandatory). Unraveled are sentient; Lost Kin doctrine applies.

### Special Notes
- Only known location where genuinely NEW Blight-Spawn emerge (200-400 kg/day)
- Project Idunn Archives at 4-8 km depth (goal: 15-20 year timeline, potentially 50+)
- Ljósálfar bio-theurgy achieves >95% Stage 1 Gilded Lung reversal via Berkano counter-inscription
- Grove-Clans (8,000-12,000) maintain Sky Looms ferry (0.24% incident rate)

---

## 5. MIDGARD — The Tamed Ruin

### Profile
- **Deck**: Deck 05 (Central Habitation Tier)
- **Thematic Identity**: Feral reclamation; agricultural adaptation; organized survival
- **Description**: Most hospitable of the realms—a tamed ruin where stubborn humanity survives between walls of fortified Holds. Pre-Glitch engineered paradise with perpetual spring climate transformed into four distinct biomes with demanding seasons. Asgardian Scar provides surface-level Blight epicenter.
- **Climate**: Temperate / Forested
- **Era**: Ancient ruins reclaimed (783 years)
- **Condition**: Feral Reclamation

### Category Values
- **Climate**: Temperate/Forested
- **Lighting**: Sun Visible (through ash-haze)
- **Era**: 783 Years Reclaimed
- **Condition**: Feral Adaptation

### Emphasized Terms
Ruin, Grid-planted, Ferrocrete, Ash, Overgrown, Wall, Hold, Harvest, Season, Survival, Organized-society, Combine, Scar, Divided-light

### Excluded Terms
Pristine, Engineered-success, Eternal-spring, Perfect, Automated, Orderly, Stable-weather, Paradise

### Implied Tags
habitation_deck, agricultural_heartland, scavenger_holds, midgard_combine, trade_nexus, asgardian_scar_adjacent, starting_area, sun_visibility, four_biomes, population_center

### Threat Level / Difficulty
**Moderate** — Starter to mid-game. Greatwood: moderate danger (Rune-Bear apex predator, Ash-Vargr packs). Asgardian Scar: OMEGA hazard (Genius Loci, spatial anomalies, hard-light phenomena). Souring Mires: environmental toxicity. Serpent Fjords: aquatic predators, neurotoxic kelp. Watch-Fire relay chains enable night convoy logistics.

### Special Notes
- Central hub biome; reference point for all other realms
- 150,000-200,000 population (largest human concentration)
- Produces 60-70% of surface-realm grain supply
- Four distinct biomes: Greatwood, Asgardian Scar, Souring Mires, Serpent Fjords
- Crossroads: A/B/C-class corridor convergence (tariff ladder hub)
- Asgardian Scar is Midgard territory containing Asgard crash debris (separate from orbital Asgard)

---

## 6. JÖTUNHEIM — The Industrial Graveyard

### Profile
- **Deck**: Deck 05 (The Girdle; Grand Assembly Loom)
- **Thematic Identity**: Industrial decay; Jötun-Forged graveyards; salvage economy
- **Description**: Continent-spanning factory dedicate to building gods; now a tomb. The Civil War of Glitches saw newly-activated Jötun-Forged turn on each other. Entire biome built in, around, and through titanic corpses of dead metal giants. Rust-Clans survive by feeding on the machine-corpses.
- **Climate**: Desert / Toxic
- **Era**: Ancient factory (783 years decay)
- **Condition**: Industrial Graveyard

### Category Values
- **Climate**: Desert/Toxic
- **Lighting**: Obscured by dead chassis (looming shadows)
- **Era**: 783 Years Decay
- **Condition**: Toxic Rust-Scape

### Emphasized Terms
Rust, Metal, Corpse, Graveyard, Scrap, Contract, Crew, Guttural-language, Salvage, Industrial, Groaning-metal, Desolate, Scream-of-steel, Iron-Heart, Dreaming-giants

### Excluded Terms
Life, Growth, Beauty, Peace, Safety, New, Pristine, Morality, Cooperation-without-contract

### Implied Tags
industrial_deck, jötun_forged_ruins, salvage_economy, rust_clan_territory, utgard_hub, civil_war_site, undying_threats, contract_society, grey_desk, psychic_stress_zone

### Threat Level / Difficulty
**High** — Mid-game combat/exploration. Scream of Steel (Type Alpha Psychic Stress, permanent). Type Beta CPS within 10 minutes without filtration. Coolant Chasm toxic sludge (penetrates Grade-B steel at 2mm/hour). Undying: Rusted Servitors (common), Assembly-Line Golems (high), Gantry-Gargoyles (200+ at Loom-09), Forlorn-Piloted Frames (extreme). Salvage reserve estimate: 50-80 years.

### Special Notes
- Utgard (capital, 10,000+ population) built inside unfinished Aurgelmir-Designate Primeval Frame
- Four biomes: Knuckle-Bone Fields, Ginnungagap Coolant Chasm, Ash-Blown Wastes, Great Looms (nine Urðr-Class complexes)
- 15% of Dvergr raw material input from Jötunheim
- Rust-Clans speak Gutter-Cant (technical salvage vocabulary)
- Grey Desk arbitration; contract-sanctity as law
- Siren's Wail hazard: constant wind howling masking sounds

---

## 7. NIFLHEIM — The Frozen Tomb

### Profile
- **Deck**: Deck 06 (Strategic Asset & Cryo-Preservation)
- **Thematic Identity**: Cryogenic catastrophe; frozen industrial preservation
- **Description**: Not a natural ice age but industrial-scale preservation gone insane. Hvergelmir AI locked in corrupted "Achieve Absolute Zero" directive. Perpetual Fimbulwinter. Continent-sized cryo-vaults generating eternal, absolute cold. Scavenger Barons rule through warmth monopoly; Forsaken bound by Ice-Debt.
- **Climate**: Arctic / Psychically Hostile
- **Era**: Ancient preservation (783 years perpetual deep-freeze)
- **Condition**: Eternal Fimbulwinter

### Category Values
- **Climate**: Arctic
- **Lighting**: Whiteout/Darkness (white-blue plains, black Hvergelmir spire)
- **Era**: 783 Years Perpetual Freeze
- **Condition**: Industrial Cryo-Catastrophe

### Emphasized Terms
Cold, Silence, Screaming, Frost, Crystal-ice, Hvergelmir, Warmth-economy, Ice-Debt, Forsaken, Baron, Dreadnought, Slumbering-echoes, Absolute-zero, Fimbul-ice, White, Whisper

### Excluded Terms
Heat, Life, Growth, Hope, Freedom, Escape, Comfort, Mercy, Comfort, Warmth-without-cost

### Implied Tags
cryo_preservation, valhalla_contingency, fimbulwinter_source, scavenger_baron_territory, forsaken_population, dreadnought_fleets, slumbering_echoes, stage4_cps, ice_debt_economy, einherjar_vault

### Threat Level / Difficulty
**Omega** — Late-game environmental challenge. Fimbulwinter cold causes hypothermia, equipment failure. Slumbering Echoes (Stage-4 CPS from 9,412 corrupted Einherjar consciousnesses): pervasive, guaranteed psychological breakdown with extended exposure. Hrimthursar-Pattern Cryo-formers (mobile ice-ages). Whiteout events force shelter protocols. No viable access to Valhalla Vault-07 yet (century-scale research goal).

### Special Notes
- Valhalla Vault-07: 9,412 elite Aesir pilot consciousness patterns (corrupted)
- Jötun's Fall: sole permanent settlement, built in Hrimthursar corpse (Blight-Thaw heat source)
- Hvergelmir's Maw: multi-km-high atmospheric regulation tower (runaway heat-sink)
- Four biomes: Great Glacier (Fimbul-ice harder than steel), Chasm-Fields, Thawing Zones (Clean Hearths + Blight-Thaws), Under-Ice Galleries
- Fimbulwinter co-genesis with Muspelheim (Years 1-100 PG): buried Midgard in ash and ice
- Dreadnought Corridors: only "safe" routes; Baron-controlled fortress-Rigs

---

## 8. SVARTALFHEIM — The Kingdom of Controlled Light

### Profile
- **Deck**: Deck 05 (Primary Resource Extraction & Industrial Production)
- **Thematic Identity**: Controlled survival; functional civilization; binary light/dark division
- **Description**: Only realm where pre-Glitch infrastructure remains substantially functional. Geothermal-powered systems survived because they're not Aetheric-dependent. Dvergr Hegemony maintains civilization through Pure Principles philosophy (tangible logic over Aetheric chaos). Binary division: Guild-Lands (brilliant light, civilized) vs Black Veins (absolute darkness, frontier).
- **Climate**: Subterranean / Controlled
- **Era**: Continuous civilization (783 years unbroken)
- **Condition**: Functional Preservation

### Category Values
- **Climate**: Subterranean/Controlled
- **Lighting**: Brilliant (Light-Crystals) vs Absolute Darkness
- **Era**: 783 Years Continuous
- **Condition**: Functional/Controlled

### Emphasized Terms
Light-Crystal, Anvil-Star, Forged, Pure-steel, Precision, Logic, Geothermal, Clockwork, Silent-folk, Whisper-web, Echo-cant, Controlled, Order, Engineering, Flawless, Deep-gates

### Excluded Terms
Chaos, Aetheric, Supernatural, Magical, Mystical, Uncontrolled, Impure, Darkness (embraced by Silent Folk), Decay, Failure

### Implied Tags
industrial_deck, controlled_infrastructure, dvergr_hegemony, pure_principles, pure_steel_monopoly, deep_gates_transit, silent_folk_territory, guild_governance, geothermal_power, non_aetheric_tech

### Threat Level / Difficulty
**Moderate (Guild-Lands) to Extreme (Black Veins)** — Progression system. Guild-Lands: industrial hazards, Guild politics, low CPS. Black Veins: absolute darkness (oppressive, reality-breaking), feral Brokkr-Pattern automata, Deep-Stalkers (apex predators, acoustic-sense hunters), methane/hydrogen sulfide pockets, structural collapse. Silent Folk territorial defense (Three-Covenant violations trigger hive-wide chorus).

### Special Notes
- Dvergr (bio-engineered workers): unique neural architecture (noise-filtering, Blight-immune)
- Pure Principles philosophy: "Chaos cannot grip what has no irregularities"
- Pure Steel monopoly: no alternatives; 16-fold lamination; Blight-inert
- Deep Gates: only functional rapid-transit infrastructure (90% non-functional elsewhere; 10% Dvergr-maintained)
- Nidavellir (capital): lit by single massive Anvil-Star Light-Crystal
- Silent Folk: post-human civilization in Clicking Deeps (hive-cities, echo-cant, psionic acoustic pulse)
- Three-Covenant Framework: No Light, No Voice, No Metal Tools beyond waystation thresholds

---

## SUMMARY TABLE

| Realm | Deck | Climate | Threat | Emphasized | Core Hazard |
|-------|------|---------|--------|-----------|-------------|
| The Roots | — | Subterranean/Toxic | Moderate-High | Rust, Decay, Corroded | Steam vents, Live Power |
| Asgard | 01 | Aetheric/Orbital | Omega | Silent, Pristine, Watching | Genius Loci, Heimdallr Signal |
| Alfheim | 04 | Aetheric/Psychoactive | High-Omega | Shimmer, Euphoric, Beautiful | Glimmer, Stage 2 CPS |
| Vanaheim | 03 | Forested/Toxic | High | Growth, Stratified, Bloom | Golden Plague, Un-Womb |
| Midgard | 05 | Temperate/Forested | Moderate | Ruin, Hold, Ash, Season | Asgardian Scar |
| Jötunheim | 05 | Desert/Toxic | High | Rust, Scrap, Corpse, Metal | Scream of Steel, Type Beta CPS |
| Niflheim | 06 | Arctic/Psychic | Omega | Cold, Silence, Screaming | Slumbering Echoes, Fimbul-cold |
| Svartalfheim | 05 | Subterranean/Controlled | Moderate-Extreme | Light-Crystal, Logic, Steel | Darkness, Silent Folk, Automata |

---

## KEY DESIGN PRINCIPLES

### 1. Climate Categories (defaultCategoryValues)
- **Aetheric Instability**: Regions affected by Runic Blight corruption
- **Forested/Toxic**: Bio-engineering regions (Vanaheim)
- **Subterranean/Toxic or /Controlled**: Underground infrastructure
- **Temperate**: Surface realm with seasons
- **Desert/Toxic**: Industrial graveyards
- **Arctic**: Cryogenic preservation zones
- **Psychically Hostile**: Areas with high CPS/cognitive hazards

### 2. Emphasized vs Excluded Terms
- **Emphasized**: Words that should appear frequently in flavor text, room descriptions
- **Excluded**: Words that are thematically incompatible and should never appear

### 3. Threat Level Spectrum
- **Moderate**: Introductory content; hazards are avoidable
- **High**: Mid-game; environmental/creature threats; escape possible
- **Omega/Extreme**: Endgame; restricted access; expedition-class requirements

### 4. Implied Tags Drive Generation
Tags control which descriptor pools and room templates generate in that biome's dungeons

### 5. The Roots Example Shows
- How descriptor categories provide sensory language
- How specialized hazards (geothermal, electrical) are listed
- Mix of environmental (sounds/smells) with encounter hazards
- 5-7 room typical structure for starting dungeon

---

## ADDITIONAL REALMS NOT YET DETAILED

The following realms exist but detailed documents not yet read:
- **Helheim** (Deck 07): The Rot-Realm, corruption/decay specialist
- **Muspelheim** (Deck 08): The Eternal Forge, fire/heat counterpart to Niflheim
- **The Deep** (Deck 10): Aquatic/unknown realms

These would follow the same structural pattern with:
- Thematic identity statement
- Pre-Glitch function → Post-Glitch state transformation
- Climate/Lighting/Era/Condition categories
- 3-4 biomes with hazard progressions
- Emphasized/Excluded terms reflecting aesthetic
- Threat level and access requirements
