namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Types of surfaces affecting stealth movement checks.
/// </summary>
/// <remarks>
/// <para>
/// Surface types determine the base DC for stealth checks. Noisier surfaces
/// require more successes to move silently. The DC values use the success-counting
/// system where each value represents successes needed.
/// </para>
/// <para>
/// Surface type is determined by the environment's floor composition and
/// can vary within a single room or area.
/// </para>
/// </remarks>
public enum StealthSurface
{
    /// <summary>
    /// Silent surface that absorbs sound.
    /// Base DC: 2 successes.
    /// </summary>
    /// <example>
    /// Thick carpet, moss-covered stone, deep sand, padded flooring.
    /// </example>
    Silent = 0,

    /// <summary>
    /// Normal surface with standard noise characteristics.
    /// Base DC: 3 successes.
    /// </summary>
    /// <example>
    /// Concrete, packed dirt, solid wood flooring, smooth stone.
    /// </example>
    Normal = 1,

    /// <summary>
    /// Noisy surface that amplifies footsteps.
    /// Base DC: 4 successes.
    /// </summary>
    /// <example>
    /// Loose rubble, gravel, dry leaves, creaky wooden boards.
    /// </example>
    Noisy = 2,

    /// <summary>
    /// Very noisy surface that makes silent movement nearly impossible.
    /// Base DC: 5 successes.
    /// </summary>
    /// <example>
    /// Scrap metal piles, broken glass, metal grating, chains.
    /// </example>
    VeryNoisy = 3
}
