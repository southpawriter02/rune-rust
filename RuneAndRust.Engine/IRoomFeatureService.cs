using RuneAndRust.Core;

namespace RuneAndRust.Engine;

/// <summary>
/// v0.44.3: Result of searching a room.
/// </summary>
public class SearchResult
{
    /// <summary>Whether any items were found.</summary>
    public bool FoundItems { get; set; }

    /// <summary>Items found during the search.</summary>
    public List<Equipment> Items { get; set; } = new();

    /// <summary>Currency found (Scrap).</summary>
    public int ScrapFound { get; set; }

    /// <summary>Environmental secrets or lore discovered.</summary>
    public List<string> Secrets { get; set; } = new();

    /// <summary>Whether the search triggered an encounter.</summary>
    public bool TriggeredEncounter { get; set; }

    /// <summary>The encounter triggered by searching (if any).</summary>
    public RandomEncounterResult? Encounter { get; set; }

    /// <summary>Narrative message about the search.</summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>Creates an empty search result.</summary>
    public static SearchResult Empty(string message = "Your search reveals nothing of interest.")
    {
        return new SearchResult
        {
            FoundItems = false,
            Message = message
        };
    }

    /// <summary>Creates a search result with loot.</summary>
    public static SearchResult WithLoot(List<Equipment> items, int scrap, List<string> secrets, string message)
    {
        return new SearchResult
        {
            FoundItems = items.Count > 0 || scrap > 0,
            Items = items,
            ScrapFound = scrap,
            Secrets = secrets,
            Message = message
        };
    }

    /// <summary>Creates a search result that triggered an encounter.</summary>
    public static SearchResult WithEncounterTriggered(RandomEncounterResult encounter, string message)
    {
        return new SearchResult
        {
            FoundItems = false,
            TriggeredEncounter = true,
            Encounter = encounter,
            Message = message
        };
    }
}

/// <summary>
/// v0.44.3: Result of resting in the field.
/// </summary>
public class RestResult
{
    /// <summary>HP restored from rest.</summary>
    public int HPRestored { get; set; }

    /// <summary>Stamina restored from rest.</summary>
    public int StaminaRestored { get; set; }

    /// <summary>Psychic Stress gained (Trauma cost of non-sanctuary rest).</summary>
    public int StressGained { get; set; }

    /// <summary>Whether the rest was interrupted by an encounter.</summary>
    public bool InterruptedByEncounter { get; set; }

    /// <summary>The interrupting encounter (if any).</summary>
    public RandomEncounterResult? InterruptingEncounter { get; set; }

    /// <summary>Narrative message about the rest.</summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>Creates a successful sanctuary rest result.</summary>
    public static RestResult SanctuaryRest(int hpRestored, int staminaRestored)
    {
        return new RestResult
        {
            HPRestored = hpRestored,
            StaminaRestored = staminaRestored,
            StressGained = 0,
            InterruptedByEncounter = false,
            Message = "You rest peacefully in the sanctuary. Your wounds heal and your strength returns."
        };
    }

    /// <summary>Creates a field rest result (partial recovery, stress cost).</summary>
    public static RestResult FieldRest(int hpRestored, int staminaRestored, int stressGained)
    {
        return new RestResult
        {
            HPRestored = hpRestored,
            StaminaRestored = staminaRestored,
            StressGained = stressGained,
            InterruptedByEncounter = false,
            Message = "You rest uneasily in the hostile environment. Partially restored, but the oppressive atmosphere weighs on your mind."
        };
    }

    /// <summary>Creates a rest result that was interrupted.</summary>
    public static RestResult Interrupted(RandomEncounterResult encounter)
    {
        return new RestResult
        {
            HPRestored = 0,
            StaminaRestored = 0,
            StressGained = 5, // Shock from being caught off-guard
            InterruptedByEncounter = true,
            InterruptingEncounter = encounter,
            Message = "Your rest is violently interrupted! Enemies have found your position!"
        };
    }
}

/// <summary>
/// v0.44.3: Service interface for room feature interactions during exploration.
/// Handles searching rooms, field rest, and feature interactions.
/// </summary>
public interface IRoomFeatureService
{
    /// <summary>
    /// Searches a room for loot, secrets, and potential dangers.
    /// </summary>
    /// <param name="room">The room to search.</param>
    /// <returns>Search result including items, secrets, or triggered encounters.</returns>
    Task<SearchResult> SearchRoomAsync(Room room);

    /// <summary>
    /// Performs a field rest in the current location.
    /// </summary>
    /// <param name="gameState">Current game state for context.</param>
    /// <returns>Rest result including recovery and potential interruptions.</returns>
    Task<RestResult> PerformFieldRestAsync(Core.GameState gameState);

    /// <summary>
    /// Attempts to solve a puzzle in the room.
    /// </summary>
    /// <param name="room">The room containing the puzzle.</param>
    /// <param name="player">The player attempting the puzzle.</param>
    /// <param name="attributeUsed">The attribute used (WITS, FINESSE, etc.).</param>
    /// <returns>True if puzzle was solved, false otherwise.</returns>
    Task<(bool Success, string Message, int? DamageTaken)> AttemptPuzzleAsync(Room room, PlayerCharacter player, string attributeUsed);

    /// <summary>
    /// Disables an environmental hazard in the room.
    /// </summary>
    /// <param name="room">The room containing the hazard.</param>
    /// <param name="player">The player attempting to disable the hazard.</param>
    /// <returns>True if hazard was disabled, false otherwise.</returns>
    Task<(bool Success, string Message)> DisableHazardAsync(Room room, PlayerCharacter player);

    /// <summary>
    /// Collects items from the ground in a room.
    /// </summary>
    /// <param name="room">The room to collect items from.</param>
    /// <param name="player">The player collecting items.</param>
    /// <returns>List of collected items.</returns>
    List<Equipment> CollectGroundItems(Room room, PlayerCharacter player);
}
