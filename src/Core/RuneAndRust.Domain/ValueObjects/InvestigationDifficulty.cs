namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Reference values for investigation difficulty by target complexity.
/// </summary>
public static class InvestigationDifficulty
{
    /// <summary>
    /// Fresh, undisturbed scene with obvious clues. DC 8.
    /// </summary>
    public const int Obvious = 8;

    /// <summary>
    /// Standard investigation with moderate complexity. DC 12.
    /// </summary>
    public const int Standard = 12;

    /// <summary>
    /// Complex scene requiring careful analysis. DC 16.
    /// </summary>
    public const int Complex = 16;

    /// <summary>
    /// Obscured or deliberately hidden evidence. DC 20.
    /// </summary>
    public const int Obscured = 20;

    /// <summary>
    /// Ancient or heavily degraded evidence. DC 24.
    /// </summary>
    public const int Ancient = 24;

    /// <summary>
    /// Gets the difficulty description for a given DC.
    /// </summary>
    /// <param name="dc">The difficulty class.</param>
    /// <returns>Human-readable difficulty description.</returns>
    public static string GetDescription(int dc) => dc switch
    {
        <= 8 => "Obvious",
        <= 12 => "Standard",
        <= 16 => "Complex",
        <= 20 => "Obscured",
        _ => "Ancient"
    };
}
