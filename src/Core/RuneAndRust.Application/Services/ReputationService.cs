using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Services;

/// <summary>
/// Service for managing player faction reputation.
/// </summary>
/// <remarks>
/// <para>Implements all reputation business logic per SPEC-REPUTATION-001:</para>
/// <list type="bullet">
///   <item><description>Applies reputation deltas with witness multiplier scaling</description></item>
///   <item><description>Clamps values to [-100, +100]</description></item>
///   <item><description>Detects and reports tier transitions</description></item>
///   <item><description>Validates faction IDs against configuration</description></item>
///   <item><description>Generates human-readable change and transition messages</description></item>
/// </list>
///
/// <para><b>Dependencies:</b></para>
/// <list type="bullet">
///   <item><description><see cref="IFactionDefinitionProvider"/> — provides faction metadata (names, allies, enemies)</description></item>
///   <item><description><see cref="ILogger{T}"/> — structured logging per Development Standards</description></item>
/// </list>
/// </remarks>
public class ReputationService : IReputationService
{
    private readonly IFactionDefinitionProvider _factionProvider;
    private readonly ILogger<ReputationService> _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="ReputationService"/>.
    /// </summary>
    /// <param name="factionProvider">Provider for faction definitions.</param>
    /// <param name="logger">Logger for structured reputation events.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="factionProvider"/> or <paramref name="logger"/> is null.
    /// </exception>
    public ReputationService(
        IFactionDefinitionProvider factionProvider,
        ILogger<ReputationService> logger)
    {
        _factionProvider = factionProvider ?? throw new ArgumentNullException(nameof(factionProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public IReadOnlyList<ReputationChangeResult> ApplyReputationChanges(
        Player player,
        IReadOnlyDictionary<string, int> reputationChanges,
        WitnessContext witnessContext)
    {
        ArgumentNullException.ThrowIfNull(player);

        // Early return for null or empty change dictionaries — common case when
        // quests have no reputation rewards defined.
        if (reputationChanges == null || reputationChanges.Count == 0)
        {
            _logger.LogDebug("No reputation changes to apply (null or empty dictionary).");
            return [];
        }

        var results = new List<ReputationChangeResult>(reputationChanges.Count);

        foreach (var (factionId, rawDelta) in reputationChanges)
        {
            // Validate faction ID exists in configuration
            if (!_factionProvider.FactionExists(factionId))
            {
                _logger.LogWarning(
                    "Unknown faction '{FactionId}' in reputation changes, skipping.",
                    factionId);
                continue;
            }

            // Calculate actual delta after witness multiplier
            // (int) truncation is intentional — partial reputation points are not tracked
            var actualDelta = (int)(rawDelta * witnessContext.Multiplier);

            // Skip zero-delta changes (e.g., unwitnessed actions) but still include in results
            // so the caller knows the action was processed
            var currentReputation = player.GetFactionReputation(factionId);
            var oldTier = currentReputation.Tier;
            var oldValue = currentReputation.Value;

            // Apply delta and get new reputation (clamping happens in FactionReputation.WithDelta)
            var newReputation = currentReputation.WithDelta(actualDelta);
            player.SetFactionReputation(newReputation);

            // Calculate the actual change after clamping (may differ from actualDelta
            // if the value was at a boundary)
            var effectiveDelta = newReputation.Value - oldValue;

            // Resolve faction display name for messages
            var factionName = GetFactionName(factionId);

            // Build human-readable message
            var message = effectiveDelta >= 0
                ? $"+{effectiveDelta} {factionName} Reputation"
                : $"{effectiveDelta} {factionName} Reputation";

            // Build tier transition message if applicable
            string? tierTransitionMessage = null;
            if (oldTier != newReputation.Tier)
            {
                tierTransitionMessage = $"Your standing with {factionName} is now {newReputation.Tier}!";

                _logger.LogInformation(
                    "Standing with {FactionName} changed: {OldTier} → {NewTier}",
                    factionName, oldTier, newReputation.Tier);
            }

            _logger.LogInformation(
                "Reputation with {FactionName}: {Delta:+#;-#;0} (now {NewTotal})",
                factionName, effectiveDelta, newReputation.Value);

            _logger.LogDebug(
                "Applying {RawDelta} to {FactionId}, witness={WitnessType}, actual={ActualDelta}",
                rawDelta, factionId, witnessContext.Type, effectiveDelta);

            results.Add(new ReputationChangeResult
            {
                FactionId = factionId,
                FactionName = factionName,
                RawDelta = rawDelta,
                ActualDelta = effectiveDelta,
                NewValue = newReputation.Value,
                OldTier = oldTier,
                NewTier = newReputation.Tier,
                Message = message,
                TierTransitionMessage = tierTransitionMessage
            });
        }

        return results;
    }

    /// <inheritdoc />
    public ReputationTier GetTier(Player player, string factionId)
    {
        ArgumentNullException.ThrowIfNull(player);

        if (string.IsNullOrWhiteSpace(factionId))
            return ReputationTier.Neutral;

        return player.GetFactionTier(factionId);
    }

    /// <inheritdoc />
    public bool MeetsReputationRequirement(Player player, string factionId, int minimumValue)
    {
        ArgumentNullException.ThrowIfNull(player);

        if (string.IsNullOrWhiteSpace(factionId))
            return false;

        var reputation = player.GetFactionReputation(factionId);
        return reputation.Value >= minimumValue;
    }

    /// <inheritdoc />
    public bool MeetsTierRequirement(Player player, string factionId, ReputationTier minimumTier)
    {
        ArgumentNullException.ThrowIfNull(player);

        if (string.IsNullOrWhiteSpace(factionId))
            return false;

        var currentTier = player.GetFactionTier(factionId);
        return currentTier >= minimumTier;
    }

    /// <inheritdoc />
    public string GetFactionName(string factionId)
    {
        if (string.IsNullOrWhiteSpace(factionId))
            return string.Empty;

        var faction = _factionProvider.GetFaction(factionId);
        return faction?.Name ?? factionId;
    }

    /// <inheritdoc />
    public IReadOnlyList<string> GetAllFactionIds()
    {
        return _factionProvider.GetAllFactions()
            .Select(f => f.FactionId)
            .ToList();
    }
}
