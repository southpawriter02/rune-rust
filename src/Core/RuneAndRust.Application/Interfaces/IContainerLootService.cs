// =============================================================================
// IContainerLootService.cs
// Service interface for generating and managing container loot.
// Version: 0.16.4d
// =============================================================================

using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Service interface for generating and managing container loot.
/// </summary>
/// <remarks>
/// <para>
/// This service orchestrates loot generation by combining container type
/// definitions, biome modifiers, and smart loot algorithms. Contents are
/// generated lazily on first access and cached for subsequent lookups.
/// </para>
/// <para>
/// The generation pipeline:
/// <list type="number">
///   <item>Retrieve container type definition for item/currency ranges</item>
///   <item>Apply biome modifiers from the container's location</item>
///   <item>Calculate adjusted item count using drop rate multiplier</item>
///   <item>Select quality tier with quality bonus applied</item>
///   <item>Generate items using <see cref="ISmartLootService"/> for class-appropriate selection</item>
///   <item>Calculate currency with gold multiplier applied</item>
///   <item>Create and store <see cref="ContainerContents"/> on the container entity</item>
/// </list>
/// </para>
/// <para>
/// Configuration data is loaded from:
/// <list type="bullet">
///   <item><c>container-types.json</c> - Container type definitions</item>
///   <item><c>biome-loot-modifiers.json</c> - Biome-specific modifiers</item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Generate contents for a container in a specific biome
/// var contents = containerLootService.GenerateContents(
///     container: chestEntity,
///     playerId: player.Id,
///     biomeId: "alfheim");
///
/// // Access generated items and currency
/// foreach (var itemId in contents.ItemIds)
/// {
///     Console.WriteLine($"Found: {itemId}");
/// }
/// Console.WriteLine($"Gold: {contents.CurrencyAmount}");
/// </code>
/// </example>
public interface IContainerLootService
{
    /// <summary>
    /// Generates contents for a container, applying biome modifiers and smart loot.
    /// </summary>
    /// <param name="container">The container to generate contents for.</param>
    /// <param name="playerId">The player ID for smart loot calculations.</param>
    /// <param name="biomeId">The biome ID for modifier lookup.</param>
    /// <returns>
    /// The generated container contents, or empty for previously looted containers.
    /// </returns>
    /// <remarks>
    /// <para>
    /// If the container already has generated contents, returns existing contents
    /// without regenerating. This ensures consistent loot across multiple accesses.
    /// </para>
    /// <para>
    /// If the container is looted, returns <see cref="ContainerContents.Empty"/>.
    /// </para>
    /// <para>
    /// Otherwise, generates new contents based on:
    /// <list type="bullet">
    ///   <item>Container type definition (item count, tier range, currency range)</item>
    ///   <item>Biome modifiers (gold multiplier, drop rate, quality bonus)</item>
    ///   <item>Player's archetype for smart loot class-appropriate filtering</item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="container"/> is null.
    /// </exception>
    /// <exception cref="KeyNotFoundException">
    /// Thrown when no definition exists for the container's type.
    /// </exception>
    /// <example>
    /// <code>
    /// // First access generates contents
    /// var contents = service.GenerateContents(chest, playerId, "the-roots");
    /// Console.WriteLine($"Generated {contents.ItemCount} items");
    ///
    /// // Subsequent access returns same contents
    /// var sameContents = service.GenerateContents(chest, playerId, "the-roots");
    /// Assert.AreEqual(contents, sameContents);
    /// </code>
    /// </example>
    ContainerContents GenerateContents(
        ContainerLootTable container,
        Guid playerId,
        string biomeId);

    /// <summary>
    /// Gets the definition for a specific container type.
    /// </summary>
    /// <param name="containerType">The container type to look up.</param>
    /// <returns>The container type definition.</returns>
    /// <exception cref="KeyNotFoundException">
    /// Thrown when no definition exists for the specified type.
    /// </exception>
    /// <remarks>
    /// <para>
    /// Container type definitions specify:
    /// <list type="bullet">
    ///   <item>Item count range (MinItems to MaxItems)</item>
    ///   <item>Quality tier range (MinTier to MaxTier)</item>
    ///   <item>Currency range (MinCurrency to MaxCurrency) if applicable</item>
    ///   <item>Optional item category filter for specialized containers</item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var definition = service.GetContainerDefinition(ContainerType.BossChest);
    /// Console.WriteLine($"Boss chest drops {definition.MinItems}-{definition.MaxItems} items");
    /// Console.WriteLine($"Quality tier: {definition.MinTier}-{definition.MaxTier}");
    /// </code>
    /// </example>
    ContainerTypeDefinition GetContainerDefinition(ContainerType containerType);

    /// <summary>
    /// Gets the biome modifiers for a specific biome.
    /// </summary>
    /// <param name="biomeId">The biome ID to look up.</param>
    /// <returns>
    /// The biome modifiers, or <see cref="BiomeLootModifiers.Default"/> if not found.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Biome IDs are case-insensitive and normalized to lowercase internally.
    /// If no modifiers are defined for a biome, the default (neutral) modifiers
    /// are returned to ensure graceful fallback behavior.
    /// </para>
    /// <para>
    /// Available biomes include:
    /// <list type="bullet">
    ///   <item><c>the-roots</c> - Baseline modifiers (1.0x gold, 1.0x drops)</item>
    ///   <item><c>muspelheim</c> - Fire realm (1.2x gold, 0.9x drops, +5% rare)</item>
    ///   <item><c>alfheim</c> - Light realm (1.5x gold, 0.7x drops, +1 quality, +15% rare)</item>
    ///   <item><c>jotunheim</c> - Giant realm (2.0x gold, 0.8x drops, +1 quality, +20% rare)</item>
    ///   <item><c>boss-room</c> - Boss encounters (1.5x gold, +1 quality, +25% rare)</item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var modifiers = service.GetBiomeModifiers("alfheim");
    /// Console.WriteLine($"Gold multiplier: {modifiers.GoldMultiplier}x");
    /// Console.WriteLine($"Quality bonus: +{modifiers.QualityBonus}");
    /// </code>
    /// </example>
    BiomeLootModifiers GetBiomeModifiers(string biomeId);

    /// <summary>
    /// Checks if contents have been generated for a container.
    /// </summary>
    /// <param name="container">The container to check.</param>
    /// <returns>True if contents exist, false otherwise.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="container"/> is null.
    /// </exception>
    /// <remarks>
    /// <para>
    /// This method does not trigger content generation. Use it to check
    /// container state before deciding whether to call <see cref="GenerateContents"/>.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// if (!service.HasGeneratedContents(chest))
    /// {
    ///     // Player hasn't opened this container yet
    ///     DisplayClosedChestIcon(chest);
    /// }
    /// </code>
    /// </example>
    bool HasGeneratedContents(ContainerLootTable container);
}
