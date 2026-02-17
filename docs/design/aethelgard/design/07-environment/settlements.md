---
id: SPEC-ENV-SETTLEMENTS
title: "Settlements — Social Sector Architecture"
version: 1.0
status: draft
last-updated: 2025-12-14
related-files:
  - path: "docs/07-environment/room-engine/core.md"
    status: Active
  - path: "docs/07-environment/navigation.md"
    status: Active
  - path: "docs/02-entities/faction-reputation.md"
    status: Active
  - path: "docs/02-entities/npcs/npc-overview.md"
    status: Active
---

# Settlements — Social Sector Architecture

> *"The Holds are not stops on a journey—they are the reason for the journey. Out there is rust and ruin. In here, we remember what we're fighting for."*
> — Jarl's steward, Crossroads Hold

---

## 1. Overview

Settlements are **Social Sectors**—a distinct sector type that uses the room-by-room navigation paradigm but optimizes for social interaction, commerce, and services rather than combat exploration.

**Key Distinction:**

| Sector Type | Purpose | Room Types | Encounters |
|-------------|---------|------------|------------|
| **Dungeon Sector** | Combat exploration | Combat-focused archetypes | Enemy spawns, hazards |
| **Social Sector** | Settlement interaction | Social-focused archetypes | NPCs, services, quests |

Players navigate settlements room-by-room, but the rooms contain merchants, quest-givers, and services rather than enemies and traps.

---

## 2. Settlement Classifications

### 2.1 Settlement Sizes

| Size | Room Count | Population | Typical Infrastructure |
|------|------------|------------|----------------------|
| **Outpost** | 3-5 | 5-20 | Gate, 1 service, residence |
| **Village** | 6-10 | 20-100 | Gate, plaza, 2-3 services, tavern |
| **Town** | 11-20 | 100-500 | Gate, plaza, 4-6 services, temple, barracks |
| **City** | 21-40 | 500-2000 | Multiple districts, all services, factions |
| **Capital** | 40+ | 2000+ | Named districts, unique locations, governance |

### 2.2 Settlement Generation Approach

| Approach | Use Case | Examples |
|----------|----------|----------|
| **Handcrafted** | Major story settlements | Utgard, Nidavellir, Jötun's Fall |
| **Procedural** | Minor waypoints, camps | Roadside outposts, Rig-stops |
| **Hybrid** | Major settlements with variable elements | Crossroads Hold (fixed layout, procedural merchants) |

---

## 3. Settlement Room Archetypes

### 3.1 Core Settlement Archetypes (9)

Settlements use a parallel archetype system to dungeon sectors:

| Archetype | Size | Exits | Purpose | Dungeon Equivalent |
|-----------|------|-------|---------|-------------------|
| **SettlementGate** | Medium | 1-2 | Entry/exit point | EntryHall |
| **Plaza** | Large | 3-5 | Central hub, quest givers | Junction |
| **MerchantStall** | Small | 1-2 | Commerce (category-specific) | — |
| **CraftingStation** | Medium | 1-2 | Crafting, repair, modification | — |
| **Tavern** | Medium | 2-3 | Rest, rumors, recruitment | — |
| **Barracks** | Medium | 1-2 | Faction services, bounties | — |
| **Temple** | Medium | 1-2 | Healing, spiritual services | — |
| **Residence** | Small | 1 | NPC homes, quest locations | SecretRoom |
| **Archive** | Medium | 1-2 | Information, lore access | — |

### 3.2 Archetype Specifications

#### SettlementGate (Entry Point)

```json
{
  "Archetype": "SettlementGate",
  "Size": "Medium",
  "Exits": [1, 2],
  "NPCs": ["Guard", "Toll Collector", "Greeter"],
  "Services": ["Entry permission", "Fast travel activation"],
  "Variations": ["Fortified Gate", "Trade Gate", "Ruined Arch", "Checkpoint"]
}
```

**Functions:**
- Settlement entry/exit
- Fast travel waypoint activation
- Faction reputation check (for gated settlements)
- Toll collection (faction-controlled settlements)

#### Plaza (Central Hub)

```json
{
  "Archetype": "Plaza",
  "Size": "Large",
  "Exits": [3, 5],
  "NPCs": ["Crier", "Quest Giver", "Wandering Merchants"],
  "Services": ["Notice board", "Announcements", "Quest hooks"],
  "Variations": ["Market Square", "Fountain Plaza", "Assembly Ground", "Trade Hub"]
}
```

**Functions:**
- Primary navigation hub within settlement
- Notice board for bounties and rumors
- Main quest NPC locations
- Festival/event staging area

#### MerchantStall (Commerce)

```json
{
  "Archetype": "MerchantStall",
  "Size": "Small",
  "Exits": [1, 2],
  "NPCs": ["Shopkeeper", "Assistant"],
  "Services": ["Buy", "Sell", "Appraise"],
  "Categories": ["General", "Weapons", "Armor", "Alchemy", "Salvage", "Specialty"]
}
```

**Functions:**
- Category-specific trading
- Reputation-gated inventory tiers
- Restocking mechanics
- Faction-influenced pricing

**Merchant Categories:**

| Category | Typical Stock | Restocking |
|----------|---------------|------------|
| **General** | Consumables, tools, common items | Every visit |
| **Weapons** | Arms, ammunition | 3-5 days |
| **Armor** | Protection, clothing | 3-5 days |
| **Alchemy** | Potions, reagents | 2-3 days |
| **Salvage** | Components, scrap | Event-based |
| **Specialty** | Rare items, faction goods | Quest-locked |

#### CraftingStation (Production)

```json
{
  "Archetype": "CraftingStation",
  "Size": "Medium",
  "Exits": [1, 2],
  "NPCs": ["Craftsman", "Apprentice"],
  "Services": ["Repair", "Craft", "Modify", "Enhance"],
  "Specialties": ["Smithy", "Workshop", "Stable", "Armorer", "Tinker"]
}
```

**Functions:**
- Equipment repair (condition restoration)
- Crafting services (NPC-assisted or player-driven)
- Modification installation
- Faction-specific enhancements

**Reputation Requirements:**

| Service | Reputation Requirement |
|---------|----------------------|
| Basic Repair | Neutral |
| Full Repair | Friendly |
| Basic Crafting | Neutral |
| Advanced Crafting | Friendly |
| Modifications | Honored |
| Faction Enhancements | Exalted |

#### Tavern (Rest & Social)

```json
{
  "Archetype": "Tavern",
  "Size": "Medium",
  "Exits": [2, 3],
  "NPCs": ["Innkeeper", "Bard", "Patrons", "Recruitables"],
  "Services": ["Rest", "Rumors", "Recruitment", "Gambling"],
  "Variations": ["Inn", "Mead Hall", "Wayfarer's Rest", "Guild House"]
}
```

**Functions:**
- Full rest (restore all resources)
- Rumor system (quest hooks, world state info)
- Companion recruitment
- Mini-games (gambling, drinking contests)

**Rest Mechanics:**

| Rest Type | Duration | Effect | Cost |
|-----------|----------|--------|------|
| Short Rest | 1 hour | Stamina recovery | Free |
| Full Rest | 8 hours | Full restoration | Room fee |
| Extended Rest | 24+ hours | Full restoration + healing | Room fee/day |

#### Barracks (Faction Services)

```json
{
  "Archetype": "Barracks",
  "Size": "Medium",
  "Exits": [1, 2],
  "NPCs": ["Commander", "Quartermaster", "Recruiter"],
  "Services": ["Bounties", "Contracts", "Training", "Faction Quests"],
  "Variations": ["Guard House", "Ranger Post", "Guild Hall", "Mercenary Camp"]
}
```

**Functions:**
- Faction-specific quest givers
- Bounty board (enemy-focused)
- Skill training services
- Faction reputation advancement

#### Temple (Spiritual Services)

```json
{
  "Archetype": "Temple",
  "Size": "Medium",
  "Exits": [1, 2],
  "NPCs": ["Priest", "Healer", "Acolyte"],
  "Services": ["Healing", "Corruption Removal", "Blessings", "Resurrection"],
  "Variations": ["Shrine", "Chapel", "Sanctuary", "Coherent Temple"]
}
```

**Functions:**
- Wound healing (HP restoration without rest)
- Corruption/contamination removal
- Temporary blessings (buffs)
- Death penalty mitigation

**Service Costs:**

| Service | Cost | Effect |
|---------|------|--------|
| Minor Healing | Donation | Heal 25% HP |
| Major Healing | Significant Donation | Heal 100% HP |
| Corruption Removal | Major Donation | Remove 1 Corruption |
| Blessing | Offering | +1 to stat for 24h |
| Resurrection | Significant Offering | Restore from death |

#### Residence (NPC Homes)

```json
{
  "Archetype": "Residence",
  "Size": "Small",
  "Exits": [1],
  "NPCs": ["Resident"],
  "Services": ["Conversation", "Quest", "Information"],
  "Variations": ["House", "Quarters", "Hall", "Estate"]
}
```

**Functions:**
- Quest NPC locations
- Character-driven side quests
- Lore delivery
- Relationship building

#### Archive (Information)

```json
{
  "Archetype": "Archive",
  "Size": "Medium",
  "Exits": [1, 2],
  "NPCs": ["Archivist", "Scribe", "Jötun-Reader"],
  "Services": ["Lore Access", "Map Updates", "Language Services", "Research"],
  "Variations": ["Library", "Scriptorium", "Records Hall", "Data Vault"]
}
```

**Functions:**
- Lore unlocks (bestiary, history, faction info)
- Map revelations (undiscovered areas)
- Language translation services
- Quest research (hints, solutions)

---

## 4. District System

### 4.1 District Definitions

Towns and larger settlements divide into **districts**—sub-graphs of rooms with thematic coherence:

| District | Function | Typical Rooms |
|----------|----------|---------------|
| **Gate District** | Entry, security | SettlementGate, Barracks, Residence |
| **Market District** | Commerce | Plaza, MerchantStalls, Archive |
| **Residential District** | Housing | Residences, Temple |
| **Craft District** | Production | CraftingStations, Storage |
| **Faction Quarter** | Faction HQ | Barracks, Archive, unique |
| **Administrative** | Governance | Residence (Jarl's Hall), Archive |

### 4.2 District Requirements by Size

| Settlement Size | District Count | Required Districts |
|-----------------|----------------|-------------------|
| **Outpost** | 0 | — (too small) |
| **Village** | 0-1 | — (optional) |
| **Town** | 2-3 | Gate, Market |
| **City** | 3-5 | Gate, Market, Residential, Craft |
| **Capital** | 5+ | All types + unique |

### 4.3 District Connections

Districts connect via **transition rooms** (Plazas, Corridors, Gates):

```
TOWN STRUCTURE (11-20 rooms)

Gate District ──────► Market District ──────► Residential District
   │                      │                        │
   └── Plaza ◄────────────┴────────────────────────┘
```

---

## 5. Safe Zone Mechanics

### 5.1 Settlement Protection

Settlements suppress hostile mechanics:

| Mechanic | In Settlement | Notes |
|----------|---------------|-------|
| **Random Encounters** | Disabled | No enemy spawns |
| **Environmental Hazards** | Disabled | Unless plot-driven |
| **Combat Initiation** | Disabled | Unless triggered by player action |
| **Fast Travel** | Enabled | Between discovered settlements |
| **Rest** | Enabled | Tavern rooms |
| **Time Passage** | Active | Affects restocking, quests |

### 5.2 Safe Zone Exceptions

Settlements can lose safe zone status:

| Event | Effect | Resolution |
|-------|--------|------------|
| **Siege** | Combat enabled in all rooms | Quest completion |
| **Infiltration** | Combat enabled in specific rooms | Clear hostiles |
| **Faction Conflict** | Combat enabled in disputed areas | Resolve conflict |
| **Corruption Event** | Hazards enabled | Cleansing quest |

---

## 6. Service Mechanics

### 6.1 Service Availability Matrix

| Service | Outpost | Village | Town | City | Capital |
|---------|---------|---------|------|------|---------|
| Basic Commerce | ✓ | ✓ | ✓ | ✓ | ✓ |
| Full Commerce | — | ✓ | ✓ | ✓ | ✓ |
| Basic Crafting | — | ✓ | ✓ | ✓ | ✓ |
| Advanced Crafting | — | — | ✓ | ✓ | ✓ |
| Rest (Short) | ✓ | ✓ | ✓ | ✓ | ✓ |
| Rest (Full) | — | ✓ | ✓ | ✓ | ✓ |
| Healing | — | — | ✓ | ✓ | ✓ |
| Corruption Removal | — | — | — | ✓ | ✓ |
| Training | — | — | ✓ | ✓ | ✓ |
| Archives | — | — | — | ✓ | ✓ |
| Faction HQ | — | — | — | ✓ | ✓ |

### 6.2 Reputation Gating

Services require minimum faction reputation:

| Reputation | Services Available |
|------------|-------------------|
| **Hostile** | None (barred entry) |
| **Unfriendly** | Basic commerce only |
| **Neutral** | Standard services |
| **Friendly** | Advanced services, discounts |
| **Honored** | Rare items, special quests |
| **Exalted** | Faction-exclusive, leadership access |

### 6.3 Pricing Modifiers

Base prices adjusted by faction standing:

| Reputation | Buy Modifier | Sell Modifier |
|------------|--------------|---------------|
| Hostile | — | — |
| Unfriendly | +50% | -30% |
| Neutral | +0% | +0% |
| Friendly | -10% | +10% |
| Honored | -20% | +20% |
| Exalted | -30% | +30% |

---

## 7. NPC Placement

### 7.1 NPC Assignment Model

NPCs are assigned to settlements and rooms:

```csharp
public record SettlementNpc(
    string NpcId,
    string SettlementId,
    string RoomId,
    string? DistrictId,
    NpcRole Role,
    Schedule Schedule
);

public enum NpcRole
{
    Merchant,
    Craftsman,
    QuestGiver,
    ServiceProvider,
    Background,
    Companion
}
```

### 7.2 NPC Distribution by Settlement Size

| Settlement Size | Named NPCs | Background NPCs |
|-----------------|------------|-----------------|
| Outpost | 1-3 | 2-5 |
| Village | 4-8 | 10-20 |
| Town | 10-20 | 30-60 |
| City | 25-50 | 100-200 |
| Capital | 50+ | 200+ |

### 7.3 NPC Schedules

NPCs follow daily schedules:

| Time Block | Merchant Behavior | Service Availability |
|------------|-------------------|---------------------|
| **Morning (6-12)** | Opening, restocking | Standard |
| **Afternoon (12-18)** | Peak business | Standard |
| **Evening (18-22)** | Closing | Limited |
| **Night (22-6)** | Home/Tavern | Emergency only |

---

## 8. Faction Influence

### 8.1 Settlement Control

Settlements are controlled by factions:

| Faction | Typical Settlements | Control Effect |
|---------|---------------------|----------------|
| **Midgard Combine** | Holds, trade hubs | Standard services, tolls |
| **Rangers Guild** | Outposts, waypoints | Survival focus, guides |
| **Rust-Clans** | Rig-stops, salvage camps | Salvage focus, Rig services |
| **Dvergr Hegemony** | Nidavellir, Deep Gates | Pure Steel access, tariffs |
| **Scavenger Barons** | Jötun's Fall, ice stations | Ice-Debt contracts |
| **Hearth-Clans** | Muspelheim settlements | Thermal services, trade |
| **Jötun-Readers** | Archives, scriptoria | Information services |

### 8.2 Faction Services

Each faction provides unique settlement services:

| Faction | Unique Service |
|---------|----------------|
| **Midgard Combine** | Courier services, toll road access |
| **Rangers Guild** | Guide hire, route information |
| **Rust-Clans** | Rig modifications, salvage trade |
| **Dvergr Hegemony** | Pure Steel equipment, Deep Gate access |
| **Scavenger Barons** | Cold-weather gear, Dreadnought passage |
| **Hearth-Clans** | Thermal equipment, heat resistance |
| **Jötun-Readers** | Language services, lore access |

### 8.3 Multi-Faction Settlements

Large settlements may have multiple faction presences:

```
CROSSROADS HOLD (Midgard Combine + Rangers Guild + Dvergr Trade Mission)

Gate District [Combine Control]:
├── Combine Gate - Combine guards, toll collection
└── Watch Barracks - Combine bounties

Market District [Mixed Control]:
├── Crossroads Plaza - Neutral ground
├── Dvergr Imports - Dvergr tariffs apply
└── Rangers Post - Guild services

Residential District [Combine Control]:
└── Standard Combine governance
```

---

## 9. Settlement Generation

### 9.1 Generation Algorithm

```
┌─────────────────────────────────────────────────────────────┐
│ INPUT: Settlement Size + Faction + Biome + Seed             │
└─────────────────────────────────────────────────────────────┘
         │
         ▼
┌─────────────────────────────────────────────────────────────┐
│ PHASE 1: District Layout                                    │
│   • Determine district count by size                        │
│   • Assign district functions                               │
│   • Generate district-level connection graph                │
└─────────────────────────────────────────────────────────────┘
         │
         ▼
┌─────────────────────────────────────────────────────────────┐
│ PHASE 2: Room Placement                                     │
│   • Place required archetypes per district                  │
│   • Add optional rooms by budget                            │
│   • Apply biome theming to room names/descriptions          │
└─────────────────────────────────────────────────────────────┘
         │
         ▼
┌─────────────────────────────────────────────────────────────┐
│ PHASE 3: NPC Assignment                                     │
│   • Generate NPCs by archetype requirements                 │
│   • Assign faction-appropriate NPCs                         │
│   • Set schedules and behaviors                             │
└─────────────────────────────────────────────────────────────┘
         │
         ▼
┌─────────────────────────────────────────────────────────────┐
│ PHASE 4: Connection Graph                                   │
│   • Connect rooms within districts                          │
│   • Connect districts via transition rooms (Plazas)         │
│   • Validate all rooms reachable from Gate                  │
└─────────────────────────────────────────────────────────────┘
         │
         ▼
┌─────────────────────────────────────────────────────────────┐
│ PHASE 5: Service Population                                 │
│   • Assign merchant inventories                             │
│   • Set service availability                                │
│   • Apply faction modifiers                                 │
└─────────────────────────────────────────────────────────────┘
         │
         ▼
┌─────────────────────────────────────────────────────────────┐
│ OUTPUT: Settlement (rooms + NPCs + services + connections)  │
└─────────────────────────────────────────────────────────────┘
```

### 9.2 Room Budget by Size

| Settlement Size | Room Budget | Distribution |
|-----------------|-------------|--------------|
| **Outpost** | 3-5 | 1 Gate, 1-2 Service, 1-2 Residence |
| **Village** | 6-10 | 1 Gate, 1 Plaza, 2-4 Service, 2-4 Residence |
| **Town** | 11-20 | 1-2 Gate, 1-2 Plaza, 4-8 Service, 5-8 Residence |
| **City** | 21-40 | 2-3 Gate, 3-5 Plaza, 8-15 Service, 8-15 Residence |
| **Capital** | 40+ | 3+ Gate, 5+ Plaza, 15+ Service, 15+ Residence |

### 9.3 Biome-Specific Theming

Settlement rooms receive biome-appropriate descriptors:

| Biome | Gate Variation | Plaza Variation | Tavern Variation |
|-------|----------------|-----------------|------------------|
| **Midgard** | Palisade Gate | Market Square | Mead Hall |
| **Jötunheim** | Rust Gate | Salvage Yard | Scrap-Heap Bar |
| **Niflheim** | Ice Gate | Frost Plaza | Warming Hall |
| **Muspelheim** | Ash Gate | Ember Plaza | Dew-Chamber |
| **Vanaheim** | Branch Gate | Canopy Platform | Vine-Wrap Inn |
| **Svartalfheim** | Stone Gate | Traders' Court | Guild House |

---

## 10. Data Schema

### 10.1 Settlement Table

```sql
CREATE TABLE Settlements (
    settlement_id TEXT PRIMARY KEY,
    name TEXT NOT NULL,
    settlement_size TEXT NOT NULL,  -- Outpost/Village/Town/City/Capital
    controlling_faction TEXT NOT NULL,
    biome_id TEXT NOT NULL,
    coord_x INTEGER,
    coord_y INTEGER,
    coord_z INTEGER,
    is_safe_zone BOOLEAN DEFAULT 1,
    fast_travel_enabled BOOLEAN DEFAULT 0,
    description TEXT,

    CHECK (settlement_size IN ('Outpost','Village','Town','City','Capital'))
);

CREATE INDEX idx_settlements_faction ON Settlements(controlling_faction);
CREATE INDEX idx_settlements_biome ON Settlements(biome_id);
CREATE INDEX idx_settlements_position ON Settlements(coord_x, coord_y, coord_z);
```

### 10.2 Settlement Rooms Table

```sql
CREATE TABLE Settlement_Rooms (
    room_id TEXT PRIMARY KEY,
    settlement_id TEXT NOT NULL,
    district_id TEXT,
    archetype TEXT NOT NULL,
    name TEXT NOT NULL,
    description TEXT,
    exits TEXT,  -- JSON array of connected room_ids
    services TEXT,  -- JSON array of available services

    FOREIGN KEY (settlement_id) REFERENCES Settlements(settlement_id),
    CHECK (archetype IN ('SettlementGate','Plaza','MerchantStall','CraftingStation','Tavern','Barracks','Temple','Residence','Archive'))
);

CREATE INDEX idx_settlement_rooms_settlement ON Settlement_Rooms(settlement_id);
CREATE INDEX idx_settlement_rooms_district ON Settlement_Rooms(district_id);
CREATE INDEX idx_settlement_rooms_archetype ON Settlement_Rooms(archetype);
```

### 10.3 Settlement NPCs Table

```sql
CREATE TABLE Settlement_NPCs (
    npc_id TEXT PRIMARY KEY,
    settlement_id TEXT NOT NULL,
    room_id TEXT NOT NULL,
    name TEXT NOT NULL,
    role TEXT NOT NULL,  -- Merchant/Craftsman/QuestGiver/ServiceProvider/Background/Companion
    faction_id TEXT,
    schedule TEXT,  -- JSON schedule object
    inventory_id TEXT,  -- For merchants
    dialogue_tree_id TEXT,

    FOREIGN KEY (settlement_id) REFERENCES Settlements(settlement_id),
    FOREIGN KEY (room_id) REFERENCES Settlement_Rooms(room_id)
);

CREATE INDEX idx_settlement_npcs_settlement ON Settlement_NPCs(settlement_id);
CREATE INDEX idx_settlement_npcs_room ON Settlement_NPCs(room_id);
CREATE INDEX idx_settlement_npcs_role ON Settlement_NPCs(role);
```

---

## 11. Example Settlements

### 11.1 Crossroads Hold (Town, Midgard)

```
CROSSROADS HOLD - Town (15 rooms)
Controlling Faction: Midgard Combine
Secondary Presence: Rangers Guild, Dvergr Trade Mission

Gate District (4 rooms):
├── Combine Gate [SettlementGate]
│   • Entry/exit point
│   • Toll collection (Combine roads)
│   • Fast travel waypoint
│
├── Watch Barracks [Barracks]
│   • Combine bounties
│   • Guard contracts
│
├── Rangers Post [Residence]
│   • Rangers Guild liaison
│   • Guide services
│   • Route information
│
└── Stable [CraftingStation]
    • Mount services
    • Caravan maintenance

Market District (6 rooms):
├── Crossroads Plaza [Plaza]
│   • Central hub
│   • Notice board
│   • Quest NPCs
│
├── Kjartan's General [MerchantStall]
│   • General goods
│   • Consumables, tools
│
├── Dvergr Imports [MerchantStall]
│   • Tools, Pure Steel items
│   • Dvergr tariffs apply
│
├── Apothecary [MerchantStall]
│   • Potions, medical supplies
│   • Reagents
│
├── Smithy [CraftingStation]
│   • Repair services
│   • Weapon modifications
│
└── Archive [Archive]
    • Jötun-Reader presence
    • Map updates
    • Lore access

Residential District (5 rooms):
├── Jarl's Hall [Residence]
│   • Governance
│   • Main quest hooks
│
├── Temple of the Coherent [Temple]
│   • Healing services
│   • Corruption removal
│
├── Wanderer's Rest [Tavern]
│   • Full rest
│   • Rumors
│   • Companion recruitment
│
├── Elder's Home [Residence]
│   • Lore NPC
│   • Side quests
│
└── Worker Housing [Residence]
    • Background NPCs
```

### 11.2 Glimmerheim Outpost (Outpost, Muspelheim)

```
GLIMMERHEIM OUTPOST - Outpost (4 rooms)
Controlling Faction: Hearth-Clans

├── Ash Gate [SettlementGate]
│   • Entry/exit
│   • Thermal gear check
│
├── Ember Stall [MerchantStall]
│   • Heat resistance supplies
│   • Trade goods
│
├── Dew-Chamber [Tavern]
│   • Cooled rest area
│   • Hearth-Clan rumors
│
└── Pathfinder's Quarters [Residence]
    • Guide services
    • Route to deeper Muspelheim
```

### 11.3 Nidavellir (Capital, Svartalfheim)

```
NIDAVELLIR - Capital (60+ rooms)
Controlling Faction: Dvergr Hegemony

[Partial Layout - Key Districts]

Gate District:
├── Deep Gate Prime [SettlementGate]
│   • Primary entry from surface
│   • Tariff enforcement
│
├── Traders' Checkpoint [SettlementGate]
│   • Trade approval processing
│
└── Customs Barracks [Barracks]
    • Guild Guard presence

Traders' Concourse:
├── Grand Concourse [Plaza]
│   • Primary trade hub
│   • Largest market in Aethelgard
│
├── [20+ MerchantStalls]
│   • All categories represented
│   • Pure Steel specialists
│
└── Trade Archive [Archive]
    • Contract records
    • Pricing information

Guild Quarter:
├── Forge-Masters Hall [CraftingStation]
│   • Master-level crafting
│   • Unique Dvergr techniques
│
├── Artisans' Row [CraftingStations x5]
│   • Specialty crafting
│
└── Guild Barracks [Barracks]
    • Dvergr contracts
    • Faction advancement

[Additional districts: Residential, Administrative, Deep Access...]
```

---

## 12. Integration Points

### 12.1 Room Engine Integration

Settlements require extensions to the room engine:

```csharp
public enum SectorType
{
    Dungeon,
    Settlement
}

public interface ISettlementGenerator
{
    Settlement Generate(SettlementSize size, string factionId, string biomeId, int seed);
    Settlement LoadHandcrafted(string settlementId);
}
```

**Changes to Room Engine:**
- Add `SectorType` property to sectors
- Skip encounter generation for `SectorType.Settlement`
- Add settlement room archetypes to template system

### 12.2 Navigation Integration

Settlements integrate with the navigation system:

- Settlements appear as waypoints on world map
- Fast travel enabled between discovered settlements
- Settlement entry triggers sector transition
- Settlement gates connect to inter-realm routes

### 12.3 Time System Integration

Settlements track time for:
- Merchant restocking (see individual merchant refresh rates)
- NPC schedules (day/night behavior)
- Quest timers
- Event progression

---

## 13. Phased Implementation Guide

### Phase 1: Data Schema
- [ ] **DB**: Create Settlement, Settlement_Rooms, Settlement_NPCs tables
- [ ] **Enums**: Define SettlementSize, SettlementArchetype enums
- [ ] **DTOs**: Define Settlement, SettlementRoom, SettlementNpc records

### Phase 2: Core Generation
- [ ] **Service**: Implement ISettlementGenerator interface
- [ ] **Algorithm**: Implement procedural settlement generation
- [ ] **Templates**: Create settlement room templates (JSON)

### Phase 3: NPC Integration
- [ ] **NPCs**: Extend NPC system with settlement assignments
- [ ] **Services**: Implement service mechanics (commerce, crafting, rest)
- [ ] **Reputation**: Wire faction reputation to service gating

### Phase 4: Navigation Integration
- [ ] **Waypoints**: Add settlement waypoints to world map
- [ ] **Fast Travel**: Implement fast travel between settlements
- [ ] **Transitions**: Handle sector transitions (dungeon ↔ settlement)

### Phase 5: Content
- [ ] **Handcraft**: Create major settlements (Utgard, Nidavellir, etc.)
- [ ] **NPCs**: Populate settlements with named NPCs
- [ ] **Quests**: Wire quest hooks to settlement NPCs

---

## 14. Testing Requirements

### 14.1 Unit Tests
- [ ] **Generation**: Settlement of size X produces X-Y rooms
- [ ] **Districts**: Town+ settlements have required districts
- [ ] **Connectivity**: All rooms reachable from gate
- [ ] **Services**: Correct services available per size

### 14.2 Integration Tests
- [ ] **Navigation**: Can travel from dungeon → settlement → dungeon
- [ ] **Fast Travel**: Discovered settlements enable fast travel
- [ ] **Commerce**: Buy/sell transactions complete correctly
- [ ] **Reputation**: Service gating enforced by reputation

### 14.3 Manual QA
- [ ] **Experience**: Settlement navigation feels coherent
- [ ] **Services**: All services accessible and functional
- [ ] **NPCs**: NPC dialogue and behavior appropriate

---

## 15. Related Documentation

| Document | Purpose |
|----------|---------|
| [Room Engine Core](room-engine/core.md) | Base room generation system |
| [Spatial Layout](room-engine/spatial-layout.md) | 3D coordinate system |
| [Inter-Realm Navigation](navigation.md) | World-scale travel |
| [Faction Reputation](../02-entities/faction-reputation.md) | Reputation mechanics |
| [NPC Overview](../02-entities/npcs/npc-overview.md) | NPC systems |
| [Biome Overview](biomes/biomes-overview.md) | Realm theming |

---

## 16. Changelog

| Version | Date | Changes |
|---------|------|---------|
| 1.0 | 2025-12-14 | Initial specification |
