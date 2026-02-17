using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Services;

/// <summary>
/// Service handling Seiðkona Aether Resonance resource management.
/// Tracks per-character Resonance state including build, reset, and accumulated
/// Aetheric damage tracking for the Unraveling capstone.
/// </summary>
/// <remarks>
/// <para>Named <c>SeidkonaResonanceService</c> following per-specialization naming convention
/// to avoid collision with other resource services.</para>
/// <para>Key behaviors:</para>
/// <list type="bullet">
/// <item>Resonance builds are capped at <see cref="AetherResonanceResource.DefaultMaxResonance"/> (10)</item>
/// <item>Threshold crossings into Corruption risk tiers are logged at Warning level</item>
/// <item>Accumulated Aetheric Damage uses immutable swap pattern — modifications return new instances</item>
/// <item>All modifications update the player's resource properties directly</item>
/// </list>
/// </remarks>
public class SeidkonaResonanceService : ISeidkonaResonanceService
{
    /// <summary>
    /// The specialization ID string for Seiðkona.
    /// </summary>
    private const string SeidkonaSpecId = "seidkona";

    private readonly ILogger<SeidkonaResonanceService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="SeidkonaResonanceService"/> class.
    /// </summary>
    /// <param name="logger">Logger for Resonance state changes and threshold transitions.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="logger"/> is null.</exception>
    public SeidkonaResonanceService(ILogger<SeidkonaResonanceService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public void InitializeResonance(Player player)
    {
        ArgumentNullException.ThrowIfNull(player);

        player.InitializeAetherResonance();

        _logger.LogInformation(
            "Aether Resonance initialized: {Player} ({PlayerId}) Resonance set to 0/{Max}. " +
            "Accumulated Aetheric Damage tracker initialized",
            player.Name, player.Id, AetherResonanceResource.DefaultMaxResonance);
    }

    /// <inheritdoc />
    public AetherResonanceResource? GetResonance(Player player)
    {
        ArgumentNullException.ThrowIfNull(player);
        return player.AetherResonance;
    }

    /// <inheritdoc />
    public int BuildResonance(Player player, int amount, string source)
    {
        ArgumentNullException.ThrowIfNull(player);

        if (player.AetherResonance == null)
        {
            _logger.LogWarning(
                "BuildResonance failed: {Player} ({PlayerId}) has no Aether Resonance resource initialized",
                player.Name, player.Id);
            return 0;
        }

        // Capture previous state for threshold transition detection
        var previousLevel = player.AetherResonance.GetResonanceLevel();
        var previousResonance = player.AetherResonance.CurrentResonance;

        // Apply the Resonance build
        var actualGain = player.AetherResonance.Build(amount, source);

        if (actualGain <= 0)
            return 0;

        // Detect threshold transitions into higher Corruption risk tiers
        var newLevel = player.AetherResonance.GetResonanceLevel();
        if (newLevel != previousLevel)
        {
            LogThresholdTransition(player, previousLevel, newLevel,
                previousResonance, player.AetherResonance.CurrentResonance);
        }

        _logger.LogInformation(
            "Resonance built: {Player} ({PlayerId}) +{Gained} Resonance from {Source}. " +
            "Now {Current}/{Max} [{Level} — {RiskPercent}% Corruption risk]",
            player.Name, player.Id, actualGain, source,
            player.AetherResonance.CurrentResonance, player.AetherResonance.MaxResonance,
            newLevel, player.AetherResonance.GetCorruptionRiskPercent());

        return actualGain;
    }

    /// <inheritdoc />
    public void ResetResonance(Player player, string source)
    {
        ArgumentNullException.ThrowIfNull(player);

        if (player.AetherResonance == null)
        {
            _logger.LogWarning(
                "ResetResonance failed: {Player} ({PlayerId}) has no Aether Resonance resource initialized",
                player.Name, player.Id);
            return;
        }

        var previousResonance = player.AetherResonance.CurrentResonance;

        player.AetherResonance.Reset(source);

        _logger.LogInformation(
            "Resonance reset: {Player} ({PlayerId}) Resonance {Previous} → 0/{Max} ({Source})",
            player.Name, player.Id, previousResonance,
            AetherResonanceResource.DefaultMaxResonance, source);
    }

    /// <inheritdoc />
    public AccumulatedAethericDamage? GetAccumulatedDamage(Player player)
    {
        ArgumentNullException.ThrowIfNull(player);
        return player.AccumulatedAethericDamage;
    }

    /// <inheritdoc />
    public void AddAccumulatedDamage(Player player, int damage)
    {
        ArgumentNullException.ThrowIfNull(player);

        if (player.AccumulatedAethericDamage == null)
        {
            _logger.LogWarning(
                "AddAccumulatedDamage failed: {Player} ({PlayerId}) has no Accumulated Aetheric Damage tracker initialized",
                player.Name, player.Id);
            return;
        }

        // Immutable swap — AddDamage returns a new instance
        var updated = player.AccumulatedAethericDamage.AddDamage(damage);
        player.SetAccumulatedAethericDamage(updated);

        _logger.LogInformation(
            "Aetheric damage accumulated: {Player} ({PlayerId}) +{Damage} damage. " +
            "Total: {Total} from {Casts} casts",
            player.Name, player.Id, damage,
            updated.TotalDamage, updated.CastCount);
    }

    /// <inheritdoc />
    public void ResetAccumulatedDamage(Player player)
    {
        ArgumentNullException.ThrowIfNull(player);

        if (player.AccumulatedAethericDamage == null)
        {
            _logger.LogWarning(
                "ResetAccumulatedDamage failed: {Player} ({PlayerId}) has no Accumulated Aetheric Damage tracker initialized",
                player.Name, player.Id);
            return;
        }

        var previousTotal = player.AccumulatedAethericDamage.TotalDamage;
        var previousCasts = player.AccumulatedAethericDamage.CastCount;

        var reset = player.AccumulatedAethericDamage.Reset();
        player.SetAccumulatedAethericDamage(reset);

        _logger.LogInformation(
            "Accumulated Aetheric Damage reset: {Player} ({PlayerId}) " +
            "released {Total} damage from {Casts} casts",
            player.Name, player.Id, previousTotal, previousCasts);
    }

    /// <summary>
    /// Logs a Resonance threshold transition with appropriate severity.
    /// Crossing into Corruption risk tiers (Risky, Dangerous, Critical) logs at Warning level;
    /// returning to Safe logs at Information level.
    /// </summary>
    /// <param name="player">The player experiencing the transition.</param>
    /// <param name="previousLevel">The Resonance level name before the change.</param>
    /// <param name="newLevel">The Resonance level name after the change.</param>
    /// <param name="previousResonance">The Resonance value before the change.</param>
    /// <param name="currentResonance">The Resonance value after the change.</param>
    private void LogThresholdTransition(
        Player player,
        string previousLevel,
        string newLevel,
        int previousResonance,
        int currentResonance)
    {
        // Entering any Corruption risk tier is Warning-level
        if (newLevel != "Safe")
        {
            _logger.LogWarning(
                "RESONANCE THRESHOLD — {Player} ({PlayerId}) entered {NewLevel} zone! " +
                "Resonance {Previous} → {Current}. Corruption risk is now active " +
                "({RiskPercent}% per cast)",
                player.Name, player.Id, newLevel, previousResonance, currentResonance,
                player.AetherResonance!.GetCorruptionRiskPercent());
        }
        else
        {
            _logger.LogInformation(
                "Resonance threshold transition: {Player} ({PlayerId}) {PreviousLevel} → {NewLevel}. " +
                "Resonance {Previous} → {Current}",
                player.Name, player.Id, previousLevel, newLevel,
                previousResonance, currentResonance);
        }
    }
}
