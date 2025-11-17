# Territory Control System Integration Guide
## v0.35 - Complete Integration Checklist

**Document ID:** RR-INTEGRATION-v0.35-GUIDE
**Status:** Implementation Guide
**Date:** 2025-11-17

---

## Overview

This guide provides step-by-step instructions for integrating the Territory Control system (v0.35) with all existing game systems. The Territory Control infrastructure is **100% complete and tested** - this guide focuses on **wiring it into the game**.

---

## Integration Checklist

### ✅ Completed (Infrastructure)
- [x] Database schema (v0.35.1)
- [x] Territory control services (v0.35.2)
- [x] World events system (v0.35.3)
- [x] Orchestration layer (v0.35.4)
- [x] Integration helpers
- [x] Unit tests (170+ tests, 100% coverage)

### ❌ Remaining (Wiring)
- [ ] Service initialization in Program.cs
- [ ] Quest completion hook
- [ ] Enemy kill hook
- [ ] Companion territory reactions
- [ ] NPC faction modifiers
- [ ] Merchant faction inventory
- [ ] Daily update loop
- [ ] Procedural generation integration

---

## Integration Point 1: Service Initialization (Program.cs)

### Location
`RuneAndRust.ConsoleApp/Program.cs`

### Current State
Services exist but are not initialized in Program.cs

### Required Changes

```csharp
// Add after existing service declarations (around line 40)

// v0.35: Territory Control & Dynamic World System
private static TerritoryControlService _territoryControlService;
private static FactionWarService _factionWarService;
private static WorldEventService _eventService;
private static TerritoryService _territoryService;
private static NPCFactionReactions _npcFactionReactions;
private static MerchantFactionInventory _merchantFactionInventory;
private static HazardDensityModifier _hazardDensityModifier;
private static FactionTerritoryIntegration _factionTerritoryIntegration;
private static CompanionTerritoryReactions _companionTerritoryReactions;

// In Main() after database initialization:

// Initialize Territory Control services
_territoryControlService = new TerritoryControlService(_connectionString);
_factionWarService = new FactionWarService(_connectionString, _territoryControlService);
_eventService = new WorldEventService(_connectionString, _territoryControlService);

// Initialize ReputationService if not already present
var _reputationService = new ReputationService(_connectionString);

_territoryService = new TerritoryService(
    _connectionString,
    _territoryControlService,
    _factionWarService,
    _eventService,
    _reputationService);

// Initialize integration helpers
_npcFactionReactions = new NPCFactionReactions();
_merchantFactionInventory = new MerchantFactionInventory();
_hazardDensityModifier = new HazardDensityModifier();
_factionTerritoryIntegration = new FactionTerritoryIntegration(_territoryService, _reputationService);
_companionTerritoryReactions = new CompanionTerritoryReactions(_territoryService);
```

**Files to Modify:**
- `RuneAndRust.ConsoleApp/Program.cs`

**Dependencies:**
- `using RuneAndRust.Engine.Integration;`

---

## Integration Point 2: Quest Completion Hook

### Location
`RuneAndRust.Engine/QuestService.cs` - `TurnInQuest()` method

### Current State
Quest completion grants rewards but doesn't affect territory

### Required Changes

```csharp
// In QuestService class, add TerritoryService dependency
private readonly TerritoryService? _territoryService;

// Update constructor
public QuestService(
    string dataPath = "Data/Quests",
    CurrencyService? currencyService = null,
    TerritoryService? territoryService = null)
{
    _questDataPath = dataPath;
    _currencyService = currencyService;
    _territoryService = territoryService; // NEW
}

// In TurnInQuest() method, after granting rewards:
public List<string> TurnInQuest(string questId, PlayerCharacter player)
{
    var messages = new List<string>();
    var quest = player.ActiveQuests.FirstOrDefault(q => q.Id == questId);

    if (quest == null || quest.Status != QuestStatus.Complete)
        return messages;

    // ... existing reward logic ...

    // NEW: Record territorial action if quest has faction affiliation
    if (_territoryService != null &&
        quest.Reward?.Faction != null &&
        player.CurrentSectorId.HasValue)
    {
        try
        {
            string factionName = ConvertFactionTypeToName(quest.Reward.Faction.Value);

            _territoryService.RecordPlayerAction(
                player.Id,
                player.CurrentSectorId.Value,
                "Complete_Quest",
                factionName,
                $"Quest: {quest.Title}");

            messages.Add($"[Territory] {factionName} influence increased in this sector!");
        }
        catch (Exception ex)
        {
            _log.Warning(ex, "Failed to record territorial action for quest {QuestId}", questId);
        }
    }

    // ... rest of method ...
}

// Helper method to convert FactionType enum to faction name string
private string ConvertFactionTypeToName(FactionType faction)
{
    return faction switch
    {
        FactionType.IronBanes => "IronBanes",
        FactionType.JotunReaders => "JotunReaders",
        FactionType.GodSleeperCultists => "GodSleeperCultists",
        FactionType.RustClans => "RustClans",
        FactionType.Independents => "Independents",
        _ => "Independents"
    };
}
```

**Files to Modify:**
- `RuneAndRust.Engine/QuestService.cs`
- `RuneAndRust.Core/PlayerCharacter.cs` (add `CurrentSectorId` property if not present)

**New Properties Needed:**
```csharp
// In PlayerCharacter.cs
public int? CurrentSectorId { get; set; } = 1; // Default to Midgard (sector 1)
```

---

## Integration Point 3: Enemy Kill Hook

### Location
`RuneAndRust.Engine/CombatEngine.cs` - Enemy death handling

### Current State
Enemy kills grant XP/loot but don't affect territory

### Required Changes

```csharp
// In CombatEngine class, add TerritoryService dependency
private readonly TerritoryService? _territoryService;

// Update constructor to accept TerritoryService
public CombatEngine(
    DiceService diceService,
    SagaService sagaService,
    LootService lootService,
    EquipmentService equipmentService,
    HazardService hazardService,
    CurrencyService currencyService,
    AdvancedStatusEffectService statusEffectService,
    CompanionAIService? companionAI,
    string connectionString,
    TerritoryService? territoryService = null) // NEW
{
    // ... existing initialization ...
    _territoryService = territoryService; // NEW
}

// In enemy death handling (wherever that occurs):
private void ProcessEnemyDeath(Enemy enemy, PlayerCharacter player)
{
    // ... existing death logic (XP, loot, etc.) ...

    // NEW: Record territorial action for enemy kill
    if (_territoryService != null &&
        player.CurrentSectorId.HasValue &&
        !string.IsNullOrEmpty(enemy.FactionAffiliation))
    {
        try
        {
            // Determine which faction benefits (player's highest reputation faction)
            string benefittingFaction = GetPlayerFaction(player);

            if (!string.IsNullOrEmpty(benefittingFaction))
            {
                _territoryService.RecordPlayerAction(
                    player.Id,
                    player.CurrentSectorId.Value,
                    "Kill_Enemy",
                    benefittingFaction,
                    $"Killed: {enemy.Name}");
            }
        }
        catch (Exception ex)
        {
            _log.Warning(ex, "Failed to record territorial action for enemy kill");
        }
    }
}

// Helper to get player's primary faction (highest reputation)
private string GetPlayerFaction(PlayerCharacter player)
{
    // Get faction with highest reputation
    var topFaction = player.FactionReputations.Reputations
        .OrderByDescending(r => r.ReputationValue)
        .FirstOrDefault();

    return topFaction?.Faction.ToString() ?? "Independents";
}
```

**Files to Modify:**
- `RuneAndRust.Engine/CombatEngine.cs`
- `RuneAndRust.Core/Enemy.cs` (ensure `FactionAffiliation` property exists)

---

## Integration Point 4: Companion Territory Reactions

### Location
`RuneAndRust.Engine/CompanionService.cs` - Sector entry handling

### Current State
Companions don't react to territory control

### Required Changes

```csharp
// In CompanionService class, add CompanionTerritoryReactions dependency
private readonly CompanionTerritoryReactions? _territoryReactions;

// Update constructor
public CompanionService(
    string connectionString,
    CompanionTerritoryReactions? territoryReactions = null)
{
    _connectionString = connectionString;
    _territoryReactions = territoryReactions;
}

// NEW method: Process companion reaction when entering sector
public (string dialogue, string? buffName, int duration, int value) OnCompanionEnterSector(
    Companion companion,
    int sectorId)
{
    if (_territoryReactions == null)
        return ("", null, 0, 0);

    try
    {
        var reaction = _territoryReactions.GetCompanionReaction(companion, sectorId);

        _log.Information(
            "Companion {Name} entered sector {SectorId}: {Dialogue}",
            companion.CompanionName, sectorId, reaction.dialogue);

        return reaction;
    }
    catch (Exception ex)
    {
        _log.Error(ex, "Failed to get companion territory reaction");
        return ("", null, 0, 0);
    }
}

// NEW method: Get companion comment on territory
public string GetCompanionTerritoryComment(Companion companion, int sectorId)
{
    if (_territoryReactions == null)
        return "";

    return _territoryReactions.GetCompanionComment(companion, sectorId);
}
```

**Files to Modify:**
- `RuneAndRust.Engine/CompanionService.cs`

**Usage in Game Loop:**
```csharp
// When player moves to new sector with companions:
foreach (var companion in activeCompanions)
{
    var (dialogue, buffName, duration, value) = _companionService.OnCompanionEnterSector(
        companion,
        newSectorId);

    if (!string.IsNullOrEmpty(dialogue))
    {
        Console.WriteLine($"[{companion.DisplayName}] {dialogue}");
    }

    if (!string.IsNullOrEmpty(buffName))
    {
        // Apply buff/debuff to companion
        ApplyCompanionBuff(companion, buffName, duration, value);
    }
}
```

---

## Integration Point 5: NPC Faction Modifiers

### Location
`RuneAndRust.Engine/NPCService.cs` - NPC interaction handling

### Current State
NPCs don't adjust behavior based on territory control

### Required Changes

```csharp
// In NPCService class, add dependencies
private readonly NPCFactionReactions? _factionReactions;
private readonly TerritoryService? _territoryService;

// Update constructor
public NPCService(
    NPCFactionReactions? factionReactions = null,
    TerritoryService? territoryService = null)
{
    _factionReactions = factionReactions;
    _territoryService = territoryService;
}

// NEW method: Get modified NPC behavior based on territory
public (NPC npc, NPCBehaviorModifier modifier) GetTerritoryModifiedNPC(
    NPC npc,
    int sectorId)
{
    if (_factionReactions == null || _territoryService == null)
        return (npc, new NPCBehaviorModifier());

    try
    {
        var status = _territoryService.GetSectorTerritoryStatus(sectorId);
        var modifier = _factionReactions.GetFactionModifiedBehavior(npc, status.DominantFaction);

        // Apply disposition modifier
        _factionReactions.ApplyBehaviorModifier(npc, modifier);

        _log.Debug(
            "NPC {NpcId} modified for territory: Hostility={Hostility}, PriceMod={PriceMod}",
            npc.Id, modifier.HostilityLevel, modifier.PriceModifier);

        return (npc, modifier);
    }
    catch (Exception ex)
    {
        _log.Error(ex, "Failed to apply territory modifiers to NPC {NpcId}", npc.Id);
        return (npc, new NPCBehaviorModifier());
    }
}

// NEW method: Get modified prices for trading
public int GetModifiedPrice(int basePrice, NPC npc, int sectorId)
{
    if (_factionReactions == null)
        return basePrice;

    var (_, modifier) = GetTerritoryModifiedNPC(npc, sectorId);
    return _factionReactions.GetModifiedPrice(basePrice, modifier);
}

// NEW method: Check if dialogue option is available
public bool IsDialogueAvailable(string dialogueType, NPC npc, int sectorId)
{
    if (_factionReactions == null)
        return true; // Default to available

    var (_, modifier) = GetTerritoryModifiedNPC(npc, sectorId);
    return _factionReactions.IsDialogueAvailable(modifier, dialogueType);
}
```

**Files to Modify:**
- `RuneAndRust.Engine/NPCService.cs`

---

## Integration Point 6: Merchant Faction Inventory

### Location
`RuneAndRust.Engine/MerchantService.cs` - Merchant stock generation

### Current State
Merchants have static inventory

### Required Changes

```csharp
// In MerchantService class, add dependencies
private readonly MerchantFactionInventory? _factionInventory;
private readonly TerritoryService? _territoryService;

// Update constructor
public MerchantService(
    MerchantFactionInventory? factionInventory = null,
    TerritoryService? territoryService = null)
{
    _factionInventory = factionInventory;
    _territoryService = territoryService;
}

// NEW method: Initialize merchant with faction-specific inventory
public void ApplyFactionInventory(Merchant merchant, int sectorId)
{
    if (_factionInventory == null || _territoryService == null)
        return;

    try
    {
        var status = _territoryService.GetSectorTerritoryStatus(sectorId);

        // Add faction-specific items
        _factionInventory.ApplyFactionInventory(merchant, status.DominantFaction);

        _log.Information(
            "Applied {Faction} inventory to merchant {MerchantId} in sector {SectorId}",
            status.DominantFaction, merchant.Id, sectorId);
    }
    catch (Exception ex)
    {
        _log.Error(ex, "Failed to apply faction inventory to merchant");
    }
}

// NEW method: Get faction-modified price
public int GetFactionModifiedPrice(int basePrice, int sectorId)
{
    if (_factionInventory == null || _territoryService == null)
        return basePrice;

    try
    {
        var status = _territoryService.GetSectorTerritoryStatus(sectorId);
        double modifier = _factionInventory.GetFactionPriceModifier(status.DominantFaction);

        return (int)(basePrice * modifier);
    }
    catch (Exception ex)
    {
        _log.Error(ex, "Failed to calculate faction price modifier");
        return basePrice;
    }
}
```

**Files to Modify:**
- `RuneAndRust.Engine/MerchantService.cs`

**Usage:**
```csharp
// When initializing merchant for player interaction:
var merchant = LoadMerchant(merchantId);
_merchantService.ApplyFactionInventory(merchant, currentSectorId);

// When calculating prices:
int modifiedPrice = _merchantService.GetFactionModifiedPrice(basePrice, currentSectorId);
```

---

## Integration Point 7: Daily Update Loop

### Location
Game loop or turn/day advancement system

### Current State
No daily territory updates

### Required Changes

```csharp
// In game loop, daily/turn advancement system:
public void ProcessDailyUpdate()
{
    try
    {
        _log.Information("Processing daily world update...");

        // Existing daily updates (merchant restock, etc.)
        // ...

        // NEW: Territory Control daily update
        if (_territoryService != null)
        {
            _territoryService.ProcessDailyTerritoryUpdate();
            _log.Information("Territory update complete");
        }

        // Display any newly resolved wars or events
        DisplayTerritoryUpdates();
    }
    catch (Exception ex)
    {
        _log.Error(ex, "Failed to process daily update");
    }
}

private void DisplayTerritoryUpdates()
{
    // Get recently resolved wars
    // Get newly spawned events
    // Display notifications to player
}
```

**Files to Modify:**
- Game loop file (need to identify where daily advancement occurs)

---

## Integration Point 8: Procedural Generation

### Location
Dungeon/encounter generation systems

### Current State
Generation doesn't account for territory control

### Required Changes

```csharp
// In procedural generation code:
public void GenerateEncounter(int sectorId, DungeonRoom room)
{
    // NEW: Get territory-influenced parameters
    SectorGenerationParams territoryParams = null;
    if (_territoryService != null)
    {
        territoryParams = _territoryService.GetSectorGenerationParams(sectorId);
    }

    // Apply faction modifiers to generation
    if (territoryParams != null)
    {
        // Modify enemy spawns
        if (!string.IsNullOrEmpty(territoryParams.EnemyFactionFilter))
        {
            room.EnemyFactionFilter = territoryParams.EnemyFactionFilter;
        }

        room.EnemyDensity *= territoryParams.EnemyDensityMultiplier;
        room.HazardDensity *= territoryParams.HazardDensityMultiplier;
        room.ArtifactSpawnRate *= territoryParams.ArtifactSpawnRate;
        room.SalvageRate *= territoryParams.SalvageMaterialRate;

        // Apply ambient description
        if (!string.IsNullOrEmpty(territoryParams.AmbientDescription))
        {
            room.AmbientDescription = territoryParams.AmbientDescription;
        }
    }

    // Continue with normal generation using modified parameters
    // ...
}
```

**Files to Modify:**
- Procedural generation files (need to identify)

---

## Testing Checklist

After implementing all integration points, test:

- [ ] **Service Initialization**: All services start without errors
- [ ] **Quest Completion**: Completing quest increases faction influence
- [ ] **Enemy Kills**: Killing enemies records territorial actions
- [ ] **Companion Reactions**: Companions comment/react to territories
- [ ] **NPC Modifiers**: NPCs have different prices/dialogue based on faction
- [ ] **Merchant Inventory**: Merchants have faction-specific items
- [ ] **Daily Updates**: Wars resolve, events spawn/process
- [ ] **Generation**: Encounters reflect controlling faction
- [ ] **Full Integration**: Complete quest → See influence change → Trigger war → See companion react

---

## Verification Commands

```csharp
// Test territory status
var status = _territoryService.GetSectorTerritoryStatus(sectorId);
Console.WriteLine($"Sector: {status.SectorName}");
Console.WriteLine($"Control: {status.ControlState}");
Console.WriteLine($"Faction: {status.DominantFaction}");

// Test player influence
var influence = _territoryService.GetPlayerTotalInfluence(characterId);
foreach (var (faction, total) in influence)
{
    Console.WriteLine($"{faction}: {total:F1} total influence");
}

// Test generation params
var params = _territoryService.GetSectorGenerationParams(sectorId);
Console.WriteLine($"Hazard Density: {params.HazardDensityMultiplier}x");
Console.WriteLine($"Enemy Filter: {params.EnemyFactionFilter}");
```

---

## Summary

**Total Integration Points:** 8
**Estimated Time:** 3-4 hours
**Files to Modify:** 6-8 files
**New Properties Needed:** 1 (PlayerCharacter.CurrentSectorId)

All infrastructure is ready - this guide provides the exact integration points to connect the Territory Control system to the existing game.
