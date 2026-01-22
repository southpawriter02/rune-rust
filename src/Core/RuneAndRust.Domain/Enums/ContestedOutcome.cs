namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Represents the outcome of a contested check between two parties.
/// </summary>
/// <remarks>
/// <para>
/// Contested checks compare the net successes of two opposing skill checks.
/// Fumbles take priority over net success comparison.
/// </para>
/// <para>
/// Resolution order:
/// <list type="bullet">
///   <item><description>Both fumble → BothFumble (mutual failure)</description></item>
///   <item><description>Initiator fumbles → InitiatorFumble (defender auto-wins)</description></item>
///   <item><description>Defender fumbles → DefenderFumble (initiator auto-wins)</description></item>
///   <item><description>Compare net successes → InitiatorWins, DefenderWins, or Tie</description></item>
/// </list>
/// </para>
/// </remarks>
public enum ContestedOutcome
{
    /// <summary>
    /// Initiator wins the contested check by having more net successes.
    /// </summary>
    /// <remarks>
    /// The initiator's net successes exceed the defender's net successes.
    /// The margin indicates the difference in net successes.
    /// </remarks>
    InitiatorWins = 0,

    /// <summary>
    /// Defender wins the contested check by having more net successes.
    /// </summary>
    /// <remarks>
    /// The defender's net successes exceed the initiator's net successes.
    /// The margin indicates the difference in net successes.
    /// </remarks>
    DefenderWins = 1,

    /// <summary>
    /// Both parties have equal net successes - initiator gets minor advantage.
    /// </summary>
    /// <remarks>
    /// <para>
    /// When net successes are equal, the initiator (active party) receives
    /// a minor advantage. This represents the momentum of being the aggressor.
    /// </para>
    /// <para>
    /// The minor advantage is context-dependent:
    /// <list type="bullet">
    ///   <item><description>Grapple: Initiator gains grip but not control</description></item>
    ///   <item><description>Chase: Neither gains/loses distance</description></item>
    ///   <item><description>Social: Conversation continues, slight edge to initiator</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    Tie = 2,

    /// <summary>
    /// Both parties fumbled - mutual failure with consequences for both.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Both rolls resulted in fumbles (0 successes AND ≥1 botch).
    /// Both parties suffer fumble consequences.
    /// </para>
    /// <para>
    /// Examples:
    /// <list type="bullet">
    ///   <item><description>Grapple: Both fall prone</description></item>
    ///   <item><description>Chase: Both stumble into hazard</description></item>
    ///   <item><description>Social: Both embarrass themselves</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    BothFumble = 3,

    /// <summary>
    /// Initiator fumbled - defender automatically wins.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The initiator's roll was a fumble (0 successes AND ≥1 botch).
    /// The defender wins regardless of their roll (unless also fumbling).
    /// </para>
    /// <para>
    /// The initiator may suffer additional fumble consequences beyond losing.
    /// The margin is the defender's net successes.
    /// </para>
    /// </remarks>
    InitiatorFumble = 4,

    /// <summary>
    /// Defender fumbled - initiator automatically wins.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The defender's roll was a fumble (0 successes AND ≥1 botch).
    /// The initiator wins regardless of their roll (unless also fumbling).
    /// </para>
    /// <para>
    /// The defender may suffer additional fumble consequences beyond losing.
    /// The margin is the initiator's net successes.
    /// </para>
    /// </remarks>
    DefenderFumble = 5
}
