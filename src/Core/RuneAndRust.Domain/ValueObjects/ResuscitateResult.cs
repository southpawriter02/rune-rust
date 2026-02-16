using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Immutable result record from the Resuscitate ability execution.
/// Records the revival of an unconscious character back to 1 HP.
/// </summary>
/// <remarks>
/// <para>Introduced in v0.20.6c as the Tier 3 active revival ability result.</para>
/// <para>Resuscitate always restores to exactly 1 HP — the target is conscious
/// but extremely vulnerable. The party should prioritize healing the revived
/// target with Field Dressing or Emergency Surgery immediately.</para>
/// <para>Key properties:</para>
/// <list type="bullet">
/// <item><see cref="HpAfter"/>: Always 1 (computed constant)</item>
/// <item><see cref="SuppliesUsed"/>: Always 2 (computed constant)</item>
/// <item><see cref="Method"/>: The resurrection method used (SkillBasedResuscitation)</item>
/// </list>
/// <para>No Corruption risk — Resuscitate follows the Coherent path.</para>
/// </remarks>
public sealed record ResuscitateResult
{
    /// <summary>
    /// Unique identifier of the resurrected target.
    /// </summary>
    public Guid TargetId { get; init; }

    /// <summary>
    /// Display name of the resurrected target.
    /// </summary>
    public string TargetName { get; init; } = string.Empty;

    /// <summary>
    /// HP value before resuscitation (should always be 0 for unconscious targets).
    /// </summary>
    public int HpBefore { get; init; }

    /// <summary>
    /// HP value after resuscitation. Always 1 — the target is barely conscious.
    /// </summary>
    public int HpAfter => 1;

    /// <summary>
    /// Number of Medical Supplies consumed by Resuscitate. Always 2.
    /// </summary>
    public int SuppliesUsed => 2;

    /// <summary>
    /// Number of Medical Supplies remaining in the Bone-Setter's inventory
    /// after spending 2 supplies for the resuscitation.
    /// </summary>
    public int SuppliesRemaining { get; init; }

    /// <summary>
    /// The method of resurrection used. For Resuscitate, this is always
    /// <see cref="ResurrectionMethod.SkillBasedResuscitation"/>.
    /// </summary>
    public ResurrectionMethod Method { get; init; }

    /// <summary>
    /// Narrative message describing the revival event.
    /// </summary>
    public string ResurrectionMessage { get; init; } = string.Empty;

    /// <summary>
    /// Returns a human-readable status message summarizing the resuscitation outcome.
    /// </summary>
    /// <returns>
    /// A formatted string showing the target name and HP transition (0 → 1 HP).
    /// </returns>
    public string GetStatusMessage() =>
        $"{TargetName} has been RESUSCITATED! ({HpBefore} -> {HpAfter} HP)";
}
