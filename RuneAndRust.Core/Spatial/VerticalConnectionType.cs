namespace RuneAndRust.Core.Spatial;

/// <summary>
/// v0.39.1: Types of vertical connections between rooms at different Z levels
/// Each type has different traversal mechanics and skill check requirements
/// </summary>
public enum VerticalConnectionType
{
    /// <summary>
    /// Gradual ascent/descent via stairs (1-2 Z levels)
    /// Traversal: Free action, no check required
    /// </summary>
    Stairs,

    /// <summary>
    /// Open vertical drop requiring climbing (2-4 Z levels)
    /// Traversal: MIGHT check (Athletics) DC 12
    /// Failure: 1d6-2d6 Physical damage from falling
    /// </summary>
    Shaft,

    /// <summary>
    /// Mechanical lift system (any Z distance)
    /// Traversal: Automatic if powered, WITS check (Repair) DC 15 if unpowered
    /// Alternative: Climb shaft manually (treat as Shaft)
    /// </summary>
    Elevator,

    /// <summary>
    /// Fixed climbing route with rungs (1-3 Z levels)
    /// Traversal: MIGHT check (Athletics) DC 10
    /// Failure: 1d4 Physical damage from slipping
    /// </summary>
    Ladder,

    /// <summary>
    /// Blocked passage requiring clearing (N/A levels until cleared)
    /// Traversal: MIGHT check (Athletics) DC 15 to clear rubble (10 minutes)
    /// Alternative: Use explosives or abilities to blast through
    /// </summary>
    Collapsed
}

/// <summary>
/// Extension methods for VerticalConnectionType
/// </summary>
public static class VerticalConnectionTypeExtensions
{
    /// <summary>
    /// Gets the base traversal DC for this connection type
    /// </summary>
    public static int GetBaseTraversalDC(this VerticalConnectionType type)
    {
        return type switch
        {
            VerticalConnectionType.Stairs => 0,      // No check required
            VerticalConnectionType.Shaft => 12,      // Athletics DC 12
            VerticalConnectionType.Elevator => 0,    // Automatic if powered
            VerticalConnectionType.Ladder => 10,     // Athletics DC 10
            VerticalConnectionType.Collapsed => 15,  // Athletics DC 15 to clear
            _ => 0
        };
    }

    /// <summary>
    /// Gets the attribute required for traversal checks
    /// </summary>
    public static string GetRequiredAttribute(this VerticalConnectionType type)
    {
        return type switch
        {
            VerticalConnectionType.Shaft => "MIGHT",
            VerticalConnectionType.Ladder => "MIGHT",
            VerticalConnectionType.Collapsed => "MIGHT",
            VerticalConnectionType.Elevator => "WITS", // For repair if unpowered
            _ => "NONE"
        };
    }

    /// <summary>
    /// Gets the skill name for traversal checks
    /// </summary>
    public static string GetRequiredSkill(this VerticalConnectionType type)
    {
        return type switch
        {
            VerticalConnectionType.Shaft => "Athletics",
            VerticalConnectionType.Ladder => "Athletics",
            VerticalConnectionType.Collapsed => "Athletics",
            VerticalConnectionType.Elevator => "Repair",
            _ => "None"
        };
    }

    /// <summary>
    /// Checks if this connection type requires a skill check to traverse
    /// </summary>
    public static bool RequiresCheck(this VerticalConnectionType type)
    {
        return type switch
        {
            VerticalConnectionType.Stairs => false,
            VerticalConnectionType.Elevator => false, // If powered; check handled separately
            _ => true
        };
    }

    /// <summary>
    /// Gets the typical maximum number of Z levels this connection type can span
    /// </summary>
    public static int GetMaxLevelsSpanned(this VerticalConnectionType type)
    {
        return type switch
        {
            VerticalConnectionType.Stairs => 2,
            VerticalConnectionType.Shaft => 4,
            VerticalConnectionType.Elevator => 6,  // Can span all layers
            VerticalConnectionType.Ladder => 3,
            VerticalConnectionType.Collapsed => 0, // Blocked
            _ => 1
        };
    }

    /// <summary>
    /// Gets a flavor description template for this connection type
    /// </summary>
    public static List<string> GetDescriptionTemplates(this VerticalConnectionType type)
    {
        return type switch
        {
            VerticalConnectionType.Stairs => new List<string>
            {
                "Corroded metal stairs {direction} into {destination}",
                "Stone steps worn smooth by centuries, {direction}",
                "Spiral staircase, rusted railing barely intact, {direction}",
                "Emergency stairs, structural integrity questionable, {direction}"
            },
            VerticalConnectionType.Shaft => new List<string>
            {
                "Maintenance shaft plunges {direction}, handholds corroded",
                "Vertical tunnel with exposed cable conduits for climbing {direction}",
                "Broken elevator shaft, cables dangling uselessly, drops {direction}",
                "Service shaft, walls slick with condensation, extends {direction}"
            },
            VerticalConnectionType.Elevator => new List<string>
            {
                "Cargo elevator, controls flickering weakly, travels {direction}",
                "Personnel lift, mechanism groaning ominously, goes {direction}",
                "Heavy freight platform suspended by fraying cables, moves {direction}",
                "Emergency elevator, power status uncertain, leads {direction}"
            },
            VerticalConnectionType.Ladder => new List<string>
            {
                "Iron ladder bolted to wall, some rungs missing, climbs {direction}",
                "Maintenance ladder with rust-eaten rungs, extends {direction}",
                "Emergency access ladder, structural integrity questionable, goes {direction}",
                "Caged ladder, safety cage partially collapsed, leads {direction}"
            },
            VerticalConnectionType.Collapsed => new List<string>
            {
                "Stairwell choked with debris from ceiling collapse",
                "Shaft filled with twisted metal and rubble, impassable",
                "Elevator crushed by structural failure, completely blocked",
                "Passage buried under tons of debris, would require clearing"
            },
            _ => new List<string> { "Unknown connection" }
        };
    }

    /// <summary>
    /// Gets fall damage dice for failed traversal
    /// </summary>
    public static (int diceCount, int dieSize) GetFallDamage(this VerticalConnectionType type, int levelsSpanned)
    {
        return type switch
        {
            VerticalConnectionType.Shaft => (Math.Min(2 + levelsSpanned, 6), 6),  // 2d6 to 6d6
            VerticalConnectionType.Ladder => (1 + levelsSpanned / 2, 4),           // 1d4 to 2d4
            _ => (0, 0) // No fall damage
        };
    }
}
