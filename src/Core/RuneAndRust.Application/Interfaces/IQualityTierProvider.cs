// ═══════════════════════════════════════════════════════════════════════════════
// IQualityTierProvider.cs
// Interface for providing quality tier definitions loaded from configuration.
// Follows the provider pattern used by IRecipeProvider and similar services.
// Version: 0.11.2c
// ═══════════════════════════════════════════════════════════════════════════════

using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Provides access to quality tier definitions loaded from configuration.
/// </summary>
/// <remarks>
/// <para>
/// This interface follows the provider pattern established by <see cref="IRecipeProvider"/>
/// and similar infrastructure services. Implementations typically load tier definitions
/// from JSON configuration files using lazy loading with thread-safe initialization.
/// </para>
/// <para>
/// The provider abstracts the configuration source, allowing for:
/// </para>
/// <list type="bullet">
///   <item><description>JSON file-based configuration (primary implementation)</description></item>
///   <item><description>In-memory configuration for testing</description></item>
///   <item><description>Database-backed configuration for dynamic updates</description></item>
/// </list>
/// <para>
/// Quality tiers are typically registered as a singleton in the DI container
/// since the tier definitions are loaded once and remain constant for the
/// application lifetime.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Get a specific tier definition
/// var provider = serviceProvider.GetRequiredService&lt;IQualityTierProvider&gt;();
/// var masterworkTier = provider.GetTier(CraftedItemQuality.Masterwork);
///
/// // Get modifiers for quality-based scaling
/// var modifiers = provider.GetModifiers(CraftedItemQuality.Fine);
/// int scaledStat = modifiers.ApplyToStat(baseStatValue);
/// </code>
/// </example>
public interface IQualityTierProvider
{
    // ═══════════════════════════════════════════════════════════════════════════
    // Tier Retrieval Methods
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the tier definition for the specified quality level.
    /// </summary>
    /// <param name="quality">The quality level to retrieve the definition for.</param>
    /// <returns>
    /// The <see cref="QualityTierDefinition"/> for the specified quality level.
    /// </returns>
    /// <exception cref="KeyNotFoundException">
    /// Thrown when no tier definition exists for the specified quality.
    /// </exception>
    /// <remarks>
    /// <para>
    /// This method throws an exception if the quality is not found. For scenarios
    /// where the quality may not exist, use <see cref="TryGetTier"/> instead.
    /// </para>
    /// <para>
    /// Implementations should ensure all standard quality levels (Standard, Fine,
    /// Masterwork, Legendary) are always available.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var tier = provider.GetTier(CraftedItemQuality.Legendary);
    /// Console.WriteLine($"Legendary items have {tier.Modifiers.StatMultiplier:P0} stat boost");
    /// </code>
    /// </example>
    QualityTierDefinition GetTier(CraftedItemQuality quality);

    /// <summary>
    /// Attempts to get the tier definition for the specified quality level.
    /// </summary>
    /// <param name="quality">The quality level to retrieve the definition for.</param>
    /// <param name="tier">
    /// When this method returns, contains the tier definition if found;
    /// otherwise, the default value for the type.
    /// </param>
    /// <returns>
    /// <c>true</c> if the tier definition was found; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// Use this method when you need to safely check for the existence of a tier
    /// definition without throwing an exception.
    /// </remarks>
    /// <example>
    /// <code>
    /// if (provider.TryGetTier(quality, out var tier))
    /// {
    ///     ApplyQualityModifiers(item, tier.Modifiers);
    /// }
    /// else
    /// {
    ///     logger.LogWarning("Unknown quality level: {Quality}", quality);
    /// }
    /// </code>
    /// </example>
    bool TryGetTier(CraftedItemQuality quality, out QualityTierDefinition tier);

    // ═══════════════════════════════════════════════════════════════════════════
    // Collection Methods
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets all available quality tier definitions.
    /// </summary>
    /// <returns>
    /// A read-only list of all <see cref="QualityTierDefinition"/> instances,
    /// ordered by quality level from lowest (Standard) to highest (Legendary).
    /// </returns>
    /// <remarks>
    /// <para>
    /// The returned list is ordered by the <see cref="CraftedItemQuality"/> enum
    /// value to ensure consistent ordering across the application.
    /// </para>
    /// <para>
    /// This method is useful for UI components that need to display all available
    /// quality tiers, such as crafting preview panels or quality selection dialogs.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var allTiers = provider.GetAllTiers();
    /// foreach (var tier in allTiers)
    /// {
    ///     Console.WriteLine($"{tier.DisplayName}: {tier.ColorCode}");
    /// }
    /// </code>
    /// </example>
    IReadOnlyList<QualityTierDefinition> GetAllTiers();

    // ═══════════════════════════════════════════════════════════════════════════
    // Modifier Retrieval Methods
    // ═══════════════════════════════════════════════════════════════════════════

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
    /// This is a convenience method that retrieves just the modifiers without
    /// the full tier definition. It is equivalent to calling:
    /// <c>GetTier(quality).Modifiers</c>
    /// </para>
    /// <para>
    /// If the quality level is not found, implementations should return
    /// <see cref="QualityModifier.None"/> as a fallback.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var modifiers = provider.GetModifiers(CraftedItemQuality.Masterwork);
    /// int scaledAttack = modifiers.ApplyToStat(baseAttack);
    /// int scaledValue = modifiers.ApplyToValue(baseValue);
    /// </code>
    /// </example>
    QualityModifier GetModifiers(CraftedItemQuality quality);
}
