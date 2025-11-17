using RuneAndRust.Core.Descriptors;
using RuneAndRust.Persistence;
using Serilog;
using System.Text.Json;

namespace RuneAndRust.Engine;

/// <summary>
/// v0.38.3: Service interface for interactive object generation and interaction
/// </summary>
public interface IObjectInteractionService
{
    /// <summary>
    /// Generates an interactive object from base template + modifier + function variant
    /// </summary>
    InteractiveObject GenerateObject(
        string baseTemplateName,
        string? modifierName = null,
        string? functionVariant = null,
        int roomId = 0);

    /// <summary>
    /// Attempts to interact with an object
    /// </summary>
    InteractionResult AttemptInteraction(InteractiveObject obj, int characterId = 0);

    /// <summary>
    /// Gets all interactive objects for a room
    /// </summary>
    List<InteractiveObject> GetObjectsForRoom(int roomId);
}

/// <summary>
/// v0.38.3: Object Interaction Service
/// Generates interactive objects from descriptor templates
/// Handles interaction resolution, skill checks, and consequences
/// </summary>
public class ObjectInteractionService : IObjectInteractionService
{
    private readonly DescriptorRepository _repository;
    private readonly ILogger _logger;
    private readonly Random _random;

    public ObjectInteractionService(
        DescriptorRepository repository,
        ILogger logger,
        Random? random = null)
    {
        _repository = repository;
        _logger = logger;
        _random = random ?? new Random();
    }

    /// <summary>
    /// Generate interactive object from descriptor composite.
    /// </summary>
    public InteractiveObject GenerateObject(
        string baseTemplateName,
        string? modifierName = null,
        string? functionVariant = null,
        int roomId = 0)
    {
        try
        {
            var baseTemplate = _repository.GetBaseTemplate(baseTemplateName);
            if (baseTemplate == null)
            {
                _logger.Error(
                    "Base template not found: {BaseTemplateName}",
                    baseTemplateName);
                throw new ArgumentException($"Base template not found: {baseTemplateName}");
            }

            ThematicModifier? modifier = null;
            if (!string.IsNullOrEmpty(modifierName))
            {
                modifier = _repository.GetModifier(modifierName);
                if (modifier == null)
                {
                    _logger.Warning(
                        "Modifier not found, using unmodified template: {ModifierName}",
                        modifierName);
                }
            }

            // Parse base mechanics
            var mechanics = ObjectMechanics.FromJson(baseTemplate.BaseMechanics);
            if (mechanics == null)
            {
                _logger.Error(
                    "Failed to parse base mechanics for template: {TemplateName}",
                    baseTemplateName);
                throw new InvalidOperationException($"Invalid base mechanics for template: {baseTemplateName}");
            }

            // Apply function variant if specified
            if (!string.IsNullOrEmpty(functionVariant))
            {
                mechanics = ApplyFunctionVariant(mechanics, baseTemplateName, functionVariant);
            }

            // Generate name and description
            var name = GenerateName(baseTemplate.NameTemplate, modifier, functionVariant);
            var description = GenerateDescription(
                baseTemplate.DescriptionTemplate,
                baseTemplate,
                modifier,
                mechanics);

            _logger.Debug(
                "Generated interactive object: {Name} (Base: {Base}, Modifier: {Modifier}, Function: {Function})",
                name,
                baseTemplateName,
                modifierName ?? "None",
                functionVariant ?? "None");

            return CreateInteractiveObject(
                name,
                description,
                baseTemplate,
                modifier,
                mechanics,
                roomId);
        }
        catch (Exception ex)
        {
            _logger.Error(ex,
                "Error generating interactive object: Base={Base}, Modifier={Modifier}, Function={Function}",
                baseTemplateName,
                modifierName,
                functionVariant);
            throw;
        }
    }

    /// <summary>
    /// Attempt interaction with object.
    /// </summary>
    public InteractionResult AttemptInteraction(InteractiveObject obj, int characterId = 0)
    {
        try
        {
            _logger.Information(
                "Attempting interaction with {ObjName} (Type: {Type}, Interaction: {Interaction})",
                obj.Name,
                obj.ObjectType,
                obj.InteractionType);

            // Check if object can be interacted with
            if (!obj.CanInteract())
            {
                if (obj.Interacted && !obj.Reversible)
                {
                    return InteractionResult.AlreadyUsed(
                        $"The {obj.Name} has already been used.");
                }

                if (obj.AttemptsRemaining <= 0)
                {
                    return InteractionResult.FailureResult(
                        $"The {obj.Name} is locked out (no attempts remaining).");
                }

                if (obj.IsDestructible && obj.CurrentHP <= 0)
                {
                    return InteractionResult.FailureResult(
                        $"The {obj.Name} is destroyed.");
                }
            }

            // Check requirements (keycard, etc.)
            if (obj.IsLocked && !string.IsNullOrEmpty(obj.Requires))
            {
                // In full implementation, would check character inventory
                return InteractionResult.InsufficientRequirements(
                    $"The {obj.Name} requires {obj.Requires}.",
                    obj.Requires);
            }

            // Check if requires skill check
            if (!obj.RequiresCheck)
            {
                return ExecuteInteraction(obj, null);
            }

            // Resolve skill check
            var checkResult = ResolveSkillCheck(obj.CheckType, obj.CheckDC);

            // Decrement attempts
            if (obj.AttemptsAllowed > 0)
            {
                obj.AttemptsRemaining--;
            }

            if (checkResult.Success)
            {
                return ExecuteInteraction(obj, checkResult);
            }
            else
            {
                return HandleFailure(obj, checkResult);
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex,
                "Error during interaction: Obj={ObjId}",
                obj.ObjectId);
            throw;
        }
    }

    /// <summary>
    /// Gets all interactive objects for a room
    /// </summary>
    public List<InteractiveObject> GetObjectsForRoom(int roomId)
    {
        // In full implementation, would query database
        _logger.Debug("Getting objects for room: {RoomId}", roomId);
        return new List<InteractiveObject>();
    }

    #region Private Helper Methods

    private InteractiveObject CreateInteractiveObject(
        string name,
        string description,
        DescriptorBaseTemplate baseTemplate,
        ThematicModifier? modifier,
        ObjectMechanics mechanics,
        int roomId)
    {
        var obj = new InteractiveObject
        {
            RoomId = roomId,
            BaseTemplateName = baseTemplate.TemplateName,
            ModifierName = modifier?.ModifierName,
            Name = name,
            Description = description,
            ObjectType = ParseObjectType(baseTemplate.Archetype),
            InteractionType = ParseInteractionType(mechanics.InteractionType),
            RequiresCheck = mechanics.RequiresCheck,
            CheckType = ParseSkillCheckType(mechanics.CheckType),
            CheckDC = mechanics.CheckDC,
            AttemptsAllowed = mechanics.AttemptsAllowed > 0 ? mechanics.AttemptsAllowed : 1,
            AttemptsRemaining = mechanics.AttemptsAllowed > 0 ? mechanics.AttemptsAllowed : 1,
            CurrentState = mechanics.States?.FirstOrDefault(),
            States = mechanics.States ?? new List<string>(),
            Interacted = false,
            Reversible = mechanics.Reversible,
            IsLocked = mechanics.Locked,
            Requires = mechanics.Requires,
            IsDestructible = mechanics.Destructible,
            HP = mechanics.HP,
            CurrentHP = mechanics.HP,
            Soak = mechanics.Soak,
            BlocksLoS = mechanics.BlocksLoS,
            LootTier = ParseLootTier(mechanics.LootTier),
            TrapChance = mechanics.TrapChance,
            ConsequenceType = ParseConsequenceType(mechanics.ConsequenceType),
            ConsequenceData = mechanics.Consequence != null
                ? JsonSerializer.Serialize(mechanics.Consequence)
                : null,
            FailureConsequence = mechanics.FailureConsequence,
            BiomeRestriction = modifier?.PrimaryBiome,
            Tags = baseTemplate.GetTags()
        };

        // Determine if trapped
        if (obj.TrapChance > 0 && _random.NextDouble() < obj.TrapChance)
        {
            obj.IsTrap = true;
        }

        return obj;
    }

    private ObjectMechanics ApplyFunctionVariant(
        ObjectMechanics baseMechanics,
        string baseTemplateName,
        string functionName)
    {
        // In full implementation, would query Object_Function_Variants table
        // For now, return base mechanics
        _logger.Debug(
            "Applying function variant: {Function} to {Template}",
            functionName,
            baseTemplateName);

        return baseMechanics;
    }

    private string GenerateName(
        string template,
        ThematicModifier? modifier,
        string? functionVariant)
    {
        if (modifier == null)
        {
            return template
                .Replace("{Modifier} ", "")
                .Replace(" {Modifier}", "")
                .Replace("{Modifier}", "Standard")
                .Replace("{Entity_Type}", functionVariant ?? "Unknown");
        }

        return template
            .Replace("{Modifier}", modifier.ModifierName.Replace("_", " "))
            .Replace("{Modifier_Adj}", modifier.Adjective)
            .Replace("{Entity_Type}", functionVariant ?? "Unknown");
    }

    private string GenerateDescription(
        string template,
        DescriptorBaseTemplate baseTemplate,
        ThematicModifier? modifier,
        ObjectMechanics mechanics)
    {
        var description = template;

        if (modifier != null)
        {
            description = description
                .Replace("{Modifier_Adj}", modifier.Adjective)
                .Replace("{Modifier_Detail}", modifier.DetailFragment);
        }
        else
        {
            description = description
                .Replace("{Modifier_Adj}", "standard")
                .Replace("{Modifier_Detail}", "shows typical construction");
        }

        // Replace state placeholder
        if (mechanics.States != null && mechanics.States.Count > 0)
        {
            description = description.Replace("{State}", mechanics.States[0]);
        }

        // Replace display state placeholder (for consoles)
        description = description.Replace("{Display_State}", "flickers with activity");

        // Replace locked state placeholder
        description = description.Replace("{Locked_State}",
            mechanics.Locked ? "locked" : "unlocked");

        // Replace article placeholder
        description = description.Replace("{Article}", GetArticle(modifier?.Adjective ?? "a"));
        description = description.Replace("{Article_Cap}", GetArticle(modifier?.Adjective ?? "a", capitalize: true));

        return description;
    }

    private string GetArticle(string nextWord, bool capitalize = false)
    {
        var article = "aeiouAEIOU".Contains(nextWord[0]) ? "an" : "a";
        return capitalize ? char.ToUpper(article[0]) + article.Substring(1) : article;
    }

    private SkillCheckResult ResolveSkillCheck(SkillCheckType checkType, int dc)
    {
        // Simplified skill check - roll 5 dice (d10)
        var roll = new List<int>();
        for (int i = 0; i < 5; i++)
        {
            roll.Add(_random.Next(1, 11));
        }

        // Count successes (7+ is a success)
        var successes = roll.Count(d => d >= 7);

        var success = successes > 0;

        _logger.Debug(
            "Skill check: {Type} DC {DC} - Roll: [{Roll}] = {Successes} success{Plural}",
            checkType,
            dc,
            string.Join(", ", roll),
            successes,
            successes != 1 ? "es" : "");

        return new SkillCheckResult
        {
            Success = success,
            Successes = successes,
            Roll = roll,
            CheckType = checkType,
            DC = dc
        };
    }

    private InteractionResult ExecuteInteraction(
        InteractiveObject obj,
        SkillCheckResult? checkResult)
    {
        // Mark as interacted
        obj.Interacted = true;

        // Execute consequence
        var consequence = ExecuteConsequence(obj);

        // Update state
        if (obj.Reversible && !string.IsNullOrEmpty(obj.CurrentState))
        {
            obj.ToggleState();
        }

        _logger.Information(
            "Interaction successful: Obj={ObjName}, Consequence={Type}",
            obj.Name,
            obj.ConsequenceType);

        var description = $"You {obj.InteractionType.ToString().ToLower()} the {obj.Name}.";

        if (checkResult != null)
        {
            description += $" [{checkResult.CheckType} Check DC {checkResult.DC}] {checkResult.GetRollString()}. Success!";
        }

        if (consequence.Executed)
        {
            description += $" {consequence.Description}";
        }

        return new InteractionResult
        {
            Success = true,
            Description = description,
            CheckResult = checkResult,
            Consequence = consequence,
            StateChanged = obj.Reversible,
            NewState = obj.CurrentState
        };
    }

    private ConsequenceResult ExecuteConsequence(InteractiveObject obj)
    {
        return obj.ConsequenceType switch
        {
            ConsequenceType.Unlock => ExecuteUnlock(obj),
            ConsequenceType.Trigger => ExecuteTrigger(obj),
            ConsequenceType.Spawn => ExecuteSpawn(obj),
            ConsequenceType.Reveal => ExecuteReveal(obj),
            ConsequenceType.Loot => ExecuteLoot(obj),
            ConsequenceType.Trap => ExecuteTrap(obj),
            _ => ConsequenceResult.None()
        };
    }

    private ConsequenceResult ExecuteUnlock(InteractiveObject obj)
    {
        var description = obj.ConsequenceType switch
        {
            ConsequenceType.Unlock when obj.ObjectType == InteractiveObjectType.Barrier =>
                "The door swings open with a creak.",
            ConsequenceType.Unlock when obj.ObjectType == InteractiveObjectType.Container =>
                "The lock clicks open.",
            _ => "You hear the sound of a mechanism unlocking."
        };

        obj.IsLocked = false;

        return ConsequenceResult.Unlock(description, obj.Name);
    }

    private ConsequenceResult ExecuteTrigger(InteractiveObject obj)
    {
        var description = "The mechanism activates.";

        // Parse consequence data for specific trigger actions
        if (!string.IsNullOrEmpty(obj.ConsequenceData))
        {
            try
            {
                var consequenceData = JsonSerializer.Deserialize<Dictionary<string, object>>(obj.ConsequenceData);
                if (consequenceData != null && consequenceData.ContainsKey("action"))
                {
                    var action = consequenceData["action"].ToString();
                    description = action switch
                    {
                        "disable_hazards" => "You hear steam vents shutting down throughout the room.",
                        "extend_bridge" => "A bridge extends across the chasm with grinding metal sounds.",
                        "lock_doors" => "All exits seal with a heavy clang!",
                        _ => description
                    };
                }
            }
            catch
            {
                // Use default description
            }
        }

        return ConsequenceResult.Trigger(description);
    }

    private ConsequenceResult ExecuteSpawn(InteractiveObject obj)
    {
        var description = "Something emerges from the shadows!";

        return ConsequenceResult.Spawn(description, "enemies");
    }

    private ConsequenceResult ExecuteReveal(InteractiveObject obj)
    {
        var description = "You discover something interesting.";
        string? narrative = null;
        string? questHook = null;

        // For data slates and investigatable objects
        if (obj.ObjectType == InteractiveObjectType.Investigatable)
        {
            narrative = "The data-slate displays corrupted text fragments about Jötun technology.";
            questHook = "Mentions coordinates to a hidden research facility.";
        }

        return ConsequenceResult.Reveal(description, narrative, questHook);
    }

    private ConsequenceResult ExecuteLoot(InteractiveObject obj)
    {
        if (obj.LootTaken)
        {
            return ConsequenceResult.Loot("The container is empty.");
        }

        obj.LootTaken = true;

        var loot = GenerateLoot(obj.LootTier);
        var description = $"You find: {string.Join(", ", loot)}.";

        return ConsequenceResult.Loot(description, loot);
    }

    private ConsequenceResult ExecuteTrap(InteractiveObject obj)
    {
        if (obj.TrapDisarmed)
        {
            return ConsequenceResult.None();
        }

        var damage = obj.TrapDamage ?? "2d6";
        var description = $"A trap triggers! You take {damage} damage.";

        return ConsequenceResult.Trap(description, damage);
    }

    private InteractionResult HandleFailure(
        InteractiveObject obj,
        SkillCheckResult checkResult)
    {
        _logger.Information(
            "Interaction failed: Obj={ObjName}, Roll={Roll}",
            obj.Name,
            checkResult.GetRollString());

        var description = $"You fail to {obj.InteractionType.ToString().ToLower()} the {obj.Name}.";
        description += $" [{checkResult.CheckType} Check DC {checkResult.DC}] {checkResult.GetRollString()}. Failure.";

        // Check for critical failure consequences
        if (!string.IsNullOrEmpty(obj.FailureConsequence))
        {
            if (obj.FailureConsequence == "Lockout")
            {
                description += " The console locks out, denying further access.";
            }
            else if (obj.FailureConsequence == "spawn_enemies")
            {
                description += " The system activates an alarm! Servitors converge on your position.";
            }
        }

        return InteractionResult.FailureResult(description, checkResult, obj.FailureConsequence);
    }

    private List<string> GenerateLoot(LootTier tier)
    {
        var loot = new List<string>();

        switch (tier)
        {
            case LootTier.Common:
                loot.Add($"{_random.Next(10, 50)} Dvergr Cogs");
                if (_random.NextDouble() < 0.5)
                    loot.Add("1× Healing Poultice");
                break;

            case LootTier.Uncommon:
                loot.Add($"{_random.Next(50, 150)} Dvergr Cogs");
                loot.Add("1× Repair Kit");
                if (_random.NextDouble() < 0.3)
                    loot.Add("1× Weapon Component");
                break;

            case LootTier.Rare:
                loot.Add($"{_random.Next(150, 500)} Dvergr Cogs");
                loot.Add("1× Advanced Repair Kit");
                loot.Add("1× Rare Component");
                break;

            default:
                loot.Add($"{_random.Next(5, 30)} Dvergr Cogs");
                break;
        }

        return loot;
    }

    private InteractiveObjectType ParseObjectType(string archetype)
    {
        return archetype switch
        {
            "Mechanism" => InteractiveObjectType.Mechanism,
            "Container" => InteractiveObjectType.Container,
            "Investigatable" => InteractiveObjectType.Investigatable,
            "Barrier" => InteractiveObjectType.Barrier,
            _ => InteractiveObjectType.Mechanism
        };
    }

    private InteractionType ParseInteractionType(string type)
    {
        return type switch
        {
            "Pull" => InteractionType.Pull,
            "Open" => InteractionType.Open,
            "Search" => InteractionType.Search,
            "Read" => InteractionType.Read,
            "Hack" => InteractionType.Hack,
            "Automatic" => InteractionType.Automatic,
            "Examine" => InteractionType.Examine,
            _ => InteractionType.Examine
        };
    }

    private SkillCheckType ParseSkillCheckType(string? type)
    {
        if (string.IsNullOrEmpty(type))
            return SkillCheckType.None;

        return type switch
        {
            "WITS" => SkillCheckType.WITS,
            "MIGHT" => SkillCheckType.MIGHT,
            "Lockpicking" => SkillCheckType.Lockpicking,
            "Hacking" => SkillCheckType.Hacking,
            _ => SkillCheckType.None
        };
    }

    private LootTier ParseLootTier(string? tier)
    {
        if (string.IsNullOrEmpty(tier))
            return LootTier.None;

        return tier switch
        {
            "Common" => LootTier.Common,
            "Uncommon" => LootTier.Uncommon,
            "Rare" => LootTier.Rare,
            "Random" => LootTier.Random,
            _ => LootTier.None
        };
    }

    private ConsequenceType ParseConsequenceType(string? type)
    {
        if (string.IsNullOrEmpty(type))
            return ConsequenceType.None;

        return type switch
        {
            "Unlock" => ConsequenceType.Unlock,
            "Trigger" => ConsequenceType.Trigger,
            "Spawn" => ConsequenceType.Spawn,
            "Reveal" => ConsequenceType.Reveal,
            "Loot" => ConsequenceType.Loot,
            "Trap" => ConsequenceType.Trap,
            _ => ConsequenceType.None
        };
    }

    #endregion
}
