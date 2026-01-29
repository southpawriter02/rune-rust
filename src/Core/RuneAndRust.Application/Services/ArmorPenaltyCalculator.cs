// ═══════════════════════════════════════════════════════════════════════════════
// ArmorPenaltyCalculator.cs
// Service for calculating effective armor penalties based on proficiency.
// Version: 0.16.2d
// ═══════════════════════════════════════════════════════════════════════════════

using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Services;

/// <summary>
/// Calculates effective armor penalties based on category and proficiency level.
/// </summary>
/// <remarks>
/// <para>
/// ArmorPenaltyCalculator orchestrates the penalty calculation pipeline by
/// combining data from <see cref="IArmorCategoryProvider"/>,
/// <see cref="IArmorProficiencyEffectProvider"/>, and
/// <see cref="IArchetypeArmorProficiencyProvider"/>.
/// </para>
/// <para>
/// The calculation pipeline:
/// <list type="number">
///   <item><description>Get proficiency effect (multiplier, tier reduction, modifiers)</description></item>
///   <item><description>Calculate effective tier after reduction</description></item>
///   <item><description>Get base penalties for effective tier</description></item>
///   <item><description>Apply penalty multiplier if NonProficient</description></item>
///   <item><description>Build result with metadata</description></item>
/// </list>
/// </para>
/// </remarks>
/// <seealso cref="IArmorPenaltyCalculator"/>
/// <seealso cref="EffectiveArmorPenalties"/>
public class ArmorPenaltyCalculator : IArmorPenaltyCalculator
{
    // ═══════════════════════════════════════════════════════════════════════════
    // Dependencies
    // ═══════════════════════════════════════════════════════════════════════════

    private readonly IArmorCategoryProvider _categoryProvider;
    private readonly IArmorProficiencyEffectProvider _effectProvider;
    private readonly IArchetypeArmorProficiencyProvider _archetypeProvider;
    private readonly ILogger<ArmorPenaltyCalculator> _logger;

    // ═══════════════════════════════════════════════════════════════════════════
    // Constructor
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Initializes a new instance of <see cref="ArmorPenaltyCalculator"/>.
    /// </summary>
    /// <param name="categoryProvider">Provider for armor category definitions.</param>
    /// <param name="effectProvider">Provider for proficiency effect data.</param>
    /// <param name="archetypeProvider">Provider for archetype proficiency mappings.</param>
    /// <param name="logger">Logger for diagnostic output.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when any required parameter is null.
    /// </exception>
    public ArmorPenaltyCalculator(
        IArmorCategoryProvider categoryProvider,
        IArmorProficiencyEffectProvider effectProvider,
        IArchetypeArmorProficiencyProvider archetypeProvider,
        ILogger<ArmorPenaltyCalculator> logger)
    {
        ArgumentNullException.ThrowIfNull(categoryProvider);
        ArgumentNullException.ThrowIfNull(effectProvider);
        ArgumentNullException.ThrowIfNull(archetypeProvider);
        ArgumentNullException.ThrowIfNull(logger);

        _categoryProvider = categoryProvider;
        _effectProvider = effectProvider;
        _archetypeProvider = archetypeProvider;
        _logger = logger;

        _logger.LogDebug(
            "ArmorPenaltyCalculator initialized with providers: {CategoryProvider}, {EffectProvider}, {ArchetypeProvider}",
            categoryProvider.GetType().Name,
            effectProvider.GetType().Name,
            archetypeProvider.GetType().Name);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Core Calculation Methods
    // ═══════════════════════════════════════════════════════════════════════════

    /// <inheritdoc/>
    public EffectiveArmorPenalties CalculatePenalties(
        ArmorCategory category,
        ArmorProficiencyLevel proficiencyLevel)
    {
        _logger.LogDebug(
            "Calculating penalties for {Category} at {ProficiencyLevel}",
            category, proficiencyLevel);

        // Step 1: Get proficiency effect
        var effect = _effectProvider.GetEffect(proficiencyLevel);

        _logger.LogDebug(
            "Retrieved proficiency effect: Multiplier={Multiplier}, TierReduction={TierReduction}, " +
            "AttackMod={AttackMod}, DefenseMod={DefenseMod}",
            effect.PenaltyMultiplier, effect.TierReduction, effect.AttackModifier, effect.DefenseModifier);

        // Step 2: Get category definition and original tier
        var categoryDef = _categoryProvider.GetDefinition(category);
        int originalTier = categoryDef.WeightTier;

        // Step 3: Calculate effective tier after reduction
        int effectiveTier = CalculateEffectiveTier(originalTier, effect.TierReduction, category);
        bool wasTierReduced = effectiveTier < originalTier && originalTier >= 0;

        _logger.LogDebug(
            "Tier calculation: Original={OriginalTier}, Reduction={Reduction}, Effective={EffectiveTier}, WasReduced={WasReduced}",
            originalTier, effect.TierReduction, effectiveTier, wasTierReduced);

        // Step 4: Get base penalties for effective tier
        var basePenalties = GetPenaltiesForTier(effectiveTier);

        // Step 5: Apply multiplier if applicable
        var effectivePenalties = basePenalties;
        bool wasMultiplied = effect.PenaltyMultiplier > 1.0m;

        if (wasMultiplied)
        {
            effectivePenalties = basePenalties.Multiply(effect.PenaltyMultiplier);

            _logger.LogDebug(
                "Applied penalty multiplier {Multiplier}: Base={BasePenalties} → Effective={EffectivePenalties}",
                effect.PenaltyMultiplier, basePenalties.ToDebugString(), effectivePenalties.ToDebugString());
        }

        // Step 6: Build result
        var result = EffectiveArmorPenalties.Create(
            category,
            basePenalties,
            effectivePenalties,
            proficiencyLevel,
            effect.AttackModifier,
            effect.DefenseModifier,
            originalTier,
            effectiveTier,
            wasMultiplied,
            wasTierReduced);

        _logger.LogInformation(
            "Calculated penalties for {Category} ({Level}): {Summary}",
            category, proficiencyLevel, result.FormatSummary());

        return result;
    }

    /// <inheritdoc/>
    public EffectiveArmorPenalties CalculateForArchetype(
        string archetypeId,
        ArmorCategory category)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(archetypeId);

        _logger.LogDebug(
            "Looking up proficiency for archetype {ArchetypeId} with {Category}",
            archetypeId, category);

        // Get the archetype's proficiency level for this category
        var proficiencyLevel = _archetypeProvider.GetStartingProficiency(archetypeId, category);

        _logger.LogDebug(
            "Archetype {ArchetypeId} has {ProficiencyLevel} proficiency with {Category}",
            archetypeId, proficiencyLevel, category);

        // Delegate to the main calculation method
        return CalculatePenalties(category, proficiencyLevel);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Tier Calculation Methods
    // ═══════════════════════════════════════════════════════════════════════════

    /// <inheritdoc/>
    public int GetEffectiveTier(ArmorCategory category, ArmorProficiencyLevel proficiencyLevel)
    {
        var categoryDef = _categoryProvider.GetDefinition(category);
        int originalTier = categoryDef.WeightTier;
        int tierReduction = _effectProvider.GetTierReduction(proficiencyLevel);

        return CalculateEffectiveTier(originalTier, tierReduction, category);
    }

    /// <inheritdoc/>
    public ArmorPenalties GetPenaltiesForTier(int tier)
    {
        // Map tier to category for penalty lookup
        var category = tier switch
        {
            0 => ArmorCategory.Light,
            1 => ArmorCategory.Medium,
            2 => ArmorCategory.Heavy,
            -1 => ArmorCategory.Shields,
            _ => ArmorCategory.Light  // Default to Light for unknown tiers
        };

        return _categoryProvider.GetPenalties(category);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Query Methods
    // ═══════════════════════════════════════════════════════════════════════════

    /// <inheritdoc/>
    public bool WouldDoublePenalties(ArmorCategory category, ArmorProficiencyLevel level)
    {
        var multiplier = _effectProvider.GetPenaltyMultiplier(level);
        return multiplier >= 2.0m;
    }

    /// <inheritdoc/>
    public bool WouldReduceTier(ArmorCategory category, ArmorProficiencyLevel level)
    {
        var categoryDef = _categoryProvider.GetDefinition(category);

        // Can't reduce if not using tier system or already at minimum
        if (!categoryDef.UsesWeightTierSystem || categoryDef.WeightTier <= 0)
        {
            return false;
        }

        var tierReduction = _effectProvider.GetTierReduction(level);
        return tierReduction > 0;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Private Helper Methods
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Calculates the effective tier after applying tier reduction.
    /// </summary>
    /// <param name="originalTier">The base tier from the category.</param>
    /// <param name="tierReduction">The reduction amount from proficiency.</param>
    /// <param name="category">The armor category (for tier system check).</param>
    /// <returns>The effective tier, clamped to valid range.</returns>
    private int CalculateEffectiveTier(int originalTier, int tierReduction, ArmorCategory category)
    {
        // Shields and Specialized don't participate in tier reduction
        if (category is ArmorCategory.Shields or ArmorCategory.Specialized)
        {
            return originalTier;
        }

        // Tier reduction only applies to categories using the tier system
        if (originalTier < 0)
        {
            return originalTier;
        }

        // Apply reduction, clamping to minimum of 0 (Light)
        int effectiveTier = originalTier - tierReduction;
        return Math.Max(0, effectiveTier);
    }
}
