namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Encapsulates the result of using the Trap Mastery ability, covering both trap placement
/// and enemy trap detection modes.
/// </summary>
/// <remarks>
/// <para>Trap Mastery is a Tier 2 active ability for the Veiðimaðr (Hunter) specialization
/// with two modes of operation:</para>
/// <list type="bullet">
/// <item><description><see cref="ResultType.TrapPlaced"/>: A hunting trap was placed at a location (2 AP).</description></item>
/// <item><description><see cref="ResultType.TrapsDetected"/>: Nearby traps were scanned for via perception check (2 AP).</description></item>
/// </list>
/// <para>Use the static factory methods to create properly configured results:
/// <see cref="CreatePlacementSuccess"/>, <see cref="CreatePlacementFailure"/>,
/// <see cref="CreateDetectionSuccess"/>, <see cref="CreateDetectionFailure"/>.</para>
/// <para>Introduced in v0.20.7b. Coherent path — zero Corruption risk.</para>
/// </remarks>
public sealed record TrapMasteryResult
{
    /// <summary>
    /// The type of Trap Mastery action that was performed.
    /// </summary>
    public enum ResultType
    {
        /// <summary>
        /// A hunting trap was placed at a location.
        /// </summary>
        TrapPlaced,

        /// <summary>
        /// The area was scanned for nearby traps.
        /// </summary>
        TrapsDetected
    }

    /// <summary>
    /// Gets the type of action performed (placement or detection).
    /// </summary>
    public ResultType Type { get; init; }

    /// <summary>
    /// Gets the trap that was placed, or null if this is a detection result.
    /// </summary>
    public TrapInstance? PlacedTrap { get; init; }

    /// <summary>
    /// Gets the X coordinate of the action location.
    /// </summary>
    public int LocationX { get; init; }

    /// <summary>
    /// Gets the Y coordinate of the action location.
    /// </summary>
    public int LocationY { get; init; }

    /// <summary>
    /// Gets whether the action succeeded.
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// Gets the number of enemy traps detected (detection mode only).
    /// </summary>
    public int DetectedTrapsCount { get; init; }

    /// <summary>
    /// Gets descriptions of detected traps (detection mode only).
    /// </summary>
    public IReadOnlyList<string> DetectedTrapDescriptions { get; init; } = [];

    /// <summary>
    /// Gets a human-readable message describing the result.
    /// </summary>
    public string Message { get; init; } = string.Empty;

    /// <summary>
    /// Gets the perception roll made during detection (null for placement).
    /// </summary>
    public int? PerceptionRoll { get; init; }

    /// <summary>
    /// Gets the DC the perception roll was checked against (null for placement).
    /// </summary>
    public int? PerceptionDc { get; init; }

    /// <summary>
    /// Gets the total perception bonus applied during detection (null for placement).
    /// </summary>
    public int? PerceptionBonus { get; init; }

    /// <summary>
    /// Creates a successful trap placement result.
    /// </summary>
    /// <param name="trap">The trap instance that was placed.</param>
    /// <param name="x">The X coordinate where the trap was placed.</param>
    /// <param name="y">The Y coordinate where the trap was placed.</param>
    /// <returns>A <see cref="TrapMasteryResult"/> indicating successful placement.</returns>
    public static TrapMasteryResult CreatePlacementSuccess(TrapInstance trap, int x, int y)
    {
        return new TrapMasteryResult
        {
            Type = ResultType.TrapPlaced,
            PlacedTrap = trap,
            LocationX = x,
            LocationY = y,
            Success = true,
            Message = $"You carefully place a {trap.Type.ToString().ToLowerInvariant()} trap at ({x}, {y})."
        };
    }

    /// <summary>
    /// Creates a failed trap placement result.
    /// </summary>
    /// <param name="x">The X coordinate where placement was attempted.</param>
    /// <param name="y">The Y coordinate where placement was attempted.</param>
    /// <param name="reason">The reason the placement failed.</param>
    /// <returns>A <see cref="TrapMasteryResult"/> indicating failed placement.</returns>
    public static TrapMasteryResult CreatePlacementFailure(int x, int y, string reason)
    {
        return new TrapMasteryResult
        {
            Type = ResultType.TrapPlaced,
            LocationX = x,
            LocationY = y,
            Success = false,
            Message = reason
        };
    }

    /// <summary>
    /// Creates a successful trap detection result.
    /// </summary>
    /// <param name="count">The number of traps detected.</param>
    /// <param name="descriptions">Descriptions of each detected trap.</param>
    /// <param name="roll">The total perception roll (dice + bonuses).</param>
    /// <param name="dc">The DC the roll was checked against.</param>
    /// <param name="bonus">The total perception bonus applied.</param>
    /// <returns>A <see cref="TrapMasteryResult"/> indicating successful detection.</returns>
    public static TrapMasteryResult CreateDetectionSuccess(
        int count,
        List<string> descriptions,
        int roll,
        int dc,
        int bonus)
    {
        return new TrapMasteryResult
        {
            Type = ResultType.TrapsDetected,
            Success = true,
            DetectedTrapsCount = count,
            DetectedTrapDescriptions = descriptions.AsReadOnly(),
            PerceptionRoll = roll,
            PerceptionDc = dc,
            PerceptionBonus = bonus,
            Message = $"You detect {count} trap{(count != 1 ? "s" : "")} nearby."
        };
    }

    /// <summary>
    /// Creates a failed trap detection result.
    /// </summary>
    /// <param name="roll">The total perception roll (dice + bonuses).</param>
    /// <param name="dc">The DC the roll was checked against.</param>
    /// <param name="bonus">The total perception bonus applied.</param>
    /// <returns>A <see cref="TrapMasteryResult"/> indicating failed detection.</returns>
    public static TrapMasteryResult CreateDetectionFailure(int roll, int dc, int bonus)
    {
        return new TrapMasteryResult
        {
            Type = ResultType.TrapsDetected,
            Success = false,
            DetectedTrapsCount = 0,
            PerceptionRoll = roll,
            PerceptionDc = dc,
            PerceptionBonus = bonus,
            Message = "You don't sense any obvious traps nearby."
        };
    }

    /// <summary>
    /// Gets a narrative description of the Trap Mastery result for combat logging.
    /// </summary>
    /// <returns>A formatted string suitable for display in the combat log.</returns>
    public string GetDescription()
    {
        if (Type == ResultType.TrapPlaced)
        {
            return Success
                ? $"Trap placed: {PlacedTrap?.Type} at ({LocationX}, {LocationY}). {PlacedTrap?.GetDamageText()}"
                : $"Trap placement failed at ({LocationX}, {LocationY}): {Message}";
        }

        return Success
            ? $"Detection: {DetectedTrapsCount} trap{(DetectedTrapsCount != 1 ? "s" : "")} found " +
              $"(Roll: {PerceptionRoll} vs DC {PerceptionDc}, bonus +{PerceptionBonus})."
            : $"Detection: No traps found (Roll: {PerceptionRoll} vs DC {PerceptionDc}, bonus +{PerceptionBonus}).";
    }

    /// <summary>
    /// Returns whether the action succeeded.
    /// </summary>
    /// <returns>True if the placement or detection was successful.</returns>
    public bool WasSuccessful() => Success;

    /// <summary>
    /// Gets the total number of traps involved in the result.
    /// Returns 1 for successful placement, <see cref="DetectedTrapsCount"/> for detection.
    /// </summary>
    /// <returns>The trap count relevant to this result type.</returns>
    public int GetTrapCount()
    {
        return Type switch
        {
            ResultType.TrapPlaced => Success ? 1 : 0,
            ResultType.TrapsDetected => DetectedTrapsCount,
            _ => 0
        };
    }
}
