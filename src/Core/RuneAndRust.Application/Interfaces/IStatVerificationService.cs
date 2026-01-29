namespace RuneAndRust.Application.Interfaces;

using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Provides stat verification for generated loot items.
/// </summary>
/// <remarks>
/// <para>
/// The stat verification service validates that generated items have stats
/// appropriate for their quality tier. It checks damage ranges for weapons,
/// defense for armor, and attribute bonuses for all items.
/// </para>
/// <para>
/// Verification uses configurable stat ranges loaded from <c>stat-ranges.json</c>,
/// allowing easy tuning of tier scaling without code changes.
/// </para>
/// <para>
/// <strong>Tier Scaling Reference:</strong>
/// <list type="table">
///   <listheader>
///     <term>Tier</term>
///     <description>Damage | Defense | Attribute</description>
///   </listheader>
///   <item>
///     <term>0 (Scavenged)</term>
///     <description>1d6 (1-6) | 1-2 | +0</description>
///   </item>
///   <item>
///     <term>1 (HandForged)</term>
///     <description>1d6+1 (2-7) | 2-3 | +1</description>
///   </item>
///   <item>
///     <term>2 (ClanForged)</term>
///     <description>1d8+2 (3-10) | 3-5 | +2</description>
///   </item>
///   <item>
///     <term>3 (MasterCrafted)</term>
///     <description>2d6+2 (4-14) | 5-7 | +3</description>
///   </item>
///   <item>
///     <term>4 (RuneEtched)</term>
///     <description>2d6+4 (6-16) | 7-10 | +4</description>
///   </item>
///   <item>
///     <term>5 (LegendaryArtifact)</term>
///     <description>2d8+6 (8-22) | 10-14 | +5</description>
///   </item>
/// </list>
/// </para>
/// <para>
/// <strong>Example Usage:</strong>
/// <code>
/// // Verify a generated item
/// var item = _itemGenerator.Generate(lootEntry, tier);
/// var result = _statVerificationService.VerifyItemStats(item, tier);
/// 
/// if (!result.IsValid)
/// {
///     _logger.LogWarning(
///         "Item {ItemId} failed stat verification: {Violations}",
///         item.Id, result.GetViolationSummary());
/// }
/// 
/// // Quick validation check
/// if (_statVerificationService.IsValid(item, tier))
/// {
///     inventory.Add(item);
/// }
/// 
/// // Get expected ranges for display
/// var damageRange = _statVerificationService.GetExpectedDamage(QualityTier.RuneEtched);
/// Console.WriteLine($"Tier 4 weapons deal {damageRange.FormatRange()} damage");
/// // Output: "Tier 4 weapons deal 6-16 (2d6+4) damage"
/// </code>
/// </para>
/// </remarks>
/// <seealso cref="StatRange"/>
/// <seealso cref="StatVerificationResult"/>
/// <seealso cref="StatViolation"/>
public interface IStatVerificationService
{
    /// <summary>
    /// Verifies all stats on an item against tier expectations.
    /// </summary>
    /// <param name="item">The item to verify.</param>
    /// <param name="tier">The expected quality tier.</param>
    /// <returns>Verification result containing validity status and any violations.</returns>
    /// <remarks>
    /// <para>
    /// This method performs comprehensive stat validation:
    /// <list type="bullet">
    ///   <item><description>For weapons: Validates damage min/max against tier damage range</description></item>
    ///   <item><description>For armor: Validates defense value against tier defense range</description></item>
    ///   <item><description>For all items: Validates attribute bonuses against tier bonus range</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// <strong>Logging:</strong> Logs Information on pass, Warning with violations on failure.
    /// </para>
    /// </remarks>
    StatVerificationResult VerifyItemStats(Item item, QualityTier tier);

    /// <summary>
    /// Gets the expected damage range for a tier.
    /// </summary>
    /// <param name="tier">The quality tier to get damage range for.</param>
    /// <returns>The expected damage <see cref="StatRange"/> including dice expression.</returns>
    /// <remarks>
    /// <code>
    /// var tier4Damage = service.GetExpectedDamage(QualityTier.RuneEtched);
    /// // Returns StatRange { MinValue = 6, MaxValue = 16, DiceExpression = "2d6+4" }
    /// </code>
    /// </remarks>
    StatRange GetExpectedDamage(QualityTier tier);

    /// <summary>
    /// Gets the expected defense range for a tier.
    /// </summary>
    /// <param name="tier">The quality tier to get defense range for.</param>
    /// <returns>The expected defense <see cref="StatRange"/>.</returns>
    /// <remarks>
    /// <code>
    /// var tier4Defense = service.GetExpectedDefense(QualityTier.RuneEtched);
    /// // Returns StatRange { MinValue = 7, MaxValue = 10, DiceExpression = "7-10" }
    /// </code>
    /// </remarks>
    StatRange GetExpectedDefense(QualityTier tier);

    /// <summary>
    /// Gets the expected attribute bonus range for a tier.
    /// </summary>
    /// <param name="tier">The quality tier to get attribute bonus range for.</param>
    /// <returns>The expected attribute bonus <see cref="StatRange"/>.</returns>
    /// <remarks>
    /// <code>
    /// var tier4Attr = service.GetExpectedAttributeBonus(QualityTier.RuneEtched);
    /// // Returns StatRange { MinValue = 4, MaxValue = 4, DiceExpression = "+4" }
    /// </code>
    /// </remarks>
    StatRange GetExpectedAttributeBonus(QualityTier tier);

    /// <summary>
    /// Quick check if an item's stats are valid for a tier.
    /// </summary>
    /// <param name="item">The item to check.</param>
    /// <param name="tier">The expected quality tier.</param>
    /// <returns><c>true</c> if all stats are within expected ranges; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// This is a convenience method that calls <see cref="VerifyItemStats"/> and returns
    /// only the validity status (without detailed violation information).
    /// </remarks>
    bool IsValid(Item item, QualityTier tier);

    /// <summary>
    /// Gets all stat ranges for a tier.
    /// </summary>
    /// <param name="tier">The quality tier to get ranges for.</param>
    /// <returns>Dictionary mapping stat names to their expected <see cref="StatRange"/>.</returns>
    /// <remarks>
    /// <para>
    /// Returns a dictionary with keys: "Damage", "Defense", "AttributeBonus".
    /// </para>
    /// <code>
    /// var ranges = service.GetAllRanges(QualityTier.RuneEtched);
    /// foreach (var (name, range) in ranges)
    /// {
    ///     Console.WriteLine($"{name}: {range.FormatRange()}");
    /// }
    /// // Output:
    /// // Damage: 6-16 (2d6+4)
    /// // Defense: 7-10 (7-10)
    /// // AttributeBonus: 4-4 (+4)
    /// </code>
    /// </remarks>
    IReadOnlyDictionary<string, StatRange> GetAllRanges(QualityTier tier);
}
