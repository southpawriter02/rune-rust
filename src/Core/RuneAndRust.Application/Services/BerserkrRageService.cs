using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Services;

/// <summary>
/// Service handling Berserkr Rage resource management.
/// Tracks per-character Rage state including gain, spend, decay, and threshold transitions.
/// </summary>
/// <remarks>
/// <para>Named <c>BerserkrRageService</c> to coexist alongside the v0.18.4d <c>RageService</c>
/// which handles general rage mechanics. This service is Berserkr-specialization-specific.</para>
/// <para>Key behaviors:</para>
/// <list type="bullet">
/// <item>Rage gains are capped at <see cref="RageResource.DefaultMaxRage"/> (100)</item>
/// <item>Threshold transitions (e.g., Calm → Irritated) are logged at Information level</item>
/// <item>Entering Enraged state (80+) is logged at Warning level due to Corruption risk</item>
/// <item>All modifications update the player's <see cref="Player.Rage"/> property directly</item>
/// </list>
/// </remarks>
public class BerserkrRageService : IBerserkrRageService
{
    /// <summary>
    /// The specialization ID string for Berserkr.
    /// </summary>
    private const string BerserkrSpecId = "berserkr";

    private readonly ILogger<BerserkrRageService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="BerserkrRageService"/> class.
    /// </summary>
    /// <param name="logger">Logger for Rage state changes and threshold transitions.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="logger"/> is null.</exception>
    public BerserkrRageService(ILogger<BerserkrRageService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public void InitializeRage(Player player)
    {
        ArgumentNullException.ThrowIfNull(player);

        player.InitializeRageResource();

        _logger.LogInformation(
            "Rage initialized: {Player} ({PlayerId}) Rage set to {Current}/{Max}",
            player.Name, player.Id, 0, RageResource.DefaultMaxRage);
    }

    /// <inheritdoc />
    public RageResource? GetRage(Player player)
    {
        ArgumentNullException.ThrowIfNull(player);
        return player.Rage;
    }

    /// <inheritdoc />
    public int AddRage(Player player, int amount, string source)
    {
        ArgumentNullException.ThrowIfNull(player);

        if (player.Rage == null)
        {
            _logger.LogWarning(
                "AddRage failed: {Player} ({PlayerId}) has no Rage resource initialized",
                player.Name, player.Id);
            return 0;
        }

        // Capture previous state for threshold transition detection
        var previousLevel = player.Rage.GetRageLevel();
        var previousRage = player.Rage.CurrentRage;

        // Apply the Rage gain
        var actualGain = player.Rage.Gain(amount, source);

        if (actualGain <= 0)
            return 0;

        // Detect threshold transitions
        var newLevel = player.Rage.GetRageLevel();
        if (newLevel != previousLevel)
        {
            LogThresholdTransition(player, previousLevel, newLevel, previousRage, player.Rage.CurrentRage);
        }

        _logger.LogInformation(
            "Rage gained: {Player} ({PlayerId}) +{Gained} Rage from {Source}. " +
            "Now {Current}/{Max} [{Level}]",
            player.Name, player.Id, actualGain, source,
            player.Rage.CurrentRage, player.Rage.MaxRage, newLevel);

        return actualGain;
    }

    /// <inheritdoc />
    public bool SpendRage(Player player, int amount)
    {
        ArgumentNullException.ThrowIfNull(player);

        if (player.Rage == null)
        {
            _logger.LogWarning(
                "SpendRage failed: {Player} ({PlayerId}) has no Rage resource initialized",
                player.Name, player.Id);
            return false;
        }

        // Capture previous state for threshold transition detection
        var previousLevel = player.Rage.GetRageLevel();
        var previousRage = player.Rage.CurrentRage;

        // Attempt to spend Rage
        if (!player.Rage.Spend(amount))
        {
            _logger.LogWarning(
                "SpendRage failed: {Player} ({PlayerId}) has insufficient Rage " +
                "(need {Required}, have {Available})",
                player.Name, player.Id, amount, player.Rage.CurrentRage);
            return false;
        }

        // Detect threshold transitions
        var newLevel = player.Rage.GetRageLevel();
        if (newLevel != previousLevel)
        {
            LogThresholdTransition(player, previousLevel, newLevel, previousRage, player.Rage.CurrentRage);
        }

        _logger.LogInformation(
            "Rage spent: {Player} ({PlayerId}) -{Spent} Rage. " +
            "Now {Current}/{Max} [{Level}]",
            player.Name, player.Id, amount,
            player.Rage.CurrentRage, player.Rage.MaxRage, newLevel);

        return true;
    }

    /// <inheritdoc />
    public int DecayRageOutOfCombat(Player player)
    {
        ArgumentNullException.ThrowIfNull(player);

        if (player.Rage == null)
        {
            _logger.LogWarning(
                "DecayRage failed: {Player} ({PlayerId}) has no Rage resource initialized",
                player.Name, player.Id);
            return 0;
        }

        // Capture previous state
        var previousLevel = player.Rage.GetRageLevel();
        var previousRage = player.Rage.CurrentRage;

        // Apply decay
        var decayed = player.Rage.DecayOutOfCombat();

        if (decayed <= 0)
            return 0;

        // Detect threshold transitions
        var newLevel = player.Rage.GetRageLevel();
        if (newLevel != previousLevel)
        {
            LogThresholdTransition(player, previousLevel, newLevel, previousRage, player.Rage.CurrentRage);
        }

        _logger.LogInformation(
            "Rage decayed: {Player} ({PlayerId}) -{Decayed} Rage (out of combat). " +
            "Now {Current}/{Max} [{Level}]",
            player.Name, player.Id, decayed,
            player.Rage.CurrentRage, player.Rage.MaxRage, newLevel);

        return decayed;
    }

    /// <inheritdoc />
    public void ResetRage(Player player)
    {
        ArgumentNullException.ThrowIfNull(player);

        if (player.Rage == null)
        {
            _logger.LogWarning(
                "ResetRage failed: {Player} ({PlayerId}) has no Rage resource initialized",
                player.Name, player.Id);
            return;
        }

        var previousRage = player.Rage.CurrentRage;

        // Re-initialize to zero
        player.InitializeRageResource();

        _logger.LogInformation(
            "Rage reset: {Player} ({PlayerId}) Rage {Previous} → 0/{Max} (rest/session end)",
            player.Name, player.Id, previousRage, RageResource.DefaultMaxRage);
    }

    /// <inheritdoc />
    public IEnumerable<Player> GetEnragedCharacters(IEnumerable<Player> players)
    {
        ArgumentNullException.ThrowIfNull(players);

        return players.Where(p =>
            p.Rage != null &&
            p.Rage.IsEnraged &&
            IsBerserkr(p));
    }

    /// <summary>
    /// Logs a Rage threshold transition with appropriate severity.
    /// Entering Enraged (80+) or Berserk (100) states logs at Warning level;
    /// other transitions log at Information level.
    /// </summary>
    /// <param name="player">The player experiencing the transition.</param>
    /// <param name="previousLevel">The Rage level before the change.</param>
    /// <param name="newLevel">The Rage level after the change.</param>
    /// <param name="previousRage">The Rage value before the change.</param>
    /// <param name="currentRage">The Rage value after the change.</param>
    private void LogThresholdTransition(
        Player player,
        RageLevel previousLevel,
        RageLevel newLevel,
        int previousRage,
        int currentRage)
    {
        // Enraged and Berserk transitions are Warning-level due to Corruption risk
        if (newLevel >= RageLevel.Enraged && previousLevel < RageLevel.Enraged)
        {
            _logger.LogWarning(
                "RAGE THRESHOLD — {Player} ({PlayerId}) entered {NewLevel} state! " +
                "Rage {Previous} → {Current}. Corruption risk is now ACTIVE",
                player.Name, player.Id, newLevel, previousRage, currentRage);
        }
        else
        {
            _logger.LogInformation(
                "Rage threshold transition: {Player} ({PlayerId}) {PreviousLevel} → {NewLevel}. " +
                "Rage {Previous} → {Current}",
                player.Name, player.Id, previousLevel, newLevel, previousRage, currentRage);
        }
    }

    /// <summary>
    /// Checks if a player is a Berserkr.
    /// </summary>
    /// <param name="player">The player to check.</param>
    /// <returns>True if the player's specialization is "berserkr".</returns>
    private static bool IsBerserkr(Player player)
    {
        return string.Equals(player.SpecializationId, BerserkrSpecId, StringComparison.OrdinalIgnoreCase);
    }
}
