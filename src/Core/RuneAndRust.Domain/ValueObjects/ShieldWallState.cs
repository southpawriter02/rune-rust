// ═══════════════════════════════════════════════════════════════════════════════
// ShieldWallState.cs
// Immutable value object tracking the active state of a Shield Wall stance,
// including defense bonuses and ally detection logic.
// Version: 0.20.1a
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents the state of an active Shield Wall stance for the Skjaldmær.
/// </summary>
/// <remarks>
/// <para>
/// Shield Wall is a stance ability that grants +3 Defense to the Skjaldmær
/// and +1 Defense to each adjacent ally within 1 space (8-directional adjacency).
/// The stance persists until explicitly ended or the character is incapacitated.
/// </para>
/// <para>
/// This is an immutable value object. State transitions (activate/deactivate)
/// produce new instances via factory methods.
/// </para>
/// </remarks>
public sealed record ShieldWallState
{
    /// <summary>Default self-defense bonus when Shield Wall is active.</summary>
    public const int DefaultSelfDefenseBonus = 3;

    /// <summary>Default defense bonus granted to each adjacent ally.</summary>
    public const int DefaultAllyDefenseBonus = 1;

    /// <summary>Gets whether the Shield Wall stance is currently active.</summary>
    public bool IsActive { get; init; }

    /// <summary>Gets the defense bonus granted to self while active.</summary>
    public int SelfDefenseBonus { get; init; } = DefaultSelfDefenseBonus;

    /// <summary>Gets the defense bonus granted to each adjacent ally.</summary>
    public int AllyDefenseBonus { get; init; } = DefaultAllyDefenseBonus;

    /// <summary>Gets the position where Shield Wall was activated.</summary>
    public (int X, int Y) ActivatedPosition { get; init; }

    /// <summary>Gets the timestamp when Shield Wall was activated.</summary>
    public DateTime? ActivatedAt { get; init; }

    /// <summary>
    /// Creates an active Shield Wall state at the specified position.
    /// </summary>
    /// <param name="position">Grid position where the stance is activated.</param>
    /// <returns>An active ShieldWallState anchored to the given position.</returns>
    public static ShieldWallState Activate((int X, int Y) position) => new()
    {
        IsActive = true,
        ActivatedPosition = position,
        ActivatedAt = DateTime.UtcNow
    };

    /// <summary>
    /// Creates an inactive (deactivated) Shield Wall state.
    /// </summary>
    /// <returns>An inactive ShieldWallState.</returns>
    public static ShieldWallState Deactivate() => new()
    {
        IsActive = false,
        ActivatedAt = null
    };

    /// <summary>
    /// Gets the effective self-defense bonus (0 if inactive).
    /// </summary>
    public int GetEffectiveSelfBonus() => IsActive ? SelfDefenseBonus : 0;

    /// <summary>
    /// Gets the effective ally defense bonus (0 if inactive).
    /// </summary>
    public int GetEffectiveAllyBonus() => IsActive ? AllyDefenseBonus : 0;

    /// <summary>
    /// Determines which characters are adjacent (within 1 space, 8-directional)
    /// to the activated position.
    /// </summary>
    /// <param name="allPositions">
    /// Dictionary mapping character IDs to their (X, Y) positions.
    /// </param>
    /// <param name="selfId">The Skjaldmær's own character ID (excluded from results).</param>
    /// <returns>List of character IDs within adjacency range.</returns>
    public IReadOnlyList<Guid> GetAdjacentAllies(
        IReadOnlyDictionary<Guid, (int X, int Y)> allPositions,
        Guid selfId)
    {
        if (!IsActive)
            return Array.Empty<Guid>();

        var adjacent = new List<Guid>();
        foreach (var (charId, pos) in allPositions)
        {
            if (charId == selfId)
                continue;

            var dx = Math.Abs(pos.X - ActivatedPosition.X);
            var dy = Math.Abs(pos.Y - ActivatedPosition.Y);

            // 8-directional adjacency: Chebyshev distance <= 1
            if (dx <= 1 && dy <= 1)
                adjacent.Add(charId);
        }

        return adjacent.AsReadOnly();
    }

    /// <inheritdoc />
    public override string ToString() =>
        IsActive
            ? $"Shield Wall [ACTIVE] at ({ActivatedPosition.X}, {ActivatedPosition.Y}): " +
              $"+{SelfDefenseBonus} self, +{AllyDefenseBonus} allies"
            : "Shield Wall [INACTIVE]";
}
