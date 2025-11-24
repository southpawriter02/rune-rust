using System.Collections.Generic;

namespace RuneAndRust.Core.AI;

/// <summary>
/// Snapshot of the battlefield state for AI decision-making.
/// Contains all combatants, grid state, and metadata.
/// v0.42.1: Tactical Decision-Making & Target Selection
/// </summary>
public class BattlefieldState
{
    /// <summary>
    /// All player characters in the battle (dead or alive).
    /// </summary>
    public List<PlayerCharacter> PlayerCharacters { get; set; } = new();

    /// <summary>
    /// All enemies in the battle (dead or alive).
    /// </summary>
    public List<Enemy> Enemies { get; set; } = new();

    /// <summary>
    /// The tactical grid (if grid combat is active).
    /// </summary>
    public BattlefieldGrid? Grid { get; set; }

    /// <summary>
    /// Current turn number in the encounter.
    /// </summary>
    public int CurrentTurn { get; set; }

    /// <summary>
    /// Active environmental conditions (weather, hazards, etc.).
    /// </summary>
    public List<string> ActiveConditions { get; set; } = new();

    /// <summary>
    /// Arbitrary metadata for extensibility.
    /// Can store boss phase info, special mechanics, etc.
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();

    /// <summary>
    /// Unique identifier for this combat encounter.
    /// </summary>
    public Guid EncounterId { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Unique identifier for the game session.
    /// </summary>
    public Guid SessionId { get; set; }
}
