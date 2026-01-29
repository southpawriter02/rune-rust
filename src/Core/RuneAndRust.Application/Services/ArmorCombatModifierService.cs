// =============================================================================
// ArmorCombatModifierService.cs
// =============================================================================
// v0.16.2f - Combat Integration
// =============================================================================
// Service implementation for applying armor penalties to combat actions.
// Integrates IArmorPenaltyCalculator and IArchetypeArmorProficiencyProvider
// to provide complete combat modifier information.
// =============================================================================

using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Services;

/// <summary>
/// Service for applying armor penalties to combat actions.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="ArmorCombatModifierService"/> integrates penalty calculation and archetype
/// proficiency data to provide complete combat modifier information. It serves as the
/// primary interface for combat services and UI components to query armor effects.
/// </para>
/// <para>
/// Dependencies:
/// </para>
/// <list type="bullet">
/// <item><description><see cref="IArmorPenaltyCalculator"/> - Calculates effective penalties</description></item>
/// <item><description><see cref="IArchetypeArmorProficiencyProvider"/> - Provides archetype proficiency and Galdr rules</description></item>
/// <item><description><see cref="ILogger{TCategoryName}"/> - Structured logging</description></item>
/// </list>
/// </remarks>
/// <seealso cref="IArmorCombatModifierService"/>
/// <seealso cref="CombatArmorState"/>
public class ArmorCombatModifierService : IArmorCombatModifierService
{
    // =========================================================================
    // Fields
    // =========================================================================

    private readonly IArmorPenaltyCalculator _penaltyCalculator;
    private readonly IArchetypeArmorProficiencyProvider _archetypeProvider;
    private readonly ILogger<ArmorCombatModifierService> _logger;

    // =========================================================================
    // Constructor
    // =========================================================================

    /// <summary>
    /// Initializes a new instance of <see cref="ArmorCombatModifierService"/>.
    /// </summary>
    /// <param name="penaltyCalculator">The penalty calculator for effective penalties.</param>
    /// <param name="archetypeProvider">The archetype proficiency provider.</param>
    /// <param name="logger">The logger for structured logging.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when any required dependency is null.
    /// </exception>
    public ArmorCombatModifierService(
        IArmorPenaltyCalculator penaltyCalculator,
        IArchetypeArmorProficiencyProvider archetypeProvider,
        ILogger<ArmorCombatModifierService> logger)
    {
        _penaltyCalculator = penaltyCalculator
            ?? throw new ArgumentNullException(nameof(penaltyCalculator));
        _archetypeProvider = archetypeProvider
            ?? throw new ArgumentNullException(nameof(archetypeProvider));
        _logger = logger
            ?? throw new ArgumentNullException(nameof(logger));

        _logger.LogDebug("ArmorCombatModifierService initialized");
    }

    // =========================================================================
    // Complete State Methods
    // =========================================================================

    /// <inheritdoc/>
    public CombatArmorState GetCombatState(string archetypeId, ArmorCategory armorCategory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(archetypeId);

        var normalizedArchetypeId = archetypeId.ToLowerInvariant();

        _logger.LogDebug(
            "Calculating combat state for archetype {ArchetypeId} with {ArmorCategory} armor",
            normalizedArchetypeId, armorCategory);

        // Get the archetype's proficiency level for this armor category
        var proficiencyLevel = _archetypeProvider.GetStartingProficiency(
            normalizedArchetypeId, armorCategory);

        // Calculate effective penalties based on category and proficiency
        var penalties = _penaltyCalculator.CalculatePenalties(armorCategory, proficiencyLevel);

        // Check Galdr interference
        var galdrBlocked = _archetypeProvider.IsGaldrBlocked(normalizedArchetypeId, armorCategory);
        var galdrPenalty = galdrBlocked
            ? 0
            : _archetypeProvider.GetGaldrPenalty(normalizedArchetypeId, armorCategory);

        // Determine stealth disadvantage
        var stealthDisadvantage = penalties.EffectivePenalties.HasStealthDisadvantage;

        // Build display warnings
        var warnings = BuildWarnings(penalties, galdrBlocked, galdrPenalty);

        // Log significant state information
        if (galdrBlocked)
        {
            _logger.LogWarning(
                "Galdr BLOCKED for {ArchetypeId} wearing {ArmorCategory} armor",
                normalizedArchetypeId, armorCategory);
        }
        else if (galdrPenalty != 0)
        {
            _logger.LogDebug(
                "Galdr penalty {Penalty} for {ArchetypeId} wearing {ArmorCategory} armor",
                galdrPenalty, normalizedArchetypeId, armorCategory);
        }

        _logger.LogDebug(
            "Combat state for {ArchetypeId}: {PenaltySummary}, GaldrBlocked={Blocked}, GaldrPenalty={Penalty}",
            normalizedArchetypeId, penalties.FormatSummary(), galdrBlocked, galdrPenalty);

        return CombatArmorState.Create(
            penalties: penalties,
            galdrBlocked: galdrBlocked,
            galdrPenalty: galdrPenalty,
            stealthDisadvantage: stealthDisadvantage,
            warnings: warnings);
    }

    // =========================================================================
    // Galdr Interference Methods
    // =========================================================================

    /// <inheritdoc/>
    public bool CanCastGaldr(string archetypeId, ArmorCategory armorCategory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(archetypeId);

        var normalizedArchetypeId = archetypeId.ToLowerInvariant();
        var isBlocked = _archetypeProvider.IsGaldrBlocked(normalizedArchetypeId, armorCategory);

        _logger.LogDebug(
            "Galdr casting check for {ArchetypeId} with {ArmorCategory}: {CanCast}",
            normalizedArchetypeId, armorCategory, !isBlocked ? "allowed" : "BLOCKED");

        return !isBlocked;
    }

    /// <inheritdoc/>
    public int GetGaldrPenalty(string archetypeId, ArmorCategory armorCategory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(archetypeId);

        var normalizedArchetypeId = archetypeId.ToLowerInvariant();

        // If blocked, penalty is irrelevant - return 0
        if (_archetypeProvider.IsGaldrBlocked(normalizedArchetypeId, armorCategory))
        {
            _logger.LogDebug(
                "Galdr penalty for {ArchetypeId}: N/A (blocked by {ArmorCategory})",
                normalizedArchetypeId, armorCategory);
            return 0;
        }

        var penalty = _archetypeProvider.GetGaldrPenalty(normalizedArchetypeId, armorCategory);

        _logger.LogDebug(
            "Galdr penalty for {ArchetypeId} with {ArmorCategory}: {Penalty}",
            normalizedArchetypeId, armorCategory, penalty);

        return penalty;
    }

    // =========================================================================
    // Individual Modifier Methods
    // =========================================================================

    /// <inheritdoc/>
    public int GetAgilityModifier(ArmorCategory armorCategory, ArmorProficiencyLevel proficiencyLevel)
    {
        var penalties = _penaltyCalculator.CalculatePenalties(armorCategory, proficiencyLevel);
        return penalties.EffectivePenalties.AgilityDicePenalty;
    }

    /// <inheritdoc/>
    public int GetStaminaCostModifier(ArmorCategory armorCategory, ArmorProficiencyLevel proficiencyLevel)
    {
        var penalties = _penaltyCalculator.CalculatePenalties(armorCategory, proficiencyLevel);
        return penalties.EffectivePenalties.StaminaCostModifier;
    }

    /// <inheritdoc/>
    public int GetMovementModifier(ArmorCategory armorCategory, ArmorProficiencyLevel proficiencyLevel)
    {
        var penalties = _penaltyCalculator.CalculatePenalties(armorCategory, proficiencyLevel);
        return penalties.EffectivePenalties.MovementPenalty;
    }

    /// <inheritdoc/>
    public int GetAttackModifier(ArmorCategory armorCategory, ArmorProficiencyLevel proficiencyLevel)
    {
        var penalties = _penaltyCalculator.CalculatePenalties(armorCategory, proficiencyLevel);
        return penalties.AttackModifier;
    }

    /// <inheritdoc/>
    public int GetDefenseModifier(ArmorCategory armorCategory, ArmorProficiencyLevel proficiencyLevel)
    {
        var penalties = _penaltyCalculator.CalculatePenalties(armorCategory, proficiencyLevel);
        return penalties.DefenseModifier;
    }

    /// <inheritdoc/>
    public bool HasStealthDisadvantage(ArmorCategory armorCategory, ArmorProficiencyLevel proficiencyLevel)
    {
        var penalties = _penaltyCalculator.CalculatePenalties(armorCategory, proficiencyLevel);
        return penalties.EffectivePenalties.HasStealthDisadvantage;
    }

    // =========================================================================
    // Private Helper Methods
    // =========================================================================

    /// <summary>
    /// Builds display warnings based on penalties and Galdr interference.
    /// </summary>
    /// <param name="penalties">The effective armor penalties.</param>
    /// <param name="galdrBlocked">Whether Galdr is blocked.</param>
    /// <param name="galdrPenalty">The Galdr penalty (if any).</param>
    /// <returns>A list of warning strings for UI display.</returns>
    private static IReadOnlyList<string> BuildWarnings(
        EffectiveArmorPenalties penalties,
        bool galdrBlocked,
        int galdrPenalty)
    {
        var warnings = new List<string>();

        // Galdr warnings (highest priority)
        if (galdrBlocked)
        {
            warnings.Add("Galdr casting is BLOCKED by your armor");
        }
        else if (galdrPenalty != 0)
        {
            warnings.Add($"Galdr casting penalty: {galdrPenalty}");
        }

        // Attack penalty warning
        if (penalties.AttackModifier < 0)
        {
            warnings.Add($"Attack penalty: {penalties.AttackModifier}");
        }

        // Non-proficiency warning
        if (penalties.WasMultiplied)
        {
            warnings.Add("Penalties DOUBLED due to lack of proficiency");
        }

        // Stealth warning
        if (penalties.EffectivePenalties.HasStealthDisadvantage)
        {
            warnings.Add("Stealth checks have disadvantage");
        }

        // Defense bonus indication (positive feedback)
        if (penalties.HasDefenseBonus)
        {
            warnings.Add($"Defense bonus: +{penalties.DefenseModifier}");
        }

        return warnings;
    }
}
