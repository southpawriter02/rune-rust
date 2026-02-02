using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Defines the adjacency relationship between two realms.
/// </summary>
/// <remarks>
/// <para>
/// AdjacencyRule specifies whether two realms can be neighbors in dungeon
/// generation, and if so, what transition requirements exist between them.
/// </para>
/// <para>
/// Critical Incompatibilities:
/// <list type="bullet">
/// <item>Muspelheim (Fire) ↔ Niflheim (Ice)</item>
/// <item>Muspelheim (Fire) ↔ Vanaheim (Bio)</item>
/// </list>
/// </para>
/// </remarks>
public sealed record AdjacencyRule
{
    /// <summary>
    /// First realm in the pair.
    /// </summary>
    public required RealmId RealmA { get; init; }

    /// <summary>
    /// Second realm in the pair.
    /// </summary>
    public required RealmId RealmB { get; init; }

    /// <summary>
    /// Compatibility classification for this realm pair.
    /// </summary>
    public required BiomeCompatibility Compatibility { get; init; }

    /// <summary>
    /// Minimum transition rooms required (0 if Compatible).
    /// </summary>
    public int MinTransitionRooms { get; init; } = 0;

    /// <summary>
    /// Maximum transition rooms allowed (0 if Compatible).
    /// </summary>
    public int MaxTransitionRooms { get; init; } = 0;

    /// <summary>
    /// Narrative description for transition zones between these realms.
    /// </summary>
    /// <remarks>
    /// Used by room generation to create thematic transition descriptions.
    /// Example: "Volcanic heat fades into geothermal stability..."
    /// </remarks>
    public string? TransitionTheme { get; init; }

    /// <summary>
    /// Gets whether this rule requires transition rooms.
    /// </summary>
    public bool RequiresTransitionRooms =>
        Compatibility == BiomeCompatibility.RequiresTransition && MinTransitionRooms > 0;

    /// <summary>
    /// Checks if this rule applies to the given realm pair (bidirectional).
    /// </summary>
    /// <param name="a">First realm to check.</param>
    /// <param name="b">Second realm to check.</param>
    /// <returns>True if this rule applies to the realm pair in either order.</returns>
    public bool AppliesTo(RealmId a, RealmId b) =>
        (RealmA == a && RealmB == b) || (RealmA == b && RealmB == a);

    /// <summary>
    /// Gets a normalized key for dictionary lookups (smaller ID first).
    /// </summary>
    /// <returns>A consistent string key regardless of realm order.</returns>
    public string GetKey() => GetKey(RealmA, RealmB);

    /// <summary>
    /// Creates a normalized key for a realm pair (smaller ID first).
    /// </summary>
    /// <param name="a">First realm.</param>
    /// <param name="b">Second realm.</param>
    /// <returns>A consistent string key regardless of realm order.</returns>
    public static string GetKey(RealmId a, RealmId b)
    {
        var first = (int)a < (int)b ? a : b;
        var second = (int)a < (int)b ? b : a;
        return $"{first}:{second}";
    }

    /// <summary>
    /// Creates an incompatible rule for realms that cannot neighbor.
    /// </summary>
    public static AdjacencyRule Incompatible(RealmId a, RealmId b) => new()
    {
        RealmA = a,
        RealmB = b,
        Compatibility = BiomeCompatibility.Incompatible
    };

    /// <summary>
    /// Creates a compatible rule for realms that can directly neighbor.
    /// </summary>
    public static AdjacencyRule Compatible(RealmId a, RealmId b, string? transitionTheme = null) => new()
    {
        RealmA = a,
        RealmB = b,
        Compatibility = BiomeCompatibility.Compatible,
        TransitionTheme = transitionTheme
    };

    /// <summary>
    /// Creates a rule requiring transition rooms between realms.
    /// </summary>
    public static AdjacencyRule WithTransition(
        RealmId a,
        RealmId b,
        int minRooms = 1,
        int maxRooms = 3,
        string? transitionTheme = null) => new()
    {
        RealmA = a,
        RealmB = b,
        Compatibility = BiomeCompatibility.RequiresTransition,
        MinTransitionRooms = minRooms,
        MaxTransitionRooms = maxRooms,
        TransitionTheme = transitionTheme
    };
}
