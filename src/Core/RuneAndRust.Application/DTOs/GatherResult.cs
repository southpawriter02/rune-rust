namespace RuneAndRust.Application.DTOs;

using RuneAndRust.Domain.Enums;

/// <summary>
/// Represents the result of a gathering attempt from a harvestable feature.
/// </summary>
/// <remarks>
/// <para>
/// GatherResult is an immutable record that captures all information about
/// a gathering attempt, including the dice roll details, success/failure
/// status, resources gathered, and whether the feature was depleted.
/// </para>
/// <para>
/// Use the static factory methods to create instances rather than the
/// constructor directly:
/// <list type="bullet">
///   <item><description><see cref="Success"/> - For successful gathering with resources.</description></item>
///   <item><description><see cref="Failed"/> - For failed dice rolls.</description></item>
///   <item><description><see cref="ValidationFailed"/> - For pre-roll validation failures.</description></item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Successful gather
/// var result = GatherResult.Success(
///     roll: 15, modifier: 3, total: 18, dc: 12,
///     resourceId: "iron-ore", resourceName: "Iron Ore",
///     quantity: 3, quality: ResourceQuality.Common,
///     featureDepleted: false);
///
/// // Failed gather (dice roll)
/// var result = GatherResult.Failed(roll: 8, modifier: 3, total: 11, dc: 12);
///
/// // Validation failure (no dice roll)
/// var result = GatherResult.ValidationFailed("Feature is depleted.");
/// </code>
/// </example>
public record GatherResult
{
    // ═══════════════════════════════════════════════════════════════
    // PROPERTIES
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets whether the gathering attempt succeeded.
    /// </summary>
    /// <remarks>
    /// True only when the dice roll met or exceeded the difficulty class
    /// and resources were successfully gathered.
    /// </remarks>
    public bool IsSuccess { get; init; }

    /// <summary>
    /// Gets the raw d20 roll result.
    /// </summary>
    /// <remarks>
    /// The unmodified dice roll value (1-20).
    /// Zero for validation failures where no roll was made.
    /// </remarks>
    public int Roll { get; init; }

    /// <summary>
    /// Gets the skill modifier applied to the roll.
    /// </summary>
    /// <remarks>
    /// Typically the player's Survival skill modifier.
    /// Zero for validation failures.
    /// </remarks>
    public int Modifier { get; init; }

    /// <summary>
    /// Gets the total roll result (Roll + Modifier).
    /// </summary>
    /// <remarks>
    /// The final value compared against the difficulty class.
    /// Zero for validation failures.
    /// </remarks>
    public int Total { get; init; }

    /// <summary>
    /// Gets the difficulty class that needed to be met or exceeded.
    /// </summary>
    /// <remarks>
    /// Determined by the harvestable feature's definition.
    /// Zero for validation failures.
    /// </remarks>
    public int DifficultyClass { get; init; }

    /// <summary>
    /// Gets the unique identifier of the gathered resource.
    /// </summary>
    /// <remarks>
    /// Null for failed attempts or validation failures.
    /// </remarks>
    public string? ResourceId { get; init; }

    /// <summary>
    /// Gets the display name of the gathered resource.
    /// </summary>
    /// <remarks>
    /// Null for failed attempts or validation failures.
    /// </remarks>
    public string? ResourceName { get; init; }

    /// <summary>
    /// Gets the quantity of resources gathered.
    /// </summary>
    /// <remarks>
    /// Zero for failed attempts or validation failures.
    /// </remarks>
    public int Quantity { get; init; }

    /// <summary>
    /// Gets the quality tier of the gathered resources.
    /// </summary>
    /// <remarks>
    /// <para>May be upgraded from the base quality if the roll margin was >= 10.</para>
    /// <para>Null for failed attempts or validation failures.</para>
    /// </remarks>
    public ResourceQuality? Quality { get; init; }

    /// <summary>
    /// Gets whether the harvestable feature was depleted after this gather.
    /// </summary>
    /// <remarks>
    /// If true, the feature cannot be gathered from again until it replenishes.
    /// </remarks>
    public bool FeatureDepleted { get; init; }

    /// <summary>
    /// Gets the reason for failure, if the attempt was not successful.
    /// </summary>
    /// <remarks>
    /// Contains a human-readable explanation for dice roll failures
    /// or validation failures. Null for successful attempts.
    /// </remarks>
    public string? FailureReason { get; init; }

    // ═══════════════════════════════════════════════════════════════
    // COMPUTED PROPERTIES
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets whether this result represents a validation failure (no roll was made).
    /// </summary>
    /// <remarks>
    /// Validation failures occur when pre-roll checks fail, such as:
    /// <list type="bullet">
    ///   <item><description>Feature is depleted.</description></item>
    ///   <item><description>Player lacks required tool.</description></item>
    ///   <item><description>Feature definition not found.</description></item>
    /// </list>
    /// </remarks>
    public bool IsValidationFailure => !IsSuccess && Roll == 0;

    /// <summary>
    /// Gets the roll margin (how much the roll exceeded or fell short of the DC).
    /// </summary>
    /// <remarks>
    /// <para>Positive values indicate success margin.</para>
    /// <para>Negative values indicate failure margin.</para>
    /// <para>Returns 0 for validation failures.</para>
    /// <para>A margin of 10 or more triggers a quality upgrade.</para>
    /// </remarks>
    public int Margin => Total - DifficultyClass;

    // ═══════════════════════════════════════════════════════════════
    // FACTORY METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a successful gather result.
    /// </summary>
    /// <param name="roll">The raw d20 roll (1-20).</param>
    /// <param name="modifier">The skill modifier applied (Survival).</param>
    /// <param name="total">The total roll result (roll + modifier).</param>
    /// <param name="dc">The difficulty class that was met or exceeded.</param>
    /// <param name="resourceId">The ID of the resource gathered.</param>
    /// <param name="resourceName">The display name of the resource.</param>
    /// <param name="quantity">The quantity gathered.</param>
    /// <param name="quality">The quality tier of the gathered resources.</param>
    /// <param name="featureDepleted">Whether the feature is now depleted.</param>
    /// <returns>A successful GatherResult.</returns>
    /// <example>
    /// <code>
    /// var result = GatherResult.Success(
    ///     roll: 15, modifier: 3, total: 18, dc: 12,
    ///     resourceId: "iron-ore", resourceName: "Iron Ore",
    ///     quantity: 3, quality: ResourceQuality.Common,
    ///     featureDepleted: false);
    ///
    /// Console.WriteLine($"Gathered {result.Quantity}x {result.ResourceName}");
    /// // Output: Gathered 3x Iron Ore
    /// </code>
    /// </example>
    public static GatherResult Success(
        int roll,
        int modifier,
        int total,
        int dc,
        string resourceId,
        string resourceName,
        int quantity,
        ResourceQuality quality,
        bool featureDepleted)
    {
        return new GatherResult
        {
            IsSuccess = true,
            Roll = roll,
            Modifier = modifier,
            Total = total,
            DifficultyClass = dc,
            ResourceId = resourceId,
            ResourceName = resourceName,
            Quantity = quantity,
            Quality = quality,
            FeatureDepleted = featureDepleted,
            FailureReason = null
        };
    }

    /// <summary>
    /// Creates a failed gather result (dice roll failed to meet DC).
    /// </summary>
    /// <param name="roll">The raw d20 roll (1-20).</param>
    /// <param name="modifier">The skill modifier applied.</param>
    /// <param name="total">The total roll result (roll + modifier).</param>
    /// <param name="dc">The difficulty class that was not met.</param>
    /// <returns>A failed GatherResult with dice roll details.</returns>
    /// <example>
    /// <code>
    /// var result = GatherResult.Failed(roll: 8, modifier: 3, total: 11, dc: 12);
    /// Console.WriteLine($"{result.GetRollDisplay()}");
    /// // Output: 1d20 (8) +3 = 11 vs DC 12
    /// </code>
    /// </example>
    public static GatherResult Failed(int roll, int modifier, int total, int dc)
    {
        return new GatherResult
        {
            IsSuccess = false,
            Roll = roll,
            Modifier = modifier,
            Total = total,
            DifficultyClass = dc,
            ResourceId = null,
            ResourceName = null,
            Quantity = 0,
            Quality = null,
            FeatureDepleted = false,
            FailureReason = "Gathering check failed."
        };
    }

    /// <summary>
    /// Creates a validation failure result (pre-roll validation failed).
    /// </summary>
    /// <param name="reason">The reason for the validation failure.</param>
    /// <returns>A validation-failed GatherResult with no dice roll data.</returns>
    /// <remarks>
    /// Use this when validation fails before a dice roll can be made, such as:
    /// <list type="bullet">
    ///   <item><description>Feature is depleted.</description></item>
    ///   <item><description>Player lacks required tool.</description></item>
    ///   <item><description>Feature or resource definition not found.</description></item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// var result = GatherResult.ValidationFailed("You need a pickaxe to gather this.");
    /// Console.WriteLine(result.FailureReason);
    /// // Output: You need a pickaxe to gather this.
    /// </code>
    /// </example>
    public static GatherResult ValidationFailed(string reason)
    {
        return new GatherResult
        {
            IsSuccess = false,
            Roll = 0,
            Modifier = 0,
            Total = 0,
            DifficultyClass = 0,
            ResourceId = null,
            ResourceName = null,
            Quantity = 0,
            Quality = null,
            FeatureDepleted = false,
            FailureReason = reason
        };
    }

    // ═══════════════════════════════════════════════════════════════
    // DISPLAY METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets a formatted string showing the dice roll details.
    /// </summary>
    /// <returns>
    /// Formatted string like "1d20 (15) +3 = 18 vs DC 12".
    /// Empty string for validation failures.
    /// </returns>
    /// <example>
    /// <code>
    /// var result = GatherResult.Success(15, 3, 18, 12, "iron-ore", "Iron Ore", 3, ResourceQuality.Common, false);
    /// Console.WriteLine(result.GetRollDisplay());
    /// // Output: 1d20 (15) +3 = 18 vs DC 12
    /// </code>
    /// </example>
    public string GetRollDisplay()
    {
        // No dice roll for validation failures
        if (IsValidationFailure)
        {
            return string.Empty;
        }

        // Format the modifier with appropriate sign
        var modifierSign = Modifier >= 0 ? "+" : "";
        return $"1d20 ({Roll}) {modifierSign}{Modifier} = {Total} vs DC {DifficultyClass}";
    }

    /// <summary>
    /// Gets a formatted string describing the gathering outcome.
    /// </summary>
    /// <returns>Human-readable outcome description.</returns>
    /// <example>
    /// <code>
    /// // Successful gather
    /// var success = GatherResult.Success(15, 3, 18, 12, "iron-ore", "Iron Ore", 3, ResourceQuality.Fine, true);
    /// Console.WriteLine(success.GetOutcomeDisplay());
    /// // Output: Gathered 3x Iron Ore (Fine)
    /// //         The resource has been depleted.
    ///
    /// // Failed gather
    /// var failure = GatherResult.Failed(8, 3, 11, 12);
    /// Console.WriteLine(failure.GetOutcomeDisplay());
    /// // Output: Gathering check failed.
    /// </code>
    /// </example>
    public string GetOutcomeDisplay()
    {
        // Return failure reason for unsuccessful attempts
        if (!IsSuccess)
        {
            return FailureReason ?? "Gathering failed.";
        }

        // Format quality display (omit for Common quality)
        var qualityDisplay = Quality == ResourceQuality.Common
            ? string.Empty
            : $" ({Quality})";

        // Add depletion notice if applicable
        var depletedNotice = FeatureDepleted
            ? "\nThe resource has been depleted."
            : string.Empty;

        return $"Gathered {Quantity}x {ResourceName}{qualityDisplay}{depletedNotice}";
    }
}
