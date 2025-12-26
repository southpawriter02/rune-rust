# v0.35: Territory Control & Dynamic World

Type: Feature
Description: Implements territory control and dynamic world systems with sector ownership mechanics, faction wars, world events, and player influence. Transforms the world into a living conflict where factions compete for control. 30-40 hours.
Priority: Must-Have
Status: In Design
Target Version: Alpha
Dependencies: v0.33 (Faction System), v0.34 (NPC Companions), v0.13 (World State Persistence), v0.14 (Quest System)
Implementation Difficulty: Hard
Balance Validated: No
Proof-of-Concept Flag: No
Sub-item: v0.35.1: Database Schema & Territory Definitions (v0%2035%201%20Database%20Schema%20&%20Territory%20Definitions%20602a9b6b963c4f08b40f665814bb1a40.md), v0.35.3: Dynamic World Events & Consequences (v0%2035%203%20Dynamic%20World%20Events%20&%20Consequences%202f7b9fda9f1d4994bb7dd8c198987cb2.md), v0.35.2: Territory Mechanics & Faction Wars (v0%2035%202%20Territory%20Mechanics%20&%20Faction%20Wars%207d490ccdbf654cc9a04e15e024b026fb.md), v0.35.4: Player Influence & Service Integration (v0%2035%204%20Player%20Influence%20&%20Service%20Integration%2014c1b9b160e84b3687612489fbb59f78.md)
Template Validated: No
Voice Validated: No

**Document ID:** RR-SPEC-v0.35-TERRITORY-CONTROL

**Status:** Design Complete — Ready for Implementation

**Timeline:** 30-40 hours (4 specifications @ 7-11 hours each)

**Prerequisites:** v0.33 (Faction System), v0.34 (NPC Companions), v0.13 (World State Persistence), v0.14 (Quest System)

**Master Roadmap Reference:** v0.35 is Phase 11's final component[[1]](https://www.notion.so/Master-Roadmap-v0-1-v1-0-4b4f512f0dd7444486e2c59e676378ad?pvs=21)

---

## I. Executive Summary

### The Deliverable

This specification defines **Territory Control and Dynamic World Systems** that make factions active participants in Aethelgard:

- **Sector ownership mechanics** — Factions can control territories
- **Dynamic world events** — Faction wars, incursions, and power shifts
- **World consequence system** — Territory control affects generation, quests, NPCs
- **Player influence mechanics** — Actions shift the balance of power

v0.35 transforms the world from static reputation meters into a living conflict where factions compete for control and player actions have lasting territorial consequences.

---

## II. The Rule: What's In vs. Out

### ✅ In Scope (v0.35)

**v0.35.1: Database Schema & Territory Definitions (7-10 hours)**

- Sector ownership tracking (Factions table extension)
- Territory state definitions (contested, stable, war zone)
- Faction strength metrics (population, resources, military)
- World event catalog (faction wars, incursions, diplomatic shifts)

**v0.35.2: Territory Mechanics & Faction Wars (8-11 hours)**

- Sector ownership calculation engine
- Faction war triggering and resolution
- Territory flip mechanics
- Dynamic encounter frequency based on control

**v0.35.3: Dynamic World Events & Consequences (7-10 hours)**

- Random event generation (faction incursions, supply raids)
- Quest generation from territory state
- NPC behavior modification based on control
- Merchant stock changes

**v0.35.4: Player Influence & Service Integration (8-9 hours)**

- TerritoryService orchestration
- Player action impact calculation
- Reputation effect on territory control
- Complete unit test suite

### ❌ Explicitly Out of Scope

- Direct territory conquest commands (defer to v2.0+)
- Advanced diplomacy system (defer to v2.0+)
- Siege mechanics (defer to v2.0+)
- Faction alliance system (defer to v2.0+)
- Multiple simultaneous faction wars (v0.35 supports 1 war at a time)
- Economic warfare (trade blockades, sanctions) (defer to v2.0+)
- Permanent territory destruction (reversible only)

---

## III. Core Mechanics Overview

### Territory Ownership

**Sector Control States:**

- **Stable Control:** Single faction owns sector (60%+ influence)
- **Contested:** Multiple factions compete (40-60% influence each)
- **War Zone:** Active faction war in progress
- **Independent:** No faction has majority control (< 40% any faction)
- **Ruined:** Sector temporarily uninhabitable (recent catastrophe)

**Faction Influence Sources:**

```
Influence = base_strength + reputation_modifier + quest_completions + war_victories
```

### Faction Wars

**War Triggering Conditions:**

1. Two factions reach 45%+ influence in same sector (contested state persists 10+ in-game days)
2. Player completes faction quest flagged as "Territorial Expansion"
3. Random event: "Faction Offensive" (5% chance per contested sector per day)

**War Resolution:**

- Wars last 5-15 in-game days
- Player actions (killing enemies, completing quests) shift war balance
- Winner gains +20% influence, loser loses -20% influence
- Collateral damage: sector hazard density increases +25%

### World Consequences

**Territory Control Effects:**

**Iron-Bane Controlled Sectors:**

- +50% Undying patrol frequency
- Anti-Undying equipment available at merchants
- "Purge" quests spawn (hunt specific Undying)
- NPCs cautious, distrusts outsiders

**Jötun-Reader Controlled Sectors:**

- +30% artifact spawn rate in ruins
- Scholar NPCs offer knowledge rewards
- "Excavation" quests spawn (retrieve data-logs)
- Environmental storytelling emphasizes pre-Glitch history

**God-Sleeper Controlled Sectors:**

- +40% Jötun-Forged encounter rate
- Cultist NPCs proselytize
- "Awakening" quests spawn (activate dormant constructs)
- Reality distortion hazards increase

**Rust-Clan Controlled Sectors:**

- +50% salvage material spawn rate
- Scavenger merchants offer better prices
- "Supply Run" quests spawn (escort caravans)
- Improvised fortifications reduce enemy ambush chance

**Independent Controlled Sectors:**

- Neutral ground, no faction hostility
- Mixed NPC population
- "Mediation" quests spawn (resolve disputes)
- Random encounter diversity increases

---

## IV. System Architecture

### Database Schema (v0.35.1)

**New Tables:**

**1. Faction_Territory_Control**

```sql
CREATE TABLE Faction_Territory_Control (
    territory_control_id INTEGER PRIMARY KEY,
    world_id INTEGER NOT NULL,
    sector_id INTEGER NOT NULL,
    faction_name TEXT NOT NULL,
    influence_value REAL NOT NULL DEFAULT 0.0, -- 0.0 to 100.0
    control_state TEXT NOT NULL CHECK(control_state IN ('Stable', 'Contested', 'War', 'Independent', 'Ruined')),
    last_updated TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    
    FOREIGN KEY (world_id) REFERENCES Worlds(world_id),
    FOREIGN KEY (sector_id) REFERENCES Sectors(sector_id),
    FOREIGN KEY (faction_name) REFERENCES Factions(faction_name),
    UNIQUE(world_id, sector_id, faction_name)
);
```

**2. Faction_Wars**

```sql
CREATE TABLE Faction_Wars (
    war_id INTEGER PRIMARY KEY,
    world_id INTEGER NOT NULL,
    sector_id INTEGER NOT NULL,
    faction_a TEXT NOT NULL,
    faction_b TEXT NOT NULL,
    war_start_date TIMESTAMP NOT NULL,
    war_end_date TIMESTAMP,
    war_balance REAL DEFAULT 0.0, -- -100 to +100 (+ favors faction_a)
    is_active BOOLEAN NOT NULL DEFAULT 1,
    victor TEXT, -- NULL if ongoing, faction_name if resolved
    
    FOREIGN KEY (world_id) REFERENCES Worlds(world_id),
    FOREIGN KEY (sector_id) REFERENCES Sectors(sector_id),
    FOREIGN KEY (faction_a) REFERENCES Factions(faction_name),
    FOREIGN KEY (faction_b) REFERENCES Factions(faction_name)
);
```

**3. World_Events**

```sql
CREATE TABLE World_Events (
    event_id INTEGER PRIMARY KEY,
    world_id INTEGER NOT NULL,
    sector_id INTEGER,
    event_type TEXT NOT NULL, -- 'Faction_War', 'Incursion', 'Supply_Raid', 'Diplomatic_Shift', 'Catastrophe'
    affected_faction TEXT,
    event_description TEXT NOT NULL,
    event_start_date TIMESTAMP NOT NULL,
    event_end_date TIMESTAMP,
    is_resolved BOOLEAN NOT NULL DEFAULT 0,
    player_influenced BOOLEAN NOT NULL DEFAULT 0,
    
    FOREIGN KEY (world_id) REFERENCES Worlds(world_id),
    FOREIGN KEY (sector_id) REFERENCES Sectors(sector_id),
    FOREIGN KEY (affected_faction) REFERENCES Factions(faction_name)
);
```

**4. Player_Territorial_Actions**

```sql
CREATE TABLE Player_Territorial_Actions (
    action_id INTEGER PRIMARY KEY,
    character_id INTEGER NOT NULL,
    sector_id INTEGER NOT NULL,
    action_type TEXT NOT NULL, -- 'Kill_Enemy', 'Complete_Quest', 'Defend_Territory', 'Sabotage'
    affected_faction TEXT NOT NULL,
    influence_delta REAL NOT NULL, -- -10.0 to +10.0
    action_timestamp TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    
    FOREIGN KEY (character_id) REFERENCES Characters(character_id),
    FOREIGN KEY (sector_id) REFERENCES Sectors(sector_id),
    FOREIGN KEY (affected_faction) REFERENCES Factions(faction_name)
);
```

### Service Architecture

**TerritoryService** (v0.35.4) — Primary orchestration

- CalculateSectorControl() — Determine current control state
- ShiftInfluence() — Adjust faction influence from player actions
- GetDominantFaction() — Query which faction controls sector

**FactionWarService** (v0.35.2) — War mechanics

- CheckWarTrigger() — Determine if contested state should escalate
- AdvanceWar() — Process daily war progression
- ResolveWar() — Determine victor and apply consequences

**WorldEventService** (v0.35.3) — Dynamic events

- GenerateRandomEvent() — Roll for events based on world state
- ProcessEvent() — Execute event consequences
- GenerateTerritorialQuest() — Create quests from territory state

---

## V. Implementation Phases

### Phase 1: v0.35.1 — Database Schema & Territory Definitions

**Timeline:** 7-10 hours

**Deliverables:**

- 4 new tables (Faction_Territory_Control, Faction_Wars, World_Events, Player_Territorial_Actions)
- Indexes for performance
- Seed data for initial territory states
- SQL migration script

**Success Criteria:**

- ✅ All tables created with constraints
- ✅ Seed data populates 5 sectors per faction
- ✅ Indexes speed up territory lookups
- ✅ Foreign key relationships enforced

---

### Phase 2: v0.35.2 — Territory Mechanics & Faction Wars

**Timeline:** 8-11 hours

**Deliverables:**

- TerritoryControlService
- FactionWarService
- War triggering logic
- Influence calculation engine
- Unit tests (10+ tests, 85%+ coverage)

**Success Criteria:**

- ✅ Sectors correctly identify control state
- ✅ Wars trigger when conditions met
- ✅ War balance shifts from player actions
- ✅ Wars resolve with victor determination

---

### Phase 3: v0.35.3 — Dynamic World Events & Consequences

**Timeline:** 7-10 hours

**Deliverables:**

- WorldEventService
- Random event generation system
- Territorial quest templates (15+ quests)
- NPC behavior modification system
- Merchant stock modifiers

**Success Criteria:**

- ✅ Random events spawn based on territory state
- ✅ Quests dynamically generated for contested sectors
- ✅ NPCs react to faction control
- ✅ Merchants adjust inventory based on controlling faction

---

### Phase 4: v0.35.4 — Player Influence & Service Integration

**Timeline:** 8-9 hours

**Deliverables:**

- TerritoryService (orchestration layer)
- Player action tracking and influence calculation
- Integration with v0.33 (FactionService)
- Integration with v0.14 (QuestService)
- Complete unit test suite (15+ tests)
- Serilog logging throughout

**Success Criteria:**

- ✅ Player actions shift faction influence
- ✅ Reputation affects territorial calculations
- ✅ Quest completions update territory control
- ✅ All services integrate cleanly
- ✅ 85%+ test coverage

---

## VI. Integration with Existing Systems

### v0.33 Faction System Integration

```csharp
// Territory control affects reputation gains
var reputationMultiplier = GetTerritoryMultiplier(sectorId, factionName);
var reputationGain = baseGain * reputationMultiplier;

// Player with high reputation influences territory more
var influencePower = CalculateInfluencePower(characterId, factionName);
```

### v0.34 NPC Companion Integration

```csharp
// Companions react to faction-controlled territory
if (companion.FactionAffiliation == dominantFaction)
{
    // Companion more effective in friendly territory
    companion.ApplyBuff("Home_Territory", +10, "morale");
}
else if (IsHostileFaction(companion.FactionAffiliation, dominantFaction))
{
    // Companion stressed in hostile territory
    companion.ApplyPsychicStress(5, "Hostile Territory");
}
```

### v0.14 Quest System Integration

```csharp
// Territory state affects quest generation
var territoryState = _territoryService.GetSectorControlState(sectorId);

if (territoryState == "Contested")
{
    // Generate "Support Faction" quests
    GenerateTerritorialExpansionQuests(sectorId);
}
else if (territoryState == "War")
{
    // Generate "War Effort" quests
    GenerateWarQuests(sectorId);
}
```

### v0.10-v0.12 Procedural Generation Integration

```csharp
// Territory control modifies generation parameters
var controllingFaction = _territoryService.GetDominantFaction(sectorId);

var generationParams = new SectorGenerationParams
{
    EnemyFactionFilter = controllingFaction,
    HazardDensity = GetHazardModifier(controllingFaction),
    LootTableModifier = GetLootModifier(controllingFaction)
};
```

---

## VII. World Consequence Examples

### Example 1: Iron-Bane Victory in Trunk Sector

**Before:**

- Trunk: Independent (Rust-Clan 35%, Iron-Bane 30%, Jötun-Reader 20%)
- Mixed enemy types, neutral NPCs

**War:** Iron-Bane vs. Rust-Clan

- Player completes 5 "Purge Undying" quests for Iron-Bane
- War balance shifts +40 toward Iron-Bane
- Iron-Bane victory after 8 in-game days

**After:**

- Trunk: Stable Control (Iron-Bane 55%, Rust-Clan 15%, Jötun-Reader 20%)
- Undying patrol frequency +50%
- Iron-Bane NPCs appear (guards, priests)
- Merchants stock anti-Undying gear
- New quest chain: "The Last Protocol" (Iron-Bane campaign)

### Example 2: God-Sleeper Incursion in Jötunheim

**Event:** "Awakening Ritual"

- God-Sleeper cultists attempt to reactivate dormant Jötun-Forged
- 7-day event window
- If player does not intervene: God-Sleepers gain +15% influence
- If player stops ritual: God-Sleepers lose -10% influence, Jötun-Readers gain +5%

**Consequence (Player Inaction):**

- Jötunheim becomes contested (God-Sleeper 45%, Independent 40%)
- Jötun-Forged encounters increase +40%
- Reality distortion hazards spawn
- Cultist NPCs spread propaganda

**Consequence (Player Intervention):**

- Jötunheim remains Independent
- God-Sleeper reputation decreases -15
- Jötun-Reader reputation increases +10
- Quest reward: Legendary artifact

---

## VIII. v5.0 Setting Compliance

### Layer 2 Voice (Diagnostic/Clinical)

**✅ Correct Usage:**

- "Faction influence calculation"
- "Territory control state: Contested"
- "War balance metric"
- "Sector ownership parameters"

**❌ Incorrect Usage:**

- ~~"Faction magic power"~~
- ~~"Territory blessing effects"~~
- ~~"Divine right to rule"~~
- ~~"Magical border wards"~~

### Technology as Failing Systems

**Territory Control Framing:**

- Factions don't "own" territory through legal titles
- Control represents "coherence zones" where faction signal is strongest
- Wars are "system conflicts" over operational dominance
- Player influence represents "signal amplification"

**v5.0 Canonical Interpretation:**

```
Iron-Bane "control" = their anti-Undying protocols are most active
Jötun-Reader "control" = their data excavation network densest
God-Sleeper "control" = their Jötun-Forged activation signal strongest
Rust-Clan "control" = their scavenging routes most established
```

---

## IX. Testing Strategy

### Unit Tests (Target: 85%+ coverage)

**TerritoryControlService Tests:**

```csharp
[TestMethod]
public void CalculateSectorControl_MultipleFactionsBalanced_ReturnsIndependent()
{
    // Arrange: 3 factions with 30-35% influence each
    // Act: Calculate control state
    // Assert: Returns "Independent"
}

[TestMethod]
public void CalculateSectorControl_OneFactionDominant_ReturnsStable()
{
    // Arrange: Iron-Bane 65%, others < 20%
    // Act: Calculate control state
    // Assert: Returns "Stable Control" with Iron-Bane as dominant
}

[TestMethod]
public void ShiftInfluence_PlayerKillsEnemy_IncreasesPlayerFactionInfluence()
{
    // Arrange: Character with Iron-Bane reputation, kills Undying in Trunk
    // Act: ShiftInfluence(characterId, sectorId, "Iron-Bane", +2)
    // Assert: Iron-Bane influence increases by 2.0
}
```

**FactionWarService Tests:**

```csharp
[TestMethod]
public void CheckWarTrigger_TwoFactions45PercentPlus_TriggersWar()
{
    // Arrange: Iron-Bane 47%, Rust-Clan 46% in same sector for 10 days
    // Act: CheckWarTrigger()
    // Assert: War initiated between two factions
}

[TestMethod]
public void AdvanceWar_PlayerActionsShiftBalance_UpdatesWarBalance()
{
    // Arrange: Ongoing war, player completes quest for faction_a
    // Act: RecordPlayerAction(questComplete, faction_a, +10)
    // Assert: War balance shifts +10 toward faction_a
}

[TestMethod]
public void ResolveWar_BalanceAboveThreshold_DeclaresVictor()
{
    // Arrange: War balance = +65 (threshold = ±50)
    // Act: ResolveWar()
    // Assert: faction_a declared victor, influence updated
}
```

**WorldEventService Tests:**

```csharp
[TestMethod]
public void GenerateRandomEvent_ContestedSector_SpawnsIncursion()
{
    // Arrange: Contested sector with 45% vs 45% influence
    // Act: GenerateRandomEvent() [mocked 5% roll success]
    // Assert: "Faction Incursion" event created
}

[TestMethod]
public void GenerateTerritorialQuest_WarZone_CreatesWarEffortQuest()
{
    // Arrange: Sector in active war state
    // Act: GenerateTerritorialQuest(sectorId)
    // Assert: Quest of type "War_Effort" created
}
```

---

## X. Success Criteria

**Functional Requirements:**

- ✅ Sectors have trackable faction influence (0-100%)
- ✅ Control states calculated correctly (Stable/Contested/War/Independent/Ruined)
- ✅ Faction wars trigger and resolve
- ✅ Player actions measurably shift influence
- ✅ Territory control affects procedural generation
- ✅ Dynamic quests spawn based on territory state
- ✅ NPC behavior changes with controlling faction
- ✅ Merchants adjust stock based on faction

**Quality Gates:**

- ✅ 85%+ unit test coverage
- ✅ Serilog structured logging throughout
- ✅ v5.0 compliance (Layer 2 voice, no fantasy language)
- ✅ ASCII-only entity names
- ✅ Complete integration with v0.33 (Faction), v0.34 (Companions), v0.14 (Quests)

**Performance:**

- ✅ Territory lookups < 50ms
- ✅ Influence calculations < 100ms
- ✅ War state updates < 200ms

---

## XI. v2.0+ Future Expansion Hooks

**Deferred Features:**

- **Multiple Simultaneous Wars:** v0.35 supports 1 war per sector, v2.0+ can parallelize
- **Faction Diplomacy:** Alliances, truces, betrayals
- **Economic Warfare:** Trade embargoes, resource monopolies
- **Permanent Conquest:** Currently reversible, v2.0+ could allow permanent faction elimination
- **Player-Founded Factions:** Build your own faction from scratch
- **Territorial Buildings:** Construct faction outposts, fortifications

**Extension Points:**

- `Faction_Territory_Control` table supports future `military_units` column
- `World_Events` table designed for 20+ event types
- `TerritoryService` interface allows custom influence modifiers

---

## XII. Child Specifications

This parent specification spawns 4 child specifications:

1. **v0.35.1: Database Schema & Territory Definitions** (7-10 hours)
    - Complete database schema
    - Seed data for initial states
    - Migration scripts
2. **v0.35.2: Territory Mechanics & Faction Wars** (8-11 hours)
    - TerritoryControlService
    - FactionWarService
    - War triggering and resolution
3. **v0.35.3: Dynamic World Events & Consequences** (7-10 hours)
    - WorldEventService
    - Random event generation
    - Territorial quest templates
4. **v0.35.4: Player Influence & Service Integration** (8-9 hours)
    - TerritoryService orchestration
    - Player action tracking
    - Complete integration testing

**Total Timeline:** 30-40 hours

---

**Implementation-ready specification for territorial conflict and dynamic world systems complete.**

**Next:** Implement v0.35.1 (Database Schema & Territory Definitions)