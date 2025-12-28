# Milestone 6 Roadmap: The Architect (v0.5.x)

> **Status:** Planning
> **Version Range:** v0.5.0 - v0.5.20
> **Theme:** Settlements, Economy, and The World

## Overview
Milestone 6 expands the game from a dungeon crawler into a world simulator. "The Architect" introduces civilized spaces (Settlements), complex economic systems, and the ability to travel across the surface world to reach new dungeons.

This cycle is significantly expanded to **v0.5.20** to accommodate the depth of the simulation required for a living world.

---

## Part I: The Foundation (Settlements & Economy)
**Focus:** Creating the physical spaces of civilization and the systems that power them.

### [v0.5.0] The Settlement Engine
*Defining the safe zones.*
- **v0.5.0a: The City Plan (Generation)**
  - `SettlementGenerator` for creating Districts (Slums, Market, Keep).
  - Population density and atmosphere descriptors.
- **v0.5.0b: The Architecture (Navigation)**
  - Local movement within settlements (Street -> Shop).
  - `Enter` / `Exit` logic for buildings.
- **v0.5.0c: The Citizen (NPCs)**
  - Basic NPC entities with Roles (Guard, Merchant, Citizen).
  - Bark triggers for passing players.

### [v0.5.1] The Marketplace
*The flow of goods.*
- **v0.5.1a: The Currency (Scrip)**
  - Implementation of `Scrip` as a centralized currency.
  - Wallet UI updates.
- **v0.5.1b: The Merchant (Trading)**
  - `TradeService` for buying/selling.
  - Stock generation based on Settlement Tier.
- **v0.5.1c: The Haggle (Barter)**
  - Social checks to influence prices.
  - "Hot Items" logic (dynamic demand).

### [v0.5.2] Services & Guilds
*Utility beyond buying items.*
- **v0.5.2a: The Inn (Rest)**
  - Rentable rooms for safe saving/resting.
  - Tavern rumor generation.
- **v0.5.2b: The Healer (Recovery)**
  - `Hospital` service for removing Trauma/Corruption (for a price).
  - Cybernetic repair stations.
- **v0.5.2c: The Guild (Work)**
  - `BountyBoard` for procedural kill quests.
  - Delivery contracts.

### [v0.5.3] The Workshop
*Industrial capacity.*
- **v0.5.3a: The Commission (Orders)**
  - Requesting specific item crafts from NPCs.
  - Wait-time mechanics (pick up later).
- **v0.5.3b: The Scrapper (Recycling)**
  - Bulk selling of junk for raw materials.
  - Dynamic material pricing.

### [v0.5.4] The Law
*Consequences of action.*
- **v0.5.4a: The Crime (Theft)**
  - Stealing mechanics and detection logic.
  - "Stolen" flag on items.
- **v0.5.4b: The Jail (Punishment)**
  - Arrest logic and fines.
  - Reputation hits for criminal acts.

---

## Part II: The Society (Politics & Factions)
**Focus:** The invisible web of influence that connects the world.

### [v0.5.5] Dynamic Population
*Making the world feel alive.*
- **v0.5.5a: The Schedule (Day/Night)**
  - NPC daily routines (Home -> Work -> Tavern).
  - Shop opening hours.
- **v0.5.5b: The Crowd (Atmosphere)**
  - Procedural crowd density based on time.
  - Event triggers (brawls, sermons).

### [v0.5.6] Faction Influence
*Territory and power.*
- **v0.5.6a: The Control (Territory)**
  - District ownership visualization.
  - Passive bonuses based on ruling faction.
- **v0.5.6b: The Conflict (Events)**
  - Dynamic faction skirmishes in streets.
  - Propaganda posters and bark changes.

### [v0.5.7] Advanced Reputation
*Who you know matters.*
- **v0.5.7a: The Tier (Status)**
  - Perks for High Reputation (discounts, access).
  - Penalties for Low Reputation (refusal of service).
- **v0.5.7b: The Favor (Currency)**
  - Spending Reputation for one-time favors (bail, unique items).

### [v0.5.8] Information Economy
*Knowledge is power.*
- **v0.5.8a: The Rumor (System)**
  - `RumorService` tracking active world events.
  - Buying/Selling secrets at Taverns.
- **v0.5.8b: The Codex (Integration)**
  - Unlocking Codex entries via conversation.
  - Library districts for research.

### [v0.5.9] Political Events
*Shifting sands.*
- **v0.5.9a: The Election (Change)**
  - Periodic leadership changes affecting taxes/prices.
  - Player influence on outcomes.
- **v0.5.9b: The Crisis (Shortages)**
  - Procedural shortages (e.g., "Iron Famine") spiking prices.

---

## Part III: The Horizon (World Map & Travel)
**Focus:** Connecting the isolated points of light.

### [v0.5.10] The World Map
*The cartographer's canvas.*
- **v0.5.10a: The Atlas (Data)**
  - Hex-grid world data structure.
  - Biome definition per hex.
- **v0.5.10b: The View (UI)**
  - ASCII World Map interface.
  - Fog of War on the macro scale.

### [v0.5.11] The Journey
*Travel mechanics.*
- **v0.5.11a: The Supplies (Rations)**
  - Travel cost calculation (Food/Water/Fuel).
  - Starvation/Dehydration risk on road.
- **v0.5.11b: The Pace (Movement)**
  - Travel stances (Forced March, Cautious, Scout).
  - Fatigue accumulation.

### [v0.5.12] Road Encounters
*The danger in between.*
- **v0.5.12a: The Ambush (Combat)**
  - Procedural combat encounters based on biome.
  - "Roadside" battlemap generation.
- **v0.5.12b: The Discovery (POI)**
  - Finding hidden ruins or stashes.
  - Mini-dungeon generation.

### [v0.5.13] Trade Routes
*The lifeblood of commerce.*
- **v0.5.13a: The Caravan (Entity)**
  - NPC caravans moving between settlements.
  - Escort missions.
- **v0.5.13b: The Market Link (Economy)**
  - Prices affecting neighboring settlements.
  - Intercepting enemy supply lines.

### [v0.5.14] The Frontier
*Establishing a foothold.*
- **v0.5.14a: The Camp (Mobile Base)**
  - Pitching tents for wilderness survival.
  - Camouflage and defense ratings.
- **v0.5.14b: The Outpost (Upgrade)**
  - Upgrading a campsite to a permanent structure.
  - Stashing loot in the wild.

---

## Part IV: The Deepening (Expansion & Polish)
**Focus:** Adding depth to the simulation and refining the player experience.

### [v0.5.15] The Festival
*Seasonal flavor.*
- **v0.5.15a: The Calendar (Time)**
  - Tracking months/seasons.
  - Seasonal weather effects on travel.
- **v0.5.15b: The Celebration (Event)**
  - Recurring festival events (e.g., "The Forging Day") with unique vendors.
  - Minigames (Dice, Drinking).

### [v0.5.16] The Underworld
*The illicit economy.*
- **v0.5.16a: The Black Market (Shop)**
  - Hidden vendors selling illegal goods.
  - Password access logic.
- **v0.5.16b: The Smuggler (Mission)**
  - Transporting contraband between settlements.
  - Avoiding Guard patrols.

### [v0.5.17] The Diplomat
*Advanced relations.*
- **v0.5.17a: The Alliance (State)**
  - Brokerage of peace between factions.
  - Joint operations.
- **v0.5.17b: The Betrayal (Consequence)**
  - Consequences for breaking pacts.
  - Assassin squads sent after player.

### [v0.5.18] The Census
*Simulation polish.*
- **v0.5.18a: The Name (Generator)**
  - Expanded name lists per culture/faction.
  - Family lineage tracking for key NPCs.
- **v0.5.18b: The Memory (Persistence)**
  - NPCs remembering past interactions with player.
  - Dynamic greeting changes.

### [v0.5.19] The Vault
*Banking and storage.*
- **v0.5.19a: The Bank (Service)**
  - Depositing scrip to avoid loss on death.
  - Loans and interest rates.
- **v0.5.19b: The Warehouse (Storage)**
  - Renting bulk storage space in settlements.
  - Cross-settlement transfer services.

### [v0.5.20] The Foundation II
*Preparing for war.*
- **v0.5.20a: The Wall (Defense)**
  - Settlement defense ratings affecting safety.
  - Siege events (preview of v0.6.x combat).
- **v0.5.20b: The Golden State (Finalize)**
  - Final balance pass on economy (prices/wages).
  - Integration tests for full Settlement loop.
