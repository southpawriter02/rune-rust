namespace RuneAndRust.Core.Enums;

/// <summary>
/// Represents the effective range at which a spell can be cast.
/// Determines valid targets based on distance from the caster.
/// </summary>
/// <remarks>
/// Ranges (v0.4.3b - The Grimoire):
/// - Self: No range, affects caster only
/// - Touch: Requires physical contact
/// - Close: Nearby targets within arm's reach
/// - Medium: Standard combat range
/// - Far: Distant targets across the battlefield
/// </remarks>
public enum SpellRange
{
    /// <summary>
    /// No range - spell affects only the caster.
    /// Personal enhancement or self-targeted effects.
    /// </summary>
    Self = 0,

    /// <summary>
    /// Requires physical contact with the target.
    /// The caster must be adjacent to or touching the target.
    /// </summary>
    Touch = 1,

    /// <summary>
    /// Close range - nearby targets within arm's reach.
    /// Short-range combat spells and close-quarters magic.
    /// </summary>
    Close = 2,

    /// <summary>
    /// Medium range - standard combat distance.
    /// Most offensive and utility spells operate at this range.
    /// </summary>
    Medium = 3,

    /// <summary>
    /// Far range - distant targets across the battlefield.
    /// Long-range attacks and reconnaissance magic.
    /// </summary>
    Far = 4
}
