// ═══════════════════════════════════════════════════════════════════════════════
// InscribedRune.cs
// Immutable value object representing a rune inscribed on an item by the
// Rúnasmiðr specialization. Tracks rune type, target item, enhancement bonus,
// remaining duration, and provides tick/expiry mechanics.
// Version: 0.20.2a
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.ValueObjects;

using RuneAndRust.Domain.Enums;

/// <summary>
/// Represents a rune inscribed on an item by the Rúnasmiðr specialization.
/// </summary>
/// <remarks>
/// <para>
/// Each inscribed rune provides a specific enhancement based on its
/// <see cref="RuneType"/>:
/// </para>
/// <list type="bullet">
///   <item><description><b>Enhancement:</b> +2 damage on weapons</description></item>
///   <item><description><b>Protection:</b> +1 Defense on armor</description></item>
/// </list>
/// <para>
/// Runes have a limited duration (default 10 turns) and expire when the
/// remaining duration reaches 0. Only one rune can be active per item at a time.
/// </para>
/// <para>
/// This is an immutable value object — the <see cref="Tick"/> method returns
/// a new instance with decremented duration.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var rune = InscribedRune.CreateEnhancement(itemId);
/// // rune.RemainingDuration = 10, rune.EnhancementBonus = 2
/// var ticked = rune.Tick();
/// // ticked.RemainingDuration = 9
/// </code>
/// </example>
public sealed record InscribedRune
{
    /// <summary>
    /// Default duration for inscribed runes, in turns.
    /// </summary>
    public const int DefaultDuration = 10;

    /// <summary>
    /// Damage bonus granted by Enhancement runes on weapons.
    /// </summary>
    public const int WeaponDamageBonus = 2;

    /// <summary>
    /// Defense bonus granted by Protection runes on armor.
    /// </summary>
    public const int ArmorDefenseBonus = 1;

    /// <summary>
    /// Gets the unique identifier for this rune instance.
    /// </summary>
    public Guid RuneId { get; init; }

    /// <summary>
    /// Gets the type of rune inscribed.
    /// </summary>
    public RuneType RuneType { get; init; }

    /// <summary>
    /// Gets the ID of the item this rune is inscribed on.
    /// </summary>
    public Guid TargetItemId { get; init; }

    /// <summary>
    /// Gets the enhancement bonus value (e.g., +2 damage or +1 defense).
    /// </summary>
    public int EnhancementBonus { get; init; }

    /// <summary>
    /// Gets the remaining duration in turns before this rune expires.
    /// </summary>
    public int RemainingDuration { get; init; }

    /// <summary>
    /// Gets the timestamp when this rune was created.
    /// </summary>
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// Gets whether this rune has expired (remaining duration is 0 or less).
    /// </summary>
    public bool IsExpired => RemainingDuration <= 0;

    /// <summary>
    /// Creates an Enhancement rune for a weapon (+2 damage, 10 turns).
    /// </summary>
    /// <param name="targetItemId">ID of the weapon to inscribe.</param>
    /// <returns>A new Enhancement rune instance.</returns>
    public static InscribedRune CreateEnhancement(Guid targetItemId) => new()
    {
        RuneId = Guid.NewGuid(),
        RuneType = RuneType.Enhancement,
        TargetItemId = targetItemId,
        EnhancementBonus = WeaponDamageBonus,
        RemainingDuration = DefaultDuration,
        CreatedAt = DateTime.UtcNow
    };

    /// <summary>
    /// Creates a Protection rune for armor (+1 Defense, 10 turns).
    /// </summary>
    /// <param name="targetItemId">ID of the armor to inscribe.</param>
    /// <returns>A new Protection rune instance.</returns>
    public static InscribedRune CreateProtection(Guid targetItemId) => new()
    {
        RuneId = Guid.NewGuid(),
        RuneType = RuneType.Protection,
        TargetItemId = targetItemId,
        EnhancementBonus = ArmorDefenseBonus,
        RemainingDuration = DefaultDuration,
        CreatedAt = DateTime.UtcNow
    };

    /// <summary>
    /// Advances the rune by one turn, decrementing the remaining duration.
    /// </summary>
    /// <returns>A new instance with decremented duration.</returns>
    public InscribedRune Tick() => this with
    {
        RemainingDuration = Math.Max(0, RemainingDuration - 1)
    };

    /// <summary>
    /// Gets a human-readable description of this rune's effect.
    /// </summary>
    /// <returns>Effect description string.</returns>
    public string GetEffectDescription() => RuneType switch
    {
        RuneType.Enhancement => $"+{EnhancementBonus} damage ({RemainingDuration} turns remaining)",
        RuneType.Protection => $"+{EnhancementBonus} Defense ({RemainingDuration} turns remaining)",
        _ => $"Rune effect ({RemainingDuration} turns remaining)"
    };

    /// <summary>
    /// Returns a human-readable representation of the inscribed rune.
    /// </summary>
    public override string ToString() =>
        $"{RuneType} Rune on item {TargetItemId}: {GetEffectDescription()}";
}
