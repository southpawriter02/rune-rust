namespace RuneAndRust.Engine.Helpers;

using RuneAndRust.Core.Enums;

/// <summary>
/// Maps GameAction enum values to command strings for CommandParser.
/// Provides the reverse translation for v0.3.23b until full CommandParser modernization.
/// </summary>
/// <remarks>
/// See: SPEC-INPUT-001 for Input System design.
/// v0.3.23b: Bridge between IInputService and CommandParser.
/// </remarks>
public static class ActionToCommandMapper
{
    private static readonly Dictionary<GameAction, string> ActionMap = new()
    {
        // Movement
        { GameAction.MoveNorth, "north" },
        { GameAction.MoveSouth, "south" },
        { GameAction.MoveEast, "east" },
        { GameAction.MoveWest, "west" },
        { GameAction.MoveUp, "up" },
        { GameAction.MoveDown, "down" },

        // Core Actions
        { GameAction.Confirm, "confirm" },
        { GameAction.Cancel, "cancel" },
        { GameAction.Menu, "menu" },
        { GameAction.Help, "help" },

        // Screen Navigation
        { GameAction.Inventory, "pack" },
        { GameAction.Character, "status" },
        { GameAction.Journal, "archive" },
        { GameAction.Crafting, "bench" },

        // Gameplay
        { GameAction.Interact, "interact" },
        { GameAction.Look, "look" },
        { GameAction.Search, "search" },
        { GameAction.Wait, "wait" },

        // Combat
        { GameAction.Attack, "attack" },
        { GameAction.LightAttack, "light" },
        { GameAction.HeavyAttack, "heavy" },
        { GameAction.UseAbility, "use" },
        { GameAction.Flee, "flee" }
    };

    /// <summary>
    /// Converts a GameAction to a command string for CommandParser.
    /// </summary>
    /// <param name="action">The game action.</param>
    /// <returns>The command string, or null if not mapped.</returns>
    public static string? ToCommand(GameAction action)
    {
        return ActionMap.TryGetValue(action, out var command) ? command : null;
    }

    /// <summary>
    /// Gets all registered action-to-command mappings.
    /// </summary>
    public static IReadOnlyDictionary<GameAction, string> GetAllMappings() => ActionMap;
}
