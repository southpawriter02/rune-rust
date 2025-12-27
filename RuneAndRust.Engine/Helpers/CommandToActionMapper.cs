namespace RuneAndRust.Engine.Helpers;

using RuneAndRust.Core.Enums;

/// <summary>
/// Maps command strings (from key bindings) to GameAction enum values.
/// Provides the translation layer between IInputConfigurationService and GameAction.
/// </summary>
/// <remarks>
/// See: SPEC-INPUT-001 for Input System design.
/// v0.3.23a: Initial implementation.
/// </remarks>
public static class CommandToActionMapper
{
    private static readonly Dictionary<string, GameAction> CommandMap = new(StringComparer.OrdinalIgnoreCase)
    {
        // Movement
        { "north", GameAction.MoveNorth },
        { "south", GameAction.MoveSouth },
        { "east", GameAction.MoveEast },
        { "west", GameAction.MoveWest },
        { "up", GameAction.MoveUp },
        { "down", GameAction.MoveDown },

        // Core Actions
        { "confirm", GameAction.Confirm },
        { "cancel", GameAction.Cancel },
        { "menu", GameAction.Menu },
        { "help", GameAction.Help },

        // Screen Navigation
        { "inventory", GameAction.Inventory },
        { "character", GameAction.Character },
        { "journal", GameAction.Journal },
        { "bench", GameAction.Crafting },

        // Gameplay
        { "interact", GameAction.Interact },
        { "look", GameAction.Look },
        { "search", GameAction.Search },
        { "wait", GameAction.Wait },

        // Combat
        { "attack", GameAction.Attack },
        { "light", GameAction.LightAttack },
        { "heavy", GameAction.HeavyAttack }
    };

    /// <summary>
    /// Attempts to map a command string to a GameAction.
    /// </summary>
    /// <param name="command">The command string (e.g., "north", "attack").</param>
    /// <param name="action">The resolved GameAction if successful.</param>
    /// <returns>True if mapping succeeded.</returns>
    public static bool TryMapCommand(string command, out GameAction action)
    {
        return CommandMap.TryGetValue(command, out action);
    }

    /// <summary>
    /// Gets all registered command-to-action mappings.
    /// </summary>
    public static IReadOnlyDictionary<string, GameAction> GetAllMappings() => CommandMap;
}
