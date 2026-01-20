namespace RuneAndRust.Application.Events;

using RuneAndRust.Domain.Enums;

/// <summary>
/// Event raised when a player attempts to gather from a harvestable feature.
/// </summary>
/// <remarks>
/// <para>
/// This event is raised after the dice roll, regardless of success or failure.
/// It captures all the roll details for statistics tracking, achievement systems,
/// and combat log display.
/// </para>
/// <para>
/// The event is published before any inventory changes or feature depletion occurs.
/// </para>
/// </remarks>
/// <param name="PlayerId">The unique identifier of the player attempting to gather.</param>
/// <param name="FeatureId">The unique identifier of the harvestable feature instance.</param>
/// <param name="FeatureDefinitionId">The definition ID of the harvestable feature (e.g., "iron-ore-vein").</param>
/// <param name="Roll">The raw d20 roll result (1-20).</param>
/// <param name="Modifier">The skill modifier applied to the roll (Survival skill).</param>
/// <param name="Total">The total roll result (Roll + Modifier).</param>
/// <param name="DifficultyClass">The DC that needed to be met or exceeded.</param>
/// <param name="Success">Whether the gathering check succeeded (Total >= DC).</param>
/// <example>
/// <code>
/// // Subscribe to gathering attempts
/// eventBus.Subscribe&lt;GatherAttemptedEvent&gt;(e =>
/// {
///     if (e.Success)
///     {
///         logger.LogInformation("{Player} succeeded at gathering (margin: +{Margin})",
///             e.PlayerId, e.Margin);
///     }
///     else
///     {
///         logger.LogInformation("{Player} failed at gathering (margin: {Margin})",
///             e.PlayerId, e.Margin);
///     }
/// });
///
/// // Publish the event
/// eventBus.Publish(new GatherAttemptedEvent(
///     PlayerId: player.Id,
///     FeatureId: feature.Id,
///     FeatureDefinitionId: feature.DefinitionId,
///     Roll: 15,
///     Modifier: 3,
///     Total: 18,
///     DifficultyClass: 12,
///     Success: true));
/// </code>
/// </example>
public record GatherAttemptedEvent(
    Guid PlayerId,
    Guid FeatureId,
    string FeatureDefinitionId,
    int Roll,
    int Modifier,
    int Total,
    int DifficultyClass,
    bool Success)
{
    /// <summary>
    /// Gets the timestamp when the event occurred.
    /// </summary>
    /// <remarks>
    /// Automatically set to the current UTC time when the event is created.
    /// </remarks>
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets the margin by which the roll succeeded or failed.
    /// </summary>
    /// <remarks>
    /// <para>Positive values indicate success margin.</para>
    /// <para>Negative values indicate failure margin.</para>
    /// <para>A margin of 10 or more on a successful roll triggers a quality upgrade.</para>
    /// </remarks>
    public int Margin => Total - DifficultyClass;

    /// <summary>
    /// Gets whether this was a critical success (natural 20).
    /// </summary>
    /// <remarks>
    /// Critical success may trigger special effects or achievements.
    /// </remarks>
    public bool IsCriticalSuccess => Roll == 20;

    /// <summary>
    /// Gets whether this was a critical failure (natural 1).
    /// </summary>
    /// <remarks>
    /// Critical failure may trigger special effects or penalties.
    /// </remarks>
    public bool IsCriticalFailure => Roll == 1;
}

/// <summary>
/// Event raised when a player successfully gathers resources from a feature.
/// </summary>
/// <remarks>
/// <para>
/// This event is only raised on successful gathering attempts. It captures
/// the resources added to the player's inventory for statistics tracking,
/// achievement systems, and UI updates.
/// </para>
/// <para>
/// The event is published after the resources have been added to inventory
/// and the feature has been depleted.
/// </para>
/// </remarks>
/// <param name="PlayerId">The unique identifier of the player who gathered resources.</param>
/// <param name="ResourceId">The definition ID of the gathered resource (e.g., "iron-ore").</param>
/// <param name="ResourceName">The display name of the gathered resource.</param>
/// <param name="Quantity">The quantity of resources gathered.</param>
/// <param name="Quality">The quality tier of the gathered resources.</param>
/// <param name="FeatureId">The unique identifier of the harvestable feature instance.</param>
/// <param name="FeatureDepleted">Whether the feature was fully depleted by this gather.</param>
/// <example>
/// <code>
/// // Subscribe to successful gathers for statistics
/// eventBus.Subscribe&lt;ResourceGatheredEvent&gt;(e =>
/// {
///     statistics.TrackResourceGathered(e.PlayerId, e.ResourceId, e.Quantity, e.Quality);
///
///     if (e.FeatureDepleted)
///     {
///         logger.LogInformation("Feature {FeatureId} was depleted", e.FeatureId);
///     }
/// });
///
/// // Publish the event
/// eventBus.Publish(new ResourceGatheredEvent(
///     PlayerId: player.Id,
///     ResourceId: "iron-ore",
///     ResourceName: "Iron Ore",
///     Quantity: 3,
///     Quality: ResourceQuality.Common,
///     FeatureId: feature.Id,
///     FeatureDepleted: false));
/// </code>
/// </example>
public record ResourceGatheredEvent(
    Guid PlayerId,
    string ResourceId,
    string ResourceName,
    int Quantity,
    ResourceQuality Quality,
    Guid FeatureId,
    bool FeatureDepleted)
{
    /// <summary>
    /// Gets the timestamp when the event occurred.
    /// </summary>
    /// <remarks>
    /// Automatically set to the current UTC time when the event is created.
    /// </remarks>
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Calculates the total value of the gathered resources.
    /// </summary>
    /// <param name="baseValue">The base value of a single unit of the resource.</param>
    /// <returns>Total value (baseValue * quantity * quality multiplier).</returns>
    /// <remarks>
    /// <para>
    /// The quality multiplier is applied from <see cref="ResourceQualityExtensions.GetValueMultiplier"/>:
    /// <list type="bullet">
    ///   <item><description>Common: 1.0x</description></item>
    ///   <item><description>Fine: 1.5x</description></item>
    ///   <item><description>Rare: 3.0x</description></item>
    ///   <item><description>Legendary: 10.0x</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var resourceEvent = new ResourceGatheredEvent(
    ///     playerId, "iron-ore", "Iron Ore", 3, ResourceQuality.Fine, featureId, false);
    ///
    /// int baseValue = 5; // Base value of iron ore
    /// int totalValue = resourceEvent.GetTotalValue(baseValue);
    /// // totalValue = (int)(5 * 3 * 1.5) = 22 gold
    /// </code>
    /// </example>
    public int GetTotalValue(int baseValue)
    {
        return (int)(baseValue * Quantity * Quality.GetValueMultiplier());
    }
}

/// <summary>
/// Event raised when a harvestable feature replenishes.
/// </summary>
/// <remarks>
/// <para>
/// This event is raised when a depleted feature's replenishment timer expires
/// and the feature is restored to a harvestable state.
/// </para>
/// </remarks>
/// <param name="FeatureId">The unique identifier of the harvestable feature instance.</param>
/// <param name="FeatureDefinitionId">The definition ID of the feature.</param>
/// <param name="RoomId">The unique identifier of the room containing the feature.</param>
/// <param name="NewQuantity">The new quantity available for harvesting.</param>
/// <example>
/// <code>
/// eventBus.Publish(new FeatureReplenishedEvent(
///     FeatureId: feature.Id,
///     FeatureDefinitionId: feature.DefinitionId,
///     RoomId: room.Id,
///     NewQuantity: 5));
/// </code>
/// </example>
public record FeatureReplenishedEvent(
    Guid FeatureId,
    string FeatureDefinitionId,
    Guid RoomId,
    int NewQuantity)
{
    /// <summary>
    /// Gets the timestamp when the event occurred.
    /// </summary>
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;
}
