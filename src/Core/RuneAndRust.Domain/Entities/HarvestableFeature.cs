using RuneAndRust.Domain.Definitions;
using RuneAndRust.Domain.Interfaces;

namespace RuneAndRust.Domain.Entities;

/// <summary>
/// Represents an instance of a harvestable feature placed in a room.
/// </summary>
/// <remarks>
/// <para>
/// Harvestable features are runtime instances created from
/// <see cref="HarvestableFeatureDefinition"/> templates. Each instance tracks
/// its own remaining quantity and replenishment state, allowing the same
/// feature definition to exist in multiple rooms with independent state.
/// </para>
/// <para>
/// Key features:
/// <list type="bullet">
///   <item><description>Tracks remaining quantity for harvest depletion</description></item>
///   <item><description>Supports replenishment timer for renewable resources</description></item>
///   <item><description>Maintains reference to definition for game logic</description></item>
///   <item><description>Interactable via "gather" verb</description></item>
/// </list>
/// </para>
/// <para>
/// Lifecycle:
/// <list type="number">
///   <item><description>Created from definition with random or specified quantity</description></item>
///   <item><description>Player gathers resources, reducing RemainingQuantity</description></item>
///   <item><description>When depleted, replenish timer is set (if feature replenishes)</description></item>
///   <item><description>After replenish turns pass, feature restores to initial quantity</description></item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Create a feature instance with specific quantity
/// var feature = HarvestableFeature.Create(definition, initialQuantity: 5);
///
/// // Or create with random quantity within definition's range
/// var randomFeature = HarvestableFeature.CreateWithRandomQuantity(definition);
///
/// // Harvest from the feature
/// feature.Harvest(2);
/// Console.WriteLine(feature.RemainingQuantity); // 3
///
/// // Check if depleted
/// if (feature.IsDepleted)
/// {
///     feature.SetReplenishTimer(currentTurn: 100, replenishTurns: 50);
/// }
/// </code>
/// </example>
public class HarvestableFeature : IEntity
{
    // ═══════════════════════════════════════════════════════════════
    // PROPERTIES
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the unique identifier for this feature instance.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Gets the ID of the definition this feature was created from.
    /// </summary>
    /// <remarks>
    /// Use this to look up the full definition from the provider
    /// when you need access to DC, resource ID, tool requirements, etc.
    /// </remarks>
    public string DefinitionId { get; private set; } = null!;

    /// <summary>
    /// Gets the display name of this feature instance.
    /// </summary>
    /// <remarks>
    /// Copied from definition at creation time for display purposes.
    /// </remarks>
    public string Name { get; private set; } = null!;

    /// <summary>
    /// Gets the description of this feature instance.
    /// </summary>
    /// <remarks>
    /// Copied from definition at creation time for display purposes.
    /// </remarks>
    public string Description { get; private set; } = null!;

    /// <summary>
    /// Gets the remaining quantity of resources available to harvest.
    /// </summary>
    /// <remarks>
    /// Decreases when harvested. When it reaches 0, the feature is depleted.
    /// </remarks>
    public int RemainingQuantity { get; private set; }

    /// <summary>
    /// Gets the initial quantity this feature was created with.
    /// </summary>
    /// <remarks>
    /// Used for replenishment to restore to the original state.
    /// </remarks>
    public int InitialQuantity { get; private set; }

    /// <summary>
    /// Gets the turn number at which this feature will replenish, if depleted.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Null if the feature is not depleted or does not replenish.
    /// </para>
    /// <para>
    /// Use <see cref="ShouldReplenish"/> to check if replenishment
    /// should occur on a given turn.
    /// </para>
    /// </remarks>
    public int? ReplenishAtTurn { get; private set; }

    /// <summary>
    /// Gets whether this feature can be interacted with.
    /// </summary>
    /// <remarks>
    /// Always true for harvestable features unless explicitly disabled.
    /// </remarks>
    public bool IsInteractable { get; private set; } = true;

    /// <summary>
    /// Gets the verb used to interact with this feature.
    /// </summary>
    /// <remarks>
    /// Default is "gather" for harvestable features.
    /// </remarks>
    public string InteractionVerb { get; private set; } = "gather";

    // ═══════════════════════════════════════════════════════════════
    // CONSTRUCTORS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Private constructor for factory methods.
    /// </summary>
    /// <remarks>
    /// Use <see cref="Create"/> or <see cref="CreateWithRandomQuantity"/>
    /// factory methods to create new instances.
    /// </remarks>
    private HarvestableFeature() { }

    // ═══════════════════════════════════════════════════════════════
    // FACTORY METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a new harvestable feature instance from a definition.
    /// </summary>
    /// <param name="definition">The definition to create from.</param>
    /// <param name="initialQuantity">The starting quantity of resources.</param>
    /// <returns>A new <see cref="HarvestableFeature"/> instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when definition is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when initialQuantity is negative.</exception>
    /// <example>
    /// <code>
    /// var definition = featureProvider.GetFeature("iron-ore-vein");
    /// var feature = HarvestableFeature.Create(definition, initialQuantity: 5);
    ///
    /// room.AddHarvestableFeature(feature);
    /// </code>
    /// </example>
    public static HarvestableFeature Create(
        HarvestableFeatureDefinition definition,
        int initialQuantity)
    {
        ArgumentNullException.ThrowIfNull(definition, nameof(definition));
        ArgumentOutOfRangeException.ThrowIfNegative(initialQuantity, nameof(initialQuantity));

        return new HarvestableFeature
        {
            Id = Guid.NewGuid(),
            DefinitionId = definition.FeatureId,
            Name = definition.Name,
            Description = definition.Description,
            RemainingQuantity = initialQuantity,
            InitialQuantity = initialQuantity,
            IsInteractable = true,
            InteractionVerb = "gather"
        };
    }

    /// <summary>
    /// Creates a new harvestable feature with a random quantity within the definition's range.
    /// </summary>
    /// <param name="definition">The definition to create from.</param>
    /// <param name="random">Optional random number generator. Uses <see cref="Random.Shared"/> if null.</param>
    /// <returns>A new <see cref="HarvestableFeature"/> instance with randomized quantity.</returns>
    /// <exception cref="ArgumentNullException">Thrown when definition is null.</exception>
    /// <remarks>
    /// <para>
    /// The quantity is randomly selected between the definition's MinQuantity
    /// and MaxQuantity (inclusive).
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var definition = featureProvider.GetFeature("herb-patch");
    /// var feature = HarvestableFeature.CreateWithRandomQuantity(definition);
    ///
    /// // Quantity will be between definition.MinQuantity and definition.MaxQuantity
    /// Console.WriteLine($"This patch has {feature.RemainingQuantity} herbs");
    /// </code>
    /// </example>
    public static HarvestableFeature CreateWithRandomQuantity(
        HarvestableFeatureDefinition definition,
        Random? random = null)
    {
        ArgumentNullException.ThrowIfNull(definition, nameof(definition));

        random ??= Random.Shared;

        // Next(min, max+1) gives inclusive range [min, max]
        var quantity = random.Next(definition.MinQuantity, definition.MaxQuantity + 1);

        return Create(definition, quantity);
    }

    // ═══════════════════════════════════════════════════════════════
    // COMPUTED PROPERTIES
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets whether this feature has been depleted (no resources remaining).
    /// </summary>
    /// <remarks>
    /// A depleted feature cannot be harvested from until it replenishes
    /// (if it supports replenishment).
    /// </remarks>
    /// <example>
    /// <code>
    /// if (feature.IsDepleted)
    /// {
    ///     Console.WriteLine("This resource has been exhausted.");
    /// }
    /// </code>
    /// </example>
    public bool IsDepleted => RemainingQuantity <= 0;

    /// <summary>
    /// Gets whether this feature is currently waiting to replenish.
    /// </summary>
    /// <remarks>
    /// True if a replenish timer has been set and the feature is awaiting
    /// the target turn to restore resources.
    /// </remarks>
    public bool IsAwaitingReplenishment => ReplenishAtTurn.HasValue;

    /// <summary>
    /// Gets whether this feature can currently be harvested from.
    /// </summary>
    /// <remarks>
    /// A feature is harvestable if it is not depleted and is interactable.
    /// </remarks>
    public bool CanHarvest => !IsDepleted && IsInteractable;

    // ═══════════════════════════════════════════════════════════════
    // HARVEST METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Reduces the remaining quantity by the amount harvested.
    /// </summary>
    /// <param name="amount">The amount to harvest (must be non-negative).</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when amount is negative.</exception>
    /// <exception cref="InvalidOperationException">Thrown when feature is depleted.</exception>
    /// <remarks>
    /// <para>
    /// The remaining quantity cannot go below 0. If the harvest amount
    /// exceeds the remaining quantity, the remaining is set to 0.
    /// </para>
    /// <para>
    /// After harvesting depletes the feature, use <see cref="SetReplenishTimer"/>
    /// to start the replenishment countdown (if the feature supports it).
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Harvest 2 resources from the feature
    /// feature.Harvest(2);
    ///
    /// // Check if we depleted it
    /// if (feature.IsDepleted &amp;&amp; definition.Replenishes)
    /// {
    ///     feature.SetReplenishTimer(currentTurn, definition.ReplenishTurns);
    /// }
    /// </code>
    /// </example>
    public void Harvest(int amount)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(amount, nameof(amount));

        if (IsDepleted)
        {
            throw new InvalidOperationException(
                "Cannot harvest from a depleted feature. " +
                $"Feature '{Name}' ({DefinitionId}) has no remaining resources.");
        }

        // Reduce quantity, but don't go below 0
        RemainingQuantity = Math.Max(0, RemainingQuantity - amount);
    }

    // ═══════════════════════════════════════════════════════════════
    // REPLENISHMENT METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Sets the replenishment timer for this feature.
    /// </summary>
    /// <param name="currentTurn">The current game turn.</param>
    /// <param name="replenishTurns">The number of turns until replenishment.</param>
    /// <remarks>
    /// <para>
    /// Only sets the timer if replenishTurns is greater than 0.
    /// If replenishTurns is 0 or negative, no timer is set (feature never replenishes).
    /// </para>
    /// <para>
    /// The replenish turn is calculated as: currentTurn + replenishTurns
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Feature was depleted, set timer if it replenishes
    /// if (definition.Replenishes)
    /// {
    ///     feature.SetReplenishTimer(
    ///         currentTurn: gameState.CurrentTurn,
    ///         replenishTurns: definition.ReplenishTurns);
    /// }
    /// </code>
    /// </example>
    public void SetReplenishTimer(int currentTurn, int replenishTurns)
    {
        if (replenishTurns > 0)
        {
            ReplenishAtTurn = currentTurn + replenishTurns;
        }
    }

    /// <summary>
    /// Checks if the feature should replenish on the given turn.
    /// </summary>
    /// <param name="currentTurn">The current game turn.</param>
    /// <returns>True if the feature should replenish, false otherwise.</returns>
    /// <remarks>
    /// <para>
    /// Returns true if:
    /// <list type="bullet">
    ///   <item><description>A replenish timer is set (<see cref="ReplenishAtTurn"/> has value)</description></item>
    ///   <item><description>The current turn is >= the replenish turn</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Call <see cref="Replenish()"/> when this returns true to restore resources.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // In turn processing loop
    /// foreach (var feature in room.GetHarvestableFeatures())
    /// {
    ///     if (feature.ShouldReplenish(currentTurn))
    ///     {
    ///         feature.Replenish();
    ///         Console.WriteLine($"{feature.Name} has regrown!");
    ///     }
    /// }
    /// </code>
    /// </example>
    public bool ShouldReplenish(int currentTurn)
    {
        return ReplenishAtTurn.HasValue && currentTurn >= ReplenishAtTurn.Value;
    }

    /// <summary>
    /// Replenishes the feature to its initial quantity.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Restores <see cref="RemainingQuantity"/> to <see cref="InitialQuantity"/>
    /// and clears the replenishment timer.
    /// </para>
    /// <para>
    /// Call this method when <see cref="ShouldReplenish"/> returns true.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// if (feature.ShouldReplenish(currentTurn))
    /// {
    ///     feature.Replenish();
    /// }
    /// </code>
    /// </example>
    public void Replenish()
    {
        RemainingQuantity = InitialQuantity;
        ReplenishAtTurn = null;
    }

    /// <summary>
    /// Replenishes the feature to a specific quantity.
    /// </summary>
    /// <param name="quantity">The quantity to replenish to (must be non-negative).</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when quantity is negative.</exception>
    /// <remarks>
    /// <para>
    /// Use this overload when you want to replenish to a different amount
    /// than the initial quantity (e.g., partial replenishment or bonus yield).
    /// </para>
    /// <para>
    /// Clears the replenishment timer after setting the new quantity.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Replenish to a specific amount (maybe with a bonus)
    /// var bonusQuantity = definition.MaxQuantity; // Give max on replenish
    /// feature.Replenish(bonusQuantity);
    /// </code>
    /// </example>
    public void Replenish(int quantity)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(quantity, nameof(quantity));

        RemainingQuantity = quantity;
        ReplenishAtTurn = null;
    }

    // ═══════════════════════════════════════════════════════════════
    // DISPLAY METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets a status display string for this feature.
    /// </summary>
    /// <returns>Formatted string showing depletion or remaining quantity.</returns>
    /// <remarks>
    /// <para>
    /// Possible return values:
    /// <list type="bullet">
    ///   <item><description>"[Depleted - Replenishes at turn 150]" - Depleted with timer</description></item>
    ///   <item><description>"[Depleted]" - Depleted without timer</description></item>
    ///   <item><description>"[5 remaining]" - Has resources</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// Console.WriteLine($"{feature.Name} {feature.GetStatusDisplay()}");
    /// // Output: "Iron Ore Vein [3 remaining]"
    /// // Or: "Herb Patch [Depleted - Replenishes at turn 150]"
    /// </code>
    /// </example>
    public string GetStatusDisplay()
    {
        if (IsDepleted && IsAwaitingReplenishment)
        {
            return $"[Depleted - Replenishes at turn {ReplenishAtTurn}]";
        }

        if (IsDepleted)
        {
            return "[Depleted]";
        }

        return $"[{RemainingQuantity} remaining]";
    }

    /// <summary>
    /// Returns a display string for this feature instance.
    /// </summary>
    /// <returns>Name followed by status display.</returns>
    /// <example>
    /// <code>
    /// Console.WriteLine(feature.ToString());
    /// // Output: "Iron Ore Vein [3 remaining]"
    /// </code>
    /// </example>
    public override string ToString()
    {
        return $"{Name} {GetStatusDisplay()}";
    }
}
