using RuneAndRust.Core.Models;
using RuneAndRust.Core.Models.Combat;

namespace RuneAndRust.Core.Interfaces;

/// <summary>
/// Service contract for generating contextual help tips (v0.3.9c).
/// Analyzes game state to provide relevant, prioritized gameplay advice.
/// </summary>
public interface IContextHelpService
{
    /// <summary>
    /// Analyzes the current game state and generates relevant help tips.
    /// Considers player status effects, resource levels, and environment.
    /// </summary>
    /// <param name="state">The current game state to analyze.</param>
    /// <returns>A list of help tips sorted by priority (highest first), limited to top results.</returns>
    List<HelpTip> Analyze(GameState state);

    /// <summary>
    /// Analyzes the current combat state and generates tactical tips.
    /// Considers enemy types, player status, and combat conditions.
    /// Enemy weakness tips are WITS-gated (WITS >= 3 or Analyzed status required).
    /// </summary>
    /// <param name="combatState">The current combat state to analyze.</param>
    /// <returns>A list of combat tips sorted by priority (highest first), limited to top 3.</returns>
    List<HelpTip> AnalyzeCombat(CombatState combatState);

    /// <summary>
    /// Gets the WITS threshold required to reveal enemy tactical tips.
    /// </summary>
    int WitsThreshold { get; }

    /// <summary>
    /// Gets the maximum number of tips to return from analysis.
    /// </summary>
    int MaxTips { get; }
}
