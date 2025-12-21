using RuneAndRust.Core.Models;

namespace RuneAndRust.Core.Interfaces;

/// <summary>
/// Defines the contract for rendering rest results to the player.
/// Displays resource consumption, recovery deltas, and ambush warnings.
/// </summary>
public interface IRestScreenRenderer
{
    /// <summary>
    /// Renders the result of a rest action to the player.
    /// Shows HP/Stamina/Stress recovery, supply consumption, and any warnings.
    /// </summary>
    /// <param name="result">The rest result to display.</param>
    void Render(RestResult result);

    /// <summary>
    /// Renders an ambush warning when rest is interrupted by enemies.
    /// Should display a dramatic alert before transitioning to combat.
    /// </summary>
    /// <param name="ambushResult">The ambush result with encounter details.</param>
    void RenderAmbushWarning(AmbushResult ambushResult);
}
