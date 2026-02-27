using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Services;

/// <summary>
/// Service handling Veiðimaðr (Hunter) specialization ability execution.
/// Implements all four tiers: Tier 1 (Foundation), Tier 2 (Discipline), Tier 3 (Mastery),
/// and Capstone (Ultimate) abilities including quarry marking, perception bonuses,
/// track investigation, cover bypass, traps, stance management, concealment denial,
/// crippling shots, and the devastating Perfect Hunt.
/// </summary>
/// <remarks>
/// <para>The Veiðimaðr is a Coherent path Skirmisher with zero Corruption risk.
/// This service does NOT depend on a corruption service, following the same pattern
/// as <see cref="BoneSetterAbilityService"/>.</para>
/// <para>Tier 1 abilities: Mark Quarry, Keen Senses, Read the Signs (v0.20.7a).</para>
/// <para>Tier 2 abilities: Hunter's Eye, Trap Mastery, Predator's Patience (v0.20.7b).</para>
/// <para>Tier 3 abilities: Apex Predator, Crippling Shot (v0.20.7c).</para>
/// <para>Capstone ability: The Perfect Hunt (v0.20.7c).</para>
/// <para>Key design decisions:</para>
/// <list type="bullet">
/// <item>No Corruption evaluation step — all Veiðimaðr abilities follow the Coherent path</item>
/// <item>Quarry Marks are mutable — marks are added/removed in-place via
/// <see cref="IVeidimadurQuarryMarksService"/></item>
/// <item>Guard-clause chain: null → spec → ability unlocked → (cooldown) → AP → (resource) → execute</item>
/// <item>Read the Signs uses a skill check: 1d20 + ability bonus (+4) + Keen Senses (+1 if unlocked) vs DC</item>
/// <item>DC scaling determined by <see cref="CreatureTrackType"/>: Fresh=10, Recent=12, Worn=15, Ancient=18, Unclear=20</item>
/// <item>Mark Quarry has no skill check — automatic success on valid target</item>
/// <item>Keen Senses and Apex Predator are passives — no AP cost, evaluated per-attack</item>
/// <item>Crippling Shot consumes 1 Quarry Mark — guaranteed effect, no attack roll</item>
/// <item>The Perfect Hunt: cooldown checked BEFORE AP to avoid deducting AP for a locked ability</item>
/// </list>
/// <para>Dice roll methods are marked <c>internal virtual</c> for unit test overriding.
/// Requires <c>InternalsVisibleTo</c> in the project file to be accessible from test assemblies.</para>
/// <para>Introduced in v0.20.7a. Tier 2 added in v0.20.7b. Tier 3 and Capstone added in v0.20.7c.</para>
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

    // ===== Tier 2 Cost Constants (v0.20.7b) =====

    /// <summary>
    /// The AC penalty normally applied by partial cover, which Hunter's Eye negates.
    /// </summary>
    private const int HuntersEyePartialCoverPenalty = 2;

    /// <summary>
    /// AP cost for placing a hunting trap via Trap Mastery.
    /// </summary>
    private const int TrapMasteryPlaceApCost = 2;

    /// <summary>
    /// AP cost for detecting enemy traps via Trap Mastery.
    /// </summary>
    private const int TrapMasteryDetectApCost = 2;

    /// <summary>
    /// Difficulty Class for the Trap Mastery detection perception check.
    /// </summary>
    private const int TrapMasteryDetectionDc = 13;

    /// <summary>
    /// Bonus applied to the Trap Mastery detection perception check (before Keen Senses).
    /// </summary>
    private const int TrapMasteryDetectionBonus = 3;

    /// <summary>
    /// Maximum number of armed hunting traps a Veiðimaðr may have active simultaneously.
    /// </summary>
    private const int TrapMasteryMaxActiveTraps = 2;

    /// <summary>
    /// AP cost for activating the Predator's Patience stance.
    /// </summary>
    private const int PredatorsPatienceApCost = 1;

    /// <summary>
    /// Hit bonus granted by the Predator's Patience stance when active and unmoved.
    /// </summary>
    private const int PredatorsPatienceHitBonus = 3;

    // ===== Tier 3 Cost Constants (v0.20.7c) =====

    /// <summary>
    /// AP cost for the Crippling Shot movement-debuff ability.
    /// </summary>
    private const int CripplingShotApCost = 1;

    /// <summary>
    /// Number of turns the Crippling Shot movement reduction lasts.
    /// </summary>
    private const int CripplingShotDurationTurns = 2;

    /// <summary>
    /// Divisor applied to the target's movement speed by Crippling Shot (halves movement).
    /// </summary>
    private const int CripplingShotMovementDivisor = 2;

    // ===== Capstone Cost Constants (v0.20.7c) =====

    /// <summary>
    /// AP cost for The Perfect Hunt capstone ability.
    /// </summary>
    private const int ThePerfectHuntApCost = 3;

    /// <summary>
    /// Critical damage multiplier for The Perfect Hunt (doubles base damage).
    /// </summary>
    private const int ThePerfectHuntCriticalMultiplier = 2;

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

    // ===== Tier 2 Ability Methods (v0.20.7b) =====

    /// <inheritdoc />
    public HuntersEyeResult? ExecuteHuntersEye(
        Player player,
        Guid targetId,
        string targetName,
        CoverType targetCover,
        int distance)
    {
        ArgumentNullException.ThrowIfNull(player);

        // Validate specialization
        if (!IsVeidimadur(player))
        {
            _logger.LogWarning(
                "Hunter's Eye failed: {Player} ({PlayerId}) is not a Veiðimaðr",
                player.Name, player.Id);
            return null;
        }

        // Validate ability unlock
        if (!player.HasVeidimadurAbilityUnlocked(VeidimadurAbilityId.HuntersEye))
        {
            _logger.LogWarning(
                "Hunter's Eye failed: {Player} ({PlayerId}) has not unlocked the ability",
                player.Name, player.Id);
            return null;
        }

        // === No AP cost — Hunter's Eye is a passive ability ===
        // === No Corruption evaluation — Coherent path ===

        // Evaluate whether the target's cover should be ignored
        var coverIgnored = HuntersEyeResult.ShouldIgnoreCover(targetCover);

        // Calculate the effective bonus from ignoring cover
        var bonusFromCoverIgnored = coverIgnored ? HuntersEyePartialCoverPenalty : 0;

        // Build result
        var result = new HuntersEyeResult
        {
            HunterId = player.Id,
            TargetId = targetId,
            TargetName = targetName,
            OriginalCoverType = targetCover,
            CoverIgnored = coverIgnored,
            BonusFromCoverIgnored = bonusFromCoverIgnored,
            Distance = distance
        };

        if (coverIgnored)
        {
            _logger.LogInformation(
                "Hunter's Eye evaluated: {Player} ({PlayerId}) ignores {CoverType} cover on " +
                "{Target} ({TargetId}) at {Distance} spaces. Effective bonus: +{Bonus} " +
                "(negated cover AC penalty)",
                player.Name, player.Id,
                targetCover, targetName, targetId,
                distance, bonusFromCoverIgnored);
        }
        else if (targetCover == CoverType.Full)
        {
            _logger.LogWarning(
                "Hunter's Eye evaluated: {Player} ({PlayerId}) cannot ignore {CoverType} cover on " +
                "{Target} ({TargetId}) at {Distance} spaces — full cover cannot be bypassed",
                player.Name, player.Id,
                targetCover, targetName, targetId, distance);
        }
        else
        {
            _logger.LogInformation(
                "Hunter's Eye evaluated: {Player} ({PlayerId}) attacking {Target} ({TargetId}) " +
                "at {Distance} spaces — no cover to ignore ({CoverType})",
                player.Name, player.Id,
                targetName, targetId, distance, targetCover);
        }

        return result;
    }

    /// <inheritdoc />
    public TrapMasteryResult? ExecutePlaceTrap(
        Player player,
        int x,
        int y,
        TrapType trapType)
    {
        ArgumentNullException.ThrowIfNull(player);

        // Validate specialization
        if (!IsVeidimadur(player))
        {
            _logger.LogWarning(
                "Trap Mastery (Place) failed: {Player} ({PlayerId}) is not a Veiðimaðr",
                player.Name, player.Id);
            return null;
        }

        // Validate ability unlock
        if (!player.HasVeidimadurAbilityUnlocked(VeidimadurAbilityId.TrapMastery))
        {
            _logger.LogWarning(
                "Trap Mastery (Place) failed: {Player} ({PlayerId}) has not unlocked the ability",
                player.Name, player.Id);
            return null;
        }

        // Validate AP cost
        if (player.CurrentAP < TrapMasteryPlaceApCost)
        {
            _logger.LogWarning(
                "Trap Mastery (Place) failed: {Player} ({PlayerId}) has insufficient AP " +
                "(need {Required}, have {Available})",
                player.Name, player.Id, TrapMasteryPlaceApCost, player.CurrentAP);
            return null;
        }

        // === No Corruption evaluation — Coherent path ===

        // Check maximum active traps constraint
        var armedTraps = player.GetArmedHuntingTraps();
        if (armedTraps.Count >= TrapMasteryMaxActiveTraps)
        {
            _logger.LogWarning(
                "Trap Mastery (Place) failed: {Player} ({PlayerId}) already has {Current}/{Max} " +
                "armed traps. Must disarm or wait for a trap to trigger before placing another",
                player.Name, player.Id, armedTraps.Count, TrapMasteryMaxActiveTraps);
            return TrapMasteryResult.CreatePlacementFailure(x, y,
                $"Maximum active traps reached ({TrapMasteryMaxActiveTraps}). " +
                "Disarm or wait for a trap to trigger before placing another.");
        }

        // Deduct AP
        player.CurrentAP -= TrapMasteryPlaceApCost;

        // Create the trap instance
        var trap = TrapInstance.Create(player.Id, x, y, trapType);

        // Add trap to player's hunting traps
        player.AddHuntingTrap(trap);

        // Build success result
        var result = TrapMasteryResult.CreatePlacementSuccess(trap, x, y);

        _logger.LogInformation(
            "Trap Mastery (Place) executed: {Player} ({PlayerId}) placed a {TrapType} trap " +
            "at ({X}, {Y}). Trap ID: {TrapId}. Damage: {DamageRoll} + {ImmobilizeTurns}-turn " +
            "immobilize. Detection DC: {DetectionDc}. Armed traps: {ArmedCount}/{MaxTraps}. " +
            "AP remaining: {RemainingAP}",
            player.Name, player.Id,
            trapType, x, y,
            trap.TrapId, trap.DamageRoll, trap.ImmobilizeTurns,
            trap.DetectionDc,
            player.GetArmedHuntingTraps().Count, TrapMasteryMaxActiveTraps,
            player.CurrentAP);

        return result;
    }

    /// <inheritdoc />
    public TrapMasteryResult? ExecuteDetectTraps(
        Player player,
        int centerX,
        int centerY)
    {
        ArgumentNullException.ThrowIfNull(player);

        // Validate specialization
        if (!IsVeidimadur(player))
        {
            _logger.LogWarning(
                "Trap Mastery (Detect) failed: {Player} ({PlayerId}) is not a Veiðimaðr",
                player.Name, player.Id);
            return null;
        }

        // Validate ability unlock
        if (!player.HasVeidimadurAbilityUnlocked(VeidimadurAbilityId.TrapMastery))
        {
            _logger.LogWarning(
                "Trap Mastery (Detect) failed: {Player} ({PlayerId}) has not unlocked the ability",
                player.Name, player.Id);
            return null;
        }

        // Validate AP cost
        if (player.CurrentAP < TrapMasteryDetectApCost)
        {
            _logger.LogWarning(
                "Trap Mastery (Detect) failed: {Player} ({PlayerId}) has insufficient AP " +
                "(need {Required}, have {Available})",
                player.Name, player.Id, TrapMasteryDetectApCost, player.CurrentAP);
            return null;
        }

        // === No Corruption evaluation — Coherent path ===

        // Deduct AP
        player.CurrentAP -= TrapMasteryDetectApCost;

        // Calculate total perception bonus: Trap Mastery bonus + Keen Senses (if unlocked)
        var keenSensesBonus = GetKeenSensesBonus(player);
        var totalBonus = TrapMasteryDetectionBonus + keenSensesBonus;

        // Roll perception check: 1d20 + total bonus
        var diceRoll = Roll1D20();
        var totalRoll = diceRoll + totalBonus;

        // Compare vs DC
        var success = totalRoll >= TrapMasteryDetectionDc;

        TrapMasteryResult result;

        if (success)
        {
            // Detection succeeds — in this implementation, the number of traps
            // detected is determined by the caller/game state. We report the
            // successful check and let the encounter system populate detected traps.
            // For the base success, we report 0 traps as a "passed check" indicator.
            var descriptions = new List<string>
            {
                "Your trained senses sweep the area, searching for hidden dangers."
            };

            result = TrapMasteryResult.CreateDetectionSuccess(
                count: 0,
                descriptions: descriptions,
                roll: totalRoll,
                dc: TrapMasteryDetectionDc,
                bonus: totalBonus);

            _logger.LogInformation(
                "Trap Mastery (Detect) executed: {Player} ({PlayerId}) scanned area around " +
                "({CenterX}, {CenterY}). Roll: {DiceRoll} + {Bonus} = {TotalRoll} vs DC {DC} " +
                "(SUCCESS). Keen Senses bonus: +{KeenSensesBonus}. AP remaining: {RemainingAP}",
                player.Name, player.Id,
                centerX, centerY,
                diceRoll, totalBonus, totalRoll, TrapMasteryDetectionDc,
                keenSensesBonus,
                player.CurrentAP);
        }
        else
        {
            result = TrapMasteryResult.CreateDetectionFailure(
                roll: totalRoll,
                dc: TrapMasteryDetectionDc,
                bonus: totalBonus);

            _logger.LogInformation(
                "Trap Mastery (Detect) executed: {Player} ({PlayerId}) scanned area around " +
                "({CenterX}, {CenterY}). Roll: {DiceRoll} + {Bonus} = {TotalRoll} vs DC {DC} " +
                "(FAILURE). Keen Senses bonus: +{KeenSensesBonus}. AP remaining: {RemainingAP}",
                player.Name, player.Id,
                centerX, centerY,
                diceRoll, totalBonus, totalRoll, TrapMasteryDetectionDc,
                keenSensesBonus,
                player.CurrentAP);
        }

        return result;
    }

    /// <inheritdoc />
    public PredatorsPatienceState? ActivatePredatorsPatience(Player player)
    {
        ArgumentNullException.ThrowIfNull(player);

        // Validate specialization
        if (!IsVeidimadur(player))
        {
            _logger.LogWarning(
                "Predator's Patience (Activate) failed: {Player} ({PlayerId}) is not a Veiðimaðr",
                player.Name, player.Id);
            return null;
        }

        // Validate ability unlock
        if (!player.HasVeidimadurAbilityUnlocked(VeidimadurAbilityId.PredatorsPatience))
        {
            _logger.LogWarning(
                "Predator's Patience (Activate) failed: {Player} ({PlayerId}) has not unlocked the ability",
                player.Name, player.Id);
            return null;
        }

        // Validate AP cost
        if (player.CurrentAP < PredatorsPatienceApCost)
        {
            _logger.LogWarning(
                "Predator's Patience (Activate) failed: {Player} ({PlayerId}) has insufficient AP " +
                "(need {Required}, have {Available})",
                player.Name, player.Id, PredatorsPatienceApCost, player.CurrentAP);
            return null;
        }

        // === No Corruption evaluation — Coherent path ===

        // Deduct AP
        player.CurrentAP -= PredatorsPatienceApCost;

        // Create or retrieve stance state, then activate
        var state = player.PredatorsPatience ?? PredatorsPatienceState.Create(player.Id);
        state.Activate();

        // Store the state on the player
        player.SetPredatorsPatience(state);

        _logger.LogInformation(
            "Predator's Patience activated: {Player} ({PlayerId}) enters the patient hunter " +
            "stance. Hit bonus: +{HitBonus} while stationary. Movement will break the stance. " +
            "AP remaining: {RemainingAP}",
            player.Name, player.Id,
            PredatorsPatienceHitBonus,
            player.CurrentAP);

        return state;
    }

    /// <inheritdoc />
    public bool DeactivatePredatorsPatience(Player player)
    {
        ArgumentNullException.ThrowIfNull(player);

        // Validate specialization
        if (!IsVeidimadur(player))
        {
            _logger.LogWarning(
                "Predator's Patience (Deactivate) failed: {Player} ({PlayerId}) is not a Veiðimaðr",
                player.Name, player.Id);
            return false;
        }

        // Validate ability unlock
        if (!player.HasVeidimadurAbilityUnlocked(VeidimadurAbilityId.PredatorsPatience))
        {
            _logger.LogWarning(
                "Predator's Patience (Deactivate) failed: {Player} ({PlayerId}) has not unlocked the ability",
                player.Name, player.Id);
            return false;
        }

        // === No AP cost to deactivate ===

        // Check if stance is actually active
        var state = player.PredatorsPatience;
        if (state == null || !state.IsActive)
        {
            _logger.LogWarning(
                "Predator's Patience (Deactivate) failed: {Player} ({PlayerId}) does not " +
                "have an active Predator's Patience stance to deactivate",
                player.Name, player.Id);
            return false;
        }

        // Deactivate the stance
        state.Deactivate();
        player.SetPredatorsPatience(state);

        _logger.LogInformation(
            "Predator's Patience deactivated: {Player} ({PlayerId}) exits the patient hunter " +
            "stance. Turns held: {TurnsInStance}",
            player.Name, player.Id,
            state.TurnsInStance);

        return true;
    }

    /// <inheritdoc />
    public int GetPredatorsPatienceBonus(Player player)
    {
        ArgumentNullException.ThrowIfNull(player);

        // Predator's Patience is a stance — bonus only applies while active and unmoved
        if (!IsVeidimadur(player))
            return 0;

        if (!player.HasVeidimadurAbilityUnlocked(VeidimadurAbilityId.PredatorsPatience))
            return 0;

        var state = player.PredatorsPatience;
        if (state == null)
            return 0;

        return state.GetCurrentBonus();
    }

    // ===== Tier 3 Ability Methods (v0.20.7c) =====

    /// <inheritdoc />
    public ApexPredatorResult? EvaluateApexPredator(
        Player player,
        Guid targetId,
        string targetName,
        ConcealmentType concealmentType)
    {
        ArgumentNullException.ThrowIfNull(player);

        // Validate specialization
        if (!IsVeidimadur(player))
        {
            _logger.LogWarning(
                "Apex Predator failed: {Player} ({PlayerId}) is not a Veiðimaðr",
                player.Name, player.Id);
            return null;
        }

        // Validate ability unlock
        if (!player.HasVeidimadurAbilityUnlocked(VeidimadurAbilityId.ApexPredator))
        {
            _logger.LogWarning(
                "Apex Predator failed: {Player} ({PlayerId}) has not unlocked the ability",
                player.Name, player.Id);
            return null;
        }

        // === No AP cost — Apex Predator is a passive ability ===
        // === No Corruption evaluation — Coherent path ===

        // Check whether the target is marked as quarry
        var isMarked = _quarryMarksService.HasActiveMark(player, targetId);

        // Evaluate concealment denial via the domain value object's static logic
        var concealmentDenied = ApexPredatorResult.ShouldDenyConcealment(concealmentType, isMarked);

        // Determine whether the target was originally concealed (any non-None type)
        var wasConcealed = concealmentType != ConcealmentType.None;

        // Build result — always non-null when prerequisites are met
        var result = new ApexPredatorResult
        {
            HunterId = player.Id,
            TargetId = targetId,
            TargetName = targetName,
            WasConcealed = wasConcealed,
            ConcealmentType = concealmentType,
            ConcealmentDenied = concealmentDenied,
            TargetWasMarked = isMarked
        };

        // Log based on outcome
        if (concealmentDenied)
        {
            _logger.LogInformation(
                "Apex Predator evaluated: {Player} ({PlayerId}) DENIES {ConcealmentType} " +
                "concealment on marked quarry {Target} ({TargetId}). " +
                "The quarry cannot hide from the apex predator",
                player.Name, player.Id,
                concealmentType, targetName, targetId);
        }
        else if (!isMarked)
        {
            _logger.LogWarning(
                "Apex Predator evaluated: {Player} ({PlayerId}) cannot deny concealment on " +
                "{Target} ({TargetId}) — target is NOT a marked quarry. " +
                "Concealment type: {ConcealmentType}",
                player.Name, player.Id,
                targetName, targetId, concealmentType);
        }
        else
        {
            // Marked but no concealment to deny (ConcealmentType.None)
            _logger.LogInformation(
                "Apex Predator evaluated: {Player} ({PlayerId}) checked {Target} ({TargetId}) " +
                "— target is marked quarry but has no concealment ({ConcealmentType})",
                player.Name, player.Id,
                targetName, targetId, concealmentType);
        }

        return result;
    }

    /// <inheritdoc />
    public CripplingShotResult? ExecuteCripplingShot(
        Player player,
        Guid targetId,
        string targetName,
        int targetMovementSpeed)
    {
        ArgumentNullException.ThrowIfNull(player);

        // Validate specialization
        if (!IsVeidimadur(player))
        {
            _logger.LogWarning(
                "Crippling Shot failed: {Player} ({PlayerId}) is not a Veiðimaðr",
                player.Name, player.Id);
            return null;
        }

        // Validate ability unlock
        if (!player.HasVeidimadurAbilityUnlocked(VeidimadurAbilityId.CripplingShot))
        {
            _logger.LogWarning(
                "Crippling Shot failed: {Player} ({PlayerId}) has not unlocked the ability",
                player.Name, player.Id);
            return null;
        }

        // Validate AP cost
        if (player.CurrentAP < CripplingShotApCost)
        {
            _logger.LogWarning(
                "Crippling Shot failed: {Player} ({PlayerId}) has insufficient AP " +
                "(need {Required}, have {Available})",
                player.Name, player.Id, CripplingShotApCost, player.CurrentAP);
            return null;
        }

        // === No Corruption evaluation — Coherent path ===

        // Validate that the target is a marked quarry
        if (!_quarryMarksService.HasActiveMark(player, targetId))
        {
            _logger.LogWarning(
                "Crippling Shot failed: {Player} ({PlayerId}) attempted to cripple " +
                "{Target} ({TargetId}) but target is not a marked quarry",
                player.Name, player.Id, targetName, targetId);
            return null;
        }

        // Deduct AP
        player.CurrentAP -= CripplingShotApCost;

        // Consume the Quarry Mark on the target
        _quarryMarksService.RemoveMark(player, targetId);

        // Build result with movement reduction
        var result = new CripplingShotResult
        {
            HunterId = player.Id,
            TargetId = targetId,
            TargetName = targetName,
            OriginalMovementSpeed = targetMovementSpeed,
            MarkConsumed = true
        };

        _logger.LogInformation(
            "Crippling Shot executed: {Player} ({PlayerId}) cripples {Target} ({TargetId}). " +
            "Movement reduced: {Original} → {Reduced} spaces for {Duration} turns. " +
            "Quarry Mark consumed. AP remaining: {RemainingAP}",
            player.Name, player.Id,
            targetName, targetId,
            targetMovementSpeed, result.ReducedMovementSpeed,
            CripplingShotDurationTurns,
            player.CurrentAP);

        return result;
    }

    // ===== Capstone Ability Method (v0.20.7c) =====

    /// <inheritdoc />
    public PerfectHuntResult? ExecuteThePerfectHunt(
        Player player,
        Guid targetId,
        string targetName,
        int baseDamageRoll)
    {
        ArgumentNullException.ThrowIfNull(player);

        // Validate specialization
        if (!IsVeidimadur(player))
        {
            _logger.LogWarning(
                "The Perfect Hunt failed: {Player} ({PlayerId}) is not a Veiðimaðr",
                player.Name, player.Id);
            return null;
        }

        // Validate ability unlock
        if (!player.HasVeidimadurAbilityUnlocked(VeidimadurAbilityId.ThePerfectHunt))
        {
            _logger.LogWarning(
                "The Perfect Hunt failed: {Player} ({PlayerId}) has not unlocked the capstone ability",
                player.Name, player.Id);
            return null;
        }

        // Validate cooldown BEFORE AP check — avoids deducting AP for a locked ability
        if (player.HasUsedThePerfectHuntThisRestCycle)
        {
            _logger.LogWarning(
                "The Perfect Hunt failed: {Player} ({PlayerId}) has already used The Perfect Hunt " +
                "this rest cycle. Available after next long rest",
                player.Name, player.Id);
            return null;
        }

        // Validate AP cost
        if (player.CurrentAP < ThePerfectHuntApCost)
        {
            _logger.LogWarning(
                "The Perfect Hunt failed: {Player} ({PlayerId}) has insufficient AP " +
                "(need {Required}, have {Available})",
                player.Name, player.Id, ThePerfectHuntApCost, player.CurrentAP);
            return null;
        }

        // === No Corruption evaluation — Coherent path ===

        // Validate that the target is a marked quarry
        if (!_quarryMarksService.HasActiveMark(player, targetId))
        {
            _logger.LogWarning(
                "The Perfect Hunt failed: {Player} ({PlayerId}) attempted to strike " +
                "{Target} ({TargetId}) but target is not a marked quarry",
                player.Name, player.Id, targetName, targetId);
            return null;
        }

        // Deduct AP
        player.CurrentAP -= ThePerfectHuntApCost;

        // Set the once-per-long-rest cooldown
        player.HasUsedThePerfectHuntThisRestCycle = true;

        // Consume the Quarry Mark on the target
        _quarryMarksService.RemoveMark(player, targetId);

        // Calculate total damage: base damage × critical multiplier
        var totalDamage = baseDamageRoll * ThePerfectHuntCriticalMultiplier;

        // Build narrative description for the devastating strike
        var narrative = $"The hunter's patience culminates in a single, perfect strike. " +
                        $"{targetName} is struck with devastating precision — " +
                        $"an automatic critical hit dealing {totalDamage} damage " +
                        $"({baseDamageRoll} × {ThePerfectHuntCriticalMultiplier}).";

        // Build result
        var result = new PerfectHuntResult
        {
            HunterId = player.Id,
            TargetId = targetId,
            TargetName = targetName,
            BaseDamageRoll = baseDamageRoll,
            MarkConsumed = true,
            CapstoneUsed = true,
            NarrativeDescription = narrative
        };

        _logger.LogInformation(
            "THE PERFECT HUNT executed: {Player} ({PlayerId}) devastates {Target} ({TargetId}). " +
            "AUTO-CRIT: {BaseDamage} × {Multiplier} = {TotalDamage} total damage. " +
            "Quarry Mark consumed. Capstone cooldown set (next available after long rest). " +
            "AP remaining: {RemainingAP}",
            player.Name, player.Id,
            targetName, targetId,
            baseDamageRoll, ThePerfectHuntCriticalMultiplier, totalDamage,
            player.CurrentAP);

        return result;
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

                // Tier 2 passive — always ready when unlocked (no AP cost)
                VeidimadurAbilityId.HuntersEye => true,

                // Tier 2 active — requires AP for placement or detection
                VeidimadurAbilityId.TrapMastery =>
                    player.CurrentAP >= TrapMasteryPlaceApCost,

                // Tier 2 stance — requires AP to activate
                VeidimadurAbilityId.PredatorsPatience =>
                    player.CurrentAP >= PredatorsPatienceApCost,

                // Tier 3 passive — always ready when unlocked (no AP cost)
                VeidimadurAbilityId.ApexPredator => true,

                // Tier 3 active — requires AP and consumes a quarry mark
                VeidimadurAbilityId.CripplingShot =>
                    player.CurrentAP >= CripplingShotApCost,

                // Capstone — requires AP, mark, and rest-cycle cooldown not used
                VeidimadurAbilityId.ThePerfectHunt =>
                    !player.HasUsedThePerfectHuntThisRestCycle &&
                    player.CurrentAP >= ThePerfectHuntApCost,

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

    /// <summary>
    /// Rolls 1d8 for trap damage calculations.
    /// </summary>
    /// <returns>A value between 1 and 8 inclusive.</returns>
    /// <remarks>
    /// Marked <c>internal virtual</c> to allow test subclasses to provide deterministic values.
    /// Used by Trap Mastery for calculating trap damage when triggered.
    /// </remarks>
    internal virtual int Roll1D8()
    {
        return Random.Shared.Next(1, 9);
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
