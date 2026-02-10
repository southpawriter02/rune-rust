using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents an active rune inscription applied to a piece of equipment.
/// Provides temporary stat bonuses (weapon damage or armor defense) that
/// decay over turns.
/// </summary>
/// <remarks>
/// <para>Inscribed runes are created by the Rúnasmiðr's Inscribe Rune ability
/// and have time-limited durations:</para>
/// <list type="bullet">
/// <item>Tier 1 Enhancement: +2 weapon damage for 10 turns</item>
/// <item>Tier 1 Protection: +1 armor Defense for 10 turns</item>
/// </list>
/// <para>Only one rune of each type may be active on a given item at a time.
/// If a new rune is inscribed on an item that already has one, the existing
/// rune is replaced.</para>
/// <para>Runes are consumed and removed when their duration expires (reaches 0 turns).</para>
/// </remarks>
public sealed record InscribedRune
{
    /// <summary>
    /// Default duration for Tier 1 inscriptions (in turns).
    /// </summary>
    public const int DefaultDuration = 10;

    /// <summary>
    /// Weapon damage bonus from a Tier 1 Enhancement rune.
    /// </summary>
    public const int Tier1WeaponDamageBonus = 2;

    /// <summary>
    /// Armor defense bonus from a Tier 1 Protection rune.
    /// </summary>
    public const int Tier1ArmorDefenseBonus = 1;

    /// <summary>
    /// Unique identifier for this rune inscription instance.
    /// </summary>
    public Guid RuneId { get; private set; }

    /// <summary>
    /// The type of rune inscription (Enhancement, Protection, etc.).
    /// Determines the stat bonus applied.
    /// </summary>
    public RuneType RuneType { get; private set; }

    /// <summary>
    /// The item ID this rune is inscribed upon.
    /// Links the rune effect to a specific piece of equipment.
    /// </summary>
    public Guid TargetItemId { get; private set; }

    /// <summary>
    /// The numeric bonus applied by this rune based on its <see cref="RuneType"/>.
    /// Enhancement: +damage, Protection: +defense.
    /// </summary>
    public int EnhancementBonus { get; private set; }

    /// <summary>
    /// Remaining turns until the rune expires.
    /// Decremented each turn via <see cref="Tick"/>.
    /// </summary>
    public int Duration { get; private set; }

    /// <summary>
    /// The original duration when the rune was first inscribed.
    /// Used for percentage calculations and display.
    /// </summary>
    public int OriginalDuration { get; private set; }

    /// <summary>
    /// UTC timestamp when the rune was inscribed.
    /// Used for auditing and display.
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// Creates a new InscribedRune for a weapon (Enhancement type).
    /// </summary>
    /// <param name="targetItemId">The weapon item ID to inscribe.</param>
    /// <param name="damageBonus">The damage bonus to apply (default <see cref="Tier1WeaponDamageBonus"/>).</param>
    /// <param name="duration">Duration in turns (default <see cref="DefaultDuration"/>).</param>
    /// <returns>A new weapon enhancement rune.</returns>
    public static InscribedRune CreateWeaponEnhancement(
        Guid targetItemId,
        int damageBonus = Tier1WeaponDamageBonus,
        int duration = DefaultDuration)
    {
        return new InscribedRune
        {
            RuneId = Guid.NewGuid(),
            RuneType = RuneType.Enhancement,
            TargetItemId = targetItemId,
            EnhancementBonus = damageBonus,
            Duration = duration,
            OriginalDuration = duration,
            CreatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Creates a new InscribedRune for armor (Protection type).
    /// </summary>
    /// <param name="targetItemId">The armor item ID to inscribe.</param>
    /// <param name="defenseBonus">The defense bonus to apply (default <see cref="Tier1ArmorDefenseBonus"/>).</param>
    /// <param name="duration">Duration in turns (default <see cref="DefaultDuration"/>).</param>
    /// <returns>A new armor protection rune.</returns>
    public static InscribedRune CreateArmorProtection(
        Guid targetItemId,
        int defenseBonus = Tier1ArmorDefenseBonus,
        int duration = DefaultDuration)
    {
        return new InscribedRune
        {
            RuneId = Guid.NewGuid(),
            RuneType = RuneType.Protection,
            TargetItemId = targetItemId,
            EnhancementBonus = defenseBonus,
            Duration = duration,
            OriginalDuration = duration,
            CreatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Advances the rune's duration by one turn.
    /// </summary>
    /// <remarks>
    /// Reduces <see cref="Duration"/> by 1, minimum 0.
    /// After ticking, check <see cref="IsExpired"/> to determine if the rune should be removed.
    /// </remarks>
    public void Tick()
    {
        if (Duration > 0)
            Duration--;
    }

    /// <summary>
    /// Checks whether this rune has expired (duration reached 0).
    /// </summary>
    /// <returns>True if <see cref="Duration"/> is 0 or less.</returns>
    public bool IsExpired() => Duration <= 0;

    /// <summary>
    /// Gets a human-readable description of the rune's current effect.
    /// </summary>
    /// <returns>Effect description string based on <see cref="RuneType"/>.</returns>
    public string GetEffectDescription() => RuneType switch
    {
        RuneType.Enhancement => $"+{EnhancementBonus} weapon damage ({Duration} turns remaining)",
        RuneType.Protection => $"+{EnhancementBonus} armor Defense ({Duration} turns remaining)",
        _ => $"Rune effect ({Duration} turns remaining)"
    };

    /// <summary>
    /// Gets a compact duration display string.
    /// </summary>
    /// <returns>Duration as "X/Y turns" format.</returns>
    public string GetDurationDisplay() => $"{Duration}/{OriginalDuration} turns";
}
