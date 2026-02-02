using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Interfaces;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.Entities;

/// <summary>
/// Represents a realm biome definition for the Nine Realms system.
/// </summary>
/// <remarks>
/// <para>
/// RealmBiomeDefinition captures all properties of a realm including its
/// environmental characteristics, hazards, vertical range, and sub-zones.
/// Each realm corresponds to a Deck of the YGGDRASIL Network.
/// </para>
/// <para>
/// This entity is separate from the generic BiomeDefinition used for
/// dungeon generation (cave, volcanic, forest). The Nine Realms system
/// represents the canonical geography of Aethelgard.
/// </para>
/// </remarks>
public sealed class RealmBiomeDefinition : IEntity
{
    /// <summary>
    /// Gets the unique database identifier.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Gets the realm identifier (1-9 matching deck numbers).
    /// </summary>
    public RealmId RealmId { get; private set; }

    /// <summary>
    /// Gets the realm's display name.
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// Gets the realm's subtitle/epithet.
    /// </summary>
    /// <remarks>
    /// Example: "The Tamed Ruin" for Midgard, "The Burning Caldera" for Muspelheim.
    /// </remarks>
    public string Subtitle { get; private set; }

    /// <summary>
    /// Gets the YGGDRASIL deck number (1-9).
    /// </summary>
    public int DeckNumber { get; private set; }

    /// <summary>
    /// Gets the realm's function before The Glitch.
    /// </summary>
    public string PreGlitchFunction { get; private set; }

    /// <summary>
    /// Gets the realm's current state after The Glitch.
    /// </summary>
    public string PostGlitchState { get; private set; }

    /// <summary>
    /// Gets the base environmental properties for this realm.
    /// </summary>
    public RealmBiomeProperties BaseProperties { get; private set; }

    /// <summary>
    /// Gets the primary environmental condition/hazard for this realm.
    /// </summary>
    public EnvironmentalConditionType PrimaryCondition { get; private set; }

    /// <summary>
    /// Gets the minimum vertical zone where this realm can appear.
    /// </summary>
    public VerticalZone MinVerticalZone { get; private set; }

    /// <summary>
    /// Gets the maximum vertical zone where this realm can appear.
    /// </summary>
    public VerticalZone MaxVerticalZone { get; private set; }

    /// <summary>
    /// Gets the sub-zones within this realm.
    /// </summary>
    public IReadOnlyList<RealmBiomeZone> Zones { get; private set; }

    /// <summary>
    /// Gets an optional flavor quote for this realm.
    /// </summary>
    public string? FlavorQuote { get; private set; }

    /// <summary>
    /// Gets the color palette descriptor for UI theming.
    /// </summary>
    public string? ColorPalette { get; private set; }

    /// <summary>
    /// Private constructor for EF Core.
    /// </summary>
    private RealmBiomeDefinition()
    {
        Name = null!;
        Subtitle = null!;
        PreGlitchFunction = null!;
        PostGlitchState = null!;
        BaseProperties = RealmBiomeProperties.Temperate();
        Zones = [];
    }

    /// <summary>
    /// Creates a new realm biome definition.
    /// </summary>
    public static RealmBiomeDefinition Create(
        RealmId realmId,
        string name,
        string subtitle,
        int deckNumber,
        string preGlitchFunction,
        string postGlitchState,
        RealmBiomeProperties baseProperties,
        EnvironmentalConditionType primaryCondition,
        VerticalZone minVerticalZone,
        VerticalZone maxVerticalZone,
        IReadOnlyList<RealmBiomeZone>? zones = null,
        string? flavorQuote = null,
        string? colorPalette = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(subtitle);
        ArgumentOutOfRangeException.ThrowIfLessThan(deckNumber, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(deckNumber, 9);

        if (minVerticalZone > maxVerticalZone)
            throw new ArgumentException("MinVerticalZone cannot be greater than MaxVerticalZone");

        return new RealmBiomeDefinition
        {
            Id = Guid.NewGuid(),
            RealmId = realmId,
            Name = name,
            Subtitle = subtitle,
            DeckNumber = deckNumber,
            PreGlitchFunction = preGlitchFunction,
            PostGlitchState = postGlitchState,
            BaseProperties = baseProperties,
            PrimaryCondition = primaryCondition,
            MinVerticalZone = minVerticalZone,
            MaxVerticalZone = maxVerticalZone,
            Zones = zones ?? [],
            FlavorQuote = flavorQuote,
            ColorPalette = colorPalette
        };
    }

    /// <summary>
    /// Gets the effective properties for a specific zone, falling back to base if not found.
    /// </summary>
    /// <param name="zoneId">The zone ID, or null for base properties.</param>
    /// <returns>Zone-specific properties if overridden, otherwise base properties.</returns>
    public RealmBiomeProperties GetEffectiveProperties(string? zoneId)
    {
        if (string.IsNullOrEmpty(zoneId))
            return BaseProperties;

        var zone = GetZone(zoneId);
        return zone?.OverrideProperties ?? BaseProperties;
    }

    /// <summary>
    /// Gets the effective condition for a specific zone, falling back to primary if not found.
    /// </summary>
    /// <param name="zoneId">The zone ID, or null for primary condition.</param>
    /// <returns>Zone-specific condition if overridden, otherwise primary condition.</returns>
    public EnvironmentalConditionType GetEffectiveCondition(string? zoneId)
    {
        if (string.IsNullOrEmpty(zoneId))
            return PrimaryCondition;

        var zone = GetZone(zoneId);
        return zone?.OverrideCondition ?? PrimaryCondition;
    }

    /// <summary>
    /// Gets a zone by its ID.
    /// </summary>
    /// <param name="zoneId">The zone identifier.</param>
    /// <returns>The zone, or null if not found.</returns>
    public RealmBiomeZone? GetZone(string zoneId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(zoneId);
        return Zones.FirstOrDefault(z =>
            z.ZoneId.Equals(zoneId, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Gets the full display name (Name — Subtitle).
    /// </summary>
    public string FullName => $"{Name} — {Subtitle}";

    /// <summary>
    /// Gets whether this realm has any sub-zones defined.
    /// </summary>
    public bool HasZones => Zones.Count > 0;

    /// <summary>
    /// Gets the vertical zone range as a human-readable string.
    /// </summary>
    public string VerticalRangeDescription =>
        MinVerticalZone == MaxVerticalZone
            ? $"{MinVerticalZone}"
            : $"{MinVerticalZone} to {MaxVerticalZone}";
}
