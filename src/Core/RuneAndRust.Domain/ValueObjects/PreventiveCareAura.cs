namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Immutable result record representing the Preventive Care passive aura evaluation.
/// Identifies which allies are within the aura radius and the saving throw bonuses they receive.
/// </summary>
/// <remarks>
/// <para>Introduced in v0.20.6c as the Tier 3 passive protection ability.</para>
/// <para>Preventive Care is always active when unlocked — no AP or supply cost.
/// All allies within 5 spaces of the Bone-Setter gain +1 to saving throws
/// against poison and disease effects.</para>
/// <para>Key properties (all computed constants):</para>
/// <list type="bullet">
/// <item><see cref="AuraRadius"/>: Always 5 spaces</item>
/// <item><see cref="PoisonSaveBonus"/>: Always +1</item>
/// <item><see cref="DiseaseSaveBonus"/>: Always +1</item>
/// <item><see cref="IsActive"/>: Always true (passive ability)</item>
/// </list>
/// <para>No Corruption risk — Preventive Care follows the Coherent path.</para>
/// </remarks>
public sealed record PreventiveCareAura
{
    /// <summary>
    /// Unique identifier of the Bone-Setter providing the aura.
    /// </summary>
    public Guid BoneSetterId { get; init; }

    /// <summary>
    /// Radius of the aura in spaces. Always 5.
    /// All allies within this distance receive the saving throw bonuses.
    /// </summary>
    public int AuraRadius => 5;

    /// <summary>
    /// Bonus granted to poison saving throws for allies within the aura. Always +1.
    /// </summary>
    public int PoisonSaveBonus => 1;

    /// <summary>
    /// Bonus granted to disease saving throws for allies within the aura. Always +1.
    /// </summary>
    public int DiseaseSaveBonus => 1;

    /// <summary>
    /// Collection of ally identifiers currently within the aura radius
    /// and benefiting from the saving throw bonuses.
    /// </summary>
    public IReadOnlyList<Guid> AffectedAllyIds { get; init; } = [];

    /// <summary>
    /// Whether the aura is currently active. Always true for passive abilities —
    /// Preventive Care is active whenever the Bone-Setter has the ability unlocked.
    /// </summary>
    public bool IsActive => true;

    /// <summary>
    /// Returns a formatted summary of the aura's current effect.
    /// </summary>
    /// <returns>
    /// A string showing the radius, bonus values, and number of affected allies.
    /// </returns>
    public string GetAuraSummary() =>
        $"Preventive Care Aura ({AuraRadius} spaces): " +
        $"+{PoisonSaveBonus} poison/disease saves for {AffectedAllyIds.Count} allies";
}
