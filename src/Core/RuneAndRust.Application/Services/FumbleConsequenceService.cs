namespace RuneAndRust.Application.Services;

using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Interfaces;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Service for managing fumble consequences.
/// </summary>
public sealed class FumbleConsequenceService : IFumbleConsequenceService
{
    private readonly IFumbleConsequenceRepository _repository;
    private readonly IFumbleConsequenceConfigurationProvider _configProvider;
    private readonly ILogger<FumbleConsequenceService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="FumbleConsequenceService"/> class.
    /// </summary>
    /// <param name="repository">The fumble consequence repository.</param>
    /// <param name="configProvider">The fumble consequence configuration provider.</param>
    /// <param name="logger">The logger instance.</param>
    public FumbleConsequenceService(
        IFumbleConsequenceRepository repository,
        IFumbleConsequenceConfigurationProvider configProvider,
        ILogger<FumbleConsequenceService> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _configProvider = configProvider ?? throw new ArgumentNullException(nameof(configProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public FumbleConsequence CreateConsequence(
        string characterId,
        string skillId,
        string? targetId,
        SkillContext? context)
    {
        var fumbleType = DetermineFumbleType(skillId);
        var config = _configProvider.GetConfiguration(fumbleType);

        var consequence = new FumbleConsequence(
            consequenceId: GenerateConsequenceId(),
            characterId: characterId,
            skillId: skillId,
            consequenceType: fumbleType,
            targetId: targetId,
            appliedAt: DateTime.UtcNow,
            expiresAt: config.Duration.HasValue
                ? DateTime.UtcNow.Add(config.Duration.Value)
                : null,
            description: config.Description,
            recoveryCondition: config.RecoveryCondition);

        _repository.Add(consequence);

        _logger.LogInformation(
            "Created fumble consequence {ConsequenceId} of type {FumbleType} for character {CharacterId} " +
            "on skill {SkillId} targeting {TargetId}",
            consequence.ConsequenceId,
            fumbleType,
            characterId,
            skillId,
            targetId ?? "none");

        return consequence;
    }

    /// <inheritdoc />
    public IReadOnlyList<FumbleConsequence> GetActiveConsequences(string characterId)
    {
        return _repository.GetActiveByCharacter(characterId);
    }

    /// <inheritdoc />
    public IReadOnlyList<FumbleConsequence> GetConsequencesAffectingCheck(
        string characterId,
        string skillId,
        string? targetId)
    {
        var activeConsequences = _repository.GetActiveByCharacter(characterId);

        return activeConsequences
            .Where(c => c.BlocksCheck(skillId, targetId) ||
                       c.GetDicePenalty() != 0 ||
                       c.GetDcModifier() != 0)
            .ToList();
    }

    /// <inheritdoc />
    public bool IsCheckBlocked(string characterId, string skillId, string? targetId)
    {
        var activeConsequences = _repository.GetActiveByCharacter(characterId);

        var blockingConsequence = activeConsequences
            .FirstOrDefault(c => c.BlocksCheck(skillId, targetId) &&
                                 c.ConsequenceType == FumbleType.TrustShattered);

        if (blockingConsequence != null)
        {
            _logger.LogDebug(
                "Skill check blocked by consequence {ConsequenceId} ({FumbleType}) " +
                "for character {CharacterId} on skill {SkillId}",
                blockingConsequence.ConsequenceId,
                blockingConsequence.ConsequenceType,
                characterId,
                skillId);
            return true;
        }

        return false;
    }

    /// <inheritdoc />
    public bool TryRecover(string consequenceId, IEnumerable<string> completedConditions)
    {
        var consequence = _repository.GetById(consequenceId);
        if (consequence == null || !consequence.IsActive)
        {
            return false;
        }

        if (consequence.CanRecover(completedConditions))
        {
            consequence.Deactivate("Recovery condition met", DateTime.UtcNow);
            _repository.Update(consequence);

            _logger.LogInformation(
                "Consequence {ConsequenceId} recovered by character {CharacterId} - condition met",
                consequenceId,
                consequence.CharacterId);

            return true;
        }

        return false;
    }

    /// <inheritdoc />
    public void DeactivateConsequence(string consequenceId, string reason)
    {
        var consequence = _repository.GetById(consequenceId);
        if (consequence == null || !consequence.IsActive)
        {
            return;
        }

        consequence.Deactivate(reason, DateTime.UtcNow);
        _repository.Update(consequence);

        _logger.LogInformation(
            "Consequence {ConsequenceId} deactivated manually: {Reason}",
            consequenceId,
            reason);
    }

    /// <inheritdoc />
    public IReadOnlyList<FumbleConsequence> ProcessExpirations(DateTime currentTime)
    {
        var allActive = _repository.GetAllActive();
        var expired = new List<FumbleConsequence>();

        foreach (var consequence in allActive)
        {
            if (consequence.IsExpired(currentTime))
            {
                consequence.Deactivate("Expired", currentTime);
                _repository.Update(consequence);
                expired.Add(consequence);

                _logger.LogDebug(
                    "Consequence {ConsequenceId} expired at {CurrentTime}",
                    consequence.ConsequenceId,
                    currentTime);
            }
        }

        return expired;
    }

    /// <inheritdoc />
    public int GetTotalDicePenalty(string characterId, string skillId, string? targetId)
    {
        var consequences = GetConsequencesAffectingCheck(characterId, skillId, targetId);
        return consequences.Sum(c => c.GetDicePenalty());
    }

    /// <inheritdoc />
    public int GetTotalDcModifier(string characterId, string skillId, string? targetId)
    {
        var consequences = GetConsequencesAffectingCheck(characterId, skillId, targetId);
        return consequences.Sum(c => c.GetDcModifier());
    }

    /// <summary>
    /// Determines the appropriate fumble type based on skill ID.
    /// </summary>
    private static FumbleType DetermineFumbleType(string skillId)
    {
        return skillId.ToLowerInvariant() switch
        {
            "persuasion" => FumbleType.TrustShattered,
            "deception" => FumbleType.LieExposed,
            "intimidation" => FumbleType.ChallengeAccepted,
            "lockpicking" => FumbleType.MechanismJammed,
            "hacking" or "terminal-hacking" => FumbleType.SystemLockout,
            "trap-disarmament" or "disarm-trap" => FumbleType.ForcedExecution,
            "climbing" or "climb" => FumbleType.TheSlip,
            "leaping" or "leap" or "jump" => FumbleType.TheLongFall,
            "stealth" or "sneak" => FumbleType.SystemWideAlert,
            _ => FumbleType.TrustShattered // Default fallback
        };
    }

    /// <summary>
    /// Generates a unique consequence ID.
    /// </summary>
    private static string GenerateConsequenceId()
    {
        return $"fc-{Guid.NewGuid():N}";
    }
}
