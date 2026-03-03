namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Describes how a reputation-affecting action was observed by faction members.
/// </summary>
/// <remarks>
/// <para>The witness system scales reputation changes based on observation context:</para>
/// <list type="bullet">
///   <item><description>Direct: Full (100%) reputation change — player interacted directly with the faction.</description></item>
///   <item><description>Witnessed: Partial (75%) reputation change — a faction member observed the action.</description></item>
///   <item><description>Unwitnessed: No (0%) reputation change — no faction members were present.</description></item>
/// </list>
/// <para>Phase 1 implementation defaults all actions to <see cref="Direct"/>. Phase 2 will
/// integrate with NPC proximity/line-of-sight for dynamic witness detection.</para>
/// </remarks>
public enum WitnessType
{
    /// <summary>
    /// Player directly interacted with the faction. 100% reputation change applied.
    /// </summary>
    Direct,

    /// <summary>
    /// A faction member witnessed the action. 75% reputation change applied.
    /// </summary>
    Witnessed,

    /// <summary>
    /// No faction members were present. 0% reputation change (action goes unnoticed).
    /// </summary>
    Unwitnessed
}
