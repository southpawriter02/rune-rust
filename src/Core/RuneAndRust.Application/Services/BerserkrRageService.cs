// ═══════════════════════════════════════════════════════════════════════════════
// BerserkrRageService.cs
// Application service managing the Berserkr's specialization-specific Rage
// resource. Tracks per-character Rage with structured logging.
// Version: 0.20.5a
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Application.Services;

using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Manages the Berserkr specialization's Rage resource for all active characters.
/// </summary>
/// <remarks>
/// <para>
/// Tracks Rage via an in-memory dictionary keyed by character ID. This service
/// handles initialization, gain, spending, decay, and Enraged state monitoring.
/// </para>
/// <para>
/// Distinct from <see cref="RageService"/> (v0.18.4d) which manages the general
/// Trauma Economy Rage resource with damage/soak bonuses.
/// </para>
/// </remarks>
/// <seealso cref="IBerserkrRageService"/>
/// <seealso cref="RageResource"/>
public class BerserkrRageService(ILogger<BerserkrRageService> logger)
    : IBerserkrRageService
{
    private readonly ILogger<BerserkrRageService> _logger = logger;
    private readonly Dictionary<Guid, RageResource> _characterRage = new();

    // ─────────────────────────────────────────────────────────────────────────
    // Initialization
    // ─────────────────────────────────────────────────────────────────────────

    /// <inheritdoc />
    public void InitializeRage(Guid characterId)
    {
        _characterRage[characterId] = RageResource.Create();

        _logger.LogInformation(
            "Initialized Berserkr Rage for character {CharacterId}: 0/{MaxRage}",
            characterId, RageResource.DefaultMaxRage);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Query
    // ─────────────────────────────────────────────────────────────────────────

    /// <inheritdoc />
    public RageResource? GetRage(Guid characterId)
    {
        _characterRage.TryGetValue(characterId, out var rage);

        _logger.LogDebug(
            "GetRage for {CharacterId}: {Result}",
            characterId, rage?.GetStatusString() ?? "not initialized");

        return rage;
    }

    /// <inheritdoc />
    public IEnumerable<Guid> GetEnragedCharacters()
    {
        var enraged = _characterRage
            .Where(kvp => kvp.Value.IsEnraged)
            .Select(kvp => kvp.Key)
            .ToList();

        _logger.LogDebug(
            "GetEnragedCharacters: {Count} characters at 80+ Rage",
            enraged.Count);

        return enraged;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Resource Operations
    // ─────────────────────────────────────────────────────────────────────────

    /// <inheritdoc />
    public void AddRage(Guid characterId, int amount, string source)
    {
        if (!_characterRage.TryGetValue(characterId, out var rage))
        {
            _logger.LogWarning(
                "AddRage called for uninitialized character {CharacterId}",
                characterId);
            return;
        }

        var previousRage = rage.CurrentRage;
        var previousLevel = rage.GetRageLevel();

        rage.Gain(amount, source);

        var newLevel = rage.GetRageLevel();

        _logger.LogInformation(
            "Rage gained for {CharacterId}: +{Amount} from {Source} " +
            "({PreviousRage} → {CurrentRage})",
            characterId, amount, source, previousRage, rage.CurrentRage);

        if (newLevel != previousLevel)
        {
            _logger.LogInformation(
                "Rage level changed for {CharacterId}: {PreviousLevel} → {NewLevel}",
                characterId, previousLevel, newLevel);
        }

        if (rage.IsEnraged && previousRage < RageResource.EnragedThreshold)
        {
            _logger.LogWarning(
                "Character {CharacterId} entered ENRAGED state ({CurrentRage} Rage). " +
                "Corruption risk active for ability usage",
                characterId, rage.CurrentRage);
        }
    }

    /// <inheritdoc />
    public bool SpendRage(Guid characterId, int amount)
    {
        if (!_characterRage.TryGetValue(characterId, out var rage))
        {
            _logger.LogWarning(
                "SpendRage called for uninitialized character {CharacterId}",
                characterId);
            return false;
        }

        var previousRage = rage.CurrentRage;
        var success = rage.Spend(amount);

        if (success)
        {
            _logger.LogInformation(
                "Rage spent for {CharacterId}: -{Amount} ({PreviousRage} → {CurrentRage})",
                characterId, amount, previousRage, rage.CurrentRage);
        }
        else
        {
            _logger.LogDebug(
                "Insufficient Rage for {CharacterId}: needed {Amount}, has {CurrentRage}",
                characterId, amount, rage.CurrentRage);
        }

        return success;
    }

    /// <inheritdoc />
    public void DecayRageOutOfCombat(Guid characterId)
    {
        if (!_characterRage.TryGetValue(characterId, out var rage))
        {
            _logger.LogDebug(
                "DecayRage called for uninitialized character {CharacterId}",
                characterId);
            return;
        }

        if (rage.CurrentRage <= 0) return;

        var previousRage = rage.CurrentRage;
        rage.DecayOutOfCombat();

        _logger.LogDebug(
            "Rage decay for {CharacterId}: {PreviousRage} → {CurrentRage} " +
            "(-{DecayAmount} out-of-combat)",
            characterId, previousRage, rage.CurrentRage, RageResource.OutOfCombatDecay);
    }

    /// <inheritdoc />
    public void ResetRage(Guid characterId)
    {
        if (_characterRage.ContainsKey(characterId))
        {
            _characterRage[characterId] = RageResource.Create();

            _logger.LogInformation(
                "Rage reset to 0 for character {CharacterId}",
                characterId);
        }
    }
}
