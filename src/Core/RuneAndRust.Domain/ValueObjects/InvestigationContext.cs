namespace RuneAndRust.Domain.ValueObjects;

using RuneAndRust.Domain.Enums;

/// <summary>
/// Represents the context of an ongoing investigation, tracking the target,
/// character, and all clues gathered during the investigation.
/// </summary>
/// <remarks>
/// <para>
/// InvestigationContext maintains state across the investigation process,
/// allowing clues to accumulate and deductions to become available as
/// sufficient evidence is gathered.
/// </para>
/// </remarks>
public sealed record InvestigationContext
{
    /// <summary>
    /// The type of target being investigated.
    /// </summary>
    public required InvestigationTarget TargetType { get; init; }

    /// <summary>
    /// The unique identifier of the target object or area.
    /// </summary>
    public required string TargetId { get; init; }

    /// <summary>
    /// The unique identifier of the investigating character.
    /// </summary>
    public required string CharacterId { get; init; }

    /// <summary>
    /// The room where the investigation is taking place.
    /// </summary>
    public required string RoomId { get; init; }

    /// <summary>
    /// Collection of clue identifiers gathered during this investigation.
    /// </summary>
    public required IReadOnlyList<string> CluesGathered { get; init; }

    /// <summary>
    /// Total time spent on this investigation, in minutes.
    /// </summary>
    public required int TimeSpent { get; init; }

    /// <summary>
    /// Collection of deductions that are now available based on gathered clues.
    /// </summary>
    public required IReadOnlyList<Deduction> DeductionsAvailable { get; init; }

    /// <summary>
    /// The base difficulty class for this investigation target.
    /// </summary>
    public required int BaseDc { get; init; }

    /// <summary>
    /// Indicates whether this investigation has been completed.
    /// </summary>
    public bool IsComplete { get; init; }

    /// <summary>
    /// Gets the total number of clues discovered.
    /// </summary>
    public int ClueCount => CluesGathered.Count;

    /// <summary>
    /// Gets whether any clues have been gathered.
    /// </summary>
    public bool HasClues => CluesGathered.Count > 0;

    /// <summary>
    /// Gets whether any deductions are available.
    /// </summary>
    public bool HasDeductions => DeductionsAvailable.Count > 0;

    /// <summary>
    /// Creates an empty investigation context.
    /// </summary>
    public static InvestigationContext Create(
        InvestigationTarget targetType,
        string targetId,
        string characterId,
        string roomId,
        int baseDc) =>
        new()
        {
            TargetType = targetType,
            TargetId = targetId,
            CharacterId = characterId,
            RoomId = roomId,
            CluesGathered = Array.Empty<string>(),
            TimeSpent = 0,
            DeductionsAvailable = Array.Empty<Deduction>(),
            BaseDc = baseDc
        };
}
