// ------------------------------------------------------------------------------
// <copyright file="TrapDetectionResult.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Represents the result of automatic trap detection via the [Sixth Sense]
// ability, including detected trap information and detection context.
// Part of v0.15.4i Specialization Integration implementation.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents a detected trap from the [Sixth Sense] ability.
/// </summary>
/// <remarks>
/// <para>
/// Contains information about a trap that was automatically detected by
/// a Ruin-Stalker with the [Sixth Sense] ability. The trap's location is
/// revealed, but its exact mechanism is not.
/// </para>
/// </remarks>
/// <param name="TrapId">Unique identifier for the trap.</param>
/// <param name="TrapName">Display name of the trap (if known).</param>
/// <param name="PositionX">X coordinate of the trap.</param>
/// <param name="PositionY">Y coordinate of the trap.</param>
/// <param name="DistanceFromCharacter">Distance in feet from the detecting character.</param>
/// <param name="DirectionFromCharacter">Cardinal direction from the character.</param>
/// <param name="IsVisible">Whether the trap is visible after detection.</param>
public readonly record struct DetectedTrap(
    string TrapId,
    string TrapName,
    int PositionX,
    int PositionY,
    int DistanceFromCharacter,
    string DirectionFromCharacter,
    bool IsVisible)
{
    /// <summary>
    /// Creates a display string for the detected trap.
    /// </summary>
    /// <returns>A formatted string showing trap location.</returns>
    public string ToDisplayString()
    {
        return $"Trap detected {DistanceFromCharacter} ft {DirectionFromCharacter}" +
               (string.IsNullOrEmpty(TrapName) ? "" : $" ({TrapName})");
    }

    /// <inheritdoc/>
    public override string ToString() => ToDisplayString();
}

/// <summary>
/// Represents the complete result of trap detection via [Sixth Sense].
/// </summary>
/// <remarks>
/// <para>
/// This value object captures all information from a [Sixth Sense] detection:
/// <list type="bullet">
///   <item><description>The character who detected the traps</description></item>
///   <item><description>The detection radius used (typically 10 ft)</description></item>
///   <item><description>All traps that were detected</description></item>
///   <item><description>Narrative description of the detection</description></item>
/// </list>
/// </para>
/// <para>
/// <b>[Sixth Sense] Rules:</b>
/// <list type="bullet">
///   <item><description>Automatic detection, no roll required</description></item>
///   <item><description>10 foot detection radius</description></item>
///   <item><description>Reveals trap location but not mechanism</description></item>
///   <item><description>Triggers on movement into new areas</description></item>
/// </list>
/// </para>
/// </remarks>
/// <param name="CharacterId">ID of the character who detected the traps.</param>
/// <param name="CharacterPositionX">X coordinate of the detecting character.</param>
/// <param name="CharacterPositionY">Y coordinate of the detecting character.</param>
/// <param name="DetectionRadiusFeet">The detection radius in feet.</param>
/// <param name="DetectedTraps">List of traps that were detected.</param>
/// <param name="NarrativeText">Descriptive text for the detection.</param>
public readonly record struct TrapDetectionResult(
    string CharacterId,
    int CharacterPositionX,
    int CharacterPositionY,
    int DetectionRadiusFeet,
    IReadOnlyList<DetectedTrap> DetectedTraps,
    string NarrativeText)
{
    // =========================================================================
    // CONSTANTS
    // =========================================================================

    /// <summary>
    /// Default detection radius for [Sixth Sense] in feet.
    /// </summary>
    public const int DefaultDetectionRadius = 10;

    // =========================================================================
    // COMPUTED PROPERTIES
    // =========================================================================

    /// <summary>
    /// Gets the number of traps detected.
    /// </summary>
    public int TrapCount => DetectedTraps.Count;

    /// <summary>
    /// Gets a value indicating whether any traps were detected.
    /// </summary>
    public bool HasDetectedTraps => DetectedTraps.Count > 0;

    /// <summary>
    /// Gets a value indicating whether no traps were detected.
    /// </summary>
    public bool IsAreaClear => DetectedTraps.Count == 0;

    /// <summary>
    /// Gets the IDs of all detected traps.
    /// </summary>
    public IReadOnlyList<string> DetectedTrapIds => DetectedTraps.Select(t => t.TrapId).ToList();

    // =========================================================================
    // STATIC FACTORY METHODS
    // =========================================================================

    /// <summary>
    /// Creates a detection result when traps are found.
    /// </summary>
    /// <param name="characterId">The detecting character's ID.</param>
    /// <param name="characterX">Character X position.</param>
    /// <param name="characterY">Character Y position.</param>
    /// <param name="detectedTraps">The traps that were detected.</param>
    /// <returns>A trap detection result with the detected traps.</returns>
    /// <remarks>
    /// <para>
    /// Called when the [Sixth Sense] ability triggers and finds traps
    /// within the detection radius.
    /// </para>
    /// </remarks>
    public static TrapDetectionResult TrapsDetected(
        string characterId,
        int characterX,
        int characterY,
        IReadOnlyList<DetectedTrap> detectedTraps)
    {
        var narrative = detectedTraps.Count switch
        {
            1 => "Your instincts scream a warning. There's a trap nearby! " +
                 "You can sense its presence, though not its exact nature.",
            2 => "Your sixth sense flares with multiple warnings. " +
                 "Two traps lurk within your detection range.",
            _ => $"Danger! Your instincts are practically screaming. " +
                 $"You detect {detectedTraps.Count} traps in the immediate area."
        };

        return new TrapDetectionResult(
            CharacterId: characterId,
            CharacterPositionX: characterX,
            CharacterPositionY: characterY,
            DetectionRadiusFeet: DefaultDetectionRadius,
            DetectedTraps: detectedTraps,
            NarrativeText: narrative);
    }

    /// <summary>
    /// Creates a detection result when no traps are found.
    /// </summary>
    /// <param name="characterId">The detecting character's ID.</param>
    /// <param name="characterX">Character X position.</param>
    /// <param name="characterY">Character Y position.</param>
    /// <returns>A trap detection result indicating the area is clear.</returns>
    /// <remarks>
    /// <para>
    /// Called when the [Sixth Sense] ability triggers but finds no traps
    /// within the detection radius.
    /// </para>
    /// </remarks>
    public static TrapDetectionResult AreaClear(
        string characterId,
        int characterX,
        int characterY)
    {
        return new TrapDetectionResult(
            CharacterId: characterId,
            CharacterPositionX: characterX,
            CharacterPositionY: characterY,
            DetectionRadiusFeet: DefaultDetectionRadius,
            DetectedTraps: Array.Empty<DetectedTrap>(),
            NarrativeText: "Your senses reach out, probing for danger. " +
                          "The immediate area feels... safe. No traps nearby.");
    }

    /// <summary>
    /// Creates a detected trap record from position data.
    /// </summary>
    /// <param name="trapId">The trap's unique identifier.</param>
    /// <param name="trapName">The trap's display name (optional).</param>
    /// <param name="trapX">Trap X coordinate.</param>
    /// <param name="trapY">Trap Y coordinate.</param>
    /// <param name="characterX">Character X coordinate.</param>
    /// <param name="characterY">Character Y coordinate.</param>
    /// <returns>A detected trap record with calculated distance and direction.</returns>
    public static DetectedTrap CreateDetectedTrap(
        string trapId,
        string trapName,
        int trapX,
        int trapY,
        int characterX,
        int characterY)
    {
        var dx = trapX - characterX;
        var dy = trapY - characterY;
        var distance = (int)Math.Sqrt(dx * dx + dy * dy);
        var direction = CalculateDirection(dx, dy);

        return new DetectedTrap(
            TrapId: trapId,
            TrapName: trapName,
            PositionX: trapX,
            PositionY: trapY,
            DistanceFromCharacter: distance,
            DirectionFromCharacter: direction,
            IsVisible: true);
    }

    // =========================================================================
    // DISPLAY METHODS
    // =========================================================================

    /// <summary>
    /// Creates a display string for the detection result.
    /// </summary>
    /// <returns>A formatted multi-line string showing detection details.</returns>
    /// <example>
    /// <code>
    /// var result = TrapDetectionResult.TrapsDetected("char-001", 5, 5, traps);
    /// Console.WriteLine(result.ToDisplayString());
    /// // Output:
    /// // [SIXTH SENSE] Trap Detection
    /// // Position: (5, 5) | Radius: 10 ft
    /// // Detected: 2 traps
    /// //   - Trap detected 5 ft north (Pressure Plate)
    /// //   - Trap detected 8 ft east (Tripwire)
    /// // Your sixth sense flares with multiple warnings...
    /// </code>
    /// </example>
    public string ToDisplayString()
    {
        var lines = new List<string>
        {
            "[SIXTH SENSE] Trap Detection",
            $"Position: ({CharacterPositionX}, {CharacterPositionY}) | Radius: {DetectionRadiusFeet} ft",
            $"Detected: {TrapCount} trap{(TrapCount == 1 ? "" : "s")}"
        };

        if (HasDetectedTraps)
        {
            foreach (var trap in DetectedTraps)
            {
                lines.Add($"  - {trap.ToDisplayString()}");
            }
        }

        lines.Add(string.Empty);
        lines.Add(NarrativeText);

        return string.Join(Environment.NewLine, lines);
    }

    /// <summary>
    /// Returns a compact log-friendly string.
    /// </summary>
    /// <returns>A single-line string for logging.</returns>
    public string ToLogString()
    {
        return $"TrapDetection[{CharacterId}] at ({CharacterPositionX},{CharacterPositionY}): " +
               $"{TrapCount} traps detected within {DetectionRadiusFeet} ft";
    }

    /// <inheritdoc/>
    public override string ToString() => ToLogString();

    // =========================================================================
    // PRIVATE HELPERS
    // =========================================================================

    /// <summary>
    /// Calculates the cardinal direction from coordinate deltas.
    /// </summary>
    /// <param name="dx">X delta (positive = east).</param>
    /// <param name="dy">Y delta (positive = north, depending on coordinate system).</param>
    /// <returns>A cardinal direction string.</returns>
    private static string CalculateDirection(int dx, int dy)
    {
        // Simple 8-direction calculation
        // Assumes positive Y is north in the game's coordinate system
        return (dx, dy) switch
        {
            (0, > 0) => "north",
            (0, < 0) => "south",
            ( > 0, 0) => "east",
            ( < 0, 0) => "west",
            ( > 0, > 0) => "northeast",
            ( < 0, > 0) => "northwest",
            ( > 0, < 0) => "southeast",
            ( < 0, < 0) => "southwest",
            _ => "nearby"
        };
    }
}
