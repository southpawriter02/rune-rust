using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents an elemental empowerment applied to a weapon via the Empowered Inscription ability.
/// Adds +1d6 elemental damage of a chosen type for a limited number of turns.
/// </summary>
/// <remarks>
/// <para>Empowered Inscription is a Tier 2 Rúnasmiðr ability (v0.20.2b) that enhances weapons
/// beyond the base Inscribe Rune bonus:</para>
/// <list type="bullet">
/// <item>Cost: 4 AP + 2 Rune Charges</item>
/// <item>Adds +1d6 elemental damage (Fire, Cold, Lightning, or Aetheric)</item>
/// <item>Stacks with base Inscribe Rune (+2 weapon damage)</item>
/// <item>Duration: 10 turns (default)</item>
/// <item>Only one empowered inscription per weapon at a time</item>
/// <item>Requires 8+ PP invested and 4 PP to unlock</item>
/// </list>
/// <para>Follows the same sealed record pattern as <see cref="InscribedRune"/> for consistency
/// across rune effect types.</para>
/// </remarks>
public sealed record EmpoweredRune
{
    /// <summary>
    /// Default duration for empowered inscriptions (in turns).
    /// </summary>
    public const int DefaultDuration = 10;

    /// <summary>
    /// Default bonus dice for empowered inscriptions.
    /// </summary>
    public const string DefaultBonusDice = "1d6";

    /// <summary>
    /// Average damage per hit from 1d6 dice roll.
    /// </summary>
    public const decimal AverageDiceRollDamage = 3.5m;

    /// <summary>
    /// Allowed elemental type IDs for empowered inscriptions.
    /// </summary>
    public static readonly IReadOnlyList<string> AllowedElements =
        ["fire", "cold", "lightning", "aetheric"];

    /// <summary>
    /// Unique identifier for this empowered rune inscription instance.
    /// </summary>
    public Guid RuneId { get; private set; }

    /// <summary>
    /// The weapon item ID this empowerment is applied to.
    /// </summary>
    public Guid TargetItemId { get; private set; }

    /// <summary>
    /// The elemental damage type ID (e.g., "fire", "cold", "lightning", "aetheric").
    /// </summary>
    /// <remarks>
    /// Uses string-based IDs consistent with <see cref="Definitions.DamageTypeDefinition"/>
    /// rather than an enum, following data-driven design principles.
    /// </remarks>
    public string ElementalTypeId { get; private set; } = string.Empty;

    /// <summary>
    /// The bonus dice expression for elemental damage (always "1d6").
    /// </summary>
    public string BonusDice { get; private set; } = DefaultBonusDice;

    /// <summary>
    /// Remaining turns until the empowerment expires.
    /// Decremented each turn via <see cref="Tick"/>.
    /// </summary>
    public int Duration { get; private set; }

    /// <summary>
    /// The original duration when the rune was first empowered.
    /// Used for percentage calculations and display.
    /// </summary>
    public int OriginalDuration { get; private set; }

    /// <summary>
    /// UTC timestamp when the empowered inscription was created.
    /// Used for auditing and display.
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// Creates a new EmpoweredRune for a weapon with the specified elemental type.
    /// </summary>
    /// <param name="targetItemId">The weapon item ID to empower.</param>
    /// <param name="elementalTypeId">The elemental type ID ("fire", "cold", "lightning", or "aetheric").</param>
    /// <param name="duration">Duration in turns (default <see cref="DefaultDuration"/>).</param>
    /// <returns>A new empowered rune with the specified element and duration.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="elementalTypeId"/> is null, whitespace, or not a valid element.
    /// </exception>
    public static EmpoweredRune Create(
        Guid targetItemId,
        string elementalTypeId,
        int duration = DefaultDuration)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(elementalTypeId);

        var normalizedElement = elementalTypeId.ToLowerInvariant();
        if (!IsValidElement(normalizedElement))
        {
            throw new ArgumentException(
                $"Invalid elemental type '{elementalTypeId}'. Allowed: {string.Join(", ", AllowedElements)}",
                nameof(elementalTypeId));
        }

        return new EmpoweredRune
        {
            RuneId = Guid.NewGuid(),
            TargetItemId = targetItemId,
            ElementalTypeId = normalizedElement,
            BonusDice = DefaultBonusDice,
            Duration = duration,
            OriginalDuration = duration,
            CreatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Validates whether the given element ID is an allowed elemental type.
    /// </summary>
    /// <param name="elementalTypeId">The element ID to validate.</param>
    /// <returns>True if the element is Fire, Cold, Lightning, or Aetheric.</returns>
    public static bool IsValidElement(string elementalTypeId)
    {
        if (string.IsNullOrWhiteSpace(elementalTypeId))
            return false;

        return AllowedElements.Contains(elementalTypeId.ToLowerInvariant());
    }

    /// <summary>
    /// Advances the empowerment's duration by one turn.
    /// </summary>
    /// <remarks>
    /// Reduces <see cref="Duration"/> by 1, minimum 0.
    /// After ticking, check <see cref="IsExpired"/> to determine if the empowerment should be removed.
    /// </remarks>
    public void Tick()
    {
        if (Duration > 0)
            Duration--;
    }

    /// <summary>
    /// Checks whether this empowered rune has expired (duration reached 0).
    /// </summary>
    /// <returns>True if <see cref="Duration"/> is 0 or less.</returns>
    public bool IsExpired() => Duration <= 0;

    /// <summary>
    /// Gets a human-readable description of the elemental type with atmospheric flavor.
    /// </summary>
    /// <returns>Descriptive string for the elemental type.</returns>
    public string GetElementDisplay() => ElementalTypeId switch
    {
        "fire" => "Fire (flames flicker)",
        "cold" => "Cold (frost rime)",
        "lightning" => "Lightning (sparks crackle)",
        "aetheric" => "Aetheric (purple glow)",
        _ => "Unknown"
    };

    /// <summary>
    /// Gets the average damage per hit from the bonus dice (1d6 = 3.5 average).
    /// </summary>
    /// <returns>Average damage value of 3.5.</returns>
    public decimal GetAverageDamage() => AverageDiceRollDamage;

    /// <summary>
    /// Gets a combined effect description showing base and elemental bonuses.
    /// </summary>
    /// <returns>Combined effect string showing +2 base and +1d6 elemental damage.</returns>
    public string GetCombinedEffectDisplay() =>
        $"+2 damage + {BonusDice} {GetElementDisplay()} damage ({Duration} turns remaining)";

    /// <summary>
    /// Gets a compact duration display string.
    /// </summary>
    /// <returns>Duration as "X/Y turns" format.</returns>
    public string GetDurationDisplay() => $"{Duration}/{OriginalDuration} turns";
}
