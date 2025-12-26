# Faction & Territory System Specification

> **Template Version**: 1.0
> **Last Updated**: 2025-11-28
> **Status**: Active
> **Specification ID**: SPEC-SYSTEM-011

---

## Document Control

### Version History
| Version | Date | Author | Changes | Reviewers |
|---------|------|--------|---------|-----------|
| 1.0 | 2025-11-28 | AI | Initial specification | - |

### Approval Status
- [x] **Draft**: Initial authoring in progress
- [x] **Review**: Ready for stakeholder review
- [x] **Approved**: Approved for implementation
- [x] **Active**: Currently implemented and maintained
- [ ] **Deprecated**: Superseded by [SPEC-ID]

### Stakeholders
- **Primary Owner**: World Systems Lead
- **Design**: Faction balance, reputation economy, territory mechanics
- **Implementation**: FactionService.cs, TerritoryService.cs, ReputationService.cs
- **QA/Testing**: 86 tests with ~90% coverage

---

## Executive Summary

### Purpose Statement
The Faction & Territory System provides dynamic world state through 5 competing factions, reputation-based player relationships, territory control mechanics, faction wars, and world events that respond to player actions.

### Scope
**In Scope**:
- 5 factions with unique philosophies and relationships
- Reputation system with 6 tiers (-100 to +100)
- Territory control across 10 sectors
- Faction war mechanics with victory/defeat resolution
- World events (9 types) with player influence
- Merchant price modifiers by reputation
- Encounter frequency modifiers
- NPC behavior modifiers by territory control
- Faction quests (25 total) and rewards (18 total)
- GUI components for faction/territory visualization

**Out of Scope**:
- Player-created factions → Not planned
- Faction leadership roles → Future enhancement
- Real-time territory simulation → Turn-based/daily updates
- Multiplayer faction conflicts → Single-player only

### Success Criteria
- **Player Experience**: Faction choices feel meaningful and consequential
- **Technical**: Daily territory updates complete in <1s
- **Design**: No faction is objectively "best" - all have trade-offs
- **Balance**: Reputation changes proportional to action difficulty

---

## Related Documentation

### Dependencies
**Depends On**:
- `SPEC-COMBAT-001`: Combat Resolution (enemy kills affect reputation)
- `SPEC-SYSTEM-010`: Companion System (faction-gated recruitment)
- Quest System: Quest completion affects reputation

**Depended Upon By**:
- `SPEC-SYSTEM-010`: Companion System (faction reputation gates)
- Merchant System: Price modifiers
- Encounter Generation: Faction-based encounters

### Related Specifications
- `SPEC-SYSTEM-009`: GUI Implementation (faction UI panels)
- `SPEC-SYSTEM-010`: Companion System (faction recruitment)
- `SPEC-ECONOMY-001`: Loot & Equipment (faction rewards)

### Implementation Documentation
- `FACTION_SYSTEM_TEST_COVERAGE.md`: Test coverage details
- `TERRITORY_INTEGRATION_GUIDE.md`: Integration points

### Code References
- **Primary Service**: `RuneAndRust.Engine/FactionService.cs` (347 lines)
- **Reputation**: `RuneAndRust.Engine/ReputationService.cs` (359 lines)
- **Territory**: `RuneAndRust.Engine/TerritoryService.cs` (559 lines)
- **Territory Control**: `RuneAndRust.Engine/TerritoryControlService.cs` (364 lines)
- **Faction Wars**: `RuneAndRust.Engine/FactionWarService.cs` (405 lines)
- **World Events**: `RuneAndRust.Engine/WorldEventService.cs` (559 lines)
- **Encounters**: `RuneAndRust.Engine/FactionEncounterService.cs` (286 lines)
- **Merchant Integration**: `RuneAndRust.Engine/MerchantFactionInventory.cs` (283 lines)
- **NPC Reactions**: `RuneAndRust.Engine/NPCFactionReactions.cs` (181 lines)

---

## Design Philosophy

### Design Pillars

1. **Choices Have Consequences**
   - **Rationale**: Faction alignment should matter; neutrality has trade-offs
   - **Examples**: Helping Iron-Banes angers God-Sleepers; being neutral limits access
   - **Anti-Pattern**: "Best" faction that has no downsides

2. **Dynamic World State**
   - **Rationale**: World should feel alive and responsive to player actions
   - **Examples**: Territory control shifts based on player quests, faction wars emerge
   - **Anti-Pattern**: Static world that never changes regardless of player choices

3. **Meaningful Reputation Tiers**
   - **Rationale**: Each tier should unlock distinct benefits and risks
   - **Examples**: Exalted = 30% discount + exclusive rewards; Hated = 50% markup + hostile NPCs
   - **Anti-Pattern**: Tiers that are just numbers with no gameplay impact

4. **Faction Identity Through Gameplay**
   - **Rationale**: Each faction should feel distinct in quests, rewards, and playstyle
   - **Examples**: Iron-Banes specialize in anti-Undying; Jötun-Readers provide knowledge
   - **Anti-Pattern**: Generic quests that could belong to any faction

---

## Faction Definitions

### 5 Implemented Factions

| ID | Faction | Philosophy | Primary Location |
|----|---------|------------|------------------|
| 1 | **Iron-Banes** | Anti-Undying specialists using purification protocols (technology, not religion) | Trunk/Roots/Muspelheim |
| 2 | **God-Sleeper Cultists** | Cargo cultists who believe Jötun-Forged are sleeping gods | Jötunheim |
| 3 | **Jötun-Readers** | Data archaeologists seeking Pre-Glitch knowledge | Alfheim |
| 4 | **Rust-Clans** | Pragmatic Midgard survivors focused on trade & survival | Midgard |
| 5 | **Independents** | Unaffiliated survivors rejecting faction membership | All |

### Faction Relationships

```
┌─────────────────────────────────────────────────────────────┐
│                    FACTION RELATIONSHIPS                    │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│     Iron-Banes ◄───────► Rust-Clans ◄───────► Jötun-Readers│
│          │                                                  │
│          │ ENEMIES                                          │
│          ▼                                                  │
│    God-Sleeper Cultists ◄─────► Independents (Allies)      │
│                                                             │
│ ─────── = Allied                                            │
│ ═══════ = Enemies                                           │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

| Faction | Allies | Enemies |
|---------|--------|---------|
| Iron-Banes | Rust-Clans | God-Sleeper Cultists |
| God-Sleeper Cultists | Independents | Iron-Banes |
| Jötun-Readers | Rust-Clans | None (but oppose God-Sleepers) |
| Rust-Clans | Iron-Banes, Jötun-Readers | None |
| Independents | None | None |

### Faction Details

#### Iron-Banes
- **Philosophy**: Technology-based purification of Undying threats
- **Recruitment**: Reputation 25+ required for companion Kára
- **Special**: Anti-Undying abilities, purification equipment
- **Quest Focus**: Combat trials, corruption containment, Undying destruction

#### God-Sleeper Cultists
- **Philosophy**: Worship of dormant Jötun-Forged as sleeping deities
- **Recruitment**: Reputation 25+ required for companion Einar
- **Special**: Jötun-based abilities, awakening rituals
- **Quest Focus**: Temple duties, offerings, awakening ceremonies

#### Jötun-Readers
- **Philosophy**: Archaeological recovery of Pre-Glitch data
- **Recruitment**: Reputation 25+ required for companion Finnr
- **Special**: Data decryption, artifact analysis
- **Quest Focus**: Data recovery, archive exploration, artifact study

#### Rust-Clans
- **Philosophy**: Pragmatic survival through trade and scavenging
- **Recruitment**: No requirement for companion Bjorn (neutral)
- **Special**: Trade networks, scavenging expertise
- **Quest Focus**: Defense duties, scavenge runs, trade escort

#### Independents
- **Philosophy**: Freedom from faction obligations
- **Recruitment**: No faction-locked companions
- **Special**: Solo bonuses, no faction conflicts
- **Quest Focus**: Personal quests, mercenary work

---

## Reputation System

### Reputation Scale

**Range**: -100 to +100

| Tier | Value Range | Status |
|------|-------------|--------|
| **Hated** | -100 to -76 | Kill-on-sight, hostile encounters |
| **Hostile** | -75 to -26 | Unfriendly, increased hostile encounters |
| **Neutral** | -25 to +24 | Standard interactions |
| **Friendly** | +25 to +49 | Reduced hostile encounters, minor discounts |
| **Allied** | +50 to +74 | No hostile encounters, significant discounts |
| **Exalted** | +75 to +100 | Best prices, exclusive rewards, full access |

### Reputation Modifiers

#### Price Modifiers

| Tier | Modifier | Effect |
|------|----------|--------|
| Exalted | 0.70× | 30% discount |
| Allied | 0.80× | 20% discount |
| Friendly | 0.90× | 10% discount |
| Neutral | 1.00× | Normal price |
| Hostile | 1.25× | 25% markup |
| Hated | 1.50× | 50% markup |

#### Encounter Frequency Modifiers

| Tier | Modifier | Effect |
|------|----------|--------|
| Exalted | 0.0× | No hostile encounters |
| Allied | 0.0× | No hostile encounters |
| Friendly | 0.5× | 50% fewer hostile encounters |
| Neutral | 1.0× | Normal encounter rate |
| Hostile | 2.0× | 2× hostile encounters |
| Hated | 3.0× | 3× hostile encounters |

### Reputation Change Sources

| Action Type | Base Change | Notes |
|-------------|-------------|-------|
| Quest Completion | +15 to +50 | Based on quest difficulty |
| Enemy Kill (faction) | -5 to -15 | Killing faction members |
| Enemy Kill (enemy faction) | +5 | Killing faction's enemies |
| Territory Defense | +10 | Defending faction territory |
| Sabotage | -20 | Sabotaging faction assets |
| Escort Caravan | +15 | Protecting faction caravan |
| Destroy Hazard | +5 | Clearing faction territory |
| Activate Artifact | +10 | For Jötun-Readers specifically |

### Allied/Enemy Reputation Spillover

- Helping allied factions: +50% reputation to ally
- Harming enemy factions: +25% reputation bonus
- Harming allied factions: -25% to ally relationship

---

## Territory Control System

### 10 Sectors

| ID | Sector | Z-Level | Default Faction |
|----|--------|---------|-----------------|
| 1 | Midgard | Trunk | Rust-Clans |
| 2 | Muspelheim | Roots | Iron-Banes |
| 3 | Niflheim | Roots | Contested |
| 4 | Alfheim | Canopy | Jötun-Readers |
| 5 | Jötunheim | Trunk | God-Sleeper Cultists |
| 6 | Svartalfheim | Roots | Independent |
| 7 | Vanaheim | Canopy | Contested |
| 8 | Helheim | Roots | Ruined |
| 9 | Asgard | Canopy | Contested |
| 10 | Valhalla | Canopy | Independent |

### Control States

| State | Definition | Effects |
|-------|------------|---------|
| **Stable** | One faction ≥60% influence | Faction controls sector fully |
| **Contested** | 2+ factions 40-60% influence | Increased conflict, mixed encounters |
| **War** | Active faction war in progress | High hazards, war-related encounters |
| **Independent** | No faction >40% influence | Neutral ground, varied encounters |
| **Ruined** | Destroyed/inaccessible | Extreme hazards, no faction presence |

### Influence Mechanics

**Influence Range**: 0.0 to 100.0 per faction per sector

**Influence Gain Sources**:
- Player kills faction enemies: +0.5 to +2.0
- Quest completion in sector: +1.0 to +5.0
- Territory defense: +2.0 to +5.0
- Daily faction activity: +0.1 to +0.5 (NPC-driven)

**Influence Loss Sources**:
- Player kills faction members: -1.0 to -5.0
- Sabotage actions: -3.0 to -10.0
- War losses: -5.0 to -20.0 (from war resolution)

### Sector Generation Parameters

Territory control affects procedural generation:

| Parameter | Controlled By | Effect |
|-----------|---------------|--------|
| EnemyFactionFilter | Controlling faction | Filter enemy types |
| EnemyDensityMultiplier | Control state | 0.8-1.5× enemy density |
| HazardDensityMultiplier | Faction + war state | See hazard modifiers |
| ArtifactSpawnRate | Jötun-Readers | +50% artifacts |
| SalvageMaterialRate | Rust-Clans | +25% salvage |
| MerchantPriceModifier | Reputation tier | Price adjustments |
| ScholarNPCChance | Jötun-Readers | +30% scholars |
| ScavengerNPCChance | Rust-Clans | +25% scavengers |

### Hazard Density by Faction Control

| Controlling Faction | Hazard Modifier |
|--------------------|-----------------|
| God-Sleeper Cultists | 1.25× (+25% - reality corruption) |
| Jötun-Readers | 1.0× (no change - research focus) |
| Rust-Clans | 0.95× (-5% - maintained trade routes) |
| Iron-Banes | 0.90× (-10% - active corruption patrols) |
| Independents | 0.85× (-15% - well-maintained) |
| **War zones** | +50% additional modifier |

---

## Faction War System

### War Trigger Conditions

```
War triggers when:
- Two factions both have ≥45% influence in same sector
- Contested state has lasted 10+ days
- No active war already in sector
```

### War Constants

| Constant | Value | Purpose |
|----------|-------|---------|
| WAR_TRIGGER_THRESHOLD | 45.0% | Minimum influence for war |
| WAR_VICTORY_THRESHOLD | ±50.0 | War balance for victory |
| WAR_MAX_DURATION | 15 days | Maximum war length |
| WAR_VICTOR_INFLUENCE_GAIN | +20% | Influence gained by winner |
| WAR_LOSER_INFLUENCE_LOSS | -20% | Influence lost by loser |
| WAR_COLLATERAL_DAMAGE | 25% | Hazard density increase |

### War Balance System

**War Balance Range**: -100 to +100

- Positive values favor Faction A
- Negative values favor Faction B
- Victory at ±50 threshold

### War Balance Shifts

| Action | Balance Shift | Notes |
|--------|---------------|-------|
| Player kills Faction A enemy | -2.0 to -5.0 | Favors Faction B |
| Player kills Faction B enemy | +2.0 to +5.0 | Favors Faction A |
| Daily attrition | ±1.0 to ±3.0 | Based on influence ratio |
| Quest completion | ±3.0 to ±10.0 | Based on quest faction |

### War Resolution

```
On war end:
1. Victor gains +20% influence in sector
2. Loser loses -20% influence in sector
3. Control state updates (may become Stable)
4. Collateral damage persists for 7 days
5. Player reputation shifts based on involvement
```

---

## World Events System

### 9 Event Types

| Event Type | Trigger | Duration | Effects |
|------------|---------|----------|---------|
| **Faction_War** | War initiation | Until resolution | War mechanics active |
| **Incursion** | Random/triggered | 5-10 days | External threat, factions unite |
| **Supply_Raid** | Low supplies | 3-7 days | Resource scarcity |
| **Diplomatic_Shift** | Player actions | Immediate | Relationship changes |
| **Catastrophe** | Random | 7-14 days | Major hazards, faction losses |
| **Awakening_Ritual** | God-Sleepers only | 5 days | Jötun activity increase |
| **Excavation_Discovery** | Jötun-Readers only | 7 days | Artifact spawns increase |
| **Purge_Campaign** | Iron-Banes only | 7 days | Anti-Undying bonuses |
| **Scavenger_Caravan** | Rust-Clans only | 3 days | Trade opportunities |

### Event Processing

```
Daily event check per sector:
1. Check for active events → Process ongoing effects
2. Check for event triggers → Start new events if conditions met
3. Check for event resolution → Resolve completed events
4. Update sector state based on event outcomes
```

### Player Influence on Events

| Outcome | Requirement | Effect |
|---------|-------------|--------|
| **Success** | Player completed event objectives | Full positive effects |
| **Failure** | Event expired without completion | Negative consequences |
| **Partial** | Some objectives completed | Mixed results |
| **Player_Intervention** | Player directly affected outcome | Enhanced rewards/penalties |

---

## Faction Quests

### 25 Faction Quests (5 per faction)

| Faction | Quest | Rep Required | Rep Reward | Repeatable |
|---------|-------|--------------|------------|------------|
| **Iron-Banes** | Training Protocols | 0 | +15 | No |
| | Purge Rust | 0 | +20 | No |
| | Corrupted Forge | 25 | +30 | No |
| | Destroy Sleeper | 50 | +40 | No |
| | Blight Containment | 75 | +50 | No |
| **God-Sleepers** | Temple Duties | 0 | +15 | Yes |
| | Awakening Offerings | 0 | +20 | Yes |
| | Sacred Pilgrimage | 25 | +30 | No |
| | Silence Heretics | 50 | +40 | No |
| | Great Awakening | 75 | +50 | No |
| **Jötun-Readers** | Data Recovery | 0 | +15 | Yes |
| | Archive Access | 0 | +20 | No |
| | Decrypt Protocols | 25 | +30 | No |
| | Lost Library | 50 | +40 | No |
| | Pre-Glitch Archive | 75 | +50 | No |
| **Rust-Clans** | Defense Duty | 0 | +15 | Yes |
| | Scavenge Run | 0 | +20 | Yes |
| | Trade Escort | 25 | +30 | No |
| | Salvage Priority | 50 | +40 | No |
| | Clan Unification | 75 | +50 | No |
| **Independents** | Lone Wolf | 0 | +15 | No |
| | Neutral Ground | 0 | +20 | No |
| | No Allegiance | 25 | +30 | No |
| | Freedom Fighter | 50 | +40 | No |
| | True Independent | 75 | +50 | No |

---

## Faction Rewards

### 18 Faction Rewards (3-4 per faction)

| Faction | Reward | Tier Required | Type |
|---------|--------|---------------|------|
| **Iron-Banes** | Purification Protocols | Friendly | Service |
| | Zealot's Blade | Allied | Equipment |
| | Anti-Undying Inscription | Exalted | Ability |
| **God-Sleepers** | Temple Blessing | Friendly | Consumable |
| | Sleeper's Favor | Allied | Service |
| | Awakened Insight | Exalted | Ability |
| **Jötun-Readers** | Data Access Terminal | Friendly | Service |
| | Archive Credentials | Allied | Service |
| | Decryption Protocols | Exalted | Ability |
| **Rust-Clans** | Clan Trade Rates | Friendly | Discount |
| | Scavenger Network | Allied | Service |
| | Hidden Cache Access | Exalted | Service |
| **Independents** | Solo Survival Training | Friendly | Ability |
| | Neutral Zone Pass | Allied | Service |
| | Lone Wolf Trait | Exalted | Ability |

---

## Database Schema

### Factions Table

```sql
CREATE TABLE Factions (
    faction_id INTEGER PRIMARY KEY,
    faction_name TEXT NOT NULL UNIQUE,
    display_name TEXT NOT NULL,
    philosophy TEXT,
    description TEXT,
    primary_location TEXT,
    allied_factions TEXT,  -- CSV format
    enemy_factions TEXT,   -- CSV format
    created_at TEXT DEFAULT CURRENT_TIMESTAMP
);
```

### Characters_FactionReputations Table

```sql
CREATE TABLE Characters_FactionReputations (
    reputation_id INTEGER PRIMARY KEY AUTOINCREMENT,
    character_id INTEGER NOT NULL,
    faction_id INTEGER NOT NULL,
    reputation_value INTEGER DEFAULT 0
        CHECK(reputation_value BETWEEN -100 AND 100),
    reputation_tier TEXT
        CHECK(reputation_tier IN ('Hated','Hostile','Neutral',
                                   'Friendly','Allied','Exalted')),
    last_modified TEXT DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (character_id) REFERENCES saves(id) ON DELETE CASCADE,
    FOREIGN KEY (faction_id) REFERENCES Factions(faction_id),
    UNIQUE(character_id, faction_id)
);
```

### Faction_Territory_Control Table

```sql
CREATE TABLE Faction_Territory_Control (
    territory_control_id INTEGER PRIMARY KEY AUTOINCREMENT,
    world_id INTEGER NOT NULL,
    sector_id INTEGER NOT NULL,
    faction_name TEXT NOT NULL,
    influence_value REAL NOT NULL DEFAULT 0.0,  -- 0.0 to 100.0
    control_state TEXT NOT NULL
        CHECK(control_state IN ('Stable','Contested','War',
                                 'Independent','Ruined')),
    last_updated TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UNIQUE(world_id, sector_id, faction_name)
);
```

### Faction_Wars Table

```sql
CREATE TABLE Faction_Wars (
    war_id INTEGER PRIMARY KEY AUTOINCREMENT,
    world_id INTEGER NOT NULL,
    sector_id INTEGER NOT NULL,
    faction_a TEXT NOT NULL,
    faction_b TEXT NOT NULL,
    war_start_date TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    war_end_date TIMESTAMP,
    war_balance REAL DEFAULT 0.0,  -- -100 to +100
    is_active BOOLEAN NOT NULL DEFAULT 1,
    victor TEXT,
    collateral_damage INTEGER DEFAULT 0,
    CHECK(faction_a != faction_b),
    CHECK(war_balance BETWEEN -100 AND 100)
);
```

### World_Events Table

```sql
CREATE TABLE World_Events (
    event_id INTEGER PRIMARY KEY AUTOINCREMENT,
    world_id INTEGER NOT NULL,
    sector_id INTEGER,
    event_type TEXT NOT NULL
        CHECK(event_type IN ('Faction_War','Incursion','Supply_Raid',
                              'Diplomatic_Shift','Catastrophe',
                              'Awakening_Ritual','Excavation_Discovery',
                              'Purge_Campaign','Scavenger_Caravan')),
    affected_faction TEXT,
    event_title TEXT NOT NULL,
    event_description TEXT NOT NULL,
    event_start_date TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    event_end_date TIMESTAMP,
    event_duration_days INTEGER DEFAULT 7,
    is_resolved BOOLEAN NOT NULL DEFAULT 0,
    player_influenced BOOLEAN NOT NULL DEFAULT 0,
    outcome TEXT,
    influence_change REAL DEFAULT 0.0
);
```

### Player_Territorial_Actions Table

```sql
CREATE TABLE Player_Territorial_Actions (
    action_id INTEGER PRIMARY KEY AUTOINCREMENT,
    character_id INTEGER NOT NULL,
    world_id INTEGER NOT NULL,
    sector_id INTEGER NOT NULL,
    action_type TEXT NOT NULL
        CHECK(action_type IN ('Kill_Enemy','Complete_Quest',
                               'Defend_Territory','Sabotage',
                               'Escort_Caravan','Destroy_Hazard',
                               'Activate_Artifact')),
    affected_faction TEXT NOT NULL,
    influence_delta REAL NOT NULL,  -- -10.0 to +10.0
    action_timestamp TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    notes TEXT,
    CHECK(influence_delta BETWEEN -10.0 AND 10.0)
);
```

---

## GUI Implementation

### Current Status

**Backend**: Complete (10 services, 9 models, 6 database tables, 86 tests)
**GUI**: Not yet implemented

### Planned GUI Components

#### 1. Faction Reputation Panel

**Location**: Character Sheet → Factions Tab
**Purpose**: Display reputation with all factions

```
┌─────────────────────────────────────────────────────────────┐
│ FACTION STANDING                                            │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│ Iron-Banes                              [ALLIED]    +62    │
│ ████████████████████░░░░░░░░░░  ──────────────────────────│
│ Benefits: 20% discount, no hostile encounters              │
│ Next tier: Exalted at +75 (13 more)                        │
│                                                             │
│ God-Sleeper Cultists                    [HOSTILE]   -45    │
│ ░░░░░░░░░░░░░░██████████████████  ────────────────────────│
│ Penalties: 25% markup, 2× hostile encounters               │
│ Warning: Enemies of Iron-Banes                             │
│                                                             │
│ Jötun-Readers                           [FRIENDLY]  +38    │
│ ████████████████░░░░░░░░░░░░░░  ──────────────────────────│
│ Benefits: 10% discount, reduced hostile encounters         │
│ Next tier: Allied at +50 (12 more)                         │
│                                                             │
│ Rust-Clans                              [NEUTRAL]   +12    │
│ ████████████░░░░░░░░░░░░░░░░░░  ──────────────────────────│
│ Standard interactions                                       │
│ Next tier: Friendly at +25 (13 more)                       │
│                                                             │
│ Independents                            [NEUTRAL]    0     │
│ ██████████░░░░░░░░░░░░░░░░░░░░  ──────────────────────────│
│ Standard interactions                                       │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

**Data Bindings**:
- `FactionReputationViewModel.Factions` → `ObservableCollection<FactionReputationDisplay>`
- Reputation bars with tier color coding
- Tooltip on hover showing benefits/penalties
- Progress to next tier indicator

#### 2. Territory Map View

**Location**: Exploration Screen → Map Tab or dedicated view
**Purpose**: Visualize sector control and events

```
┌─────────────────────────────────────────────────────────────┐
│ YGGDRASIL TERRITORY MAP                      [Sector: 4/10]│
├─────────────────────────────────────────────────────────────┤
│                                                             │
│                    ┌─────────┐                              │
│                    │ ASGARD  │                              │
│                    │[CONTEST]│                              │
│            ┌───────┴─────────┴───────┐                      │
│            │      ALFHEIM            │                      │
│            │ [JÖTUN-READERS: STABLE] │ ◄── You are here    │
│            │ Influence: 72%          │                      │
│            └───────┬─────────────────┘                      │
│     ┌──────────────┼──────────────┐                         │
│     │              │              │                         │
│ ┌───┴───┐    ┌─────┴─────┐  ┌─────┴─────┐                  │
│ │JÖTUN- │    │  MIDGARD  │  │  VANAHEIM │                  │
│ │ HEIM  │    │[RUST-CLAN]│  │ [CONTEST] │                  │
│ │ [GOD- │    │  STABLE   │  │           │                  │
│ │SLEEPER│    │           │  │           │                  │
│ └───────┘    └─────┬─────┘  └───────────┘                  │
│                    │                                        │
│            ┌───────┴───────┐                                │
│            │  MUSPELHEIM   │                                │
│            │ [IRON-BANES]  │                                │
│            │    STABLE     │                                │
│            └───────────────┘                                │
│                                                             │
│ Legend: [STABLE] [CONTEST] [WAR!] [INDEP] [RUINED]        │
└─────────────────────────────────────────────────────────────┘
```

**Interaction**:
- Click sector to view details
- Color-coded by control state
- Faction icons for controlling faction
- Pulsing indicator for active events/wars

#### 3. Sector Detail Panel

**Location**: Territory Map → Click sector
**Purpose**: Show detailed sector information

```
┌─────────────────────────────────────────────────────────────┐
│ SECTOR: ALFHEIM                                             │
├─────────────────────────────────────────────────────────────┤
│ Z-Level: Canopy                                             │
│ Control State: STABLE                                       │
│ Dominant Faction: Jötun-Readers (72%)                      │
│                                                             │
│ FACTION INFLUENCE                                           │
│ ├─ Jötun-Readers:  ██████████████░░░░  72%                 │
│ ├─ Rust-Clans:     ███░░░░░░░░░░░░░░░  18%                 │
│ ├─ Iron-Banes:     █░░░░░░░░░░░░░░░░░   7%                 │
│ └─ Others:         ░░░░░░░░░░░░░░░░░░   3%                 │
│                                                             │
│ ACTIVE EVENTS                                               │
│ ├─ Excavation Discovery (3 days remaining)                 │
│ │   Artifact spawn rate +50%                               │
│ │   [Participate]                                          │
│ └─ None other                                               │
│                                                             │
│ SECTOR EFFECTS                                              │
│ ├─ Scholar NPCs: +30%                                      │
│ ├─ Hazard Density: Normal                                  │
│ └─ Artifact Rate: +50% (event)                             │
│                                                             │
│ AVAILABLE QUESTS                                            │
│ ├─ Data Recovery (Repeatable, +15 rep)                     │
│ └─ Decrypt Protocols (+30 rep, requires Friendly)          │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

#### 4. War Status Panel

**Location**: Territory Map overlay or notification
**Purpose**: Display active faction wars

```
┌─────────────────────────────────────────────────────────────┐
│ ⚔️ ACTIVE WAR: VANAHEIM                                    │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│ Iron-Banes          vs          God-Sleeper Cultists       │
│     ████████████████░░░░░░░░░░░░░░░░                       │
│                  +35                                        │
│                                                             │
│ War Balance: Iron-Banes Favored                            │
│ Duration: Day 8 of 15                                       │
│ Victor at: ±50                                              │
│                                                             │
│ RECENT ACTIONS                                              │
│ ├─ You killed 3 God-Sleeper enemies (+6.0)                 │
│ ├─ Iron-Banes raid succeeded (+3.0)                        │
│ └─ God-Sleeper reinforcements arrived (-2.0)               │
│                                                             │
│ COLLATERAL DAMAGE: 25% increased hazards                   │
│                                                             │
│ YOUR INFLUENCE                                              │
│ ├─ Kills for Iron-Banes: 12 (+24.0 total)                  │
│ ├─ Kills for God-Sleepers: 0                               │
│ └─ Net contribution: Iron-Banes +24.0                      │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

#### 5. World Events Log

**Location**: Notification panel or dedicated view
**Purpose**: Track active and recent world events

```
┌─────────────────────────────────────────────────────────────┐
│ WORLD EVENTS                                    [Filter ▼] │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│ ACTIVE EVENTS                                               │
│ ┌─────────────────────────────────────────────────────────┐│
│ │ ⚔️ Faction War: Vanaheim                                ││
│ │    Iron-Banes vs God-Sleeper Cultists                   ││
│ │    Day 8/15 | Balance: +35 (Iron-Banes)                 ││
│ └─────────────────────────────────────────────────────────┘│
│ ┌─────────────────────────────────────────────────────────┐│
│ │ 📜 Excavation Discovery: Alfheim                        ││
│ │    Jötun-Readers unearthed Pre-Glitch data core        ││
│ │    3 days remaining | +50% artifact spawns             ││
│ └─────────────────────────────────────────────────────────┘│
│                                                             │
│ RECENT EVENTS (Last 7 days)                                │
│ ├─ Supply Raid (Midgard) - Resolved: Success              │
│ │   Rust-Clans defended caravan, +5% influence            │
│ ├─ Diplomatic Shift (Jötunheim) - Resolved: Partial       │
│ │   God-Sleeper/Independent tensions eased                 │
│ └─ Purge Campaign (Muspelheim) - Resolved: Success        │
│     Iron-Banes eliminated Undying nest, +8% influence     │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

#### 6. Faction Quest Board

**Location**: NPC interaction or dedicated panel
**Purpose**: Show available faction quests

```
┌─────────────────────────────────────────────────────────────┐
│ FACTION QUESTS: IRON-BANES                    [Your Rep: 62]│
├─────────────────────────────────────────────────────────────┤
│                                                             │
│ AVAILABLE                                                   │
│ ┌─────────────────────────────────────────────────────────┐│
│ │ ✓ Corrupted Forge                          [+30 Rep]   ││
│ │   Destroy the corrupted forge in Muspelheim depths     ││
│ │   Requires: Friendly (25) ✓                            ││
│ │   [Accept Quest]                                        ││
│ └─────────────────────────────────────────────────────────┘│
│ ┌─────────────────────────────────────────────────────────┐│
│ │ ✓ Destroy Sleeper                          [+40 Rep]   ││
│ │   Eliminate the dormant Jötun-Forged threat            ││
│ │   Requires: Allied (50) ✓                              ││
│ │   [Accept Quest]                                        ││
│ └─────────────────────────────────────────────────────────┘│
│                                                             │
│ LOCKED                                                      │
│ ┌─────────────────────────────────────────────────────────┐│
│ │ 🔒 Blight Containment                      [+50 Rep]   ││
│ │   Lead containment of major Blight outbreak            ││
│ │   Requires: Exalted (75) - Need 13 more rep            ││
│ └─────────────────────────────────────────────────────────┘│
│                                                             │
│ COMPLETED                                                   │
│ ├─ Training Protocols ✓                                    │
│ └─ Purge Rust ✓                                            │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

### ViewModel Architecture

```
FactionSystemViewModel
├── FactionReputationViewModel
│   ├── Factions: ObservableCollection<FactionReputationDisplay>
│   ├── SelectedFaction: FactionReputationDisplay
│   └── ViewRewardsCommand: ReactiveCommand
│
├── TerritoryMapViewModel
│   ├── Sectors: ObservableCollection<SectorViewModel>
│   ├── SelectedSector: SectorViewModel
│   ├── ActiveWars: ObservableCollection<WarViewModel>
│   └── SelectSectorCommand: ReactiveCommand<int>
│
├── SectorDetailViewModel
│   ├── Sector: TerritoryStatus
│   ├── Influences: ObservableCollection<FactionInfluenceDisplay>
│   ├── ActiveEvents: ObservableCollection<WorldEventDisplay>
│   ├── AvailableQuests: ObservableCollection<FactionQuestDisplay>
│   └── ParticipateInEventCommand: ReactiveCommand<int>
│
├── WarStatusViewModel
│   ├── War: FactionWar
│   ├── RecentActions: ObservableCollection<WarActionDisplay>
│   ├── PlayerContribution: WarContributionSummary
│   └── RefreshCommand: ReactiveCommand
│
└── WorldEventsViewModel
    ├── ActiveEvents: ObservableCollection<WorldEventDisplay>
    ├── RecentEvents: ObservableCollection<WorldEventDisplay>
    ├── FilterType: EventFilter
    └── FilterCommand: ReactiveCommand<string>
```

---

## Integration Points

### Pending Integration (from TERRITORY_INTEGRATION_GUIDE.md)

| Integration Point | Status | Description |
|-------------------|--------|-------------|
| Service Initialization | Pending | Wire services in Program.cs |
| Quest Completion Hook | Pending | Record territorial actions on quest complete |
| Enemy Kill Hook | Pending | Record territorial actions on enemy kill |
| Daily Update Loop | Pending | Call ProcessDailyTerritoryUpdate() |
| Generation Integration | Pending | Apply SectorGenerationParams to dungeons |
| Companion Reactions | Partial | CompanionTerritoryReactions exists |
| NPC Modifiers | Pending | Integrate NPCFactionReactions |
| Merchant Inventory | Pending | Integrate MerchantFactionInventory |

---

## Performance Considerations

### Daily Update Performance

| Operation | Target | Notes |
|-----------|--------|-------|
| Territory status calculation | <100ms | Per sector |
| War check and advancement | <50ms | Per active war |
| Event processing | <200ms | All sectors |
| Full daily update | <1s | All operations |

### Caching Strategy

- Faction definitions cached at startup
- Sector status cached, invalidated on actions
- Reputation cached per-session, updated on change

---

## Testing Strategy

### Existing Test Coverage (86 tests)

| Test File | Tests | Coverage |
|-----------|-------|----------|
| FactionDatabaseTests | 12 | Schema validation |
| ReputationServiceTests | 16 | Tier calculations, modifiers |
| FactionServiceTests | 17 | Witness system, relationships |
| FactionContentTests | 12 | Quests, rewards |
| FactionIntegrationTests | 16 | End-to-end workflows |
| FactionEncounterServiceTests | 13 | Encounter generation |

### Additional Test Scenarios Needed

- Territory control state transitions
- War trigger conditions
- War resolution outcomes
- World event processing
- GUI ViewModel behavior

---

## Known Limitations & Future Work

### Current Limitations

1. **No GUI**: Backend complete, UI not implemented
2. **Service Wiring**: Services exist but not initialized in Program.cs
3. **Integration Hooks**: Quest/combat hooks not connected
4. **Daily Loop**: No game loop integration for daily updates

### Planned Enhancements

1. **Multi-World Support**: Schema supports multiple worlds
2. **Faction Leadership**: Player can become faction leader
3. **Custom Factions**: Player-created factions
4. **Diplomatic Actions**: Direct diplomacy between factions
5. **Territory Conquest**: Player can claim sectors

---

## Appendix A: Service Method Reference

### FactionService

| Method | Purpose |
|--------|---------|
| `ProcessWitnessedAction()` | Handle reputation from witnessed actions |
| `GetAllFactions()` | List all factions |
| `GetFactionById()` | Get faction by ID |
| `GetFactionByName()` | Get faction by name |
| `IsFactionHostile()` | Check hostility status |
| `GetHostileFactions()` | List hostile factions |

### ReputationService

| Method | Purpose |
|--------|---------|
| `ModifyReputation()` | Change reputation value |
| `GetFactionReputation()` | Get current reputation |
| `GetReputationTier()` | Calculate tier from value |
| `GetPriceModifier()` | Get price modifier for tier |
| `GetEncounterFrequencyModifier()` | Get encounter modifier |

### TerritoryService

| Method | Purpose |
|--------|---------|
| `RecordPlayerAction()` | Log player territorial action |
| `ProcessDailyTerritoryUpdate()` | Run daily update cycle |
| `GetSectorTerritoryStatus()` | Get sector status |
| `GetPlayerTotalInfluence()` | Get player's total influence |
| `GetSectorGenerationParams()` | Get generation parameters |

### TerritoryControlService

| Method | Purpose |
|--------|---------|
| `CalculateSectorControlState()` | Determine control state |
| `ShiftInfluence()` | Change faction influence |
| `GetSectorInfluences()` | List all influences |

### FactionWarService

| Method | Purpose |
|--------|---------|
| `CheckWarTrigger()` | Check if war should start |
| `InitiateWar()` | Start new faction war |
| `GetActiveWarForSector()` | Get active war |
| `AdvanceWar()` | Shift war balance |
| `ResolveWar()` | End war with victor |

### WorldEventService

| Method | Purpose |
|--------|---------|
| `ProcessDailyEventCheck()` | Check for new events |
| `GetActiveSectorEvents()` | List active events |
| `ProcessEvent()` | Handle event effects |

---

**End of Specification**
