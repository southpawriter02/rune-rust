# v0.32: Jötunheim Biome Implementation

Type: Feature
Description: Implements the industrial decay biome - graveyard of fallen Jötun-Forged. Trunk/Roots verticality, Undying-heavy encounters, Live Power Conduits, and armor-shredding tactical requirement. 38-56 hours.
Priority: Must-Have
Status: In Design
Target Version: Alpha
Dependencies: v0.31 (Alfheim), v0.20 (Tactical Grid), v0.22 (Environmental Combat)
Implementation Difficulty: Hard
Balance Validated: No
Proof-of-Concept Flag: No
Sub-item: v0.32.1: Database Schema & Room Templates (v0%2032%201%20Database%20Schema%20&%20Room%20Templates%20ffc37b6b82c1421bb1a599bdb61194d3.md), v0.32.2: Environmental Hazards & Industrial Terrain (v0%2032%202%20Environmental%20Hazards%20&%20Industrial%20Terrain%2008ebb5b9a68843d6b1b607cfb4736edf.md), v0.32.4: Service Implementation & Testing (v0%2032%204%20Service%20Implementation%20&%20Testing%2064bb59b16f34461f83d7c962d6ce62c0.md), v0.32.3: Enemy Definitions & Spawn System (v0%2032%203%20Enemy%20Definitions%20&%20Spawn%20System%20f48b7ad23fe34ff2b1780db59e5fc0e4.md)
Template Validated: No
Voice Validated: No

**Document ID:** RR-SPEC-v0.32-JOTUNHEIM

**Status:** Design Complete — Ready for Implementation

**Timeline:** 38-56 hours total across 4 sub-specifications

**Prerequisites:** v0.31 (Alfheim complete), v0.20 (Tactical Grid), v0.22 (Environmental Combat)

---

## I. Executive Summary

### The Fantasy

Jötunheim is the **industrial decay biome** - the graveyard of the Old World's greatest ambitions. These are the vast, silent factory floors, the colossal assembly yards, and the crumbling transport networks where the god-like Jötun-Forged were built, maintained, and ultimately, where they fell. The entire biome is built in, around, and through the **titanic corpses of dead metal giants**.

This is not a natural landscape. It is a world of rust, concrete, and sorrow - a melancholic testament to a future that died at the moment of its greatest triumph. Players walk on the outer hull of dead giants, navigate through their ribcages, and fight in the shadows of their colossal, motionless limbs.

### v2.0 Canonical Source

v2.0 Specification: Feature Specification: The Jotunheim Biome[[1]](https://www.notion.so/Feature-Specification-The-Jotunheim-Biome-2a355eb312da803586a5d9233ffc10b2?pvs=21)

**Migration Status:** Converting from v2.0 concept to full v0.32 implementation with complete database schema, service architecture, enemy definitions, and testing framework.

### Core Mechanics

- **Verticality:** Primarily [Trunk], with descents into [Roots] maintenance tunnels
- **Scale:** Massive industrial structures and fallen Jötun-Forged create extreme verticality
- **Undying Dominance:** High concentration of Undying enemies (Rusted Servitors, Rusted Wardens, Draugr Juggernauts)
- **Industrial Hazards:** Live Power Conduits, High-Pressure Steam Vents, Unstable Ceilings
- **Resource Economy:** Mechanical components (Tier 1-3), with legendary Unblemished Jötun Plating
- **Mid-Game Positioning:** Tests tactical combat, armor-shredding, and environmental awareness

---

## II. The Rule: What's In vs. Out

### ✅ In Scope (v0.32 Complete)

**Core Systems:**

- Complete Jötunheim biome with procedural generation integration
- Trunk/Roots verticality (70% Trunk, 30% Roots)
- Massive scale implementation (fallen Jötun-Forged as terrain)
- DynamicRoomEngine.GenerateJotunheimSector() implementation
- No ambient condition (physical/technological threats only)

**Content (6 Enemy Types):**

- Rusted Servitor (Common Undying - janitorial constructs)
- Rusted Warden (Medium Undying - security forces)
- Draugr Juggernaut (Rare Undying - heavily armored)
- God-Sleeper Cultist (Medium Humanoid - fanatics)
- Scrap-Tinker (Rare Humanoid - rival scavengers)
- Iron-Husked Boar (Low Beast - Svin-fylking mutants)

**Environmental Features (10+ Types):**

- [Live Power Conduit] (high spawn weight - signature hazard)
- [High-Pressure Steam Vent] (medium)
- [Unstable Ceiling/Wall] (medium - structural collapse)
- [Flooded] terrain (coolant spills)
- [Toxic Haze] (in coolant sectors)
- [Cover] (shipping containers, engine blocks - very high)
- [Jötun Corpse Terrain] (unique - walking on giants)
- [Assembly Line] (conveyor hazard)
- [Scrap Heap] (difficult terrain + salvage)
- [Coolant Reservoir] (flooded hazard zone)

**Resources (10 Types across 4 tiers):**

- Tier 1-2: Rusted Scrap Metal, Intact Servomotors, Coolant Fluid, Ball Bearings
- Tier 3: Unblemished Jötun Plating (rare, from pristine hull sections)
- Tier 4 (Legendary): Uncorrupted Power Coil (deep power stations only)

**Technical Deliverables:**

- Complete database schema (5 new/extended tables, 10+ room templates)
- Service implementation (JotunheimService, BiomeGenerationService extensions)
- Unit test suite (12+ tests, ~85% coverage)
- Integration with Undying enemy system and environmental combat
- v5.0 setting compliance (industrial failure, not supernatural)
- Serilog structured logging throughout

### ❌ Explicitly Out of Scope

- Full Jötun-Forged boss encounters (framework only; complete in v0.35)
- God-Sleeper Cultist faction system (defer to v0.33 - Faction Implementation)
- Legendary crafting recipes using Jötun Plating (defer to v0.36)
- Hardware Malfunction Puzzles (defer to v0.37 - Puzzle Systems)
- Biome-specific achievements/statistics (defer to v0.38)
- Gantry-Runner specialization synergies (defer to v0.25.x)
- Advanced verticality climbing mechanics (defer to v0.37)
- Scrap-Tinker salvage mechanics expansion (defer to v0.36)

---

## III. v2.0 → v5.0 Migration Summary

### Preserved from v2.0

**Mechanical Intent:**

- Mid-game combat and exploration challenge
- High concentration of Undying enemies
- Armor-shredding as core tactical requirement (Draugr Juggernauts teach this)
- Industrial hazards over metaphysical threats
- Extreme verticality via fallen Jötun-Forged
- Mechanical component resource focus (Tier 1-3)
- No ambient condition (physical threats only)

**Thematic Elements:**

- "Graveyard of giants" aesthetic
- Walking on/through dead Jötun-Forged
- Silent assembly lines and colossal gantries
- Rust, oil, and ozone atmosphere
- Melancholic monument to failed ambition
- "The Scrapyard," "The Assembly-Grave," "The Fall"

### Updated for v5.0

**Voice Layer Changes:**

- v2.0: "metal gods," "titans," "divine ambition"
- v5.0: "Jötun-Forged terraforming units," "industrial mega-constructs," "Pre-Glitch manufacturing sector"

**Setting Compliance:**

- Emphasize 800 years of industrial decay
- Jötun-Forged are **Pre-Glitch mega-scale construction equipment**, not mystical titans
- Undying are maintenance/security units following corrupted protocols
- God-Sleeper Cultists worship Jötun-Forged as technology (cargo cult), not literal gods
- All hazards are industrial failures (power grid, steam systems, structural collapse)

**Architecture Integration:**

- v0.20 Tactical Grid: Extreme verticality, fallen Jötun create multi-level battlefields
- v0.22 Environmental Combat: Live Power Conduits interact with [Flooded] terrain
- v0.15 Trauma Economy: Psychic Stress from proximity to dormant Jötun-Forged (passive)
- v0.21: Undying have high Physical Soak, require armor-shredding

**ASCII Compliance:**

- "Jötunheim" → Internal code uses "Jotunheim" (ASCII)
- "Jötun-Forged" → Display as "Jötun-Forged," store as "Jotun-Forged"
- All entity names internally ASCII, display layer handles special characters

---

## IV. Implementation Structure

### Sub-Specification Breakdown

v0.32 is divided into 4 focused sub-specifications for manageable implementation:

**v0.32.1: Database Schema & Room Templates** (10-14 hours)

- Biomes table extension (biome_id: 7)
- Biome_RoomTemplates table (10+ templates for Trunk and Roots)
- Biome_ResourceDrops table (10 resource types)
- Complete SQL seeding scripts
- Trunk/Roots verticality support (70/30 split)

**v0.32.2: Environmental Hazards & Industrial Terrain** (10-14 hours)

- Biome_EnvironmentalFeatures table
- [Live Power Conduit] signature hazard (interacts with [Flooded])
- [High-Pressure Steam Vent] implementation
- [Unstable Ceiling/Wall] structural collapse system
- 10+ hazard/terrain types
- Integration with Environmental Combat

**v0.32.3: Enemy Definitions & Spawn System** (10-16 hours)

- 6 enemy type definitions (4 Undying, 2 Humanoid, 1 Beast)
- Biome_EnemySpawns table
- Undying Physical Soak patterns (high armor standard)
- Armor-shredding tactical requirement
- Draugr Juggernaut as "teach armor-shredding" enemy

**v0.32.4: Service Implementation & Testing** (8-12 hours)

- JotunheimService complete implementation
- BiomeGenerationService.GenerateJotunheimSector()
- Jötun-Forged terrain generation (fallen giants as battlefields)
- Unit test suite (12+ tests)
- Integration testing scenarios

---

## V. Technical Architecture Overview

### Database Schema Summary

**5 New/Extended Tables:**

1. **Biomes** - Extended with Jötunheim entry (biome_id: 7)
2. **Biome_RoomTemplates** - 10+ templates (7 Trunk, 3 Roots)
3. **Biome_EnvironmentalFeatures** - 12+ hazards/terrain types
4. **Biome_EnemySpawns** - Weighted spawn tables (Undying-heavy)
5. **Biome_ResourceDrops** - 10 resource types with drop rates

### Service Architecture Summary

**JotunheimService** (Primary service)

- ProcessPowerConduitHazard() - Live Power Conduit damage
- ProcessSteamVent() - High-pressure steam damage
- ProcessStructuralCollapse() - Unstable ceiling/wall collapse
- ApplyJotunProximityStress() - Passive Stress near dormant Jötun-Forged

**BiomeGenerationService** (Extension)

- GenerateJotunheimSector() - WFC-based generation with extreme verticality
- PopulateJotunheimRoom() - Enemy/hazard placement
- PlaceJotunCorpseTerrain() - Fallen Jötun-Forged as multi-level terrain
- GenerateCoolantFlood() - [Flooded] terrain placement

**JotunCorpseTerrainService** (New)

- GenerateCorpseLayout() - Create multi-level battlefield from fallen Jötun
- PlaceCorpseFeatures() - Add traversable limbs, hull sections, gantries
- CalculateVerticality() - Determine Z-level differences from corpse structure

### Integration Points

**Existing Systems:**

- v0.10-v0.12: DynamicRoomEngine (procedural generation)
- v0.20: Tactical Grid (extreme verticality, multi-level combat)
- v0.22: Environmental Combat ([Live Power Conduit] + [Flooded] = lethal combo)
- v0.15: Trauma Economy (passive Stress from Jötun proximity)
- v0.21: Undying enemy system (high Physical Soak standard)
- v0.21: Armor-shredding mechanics (required for Draugr Juggernauts)

---

## VI. Quality Standards

### v5.0 Setting Compliance

✅ **Technology, Not Mythology:**

- Jötun-Forged are **Pre-Glitch mega-scale construction equipment**, not titans/gods
- "Assembly yards" are **industrial manufacturing facilities**, not divine forges
- God-Sleeper Cultists are **cargo cultists**, not literal god-worshippers
- All hazards are **industrial system failures**, not curses
- Layer 2 voice: "terraforming unit malfunction," "power grid failure," "structural collapse"

✅ **800-Year Decay:**

- Everything covered in rust and corrosion
- Systems still executing corrupted protocols after centuries
- No one can restart the assembly lines
- Makeshift scavenger camps in the wreckage

✅ **No Supernatural Elements:**

- Undying are **corrupted industrial units**, not undead
- Jötun-Forged dormancy is **system hibernation**, not mystical sleep
- "The Fall" refers to **industrial collapse**, not mythological event

### v2.0 Canonical Accuracy

✅ **Mechanical Preservation:**

- Mid-game positioning (v2.0 value)
- Undying-heavy spawn tables (v2.0 enemy distribution)
- Industrial hazards over ambient conditions (v2.0 design)
- Extreme verticality via fallen Jötun (v2.0 concept)
- Mechanical component focus (v2.0 resource tier)

✅ **Thematic Consistency:**

- "Graveyard of giants" aesthetic (v2.0 core fantasy)
- "Silent monument to failed ambition" (v2.0 narrative)
- Melancholic, not horrifying (v2.0 emotional tone)
- Walking on dead giants (v2.0 environmental storytelling)

### Testing Requirements

✅ **85%+ Coverage Target:**

- Unit tests for power conduit damage calculations
- Integration tests for biome generation
- Edge case tests (conduit + flooded terrain combo)
- Balance validation tests

✅ **Test Categories:**

- Power conduit interactions with flooded terrain
- Steam vent damage and positioning
- Structural collapse triggers
- Jötun proximity Stress accumulation
- Spawn weight system (Undying-heavy verification)
- Resource drop rates

### Logging Requirements

✅ **Serilog Structured Logging:**

- Power conduit activations (damage dealt, targets affected)
- Steam vent eruptions
- Structural collapses (ceiling/wall failures)
- Jötun proximity Stress tracking
- Sector generation metrics
- Enemy spawn counts (verify Undying dominance)
- Resource drop tracking

---

## VII. Success Criteria Checklist

### Functional Requirements

- [ ]  Jötunheim biome generates via DynamicRoomEngine
- [ ]  Trunk/Roots verticality splits correctly (70/30)
- [ ]  [Live Power Conduit] deals damage correctly
- [ ]  [Live Power Conduit] + [Flooded] terrain combo works (amplified damage)
- [ ]  [High-Pressure Steam Vent] erupts on schedule
- [ ]  [Unstable Ceiling/Wall] collapses trigger correctly
- [ ]  Jötun-Forged corpse terrain generates as multi-level battlefields
- [ ]  All 10 room templates can generate
- [ ]  All 6 enemy types spawn correctly (Undying-heavy)
- [ ]  Resource drops occur with correct probabilities
- [ ]  Draugr Juggernauts require armor-shredding to defeat efficiently

### Quality Gates

- [ ]  85%+ unit test coverage achieved
- [ ]  All services use Serilog structured logging
- [ ]  v5.0 setting compliance verified (industrial, not mythological)
- [ ]  v2.0 mechanical values preserved
- [ ]  ASCII-compliant internal naming (display handles "ö")

### Balance Validation

- [ ]  [Live Power Conduit] feels dangerous but predictable
- [ ]  Flooded + conduit combo creates tactical decision-making
- [ ]  Steam vents create positioning challenges
- [ ]  Structural collapses feel cinematic, not frustrating
- [ ]  Undying dominance feels thematic (industrial graveyard)
- [ ]  Draugr Juggernauts teach armor-shredding without being unfair
- [ ]  Resource drops reward exploration

### Integration Verification

- [ ]  Tactical grid extreme verticality works
- [ ]  Environmental Combat conduit + flooded interaction correct
- [ ]  Trauma Economy Jötun proximity Stress accumulates
- [ ]  Undying Physical Soak requires armor-shredding
- [ ]  Quest system Jötunheim hooks available

---

## VIII. Implementation Timeline

**Phase 1: Database Foundation** (v0.32.1) - 10-14 hours

- Create/extend tables
- Populate room templates (Trunk/Roots split)
- Seed resource drops
- Test data integrity

**Phase 2: Environmental Systems** (v0.32.2) - 10-14 hours

- Implement hazard types
- [Live Power Conduit] logic + flooded interaction
- [High-Pressure Steam Vent] mechanics
- [Unstable Ceiling/Wall] collapse system
- Integration testing

**Phase 3: Enemy Content** (v0.32.3) - 10-16 hours

- Define 6 enemy types
- Implement Undying Physical Soak patterns
- Spawn weight system (Undying-heavy verification)
- Draugr Juggernaut armor-shredding requirement

**Phase 4: Services & Testing** (v0.32.4) - 8-12 hours

- JotunheimService complete
- Generation algorithms
- Jötun corpse terrain system
- Unit test suite
- End-to-end testing

**Total: 38-56 hours**

---

## IX. Next Steps

**Total: 38-56 hours**

**Integration Work Remaining:** 11-15 hours

1. Database execution: ~30 minutes
2. Combat Engine integration: 6-8 hours
3. Sector Generation integration: 2-3 hours
4. Manual testing: 2-3 hours

*See v0.32.4 Section IX for complete integration guide.*

---

## IX. Next Steps

Implementation proceeds in order:

1. **Start with v0.32.1** - Database schema provides foundation
2. **Then v0.32.2** - Environmental systems build on schema
3. **Then v0.32.3** - Enemies require hazards to be functional
4. **Finally v0.32.4** - Services orchestrate all components

Each sub-specification is **implementation-ready** and can be completed independently once its prerequisites are met.

---

## X. Related Documents

**Canonical Sources:**

- v2.0: Feature Specification: The Jotunheim Biome[[1]](https://www.notion.so/Feature-Specification-The-Jotunheim-Biome-2a355eb312da803586a5d9233ffc10b2?pvs=21)
- v5.0: Aethelgard Setting Fundamentals[[2]](https://www.notion.so/META-Aethelgard-Setting-Fundamentals-Canonical-Ground-Rules-d9b4c6bed0b0434dae14e8a2767235c3?pvs=21)

**Prerequisites:**

- v0.10-v0.12: Dynamic Room Engine[[3]](https://www.notion.so/v0-10-Dynamic-Room-Engine-Core-f6b2626559d844d78fc65f1fe2c30798?pvs=21)
- v0.20: Tactical Combat Grid[[4]](https://www.notion.so/v0-20-Tactical-Combat-Grid-System-987463086e1547219f70810a6e99fe01?pvs=21)
- v0.22: Environmental Combat[[5]](https://www.notion.so/v0-22-Environmental-Combat-System-f2f10fecac364272a084cd0655b10998?pvs=21)
- v0.15: Trauma Economy[[6]](https://www.notion.so/v0-15-Trauma-Economy-Breaking-Points-Consequences-a1e59f904171485284d6754193af333b?pvs=21)
- v0.21: Undying Enemy System[[7]](https://www.notion.so/v0-20-4-Advanced-Movement-Abilities-35e0ee82ed4344ad824d914d31de7e0a?pvs=21)
- v0.29: Muspelheim Biome (reference)[[8]](v0%2029%20Muspelheim%20Biome%20Implementation%20d725fd4f95a041bfa3d0ee650ef68dd3.md)
- v0.30: Niflheim Biome (reference)[[9]](v0%2030%20Niflheim%20Biome%20Implementation%20106edccf7fa44a2c81f1ec738c809e2f.md)
- v0.31: Alfheim Biome (reference)[[10]](v0%2031%20Alfheim%20Biome%20Implementation%20efa0af4639af46c19be493eb264c0489.md)

**Project Context:**

- Master Roadmap[[11]](https://www.notion.so/Master-Roadmap-v0-1-v1-0-4b4f512f0dd7444486e2c59e676378ad?pvs=21)
- AI Session Handoff[[12]](https://www.notion.so/AI-Session-Handoff-Rune-Rust-Development-Status-e19fe6060d6d4e44ae7402d88e3cc6a3?pvs=21)
- MANDATORY Requirements[[13]](https://www.notion.so/MANDATORY-Specification-Requirements-Quality-Standards-a40022443c8b4abfb8cbd4882839447d?pvs=21)

---

**This parent specification provides the framework. Implementation details are in v0.32.1-v0.32.4 child specifications.**