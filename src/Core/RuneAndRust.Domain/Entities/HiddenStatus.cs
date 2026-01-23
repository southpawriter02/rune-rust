namespace RuneAndRust.Domain.Entities;

using RuneAndRust.Domain.Enums;

/// <summary>
/// Tracks the [Hidden] status for a character.
/// </summary>
/// <remarks>
/// <para>
/// The [Hidden] status provides significant tactical advantages:
/// <list type="bullet">
///   <item><description>Cannot be directly targeted by single-target attacks</description></item>
///   <item><description>Can still be hit by AoE abilities</description></item>
///   <item><description>First attack from hiding grants advantage</description></item>
///   <item><description>Enemies must pass Perception checks to detect</description></item>
/// </list>
/// </para>
/// <para>
/// [Hidden] is broken by attacking, loud actions, or enemy critical Perception.
/// Certain specialization abilities allow maintaining or re-entering [Hidden].
/// </para>
/// </remarks>
public sealed class HiddenStatus
{
    /// <summary>
    /// Gets the unique identifier for this hidden status instance.
    /// </summary>
    public string HiddenId { get; private set; }

    /// <summary>
    /// Gets the identifier of the character who is hidden.
    /// </summary>
    public string CharacterId { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the character is currently hidden.
    /// </summary>
    public bool IsHidden { get; private set; }

    /// <summary>
    /// Gets the timestamp when the character entered [Hidden] state.
    /// </summary>
    public DateTime HiddenSince { get; private set; }

    /// <summary>
    /// Gets the timestamp when [Hidden] was broken, if applicable.
    /// </summary>
    public DateTime? BrokenAt { get; private set; }

    /// <summary>
    /// Gets the conditions that will break the [Hidden] status.
    /// </summary>
    public IReadOnlyList<HiddenBreakCondition> BreakConditions => _breakConditions.AsReadOnly();
    private readonly List<HiddenBreakCondition> _breakConditions;

    /// <summary>
    /// Gets the detection modifier for enemies trying to spot this character.
    /// </summary>
    /// <remarks>
    /// Positive values make the character harder to detect.
    /// Added to the DC for enemy Perception checks.
    /// </remarks>
    public int DetectionModifier { get; private set; }

    /// <summary>
    /// Gets the condition that broke [Hidden], if applicable.
    /// </summary>
    public HiddenBreakCondition? BrokenBy { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the character has used their "from hiding" attack bonus.
    /// </summary>
    public bool HasUsedHidingBonus { get; private set; }

    /// <summary>
    /// Gets the source of this [Hidden] status.
    /// </summary>
    public string Source { get; private set; }

    // Private constructor for factory pattern
    private HiddenStatus()
    {
        HiddenId = string.Empty;
        CharacterId = string.Empty;
        Source = string.Empty;
        _breakConditions = [];
    }

    /// <summary>
    /// Creates a new [Hidden] status for a character via successful stealth check.
    /// </summary>
    /// <param name="characterId">The ID of the character becoming hidden.</param>
    /// <param name="detectionModifier">Modifier to enemy Perception DCs (default 0).</param>
    /// <returns>A new HiddenStatus in the active [Hidden] state.</returns>
    public static HiddenStatus FromStealthCheck(string characterId, int detectionModifier = 0)
    {
        return new HiddenStatus
        {
            HiddenId = Guid.NewGuid().ToString(),
            CharacterId = characterId,
            IsHidden = true,
            HiddenSince = DateTime.UtcNow,
            DetectionModifier = detectionModifier,
            Source = "StealthCheck",
            _breakConditions =
            {
                HiddenBreakCondition.Attack,
                HiddenBreakCondition.LoudAction,
                HiddenBreakCondition.EnemyCriticalPerception
            }
        };
    }

    /// <summary>
    /// Creates a new [Hidden] status via [Slip into Shadow] ability.
    /// </summary>
    /// <param name="characterId">The ID of the character becoming hidden.</param>
    /// <returns>A new HiddenStatus with enhanced detection modifier.</returns>
    /// <remarks>
    /// [Slip into Shadow] grants +1 detection modifier compared to normal stealth.
    /// </remarks>
    public static HiddenStatus FromSlipIntoShadow(string characterId)
    {
        return new HiddenStatus
        {
            HiddenId = Guid.NewGuid().ToString(),
            CharacterId = characterId,
            IsHidden = true,
            HiddenSince = DateTime.UtcNow,
            DetectionModifier = 1, // Enhanced from ability
            Source = "SlipIntoShadow",
            _breakConditions =
            {
                HiddenBreakCondition.Attack,
                HiddenBreakCondition.LoudAction,
                HiddenBreakCondition.EnemyCriticalPerception
            }
        };
    }

    /// <summary>
    /// Creates a new [Hidden] status via [One with the Static] in [Psychic Resonance] zones.
    /// </summary>
    /// <param name="characterId">The ID of the character becoming hidden.</param>
    /// <returns>A new HiddenStatus with zone-specific break conditions.</returns>
    public static HiddenStatus FromOneWithTheStatic(string characterId)
    {
        return new HiddenStatus
        {
            HiddenId = Guid.NewGuid().ToString(),
            CharacterId = characterId,
            IsHidden = true,
            HiddenSince = DateTime.UtcNow,
            DetectionModifier = 2, // Strong bonus in corruption zones
            Source = "OneWithTheStatic",
            _breakConditions =
            {
                HiddenBreakCondition.Attack,
                HiddenBreakCondition.LoudAction,
                HiddenBreakCondition.EnemyCriticalPerception,
                HiddenBreakCondition.LeaveZone
            }
        };
    }

    /// <summary>
    /// Breaks the [Hidden] status due to a specific condition.
    /// </summary>
    /// <param name="condition">The condition that broke [Hidden].</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when already not hidden.
    /// </exception>
    public void Break(HiddenBreakCondition condition)
    {
        if (!IsHidden)
        {
            throw new InvalidOperationException("Cannot break [Hidden] status - character is not hidden.");
        }

        IsHidden = false;
        BrokenAt = DateTime.UtcNow;
        BrokenBy = condition;
    }

    /// <summary>
    /// Marks that the character has used their "from hiding" attack advantage.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when not hidden or bonus already used.
    /// </exception>
    public void UseHidingBonus()
    {
        if (!IsHidden)
        {
            throw new InvalidOperationException("Cannot use hiding bonus - character is not hidden.");
        }

        if (HasUsedHidingBonus)
        {
            throw new InvalidOperationException("Hiding bonus has already been used.");
        }

        HasUsedHidingBonus = true;
    }

    /// <summary>
    /// Checks if a specific condition would break this [Hidden] status.
    /// </summary>
    /// <param name="condition">The condition to check.</param>
    /// <returns>True if this condition would break [Hidden].</returns>
    public bool WouldBreak(HiddenBreakCondition condition)
    {
        return IsHidden && _breakConditions.Contains(condition);
    }

    /// <summary>
    /// Gets a description of the [Hidden] status for display.
    /// </summary>
    /// <returns>A human-readable status description.</returns>
    public string ToDescription()
    {
        if (!IsHidden)
        {
            var brokenStr = BrokenBy.HasValue
                ? $" (broken by {BrokenBy.Value})"
                : "";
            return $"Not hidden{brokenStr}";
        }

        var durationSeconds = (DateTime.UtcNow - HiddenSince).TotalSeconds;
        var modStr = DetectionModifier != 0
            ? $", Detection +{DetectionModifier}"
            : "";
        var bonusStr = HasUsedHidingBonus
            ? ", attack bonus used"
            : ", attack bonus available";

        return $"[Hidden] for {durationSeconds:F0}s{modStr}{bonusStr}";
    }

    /// <inheritdoc/>
    public override string ToString() => ToDescription();
}
