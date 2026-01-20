namespace RuneAndRust.Application.DTOs;

/// <summary>
/// Represents a resource the player is missing for crafting.
/// </summary>
/// <remarks>
/// <para>
/// Used by the crafting service to provide detailed feedback about which resources
/// the player needs to gather before they can craft a recipe. Unlike
/// <see cref="MissingIngredient"/>, this record tracks both the total needed
/// quantity and the current amount the player has.
/// </para>
/// <para>
/// The <see cref="Shortage"/> computed property calculates how many more
/// of this resource are required.
/// </para>
/// </remarks>
/// <param name="ResourceId">The unique identifier of the missing resource.</param>
/// <param name="Name">The display name of the resource.</param>
/// <param name="Needed">The total quantity required by the recipe.</param>
/// <param name="Have">The quantity the player currently has in inventory.</param>
/// <example>
/// <code>
/// // Check missing resources before crafting
/// var missing = new MissingResource("iron-ore", "Iron Ore", 5, 2);
/// Console.WriteLine($"Need {missing.Shortage} more {missing.Name}");
/// // Output: Need 3 more Iron Ore
///
/// // Display in validation result
/// Console.WriteLine(missing.ToDisplayString());
/// // Output: Iron Ore: need 5, have 2
/// </code>
/// </example>
public sealed record MissingResource(
    string ResourceId,
    string Name,
    int Needed,
    int Have)
{
    /// <summary>
    /// Gets how many more of this resource are needed.
    /// </summary>
    /// <remarks>
    /// Always returns a non-negative value. If the player somehow has more
    /// than needed (shouldn't happen for missing resources), returns 0.
    /// </remarks>
    public int Shortage => Math.Max(0, Needed - Have);

    /// <summary>
    /// Gets a formatted display string showing the shortage.
    /// </summary>
    /// <returns>A string in the format "Name: need X, have Y".</returns>
    /// <example>
    /// <code>
    /// var missing = new MissingResource("iron-ore", "Iron Ore", 5, 2);
    /// Console.WriteLine(missing.ToDisplayString());
    /// // Output: Iron Ore: need 5, have 2
    /// </code>
    /// </example>
    public string ToDisplayString() => $"{Name}: need {Needed}, have {Have}";
}

/// <summary>
/// Represents resource loss from a failed crafting attempt.
/// </summary>
/// <remarks>
/// <para>
/// When a crafting attempt fails, some portion of the required resources
/// is lost as a consequence. The loss amount depends on how badly the
/// roll failed:
/// <list type="bullet">
///   <item><description>Close failure (margin >= -5): 25% of each ingredient (minimum 1)</description></item>
///   <item><description>Bad failure (margin &lt; -5): 50% of each ingredient (minimum 1)</description></item>
/// </list>
/// </para>
/// <para>
/// This record captures the loss for a single resource type. A failed craft
/// typically results in a list of <see cref="ResourceLoss"/> entries, one
/// for each ingredient in the recipe.
/// </para>
/// </remarks>
/// <param name="ResourceId">The unique identifier of the lost resource.</param>
/// <param name="Name">The display name of the resource.</param>
/// <param name="Amount">The quantity lost.</param>
/// <example>
/// <code>
/// // Calculate and display resource loss
/// var loss = new ResourceLoss("iron-ore", "Iron Ore", 2);
/// Console.WriteLine($"Lost: {loss.ToDisplayString()}");
/// // Output: Lost: Iron Ore x2
/// </code>
/// </example>
public sealed record ResourceLoss(
    string ResourceId,
    string Name,
    int Amount)
{
    /// <summary>
    /// Gets a formatted display string showing the loss.
    /// </summary>
    /// <returns>A string in the format "Name xAmount".</returns>
    /// <example>
    /// <code>
    /// var loss = new ResourceLoss("iron-ore", "Iron Ore", 2);
    /// Console.WriteLine(loss.ToDisplayString());
    /// // Output: Iron Ore x2
    /// </code>
    /// </example>
    public string ToDisplayString() => $"{Name} x{Amount}";
}
