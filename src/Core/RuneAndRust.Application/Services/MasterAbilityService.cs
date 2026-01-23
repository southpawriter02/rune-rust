namespace RuneAndRust.Application.Services;

using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Evaluates and applies master abilities during skill checks.
/// </summary>
/// <remarks>
/// <para>
/// This service determines how master abilities affect skill checks. It is called
/// by the skill check service before rolling dice to check for auto-succeed
/// conditions and apply dice bonuses.
/// </para>
/// <para>
/// Re-roll usage is tracked in memory per player and reset when the appropriate
/// period elapses (conversation end, scene change, etc.).
/// </para>
/// </remarks>
public sealed class MasterAbilityService : IMasterAbilityService
{
    private readonly IMasterAbilityProvider _provider;
    private readonly ILogger<MasterAbilityService> _logger;

    // Track re-roll usage: PlayerId -> AbilityId -> LastUsedAt
    private readonly Dictionary<string, Dictionary<string, DateTime>> _rerollUsage = new();

    /// <summary>
    /// Creates a new master ability service.
    /// </summary>
    /// <param name="provider">Provider for ability definitions.</param>
    /// <param name="logger">Logger for diagnostic output.</param>
    public MasterAbilityService(
        IMasterAbilityProvider provider,
        ILogger<MasterAbilityService> logger)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _logger.LogDebug("MasterAbilityService initialized");
    }

    /// <inheritdoc />
    public MasterAbilityEvaluationResult EvaluateForCheck(
        Player player,
        string skillId,
        string? subType,
        int difficultyClass)
    {
        ArgumentNullException.ThrowIfNull(player);
        ArgumentException.ThrowIfNullOrWhiteSpace(skillId);

        _logger.LogDebug(
            "Evaluating master abilities for Player {PlayerId}, Skill {SkillId}/{SubType}, DC {DC}",
            player.Id, skillId, subType ?? "general", difficultyClass);

        // Get applicable abilities (checks Rank 5 requirement)
        var abilities = GetApplicableAbilities(player, skillId, subType);

        if (abilities.Count == 0)
        {
            _logger.LogDebug(
                "No master abilities apply for {SkillId}/{SubType}",
                skillId, subType ?? "general");
            return MasterAbilityEvaluationResult.None;
        }

        _logger.LogDebug(
            "Found {Count} applicable master abilities for {SkillId}/{SubType}",
            abilities.Count, skillId, subType ?? "general");

        // Check for auto-succeed (highest DC threshold wins)
        var autoSucceedAbility = abilities
            .Where(a => a.WouldAutoSucceed(difficultyClass))
            .OrderByDescending(a => a.Effect.AutoSucceedDc)
            .FirstOrDefault();

        if (autoSucceedAbility != null)
        {
            _logger.LogInformation(
                "Master ability {AbilityId} ({AbilityName}) auto-succeeds DC {DC} check for {SkillId}",
                autoSucceedAbility.AbilityId, autoSucceedAbility.Name, difficultyClass, skillId);

            return MasterAbilityEvaluationResult.ForAutoSucceed(autoSucceedAbility);
        }

        // Gather dice bonuses
        var diceBonusAbilities = abilities
            .Where(a => a.AbilityType == MasterAbilityType.DiceBonus)
            .ToList();

        var totalDiceBonus = diceBonusAbilities.Sum(a => a.GetDiceBonus());

        // Gather special effects
        var specialEffects = abilities
            .Where(a => a.AbilityType == MasterAbilityType.SpecialAction)
            .Select(a => a.Effect.SpecialEffect!)
            .Where(e => !string.IsNullOrWhiteSpace(e))
            .ToList();

        // Check for available re-roll
        var rerollAbility = abilities
            .FirstOrDefault(a =>
                a.AbilityType == MasterAbilityType.RerollFailure &&
                CanUseReroll(player, a.AbilityId));

        if (totalDiceBonus > 0)
        {
            _logger.LogDebug(
                "Master abilities provide +{Bonus}d10 for {SkillId}/{SubType} from {AbilityNames}",
                totalDiceBonus, skillId, subType ?? "general",
                string.Join(", ", diceBonusAbilities.Select(a => a.Name)));
        }

        if (specialEffects.Count > 0)
        {
            _logger.LogDebug(
                "Active special effects for {SkillId}: {Effects}",
                skillId, string.Join(", ", specialEffects));
        }

        if (rerollAbility != null)
        {
            _logger.LogDebug(
                "Re-roll ability {AbilityId} available for player {PlayerId}",
                rerollAbility.AbilityId, player.Id);
        }

        return new MasterAbilityEvaluationResult(
            ShouldAutoSucceed: false,
            AutoSucceedAbility: null,
            TotalDiceBonus: totalDiceBonus,
            DiceBonusAbilities: diceBonusAbilities.AsReadOnly(),
            ActiveSpecialEffects: specialEffects.AsReadOnly(),
            CanReroll: rerollAbility != null,
            RerollAbilityId: rerollAbility?.AbilityId);
    }

    /// <inheritdoc />
    public IReadOnlyList<MasterAbility> GetApplicableAbilities(
        Player player,
        string skillId,
        string? subType)
    {
        ArgumentNullException.ThrowIfNull(player);
        ArgumentException.ThrowIfNullOrWhiteSpace(skillId);

        // Check if player has Rank 5 proficiency
        var proficiency = player.GetSkillProficiency(skillId);
        if (proficiency != SkillProficiency.Master)
        {
            _logger.LogDebug(
                "Player {PlayerId} does not have Master rank in {SkillId} (has {Proficiency})",
                player.Id, skillId, proficiency);
            return Array.Empty<MasterAbility>();
        }

        // Get abilities for skill and subtype
        var abilities = _provider.GetAbilitiesForSkillAndSubType(skillId, subType);

        _logger.LogDebug(
            "Player {PlayerId} has Master rank in {SkillId}, found {Count} abilities for subtype {SubType}",
            player.Id, skillId, abilities.Count, subType ?? "general");

        return abilities;
    }

    /// <inheritdoc />
    public bool CanUseReroll(Player player, string abilityId)
    {
        ArgumentNullException.ThrowIfNull(player);
        ArgumentException.ThrowIfNullOrWhiteSpace(abilityId);

        if (!_rerollUsage.TryGetValue(player.Id.ToString(), out var playerUsage))
        {
            _logger.LogDebug(
                "No re-roll usage tracked for player {PlayerId}, ability {AbilityId} available",
                player.Id, abilityId);
            return true;
        }

        if (!playerUsage.TryGetValue(abilityId, out var lastUsed))
        {
            _logger.LogDebug(
                "Re-roll ability {AbilityId} not used by player {PlayerId}, available",
                abilityId, player.Id);
            return true;
        }

        // Get the ability to check its period
        var ability = _provider.GetAbilityById(abilityId);
        if (ability == null || ability.Effect.RerollPeriod == null)
        {
            _logger.LogWarning(
                "Re-roll ability {AbilityId} not found or missing period configuration",
                abilityId);
            return false;
        }

        // For now, we track usage but actual period reset is handled by
        // ResetRerollsForPeriod when game events occur
        _logger.LogDebug(
            "Re-roll ability {AbilityId} already used by player {PlayerId} at {LastUsed}",
            abilityId, player.Id, lastUsed);
        return false; // Used this period
    }

    /// <inheritdoc />
    public void UseReroll(Player player, string abilityId)
    {
        ArgumentNullException.ThrowIfNull(player);
        ArgumentException.ThrowIfNullOrWhiteSpace(abilityId);

        if (!_rerollUsage.TryGetValue(player.Id.ToString(), out var playerUsage))
        {
            playerUsage = new Dictionary<string, DateTime>(StringComparer.OrdinalIgnoreCase);
            _rerollUsage[player.Id.ToString()] = playerUsage;
        }

        playerUsage[abilityId] = DateTime.UtcNow;

        _logger.LogInformation(
            "Player {PlayerId} used re-roll ability {AbilityId}",
            player.Id, abilityId);
    }

    /// <inheritdoc />
    public void ResetRerollsForPeriod(Player player, RerollPeriod period)
    {
        ArgumentNullException.ThrowIfNull(player);

        if (!_rerollUsage.TryGetValue(player.Id.ToString(), out var playerUsage))
        {
            _logger.LogDebug(
                "No re-roll usage to reset for player {PlayerId}, period {Period}",
                player.Id, period);
            return;
        }

        var abilitiesToReset = _provider.GetAllAbilities()
            .Where(a =>
                a.AbilityType == MasterAbilityType.RerollFailure &&
                a.Effect.RerollPeriod == period)
            .Select(a => a.AbilityId)
            .ToList();

        var resetCount = 0;
        foreach (var abilityId in abilitiesToReset)
        {
            if (playerUsage.Remove(abilityId))
            {
                resetCount++;
                _logger.LogDebug(
                    "Reset re-roll ability {AbilityId} for player {PlayerId} (period: {Period})",
                    abilityId, player.Id, period);
            }
        }

        _logger.LogInformation(
            "Reset {Count} re-roll abilities for player {PlayerId} (period: {Period})",
            resetCount, player.Id, period);
    }
}
