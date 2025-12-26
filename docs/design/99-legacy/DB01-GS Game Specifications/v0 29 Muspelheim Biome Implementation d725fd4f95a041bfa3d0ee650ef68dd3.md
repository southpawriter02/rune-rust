# v0.29: Muspelheim Biome Implementation

Type: Feature
Description: Implements the geothermal catastrophe biome with [Intense Heat] ambient condition, Forge-Hardened enemies, lava rivers, and Fire/Ice brittleness mechanics. 5 enemy types, 8+ environmental hazards, 9 resource types across 35-50 hours.
Priority: Must-Have
Status: In Design
Target Version: Alpha
Dependencies: v0.10-v0.12 (Dynamic Room Engine), v0.20 (Tactical Grid), v0.22 (Environmental Combat)
Implementation Difficulty: Hard
Balance Validated: No
Proof-of-Concept Flag: No
Sub-item: v0.29.1: Database Schema & Room Templates (v0%2029%201%20Database%20Schema%20&%20Room%20Templates%204459437f44df4cb2aa9f3c4a71efe23d.md), v0.29.2: Environmental Hazards & Ambient Conditions (v0%2029%202%20Environmental%20Hazards%20&%20Ambient%20Conditions%200d398758edcb4560a4b0b4a9875c4a04.md), v0.29.4: Service Implementation & Testing (v0%2029%204%20Service%20Implementation%20&%20Testing%20ce4a69fbcddb40e2ac66db616c52bc6f.md), v0.29.5: Movement System Integration & Forced Movement (v0%2029%205%20Movement%20System%20Integration%20&%20Forced%20Movem%20331def7fa1a94043abad52989df1792a.md), v0.29.3: Enemy Definitions & Spawn System (v0%2029%203%20Enemy%20Definitions%20&%20Spawn%20System%208f34dab3be0e4869bd99f792215abd8a.md)
Template Validated: No
Voice Validated: No

**Document ID:** RR-SPEC-v0.29-MUSPELHEIM

**Status:** Design Complete — Ready for Phase 2 (Database Implementation)

**Timeline:** 35-50 hours total across 4 sub-specifications

**Prerequisites:** v0.10-v0.12 (Dynamic Room Engine), v0.20 (Tactical Grid), v0.22 (Environmental Combat)

---

## I. Executive Summary

### The Fantasy

Muspelheim is the **geothermal catastrophe biome** - the searing depths where civilization's power core has failed catastrophically. Deep geothermal plants, colossal forges, and magma-tap stations now vent raw, untamed heat into the ruins. This is not a natural volcano; it is **industrial hubris melting down**, where containment systems have shattered and regulators have liquefied.

The air shimmers with superheated mirage-distortions. Rivers of molten slag cut through blackened infrastructure. Every breath scalds the lungs. Players descend into hell not because demons live there, but because **the reactor is failing and no one knows how to shut it down**.

### v2.0 Canonical Source

v2.0 Specification: Feature Specification: The Muspelheim Biome[[1]](https://www.notion.so/Feature-Specification-The-Muspelheim-Biome-2a355eb312da80cdab65de771b57e414?pvs=21)

**Migration Status:** Converting from v2.0 concept to full v0.29 implementation with complete database schema, service architecture, enemy definitions, and testing framework.

### Core Mechanics

- **[Intense Heat] Ambient Condition:** Mandatory STURDINESS Resolve checks every turn, failure = Fire damage
- **Forge-Hardened Enemy Mechanic:** Fire-resistant but Ice-vulnerable, [Brittle] debuff system
- **Environmental Lethality:** Lava rivers ([Chasm]), burning ground, steam vents, volatile gas pockets
- **Resource Economy:** Star-Metal Ore, Obsidian Shards, Hardened Servomotors, Heart of the Inferno (legendary: Surtur Engine Core)
- **Late-Game Challenge:** Tests party preparation, Fire Resistance builds, healer stamina management

---

## II. The Rule: What's In vs. Out

### ✅ In Scope (v0.29 Complete)

**Core Systems:**

- Complete Muspelheim biome with procedural generation integration
- [Intense Heat] ambient condition (STURDINESS check DC 12, 2d6 Fire damage on fail)
- Brittleness mechanic (Fire Resistance + Ice Vulnerability → [Brittle] debuff)
- DynamicRoomEngine.GenerateMuspelheimSector() implementation

**Content (5 Enemy Types):**

- Forge-Hardened Undying (Fire Resistant, Ice Vulnerable)
- Magma Elemental (Fire Immune, leaves burning trail)
- Rival Berserker (Fury-driven, Fire adapted)
- Surtur's Herald (Boss framework only)
- Iron-Bane Crusaders (potential allies)

**Environmental Features (8+ Types):**

- [Burning Ground] terrain (1d8 Fire/turn)
- [Chasm/Lava River] obstacles (instant death)
- [High-Pressure Steam Vent] hazards (2d8 + Disoriented)
- [Volatile Gas Pocket] explosives (4d6 AoE)
- Scorched Metal Plating, Molten Slag Pools, etc.

**Resources (9 Types across 4 tiers):**

- Tier 3: Star-Metal Ore, Obsidian Shards, Hardened Servomotors
- Tier 4: Heart of the Inferno (Runic Catalyst), Molten Core Fragment
- Tier 5 (Legendary): Surtur Engine Core, Eternal Ember

**Technical Deliverables:**

- Complete database schema (5 new tables, 8 room templates)
- Service implementation (MuspelheimService, BiomeGenerationService extensions)
- Unit test suite (10+ tests, ~85% coverage)
- Integration with existing tactical combat and environmental systems
- v5.0 setting compliance (thermal regulation failure, not supernatural)
- Serilog structured logging throughout

### ❌ Explicitly Out of Scope

- Additional biomes (Niflheim v0.30, Alfheim v0.31, Jotunheim v0.32)
- Advanced heat-based puzzle mechanics (pressure valve mini-games) (defer to v0.34)
- Surtur's Herald **full boss fight** (framework only; complete multi-phase encounter in v0.35)
- Legendary crafting recipes using Muspelheim materials (defer to v0.36)
- Biome-specific achievements/statistics tracking (defer to v0.38)
- Fire Resistance equipment set expansion (defer to v0.36)
- Environmental storytelling audio/visual polish (separate phase)

---

## III. v2.0 → v5.0 Migration Summary

### Preserved from v2.0

**Mechanical Intent:**

- [Intense Heat] as relentless HP tax (STURDINESS checks)
- Fire Resistance as near-mandatory survival preparation
- Forge-Hardened enemies: Fire Resistance + Ice Vulnerability
- [Brittle] mechanic: Ice attack → Physical vulnerability
- Late-game positioning: [Roots] Z-level exclusively
- Environmental lethality: Lava, burning ground, steam
- Resource tiers: Star-Metal Ore, Obsidian as Tier 3 crafting

**Thematic Elements:**

- Catastrophic power core failure (industrial, not supernatural)
- Oppressive heat as constant existential threat
- Scorched metal aesthetics
- Hubris: civilization tried to "tame a volcano" and failed
- Contrasts with Niflheim's cold silence

### Updated for v5.0

**Voice Layer Changes:**

- v2.0: "geothermal hazard"
- v5.0: "thermal regulation failure," "containment breach," "reactor meltdown scenario"

**Setting Compliance:**

- Emphasize 800 years of Pre-Glitch system decay
- Magma Elementals = **corrupted geological monitoring constructs** (fell into magma), NOT supernatural fire beings
- Heat effects use engineering terminology (thermal load, heat dissipation failure, ablative shielding)
- No "fire magic" - all heat is technological/geological

**Architecture Integration:**

- v0.20 Tactical Grid: Lava rivers as [Chasm] obstacles
- v0.22 Environmental Combat: Destructible steam vents, gas ignition
- v0.15 Trauma Economy: Extreme heat = Stress accumulation

**ASCII Compliance:**

- "Surtur's Herald" (already compliant)
- All entity names verified ASCII-only

---

## IV. Implementation Structure

### Sub-Specification Breakdown

v0.29 is divided into 4 focused sub-specifications for manageable implementation:

**v0.29.1: Database Schema & Room Templates** (8-12 hours)

- Biomes table extension
- Biome_RoomTemplates table (8 templates)
- Biome_ResourceDrops table (9 resource types)
- Characters_BiomeStatus table
- Complete SQL seeding scripts

**v0.29.2: Environmental Hazards & Ambient Conditions** (8-12 hours)

- Biome_EnvironmentalFeatures table
- [Intense Heat] condition implementation
- 8+ hazard/terrain types with mechanics
- Integration with ConditionService
- Hazard interaction system

**v0.29.3: Enemy Definitions & Spawn System** (10-15 hours)

- 5 enemy type definitions
- Biome_EnemySpawns table
- Brittleness mechanic (Fire Resistance + Ice Vulnerability)
- Spawn weight system
- Surtur's Herald boss framework

**v0.29.4: Service Implementation & Testing** (9-15 hours)

- MuspelheimService complete implementation
- BiomeGenerationService.GenerateMuspelheimSector()
- IntenseHeatService
- Unit test suite (10+ tests)
- Integration testing scenarios

---

## V. Technical Architecture Overview

### Database Schema Summary

**5 New/Extended Tables:**

1. **Biomes** - Extended with Muspelheim entry (biome_id: 4)
2. **Biome_RoomTemplates** - 8 Muspelheim room templates
3. **Biome_EnvironmentalFeatures** - 10+ hazards/terrain types
4. **Biome_EnemySpawns** - Weighted spawn tables
5. **Biome_ResourceDrops** - 9 resource types with drop rates
6. **Characters_BiomeStatus** - Per-character biome tracking

### Service Architecture Summary

**MuspelheimService** (Primary service)

- ApplyIntenseHeat() - End-of-turn ambient condition
- TryApplyBrittleDebuff() - Ice → [Brittle] mechanic
- CalculateHeatDamage() - Resistance calculations

**BiomeGenerationService** (Extension)

- GenerateMuspelheimSector() - WFC-based generation
- PopulateMuspelheimRoom() - Enemy/hazard placement
- PlaceLavaRivers() - [Chasm] obstacle generation

**IntenseHeatService** (New)

- ProcessHeatChecks() - STURDINESS Resolve checks
- ApplyFireResistance() - Damage mitigation
- TrackBiomeStatus() - Statistics persistence

### Integration Points

**Existing Systems:**

- v0.10-v0.12: DynamicRoomEngine (procedural generation)
- v0.20: Tactical Grid (lava as [Chasm])
- v0.22: Environmental Combat (destructible vents)
- v0.15: Trauma Economy (heat Stress)
- v0.21: Status Effects ([Brittle], [Disoriented])

---

## VI. Quality Standards

### v5.0 Setting Compliance

✅ **Technology, Not Magic:**

- All heat effects are **thermal regulation failures**
- Magma Elementals are **corrupted monitoring drones**
- [Intense Heat] is **containment breach**, not curse
- Layer 2 voice: "reactor core," "thermal load," "ablative shielding"

✅ **800-Year Decay:**

- Systems failing after centuries of neglect
- No one can repair or shut down the reactors
- Jury-rigged coolant systems, makeshift heat shields

✅ **No Supernatural Elements:**

- Surtur = Jötun-Forged warmachine name (Norse inspiration)
- Heat is physics/engineering, not elemental magic
- Fire damage = thermal energy transfer

### v2.0 Canonical Accuracy

✅ **Mechanical Preservation:**

- [Intense Heat] DC 12 STURDINESS check (v2.0 value)
- 2d6 Fire damage on failure (v2.0 value)
- Fire Resistance 75% on Forge-Hardened (v2.0 concept)
- Ice → [Brittle] → Physical vulnerability (v2.0 mechanic)

✅ **Thematic Consistency:**

- "Test of attrition and preparation" (v2.0 philosophy)
- "Controller's playground" (Push into lava) (v2.0 gameplay)
- "Inverse of Niflheim" (v2.0 contrast)

### Testing Requirements

✅ **80%+ Coverage Target:**

- Unit tests for heat calculations
- Integration tests for biome generation
- Edge case tests (0% resistance, 100% immunity)
- Balance validation tests

✅ **Test Categories:**

- Intense Heat application and damage
- Brittleness mechanic triggering
- Fire Resistance calculations
- Spawn weight system
- Resource drop rates

### Logging Requirements

✅ **Serilog Structured Logging:**

- Heat check results (pass/fail, damage)
- Brittle application events
- Sector generation metrics
- Enemy spawn counts
- Resource drop tracking

---

## VII. Success Criteria Checklist

### Functional Requirements

- [ ]  Muspelheim biome generates via DynamicRoomEngine
- [ ]  [Intense Heat] applies to all combatants each turn
- [ ]  STURDINESS checks resolve correctly (DC 12)
- [ ]  Fire damage applies with Resistance calculations
- [ ]  [Brittle] debuff triggers on Ice → Fire Resistant targets
- [ ]  Lava rivers function as [Chasm] obstacles
- [ ]  All 8 room templates can generate
- [ ]  All 5 enemy types spawn correctly
- [ ]  Resource drops occur with correct probabilities
- [ ]  Biome statistics track per character

### Quality Gates

- [ ]  80%+ unit test coverage achieved
- [ ]  All services use Serilog structured logging
- [ ]  v5.0 setting compliance verified (no magic language)
- [ ]  v2.0 mechanical values preserved
- [ ]  ASCII-only entity names confirmed

### Balance Validation

- [ ]  [Intense Heat] damage feels like constant pressure
- [ ]  Fire Resistance provides meaningful survival advantage
- [ ]  Ice → [Brittle] combo is discoverable and rewarding
- [ ]  Party without Fire prep struggles but can adapt
- [ ]  Resource drops feel appropriately rare/common

### Integration Verification

- [ ]  Tactical grid lava rivers block movement
- [ ]  Environmental Combat steam vents destructible
- [ ]  Trauma Economy heat Stress accumulates
- [ ]  Status Effects [Brittle] applies vulnerabilities
- [ ]  Quest system Muspelheim hooks available

---

## VIII. Implementation Timeline

**Phase 1: Database Foundation** (v0.29.1) - 8-12 hours

- Create/extend tables
- Populate room templates
- Seed resource drops
- Test data integrity

**Phase 2: Environmental Systems** (v0.29.2) - 8-12 hours

- Implement hazard types
- [Intense Heat] condition logic
- Terrain interaction system
- Integration testing

**Phase 3: Enemy Content** (v0.29.3) - 10-15 hours

- Define 5 enemy types
- Implement brittleness mechanic
- Spawn weight system
- Boss framework

**Phase 4: Services & Testing** (v0.29.4) - 9-15 hours

- MuspelheimService complete
- Generation algorithms
- Unit test suite
- End-to-end testing

**Total: 35-54 hours**

---

## IX. Next Steps

Implementation proceeds in order:

1. **Start with v0.29.1** - Database schema provides foundation
2. **Then v0.29.2** - Environmental systems build on schema
3. **Then v0.29.3** - Enemies require hazards to be functional
4. **Finally v0.29.4** - Services orchestrate all components

Each sub-specification is **implementation-ready** and can be completed independently once its prerequisites are met.

---

## X. Related Documents

**Canonical Sources:**

- v2.0: Feature Specification: The Muspelheim Biome[[1]](https://www.notion.so/Feature-Specification-The-Muspelheim-Biome-2a355eb312da80cdab65de771b57e414?pvs=21)
- v5.0: Aethelgard Setting Fundamentals[[2]](https://www.notion.so/META-Aethelgard-Setting-Fundamentals-Canonical-Ground-Rules-d9b4c6bed0b0434dae14e8a2767235c3?pvs=21)

**Prerequisites:**

- v0.10-v0.12: Dynamic Room Engine[[3]](https://www.notion.so/v0-10-Dynamic-Room-Engine-Core-f6b2626559d844d78fc65f1fe2c30798?pvs=21)
- v0.20: Tactical Combat Grid[[4]](https://www.notion.so/v0-20-Tactical-Combat-Grid-System-987463086e1547219f70810a6e99fe01?pvs=21)
- v0.22: Environmental Combat[[5]](https://www.notion.so/v0-22-Environmental-Combat-System-f2f10fecac364272a084cd0655b10998?pvs=21)

**Project Context:**

- Master Roadmap[[6]](https://www.notion.so/Master-Roadmap-v0-1-v1-0-4b4f512f0dd7444486e2c59e676378ad?pvs=21)
- AI Session Handoff[[7]](https://www.notion.so/AI-Session-Handoff-Rune-Rust-Development-Status-e19fe6060d6d4e44ae7402d88e3cc6a3?pvs=21)
- MANDATORY Requirements[[8]](https://www.notion.so/MANDATORY-Specification-Requirements-Quality-Standards-a40022443c8b4abfb8cbd4882839447d?pvs=21)

---

**This parent specification provides the framework. Implementation details are in v0.29.1-v0.29.4 child specifications.**