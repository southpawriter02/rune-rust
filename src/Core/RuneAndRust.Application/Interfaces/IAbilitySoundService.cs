namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Triggers sound effects for ability usage.
/// </summary>
/// <remarks>
/// <para>
/// Provides ability sound triggering:
/// <list type="bullet">
///   <item><description>Ability casts by school (fire, ice, heal, etc.)</description></item>
///   <item><description>Buff and debuff application</description></item>
///   <item><description>Effect expiration</description></item>
/// </list>
/// </para>
/// </remarks>
public interface IAbilitySoundService
{
    /// <summary>
    /// Plays an ability cast sound based on school.
    /// </summary>
    /// <param name="school">The ability school (fire, ice, heal, buff, etc.).</param>
    void PlayAbilityCast(string? school);

    /// <summary>
    /// Plays a buff application sound.
    /// </summary>
    void PlayBuffApplied();

    /// <summary>
    /// Plays a debuff application sound.
    /// </summary>
    void PlayDebuffApplied();

    /// <summary>
    /// Plays a healing received sound.
    /// </summary>
    void PlayHealReceived();

    /// <summary>
    /// Plays an effect expired sound.
    /// </summary>
    void PlayEffectExpired();
}
