# Procedural Dungeon Generation (v0.10)

## Quick Start

```csharp
// 1. Initialize services
var templateLibrary = new TemplateLibrary("Data/RoomTemplates");
templateLibrary.LoadTemplates();

var biomeLibrary = new BiomeLibrary("Data/Biomes");
biomeLibrary.LoadBiomes();

var seedManager = new SeedManager();

var dungeonService = new DungeonService(templateLibrary, biomeLibrary, seedManager);

// 2. Create a procedurally generated world
var world = dungeonService.CreateProceduralWorld();

// 3. Use like normal GameWorld
var startRoom = world.GetRoom(world.StartRoomName);
Console.WriteLine(startRoom.Name);
Console.WriteLine(startRoom.Description);
```

## System Overview

### Core Components

1. **RoomTemplate** - Blueprint for generating rooms
   - Name/description templates with placeholders
   - Valid connections (which archetypes can connect)
   - Stored in `Data/RoomTemplates/*.json`

2. **DungeonGraph** - Graph structure of nodes (rooms) and edges (connections)
   - Generates main path (Start → N rooms → Boss)
   - Adds optional branching paths and secret rooms
   - Assigns compass directions to edges

3. **Dungeon** - Final product with instantiated Room objects
   - Dictionary of Room instances
   - Start and boss room tracking
   - Fully navigable with compass directions

4. **BiomeDefinition** - Thematic style guide for generation
   - Template pool
   - Descriptor categories (adjectives, details, sounds, smells)
   - Generation parameters (room count, branching probability)

### Generation Pipeline

```
Seed → DungeonGraph → Direction Assignment → Room Instantiation → GameWorld
  ↓         ↓                    ↓                    ↓                ↓
 RNG    Templates            Compass Dirs        Room Objects    Playable!
        + Biome
```

## Usage Examples

### Generate with Specific Seed

```csharp
var world = dungeonService.CreateProceduralWorld(seed: 12345);
```

### Generate with Specific Biome

```csharp
var world = dungeonService.CreateProceduralWorld(biomeId: "the_roots");
```

### Generate from Seed String

```csharp
var world = dungeonService.CreateProceduralWorldFromSeedString("WARRIOR");
```

### Regenerate for Save/Load

```csharp
// Save
int seed = dungeon.Seed;
string biomeId = dungeon.Biome;

// Load
var world = dungeonService.RegenerateDungeon(seed, biomeId, dungeonId: 1);
```

## Creating New Templates

1. Create JSON file in `Data/RoomTemplates/{category}/`
2. Follow this structure:

```json
{
  "TemplateId": "my_chamber",
  "Biome": "the_roots",
  "Size": "Medium",
  "Archetype": "Chamber",
  "NameTemplates": [
    "The {Adjective} Chamber",
    "The {Adjective} Hall"
  ],
  "Adjectives": [
    "Vast",
    "Empty",
    "Echoing"
  ],
  "DescriptionTemplates": [
    "A {Adjective} chamber. {Detail}."
  ],
  "Details": [
    "Shadows fill the corners",
    "Dust covers everything"
  ],
  "MinConnectionPoints": 2,
  "MaxConnectionPoints": 4,
  "ValidConnections": [
    "Corridor",
    "Chamber",
    "Junction"
  ],
  "Difficulty": "Easy",
  "Tags": ["Exploration"]
}
```

3. Reload templates: `templateLibrary.LoadTemplates()`

## Creating New Biomes

1. Create JSON file in `Data/Biomes/`
2. Follow `the_roots.json` structure
3. List available templates for this biome
4. Define descriptor categories (adjectives, details, etc.)
5. Set generation parameters

## Backward Compatibility

The system maintains backward compatibility:

```csharp
// Legacy handcrafted rooms (still works)
var legacyWorld = new GameWorld();

// New procedural generation
var proceduralWorld = new GameWorld(dungeon);

// Both use the same interface
var room = world.GetRoom(roomName);
```

## Architecture Notes

### Room Identification

- **Legacy rooms**: Use `Name` as dictionary key (e.g., "Entrance")
- **Procedural rooms**: Use `RoomId` as dictionary key (e.g., "room_d1_n1")
- `GetRoom(string)` works for both systems

### Direction System

Rooms are connected with compass directions:
- `room.Exits["north"]` → target room ID
- Bidirectional by default
- Secret exits marked in `NodeType.Secret`

### Seed Reproducibility

Same seed → identical dungeon every time:
- Template selection
- Graph structure
- Direction assignment
- Name/description generation

All use the same Random instance seeded consistently.

## What's NOT in v0.10

v0.10 generates **layout only** (no content population):

- ❌ No enemy spawning (planned for v0.11)
- ❌ No loot placement (planned for v0.11)
- ❌ No hazards (planned for v0.11)
- ❌ No NPCs (planned for v0.11)

Rooms are "empty shells" with names and descriptions only.

## Testing

Run the test suite:

```bash
dotnet test --filter "FullyQualifiedName~Dungeon"
```

Tests cover:
- Template validation
- Graph generation and validation
- Direction assignment
- Room instantiation
- Name/description generation
- Seed reproducibility

## Performance

- Generation time: < 100ms for 5-7 room dungeon
- Memory: ~1KB per room
- Templates cached after loading
- Thread-safe (each generation uses its own Random instance)

## Troubleshooting

**"No templates found"**
- Check `Data/RoomTemplates/` exists
- Ensure JSON files are valid
- Call `templateLibrary.LoadTemplates()`

**"No path from start to boss"**
- Bug in generation (should never happen)
- Check logs for validation errors
- Report with seed for reproduction

**"Room names have {Adjective} placeholders"**
- Template missing adjectives
- Check template validation
- Ensure adjectives list is not empty

## Future Enhancements

Planned for v0.11-v0.12:
- Enemy/NPC population
- Loot placement
- Hazard generation
- "Coherent Glitch" polish
- Multiple biomes
- Difficulty scaling

---

*For full v0.10 specification, see the design document.*
