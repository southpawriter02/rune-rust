using Serilog;

namespace RuneAndRust.Engine;

public enum CommandType
{
    Unknown,
    Look,
    Move,
    Stats,
    Inventory,
    Help,
    Quit,
    Attack,
    Defend,
    Ability,
    Flee,
    Solve,
    Legend,
    Saga,
    Milestone,
    Spend,
    PP,
    Save,
    Load,
    // Equipment commands (v0.3)
    Equip,
    Unequip,
    Pickup,
    Drop,
    Compare,
    // v0.4 - NPC interaction
    Talk,
    // v0.5 - Trauma Economy
    Rest,
    // v0.8 - NPC & Quest System
    Quests,
    Quest,
    Reputation,
    // v0.9 - Merchant System
    Shop,
    Buy,
    Sell,
    // v0.13 - Persistent World State / Destruction
    Destroy,
    History
}

public class ParsedCommand
{
    public CommandType Type { get; set; }
    public string RawInput { get; set; } = string.Empty;
    public string Direction { get; set; } = string.Empty;
    public string Target { get; set; } = string.Empty;
    public string AbilityName { get; set; } = string.Empty;
    public List<string> Arguments { get; set; } = new();
}

public class CommandParser
{
    private static readonly ILogger _log = Log.ForContext<CommandParser>();

    private static readonly Dictionary<string, CommandType> CommandAliases = new()
    {
        // Look
        { "look", CommandType.Look },
        { "l", CommandType.Look },
        { "examine", CommandType.Look },

        // Movement
        { "north", CommandType.Move },
        { "n", CommandType.Move },
        { "south", CommandType.Move },
        { "s", CommandType.Move },
        { "east", CommandType.Move },
        { "e", CommandType.Move },
        { "west", CommandType.Move },
        { "w", CommandType.Move },
        { "go", CommandType.Move },
        { "move", CommandType.Move },

        // Character info
        { "stats", CommandType.Stats },
        { "status", CommandType.Stats },
        { "character", CommandType.Stats },
        { "sheet", CommandType.Stats },
        { "legend", CommandType.Legend },
        { "saga", CommandType.Saga },
        { "milestone", CommandType.Milestone },
        { "ms", CommandType.Milestone },
        { "spend", CommandType.Spend },
        { "pp", CommandType.PP },
        { "progression", CommandType.PP },

        // Inventory
        { "inventory", CommandType.Inventory },
        { "inv", CommandType.Inventory },
        { "i", CommandType.Inventory },

        // Equipment (v0.3)
        { "equip", CommandType.Equip },
        { "wear", CommandType.Equip },
        { "wield", CommandType.Equip },
        { "unequip", CommandType.Unequip },
        { "remove", CommandType.Unequip },
        { "pickup", CommandType.Pickup },
        { "take", CommandType.Pickup },
        { "get", CommandType.Pickup },
        { "drop", CommandType.Drop },
        { "compare", CommandType.Compare },
        { "comp", CommandType.Compare },

        // Combat
        { "attack", CommandType.Attack },
        { "a", CommandType.Attack },
        { "hit", CommandType.Attack },
        { "defend", CommandType.Defend },
        { "d", CommandType.Defend },
        { "block", CommandType.Defend },
        { "ability", CommandType.Ability },
        { "ab", CommandType.Ability },
        { "skill", CommandType.Ability },
        { "flee", CommandType.Flee },
        { "run", CommandType.Flee },
        { "escape", CommandType.Flee },

        // Puzzle
        { "solve", CommandType.Solve },
        { "analyze", CommandType.Solve },
        { "puzzle", CommandType.Solve },

        // NPC Interaction (v0.4)
        { "talk", CommandType.Talk },
        { "speak", CommandType.Talk },
        { "negotiate", CommandType.Talk },
        { "convince", CommandType.Talk },

        // Trauma Economy (v0.5)
        { "rest", CommandType.Rest },
        { "sleep", CommandType.Rest },
        { "recover", CommandType.Rest },

        // NPC & Quest System (v0.8)
        { "quests", CommandType.Quests },
        { "questlog", CommandType.Quests },
        { "quest", CommandType.Quest },
        { "reputation", CommandType.Reputation },
        { "rep", CommandType.Reputation },
        { "faction", CommandType.Reputation },
        { "factions", CommandType.Reputation },

        // Merchant System (v0.9)
        { "shop", CommandType.Shop },
        { "browse", CommandType.Shop },
        { "buy", CommandType.Buy },
        { "purchase", CommandType.Buy },
        { "sell", CommandType.Sell },
        { "trade", CommandType.Sell },

        // Destruction (v0.13)
        { "destroy", CommandType.Destroy },
        { "smash", CommandType.Destroy },
        { "break", CommandType.Destroy },
        { "clear", CommandType.Destroy },
        { "demolish", CommandType.Destroy },
        { "history", CommandType.History },
        { "changes", CommandType.History },
        { "modifications", CommandType.History },

        // Utility
        { "help", CommandType.Help },
        { "h", CommandType.Help },
        { "?", CommandType.Help },
        { "quit", CommandType.Quit },
        { "exit", CommandType.Quit },
        { "q", CommandType.Quit },
        { "save", CommandType.Save },
        { "load", CommandType.Load }
    };

    private static readonly HashSet<string> DirectionWords = new()
    {
        "north", "n", "south", "s", "east", "e", "west", "w"
    };

    public ParsedCommand Parse(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            _log.Debug("Empty command received");
            return new ParsedCommand { Type = CommandType.Unknown, RawInput = input };
        }

        var trimmed = input.Trim().ToLower();
        var parts = trimmed.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length == 0)
        {
            _log.Debug("Command parsed to zero parts");
            return new ParsedCommand { Type = CommandType.Unknown, RawInput = input };
        }

        var firstWord = parts[0];
        var command = new ParsedCommand
        {
            RawInput = input,
            Arguments = parts.Skip(1).ToList()
        };

        // Check if first word is a known command
        if (CommandAliases.TryGetValue(firstWord, out var commandType))
        {
            command.Type = commandType;
            _log.Information("Player command: {Command} -> Type: {CommandType}", input, commandType);

            // Handle direction-specific logic
            if (DirectionWords.Contains(firstWord))
            {
                command.Type = CommandType.Move;
                command.Direction = firstWord switch
                {
                    "n" => "north",
                    "s" => "south",
                    "e" => "east",
                    "w" => "west",
                    _ => firstWord
                };
            }
            // Handle "go north" or "move west"
            else if (commandType == CommandType.Move && parts.Length > 1)
            {
                var direction = parts[1];
                command.Direction = direction switch
                {
                    "n" => "north",
                    "s" => "south",
                    "e" => "east",
                    "w" => "west",
                    _ => direction
                };
            }
            // Handle "attack enemy" or "attack 1"
            else if (commandType == CommandType.Attack && parts.Length > 1)
            {
                command.Target = string.Join(" ", parts.Skip(1));
            }
            // Handle "ability power strike" or "ab 1"
            else if (commandType == CommandType.Ability && parts.Length > 1)
            {
                command.AbilityName = string.Join(" ", parts.Skip(1));
            }
            // Handle "equip [item name]"
            else if (commandType == CommandType.Equip && parts.Length > 1)
            {
                command.Target = string.Join(" ", parts.Skip(1));
            }
            // Handle "unequip [weapon|armor]"
            else if (commandType == CommandType.Unequip && parts.Length > 1)
            {
                command.Target = string.Join(" ", parts.Skip(1));
            }
            // Handle "pickup [item name]" or "take [item name]"
            else if (commandType == CommandType.Pickup && parts.Length > 1)
            {
                command.Target = string.Join(" ", parts.Skip(1));
            }
            // Handle "drop [item name]"
            else if (commandType == CommandType.Drop && parts.Length > 1)
            {
                command.Target = string.Join(" ", parts.Skip(1));
            }
            // Handle "compare [item name]"
            else if (commandType == CommandType.Compare && parts.Length > 1)
            {
                command.Target = string.Join(" ", parts.Skip(1));
            }
            // Handle "destroy [target]" or "smash [target]" (v0.13)
            else if (commandType == CommandType.Destroy && parts.Length > 1)
            {
                command.Target = string.Join(" ", parts.Skip(1));
            }
        }
        else
        {
            command.Type = CommandType.Unknown;
            _log.Warning("Unknown command: {Command}", input);
        }

        return command;
    }

    public static string GetHelpText()
    {
        return @"
EXPLORATION COMMANDS:
  look, l              - Examine your surroundings
  north, n             - Move north
  south, s             - Move south
  east, e              - Move east
  west, w              - Move west
  stats, status        - View your character stats
  legend, saga         - View Legend and Saga progress
  milestone, ms        - View milestone information
  spend, pp            - Spend Progression Points
  inventory, inv, i    - View your inventory

EQUIPMENT COMMANDS (v0.3):
  equip [item]         - Equip weapon or armor from inventory
  unequip [slot]       - Unequip weapon or armor to inventory
  pickup [item]        - Pick up item from ground
  drop [item]          - Drop item from inventory to ground
  compare [item]       - Compare item to currently equipped

COMBAT COMMANDS:
  attack [target], a   - Attack an enemy
  defend, d            - Take a defensive stance
  ability [name], ab   - Use a special ability
  flee, run            - Attempt to flee from combat

PUZZLE COMMANDS:
  solve, analyze       - Attempt to solve a puzzle

NPC INTERACTION (v0.4, v0.8):
  talk, speak          - Talk to NPCs and access dialogue
  quests, questlog     - View all active and completed quests
  quest [quest name]   - View details of a specific quest
  reputation, rep      - View faction reputation standings

TRAUMA ECONOMY (v0.5):
  rest, sleep          - Rest at Sanctuary to recover Stress (Corruption persists)

MERCHANT SYSTEM (v0.9):
  shop, browse         - View merchant's inventory with prices
  buy [item name]      - Purchase an item from a merchant
  sell [item name]     - Sell equipment or materials to a merchant
  sell                 - View all items you can sell with prices

WORLD INTERACTION (v0.13):
  destroy [target]     - Destroy terrain or hazards (smash, break, clear)
  history              - View modifications to the current room

OTHER COMMANDS:
  help, h, ?           - Show this help text
  save                 - Save your current game
  quit, exit, q        - Exit the game
";
    }
}
