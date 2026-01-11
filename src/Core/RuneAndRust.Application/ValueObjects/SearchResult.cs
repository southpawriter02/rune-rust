using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.ValueObjects;

/// <summary>
/// Result of a search action in a room.
/// </summary>
/// <param name="SkillCheck">The skill check result for the search.</param>
/// <param name="DiscoveredExits">Directions of newly discovered exits.</param>
/// <param name="DiscoveredItems">IDs of newly discovered items.</param>
/// <param name="NothingToFind">True if there was nothing hidden to find.</param>
public readonly record struct SearchResult(
    SkillCheckResult? SkillCheck,
    IReadOnlyList<Direction> DiscoveredExits,
    IReadOnlyList<Guid> DiscoveredItems,
    bool NothingToFind)
{
    /// <summary>
    /// Gets whether any discoveries were made.
    /// </summary>
    public bool FoundSomething => DiscoveredExits.Count > 0 || DiscoveredItems.Count > 0;

    /// <summary>
    /// Gets whether the search succeeded (check passed or nothing to find).
    /// </summary>
    public bool IsSuccess => NothingToFind || (SkillCheck?.IsSuccess ?? false);

    /// <summary>
    /// Creates a result for when there's nothing hidden in the room.
    /// </summary>
    public static SearchResult NothingHidden() => new(
        null,
        Array.Empty<Direction>(),
        Array.Empty<Guid>(),
        NothingToFind: true);

    /// <summary>
    /// Creates a result for a failed search attempt.
    /// </summary>
    public static SearchResult Failed(SkillCheckResult check) => new(
        check,
        Array.Empty<Direction>(),
        Array.Empty<Guid>(),
        NothingToFind: false);

    /// <summary>
    /// Creates a result for a successful search with discoveries.
    /// </summary>
    public static SearchResult Success(
        SkillCheckResult check,
        IReadOnlyList<Direction> exits,
        IReadOnlyList<Guid> items) => new(
        check,
        exits,
        items,
        NothingToFind: false);
}
