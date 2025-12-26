# Class Hierarchies & Design Patterns

Parent item: Technical Reference (Technical%20Reference%202ba55eb312da8079a291e020980301c1.md)

> Version: 0.41+
Last Updated: November 2024
Location: RuneAndRust.Core/, RuneAndRust.Engine/
> 

## Overview

Rune & Rust uses a combination of inheritance hierarchies for polymorphic behavior and composition patterns for flexible system design. This document covers both the class hierarchies and the design patterns employed throughout the codebase.

## Class Hierarchies

### Character System

The character system uses **composition over inheritance** for player/enemy/companion entities, unified through the `Combatant` adapter class.

```
┌─────────────────────────────────────────────────────────────┐
│                        Combatant                             │
│  (Adapter - unifies different character types for combat)   │
└─────────────────────────────────────────────────────────────┘
                              │
          ┌───────────────────┼───────────────────┐
          │                   │                   │
          ▼                   ▼                   ▼
┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐
│ PlayerCharacter │  │     Enemy       │  │    Companion    │
│                 │  │                 │  │                 │
│ - CharacterID   │  │ - EnemyType     │  │ - CompanionType │
│ - Specialization│  │ - AIArchetype   │  │ - LoyaltyLevel  │
│ - Archetype     │  │ - Phase (boss)  │  │ - Position      │
│ - Traumas       │  │ - Position      │  └─────────────────┘
│ - Equipment     │  │ - StatusEffects │
│ - Abilities     │  └─────────────────┘
└─────────────────┘

```

**Files:**

- `RuneAndRust.Core/PlayerCharacter.cs`
- `RuneAndRust.Core/Enemy.cs`
- `RuneAndRust.Core/Companion.cs`
- `RuneAndRust.Core/Combatant.cs`

### Archetype Hierarchy

Character archetypes use the **Template Method Pattern** with an abstract base class.

```
                    ┌─────────────────────┐
                    │  Archetype (abstract)│
                    │                     │
                    │ + ArchetypeType     │
                    │ + PrimaryResource   │
                    │ + GetBaseAttributes()│
                    │ + GetStartingAbilities()│
                    │ + GetWeaponProficiencies()│
                    │ + GetArmorProficiencies()│
                    └──────────┬──────────┘
                               │
       ┌───────────────────────┼───────────────────────┐
       │                       │                       │
       ▼                       ▼                       ▼
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│ WarriorArchetype│    │ MysticArchetype │    │SkirmisherArchetype│
│                 │    │                 │    │                 │
│ Might/Sturdiness│    │ Will/Wits       │    │ Finesse/Wits    │
│ Resource:Stamina│    │ Resource: AP    │    │ Resource:Stamina│
│ All weapons     │    │ Simple weapons  │    │ Spears/Daggers  │
│ All armor       │    │ Light armor     │    │ Light/Medium    │
└─────────────────┘    └─────────────────┘    └─────────────────┘

```

**Files:**

- `RuneAndRust.Core/Archetype.cs` (abstract base)
- `RuneAndRust.Core/Archetypes/WarriorArchetype.cs`
- `RuneAndRust.Core/Archetypes/MysticArchetype.cs`
- `RuneAndRust.Core/Archetypes/SkirmisherArchetype.cs`

### Quest Objective Hierarchy

Quest objectives use inheritance for different objective types.

```
               ┌─────────────────────────┐
               │ BaseQuestObjective      │
               │ (abstract)              │
               │                         │
               │ + CurrentProgress       │
               │ + TargetProgress        │
               │ + ProgressText          │
               │ + IsComplete            │
               │ + CheckCompletion()     │
               └───────────┬─────────────┘
                           │
       ┌───────────────────┼───────────────────┐
       │                   │                   │
       ▼                   ▼                   ▼
┌─────────────────┐ ┌─────────────────┐ ┌─────────────────┐
│ KillObjective   │ │ CollectObjective│ │ ExploreObjective│
│                 │ │                 │ │                 │
│ - EnemyType     │ │ - ItemType      │ │ - LocationId    │
│ - KillCount     │ │ - RequiredCount │ │ - Discovered    │
└─────────────────┘ └─────────────────┘ └─────────────────┘
                           │
                           ▼
                   ┌─────────────────┐
                   │InteractObjective│
                   │                 │
                   │ - TargetNpcId   │
                   │ - TargetObjectId│
                   └─────────────────┘

```

**Files:**

- `RuneAndRust.Core/Quests/Quest.cs`
- `RuneAndRust.Core/Quests/ObjectiveTypes.cs`

### Trauma Effect Hierarchy

Trauma effects use the **Strategy Pattern** with an abstract base.

```
                    ┌─────────────────────┐
                    │ TraumaEffect        │
                    │ (abstract)          │
                    │                     │
                    │ + Apply(player)     │
                    │ + Remove(player)    │
                    │ + GetDescription()  │
                    └──────────┬──────────┘
                               │
    ┌──────────────────────────┼──────────────────────────┐
    │              │           │           │              │
    ▼              ▼           ▼           ▼              ▼
┌────────────┐┌────────────┐┌────────────┐┌────────────┐┌────────────┐
│Attribute   ││Stress      ││Passive     ││Rest        ││Behavior    │
│Penalty     ││Multiplier  ││Stress      ││Restriction ││Restriction │
│Effect      ││Effect      ││Effect      ││Effect      ││Effect      │
└────────────┘└────────────┘└────────────┘└────────────┘└────────────┘
                                                              │
                                                              ▼
                                                    ┌────────────────┐
                                                    │Immediate       │
                                                    │Corruption      │
                                                    │Effect          │
                                                    └────────────────┘

```

**Files:**

- `RuneAndRust.Core/TraumaEffect.cs`

### Environmental Hazard Hierarchy

Dynamic hazards for procedural generation.

```
                    ┌─────────────────────┐
                    │ DynamicHazard       │
                    │ (abstract)          │
                    │                     │
                    │ + HazardName        │
                    │ + DamageDice        │
                    │ + StressPerTurn     │
                    │ + Position          │
                    │ + Range             │
                    └──────────┬──────────┘
                               │
    ┌──────────┬───────────────┼───────────────┬──────────┐
    │          │               │               │          │
    ▼          ▼               ▼               ▼          ▼
┌────────┐┌────────┐    ┌────────────┐    ┌────────┐┌────────┐
│Steam   ││Live    │    │Unstable    │    │Toxic   ││Chasm   │
│Vent    ││Power   │    │Ceiling     │    │Spore   ││Hazard  │
│(2d6)   ││Conduit │    │(4d6)       │    │Cloud   ││(6d6)   │
│        ││(3d6)   │    │            │    │(1d6+1S)││        │
└────────┘└────────┘    └────────────┘    └────────┘└────────┘
                               │
              ┌────────────────┼────────────────┐
              ▼                                 ▼
       ┌────────────┐                    ┌────────────┐
       │Corroded    │                    │Leaking     │
       │Grating     │                    │Coolant     │
       │(2d6)       │                    │(1d6)       │
       └────────────┘                    └────────────┘

```

**Files:**

- `RuneAndRust.Core/Population/DynamicHazard.cs`

### Ambient Condition Hierarchy

Room-wide environmental conditions.

```
                    ┌─────────────────────┐
                    │ AmbientCondition    │
                    │ (abstract)          │
                    │                     │
                    │ + ConditionName     │
                    │ + AccuracyModifier  │
                    │ + DefenseModifier   │
                    │ + MovementModifier  │
                    │ + StressPerTurn     │
                    └──────────┬──────────┘
                               │
    ┌──────────┬───────────────┼───────────────┬──────────┐
    │          │               │               │          │
    ▼          ▼               ▼               ▼          ▼
┌────────┐┌────────┐    ┌────────────┐    ┌────────┐┌────────┐
│Flooded ││Darkness│    │Psychic     │    │Extreme ││Runic   │
│(-20%   ││(-2 acc │    │Resonance   │    │Heat    ││Instab- │
│ move)  ││+1S/turn│    │(+2S/turn)  │    │(+1S)   ││ility   │
└────────┘└────────┘    └────────────┘    └────────┘└────────┘
                                                        │
                                                        ▼
                                              ┌────────────────┐
                                              │Corroded        │
                                              │Atmosphere      │
                                              │(equipment deg.)│
                                              └────────────────┘

```

**Files:**

- `RuneAndRust.Core/Population/AmbientCondition.cs`

### Coherent Glitch Rule Hierarchy

Rules for procedural sector generation coherence.

```
                    ┌─────────────────────┐
                    │ CoherentGlitchRule  │
                    │ (abstract)          │
                    │                     │
                    │ + RuleName          │
                    │ + Priority          │
                    │ + CanApply(sector)  │
                    │ + Apply(sector)     │
                    └──────────┬──────────┘
                               │
    ┌──────────────────────────┼──────────────────────────┐
    │                          │                          │
    ▼                          ▼                          ▼
┌──────────────┐      ┌──────────────┐      ┌──────────────┐
│BossArena     │      │Flooded       │      │Chasm         │
│AmplifierRule │      │Electrical    │      │Infrastructure│
│              │      │DangerRule    │      │Rule          │
└──────────────┘      └──────────────┘      └──────────────┘
        │                     │                     │
        └─────────────────────┴─────────────────────┘
                              │
         ┌────────────────────┼────────────────────┐
         │                    │                    │
         ▼                    ▼                    ▼
┌──────────────┐      ┌──────────────┐      ┌──────────────┐
│ResourceVein  │      │TacticalCover │      │...13+ more   │
│ClusterRule   │      │PlacementRule │      │rules         │
└──────────────┘      └──────────────┘      └──────────────┘

```

**Files:**

- `RuneAndRust.Engine/CoherentGlitch/CoherentGlitchRuleEngine.cs`
- `RuneAndRust.Engine/CoherentGlitch/Rules/` (rule implementations)

---

## Design Patterns

### Factory Pattern

**Purpose:** Create complex objects without exposing instantiation logic.

### CharacterFactory

Creates fully-configured `PlayerCharacter` instances.

```csharp
// RuneAndRust.Engine/CharacterFactory.cs

public class CharacterFactory
{
    private readonly AccountProgressionService? _accountProgression;

    public PlayerCharacter CreateCharacter(string name, CharacterClass characterClass)
    {
        var player = new PlayerCharacter { Name = name, Class = characterClass };

        switch (characterClass)
        {
            case CharacterClass.Warrior:
                InitializeWarrior(player);
                break;
            case CharacterClass.Mystic:
                InitializeMystic(player);
                break;
            case CharacterClass.Skirmisher:
                InitializeSkirmisher(player);
                break;
            case CharacterClass.Adept:
                InitializeAdept(player);
                break;
        }

        ApplyAccountUnlocks(player);
        return player;
    }

    private void InitializeWarrior(PlayerCharacter player)
    {
        var archetype = new WarriorArchetype();
        player.Attributes = archetype.GetBaseAttributes();
        player.Abilities = archetype.GetStartingAbilities();
        // ... additional setup
    }
}

```

**Files:**

- `RuneAndRust.Engine/CharacterFactory.cs`
- `RuneAndRust.Engine/EnemyFactory.cs`
- `RuneAndRust.Engine/SpecializationFactory.cs`

### EnemyFactory

Creates `Enemy` instances from `EnemyType` enum.

```csharp
// RuneAndRust.Engine/EnemyFactory.cs

public class EnemyFactory
{
    public Enemy CreateEnemy(EnemyType type)
    {
        return type switch
        {
            EnemyType.Draugr => CreateDraugr(),
            EnemyType.Forlorn => CreateForlorn(),
            EnemyType.RevenantKnight => CreateRevenantKnight(),
            EnemyType.RustHulk => CreateRustHulk(),
            // ... 20+ enemy types
            _ => throw new ArgumentException($"Unknown enemy type: {type}")
        };
    }

    private Enemy CreateDraugr()
    {
        return new Enemy
        {
            Type = EnemyType.Draugr,
            Name = "Draugr",
            MaxHP = 15,
            HP = 15,
            Attributes = new Attributes { Might = 3, Finesse = 2 },
            BaseDamageDice = 2,
            Soak = 1,
            AIArchetype = AIArchetype.Aggressive
        };
    }
}

```

---

### Command Pattern

**Purpose:** Encapsulate player actions as objects for uniform processing.

### Interface

```csharp
// RuneAndRust.Engine/Commands/ICommand.cs

public interface ICommand
{
    CommandResult Execute(GameState state, string[] args);
}

public class CommandResult
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public bool EndsTurn { get; set; }
}

```

### Implementations

```csharp
// RuneAndRust.Engine/Commands/AttackCommand.cs

public class AttackCommand : ICommand
{
    private readonly CombatEngine _combatEngine;

    public CommandResult Execute(GameState state, string[] args)
    {
        if (!state.InCombat)
            return new CommandResult { Success = false, Message = "Not in combat." };

        var target = ParseTarget(args, state);
        var result = _combatEngine.ExecutePlayerAttack(state.Player, target);

        return new CommandResult
        {
            Success = true,
            Message = result.Description,
            EndsTurn = true
        };
    }
}

```

### Dispatcher

```csharp
// RuneAndRust.Engine/Commands/CommandDispatcher.cs

public class CommandDispatcher
{
    private readonly Dictionary<string, ICommand> _commands;

    public CommandDispatcher(/* dependencies */)
    {
        _commands = new Dictionary<string, ICommand>
        {
            ["attack"] = new AttackCommand(combatEngine),
            ["look"] = new LookCommand(roomService),
            ["go"] = new GoCommand(navigationService),
            ["inventory"] = new InventoryCommand(),
            ["equip"] = new EquipmentCommand(equipmentService),
            ["ability"] = new AbilityCommand(abilityService),
            ["rest"] = new RestCommand(restService),
            ["stance"] = new StanceCommand(stanceService),
            // ... 20+ commands
        };
    }

    public CommandResult Dispatch(string input, GameState state)
    {
        var parts = input.Split(' ');
        var commandName = parts[0].ToLower();
        var args = parts.Skip(1).ToArray();

        if (_commands.TryGetValue(commandName, out var command))
            return command.Execute(state, args);

        return new CommandResult { Success = false, Message = "Unknown command." };
    }
}

```

**Files:**

- `RuneAndRust.Engine/Commands/ICommand.cs`
- `RuneAndRust.Engine/Commands/CommandDispatcher.cs`
- `RuneAndRust.Engine/Commands/*.cs` (20+ command implementations)

---

### Strategy Pattern

**Purpose:** Define interchangeable algorithms for AI behavior.

### AI Service Interfaces

```csharp
// RuneAndRust.Engine/AI/IThreatAssessmentService.cs

public interface IThreatAssessmentService
{
    ThreatLevel AssessThreat(Enemy enemy, PlayerCharacter player, BattlefieldGrid grid);
    List<Combatant> PrioritizeThreats(Enemy enemy, List<Combatant> targets);
}

// RuneAndRust.Engine/AI/ITargetSelectionService.cs

public interface ITargetSelectionService
{
    Combatant SelectTarget(Enemy enemy, List<Combatant> validTargets, CombatContext context);
}

// RuneAndRust.Engine/AI/IBehaviorPatternService.cs

public interface IBehaviorPatternService
{
    AIAction DetermineAction(Enemy enemy, CombatContext context);
}

```

### Implementation

```csharp
// RuneAndRust.Engine/AI/ThreatAssessmentService.cs

public class ThreatAssessmentService : IThreatAssessmentService
{
    public ThreatLevel AssessThreat(Enemy enemy, PlayerCharacter player, BattlefieldGrid grid)
    {
        var threatScore = 0;

        // Factor: Player HP percentage
        var hpPercent = (float)player.HP / player.MaxHP;
        if (hpPercent < 0.25f) threatScore -= 20;  // Low threat if almost dead
        else if (hpPercent > 0.75f) threatScore += 20;

        // Factor: Player damage potential
        threatScore += player.EquippedWeapon?.DamageDice ?? 0 * 5;

        // Factor: Distance
        var distance = grid.GetDistance(enemy.Position, player.Position);
        if (distance <= 1) threatScore += 10;  // Close = more threatening

        return ClassifyThreatLevel(threatScore);
    }
}

```

**Files:**

- `RuneAndRust.Engine/AI/IThreatAssessmentService.cs`
- `RuneAndRust.Engine/AI/ITargetSelectionService.cs`
- `RuneAndRust.Engine/AI/IBehaviorPatternService.cs`
- `RuneAndRust.Engine/AI/IAbilityRotationService.cs`
- `RuneAndRust.Engine/AI/IDifficultyScalingService.cs`

---

### Adapter Pattern

**Purpose:** Unify different character types under a common interface.

```csharp
// RuneAndRust.Core/Combatant.cs

public class Combatant
{
    public object Character { get; }  // PlayerCharacter, Enemy, or Companion

    public Combatant(object character)
    {
        Character = character;
    }

    public int CurrentHP => Character switch
    {
        PlayerCharacter pc => pc.HP,
        Enemy e => e.HP,
        Companion c => c.HP,
        _ => 0
    };

    public int MaxHP => Character switch
    {
        PlayerCharacter pc => pc.MaxHP,
        Enemy e => e.MaxHP,
        Companion c => c.MaxHP,
        _ => 0
    };

    public GridPosition? CurrentPosition => Character switch
    {
        PlayerCharacter pc => GetPlayerPosition(pc),
        Enemy e => e.Position,
        Companion c => c.Position,
        _ => null
    };

    public bool IsAlive => CurrentHP > 0;

    public List<StatusEffect> ActiveEffects => Character switch
    {
        PlayerCharacter pc => pc.StatusEffects,
        Enemy e => e.StatusEffects,
        Companion c => c.StatusEffects,
        _ => new List<StatusEffect>()
    };
}

```

**Usage in CombatEngine:**

```csharp
public class CombatEngine
{
    public void ProcessTurn(List<Combatant> combatants)
    {
        foreach (var combatant in combatants.Where(c => c.IsAlive))
        {
            // Uniform interface regardless of actual type
            ApplyStatusEffects(combatant);
            ProcessAction(combatant);
        }
    }
}

```

---

### Template Method Pattern

**Purpose:** Define skeleton of algorithm in base class, defer steps to subclasses.

```csharp
// RuneAndRust.Core/Archetype.cs

public abstract class Archetype
{
    // Template properties
    public abstract CharacterClass ArchetypeType { get; }
    public abstract ResourceType PrimaryResource { get; }

    // Template methods - subclasses must implement
    public abstract Attributes GetBaseAttributes();
    public abstract List<Ability> GetStartingAbilities();
    public abstract List<string> GetWeaponProficiencies();
    public abstract List<string> GetArmorProficiencies();

    // Shared implementation
    public virtual int GetStartingHP(Attributes attributes)
    {
        return 10 + (attributes.Sturdiness * 2);
    }

    public virtual int GetStartingStamina(Attributes attributes)
    {
        return 5 + attributes.Might + attributes.Finesse;
    }
}

// RuneAndRust.Core/Archetypes/WarriorArchetype.cs

public class WarriorArchetype : Archetype
{
    public override CharacterClass ArchetypeType => CharacterClass.Warrior;
    public override ResourceType PrimaryResource => ResourceType.Stamina;

    public override Attributes GetBaseAttributes()
    {
        return new Attributes
        {
            Might = 4,
            Finesse = 2,
            Wits = 2,
            Will = 2,
            Sturdiness = 4
        };
    }

    public override List<Ability> GetStartingAbilities()
    {
        return new List<Ability>
        {
            AbilityLibrary.Get("Strike"),
            AbilityLibrary.Get("Defensive Stance"),
            AbilityLibrary.Get("Warrior's Vigor")
        };
    }

    public override List<string> GetWeaponProficiencies()
    {
        return new List<string> { "All" };  // Warriors can use all weapons
    }

    public override List<string> GetArmorProficiencies()
    {
        return new List<string> { "Light", "Medium", "Heavy", "Shields" };
    }
}

```

---

### Decorator Pattern

**Purpose:** Add behavior to objects dynamically.

### Equipment Bonuses

```csharp
// RuneAndRust.Core/Equipment.cs

public class Equipment
{
    public string Name { get; set; }
    public EquipmentType Type { get; set; }
    public int BaseDamageDice { get; set; }
    public int BaseDefenseBonus { get; set; }

    // Decorating bonuses
    public List<EquipmentBonus> Bonuses { get; set; } = new();

    public int GetTotalDamage()
    {
        var total = BaseDamageDice;
        foreach (var bonus in Bonuses.Where(b => b.AttributeName == "Damage"))
        {
            total += bonus.BonusValue;
        }
        return total;
    }
}

public class EquipmentBonus
{
    public string AttributeName { get; set; }  // "Damage", "Accuracy", "Defense"
    public int BonusValue { get; set; }
    public string Description { get; set; }
}

```

### Status Effect Stacking

```csharp
// Multiple status effects decorate character state

public class PlayerCharacter
{
    public List<StatusEffect> StatusEffects { get; set; } = new();

    public int GetEffectiveAccuracy()
    {
        var baseAccuracy = Attributes.Finesse + Attributes.Wits;

        // Each effect decorates the base value
        foreach (var effect in StatusEffects)
        {
            baseAccuracy += effect.AccuracyModifier;
        }

        return baseAccuracy;
    }
}

```

---

### Composite Pattern

**Purpose:** Treat individual objects and compositions uniformly.

### Quest Objectives

```csharp
// RuneAndRust.Core/Quests/Quest.cs

public class Quest
{
    public string Id { get; set; }
    public string Title { get; set; }
    public List<BaseQuestObjective> Objectives { get; set; } = new();

    // Composite check - all objectives must be complete
    public bool IsComplete => Objectives.All(o => o.IsComplete);

    // Progress as percentage of completed objectives
    public float Progress => (float)Objectives.Count(o => o.IsComplete) / Objectives.Count;
}

```

### Ambient Conditions

```csharp
// Multiple conditions stack in a room

public class Room
{
    public List<AmbientCondition> Conditions { get; set; } = new();

    // Composite modifier calculation
    public int GetTotalAccuracyModifier()
    {
        return Conditions.Sum(c => c.AccuracyModifier);
    }

    public int GetTotalStressPerTurn()
    {
        return Conditions.Sum(c => c.StressPerTurn);
    }
}

```

---

### Repository Pattern

**Purpose:** Abstract data access from business logic.

See [Data Access Patterns](https://www.notion.so/data-access-patterns.md) for full documentation.

```csharp
// Example usage
public class SaveRepository
{
    public void SaveGame(PlayerCharacter player, WorldState world) { }
    public PlayerCharacter? LoadGame(string characterName) { }
    public List<SaveInfo> ListSaves() { }
    public void DeleteSave(string name) { }
}

```

---

### Rule Engine Pattern

**Purpose:** Evaluate and apply rules dynamically.

```csharp
// RuneAndRust.Engine/CoherentGlitch/CoherentGlitchRuleEngine.cs

public class CoherentGlitchRuleEngine
{
    private readonly List<CoherentGlitchRule> _rules;

    public CoherentGlitchRuleEngine()
    {
        _rules = new List<CoherentGlitchRule>
        {
            new BossArenaAmplifierRule(),
            new FloodedElectricalDangerRule(),
            new ChasmInfrastructureRule(),
            new ResourceVeinClusterRule(),
            new TacticalCoverPlacementRule(),
            // ... 16+ rules
        };

        // Sort by priority
        _rules = _rules.OrderBy(r => r.Priority).ToList();
    }

    public void ApplyRules(ProceduralSector sector)
    {
        foreach (var rule in _rules)
        {
            if (rule.CanApply(sector))
            {
                rule.Apply(sector);
            }
        }
    }
}

```

---

## Interface Definitions

### Core Interfaces

| Interface | Purpose | Implementations |
| --- | --- | --- |
| `IDiceRoller` | Dice rolling contract | `DiceService` |
| `ICommand` | Player action execution | 20+ command classes |
| `IDescriptorService` | Flavor text generation | `DescriptorService` |

### AI Interfaces

| Interface | Purpose |
| --- | --- |
| `IThreatAssessmentService` | Evaluate threats to AI |
| `ITargetSelectionService` | Choose targets |
| `IBehaviorPatternService` | Determine AI behavior |
| `IAbilityRotationService` | Manage ability usage order |
| `IDifficultyScalingService` | Scale encounter difficulty |
| `IAdaptiveDifficultyService` | Dynamic difficulty adjustment |
| `IBossAIService` | Boss-specific AI logic |
| `IAIPerformanceMonitor` | Monitor AI performance |
| `IAIDebugService` | Debug AI decisions |

### Spatial Interfaces

| Interface | Purpose |
| --- | --- |
| `ISpatialLayoutService` | Manage battlefield layout |
| `ISpatialValidationService` | Validate grid positions |
| `IVerticalTraversalService` | Handle elevation changes |

---

## Summary

| Pattern | Primary Use | Key Files |
| --- | --- | --- |
| **Factory** | Object creation | `CharacterFactory.cs`, `EnemyFactory.cs` |
| **Command** | Player input handling | `Commands/*.cs` |
| **Strategy** | AI behavior variation | `AI/*.cs` interfaces |
| **Adapter** | Character type unification | `Combatant.cs` |
| **Template Method** | Archetype configuration | `Archetype.cs`, `*Archetype.cs` |
| **Decorator** | Equipment bonuses, effects | `Equipment.cs`, `StatusEffect` |
| **Composite** | Quest objectives, conditions | `Quest.cs`, `Room.cs` |
| **Repository** | Data access | `*Repository.cs` |
| **Rule Engine** | Procedural generation | `CoherentGlitchRuleEngine.cs` |

## Related Documentation

- [Service Architecture](https://www.notion.so/service-architecture.md) - Service organization
- [Data Flow](https://www.notion.so/data-flow.md) - How data moves through patterns
- [System Integration Map](https://www.notion.so/system-integration-map.md) - Pattern interactions