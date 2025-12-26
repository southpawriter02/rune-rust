# Companion System Integration Guide

**Version:** v0.34 Integration
**Date:** 2025-11-16
**Status:** Integration Complete - Ready for Game Loop Implementation

---

## 📋 Overview

This document provides a complete guide for integrating the v0.34 NPC Companion System into the Rune & Rust game loop. All database, service, and engine integrations have been completed. This guide shows how to use the integrated systems in your game code.

---

## ✅ Completed Integrations

### 1. Database Integration (COMPLETE)

**Location:** `RuneAndRust.Persistence/SaveRepository.cs`

The companion database schema is automatically created and seeded when `SaveRepository.InitializeDatabase()` is called.

**Tables Created:**

- `Companions` - 6 base companion definitions
- `Characters_Companions` - Recruitment status and party membership
- `Companion_Progression` - Leveling, legend, equipment, abilities
- `Companion_Quests` - Personal quest unlocking
- `Companion_Abilities` - 18 companion abilities

**Verification:**

```bash
sqlite3 runeandrust.db
SELECT COUNT(*) FROM Companions;  -- Should return 6
SELECT COUNT(*) FROM Companion_Abilities;  -- Should return 18

```

### 2. CombatState Integration (COMPLETE)

**Location:** `RuneAndRust.Core/CombatState.cs`

**Added Properties:**

```csharp
public class CombatState
{
    // v0.34.4: Companion System integration
    public List<Companion> Companions { get; set; } = new();
    public int CharacterId { get; set; } // For loading companion data
    // ... existing properties
}

public class CombatParticipant
{
    public bool IsCompanion { get; set; } // v0.34.4: Companion support
    // Character can now be PlayerCharacter, Companion, or Enemy
}

```

### 3. CombatEngine Integration (COMPLETE)

**Location:** `RuneAndRust.Engine/CombatEngine.cs`

**Constructor Updated:**

```csharp
public CombatEngine(
    DiceService diceService,
    SagaService sagaService,
    LootService lootService,
    EquipmentService equipmentService,
    HazardService hazardService,
    CurrencyService currencyService,
    AdvancedStatusEffectService? statusEffectService = null,
    CounterAttackService? counterAttackService = null,
    string? connectionString = null  // NEW: For companion system
)

```

**New Methods Added:**

- `ProcessCompanionTurn()` - Process companion's combat turn
- `IsCompanionTurn()` - Check if current turn is a companion
- `GetCurrentCompanion()` - Get companion for current turn
- `DamageCompanion()` - Apply damage and check for System Crash
- `RecoverCompanionsAfterCombat()` - 50% HP recovery after victory

**Modified Methods:**

- `InitializeCombat()` - Now accepts `characterId` parameter, loads companions from database
- `RollInitiative()` - Includes companions in initiative order

### 4. CommandParser Integration (COMPLETE)

**Location:** `RuneAndRust.Engine/CommandParser.cs`

**New CommandType Enum Values:**

```csharp
public enum CommandType
{
    // ... existing commands
    Command,  // v0.34.4 - Direct companion ability usage
    Stance    // v0.34.4 - Change companion AI stance
}

```

**New Command Aliases:**

```csharp
{ "command", CommandType.Command },
{ "cmd", CommandType.Command },
{ "order", CommandType.Command },
{ "stance", CommandType.Stance },
{ "mode", CommandType.Stance },

```

---

## 🚀 Game Loop Integration

### Combat Initialization

Update your combat initialization to pass the character ID:

**Before:**

```csharp
var combatEngine = new CombatEngine(
    diceService,
    sagaService,
    lootService,
    equipmentService,
    hazardService,
    currencyService);

var combatState = combatEngine.InitializeCombat(player, enemies, currentRoom);

```

**After:**

```csharp
var combatEngine = new CombatEngine(
    diceService,
    sagaService,
    lootService,
    equipmentService,
    hazardService,
    currencyService,
    statusEffectService,      // Optional
    counterAttackService,      // Optional
    connectionString);         // NEW: Required for companions

var combatState = combatEngine.InitializeCombat(
    player,
    enemies,
    currentRoom,
    canFlee: true,
    characterId: player.CharacterID);  // NEW: Loads companions automatically

```

### Turn Processing

Add companion turn handling to your combat loop:

```csharp
// In your main combat loop
while (combatState.IsActive)
{
    var currentParticipant = combatState.CurrentParticipant;

    if (currentParticipant.IsPlayer)
    {
        // Existing player turn logic
        var command = GetPlayerInput();
        ProcessPlayerCommand(command, combatState, combatEngine);
    }
    else if (currentParticipant.IsCompanion)  // NEW: Companion turn
    {
        var companion = combatEngine.GetCurrentCompanion(combatState);
        if (companion != null)
        {
            combatEngine.ProcessCompanionTurn(combatState, companion);
        }
    }
    else // Enemy turn
    {
        // Existing enemy turn logic
        ProcessEnemyTurn(currentParticipant, combatState);
    }

    combatState.NextTurn();
}

```

### Command Handling

Add companion command handling to your command processor:

```csharp
// Initialize CompanionCommands service (do this once at startup)
var companionService = new CompanionService(connectionString);
var companionCommands = new CompanionCommands(companionService);

// In your command handler
var parsedCommand = commandParser.Parse(input);

switch (parsedCommand.Type)
{
    case CommandType.Command:  // NEW: Direct companion command
        var result = companionCommands.ParseCommandVerb(
            input,
            player.CharacterID,
            player,
            combatState.Enemies.Where(e => e.IsAlive).ToList());

        if (result.Success)
        {
            Console.WriteLine(result.Message);
        }
        else
        {
            Console.WriteLine($"Error: {result.Message}");
        }
        break;

    case CommandType.Stance:  // NEW: Change companion stance
        var stanceResult = companionCommands.ParseStanceVerb(
            input,
            player.CharacterID);

        if (stanceResult.Success)
        {
            Console.WriteLine(stanceResult.Message);
        }
        else
        {
            Console.WriteLine($"Error: {stanceResult.Message}");
        }
        break;

    // ... existing command cases
}

```

### After-Combat Recovery

Add companion recovery after combat victory:

```csharp
// After combat ends successfully
if (!player.IsAlive)
{
    // Player defeated
    HandlePlayerDefeat();
}
else if (combatState.Enemies.All(e => !e.IsAlive))
{
    // Victory!
    Console.WriteLine("Victory!");

    // NEW: Recover incapacitated companions
    combatEngine.RecoverCompanionsAfterCombat(combatState);

    // Existing victory logic (loot, legend, etc.)
    AwardLegendAndLoot(player, combatState);
}

```

### Enemy Damage to Companions

When enemies attack, check if they target companions:

```csharp
// In enemy turn processing
public void ProcessEnemyAttack(Enemy enemy, CombatState combatState)
{
    // Select target (could be player or companion)
    var target = SelectEnemyTarget(enemy, combatState);

    if (target == "player")
    {
        // Existing player damage logic
        DamagePlayer(combatState.Player, damage);
    }
    else if (target.StartsWith("companion_"))
    {
        // NEW: Companion damage
        var companionId = int.Parse(target.Replace("companion_", ""));
        var companion = combatState.Companions.FirstOrDefault(c => c.CompanionID == companionId);

        if (companion != null)
        {
            combatEngine.DamageCompanion(combatState, companion, damage);
        }
    }
}

```

---

## 📖 Usage Examples

### Example 1: Recruit a Companion

```csharp
// Initialize services
var recruitmentService = new RecruitmentService(connectionString);
var reputationService = new ReputationService(connectionString);

// Grant reputation to meet Kara's requirement (Jarnheim Resistance 20)
int jarnheimFactionId = 1; // From Factions table
reputationService.ModifyReputation(characterId: player.CharacterID, factionId: jarnheimFactionId, change: 25);

// Attempt recruitment
if (recruitmentService.CanRecruitCompanion(player.CharacterID, companionId: 1, out string reason))
{
    var success = recruitmentService.RecruitCompanion(player.CharacterID, companionId: 1);
    Console.WriteLine("Kara Ironbreaker has joined your party!");
    Console.WriteLine("Her personal quest 'The Last Protocol' is now available.");
}
else
{
    Console.WriteLine($"Cannot recruit: {reason}");
}

```

### Example 2: Combat with Companions

```csharp
// Combat initialization automatically loads companions
var combatState = combatEngine.InitializeCombat(
    player,
    enemies,
    currentRoom,
    canFlee: true,
    characterId: player.CharacterID);

// Initiative order now includes companions
// Output example:
// === COMBAT BEGINS ===
// Initiative order determined:
//   Kara Ironbreaker (Initiative: 5)
//   Test Player (Initiative: 4)
//   Finnr the Forlorn (Initiative: 3)
//   Corrupted Warden (Initiative: 2)

// Companion turns are processed automatically
// --- Kara Ironbreaker's Turn ---
// Kara Ironbreaker: Targeting wounded enemy (Aggressive stance)
// Kara attacks Corrupted Warden for 12 damage

```

### Example 3: Direct Companion Commands

```csharp
// Player inputs
> command Kara shield_bash warden
Kara Ironbreaker will use shield_bash on Corrupted Warden

// On Kara's next turn, she uses the commanded ability instead of AI selection

> stance Finnr defensive
Finnr the Forlorn is now DEFENSIVE

// Finnr now prioritizes protecting low-HP allies

```

### Example 4: System Crash and Recovery

```csharp
// During combat
// Enemy deals 70 damage to Finnr (HP: 35 → 0)
//   [SYSTEM CRASH] Finnr the Forlorn is incapacitated!
//   You feel the psychic feedback (+10 Psychic Stress)
// Player Psychic Stress: 20 → 30

// After combat victory
// Finnr the Forlorn recovered to 35 HP (50% recovery)

```

---

## 🧪 Testing Integration

### Integration Test Checklist

- [ ]  **Database Creation**: Verify 6 companions and 18 abilities seeded
- [ ]  **Combat Initialization**: Companions load correctly with `characterId` parameter
- [ ]  **Initiative Order**: Companions appear in initiative order with correct rolls
- [ ]  **Companion Turns**: Companions take actions according to stance
- [ ]  **Direct Commands**: `command` verb queues ability correctly
- [ ]  **Stance Changes**: `stance` verb changes AI behavior
- [ ]  **System Crash**: Companion at 0 HP triggers crash, player gains +10 Psychic Stress
- [ ]  **After-Combat Recovery**: Companions recover to 50% HP automatically
- [ ]  **Recruitment**: Faction reputation gates work correctly
- [ ]  **Progression**: Companions gain Legend and level up
- [ ]  **Personal Quests**: Quests unlock on recruitment

### Manual Testing Script

```bash
# 1. Start new game
# 2. Recruit Finnr (no faction requirement)
> recruit finnr

# 3. Enter combat
> explore
[Encounter: 2 Corrupted Servitors]

# 4. Verify initiative order includes Finnr
=== COMBAT BEGINS ===
Initiative order determined:
  Finnr the Forlorn (Initiative: 4)
  Test Player (Initiative: 3)
  Corrupted Servitor (Initiative: 2)

# 5. Test Finnr's AI turn (passive stance)
--- Finnr the Forlorn's Turn ---
Finnr the Forlorn awaits orders (Passive stance)

# 6. Change stance
> stance finnr aggressive

# 7. Next combat round, Finnr should attack
--- Finnr the Forlorn's Turn ---
Finnr the Forlorn: Targeting wounded enemy (Aggressive stance)
Finnr attacks Corrupted Servitor for 8 damage

# 8. Test direct command
> command finnr aetheric_bolt servitor
Finnr the Forlorn will use aetheric_bolt on Corrupted Servitor

# 9. Test System Crash
[Enemy deals 70+ damage to Finnr]
  [SYSTEM CRASH] Finnr the Forlorn is incapacitated!
  You feel the psychic feedback (+10 Psychic Stress)

# 10. Victory and recovery
Victory!
Finnr the Forlorn recovered to 35 HP (50% recovery)

```

---

## 🔧 Configuration

### Connection String

Ensure the connection string is passed to all services:

```csharp
// Typically read from configuration
string connectionString = $"Data Source={dbPath}";

// Pass to CombatEngine
var combatEngine = new CombatEngine(
    // ... other services
    connectionString: connectionString);

// Pass to Companion services
var companionService = new CompanionService(connectionString);
var recruitmentService = new RecruitmentService(connectionString);
var progressionService = new CompanionProgressionService(connectionString);

```

### Optional: Pre-load Companions

For performance, you can pre-load companions before combat:

```csharp
// Pre-load companions for UI display
var companionService = new CompanionService(connectionString);
var partyCompanions = companionService.GetPartyCompanions(player.CharacterID);

// Display in UI
foreach (var companion in partyCompanions)
{
    Console.WriteLine($"{companion.DisplayName} - HP: {companion.CurrentHitPoints}/{companion.MaxHitPoints}");
}

```

---

## 📊 Expected Behavior

### Companion AI by Stance

**Aggressive:**

- Targets wounded enemies first
- Uses high-damage abilities
- Ignores self-preservation

**Defensive:**

- Protects player and low-HP allies
- Targets threats to player
- Prefers defensive abilities

**Passive:**

- Takes no autonomous action
- Waits for direct commands
- Useful for precise control

### System Crash Mechanics

**When Companion Reaches 0 HP:**

1. Companion marked as `IsIncapacitated = true`
2. Player receives **+10 Psychic Stress** (Trauma Economy)
3. Companion skips all remaining turns
4. No permadeath - recoverable

**Recovery Timeline:**

- **After-Combat**: 50% HP, full resources (automatic)
- **Field Revival**: Variable HP via Bone-Setter abilities (optional)
- **Sanctuary**: 100% HP, full resources (rest action)

---

## 🐛 Troubleshooting

### "CompanionService not initialized"

**Cause:** CombatEngine created without `connectionString` parameter.

**Fix:**

```csharp
var combatEngine = new CombatEngine(
    diceService,
    sagaService,
    lootService,
    equipmentService,
    hazardService,
    currencyService,
    statusEffectService: null,
    counterAttackService: null,
    connectionString: dbConnectionString  // Add this!
);

```

### Companions Not Loading

**Cause:** `characterId` not passed to `InitializeCombat()`.

**Fix:**

```csharp
var combatState = combatEngine.InitializeCombat(
    player,
    enemies,
    currentRoom,
    canFlee: true,
    characterId: player.CharacterID  // Add this!
);

```

### "Companion not found in party"

**Cause:** Companion not recruited or not in active party.

**Fix:**

```csharp
// Check if recruited
var recruitmentService = new RecruitmentService(connectionString);
var recruitable = recruitmentService.GetRecruitableCompanions(characterId);

if (recruitable.Any(c => c.CompanionName == "kara"))
{
    // Not yet recruited - need to recruit first
    recruitmentService.RecruitCompanion(characterId, 1);
}
else
{
    // Already recruited, check if in party
    var partyCompanions = companionService.GetPartyCompanions(characterId);
    if (!partyCompanions.Any(c => c.CompanionName == "kara"))
    {
        // In roster but not active party - add to party
        recruitmentService.AddToParty(characterId, 1);
    }
}

```

---

## 📚 Reference

### Key Files Modified

- `RuneAndRust.Core/CombatState.cs` - Added Companions list and CharacterId
- `RuneAndRust.Engine/CombatEngine.cs` - Added companion turn processing and recovery
- `RuneAndRust.Engine/CommandParser.cs` - Added Command and Stance command types
- `RuneAndRust.Persistence/SaveRepository.cs` - Companion schema creation (already done in v0.34.1)

### New Files Created

- `RuneAndRust.Engine/CompanionService.cs` (v0.34.4)
- `RuneAndRust.Engine/CompanionCommands.cs` (v0.34.4)
- `RuneAndRust.Engine/CompanionAIService.cs` (v0.34.2)
- `RuneAndRust.Engine/RecruitmentService.cs` (v0.34.3)
- `RuneAndRust.Engine/CompanionProgressionService.cs` (v0.34.3)

### Database Tables

- `Companions` - Base companion definitions
- `Characters_Companions` - Recruitment and party status
- `Companion_Progression` - Leveling and equipment
- `Companion_Quests` - Personal quest tracking
- `Companion_Abilities` - Ability definitions

---

**Document Version:** 1.0
**Last Updated:** 2025-11-16
**Integration Status:** ✅ COMPLETE