namespace RuneAndRust.Core.Spatial;

/// <summary>
/// v0.39.1: Represents a vertical connection between two rooms at different Z levels
/// Stores traversal mechanics, skill check requirements, and descriptive elements
/// </summary>
public class VerticalConnection
{
    /// <summary>
    /// Unique identifier for this connection
    /// </summary>
    public string ConnectionId { get; set; } = string.Empty;

    /// <summary>
    /// Source room ID (origin of traversal)
    /// </summary>
    public string FromRoomId { get; set; } = string.Empty;

    /// <summary>
    /// Destination room ID (target of traversal)
    /// </summary>
    public string ToRoomId { get; set; } = string.Empty;

    /// <summary>
    /// Type of vertical connection (Stairs, Shaft, Elevator, Ladder, Collapsed)
    /// </summary>
    public VerticalConnectionType Type { get; set; } = VerticalConnectionType.Stairs;

    /// <summary>
    /// Skill check difficulty for traversal (0 = no check required)
    /// </summary>
    public int TraversalDC { get; set; } = 0;

    /// <summary>
    /// Whether this connection is currently blocked and impassable
    /// </summary>
    public bool IsBlocked { get; set; } = false;

    /// <summary>
    /// Description of blockage if IsBlocked is true
    /// </summary>
    public string? BlockageDescription { get; set; } = null;

    /// <summary>
    /// Number of Z levels this connection spans (typically 1-4)
    /// </summary>
    public int LevelsSpanned { get; set; } = 1;

    /// <summary>
    /// Flavor text description of this connection
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Hazards present during traversal (e.g., steam vents, electrical discharge)
    /// </summary>
    public List<string> Hazards { get; set; } = new List<string>();

    /// <summary>
    /// Whether this connection can be traversed in both directions
    /// </summary>
    public bool IsBidirectional { get; set; } = true;

    /// <summary>
    /// For elevators: whether the elevator is currently powered
    /// </summary>
    public bool? IsPowered { get; set; } = null;

    /// <summary>
    /// For collapsed connections: DC to clear the blockage
    /// </summary>
    public int? ClearanceDC { get; set; } = null;

    /// <summary>
    /// For collapsed connections: time in minutes to clear
    /// </summary>
    public int? ClearanceTimeMinutes { get; set; } = null;

    /// <summary>
    /// Gets the direction of traversal (ascending or descending)
    /// </summary>
    public string GetTraversalDirection(string fromRoomId)
    {
        // This will be populated by the spatial layout service
        // based on actual Z coordinates
        return fromRoomId == FromRoomId ? "down" : "up";
    }

    /// <summary>
    /// Checks if this connection can currently be traversed
    /// </summary>
    public bool CanTraverse()
    {
        if (IsBlocked) return false;
        if (Type == VerticalConnectionType.Collapsed) return false;
        if (Type == VerticalConnectionType.Elevator && IsPowered == false) return false;
        return true;
    }

    /// <summary>
    /// Gets a user-friendly description of traversal requirements
    /// </summary>
    public string GetTraversalRequirementsDescription()
    {
        if (IsBlocked)
        {
            return $"Blocked. {BlockageDescription ?? "The passage is impassable."}";
        }

        return Type switch
        {
            VerticalConnectionType.Stairs => "No skill check required. Free traversal.",
            VerticalConnectionType.Shaft => $"Requires Athletics check (MIGHT DC {TraversalDC}). Failure causes fall damage.",
            VerticalConnectionType.Ladder => $"Requires Athletics check (MIGHT DC {TraversalDC}). Failure causes minor fall damage.",
            VerticalConnectionType.Elevator => IsPowered == true
                ? "Elevator is powered. No check required."
                : "Elevator is unpowered. Requires Repair check (WITS DC 15) or climb shaft manually.",
            VerticalConnectionType.Collapsed => $"Impassable. Clearing requires MIGHT DC {ClearanceDC ?? 15} ({ClearanceTimeMinutes ?? 10} minutes).",
            _ => "Unknown connection type."
        };
    }

    /// <summary>
    /// Gets flavor text for successful traversal
    /// </summary>
    public string GetSuccessDescription(string direction)
    {
        return Type switch
        {
            VerticalConnectionType.Stairs => $"You carefully navigate the stairs {direction}.",
            VerticalConnectionType.Shaft => $"You successfully climb the shaft {direction}, your hands finding purchase on the corroded handholds.",
            VerticalConnectionType.Ladder => $"You ascend/descend the ladder {direction}, testing each rung before trusting your weight to it.",
            VerticalConnectionType.Elevator => $"The elevator groans and shudders as it carries you {direction}.",
            _ => $"You traverse the connection {direction}."
        };
    }

    /// <summary>
    /// Gets flavor text for failed traversal
    /// </summary>
    public string GetFailureDescription(int damage)
    {
        return Type switch
        {
            VerticalConnectionType.Shaft => $"Your grip fails and you plummet! You take {damage} Physical damage from the fall.",
            VerticalConnectionType.Ladder => $"A rung gives way beneath you! You fall a short distance, taking {damage} Physical damage.",
            _ => $"Your attempt fails, causing {damage} Physical damage."
        };
    }

    /// <summary>
    /// Creates a basic stairs connection between two rooms
    /// </summary>
    public static VerticalConnection CreateStairs(string fromRoomId, string toRoomId, int levelsSpanned = 1)
    {
        return new VerticalConnection
        {
            ConnectionId = $"vc_{fromRoomId}_{toRoomId}_stairs",
            FromRoomId = fromRoomId,
            ToRoomId = toRoomId,
            Type = VerticalConnectionType.Stairs,
            TraversalDC = 0,
            LevelsSpanned = levelsSpanned,
            Description = "Corroded metal stairs descend into the depths.",
            IsBidirectional = true
        };
    }

    /// <summary>
    /// Creates a shaft connection requiring climbing
    /// </summary>
    public static VerticalConnection CreateShaft(string fromRoomId, string toRoomId, int levelsSpanned = 2, int dc = 12)
    {
        return new VerticalConnection
        {
            ConnectionId = $"vc_{fromRoomId}_{toRoomId}_shaft",
            FromRoomId = fromRoomId,
            ToRoomId = toRoomId,
            Type = VerticalConnectionType.Shaft,
            TraversalDC = dc,
            LevelsSpanned = levelsSpanned,
            Description = "A maintenance shaft plunges into the depths. Rusted handholds line the walls.",
            IsBidirectional = true
        };
    }

    /// <summary>
    /// Creates an elevator connection
    /// </summary>
    public static VerticalConnection CreateElevator(string fromRoomId, string toRoomId, int levelsSpanned, bool isPowered = true)
    {
        return new VerticalConnection
        {
            ConnectionId = $"vc_{fromRoomId}_{toRoomId}_elevator",
            FromRoomId = fromRoomId,
            ToRoomId = toRoomId,
            Type = VerticalConnectionType.Elevator,
            TraversalDC = isPowered ? 0 : 15,
            LevelsSpanned = levelsSpanned,
            IsPowered = isPowered,
            Description = isPowered
                ? "A cargo elevator with flickering control panels. The mechanism appears functional."
                : "A cargo elevator. The controls are dark—no power. The shaft could be climbed manually.",
            IsBidirectional = true
        };
    }

    /// <summary>
    /// Creates a ladder connection
    /// </summary>
    public static VerticalConnection CreateLadder(string fromRoomId, string toRoomId, int levelsSpanned = 1, int dc = 10)
    {
        return new VerticalConnection
        {
            ConnectionId = $"vc_{fromRoomId}_{toRoomId}_ladder",
            FromRoomId = fromRoomId,
            ToRoomId = toRoomId,
            Type = VerticalConnectionType.Ladder,
            TraversalDC = dc,
            LevelsSpanned = levelsSpanned,
            Description = "An iron ladder bolted to the wall. Some rungs are missing.",
            IsBidirectional = true
        };
    }

    /// <summary>
    /// Creates a blocked/collapsed connection
    /// </summary>
    public static VerticalConnection CreateCollapsed(string fromRoomId, string toRoomId, string blockageDescription, int clearanceDC = 15)
    {
        return new VerticalConnection
        {
            ConnectionId = $"vc_{fromRoomId}_{toRoomId}_collapsed",
            FromRoomId = fromRoomId,
            ToRoomId = toRoomId,
            Type = VerticalConnectionType.Collapsed,
            IsBlocked = true,
            BlockageDescription = blockageDescription,
            ClearanceDC = clearanceDC,
            ClearanceTimeMinutes = 10,
            Description = "The passage is choked with debris.",
            IsBidirectional = true
        };
    }

    public override string ToString()
    {
        return $"{Type} from {FromRoomId} to {ToRoomId} ({LevelsSpanned} levels)";
    }
}
