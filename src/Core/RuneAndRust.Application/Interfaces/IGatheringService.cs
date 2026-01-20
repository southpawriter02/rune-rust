using RuneAndRust.Application.DTOs;
using RuneAndRust.Domain.Entities;

namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Service for gathering resources from harvestable features.
/// </summary>
/// <remarks>
/// <para>
/// The gathering service handles the complete resource harvesting flow:
/// validation, dice rolling, quantity determination, quality upgrades,
/// inventory updates, and feature depletion.
/// </para>
/// <para>
/// Gathering uses the Survival skill. The player rolls d20 + Survival modifier
/// against the feature's Difficulty Class (DC). Success yields resources,
/// with high rolls (margin >= 10) potentially upgrading the quality tier.
/// </para>
/// <para>
/// Key responsibilities:
/// <list type="bullet">
///   <item><description>Retrieve available harvestable features from rooms</description></item>
///   <item><description>Validate gathering attempts (tool requirements, depletion state)</description></item>
///   <item><description>Execute gathering dice checks and calculate results</description></item>
///   <item><description>Determine resource quantity and quality</description></item>
///   <item><description>Update player inventory with gathered resources</description></item>
///   <item><description>Deplete features and manage replenishment timers</description></item>
///   <item><description>Publish gathering-related events</description></item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Basic gathering flow
/// var features = gatheringService.GetHarvestableFeatures(room);
/// var feature = features.FirstOrDefault(f => f.Name.Contains("ore"));
///
/// if (feature is not null)
/// {
///     var validation = gatheringService.CanGather(player, feature);
///     if (validation.IsValid)
///     {
///         var result = gatheringService.Gather(player, feature);
///         if (result.IsSuccess)
///         {
///             Console.WriteLine($"Gathered {result.Quantity}x {result.ResourceName}!");
///         }
///         else
///         {
///             Console.WriteLine($"Failed to gather: {result.GetRollDisplay()}");
///         }
///     }
///     else
///     {
///         Console.WriteLine(validation.FailureReason);
///     }
/// }
/// </code>
/// </example>
public interface IGatheringService
{
    // ═══════════════════════════════════════════════════════════════
    // FEATURE ACCESS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets all harvestable features in a room that are not depleted.
    /// </summary>
    /// <param name="room">The room to check.</param>
    /// <returns>A read-only list of available harvestable features.</returns>
    /// <exception cref="ArgumentNullException">Thrown when room is null.</exception>
    /// <remarks>
    /// <para>
    /// Returns only features where <see cref="HarvestableFeature.IsDepleted"/> is false.
    /// Features awaiting replenishment are excluded.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var features = gatheringService.GetHarvestableFeatures(currentRoom);
    /// foreach (var feature in features)
    /// {
    ///     var dc = gatheringService.GetGatherDC(feature);
    ///     Console.WriteLine($"[{feature.Name}] - DC {dc}, {feature.RemainingQuantity} remaining");
    /// }
    /// </code>
    /// </example>
    IReadOnlyList<HarvestableFeature> GetHarvestableFeatures(Room room);

    // ═══════════════════════════════════════════════════════════════
    // VALIDATION
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Validates if a player can gather from a specific feature.
    /// </summary>
    /// <param name="player">The player attempting to gather.</param>
    /// <param name="feature">The feature to gather from.</param>
    /// <returns>A validation result indicating if gathering can proceed.</returns>
    /// <exception cref="ArgumentNullException">Thrown when player or feature is null.</exception>
    /// <remarks>
    /// <para>
    /// Validation checks performed:
    /// <list type="bullet">
    ///   <item><description>Feature is not depleted</description></item>
    ///   <item><description>Feature definition exists in provider</description></item>
    ///   <item><description>Player has required tool (if any)</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// If validation passes, the returned <see cref="GatherValidation.DifficultyClass"/>
    /// contains the DC for the gathering check.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var validation = gatheringService.CanGather(player, oreVein);
    /// if (validation.IsValid)
    /// {
    ///     Console.WriteLine($"Ready to gather! DC: {validation.DifficultyClass}");
    /// }
    /// else
    /// {
    ///     Console.WriteLine($"Cannot gather: {validation.FailureReason}");
    /// }
    /// </code>
    /// </example>
    GatherValidation CanGather(Player player, HarvestableFeature feature);

    // ═══════════════════════════════════════════════════════════════
    // GATHERING EXECUTION
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Attempts to gather resources from a harvestable feature.
    /// </summary>
    /// <param name="player">The player attempting to gather.</param>
    /// <param name="feature">The feature to gather from.</param>
    /// <returns>A result containing the outcome of the gathering attempt.</returns>
    /// <exception cref="ArgumentNullException">Thrown when player or feature is null.</exception>
    /// <remarks>
    /// <para>
    /// This method performs the complete gathering flow:
    /// <list type="number">
    ///   <item><description>Validates the attempt using <see cref="CanGather"/></description></item>
    ///   <item><description>Rolls d20 + Survival modifier vs DC</description></item>
    ///   <item><description>Publishes <see cref="Events.GatherAttemptedEvent"/></description></item>
    ///   <item><description>On success: determines quantity within feature's min/max range</description></item>
    ///   <item><description>On success with margin >= 10: upgrades resource quality by one tier</description></item>
    ///   <item><description>Adds resources to player inventory</description></item>
    ///   <item><description>Depletes feature by gathered quantity</description></item>
    ///   <item><description>Sets replenishment timer if feature is depleted and replenishes</description></item>
    ///   <item><description>Publishes <see cref="Events.ResourceGatheredEvent"/> on success</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// The returned <see cref="GatherResult"/> contains:
    /// <list type="bullet">
    ///   <item><description>Dice roll details (roll, modifier, total, DC)</description></item>
    ///   <item><description>Resource information (id, name, quantity, quality)</description></item>
    ///   <item><description>Feature depletion state</description></item>
    ///   <item><description>Failure reason if unsuccessful</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var result = gatheringService.Gather(player, oreVein);
    ///
    /// Console.WriteLine(result.GetRollDisplay());
    /// // Output: 1d20 (15) +3 = 18 vs DC 12
    ///
    /// if (result.IsSuccess)
    /// {
    ///     Console.WriteLine($"Gathered {result.Quantity}x {result.ResourceName} ({result.Quality})!");
    ///     if (result.FeatureDepleted)
    ///     {
    ///         Console.WriteLine("The resource has been depleted.");
    ///     }
    /// }
    /// else if (result.IsValidationFailure)
    /// {
    ///     Console.WriteLine($"Cannot gather: {result.FailureReason}");
    /// }
    /// else
    /// {
    ///     Console.WriteLine("Failed to gather any resources.");
    /// }
    /// </code>
    /// </example>
    GatherResult Gather(Player player, HarvestableFeature feature);

    // ═══════════════════════════════════════════════════════════════
    // GATHERING INFORMATION
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the difficulty class for gathering from a feature.
    /// </summary>
    /// <param name="feature">The harvestable feature.</param>
    /// <returns>The DC, or 0 if the feature definition is not found.</returns>
    /// <exception cref="ArgumentNullException">Thrown when feature is null.</exception>
    /// <remarks>
    /// <para>
    /// Looks up the feature's definition to retrieve its configured difficulty class.
    /// Returns 0 if the definition cannot be found (indicating a configuration error).
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var dc = gatheringService.GetGatherDC(oreVein);
    /// var modifier = gatheringService.GetGatherModifier(player);
    /// Console.WriteLine($"Gathering check: 1d20 + {modifier} vs DC {dc}");
    /// </code>
    /// </example>
    int GetGatherDC(HarvestableFeature feature);

    /// <summary>
    /// Gets the player's gathering skill modifier.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <returns>The Survival skill modifier.</returns>
    /// <exception cref="ArgumentNullException">Thrown when player is null.</exception>
    /// <remarks>
    /// <para>
    /// Gathering uses the Survival skill. This method retrieves the player's
    /// Survival skill modifier which is added to the d20 roll.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var modifier = gatheringService.GetGatherModifier(player);
    /// Console.WriteLine($"Your Survival modifier: {modifier:+#;-#;0}");
    /// </code>
    /// </example>
    int GetGatherModifier(Player player);

    // ═══════════════════════════════════════════════════════════════
    // REPLENISHMENT
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Processes replenishment for all harvestable features in a room.
    /// </summary>
    /// <param name="room">The room containing features.</param>
    /// <param name="currentTurn">The current game turn number.</param>
    /// <returns>A list of features that were replenished.</returns>
    /// <exception cref="ArgumentNullException">Thrown when room is null.</exception>
    /// <remarks>
    /// <para>
    /// Call this at the start of each turn to restore depleted features
    /// whose replenishment timers have expired.
    /// </para>
    /// <para>
    /// For each feature that replenishes:
    /// <list type="bullet">
    ///   <item><description>Checks if <see cref="HarvestableFeature.ShouldReplenish"/> returns true</description></item>
    ///   <item><description>Restores feature with random quantity within definition's range</description></item>
    ///   <item><description>Clears the replenishment timer</description></item>
    ///   <item><description>Publishes <see cref="Events.FeatureReplenishedEvent"/></description></item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Call each turn to process replenishment
    /// var replenished = gatheringService.ProcessReplenishment(room, gameState.CurrentTurn);
    /// foreach (var feature in replenished)
    /// {
    ///     Console.WriteLine($"The {feature.Name} has regrown!");
    /// }
    /// </code>
    /// </example>
    IReadOnlyList<HarvestableFeature> ProcessReplenishment(Room room, int currentTurn);
}
