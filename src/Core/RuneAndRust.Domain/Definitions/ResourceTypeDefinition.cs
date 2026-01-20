namespace RuneAndRust.Domain.Definitions;

/// <summary>
/// Defines a type of resource that can be used by player classes.
/// </summary>
/// <remarks>
/// Resource types are loaded from configuration and define mechanics like
/// regeneration, decay, and build-on-hit. Examples include Mana, Rage, Energy.
/// </remarks>
public class ResourceTypeDefinition
{
    /// <summary>Unique identifier (e.g., "mana", "rage", "energy").</summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>Display name shown to players.</summary>
    public string DisplayName { get; init; } = string.Empty;

    /// <summary>Abbreviated form for compact display (e.g., "MP", "RG").</summary>
    public string Abbreviation { get; init; } = string.Empty;

    /// <summary>Description of this resource type.</summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>Hex color code for UI rendering.</summary>
    public string Color { get; init; } = "#FFFFFF";

    /// <summary>Default maximum value for this resource.</summary>
    public int DefaultMax { get; init; } = 100;

    /// <summary>Amount regenerated at the end of each turn.</summary>
    public int RegenPerTurn { get; init; } = 0;

    /// <summary>Amount lost at the end of each turn.</summary>
    public int DecayPerTurn { get; init; } = 0;

    /// <summary>Whether decay only occurs outside of combat.</summary>
    public bool DecayOnlyOutOfCombat { get; init; } = true;

    /// <summary>Amount gained when dealing damage.</summary>
    public int BuildOnDamageDealt { get; init; } = 0;

    /// <summary>Amount gained when taking damage.</summary>
    public int BuildOnDamageTaken { get; init; } = 0;

    /// <summary>Amount gained when performing healing.</summary>
    public int BuildOnHeal { get; init; } = 0;

    /// <summary>Whether all classes have this resource (true for Health).</summary>
    public bool IsUniversal { get; init; } = false;

    /// <summary>Whether this resource starts at zero instead of maximum.</summary>
    public bool StartsAtZero { get; init; } = false;

    /// <summary>Display sort order.</summary>
    public int SortOrder { get; init; } = 0;

    /// <summary>Gets whether this resource regenerates over time.</summary>
    public bool Regenerates => RegenPerTurn > 0;

    /// <summary>Gets whether this resource decays over time.</summary>
    public bool Decays => DecayPerTurn > 0;

    /// <summary>Gets whether this resource builds from combat.</summary>
    public bool BuildsFromCombat => BuildOnDamageDealt > 0 || BuildOnDamageTaken > 0;

    /// <summary>Gets whether this resource builds from support actions.</summary>
    public bool BuildsFromSupport => BuildOnHeal > 0;

    /// <summary>
    /// Creates a new resource type definition with validation.
    /// </summary>
    public static ResourceTypeDefinition Create(
        string id,
        string displayName,
        string abbreviation,
        string description,
        string color,
        int defaultMax,
        int regenPerTurn = 0,
        int decayPerTurn = 0,
        bool decayOnlyOutOfCombat = true,
        int buildOnDamageDealt = 0,
        int buildOnDamageTaken = 0,
        int buildOnHeal = 0,
        bool isUniversal = false,
        bool startsAtZero = false,
        int sortOrder = 0)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);
        ArgumentException.ThrowIfNullOrWhiteSpace(abbreviation);
        ArgumentOutOfRangeException.ThrowIfNegative(defaultMax);

        return new ResourceTypeDefinition
        {
            Id = id.ToLowerInvariant(),
            DisplayName = displayName,
            Abbreviation = abbreviation.ToUpperInvariant(),
            Description = description,
            Color = color,
            DefaultMax = defaultMax,
            RegenPerTurn = regenPerTurn,
            DecayPerTurn = decayPerTurn,
            DecayOnlyOutOfCombat = decayOnlyOutOfCombat,
            BuildOnDamageDealt = buildOnDamageDealt,
            BuildOnDamageTaken = buildOnDamageTaken,
            BuildOnHeal = buildOnHeal,
            IsUniversal = isUniversal,
            StartsAtZero = startsAtZero,
            SortOrder = sortOrder
        };
    }
}
