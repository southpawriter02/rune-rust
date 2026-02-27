namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Types of hunting traps available to the Veiðimaðr (Hunter) specialization.
/// </summary>
/// <remarks>
/// <para>All trap types share the same base statistics (1d8 damage, 1-turn immobilize, DC 13 detection)
/// but differ in thematic description and narrative presentation.</para>
/// <para>Introduced in v0.20.7b as part of the Trap Mastery ability.</para>
/// </remarks>
public enum TrapType
{
    /// <summary>
    /// Spike trap — spikes protrude from the ground, causing piercing wounds.
    /// </summary>
    Spike = 0,

    /// <summary>
    /// Net trap — entangling webbing catches and holds targets.
    /// </summary>
    Net = 1,

    /// <summary>
    /// Pitfall trap — a concealed pit causes falling damage and immobilization.
    /// </summary>
    PitFall = 2,

    /// <summary>
    /// Deadfall trap — logs or debris fall on the target when triggered.
    /// </summary>
    Deadfall = 3,

    /// <summary>
    /// Snare trap — a wire snare at foot level catches and restrains.
    /// </summary>
    Snare = 4
}
