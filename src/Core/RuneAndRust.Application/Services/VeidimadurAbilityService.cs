using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Services;

/// <summary>
/// Service handling Veiðimaðr (Hunter) specialization ability execution.
/// Implements Tier 1 (Foundation) abilities including quarry marking,
/// perception bonuses, and track investigation.
/// </summary>
/// <remarks>
/// <para>The Veiðimaðr is a Coherent path Skirmisher with zero Corruption risk.
/// This service does NOT depend on a corruption service, following the same pattern
/// as <see cref="BoneSetterAbilityService"/>.</para>
/// <para>Tier 1 abilities: Mark Quarry, Keen Senses, Read the Signs (v0.20.7a).</para>
/// <para>Key design decisions:</para>
/// <list type="bullet">
/// <item>No Corruption evaluation step — all Veiðimaðr abilities follow the Coherent path</item>
/// <item>Quarry Marks are mutable — marks are added/removed in-place via
/// <see cref="IVeidimadurQuarryMarksService"/></item>
/// <item>Guard-clause chain: null → spec → ability unlocked → AP → execute</item>
/// <item>Read the Signs uses a skill check: 1d20 + ability bonus (+4) + Keen Senses (+1 if unlocked) vs DC</item>
/// <item>DC scaling determined by <see cref="CreatureTrackType"/>: Fresh=10, Recent=12, Worn=15, Ancient=18, Unclear=20</item>
/// <item>Mark Quarry has no skill check — automatic success on valid target</item>
/// <item>Keen Senses is a passive — no AP cost, no execution method, just a bonus query</item>
/// </list>
/// <para>Dice roll methods are marked <c>internal virtual</c> for unit test overriding.
/// Requires <c>InternalsVisibleTo</c> in the project file to be accessible from test assemblies.</para>
/// <para>Introduced in v0.20.7a as part of the Veiðimaðr specialization framework.
/// Tier 2 abilities will be added in v0.20.7b, Tier 3 and Capstone in v0.20.7c.</para>
/// </remarks>
public class VeidimadurAbilityService : IVeidimadurAbilityService
{
    // ===== Tier 1 Cost Constants (v0.20.7a) =====

    /// <summary>
    /// AP cost for the Mark Quarry targeting ability.
    /// </summary>
    private const int MarkQuarryApCost = 1;

    /// <summary>
    /// Maximum range in spaces for Mark Quarry (must see target within this distance).
    /// </summary>
    private const int MarkQuarryRange = 12;

    /// <summary>
    /// Hit bonus granted by a Quarry Mark against the marked target.
    /// </summary>
    private const int MarkQuarryHitBonus = 2;

    /// <summary>
    /// AP cost for the Read the Signs investigation ability.
    /// </summary>
    private const int ReadTheSignsApCost = 1;

    /// <summary>
    /// Ability bonus applied to Read the Signs skill checks (1d20 + this value).
    /// </summary>
    private const int ReadTheSignsAbilityBonus = 4;

    /// <summary>
    /// Perception bonus granted by the Keen Senses passive ability.
    /// Applied to all Perception checks when the ability is unlocked.
    /// </summary>
    private const int KeenSensesPerceptionBonus = 1;

    /// <summary>
    /// Investigation bonus granted by the Keen Senses passive ability.
    /// Applied to all Investigation checks (including Read the Signs) when unlocked.
    /// </summary>
    private const int KeenSensesInvestigationBonus = 1;

    // ===== Read the Signs DC Constants =====

    /// <summary>
    /// Difficulty Class for investigating Fresh tracks.
    /// </summary>
    private const int FreshTrackDc = 10;

    /// <summary>
    /// Difficulty Class for investigating Recent tracks.
    /// </summary>
    private const int RecentTrackDc = 12;

    /// <summary>
    /// Difficulty Class for investigating Worn tracks.
    /// </summary>
    private const int WornTrackDc = 15;

    /// <summary>
    /// Difficulty Class for investigating Ancient tracks.
    /// </summary>
    private const int AncientTrackDc = 18;

    /// <summary>
    /// Difficulty Class for investigating Unclear tracks (highest difficulty).
    /// </summary>
    private const int UnclearTrackDc = 20;

    // ===== Tier Unlock PP Requirements =====

    /// <summary>
    /// Minimum PP invested to unlock Tier 2 abilities.
    /// </summary>
    private const int Tier2PpRequirement = 8;

    /// <summary>
    /// Minimum PP invested to unlock Tier 3 abilities.
    /// </summary>
    private const int Tier3PpRequirement = 16;

    /// <summary>
    /// Minimum PP invested to unlock the Capstone ability.
    /// </summary>
    private const int CapstonePpRequirement = 24;

    /// <summary>
    /// The specialization ID string for Veiðimaðr (Hunter).
    /// </summary>
    private const string VeidimadurSpecId = "veidimadur";

    private readonly IVeidimadurQuarryMarksService _quarryMarksService;
    private readonly ILogger<VeidimadurAbilityService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="VeidimadurAbilityService"/> class.
    /// </summary>
    /// <param name="quarryMarksService">Service for Quarry Marks resource management.</param>
    /// <param name="logger">Logger for ability execution events.</param>
    /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
    public VeidimadurAbilityService(
        IVeidimadurQuarryMarksService quarryMarksService,
        ILogger<VeidimadurAbilityService> logger)
    {
        _quarryMarksService = quarryMarksService ?? throw new ArgumentNullException(nameof(quarryMarksService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // ===== Tier 1 Ability Methods (v0.20.7a) =====

    /// <inheritdoc />
    public MarkQuarryResult? ExecuteMarkQuarry(
        Player player,
        Guid targetId,
        string targetName,
        Guid? encounterId = null)
    {
        ArgumentNullException.ThrowIfNull(player);

        // Validate specialization
        if (!IsVeidimadur(player))
        {
            _logger.LogWarning(
                "Mark Quarry failed: {Player} ({PlayerId}) is not a Veiðimaðr",
                player.Name, player.Id);
            return null;
        }

        // Validate ability unlock
        if (!player.HasVeidimadurAbilityUnlocked(VeidimadurAbilityId.MarkQuarry))
        {
            _logger.LogWarning(
                "Mark Quarry failed: {Player} ({PlayerId}) has not unlocked the ability",
                player.Name, player.Id);
            return null;
        }

        // Validate AP cost
        if (player.CurrentAP < MarkQuarryApCost)
        {
            _logger.LogWarning(
                "Mark Quarry failed: {Player} ({PlayerId}) has insufficient AP " +
                "(need {Required}, have {Available})",
                player.Name, player.Id, MarkQuarryApCost, player.CurrentAP);
            return null;
        }

        // === No Corruption evaluation — Coherent path ===

        // Deduct AP
        player.CurrentAP -= MarkQuarryApCost;

        // Get previous mark count before adding
        var previousMarksCount = _quarryMarksService.GetMarkCount(player);

        // Create the Quarry Mark
        var mark = QuarryMark.Create(targetId, targetName, player.Id, encounterId);

        // Add mark via the Quarry Marks service (handles FIFO replacement)
        var replacedMark = _quarryMarksService.AddMark(player, mark);

        // Get current mark count after adding
        var currentMarksCount = _quarryMarksService.GetMarkCount(player);

        // Build result
        var result = new MarkQuarryResult
        {
            HunterId = player.Id,
            HunterName = player.Name,
            TargetId = targetId,
            TargetName = targetName,
            MarkCreated = mark,
            PreviousMarksCount = previousMarksCount,
            CurrentMarksCount = currentMarksCount,
            ReplacedMark = replacedMark,
            MarkedAt = DateTime.UtcNow
        };

        if (replacedMark != null)
        {
            _logger.LogInformation(
                "Mark Quarry executed (FIFO replacement): {Player} ({PlayerId}) marked " +
                "{Target} ({TargetId}) as quarry, replacing oldest mark on " +
                "{OldTarget} ({OldTargetId}). +{HitBonus} to hit. " +
                "Marks: {PreviousCount} -> {CurrentCount}/{MaxMarks}. " +
                "Encounter: {EncounterId}. AP remaining: {RemainingAP}",
                player.Name, player.Id,
                targetName, targetId,
                replacedMark.TargetName, replacedMark.TargetId,
                MarkQuarryHitBonus,
                previousMarksCount, currentMarksCount, QuarryMarksResource.DefaultMaxMarks,
                encounterId?.ToString() ?? "N/A",
                player.CurrentAP);
        }
        else
        {
            _logger.LogInformation(
                "Mark Quarry executed: {Player} ({PlayerId}) marked {Target} ({TargetId}) " +
                "as quarry. +{HitBonus} to hit. " +
                "Marks: {PreviousCount} -> {CurrentCount}/{MaxMarks}. " +
                "Encounter: {EncounterId}. AP remaining: {RemainingAP}",
                player.Name, player.Id,
                targetName, targetId,
                MarkQuarryHitBonus,
                previousMarksCount, currentMarksCount, QuarryMarksResource.DefaultMaxMarks,
                encounterId?.ToString() ?? "N/A",
                player.CurrentAP);
        }

        return result;
    }

    /// <inheritdoc />
    public ReadTheSignsResult? ExecuteReadTheSigns(
        Player player,
        string locationDescription,
        CreatureTrackType trackQuality,
        string? creatureType = null,
        int? creatureCount = null,
        string? timePassedEstimate = null,
        string? directionOfTravel = null,
        string? creatureCondition = null)
    {
        ArgumentNullException.ThrowIfNull(player);

        // Validate specialization
        if (!IsVeidimadur(player))
        {
            _logger.LogWarning(
                "Read the Signs failed: {Player} ({PlayerId}) is not a Veiðimaðr",
                player.Name, player.Id);
            return null;
        }

        // Validate ability unlock
        if (!player.HasVeidimadurAbilityUnlocked(VeidimadurAbilityId.ReadTheSigns))
        {
            _logger.LogWarning(
                "Read the Signs failed: {Player} ({PlayerId}) has not unlocked the ability",
                player.Name, player.Id);
            return null;
        }

        // Validate AP cost
        if (player.CurrentAP < ReadTheSignsApCost)
        {
            _logger.LogWarning(
                "Read the Signs failed: {Player} ({PlayerId}) has insufficient AP " +
                "(need {Required}, have {Available})",
                player.Name, player.Id, ReadTheSignsApCost, player.CurrentAP);
            return null;
        }

        // === No Corruption evaluation — Coherent path ===

        // Deduct AP
        player.CurrentAP -= ReadTheSignsApCost;

        // Calculate total bonus: ability bonus + Keen Senses (if unlocked)
        var keenSensesBonus = GetKeenSensesBonus(player);
        var totalBonus = ReadTheSignsAbilityBonus + keenSensesBonus;

        // Roll skill check: 1d20 + total bonus
        var diceRoll = Roll1D20();
        var totalRoll = diceRoll + totalBonus;

        // Determine DC from track quality
        var dc = GetTrackDc(trackQuality);

        // Evaluate success
        var success = totalRoll >= dc;

        // Build information revealed list
        var informationRevealed = new List<string>();

        if (success)
        {
            // On success: populate creature information from provided parameters
            if (creatureType != null)
                informationRevealed.Add($"Creature identified: {creatureType}");
            if (creatureCount.HasValue)
                informationRevealed.Add($"Estimated count: approximately {creatureCount.Value}");
            if (timePassedEstimate != null)
                informationRevealed.Add($"Time passed: {timePassedEstimate}");
            if (directionOfTravel != null)
                informationRevealed.Add($"Direction of travel: {directionOfTravel}");
            if (creatureCondition != null)
                informationRevealed.Add($"Creature condition: {creatureCondition}");
        }
        else
        {
            // On failure: vague impressions only
            informationRevealed.Add("The tracks are too indistinct to determine details.");
            informationRevealed.Add($"Something passed through this area ({trackQuality.ToString().ToLowerInvariant()} tracks).");
        }

        // Build result
        var result = new ReadTheSignsResult
        {
            InvestigatorId = player.Id,
            LocationDescription = locationDescription,
            CreatureType = success ? creatureType : null,
            CreatureCount = success ? creatureCount : null,
            TimePassedEstimate = success ? timePassedEstimate : null,
            DirectionOfTravel = success ? directionOfTravel : null,
            CreatureCondition = success ? creatureCondition : null,
            TrackQuality = trackQuality,
            BonusApplied = totalBonus,
            SkillCheckRoll = totalRoll,
            SkillCheckDc = dc,
            Success = success,
            InformationRevealed = informationRevealed.AsReadOnly()
        };

        _logger.LogInformation(
            "Read the Signs executed: {Player} ({PlayerId}) investigated {TrackQuality} " +
            "tracks at {Location}. Roll: {DiceRoll} + {Bonus} = {TotalRoll} vs DC {DC} " +
            "({Outcome}). Keen Senses bonus: +{KeenSensesBonus}. " +
            "Information revealed: {InfoCount} items. AP remaining: {RemainingAP}",
            player.Name, player.Id,
            trackQuality, locationDescription,
            diceRoll, totalBonus, totalRoll, dc,
            success ? "SUCCESS" : "FAILURE",
            keenSensesBonus,
            informationRevealed.Count,
            player.CurrentAP);

        return result;
    }

    /// <inheritdoc />
    public int GetKeenSensesBonus(Player player)
    {
        ArgumentNullException.ThrowIfNull(player);

        // Keen Senses is a passive ability — no AP cost, no execution
        // Returns the bonus value if unlocked, 0 otherwise
        if (!IsVeidimadur(player))
            return 0;

        if (!player.HasVeidimadurAbilityUnlocked(VeidimadurAbilityId.KeenSenses))
            return 0;

        return KeenSensesPerceptionBonus;
    }

    // ===== Utility Methods =====

    /// <inheritdoc />
    public Dictionary<VeidimadurAbilityId, bool> GetAbilityReadiness(Player player)
    {
        ArgumentNullException.ThrowIfNull(player);

        var readiness = new Dictionary<VeidimadurAbilityId, bool>();

        if (!IsVeidimadur(player))
            return readiness;

        // Check each unlocked ability's readiness
        foreach (var abilityId in player.UnlockedVeidimadurAbilities)
        {
            var isReady = abilityId switch
            {
                // Tier 1 active — requires AP
                VeidimadurAbilityId.MarkQuarry =>
                    player.CurrentAP >= MarkQuarryApCost,

                // Tier 1 passive — always ready when unlocked
                VeidimadurAbilityId.KeenSenses => true,

                // Tier 1 active — requires AP
                VeidimadurAbilityId.ReadTheSigns =>
                    player.CurrentAP >= ReadTheSignsApCost,

                // Future tier abilities will be added in v0.20.7b/c
                _ => false
            };

            readiness[abilityId] = isReady;
        }

        return readiness;
    }

    /// <inheritdoc />
    public bool CanUnlockTier2(Player player)
    {
        if (player == null)
            return false;

        if (!IsVeidimadur(player))
            return false;

        return GetPPInvested(player) >= Tier2PpRequirement;
    }

    /// <inheritdoc />
    public bool CanUnlockTier3(Player player)
    {
        if (player == null)
            return false;

        if (!IsVeidimadur(player))
            return false;

        return GetPPInvested(player) >= Tier3PpRequirement;
    }

    /// <inheritdoc />
    public bool CanUnlockCapstone(Player player)
    {
        if (player == null)
            return false;

        if (!IsVeidimadur(player))
            return false;

        return GetPPInvested(player) >= CapstonePpRequirement;
    }

    /// <inheritdoc />
    public int GetPPInvested(Player player)
    {
        ArgumentNullException.ThrowIfNull(player);
        return player.GetVeidimadurPPInvested();
    }

    // ===== Dice Roll Methods (internal virtual for testing) =====

    /// <summary>
    /// Rolls 1d20 for Read the Signs skill checks.
    /// </summary>
    /// <returns>A value between 1 and 20 inclusive.</returns>
    /// <remarks>
    /// Marked <c>internal virtual</c> to allow test subclasses to provide deterministic values.
    /// Used by <see cref="ExecuteReadTheSigns"/> for the investigation skill check.
    /// </remarks>
    internal virtual int Roll1D20()
    {
        return Random.Shared.Next(1, 21);
    }

    // ===== Private Helper Methods =====

    /// <summary>
    /// Checks if a player is a Veiðimaðr.
    /// </summary>
    /// <param name="player">The player to check.</param>
    /// <returns>True if the player's specialization is "veidimadur".</returns>
    private static bool IsVeidimadur(Player player)
    {
        return string.Equals(player.SpecializationId, VeidimadurSpecId, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Gets the Difficulty Class for a given creature track type.
    /// Higher DC values indicate harder-to-read tracks.
    /// </summary>
    /// <param name="trackType">The quality/freshness of the creature tracks.</param>
    /// <returns>
    /// The DC value: Fresh=10, Recent=12, Worn=15, Ancient=18, Unclear=20.
    /// Defaults to <see cref="UnclearTrackDc"/> for unknown track types.
    /// </returns>
    private static int GetTrackDc(CreatureTrackType trackType)
    {
        return trackType switch
        {
            CreatureTrackType.Fresh => FreshTrackDc,
            CreatureTrackType.Recent => RecentTrackDc,
            CreatureTrackType.Worn => WornTrackDc,
            CreatureTrackType.Ancient => AncientTrackDc,
            CreatureTrackType.Unclear => UnclearTrackDc,
            _ => UnclearTrackDc
        };
    }
}
