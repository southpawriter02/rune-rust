// ═══════════════════════════════════════════════════════════════════════════════
// QualityDeterminationService.cs
// Service implementation for determining crafted item quality based on dice rolls.
// Applies margin-based thresholds and natural 20 rules to determine quality tiers.
// Version: 0.11.2c
// ═══════════════════════════════════════════════════════════════════════════════

using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Services;

/// <summary>
/// Service for determining crafted item quality based on crafting roll results.
/// </summary>
/// <remarks>
/// <para>
/// This service implements the quality determination logic used during crafting.
/// It evaluates the crafting roll result against the difficulty class (DC) and
/// applies the following rules (in priority order):
/// </para>
/// <list type="number">
///   <item><description>Natural 20: Always results in Legendary quality</description></item>
///   <item><description>Margin ≥10: Results in Masterwork quality</description></item>
///   <item><description>Margin ≥5: Results in Fine quality</description></item>
///   <item><description>Otherwise: Results in Standard quality</description></item>
/// </list>
/// <para>
/// The service uses <see cref="IQualityTierProvider"/> to retrieve tier definitions
/// and modifiers, allowing for configuration-driven quality tier settings.
/// </para>
/// <para>
/// Logging is performed at appropriate levels:
/// </para>
/// <list type="bullet">
///   <item><description>Debug: Input parameters and intermediate calculations</description></item>
///   <item><description>Information: Final quality determination results</description></item>
///   <item><description>Warning: Edge cases or unexpected conditions</description></item>
/// </list>
/// </remarks>
/// <example>
/// <code>
/// // Inject the service via DI
/// public class CraftingService
/// {
///     private readonly IQualityDeterminationService _qualityService;
///
///     public CraftingService(IQualityDeterminationService qualityService)
///     {
///         _qualityService = qualityService;
///     }
///
///     public Item Craft(Recipe recipe, int rollResult, int dc, bool isNatural20)
///     {
///         var quality = _qualityService.DetermineQuality(rollResult, dc, isNatural20);
///         var modifiers = _qualityService.GetQualityModifiers(quality);
///         return CreateItemWithQuality(recipe, quality, modifiers);
///     }
/// }
/// </code>
/// </example>
public sealed class QualityDeterminationService : IQualityDeterminationService
{
    // ═══════════════════════════════════════════════════════════════════════════
    // Constants
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// The minimum margin required to achieve Masterwork quality.
    /// </summary>
    /// <remarks>
    /// Beating the DC by 10 or more results in Masterwork quality.
    /// </remarks>
    private const int MasterworkMarginThreshold = 10;

    /// <summary>
    /// The minimum margin required to achieve Fine quality.
    /// </summary>
    /// <remarks>
    /// Beating the DC by 5-9 results in Fine quality.
    /// </remarks>
    private const int FineMarginThreshold = 5;

    // ═══════════════════════════════════════════════════════════════════════════
    // Dependencies
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// The quality tier provider for retrieving tier definitions.
    /// </summary>
    private readonly IQualityTierProvider _tierProvider;

    /// <summary>
    /// The logger for diagnostic output.
    /// </summary>
    private readonly ILogger<QualityDeterminationService> _logger;

    // ═══════════════════════════════════════════════════════════════════════════
    // Constructor
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Initializes a new instance of the <see cref="QualityDeterminationService"/> class.
    /// </summary>
    /// <param name="tierProvider">
    /// The quality tier provider for retrieving tier definitions and modifiers.
    /// </param>
    /// <param name="logger">
    /// The logger for diagnostic output.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="tierProvider"/> or <paramref name="logger"/> is null.
    /// </exception>
    public QualityDeterminationService(
        IQualityTierProvider tierProvider,
        ILogger<QualityDeterminationService> logger)
    {
        // Validate constructor parameters
        ArgumentNullException.ThrowIfNull(tierProvider, nameof(tierProvider));
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));

        _tierProvider = tierProvider;
        _logger = logger;

        // Log service initialization
        _logger.LogDebug(
            "QualityDeterminationService initialized with tier provider: {ProviderType}",
            tierProvider.GetType().Name);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // IQualityDeterminationService Implementation - Quality Determination
    // ═══════════════════════════════════════════════════════════════════════════

    /// <inheritdoc />
    /// <remarks>
    /// <para>
    /// This method applies the quality determination rules in the following order:
    /// </para>
    /// <list type="number">
    ///   <item><description>Check for natural 20 (always Legendary)</description></item>
    ///   <item><description>Calculate margin (rollResult - dc)</description></item>
    ///   <item><description>Compare margin against thresholds</description></item>
    /// </list>
    /// <para>
    /// All inputs and the determination result are logged for debugging and audit purposes.
    /// </para>
    /// </remarks>
    public CraftedItemQuality DetermineQuality(int rollResult, int dc, bool isNatural20)
    {
        // Log input parameters for debugging
        _logger.LogDebug(
            "Determining quality - Roll: {RollResult}, DC: {DC}, Natural20: {IsNatural20}",
            rollResult,
            dc,
            isNatural20);

        // Rule 1: Natural 20 always results in Legendary quality
        if (isNatural20)
        {
            _logger.LogInformation(
                "Natural 20 rolled - Legendary quality achieved (Roll: {RollResult}, DC: {DC})",
                rollResult,
                dc);

            return CraftedItemQuality.Legendary;
        }

        // Calculate the margin by which the roll exceeded the DC
        var margin = rollResult - dc;

        _logger.LogDebug(
            "Calculated roll margin: {Margin} (Roll: {RollResult} - DC: {DC})",
            margin,
            rollResult,
            dc);

        // Determine quality based on margin thresholds
        var quality = margin switch
        {
            >= MasterworkMarginThreshold => CraftedItemQuality.Masterwork,
            >= FineMarginThreshold => CraftedItemQuality.Fine,
            _ => CraftedItemQuality.Standard
        };

        // Log the determination result
        _logger.LogInformation(
            "Quality determined: {Quality} (Margin: {Margin}, Roll: {RollResult}, DC: {DC})",
            quality,
            margin,
            rollResult,
            dc);

        return quality;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // IQualityDeterminationService Implementation - Tier Lookups
    // ═══════════════════════════════════════════════════════════════════════════

    /// <inheritdoc />
    /// <remarks>
    /// Delegates to the <see cref="IQualityTierProvider"/> for tier retrieval.
    /// Logs the lookup request for debugging purposes.
    /// </remarks>
    public QualityTierDefinition GetQualityTier(CraftedItemQuality quality)
    {
        _logger.LogDebug("Retrieving tier definition for quality: {Quality}", quality);

        var tier = _tierProvider.GetTier(quality);

        _logger.LogDebug(
            "Retrieved tier definition: {Quality} - StatMultiplier: {StatMult:F2}x, ValueMultiplier: {ValueMult:F1}x",
            tier.Quality,
            tier.Modifiers.StatMultiplier,
            tier.Modifiers.ValueMultiplier);

        return tier;
    }

    /// <inheritdoc />
    /// <remarks>
    /// <para>
    /// Delegates to the <see cref="IQualityTierProvider"/> for modifier retrieval.
    /// This is a convenience method that avoids retrieving the full tier definition
    /// when only the modifiers are needed.
    /// </para>
    /// <para>
    /// If the quality level is not found in the provider, returns <see cref="QualityModifier.None"/>
    /// as a safe fallback.
    /// </para>
    /// </remarks>
    public QualityModifier GetQualityModifiers(CraftedItemQuality quality)
    {
        _logger.LogDebug("Retrieving modifiers for quality: {Quality}", quality);

        var modifiers = _tierProvider.GetModifiers(quality);

        _logger.LogDebug(
            "Retrieved modifiers for {Quality}: StatMultiplier: {StatMult:F2}x, ValueMultiplier: {ValueMult:F1}x",
            quality,
            modifiers.StatMultiplier,
            modifiers.ValueMultiplier);

        return modifiers;
    }
}
