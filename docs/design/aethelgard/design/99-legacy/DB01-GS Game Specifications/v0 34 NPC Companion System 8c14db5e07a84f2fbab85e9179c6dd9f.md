# v0.34: NPC Companion System

Type: Feature
Description: Implements 6-8 recruitable NPC companions with AI-controlled tactical behavior, Command verb for direct control, stance system, companion progression, and System Crash/recovery mechanics. 30-45 hours.
Priority: Must-Have
Status: In Design
Target Version: Alpha
Dependencies: v0.33 (Faction System), v0.20 (Tactical Grid), v0.8 (NPC System), v0.15 (Trauma Economy)
Implementation Difficulty: Hard
Balance Validated: No
Proof-of-Concept Flag: No
Sub-item: v0.34.1: Database Schema & Companion Definitions (v0%2034%201%20Database%20Schema%20&%20Companion%20Definitions%200d9bf4c187e94d2dbebf7d73e81ded97.md), v0.34.3: Recruitment & Progression Systems (v0%2034%203%20Recruitment%20&%20Progression%20Systems%20c234daa5f1074be8b7323d6137cf70b3.md), v0.34.4: Service Implementation & Testing (v0%2034%204%20Service%20Implementation%20&%20Testing%2090fcbf84cb89413bbf6b49fc459e4cac.md), v0.34.2: Companion AI & Tactical Behavior (v0%2034%202%20Companion%20AI%20&%20Tactical%20Behavior%20b844723698524ce4939ece492a91bb75.md)
Template Validated: No
Voice Validated: No

**Document ID:** RR-SPEC-v0.34-COMPANIONS

**Status:** Design Complete — Ready for Implementation

**Timeline:** 30-45 hours total across 4 sub-specifications

**Prerequisites:** v0.33 (Faction System), v0.20 (Tactical Grid), v0.8 (NPC System), v0.15 (Trauma Economy)

---

## I. Executive Summary

### The Fantasy

"You're not alone. The Echo-Caller hums beside you, their spirit-woven form translucent in the flickering light. Kára, the Rust-Witch you recruited from the Iron-Bane enclave, checks her rifle. And your custom-built Scavenger Drone hovers overhead, scanning for threats. This is your party—your chosen interface with the broken world. Each companion brings unique abilities, tactical options, and personal stories. Their deaths are not cheap—when a companion falls, you feel the psychic feedback. They matter."

**The Companion System** transforms Rune & Rust from a solo experience into a **tactical squad-based game** where you recruit, equip, command, and develop AI-controlled party members. Companions are not disposable summons—they are persistent characters with progression, personality, and mechanical depth.

### v2.0 Canonical Foundation

This specification migrates and expands v2.0 Companion System specifications:

- **Feature Specification: The Companion System**[[1]](https://www.notion.so/Feature-Specification-The-Companion-System-2a355eb312da8006b181d82f4267a42d?pvs=21)
- **Revision: Companion System Concept Design**[[2]](https://www.notion.so/Revision-Companion-System-Concept-Design-2a355eb312da801eb056cec714b8769b?pvs=21)
- **Revision: Party Dynamics & Multiplayer Design**[[3]](https://www.notion.so/Revision-Party-Dynamics-Multiplayer-Design-2a355eb312da8007ad93c387a070fc73?pvs=21) (Echo Troupe system)

**v2.0 Core Concepts Preserved:**

- Companions as "corrupted processes" with HP, stats, and Corruption
- Command verb for tactical control
- Companion "system crash" as temporary defeat
- Specialized companion types (living/mechanical/summoned)
- Loyalty and Corruption mechanics
- Integration with Trauma Economy (psychic feedback on companion death)

**v5.0 Adaptations:**

- 6-8 recruitable NPCs instead of specialization-bound pets
- Simplified from v2.0's Echo Troupe (3 companions) to manageable party size
- Faction integration for recruitment
- Party composition strategies
- Companion personal quests

### Core Mechanics

- **6-8 Recruitable Companions** with distinct personalities and builds
- **AI-Controlled Tactical Behavior** using existing combat grid
- **Companion Progression** — Level up, equip items, learn abilities
- **Companion HP & System Crash** — Defeat = psychic feedback, recovery required
- **Command Verb** — Direct tactical orders (v2.0 canonical)
- **Stance System** — Aggressive/Defensive/Passive AI behavior
- **Faction-Locked Recruitment** — Some companions require faction reputation
- **Personal Questlines** — Each companion has narrative arc
- **Trauma Economy Integration** — Companion death = Psychic Stress

---

## II. The Rule: What's In vs. Out

### ✅ In Scope (v0.34 Complete)

**Core Systems:**

- Complete companion recruitment system (6-8 NPCs)
- AI-controlled companion combat behavior
- Command verb for direct tactical control
- Stance system (Aggressive/Defensive/Passive)
- Companion progression (level, equipment, abilities)
- Companion HP and System Crash mechanics
- Psychic Stress feedback on companion defeat
- Party composition management
- Companion personal quests (1-2 per companion)

**6-8 Recruitable Companions:**

- Varied archetypes (Warrior/Adept/Mystic mix)
- Faction affiliations for recruitment gating
- Distinct personalities and backstories
- Unique ability loadouts
- Personal quest integration

**Technical Deliverables:**

- Complete database schema (4 new tables)
- Service implementation (CompanionService, CompanionAIService)
- Unit test suite (15+ tests, ~85% coverage)
- Integration with Tactical Grid, Faction System, Quest System
- v5.0 setting compliance
- Serilog structured logging throughout
- ASCII-only character names

### ❌ Explicitly Out of Scope

- Specialization-bound pets (Beast-Binder, Scrap-Tinker) (defer to v2.0+)
- Summoned temporary companions (God-Sleeper constructs) (defer to v2.0+)
- Companion romance or deep relationship systems (defer to v0.40+)
- Player-vs-player multiplayer (single-player focus for v1.0)
- Companion equipment crafting (use existing equipment system)
- Advanced Companion Corruption/Loyalty mechanics (simplified for v0.34)
- Companion permadeath (all companions recoverable)
- Party size beyond 4 total (1 player + 3 companions max)
- Companion voice acting or portraits (text-only)

---

## III. v2.0 → v5.0 Migration Summary

### Preserved from v2.0

**Core Companion Mechanics:**

- Companion as "corrupted process" with simplified character sheet
- HP, Defense, Soak, Corruption tracking
- Command verb for direct control
- Stance system (Aggressive/Defensive/Passive)
- System Crash on 0 HP (not permanent death)
- Psychic Stress feedback on companion defeat
- Scaling with player power

**v2.0 Command Syntax (Preserved):**

```
command [companion_name] [ability] [target]
stance [companion_name] aggressive|defensive|passive
```

**v2.0 Companion Types (Adapted):**

- v2.0: "Permanent" (Beast-Binder pet), "Echo" (cached NPCs), "Summoned" (temporary)
- v0.34: Focus on "Recruitable NPCs" (Echo Troupe concept adapted)
- Specialized pets deferred to v2.0+ (beyond v1.0 scope)

### Updated for v5.0

**Simplifications:**

- **Party Size:** 4 total (1 player + 3 companions) vs. v2.0's flexible size
- **Recruitment Model:** Faction-gated NPC recruitment vs. v2.0's "caching Echo data"
- **Companion Depth:** Moderate progression vs. v2.0's deep Corruption/Loyalty systems
- **Scope:** 6-8 pre-designed NPCs vs. v2.0's expansive companion catalog

**Integration with v5.0 Systems:**

- v0.33 Faction System: Companions have faction requirements
- v0.20 Tactical Grid: AI uses positioning, flanking, cover
- v0.14 Quest System: Companion personal quests
- v0.15 Trauma Economy: Psychic Stress on companion defeat
- v0.21 Stance System: Companions can use combat stances

**Voice Layer (Layer 2 Clinical):**

- ❌ v2.0: "Echo cached from data-remnant"
- ✅ v5.0: "Recruited NPC joins party"
- ❌ v2.0: "Companion system crashes"
- ✅ v5.0: "Companion incapacitated, requires recovery"

---

## IV. Implementation Structure

### Sub-Specification Breakdown

v0.34 is divided into 4 focused sub-specifications:

**v0.34.1: Database Schema & Companion Definitions** (8-12 hours)

- Companions table (6-8 recruitable NPCs)
- Characters_Companions table (party membership)
- Companion_Progression table (levels, equipment)
- Companion_Quests table (personal storylines)
- Complete SQL seeding scripts

**v0.34.2: Companion AI & Tactical Behavior** (8-12 hours)

- CompanionAIService implementation
- Stance-based behavior (Aggressive/Defensive/Passive)
- Tactical grid integration (positioning, flanking)
- Target selection algorithms
- Ability usage AI

**v0.34.3: Recruitment & Progression Systems** (7-10 hours)

- Recruitment mechanics (faction requirements)
- Companion leveling and equipment
- Personal quest integration
- Party composition management

**v0.34.4: Service Implementation & Testing** (7-11 hours)

- CompanionService complete implementation
- Command verb integration
- System Crash and recovery mechanics
- Unit test suite (15+ tests)
- Integration testing

---

## V. Technical Architecture Overview

### Database Schema Summary

**4 New Tables:**

1. **Companions** - 6-8 NPC definitions with stats, abilities, faction requirements
2. **Characters_Companions** - Per-character party membership and companion state
3. **Companion_Progression** - Level, equipment, ability unlocks
4. **Companion_Quests** - Personal questlines for each companion

### Service Architecture Summary

**CompanionService** (Primary service)

- RecruitCompanion() - Add companion to party
- DismissCompanion() - Remove from party
- GetPartyCompanions() - List active companions
- ProcessCompanionTurn() - Execute companion actions in combat
- HandleSystemCrash() - Companion defeat and recovery
- CommandCompanion() - Direct tactical control

**CompanionAIService** (Behavior engine)

- SelectAction() - Choose ability based on stance and situation
- SelectTarget() - Pick optimal target
- EvaluateThreat() - Threat assessment for Defensive stance
- FindOptimalPosition() - Movement for tactical advantage

**CompanionProgressionService** (Leveling and equipment)

- LevelUpCompanion() - Apply level-based stat increases
- EquipItem() - Manage companion equipment
- UnlockAbility() - Grant new abilities at level thresholds

### Integration Points

**Existing Systems:**

- v0.33: Faction System (recruitment requirements)
- v0.20: Tactical Grid (AI positioning, combat)
- v0.14: Quest System (personal quests)
- v0.15: Trauma Economy (Psychic Stress on defeat)
- v0.21: Stance System (companion stance behavior)
- v0.3: Equipment System (companion gear)
- v0.2: Progression System (companion leveling)

---

## VI. The Six Recruitable Companions

### 1. Kára Ironbreaker (Warrior - Iron-Bane)

**Recruitment:**

- **Location:** Iron-Bane enclave (Trunk)
- **Requirement:** Iron-Bane reputation Friendly (+25)
- **Quest:** "Purge the Corrupted Forge"

**Background:**

"Former security protocol enforcer turned Undying hunter. Kára lost her entire squad to a Draugr Juggernaut ambush. She's methodical, disciplined, and carries survivor's guilt. Sees every Undying destroyed as atonement."

**Build:**

- **Archetype:** Warrior (MIGHT + Stamina)
- **Role:** Tank / Anti-Undying specialist
- **Starting Abilities:** Shield Bash, Taunt, Purification Strike (+2d6 vs. Undying)
- **Personality:** Stoic, duty-driven, distrusts magic

**Personal Quest:** "The Last Protocol" - Recover her squad's final mission data

---

### 2. Finnr the Rust-Sage (Mystic - Jotun-Reader)

**Recruitment:**

- **Location:** Alfheim archives
- **Requirement:** Jötun-Reader reputation Friendly (+25)
- **Quest:** "Decode the Corrupted Schematic"

**Background:**

"Scholar obsessed with Pre-Glitch knowledge. Finnr believes understanding the Glitch is the only path forward. Socially awkward, brilliant, and dangerously curious about forbidden data."

**Build:**

- **Archetype:** Mystic (WILL + Aether Pool)
- **Role:** Support / Knowledge specialist
- **Starting Abilities:** Aetheric Bolt, Data Analysis (reveal enemy weaknesses), Runic Shield
- **Personality:** Curious, verbose, oblivious to danger

**Personal Quest:** "The Forlorn Archive" - Access restricted Pre-Glitch database

---

### 3. Bjorn Scrap-Hand (Adept - Rust-Clan)

**Recruitment:**

- **Location:** Midgard trade outpost
- **Requirement:** Rust-Clan reputation Neutral (0)
- **Quest:** "Salvage the Supply Cache"

**Background:**

"Pragmatic scavenger who's survived by being useful. Bjorn can fix anything, build weapons from scrap, and knows where to find resources. No ideology—just survival."

**Build:**

- **Archetype:** Adept (WITS + Stamina)
- **Role:** Utility / Crafting support
- **Starting Abilities:** Improvised Repair, Scrap Grenade, Resourceful (find extra loot)
- **Personality:** Practical, cynical, surprisingly loyal

**Personal Quest:** "The Old Workshop" - Reclaim his pre-Glitch family workshop

---

### 4. Valdis the Forlorn-Touched (Mystic - Independent)

**Recruitment:**

- **Location:** Niflheim frozen ruins
- **Requirement:** None (found during exploration)
- **Quest:** "Silence the Echoes" (rescue quest)

**Background:**

"Seidkona who communes with Forlorn too deeply. Valdis hears voices, sees ghosts, and balances on the edge of Breaking. Powerful but unstable—a high-risk, high-reward companion."

**Build:**

- **Archetype:** Mystic (WILL + Aether Pool)
- **Role:** Glass cannon / Psychic damage
- **Starting Abilities:** Spirit Bolt, Forlorn Whisper (fear effect), Fragile Mind (high damage, low HP)
- **Personality:** Haunted, prophetic, unpredictable

**Personal Quest:** "Breaking the Voices" - Confront the Forlorn controlling her

---

### 5. Runa Shield-Sister (Warrior - Independent)

**Recruitment:**

- **Location:** Jotunheim assembly yards
- **Requirement:** Complete "Defend the Caravan" event
- **Quest:** Dynamic encounter rescue

**Background:**

"Mercenary guard who believes in protecting those weaker than herself. Runa has no faction loyalties—only a personal code. Straightforward, protective, and suspicious of authority."

**Build:**

- **Archetype:** Warrior (MIGHT + Stamina)
- **Role:** Tank / Bodyguard
- **Starting Abilities:** Defensive Stance, Interpose (protect ally), Shield Wall
- **Personality:** Honorable, protective, independent

**Personal Quest:** "The Broken Oath" - Track down the employer who betrayed her

---

### 6. Einar the God-Touched (Warrior - God-Sleeper)

**Recruitment:**

- **Location:** Jötunheim temple (Fallen Einherjar Torso-Cave)
- **Requirement:** God-Sleeper Cultist reputation Friendly (+25)
- **Quest:** "Prove Your Faith"

**Background:**

"Cargo cultist who believes Jötun-Forged are sleeping gods. Einar is zealous, powerful near Jötun corpses, and sees your journey as divine will. Dangerous fanaticism mixed with genuine combat skill."

**Build:**

- **Archetype:** Warrior (MIGHT + Stamina)
- **Role:** DPS / Conditional powerhouse
- **Starting Abilities:** Berserker Rage, Jötun Attunement (+4 near corpses), Reckless Strike
- **Personality:** Zealous, charismatic, sees omens everywhere

**Personal Quest:** "Awaken the Sleeper" - Attempt to reactivate dormant Jötun

---

## VII. Companion Mechanics

### A. Recruitment

**Recruitment Triggers:**

- Faction reputation thresholds
- Specific quest completion
- Dynamic world events
- Exploration discovery

**Party Size Limit:** 4 total (1 player + 3 companions max)

---

### B. Companion Progression

**Leveling:**

- Companions gain Legend (XP) alongside player
- Level thresholds same as player
- Stat increases per level (MIGHT/FINESSE/STURDINESS/WITS/WILL)
- New ability unlocks at levels 3, 5, 7

**Equipment:**

- Companions can equip weapons, armor, accessories
- Use existing Equipment System (v0.3)
- Quality tier restrictions same as player

---

### C. Combat Behavior & AI

**Stance System (v2.0 Canonical):**

**Aggressive:**

- Prioritizes damage output
- Attacks current player target
- Uses offensive abilities
- May overextend for kills

**Defensive:**

- Stays near player
- Protects low-HP allies
- Uses defensive/support abilities
- Conservative positioning

**Passive:**

- Takes no action unless commanded
- Useful for keeping companion safe
- Manual control only

---

### D. Command Verb (v2.0 Canonical)

**Direct Control:**

```
command [companion_name] [ability] [target]
```

**Examples:**

```
command Kara shield_bash warden
command Finnr aetheric_bolt cultist
command Bjorn repair Kara
```

**Stance Control:**

```
stance [companion_name] aggressive|defensive|passive
```

**Examples:**

```
stance Runa defensive
stance Einar aggressive
stance Valdis passive
```

---

### E. System Crash & Recovery (v2.0 Adapted)

**When Companion Reaches 0 HP:**

1. **System Crash:** Companion is removed from combat
2. **Psychic Feedback:** Player suffers +10 Psychic Stress (Trauma Economy)
3. **Recovery Required:** Companion cannot return to combat this encounter

**Recovery Methods:**

- **After Combat:** Automatic recovery to 50% HP
- **At Sanctuary:** Full HP recovery
- **Field Medicine:** Bone-Setter abilities can revive mid-dungeon

**No Permadeath:** All companions recoverable (v0.34 simplification)

---

## VIII. Quality Standards

### v5.0 Setting Compliance

✅ **Technology & Magic Distinction:**

- Companions are NPCs (living humans), not summoned entities
- Mystic companions use rune-based Aetheric weaving
- "System Crash" framed as incapacitation, not data corruption
- Trauma Economy integration (Psychic Stress on companion defeat)

✅ **Layer 2 Voice:**

- "Companion incapacitated" not "companion crashed"
- "Recruit NPC" not "cache Echo data"
- "Party composition" not "coherent signal strength"

✅ **ASCII Compliance:**

- Kára → Kara
- Björn → Bjorn
- Jötun → Jotun (in code/database)
- Display layer can show "Kára" but data layer stores "Kara"

### Testing Requirements

✅ **85%+ Coverage Target:**

- Companion AI behavior tests
- Command verb parsing
- Stance switching
- Recruitment requirement checks
- System Crash and recovery
- Party size limits
- Equipment management

### Logging Requirements

✅ **Serilog Structured Logging:**

- Companion actions (ability used, target)
- AI decision-making (why companion chose action)
- System Crash events
- Recruitment and dismissal
- Quest completion

---

## IX. Success Criteria Checklist

### Functional Requirements

- [ ]  6-8 companions recruitable
- [ ]  Faction requirements enforced
- [ ]  AI behavior functional (Aggressive/Defensive/Passive)
- [ ]  Command verb parses correctly
- [ ]  Companions level up with player
- [ ]  Equipment system integrated
- [ ]  System Crash applies Psychic Stress
- [ ]  Personal quests trigger correctly
- [ ]  Party size limited to 4 total
- [ ]  Tactical grid AI uses positioning

### Quality Gates

- [ ]  85%+ unit test coverage
- [ ]  Serilog logging implemented
- [ ]  v5.0 setting compliance verified
- [ ]  v2.0 command syntax preserved
- [ ]  ASCII-only companion names in database

### Balance Validation

- [ ]  Companion power scales with player
- [ ]  AI doesn't make obviously bad decisions
- [ ]  System Crash Stress cost meaningful
- [ ]  Personal quests provide progression rewards
- [ ]  No dominant companion (all viable)

### Integration Verification

- [ ]  Faction System integration functional
- [ ]  Tactical Grid AI uses cover, flanking
- [ ]  Quest System personal quests work
- [ ]  Trauma Economy Stress applied
- [ ]  Equipment System companion gear works

---

## X. Implementation Timeline

**Phase 1: Database & Definitions** (v0.34.1) - 8-12 hours

- Define 6-8 companions
- Create database tables
- Seed companion data

**Phase 2: AI & Tactical Behavior** (v0.34.2) - 8-12 hours

- Implement CompanionAIService
- Stance-based behavior
- Tactical grid integration

**Phase 3: Recruitment & Progression** (v0.34.3) - 7-10 hours

- Recruitment system
- Leveling and equipment
- Personal quests

**Phase 4: Services & Testing** (v0.34.4) - 7-11 hours

- Complete CompanionService
- Command verb integration
- Testing and validation

**Total: 30-45 hours**

---

## XI. Next Steps

Implementation proceeds in order:

1. **Start with v0.34.1** - Database schema and companion definitions
2. **Then v0.34.2** - AI behavior and tactical integration
3. **Then v0.34.3** - Recruitment, progression, quests
4. **Finally v0.34.4** - Services orchestration and testing

Each sub-specification is **implementation-ready** and follows v2.0 canonical mechanics.

---

## XII. Related Documents

**v2.0 Canonical Sources:**

- Feature Specification: The Companion System[[1]](https://www.notion.so/Feature-Specification-The-Companion-System-2a355eb312da8006b181d82f4267a42d?pvs=21)
- Revision: Companion System Concept Design[[2]](https://www.notion.so/Revision-Companion-System-Concept-Design-2a355eb312da801eb056cec714b8769b?pvs=21)
- Revision: Party Dynamics & Multiplayer Design[[3]](https://www.notion.so/Revision-Party-Dynamics-Multiplayer-Design-2a355eb312da8007ad93c387a070fc73?pvs=21)

**Prerequisites:**

- v0.33: Faction System & Reputation[[4]](v0%2033%20Faction%20System%20&%20Reputation%20161115b505034a2fa3ad8288e2513b36.md)
- v0.20: Tactical Combat Grid System[[5]](https://www.notion.so/v0-20-Tactical-Combat-Grid-System-987463086e1547219f70810a6e99fe01?pvs=21)
- v0.14: Quest System[[6]](https://www.notion.so/v0-14-Quest-System-Narrative-Integration-19fb9dd195df4fd2b9bf4dbddce6448f?pvs=21)
- v0.15: Trauma Economy[[7]](https://www.notion.so/v0-15-Trauma-Economy-Breaking-Points-Consequences-a1e59f904171485284d6754193af333b?pvs=21)
- v0.8: NPCs & Dialogue System[[8]](https://www.notion.so/v0-8-NPCs-Dialogue-System-67b6f009a8374a52afa8dd0e6bfa76d3?pvs=21)

**Project Context:**

- Master Roadmap[[9]](https://www.notion.so/Master-Roadmap-v0-1-v1-0-4b4f512f0dd7444486e2c59e676378ad?pvs=21)
- MANDATORY Requirements[[10]](https://www.notion.so/MANDATORY-Specification-Requirements-Quality-Standards-a40022443c8b4abfb8cbd4882839447d?pvs=21)
- AI Session Handoff[[11]](https://www.notion.so/AI-Session-Handoff-Rune-Rust-Development-Status-e19fe6060d6d4e44ae7402d88e3cc6a3?pvs=21)

---

**This parent specification provides the framework. Implementation details are in v0.34.1-v0.34.4 child specifications.**