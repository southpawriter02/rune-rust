namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Lighting conditions affecting visual tasks.
/// </summary>
/// <remarks>
/// <para>
/// Lighting levels affect skill checks for visual tasks like lockpicking,
/// perception, and examination through DC modifiers in <see cref="ValueObjects.EnvironmentModifier"/>.
/// </para>
/// <para>
/// Modifier values:
/// <list type="bullet">
///   <item><description>Bright: DC -1</description></item>
///   <item><description>Normal: DC +0</description></item>
///   <item><description>Dim: DC +1</description></item>
///   <item><description>Dark: DC +2</description></item>
/// </list>
/// </para>
/// </remarks>
public enum LightingLevel
{
    /// <summary>
    /// Excellent visibility. DC -1 for visual tasks.
    /// </summary>
    Bright = 0,

    /// <summary>
    /// Standard lighting. No modifier.
    /// </summary>
    Normal = 1,

    /// <summary>
    /// Reduced visibility. DC +1 for visual tasks.
    /// </summary>
    Dim = 2,

    /// <summary>
    /// Very low or no light. DC +2 for visual tasks.
    /// </summary>
    Dark = 3
}
