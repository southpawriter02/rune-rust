namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Configuration interface for respec (talent point reallocation) settings.
/// </summary>
/// <remarks>
/// <para>IRespecConfiguration provides configurable parameters for the respec system:</para>
/// <list type="bullet">
///   <item><description>Base cost and level-based scaling for respec gold cost</description></item>
///   <item><description>Feature toggle to enable/disable respec entirely</description></item>
///   <item><description>Minimum level requirement before respec is available</description></item>
///   <item><description>Currency type used for payment (defaults to "gold")</description></item>
/// </list>
/// <para>The cost formula is: <c>BaseRespecCost + (PlayerLevel * LevelMultiplier)</c></para>
/// <para>Example at level 10 with defaults: 100 + (10 * 10) = 200 gold</para>
/// </remarks>
public interface IRespecConfiguration
{
    /// <summary>
    /// Gets the base gold cost for respec operations.
    /// </summary>
    /// <remarks>
    /// This is the minimum cost regardless of player level.
    /// Default value is typically 100.
    /// </remarks>
    int BaseRespecCost { get; }

    /// <summary>
    /// Gets the additional cost per player level.
    /// </summary>
    /// <remarks>
    /// <para>Combined with BaseRespecCost to calculate total respec cost.</para>
    /// <para>Formula: BaseRespecCost + (Level * LevelMultiplier)</para>
    /// <para>Default value is typically 10.</para>
    /// </remarks>
    int LevelMultiplier { get; }

    /// <summary>
    /// Gets whether the respec feature is enabled.
    /// </summary>
    /// <remarks>
    /// When false, all respec attempts will return <see cref="DTOs.RespecResultType.Disabled"/>.
    /// This allows temporarily disabling respec without removing the feature.
    /// </remarks>
    bool IsRespecEnabled { get; }

    /// <summary>
    /// Gets the minimum player level required to use respec.
    /// </summary>
    /// <remarks>
    /// <para>Players below this level cannot respec and will receive
    /// <see cref="DTOs.RespecResultType.LevelTooLow"/>.</para>
    /// <para>Default value is typically 2 (allowing respec after first level-up).</para>
    /// </remarks>
    int MinimumLevelToRespec { get; }

    /// <summary>
    /// Gets the currency ID used for respec payment.
    /// </summary>
    /// <remarks>
    /// Defaults to "gold". Must match a valid currency ID in the player's
    /// currency dictionary (accessed via <see cref="Domain.Entities.Player.GetCurrency"/>).
    /// </remarks>
    string CurrencyId { get; }
}
