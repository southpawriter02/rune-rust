namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Represents the sources of Coherence generation for the Arcanist specialization.
/// </summary>
/// <remarks>
/// <para>
/// Each source determines how coherence is gained and has different mechanics.
/// Coherence generation is tied to successful spellcasting and magical control.
/// </para>
/// <para>
/// Generation Mechanics:
/// <list type="bullet">
/// <item>SuccessfulCast: +5 coherence per successful spell completion</item>
/// <item>ControlledChannel: +3 coherence per turn of maintained spell</item>
/// <item>MeditationAction: +20 coherence per meditation (non-combat only)</item>
/// <item>StabilityField: +1 to +10 coherence from environmental/ally effects</item>
/// </list>
/// </para>
/// </remarks>
public enum CoherenceSource
{
    /// <summary>
    /// Coherence from a successfully completed spell.
    /// </summary>
    /// <remarks>
    /// Flat: 5 coherence per successful cast.
    /// Encourages active casting to build towards Apotheosis.
    /// </remarks>
    SuccessfulCast = 0,

    /// <summary>
    /// Coherence from maintaining controlled channeled spells.
    /// </summary>
    /// <remarks>
    /// Formula: 3 coherence per turn of maintained spell.
    /// Encourages concentration and sustained magic over quick bursts.
    /// </remarks>
    ControlledChannel = 1,

    /// <summary>
    /// Coherence from meditation (non-combat recovery).
    /// </summary>
    /// <remarks>
    /// Flat: 20 coherence per meditation action.
    /// Only available outside combat. Primary method for restoring stability.
    /// Interrupted by damage.
    /// </remarks>
    MeditationAction = 2,

    /// <summary>
    /// Coherence bonus from environmental or ally effects.
    /// </summary>
    /// <remarks>
    /// Variable based on effect; 1-10 coherence.
    /// External stability sources such as ley line convergences,
    /// ally support abilities, or magical sanctuaries.
    /// </remarks>
    StabilityField = 3
}
