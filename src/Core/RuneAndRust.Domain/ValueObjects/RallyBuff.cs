// ═══════════════════════════════════════════════════════════════════════════════
// RallyBuff.cs
// Immutable value object representing a Rally saving throw bonus applied to an
// ally by a Skjaldmær. Consumed after the ally's next saving throw.
// Version: 0.20.1b
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents a Rally buff granting a saving throw bonus to an allied character.
/// </summary>
/// <remarks>
/// <para>
/// Rally is a Tier 2 active ability (2 AP cost, 6-space radius) that grants
/// all allies within range +2 to their next saving throw. The buff is consumed
/// after one save attempt (whether successful or failed).
/// </para>
/// <para>
/// This is an immutable value object. The <see cref="Consume"/> method returns
/// a new instance with <see cref="IsConsumed"/> set to true.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var buff = RallyBuff.Create(allyId, casterId);
/// // buff.SaveBonus == 2, buff.IsConsumed == false
/// int bonus = buff.GetBonusForCharacter(allyId); // 2
/// var consumed = buff.Consume();
/// // consumed.IsConsumed == true, consumed.IsActive() == false
/// </code>
/// </example>
public sealed record RallyBuff
{
    /// <summary>Default saving throw bonus granted by Rally.</summary>
    public const int DefaultSaveBonus = 2;

    /// <summary>AP cost to activate Rally.</summary>
    public const int ActivationCost = 2;

    /// <summary>Maximum radius in spaces for Rally to affect allies.</summary>
    public const int EffectRadius = 6;

    /// <summary>Gets the ID of the character receiving the buff.</summary>
    public Guid AffectedCharacterId { get; init; }

    /// <summary>Gets the ID of the Skjaldmær who cast Rally.</summary>
    public Guid CasterCharacterId { get; init; }

    /// <summary>Gets the saving throw bonus granted by this buff.</summary>
    public int SaveBonus { get; init; } = DefaultSaveBonus;

    /// <summary>Gets whether this buff has been consumed by a saving throw.</summary>
    public bool IsConsumed { get; init; }

    /// <summary>Gets the timestamp when the buff was applied.</summary>
    public DateTime AppliedAt { get; init; }

    /// <summary>
    /// Creates a new Rally buff for the specified ally from the specified caster.
    /// </summary>
    /// <param name="affectedCharacterId">ID of the ally receiving the buff.</param>
    /// <param name="casterCharacterId">ID of the Skjaldmær casting Rally.</param>
    /// <returns>A new active RallyBuff with <see cref="DefaultSaveBonus"/>.</returns>
    public static RallyBuff Create(Guid affectedCharacterId, Guid casterCharacterId) => new()
    {
        AffectedCharacterId = affectedCharacterId,
        CasterCharacterId = casterCharacterId,
        SaveBonus = DefaultSaveBonus,
        IsConsumed = false,
        AppliedAt = DateTime.UtcNow
    };

    /// <summary>
    /// Marks the buff as consumed. Called after the ally makes a saving throw.
    /// </summary>
    /// <returns>A new instance with <see cref="IsConsumed"/> set to true.</returns>
    public RallyBuff Consume() => this with { IsConsumed = true };

    /// <summary>
    /// Gets whether the buff is still active (not yet consumed).
    /// </summary>
    /// <returns>True if the buff has not been consumed.</returns>
    public bool IsActive() => !IsConsumed;

    /// <summary>
    /// Gets the saving throw bonus for the specified character.
    /// </summary>
    /// <param name="characterId">The character to check.</param>
    /// <returns>
    /// The save bonus if the buff is active and targets this character; 0 otherwise.
    /// </returns>
    public int GetBonusForCharacter(Guid characterId)
    {
        if (IsConsumed || characterId != AffectedCharacterId)
            return 0;

        return SaveBonus;
    }

    /// <inheritdoc />
    public override string ToString() =>
        IsConsumed
            ? $"Rally Buff [CONSUMED] for {AffectedCharacterId}"
            : $"Rally Buff [ACTIVE] +{SaveBonus} save for {AffectedCharacterId}";
}
