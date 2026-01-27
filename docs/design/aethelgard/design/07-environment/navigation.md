---
id: SPEC-ENV-NAVIGATION
title: "Inter-Realm Navigation — Routes, Travel, and Transit"
version: 1.0
status: design-complete
last-updated: 2025-12-14
related-files:
  - path: "docs/07-environment/biomes/biomes-overview.md"
    status: Active
  - path: "docs/07-environment/the-deep.md"
    status: Active
  - path: "docs/07-environment/room-engine/spatial-layout.md"
    status: Active
---

# Inter-Realm Navigation — Routes, Travel, and Transit

> *"The world is not nine separate places—it is one vast machine with nine floors. The question is never 'can I get there?' but 'what will the journey cost me?'"*
> — Senior Ranger, Midgard Rangers Guild

---

## 1. Overview

Aethelgard is a **planetary-scale megastructure** with continent-sized realms connected by surface routes, subterranean tunnels, and vertical transit systems. Navigation operates at three distinct scales:

| Scale | Scope | Primary Specification |
|-------|-------|----------------------|
| **Dungeon-Scale** | Room-to-room (10-300m) | [Spatial Layout](room-engine/spatial-layout.md) |
| **Realm-Scale** | Intra-biome travel (1-50 km/day) | This document + individual biome specs |
| **Inter-Realm** | Cross-biome transit (multi-day journeys) | This document |

> [!IMPORTANT]
> **The Nine Realms are connected, not isolated.** Players traverse an entire planet—surface roads, underground corridors, vertical lifts, and ferry routes form a complex transit network. This is not a dungeon crawler; this is continental exploration with dungeon delving.

---

## 2. Route Classification System

All travel routes in Aethelgard use a standardized **Class A-D** rating system based on maintenance, hazard level, and infrastructure:

### 2.1 Classification Definitions

| Class | Name | Speed | Characteristics | Typical Infrastructure |
|-------|------|-------|-----------------|----------------------|
| **A** | Trunk Routes | 25–35 km/day | Maintained, patrolled, lit | Toll roads, Guild corridors, Deep Gates |
| **B** | Secondary Routes | 15–25 km/day | Partially maintained, hazards present | Long roads, trade routes, ferry lines |
| **C** | Frontier Routes | 8–15 km/day | Unmaintained, significant hazards | Wilderness tracks, unmapped passages |
| **D** | Expedition Routes | 3–10 km/day | No infrastructure, extreme hazards | Expedition paths, hostile territory |

### 2.2 Speed Calculation

Base travel speed is modified by:

| Factor | Modifier |
|--------|----------|
| **Weather Conditions** | ×0.5 to ×1.0 (storms, whiteouts, Elding-Storm) |
| **Environmental Hazards** | ×0.6 to ×0.9 (toxic air, extreme temperature) |
| **Party Composition** | ×0.8 (wounded), ×0.7 (heavy cargo) |
| **Local Knowledge** | ×1.2 (guide), ×1.0 (map), ×0.8 (unknown) |
| **Vehicle/Mount** | ×1.5 to ×2.0 (Rig, Sky Loom, Dreadnought) |

```
Effective Speed = Base Speed × Weather × Hazards × Composition × Knowledge × Vehicle
```

### 2.3 Route Degradation

Routes can shift classification based on conditions:

| Event | Effect | Duration |
|-------|--------|----------|
| **Seasonal Change** | ±1 Class (flooding, freeze-thaw) | Seasonal |
| **Major Storm** | -1 Class (infrastructure damage) | 1-4 weeks |
| **Faction Conflict** | -1 to -2 Class (patrols withdrawn) | Conflict duration |
| **Bloom Cycle (Vanaheim)** | -2 Class (route closure) | 30-45 minutes |
| **Elding-Storm (Muspelheim)** | -3 Class (lethal) | 2-6 hours |

---

## 3. Realm-Specific Navigation

Each realm has unique navigation characteristics based on its environment and infrastructure:

### 3.1 Midgard (Surface Realm)

The most accessible realm with the best-maintained surface routes.

| Class | Type | Speed | Requirements |
|-------|------|-------|--------------|
| **A** | Combine Toll Roads | 25-35 km/day | Maintained, patrolled, courier seals honored |
| **B** | Long Roads | 15-25 km/day | Waystone navigation, watch-fire relay dependent |
| **C** | Local Tracks | 10-18 km/day | Ferry crossings, corduroy spans, seasonal |
| **D** | Expedition Paths | 5-12 km/day | Scar marches, quiet-window relays only |

**Key Infrastructure:**
- Rangers Guild maintains Long Roads and watch-fire relays
- Ferry Guild operates river crossings
- Combine Toll Roads connect major Holds

**Special Conditions:**
- Night travel requires active watch-fire relays
- Seasonal flooding affects Mire and Fjord crossings
- Asgardian Scar requires safe-conduct writs

### 3.2 Jötunheim (Industrial Graveyard)

Vast scale creates unique navigation challenges among the fallen giants.

| Class | Type | Speed | Requirements |
|-------|------|-------|--------------|
| **A-B** | Debris corridors to Utgard Gates | 8–12 km/day | Convoy-capable with caution; armed escort |
| **C** | Ash-Blown Wastes transit | 15–20 km/day | Open exposure; Rust-Devil risk; Rig recommended |
| **D** | Rust-Flow channels | 5–8 km/day | Covered but Conveyor-Serpent territory |

**Key Infrastructure:**
- Utgard Gates connect to Midgard
- Rig convoys follow seasonal salvage routes
- Rust-Clan territory markers

**Special Conditions:**
- Giant-scale terrain requires specialized equipment
- Ash storms reduce visibility to near-zero
- Conveyor-Serpent ambushes in covered routes

### 3.3 Niflheim (Frozen Tomb)

Extreme cold and mobile Dreadnought routes define navigation.

| Class | Type | Speed | Requirements |
|-------|------|-------|--------------|
| **B** | Dreadnought Corridors | 8–12 km/day | Mobile routes behind fortress-Rigs; safest option |
| **C** | Ridge Spine Routes | 5–8 km/day | Great Glacier traversal; exposed to whiteouts |
| **D** | Chasm-Field Passages | 2–4 km/day | Crevasse navigation; probe surveys mandatory |

**Key Infrastructure:**
- Scavenger Baron Dreadnoughts provide mobile safe zones
- Ridge Routes connect to Midgard via alpine passes
- Jötun's Fall provides the only permanent settlement

**Special Conditions:**
- [Bitter Cold] ambient condition; STURDINESS checks every 4 hours
- Ice-Debt labor contracts for Dreadnought passage
- Crevasse fields require probe-and-advance protocol

### 3.4 Muspelheim (Eternal Meltdown)

The most hostile surface navigation environment.

| Class | Type | Speed | Requirements |
|-------|------|-------|--------------|
| **C** | Hearth-to-Hearth corridors | 8-12 km/day | Thermal protection; mapped paths; still extremely dangerous |
| **D** | Slag Wastes traversal | 3-6 km/day | Emergency only; no shelter; thermal exposure; probe protocol |
| **—** | Forge Core approach | — | Lethal; Surtr AI active defense |

**Key Infrastructure:**
- Hearth-Clan settlements as safe havens (Dew-Chambers)
- Glimmerheim as primary gateway
- Trade route to Svartalfheim (geothermal link)

**Special Conditions:**
- [Intense Heat] ambient condition; STURDINESS checks every 2 hours
- No Class A or B routes exist
- Elding-Storm events can trap parties for hours
- Slag-Crust Protocol for Gjöllflow crossings

### 3.5 Vanaheim (Verdant Hell)

Vertical stratification creates unique navigation paradigm.

| Class | Type | Speed | Requirements |
|-------|------|-------|--------------|
| **B** | Canopy Sea Skyways | 6–10 km/day | Sky Looms Guild certification; predator countermeasures |
| **C** | Gloom-Veil passages | 3–6 km/day | Warden escort; Golden Plague monitoring |
| **D** | Under-growth penetration | <1 km/day | Maximum contamination protocols; Unraveled sovereignty respect |

**Key Infrastructure:**
- Sky Looms ferries connect Canopy Sea settlements (0.24% incident rate)
- Emergency shelters every 400-600m in Gloom-Veil
- Rope bridges between platform settlements

**Special Conditions:**
- Vertical navigation (elevation = safety)
- Golden Plague contamination monitoring
- Bloom Cycle events force immediate evacuation
- Unraveled territorial sovereignty must be honored

### 3.6 Svartalfheim (Kingdom of Controlled Light)

Best-maintained subterranean infrastructure in Aethelgard.

| Class | Type | Speed | Requirements |
|-------|------|-------|--------------|
| **A** | Guild Corridors | 20-30 km/day | Fully lit; patrolled; safe within Guild-Lands |
| **B** | Trade Routes | 15-20 km/day | Maintained; tariff checkpoints; approved traders |
| **C** | Black Veins Fringe | 5-10 km/day | Unlit; dangerous; unauthorized entry prohibited |
| **—** | Whispering Chasm | — | Silent Folk territory; extreme lethality |

**Key Infrastructure:**
- Deep Gates connect Nidavellir to surface trade hubs
- Traders' Concourse for external access
- Guild patrol networks

**Special Conditions:**
- Light discipline (illumination = civilization)
- Tariff compliance enforced
- Acoustic predators in Black Veins
- Silent Folk territory marked by click-patterns

### 3.7 Alfheim (Supercomputer of Madness)

Cognitive hazards dominate navigation concerns.

| Class | Type | Speed | Requirements |
|-------|------|-------|--------------|
| **B** | Heuristic aisles | 12-20 km/day (green), 8-15 km/day (amber) | Breadcrumb 15-20m, CPS ≤0.8 gate |
| **C** | Choir bridges | 8-15 km/day | Null-band windows, Hz validation |
| **D** | Sanctuary hops | 3-8 km/day | Occlusion veils, node spacing 400-600m |

**Key Infrastructure:**
- Dead-Light Sanctuary network (40-60 nodes)
- Fractal Woods heuristic lattices
- Dökkálfar guide services

**Special Conditions:**
- The Glimmer causes progressive cognitive degradation
- Non-Euclidean geometry in Fractal Woods
- Choir crescendos shrink null-band windows
- Dökkálfar guides provide 40-60% risk reduction

### 3.8 Asgard (Shattered Spire)

The most restricted realm; orbital access only.

| Class | Type | Speed | Requirements |
|-------|------|-------|--------------|
| **C** | Crown approaches | 8-15 km/day | Kvasir coverage, safe-conduct writ |
| **D** | Interior corridors | 3-8 km/day | Quiet Walk protocol, cognitive windows |
| **—** | Exclusion Zone | — | Lethal; Heimdallr Signal; no confirmed returns |

**Key Infrastructure:**
- Scar Rim Lifts connect to Midgard (3-5 days B-class)
- Scriptorium safe-conduct writs required
- Kvasir-Pattern Logic Engines mandatory for Red Sector

**Special Conditions:**
- Zero gravity in breached sections
- Vacuum exposure risk
- Genius Loci attention cycles (5-7 minute windows)
- Type Omega CPS in Exclusion Zone

---

## 4. Inter-Realm Connections

### 4.1 Major Transit Routes

| Route | From | To | Travel Time | Class | Notes |
|-------|------|----|----|-------|-------|
| **Utgard Gates** | Midgard | Jötunheim | 5-8 days | B with C feeders | Grey Desk remedies available |
| **Ridge Routes** | Midgard | Niflheim | 7-12 days | B with winter D options | Alpine passes; seasonal |
| **Up-River Ferries** | Midgard | Vanaheim | 6-9 days | B/C with flood detours | Ferry Guild operated |
| **Scar Rim Lifts** | Midgard | Asgard | 3-5 days | B-class last-mile | Safe-conduct writ required |
| **Trade Corridor** | Svartalfheim | Jötunheim | 6-8 days | A/B | Adamant-Weave trade |
| **Geothermal Link** | Svartalfheim | Muspelheim | 4-6 days | B/C | Hearth-Clan trade |
| **Deep Gates** | Svartalfheim | Multiple | Variable | A | Dvergr-controlled; tariffs apply |
| **Dreadnought Corridors** | Niflheim | Jötunheim | 4-7 days | B | Mobile route; following Rigs |

### 4.2 Connection Matrix

```
                    INTER-REALM CONNECTIONS

             Mid  Jöt  Nif  Mus  Van  Sva  Alf  Asg
Midgard       —   5-8  7-12  —   6-9   —   *   3-5
Jötunheim   5-8    —   4-7   —    —   6-8   —    —
Niflheim   7-12  4-7    —    —    —    —    —    —
Muspelheim   —    —     —    —    —   4-6   —    —
Vanaheim    6-9   —     —    —    —    —   **   —
Svartalfheim —   6-8    —   4-6   —    —    —    —
Alfheim      *    —     —    —   **    —    —    —
Asgard      3-5   —     —    —    —    —    —    —

Legend: Numbers = travel time in days
        — = No direct connection
        * = Blight contamination gradient (Scar approach)
        ** = Conceptual parallel (no physical route)
```

### 4.3 Transit Infrastructure Types

| Type | Description | Realms | Capacity |
|------|-------------|--------|----------|
| **Surface Roads** | Maintained paths, toll roads | Midgard, Jötunheim | High |
| **Mountain Passes** | Ridge routes, alpine trails | Midgard↔Niflheim | Low |
| **River Ferries** | Guild-operated water transit | Midgard↔Vanaheim | Medium |
| **Deep Gates** | Subterranean high-capacity transit | Svartalfheim hub | High |
| **Geothermal Corridors** | Heat-resistant tunnels | Svartalfheim↔Muspelheim | Low |
| **Orbital Lifts** | Tensile spire remnants | Midgard↔Asgard | Very Low |
| **Sky Looms** | Aerial cable ferries | Vanaheim internal | Medium |
| **Dreadnought Corridors** | Mobile fortress routes | Niflheim, Jötunheim | Medium |

---

## 5. The Deep Transit Network

The Deep provides an alternative to surface travel—subterranean corridors that once connected all realms.

### 5.1 Deep Navigation

See [The Deep](the-deep.md) for comprehensive depth stratification.

| Depth | Class | Speed | Availability |
|-------|-------|-------|--------------|
| **Surface Interface** (0-50m) | Variable | By surface class | All realms |
| **Shallow Deep** (50-500m) | B-C | 8-20 km/day | Partial |
| **True Deep** (500-2000m) | C-D | 3-10 km/day | Svartalfheim only |
| **Abyssal Zones** (>2000m) | D or worse | <3 km/day | Expedition only |

### 5.2 Deep Gate Network

| Gate | Location | Connects To | Status |
|------|----------|-------------|--------|
| **Nidavellir Prime** | Svartalfheim capital | Multiple surface hubs | Functional |
| **Utgard Underpass** | Jötunheim | Midgard industrial zone | Partial |
| **Helheim Sump Access** | Helheim | Svartalfheim waste flow | Hazardous |
| **Ridge Vault Gates** | Niflheim | Cryo-preservation network | Frozen |
| **Root Terminus** | Vanaheim | Under-growth access | Overgrown |

---

## 6. Navigation Mechanics

### 6.1 Travel Checks

Long-distance travel requires periodic skill checks:

| Check Type | Trigger | Skill | DC |
|------------|---------|-------|-----|
| **Orientation** | Every 4 hours (C/D routes) | Wasteland Survival | 10-15 |
| **Hazard Avoidance** | Environmental encounter | Acrobatics/Awareness | Variable |
| **Weather Endurance** | Adverse conditions | STURDINESS | By environment |
| **Social Navigation** | Faction checkpoints | Influence/Deception | By faction |

### 6.2 Getting Lost

Failure on orientation checks results in:

| Margin | Effect |
|--------|--------|
| 1-3 | Minor detour: +2 hours travel time |
| 4-6 | Significant detour: +4-8 hours, hazard encounter |
| 7+ | Lost: Must backtrack or make camp; survival situation |

### 6.3 Forced March

Parties can attempt to exceed normal travel speed:

| Intensity | Speed Bonus | Cost |
|-----------|-------------|------|
| **Hustle** | +25% speed | -1 Stamina/hour |
| **Forced March** | +50% speed | -2 Stamina/hour, Exhaustion risk |
| **Sprint** | +100% speed | Combat encounter pace; 10 min max |

---

## 7. Navigation Tools & Services

### 7.1 Equipment

| Tool | Function | Limitation |
|------|----------|------------|
| **Compass** | Directional reference | Fails near Aetheric concentrations |
| **Maps** | Route knowledge (+1 Class effective) | Accuracy varies; may be outdated |
| **Waystone Markers** | Long Road navigation | Midgard only; depend on maintenance |
| **Echo-Mapper** | Acoustic surveying (Deep) | Attracts sound-hunting predators |
| **Kvasir Engine** | CPS protection (Asgard/Alfheim) | Dvergr-made; expensive |
| **Thermal Suit** | Heat/cold protection | Duration limited; maintenance required |

### 7.2 Guide Services

| Service | Availability | Cost | Benefit |
|---------|--------------|------|---------|
| **Rangers Guild Scout** | Midgard | Variable | +1 Class effective; local knowledge |
| **Dvergr Guide** | Svartalfheim | Expensive | Deep navigation; tariff handling |
| **Dökkálfar Guide** | Alfheim | Negotiated | 40-60% risk reduction; sanctuary access |
| **Hearth-Clan Pathfinder** | Muspelheim | Trade goods | Thermal route knowledge; shelter access |
| **Rust-Clan Rig Passage** | Jötunheim | Salvage share | Convoy protection; Rig amenities |
| **Dreadnought Berth** | Niflheim | Ice-Debt | Mobile safe zone; warmth |

---

## 8. Vertical Navigation

### 8.1 Z-Axis Movement

Aethelgard has significant vertical extent. See [Spatial Layout](room-engine/spatial-layout.md) for dungeon-scale details.

| Vertical System | Scale | Realms |
|-----------------|-------|--------|
| **Stairs/Ladders** | 1-3 levels | All interior spaces |
| **Shafts/Elevators** | 2-6 levels | Industrial facilities |
| **Lift Systems** | Multi-deck | Svartalfheim, Asgard |
| **Orbital Spires** | Surface to orbit | Midgard↔Asgard |
| **Vertical Stratification** | Zone-based | Vanaheim (Canopy/Gloom/Under) |
| **Depth Stratification** | 0-2000m+ | The Deep (all realms) |

### 8.2 Layer Boundaries

| Layer | Z-Level | Typical Biomes |
|-------|---------|----------------|
| **Canopy** | +3 | Alfheim, Vanaheim (Canopy Sea) |
| **Upper Trunk** | +2 | Alfheim, Jötunheim, Vanaheim |
| **Lower Trunk** | +1 | Alfheim, Jötunheim, Niflheim, Midgard |
| **Ground Level** | 0 | The Roots, Niflheim, Jötunheim, Midgard, Vanaheim |
| **Upper Roots** | -1 | The Roots, Muspelheim, Niflheim, Svartalfheim |
| **Lower Roots** | -2 | Muspelheim, The Roots, Jötunheim, Svartalfheim |
| **Deep Roots** | -3 | Muspelheim, Jötunheim (ancient) |
| **The Deep** | <-3 | All realms (subterranean framework) |

---

## 9. Faction-Controlled Routes

### 9.1 Route Ownership

| Faction | Controlled Routes | Access Policy |
|---------|-------------------|---------------|
| **Midgard Combine** | Toll Roads (A-class) | Courier seals, tolls |
| **Rangers Guild** | Long Roads (B-class) | Watch-fire protocol |
| **Ferry Guild** | River crossings | Passenger fees |
| **Dvergr Hegemony** | Guild Corridors, Deep Gates | Tariffs, approved traders |
| **Scavenger Barons** | Dreadnought Corridors | Ice-Debt contracts |
| **Rust-Clans** | Rig convoy routes | Salvage share |
| **Hearth-Clans** | Muspelheim corridors | Trade goods, respect |
| **Scriptorium** | Asgard access | Safe-conduct writs |

### 9.2 Access Requirements

| Requirement | Description | Obtaining |
|-------------|-------------|-----------|
| **Courier Seal** | Combine road authorization | Combine faction standing |
| **Safe-Conduct Writ** | Scriptorium expedition permit | Jötun-Reader reputation |
| **Trade Approval** | Dvergr commercial access | Guild negotiation |
| **Ice-Debt Contract** | Dreadnought passage | Labor commitment |
| **Salvage Share** | Rig convoy participation | Resource contribution |
| **Sky Looms Certification** | Vanaheim ferry access | Guild training |

---

## 10. Settlement Waypoints

### 10.1 Settlement as Navigation Nodes

Settlements function as waypoints in the navigation network:

| Settlement Size | Waypoint Type | Fast Travel | Services |
|-----------------|---------------|-------------|----------|
| **Outpost** | Minor Waypoint | Discovered only | Limited |
| **Village** | Minor Waypoint | Discovered only | Basic |
| **Town** | Major Waypoint | Always available | Standard |
| **City** | Hub Waypoint | Always available | Full |
| **Capital** | Hub Waypoint | Always available | Complete |

### 10.2 Fast Travel Rules

Fast travel between settlements follows these rules:

| Condition | Effect |
|-----------|--------|
| **Discovery Required** | Must physically visit settlement once |
| **Safe Zone Origin** | Must depart from settlement or camp |
| **Clear Path** | Route must not be blocked by hostile faction |
| **Travel Time** | Condensed to seconds (narrative time passes) |
| **Supplies Consumed** | Proportional to route distance |
| **Interruption Chance** | Small chance of random encounter en route |

### 10.3 Settlement Entry

Entering a settlement triggers sector transition:

```
OVERWORLD → SETTLEMENT ENTRY FLOW

1. Approach settlement (overworld navigation)
2. Reach SettlementGate room
3. Entry check (faction reputation, toll payment)
4. Sector transition: SectorType.Dungeon → SectorType.Settlement
5. Safe zone mechanics activate
6. Fast travel waypoint unlocked (if first visit)
```

### 10.4 Settlement Exit

Exiting a settlement reverses the transition:

```
SETTLEMENT → OVERWORLD EXIT FLOW

1. Navigate to SettlementGate room
2. Choose exit direction
3. Sector transition: SectorType.Settlement → SectorType.Dungeon
4. Re-enter overworld at settlement location
5. Safe zone mechanics deactivate
```

> [!NOTE]
> See [Settlements](settlements.md) for full Social Sector architecture.

---

## 11. Emergency Protocols

### 11.1 Stranded Party Procedures

| Situation | Priority | Action |
|-----------|----------|--------|
| **Lost** | Shelter | Make camp; signal if possible; conserve resources |
| **Weather Event** | Shelter | Seek immediate cover; do not travel in storms |
| **Hostile Contact** | Evade | Avoid engagement; reach nearest safe zone |
| **Injured Party Member** | Medical | Stabilize; reduce travel speed; seek settlement |
| **Equipment Failure** | Repair | Field repair if possible; abort if critical |
| **Contamination** | Treatment | Seek faction healers (Ljósálfar for Golden Plague) |

### 11.2 Rescue Services

| Service | Realm | Response Time | Cost |
|---------|-------|---------------|------|
| **Rangers Guild Patrol** | Midgard | 4-24 hours | Guild dues |
| **Dvergr Recovery Team** | Svartalfheim | 12-48 hours | Significant |
| **Dreadnought Intercept** | Niflheim | Variable | Ice-Debt |
| **Grove-Warden Response** | Vanaheim | 4-6 hours (Canopy) | Guild relations |
| **None Available** | Muspelheim, Asgard, Alfheim | — | Self-rescue only |

---

## 12. Balance Notes

### 12.1 Design Intent

- **Meaningful Distance:** Travel takes real time and resources; realms feel distant
- **Route Choice:** Multiple paths with different risk/reward profiles
- **Faction Integration:** Travel requires engaging with the political landscape
- **Vertical Dimension:** Z-axis travel is as important as horizontal
- **Preparation Matters:** Equipment, guides, and knowledge affect outcomes

### 12.2 Travel as Gameplay

| Element | Gameplay Function |
|---------|-------------------|
| **Route Planning** | Strategic resource management |
| **Faction Navigation** | Social/political gameplay |
| **Environmental Hazards** | Survival mechanics |
| **Encounters** | Combat and exploration content |
| **Time Pressure** | Stakes for mission objectives |

---

## 13. Related Documentation

| Document | Purpose |
|----------|---------|
| [Spatial Layout](room-engine/spatial-layout.md) | Dungeon-scale 3D navigation |
| [Settlements](settlements.md) | Social Sector waypoints |
| [Biome Overview](biomes/biomes-overview.md) | Realm adjacency and transitions |
| [The Deep](the-deep.md) | Subterranean transit framework |
| [Midgard](biomes/midgard.md) | Surface realm navigation details |
| [Svartalfheim](biomes/svartalfheim.md) | Primary Deep Gate hub |
| [Faction Reputation](../02-entities/faction-reputation.md) | Access requirements |

---

## Summary

Aethelgard is a connected world. The Nine Realms are not isolated dungeons but a continental megastructure with trade routes, transit networks, and territorial boundaries. Players navigate surface roads, mountain passes, underground corridors, vertical lifts, and aerial ferries—each with its own hazards, factions, and protocols.

The Class A-D route system provides a unified framework for understanding travel difficulty across all environments. Class A routes are safe and fast; Class D routes are slow and deadly. Most inter-realm travel involves multiple route classes as terrain and infrastructure change.

Travel is not just getting from point A to point B. It is:
- **Resource Management:** Time, supplies, Stamina
- **Political Navigation:** Faction permissions, tolls, contracts
- **Risk Assessment:** Route choice, weather, escorts
- **Survival Challenge:** Environmental hazards, encounters
- **World Engagement:** The spaces between settlements matter

The journey is part of the adventure. The world is vast. Plan accordingly.
