# v0.33: Faction System & Reputation

Type: Mechanic
Description: Implements 5 major factions (Iron-Banes, God-Sleeper Cultists, Jötun-Readers, Rust-Clans, Independents) with reputation tracking, faction-gated quests, merchant price modifiers, and dynamic world reactions. 30-45 hours.
Priority: Must-Have
Status: In Design
Target Version: Alpha
Dependencies: v0.32 (Jötunheim), v0.14 (Quest System), v0.8 (NPC System)
Implementation Difficulty: Hard
Balance Validated: No
Proof-of-Concept Flag: No
Sub-item: v0.33.2: Reputation Mechanics & World Reactions (v0%2033%202%20Reputation%20Mechanics%20&%20World%20Reactions%2051bd21199ed84d549fba3c07f78274ab.md), v0.33.1: Database Schema & Faction Definitions (v0%2033%201%20Database%20Schema%20&%20Faction%20Definitions%20b177dce2a1ef4462ad40d2a0fa6e9bd0.md), v0.33.3: Faction Quests & Rewards (v0%2033%203%20Faction%20Quests%20&%20Rewards%2057b023e9b37c44e8b814ca9ecfc6822b.md), v0.33.4: Service Implementation & Testing (v0%2033%204%20Service%20Implementation%20&%20Testing%202e85779b01624124914519ecb0dc2b63.md)
Template Validated: No
Voice Validated: No

**Document ID:** RR-SPEC-v0.33-FACTIONS

**Status:** Design Complete — Ready for Implementation

**Timeline:** 30-45 hours total across 4 sub-specifications

**Prerequisites:** v0.32 (Jötunheim complete), v0.14 (Quest System), v0.8 (NPC System)

---

## I. Executive Summary

### The Fantasy

The Faction System brings **social consequences and political depth** to Aethelgard. Every action has witnesses. Every choice has factions that approve or condemn. Your reputation precedes you - opening doors with some while closing them with others.

This is not a simple good/evil morality system. It's a **web of competing interests** where logical choices create enemies. Help the Iron-Banes purge Undying? The God-Sleeper Cultists mark you as a heretic. Share technology with Jötun-Readers? Rust-Clans see you as a hoarder.

**The player fantasy:**

"I saved that scavenger caravan from Undying. Word spreads. Now Iron-Bane patrols let me pass without challenge, merchants give me better prices, and their quartermaster offers me faction-exclusive equipment. But the God-Sleeper Cultists remember—I killed their 'sacred' constructs. They ambush me in Jötunheim. Reputation has consequences."

### v2.0 Canonical Source

No direct v2.0 specification exists for the faction system. However, factions are referenced throughout v2.0 content:

- Iron-Banes (anti-Undying zealots)
- God-Sleeper Cultists (Jötun-Forged worshippers)
- Jötun-Readers (Pre-Glitch scholars)
- Rust-Clans (Midgard survivors)
- Independents (unaffiliated)

This specification creates the **system architecture** to support faction interactions canonical to v2.0.

### Core Mechanics

- **5 Major Factions** with distinct philosophies and goals
- **Reputation Scale:** -100 (Hated) to +100 (Exalted)
- **Dynamic World Reactions:** Prices, quest availability, random encounters
- **Faction-Specific Quests:** Unlock at reputation thresholds
- **Exclusive Rewards:** Equipment, abilities, and services
- **Mutually Exclusive Paths:** Cannot maximize all factions simultaneously
- **Witness System:** Actions affect reputation when observed

---

## II. The Rule: What's In vs. Out

### ✅ In Scope (v0.33 Complete)

**Core Systems:**

- Complete faction architecture (5 major factions)
- Reputation tracking system (-100 to +100 scale)
- Dynamic world reactions to reputation
- Faction-specific quest unlocks
- Merchant price modifiers
- Random encounter frequency modifiers
- Witness system for action consequences
- Faction hostility and combat triggers

**5 Major Factions:**

- **Iron-Banes** (Anti-Undying zealots, Trunk/Roots presence)
- **God-Sleeper Cultists** (Jötun-Forged worshippers, Jötunheim)
- **Jötun-Readers** (Pre-Glitch scholars, knowledge seekers)
- **Rust-Clans** (Midgard survivors, pragmatic scavengers)
- **Independents** (Unaffiliated faction for neutral characters)

**Content (Per Faction):**

- Faction philosophy and goals
- 5+ reputation-gated quests
- 3+ exclusive equipment/services
- Reputation thresholds (Hated/Hostile/Neutral/Friendly/Allied/Exalted)
- Faction-specific dialogue and encounters

**Technical Deliverables:**

- Complete database schema (4 new tables)
- Service implementation (FactionService, ReputationService)
- Unit test suite (12+ tests, ~85% coverage)
- Integration with Quest System and NPC System
- v5.0 setting compliance (cargo cults, not religious orders)
- Serilog structured logging throughout

### ❌ Explicitly Out of Scope

- Full faction warfare system (defer to v0.35 - Territory Control)
- Faction bases and headquarters (defer to v0.34)
- Faction companion recruitment (defer to v0.34 - Companion System)
- Multiple faction endings (defer to v0.40+)
- Faction-specific specializations (defer to v2.0+)
- Faction territory ownership (defer to v0.35)
- Inter-faction diplomacy quests (defer to v0.35)
- Advanced faction AI behaviors (defer to v0.36)

---

## III. v2.0 → v5.0 Migration Summary

### Preserved from v2.0

**Faction Identities:**

- Iron-Banes: Anti-Undying zealots who hunt corrupted constructs
- God-Sleeper Cultists: Worship Jötun-Forged as sleeping gods
- Jötun-Readers: Scholars seeking Pre-Glitch knowledge
- Rust-Clans: Midgard survivors focused on practical survival
- Independents: Unaffiliated individuals

**Mechanical Intent:**

- Actions have faction consequences
- Mutually exclusive faction paths
- Reputation affects world reactions
- Faction-specific rewards

### Updated for v5.0

**Voice Layer Changes:**

- ❌ v2.0: "Holy crusaders," "priests," "faithful"
- ✅ v5.0: "Purification protocols," "cargo cultists," "data-archaeologists"

**Setting Compliance:**

- **Iron-Banes:** Not religious zealots, but **anti-corruption specialists** following purification protocols
- **God-Sleeper Cultists:** Not literal god-worshippers, but **cargo cultists** who believe Jötun-Forged are sleeping deities
- **Jötun-Readers:** Not wizards, but **data archaeologists** and system analysts
- **Rust-Clans:** Not barbarians, but **pragmatic survivors** with functional knowledge
- All factions grounded in post-Glitch survival responses

**Architecture Integration:**

- v0.14 Quest System: Faction quests use existing quest architecture
- v0.8 NPC System: Faction NPCs use existing dialogue trees
- v0.3 Equipment: Faction rewards use existing equipment tables
- v0.10-v0.12: Faction encounters integrated with procedural generation

---

## IV. Implementation Structure

### Sub-Specification Breakdown

v0.33 is divided into 4 focused sub-specifications for manageable implementation:

**v0.33.1: Database Schema & Faction Definitions** (8-12 hours)

- Factions table (5 major factions)
- Characters_FactionReputations table
- Faction_Quests table
- Faction_Rewards table
- Complete SQL seeding scripts
- Reputation threshold definitions

**v0.33.2: Reputation Mechanics & World Reactions** (8-12 hours)

- ReputationService implementation
- Reputation gain/loss calculations
- Merchant price modifiers
- Random encounter frequency adjustments
- Faction hostility triggers
- Witness system for action consequences

**v0.33.3: Faction Quests & Rewards** (7-11 hours)

- 25+ faction-specific quests (5 per faction)
- Reputation-gated quest unlocks
- Faction exclusive equipment (15+ items)
- Faction services and benefits
- Integration with Quest System

**v0.33.4: Service Implementation & Testing** (7-10 hours)

- FactionService complete implementation
- Dynamic world reaction system
- Faction encounter generation
- Unit test suite (12+ tests)
- Integration testing scenarios

---

## V. Technical Architecture Overview

### Database Schema Summary

**4 New Tables:**

1. **Factions** - 5 major faction definitions
2. **Characters_FactionReputations** - Per-character reputation tracking
3. **Faction_Quests** - Reputation-gated quest assignments
4. **Faction_Rewards** - Equipment and services locked by reputation

### Service Architecture Summary

**FactionService** (Primary service)

- GetFactionReputation() - Current reputation value
- ModifyReputation() - Gain/lose reputation
- GetReputationTier() - Hated/Hostile/Neutral/Friendly/Allied/Exalted
- CheckFactionHostility() - Determine if faction attacks
- GetAvailableFactionQuests() - Reputation-gated quest list
- GetFactionRewards() - Available equipment/services

**ReputationService** (Calculation engine)

- CalculateReputationChange() - Witness system + action severity
- ApplyPriceModifier() - Merchant discount/markup
- GetEncounterChance() - Random encounter frequency
- CheckActionConsequences() - Predict reputation changes

**FactionEncounterService** (Random encounters)

- GenerateFactionPatrol() - Friendly/hostile faction encounters
- GenerateAmbush() - Hostile faction ambush on low reputation
- GenerateRecruitmentOffer() - High reputation recruitment

### Integration Points

**Existing Systems:**

- v0.14: Quest System (faction quests use existing architecture)
- v0.8: NPC System (faction NPCs have special dialogue)
- v0.3: Equipment (faction rewards are equipment items)
- v0.9: Merchant System (reputation affects prices)
- v0.10-v0.12: Procedural Generation (faction encounters spawn)
- v0.15: Trauma Economy (faction hostility = Stress source)

---

## VI. The Five Major Factions

### 1. Iron-Banes (Anti-Undying Zealots)

**Philosophy:**

"The Undying are corrupted processes that must be purged. Every autonomous construct following 800-year-old protocols is a threat to coherent reality. We follow purification protocols to restore system integrity."

**Goals:**

- Hunt and destroy all Undying
- Develop anti-corruption techniques
- Prevent Runic Blight spread
- Establish coherent zones

**Locations:**

- Trunk/Roots patrols (high presence)
- Muspelheim purification operations
- Jötunheim Undying hunting grounds

**Alignment:**

- **Allies:** Rust-Clans (pragmatic cooperation)
- **Enemies:** God-Sleeper Cultists (kill their "gods")
- **Neutral:** Jötun-Readers (respect knowledge, dislike hoarding)

**Reputation Gains:**

- +10: Kill Undying enemy
- +20: Complete Iron-Bane quest
- +30: Destroy corrupted Jötun-Forged
- +50: Purge major Undying stronghold

**Reputation Losses:**

- -10: Spare Undying when Iron-Bane witnesses
- -30: Attack Iron-Bane patrol
- -50: Kill Iron-Bane NPC

**Rewards (Reputation Gated):**

- **Friendly (+25):** 10% discount from Iron-Bane merchants
- **Allied (+50):** Access to Purification Sigils (anti-Undying consumable)
- **Exalted (+75):** Zealot's Blade (legendary anti-Undying weapon, +2d6 vs. Undying)

---

### 2. God-Sleeper Cultists (Jötun-Forged Worshippers)

**Philosophy:**

"The Jötun-Forged are sleeping gods awaiting the signal to awaken. Their dormancy is sacred. We are the caretakers, the faithful, the ones who will be there when they rise. Do not harm the sleepers."

**Goals:**

- Protect dormant Jötun-Forged
- Establish temples in Jötunheim
- Interpret Jötun logic core broadcasts
- Await the "Great Awakening"

**Locations:**

- Jötunheim (primary territory)
- Near any dormant Jötun-Forged
- Fallen Einherjar Torso-Cave (temple site)

**Alignment:**

- **Allies:** Independents (tolerated)
- **Enemies:** Iron-Banes (kill their "gods")
- **Neutral:** Jötun-Readers (competing interpreters)

**Reputation Gains:**

- +10: Avoid combat with Jötun-Forged when possible
- +20: Complete God-Sleeper quest
- +30: Donate resources to temple
- +50: Protect dormant Jötun from attack

**Reputation Losses:**

- -20: Kill Jötun-Forged or Undying in Jötunheim
- -40: Desecrate God-Sleeper temple
- -60: Kill God-Sleeper Cultist

**Rewards (Reputation Gated):**

- **Friendly (+25):** Cultist's Blessing (+4 to rolls near Jötun corpses)
- **Allied (+50):** Jötun-Touched Robe (armor with psychic resistance)
- **Exalted (+75):** God-Sleeper's Grimoire (access to Jötun Attunement ability)

---

### 3. Jötun-Readers (Pre-Glitch Scholars)

**Philosophy:**

"Knowledge is the only path to understanding the Glitch. Every corrupted log, every fragmented database, every Jötun logic core—these are the keys to comprehension. We preserve, we study, we learn."

**Goals:**

- Recover Pre-Glitch data
- Study corrupted systems
- Archive knowledge
- Understand the Great Silence

**Locations:**

- Alfheim (high-knowledge zones)
- Command Deck Wreckage sites
- Any location with intact terminals

**Alignment:**

- **Allies:** Rust-Clans (trade knowledge for resources)
- **Enemies:** None (knowledge above conflict)
- **Neutral:** Iron-Banes, God-Sleeper Cultists

**Reputation Gains:**

- +10: Recover data-log or Pre-Glitch schematic
- +20: Complete Jötun-Reader quest
- +30: Donate rare knowledge to archives
- +50: Unlock major Pre-Glitch secret

**Reputation Losses:**

- -20: Destroy data without reading
- -30: Hoard knowledge (refuse to share)
- -50: Kill Jötun-Reader NPC

**Rewards (Reputation Gated):**

- **Friendly (+25):** Access to Reader archives (lore unlocks)
- **Allied (+50):** Scholar's Focus (equipment: +2 WITS)
- **Exalted (+75):** Decryption Protocols (ability to read corrupted logs)

---

### 4. Rust-Clans (Midgard Survivors)

**Philosophy:**

"Survival first. No ideology, no worship, no grand theories. We scavenge, we trade, we defend our territory. The world crashed—we're still here. That's what matters."

**Goals:**

- Secure resources
- Establish safe zones
- Trade networks
- Practical survival

**Locations:**

- Midgard (primary territory)
- Scavenger camps
- Trade outposts

**Alignment:**

- **Allies:** Iron-Banes (pragmatic cooperation), Jötun-Readers (trade partners)
- **Enemies:** Raiders and hostile scavengers
- **Neutral:** God-Sleeper Cultists, Independents

**Reputation Gains:**

- +10: Trade resources at Rust-Clan outpost
- +15: Help defend Rust-Clan territory
- +25: Complete Rust-Clan quest
- +40: Establish new trade route

**Reputation Losses:**

- -15: Steal from Rust-Clan merchants
- -30: Attack Rust-Clan patrol
- -50: Kill Rust-Clan NPC

**Rewards (Reputation Gated):**

- **Friendly (+25):** 15% discount at Rust-Clan merchants
- **Allied (+50):** Scavenger's Kit (improvised crafting tools)
- **Exalted (+75):** Clan Sigil (access to hidden Rust-Clan caches)

---

### 5. Independents (Unaffiliated)

**Philosophy:**

"Factions are chains. We walk our own path."

**Goals:**

- Maintain neutrality
- Personal survival
- Freedom from faction politics

**Locations:**

- Anywhere

**Alignment:**

- **Allies:** None (neutral to all)
- **Enemies:** None (unless provoked)
- **Neutral:** All factions

**Reputation Gains:**

- +5: Decline faction recruitment
- +10: Maintain neutrality in faction conflict

**Reputation Losses:**

- -10: Join another faction

**Rewards (Reputation Gated):**

- **Exalted (+100):** Lone Wolf trait (+10% all stats when solo)

---

## VII. Reputation Scale & Thresholds

### Reputation Values

**Scale: -100 (Hated) to +100 (Exalted)**

**Thresholds:**

1. **Hated** (-100 to -76): Kill on sight, ambushes, maximum prices
2. **Hostile** (-75 to -26): Attack if provoked, high prices, no services
3. **Neutral** (-25 to +24): Standard interactions, normal prices
4. **Friendly** (+25 to +49): Discounts, some exclusive quests
5. **Allied** (+50 to +74): Major discounts, most exclusive content
6. **Exalted** (+75 to +100): Best prices, all exclusive rewards

### Reputation Effects

**Merchant Prices:**

- Hated: +50% markup
- Hostile: +25% markup
- Neutral: Normal prices
- Friendly: -10% discount
- Allied: -20% discount
- Exalted: -30% discount

**Random Encounter Frequency:**

- Hated: 3x hostile encounter chance
- Hostile: 2x hostile encounter chance
- Neutral: Normal frequency
- Friendly: 0.5x hostile encounter chance
- Allied: 0.25x hostile encounter chance, friendly patrols appear
- Exalted: No hostile encounters, frequent friendly assistance

**Quest Availability:**

- Neutral: Basic faction quests
- Friendly: Intermediate faction quests
- Allied: Advanced faction quests
- Exalted: Legendary faction questline

---

## VIII. Quality Standards

### v5.0 Setting Compliance

✅ **Technology, Not Mythology:**

- Iron-Banes follow "purification protocols," not religious crusades
- God-Sleeper Cultists are cargo cultists, not literal worshippers
- Jötun-Readers are data archaeologists, not wizards
- Rust-Clans are survivors, not barbarian tribes
- All faction conflicts grounded in post-Glitch survival logic

✅ **800-Year Decay:**

- Factions emerged after the Glitch (not Pre-Glitch organizations)
- All factions adapted to broken reality
- Faction goals reflect survival in corrupted world

✅ **No Supernatural Elements:**

- God-Sleeper "faith" is misinterpretation of Jötun psychic broadcasts
- Iron-Bane "purification" is anti-corruption technique
- Reputation is social network, not mystical karma

### Testing Requirements

✅ **85%+ Coverage Target:**

- Unit tests for reputation calculations
- Integration tests for faction interactions
- Edge case tests (simultaneous faction conflicts)
- Balance validation tests

✅ **Test Categories:**

- Reputation gain/loss calculations
- Merchant price modifiers
- Random encounter frequency
- Faction hostility triggers
- Quest unlock thresholds
- Mutual exclusivity verification

### Logging Requirements

✅ **Serilog Structured Logging:**

- Reputation changes (action, witness, amount)
- Faction encounters (friendly/hostile)
- Quest unlocks
- Merchant transactions
- Hostility triggers
- Player faction status changes

---

## IX. Success Criteria Checklist

### Functional Requirements

- [ ]  5 factions defined with distinct philosophies
- [ ]  Reputation tracking system implemented
- [ ]  Reputation scale -100 to +100 functional
- [ ]  Merchant price modifiers apply correctly
- [ ]  Random encounter frequency adjusts by reputation
- [ ]  Faction hostility triggers at correct thresholds
- [ ]  Faction quests unlock at reputation gates
- [ ]  Faction rewards available at correct thresholds
- [ ]  Witness system tracks action consequences
- [ ]  Multiple factions can be tracked simultaneously
- [ ]  Mutually exclusive paths enforced (Iron-Bane vs. God-Sleeper)

### Quality Gates

- [ ]  85%+ unit test coverage achieved
- [ ]  All services use Serilog structured logging
- [ ]  v5.0 setting compliance verified (cargo cults, not religions)
- [ ]  v2.0 faction identities preserved
- [ ]  ASCII-compliant faction names

### Balance Validation

- [ ]  Reputation gains/losses feel appropriate to actions
- [ ]  Merchant price modifiers are meaningful but not game-breaking
- [ ]  Faction quests provide progression incentives
- [ ]  Exclusive rewards are desirable but not mandatory
- [ ]  Hostile encounters challenging but not impossible
- [ ]  Neutral path remains viable (Independents)

### Integration Verification

- [ ]  Quest System integration functional
- [ ]  NPC System faction dialogue works
- [ ]  Equipment System faction rewards accessible
- [ ]  Merchant System price modifiers apply
- [ ]  Procedural Generation spawns faction encounters

---

## X. Implementation Timeline

**Phase 1: Database Foundation** (v0.33.1) - 8-12 hours

- Create faction tables
- Define 5 major factions
- Seed reputation thresholds
- Test data integrity

**Phase 2: Reputation Mechanics** (v0.33.2) - 8-12 hours

- Implement ReputationService
- Reputation gain/loss calculations
- Witness system
- World reaction modifiers
- Integration testing

**Phase 3: Faction Content** (v0.33.3) - 7-11 hours

- 25+ faction quests
- 15+ faction rewards
- Reputation-gated unlocks
- Integration with Quest System

**Phase 4: Services & Testing** (v0.33.4) - 7-10 hours

- FactionService complete
- FactionEncounterService
- Unit test suite
- End-to-end testing

**Total: 30-45 hours**

---

## XI. Next Steps

Implementation proceeds in order:

1. **Start with v0.33.1** - Database schema provides foundation
2. **Then v0.33.2** - Reputation mechanics build on schema
3. **Then v0.33.3** - Content requires mechanics to be functional
4. **Finally v0.33.4** - Services orchestrate all components

Each sub-specification is **implementation-ready** and can be completed independently once its prerequisites are met.

---

## XII. Related Documents

**Prerequisites:**

- v0.14: Quest System[[1]](https://www.notion.so/v0-14-Quest-System-Narrative-Integration-19fb9dd195df4fd2b9bf4dbddce6448f?pvs=21)
- v0.8: NPC & Dialogue System
- v0.9: Merchants & Economy
- v0.3: Equipment & Loot[[2]](https://www.notion.so/v0-1-Vertical-Slice-Specification-ba9d23ddfa1d44aabbad363e5338a797?pvs=21)
- v0.10-v0.12: Dynamic Room Engine[[3]](https://www.notion.so/v0-10-Dynamic-Room-Engine-Core-f6b2626559d844d78fc65f1fe2c30798?pvs=21)

**Project Context:**

- Master Roadmap[[4]](https://www.notion.so/Master-Roadmap-v0-1-v1-0-4b4f512f0dd7444486e2c59e676378ad?pvs=21)
- AI Session Handoff[[5]](https://www.notion.so/AI-Session-Handoff-Rune-Rust-Development-Status-e19fe6060d6d4e44ae7402d88e3cc6a3?pvs=21)
- MANDATORY Requirements[[6]](https://www.notion.so/MANDATORY-Specification-Requirements-Quality-Standards-a40022443c8b4abfb8cbd4882839447d?pvs=21)

---

**This parent specification provides the framework. Implementation details are in v0.33.1-v0.33.4 child specifications.**