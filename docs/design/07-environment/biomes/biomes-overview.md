---
id: SPEC-BIOMES
title: "Biome System — Transitions & Blending"
version: 1.0
status: draft
last-updated: 2025-12-07
related-files:
  - path: "data/schemas/v0.39.2_biome_transition_schema.sql"
    status: Active
  - path: "data/biomes/"
    status: Active
---

# Biome System — Transitions & Blending

---

## 1. Overview

The biome system defines **environmental theming** with support for:
- Multi-biome sectors (not limited to one biome per dungeon)
- **Biomes span multiple Z-levels** (not rigidly tied to layers)
- Transition zones with blended descriptors
- Logical adjacency rules (fire/ice cannot neighbor directly)

> [!IMPORTANT]
> **Biomes are NOT locked to specific Z-levels.** A sector might have The Roots spanning Z=0 to Z=-2, with Muspelheim starting at Z=-2 and extending to Z=-3. The layer table shows *typical* associations, not rigid constraints.

---

## 2. Biome Definitions

### 2.1 The Nine Biomes

| Biome | Theme | Temperature | Typical Layers | Description |
|-------|-------|-------------|----------------|-------------|
| **The Roots** | Industrial decay | Cool (5-20°C) | Z=-1 to Z=+1 | Rusted corridors, leaking pipes, maintenance tunnels |
| **Muspelheim** | Volcanic fire | Extreme (50-200°C) | Z=-2 to Z=-3 | Eternal meltdown, lava, scorched metal |
| **Niflheim** | Frozen ice | Extreme (-40 to 0°C) | Z=-1 to Z=+1 | Ice coating, frozen machinery, frost |
| **Jötunheim** | Giant-scale industrial | Moderate (10-25°C) | Z=-2 to Z=+2 | Massive chambers, titanic machinery |
| **Alfheim** | Aetheric corruption | Variable | Z=+1 to Z=+3 | Reality distortion, crystal growths, paradox |
| **Midgard** | Agricultural heartland | Temperate (10-25°C) | Z=0 to Z=+1 | Tamed wilderness, fortified Holds, feral forests |
| **Vanaheim** | Bio-organic overgrowth | Humid (15-35°C) | Z=-1 to Z=+2 | Vertical stratification, mutagenic flora, Golden Plague |
| **Svartalfheim** | Subterranean manufacturing | Cool (12-18°C) | Z=-1 to Z=-2 | Dvergr forges, Black Veins darkness, Pure Steel |
| **NeutralZone** | Transition | Moderate | Any | Buffer between incompatible biomes |

### 2.2 Layer Tendencies (Not Constraints)

```
                    TYPICAL BIOME DISTRIBUTION

Z=+3  Canopy       │ Alfheim (surface exposure, high Aether)
Z=+2  UpperTrunk   │ Alfheim, Jötunheim, Vanaheim (Canopy Sea)
Z=+1  LowerTrunk   │ Alfheim, Jötunheim, Niflheim, Midgard (surface), Vanaheim (Gloom-Veil)
Z=0   GroundLevel  │ The Roots, Niflheim, Jötunheim, Midgard, Vanaheim (Under-growth)  ← ENTRY
Z=-1  UpperRoots   │ The Roots, Muspelheim, Niflheim, Svartalfheim (Guild-Lands), Vanaheim (deep)
Z=-2  LowerRoots   │ Muspelheim, The Roots, Jötunheim, Svartalfheim (Black Veins)
Z=-3  DeepRoots    │ Muspelheim, Jötunheim (ancient)
```

> [!NOTE]
> **Midgard is a surface realm.** Unlike dungeon biomes, Midgard represents exterior environments (forests, settlements, coastlines) at ground level and above. The Asgardian Scar sub-biome contains crashed orbital debris but remains at surface Z-levels.

> [!NOTE]
> **Vanaheim is vertically stratified.** Unlike horizontal biomes, Vanaheim's zones are strictly vertical: Canopy Sea (Z=+2, livable), Gloom-Veil (Z=+1 to Z=0, transitional), Under-growth (Z=0 to Z=-1, extreme hazard). Elevation determines survivability.

> [!NOTE]
> **Svartalfheim is light-stratified.** The Guild-Lands (lit, patrolled) exist at Z=-1, while the Black Veins (absolute darkness, hostile) extend to Z=-2 and beyond. Illumination determines civilization.

> [!NOTE]
> A biome can span 2-4 Z-levels within a sector. The generator randomly places biome boundaries, respecting adjacency rules.

---

## 3. Biome Adjacency Matrix

### 3.1 Compatibility Types

| Type | Meaning | Transition Rooms |
|------|---------|------------------|
| **Compatible** | Can neighbor directly | 0-1 |
| **RequiresTransition** | Needs transition zone | 1-3 |
| **Incompatible** | Cannot neighbor directly | N/A (use NeutralZone) |

### 3.2 Adjacency Rules

```
                              ADJACENCY MATRIX (9 Biomes)

             TheRoots  Muspelheim  Niflheim  Jötunheim  Alfheim  Midgard  Vanaheim  Svartalfheim  Neutral
TheRoots        —       Trans(1-2)  Trans(1-2)  Compat    Compat   Compat   Trans(1-2)  Compat      Compat
Muspelheim   Trans(1-2)     —       ❌INCOMPAT  Trans(2-3) Trans(1-2) Trans(2-3) ❌INCOMPAT  Compat      Trans(2-3)
Niflheim     Trans(1-2)  ❌INCOMPAT     —       Trans(1-2) Trans(1-2) Trans(1-2) Trans(2-3)  Trans(2-3)  Trans(2-3)
Jötunheim    Compat     Trans(2-3)  Trans(1-2)     —      Trans(1-2) Trans(1-2) Trans(1-2)  Trans(1-2)  Trans(1-2)
Alfheim      Compat     Trans(1-2)  Trans(1-2) Trans(1-2)     —      Trans(1-2) Trans(1-2)  Trans(2-3)  Compat
Midgard      Compat     Trans(2-3)  Trans(1-2) Trans(1-2) Trans(1-2)    —       Compat      Trans(1-2)  Compat
Vanaheim     Trans(1-2) ❌INCOMPAT  Trans(2-3) Trans(1-2) Trans(1-2) Compat      —          Trans(2-3)  Trans(1-2)
Svartalfheim Compat     Compat      Trans(2-3) Trans(1-2) Trans(2-3) Trans(1-2) Trans(2-3)     —        Trans(1-2)
NeutralZone  Compat     Trans(2-3)  Trans(2-3) Trans(1-2) Compat    Compat   Trans(1-2)  Trans(1-2)     —

Legend: Compat=Compatible, Trans(n-m)=RequiresTransition with n-m rooms
        ❌INCOMPAT = Cannot neighbor (must use NeutralZone between)
```

> [!NOTE]
> **Midgard Inter-Realm Connections:** Midgard connects to other realms via established routes: Utgard Gates (→Jötunheim), Ridge Routes (→Niflheim), Up-River Ferries (→Vanaheim). The Asgardian Scar contains Asgard debris but Asgard proper remains in degraded orbit.

> [!NOTE]
> **Vanaheim Inter-Realm Connections:** Vanaheim connects to Midgard via Up-River Ferry (6–9 days, B/C-class). No direct connection to Alfheim exists, though both share bio-engineering heritage as Vanir research descendants.

> [!NOTE]
> **Svartalfheim Inter-Realm Connections:** Svartalfheim connects to Jötunheim (trade routes, 6-8 days), Midgard (manufactured goods, 8-12 days), and Muspelheim (geothermal link to Hearth-Clans, 4-6 days). The Dvergr maintain strict access control via the Traders' Concourse.

### 3.3 Critical Rule: Fire ↔ Ice/Bio

**Muspelheim and Niflheim CANNOT directly neighbor.**
**Muspelheim and Vanaheim CANNOT directly neighbor.** (Fire destroys bio-organic overgrowth catastrophically)

```
INVALID:  Muspelheim → Niflheim
INVALID:  Muspelheim → Vanaheim
VALID:    Muspelheim → NeutralZone → Niflheim
VALID:    Muspelheim → The Roots → Niflheim
VALID:    Muspelheim → NeutralZone → Vanaheim
```

---

## 4. Transition Zones

### 4.1 Transition Room Properties

Transition rooms have **blended characteristics**:

```csharp
public class TransitionRoom
{
    public string PrimaryBiome { get; set; }      // "TheRoots"
    public string? SecondaryBiome { get; set; }   // "Muspelheim"
    public float BlendRatio { get; set; }         // 0.0-1.0
}
```

| Blend Ratio | Meaning |
|-------------|---------|
| 0.0 | 100% Primary biome |
| 0.25 | Primary dominant, hints of secondary |
| 0.5 | Equal mix |
| 0.75 | Secondary dominant |
| 1.0 | 100% Secondary biome |

### 4.2 Transition Themes

| From → To | Theme | Example Descriptors |
|-----------|-------|---------------------|
| TheRoots → Muspelheim | Geothermal escalation | "Pipes glow cherry-red... heat increases" |
| TheRoots → Niflheim | Cooling failure | "Frost creeps along the walls... temperature drops" |
| TheRoots → Jötunheim | Industrial overlap | "Chambers grow... ceiling rises beyond sight" |
| TheRoots → Alfheim | Aetheric seepage | "Reality shimmers... runes glow brighter" |
| TheRoots → Midgard | Surface emergence | "Corridors give way to open sky... ruins of the old world visible above" |
| Muspelheim → Jötunheim | Heat dissipation | "Volcanic heat fades in vast chambers" |
| Niflheim → Jötunheim | Frozen scale | "Massive ice-encased machinery" |
| Niflheim → Midgard | Alpine descent | "Ice gives way to hardy scrubland... Ridge Hold watch-fires visible below" |
| Jötunheim → Alfheim | Scale distortion | "Titanic architecture warps impossibly" |
| Jötunheim → Midgard | Industrial frontier | "Giant-scale machinery yields to salvaged ferrocrete walls... the Utgard Gates" |
| Jötunheim → Vanaheim | Industrial overgrowth | "Titanic machinery yields to titanic vegetation... vines thick as corridors" |
| Midgard → Alfheim | Scar approach | "Blight contamination increases... reality bleeds at the crater's edge" |
| Midgard → Vanaheim | River ascent | "Up-river ferries... canopy shadows deepen... golden spores drift on the wind" |
| Vanaheim → Alfheim | Bio-Aetheric bleed | "Flora pulses with corrupted light... reality distorts in organic patterns" |
| TheRoots → Vanaheim | Industrial to organic | "Rusted metal yields to root-wrapped infrastructure... living walls breathe" |
| Niflheim → Vanaheim | Frozen to humid | "Ice gives way to moisture... fungal growth on thawing surfaces" |
| TheRoots → Svartalfheim | Decay to order | "Rusted corridors give way to maintained stonework... the clang of hammers grows louder" |
| Muspelheim → Svartalfheim | Heat to cool | "Volcanic heat fades into geothermal stability... Dvergr trade routes begin" |
| Jötunheim → Svartalfheim | Giant to compact | "Titanic chambers narrow into precision-carved tunnels... scale shifts to Dvergr proportions" |
| Svartalfheim → Black Veins | Light to dark | "Anvil-Star crystals grow sparse... the darkness ahead swallows all light" |

### 4.3 Transition Generation Algorithm

```csharp
public List<Room> GenerateTransitionZone(
    string fromBiome, 
    string toBiome,
    int roomCount,
    Random rng)
{
    var rooms = new List<Room>();
    
    for (int i = 0; i < roomCount; i++)
    {
        // Linear blend from 0 to 1 across transition
        float blendRatio = (float)(i + 1) / (roomCount + 1);
        
        rooms.Add(new Room
        {
            PrimaryBiome = fromBiome,
            SecondaryBiome = toBiome,
            BlendRatio = blendRatio
        });
    }
    
    return rooms;
}
```

---

## 5. Environmental Properties

### 5.1 Property Types

Each room can have environmental gradients:

| Property | Unit | Range | Notes |
|----------|------|-------|-------|
| Temperature | Celsius | -40 to 200 | Hazard thresholds at extremes |
| AethericIntensity | 0-1 | 0.0-1.0 | Higher = more magic/paradox |
| ScaleFactor | Multiplier | 0.5-3.0 | 1.0=human, 3.0=giant |
| Humidity | Percent | 0-100 | Affects corrosion/frost |
| LightLevel | 0-1 | 0.0-1.0 | 0=dark, 1=bright |
| CorrosionRate | 0-1 | 0.0-1.0 | Affects equipment degradation |

### 5.2 Biome Baseline Properties

| Biome | Temp | Aetheric | Scale | Humidity | Light |
|-------|------|----------|-------|----------|-------|
| The Roots | 15°C | 0.2 | 1.0 | 70% | 0.3 |
| Muspelheim | 100°C | 0.1 | 1.0 | 10% | 0.6 |
| Niflheim | -20°C | 0.3 | 1.0 | 90% | 0.4 |
| Jötunheim | 18°C | 0.2 | 2.5 | 40% | 0.5 |
| Alfheim | 20°C | 0.9 | 1.5 | 30% | 0.8 |
| Midgard | 18°C | 0.3 | 1.0 | 60% | 0.7 |
| Vanaheim | 25°C | 0.5 | 1.2 | 85% | 0.4 |
| Svartalfheim | 15°C | 0.1 | 0.9 | 50% | 0.6 |

> [!NOTE]
> **Midgard Aetheric Variance:** Base Aetheric is 0.3, but the Asgardian Scar sub-biome has elevated intensity (0.7-0.9) due to Runic Blight contamination from crashed Asgard infrastructure.

> [!NOTE]
> **Vanaheim Zone Variance:** Properties vary dramatically by vertical zone. Canopy Sea: Temp 20°C, Light 0.6, Humidity 60%. Gloom-Veil: Temp 25°C, Light 0.3, Humidity 80%. Under-growth: Temp 32°C, Light 0.0, Humidity 95%. Aetheric intensity increases with depth (0.4 → 0.6 → 0.8).

> [!NOTE]
> **Svartalfheim Zone Variance:** Properties vary by illumination zone. Guild-Lands: Light 0.6, Aetheric 0.1 (Dvergr-controlled). Black Veins: Light 0.0, Aetheric 0.2 (feral territory). Glimmering Grottos: Light 0.3 (Blight-glow), Aetheric 0.8 (extreme hazard). Darkness is the primary environmental hazard.

### 5.3 Property Interpolation

In transition zones, properties interpolate:

```csharp
public float GetInterpolatedProperty(
    Room room, 
    string propertyName)
{
    var primaryValue = GetBiomeProperty(room.PrimaryBiome, propertyName);
    var secondaryValue = GetBiomeProperty(room.SecondaryBiome, propertyName);
    
    return primaryValue + (secondaryValue - primaryValue) * room.BlendRatio;
}

// Example: Room at 50% blend between TheRoots (15°C) and Muspelheim (100°C)
// Temperature = 15 + (100 - 15) * 0.5 = 57.5°C
```

---

## 6. Biome Assignment Algorithm

### 6.1 Sector Biome Planning

```
┌─────────────────────────────────────────────────────────────┐
│ INPUT: Sector layout (rooms with Z coordinates)             │
└─────────────────────────────────────────────────────────────┘
         │
         ▼
┌─────────────────────────────────────────────────────────────┐
│ STEP 1: Select Primary Biome                                │
│   • Based on sector type and player progression             │
│   • e.g., "The Roots" for early game                       │
└─────────────────────────────────────────────────────────────┘
         │
         ▼
┌─────────────────────────────────────────────────────────────┐
│ STEP 2: Determine Biome Span (2-4 Z levels)                 │
│   • Primary biome covers majority of sector                 │
│   • Select secondary biome (if multi-biome sector)         │
└─────────────────────────────────────────────────────────────┘
         │
         ▼
┌─────────────────────────────────────────────────────────────┐
│ STEP 3: Place Biome Boundary                                │
│   • Random Z-level for transition                          │
│   • Respect adjacency rules                                 │
└─────────────────────────────────────────────────────────────┘
         │
         ▼
┌─────────────────────────────────────────────────────────────┐
│ STEP 4: Generate Transition Zones                           │
│   • 1-3 rooms with blended descriptors                     │
│   • Interpolate environmental properties                    │
└─────────────────────────────────────────────────────────────┘
         │
         ▼
┌─────────────────────────────────────────────────────────────┐
│ OUTPUT: Rooms with biome assignments + environmental props  │
└─────────────────────────────────────────────────────────────┘
```

### 6.2 Example Multi-Biome Sector

```
SECTOR "Geothermal Descent"

Z=+1  │ The Roots (entry area)
Z=0   │ The Roots
Z=-1  │ The Roots → Muspelheim (TRANSITION: 2 rooms)
Z=-2  │ Muspelheim
Z=-3  │ Muspelheim (boss arena)
```

**Room breakdown:**
- Rooms 1-5: The Roots (Z=0 to Z=-1)
- Rooms 6-7: Transition (blend 0.33, 0.66)
- Rooms 8-11: Muspelheim (Z=-2 to Z=-3)
- Room 12: Boss Arena (Muspelheim, Z=-3)

---

## 7. Data Schema

### 7.1 Room Biome Columns

```sql
ALTER TABLE Rooms ADD COLUMN primary_biome TEXT DEFAULT 'TheRoots';
ALTER TABLE Rooms ADD COLUMN secondary_biome TEXT DEFAULT NULL;
ALTER TABLE Rooms ADD COLUMN biome_blend_ratio REAL DEFAULT 0.0;
```

### 7.2 Adjacency Rules Table

```sql
CREATE TABLE Biome_Adjacency (
    biome_a TEXT NOT NULL,
    biome_b TEXT NOT NULL,
    compatibility TEXT NOT NULL,  -- Compatible/RequiresTransition/Incompatible
    min_transition_rooms INTEGER DEFAULT 0,
    max_transition_rooms INTEGER DEFAULT 3,
    transition_theme TEXT,
    UNIQUE(biome_a, biome_b)
);
```

**Full schema:** [v0.39.2_biome_transition_schema.sql](../../data/schemas/v0.39.2_biome_transition_schema.sql)

---

## 8. Service Interface

```csharp
public interface IBiomeTransitionService
{
    bool CanBiomesNeighbor(string biomeA, string biomeB);
    int GetRequiredTransitionRooms(string biomeA, string biomeB);
    string GetTransitionTheme(string fromBiome, string toBiome);
    
    void AssignBiomesToSector(Sector sector, string primaryBiome);
    void GenerateTransitionZones(Sector sector);
    
    float GetEnvironmentalProperty(Room room, string propertyName);
}
```

---

## 9. Biome Content Reference

Each biome definition includes:
- **Available Templates** — Room templates filtered by biome
- **Descriptor Categories** — Adjectives, Details, Sounds, Smells
- **Element Pools** — Enemies, hazards, loot nodes, ambient conditions

**Example:** [the_roots.json](../../data/biomes/the_roots.json)

---

## 10. Related Documentation

| Document | Purpose |
|----------|---------|
| [Spatial Layout](../room-engine/spatial-layout.md) | 3D coordinates, Z-levels |
| [Inter-Realm Navigation](../navigation.md) | Route classification, travel times |
| [Room Engine Core](../room-engine/core.md) | Template system |
| [The Roots](the-roots.md) | Biome-specific details |
| [Midgard](midgard.md) | Surface realm, four sub-biomes |
| [Vanaheim](vanaheim.md) | Bio-organic overgrowth, three vertical zones |
| [Alfheim](alfheim.md) | Aetheric corruption, Glimmer hazard |
| [Asgard](asgard.md) | Orbital command structure, Undying hazards |
| [Muspelheim](muspelheim.md) | Eternal meltdown, Surtr AI, Hearth-Clans |
| [Niflheim](niflheim.md) | Frozen tomb, Einherjar archive, Ice-Debt |
| [Jötunheim](jotunheim.md) | Industrial graveyard, Rust-Clans, Great Looms |
| [Svartalfheim](svartalfheim.md) | Dvergr forges, Black Veins, Pure Steel |
| [The Deep](../the-deep.md) | Subterranean framework across all realms |
| [Encounter Generation](../../03-combat/encounter-generation.md) | Faction spawn pools per biome |
| [Spawn Scaling](../../03-combat/spawn-scaling.md) | Zone minimum TDR floors |

---

## 11. Faction Spawn Pools

> **Reference:** [SPEC-COMBAT-016](../../03-combat/encounter-generation.md)

Each biome has weighted faction pools for enemy spawning:

| Biome | Primary Faction (60%) | Secondary Faction (30%) | Rare Faction (10%) |
|-------|----------------------|------------------------|-------------------|
| **The Roots** | Corrupted Machinery | Forlorn | Symbiotic Plates |
| **Muspelheim** | Fire-Forged | Corrupted Machinery | Forlorn |
| **Niflheim** | Frost-Touched | Corrupted Machinery | Forlorn |
| **Jötunheim** | Ancient Constructs | Corrupted Machinery | Forlorn |
| **Alfheim** | Aetheric Anomalies | Forlorn | Temporal |
| **Midgard** | Blighted Beasts | Humanoid | Forlorn (Scar only) |
| **Vanaheim** | Weaponized Flora | Blighted Beasts | The Unraveled |
| **Svartalfheim** | Corrupted Automata | Blighted Beasts | Silent Folk |

> [!NOTE]
> **Midgard Sub-Biome Variance:** Spawn pools shift by sub-biome. Greatwood favors Blighted Beasts; Souring Mires/Serpent Fjords have higher Humanoid presence; Asgardian Scar is dominated by Forlorn. See [Midgard](midgard.md) for detailed sub-biome spawn tables.

> [!NOTE]
> **Vanaheim Zone Variance:** Spawn pools shift dramatically by vertical zone. Canopy Sea: Blighted Beasts (60%), Weaponized Flora (30%), Humanoid (10%). Gloom-Veil: Weaponized Flora (60%), Data-Wraiths (30%), Blighted Beasts (10%). Under-growth: The Unraveled (60%), Weaponized Flora (30%), Un-Womb Spawn (10%). See [Vanaheim](vanaheim.md) for detailed zone spawn tables.

> [!NOTE]
> **Svartalfheim Zone Variance:** Guild-Lands have minimal spawns (patrolled). Black Veins: Corrupted Automata (60%), Blighted Beasts (30%), Silent Folk (10%). Glimmering Grottos: Aetheric Entities (60%), Corrupted Automata (30%), Deep-Stalkers (10%). See [Svartalfheim](svartalfheim.md) for detailed zone spawn tables.

**Spawn Pool Integration:**
- Room archetypes reference biome faction pools
- Encounter generation draws from weighted pools
- Elite/Champion spawns may pull from secondary/rare factions
