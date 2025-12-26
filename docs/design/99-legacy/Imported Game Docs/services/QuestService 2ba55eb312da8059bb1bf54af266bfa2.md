# QuestService

Parent item: Service Architecture Overview (Service%20Architecture%20Overview%202ba55eb312da80a18965d6f5e87a15ec.md)

> File: RuneAndRust.Engine/QuestService.csVersion: v0.8, v0.9, v0.14, v0.35
Purpose: Manages quest lifecycle, objective tracking, and reward distribution
> 

## Overview

`QuestService` handles all aspects of the quest system including:

- Loading quest definitions from JSON files
- Offering quests to players based on eligibility
- Tracking objective progress through game events
- Granting rewards on quest completion
- Territory/faction integration for quest actions

## Dependencies

```csharp
public QuestService(
    string dataPath = "Data/Quests",
    CurrencyService? currencyService = null,      // v0.9 - currency rewards
    TerritoryService? territoryService = null     // v0.35 - faction influence
)

```

| Dependency | Purpose | Optional |
| --- | --- | --- |
| `CurrencyService` | Grant currency rewards (Dvergr Cogs) | Yes (v0.9+) |
| `TerritoryService` | Record faction influence from quest completion | Yes (v0.35+) |

## Quest Lifecycle

```
                    ┌─────────────────┐
                    │   NotStarted    │
                    │  (in database)  │
                    └────────┬────────┘
                             │ OfferQuest()
                             ▼
                    ┌─────────────────┐
                    │    Available    │  Player can see but hasn't accepted
                    └────────┬────────┘
                             │ AcceptQuest()
                             ▼
                    ┌─────────────────┐
                    │     Active      │  Objectives being tracked
                    └────────┬────────┘
                             │ All objectives complete
                             ▼
                    ┌─────────────────┐
                    │    Complete     │  Ready to turn in
                    └────────┬────────┘
                             │ TurnInQuest()
                             ▼
                    ┌─────────────────┐
                    │    TurnedIn     │  Rewards granted, quest finished
                    └─────────────────┘

```

## Quest Status Enum

```csharp
public enum QuestStatus
{
    NotStarted,    // Quest available but not accepted
    Available,     // Quest offered to player (v0.14)
    Active,        // Quest accepted and in progress
    Complete,      // Objectives done, not turned in (v0.14)
    Completed,     // Legacy: quest turned in
    TurnedIn,      // Quest turned in, rewards claimed (v0.14)
    Failed,        // Quest failed (not used in v0.14)
    Abandoned      // Player abandoned quest (v0.14)
}

```

## Public Methods

### Quest Database

### LoadQuestDatabase

```csharp
public void LoadQuestDatabase()

```

Loads all quest definitions from JSON files in the data directory.

**File Format:** `Data/Quests/*.json`

**Example Quest JSON:**

```json
{
  "Id": "roots_scavenge_01",
  "Title": "Scavenger's Pact",
  "Description": "Collect scrap metal from the ruins.",
  "GiverNpcId": "merchant_kjartan",
  "GiverNpcName": "Kjartan",
  "Type": "Side",
  "Category": "Retrieval",
  "MinimumLegend": 0,
  "Objectives": [
    {
      "Description": "Collect Scrap Metal",
      "Type": "CollectItem",
      "TargetId": "scrap_metal",
      "Required": 5,
      "Current": 0
    }
  ],
  "Reward": {
    "Experience": 50,
    "Currency": 25,
    "ReputationChange": 10,
    "Faction": "RustClans"
  }
}

```

---

### Quest Offering & Acceptance

### OfferQuest

```csharp
public bool OfferQuest(string questId, PlayerCharacter player)

```

Makes a quest available to the player if eligible.

**Eligibility Checks:**

1. Quest exists in database
2. Player meets minimum Legend requirement
3. Player has completed all prerequisite quests
4. Quest not already active or completed

**Returns:** `true` if quest was offered, `false` otherwise

---

### CanOfferQuest

```csharp
public bool CanOfferQuest(PlayerCharacter player, Quest quest)

```

Checks if a player is eligible for a quest.

**Checks:**

- `player.CurrentLegend >= quest.MinimumLegend`
- All `quest.PrerequisiteQuests` in `player.CompletedQuests`

---

### AcceptQuest

```csharp
public bool AcceptQuest(string questId, PlayerCharacter player)

```

Accepts an available quest, changing status to `Active`.

**Behavior:**

1. If quest not already offered, auto-offers it
2. Sets `quest.Status = QuestStatus.Active`
3. Records `quest.AcceptedAt = DateTime.UtcNow`

---

### Objective Tracking

The service tracks objectives through event handlers called by game systems.

### OnEnemyKilled

```csharp
public List<string> OnEnemyKilled(string enemyId, PlayerCharacter player)

```

Updates `KillEnemy` objectives when an enemy is defeated.

**Called by:** `CombatEngine` after enemy death

**Returns:** List of progress messages for UI display

---

### OnItemCollected

```csharp
public List<string> OnItemCollected(string itemId, PlayerCharacter player)

```

Updates `CollectItem` objectives when an item is picked up.

**Called by:** Item pickup systems

---

### OnNPCTalk

```csharp
public List<string> OnNPCTalk(string npcId, PlayerCharacter player)

```

Updates `TalkToNPC` objectives when interacting with NPCs.

**Called by:** `DialogueService` or NPC interaction handlers

---

### OnRoomEntered

```csharp
public List<string> OnRoomEntered(int roomId, PlayerCharacter player)

```

Updates `ExploreRoom` objectives when entering a room.

**Called by:** Navigation/movement systems

---

### UpdateQuestProgress

```csharp
public List<string> UpdateQuestProgress(PlayerCharacter player)

```

Checks all active quests and marks those with all objectives complete.

**Returns:** Messages about newly completed quests

---

### Quest Completion

### TurnInQuest

```csharp
public List<string> TurnInQuest(string questId, PlayerCharacter player)

```

Turns in a completed quest and grants all rewards.

**Reward Processing:**

1. Grant Legend experience
2. Grant currency (if `CurrencyService` available)
3. Grant items
4. Apply faction reputation changes
5. Record territory action (if `TerritoryService` available)

**Quest State:**

- Moves from `ActiveQuests` to `CompletedQuests`
- Sets `Status = QuestStatus.TurnedIn`
- Records `CompletedAt = DateTime.UtcNow`

---

### CompleteQuest

```csharp
public List<string> CompleteQuest(string questId, PlayerCharacter player)

```

Legacy method - auto-completes objectives and calls `TurnInQuest`.

---

### Query Methods

### GetQuest

```csharp
public Quest? GetQuest(string questId)

```

Gets a quest template from the database.

---

### GetPlayerQuest

```csharp
public Quest? GetPlayerQuest(PlayerCharacter player, string questId)

```

Gets a player's quest instance (searches active and completed).

---

### GetQuestsByStatus

```csharp
public List<Quest> GetQuestsByStatus(PlayerCharacter player, QuestStatus status)

```

Gets all player quests with a specific status.

---

### Convenience Methods

```csharp
public List<Quest> GetAvailableQuests(PlayerCharacter player)  // Status = Available
public List<Quest> GetActiveQuests(PlayerCharacter player)     // Status = Active
public List<Quest> GetCompleteQuests(PlayerCharacter player)   // Status = Complete
public List<Quest> GetTurnedInQuests(PlayerCharacter player)   // In CompletedQuests

```

---

## Data Models

### Quest

```csharp
public class Quest
{
    // Identity
    public string Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }

    // Quest Giver
    public string GiverNpcId { get; set; }
    public string GiverNpcName { get; set; }

    // State
    public QuestStatus Status { get; set; }
    public DateTime? AcceptedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    // Requirements (v0.14)
    public int MinimumLegend { get; set; }
    public List<string> PrerequisiteQuests { get; set; }

    // Content
    public List<QuestObjective> Objectives { get; set; }
    public QuestReward? Reward { get; set; }

    // Classification
    public QuestType Type { get; set; }        // Main, Side, Dynamic, Repeatable
    public QuestCategory Category { get; set; } // Combat, Exploration, Retrieval, etc.

    // Generation (v0.14)
    public QuestGenerationRequirements? GenerationReqs { get; set; }
}

```

---

### QuestObjective

Simple objective tracking (v0.8):

```csharp
public class QuestObjective
{
    public string Description { get; set; }
    public ObjectiveType Type { get; set; }  // CollectItem, KillEnemy, TalkToNPC, ExploreRoom
    public string TargetId { get; set; }     // Item/Enemy/NPC/Room ID
    public int Required { get; set; }
    public int Current { get; set; }
    public bool IsComplete => Current >= Required;
}

```

---

### Typed Objectives (v0.14)

For enhanced type safety:

```csharp
// Base class
public abstract class BaseQuestObjective
{
    public string ObjectiveId { get; set; }
    public string Description { get; set; }
    public bool IsOptional { get; set; }

    public abstract int CurrentProgress { get; }
    public abstract int TargetProgress { get; }
    public abstract bool IsComplete { get; }
}

// Implementations
public class KillObjective : BaseQuestObjective { ... }
public class CollectObjective : BaseQuestObjective { ... }
public class ExploreObjective : BaseQuestObjective { ... }
public class InteractObjective : BaseQuestObjective { ... }

```

---

### QuestReward

```csharp
public class QuestReward
{
    public int Experience { get; set; }              // Legend points
    public int Currency { get; set; }                // Dvergr Cogs (v0.9)
    public List<string> ItemIds { get; set; }        // Simple item grants
    public List<ItemReward> DetailedItems { get; set; } // Items with quantity/quality
    public int ReputationChange { get; set; }        // Legacy single faction
    public FactionType? Faction { get; set; }        // Legacy faction type
    public Dictionary<string, int> ReputationGains { get; set; } // Multi-faction (v0.14)
    public List<string> UnlockedAbilities { get; set; }
    public List<string> UnlockedAreas { get; set; }
    public List<string> UnlockedQuests { get; set; }
}

```

---

### QuestGenerationRequirements

For procedurally generated quest dungeons:

```csharp
public class QuestGenerationRequirements
{
    public string BiomeId { get; set; }
    public int MinDepth { get; set; }
    public int MaxDepth { get; set; }
    public int TargetRoomCount { get; set; }
    public List<string> RequiredAnchorIds { get; set; }  // Handcrafted rooms
    public Dictionary<string, int> RequiredEnemies { get; set; }
    public List<string> RequiredLootNodes { get; set; }
}

```

---

## Integration Points

### With CombatEngine

```csharp
// After defeating an enemy
var questMessages = questService.OnEnemyKilled(enemy.Type.ToString(), player);
foreach (var msg in questMessages)
{
    ui.DisplayMessage(msg);
}

```

### With NavigationService

```csharp
// After entering a room
var questMessages = questService.OnRoomEntered(room.Id, player);

```

### With DialogueService

```csharp
// After talking to an NPC
var questMessages = questService.OnNPCTalk(npc.Id, player);

```

### With TerritoryService (v0.35)

When a quest with faction affiliation is completed:

```csharp
_territoryService.RecordPlayerAction(
    player.Id,
    player.CurrentSectorId.Value,
    "Complete_Quest",
    factionName,
    $"Quest: {quest.Title}");

```

---

## Quest Type Classification

| Type | Description |
| --- | --- |
| `Main` | Critical path quests, advance main story |
| `Side` | Optional quests, additional content |
| `Dynamic` | Procedurally generated quests |
| `Repeatable` | Can be completed multiple times |

## Quest Category Classification

| Category | Objective Types |
| --- | --- |
| `Combat` | Kill enemies |
| `Exploration` | Discover locations |
| `Retrieval` | Collect items |
| `Delivery` | Transport items to NPC |
| `Investigation` | Examine objects |
| `Dialogue` | Talk to NPCs |

---

## Usage Examples

### Offering and Accepting a Quest

```csharp
var questService = new QuestService("Data/Quests", currencyService, territoryService);
questService.LoadQuestDatabase();

// NPC offers quest when player talks to them
if (npc.HasQuest && questService.OfferQuest(npc.QuestId, player))
{
    Console.WriteLine($"Quest Available: {questService.GetQuest(npc.QuestId)?.Title}");
}

// Player accepts the quest
if (questService.AcceptQuest("roots_scavenge_01", player))
{
    Console.WriteLine("Quest accepted!");
}

```

### Tracking Progress

```csharp
// In combat resolution
var enemy = combat.DefeatedEnemy;
var messages = questService.OnEnemyKilled(enemy.Type.ToString(), player);
messages.ForEach(m => Console.WriteLine(m));

// Periodically check for completion
var completionMessages = questService.UpdateQuestProgress(player);
completionMessages.ForEach(m => Console.WriteLine(m));

```

### Turning In a Quest

```csharp
// When talking to quest giver NPC
var completeQuests = questService.GetCompleteQuests(player)
    .Where(q => q.GiverNpcId == npc.Id);

foreach (var quest in completeQuests)
{
    var rewards = questService.TurnInQuest(quest.Id, player);
    rewards.ForEach(r => Console.WriteLine(r));
}

```

---

## Related Documentation

- [DialogueService](https://www.notion.so/dialogue-service.md) - NPC dialogue system
- [CurrencyService](https://www.notion.so/currency-service.md) - Currency rewards
- [TerritoryControlService](https://www.notion.so/territory-control-service.md) - Faction influence
- [DungeonGenerator](https://www.notion.so/dungeon-generator.md) - Quest anchor placement