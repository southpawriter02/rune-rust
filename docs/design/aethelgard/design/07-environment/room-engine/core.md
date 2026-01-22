---
id: SPEC-ROOMENGINE-CORE
title: "Room Engine Core — Layout Generation"
version: 1.0
status: draft
last-updated: 2025-12-07
related-files:
  - path: "data/templates/"
    status: Active
  - path: "RuneAndRust.Engine/DungeonGenerator.cs"
    status: Planned
---

# Room Engine Core — Layout Generation

---

## 1. Overview

The Room Engine generates procedural dungeon layouts using:
- **Graph-based structure** — Rooms as nodes, connections as edges
- **Template system** — 20+ handcrafted templates with variation points
- **Seed-based reproducibility** — Same seed = identical dungeon
- **Biome theming** — Templates filtered by active biome

---

## 2. Generation Pipeline

```
┌─────────────────────────────────────────────────────────────┐
│ INPUT: Seed + Biome + Difficulty                            │
└─────────────────────────────────────────────────────────────┘
         │
         ▼
┌─────────────────────────────────────────────────────────────┐
│ PHASE 1: Graph Generation                                   │
│   • Create start node (EntryHall)                           │
│   • Generate main path (5-7 rooms → Boss)                   │
│   • Add branching paths (40% probability, 1-2 rooms)        │
│   • Add secret rooms (20% probability)                      │
└─────────────────────────────────────────────────────────────┘
         │
         ▼
┌─────────────────────────────────────────────────────────────┐
│ PHASE 2: Template Selection                                 │
│   • Filter by biome affinity                                │
│   • Select based on node type (Main/Branch/Secret/Boss)     │
│   • Avoid repetition (variety logic)                        │
└─────────────────────────────────────────────────────────────┘
         │
         ▼
┌─────────────────────────────────────────────────────────────┐
│ PHASE 3: Direction Assignment                               │
│   • BFS traversal from start                                │
│   • Assign compass directions (N/S/E/W)                     │
│   • Bidirectional connections                               │
└─────────────────────────────────────────────────────────────┘
         │
         ▼
┌─────────────────────────────────────────────────────────────┐
│ PHASE 4: Room Instantiation                                 │
│   • Apply template to generate name                         │
│   • Fill placeholders with biome descriptors                │
│   • Build exit dictionary                                   │
└─────────────────────────────────────────────────────────────┘
         │
         ▼
┌─────────────────────────────────────────────────────────────┐
│ OUTPUT: Dungeon (List<Room> + Connections)                  │
└─────────────────────────────────────────────────────────────┘
```

---

## 3. Core Data Structures

### 3.1 Sector Types

The room engine generates two distinct sector types:

```csharp
public enum SectorType
{
    Dungeon,     // Combat-focused exploration (default)
    Settlement   // Social interaction, commerce, services
}
```

| Sector Type | Purpose | Room Types | Encounters |
|-------------|---------|------------|------------|
| **Dungeon** | Combat exploration | Combat-focused archetypes | Enemy spawns, hazards |
| **Settlement** | Settlement interaction | Social-focused archetypes | NPCs, services, quests |

> [!NOTE]
> Settlement generation uses a parallel archetype system. See [Settlements](../settlements.md) for Social Sector architecture.

### 3.2 DungeonNode

Represents a room in the abstract graph.

```csharp
public record DungeonNode(
    string NodeId,
    RoomTemplate Template,
    NodeType Type,           // Start, Main, Branch, Secret, Boss
    int Depth                // Distance from start
);

public enum NodeType
{
    Start,
    Main,
    Branch,
    Secret,
    Boss
}
```

### 3.3 DungeonEdge

Represents a connection between rooms.

```csharp
public record DungeonEdge(
    string FromNodeId,
    string ToNodeId,
    Direction Direction,
    bool IsBidirectional
);

public enum Direction
{
    North,
    South,
    East,
    West,
    Up,      // Vertical connections (v0.39+)
    Down
}
```

### 3.4 DungeonGraph

Container for the complete dungeon structure.

```csharp
public class DungeonGraph
{
    public string Seed { get; }
    public string BiomeId { get; }
    public IReadOnlyList<DungeonNode> Nodes { get; }
    public IReadOnlyList<DungeonEdge> Edges { get; }
    
    public DungeonNode GetNode(string nodeId);
    public IEnumerable<DungeonEdge> GetEdgesFrom(string nodeId);
    public bool IsConnected(string fromId, string toId);
}
```

---

## 4. Room Templates

### 4.1 Template Structure

Templates are JSON files with variation points:

```json
{
  "TemplateId": "rust_choked_corridor",
  "Biome": "the_roots",
  "Size": "Medium",
  "Archetype": "Corridor",
  "NameTemplates": [
    "The {Adjective} Corridor",
    "The {Adjective} Passage"
  ],
  "Adjectives": [
    "Rust-Choked",
    "Corroded",
    "Decaying"
  ],
  "DescriptionTemplates": [
    "A {Adjective} corridor stretches before you. {Detail}."
  ],
  "Details": [
    "Rust flakes fall like snow from the ceiling",
    "Condensation pools on the uneven floor"
  ],
  "MinConnectionPoints": 2,
  "MaxConnectionPoints": 3,
  "ValidConnections": ["Corridor", "Chamber", "Junction"],
  "Difficulty": "Easy",
  "Tags": ["Connector", "Linear"]
}
```

### 4.2 Room Archetypes (6)

| Archetype | Size | Exits | Purpose |
|-----------|------|-------|---------|
| **EntryHall** | Medium | 1-2 | Sector entry point |
| **Corridor** | Small-Medium | 2-3 | Linear connectors |
| **Chamber** | Large | 2-4 | Combat, exploration |
| **Junction** | Medium | 3-4 | Branching points |
| **BossArena** | XLarge | 1 | Boss encounters |
| **SecretRoom** | Small | 1 | Hidden rewards |

### 4.3 Template Catalog (20)

| Archetype | Templates | Location |
|-----------|-----------|----------|
| EntryHall | 3 | [data/templates/](file:///Volumes/GitHub/github/southpawriter02/r-r/data/templates/) |
| Corridor | 6 | [data/templates/](file:///Volumes/GitHub/github/southpawriter02/r-r/data/templates/) |
| Chamber | 5 | [data/templates/](file:///Volumes/GitHub/github/southpawriter02/r-r/data/templates/) |
| Junction | 2 | [data/templates/](file:///Volumes/GitHub/github/southpawriter02/r-r/data/templates/) |
| BossArena | 2 | [data/templates/](file:///Volumes/GitHub/github/southpawriter02/r-r/data/templates/) |
| SecretRoom | 2 | [data/templates/](file:///Volumes/GitHub/github/southpawriter02/r-r/data/templates/) |

---

## 5. Seed Management

### 5.1 Seed Generation

```csharp
public class SeedManager
{
    public string GenerateSeed()
    {
        // Timestamp-based: "2025-12-07-143052"
        return DateTime.UtcNow.ToString("yyyy-MM-dd-HHmmss");
    }
    
    public int ParseSeed(string seed)
    {
        // Convert to deterministic integer for Random()
        return seed.GetHashCode();
    }
}
```

### 5.2 Reproducibility Rules

All randomness uses a **single seeded Random instance**:

```csharp
var random = new Random(seedManager.ParseSeed(seed));

// Template selection
var template = templates[random.Next(templates.Count)];

// Adjective selection
var adjective = template.Adjectives[random.Next(template.Adjectives.Length)];

// Branching probability (40%)
var hasBranch = random.NextDouble() < 0.40;
```

---

## 6. Generation Algorithm

### 6.1 Main Path Generation

```csharp
public DungeonGraph Generate(string seed, string biomeId, int targetRooms)
{
    var random = new Random(ParseSeed(seed));
    var nodes = new List<DungeonNode>();
    var edges = new List<DungeonEdge>();
    
    // 1. Create start node
    var start = CreateNode("start", NodeType.Start, GetTemplate("EntryHall"));
    nodes.Add(start);
    
    // 2. Generate main path (targetRooms - 2 for start/boss)
    var current = start;
    for (int i = 0; i < targetRooms - 2; i++)
    {
        var next = CreateMainPathNode(current, i + 1, random);
        nodes.Add(next);
        edges.Add(CreateEdge(current, next));
        current = next;
    }
    
    // 3. Add boss room
    var boss = CreateNode("boss", NodeType.Boss, GetTemplate("BossArena"));
    nodes.Add(boss);
    edges.Add(CreateEdge(current, boss));
    
    // 4. Add branches (40% per main path node)
    foreach (var mainNode in nodes.Where(n => n.Type == NodeType.Main))
    {
        if (random.NextDouble() < 0.40)
        {
            AddBranch(mainNode, nodes, edges, random);
        }
    }
    
    // 5. Add secret rooms (20% per branch endpoint)
    foreach (var branchEnd in GetBranchEndpoints(nodes, edges))
    {
        if (random.NextDouble() < 0.20)
        {
            AddSecretRoom(branchEnd, nodes, edges, random);
        }
    }
    
    return new DungeonGraph(seed, biomeId, nodes, edges);
}
```

### 6.2 Template Variety Logic

Prevents repetitive room selection:

```csharp
private RoomTemplate SelectTemplate(
    RoomArchetype archetype, 
    string biomeId,
    IReadOnlyList<RoomTemplate> recentlyUsed,
    Random random)
{
    var candidates = _templates
        .Where(t => t.Archetype == archetype)
        .Where(t => t.Biome == biomeId || t.Biome == "universal")
        .Where(t => !recentlyUsed.Contains(t))  // Avoid recent
        .ToList();
    
    if (candidates.Count == 0)
    {
        // Fallback: allow repeats if necessary
        candidates = _templates
            .Where(t => t.Archetype == archetype)
            .ToList();
    }
    
    return candidates[random.Next(candidates.Count)];
}
```

---

## 7. Direction Assignment

### 7.1 BFS Traversal

Directions assigned via breadth-first traversal:

```csharp
public void AssignDirections(DungeonGraph graph)
{
    var queue = new Queue<DungeonNode>();
    var visited = new HashSet<string>();
    var availableDirections = new Dictionary<string, List<Direction>>();
    
    queue.Enqueue(graph.StartNode);
    visited.Add(graph.StartNode.NodeId);
    
    while (queue.Count > 0)
    {
        var current = queue.Dequeue();
        var directions = GetAvailableDirections(current);
        
        foreach (var edge in graph.GetEdgesFrom(current.NodeId))
        {
            if (!visited.Contains(edge.ToNodeId))
            {
                var direction = directions[0];
                directions.RemoveAt(0);
                
                edge.Direction = direction;
                edge.ReverseDirection = GetOpposite(direction);
                
                visited.Add(edge.ToNodeId);
                queue.Enqueue(graph.GetNode(edge.ToNodeId));
            }
        }
    }
}
```

### 7.2 Bidirectional Connections

Every edge creates a reverse connection:

| Forward | Reverse |
|---------|---------|
| North | South |
| East | West |
| Up | Down |

---

## 8. Room Instantiation

### 8.1 Name Generation

```csharp
public string GenerateRoomName(RoomTemplate template, Random random)
{
    var nameTemplate = template.NameTemplates[random.Next(template.NameTemplates.Length)];
    var adjective = template.Adjectives[random.Next(template.Adjectives.Length)];
    
    return nameTemplate.Replace("{Adjective}", adjective);
}
// Example: "The Rust-Choked Corridor"
```

### 8.2 Description Generation

```csharp
public string GenerateDescription(RoomTemplate template, Random random)
{
    var descTemplate = template.DescriptionTemplates[random.Next(...)];
    var adjective = template.Adjectives[random.Next(...)];
    var detail = template.Details[random.Next(...)];
    
    return descTemplate
        .Replace("{Adjective}", adjective.ToLower())
        .Replace("{Detail}", detail);
}
// Example: "A rust-choked corridor stretches before you. 
//           Rust flakes fall like snow from the ceiling."
```

---

## 9. Integration Points

### 9.1 Services

| Service | Interface | Purpose |
|---------|-----------|---------|
| DungeonGenerator | `IDungeonGenerator` | Graph creation |
| TemplateLibrary | `ITemplateLibrary` | Template loading |
| SeedManager | `ISeedManager` | Seed handling |
| DirectionAssigner | `IDirectionAssigner` | Compass directions |
| RoomInstantiator | `IRoomInstantiator` | Template → Room |

### 9.2 Downstream Dependencies

- **Spatial Layout** (Phase 2) — Converts graph to 3D coordinates
- **Population** (Phase 4) — Spawns content per room budget
- **Descriptors** (Phase 3) — Enhanced text generation

---

## 10. Phased Implementation Guide

### Phase 1: Data Structures
- [ ] **Templates**: Define `RoomTemplate` and `BiomeDefinition` classes/records.
- [ ] **Graph**: Implement `DungeonNode` and `DungeonEdge` data structures.
- [ ] **JSON**: Create initial JSON template files in `data/templates/`.

### Phase 2: Core Algorithms
- [ ] **Graph**: Implement `IDungeonGenerator.GenerateGraph()` for node creation.
- [ ] **Layout**: Implement `GenerateMainPath()` and `AddBranches()`.
- [ ] **Directions**: Implement BFS-based `AssignDirections()`.

### Phase 3: Content Instantiation
- [ ] **Templates**: Implement `ITemplateLibrary` to load/select templates.
- [ ] **Rooms**: Implement `IRoomInstantiator` to generate names/descriptions.
- [ ] **Seed**: Implement `SeedManager` for deterministic randomness.

### Phase 4: Integration
- [ ] **Service**: Wire up `DungeonGenerator` into the DI container.
- [ ] **CLI**: Add specific `Generate` command to CLI for testing output.

---

## 11. Testing Requirements

### 11.1 Unit Tests
- [ ] **Seeding**: `Generate("seed1")` vs `Generate("seed1")` -> Returns identical graphs.
- [ ] **Graph**: Main path length equals target length (-2).
- [ ] **Connections**: All nodes reachable from Start (Graph is fully connected).
- [ ] **Templates**: Invalid/Missing templates throws explicit `TemplateNotFoundException`.

### 11.2 Integration Tests
- [ ] **Loader**: Can load all JSON templates from disk without error.
- [ ] **Benchmark**: Generate 100 dungeons < 500ms total.

### 11.3 Manual QA
- [ ] **Visual**: Use debug visualizer to render graph DOT file.
- [ ] **Text**: Verify descriptions make grammatical sense.

---

## 12. Logging Requirements

**Reference:** [logging.md](../../00-project/logging.md)

### 12.1 Log Events
| Event | Level | Message Template | Properties |
|-------|-------|------------------|------------|
| Start | Info | "Generating dungeon with seed {Seed} (Target: {Rooms} rooms)." | `Seed`, `Rooms` |
| Phase | Debug | "Phase {Phase} complete. Nodes: {Count}." | `Phase`, `Count` |
| Branch | Debug | "Created branch at {NodeId}." | `NodeId` |
| Error | Error | "Failed to select template for {Archetype} in {Biome}." | `Archetype`, `Biome` |

---

## 13. Related Documentation
| Document | Purpose |
|----------|---------|
| [Spatial Layout](spatial-layout.md) | Physical coordinates |
| [Settlements](../settlements.md) | Social Sector architecture |
| [Descriptor System](descriptors.md) | Text generation |
| [Ambient Conditions](../ambient-conditions.md) | Room-wide environmental modifiers |
| [Dynamic Hazards](../dynamic-hazards.md) | Interactive battlefield elements |
| [Game Loop](../../01-core/game-loop.md) | Exploration context |
| [Encounter Generation](../../03-combat/encounter-generation.md) | Spawn budget per room archetype |
| [Biome Overview](../biomes/biomes-overview.md) | Faction spawn pools by biome |

---

## 14. Changelog
| Version | Date | Changes |
|---------|------|---------|
| 1.0 | 2025-12-07 | Initial specification |
| 1.1 | 2025-12-14 | Standardized with Phased Guide, Testing, and Logging |
| 1.2 | 2025-12-14 | Added SectorType distinction for Settlement support |
