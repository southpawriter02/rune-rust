namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Defines a saving throw to avoid or reduce an effect.
/// </summary>
/// <remarks>
/// Saving throws allow players to mitigate hazard effects through dice rolls.
/// The roll formula is: 1d20 + attribute modifier vs DC.
/// <para>
/// A successful save either:
/// <list type="bullet">
///   <item><description>Halves the damage when <see cref="Negates"/> is <c>false</c></description></item>
///   <item><description>Completely negates the effect when <see cref="Negates"/> is <c>true</c></description></item>
/// </list>
/// </para>
/// <para>
/// Common save attributes:
/// <list type="bullet">
///   <item><description><b>Fortitude</b> - Resisting poison, disease, and physical effects</description></item>
///   <item><description><b>Agility</b> - Dodging area effects and physical hazards</description></item>
///   <item><description><b>Will</b> - Resisting mental effects and magical compulsions</description></item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Create a Fortitude save DC 12 that halves damage on success
/// var poisonSave = SavingThrow.Fortitude(12);
/// 
/// // Create an Agility save DC 14 that negates all damage on success
/// var fireSave = SavingThrow.Agility(14, negates: true);
/// </code>
/// </example>
public readonly record struct SavingThrow
{
    /// <summary>
    /// Gets the attribute used for the save (e.g., "Fortitude", "Agility", "Will").
    /// </summary>
    /// <remarks>
    /// The attribute determines which player stat modifier is added to the d20 roll.
    /// </remarks>
    public string Attribute { get; init; }

    /// <summary>
    /// Gets the Difficulty Class (DC) to beat for success.
    /// </summary>
    /// <remarks>
    /// The player must roll 1d20 + modifier >= DC to succeed.
    /// Typical DCs range from 10 (easy) to 20+ (very hard).
    /// </remarks>
    public int DC { get; init; }

    /// <summary>
    /// Gets whether success negates (true) or halves (false) the effect.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    ///   <item><description><c>true</c> - Success completely avoids all damage and effects</description></item>
    ///   <item><description><c>false</c> - Success halves damage but still applies (no status effects)</description></item>
    /// </list>
    /// </remarks>
    public bool Negates { get; init; }

    /// <summary>
    /// Creates a saving throw with specified parameters.
    /// </summary>
    /// <param name="attribute">The attribute used for the save (e.g., "Fortitude").</param>
    /// <param name="dc">The Difficulty Class to beat.</param>
    /// <param name="negates">Whether success negates (true) or halves (false) the effect.</param>
    /// <returns>A new <see cref="SavingThrow"/> instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="attribute"/> is null.</exception>
    public static SavingThrow Create(string attribute, int dc, bool negates = false)
    {
        ArgumentNullException.ThrowIfNull(attribute);
        ArgumentException.ThrowIfNullOrWhiteSpace(attribute);

        return new SavingThrow
        {
            Attribute = attribute,
            DC = dc,
            Negates = negates
        };
    }

    /// <summary>
    /// Creates a Fortitude save for resisting physical effects.
    /// </summary>
    /// <param name="dc">The Difficulty Class to beat.</param>
    /// <param name="negates">Whether success negates (true) or halves (false) the effect.</param>
    /// <returns>A Fortitude-based <see cref="SavingThrow"/>.</returns>
    public static SavingThrow Fortitude(int dc, bool negates = false) =>
        Create("Fortitude", dc, negates);

    /// <summary>
    /// Creates an Agility save for dodging effects.
    /// </summary>
    /// <param name="dc">The Difficulty Class to beat.</param>
    /// <param name="negates">Whether success negates (true) or halves (false) the effect.</param>
    /// <returns>An Agility-based <see cref="SavingThrow"/>.</returns>
    public static SavingThrow Agility(int dc, bool negates = false) =>
        Create("Agility", dc, negates);

    /// <summary>
    /// Creates a Will save for resisting mental effects.
    /// </summary>
    /// <param name="dc">The Difficulty Class to beat.</param>
    /// <param name="negates">Whether success negates (true) or halves (false) the effect.</param>
    /// <returns>A Will-based <see cref="SavingThrow"/>.</returns>
    public static SavingThrow Will(int dc, bool negates = false) =>
        Create("Will", dc, negates);

    /// <summary>
    /// Returns a string representation of this saving throw.
    /// </summary>
    /// <returns>A string in the format "Attribute DC X (halves/negates)".</returns>
    public override string ToString() =>
        $"{Attribute} DC {DC} ({(Negates ? "negates" : "halves")})";
}
