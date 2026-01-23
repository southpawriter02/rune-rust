namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Identifies Gantry-Runner specialization abilities.
/// </summary>
/// <remarks>
/// <para>
/// The Gantry-Runner specialization focuses on acrobatic movement across
/// the urban landscape of the Sprawl, particularly rooftop traversal
/// and death-defying leaps between buildings.
/// </para>
/// <para>
/// These abilities enhance the base climbing (v0.15.2a), leaping (v0.15.2b),
/// and fall damage (v0.15.2c) systems.
/// </para>
/// <para>
/// <b>v0.15.2g:</b> Initial implementation of Gantry-Runner abilities.
/// </para>
/// </remarks>
public enum GantryRunnerAbility
{
    /// <summary>
    /// Reduces climbing stages required by 1.
    /// Type: Passive
    /// </summary>
    /// <remarks>
    /// <para>
    /// Effect: When calculating stages for a climb, subtract 1 from the total
    /// (minimum 1 stage). This represents expertise in finding efficient routes.
    /// </para>
    /// <para>
    /// Formula: StagesRequired = max(1, (Height / 10) - 1)
    /// </para>
    /// <para>
    /// Example: A 30-foot climb normally requires 3 stages.
    /// With [Roof-Runner]: 30 / 10 - 1 = 2 stages.
    /// </para>
    /// </remarks>
    RoofRunner = 0,

    /// <summary>
    /// Adds +10 ft to maximum leap distance.
    /// Type: Passive
    /// </summary>
    /// <remarks>
    /// <para>
    /// Effect: Increases the maximum possible leap distance by 10 feet,
    /// allowing jumps beyond the normal Heroic (25ft) limit.
    /// </para>
    /// <para>
    /// Formula: MaxLeapDistance = BaseMax + 10
    /// </para>
    /// <para>
    /// Example: Normal maximum: 25ft (Heroic leap, DC 5)
    /// With [Death-Defying Leap]: 35ft maximum
    /// </para>
    /// </remarks>
    DeathDefyingLeap = 1,

    /// <summary>
    /// Run vertically up walls as a combat action.
    /// Type: Active (1 Action)
    /// </summary>
    /// <remarks>
    /// <para>
    /// Effect: Spend 1 action to run vertically up a wall up to your
    /// movement speed. Requires Acrobatics check DC 3 (success-counting).
    /// </para>
    /// <para>
    /// Limitation: Must end movement on a surface that can support you,
    /// or fall to the ground at the end of the action.
    /// </para>
    /// </remarks>
    WallRun = 2,

    /// <summary>
    /// Correct a failed leap mid-air once per day.
    /// Type: Reactive (1/day)
    /// </summary>
    /// <remarks>
    /// <para>
    /// Effect: After failing a leap check but before falling, you may
    /// reroll the leap check with a +1d10 bonus.
    /// </para>
    /// <para>
    /// Uses: Once per day. Resets on long rest.
    /// If second roll fails: Fall proceeds as normal.
    /// </para>
    /// </remarks>
    DoubleJump = 3,

    /// <summary>
    /// Auto-succeed Crash Landing checks with DC ≤ 3.
    /// Type: Passive
    /// </summary>
    /// <remarks>
    /// <para>
    /// Effect: When making a Crash Landing check, if the DC is 3 or less
    /// (falls of ~10 feet or less), automatically succeed.
    /// </para>
    /// <para>
    /// Formula: CrashLandingDc = 2 + (Height / 10)
    /// DC ≤ 3 means Height ≤ 10ft.
    /// </para>
    /// </remarks>
    Featherfall = 4
}
