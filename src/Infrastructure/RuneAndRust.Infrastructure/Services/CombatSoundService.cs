namespace RuneAndRust.Infrastructure.Services;

using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;

/// <summary>
/// Triggers sound effects for combat actions.
/// </summary>
/// <remarks>
/// <para>
/// Features:
/// <list type="bullet">
///   <item><description>Attack hit sounds with critical detection</description></item>
///   <item><description>Damage type-specific sounds</description></item>
///   <item><description>Death sounds for monsters and players</description></item>
/// </list>
/// </para>
/// </remarks>
public class CombatSoundService : ICombatSoundService
{
    private readonly ISoundEffectService _sfx;
    private readonly ILogger<CombatSoundService> _logger;

    /// <summary>
    /// Creates a new combat sound service.
    /// </summary>
    /// <param name="sfx">Sound effect service for playback.</param>
    /// <param name="logger">Logger for combat sounds.</param>
    public CombatSoundService(ISoundEffectService sfx, ILogger<CombatSoundService> logger)
    {
        _sfx = sfx;
        _logger = logger;
        _logger.LogDebug("CombatSoundService initialized");
    }

    /// <inheritdoc />
    public void PlayAttackHit(bool isCritical = false)
    {
        if (isCritical)
        {
            _sfx.Play("attack-critical");
            _logger.LogDebug("Combat sound: critical hit");
        }
        else
        {
            _sfx.Play("attack-hit");
            _logger.LogDebug("Combat sound: attack hit");
        }
    }

    /// <inheritdoc />
    public void PlayAttackMiss()
    {
        _sfx.Play("attack-miss");
        _logger.LogDebug("Combat sound: attack miss");
    }

    /// <inheritdoc />
    public void PlayAttackBlocked()
    {
        _sfx.Play("attack-blocked");
        _logger.LogDebug("Combat sound: attack blocked");
    }

    /// <inheritdoc />
    public void PlayDamage(string? damageType)
    {
        var effectId = MapDamageTypeToEffect(damageType);
        if (effectId is not null)
        {
            _sfx.Play(effectId);
            _logger.LogDebug("Combat sound: {DamageType} damage → {EffectId}", damageType, effectId);
        }
        else
        {
            _logger.LogDebug("Combat sound: no sound for damage type {DamageType}", damageType);
        }
    }

    /// <inheritdoc />
    public void PlayMonsterDeath()
    {
        _sfx.Play("death-monster");
        _logger.LogDebug("Combat sound: monster death");
    }

    /// <inheritdoc />
    public void PlayPlayerDeath()
    {
        _sfx.Play("death-player");
        _logger.LogDebug("Combat sound: player death");
    }

    /// <summary>
    /// Maps a damage type to a sound effect ID.
    /// </summary>
    private static string? MapDamageTypeToEffect(string? damageType) => damageType?.ToLowerInvariant() switch
    {
        "fire" => "damage-fire",
        "ice" or "cold" or "frost" => "damage-ice",
        "lightning" or "electric" or "shock" => "damage-lightning",
        "poison" => "damage-poison",
        "holy" or "radiant" => "damage-holy",
        "shadow" or "dark" or "necrotic" => "damage-shadow",
        _ => null // No special sound for physical/untyped
    };
}
