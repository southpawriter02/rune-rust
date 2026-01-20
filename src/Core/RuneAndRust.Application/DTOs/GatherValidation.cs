namespace RuneAndRust.Application.DTOs;

/// <summary>
/// Represents the result of validating a gathering attempt before rolling.
/// </summary>
/// <remarks>
/// <para>
/// GatherValidation checks pre-roll conditions such as feature availability,
/// tool requirements, and depletion status. If validation passes, it also
/// provides the difficulty class for the upcoming dice roll.
/// </para>
/// <para>
/// Use the static factory methods to create instances:
/// <list type="bullet">
///   <item><description><see cref="Success"/> - Validation passed, gathering can proceed.</description></item>
///   <item><description><see cref="Failed"/> - Validation failed, gathering cannot proceed.</description></item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Successful validation
/// var validation = GatherValidation.Success(dc: 12);
/// if (validation.IsValid)
/// {
///     var result = gatheringService.RollGatherCheck(player, feature, validation.DifficultyClass!.Value);
/// }
///
/// // Failed validation
/// var validation = GatherValidation.Failed("You need a pickaxe to gather from this.");
/// if (!validation.IsValid)
/// {
///     Console.WriteLine(validation.FailureReason);
/// }
/// </code>
/// </example>
public record GatherValidation
{
    // ═══════════════════════════════════════════════════════════════
    // PROPERTIES
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets whether the validation passed and gathering can proceed.
    /// </summary>
    /// <remarks>
    /// When true, <see cref="DifficultyClass"/> will contain the DC for the roll.
    /// When false, <see cref="FailureReason"/> will explain why validation failed.
    /// </remarks>
    public bool IsValid { get; init; }

    /// <summary>
    /// Gets the difficulty class for the gathering check.
    /// </summary>
    /// <remarks>
    /// Only populated when <see cref="IsValid"/> is true.
    /// Null when validation fails.
    /// </remarks>
    public int? DifficultyClass { get; init; }

    /// <summary>
    /// Gets the reason validation failed.
    /// </summary>
    /// <remarks>
    /// Only populated when <see cref="IsValid"/> is false.
    /// Contains a human-readable explanation of the validation failure.
    /// Null when validation succeeds.
    /// </remarks>
    public string? FailureReason { get; init; }

    // ═══════════════════════════════════════════════════════════════
    // FACTORY METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a successful validation result.
    /// </summary>
    /// <param name="dc">The difficulty class for the gathering check.</param>
    /// <returns>A successful GatherValidation with the DC for the roll.</returns>
    /// <example>
    /// <code>
    /// var validation = GatherValidation.Success(dc: 12);
    /// if (validation.IsValid)
    /// {
    ///     Console.WriteLine($"Gathering DC: {validation.DifficultyClass}");
    /// }
    /// // Output: Gathering DC: 12
    /// </code>
    /// </example>
    public static GatherValidation Success(int dc)
    {
        return new GatherValidation
        {
            IsValid = true,
            DifficultyClass = dc,
            FailureReason = null
        };
    }

    /// <summary>
    /// Creates a failed validation result.
    /// </summary>
    /// <param name="reason">The reason validation failed.</param>
    /// <returns>A failed GatherValidation with the failure reason.</returns>
    /// <remarks>
    /// Common validation failure reasons include:
    /// <list type="bullet">
    ///   <item><description>Feature is depleted.</description></item>
    ///   <item><description>Player lacks required tool.</description></item>
    ///   <item><description>Feature definition not found.</description></item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// var validation = GatherValidation.Failed("This resource has been depleted.");
    /// if (!validation.IsValid)
    /// {
    ///     Console.WriteLine(validation.FailureReason);
    /// }
    /// // Output: This resource has been depleted.
    /// </code>
    /// </example>
    public static GatherValidation Failed(string reason)
    {
        return new GatherValidation
        {
            IsValid = false,
            DifficultyClass = null,
            FailureReason = reason
        };
    }
}
