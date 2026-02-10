// ═══════════════════════════════════════════════════════════════════════════════
// EmpoweredRune.cs
// Immutable value object representing an elemental-empowered rune inscribed on
// a weapon by the Rúnasmiðr specialization. Adds bonus elemental damage to
// attacks for a limited duration.
// Version: 0.20.2b
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents an elemental-empowered rune inscribed on a weapon by the Rúnasmiðr.
/// </summary>
/// <remarks>
/// <para>
/// Empowered Inscription is a Tier 2 ability that enhances a weapon with elemental
/// damage. The rune adds +1d6 damage of a chosen element (fire, cold, lightning,
/// or aetheric) to all attacks made with the inscribed weapon. It stacks with the
/// base Inscribe Rune bonus (+2 damage) for a combined effect.
/// </para>
/// <para>
/// Only one empowered rune may be active per weapon at a time. Applying a new
/// empowered rune to a weapon that already has one replaces the existing rune.
/// </para>
/// <para>
/// This is an immutable value object — the <see cref="Tick"/> method returns
/// a new instance with decremented duration. The original instance is never modified.
/// </para>
/// <para>
/// <b>Cost:</b> 4 AP, 2 Rune Charges. <b>Duration:</b> 10 turns.
/// <b>Tier:</b> 2 (requires 8 PP invested in Rúnasmiðr tree).
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var rune = EmpoweredRune.Create(weaponId, "fire");
/// // rune.RemainingDuration = 10, rune.BonusDice = "1d6"
/// var ticked = rune.Tick();
/// // ticked.RemainingDuration = 9
/// </code>
/// </example>
/// <seealso cref="InscribedRune"/>
/// <seealso cref="RuneAndRust.Domain.Enums.RuneType"/>
public sealed record EmpoweredRune
{
    /// <summary>
    /// Default duration for empowered runes, in turns.
    /// </summary>
    public const int DefaultDuration = 10;

    /// <summary>
    /// Bonus dice added by the empowered rune (1d6).
    /// </summary>
    public const string BonusDiceValue = "1d6";

    /// <summary>
    /// Average damage dealt by a 1d6 roll.
    /// </summary>
    public const decimal AverageDamagePerDie = 3.5m;

    /// <summary>
    /// Valid elemental damage type IDs that can be applied via Empowered Inscription.
    /// </summary>
    /// <remarks>
    /// Uses string-based damage type IDs from <c>config/damage-types.json</c>
    /// rather than an enum, following the project's data-driven design rules.
    /// </remarks>
    public static readonly IReadOnlyList<string> ValidElements = new[]
    {
        "fire",
        "cold",
        "lightning",
        "aetheric"
    };

    /// <summary>
    /// Gets the unique identifier for this empowered rune instance.
    /// </summary>
    public Guid RuneId { get; init; }

    /// <summary>
    /// Gets the ID of the weapon this rune is inscribed on.
    /// </summary>
    public Guid TargetItemId { get; init; }

    /// <summary>
    /// Gets the elemental damage type ID for this rune.
    /// </summary>
    /// <remarks>
    /// Must be one of the values in <see cref="ValidElements"/>:
    /// "fire", "cold", "lightning", or "aetheric".
    /// </remarks>
    public string ElementalDamageTypeId { get; init; } = string.Empty;

    /// <summary>
    /// Gets the bonus dice expression for this rune (always "1d6").
    /// </summary>
    public string BonusDice { get; init; } = BonusDiceValue;

    /// <summary>
    /// Gets the remaining duration in turns before this rune expires.
    /// </summary>
    public int RemainingDuration { get; init; }

    /// <summary>
    /// Gets the original (maximum) duration in turns.
    /// </summary>
    public int OriginalDuration { get; init; }

    /// <summary>
    /// Gets the timestamp when this rune was created.
    /// </summary>
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// Gets whether this rune has expired (remaining duration is 0 or less).
    /// </summary>
    public bool IsExpired => RemainingDuration <= 0;

    /// <summary>
    /// Creates an empowered rune with the specified elemental damage type.
    /// </summary>
    /// <param name="targetItemId">ID of the weapon to inscribe.</param>
    /// <param name="elementalDamageTypeId">
    /// The elemental damage type ID. Must be one of:
    /// "fire", "cold", "lightning", or "aetheric".
    /// </param>
    /// <returns>A new EmpoweredRune instance with default duration (10 turns).</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="elementalDamageTypeId"/> is null, whitespace,
    /// or not a valid element.
    /// </exception>
    public static EmpoweredRune Create(Guid targetItemId, string elementalDamageTypeId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(elementalDamageTypeId);

        var normalizedElement = elementalDamageTypeId.ToLowerInvariant();
        if (!IsValidElement(normalizedElement))
        {
            throw new ArgumentException(
                $"Invalid elemental damage type '{elementalDamageTypeId}'. " +
                $"Must be one of: {string.Join(", ", ValidElements)}.",
                nameof(elementalDamageTypeId));
        }

        return new EmpoweredRune
        {
            RuneId = Guid.NewGuid(),
            TargetItemId = targetItemId,
            ElementalDamageTypeId = normalizedElement,
            BonusDice = BonusDiceValue,
            RemainingDuration = DefaultDuration,
            OriginalDuration = DefaultDuration,
            CreatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Advances the rune by one turn, decrementing the remaining duration.
    /// </summary>
    /// <returns>A new instance with decremented duration (minimum 0).</returns>
    public EmpoweredRune Tick() => this with
    {
        RemainingDuration = Math.Max(0, RemainingDuration - 1)
    };

    /// <summary>
    /// Validates whether the given damage type ID is a valid element for empowered runes.
    /// </summary>
    /// <param name="elementId">Damage type ID to validate.</param>
    /// <returns><c>true</c> if the element is valid; otherwise, <c>false</c>.</returns>
    public static bool IsValidElement(string elementId)
    {
        if (string.IsNullOrWhiteSpace(elementId))
            return false;

        return ValidElements.Contains(elementId.ToLowerInvariant());
    }

    /// <summary>
    /// Gets a human-readable display of the elemental type and its visual effect.
    /// </summary>
    /// <returns>A descriptive string including the element name and visual flavor.</returns>
    public string GetElementDisplay() => ElementalDamageTypeId switch
    {
        "fire" => "Fire (flames flicker along blade)",
        "cold" => "Cold (frost rimes the edge)",
        "lightning" => "Lightning (sparks crackle)",
        "aetheric" => "Aetheric (purple glow pulses)",
        _ => $"Unknown ({ElementalDamageTypeId})"
    };

    /// <summary>
    /// Gets the average damage per hit from this empowered rune (1d6 = 3.5 average).
    /// </summary>
    /// <returns>The average damage as a decimal value.</returns>
    public decimal GetAverageDamage() => AverageDamagePerDie;

    /// <summary>
    /// Gets a combined effect description showing both base inscription bonus
    /// and this elemental enhancement.
    /// </summary>
    /// <returns>A combined effect description string.</returns>
    public string GetCombinedEffectDisplay() =>
        $"+2 damage + {BonusDice} {GetElementDisplay()} damage";

    /// <summary>
    /// Returns a human-readable representation of the empowered rune.
    /// </summary>
    public override string ToString() =>
        $"Empowered Rune [{ElementalDamageTypeId}] on item {TargetItemId}: " +
        $"+{BonusDice} {ElementalDamageTypeId} damage ({RemainingDuration}/{OriginalDuration} turns)";
}
