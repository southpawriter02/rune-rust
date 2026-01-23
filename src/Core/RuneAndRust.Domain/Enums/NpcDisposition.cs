namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Represents an NPC's attitude toward the player, providing dice modifiers.
/// </summary>
/// <remarks>
/// <para>
/// Disposition is derived from a numeric value (-100 to +100) and provides
/// dice modifiers that apply to social skill checks. Higher disposition
/// makes social interactions easier; lower makes them harder.
/// </para>
/// <para>
/// The disposition value is stored in <see cref="ValueObjects.DispositionLevel"/>
/// and converted to this enum for modifier calculation.
/// </para>
/// </remarks>
public enum NpcDisposition
{
    /// <summary>
    /// Deep trust and loyalty. Will take personal risks for the player.
    /// </summary>
    /// <remarks>
    /// <para>Threshold: Disposition value ≥ 75</para>
    /// <para>Modifier: +3d10 to social checks</para>
    /// <para>Examples: Sworn companions, family, life-debt holders</para>
    /// </remarks>
    Ally = 5,

    /// <summary>
    /// Positive regard, willing to help within reason.
    /// </summary>
    /// <remarks>
    /// <para>Threshold: Disposition value 50-74</para>
    /// <para>Modifier: +2d10 to social checks</para>
    /// <para>Examples: Friends, trusted associates, grateful NPCs</para>
    /// </remarks>
    Friendly = 4,

    /// <summary>
    /// Generally well-disposed, open to interaction.
    /// </summary>
    /// <remarks>
    /// <para>Threshold: Disposition value 10-49</para>
    /// <para>Modifier: +1d10 to social checks</para>
    /// <para>Examples: Acquaintances, fellow guild members, satisfied customers</para>
    /// </remarks>
    NeutralPositive = 3,

    /// <summary>
    /// No particular opinion, professional distance.
    /// </summary>
    /// <remarks>
    /// <para>Threshold: Disposition value -9 to +9</para>
    /// <para>Modifier: +0 to social checks</para>
    /// <para>Examples: Strangers, merchants, guards on duty</para>
    /// </remarks>
    Neutral = 2,

    /// <summary>
    /// Distrustful, unwilling to help without strong incentive.
    /// </summary>
    /// <remarks>
    /// <para>Threshold: Disposition value -49 to -10</para>
    /// <para>Modifier: -1d10 to social checks</para>
    /// <para>Examples: Rival faction members, cheated traders, spurned suitors</para>
    /// </remarks>
    Unfriendly = 1,

    /// <summary>
    /// Active animosity, may refuse interaction entirely.
    /// </summary>
    /// <remarks>
    /// <para>Threshold: Disposition value ≤ -50</para>
    /// <para>Modifier: -2d10 to social checks</para>
    /// <para>Examples: Enemies, betrayed allies, faction at war</para>
    /// <para>Note: Some interactions may be impossible at this level.</para>
    /// </remarks>
    Hostile = 0
}
