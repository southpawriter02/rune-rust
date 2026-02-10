// ═══════════════════════════════════════════════════════════════════════════════
// JotunReaderTier2AbilityService.cs
// Implements Tier 2 ability operations for the Jötun-Reader specialization:
// Technical Memory, Exploit Weakness, and Data Recovery.
// Version: 0.20.3b
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Application.Services;

using Microsoft.Extensions.Logging;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Implements Tier 2 Jötun-Reader abilities: Technical Memory, Exploit Weakness,
/// and Data Recovery.
/// </summary>
/// <remarks>
/// <para>
/// This service handles all Tier 2 ability operations for the Jötun-Reader
/// specialization. It operates on immutable value objects and returns new
/// instances for all state transitions.
/// </para>
/// <para>
/// <b>Tier 2 requires 8 PP invested in the Jötun-Reader ability tree.</b>
/// Each Tier 2 ability costs 4 PP to unlock.
/// </para>
/// <para>
/// Logging follows the established specialization service pattern:
/// </para>
/// <list type="bullet">
///   <item><description>
///     <b>Information:</b> Successful recalls, analyses, recoveries, and
///     bonus Lore Insight generation
///   </description></item>
///   <item><description>
///     <b>Warning:</b> Rejected operations (no match found, invalid terminal
///     state, insufficient PP)
///   </description></item>
///   <item><description>
///     <b>Debug:</b> Prerequisite checks, pattern matching details, DC lookups
///   </description></item>
/// </list>
/// </remarks>
/// <seealso cref="TechnicalMemoryRecord"/>
/// <seealso cref="WeaknessAnalysis"/>
/// <seealso cref="RecoveredData"/>
public class JotunReaderTier2AbilityService
{
    private readonly ILogger<JotunReaderTier2AbilityService> _logger;
    private readonly Random _random;

    /// <summary>
    /// PP threshold required to unlock Tier 2 Jötun-Reader abilities.
    /// </summary>
    public const int Tier2Threshold = 8;

    /// <summary>
    /// Bonus Lore Insight generated on critical Data Recovery success.
    /// </summary>
    public const int CriticalSuccessBonusInsight = 1;

    /// <summary>
    /// How far above the DC a check must be for critical success.
    /// </summary>
    public const int CriticalSuccessMargin = 10;

    /// <summary>
    /// How far below the DC a check can be for partial success.
    /// </summary>
    public const int PartialSuccessMargin = 5;

    /// <summary>
    /// Bonus data fragments awarded on critical success.
    /// </summary>
    public const int CriticalSuccessBonusFragments = 2;

    /// <summary>
    /// Number of sides on fragment dice (1d4).
    /// </summary>
    public const int FragmentDiceSides = 4;

    /// <summary>
    /// Maps terminal states to base recovery DCs.
    /// </summary>
    private static readonly Dictionary<TerminalState, int> RecoveryDCs = new()
    {
        { TerminalState.Active, 10 },
        { TerminalState.Dormant, 12 },
        { TerminalState.Corrupted, 16 },
        { TerminalState.Dead, 18 },
        { TerminalState.Glitched, 14 }
    };

    /// <summary>
    /// Initializes a new instance of <see cref="JotunReaderTier2AbilityService"/>.
    /// </summary>
    /// <param name="logger">Logger for ability audit trail.</param>
    /// <param name="random">Optional random instance for testability. Defaults to shared instance.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="logger"/> is null.</exception>
    public JotunReaderTier2AbilityService(
        ILogger<JotunReaderTier2AbilityService> logger,
        Random? random = null)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _random = random ?? Random.Shared;
    }

    // ═══════ Technical Memory ═══════

    /// <summary>
    /// Records a solved puzzle into the character's technical memory.
    /// </summary>
    /// <param name="category">Category of the puzzle (e.g., "Mechanical").</param>
    /// <param name="description">Description of the puzzle mechanism.</param>
    /// <param name="solution">The solution used to solve the puzzle.</param>
    /// <param name="locationId">ID of the location where the puzzle was solved.</param>
    /// <param name="locationName">Display name of the location.</param>
    /// <param name="originalDC">The original DC of the puzzle.</param>
    /// <returns>A new <see cref="TechnicalMemoryRecord"/>.</returns>
    public TechnicalMemoryRecord RecordPuzzleSolution(
        string category,
        string description,
        string solution,
        Guid locationId,
        string locationName,
        int originalDC)
    {
        var record = TechnicalMemoryRecord.Create(
            category, description, solution,
            locationId, locationName, originalDC);

        _logger.LogInformation(
            "Technical Memory: Puzzle recorded - Category: {Category}, " +
            "Location: {Location}, DC: {DC}. RecordId: {RecordId}",
            category, locationName, originalDC, record.RecordId);

        return record;
    }

    /// <summary>
    /// Attempts to recall a solution for the current puzzle from stored memories.
    /// </summary>
    /// <param name="puzzleCategory">Category of the puzzle to recall.</param>
    /// <param name="puzzleDescription">Description of the current puzzle.</param>
    /// <param name="storedMemories">Previously recorded puzzle solutions.</param>
    /// <returns>
    /// A tuple of (found, isExactMatch, solutionOrHint, dcReduction).
    /// If no match: found = false.
    /// If exact match: found = true, isExactMatch = true, solutionOrHint = solution.
    /// If similar: found = true, isExactMatch = false, solutionOrHint = hint, dcReduction = 5.
    /// </returns>
    public (bool Found, bool IsExactMatch, string? SolutionOrHint, int DCReduction)
        AttemptRecall(
            string puzzleCategory,
            string puzzleDescription,
            IReadOnlyList<TechnicalMemoryRecord> storedMemories)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(puzzleCategory);
        ArgumentException.ThrowIfNullOrWhiteSpace(puzzleDescription);
        ArgumentNullException.ThrowIfNull(storedMemories);

        foreach (var record in storedMemories)
        {
            // Check for exact match (same category AND same description)
            if (record.PuzzleCategory.Equals(puzzleCategory, StringComparison.OrdinalIgnoreCase) &&
                record.PuzzleDescription.Equals(puzzleDescription, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogInformation(
                    "Technical Memory: Exact match found! Category: {Category}, " +
                    "Original location: {Location}. RecordId: {RecordId}",
                    puzzleCategory, record.LocationName, record.RecordId);

                return (true, true, record.Solution, 0);
            }

            // Check for similar pattern
            if (record.MatchesPuzzle(puzzleCategory, puzzleDescription))
            {
                _logger.LogInformation(
                    "Technical Memory: Similar pattern found. Category: {Category}, " +
                    "DC reduced by {Reduction}. Original location: {Location}. " +
                    "RecordId: {RecordId}",
                    puzzleCategory,
                    TechnicalMemoryRecord.SimilarPatternDCReduction,
                    record.LocationName,
                    record.RecordId);

                return (true, false, record.GetSolutionHint(),
                    TechnicalMemoryRecord.SimilarPatternDCReduction);
            }
        }

        _logger.LogWarning(
            "Technical Memory: No matching puzzle found for category {Category}. " +
            "Searched {Count} stored memories",
            puzzleCategory, storedMemories.Count);

        return (false, false, null, 0);
    }

    // ═══════ Exploit Weakness ═══════

    /// <summary>
    /// Creates an enemy weakness analysis containing vulnerabilities, resistances,
    /// immunities, weak points, and behavioral patterns.
    /// </summary>
    /// <param name="targetId">ID of the enemy to analyze.</param>
    /// <param name="targetName">Display name of the enemy.</param>
    /// <param name="vulnerabilities">Damage types the enemy is vulnerable to.</param>
    /// <param name="resistances">Damage types the enemy resists.</param>
    /// <param name="immunities">Damage types the enemy is immune to.</param>
    /// <param name="weakPoints">Specific weak points on the enemy.</param>
    /// <param name="behaviors">Observed behavioral patterns.</param>
    /// <returns>A new <see cref="WeaknessAnalysis"/> instance.</returns>
    public WeaknessAnalysis AnalyzeEnemy(
        Guid targetId,
        string targetName,
        IEnumerable<DamageType> vulnerabilities,
        IEnumerable<DamageType> resistances,
        IEnumerable<DamageType> immunities,
        IEnumerable<WeakPoint> weakPoints,
        IEnumerable<string> behaviors)
    {
        var analysis = WeaknessAnalysis.Create(
            targetId, targetName,
            vulnerabilities, resistances, immunities,
            weakPoints, behaviors);

        _logger.LogInformation(
            "Exploit Weakness: Analyzed {TargetName} - {VulnCount} vulnerabilities, " +
            "{ResistCount} resistances, {ImmuneCount} immunities, {WeakPointCount} weak points. " +
            "AnalysisId: {AnalysisId}",
            targetName,
            analysis.Vulnerabilities.Count,
            analysis.Resistances.Count,
            analysis.Immunities.Count,
            analysis.WeakPoints.Count,
            analysis.AnalysisId);

        return analysis;
    }

    /// <summary>
    /// Calculates the bonus damage when exploiting a vulnerability.
    /// </summary>
    /// <remarks>
    /// Returns a 1d6 roll when the attack's damage type matches a vulnerability
    /// in the analysis. Returns 0 if no analysis exists or the type is not vulnerable.
    /// </remarks>
    /// <param name="attackDamageType">The damage type of the incoming attack.</param>
    /// <param name="analysis">Active weakness analysis, or null.</param>
    /// <returns>Bonus damage (0 if not exploiting a weakness, 1-6 otherwise).</returns>
    public int CalculateExploitDamageBonus(
        DamageType attackDamageType,
        WeaknessAnalysis? analysis)
    {
        if (analysis == null || !analysis.IsVulnerableTo(attackDamageType))
            return 0;

        var bonus = _random.Next(1, WeaknessAnalysis.ExploitBonusDieSides + 1);

        _logger.LogInformation(
            "Exploit Weakness: Vulnerability exploited! {DamageType} attack deals " +
            "+{BonusDamage} bonus damage to {TargetName}",
            attackDamageType, bonus, analysis.TargetName);

        return bonus;
    }

    /// <summary>
    /// Calculates the hit bonus for targeting a specific weak point.
    /// </summary>
    /// <param name="targetLocation">The body location being targeted.</param>
    /// <param name="analysis">Active weakness analysis, or null.</param>
    /// <returns>Hit bonus (0 if no analysis or no weak point at location).</returns>
    public static int CalculateHitBonus(
        string targetLocation,
        WeaknessAnalysis? analysis)
    {
        if (analysis == null)
            return 0;

        return analysis.GetWeakPointBonus(targetLocation);
    }

    // ═══════ Data Recovery ═══════

    /// <summary>
    /// Attempts to recover data from a terminal based on the check result.
    /// </summary>
    /// <param name="terminalId">ID of the terminal to recover data from.</param>
    /// <param name="terminalName">Display name of the terminal.</param>
    /// <param name="terminalState">Current condition of the terminal.</param>
    /// <param name="recoveryCheck">The character's recovery check result.</param>
    /// <param name="availableFragments">Data fragments available in the terminal.</param>
    /// <returns>
    /// A tuple of (recoveredData, bonusInsightGenerated).
    /// recoveredData is null on total failure. bonusInsightGenerated is true
    /// on critical success.
    /// </returns>
    public (RecoveredData? Data, bool BonusInsightGenerated) AttemptDataRecovery(
        Guid terminalId,
        string terminalName,
        TerminalState terminalState,
        int recoveryCheck,
        IReadOnlyList<string> availableFragments)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(terminalName);
        ArgumentNullException.ThrowIfNull(availableFragments);

        var baseDC = GetRecoveryDC(terminalState);
        int fragmentCount;
        var isComplete = false;
        var bonusInsight = false;

        if (recoveryCheck >= baseDC + CriticalSuccessMargin)
        {
            // Critical success: full recovery + bonus fragments + bonus Insight
            fragmentCount = RollFragmentCount() + CriticalSuccessBonusFragments;
            isComplete = true;
            bonusInsight = true;

            _logger.LogInformation(
                "Data Recovery: CRITICAL SUCCESS on {TerminalName} ({State})! " +
                "Check {Check} vs DC {DC} (+{Margin}). " +
                "+{BonusInsight} bonus Lore Insight generated",
                terminalName, terminalState,
                recoveryCheck, baseDC, CriticalSuccessMargin,
                CriticalSuccessBonusInsight);
        }
        else if (recoveryCheck >= baseDC)
        {
            // Normal success: recover 1d4 fragments
            fragmentCount = RollFragmentCount();

            _logger.LogInformation(
                "Data Recovery: SUCCESS on {TerminalName} ({State}). " +
                "Check {Check} vs DC {DC}. Recovered {Fragments} fragments",
                terminalName, terminalState,
                recoveryCheck, baseDC, fragmentCount);
        }
        else if (recoveryCheck >= baseDC - PartialSuccessMargin)
        {
            // Partial success: recover 1 hint fragment
            fragmentCount = 1;

            _logger.LogInformation(
                "Data Recovery: PARTIAL SUCCESS on {TerminalName} ({State}). " +
                "Check {Check} vs DC {DC}. Recovered 1 hint fragment",
                terminalName, terminalState,
                recoveryCheck, baseDC);
        }
        else
        {
            // Total failure
            _logger.LogWarning(
                "Data Recovery: FAILED on {TerminalName} ({State}). " +
                "Check {Check} vs DC {DC}. No data recovered",
                terminalName, terminalState,
                recoveryCheck, baseDC);

            return (null, false);
        }

        // Take the appropriate number of fragments from available data
        var fragments = availableFragments
            .Take(Math.Min(fragmentCount, availableFragments.Count))
            .ToList();

        var recoveredData = RecoveredData.Create(
            terminalId, terminalName, fragments, isComplete);

        return (recoveredData, bonusInsight);
    }

    /// <summary>
    /// Gets the base DC for recovering data from a terminal in the given state.
    /// </summary>
    /// <param name="state">The terminal's current condition.</param>
    /// <returns>The base DC. Defaults to 16 for unknown states.</returns>
    public int GetRecoveryDC(TerminalState state) =>
        RecoveryDCs.TryGetValue(state, out var dc) ? dc : 16;

    // ═══════ Prerequisite Helpers ═══════

    /// <summary>
    /// Checks whether Tier 2 abilities can be unlocked based on PP invested.
    /// Requires 8 PP invested in the Jötun-Reader tree.
    /// </summary>
    /// <param name="ppInvested">Total PP invested.</param>
    /// <returns>True if threshold is met.</returns>
    public bool CanUnlockTier2(int ppInvested)
    {
        var canUnlock = ppInvested >= Tier2Threshold;

        _logger.LogDebug(
            "Jötun-Reader Tier 2 unlock check: PP invested {PPInvested}, " +
            "threshold {Threshold}, result {CanUnlock}",
            ppInvested, Tier2Threshold, canUnlock);

        return canUnlock;
    }

    /// <summary>
    /// Calculates total PP invested from a list of unlocked Jötun-Reader abilities.
    /// </summary>
    /// <param name="unlockedAbilities">Currently unlocked abilities.</param>
    /// <returns>Total PP cost of all unlocked abilities.</returns>
    public int CalculatePPInvested(IReadOnlyList<JotunReaderAbilityId> unlockedAbilities)
    {
        ArgumentNullException.ThrowIfNull(unlockedAbilities);

        var total = 0;
        foreach (var ability in unlockedAbilities)
        {
            total += GetAbilityPPCost(ability);
        }

        _logger.LogDebug(
            "Jötun-Reader PP invested calculation: {AbilityCount} abilities unlocked, " +
            "total PP invested: {TotalPP}",
            unlockedAbilities.Count, total);

        return total;
    }

    /// <summary>
    /// Gets the PP cost for a specific Jötun-Reader ability based on its tier.
    /// </summary>
    /// <param name="abilityId">The ability to look up.</param>
    /// <returns>PP cost: 0 (Tier 1), 4 (Tier 2), 5 (Tier 3), 6 (Capstone).</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown for unknown abilities.</exception>
    public static int GetAbilityPPCost(JotunReaderAbilityId abilityId) => abilityId switch
    {
        // Tier 1: Free
        JotunReaderAbilityId.DeepScan => 0,
        JotunReaderAbilityId.PatternRecognition => 0,
        JotunReaderAbilityId.AncientTongues => 0,

        // Tier 2: 4 PP each
        JotunReaderAbilityId.TechnicalMemory => 4,
        JotunReaderAbilityId.ExploitWeakness => 4,
        JotunReaderAbilityId.DataRecovery => 4,

        // Tier 3: 5 PP each (future — v0.20.3c)
        JotunReaderAbilityId.LoreKeeper => 5,
        JotunReaderAbilityId.AncientKnowledge => 5,

        // Capstone: 6 PP (future — v0.20.3c)
        JotunReaderAbilityId.VoiceOfTheGiants => 6,

        _ => throw new ArgumentOutOfRangeException(nameof(abilityId), abilityId,
            $"Unknown Jötun-Reader ability: {abilityId}")
    };

    private int RollFragmentCount() => _random.Next(1, FragmentDiceSides + 1);
}
