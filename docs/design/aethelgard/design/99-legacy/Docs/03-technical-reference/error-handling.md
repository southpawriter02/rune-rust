# Error Handling Patterns

> **Version:** v0.44.7+
> **Last Updated:** November 2024
> **Location:** `RuneAndRust.Engine/`, `RuneAndRust.Core/`, `RuneAndRust.DesktopUI/`

## Overview

Rune & Rust implements a multi-layered error handling strategy that combines:

1. **Defensive validation** at system boundaries
2. **Structured exception handling** with detailed logging
3. **Result types** for expected failure scenarios
4. **Null-safe patterns** for optional data

---

## 1. Constructor Validation (Guard Clauses)

### Purpose

Fail fast at construction time rather than during operation. Used extensively in Controllers and ViewModels.

### Pattern: ArgumentNullException Guards

**File:** `RuneAndRust.DesktopUI/Controllers/CombatController.cs`

```csharp
public CombatController(
    ILogger logger,
    GameStateController gameStateController,
    INavigationService navigationService,
    CombatEngine combatEngine,
    EnemyAI enemyAI,
    SagaService sagaService,
    LootService lootService)
{
    _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    _gameStateController = gameStateController ?? throw new ArgumentNullException(nameof(gameStateController));
    _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
    _combatEngine = combatEngine ?? throw new ArgumentNullException(nameof(combatEngine));
    _enemyAI = enemyAI ?? throw new ArgumentNullException(nameof(enemyAI));
    _sagaService = sagaService ?? throw new ArgumentNullException(nameof(sagaService));
    _lootService = lootService ?? throw new ArgumentNullException(nameof(lootService));
}
```

**File:** `RuneAndRust.DesktopUI/ViewModels/MenuViewModel.cs`

```csharp
public MenuViewModel(
    MenuController controller,
    INavigationService navigationService,
    SpriteService spriteService,
    MetaProgressionService metaProgressionService,
    EndgameService endgameService,
    ConfigurationService configurationService,
    AudioService audioService,
    SaveGameService saveGameService,
    DialogService dialogService,
    ILogger logger)
{
    _controller = controller ?? throw new ArgumentNullException(nameof(controller));
    _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
    _spriteService = spriteService ?? throw new ArgumentNullException(nameof(spriteService));
    _metaProgressionService = metaProgressionService ?? throw new ArgumentNullException(nameof(metaProgressionService));
    _endgameService = endgameService ?? throw new ArgumentNullException(nameof(endgameService));
    _configurationService = configurationService ?? throw new ArgumentNullException(nameof(configurationService));
    _audioService = audioService ?? throw new ArgumentNullException(nameof(audioService));
    _saveGameService = saveGameService ?? throw new ArgumentNullException(nameof(saveGameService));
    _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
    _logger = logger ?? throw new ArgumentNullException(nameof(logger));
}
```

### Benefits
- Clear contract enforcement
- Immediate failure with meaningful exception
- `nameof()` provides compile-time parameter name safety

---

## 2. Service-Level Exception Handling

### Pattern: Try-Catch with Structured Logging

Services wrap operations in try-catch blocks, log structured errors, and re-throw to preserve the call stack.

**File:** `RuneAndRust.Engine/WorldEventService.cs`

```csharp
public void ProcessDailyEventCheck(int sectorId)
{
    _log.Debug("Processing daily event check for sector {SectorId}", sectorId);

    try
    {
        var controlState = _territoryService.CalculateSectorControlState(sectorId);

        // ... event processing logic ...

        _log.Debug("Daily event check complete for sector {SectorId}: {ActiveCount} active events",
            sectorId, activeEvents.Count);
    }
    catch (Exception ex)
    {
        _log.Error(ex, "Failed daily event check for sector {SectorId}", sectorId);
        throw;  // Re-throw to preserve stack trace
    }
}
```

**File:** `RuneAndRust.Engine/FactionWarService.cs`

```csharp
public void CheckWarTrigger(int sectorId)
{
    try
    {
        // ... war trigger logic ...
    }
    catch (Exception ex)
    {
        _log.Error(ex, "Failed to check war trigger for sector {SectorId}", sectorId);
        throw;
    }
}

public bool AdvanceWar(int warId)
{
    try
    {
        var war = GetActiveWar(warId);
        if (war == null || !war.IsActive)
        {
            _log.Warning("Cannot advance inactive or non-existent war: {WarId}", warId);
            return false;  // Graceful failure for expected condition
        }
        // ... advancement logic ...
    }
    catch (Exception ex)
    {
        _log.Error(ex, "Failed to advance war {WarId}", warId);
        throw;
    }
}
```

**File:** `RuneAndRust.Engine/SpiritBargainService.cs`

```csharp
public SpiritBargainResult AttemptSpiritBargain(int characterId, string abilityName)
{
    try
    {
        // ... bargain logic ...
    }
    catch (Exception ex)
    {
        _log.Error(ex, "Failed to attempt Spirit Bargain: CharacterId={CharacterId}, Ability={Ability}",
            characterId, abilityName);
        throw;
    }
}

public bool ActivateMomentOfClarity(int characterId)
{
    try
    {
        // ... clarity activation logic ...
    }
    catch (Exception ex)
    {
        _log.Error(ex, "Failed to activate Moment of Clarity: CharacterId={CharacterId}", characterId);
        throw;
    }
}
```

### Structured Logging Fields

The codebase uses Serilog's structured logging with consistent field naming:

| Field | Usage |
|-------|-------|
| `{SectorId}` | Territory/sector operations |
| `{CharacterId}` | Character-related operations |
| `{WarId}` | Faction war operations |
| `{EventId}` | Event processing |
| `{AbilityName}` | Ability/spell operations |
| `{TemplateName}` | Template loading operations |
| `{FileName}` | File I/O operations |

---

## 3. Validation Exceptions

### InvalidOperationException

Used when an operation is attempted in an invalid state.

**File:** `RuneAndRust.Engine/SpecializationFactory.cs`

```csharp
public Specialization CreateSpecialization(Character character, string specializationName)
{
    if (character.Specialization != null)
    {
        throw new InvalidOperationException("Character already has a specialization!");
    }
    // ...
}
```

**File:** `RuneAndRust.Engine/EndgameContentOrchestrator.cs`

```csharp
public bool AdvanceToNextTier(ICharacterState characterState)
{
    int currentTier = characterState.NewGamePlusTier;
    if (currentTier >= MAX_NG_PLUS_TIER)
    {
        throw new InvalidOperationException($"Cannot advance from NG+{currentTier}");
    }

    // ...

    if (!_ngPlusService.InitializeNewGamePlus(targetTier))
    {
        throw new InvalidOperationException($"Failed to initialize NG+{targetTier}");
    }
}
```

**File:** `RuneAndRust.Engine/ObjectInteractionService.cs`

```csharp
public InteractiveObject GenerateObject(string baseTemplateName, ...)
{
    var baseTemplate = _repository.GetBaseTemplateByName(baseTemplateName);
    if (baseTemplate == null)
    {
        throw new ArgumentException($"Base template not found: {baseTemplateName}");
    }

    var mechanics = ObjectMechanics.FromJson(baseTemplate.BaseMechanics);
    if (mechanics == null)
    {
        throw new InvalidOperationException($"Invalid base mechanics for template: {baseTemplateName}");
    }
    // ...
}
```

### ArgumentException

Used for invalid input parameters.

**File:** `RuneAndRust.Engine/EnemyFactory.cs`

```csharp
public Enemy CreateEnemy(string type)
{
    return type switch
    {
        "Draugr" => CreateDraugr(),
        "TrollKin" => CreateTrollKin(),
        "Einherjar" => CreateEinherjar(),
        _ => throw new ArgumentException($"Unknown enemy type: {type}")
    };
}
```

**File:** `RuneAndRust.Engine/FormationService.cs`

```csharp
public int GetActorId(object actor)
{
    return actor switch
    {
        PlayerCharacter pc => pc.Id,
        Enemy e => e.CombatId,
        Companion c => c.Id,
        _ => throw new ArgumentException($"Invalid actor type: {actor.GetType().Name}")
    };
}
```

**File:** `RuneAndRust.Engine/EndgameContentOrchestrator.cs`

```csharp
public bool AdvanceToNextTier(ICharacterState characterState)
{
    if (characterState is not PlayerCharacter character)
    {
        throw new ArgumentException("characterState must be a PlayerCharacter", nameof(characterState));
    }
    // ...
}
```

---

## 4. Null-Safe Patterns

### Pattern: Early Return on Null

Services return early when encountering null values for optional data.

**File:** `RuneAndRust.Engine/MilestoneService.cs`

```csharp
public MilestoneTier? GetNextTier(int accountId)
{
    var account = GetAccount(accountId);
    if (account == null) return null;  // Early return for missing account

    var currentTier = GetCurrentTier(accountId);
    if (currentTier == null) return GetTier(1);  // Default to first tier

    var nextTier = GetTier(currentTier.TierNumber + 1);
    if (nextTier == null) return null;  // No next tier available

    return nextTier;
}
```

**File:** `RuneAndRust.Engine/CoverService.cs`

```csharp
public CoverQuality GetCoverBetweenPositions(CombatGrid? grid, GridPosition? target, GridPosition? attacker)
{
    if (targetPosition == null || attackerPosition == null)
    {
        return CoverQuality.None;  // No cover calculation possible
    }

    var tile = grid?.GetTile(target);
    if (tile == null || tile.Cover == CoverType.None)
    {
        return CoverQuality.None;
    }
    // ...
}
```

**File:** `RuneAndRust.Engine/EnvironmentalObjectService.cs`

```csharp
public DestructionResult? AttemptDestruction(int objectId, int damage)
{
    var obj = GetObject(objectId);
    if (obj == null || !obj.IsDestructible || obj.State == EnvironmentalObjectState.Destroyed)
    {
        return null;  // Cannot destroy non-existent or already destroyed objects
    }
    // ...
}
```

### Pattern: Null-Coalescing Default

**File:** `RuneAndRust.Engine/ObjectInteractionService.cs`

```csharp
public ObjectInteractionService(
    DescriptorRepository repository,
    ILogger logger,
    Random? random = null,
    SkillUsageFlavorTextService? skillFlavorService = null)
{
    _repository = repository;
    _logger = logger;
    _random = random ?? new Random();           // Default if null
    _skillFlavorService = skillFlavorService;   // Nullable optional service
}
```

---

## 5. Result Types

### Purpose

Return structured result objects for operations that can fail gracefully, avoiding exceptions for expected failure scenarios.

### DestructionResult

**File:** `RuneAndRust.Core/EnvironmentalEvent.cs`

```csharp
public class DestructionResult
{
    public bool Success { get; set; } = true;
    public bool ObjectDestroyed { get; set; }
    public int ObjectId { get; set; }
    public string ObjectName { get; set; } = string.Empty;
    public string? DestructionMethod { get; set; }
    public int DamageDealt { get; set; }
    public int RemainingDurability { get; set; }
    public List<int> SecondaryTargets { get; set; } = new();
    public string? TerrainCreated { get; set; }
    public List<DestructionResult> ChainReactions { get; set; } = new();
    public string Message { get; set; } = string.Empty;
}
```

### HazardResult

```csharp
public class HazardResult
{
    public bool WasTriggered { get; set; }
    public int TotalDamage { get; set; }
    public List<int> AffectedCharacters { get; set; } = new();
    public string? StatusEffectApplied { get; set; }
    public string Description { get; set; } = string.Empty;
    public string HazardName { get; set; } = string.Empty;
}
```

### InteractionResult

```csharp
public class InteractionResult
{
    public bool Success { get; set; }
    public int StaminaCost { get; set; }
    public string? EffectDescription { get; set; }
    public int DamageDealt { get; set; } = 0;
    public List<int> AffectedCharacters { get; set; } = new();
    public string LogMessage { get; set; } = string.Empty;
}
```

### Usage Pattern

```csharp
public InteractionResult AttemptInteraction(InteractiveObject obj, int characterId = 0)
{
    try
    {
        // Validate preconditions
        if (!obj.IsInteractable)
        {
            return new InteractionResult
            {
                Success = false,
                LogMessage = $"{obj.Name} cannot be interacted with."
            };
        }

        // Perform interaction
        var result = ProcessInteraction(obj, characterId);

        return new InteractionResult
        {
            Success = true,
            StaminaCost = result.Cost,
            EffectDescription = result.Description,
            LogMessage = $"You interact with {obj.Name}. {result.Description}"
        };
    }
    catch (Exception ex)
    {
        _logger.Error(ex, "Error during interaction with {ObjectName}", obj.Name);
        return new InteractionResult
        {
            Success = false,
            LogMessage = "Something went wrong during the interaction."
        };
    }
}
```

---

## 6. File I/O Error Handling

### Pattern: Per-File Exception Handling

File operations catch exceptions per-file to allow partial success.

**File:** `RuneAndRust.Engine/HandcraftedRoomLibrary.cs`

```csharp
public void LoadAllHandcraftedRooms(string directory)
{
    foreach (var file in Directory.GetFiles(directory, "*.json"))
    {
        try
        {
            var room = LoadRoom(file);
            _rooms.Add(room);
            _log.Debug("Loaded handcrafted room: {RoomName}", room.Name);
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Error loading handcrafted room from file: {FileName}",
                Path.GetFileName(file));
            // Continue loading other files
        }
    }
}
```

**File:** `RuneAndRust.Engine/TemplateLibrary.cs`

```csharp
public void LoadTemplates(string directory)
{
    foreach (var file in Directory.GetFiles(directory, "*.json"))
    {
        try
        {
            var template = JsonSerializer.Deserialize<EnemyTemplate>(File.ReadAllText(file));
            _templates.Add(template.Id, template);
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Error loading template from file: {FileName}",
                Path.GetFileName(file));
            // Continue loading other templates
        }
    }

    // Validate required templates exist
    foreach (var archetype in RequiredArchetypes)
    {
        if (!_templates.ContainsKey(archetype))
        {
            _log.Error("Missing required template archetype: {Archetype}", archetype);
        }
    }
}
```

### JSON Parsing Errors

**File:** `RuneAndRust.Engine/CombatFlavorTextService.cs`

```csharp
public Dictionary<string, FlavorText> LoadFlavorTexts(string json)
{
    try
    {
        return JsonSerializer.Deserialize<Dictionary<string, FlavorText>>(json)
            ?? new Dictionary<string, FlavorText>();
    }
    catch (JsonException ex)
    {
        _log.Error(ex, "Failed to parse flavor text JSON");
        return new Dictionary<string, FlavorText>();  // Return empty on failure
    }
}
```

---

## 7. Collection Safety

### Empty Collection Checks

**File:** `RuneAndRust.Engine/EchoCallerService.cs`

```csharp
public List<Enemy> GetValidTargets(List<Enemy> enemies)
{
    var adjacentEnemies = enemies.Where(e => e.IsAdjacent).ToList();

    if (!adjacentEnemies.Any())
    {
        _log.Debug("No adjacent enemies for Echo Call effect");
        return new List<Enemy>();  // Return empty list, not null
    }

    return adjacentEnemies;
}
```

**File:** `RuneAndRust.Engine/BossLootService.cs`

```csharp
public Equipment? SelectArtifact(List<ArtifactData> artifacts)
{
    if (!availableArtifacts.Any())
    {
        _log.Warning("No available artifacts for boss loot");
        return null;
    }
    // ...
}
```

---

## 8. Logging Levels

### Error Severity Guidelines

| Level | Usage | Example |
|-------|-------|---------|
| `Trace` | Fine-grained debugging | Per-metric recordings |
| `Debug` | Development diagnostics | Operation start/complete |
| `Information` | Significant operations | Pipeline completion |
| `Warning` | Recoverable issues | Missing optional data |
| `Error` | Operation failures | Exception with context |
| `Fatal` | Unrecoverable state | Terminal corruption |

### Fatal Error Example

**File:** `RuneAndRust.Engine/TraumaEconomyService.cs`

```csharp
public void ProcessTerminalCorruption(Character character)
{
    _log.Error("TERMINAL CORRUPTION: Character={CharacterName}", character.Name);
    // Character is in unrecoverable state - game over scenario
}
```

---

## 9. Exception Handling Summary

| Pattern | When to Use | Example |
|---------|-------------|---------|
| **ArgumentNullException** | Required constructor params | Controllers, ViewModels |
| **ArgumentException** | Invalid method params | Factory switch default |
| **InvalidOperationException** | Invalid state transitions | Already specialized character |
| **Try-Catch-Log-Throw** | Service operations | Database, file I/O |
| **Try-Catch-Return-Default** | Non-critical operations | JSON parsing |
| **Result Types** | Expected failure scenarios | Combat interactions |
| **Early Return on Null** | Optional data handling | Milestone lookups |

---

## 10. Best Practices

### Do
- Use `nameof()` for parameter names in exceptions
- Log structured data with Serilog placeholders (`{FieldName}`)
- Re-throw exceptions to preserve stack traces (`throw;` not `throw ex;`)
- Return result types for expected failure cases
- Check for null/empty collections before operations
- Provide default values for optional parameters

### Don't
- Catch exceptions without logging
- Swallow exceptions silently
- Use exceptions for control flow
- Return null for collection types (use empty collection)
- Throw generic `Exception` type

---

## Related Documentation

- [Service Architecture](service-architecture.md) - Service design patterns
- [Data Flow](data-flow.md) - Pipeline error handling
- [Event System](event-system.md) - Event result types
- [Performance Benchmarks](performance-benchmarks.md) - Error timing metrics
