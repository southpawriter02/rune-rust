using RuneAndRust.Core;
using RuneAndRust.Core.NewGamePlus;
using RuneAndRust.Persistence;
using Serilog;

namespace RuneAndRust.Engine.NewGamePlus;

/// <summary>
/// v0.40.1: New Game+ Service
/// Manages New Game+ initialization, tier progression, and completion tracking
/// </summary>
public class NewGamePlusService
{
    private static readonly ILogger _log = Log.ForContext<NewGamePlusService>();
    private readonly NewGamePlusRepository _repository;
    private readonly CarryoverService _carryoverService;

    public NewGamePlusService(NewGamePlusRepository repository, CarryoverService carryoverService)
    {
        _repository = repository;
        _carryoverService = carryoverService;
    }

    // ═════════════════════════════════════════════════════════════
    // NG+ STATUS & AVAILABILITY
    // ═════════════════════════════════════════════════════════════

    /// <summary>
    /// Get comprehensive NG+ information for a character
    /// </summary>
    public NewGamePlusInfo GetAvailableTiers(int characterId)
    {
        using var operation = _log.BeginOperation("GetAvailableTiers for CharacterId={CharacterId}", characterId);

        var currentTier = _repository.GetCurrentNGPlusTier(characterId);
        var highestTier = _repository.GetHighestNGPlusTier(characterId);
        var hasCompleted = _repository.HasCompletedCampaign(characterId);
        var completions = _repository.GetNGPlusCompletionCount(characterId);

        var info = new NewGamePlusInfo
        {
            CurrentTier = currentTier,
            HighestTierCompleted = highestTier,
            HasCompletedCampaign = hasCompleted,
            TotalNGPlusCompletions = completions,
            AvailableTiers = GetUnlockedTiers(hasCompleted, highestTier),
            NextTierRequirement = GetNextTierRequirement(hasCompleted, highestTier)
        };

        _log.Information(
            "Character {CharacterId} NG+ status: Current={Current}, Highest={Highest}, Available={AvailableCount}",
            characterId, info.CurrentTier, info.HighestTierCompleted, info.AvailableTiers.Count);

        operation.Complete();
        return info;
    }

    /// <summary>
    /// Get the current NG+ tier for a character
    /// </summary>
    public int GetCurrentNGPlusTier(int characterId)
    {
        return _repository.GetCurrentNGPlusTier(characterId);
    }

    /// <summary>
    /// Check if a character can access a specific NG+ tier
    /// </summary>
    public bool CanAccessTier(int characterId, int tier)
    {
        if (tier < 1 || tier > 5)
        {
            return false;
        }

        var hasCompleted = _repository.HasCompletedCampaign(characterId);
        if (!hasCompleted)
        {
            _log.Debug("Character {CharacterId} cannot access NG+{Tier}: Campaign not completed",
                characterId, tier);
            return false;
        }

        var highestTier = _repository.GetHighestNGPlusTier(characterId);

        // Must complete previous tier first (no skipping)
        var canAccess = tier <= highestTier + 1;

        if (!canAccess)
        {
            _log.Debug("Character {CharacterId} cannot access NG+{Tier}: Must complete NG+{RequiredTier} first",
                characterId, tier, highestTier);
        }

        return canAccess;
    }

    private List<int> GetUnlockedTiers(bool hasCompletedCampaign, int highestTier)
    {
        if (!hasCompletedCampaign)
        {
            return new List<int>();
        }

        // Can access tiers 1 through (highest + 1), capped at 5
        var available = new List<int>();
        for (int tier = 1; tier <= Math.Min(5, highestTier + 1); tier++)
        {
            available.Add(tier);
        }

        return available;
    }

    private string GetNextTierRequirement(bool hasCompletedCampaign, int highestTier)
    {
        if (!hasCompletedCampaign)
        {
            return "Complete the campaign to unlock New Game+";
        }

        if (highestTier >= 5)
        {
            return "All NG+ tiers unlocked (Maximum: NG+5)";
        }

        return $"Complete NG+{highestTier} to unlock NG+{highestTier + 1}";
    }

    // ═════════════════════════════════════════════════════════════
    // NG+ INITIALIZATION
    // ═════════════════════════════════════════════════════════════

    /// <summary>
    /// Initialize a New Game+ run for a character
    /// Creates carryover snapshot, resets world state, and applies tier
    /// </summary>
    public bool InitializeNewGamePlus(PlayerCharacter character, int targetTier)
    {
        using var operation = _log.BeginOperation(
            "InitializeNewGamePlus: CharacterId={CharacterId}, TargetTier={Tier}",
            character.CharacterID, targetTier);

        // Validation: Campaign completion
        if (!_repository.HasCompletedCampaign(character.CharacterID))
        {
            _log.Warning(
                "Character {CharacterId} attempted NG+ without campaign completion",
                character.CharacterID);
            return false;
        }

        // Validation: Tier bounds
        if (targetTier < 1 || targetTier > 5)
        {
            _log.Warning(
                "Character {CharacterId} attempted invalid NG+ tier: {Tier}",
                character.CharacterID, targetTier);
            return false;
        }

        // Validation: No tier skipping
        var highestTier = _repository.GetHighestNGPlusTier(character.CharacterID);
        if (targetTier > highestTier + 1)
        {
            _log.Warning(
                "Character {CharacterId} attempted to skip to NG+{Target} from NG+{Current}",
                character.CharacterID, targetTier, highestTier);
            return false;
        }

        try
        {
            // STEP 1: Create carryover snapshot
            _log.Information("Creating carryover snapshot for character {CharacterId}", character.CharacterID);
            var snapshot = _carryoverService.CreateSnapshot(character, targetTier);
            _repository.SaveCarryoverSnapshot(snapshot);

            // STEP 2: Reset Trauma Economy (fresh psychological slate)
            _log.Information("Resetting Trauma Economy for character {CharacterId}", character.CharacterID);
            ResetTraumaEconomy(character);

            // STEP 3: Apply carryover data (handled by CarryoverService)
            // Note: World state reset and quest reset would be handled by other services
            // For now, we just log that it should happen
            _log.Information("NG+ initialization: World state and quest reset should be applied");

            // STEP 4: Set NG+ tier
            _repository.SetCurrentNGPlusTier(character.CharacterID, targetTier);

            _log.Information(
                "NG+{Tier} initialization complete for character {CharacterId}",
                targetTier, character.CharacterID);

            operation.Complete();
            return true;
        }
        catch (Exception ex)
        {
            _log.Error(ex,
                "Failed to initialize NG+{Tier} for character {CharacterId}: {Error}",
                targetTier, character.CharacterID, ex.Message);
            return false;
        }
    }

    /// <summary>
    /// Reset Trauma Economy (Stress, Corruption, Traumas)
    /// </summary>
    private void ResetTraumaEconomy(PlayerCharacter character)
    {
        character.PsychicStress = 0;
        character.Corruption = 0;
        character.Traumas.Clear();

        _log.Debug("Trauma Economy reset: CharacterId={CharacterId}, Stress=0, Corruption=0, Traumas=0",
            character.CharacterID);
    }

    // ═════════════════════════════════════════════════════════════
    // NG+ COMPLETION
    // ═════════════════════════════════════════════════════════════

    /// <summary>
    /// Mark an NG+ tier as completed
    /// Updates highest tier and logs completion
    /// </summary>
    public bool CompleteNewGamePlusTier(int characterId, int? playtimeSeconds = null,
        int deaths = 0, int bossesDefeated = 0)
    {
        using var operation = _log.BeginOperation(
            "CompleteNewGamePlusTier: CharacterId={CharacterId}",
            characterId);

        var currentTier = _repository.GetCurrentNGPlusTier(characterId);
        var highestTier = _repository.GetHighestNGPlusTier(characterId);

        // Update highest tier if this is a new record
        if (currentTier > highestTier)
        {
            _repository.SetHighestNGPlusTier(characterId, currentTier);
            _log.Information(
                "Character {CharacterId} achieved new highest NG+ tier: {Tier}",
                characterId, currentTier);
        }

        // Increment completion counter
        _repository.IncrementNGPlusCompletions(characterId);

        // Log completion
        _repository.LogCompletion(characterId, currentTier, playtimeSeconds, deaths, bossesDefeated);

        _log.Information(
            "Character {CharacterId} completed NG+{Tier}",
            characterId, currentTier);

        operation.Complete();
        return true;
    }

    /// <summary>
    /// Mark character as having completed the campaign (unlocks NG+1)
    /// </summary>
    public void MarkCampaignComplete(int characterId)
    {
        _repository.MarkCampaignComplete(characterId);

        _log.Information(
            "Character {CharacterId} completed campaign - NG+1 now available",
            characterId);
    }

    // ═════════════════════════════════════════════════════════════
    // SCALING PARAMETERS
    // ═════════════════════════════════════════════════════════════

    /// <summary>
    /// Get scaling parameters for a specific tier
    /// </summary>
    public NGPlusScaling? GetScalingForTier(int tier)
    {
        if (tier < 0 || tier > 5)
        {
            return null;
        }

        if (tier == 0)
        {
            // Normal mode - no scaling
            return new NGPlusScaling
            {
                Tier = 0,
                DifficultyMultiplier = 1.0f,
                EnemyLevelIncrease = 0,
                BossPhaseThresholdReduction = 0.0f,
                CorruptionRateMultiplier = 1.0f,
                LegendRewardMultiplier = 1.0f,
                Description = "Normal difficulty (first playthrough)"
            };
        }

        return _repository.GetScalingForTier(tier);
    }

    /// <summary>
    /// Get all scaling tiers (1-5)
    /// </summary>
    public List<NGPlusScaling> GetAllScalingTiers()
    {
        return _repository.GetAllScalingTiers();
    }

    // ═════════════════════════════════════════════════════════════
    // COMPLETION HISTORY
    // ═════════════════════════════════════════════════════════════

    /// <summary>
    /// Get all completions for a character
    /// </summary>
    public List<NGPlusCompletion> GetCompletions(int characterId)
    {
        return _repository.GetCompletions(characterId);
    }

    /// <summary>
    /// Get completions for a specific tier
    /// </summary>
    public List<NGPlusCompletion> GetCompletionsForTier(int characterId, int tier)
    {
        return _repository.GetCompletionsForTier(characterId, tier);
    }
}

/// <summary>
/// Interface for log operations that can be marked as complete
/// </summary>
internal interface ILogOperation : IDisposable
{
    void Complete();
}

/// <summary>
/// Extension methods for Serilog operation tracking
/// </summary>
internal static class LoggerExtensions
{
    public static ILogOperation BeginOperation(this ILogger logger, string messageTemplate, params object[] args)
    {
        var operationId = Guid.NewGuid();
        logger.Debug("BEGIN: " + messageTemplate + " | OperationId={OperationId}", args.Concat(new object[] { operationId }).ToArray());

        return new LogOperation(logger, messageTemplate, args, operationId);
    }

    private class LogOperation : ILogOperation
    {
        private readonly ILogger _logger;
        private readonly string _messageTemplate;
        private readonly object[] _args;
        private readonly Guid _operationId;
        private readonly DateTime _startTime;
        private bool _completed;

        public LogOperation(ILogger logger, string messageTemplate, object[] args, Guid operationId)
        {
            _logger = logger;
            _messageTemplate = messageTemplate;
            _args = args;
            _operationId = operationId;
            _startTime = DateTime.UtcNow;
            _completed = false;
        }

        public void Complete()
        {
            _completed = true;
        }

        public void Dispose()
        {
            var duration = DateTime.UtcNow - _startTime;
            var status = _completed ? "COMPLETE" : "INCOMPLETE";

            _logger.Debug(
                "{Status}: " + _messageTemplate + " | OperationId={OperationId} | Duration={Duration}ms",
                new object[] { status }
                    .Concat(_args)
                    .Concat(new object[] { _operationId, duration.TotalMilliseconds })
                    .ToArray());
        }
    }
}
