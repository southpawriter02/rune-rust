namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Corruption tiers affecting all checks in an area.
/// </summary>
/// <remarks>
/// <para>
/// Corruption levels affect skill checks through DC modifiers in
/// <see cref="ValueObjects.EnvironmentModifier"/>.
/// </para>
/// <para>
/// DC Modifiers:
/// <list type="bullet">
///   <item><description>Normal: +0 DC</description></item>
///   <item><description>Glitched: +2 DC</description></item>
///   <item><description>Blighted: +4 DC</description></item>
///   <item><description>Resonance: +6 DC</description></item>
/// </list>
/// </para>
/// </remarks>
public enum CorruptionTier
{
    /// <summary>
    /// No corruption. No modifier.
    /// </summary>
    Normal = 0,

    /// <summary>
    /// Mild corruption. DC +2 to all checks.
    /// </summary>
    Glitched = 1,

    /// <summary>
    /// Significant corruption. DC +4 to all checks.
    /// </summary>
    Blighted = 2,

    /// <summary>
    /// Severe corruption. DC +6 to all checks.
    /// </summary>
    Resonance = 3
}
