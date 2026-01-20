// ═══════════════════════════════════════════════════════════════════════════════
// IQualityDeterminationService.cs
// Interface for determining crafted item quality based on dice roll results.
// Provides quality determination logic and tier lookups for the crafting system.
// Version: 0.11.2c
// ═══════════════════════════════════════════════════════════════════════════════

using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Service for determining crafted item quality based on crafting roll results.
/// </summary>
/// <remarks>
/// <para>
/// This service encapsulates the quality determination logic used during crafting:
/// </para>
/// <list type="bullet">
///   <item><description>Natural 20 → Legendary quality</description></item>
///   <item><description>Margin ≥10 → Masterwork quality</description></item>
///   <item><description>Margin ≥5 → Fine quality</description></item>
///   <item><description>Otherwise → Standard quality</description></item>
/// </list>
/// <para>
/// The service also provides convenient access to tier definitions and modifiers,
/// delegating to <see cref="IQualityTierProvider"/> for the underlying data.
/// </para>
/// <para>
/// This service is registered with scoped lifetime, allowing for potential
/// future enhancements such as player-specific quality bonuses or session-based
/// crafting modifiers.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Determine quality from a crafting roll
/// var service = serviceProvider.GetRequiredService&lt;IQualityDeterminationService&gt;();
/// var quality = service.DetermineQuality(rollResult: 25, dc: 15, isNatural20: false);
/// // Returns CraftedItemQuality.Masterwork (margin = 10)
///
/// // Get modifiers for the determined quality
/// var modifiers = service.GetQualityModifiers(quality);
/// int scaledAttack = modifiers.ApplyToStat(baseAttack);
/// </code>
/// </example>
public interface IQualityDeterminationService
{
    // ═══════════════════════════════════════════════════════════════════════════
    // Quality Determination Methods
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Determines the quality tier achieved based on crafting roll parameters.
    /// </summary>
    /// <param name="rollResult">
    /// The total result of the crafting roll after all modifiers.
    /// </param>
    /// <param name="dc">
    /// The difficulty class (DC) of the crafting check.
    /// </param>
    /// <param name="isNatural20">
    /// <c>true</c> if the roll was a natural 20 (before modifiers);
    /// otherwise, <c>false</c>.
    /// </param>
    /// <returns>
    /// The <see cref="CraftedItemQuality"/> achieved based on the roll.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Quality determination follows these rules (in order of priority):
    /// </para>
    /// <list type="number">
    ///   <item><description>Natural 20: Always returns Legendary</description></item>
    ///   <item><description>Margin ≥10: Returns Masterwork</description></item>
    ///   <item><description>Margin ≥5: Returns Fine</description></item>
    ///   <item><description>Otherwise: Returns Standard</description></item>
    /// </list>
    /// <para>
    /// The margin is calculated as: <paramref name="rollResult"/> - <paramref name="dc"/>.
    /// </para>
    /// <para>
    /// Note: This method assumes the crafting check was successful. Failed crafting
    /// checks should be handled before calling this method.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Natural 20 always gives Legendary
    /// var quality1 = service.DetermineQuality(25, 20, true);  // Legendary
    ///
    /// // Margin of 10+ gives Masterwork
    /// var quality2 = service.DetermineQuality(25, 15, false); // Masterwork (margin = 10)
    ///
    /// // Margin of 5-9 gives Fine
    /// var quality3 = service.DetermineQuality(20, 15, false); // Fine (margin = 5)
    ///
    /// // Margin of 0-4 gives Standard
    /// var quality4 = service.DetermineQuality(18, 15, false); // Standard (margin = 3)
    /// </code>
    /// </example>
    CraftedItemQuality DetermineQuality(int rollResult, int dc, bool isNatural20);

    // ═══════════════════════════════════════════════════════════════════════════
    // Tier Lookup Methods
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the full tier definition for the specified quality level.
    /// </summary>
    /// <param name="quality">The quality level to retrieve the tier for.</param>
    /// <returns>
    /// The <see cref="QualityTierDefinition"/> containing all tier properties
    /// including display name, color code, modifiers, and thresholds.
    /// </returns>
    /// <remarks>
    /// This method delegates to <see cref="IQualityTierProvider.GetTier"/> and
    /// provides a convenient access point from the determination service.
    /// </remarks>
    /// <example>
    /// <code>
    /// var tier = service.GetQualityTier(CraftedItemQuality.Masterwork);
    /// Console.WriteLine($"Color: {tier.ColorCode}");
    /// Console.WriteLine($"Stat Multiplier: {tier.Modifiers.StatMultiplier:P0}");
    /// </code>
    /// </example>
    QualityTierDefinition GetQualityTier(CraftedItemQuality quality);

    /// <summary>
    /// Gets the stat and value modifiers for the specified quality level.
    /// </summary>
    /// <param name="quality">The quality level to retrieve modifiers for.</param>
    /// <returns>
    /// The <see cref="QualityModifier"/> containing stat and value multipliers
    /// for the specified quality level.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This is a convenience method for quick access to modifiers without
    /// retrieving the full tier definition.
    /// </para>
    /// <para>
    /// The modifiers can be used to scale item stats and values:
    /// </para>
    /// <list type="bullet">
    ///   <item><description><see cref="QualityModifier.ApplyToStat"/> - Scale attack, defense, etc.</description></item>
    ///   <item><description><see cref="QualityModifier.ApplyToValue"/> - Scale gold/currency value</description></item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// var quality = service.DetermineQuality(rollResult, dc, isNatural20);
    /// var modifiers = service.GetQualityModifiers(quality);
    ///
    /// // Scale the item's base stats
    /// int finalAttack = modifiers.ApplyToStat(baseAttack);
    /// int finalDefense = modifiers.ApplyToStat(baseDefense);
    /// int finalValue = modifiers.ApplyToValue(baseValue);
    /// </code>
    /// </example>
    QualityModifier GetQualityModifiers(CraftedItemQuality quality);
}
