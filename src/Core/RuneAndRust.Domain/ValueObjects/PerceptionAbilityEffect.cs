namespace RuneAndRust.Domain.ValueObjects;

using RuneAndRust.Domain.Enums;

/// <summary>
/// Defines the effect applied when a perception ability activates.
/// </summary>
/// <remarks>
/// This is specifically for specialization perception abilities, distinct
/// from combat AbilityEffect.
/// </remarks>
public sealed record PerceptionAbilityEffect
{
    /// <summary>
    /// The type of effect applied.
    /// </summary>
    public required PerceptionEffectType Type { get; init; }

    /// <summary>
    /// Bonus dice added to the pool (for DiceBonus effects).
    /// Example: 2 for Jötun-Reader's +2d10 Deep Scan.
    /// </summary>
    public int BonusDice { get; init; }

    /// <summary>
    /// Flat bonus added to perception value (for PassiveBonus effects).
    /// Example: 1 for Veiðimaðr's +1 passive perception.
    /// </summary>
    public int FlatBonus { get; init; }

    /// <summary>
    /// The examination layer that is auto-succeeded (for AutoSuccess effects).
    /// Example: 2 for Pattern Recognition, 3 for Lore Keeper.
    /// </summary>
    public int? AutoSuccessLayer { get; init; }

    /// <summary>
    /// Whether this effect causes auto-detection without a check.
    /// Example: true for Sixth Sense trap auto-detection.
    /// </summary>
    public bool AutoDetect { get; init; }

    /// <summary>
    /// Additional lore or information revealed by this effect.
    /// Example: Extra historical context for Thul's Ancient Knowledge.
    /// </summary>
    public string? BonusLoreKey { get; init; }

    /// <summary>
    /// Bonus to investigation checks (for Investigation effects).
    /// </summary>
    public int InvestigationBonus { get; init; }

    /// <summary>
    /// Creates a dice bonus effect.
    /// </summary>
    public static PerceptionAbilityEffect DiceBonusEffect(int bonusDice) => new()
    {
        Type = PerceptionEffectType.DiceBonus,
        BonusDice = bonusDice
    };

    /// <summary>
    /// Creates a passive bonus effect.
    /// </summary>
    public static PerceptionAbilityEffect PassiveBonusEffect(int flatBonus) => new()
    {
        Type = PerceptionEffectType.PassiveBonus,
        FlatBonus = flatBonus
    };

    /// <summary>
    /// Creates an auto-success layer effect.
    /// </summary>
    public static PerceptionAbilityEffect AutoSuccessEffect(int layer) => new()
    {
        Type = PerceptionEffectType.AutoSuccessLayer,
        AutoSuccessLayer = layer
    };

    /// <summary>
    /// Creates an auto-detect effect.
    /// </summary>
    public static PerceptionAbilityEffect AutoDetectEffect() => new()
    {
        Type = PerceptionEffectType.AutoDetect,
        AutoDetect = true
    };
}
