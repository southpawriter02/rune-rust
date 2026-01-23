namespace RuneAndRust.Domain.ValueObjects;

using RuneAndRust.Domain.Enums;

/// <summary>
/// Represents an obstacle encountered during a chase sequence that both
/// participants must attempt to overcome.
/// </summary>
/// <remarks>
/// <para>
/// Obstacles are generated dynamically each round based on the environment
/// and chase context. Both the fleeing character and pursuer must make
/// skill checks against the same obstacle, with outcomes affecting distance.
/// </para>
/// <para>
/// <b>v0.15.2e:</b> Initial implementation of chase obstacles.
/// </para>
/// </remarks>
/// <param name="ObstacleType">The category of obstacle.</param>
/// <param name="SkillRequired">The skill used for the check (typically Acrobatics).</param>
/// <param name="Dc">The DC in successes needed to overcome the obstacle.</param>
/// <param name="SuccessDescription">Narrative text for successful navigation.</param>
/// <param name="FailureDescription">Narrative text for failed navigation.</param>
/// <param name="EnvironmentTag">Optional tag indicating environment context.</param>
public readonly record struct ChaseObstacle(
    ObstacleType ObstacleType,
    string SkillRequired,
    int Dc,
    string SuccessDescription,
    string FailureDescription,
    string? EnvironmentTag = null)
{
    /// <summary>
    /// Gets the display name for this obstacle type.
    /// </summary>
    public string TypeName => ObstacleType switch
    {
        ObstacleType.Gap => "Gap",
        ObstacleType.Climb => "Climb",
        ObstacleType.Debris => "Debris",
        ObstacleType.Crowd => "Crowd",
        ObstacleType.Hazard => "Hazard",
        _ => "Unknown"
    };

    /// <summary>
    /// Determines if this obstacle requires special handling from an
    /// existing acrobatics service (climb, leap).
    /// </summary>
    public bool RequiresSpecializedService => ObstacleType is
        ObstacleType.Gap or ObstacleType.Climb;

    /// <summary>
    /// Creates a display string for obstacle presentation.
    /// </summary>
    /// <returns>Formatted obstacle description for UI display.</returns>
    public string ToDisplayString() =>
        $"{TypeName} (DC {Dc}) - {SkillRequired}";

    /// <summary>
    /// Creates a Gap obstacle with default parameters.
    /// </summary>
    /// <param name="dc">The DC in successes needed.</param>
    /// <param name="gapWidth">Optional gap width description.</param>
    /// <returns>A new Gap obstacle.</returns>
    public static ChaseObstacle CreateGap(int dc, string? gapWidth = null)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(dc, 1, nameof(dc));

        var width = gapWidth ?? $"{dc * 5} feet";
        return new ChaseObstacle(
            ObstacleType.Gap,
            "Acrobatics",
            dc,
            $"You clear the {width} gap with a powerful leap.",
            $"You misjudge the {width} gap and lose momentum.");
    }

    /// <summary>
    /// Creates a Climb obstacle with default parameters.
    /// </summary>
    /// <param name="dc">The DC in successes needed.</param>
    /// <param name="surface">Optional surface description.</param>
    /// <returns>A new Climb obstacle.</returns>
    public static ChaseObstacle CreateClimb(int dc, string? surface = null)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(dc, 1, nameof(dc));

        var desc = surface ?? "wall";
        return new ChaseObstacle(
            ObstacleType.Climb,
            "Acrobatics",
            dc,
            $"You scale the {desc} without breaking stride.",
            $"You slip on the {desc} and lose precious seconds.");
    }

    /// <summary>
    /// Creates a Debris obstacle with default parameters.
    /// </summary>
    /// <param name="dc">The DC in successes needed.</param>
    /// <param name="debris">Optional debris description.</param>
    /// <returns>A new Debris obstacle.</returns>
    public static ChaseObstacle CreateDebris(int dc, string? debris = null)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(dc, 1, nameof(dc));

        var desc = debris ?? "scattered rubble";
        return new ChaseObstacle(
            ObstacleType.Debris,
            "Acrobatics",
            dc,
            $"You weave through the {desc} with nimble footwork.",
            $"The {desc} trips you up momentarily.");
    }

    /// <summary>
    /// Creates a Crowd obstacle with default parameters.
    /// </summary>
    /// <param name="dc">The DC in successes needed.</param>
    /// <param name="crowd">Optional crowd description.</param>
    /// <returns>A new Crowd obstacle.</returns>
    public static ChaseObstacle CreateCrowd(int dc, string? crowd = null)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(dc, 1, nameof(dc));

        var desc = crowd ?? "panicking civilians";
        return new ChaseObstacle(
            ObstacleType.Crowd,
            "Acrobatics",
            dc,
            $"You slip through the {desc} like water.",
            $"The {desc} slow you down as you push through.");
    }

    /// <summary>
    /// Creates a Hazard obstacle with default parameters.
    /// </summary>
    /// <param name="dc">The DC in successes needed.</param>
    /// <param name="hazard">Optional hazard description.</param>
    /// <returns>A new Hazard obstacle.</returns>
    public static ChaseObstacle CreateHazard(int dc, string? hazard = null)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(dc, 1, nameof(dc));

        var desc = hazard ?? "steam vent";
        return new ChaseObstacle(
            ObstacleType.Hazard,
            "Acrobatics",
            dc,
            $"You dodge the {desc} with split-second timing.",
            $"The {desc} forces you to take a longer route.");
    }

    /// <summary>
    /// Creates a custom obstacle with full parameters.
    /// </summary>
    /// <param name="type">The obstacle type.</param>
    /// <param name="dc">The DC in successes needed.</param>
    /// <param name="successDesc">Description for success.</param>
    /// <param name="failureDesc">Description for failure.</param>
    /// <param name="environmentTag">Optional environment tag.</param>
    /// <returns>A new ChaseObstacle.</returns>
    public static ChaseObstacle Create(
        ObstacleType type,
        int dc,
        string successDesc,
        string failureDesc,
        string? environmentTag = null)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(dc, 1, nameof(dc));
        ArgumentException.ThrowIfNullOrWhiteSpace(successDesc, nameof(successDesc));
        ArgumentException.ThrowIfNullOrWhiteSpace(failureDesc, nameof(failureDesc));

        return new ChaseObstacle(
            type,
            "Acrobatics",
            dc,
            successDesc,
            failureDesc,
            environmentTag);
    }
}
