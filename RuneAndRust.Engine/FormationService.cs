using RuneAndRust.Core;
using Serilog;

namespace RuneAndRust.Engine;

/// <summary>
/// v0.20.5: Service responsible for managing party formations and tactical positioning
///
/// Philosophy: Formation bonuses represent emergent properties of coordinated positioning -
/// overlapping fields of fire, threat saturation, and mutual support. NOT magical buffs.
/// </summary>
public class FormationService
{
    private static readonly ILogger _log = Log.ForContext<FormationService>();

    /// <summary>
    /// Applies a formation to a party, positioning them on the grid and applying formation bonuses
    /// </summary>
    public FormationResult ApplyFormation(List<object> party, FormationType formation, BattlefieldGrid grid)
    {
        if (party == null || party.Count == 0)
        {
            return FormationResult.Failure("Party is empty");
        }

        _log.Information("Applying formation: Formation={FormationType}, PartySize={Size}",
            formation, party.Count);

        // Get formation positions for party size
        var positions = GetFormationPositions(formation, party.Count, grid.Columns);

        if (positions.Count != party.Count)
        {
            _log.Warning("Formation mismatch: Expected={Expected}, Actual={Actual}",
                positions.Count, party.Count);
            return FormationResult.Failure($"Party size {party.Count} doesn't match formation (needs {positions.Count})");
        }

        // Validate all positions are unoccupied
        foreach (var position in positions)
        {
            var tile = grid.GetTile(position);
            if (tile == null)
            {
                _log.Warning("Formation position invalid: Position={Position}", position);
                return FormationResult.Failure($"Position {position} is invalid");
            }

            if (tile.IsOccupied)
            {
                _log.Warning("Formation position occupied: Position={Position}", position);
                return FormationResult.Failure($"Position {position} already occupied");
            }
        }

        // Assign positions to party members
        for (int i = 0; i < party.Count; i++)
        {
            var member = party[i];
            var position = positions[i];
            var tile = grid.GetTile(position);

            if (tile == null)
                continue;

            // Update member position and tile occupancy
            switch (member)
            {
                case PlayerCharacter player:
                    player.Position = position;
                    tile.IsOccupied = true;
                    tile.OccupantId = "player";
                    _log.Information("Party member positioned: Name={Name}, Position={Position}",
                        player.Name, position);
                    break;

                case Enemy enemy: // For future allied NPCs that might use Enemy template
                    enemy.Position = position;
                    tile.IsOccupied = true;
                    tile.OccupantId = enemy.Id;
                    _log.Information("Party member positioned: Name={Name}, Position={Position}",
                        enemy.Name, position);
                    break;

                default:
                    throw new ArgumentException($"Invalid party member type: {member.GetType().Name}");
            }
        }

        // Calculate and apply formation bonuses
        var bonuses = CalculateFormationBonuses(formation, party, grid);

        _log.Information("Formation applied: Formation={Formation}, Bonuses={BonusCount}",
            formation, bonuses.Count);

        return FormationResult.CreateSuccess(formation, bonuses);
    }

    /// <summary>
    /// Gets the grid positions for a specific formation type and party size
    /// </summary>
    private List<GridPosition> GetFormationPositions(FormationType formation, int partySize, int gridColumns)
    {
        return formation switch
        {
            FormationType.Line => GetLinePositions(partySize, gridColumns),
            FormationType.Wedge => GetWedgePositions(partySize, gridColumns),
            FormationType.Scattered => GetScatteredPositions(partySize, gridColumns),
            FormationType.Protect => GetProtectPositions(partySize, gridColumns),
            _ => GetLinePositions(partySize, gridColumns)
        };
    }

    /// <summary>
    /// Line Formation: Defensive front-line hold
    /// Front: [T][T][T]  Back: [C][C]
    /// </summary>
    private List<GridPosition> GetLinePositions(int partySize, int gridColumns)
    {
        var positions = new List<GridPosition>();

        // Fill front row first (up to 3 members)
        int frontCount = Math.Min(3, partySize);
        int startColumn = Math.Max(0, (gridColumns - frontCount) / 2); // Center the formation

        for (int i = 0; i < frontCount; i++)
        {
            positions.Add(new GridPosition(Zone.Player, Row.Front, startColumn + i));
        }

        // Remaining members go to back row
        int backCount = partySize - frontCount;
        if (backCount > 0)
        {
            int backStartColumn = Math.Max(0, (gridColumns - backCount) / 2);
            for (int i = 0; i < backCount; i++)
            {
                positions.Add(new GridPosition(Zone.Player, Row.Back, backStartColumn + i));
            }
        }

        return positions;
    }

    /// <summary>
    /// Wedge Formation: Concentrated assault
    /// Front: [T][T][T]  Back: [C]
    /// </summary>
    private List<GridPosition> GetWedgePositions(int partySize, int gridColumns)
    {
        var positions = new List<GridPosition>();

        // Heavy front row (up to 3 members)
        int frontCount = Math.Min(3, partySize);
        int startColumn = Math.Max(0, (gridColumns - frontCount) / 2);

        for (int i = 0; i < frontCount; i++)
        {
            positions.Add(new GridPosition(Zone.Player, Row.Front, startColumn + i));
        }

        // Back row: center position only (wedge point)
        int backCount = partySize - frontCount;
        if (backCount > 0)
        {
            int centerColumn = gridColumns / 2;
            positions.Add(new GridPosition(Zone.Player, Row.Back, centerColumn));

            // If more than 1 back row member, add to sides
            for (int i = 1; i < backCount; i++)
            {
                int column = i % 2 == 0 ? centerColumn + (i / 2) : centerColumn - ((i + 1) / 2);
                column = Math.Clamp(column, 0, gridColumns - 1);
                positions.Add(new GridPosition(Zone.Player, Row.Back, column));
            }
        }

        return positions;
    }

    /// <summary>
    /// Scattered Formation: Spread out for mobility and anti-flanking
    /// Front: [T][ ][T]  Back: [C][ ][C]
    /// </summary>
    private List<GridPosition> GetScatteredPositions(int partySize, int gridColumns)
    {
        var positions = new List<GridPosition>();

        // Alternate between front and back rows, spread across columns
        for (int i = 0; i < partySize; i++)
        {
            Row row = i % 2 == 0 ? Row.Front : Row.Back;

            // Spread across available columns with gaps
            int spreadIndex = i / 2;
            int column = Math.Min(spreadIndex * 2, gridColumns - 1);

            positions.Add(new GridPosition(Zone.Player, row, column));
        }

        return positions;
    }

    /// <summary>
    /// Protect Formation: Shield high-value back-row target
    /// Front: [T][T][T]  Back: [ ][S][ ]
    /// </summary>
    private List<GridPosition> GetProtectPositions(int partySize, int gridColumns)
    {
        var positions = new List<GridPosition>();

        int centerColumn = gridColumns / 2;

        if (partySize == 1)
        {
            // Single member goes to back center
            positions.Add(new GridPosition(Zone.Player, Row.Back, centerColumn));
        }
        else if (partySize == 2)
        {
            // One front, one protected back
            positions.Add(new GridPosition(Zone.Player, Row.Front, centerColumn));
            positions.Add(new GridPosition(Zone.Player, Row.Back, centerColumn));
        }
        else if (partySize == 3)
        {
            // Two front guards, one protected back
            positions.Add(new GridPosition(Zone.Player, Row.Front, Math.Max(0, centerColumn - 1)));
            positions.Add(new GridPosition(Zone.Player, Row.Front, Math.Min(gridColumns - 1, centerColumn + 1)));
            positions.Add(new GridPosition(Zone.Player, Row.Back, centerColumn));
        }
        else // 4+ members
        {
            // Three front guards
            positions.Add(new GridPosition(Zone.Player, Row.Front, Math.Max(0, centerColumn - 1)));
            positions.Add(new GridPosition(Zone.Player, Row.Front, centerColumn));
            positions.Add(new GridPosition(Zone.Player, Row.Front, Math.Min(gridColumns - 1, centerColumn + 1)));

            // Protected member at back center
            positions.Add(new GridPosition(Zone.Player, Row.Back, centerColumn));

            // Additional members fill in back row
            for (int i = 4; i < partySize; i++)
            {
                int offset = (i - 3) % 2 == 0 ? (i - 3) / 2 : -((i - 2) / 2);
                int column = Math.Clamp(centerColumn + offset, 0, gridColumns - 1);
                positions.Add(new GridPosition(Zone.Player, Row.Back, column));
            }
        }

        return positions;
    }

    /// <summary>
    /// Calculates formation bonuses based on formation type and current party positions
    /// </summary>
    private List<FormationBonus> CalculateFormationBonuses(FormationType formation, List<object> party, BattlefieldGrid grid)
    {
        return formation switch
        {
            FormationType.Line => GetLineBonuses(party, grid),
            FormationType.Wedge => GetWedgeBonuses(party, grid),
            FormationType.Scattered => GetScatteredBonuses(party, grid),
            FormationType.Protect => GetProtectBonuses(party, grid),
            _ => new List<FormationBonus>()
        };
    }

    /// <summary>
    /// Line Formation bonuses: +1 Defense for all front-row members
    /// </summary>
    private List<FormationBonus> GetLineBonuses(List<object> party, BattlefieldGrid grid)
    {
        var bonuses = new List<FormationBonus>();

        foreach (var member in party)
        {
            var position = GetPosition(member);
            if (position.Row == Row.Front)
            {
                bonuses.Add(new FormationBonus
                {
                    Target = member,
                    Type = BonusType.Defense,
                    Amount = 1,
                    Description = "Line Formation: Defensive Stance"
                });
            }
        }

        _log.Debug("Line Formation bonuses: BonusCount={BonusCount}", bonuses.Count);
        return bonuses;
    }

    /// <summary>
    /// Wedge Formation bonuses: +1 Accuracy for front-row members
    /// </summary>
    private List<FormationBonus> GetWedgeBonuses(List<object> party, BattlefieldGrid grid)
    {
        var bonuses = new List<FormationBonus>();

        foreach (var member in party)
        {
            var position = GetPosition(member);
            if (position.Row == Row.Front)
            {
                bonuses.Add(new FormationBonus
                {
                    Target = member,
                    Type = BonusType.Accuracy,
                    Amount = 1,
                    Description = "Wedge Formation: Concentrated Assault"
                });
            }
        }

        _log.Debug("Wedge Formation bonuses: BonusCount={BonusCount}", bonuses.Count);
        return bonuses;
    }

    /// <summary>
    /// Scattered Formation bonuses: -1 enemy flanking chance (harder to flank spread-out party)
    /// </summary>
    private List<FormationBonus> GetScatteredBonuses(List<object> party, BattlefieldGrid grid)
    {
        var bonuses = new List<FormationBonus>();

        // Apply anti-flanking bonus to all party members
        foreach (var member in party)
        {
            bonuses.Add(new FormationBonus
            {
                Target = member,
                Type = BonusType.AntiFlanking,
                Amount = 1,
                Description = "Scattered Formation: Harder to Flank"
            });
        }

        _log.Debug("Scattered Formation bonuses: BonusCount={BonusCount}", bonuses.Count);
        return bonuses;
    }

    /// <summary>
    /// Protect Formation bonuses: +2 Defense for back-row center member (protected target)
    /// </summary>
    private List<FormationBonus> GetProtectBonuses(List<object> party, BattlefieldGrid grid)
    {
        var bonuses = new List<FormationBonus>();

        // Find the back-row center member (the protected target)
        int centerColumn = grid.Columns / 2;

        foreach (var member in party)
        {
            var position = GetPosition(member);
            if (position.Row == Row.Back && position.Column == centerColumn)
            {
                bonuses.Add(new FormationBonus
                {
                    Target = member,
                    Type = BonusType.Defense,
                    Amount = 2,
                    Description = "Protect Formation: Shielded Position"
                });
                break; // Only one protected target
            }
        }

        _log.Debug("Protect Formation bonuses: BonusCount={BonusCount}", bonuses.Count);
        return bonuses;
    }

    /// <summary>
    /// Swaps the positions of two party members
    /// Cost: 5 Stamina per person (10 total)
    /// </summary>
    public SwapResult SwapPositions(object actor1, object actor2, BattlefieldGrid grid)
    {
        var (pos1, stamina1, name1, id1) = actor1 switch
        {
            PlayerCharacter player => (player.Position, player.Stamina, player.Name, "player"),
            Enemy enemy => (enemy.Position, 100, enemy.Name, enemy.Id), // Enemies have unlimited stamina for swaps
            _ => throw new ArgumentException("Invalid actor1 type")
        };

        var (pos2, stamina2, name2, id2) = actor2 switch
        {
            PlayerCharacter player => (player.Position, player.Stamina, player.Name, "player"),
            Enemy enemy => (enemy.Position, 100, enemy.Name, enemy.Id),
            _ => throw new ArgumentException("Invalid actor2 type")
        };

        if (pos1 == null || pos2 == null)
        {
            return SwapResult.Failure("One or both actors have no position");
        }

        _log.Information("Position swap: Actor1={Actor1Name}@{Pos1}, Actor2={Actor2Name}@{Pos2}",
            name1, pos1, name2, pos2);

        // Validate swap (must be allies - same zone)
        if (pos1.Value.Zone != pos2.Value.Zone)
        {
            return SwapResult.Failure("Cannot swap positions with enemies");
        }

        // Check if within swap range (max 2 tiles Manhattan distance)
        int distance = CalculateManhattanDistance(pos1.Value, pos2.Value);
        if (distance > 2)
        {
            return SwapResult.Failure("Too far to swap positions (max 2 tiles)");
        }

        // Check Stamina cost (5 per person, only for PlayerCharacter)
        const int staminaCost = 5;

        if (actor1 is PlayerCharacter && stamina1 < staminaCost)
        {
            return SwapResult.Failure($"{name1} has insufficient Stamina to swap (need {staminaCost}, have {stamina1})");
        }

        if (actor2 is PlayerCharacter && stamina2 < staminaCost)
        {
            return SwapResult.Failure($"{name2} has insufficient Stamina to swap (need {staminaCost}, have {stamina2})");
        }

        // Execute swap
        var tile1 = grid.GetTile(pos1.Value);
        var tile2 = grid.GetTile(pos2.Value);

        if (tile1 == null || tile2 == null)
        {
            return SwapResult.Failure("Invalid tile positions");
        }

        // Swap positions
        switch (actor1)
        {
            case PlayerCharacter player:
                player.Position = pos2.Value;
                player.Stamina -= staminaCost;
                break;
            case Enemy enemy:
                enemy.Position = pos2.Value;
                break;
        }

        switch (actor2)
        {
            case PlayerCharacter player:
                player.Position = pos1.Value;
                player.Stamina -= staminaCost;
                break;
            case Enemy enemy:
                enemy.Position = pos1.Value;
                break;
        }

        // Swap tile occupancy
        tile1.OccupantId = id2;
        tile2.OccupantId = id1;

        _log.Information("Position swap successful: Actor1={Actor1Name}, Actor2={Actor2Name}, StaminaCost={Cost}",
            name1, name2, staminaCost * 2);

        return SwapResult.CreateSuccess(staminaCost * 2);
    }

    /// <summary>
    /// Detects what formation a party is currently in
    /// </summary>
    public FormationType DetectFormation(List<object> party, BattlefieldGrid grid)
    {
        if (party == null || party.Count == 0)
            return FormationType.None;

        var positions = party.Select(GetPosition).ToList();

        // Count front vs back row
        int frontCount = positions.Count(p => p.Row == Row.Front);
        int backCount = positions.Count(p => p.Row == Row.Back);

        // Check for concentration (same/adjacent columns)
        var columns = positions.Select(p => p.Column).Distinct().ToList();
        bool concentrated = columns.Count <= 2 && columns.Count < party.Count;

        // Check for spread (gaps between columns)
        bool scattered = columns.Count >= party.Count / 2 + 1;

        // Check for protect pattern (back row center)
        int centerColumn = grid.Columns / 2;
        bool hasProtectedCenter = positions.Any(p => p.Row == Row.Back && p.Column == centerColumn);
        bool hasMultipleFrontGuards = positions.Count(p => p.Row == Row.Front) >= 2;

        // Pattern matching
        if (hasProtectedCenter && hasMultipleFrontGuards && backCount == 1)
            return FormationType.Protect;

        if (scattered)
            return FormationType.Scattered;

        if (concentrated && frontCount > backCount)
            return FormationType.Wedge;

        if (frontCount >= party.Count / 2)
            return FormationType.Line;

        return FormationType.None;
    }

    /// <summary>
    /// Helper to get position from any combatant type
    /// </summary>
    private GridPosition GetPosition(object combatant)
    {
        return combatant switch
        {
            PlayerCharacter player => player.Position ?? new GridPosition(Zone.Player, Row.Front, 0),
            Enemy enemy => enemy.Position ?? new GridPosition(Zone.Enemy, Row.Front, 0),
            _ => new GridPosition(Zone.Player, Row.Front, 0)
        };
    }

    /// <summary>
    /// Calculates Manhattan distance between two grid positions
    /// </summary>
    private int CalculateManhattanDistance(GridPosition from, GridPosition to)
    {
        int columnDistance = Math.Abs(to.Column - from.Column);
        int rowDistance = from.Row != to.Row ? 1 : 0;
        return columnDistance + rowDistance;
    }
}

/// <summary>
/// Formation types available to the party
/// </summary>
public enum FormationType
{
    None,       // No specific formation
    Line,       // Defensive front-line hold (+1 Defense for front row)
    Wedge,      // Aggressive concentrated assault (+1 Accuracy for front row)
    Scattered,  // Spread out for mobility (harder to flank)
    Protect     // Shield high-value back-row target (+2 Defense for back center)
}

/// <summary>
/// Bonus types that can be applied from formations
/// </summary>
public enum BonusType
{
    Defense,        // +N Defense dice
    Accuracy,       // +N Accuracy dice
    Resolve,        // +N Resolve (WILL) dice
    Damage,         // +N Damage dice
    AntiFlanking    // -N to enemy flanking attempts
}

/// <summary>
/// Represents a formation bonus applied to a party member
/// </summary>
public class FormationBonus
{
    public object Target { get; set; } = null!;         // The party member receiving the bonus
    public BonusType Type { get; set; }                 // Type of bonus
    public int Amount { get; set; }                     // Magnitude of bonus
    public string Description { get; set; } = string.Empty; // Human-readable description
}

/// <summary>
/// Result of a formation application
/// </summary>
public class FormationResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public FormationType Formation { get; set; }
    public List<FormationBonus> Bonuses { get; set; } = new();

    public static FormationResult CreateSuccess(FormationType formation, List<FormationBonus> bonuses)
    {
        return new FormationResult
        {
            Success = true,
            Message = $"{formation} formation applied successfully",
            Formation = formation,
            Bonuses = bonuses
        };
    }

    public static FormationResult Failure(string message)
    {
        return new FormationResult
        {
            Success = false,
            Message = message,
            Formation = FormationType.None,
            Bonuses = new List<FormationBonus>()
        };
    }
}

/// <summary>
/// Result of a position swap
/// </summary>
public class SwapResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int StaminaCost { get; set; }

    public static SwapResult CreateSuccess(int staminaCost)
    {
        return new SwapResult
        {
            Success = true,
            Message = "Positions swapped successfully",
            StaminaCost = staminaCost
        };
    }

    public static SwapResult Failure(string message)
    {
        return new SwapResult
        {
            Success = false,
            Message = message,
            StaminaCost = 0
        };
    }
}
