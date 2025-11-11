# v0.8 NPC & Dialogue System - Integration Guide

## Overview
This guide explains how to integrate the v0.8 NPC & Dialogue System into the existing Rune & Rust game engine.

## Architecture

### Core Components

#### Data Models (RuneAndRust.Core)
- `NPC` - Represents non-player characters
- `FactionType` - Enum for the 3 factions (MidgardCombine, RustClans, Independents)
- `FactionReputationSystem` - Manages faction reputation tracking
- `DialogueNode` - Represents dialogue tree nodes
- `DialogueOption` - Player response options
- `SkillCheckRequirement` - Attribute/skill requirements for dialogue options
- `DialogueOutcome` - Results of dialogue choices
- `Quest` - Quest definitions
- `QuestObjective` - Individual quest objectives (collect, kill, talk, explore)
- `QuestReward` - Quest completion rewards

#### Services (RuneAndRust.Engine)
- `NPCService` - NPC lifecycle and management
- `DialogueService` - Conversation flow and skill checks
- `QuestService` - Quest tracking and completion

#### Data Files (Data/)
- `Data/NPCs/*.json` - 8 NPC definitions
- `Data/Dialogues/*.json` - Dialogue trees for each NPC
- `Data/Quests/*.json` - 5 quest definitions

## Integration Steps

### 1. Initialize Services (Already done in GameState)

```csharp
public class GameState
{
    public NPCService NPCService { get; set; }
    public DialogueService DialogueService { get; set; }
    public QuestService QuestService { get; set; }

    public GameState()
    {
        // ... existing initialization ...

        NPCService = new NPCService();
        DialogueService = new DialogueService();
        QuestService = new QuestService();

        // Load databases (do this after initialization)
        NPCService.LoadNPCDatabase();
        DialogueService.LoadDialogueDatabase();
        QuestService.LoadQuestDatabase();
    }
}
```

### 2. Place NPCs in Rooms

In `GameWorld` or room initialization code:

```csharp
public void InitializeNPCs(GameState state)
{
    // Create NPC instances and place them in rooms
    var sigrun = state.NPCService.CreateNPCInstance("sigrun_scavenger");
    if (sigrun != null)
    {
        var room2 = GetRoom(2);
        room2.NPCs.Add(sigrun);
    }

    var kjartan = state.NPCService.CreateNPCInstance("kjartan_smith");
    if (kjartan != null)
    {
        var room5 = GetRoom(5);
        room5.NPCs.Add(kjartan);
    }

    // ... repeat for all 8 NPCs ...
}
```

### 3. Update Room Entry Logic

When player enters a room, update NPC dispositions and show NPCs:

```csharp
public void OnRoomEnter(GameState state)
{
    // Update NPC dispositions based on faction reputation
    state.NPCService.UpdateRoomNPCDispositions(state.CurrentRoom, state.Player);

    // Check for hostile NPCs
    foreach (var npc in state.CurrentRoom.NPCs)
    {
        if (state.NPCService.IsHostile(npc) && npc.IsAlive)
        {
            // Trigger combat with hostile NPC
            // Convert NPC to enemy or flag room for combat
        }
    }

    // Update quest objectives for room exploration
    var questMessages = state.QuestService.OnRoomEntered(state.CurrentRoom.Id, state.Player);
    foreach (var msg in questMessages)
    {
        Console.WriteLine(msg);
    }
}
```

### 4. Implement Command Handlers

#### Talk Command

```csharp
case CommandType.Talk:
    if (string.IsNullOrEmpty(command.Target))
    {
        Console.WriteLine("Talk to whom? Use: talk [npc name]");
        break;
    }

    var npc = state.NPCService.FindNPCByName(state.CurrentRoom, command.Target);
    if (npc == null)
    {
        Console.WriteLine($"No one named '{command.Target}' is here.");
        break;
    }

    if (!npc.IsAlive)
    {
        Console.WriteLine($"{npc.Name} is dead.");
        break;
    }

    // Mark NPC as met
    if (!npc.HasBeenMet)
    {
        state.NPCService.MarkAsMet(npc);
        Console.WriteLine(npc.InitialGreeting);
    }

    // Start conversation
    var dialogueNode = state.DialogueService.StartConversation(npc, state.Player);
    if (dialogueNode == null)
    {
        Console.WriteLine($"{npc.Name} has nothing to say.");
        break;
    }

    // Update quest objectives
    var talkMessages = state.QuestService.OnNPCTalk(npc.Id, state.Player);
    foreach (var msg in talkMessages)
    {
        Console.WriteLine(msg);
    }

    // Enter dialogue loop
    while (dialogueNode != null && !dialogueNode.EndsConversation)
    {
        Console.WriteLine($"\n{npc.Name}: {dialogueNode.Text}\n");

        var options = state.DialogueService.GetAvailableOptions(dialogueNode, state.Player);
        if (options.Count == 0)
        {
            Console.WriteLine("(End of conversation)");
            break;
        }

        for (int i = 0; i < options.Count; i++)
        {
            var opt = options[i];
            var skillTag = opt.SkillCheck != null ?
                state.DialogueService.FormatSkillCheckTag(opt.SkillCheck) + " " : "";
            Console.WriteLine($"{i + 1}. {skillTag}{opt.Text}");
        }

        Console.Write("\nChoose an option (1-" + options.Count + "): ");
        var choice = Console.ReadLine();
        if (!int.TryParse(choice, out int optionIndex) ||
            optionIndex < 1 || optionIndex > options.Count)
        {
            Console.WriteLine("Invalid choice.");
            continue;
        }

        var selectedOption = options[optionIndex - 1];
        var (nextNode, outcome) = state.DialogueService.SelectOption(selectedOption, state.Player);

        // Process outcome
        if (outcome != null)
        {
            var outcomeMessages = state.DialogueService.ProcessOutcome(outcome, state.Player, npc);
            foreach (var msg in outcomeMessages)
            {
                Console.WriteLine(msg);
            }

            // Handle quest-related outcomes
            if (outcome.Type == OutcomeType.QuestGiven && !string.IsNullOrEmpty(outcome.Data))
            {
                state.QuestService.AcceptQuest(outcome.Data, state.Player);
            }
            else if (outcome.Type == OutcomeType.QuestComplete && !string.IsNullOrEmpty(outcome.Data))
            {
                var completionMessages = state.QuestService.CompleteQuest(outcome.Data, state.Player);
                foreach (var msg in completionMessages)
                {
                    Console.WriteLine(msg);
                }
            }
        }

        dialogueNode = nextNode;
    }

    state.DialogueService.EndConversation();
    break;
```

#### Quests Command

```csharp
case CommandType.Quests:
    if (state.Player.ActiveQuests.Count == 0 && state.Player.CompletedQuests.Count == 0)
    {
        Console.WriteLine("You have no quests.");
        break;
    }

    if (state.Player.ActiveQuests.Count > 0)
    {
        Console.WriteLine("=== ACTIVE QUESTS ===\n");
        for (int i = 0; i < state.Player.ActiveQuests.Count; i++)
        {
            var quest = state.Player.ActiveQuests[i];
            Console.WriteLine($"[{i + 1}] {quest.Title}");
            foreach (var obj in quest.Objectives)
            {
                var check = obj.IsComplete ? "[✓]" : "[ ]";
                Console.WriteLine($"    {check} {obj.Description}: {obj.GetProgress()}");
            }
            if (quest.Reward != null)
            {
                Console.Write($"    Reward: {quest.Reward.Experience} Legend");
                if (quest.Reward.ItemIds.Count > 0)
                {
                    Console.Write($", {quest.Reward.ItemIds.Count} items");
                }
                if (quest.Reward.ReputationChange > 0 && quest.Reward.Faction.HasValue)
                {
                    Console.Write($", +{quest.Reward.ReputationChange} {quest.Reward.Faction}");
                }
                Console.WriteLine();
            }
            Console.WriteLine();
        }
    }

    if (state.Player.CompletedQuests.Count > 0)
    {
        Console.WriteLine("\n=== COMPLETED QUESTS ===\n");
        foreach (var quest in state.Player.CompletedQuests)
        {
            Console.WriteLine($"[✓] {quest.Title}");
        }
    }
    break;
```

#### Reputation Command

```csharp
case CommandType.Reputation:
    Console.WriteLine("=== FACTION REPUTATION ===\n");

    foreach (FactionType faction in Enum.GetValues(typeof(FactionType)))
    {
        int rep = state.Player.FactionReputations.GetReputation(faction);
        var tier = state.Player.FactionReputations.GetReputationTier(faction);

        string repBar = GenerateReputationBar(rep);
        Console.WriteLine($"{faction}: {tier} ({rep})");
        Console.WriteLine($"  {repBar}");
    }
    break;

// Helper method
private string GenerateReputationBar(int reputation)
{
    // -100 to +100, map to 21 characters
    int pos = (reputation + 100) / 10; // 0-20
    var bar = new char[21];
    for (int i = 0; i < 21; i++)
    {
        bar[i] = i == pos ? '█' : (i == 10 ? '|' : '▁');
    }
    return new string(bar);
}
```

### 5. Update Combat Integration

When enemy is killed, update quest objectives:

```csharp
public void OnEnemyKilled(Enemy enemy, GameState state)
{
    // ... existing combat logic ...

    // v0.8: Update quest objectives
    var questMessages = state.QuestService.OnEnemyKilled(enemy.Id, state.Player);
    foreach (var msg in questMessages)
    {
        Console.WriteLine(msg);
    }
}
```

### 6. Update Loot Integration

When item is picked up, update quest objectives:

```csharp
case CommandType.Pickup:
    // ... existing pickup logic ...

    // v0.8: Update quest objectives
    var lootMessages = state.QuestService.OnItemCollected(itemId, state.Player);
    foreach (var msg in lootMessages)
    {
        Console.WriteLine(msg);
    }
    break;
```

## Special NPC Behaviors

### Bjorn the Exile (Hostile Negotiation)
- Set `IsHostile = true` in NPC definition
- WILL 4 check in dialogue allows player to stand down
- If successful, mark NPC as non-hostile and grant reputation

### Rolf the Hermit (Shortcut Reveal)
- WITS 5 check reveals hidden passage
- Alternative: 50 gold bribe (not implemented in v0.8, placeholder for v0.9)
- Unlock shortcut: Add exit to Room 18 pointing to Room 24

### Eydis the Survivor (Trauma Victim)
- BaseDisposition = -50 (very negative due to Corruption)
- 60% chance hostile on encounter (implement in room entry logic)
- Bone-Setter specialization check in dialogue
- If calmed, grant rare consumable ("Aetheric Stabilizer")

### Gunnar the Raider (Conflicting Quest)
- Accepting "Sabotage" quest: -20 Midgard Combine reputation immediately
- Completing quest: +30 Rust-Clans, -30 Midgard Combine
- If Midgard Combine reputation drops below -25, Thorvald becomes hostile

## Save/Load Integration

Add to `SaveData` class:

```csharp
// v0.8: NPC & Quest System
public string FactionReputationsJson { get; set; } = "{}";
public string ActiveQuestsJson { get; set; } = "[]";
public string CompletedQuestsJson { get; set; } = "[]";
public string NPCStatesJson { get; set; } = "[]"; // Serialized NPC states
```

Serialize/deserialize these in `SaveRepository`:

```csharp
// Save
data.FactionReputationsJson = JsonSerializer.Serialize(player.FactionReputations.Reputations);
data.ActiveQuestsJson = JsonSerializer.Serialize(player.ActiveQuests);
data.CompletedQuestsJson = JsonSerializer.Serialize(player.CompletedQuests);

// Collect all NPCs from all rooms
var allNPCs = world.GetAllRooms().SelectMany(r => r.NPCs).ToList();
data.NPCStatesJson = JsonSerializer.Serialize(allNPCs);

// Load
player.FactionReputations.Reputations = JsonSerializer.Deserialize<Dictionary<FactionType, int>>(data.FactionReputationsJson);
player.ActiveQuests = JsonSerializer.Deserialize<List<Quest>>(data.ActiveQuestsJson);
player.CompletedQuests = JsonSerializer.Deserialize<List<Quest>>(data.CompletedQuestsJson);

var npcStates = JsonSerializer.Deserialize<List<NPC>>(data.NPCStatesJson);
// Restore NPC states to rooms...
```

## Testing Checklist

- [ ] NPCs appear in designated rooms
- [ ] Faction reputation starts at 0 for all factions
- [ ] Talk command initiates dialogue
- [ ] Skill checks hide options when requirements not met
- [ ] Dialogue outcomes trigger correctly (reputation, quests, items)
- [ ] Quest acceptance adds to active quests
- [ ] Quest objectives track progress (collect, kill, talk, explore)
- [ ] Quest completion grants rewards and removes from active
- [ ] Reputation command shows all faction standings
- [ ] Rival faction penalties apply (Combine vs Rust-Clans)
- [ ] NPC disposition updates based on reputation
- [ ] Save/load preserves NPC states, quests, and reputation
- [ ] Bjorn can be talked down with WILL 4
- [ ] Rolf reveals shortcut with WITS 5
- [ ] Eydis can be calmed with Bone-Setter specialization
- [ ] Gunnar's sabotage quest damages Combine reputation

## Known Limitations (v0.8)

1. **No merchant system** - Kjartan hints at future shop (v0.9)
2. **No currency** - Rolf's bribe option not functional (v0.9)
3. **No quest chains** - Each quest is standalone (v1.0)
4. **No quest failure** - All quests stay active until completed (v1.0)
5. **Simple skill checks** - Just pass/fail, no degrees of success (future)
6. **No companion NPCs** - NPCs don't join party (v2.0+)

## Next Steps (v0.9)

1. Add merchant system (convert Kjartan to shop)
2. Implement currency (Dvergr Cogs)
3. Add buy/sell commands
4. Make Rolf's bribe functional
5. Add reputation-based pricing
6. Create shop inventories

## File Locations

```
RuneAndRust.Core/
  ├── NPC.cs
  ├── FactionType.cs
  ├── FactionReputationSystem.cs
  ├── Dialogue/
  │   ├── DialogueNode.cs
  │   ├── DialogueOption.cs
  │   ├── SkillCheckRequirement.cs
  │   └── DialogueOutcome.cs
  └── Quests/
      ├── Quest.cs
      ├── QuestObjective.cs
      └── QuestReward.cs

RuneAndRust.Engine/
  ├── NPCService.cs
  ├── DialogueService.cs
  ├── QuestService.cs
  ├── GameState.cs (updated)
  └── CommandParser.cs (updated)

Data/
  ├── NPCs/
  │   ├── sigrun_scavenger.json
  │   ├── kjartan_smith.json
  │   ├── bjorn_exile.json
  │   ├── astrid_reader.json
  │   ├── thorvald_guard.json
  │   ├── gunnar_raider.json
  │   ├── rolf_hermit.json
  │   └── eydis_survivor.json
  ├── Dialogues/
  │   ├── sigrun_dialogues.json
  │   ├── kjartan_dialogues.json
  │   ├── bjorn_dialogues.json
  │   ├── astrid_dialogues.json
  │   ├── thorvald_dialogues.json
  │   ├── gunnar_dialogues.json
  │   ├── rolf_dialogues.json
  │   └── eydis_dialogues.json
  └── Quests/
      ├── quest_scrap_collection.json
      ├── quest_scrap_collection_upgraded.json
      ├── quest_clear_nest.json
      ├── quest_diplomatic_errand.json
      └── quest_sabotage.json
```

## Support

For questions or issues with v0.8 integration, refer to:
- NPC System design doc (main README)
- Dialogue tree examples in Data/Dialogues/
- Quest definition examples in Data/Quests/
