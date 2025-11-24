using RuneAndRust.Core;
using RuneAndRust.Core.Spatial;
using Serilog;

namespace RuneAndRust.Engine.Spatial;

/// <summary>
/// v0.39.1: Handles vertical traversal mechanics including skill checks and fall damage
/// </summary>
public interface IVerticalTraversalService
{
    VerticalConnection? GetConnectionBetween(string fromRoomId, string toRoomId, List<VerticalConnection> connections);
    bool CanTraverse(Character character, VerticalConnection connection);
    TraversalResult AttemptTraversal(Character character, VerticalConnection connection, string direction);
    List<VerticalLayer> GetReachableLayers(string startRoomId, Dictionary<string, RoomPosition> positions, List<VerticalConnection> connections);
    TraversalResult AttemptClearBlockage(Character character, VerticalConnection connection);
}

public class VerticalTraversalService : IVerticalTraversalService
{
    private readonly ILogger _logger;
    private readonly IDiceRoller? _diceRoller;

    public VerticalTraversalService(ILogger logger, IDiceRoller? diceRoller = null)
    {
        _logger = logger;
        _diceRoller = diceRoller;
    }

    /// <summary>
    /// Gets the vertical connection between two rooms, if one exists
    /// </summary>
    public VerticalConnection? GetConnectionBetween(string fromRoomId, string toRoomId, List<VerticalConnection> connections)
    {
        return connections.FirstOrDefault(c =>
            (c.FromRoomId == fromRoomId && c.ToRoomId == toRoomId) ||
            (c.IsBidirectional && c.FromRoomId == toRoomId && c.ToRoomId == fromRoomId));
    }

    /// <summary>
    /// Checks if a character can currently traverse a vertical connection
    /// </summary>
    public bool CanTraverse(Character character, VerticalConnection connection)
    {
        // Blocked connections cannot be traversed
        if (connection.IsBlocked)
        {
            _logger.Debug("Cannot traverse: Connection {ConnectionId} is blocked", connection.ConnectionId);
            return false;
        }

        // Collapsed connections cannot be traversed
        if (connection.Type == VerticalConnectionType.Collapsed)
        {
            _logger.Debug("Cannot traverse: Connection {ConnectionId} is collapsed", connection.ConnectionId);
            return false;
        }

        // Stairs are always traversable
        if (connection.Type == VerticalConnectionType.Stairs)
        {
            return true;
        }

        // Unpowered elevators require alternative traversal
        if (connection.Type == VerticalConnectionType.Elevator && connection.IsPowered == false)
        {
            _logger.Debug("Elevator {ConnectionId} is unpowered, requires manual climbing", connection.ConnectionId);
            // Can still attempt to climb shaft, so return true
            return true;
        }

        // Other connection types require skill checks but are traversable
        return true;
    }

    /// <summary>
    /// Attempts to traverse a vertical connection
    /// Returns result with success/failure and any damage taken
    /// </summary>
    public TraversalResult AttemptTraversal(Character character, VerticalConnection connection, string direction)
    {
        _logger.Information(
            "Character {CharName} attempting {ConnectionType} traversal {Direction}, DC={DC}",
            character.Name,
            connection.Type,
            direction,
            connection.TraversalDC);

        // Blocked: cannot traverse
        if (connection.IsBlocked)
        {
            return TraversalResult.Failure(
                $"The passage is blocked. {connection.BlockageDescription ?? "You cannot proceed."}",
                0);
        }

        // Stairs: automatic success
        if (connection.Type == VerticalConnectionType.Stairs)
        {
            _logger.Debug("Stairs traversal automatic success");
            return TraversalResult.CreateSuccess(
                connection.GetSuccessDescription(direction),
                connection.LevelsSpanned);
        }

        // Elevator: check if powered
        if (connection.Type == VerticalConnectionType.Elevator)
        {
            if (connection.IsPowered == true)
            {
                _logger.Debug("Powered elevator traversal automatic success");
                return TraversalResult.CreateSuccess(
                    "The elevator groans and shudders as it carries you " + direction + ".",
                    connection.LevelsSpanned);
            }
            else
            {
                _logger.Warning("Elevator unpowered, character must climb manually");
                return TraversalResult.Failure(
                    "The elevator has no power. You'll need to climb the shaft manually (treat as Shaft) or repair the elevator (WITS DC 15).",
                    0);
            }
        }

        // Shaft/Ladder: Athletics check required
        if (connection.Type == VerticalConnectionType.Shaft ||
            connection.Type == VerticalConnectionType.Ladder)
        {
            var checkResult = ResolveAthleticsCheck(character, connection.TraversalDC);

            if (checkResult.Success)
            {
                _logger.Information(
                    "Traversal successful: {CharName} climbed {ConnectionType} (rolled {Roll} vs DC {DC})",
                    character.Name,
                    connection.Type,
                    checkResult.TotalRoll,
                    connection.TraversalDC);

                return TraversalResult.CreateSuccess(
                    connection.GetSuccessDescription(direction),
                    connection.LevelsSpanned);
            }
            else
            {
                var damage = CalculateFallDamage(connection, checkResult.Margin);

                _logger.Warning(
                    "Traversal failed: {CharName} fell from {ConnectionType}, Damage={Damage} (rolled {Roll} vs DC {DC})",
                    character.Name,
                    connection.Type,
                    damage,
                    checkResult.TotalRoll,
                    connection.TraversalDC);

                return TraversalResult.Failure(
                    connection.GetFailureDescription(damage),
                    damage);
            }
        }

        // Collapsed: should have been caught earlier
        if (connection.Type == VerticalConnectionType.Collapsed)
        {
            return TraversalResult.Failure(
                $"The passage is completely blocked. {connection.BlockageDescription}",
                0);
        }

        return TraversalResult.Failure("Unknown connection type.", 0);
    }

    /// <summary>
    /// Attempts to clear a blocked connection
    /// </summary>
    public TraversalResult AttemptClearBlockage(Character character, VerticalConnection connection)
    {
        if (!connection.IsBlocked && connection.Type != VerticalConnectionType.Collapsed)
        {
            return TraversalResult.Failure("This passage is not blocked.", 0);
        }

        var clearanceDC = connection.ClearanceDC ?? 15;

        _logger.Information(
            "Character {CharName} attempting to clear blockage, DC={DC}",
            character.Name,
            clearanceDC);

        var checkResult = ResolveAthleticsCheck(character, clearanceDC);

        if (checkResult.Success)
        {
            connection.IsBlocked = false;
            connection.BlockageDescription = "The passage has been cleared of debris.";

            _logger.Information(
                "Blockage cleared successfully: {CharName} cleared {ConnectionId}",
                character.Name,
                connection.ConnectionId);

            return TraversalResult.CreateSuccess(
                $"After {connection.ClearanceTimeMinutes ?? 10} minutes of effort, you clear the debris. The passage is now open.",
                0);
        }
        else
        {
            var damage = Math.Abs(checkResult.Margin); // Minor strain damage

            _logger.Warning(
                "Blockage clearing failed: {CharName} took {Damage} damage from exertion",
                character.Name,
                damage);

            return TraversalResult.Failure(
                $"Despite your efforts, the debris won't budge. You strain yourself, taking {damage} Physical damage from overexertion.",
                damage);
        }
    }

    /// <summary>
    /// Gets all vertical layers reachable from a starting room
    /// </summary>
    public List<VerticalLayer> GetReachableLayers(
        string startRoomId,
        Dictionary<string, RoomPosition> positions,
        List<VerticalConnection> connections)
    {
        var reachableLayers = new HashSet<VerticalLayer>();
        var visitedRooms = new HashSet<string>();
        var queue = new Queue<string>();

        // Get starting position
        if (!positions.TryGetValue(startRoomId, out var startPosition))
        {
            _logger.Warning("Cannot determine reachable layers: Room {RoomId} not found", startRoomId);
            return new List<VerticalLayer>();
        }

        queue.Enqueue(startRoomId);
        visitedRooms.Add(startRoomId);
        reachableLayers.Add(VerticalLayerExtensions.FromZCoordinate(startPosition.Z));

        // BFS to find all reachable rooms
        while (queue.Count > 0)
        {
            var currentRoomId = queue.Dequeue();

            // Find all vertical connections from this room
            var connectionsFrom = connections
                .Where(c => !c.IsBlocked &&
                            ((c.FromRoomId == currentRoomId) ||
                             (c.IsBidirectional && c.ToRoomId == currentRoomId)))
                .ToList();

            foreach (var connection in connectionsFrom)
            {
                var nextRoomId = connection.FromRoomId == currentRoomId
                    ? connection.ToRoomId
                    : connection.FromRoomId;

                if (!visitedRooms.Contains(nextRoomId))
                {
                    visitedRooms.Add(nextRoomId);
                    queue.Enqueue(nextRoomId);

                    // Add this room's layer to reachable set
                    if (positions.TryGetValue(nextRoomId, out var position))
                    {
                        reachableLayers.Add(VerticalLayerExtensions.FromZCoordinate(position.Z));
                    }
                }
            }
        }

        _logger.Debug("Reachable layers from {RoomId}: {Layers}",
            startRoomId,
            string.Join(", ", reachableLayers));

        return reachableLayers.OrderBy(l => (int)l).ToList();
    }

    #region Skill Check Resolution

    /// <summary>
    /// Resolves an Athletics check for climbing/traversal
    /// </summary>
    private CheckResult ResolveAthleticsCheck(Character character, int dc)
    {
        // Roll d20 + MIGHT attribute
        var d20Roll = _diceRoller?.Roll(1, 20) ?? RollD20();
        var mightBonus = character.Attributes.MIGHT;
        var totalRoll = d20Roll + mightBonus;

        var success = totalRoll >= dc;
        var margin = totalRoll - dc;

        _logger.Debug(
            "Athletics check: d20={D20} + MIGHT={Might} = {Total} vs DC {DC} → {Result}",
            d20Roll,
            mightBonus,
            totalRoll,
            dc,
            success ? "SUCCESS" : "FAILURE");

        return new CheckResult
        {
            Success = success,
            TotalRoll = totalRoll,
            Margin = margin,
            D20Roll = d20Roll
        };
    }

    /// <summary>
    /// Fallback d20 roller if no dice service provided
    /// </summary>
    private int RollD20()
    {
        return new Random().Next(1, 21);
    }

    #endregion

    #region Damage Calculation

    /// <summary>
    /// Calculates fall damage based on connection type and check failure margin
    /// </summary>
    private int CalculateFallDamage(VerticalConnection connection, int failureMargin)
    {
        var (diceCount, dieSize) = connection.Type.GetFallDamage(connection.LevelsSpanned);

        if (diceCount == 0)
            return 0;

        // Add extra damage for catastrophic failures
        if (failureMargin <= -5)
        {
            diceCount += 1;
        }

        // Roll damage
        var damage = 0;
        for (int i = 0; i < diceCount; i++)
        {
            damage += _diceRoller?.Roll(1, dieSize) ?? RollDie(dieSize);
        }

        _logger.Debug("Fall damage: {DiceCount}d{DieSize} = {Damage}", diceCount, dieSize, damage);

        return damage;
    }

    /// <summary>
    /// Fallback die roller
    /// </summary>
    private int RollDie(int sides)
    {
        return new Random().Next(1, sides + 1);
    }

    #endregion
}

/// <summary>
/// Result of a traversal attempt
/// </summary>
public class TraversalResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int Damage { get; set; } = 0;
    public int LevelsTraversed { get; set; } = 0;

    public static TraversalResult CreateSuccess(string message, int levelsTraversed = 1)
    {
        return new TraversalResult
        {
            Success = true,
            Message = message,
            Damage = 0,
            LevelsTraversed = levelsTraversed
        };
    }

    public static TraversalResult Failure(string message, int damage = 0)
    {
        return new TraversalResult
        {
            Success = false,
            Message = message,
            Damage = damage,
            LevelsTraversed = 0
        };
    }
}

/// <summary>
/// Result of a skill check
/// </summary>
internal class CheckResult
{
    public bool Success { get; set; }
    public int TotalRoll { get; set; }
    public int Margin { get; set; }
    public int D20Roll { get; set; }
}
