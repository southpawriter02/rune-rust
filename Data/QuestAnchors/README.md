# Quest Anchor System (v0.11)

## Overview

Quest Anchors are **handcrafted rooms** that are inserted into procedurally generated dungeons for narrative purposes. They allow you to guarantee specific story-critical rooms appear while still maintaining the benefits of procedural generation.

## Use Cases

- **Quest Objectives**: Rooms containing quest items or goals
- **Boss Encounters**: Special boss arenas with custom hazards/terrain
- **NPC Encounters**: Safe zones with merchants, quest givers, or narrative moments
- **Narrative Moments**: Cutscene locations or lore revelations
- **Checkpoints**: Save points or rest areas

## How It Works

1. **Design a Handcrafted Room** - Create a JSON file defining the room's enemies, hazards, terrain, loot, NPCs
2. **Create a Quest Anchor** - Define constraints for where/when the room can appear in the dungeon
3. **Generate with Blueprint** - Use `DungeonBlueprint` to request specific anchors
4. **Insertion** - The `AnchorInserter` replaces procedural nodes with your handcrafted rooms
5. **Population** - Procedural population **skips** handcrafted rooms (preserves your design)

## File Structure

```
Data/QuestAnchors/
├── jotun_reader_archive.json      # Example: Quest objective room
├── servitor_command_node.json     # Example: Boss arena
├── abandoned_workshop.json        # Example: NPC encounter (safe zone)
├── example_blueprint.json         # Example: How to use anchors in generation
└── README.md                      # This file
```

## Creating a Handcrafted Room

**File:** `Data/QuestAnchors/my_room.json`

```json
{
  "RoomId": "my_custom_room",
  "Name": "[My Custom Room]",
  "Description": "Detailed description of the room's appearance and atmosphere.",
  "Archetype": "Chamber",
  "SuggestedNodeType": "Main",
  "NarrativeText": "Optional: Story context or cutscene text.",
  "FlavorText": "Optional: Additional atmospheric details.",
  "EnemyIds": ["enemy_01", "enemy_02"],
  "HazardIds": ["steam_vent_custom"],
  "TerrainIds": ["collapsed_pillar_01"],
  "LootIds": ["quest_item_schematics"],
  "ConditionIds": ["psychic_resonance"],
  "NPCIds": [],
  "IsSafeZone": false,
  "IsBossArena": false,
  "HasQuestObjective": true,
  "QuestObjectiveId": "retrieve_schematics",
  "PreferredExits": {
    "North": "Main",
    "South": "Corridor"
  }
}
```

### Key Properties

- **EnemyIds**: Specific enemies to spawn (won't be procedurally populated)
- **HazardIds**: Specific hazards (e.g., `steam_vent_custom`)
- **TerrainIds**: Specific terrain features
- **LootIds**: Specific loot nodes (quest items, etc.)
- **IsSafeZone**: If `true`, no combat allowed (merchants, rest areas)
- **IsBossArena**: If `true`, marks as boss room
- **HasQuestObjective**: If `true`, contains a quest goal

## Using Quest Anchors in Generation

### Step 1: Create a DungeonBlueprint

```csharp
var blueprint = new DungeonBlueprint
{
    Seed = 42,
    TargetRoomCount = 7,
    BiomeId = "the_roots",
    QuestId = "find_schematics",
    RequiredAnchors = new List<QuestAnchor>
    {
        new QuestAnchor
        {
            AnchorId = "quest_archive",
            Name = "[Jötun-Reader Archive]",
            HandcraftedRoomId = "jotun_reader_archive",
            IsMandatory = true,
            PreferredNodeType = NodeType.Main,
            Constraints = new QuestAnchorConstraints
            {
                MinDepth = 3,
                MaxDepth = 5,
                MustBeOnMainPath = true
            }
        }
    }
};
```

### Step 2: Generate with Blueprint

```csharp
// Initialize services
var roomLibrary = new HandcraftedRoomLibrary("Data/QuestAnchors");
roomLibrary.LoadRooms();

var anchorInserter = new AnchorInserter(roomLibrary);

var generator = new DungeonGenerator(
    templateLibrary,
    populationPipeline,
    anchorInserter  // NEW: Pass anchor inserter
);

// Generate dungeon
var dungeon = generator.GenerateFromBlueprint(blueprint, dungeonId: 1, biome: theRoots);
```

## Quest Anchor Constraints

Control where anchors can appear:

```csharp
Constraints = new QuestAnchorConstraints
{
    // Depth constraints
    MinDepth = 3,              // Must be at least 3 rooms from start
    MaxDepth = 5,              // Must be at most 5 rooms from start

    // Position constraints
    MustBeOnMainPath = true,   // Must be on critical path to boss
    CanBeOnBranchPath = false, // Cannot be on optional branch
    CanBeSecret = false,       // Cannot be a secret room

    // Never constraints
    NeverAsStartRoom = true,   // Cannot replace the entry hall
    NeverAsBossRoom = false,   // Can replace the boss room (for custom bosses)

    // Ordering constraints
    MustAppearAfter = ["other_anchor_id"]  // Must come after another anchor
}
```

## Examples

### Example 1: Quest Objective Room

A room containing quest-critical item, guarded by specific enemies.

**See:** `jotun_reader_archive.json`

- **Purpose**: Quest objective location
- **Depth**: 3-5 (mid-dungeon)
- **Enemies**: 2 Servitor Archivists, 1 Data Wraith
- **Loot**: Lost Schematics (quest item)

### Example 2: Boss Arena

Custom boss encounter with handcrafted hazards and terrain.

**See:** `servitor_command_node.json`

- **Purpose**: Boss encounter
- **Depth**: 6-7 (end of dungeon)
- **Enemies**: Servitor Overseer (boss only)
- **Hazards**: Unstable Ceiling, Live Power Conduit
- **Terrain**: Multiple cover pillars, elevated platform

### Example 3: NPC Safe Zone

Merchant/quest giver location with no combat.

**See:** `abandoned_workshop.json`

- **Purpose**: NPC encounter
- **Depth**: 2-4 (mid-dungeon)
- **Enemies**: None (IsSafeZone = true)
- **NPCs**: Grelda Ironbraid (tinkerer/merchant)

## Integration with Procedural Population

**Key Feature**: Handcrafted rooms are **automatically skipped** by the procedural population pipeline.

When you mark a room with `Room.IsHandcrafted = true`, the following spawners skip it:
- `DormantProcessSpawner` - Won't add random enemies
- `HazardSpawner` - Won't add random hazards
- `TerrainSpawner` - Won't add random terrain
- `LootSpawner` - Won't add random loot
- `ConditionApplier` - Won't add random conditions

This ensures your custom design is preserved exactly as you specified.

## Validation

The system validates blueprints before generation:

```csharp
var (isValid, errors) = blueprint.Validate();
if (!isValid)
{
    foreach (var error in errors)
    {
        Console.WriteLine($"Blueprint error: {error}");
    }
}
```

**Validation Checks:**
- ✅ Enough room budget for mandatory anchors
- ✅ No circular dependencies (A before B, B before A)
- ✅ All HandcraftedRoomId references exist
- ✅ Handcrafted rooms pass their own validation

## Best Practices

1. **Use Sparingly** - Quest Anchors override procedural generation. Use them only for narrative-critical rooms.

2. **Prefer Optional** - Set `IsMandatory = false` for anchors that enhance but aren't required (like NPC encounters).

3. **Depth Ranges** - Use `MinDepth` and `MaxDepth` to ensure pacing (e.g., bosses at depth 6+).

4. **Test Seeds** - Generate with multiple seeds to verify anchors insert correctly in various graph topologies.

5. **Name Convention** - Use `[Brackets]` for Quest Anchor room names to distinguish from procedural rooms.

6. **Backup Plan** - If an optional anchor can't be inserted (no valid location), generation continues without it.

## Troubleshooting

**Problem**: "Failed to insert mandatory Quest Anchor"
- **Cause**: No nodes meet the anchor's constraints (e.g., asking for depth 10 in a 7-room dungeon)
- **Solution**: Relax constraints or increase `TargetRoomCount`

**Problem**: Handcrafted room is getting procedurally populated
- **Cause**: `Room.IsHandcrafted` flag not set
- **Solution**: Verify `HandcraftedRoom.Instantiate()` sets the flag

**Problem**: Anchor always appears in the same location
- **Cause**: Only one node meets the constraints
- **Solution**: Widen `MinDepth`/`MaxDepth` range or allow branch paths

## Future Enhancements

**Planned for v0.12:**
- Blueprint JSON deserialization (load blueprints from files)
- Quest Anchor templating (reusable anchor definitions)
- Multi-anchor quests (chains of rooms with ordering)
- Conditional anchors (appear only if player has certain items)

---

**v5.0 Compliance**: All Quest Anchors respect the 800-year decay aesthetic. Handcrafted rooms should feel like discovered Pre-Glitch spaces with malfunctioning systems, not brand-new construction.
