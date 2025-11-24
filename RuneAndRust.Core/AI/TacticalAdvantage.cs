namespace RuneAndRust.Core.AI;

/// <summary>
/// Represents the AI's assessment of its tactical advantage in the current battle.
/// v0.42.1: Tactical Decision-Making & Target Selection
/// </summary>
public enum TacticalAdvantage
{
    /// <summary>
    /// Strong advantage: Winning the battle, outnumbering enemies, good positioning.
    /// AI should be aggressive and press the advantage.
    /// </summary>
    Strong,

    /// <summary>
    /// Slight advantage: Marginally ahead, could tip either way.
    /// AI should maintain pressure but be cautious.
    /// </summary>
    Slight,

    /// <summary>
    /// Neutral: Even match, no clear advantage.
    /// AI should play tactically and look for opportunities.
    /// </summary>
    Neutral,

    /// <summary>
    /// Disadvantaged: Losing the battle, outnumbered, poor positioning.
    /// AI should consider retreating, defensive play, or desperate tactics.
    /// </summary>
    Disadvantaged
}
