using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Defines the contract for Veiðimaðr (Hunter) Quarry Marks resource management.
/// Handles mark creation, removal, querying, and lifecycle operations.
/// </summary>
/// <remarks>
/// <para>The Quarry Marks service manages the Veiðimaðr's unique target-tracking resource.
/// Unlike <see cref="IBoneSetterMedicalSuppliesService"/> which manages a consumable inventory,
/// this service manages discrete target marks with FIFO replacement policy.</para>
/// <para>All methods are synchronous and take a <see cref="Player"/> parameter,
/// following the established specialization service pattern. No repository dependency —
/// all state is managed directly on the Player entity.</para>
/// <para>Introduced in v0.20.7a as part of the Veiðimaðr specialization framework.</para>
/// </remarks>
public interface IVeidimadurQuarryMarksService
{
    /// <summary>
    /// Initializes the Quarry Marks resource on the player with default settings (max 3 marks, 0 active).
    /// </summary>
    /// <param name="player">The player to initialize. Must be a Veiðimaðr specialist.</param>
    void InitializeQuarryMarks(Player player);

    /// <summary>
    /// Gets the current Quarry Marks resource for the player.
    /// </summary>
    /// <param name="player">The player whose marks to retrieve.</param>
    /// <returns>The <see cref="QuarryMarksResource"/>, or null if not initialized.</returns>
    QuarryMarksResource? GetQuarryMarks(Player player);

    /// <summary>
    /// Adds a new Quarry Mark to the player's resource. If at maximum capacity (3),
    /// the oldest mark is replaced via FIFO policy.
    /// </summary>
    /// <param name="player">The player adding the mark.</param>
    /// <param name="mark">The mark to add.</param>
    /// <returns>The replaced <see cref="QuarryMark"/> if FIFO replacement occurred, or null.</returns>
    QuarryMark? AddMark(Player player, QuarryMark mark);

    /// <summary>
    /// Removes a specific mark by target ID (e.g., when target is defeated or escapes).
    /// </summary>
    /// <param name="player">The player whose mark to remove.</param>
    /// <param name="targetId">The target ID whose mark should be removed.</param>
    /// <returns>True if a mark was found and removed; false otherwise.</returns>
    bool RemoveMark(Player player, Guid targetId);

    /// <summary>
    /// Checks if a target is currently marked with an active Quarry Mark.
    /// </summary>
    /// <param name="player">The player whose marks to check.</param>
    /// <param name="targetId">The target ID to check.</param>
    /// <returns>True if the target has an active Quarry Mark.</returns>
    bool HasActiveMark(Player player, Guid targetId);

    /// <summary>
    /// Gets a specific mark for a target.
    /// </summary>
    /// <param name="player">The player whose marks to search.</param>
    /// <param name="targetId">The target ID to look up.</param>
    /// <returns>The <see cref="QuarryMark"/> for the target, or null if not marked.</returns>
    QuarryMark? GetMarkFor(Player player, Guid targetId);

    /// <summary>
    /// Checks if another mark can be added without replacing an existing one.
    /// </summary>
    /// <param name="player">The player whose capacity to check.</param>
    /// <returns>True if the current mark count is below the maximum (3).</returns>
    bool CanAddMark(Player player);

    /// <summary>
    /// Clears all marks. Called at encounter end or when the resource is reset.
    /// </summary>
    /// <param name="player">The player whose marks to clear.</param>
    void ClearAllMarks(Player player);

    /// <summary>
    /// Gets the total number of active marks.
    /// </summary>
    /// <param name="player">The player whose mark count to retrieve.</param>
    /// <returns>The number of active marks (0–3).</returns>
    int GetMarkCount(Player player);
}
