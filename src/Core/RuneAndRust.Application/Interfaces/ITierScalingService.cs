namespace RuneAndRust.Application.Interfaces;

using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Provides access to tier-based stat scaling for weapons and armor.
/// </summary>
/// <remarks>
/// <para>
/// This interface abstracts the source of scaling data, allowing it to be
/// loaded from configuration files, databases, or hardcoded defaults.
/// </para>
/// <para>
/// Implementations should cache loaded scaling data for performance and
/// validate that all quality tiers have corresponding entries.
/// </para>
/// </remarks>
public interface ITierScalingService
{
    /// <summary>
    /// Gets the weapon stat scaling for a specific quality tier.
    /// </summary>
    /// <param name="tier">The quality tier to get scaling for.</param>
    /// <returns>The WeaponTierScaling for the specified tier.</returns>
    /// <exception cref="ArgumentException">Thrown when scaling for the tier is not found.</exception>
    WeaponTierScaling GetWeaponScaling(QualityTier tier);

    /// <summary>
    /// Gets the armor stat scaling for a specific quality tier.
    /// </summary>
    /// <param name="tier">The quality tier to get scaling for.</param>
    /// <returns>The ArmorTierScaling for the specified tier.</returns>
    /// <exception cref="ArgumentException">Thrown when scaling for the tier is not found.</exception>
    ArmorTierScaling GetArmorScaling(QualityTier tier);

    /// <summary>
    /// Gets all weapon scaling entries, ordered by tier.
    /// </summary>
    /// <returns>A read-only list of all weapon tier scaling entries.</returns>
    IReadOnlyList<WeaponTierScaling> GetAllWeaponScaling();

    /// <summary>
    /// Gets all armor scaling entries, ordered by tier.
    /// </summary>
    /// <returns>A read-only list of all armor tier scaling entries.</returns>
    IReadOnlyList<ArmorTierScaling> GetAllArmorScaling();

    /// <summary>
    /// Rolls a weapon attribute bonus for the specified tier.
    /// </summary>
    /// <param name="tier">The quality tier.</param>
    /// <param name="random">Random number generator.</param>
    /// <returns>The rolled bonus, or null for tiers without attribute bonuses.</returns>
    int? RollWeaponAttributeBonus(QualityTier tier, Random random);

    /// <summary>
    /// Rolls an armor attribute bonus for the specified tier.
    /// </summary>
    /// <param name="tier">The quality tier.</param>
    /// <param name="random">Random number generator.</param>
    /// <returns>The rolled bonus, or null for tiers without attribute bonuses.</returns>
    int? RollArmorAttributeBonus(QualityTier tier, Random random);

    /// <summary>
    /// Validates that all required tiers have scaling entries.
    /// </summary>
    /// <returns>True if all tiers are configured, false otherwise.</returns>
    bool ValidateConfiguration();
}
