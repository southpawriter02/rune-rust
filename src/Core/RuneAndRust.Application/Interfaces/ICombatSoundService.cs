namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Triggers sound effects for combat actions.
/// </summary>
/// <remarks>
/// <para>
/// Provides combat sound triggering:
/// <list type="bullet">
///   <item><description>Attack hits (normal and critical)</description></item>
///   <item><description>Damage type-specific sounds</description></item>
///   <item><description>Death sounds for monsters and players</description></item>
/// </list>
/// </para>
/// </remarks>
public interface ICombatSoundService
{
    /// <summary>
    /// Plays an attack hit sound.
    /// </summary>
    /// <param name="isCritical">Whether this is a critical hit.</param>
    void PlayAttackHit(bool isCritical = false);

    /// <summary>
    /// Plays an attack miss sound.
    /// </summary>
    void PlayAttackMiss();

    /// <summary>
    /// Plays an attack blocked sound.
    /// </summary>
    void PlayAttackBlocked();

    /// <summary>
    /// Plays a damage sound based on damage type.
    /// </summary>
    /// <param name="damageType">The damage type (fire, ice, lightning, etc.).</param>
    void PlayDamage(string? damageType);

    /// <summary>
    /// Plays a monster death sound.
    /// </summary>
    void PlayMonsterDeath();

    /// <summary>
    /// Plays a player death sound.
    /// </summary>
    void PlayPlayerDeath();
}
