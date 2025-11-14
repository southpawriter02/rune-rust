using RuneAndRust.Core;
using Serilog;

namespace RuneAndRust.Engine;

/// <summary>
/// v0.20.5: Service responsible for coordinated party movement - moving multiple members together
///
/// Philosophy: Coordinated movement represents synchronized spatial repositioning algorithms.
/// The party's combat AIs coordinate their positioning to maintain formation cohesion.
/// </summary>
public class CoordinatedMovementService
{
    private static readonly ILogger _log = Log.ForContext<CoordinatedMovementService>();
    private readonly PositioningService _positioning;

    public CoordinatedMovementService(PositioningService? positioningService = null)
    {
        _positioning = positioningService ?? new PositioningService();
    }

    /// <summary>
    /// Moves the entire party in a coordinated direction while maintaining formation
    /// </summary>
    public CoordinatedResult MoveFormation(List<object> party, Direction direction, BattlefieldGrid grid)
    {
        if (party == null || party.Count == 0)
        {
            return CoordinatedResult.Failure("Party is empty");
        }

        _log.Information("Coordinated movement: PartySize={Size}, Direction={Direction}",
            party.Count, direction);

        // Validate all party members have sufficient Stamina
        var staminaChecks = new List<(object member, string name, int stamina, int required)>();

        foreach (var member in party)
        {
            var (name, stamina, position) = member switch
            {
                PlayerCharacter player => (player.Name, player.Stamina, player.Position),
                Enemy enemy => (enemy.Name, 100, enemy.Position), // Enemies don't use stamina
                _ => throw new ArgumentException("Invalid party member type")
            };

            if (position == null)
            {
                return CoordinatedResult.Failure($"{name} has no position");
            }

            // Calculate stamina cost for this movement
            var newPosition = CalculateNewPosition(position.Value, direction);
            int cost = CalculateMovementCost(position.Value, newPosition);

            staminaChecks.Add((member, name, stamina, cost));

            if (member is PlayerCharacter && stamina < cost)
            {
                return CoordinatedResult.Failure($"{name} lacks Stamina for coordinated movement (need {cost}, have {stamina})");
            }
        }

        // Calculate all new positions
        var movePlan = new List<(object member, GridPosition from, GridPosition to)>();

        foreach (var member in party)
        {
            var position = member switch
            {
                PlayerCharacter player => player.Position!.Value,
                Enemy enemy => enemy.Position!.Value,
                _ => throw new ArgumentException("Invalid party member type")
            };

            var newPosition = CalculateNewPosition(position, direction);
            movePlan.Add((member, position, newPosition));
        }

        // Validate all new positions are valid and unoccupied (or will be vacated by another party member)
        var partyPositions = movePlan.Select(p => p.from).ToHashSet();

        foreach (var (member, from, to) in movePlan)
        {
            // Check if position is valid
            if (!grid.IsValidPosition(to))
            {
                var memberName = member switch
                {
                    PlayerCharacter player => player.Name,
                    Enemy enemy => enemy.Name,
                    _ => "Unknown"
                };

                return CoordinatedResult.Failure($"Coordinated movement blocked: {memberName} would move to invalid position {to}");
            }

            var tile = grid.GetTile(to);
            if (tile == null)
            {
                return CoordinatedResult.Failure($"Invalid target tile at {to}");
            }

            // Tile must be passable or occupied by another party member who is also moving
            if (tile.IsOccupied && !partyPositions.Contains(to))
            {
                return CoordinatedResult.Failure($"Coordinated movement blocked: Position {to} is occupied");
            }
        }

        // Execute coordinated movement
        // Clear all old positions first to avoid conflicts
        foreach (var (member, from, to) in movePlan)
        {
            var oldTile = grid.GetTile(from);
            if (oldTile != null)
            {
                oldTile.IsOccupied = false;
                oldTile.OccupantId = null;
            }
        }

        // Apply all new positions
        int totalStaminaCost = 0;

        foreach (var (member, from, to) in movePlan)
        {
            var tile = grid.GetTile(to);
            if (tile == null)
                continue;

            int cost = CalculateMovementCost(from, to);

            switch (member)
            {
                case PlayerCharacter player:
                    player.Position = to;
                    player.Stamina -= cost;
                    player.TilesMovedThisTurn++;
                    player.HasMovedThisTurn = true;
                    totalStaminaCost += cost;
                    tile.IsOccupied = true;
                    tile.OccupantId = "player";
                    _log.Debug("Party member moved: Name={Name}, From={From}, To={To}, Cost={Cost}",
                        player.Name, from, to, cost);
                    break;

                case Enemy enemy:
                    enemy.Position = to;
                    enemy.TilesMovedThisTurn++;
                    enemy.HasMovedThisTurn = true;
                    tile.IsOccupied = true;
                    tile.OccupantId = enemy.Id;
                    _log.Debug("Party member moved: Name={Name}, From={From}, To={To}",
                        enemy.Name, from, to);
                    break;
            }
        }

        _log.Information("Coordinated movement successful: Direction={Direction}, PartySize={Size}, TotalStaminaCost={Cost}",
            direction, party.Count, totalStaminaCost);

        return CoordinatedResult.Success(direction, totalStaminaCost);
    }

    /// <summary>
    /// Moves the party forward (toward enemy zone)
    /// </summary>
    public CoordinatedResult MoveForward(List<object> party, BattlefieldGrid grid)
    {
        return MoveFormation(party, Direction.Forward, grid);
    }

    /// <summary>
    /// Moves the party backward (away from enemy zone)
    /// </summary>
    public CoordinatedResult MoveBackward(List<object> party, BattlefieldGrid grid)
    {
        return MoveFormation(party, Direction.Backward, grid);
    }

    /// <summary>
    /// Shifts the party left (column - 1)
    /// </summary>
    public CoordinatedResult ShiftLeft(List<object> party, BattlefieldGrid grid)
    {
        return MoveFormation(party, Direction.Left, grid);
    }

    /// <summary>
    /// Shifts the party right (column + 1)
    /// </summary>
    public CoordinatedResult ShiftRight(List<object> party, BattlefieldGrid grid)
    {
        return MoveFormation(party, Direction.Right, grid);
    }

    /// <summary>
    /// Calculates the new position based on direction
    /// </summary>
    private GridPosition CalculateNewPosition(GridPosition current, Direction direction)
    {
        return direction switch
        {
            Direction.Forward => new GridPosition(
                current.Zone,
                current.Row == Row.Back ? Row.Front : current.Row, // Move to front row
                current.Column,
                current.Elevation
            ),

            Direction.Backward => new GridPosition(
                current.Zone,
                current.Row == Row.Front ? Row.Back : current.Row, // Move to back row
                current.Column,
                current.Elevation
            ),

            Direction.Left => new GridPosition(
                current.Zone,
                current.Row,
                Math.Max(0, current.Column - 1), // Move left (decrease column)
                current.Elevation
            ),

            Direction.Right => new GridPosition(
                current.Zone,
                current.Row,
                current.Column + 1, // Move right (increase column)
                current.Elevation
            ),

            _ => current
        };
    }

    /// <summary>
    /// Calculates the Stamina cost for a movement
    /// Row change: 10 Stamina
    /// Column change: 5 Stamina per column
    /// </summary>
    private int CalculateMovementCost(GridPosition from, GridPosition to)
    {
        int cost = 0;

        // Row change cost
        if (from.Row != to.Row)
        {
            cost += 10;
        }

        // Column change cost
        int columnDelta = Math.Abs(to.Column - from.Column);
        cost += columnDelta * 5;

        return cost;
    }

    /// <summary>
    /// Checks if the party can move in a specific direction
    /// </summary>
    public bool CanMoveInDirection(List<object> party, Direction direction, BattlefieldGrid grid)
    {
        if (party == null || party.Count == 0)
            return false;

        foreach (var member in party)
        {
            var position = member switch
            {
                PlayerCharacter player => player.Position,
                Enemy enemy => enemy.Position,
                _ => null
            };

            if (position == null)
                return false;

            var newPosition = CalculateNewPosition(position.Value, direction);

            // Check if new position is valid
            if (!grid.IsValidPosition(newPosition))
                return false;

            var tile = grid.GetTile(newPosition);
            if (tile == null || (!tile.IsPassable() && !IsPartyPosition(newPosition, party)))
                return false;
        }

        return true;
    }

    /// <summary>
    /// Checks if a position is occupied by a party member
    /// </summary>
    private bool IsPartyPosition(GridPosition position, List<object> party)
    {
        foreach (var member in party)
        {
            var memberPosition = member switch
            {
                PlayerCharacter player => player.Position,
                Enemy enemy => enemy.Position,
                _ => null
            };

            if (memberPosition.HasValue && memberPosition.Value == position)
                return true;
        }

        return false;
    }
}

/// <summary>
/// Direction for coordinated movement
/// </summary>
public enum Direction
{
    Forward,    // Toward enemy zone (Back -> Front row)
    Backward,   // Away from enemy zone (Front -> Back row)
    Left,       // Decrease column
    Right       // Increase column
}

/// <summary>
/// Result of a coordinated movement attempt
/// </summary>
public class CoordinatedResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public Direction Direction { get; set; }
    public int TotalStaminaCost { get; set; }

    public static CoordinatedResult Success(Direction direction, int totalStaminaCost)
    {
        return new CoordinatedResult
        {
            Success = true,
            Message = $"Party moved {direction}",
            Direction = direction,
            TotalStaminaCost = totalStaminaCost
        };
    }

    public static CoordinatedResult Failure(string message)
    {
        return new CoordinatedResult
        {
            Success = false,
            Message = message,
            Direction = Direction.Forward,
            TotalStaminaCost = 0
        };
    }
}
