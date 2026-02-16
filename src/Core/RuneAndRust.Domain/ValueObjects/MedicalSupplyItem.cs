using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents a single Medical Supply item in the Bone-Setter's inventory.
/// Each supply has a type classification, quality rating (1–5), and provides
/// healing bonuses when consumed by medical abilities.
/// </summary>
/// <remarks>
/// <para>Medical Supply items are the individual consumable units within the
/// <see cref="MedicalSuppliesResource"/> inventory. Quality directly affects
/// healing effectiveness via the formula: bonus = Quality - 1 (range: 0–4).</para>
/// <para>Quality ratings:</para>
/// <list type="bullet">
/// <item>1 (Poor): Basic salvage, +0 healing bonus</item>
/// <item>2 (Standard): Common loot, +1 healing bonus</item>
/// <item>3 (Good): Quality loot or crafted, +2 healing bonus</item>
/// <item>4 (Excellent): Rare find or skilled craft, +3 healing bonus</item>
/// <item>5 (Superior): Very rare or master craft, +4 healing bonus</item>
/// </list>
/// </remarks>
public sealed record MedicalSupplyItem
{
    /// <summary>
    /// Minimum valid quality rating for a Medical Supply item.
    /// </summary>
    private const int MinQuality = 1;

    /// <summary>
    /// Maximum valid quality rating for a Medical Supply item.
    /// </summary>
    private const int MaxQuality = 5;

    /// <summary>
    /// Unique identifier for this specific supply item instance.
    /// </summary>
    public Guid ItemId { get; init; }

    /// <summary>
    /// Type classification of this supply (Bandage, Salve, Splint, etc.).
    /// Determines which abilities can specifically consume this supply type.
    /// </summary>
    public MedicalSupplyType SupplyType { get; init; }

    /// <summary>
    /// Human-readable display name including quality indicator.
    /// Example: "Reinforced Bandage", "Potent Herbs".
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Detailed description of supply origin and medicinal properties.
    /// </summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// Quality rating from 1 (poor salvage) to 5 (pristine/master crafted).
    /// Directly affects healing bonus: bonus = Quality - 1 (range: 0–4).
    /// </summary>
    public int Quality { get; init; }

    /// <summary>
    /// UTC timestamp when this supply was acquired.
    /// Used for inventory tracking and potential future expiration mechanics.
    /// </summary>
    public DateTime AcquiredAt { get; init; }

    /// <summary>
    /// Source classification for this supply: "salvage", "purchase", "craft", or "quest_reward".
    /// Used for audit trail and future restock mechanics.
    /// </summary>
    public string Source { get; init; } = string.Empty;

    /// <summary>
    /// Creates a new Medical Supply item with validation.
    /// </summary>
    /// <param name="supplyType">The type classification of the supply.</param>
    /// <param name="name">Human-readable display name. Cannot be empty.</param>
    /// <param name="description">Detailed description of the supply.</param>
    /// <param name="quality">Quality rating (1–5). Affects healing bonus.</param>
    /// <param name="source">Acquisition source ("salvage", "purchase", "craft", "quest_reward"). Cannot be empty.</param>
    /// <returns>A new <see cref="MedicalSupplyItem"/> with a generated unique ID and current timestamp.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="name"/> or <paramref name="source"/> is empty or whitespace.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="quality"/> is outside the 1–5 range.</exception>
    public static MedicalSupplyItem Create(
        MedicalSupplyType supplyType,
        string name,
        string description,
        int quality,
        string source)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Supply name cannot be empty.", nameof(name));

        if (quality < MinQuality || quality > MaxQuality)
            throw new ArgumentOutOfRangeException(nameof(quality), quality,
                $"Quality must be between {MinQuality} and {MaxQuality}.");

        if (string.IsNullOrWhiteSpace(source))
            throw new ArgumentException("Supply source cannot be empty.", nameof(source));

        return new MedicalSupplyItem
        {
            ItemId = Guid.NewGuid(),
            SupplyType = supplyType,
            Name = name,
            Description = description,
            Quality = quality,
            AcquiredAt = DateTime.UtcNow,
            Source = source
        };
    }

    /// <summary>
    /// Calculates the healing bonus provided by this supply's quality.
    /// Formula: Quality - 1 (range: 0 for poor to 4 for superior).
    /// </summary>
    /// <returns>The healing bonus value (0–4).</returns>
    public int GetHealingBonus() => Quality - MinQuality;

    /// <summary>
    /// Returns a formatted display string with quality star notation.
    /// Example: "Reinforced Bandage (Quality: ★★★☆☆)"
    /// </summary>
    /// <returns>A human-readable string with visual quality indicator.</returns>
    public string GetFormattedDescription()
    {
        var stars = new string('\u2605', Quality);
        var empty = new string('\u2606', MaxQuality - Quality);
        return $"{Name} (Quality: {stars}{empty})";
    }
}
