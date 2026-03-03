using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Context describing how a reputation-affecting action was observed by faction members.
/// </summary>
/// <remarks>
/// <para>Immutable value object that pairs a <see cref="WitnessType"/> with its corresponding
/// reputation multiplier. Used by <c>IReputationService.ApplyReputationChanges()</c> to scale
/// reputation deltas based on whether faction members observed the action.</para>
///
/// <para><b>Phase 1:</b> All actions default to <see cref="Direct"/> (100% change).
/// <b>Phase 2:</b> Will integrate with NPC proximity/line-of-sight for
/// <see cref="Witnessed"/> (75%) and <see cref="Unwitnessed"/> (0%) detection.</para>
///
/// <para>Multiplier values per design doc (v1.2, Section 4.2):</para>
/// <list type="bullet">
///   <item><description>Direct action: 1.0 (full reputation change)</description></item>
///   <item><description>Witnessed action: 0.75 (75% reputation change)</description></item>
///   <item><description>Unwitnessed: 0.0 (no change)</description></item>
/// </list>
/// </remarks>
public readonly record struct WitnessContext
{
    /// <summary>
    /// Gets the type of witness observation for this context.
    /// </summary>
    public WitnessType Type { get; }

    /// <summary>
    /// Gets the reputation multiplier for this witness type.
    /// Applied to the raw reputation delta before clamping.
    /// </summary>
    /// <value>
    /// A value between 0.0 and 1.0 inclusive:
    /// Direct = 1.0, Witnessed = 0.75, Unwitnessed = 0.0.
    /// </value>
    public double Multiplier { get; }

    /// <summary>
    /// Creates a new WitnessContext with the specified type and multiplier.
    /// </summary>
    /// <param name="type">The witness observation type.</param>
    /// <param name="multiplier">The reputation multiplier (0.0 to 1.0).</param>
    private WitnessContext(WitnessType type, double multiplier)
    {
        Type = type;
        Multiplier = Math.Clamp(multiplier, 0.0, 1.0);
    }

    /// <summary>
    /// Creates a Direct witness context (100% reputation change).
    /// Use when the player interacts directly with a faction member.
    /// </summary>
    public static WitnessContext Direct => new(WitnessType.Direct, 1.0);

    /// <summary>
    /// Creates a Witnessed context (75% reputation change).
    /// Use when a faction member observes the player's action.
    /// </summary>
    public static WitnessContext Witnessed => new(WitnessType.Witnessed, 0.75);

    /// <summary>
    /// Creates an Unwitnessed context (0% reputation change).
    /// Use when no faction members are present to observe the action.
    /// </summary>
    public static WitnessContext Unwitnessed => new(WitnessType.Unwitnessed, 0.0);
}
