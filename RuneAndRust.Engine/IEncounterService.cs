using RuneAndRust.Core;

namespace RuneAndRust.Engine;

/// <summary>
/// v0.44.3: Result of a random encounter roll during exploration.
/// </summary>
public class RandomEncounterResult
{
    /// <summary>Whether an encounter was triggered.</summary>
    public bool EncounterTriggered { get; set; }

    /// <summary>The encounter enemies if triggered.</summary>
    public List<Enemy> Enemies { get; set; } = new();

    /// <summary>Difficulty rating of the encounter (1-10).</summary>
    public int DifficultyRating { get; set; }

    /// <summary>Narrative description of the encounter.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Whether the encounter can be fled from.</summary>
    public bool CanFlee { get; set; } = true;

    /// <summary>Creates a result where no encounter occurred.</summary>
    public static RandomEncounterResult NoEncounter() => new() { EncounterTriggered = false };

    /// <summary>Creates a result with an encounter.</summary>
    public static RandomEncounterResult WithEncounter(List<Enemy> enemies, int difficulty, string description, bool canFlee = true)
    {
        return new RandomEncounterResult
        {
            EncounterTriggered = true,
            Enemies = enemies,
            DifficultyRating = difficulty,
            Description = description,
            CanFlee = canFlee
        };
    }
}

/// <summary>
/// v0.44.3: Service interface for encounter generation during exploration.
/// Unifies random encounters, room encounters, and faction encounters.
/// </summary>
public interface IEncounterService
{
    /// <summary>
    /// Rolls for a random encounter based on current game state.
    /// </summary>
    /// <param name="gameState">Current game state including player and location.</param>
    /// <returns>Encounter result indicating if an encounter occurred.</returns>
    Task<RandomEncounterResult> RollForRandomEncounterAsync(GameState gameState);

    /// <summary>
    /// Generates an encounter appropriate for the given room.
    /// Used for forced encounters in specific rooms.
    /// </summary>
    /// <param name="room">The room to generate an encounter for.</param>
    /// <param name="playerLevel">Player level for scaling.</param>
    /// <returns>List of enemies for the encounter.</returns>
    List<Enemy> GenerateRoomEncounter(Room room, int playerLevel);

    /// <summary>
    /// Generates enemies that might appear during a search action.
    /// </summary>
    /// <param name="room">The room being searched.</param>
    /// <param name="playerLevel">Player level for scaling.</param>
    /// <returns>Encounter result, may be empty.</returns>
    RandomEncounterResult GenerateSearchEncounter(Room room, int playerLevel);

    /// <summary>
    /// Generates enemies that might interrupt a rest action.
    /// </summary>
    /// <param name="room">The room where player is resting.</param>
    /// <param name="isSanctuary">Whether the room is a sanctuary (no interrupts).</param>
    /// <param name="playerLevel">Player level for scaling.</param>
    /// <returns>Encounter result, may be empty.</returns>
    RandomEncounterResult GenerateRestInterruptEncounter(Room room, bool isSanctuary, int playerLevel);
}
