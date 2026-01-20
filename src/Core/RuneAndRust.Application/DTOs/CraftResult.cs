using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Application.DTOs;

/// <summary>
/// Represents the result of a crafting attempt.
/// </summary>
/// <remarks>
/// <para>
/// CraftResult captures the complete outcome of attempting to craft an item,
/// including the dice roll details, success/failure status, and consequences.
/// </para>
/// <para>
/// There are three possible outcome types:
/// <list type="bullet">
///   <item><description>
///     <b>Validation Failure</b> - Crafting was not attempted due to failed prerequisites
///     (no station, unknown recipe, missing resources). Use <see cref="ValidationFailed"/>.
///   </description></item>
///   <item><description>
///     <b>Dice Roll Failure</b> - Dice check failed. Resources are partially lost.
///     Use <see cref="Failed"/>.
///   </description></item>
///   <item><description>
///     <b>Success</b> - Crafting succeeded. All resources consumed, item created.
///     Use <see cref="Success"/>.
///   </description></item>
/// </list>
/// </para>
/// <para>
/// Factory methods ensure consistent construction and proper initialization
/// of computed properties.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Check result and handle outcomes
/// if (result.IsSuccess)
/// {
///     Console.WriteLine($"Crafted {result.CraftedItem!.Name} ({result.Quality} quality)");
///     Console.WriteLine(result.GetRollDisplay());
///     // Output: d20(15) +4 = 19 vs DC 12
/// }
/// else if (result.WasDiceRollFailure)
/// {
///     Console.WriteLine($"Failed! Lost {result.TotalResourcesLost} resources.");
///     var failType = result.IsBadFailure ? "BAD FAILURE" : "CLOSE FAILURE";
///     Console.WriteLine($"{failType}: {result.FailureReason}");
/// }
/// else
/// {
///     // Validation failure - no dice was rolled
///     Console.WriteLine(result.FailureReason);
/// }
/// </code>
/// </example>
public sealed record CraftResult
{
    // ═══════════════════════════════════════════════════════════════
    // PROPERTIES
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets whether crafting succeeded.
    /// </summary>
    public bool IsSuccess { get; init; }

    /// <summary>
    /// Gets the d20 roll value (1-20).
    /// </summary>
    /// <remarks>
    /// Zero if crafting was not attempted (validation failure).
    /// </remarks>
    public int Roll { get; init; }

    /// <summary>
    /// Gets the skill modifier applied to the roll.
    /// </summary>
    /// <remarks>
    /// The modifier comes from the player's skill level in the
    /// crafting station's associated skill.
    /// </remarks>
    public int Modifier { get; init; }

    /// <summary>
    /// Gets the total roll result (Roll + Modifier).
    /// </summary>
    public int Total { get; init; }

    /// <summary>
    /// Gets the difficulty class of the recipe.
    /// </summary>
    public int DifficultyClass { get; init; }

    /// <summary>
    /// Gets the created item (null on failure).
    /// </summary>
    /// <remarks>
    /// Only populated on successful crafting. Contains the fully
    /// configured item ready for inventory.
    /// </remarks>
    public Item? CraftedItem { get; init; }

    /// <summary>
    /// Gets the quality tier of the crafted item (null on failure).
    /// </summary>
    /// <remarks>
    /// Quality is determined by the roll:
    /// <list type="bullet">
    ///   <item><description>Natural 20 → Legendary</description></item>
    ///   <item><description>Margin >= 10 → Masterwork</description></item>
    ///   <item><description>Margin >= 5 → Fine</description></item>
    ///   <item><description>Otherwise → Standard</description></item>
    /// </list>
    /// </remarks>
    public CraftedItemQuality? Quality { get; init; }

    /// <summary>
    /// Gets resources lost on failure (null on success or validation failure).
    /// </summary>
    /// <remarks>
    /// Loss percentage depends on how badly the roll failed:
    /// <list type="bullet">
    ///   <item><description>Close failure (margin >= -5): 25% loss</description></item>
    ///   <item><description>Bad failure (margin &lt; -5): 50% loss</description></item>
    /// </list>
    /// </remarks>
    public IReadOnlyList<ResourceLoss>? ResourcesLost { get; init; }

    /// <summary>
    /// Gets the reason for failure (null on success).
    /// </summary>
    public string? FailureReason { get; init; }

    // ═══════════════════════════════════════════════════════════════
    // COMPUTED PROPERTIES
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets whether this was a dice roll failure vs validation failure.
    /// </summary>
    /// <remarks>
    /// True when crafting was attempted but the dice check failed.
    /// False for both success and validation failures.
    /// </remarks>
    public bool WasDiceRollFailure => !IsSuccess && Roll > 0;

    /// <summary>
    /// Gets the margin (how much the roll beat or missed the DC).
    /// </summary>
    /// <remarks>
    /// Positive values indicate success margin.
    /// Negative values indicate failure margin.
    /// </remarks>
    public int Margin => Total - DifficultyClass;

    /// <summary>
    /// Gets whether this was a close failure (within 5 of DC).
    /// </summary>
    /// <remarks>
    /// Close failures result in 25% resource loss.
    /// </remarks>
    public bool IsCloseFailure => WasDiceRollFailure && Margin >= -5;

    /// <summary>
    /// Gets whether this was a bad failure (more than 5 below DC).
    /// </summary>
    /// <remarks>
    /// Bad failures result in 50% resource loss.
    /// </remarks>
    public bool IsBadFailure => WasDiceRollFailure && Margin < -5;

    /// <summary>
    /// Gets the total resources lost count.
    /// </summary>
    public int TotalResourcesLost => ResourcesLost?.Sum(r => r.Amount) ?? 0;

    /// <summary>
    /// Gets whether this was a natural 20 roll.
    /// </summary>
    public bool IsNatural20 => Roll == 20;

    // ═══════════════════════════════════════════════════════════════
    // FACTORY METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a successful crafting result.
    /// </summary>
    /// <param name="roll">The d20 roll value.</param>
    /// <param name="modifier">The skill modifier.</param>
    /// <param name="total">The total roll (roll + modifier).</param>
    /// <param name="dc">The difficulty class.</param>
    /// <param name="item">The crafted item.</param>
    /// <param name="quality">The item quality.</param>
    /// <returns>A successful craft result.</returns>
    /// <example>
    /// <code>
    /// var result = CraftResult.Success(
    ///     roll: 15,
    ///     modifier: 4,
    ///     total: 19,
    ///     dc: 12,
    ///     item: craftedSword,
    ///     quality: CraftedItemQuality.Fine);
    /// </code>
    /// </example>
    public static CraftResult Success(
        int roll,
        int modifier,
        int total,
        int dc,
        Item item,
        CraftedItemQuality quality)
    {
        return new CraftResult
        {
            IsSuccess = true,
            Roll = roll,
            Modifier = modifier,
            Total = total,
            DifficultyClass = dc,
            CraftedItem = item,
            Quality = quality,
            ResourcesLost = null,
            FailureReason = null
        };
    }

    /// <summary>
    /// Creates a failed crafting result (dice check failed).
    /// </summary>
    /// <param name="roll">The d20 roll value.</param>
    /// <param name="modifier">The skill modifier.</param>
    /// <param name="total">The total roll.</param>
    /// <param name="dc">The difficulty class.</param>
    /// <param name="losses">Resources lost due to failure.</param>
    /// <returns>A failed craft result.</returns>
    /// <example>
    /// <code>
    /// var losses = new List&lt;ResourceLoss&gt;
    /// {
    ///     new("iron-ore", "Iron Ore", 2),
    ///     new("leather", "Leather", 1)
    /// };
    /// var result = CraftResult.Failed(
    ///     roll: 5,
    ///     modifier: 2,
    ///     total: 7,
    ///     dc: 12,
    ///     losses: losses);
    /// </code>
    /// </example>
    public static CraftResult Failed(
        int roll,
        int modifier,
        int total,
        int dc,
        IReadOnlyList<ResourceLoss> losses)
    {
        return new CraftResult
        {
            IsSuccess = false,
            Roll = roll,
            Modifier = modifier,
            Total = total,
            DifficultyClass = dc,
            CraftedItem = null,
            Quality = null,
            ResourcesLost = losses,
            FailureReason = "Crafting failed. Some materials were lost."
        };
    }

    /// <summary>
    /// Creates a result for validation failure (no dice roll).
    /// </summary>
    /// <param name="reason">The validation failure reason.</param>
    /// <returns>A validation failed result.</returns>
    /// <remarks>
    /// Use this when crafting cannot be attempted because prerequisites
    /// are not met (e.g., no station, unknown recipe, missing resources).
    /// No dice is rolled, and no resources are lost.
    /// </remarks>
    /// <example>
    /// <code>
    /// var result = CraftResult.ValidationFailed("You need to be at a crafting station.");
    /// // result.Roll == 0, result.WasDiceRollFailure == false
    /// </code>
    /// </example>
    public static CraftResult ValidationFailed(string reason)
    {
        return new CraftResult
        {
            IsSuccess = false,
            Roll = 0,
            Modifier = 0,
            Total = 0,
            DifficultyClass = 0,
            CraftedItem = null,
            Quality = null,
            ResourcesLost = null,
            FailureReason = reason
        };
    }

    // ═══════════════════════════════════════════════════════════════
    // DISPLAY METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets a formatted string showing the dice roll details.
    /// </summary>
    /// <returns>A string like "d20(15) +4 = 19 vs DC 12", or empty if no roll.</returns>
    /// <example>
    /// <code>
    /// var result = CraftResult.Success(15, 4, 19, 12, item, quality);
    /// Console.WriteLine(result.GetRollDisplay());
    /// // Output: d20(15) +4 = 19 vs DC 12
    ///
    /// var failed = CraftResult.ValidationFailed("No station.");
    /// Console.WriteLine(failed.GetRollDisplay());
    /// // Output: (empty string)
    /// </code>
    /// </example>
    public string GetRollDisplay()
    {
        if (Roll == 0)
        {
            return string.Empty;
        }

        var modifierSign = Modifier >= 0 ? "+" : "";
        return $"d20({Roll}) {modifierSign}{Modifier} = {Total} vs DC {DifficultyClass}";
    }

    /// <summary>
    /// Gets a formatted string showing resources lost.
    /// </summary>
    /// <returns>A multi-line string listing lost resources, or empty if none.</returns>
    /// <example>
    /// <code>
    /// if (result.ResourcesLost?.Count > 0)
    /// {
    ///     Console.WriteLine("Resources lost:");
    ///     Console.WriteLine(result.GetResourceLossDisplay());
    /// }
    /// </code>
    /// </example>
    public string GetResourceLossDisplay()
    {
        if (ResourcesLost is null || ResourcesLost.Count == 0)
        {
            return string.Empty;
        }

        return string.Join("\n", ResourcesLost.Select(r => $"  - {r.ToDisplayString()}"));
    }
}
