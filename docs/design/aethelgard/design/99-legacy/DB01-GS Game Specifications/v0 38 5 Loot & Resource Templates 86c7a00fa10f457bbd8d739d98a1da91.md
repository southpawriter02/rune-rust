# v0.38.5: Loot & Resource Templates

Description: 6+ resource node templates, 20+ descriptors, 40+ composite loot nodes, biome distribution profiles
Priority: Must-Have
Status: In Design
Target Version: Alpha
Dependencies: v0.38, v0.11
Implementation Difficulty: Medium
Balance Validated: No
Parent item: v0.38: Descriptor Library & Content Database (v0%2038%20Descriptor%20Library%20&%20Content%20Database%200a9293f3a9b44c968a36c0a429ab841d.md)
Proof-of-Concept Flag: No
Template Validated: No
Voice Validated: No

**Parent Specification:** [v0.38: Descriptor Library & Content Database](v0%2038%20Descriptor%20Library%20&%20Content%20Database%200a9293f3a9b44c968a36c0a429ab841d.md)

**Status:** Design Phase

**Timeline:** 8-10 hours

**Goal:** Build comprehensive loot and resource node library

**Philosophy:** Procedural reward generation with biome-appropriate resource distribution

---

## I. Purpose

v0.38.5 creates the **Loot & Resource Templates**, defining how rewards appear in the procedurally generated world:

- **6+ Resource Node Base Templates** (ore veins, salvage, organics)
- **20+ Resource Descriptors** (appearance, extraction mechanics)
- **40+ Composite Loot Nodes** (base + modifier + biome combinations)

**Strategic Function:**

Currently, loot is defined separately from environmental generation:

- ❌ Resource nodes not integrated with room generation
- ❌ No biome-specific resource distribution
- ❌ Missed environmental storytelling through resource placement

**v0.38.5 Solution:**

- Standardized resource node templates
- Biome-specific resource distribution profiles
- Extraction mechanics tied to resource types
- Integration with Dynamic Room Engine

---

## II. The Rule: What's In vs. Out

### ✅ In Scope

- **Resource Nodes:** Ore veins, salvageable wreckage, organic resources
- **Extraction Mechanics:** Mining, salvaging, harvesting
- **Resource Types:** Metals, organics, Aetheric components, mechanical parts
- **Rarity Tiers:** Common, Uncommon, Rare, Legendary
- **Biome resource profiles**
- **Node base templates (6+)**
- **Composite generation**
- **Integration with v0.11 population**
- **Database schema**

### ❌ Out of Scope

- Loot table contents (defined elsewhere)
- Container loot (handled by v0.38.3 Interactive Objects)
- Enemy drops (separate system)
- Quest rewards (v0.40)
- Crafting recipes (v0.36)
- UI/rendering changes

---

## III. Resource Node Taxonomy

### A. Four Core Types

```csharp
public enum ResourceNodeType
{
    MineralVein,        // Ore deposits (metal, crystal)
    SalvageWreckage,    // Mechanical salvage
    OrganicHarvest,     // Biological/chemical resources  
    AethericAnomaly     // Runic/magical resources
}
```

### B. Extraction Mechanics

**Mining (Mineral Veins):**

- Requires: Mining Tool or MIGHT check
- Time Cost: 2 turns
- Yield: 2-4 units

**Salvaging (Wreckage):**

- Requires: Salvage Kit or WITS check
- Time Cost: 3 turns
- Yield: 1-3 components

**Harvesting (Organics):**

- Requires: No tool (bare hands)
- Time Cost: 1 turn
- Yield: 1-2 units

**Siphoning (Aetheric):**

- Requires: Aether Siphon or Galdr-caster
- Time Cost: 2 turns
- Yield: 1 unit (volatile)

---

## IV. Resource Node Base Templates

### Template 1: Ore_Vein_Base

**Type:** MineralVein

**Tags:** `["Mining", "Metal", "Industrial"]`

**Base Mechanics:**

```json
{
  "extraction_type": "Mining",
  "extraction_dc": 12,
  "extraction_time": 2,
  "yield_min": 2,
  "yield_max": 4,
  "depletes": true,
  "uses": 3
}
```

**Name Template:** `"{Modifier} {Resource_Type} Vein"`

**Description Template:**

`"A {Modifier_Adj} vein of {Resource_Type} {Modifier_Detail}. [Mining DC 12]"`

**Resource Type Variants:**

- **Iron:** Common, industrial applications
- **Star-Metal:** Uncommon, high-quality alloy
- **Obsidian:** Rare (Muspelheim), volcanic glass
- **Aetheric Crystal:** Rare (Alfheim), runic catalyst

**Example Composites:**

- Ore_Vein_Base + [Rusted] + Iron = "Corroded Iron Vein"
- Ore_Vein_Base + [Scorched] + Star-Metal = "Scorched Star-Metal Vein"
- Ore_Vein_Base + [Crystalline] + Aetheric Crystal = "Crystalline Aether Vein"

---

### Template 2: Salvage_Wreckage_Base

**Type:** SalvageWreckage

**Tags:** `["Salvage", "Mechanical", "Junk"]`

**Base Mechanics:**

```json
{
  "extraction_type": "Salvaging",
  "extraction_dc": 15,
  "extraction_time": 3,
  "yield_min": 1,
  "yield_max": 3,
  "depletes": true,
  "uses": 2,
  "trap_chance": 0.1
}
```

**Name Template:** `"{Modifier} {Wreckage_Type}"`

**Description Template:**

`"The wreckage of {Article} {Modifier_Adj} {Wreckage_Type}. {Modifier_Detail}. [Salvage DC 15]"`

**Wreckage Type Variants:**

- **Servitor Chassis:** Common, basic components
- **Power Conduit:** Uncommon, energy systems
- **Forge Equipment:** Rare (Muspelheim), high-temp parts
- **Cryo System:** Rare (Niflheim), refrigeration tech
- **Jötun Console:** Rare (Jötunheim), advanced circuitry

---

### Template 3: Fungal_Growth_Base

**Type:** OrganicHarvest

**Tags:** `["Harvest", "Organic", "Alchemy"]`

**Base Mechanics:**

```json
{
  "extraction_type": "Harvesting",
  "extraction_dc": 10,
  "extraction_time": 1,
  "yield_min": 1,
  "yield_max": 2,
  "depletes": true,
  "uses": 2,
  "poisonous": false
}
```

**Name Template:** `"{Modifier} {Fungus_Type}"`

**Description Template:**

`"{Article_Cap} {Modifier_Adj} {Fungus_Type} grows here. {Modifier_Detail}."`

**Fungus Type Variants:**

- **Luminous Shelf Fungus:** Common, alchemical base
- **Frost Lichen:** Uncommon (Niflheim), cold resist
- **Ember Moss:** Uncommon (Muspelheim), fire resist
- **Paradox Spore:** Rare (Alfheim), psychic ingredient

---

### Template 4: Chemical_Deposit_Base

**Type:** OrganicHarvest

**Tags:** `["Harvest", "Chemical", "Hazardous"]`

**Base Mechanics:**

```json
{
  "extraction_type": "Harvesting",
  "extraction_dc": 14,
  "extraction_time": 2,
  "yield_min": 1,
  "yield_max": 1,
  "depletes": true,
  "uses": 1,
  "hazardous": true
}
```

**Name Template:** `"{Chemical_Type} Deposit"`

**Description Template:**

`"A deposit of {Chemical_Type} {Modifier_Detail}. Handle with care. [Harvest DC 14]"`

**Chemical Type Variants:**

- **Volatile Oil:** Common, flammable liquid
- **Caustic Sludge:** Uncommon, acid component
- **Cryogenic Fluid:** Rare (Niflheim), freezing agent
- **Magma Residue:** Rare (Muspelheim), thermal catalyst

---

### Template 5: Aetheric_Anomaly_Base

**Type:** AethericAnomaly

**Tags:** `["Siphon", "Aetheric", "Magical"]`

**Base Mechanics:**

```json
{
  "extraction_type": "Siphoning",
  "extraction_dc": 18,
  "extraction_time": 2,
  "yield_min": 1,
  "yield_max": 1,
  "depletes": true,
  "uses": 1,
  "unstable": true,
  "requires_galdr": true
}
```

**Name Template:** `"{Anomaly_Type}"`

**Description Template:**

`"{Article_Cap} {Anomaly_Type} pulses with unstable Aetheric energy. {Modifier_Detail}. [Siphon DC 18, Galdr required]"`

**Anomaly Type Variants:**

- **Runic Eddy:** Uncommon, raw Aether
- **Reality Fracture:** Rare (Alfheim), glitched essence
- **Forlorn Echo:** Rare, trapped psychic energy
- **All-Rune Fragment:** Legendary (Alfheim), paradox shard

---

### Template 6: Ancient_Cache_Base

**Type:** SalvageWreckage

**Tags:** `["Cache", "Hidden", "Valuable"]`

**Base Mechanics:**

```json
{
  "extraction_type": "Search",
  "extraction_dc": 16,
  "extraction_time": 2,
  "yield_quality": "Rare",
  "depletes": true,
  "uses": 1,
  "hidden": true,
  "trap_chance": 0.3
}
```

**Name Template:** `"{Culture} Supply Cache"`

**Description Template:**

`"A hidden {Culture} supply cache {Modifier_Detail}. [Hidden, WITS DC 18 to detect]"`

**Culture Variants:**

- **Dvergr:** Mechanical parts, tools
- **Seiðkona:** Runic components, scrolls
- **Jötun:** Ancient tech, data slates
- **Pre-Blight:** Legendary artifacts

---

## V. Biome Resource Distribution Profiles

### Profile 1: The Roots

**Common Resources:**

- Iron Ore Veins (40%)
- Salvageable Servitor Wreckage (30%)
- Luminous Shelf Fungus (20%)
- Volatile Oil Deposits (10%)

**Uncommon Resources:**

- Star-Metal Veins (50%)
- Power Conduit Wreckage (30%)
- Caustic Sludge (20%)

**Rare Resources:**

- Dvergr Supply Caches (60%)
- Runic Eddies (40%)

**Spawn Density:** Medium (2-3 nodes per large room)

---

### Profile 2: Muspelheim

**Common Resources:**

- Obsidian Veins (50%)
- Ember Moss (30%)
- Magma Residue (20%)

**Uncommon Resources:**

- Star-Metal Veins (fire-hardened) (60%)
- Forge Equipment Wreckage (40%)

**Rare Resources:**

- Heart of the Inferno (unique resource)
- Ancient Forge Caches (30%)

**Spawn Density:** Low (1-2 nodes per large room, hazardous)

---

### Profile 3: Niflheim

**Common Resources:**

- Frost Lichen (40%)
- Cryogenic Fluid (30%)
- Ice Crystal Formations (30%)

**Uncommon Resources:**

- Cryo System Wreckage (60%)
- Frozen Organics (preserved specimens) (40%)

**Rare Resources:**

- Eternal Ice Shards (unique resource)
- Seiðkona Caches (30%)

**Spawn Density:** Medium (2-3 nodes, often encased in ice)

---

### Profile 4: Alfheim

**Common Resources:**

- Paradox Spores (40%)
- Crystalline Formations (30%)
- Runic Eddies (30%)

**Uncommon Resources:**

- Aetheric Crystal Veins (60%)
- Reality Fractures (40%)

**Rare Resources:**

- All-Rune Fragments (legendary)
- Pre-Blight Caches (30%)

**Spawn Density:** High (3-4 nodes, unstable/dangerous)

---

### Profile 5: Jötunheim

**Common Resources:**

- Salvageable Jötun Tech (50%)
- Ancient Alloy Veins (30%)
- Industrial Chemical Deposits (20%)

**Uncommon Resources:**

- Jötun Console Wreckage (70%)
- Hardened Servomotors (30%)

**Rare Resources:**

- Jötun Data Archives (legendary)
- Titanic Component Caches (40%)

**Spawn Density:** Medium-High (2-4 nodes, industrial scale)

---

*Continued with database schema and implementation...*

## VI. Database Schema

```sql
CREATE TABLE IF NOT EXISTS Resource_Nodes (
    node_id INTEGER PRIMARY KEY AUTOINCREMENT,
    room_id INTEGER NOT NULL,
    
    -- Descriptor reference
    composite_descriptor_id INTEGER,
    
    -- Node identity
    node_name TEXT NOT NULL,
    description TEXT NOT NULL,
    node_type TEXT NOT NULL,  -- 'MineralVein', 'SalvageWreckage', etc.
    
    -- Extraction mechanics
    extraction_type TEXT NOT NULL,  -- 'Mining', 'Salvaging', 'Harvesting'
    extraction_dc INTEGER,
    extraction_time INTEGER,  -- Turns
    
    -- Yield
    yield_min INTEGER,
    yield_max INTEGER,
    resource_type TEXT,  -- 'Iron_Ore', 'Star_Metal', etc.
    rarity_tier TEXT,  -- 'Common', 'Uncommon', 'Rare', 'Legendary'
    
    -- State
    depleted BOOLEAN DEFAULT 0,
    uses_remaining INTEGER,
    
    -- Hazards
    hazardous BOOLEAN DEFAULT 0,
    trap_chance REAL DEFAULT 0,
    
    FOREIGN KEY (room_id) REFERENCES Rooms(room_id),
    FOREIGN KEY (composite_descriptor_id) REFERENCES Descriptor_Composites(composite_id),
    CHECK (node_type IN ('MineralVein', 'SalvageWreckage', 'OrganicHarvest', 'AethericAnomaly')),
    CHECK (rarity_tier IN ('Common', 'Uncommon', 'Rare', 'Legendary'))
);

CREATE TABLE IF NOT EXISTS Biome_Resource_Profiles (
    profile_id INTEGER PRIMARY KEY AUTOINCREMENT,
    biome_name TEXT NOT NULL UNIQUE,
    
    -- Distribution tables (JSON)
    common_resources TEXT NOT NULL,  -- [{node_template, weight, resource_type}]
    uncommon_resources TEXT NOT NULL,
    rare_resources TEXT NOT NULL,
    legendary_resources TEXT,
    
    -- Spawn rules
    spawn_density_small INTEGER,  -- Per small room
    spawn_density_medium INTEGER,
    spawn_density_large INTEGER
);
```

### Insert Resource Profiles

```sql
INSERT INTO Biome_Resource_Profiles (
    biome_name,
    common_resources,
    uncommon_resources,
    rare_resources,
    spawn_density_small,
    spawn_density_medium,
    spawn_density_large
) VALUES (
    'The_Roots',
    '[
        {"template": "Ore_Vein_Base", "resource": "Iron", "weight": 0.4},
        {"template": "Salvage_Wreckage_Base", "resource": "Servitor", "weight": 0.3},
        {"template": "Fungal_Growth_Base", "resource": "Luminous_Fungus", "weight": 0.2},
        {"template": "Chemical_Deposit_Base", "resource": "Volatile_Oil", "weight": 0.1}
    ]',
    '[
        {"template": "Ore_Vein_Base", "resource": "Star_Metal", "weight": 0.5},
        {"template": "Salvage_Wreckage_Base", "resource": "Power_Conduit", "weight": 0.3},
        {"template": "Chemical_Deposit_Base", "resource": "Caustic_Sludge", "weight": 0.2}
    ]',
    '[
        {"template": "Ancient_Cache_Base", "resource": "Dvergr_Cache", "weight": 0.6},
        {"template": "Aetheric_Anomaly_Base", "resource": "Runic_Eddy", "weight": 0.4}
    ]',
    0, 2, 3
);

-- Continue for remaining biomes...
```

---

## VII. Service Implementation

```csharp
public class ResourceNodeService
{
    private readonly IDescriptorRepository _repository;
    private readonly Random _random;
    
    /// <summary>
    /// Generate resource nodes for room based on biome profile.
    /// </summary>
    public List<ResourceNode> GenerateResourceNodes(
        Room room,
        BiomeDefinition biome)
    {
        var profile = _repository.GetBiomeResourceProfile(biome.BiomeId);
        var nodeCount = GetNodeCount(room.Size, profile);
        
        var nodes = new List<ResourceNode>();
        
        for (int i = 0; i < nodeCount; i++)
        {
            var node = GenerateNode(room, biome, profile);
            if (node != null)
            {
                nodes.Add(node);
            }
        }
        
        return nodes;
    }
    
    private int GetNodeCount(string roomSize, BiomeResourceProfile profile)
    {
        return roomSize switch
        {
            "Small" => profile.SpawnDensitySmall,
            "Medium" => profile.SpawnDensityMedium,
            "Large" => profile.SpawnDensityLarge,
            _ => 1
        };
    }
    
    private ResourceNode GenerateNode(
        Room room,
        BiomeDefinition biome,
        BiomeResourceProfile profile)
    {
        // Roll for rarity tier
        var rarityRoll = _random.NextDouble();
        var resourceDef = SelectResource(profile, rarityRoll);
        
        if (resourceDef == null)
            return null;
        
        // Get base template and modifier
        var baseTemplate = _repository.GetBaseTemplate(resourceDef.Template);
        var modifier = GetModifierForBiome(biome.BiomeId);
        
        // Generate node
        var node = InstantiateNode(
            baseTemplate,
            modifier,
            resourceDef.Resource,
            resourceDef.RarityTier);
        
        node.RoomId = [room.Id](http://room.Id);
        
        return node;
    }
    
    private ResourceDefinition SelectResource(
        BiomeResourceProfile profile,
        double rarityRoll)
    {
        // 70% common, 25% uncommon, 5% rare
        if (rarityRoll < 0.70)
        {
            return SelectWeighted(profile.CommonResources);
        }
        else if (rarityRoll < 0.95)
        {
            return SelectWeighted(profile.UncommonResources);
        }
        else
        {
            return SelectWeighted(profile.RareResources);
        }
    }
}
```

---

## VIII. Integration with Room Population

```csharp
public class DynamicRoomEngine
{
    private readonly IResourceNodeService _resourceNodeService;
    
    public void PopulateRoomWithResources(
        Room room,
        BiomeDefinition biome)
    {
        var nodes = _resourceNodeService.GenerateResourceNodes(room, biome);
        
        foreach (var node in nodes)
        {
            room.ResourceNodes.Add(node);
            
            _logger.LogDebug(
                "Placed resource node: Room={RoomId}, Node={NodeName}, Rarity={Rarity}",
                [room.Id](http://room.Id),
                [node.Name](http://node.Name),
                node.RarityTier);
        }
    }
}
```

---

## IX. Success Criteria

**v0.38.5 is DONE when:**

### Base Templates

- [ ]  6 resource node base templates
- [ ]  All 4 core types covered
- [ ]  Extraction mechanics defined
- [ ]  Yield parameters configured

### Resource Descriptors

- [ ]  20+ resource type variants
- [ ]  Rarity tiers mapped
- [ ]  Extraction DCs balanced
- [ ]  Hazard flags set appropriately

### Composite Generation

- [ ]  40+ composite resource nodes
- [ ]  Base + modifier + resource type combinations
- [ ]  Biome-appropriate distribution
- [ ]  Rarity balanced

### Biome Profiles

- [ ]  5 complete biome resource profiles
- [ ]  Distribution percentages defined
- [ ]  Spawn densities configured
- [ ]  Unique resources per biome

### Service Implementation

- [ ]  ResourceNodeService complete
- [ ]  GenerateResourceNodes() functional
- [ ]  Rarity selection logic
- [ ]  Weighted resource selection

### Database

- [ ]  Resource_Nodes table created
- [ ]  Biome_Resource_Profiles table created
- [ ]  Sample data inserted
- [ ]  All 5 biome profiles defined

### Integration

- [ ]  DynamicRoomEngine populates resources
- [ ]  Resources appear in generated rooms
- [ ]  Extraction mechanics functional
- [ ]  Depletion tracking works

### Testing

- [ ]  Unit tests (80%+ coverage)
- [ ]  Resource generation tests
- [ ]  Distribution tests
- [ ]  Extraction mechanic tests

---

## X. Implementation Roadmap

**Phase 1: Database Schema** — 2 hours

- Resource node base templates (6)
- Resource_Nodes table
- Biome_Resource_Profiles table
- Sample data

**Phase 2: Service Implementation** — 3 hours

- ResourceNodeService
- Generation logic
- Rarity selection
- Weighted distribution

**Phase 3: Biome Profiles** — 2 hours

- Define 5 biome profiles
- Resource distribution tables
- Spawn density configuration

**Phase 4: Integration** — 2 hours

- Update DynamicRoomEngine
- Test resource population
- Verify distribution

**Phase 5: Testing** — 1 hour

- Unit tests
- Integration tests
- Distribution validation

**Total: 10 hours**

---

## XI. Example Generated Resource Nodes

### Example 1: Corroded Iron Vein (The Roots)

**Base:** Ore_Vein_Base

**Modifier:** [Rusted]

**Resource:** Iron Ore

**Rarity:** Common

**Generated:**

```json
{
  "name": "Corroded Iron Vein",
  "description": "A corroded vein of iron ore shows centuries of oxidation. [Mining DC 12]",
  "extraction_type": "Mining",
  "extraction_dc": 12,
  "extraction_time": 2,
  "yield_min": 2,
  "yield_max": 4,
  "resource_type": "Iron_Ore",
  "rarity_tier": "Common"
}
```

**Extraction:**

> **Player:** `mine vein`
> 

> 
> 

> **System:** You spend 2 turns mining the vein. [MIGHT Check DC 12]
> 

> Success! You extract 3 units of Iron Ore.
> 

---

### Example 2: Scorched Star-Metal Vein (Muspelheim)

**Base:** Ore_Vein_Base

**Modifier:** [Scorched]

**Resource:** Star-Metal

**Rarity:** Uncommon

**Generated:**

```json
{
  "name": "Scorched Star-Metal Vein",
  "description": "A fire-hardened vein of star-metal radiates intense heat. [Mining DC 14, Fire Hazard]",
  "extraction_type": "Mining",
  "extraction_dc": 14,
  "extraction_time": 2,
  "yield_min": 2,
  "yield_max": 3,
  "resource_type": "Star_Metal",
  "rarity_tier": "Uncommon",
  "hazardous": true
}
```

---

### Example 3: Jötun Supply Cache (Jötunheim)

**Base:** Ancient_Cache_Base

**Culture:** Jötun

**Rarity:** Rare

**Generated:**

```json
{
  "name": "Hidden Jötun Supply Cache",
  "description": "A hidden Jötun supply cache, barely visible behind fallen machinery. [Hidden, WITS DC 18, 30% trap chance]",
  "extraction_type": "Search",
  "extraction_dc": 18,
  "extraction_time": 2,
  "yield_quality": "Rare",
  "resource_type": "Jotun_Tech",
  "rarity_tier": "Rare",
  "hidden": true,
  "trap_chance": 0.3
}
```

**Discovery:**

> **Player:** `search room thoroughly`
> 

> 
> 

> **System:** [WITS Check DC 18]
> 

> Success! You notice something behind the debris—a hidden Jötun supply cache!
> 

> 
> 

> **Player:** `investigate cache`
> 

> 
> 

> **System:** You carefully approach. [Trap check: Safe]
> 

> You find: 1× Jötun Data-Slate, 2× Hardened Servomotors, 1× Ancient Alloy Ingot.
> 

---

**v0.38.5 Complete.**

---

## XII. Summary: v0.38 Complete

**All child specifications finished:**

1. ✅ **v0.38.1:** Room Description Library
2. ✅ **v0.38.2:** Environmental Feature Catalog
3. ✅ **v0.38.3:** Interactive Object Repository
4. ✅ **v0.38.4:** Atmospheric Descriptor System
5. ✅ **v0.38.5:** Loot & Resource Templates

**The Descriptor Library framework is fully specified and ready for implementation.**