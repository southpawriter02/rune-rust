using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Interfaces;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.Entities;

/// <summary>
/// Represents a sub-zone within a realm biome.
/// </summary>
/// <remarks>
/// <para>
/// RealmBiomeZone allows for environmental variation within a single realm.
/// Zones can override the parent realm's properties, conditions, and DC modifiers.
/// </para>
/// <para>
/// Examples:
/// <list type="bullet">
/// <item>Muspelheim: Obsidian Fields (DC +2), Magma Rivers (DC +4), Forge Core (DC +8)</item>
/// <item>Midgard: The Greatwood (DC +2), The Scar (DC +0), The Mires (DC +4)</item>
/// </list>
/// </para>
/// </remarks>
public sealed class RealmBiomeZone : IEntity
{
    /// <summary>
    /// Gets the unique database identifier.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Gets the zone's string identifier within its parent realm.
    /// </summary>
    public string ZoneId { get; private set; }

    /// <summary>
    /// Gets the zone's display name.
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// Gets the parent realm identifier.
    /// </summary>
    public RealmId ParentRealm { get; private set; }

    /// <summary>
    /// Gets optional property overrides for this zone.
    /// </summary>
    /// <remarks>
    /// If null, the zone uses the parent realm's base properties.
    /// </remarks>
    public RealmBiomeProperties? OverrideProperties { get; private set; }

    /// <summary>
    /// Gets optional condition override for this zone.
    /// </summary>
    /// <remarks>
    /// If null, the zone uses the parent realm's primary condition.
    /// </remarks>
    public EnvironmentalConditionType? OverrideCondition { get; private set; }

    /// <summary>
    /// Gets the DC modifier for environmental checks in this zone.
    /// </summary>
    /// <remarks>
    /// Standard range: -6 (very safe) to +8 (extremely dangerous).
    /// Applied on top of the base condition DC (typically 12).
    /// </remarks>
    public int ConditionDcModifier { get; private set; }

    /// <summary>
    /// Gets optional damage dice override for this zone.
    /// </summary>
    /// <remarks>
    /// If specified, overrides the base condition's damage dice.
    /// Example: "4d6" for Forge Core instead of default "2d6".
    /// </remarks>
    public string? DamageOverride { get; private set; }

    /// <summary>
    /// Gets the faction spawn pool IDs active in this zone.
    /// </summary>
    /// <remarks>
    /// Each string is a faction pool identifier (e.g., "blighted-beasts", "humanoid").
    /// When null, the zone inherits the parent realm's default spawn configuration.
    /// </remarks>
    public IReadOnlyList<string>? FactionPools { get; private set; }

    /// <summary>
    /// Gets the narrative description of this zone.
    /// </summary>
    /// <remarks>
    /// Sourced from canonical lore. Used for room generation and environmental descriptions.
    /// </remarks>
    public string? Description { get; private set; }

    /// <summary>
    /// Gets optional minimum vertical zone override.
    /// </summary>
    public VerticalZone? MinVerticalZone { get; private set; }

    /// <summary>
    /// Gets optional maximum vertical zone override.
    /// </summary>
    public VerticalZone? MaxVerticalZone { get; private set; }

    /// <summary>
    /// Private constructor for EF Core.
    /// </summary>
    private RealmBiomeZone()
    {
        ZoneId = null!;
        Name = null!;
    }

    /// <summary>
    /// Creates a new realm biome zone.
    /// </summary>
    public static RealmBiomeZone Create(
        string zoneId,
        string name,
        RealmId parentRealm,
        RealmBiomeProperties? overrideProperties = null,
        EnvironmentalConditionType? overrideCondition = null,
        int conditionDcModifier = 0,
        string? damageOverride = null,
        VerticalZone? minVerticalZone = null,
        VerticalZone? maxVerticalZone = null,
        IReadOnlyList<string>? factionPools = null,
        string? description = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(zoneId);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        if (minVerticalZone.HasValue && maxVerticalZone.HasValue &&
            minVerticalZone.Value > maxVerticalZone.Value)
        {
            throw new ArgumentException("MinVerticalZone cannot be greater than MaxVerticalZone");
        }

        return new RealmBiomeZone
        {
            Id = Guid.NewGuid(),
            ZoneId = zoneId.ToLowerInvariant(),
            Name = name,
            ParentRealm = parentRealm,
            OverrideProperties = overrideProperties,
            OverrideCondition = overrideCondition,
            ConditionDcModifier = conditionDcModifier,
            DamageOverride = damageOverride,
            MinVerticalZone = minVerticalZone,
            MaxVerticalZone = maxVerticalZone,
            FactionPools = factionPools,
            Description = description
        };
    }

    /// <summary>
    /// Gets whether this zone has property overrides.
    /// </summary>
    public bool HasPropertyOverrides => OverrideProperties is not null;

    /// <summary>
    /// Gets whether this zone has a condition override.
    /// </summary>
    public bool HasConditionOverride => OverrideCondition.HasValue;

    /// <summary>
    /// Gets whether this zone has a damage override.
    /// </summary>
    public bool HasDamageOverride => !string.IsNullOrEmpty(DamageOverride);

    /// <summary>
    /// Gets whether this zone has faction pools defined.
    /// </summary>
    public bool HasFactionPools => FactionPools is { Count: > 0 };

    /// <summary>
    /// Gets a human-readable description of the DC modifier.
    /// </summary>
    public string DcModifierDescription => ConditionDcModifier switch
    {
        < -4 => "Very Safe",
        < 0 => "Safer",
        0 => "Standard",
        <= 4 => "Dangerous",
        _ => "Extremely Dangerous"
    };

    /// <summary>
    /// Gets the effective DC for this zone given a base DC.
    /// </summary>
    /// <param name="baseDc">The base DC from the environmental condition.</param>
    /// <returns>The adjusted DC with zone modifier applied.</returns>
    public int GetEffectiveDc(int baseDc) => baseDc + ConditionDcModifier;
}
