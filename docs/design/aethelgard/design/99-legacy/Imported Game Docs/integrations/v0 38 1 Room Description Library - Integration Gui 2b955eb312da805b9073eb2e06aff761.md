# v0.38.1: Room Description Library - Integration Guide

## Overview

The Room Description Library extends the v0.38 Descriptor Framework with comprehensive room template generation. Instead of hardcoding room descriptions per biome, v0.38.1 provides:

- **15+ Base Room Templates** (Entry Hall, Corridor, Chamber, Junction, Boss Arena, etc.)
- **60+ Descriptor Fragments** (spatial, architectural, detail, atmospheric)
- **18+ Room Function Variants** (Pumping Station, Forge Hall, Cryo Chamber, etc.)
- **5 Thematic Modifiers** (Rusted, Scorched, Frozen, Crystalline, Monolithic)
- **Procedural composition** = Infinite unique room descriptions

## Quick Start

### 1. Run Database Migrations

Execute the v0.38.1 SQL scripts in order:

```bash
# 1. Schema (tables for fragments and function variants)
sqlite3 rune_and_rust.db < Data/v0.38.1_room_description_library_schema.sql

# 2. Descriptor fragments (60+ text fragments)
sqlite3 rune_and_rust.db < Data/v0.38.1_descriptor_fragments_content.sql

# 3. Room function variants (18+ chamber functions)
sqlite3 rune_and_rust.db < Data/v0.38.1_room_function_variants.sql

```

### 2. Initialize the Service

```csharp
using RuneAndRust.Persistence;
using RuneAndRust.Engine;

// Create repositories
var descriptorRepository = new DescriptorRepository(connectionString);

// Create services
var descriptorService = new DescriptorService(descriptorRepository);
var roomDescriptorService = new RoomDescriptorService(descriptorRepository, descriptorService);

```

### 3. Generate Room Descriptions

```csharp
using RuneAndRust.Core.Descriptors;

// Generate room name
var roomName = roomDescriptorService.GenerateRoomName(
    RoomArchetype.Corridor,
    "Muspelheim");

Console.WriteLine(roomName);
// Output: "The Scorched Corridor"

// Generate complete room description
var roomDescription = roomDescriptorService.GenerateRoomDescription(
    RoomArchetype.Chamber,
    "Niflheim");

Console.WriteLine(roomDescription);
// Output: "An ice-covered Cryo Chamber dominates this space. The chamber is vast, its far walls barely visible in the dim light. Cryo pods line the walls, their contents frozen in time. Many pods show critical failures. The temperature is lethally cold."

```

## Room Archetypes

v0.38.1 provides 15 room archetypes:

| Archetype | Size | Exits | Danger | Purpose |
| --- | --- | --- | --- | --- |
| EntryHall | Medium | 1-2 | Low | Safe starting room |
| Corridor | Small | 2 | Medium | Linear transit |
| Chamber | Large | 1-4 | High | Combat/exploration |
| Junction | Medium | 3-4 | Medium | Branching decisions |
| BossArena | XLarge | 1 | Extreme | Boss encounters |
| SecretRoom | Small | 1 | Low | Hidden rewards |
| VerticalShaft | Medium | 2 | High | Vertical transit |
| MaintenanceHub | Medium | 2-4 | Medium | Utility junction |
| StorageBay | Large | 1-2 | Low | Salvage area |
| ObservationPlatform | Medium | 1-2 | Low | Tactical vantage |
| PowerStation | Large | 1-3 | High | Energy facility |
| Laboratory | Medium | 1-2 | Medium | Research facility |
| Barracks | Medium | 1-2 | Medium | Military quarters |
| ForgeCharnber | Large | 1-2 | High | Muspelheim forges |
| CryoVault | Large | 1-2 | Medium | Niflheim cryogenics |

## Example Generated Rooms

### Example 1: The Corroded Entry Hall (The Roots)

```
Base: Entry_Hall_Base
Modifier: Rusted (The Roots)

Generated Name: "The Corroded Entry Hall"

Generated Description:
"You enter a corroded entry hall. The ceiling presses low overhead, and the walls feel uncomfortably close. Corroded metal plates form the walls, held together by massive rivets. Rust streaks mark the surfaces like old blood. Runic glyphs flicker weakly on the walls, their light stuttering. The air smells of rust and stale water."

```

### Example 2: The Scorched Forge (Muspelheim)

```
Base: Forge_Chamber_Base
Modifier: Scorched (Muspelheim)
Function: Forge Hall

Generated Name: "The Scorched Forge"

Generated Description:
"A scorched forge dominates this chamber. The room is cavernous, your footsteps echoing into the distance. The ceiling is a tangle of exposed conduits and pipes. The forge equipment sits cold and abandoned. Massive anvils and quenching tanks dominate the space. The ambient temperature is dangerously high. The air is thick with the smell of brimstone and superheated metal."

```

### Example 3: The Frozen Cryo Vault (Niflheim)

```
Base: Cryo_Vault_Base
Modifier: Frozen (Niflheim)
Function: Cryo Chamber

Generated Name: "The Frozen Cryo Vault"

Generated Description:
"An ice-covered cryo vault preserves hundreds of cryogenic suspension pods. The chamber is vast, its far walls barely visible in the dim light. The ceiling is studded with defunct light panels. Everything is encased in thick sheets of ancient ice. The cryogenic systems are still partially functional. Frostbite is a constant danger. The air is bone-chillingly cold."

```

### Example 4: The Crystalline Laboratory (Alfheim)

```
Base: Laboratory_Base
Modifier: Crystalline (Alfheim)
Function: Crystallography Lab

Generated Name: "The Crystalline Laboratory"

Generated Description:
"A crystalline Crystallography Lab contains Aetheric containment vessels. The space extends dramatically upward, disappearing into darkness above. Smooth, seamless walls suggest advanced pre-Glitch fabrication. The research here focused on reality manipulation. This laboratory studied Aetheric crystal formations and properties. The air crackles with uncontrolled Aether."

```

## Integration with Dynamic Room Engine (v0.10)

### Before v0.38.1: Hardcoded Descriptions

```csharp
// Old approach - per-biome hardcoded descriptions
public Room InstantiateRoom(DungeonNode node, BiomeDefinition biome)
{
    var name = biome.BiomeId switch
    {
        "Muspelheim" => "The Scorched Corridor",
        "Niflheim" => "The Frozen Passage",
        _ => "The Corridor"
    };

    var description = biome.BiomeId switch
    {
        "Muspelheim" => "A corridor radiating intense heat...",
        "Niflheim" => "A corridor coated in ice...",
        _ => "A nondescript corridor."
    };

    return new Room { Name = name, Description = description };
}

```

### After v0.38.1: Procedural Generation

```csharp
// New approach - procedural using RoomDescriptorService
public Room InstantiateRoom(DungeonNode node, BiomeDefinition biome)
{
    // Generate unique name and description
    var name = _roomDescriptorService.GenerateRoomName(
        node.Archetype,
        biome.BiomeId);

    var description = _roomDescriptorService.GenerateRoomDescription(
        node.Archetype,
        biome.BiomeId,
        room);

    return new Room
    {
        Name = name,
        Description = description,
        Archetype = node.Archetype,
        Size = node.Archetype.GetExpectedSize()
    };
}

```

**Benefits:**

- Infinite variety (random fragment selection)
- Consistent theming (modifier-based)
- No duplication (reusable fragments)
- Easy to extend (add fragments/modifiers)

## Fragment System

Descriptor fragments are categorized and tagged for intelligent selection:

### Categories

1. **SpatialDescriptor**: Room dimensions and feel
    - "The ceiling presses low overhead"
    - "The chamber is vast"
    - "The space extends dramatically upward"
2. **ArchitecturalFeature**: Structural elements
    - Walls: "Corroded metal plates form the walls"
    - Ceilings: "The ceiling is a tangle of exposed conduits"
    - Floors: "The floor is corrugated metal grating"
3. **Detail**: Environmental storytelling
    - Decay: "Rust streaks mark the surfaces like old blood"
    - Runes: "Runic glyphs flicker weakly on the walls"
    - Activity: "Fresh tracks mar the dust on the floor"
4. **Atmospheric**: Sensory details
    - "smells of rust and stale water"
    - "is thick with the smell of brimstone"
    - "carries the sharp scent of ozone"
5. **Direction**: Spatial orientation
    - "before you, narrowing into darkness"
    - "upward into the darkness above"
    - "in multiple directions"

### Tag-Based Selection

Fragments are tagged for intelligent filtering:

```csharp
// Example fragment with tags
{
    "category": "SpatialDescriptor",
    "text": "The ceiling presses low overhead",
    "tags": ["Small", "Narrow", "Corridor"],
    "weight": 1.0
}

```

The service automatically selects fragments matching:

- Room archetype tags (from base template)
- Biome tags (from modifier)
- Room size/type

This ensures coherent descriptions:

- Small corridors get "cramped" spatial descriptors
- Large chambers get "vast" descriptors
- Muspelheim rooms get fire-related details

## Room Function Variants

Chamber-type rooms can have functional descriptors:

```csharp
// Example function variant
{
    "function_name": "Pumping Station",
    "function_detail": "manages hydraulic systems throughout the facility",
    "biome_affinity": ["The_Roots", "Muspelheim"],
    "archetype": "Chamber"
}

```

Automatically selected based on:

- Room archetype (Chamber, PowerStation, Laboratory)
- Biome affinity (some functions only in specific biomes)

## Advanced Usage

### Custom Room Generation

```csharp
// Generate description with specific function
var function = descriptorRepository.GetRandomFunctionVariant("PowerStation", "Muspelheim");

var description = roomDescriptorService.GenerateRoomDescription(
    RoomArchetype.PowerStation,
    "Muspelheim");

// Will use "Geothermal Tap Station" or similar Muspelheim-appropriate function

```

### Batch Generation

```csharp
// Generate multiple rooms for a dungeon
var biome = "Niflheim";
var archetypes = new[]
{
    RoomArchetype.EntryHall,
    RoomArchetype.Corridor,
    RoomArchetype.Chamber,
    RoomArchetype.Junction,
    RoomArchetype.BossArena
};

foreach (var archetype in archetypes)
{
    var name = roomDescriptorService.GenerateRoomName(archetype, biome);
    var description = roomDescriptorService.GenerateRoomDescription(archetype, biome);

    Console.WriteLine($"{name}:\\n{description}\\n");
}

// Output:
// The Frozen Entry Hall: ...
// The Frozen Corridor: ...
// The Frozen Cryo Chamber: ...
// The Frozen Junction: ...
// The Frozen Arena: ...

```

## Statistics

v0.38.1 delivers:

- **15 Base Room Templates**
- **60+ Descriptor Fragments**
    - 8 spatial descriptors
    - 12 architectural features
    - 30+ detail fragments
    - 6 atmospheric details
    - 6 direction descriptors
- **18 Room Function Variants**
- **5 Thematic Modifiers**

**Total Combinations**: 15 templates × 5 modifiers × 18 functions = **1,350+ unique room types**

With random fragment selection, the actual variety is exponentially higher.

## Troubleshooting

### Issue: Generic/boring descriptions

**Check:**

- Are descriptor fragments populated? Run verification query:
    
    ```sql
    SELECT category, COUNT(*) FROM Descriptor_Fragments GROUP BY category;
    
    ```
    
- Are fragments tagged correctly?
- Is biome modifier found?

### Issue: Template placeholders not replaced

**Check:**

- All required fragments exist for the room archetype
- Subcategory fragments exist for specialized archetypes (Forge, Cryo, etc.)
- Logs for missing fragment warnings

```csharp
// Enable debug logging
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .CreateLogger();

```

### Issue: Same description every time

**Verify:**

- Fragment weights are varied (not all 1.0)
- Multiple fragments exist per category
- Random selection is working

```csharp
// Check fragment count
var fragments = descriptorRepository.GetDescriptorFragments("SpatialDescriptor");
Console.WriteLine($"Spatial descriptors available: {fragments.Count}");

```

## Next Steps

After v0.38.1:

- **v0.38.2**: Environmental Feature Catalog (hazards, terrain, cover)
- **v0.38.3**: Interactive Object Repository (containers, levers, doors)
- **v0.38.4**: Atmospheric Descriptor System (lighting, sound, psychic presence)
- **v0.38.5**: Loot & Resource Templates (resource nodes, salvage)

## References

- **Parent Spec**: `docs/v0.38_descriptor_framework_integration.md`
- **Database Schema**: `Data/v0.38.1_room_description_library_schema.sql`
- **Descriptor Fragments**: `Data/v0.38.1_descriptor_fragments_content.sql`
- **Room Functions**: `Data/v0.38.1_room_function_variants.sql`
- **Service Implementation**: `RuneAndRust.Engine/RoomDescriptorService.cs`

---

**v0.38.1 Room Description Library Status**: ✅ Complete
**Next**: v0.38.2 - Environmental Feature Catalog