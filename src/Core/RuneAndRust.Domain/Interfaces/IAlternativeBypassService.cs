// ------------------------------------------------------------------------------
// <copyright file="IAlternativeBypassService.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Service interface for handling alternative bypass methods when primary
// skill-based approaches are unavailable. Includes MIGHT-based brute force
// for physical obstacles and alternative approaches for doors, terminals,
// and traps.
// Part of v0.15.4h Alternative Bypass Methods implementation.
// </summary>
// ------------------------------------------------------------------------------

using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.Interfaces;

// =============================================================================
// CONTEXT VALUE OBJECTS
// =============================================================================

/// <summary>
/// Context for a brute force bypass attempt.
/// </summary>
/// <remarks>
/// <para>
/// Provides all context-specific information that affects the brute force
/// attempt beyond the base obstacle characteristics:
/// <list type="bullet">
///   <item><description>Attempt tracking for retry penalties</description></item>
///   <item><description>Fumble history for permanent DC penalties</description></item>
///   <item><description>Tool usage for DC modifiers and bonus dice</description></item>
///   <item><description>Assistance for bonus dice (max +2d10)</description></item>
///   <item><description>Exhaustion for penalty dice</description></item>
/// </list>
/// </para>
/// </remarks>
/// <param name="AttemptNumber">Which attempt this is (1-based).</param>
/// <param name="HasFumbled">Whether a fumble occurred in previous attempts.</param>
/// <param name="ToolUsed">Name of tool being used, if any (e.g., "Crowbar", "Sledgehammer").</param>
/// <param name="AssistingCharacters">Number of characters helping (max +2d10 bonus).</param>
/// <param name="ExhaustionLevel">Current exhaustion level (penalty = level / 2).</param>
public readonly record struct BruteForceContext(
    int AttemptNumber,
    bool HasFumbled,
    string? ToolUsed,
    int AssistingCharacters,
    int ExhaustionLevel)
{
    /// <summary>
    /// Creates a context for a first attempt with no tools or assistance.
    /// </summary>
    /// <returns>A default first-attempt context.</returns>
    public static BruteForceContext FirstAttempt() => new(
        AttemptNumber: 1,
        HasFumbled: false,
        ToolUsed: null,
        AssistingCharacters: 0,
        ExhaustionLevel: 0);

    /// <summary>
    /// Creates a context for a first attempt with a specific tool.
    /// </summary>
    /// <param name="toolName">Name of the tool being used.</param>
    /// <returns>A first-attempt context with the specified tool.</returns>
    public static BruteForceContext FirstAttemptWithTool(string toolName) => new(
        AttemptNumber: 1,
        HasFumbled: false,
        ToolUsed: toolName,
        AssistingCharacters: 0,
        ExhaustionLevel: 0);

    /// <summary>
    /// Creates a context for a retry attempt after a previous failure.
    /// </summary>
    /// <param name="previousAttempts">Number of previous attempts.</param>
    /// <param name="previouslyFumbled">Whether any previous attempt was a fumble.</param>
    /// <param name="toolName">Name of the tool being used, if any.</param>
    /// <returns>A retry context with proper attempt number.</returns>
    public static BruteForceContext Retry(
        int previousAttempts,
        bool previouslyFumbled,
        string? toolName = null) => new(
        AttemptNumber: previousAttempts + 1,
        HasFumbled: previouslyFumbled,
        ToolUsed: toolName,
        AssistingCharacters: 0,
        ExhaustionLevel: 0);

    /// <summary>
    /// Gets the effective assistance bonus dice (capped at 2).
    /// </summary>
    /// <returns>Number of bonus dice from assistance (0-2).</returns>
    public int GetAssistanceBonusDice() => Math.Min(AssistingCharacters, 2);

    /// <summary>
    /// Gets the exhaustion penalty to dice pool.
    /// </summary>
    /// <returns>Number of dice to remove from pool.</returns>
    public int GetExhaustionPenalty() => ExhaustionLevel / 2;

    /// <summary>
    /// Creates a display string for this context.
    /// </summary>
    /// <returns>A formatted string showing the context details.</returns>
    public string ToDisplayString()
    {
        var parts = new List<string>
        {
            $"Attempt #{AttemptNumber}"
        };

        if (HasFumbled)
        {
            parts.Add("fumbled previously");
        }

        if (!string.IsNullOrEmpty(ToolUsed))
        {
            parts.Add($"using {ToolUsed}");
        }

        if (AssistingCharacters > 0)
        {
            parts.Add($"{AssistingCharacters} assistant{(AssistingCharacters > 1 ? "s" : "")}");
        }

        if (ExhaustionLevel > 0)
        {
            parts.Add($"exhaustion {ExhaustionLevel}");
        }

        return string.Join(", ", parts);
    }
}

/// <summary>
/// Result of evaluating whether a character can attempt an alternative method.
/// </summary>
/// <remarks>
/// <para>
/// Provides detailed feedback about prerequisite checking:
/// <list type="bullet">
///   <item><description>Whether the method can be attempted</description></item>
///   <item><description>Which prerequisites are missing, if any</description></item>
///   <item><description>Estimated difficulty for player guidance</description></item>
///   <item><description>Recommended approach for next steps</description></item>
/// </list>
/// </para>
/// </remarks>
/// <param name="CanAttempt">Whether all prerequisites are met.</param>
/// <param name="MissingPrerequisites">List of unmet prerequisites.</param>
/// <param name="EstimatedDifficulty">Descriptive difficulty level (e.g., "Moderate", "Hard").</param>
/// <param name="RecommendedApproach">Suggestion for the player.</param>
public readonly record struct AlternativeEvaluationResult(
    bool CanAttempt,
    IReadOnlyList<string> MissingPrerequisites,
    string EstimatedDifficulty,
    string RecommendedApproach)
{
    /// <summary>
    /// Creates a result indicating the method can be attempted.
    /// </summary>
    /// <param name="difficulty">Estimated difficulty description.</param>
    /// <returns>A positive evaluation result.</returns>
    public static AlternativeEvaluationResult Ready(string difficulty) => new(
        CanAttempt: true,
        MissingPrerequisites: Array.Empty<string>(),
        EstimatedDifficulty: difficulty,
        RecommendedApproach: "Ready to attempt");

    /// <summary>
    /// Creates a result indicating prerequisites are missing.
    /// </summary>
    /// <param name="missingPrerequisites">List of unmet prerequisites.</param>
    /// <param name="difficulty">Estimated difficulty description.</param>
    /// <returns>A negative evaluation result with missing prerequisites.</returns>
    public static AlternativeEvaluationResult NotReady(
        IReadOnlyList<string> missingPrerequisites,
        string difficulty) => new(
        CanAttempt: false,
        MissingPrerequisites: missingPrerequisites,
        EstimatedDifficulty: difficulty,
        RecommendedApproach: "Acquire missing prerequisites first");

    /// <summary>
    /// Gets a value indicating whether any prerequisites are missing.
    /// </summary>
    public bool HasMissingPrerequisites => MissingPrerequisites.Count > 0;

    /// <summary>
    /// Creates a display string for this evaluation result.
    /// </summary>
    /// <returns>A formatted multi-line string showing the evaluation.</returns>
    public string ToDisplayString()
    {
        var lines = new List<string>
        {
            $"Can Attempt: {(CanAttempt ? "Yes" : "No")}",
            $"Difficulty: {EstimatedDifficulty}"
        };

        if (MissingPrerequisites.Count > 0)
        {
            lines.Add(string.Empty);
            lines.Add("Missing Prerequisites:");
            foreach (var prereq in MissingPrerequisites)
            {
                lines.Add($"  - {prereq}");
            }
        }

        lines.Add(string.Empty);
        lines.Add($"Recommendation: {RecommendedApproach}");

        return string.Join(Environment.NewLine, lines);
    }
}

// =============================================================================
// SERVICE INTERFACE
// =============================================================================

/// <summary>
/// Service interface for handling alternative bypass methods when primary
/// skill-based approaches are unavailable.
/// </summary>
/// <remarks>
/// <para>
/// This service ensures that obstacles are never absolute barriers. Every
/// locked door, secured terminal, and active trap has multiple solutions,
/// though each comes with its own trade-offs:
/// <list type="bullet">
///   <item><description>Brute Force: MIGHT-based, fast but loud and may damage contents</description></item>
///   <item><description>Find Key/Codes: Investigation-based, takes time but clean</description></item>
///   <item><description>Runic Bypass: Runecraft-based, requires abilities but quick</description></item>
///   <item><description>Alternate Routes: Perception-based, avoids obstacle entirely</description></item>
///   <item><description>And more specific to each obstacle type...</description></item>
/// </list>
/// </para>
/// <para>
/// <b>Design Principle:</b> No character should be permanently stuck because
/// they lack a specific skill. Alternative methods provide different paths
/// forward, each suited to different character builds.
/// </para>
/// </remarks>
public interface IAlternativeBypassService
{
    // =========================================================================
    // BRUTE FORCE METHODS
    // =========================================================================

    /// <summary>
    /// Gets the brute force option for a given target type.
    /// </summary>
    /// <param name="targetType">Type of physical obstacle.</param>
    /// <param name="targetStrength">Strength modifier for containers (1-5). Defaults to 3.</param>
    /// <returns>The brute force option with DC, consequences, and tool modifiers.</returns>
    /// <remarks>
    /// <para>
    /// Target Type DCs:
    /// <list type="bullet">
    ///   <item><description>SimpleDoor: DC 12, max 5 attempts, +1 DC per retry</description></item>
    ///   <item><description>ReinforcedDoor: DC 16, max 3 attempts, +2 DC per retry</description></item>
    ///   <item><description>Vault: DC 22, max 2 attempts, +3 DC per retry</description></item>
    ///   <item><description>Container: DC 8 + (strength × 2), max 3 attempts, +1 DC per retry</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var option = service.GetBruteForceOption(BruteForceTargetType.SimpleDoor);
    /// Console.WriteLine($"DC: {option.BaseDc}"); // Output: "DC: 12"
    /// </code>
    /// </example>
    BruteForceOption GetBruteForceOption(BruteForceTargetType targetType, int targetStrength = 3);

    /// <summary>
    /// Attempts a brute force bypass of a physical obstacle.
    /// </summary>
    /// <param name="mightAttribute">The character's MIGHT attribute score.</param>
    /// <param name="target">The brute force target details.</param>
    /// <param name="context">Context including previous attempts and tools.</param>
    /// <returns>Result of the brute force attempt.</returns>
    /// <remarks>
    /// <para>
    /// The dice pool is calculated as:
    /// <code>
    /// Pool = MIGHT + ToolBonusDice + AssistanceBonus - ExhaustionPenalty
    /// </code>
    /// </para>
    /// <para>
    /// Outcomes:
    /// <list type="bullet">
    ///   <item><description>Critical Success (net ≥ 5): Obstacle destroyed, reduced noise, no consequences</description></item>
    ///   <item><description>Success (net > 0): Obstacle destroyed, consequences apply</description></item>
    ///   <item><description>Failure (net ≤ 0): Obstacle intact, may retry</description></item>
    ///   <item><description>Fumble (0 successes + botch): 1d6 damage, tool breaks, +2 permanent DC</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var target = service.GetBruteForceOption(BruteForceTargetType.SimpleDoor);
    /// var context = BruteForceContext.FirstAttemptWithTool("Crowbar");
    /// var result = service.AttemptBruteForce(mightScore: 4, target, context);
    /// if (result.Success)
    /// {
    ///     Console.WriteLine("Door destroyed!");
    /// }
    /// </code>
    /// </example>
    BruteForceResult AttemptBruteForce(int mightAttribute, BruteForceOption target, BruteForceContext context);

    /// <summary>
    /// Determines if a retry is possible for a given target.
    /// </summary>
    /// <param name="target">The brute force target.</param>
    /// <param name="attemptsMade">Number of attempts already made.</param>
    /// <returns>True if the character can retry, false if max attempts reached.</returns>
    bool CanRetry(BruteForceOption target, int attemptsMade);

    /// <summary>
    /// Gets the effective DC for a retry attempt.
    /// </summary>
    /// <param name="target">The brute force target.</param>
    /// <param name="attemptsMade">Number of attempts already made.</param>
    /// <param name="hasFumbled">Whether any previous attempt was a fumble.</param>
    /// <returns>The adjusted DC for the retry attempt.</returns>
    /// <remarks>
    /// <para>
    /// DC calculation:
    /// <code>
    /// RetryDC = BaseDC + (attemptsMade × RetryPenalty) + (fumbled ? 2 : 0)
    /// </code>
    /// </para>
    /// </remarks>
    int GetRetryDc(BruteForceOption target, int attemptsMade, bool hasFumbled);

    // =========================================================================
    // ALTERNATIVE METHOD QUERIES
    // =========================================================================

    /// <summary>
    /// Gets all alternative bypass methods for a given obstacle type.
    /// </summary>
    /// <param name="obstacleType">Type of obstacle.</param>
    /// <returns>Available alternative methods for the obstacle.</returns>
    /// <remarks>
    /// <para>
    /// Returns the following alternatives by obstacle type:
    /// <list type="bullet">
    ///   <item><description>LockedDoor: FindKey, BruteForce, RunicBypass, AlternateRoute</description></item>
    ///   <item><description>SecuredTerminal: FindCodes, HotwireTerminal, AcceptPartialAccess</description></item>
    ///   <item><description>ActiveTrap: TriggerFromDistance, DestroyMechanism, Sacrifice</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var alternatives = service.GetAlternatives(ObstacleType.LockedDoor);
    /// foreach (var alt in alternatives)
    /// {
    ///     Console.WriteLine($"{alt.MethodName}: {alt.Description}");
    /// }
    /// </code>
    /// </example>
    IEnumerable<AlternativeMethod> GetAlternatives(BypassObstacleType obstacleType);

    /// <summary>
    /// Gets an alternative method by its unique identifier.
    /// </summary>
    /// <param name="methodId">The method ID to search for.</param>
    /// <returns>The matching method, or null if not found.</returns>
    AlternativeMethod? GetAlternativeById(string methodId);

    // =========================================================================
    // PREREQUISITE EVALUATION
    // =========================================================================

    /// <summary>
    /// Evaluates whether a character can attempt an alternative method.
    /// </summary>
    /// <param name="method">The alternative method to evaluate.</param>
    /// <param name="characterAbilities">List of abilities the character possesses.</param>
    /// <param name="characterItems">List of items the character has available.</param>
    /// <returns>Evaluation result with any missing prerequisites.</returns>
    /// <remarks>
    /// <para>
    /// Prerequisite examples:
    /// <list type="bullet">
    ///   <item><description>"[Rune Sight] ability or equivalent" - Checks characterAbilities</description></item>
    ///   <item><description>"Wire or cable available" - Checks characterItems</description></item>
    ///   <item><description>"Must have achieved at least User Level access" - Checks context</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var method = AlternativeMethod.RunicBypass;
    /// var abilities = new[] { "Rune Sight", "Dark Vision" };
    /// var items = new[] { "Lockpicks", "Rope" };
    /// var result = service.EvaluateAlternative(method, abilities, items);
    /// if (result.CanAttempt)
    /// {
    ///     Console.WriteLine("Ready to attempt Runic Bypass!");
    /// }
    /// </code>
    /// </example>
    AlternativeEvaluationResult EvaluateAlternative(
        AlternativeMethod method,
        IReadOnlyList<string> characterAbilities,
        IReadOnlyList<string> characterItems);

    // =========================================================================
    // UTILITY METHODS
    // =========================================================================

    /// <summary>
    /// Gets a human-readable difficulty description for a DC value.
    /// </summary>
    /// <param name="dc">The difficulty class value.</param>
    /// <returns>A descriptive string (e.g., "Easy", "Moderate", "Hard").</returns>
    /// <remarks>
    /// <para>
    /// Difficulty thresholds:
    /// <list type="bullet">
    ///   <item><description>DC 0: Automatic</description></item>
    ///   <item><description>DC 1-8: Very Easy</description></item>
    ///   <item><description>DC 9-12: Easy</description></item>
    ///   <item><description>DC 13-16: Moderate</description></item>
    ///   <item><description>DC 17-20: Hard</description></item>
    ///   <item><description>DC 21-24: Very Hard</description></item>
    ///   <item><description>DC 25+: Nearly Impossible</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    string GetDifficultyDescription(int dc);

    /// <summary>
    /// Gets the bonus dice provided by a specific tool for brute force.
    /// </summary>
    /// <param name="toolName">Name of the tool.</param>
    /// <returns>Number of bonus d10s, or 0 if tool not recognized.</returns>
    /// <remarks>
    /// <para>
    /// Tool bonus dice:
    /// <list type="bullet">
    ///   <item><description>Crowbar: +1d10</description></item>
    ///   <item><description>Sledgehammer: +2d10</description></item>
    ///   <item><description>Breaching Charge: +2d10</description></item>
    ///   <item><description>Industrial Cutter: +2d10</description></item>
    ///   <item><description>Knife: +0d10 (DC modifier only)</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    int GetToolBonusDice(string toolName);
}
