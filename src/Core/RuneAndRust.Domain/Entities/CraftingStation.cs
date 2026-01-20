using RuneAndRust.Domain.Definitions;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Interfaces;

namespace RuneAndRust.Domain.Entities;

/// <summary>
/// Represents a crafting station instance placed in a room.
/// </summary>
/// <remarks>
/// <para>
/// CraftingStation is a room feature instance created from
/// <see cref="CraftingStationDefinition"/> templates. Each instance tracks
/// its own availability state, allowing the same station definition to
/// exist in multiple rooms with independent state.
/// </para>
/// <para>
/// Key features:
/// <list type="bullet">
///   <item><description>Tracks availability for crafting operations</description></item>
///   <item><description>Records last usage timestamp</description></item>
///   <item><description>Interactable via "use" verb</description></item>
///   <item><description>RoomFeatureType.CraftingStation for feature identification</description></item>
/// </list>
/// </para>
/// <para>
/// Lifecycle:
/// <list type="number">
///   <item><description>Created from definition via <see cref="CraftingStationDefinition.CreateInstance()"/></description></item>
///   <item><description>Added to room features collection</description></item>
///   <item><description>Player uses station - marked as in use via <see cref="SetInUse"/></description></item>
///   <item><description>Crafting completes - restored via <see cref="SetAvailable"/></description></item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Create a station instance from definition
/// var definition = stationProvider.GetStation("anvil");
/// var station = CraftingStation.Create(definition);
///
/// // Add to room
/// room.AddCraftingStation(station);
///
/// // Mark as in use during crafting
/// station.SetInUse();
/// Console.WriteLine(station.GetStatusDescription()); // "The station is currently in use."
///
/// // Restore availability after crafting
/// station.SetAvailable();
/// Console.WriteLine(station.GetStatusDescription()); // "The station is ready for use."
/// </code>
/// </example>
public sealed class CraftingStation : IEntity
{
    // ═══════════════════════════════════════════════════════════════
    // PROPERTIES
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the unique identifier for this station instance.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Gets the ID of the station definition this instance was created from.
    /// </summary>
    /// <remarks>
    /// Use this to look up the full definition from the provider when you need
    /// access to supported categories, crafting skill, etc.
    /// </remarks>
    /// <example>"anvil", "alchemy-table", "enchanting-altar"</example>
    public string DefinitionId { get; private set; } = null!;

    /// <summary>
    /// Gets the display name of this station instance.
    /// </summary>
    /// <remarks>
    /// Copied from definition at creation time for display purposes.
    /// </remarks>
    public string Name { get; private set; } = null!;

    /// <summary>
    /// Gets or sets the description of this station instance.
    /// </summary>
    /// <remarks>
    /// Can be customized per instance to provide room-specific flavor text.
    /// Set at creation time or modified via custom creation factory.
    /// </remarks>
    public string Description { get; private set; } = null!;

    /// <summary>
    /// Gets the feature type identifier.
    /// </summary>
    /// <remarks>
    /// Always <see cref="RoomFeatureType.CraftingStation"/> for crafting stations.
    /// Used for filtering and identifying station features in rooms.
    /// </remarks>
    public RoomFeatureType FeatureType { get; private set; }

    /// <summary>
    /// Gets whether this station can be interacted with.
    /// </summary>
    /// <remarks>
    /// Always true for crafting stations - they can always be examined or used.
    /// </remarks>
    public bool IsInteractable { get; private set; } = true;

    /// <summary>
    /// Gets the verb used to interact with this station.
    /// </summary>
    /// <remarks>
    /// Default is "use" for crafting stations. Players type "use anvil"
    /// or "craft &lt;recipe&gt; at anvil" to interact.
    /// </remarks>
    public string InteractionVerb { get; private set; } = "use";

    /// <summary>
    /// Gets whether the station is currently available for use.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A station becomes unavailable while a player is actively crafting.
    /// This prevents multiple simultaneous crafting operations at the same station.
    /// </para>
    /// <para>
    /// Use <see cref="SetInUse"/> to mark as unavailable and
    /// <see cref="SetAvailable"/> to restore availability.
    /// </para>
    /// </remarks>
    public bool IsAvailable { get; private set; } = true;

    /// <summary>
    /// Gets the timestamp when the station was last used.
    /// </summary>
    /// <remarks>
    /// Updated when <see cref="SetInUse"/> is called. Null if never used.
    /// Can be used for tracking station usage patterns or cooldowns.
    /// </remarks>
    public DateTime? LastUsedAt { get; private set; }

    // ═══════════════════════════════════════════════════════════════
    // CONSTRUCTORS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Private constructor for factory pattern and serialization.
    /// </summary>
    /// <remarks>
    /// Use <see cref="Create(CraftingStationDefinition)"/> or
    /// <see cref="Create(CraftingStationDefinition, string)"/> factory methods.
    /// </remarks>
    private CraftingStation() { }

    // ═══════════════════════════════════════════════════════════════
    // FACTORY METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a new crafting station instance from a definition.
    /// </summary>
    /// <param name="definition">The station definition to instantiate.</param>
    /// <returns>A new CraftingStation instance.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when definition is null.
    /// </exception>
    /// <remarks>
    /// <para>
    /// The new station inherits name and description from the definition.
    /// It is initialized as available with FeatureType.CraftingStation,
    /// IsInteractable = true, and InteractionVerb = "use".
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var definition = stationProvider.GetStation("anvil");
    /// var station = CraftingStation.Create(definition);
    ///
    /// Console.WriteLine($"Created {station.Name} (available: {station.IsAvailable})");
    /// // Output: Created Anvil (available: True)
    /// </code>
    /// </example>
    public static CraftingStation Create(CraftingStationDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition, nameof(definition));

        return new CraftingStation
        {
            Id = Guid.NewGuid(),
            DefinitionId = definition.StationId,
            Name = definition.Name,
            Description = definition.Description,
            FeatureType = RoomFeatureType.CraftingStation,
            IsInteractable = true,
            InteractionVerb = "use",
            IsAvailable = true,
            LastUsedAt = null
        };
    }

    /// <summary>
    /// Creates a crafting station with a custom description.
    /// </summary>
    /// <param name="definition">The station definition to instantiate.</param>
    /// <param name="customDescription">A custom description for this specific instance.</param>
    /// <returns>A new CraftingStation instance with the custom description.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when definition is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when customDescription is null or whitespace.
    /// </exception>
    /// <remarks>
    /// <para>
    /// Useful for creating unique station instances with room-specific flavor text
    /// while maintaining the same crafting capabilities. The custom description
    /// is shown when examining the station.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var definition = stationProvider.GetStation("anvil");
    /// var station = CraftingStation.Create(
    ///     definition,
    ///     "An ancient anvil, its surface scarred by centuries of legendary smithing.");
    ///
    /// Console.WriteLine(station.Description);
    /// // Output: An ancient anvil, its surface scarred by centuries of legendary smithing.
    /// </code>
    /// </example>
    public static CraftingStation Create(
        CraftingStationDefinition definition,
        string customDescription)
    {
        ArgumentNullException.ThrowIfNull(definition, nameof(definition));
        ArgumentException.ThrowIfNullOrWhiteSpace(customDescription, nameof(customDescription));

        var station = Create(definition);
        station.Description = customDescription;
        return station;
    }

    // ═══════════════════════════════════════════════════════════════
    // STATE METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Marks the station as in use (unavailable for other crafting).
    /// </summary>
    /// <remarks>
    /// <para>
    /// Call this when a player begins crafting at this station.
    /// Sets <see cref="IsAvailable"/> to false and updates <see cref="LastUsedAt"/>.
    /// </para>
    /// <para>
    /// Remember to call <see cref="SetAvailable"/> when crafting completes
    /// (success, failure, or cancellation).
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Player starts crafting
    /// if (station.IsAvailable)
    /// {
    ///     station.SetInUse();
    ///     // Begin crafting process...
    /// }
    /// </code>
    /// </example>
    public void SetInUse()
    {
        IsAvailable = false;
        LastUsedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Marks the station as available for use.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Call this when a crafting operation completes (success, failure, or cancel).
    /// Sets <see cref="IsAvailable"/> to true.
    /// </para>
    /// <para>
    /// Does not clear <see cref="LastUsedAt"/> - that timestamp is preserved
    /// for usage tracking purposes.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Crafting completes
    /// try
    /// {
    ///     await DoCrafting();
    /// }
    /// finally
    /// {
    ///     station.SetAvailable();
    /// }
    /// </code>
    /// </example>
    public void SetAvailable()
    {
        IsAvailable = true;
    }

    // ═══════════════════════════════════════════════════════════════
    // DISPLAY METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets a description of the station's current status.
    /// </summary>
    /// <returns>A human-readable status string.</returns>
    /// <remarks>
    /// Returns "The station is ready for use." when available,
    /// or "The station is currently in use." when unavailable.
    /// </remarks>
    /// <example>
    /// <code>
    /// Console.WriteLine(station.GetStatusDescription());
    /// // Output: The station is ready for use.
    ///
    /// station.SetInUse();
    /// Console.WriteLine(station.GetStatusDescription());
    /// // Output: The station is currently in use.
    /// </code>
    /// </example>
    public string GetStatusDescription()
    {
        return IsAvailable
            ? "The station is ready for use."
            : "The station is currently in use.";
    }

    /// <summary>
    /// Gets the interaction prompt for this station.
    /// </summary>
    /// <returns>The interaction prompt text appropriate for current state.</returns>
    /// <remarks>
    /// <para>
    /// When available, prompts the player to craft.
    /// When unavailable, informs that the station is in use.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// Console.WriteLine(station.GetInteractionPrompt());
    /// // Output: Type 'craft &lt;recipe&gt;' to craft at this Anvil.
    ///
    /// station.SetInUse();
    /// Console.WriteLine(station.GetInteractionPrompt());
    /// // Output: The Anvil is currently in use.
    /// </code>
    /// </example>
    public string GetInteractionPrompt()
    {
        return IsAvailable
            ? $"Type 'craft <recipe>' to craft at this {Name}."
            : $"The {Name} is currently in use.";
    }

    /// <summary>
    /// Gets a status indicator string for display.
    /// </summary>
    /// <returns>A short status indicator.</returns>
    /// <remarks>
    /// Returns "[Available]" or "[In Use]" for compact status display.
    /// </remarks>
    /// <example>
    /// <code>
    /// Console.WriteLine($"{station.Name} {station.GetStatusIndicator()}");
    /// // Output: Anvil [Available]
    /// </code>
    /// </example>
    public string GetStatusIndicator()
    {
        return IsAvailable ? "[Available]" : "[In Use]";
    }

    /// <summary>
    /// Returns a string representation of the station instance.
    /// </summary>
    /// <returns>The station name followed by status indicator.</returns>
    /// <example>
    /// <code>
    /// Console.WriteLine(station.ToString());
    /// // Output: Anvil [Available]
    /// </code>
    /// </example>
    public override string ToString()
    {
        return $"{Name} {GetStatusIndicator()}";
    }
}
