namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents an active Runestone Ward that absorbs incoming damage before
/// the character's HP is affected.
/// </summary>
/// <remarks>
/// <para>Created by the Rúnasmiðr's Runestone Ward ability (Tier 1):</para>
/// <list type="bullet">
/// <item>Absorbs up to 10 damage (default)</item>
/// <item>Only one ward per character at a time</item>
/// <item>Ward is destroyed when absorption capacity reaches 0</item>
/// <item>Excess damage beyond the ward's remaining capacity passes through to HP</item>
/// </list>
/// <para>The ward interacts with the damage pipeline: incoming damage is first
/// applied to the ward's <see cref="RemainingAbsorption"/>. Any overflow damage
/// is then applied to the character's HP normally.</para>
/// </remarks>
public sealed record RunestoneWard
{
    /// <summary>
    /// Default maximum damage absorption for a Tier 1 ward.
    /// </summary>
    public const int DefaultAbsorption = 10;

    /// <summary>
    /// Visual bar segment count for absorption display.
    /// </summary>
    private const int VisualBarSegments = 10;

    /// <summary>
    /// Unique identifier for this ward instance.
    /// </summary>
    public Guid WardId { get; private set; }

    /// <summary>
    /// The player ID of the ward's owner.
    /// </summary>
    public Guid OwnerId { get; private set; }

    /// <summary>
    /// Maximum damage this ward can absorb.
    /// </summary>
    public int DamageAbsorption { get; private set; }

    /// <summary>
    /// Current remaining absorption capacity.
    /// Decreases as the ward absorbs damage.
    /// </summary>
    public int RemainingAbsorption { get; private set; }

    /// <summary>
    /// UTC timestamp when this ward was created.
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// Creates a new RunestoneWard at full absorption capacity.
    /// </summary>
    /// <param name="ownerId">The player ID of the ward owner.</param>
    /// <param name="absorption">Maximum absorption amount (default <see cref="DefaultAbsorption"/>).</param>
    /// <returns>A new ward initialized to full absorption capacity.</returns>
    public static RunestoneWard Create(Guid ownerId, int absorption = DefaultAbsorption)
    {
        return new RunestoneWard
        {
            WardId = Guid.NewGuid(),
            OwnerId = ownerId,
            DamageAbsorption = absorption,
            RemainingAbsorption = absorption,
            CreatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Absorbs incoming damage and returns any overflow that passes through.
    /// </summary>
    /// <param name="incomingDamage">The raw damage amount to absorb.</param>
    /// <returns>
    /// The amount of damage that overflows past the ward (0 if fully absorbed).
    /// </returns>
    /// <remarks>
    /// <para>If the ward can absorb the full damage, <see cref="RemainingAbsorption"/>
    /// is reduced by the damage amount and 0 overflow is returned.</para>
    /// <para>If the damage exceeds <see cref="RemainingAbsorption"/>, the ward is
    /// reduced to 0 and the excess is returned as overflow damage.</para>
    /// <para>Negative or zero damage returns 0 overflow and does not affect the ward.</para>
    /// </remarks>
    public int AbsorbDamage(int incomingDamage)
    {
        if (incomingDamage <= 0)
            return 0;

        if (RemainingAbsorption >= incomingDamage)
        {
            // Ward absorbs all damage
            RemainingAbsorption -= incomingDamage;
            return 0;
        }

        // Ward absorbs partial damage, overflow passes through
        var overflow = incomingDamage - RemainingAbsorption;
        RemainingAbsorption = 0;
        return overflow;
    }

    /// <summary>
    /// Checks whether the ward has been destroyed (no absorption remaining).
    /// </summary>
    /// <returns>True if <see cref="RemainingAbsorption"/> is 0 or less.</returns>
    public bool IsDestroyed() => RemainingAbsorption <= 0;

    /// <summary>
    /// Gets remaining absorption capacity as a percentage.
    /// </summary>
    /// <returns>Percentage from 0.0 to 1.0.</returns>
    public double GetRemainingCapacityPercent() =>
        DamageAbsorption > 0 ? (double)RemainingAbsorption / DamageAbsorption : 0.0;

    /// <summary>
    /// Gets a visual bar representation of remaining ward strength.
    /// </summary>
    /// <returns>
    /// ASCII bar like "[████████░░]" showing remaining/total absorption.
    /// </returns>
    public string GetVisualBar()
    {
        var filledSegments = DamageAbsorption > 0
            ? (int)Math.Round((double)RemainingAbsorption / DamageAbsorption * VisualBarSegments)
            : 0;
        var emptySegments = VisualBarSegments - filledSegments;

        return $"[{new string('█', filledSegments)}{new string('░', emptySegments)}]";
    }

    /// <summary>
    /// Gets a display string showing absorption status.
    /// </summary>
    /// <returns>Absorption as "X/Y" format (e.g., "7/10").</returns>
    public string GetAbsorptionDisplay() =>
        $"{RemainingAbsorption}/{DamageAbsorption}";
}
