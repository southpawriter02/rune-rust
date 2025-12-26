# v0.31: Alfheim Biome Implementation

Type: Feature
Description: Implements the Aetheric catastrophe biome with [Runic Instability] Wild Magic Surges, [Psychic Resonance], Reality Tears, and Canopy-exclusive verticality. Endgame challenge testing Mystic mastery and WILL attribute. 35-50 hours.
Priority: Must-Have
Status: In Design
Target Version: Alpha
Dependencies: v0.30 (Niflheim), v0.20 (Tactical Grid), v0.22 (Environmental Combat)
Implementation Difficulty: Hard
Balance Validated: No
Proof-of-Concept Flag: No
Sub-item: v0.31.4: Service Implementation & Testing (v0%2031%204%20Service%20Implementation%20&%20Testing%20539e060d40734aa88823b247166c44e6.md), v0.31.2: Environmental Hazards & Ambient Conditions (v0%2031%202%20Environmental%20Hazards%20&%20Ambient%20Conditions%20babad7a1095a4acea8314f2e9162344c.md), v0.31.3: Enemy Definitions & Spawn System (v0%2031%203%20Enemy%20Definitions%20&%20Spawn%20System%2014b78664d0ba43b8ac6ae5df815c2cce.md), v0.31.1: Database Schema & Room Templates (v0%2031%201%20Database%20Schema%20&%20Room%20Templates%20b3f608de386941b5a8a45ddfa962641a.md)
Template Validated: No
Voice Validated: No

**Document ID:** RR-SPEC-v0.31-ALFHEIM

**Status:** Design Complete — Ready for Implementation

**Timeline:** 35-50 hours total across 4 sub-specifications

**Prerequisites:** v0.30 (Niflheim complete), v0.20 (Tactical Grid), v0.22 (Environmental Combat)

---

## I. Executive Summary

### The Fantasy

Alfheim is the **Aetheric catastrophe biome** - the unstable energy research laboratories where reality itself is bleeding. Pre-Glitch Aetheric research facilities, power conduits, and reality-manipulation experiments have ruptured, leaking raw, uncontrolled Aetheric energy into the world. This is not natural lightning; it is **industrial-scale reality manipulation gone catastrophically wrong**, where containment fields have collapsed and Aetheric generators cycle out of control.

The air crackles with unstable energy. Crystalline structures hum with multi-colored, chaotic light. Platforms flicker between solid and incorporeal states. Players navigate a world where the laws of physics are suggestions, where **Aetheric energy is a hostile, reality-warping force** and every step risks exposure to metaphysical corruption.

### v2.0 Canonical Source

v2.0 Specification: Feature Specification: The Alfheim Biome[[1]](https://www.notion.so/Feature-Specification-The-Alfheim-Biome-2a355eb312da803888c2dc2549e6940f?pvs=21)

**Migration Status:** Converting from v2.0 concept to full v0.31 implementation with complete database schema, service architecture, enemy definitions, and testing framework.

### Core Mechanics

- **[Runic Instability] Ambient Condition:** Wild Magic Surges, unpredictable spell effects, Reality Glitches
- **[Psychic Resonance] Secondary:** High Stress accumulation from Aetheric exposure
- **Reality Tear Mechanic:** Hazards that warp positioning and cause Corruption
- **Resource Economy:** Aetheric Components, Crystallized Aether, Pure Aether Shards (legendary: Fragment of the All-Rune)
- **Endgame Challenge:** Tests Mystic mastery, WILL attribute, Trauma Economy resilience

---

## II. The Rule: What's In vs. Out

### ✅ In Scope (v0.31 Complete)

**Core Systems:**

- Complete Alfheim biome with procedural generation integration
- [Runic Instability] ambient condition (Wild Magic Surges, spell volatility)
- [Psychic Resonance] high-intensity variant (constant Stress pressure)
- Reality Tear mechanic (positional warping + Corruption)
- DynamicRoomEngine.GenerateAlfheimSector() implementation
- Canopy-exclusive verticality (high-altitude only)

**Content (5 Enemy Types):**

- Aether-Vulture (Energy-based aerial predator)
- Energy Elemental (Pure Aetheric manifestation)
- Forlorn Echo (The Original Dead - strongest psychic echoes)
- Crystalline Construct (Animated by Blight)
- All-Rune's Echo (boss framework - reality-warping entity)

**Environmental Features (8+ Types):**

- [Runic Instability] (ambient - Wild Magic Surges)
- [Reality Tear] (positional hazard - warps movement)
- [Psychic Resonance] (high-intensity Stress)
- [Low Gravity Pocket] (physics disruption)
- [Unstable Platform] (flickers between solid/incorporeal)
- [Energy Conduit] (damage + buffs for Energy resistance)
- [Aetheric Vortex] (pull effect + damage)
- [Crystalline Spire] (cover + energy reflection)

**Resources (9 Types across 4 tiers):**

- Tier 2-3: Aetheric Residue, Energy Crystal Shard, Unstable Aether Sample
- Tier 4: Pure Aether Shard (Runic Catalyst), Crystallized Aether, Coherent Power Coil
- Tier 5 (Legendary): Fragment of the All-Rune, Reality Anchor Core

**Technical Deliverables:**

- Complete database schema (5 new tables, 8 room templates)
- Service implementation (AlfheimService, BiomeGenerationService extensions)
- Unit test suite (10+ tests, ~85% coverage)
- Integration with existing tactical combat and Mystic systems
- v5.0 setting compliance (Aetheric research facility failure, not supernatural)
- Serilog structured logging throughout

### ❌ Explicitly Out of Scope

- Additional biomes (Jotunheim v0.32, others defer to later phases)
- Advanced Wild Magic Surge tables (basic implementation only, expand in v0.34)
- All-Rune's Echo **full boss fight** (framework only; complete encounter in v0.35)
- Legendary crafting recipes using Alfheim materials (defer to v0.36)
- Biome-specific achievements/statistics tracking (defer to v0.38)
- Aetheric Resistance equipment set expansion (defer to v0.36)
- Environmental storytelling audio/visual polish (separate phase)
- Advanced physics simulation (time dilation, gravity inversion) (defer to v0.37)

---

## III. v2.0 → v5.0 Migration Summary

### Preserved from v2.0

**Mechanical Intent:**

- [Runic Instability] as Wild Magic Surge generator
- [Psychic Resonance] at highest intensity
- Reality Tears as positional hazards
- Endgame positioning: Late-game biome for prepared parties
- Canopy-exclusive (high-altitude facilities)
- Resource tier: Aetheric Components, Fragment of the All-Rune
- Mystic specialization challenge: Wild Magic makes casting dangerous

**Thematic Elements:**

- Aetheric research facility catastrophe (industrial, not supernatural)
- Reality bleeding from failed experiments
- Beautiful but terrifying chaos
- Floating platforms and disconnected spaces
- Crystalline structures and unstable energy
- Hubris: civilization tried to control reality itself
- "Open wound in crashed reality's operating system"

### Updated for v5.0

**Voice Layer Changes:**

- v2.0: "metaphysical corruption," "magic gone mad"
- v5.0: "Aetheric research containment failure," "reality manipulation system crash," "energy conduit rupture"

**Setting Compliance:**

- Emphasize 800 years of unstable Aetheric energy leakage
- Energy Elementals = **Aetheric manifestations from ruptured containment**, NOT elemental spirits
- Crystalline Constructs = **Pre-Glitch structures animated by Aetheric Blight**, NOT magical golems
- Wild Magic Surges use engineering terminology (feedback loops, resonance cascade, quantum flux)
- No "pure magic" - all Aetheric effects are technological/physics system failures

**Architecture Integration:**

- v0.20 Tactical Grid: Reality Tears warp positioning, Low Gravity affects movement
- v0.22 Environmental Combat: Crystalline Spires destructible, Energy Conduits interactive
- v0.15 Trauma Economy: High Psychic Resonance = extreme Stress accumulation
- v0.19 Mystic Integration: Wild Magic Surges modify Mystic abilities

**ASCII Compliance:**

- "All-Rune's Echo" (already compliant)
- "Aether-Vulture" (already compliant)
- All entity names verified ASCII-only

---

## IV. Implementation Structure

### Sub-Specification Breakdown

v0.31 is divided into 4 focused sub-specifications for manageable implementation:

**v0.31.1: Database Schema & Room Templates** (8-12 hours)

- Biomes table extension (biome_id: 6)
- Biome_RoomTemplates table (8 templates for [Canopy] only)
- Biome_ResourceDrops table (9 resource types)
- Complete SQL seeding scripts
- Canopy-exclusive verticality support data

**v0.31.2: Environmental Hazards & Ambient Conditions** (8-12 hours)

- Biome_EnvironmentalFeatures table
- [Runic Instability] condition implementation (Wild Magic Surges)
- [Psychic Resonance] high-intensity implementation
- 8+ hazard/terrain types with mechanics
- Integration with ConditionService and Trauma Economy
- Reality Tear positioning system

**v0.31.3: Enemy Definitions & Spawn System** (10-15 hours)

- 5 enemy type definitions
- Biome_EnemySpawns table
- Energy/Aetheric resistance patterns
- Spawn weight system
- All-Rune's Echo boss framework

**v0.31.4: Service Implementation & Testing** (9-15 hours)

- AlfheimService complete implementation
- BiomeGenerationService.GenerateAlfheimSector()
- RunicInstabilityService (Wild Magic Surge system)
- RealityTearService (positional warping)
- Unit test suite (10+ tests)
- Integration testing scenarios

---

## V. Technical Architecture Overview

### Database Schema Summary

**5 New/Extended Tables:**

1. **Biomes** - Extended with Alfheim entry (biome_id: 6)
2. **Biome_RoomTemplates** - 8 Alfheim room templates (all Canopy)
3. **Biome_EnvironmentalFeatures** - 10+ hazards/terrain types
4. **Biome_EnemySpawns** - Weighted spawn tables
5. **Biome_ResourceDrops** - 9 resource types with drop rates

### Service Architecture Summary

**AlfheimService** (Primary service)

- ApplyRunicInstability() - Wild Magic Surge triggering
- ProcessWildMagicSurge() - Random spell effect modification
- ApplyPsychicResonance() - High-intensity Stress accumulation
- ProcessRealityTear() - Positional warping and Corruption

**BiomeGenerationService** (Extension)

- GenerateAlfheimSector() - WFC-based generation
- PopulateAlfheimRoom() - Enemy/hazard placement
- PlaceFloatingPlatforms() - Disconnected platform generation
- GenerateRealityTears() - Hazard placement with tactical consideration

**RunicInstabilityService** (New)

- TriggerWildMagicSurge() - Random effect on Mystic ability use
- CalculateSurgeIntensity() - Difficulty-based surge strength
- ApplySurgeEffect() - Modify spell effects (damage, range, targets)
- TrackSurgeStatistics() - Logging and balance data

**RealityTearService** (New)

- ProcessTearEncounter() - Character enters Reality Tear
- WarpPosition() - Teleport to random valid tile
- ApplyCorruption() - Corruption threshold increase
- CalculateTearDamage() - Energy damage on warp

### Integration Points

**Existing Systems:**

- v0.10-v0.12: DynamicRoomEngine (procedural generation)
- v0.20: Tactical Grid (Reality Tears modify positioning)
- v0.22: Environmental Combat (destructible Crystalline Spires)
- v0.15: Trauma Economy (Psychic Resonance Stress, Corruption)
- v0.19: Mystic abilities (Wild Magic Surges modify casting)
- v0.21: Status Effects (Energy Vulnerability, Aetheric Corruption)

---

## VI. Quality Standards

### v5.0 Setting Compliance

✅ **Technology, Not Magic:**

- All Aetheric effects are **research facility failures**
- Energy Elementals are **ruptured containment manifestations**
- Crystalline Constructs are **Pre-Glitch structures animated by Blight**
- [Runic Instability] is **feedback loop in Aetheric generators**, not curse
- Layer 2 voice: "Aetheric containment breach," "reality manipulation failure," "energy field collapse"

✅ **800-Year Decay:**

- Systems failing after centuries of uncontrolled energy discharge
- No one can repair or stabilize Aetheric generators
- Makeshift shielding, improvised containment fields

✅ **No Supernatural Elements:**

- All-Rune's Echo = **sentient Reality Glitch**, not demon (Norse inspiration)
- Aetheric energy is **physics/quantum mechanics failure**, not elemental magic
- Energy damage = **radiation/electromagnetic exposure**, not holy/unholy magic

### v2.0 Canonical Accuracy

✅ **Mechanical Preservation:**

- [Runic Instability] Wild Magic Surge system (v2.0 concept)
- [Psychic Resonance] highest intensity (v2.0 value)
- Reality Tears as positional hazards (v2.0 concept)
- Canopy-exclusive verticality (v2.0 positioning)

✅ **Thematic Consistency:**

- "Ultimate test of WILL" (v2.0 philosophy)
- "Mystic's crucible" (dangerous but powerful) (v2.0 gameplay)
- "Open wound in reality's operating system" (v2.0 narrative)

### Testing Requirements

✅ **80%+ Coverage Target:**

- Unit tests for Wild Magic Surge calculations
- Integration tests for biome generation
- Edge case tests (surge on 0 Aether Pool, tear on edge tiles)
- Balance validation tests

✅ **Test Categories:**

- Runic Instability application and surge triggering
- Psychic Resonance Stress accumulation
- Reality Tear positioning and Corruption
- Energy Resistance calculations
- Spawn weight system
- Resource drop rates

### Logging Requirements

✅ **Serilog Structured Logging:**

- Wild Magic Surge events (ability, surge effect, result)
- Psychic Resonance Stress tracking
- Reality Tear encounters (position before/after, Corruption applied)
- Sector generation metrics
- Enemy spawn counts
- Resource drop tracking

---

## VII. Success Criteria Checklist

### Functional Requirements

- [ ]  Alfheim biome generates via DynamicRoomEngine
- [ ]  [Runic Instability] triggers Wild Magic Surges on Mystic abilities
- [ ]  Wild Magic Surges modify spell effects correctly
- [ ]  [Psychic Resonance] applies high Stress accumulation
- [ ]  Reality Tears warp character positions
- [ ]  Reality Tears apply Corruption correctly
- [ ]  All 8 room templates can generate (Canopy only)
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

- [ ]  [Runic Instability] feels dangerous but not punishing
- [ ]  Wild Magic Surges create tactical decision-making
- [ ]  [Psychic Resonance] is highest intensity but manageable
- [ ]  Reality Tears create positioning challenge
- [ ]  Party without high WILL struggles but can adapt
- [ ]  Mystics feel challenged but not invalidated
- [ ]  Resource drops feel appropriately rare/common

### Integration Verification

- [ ]  Tactical grid Reality Tears teleport correctly
- [ ]  Environmental Combat Crystalline Spires destructible
- [ ]  Trauma Economy Stress/Corruption accumulates
- [ ]  Mystic abilities interact with Wild Magic Surges
- [ ]  Status Effects apply correctly
- [ ]  Quest system Alfheim hooks available

---

## VIII. Implementation Timeline

**Phase 1: Database Foundation** (v0.31.1) - 8-12 hours

- Create/extend tables
- Populate room templates
- Seed resource drops
- Test data integrity

**Phase 2: Environmental Systems** (v0.31.2) - 8-12 hours

- Implement hazard types
- [Runic Instability] condition logic
- [Psychic Resonance] mechanics
- Reality Tear system
- Integration testing

**Phase 3: Enemy Content** (v0.31.3) - 10-15 hours

- Define 5 enemy types
- Implement Energy resistance patterns
- Spawn weight system
- Boss framework

**Phase 4: Services & Testing** (v0.31.4) - 9-15 hours

- AlfheimService complete
- Generation algorithms
- Unit test suite
- End-to-end testing

**Total: 35-54 hours**

---

## IX. Next Steps

**Total: 35-54 hours**

**Integration Work Remaining:** 9-12 hours

- Database execution: 30 minutes
- CombatEngine integration: 6-8 hours
- Manual testing: 2-3 hours

*See v0.31.4 Section IX for complete integration guide.*

---

## IX. Next Steps

Implementation proceeds in order:

1. **Start with v0.31.1** - Database schema provides foundation
2. **Then v0.31.2** - Environmental systems build on schema
3. **Then v0.31.3** - Enemies require hazards to be functional
4. **Finally v0.31.4** - Services orchestrate all components

Each sub-specification is **implementation-ready** and can be completed independently once its prerequisites are met.

---

## X. Related Documents

**Canonical Sources:**

- v2.0: Feature Specification: The Alfheim Biome[[1]](https://www.notion.so/Feature-Specification-The-Alfheim-Biome-2a355eb312da803888c2dc2549e6940f?pvs=21)
- v5.0: Aethelgard Setting Fundamentals[[2]](https://www.notion.so/META-Aethelgard-Setting-Fundamentals-Canonical-Ground-Rules-d9b4c6bed0b0434dae14e8a2767235c3?pvs=21)

**Prerequisites:**

- v0.10-v0.12: Dynamic Room Engine[[3]](https://www.notion.so/v0-10-Dynamic-Room-Engine-Core-f6b2626559d844d78fc65f1fe2c30798?pvs=21)
- v0.20: Tactical Combat Grid[[4]](https://www.notion.so/v0-20-Tactical-Combat-Grid-System-987463086e1547219f70810a6e99fe01?pvs=21)
- v0.22: Environmental Combat[[5]](https://www.notion.so/v0-22-Environmental-Combat-System-f2f10fecac364272a084cd0655b10998?pvs=21)
- v0.15: Trauma Economy[[6]](https://www.notion.so/v0-15-Trauma-Economy-Breaking-Points-Consequences-a1e59f904171485284d6754193af333b?pvs=21)
- v0.29: Muspelheim Biome (reference)[[7]](v0%2029%20Muspelheim%20Biome%20Implementation%20d725fd4f95a041bfa3d0ee650ef68dd3.md)
- v0.30: Niflheim Biome (reference)[[8]](v0%2030%20Niflheim%20Biome%20Implementation%20106edccf7fa44a2c81f1ec738c809e2f.md)

**Project Context:**

- Master Roadmap[[9]](https://www.notion.so/Master-Roadmap-v0-1-v1-0-4b4f512f0dd7444486e2c59e676378ad?pvs=21)
- AI Session Handoff[[10]](https://www.notion.so/AI-Session-Handoff-Rune-Rust-Development-Status-e19fe6060d6d4e44ae7402d88e3cc6a3?pvs=21)
- MANDATORY Requirements[[11]](https://www.notion.so/MANDATORY-Specification-Requirements-Quality-Standards-a40022443c8b4abfb8cbd4882839447d?pvs=21)

---

**This parent specification provides the framework. Implementation details are in v0.31.1-v0.31.4 child specifications.**