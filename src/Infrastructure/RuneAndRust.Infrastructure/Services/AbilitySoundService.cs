namespace RuneAndRust.Infrastructure.Services;

using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;

/// <summary>
/// Triggers sound effects for ability usage.
/// </summary>
/// <remarks>
/// <para>
/// Features:
/// <list type="bullet">
///   <item><description>Ability school-specific cast sounds</description></item>
///   <item><description>Buff and debuff application sounds</description></item>
///   <item><description>Effect expiration sounds</description></item>
/// </list>
/// </para>
/// </remarks>
public class AbilitySoundService : IAbilitySoundService
{
    private readonly ISoundEffectService _sfx;
    private readonly ILogger<AbilitySoundService> _logger;

    /// <summary>
    /// Creates a new ability sound service.
    /// </summary>
    /// <param name="sfx">Sound effect service for playback.</param>
    /// <param name="logger">Logger for ability sounds.</param>
    public AbilitySoundService(ISoundEffectService sfx, ILogger<AbilitySoundService> logger)
    {
        _sfx = sfx;
        _logger = logger;
        _logger.LogDebug("AbilitySoundService initialized");
    }

    /// <inheritdoc />
    public void PlayAbilityCast(string? school)
    {
        var effectId = MapAbilitySchoolToEffect(school);
        _sfx.Play(effectId);
        _logger.LogDebug("Ability sound: {School} cast → {EffectId}", school ?? "generic", effectId);
    }

    /// <inheritdoc />
    public void PlayBuffApplied()
    {
        _sfx.Play("ability-buff");
        _logger.LogDebug("Ability sound: buff applied");
    }

    /// <inheritdoc />
    public void PlayDebuffApplied()
    {
        _sfx.Play("ability-debuff", volume: 0.7f);
        _logger.LogDebug("Ability sound: debuff applied");
    }

    /// <inheritdoc />
    public void PlayHealReceived()
    {
        _sfx.Play("ability-heal");
        _logger.LogDebug("Ability sound: healing received");
    }

    /// <inheritdoc />
    public void PlayEffectExpired()
    {
        _sfx.Play("ability-expire", volume: 0.5f);
        _logger.LogDebug("Ability sound: effect expired");
    }

    /// <summary>
    /// Maps an ability school to a sound effect ID.
    /// </summary>
    private static string MapAbilitySchoolToEffect(string? school) => school?.ToLowerInvariant() switch
    {
        "fire" or "evocation" or "pyromancy" => "ability-fire",
        "ice" or "cold" or "frost" or "cryomancy" => "ability-ice",
        "lightning" or "storm" or "electromancy" => "ability-lightning",
        "healing" or "restoration" or "holy" => "ability-heal",
        "buff" or "enhancement" or "abjuration" => "ability-buff",
        "shadow" or "dark" or "necromancy" => "ability-shadow",
        "nature" or "druid" => "ability-nature",
        _ => "ability-cast" // Generic fallback
    };
}
