using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Service for managing threatened squares and opportunity attacks.
/// </summary>
/// <remarks>
/// Tracks which entities threaten which squares, manages the reaction economy,
/// and handles the disengage action that allows movement without triggering attacks.
/// </remarks>
public interface IThreatService
{
    // ===== Threatened Square Queries =====

    /// <summary>
    /// Gets all squares threatened by an entity.
    /// </summary>
    /// <param name="entityId">The entity that is threatening.</param>
    /// <returns>All positions the entity threatens (typically 8 adjacent squares for melee).</returns>
    /// <remarks>
    /// Only entities with melee weapons threaten squares.
    /// </remarks>
    IEnumerable<GridPosition> GetThreatenedSquares(Guid entityId);

    /// <summary>
    /// Gets all entities threatening a position.
    /// </summary>
    /// <param name="position">The position to check.</param>
    /// <returns>IDs of all entities that threaten this position.</returns>
    IEnumerable<Guid> GetThreateningEntities(GridPosition position);

    /// <summary>
    /// Checks if a position is threatened by an enemy of the moving entity.
    /// </summary>
    /// <param name="position">The position to check.</param>
    /// <param name="movingEntityId">The entity that would be at this position.</param>
    /// <returns><c>true</c> if any enemy threatens this position.</returns>
    bool IsPositionThreatened(GridPosition position, Guid movingEntityId);

    // ===== Opportunity Attack Methods =====

    /// <summary>
    /// Checks if moving from one position to another will trigger opportunity attacks.
    /// </summary>
    /// <param name="movingEntityId">The entity attempting to move.</param>
    /// <param name="from">The starting position.</param>
    /// <param name="to">The destination position.</param>
    /// <returns>A result indicating if OAs will trigger and who will attack.</returns>
    OpportunityAttackCheckResult CheckOpportunityAttacks(Guid movingEntityId, GridPosition from, GridPosition to);

    /// <summary>
    /// Executes all opportunity attacks triggered by movement.
    /// </summary>
    /// <param name="movingEntityId">The entity that is moving.</param>
    /// <param name="from">The starting position.</param>
    /// <param name="to">The destination position.</param>
    /// <returns>Results of each opportunity attack executed.</returns>
    IEnumerable<OpportunityAttackResult> ExecuteOpportunityAttacks(Guid movingEntityId, GridPosition from, GridPosition to);

    // ===== Reaction System =====

    /// <summary>
    /// Checks if an entity has already used their reaction this round.
    /// </summary>
    /// <param name="entityId">The entity to check.</param>
    /// <returns><c>true</c> if the entity has used their reaction.</returns>
    bool HasUsedReaction(Guid entityId);

    /// <summary>
    /// Marks an entity as having used their reaction.
    /// </summary>
    /// <param name="entityId">The entity that used their reaction.</param>
    void UseReaction(Guid entityId);

    /// <summary>
    /// Resets all reactions at the start of a new round.
    /// </summary>
    void ResetReactions();

    // ===== Disengage System =====

    /// <summary>
    /// Checks if an entity is currently disengaging.
    /// </summary>
    /// <param name="entityId">The entity to check.</param>
    /// <returns><c>true</c> if the entity is disengaging this turn.</returns>
    bool IsDisengaging(Guid entityId);

    /// <summary>
    /// Marks an entity as disengaging for the current turn.
    /// </summary>
    /// <param name="entityId">The entity that is disengaging.</param>
    void SetDisengaging(Guid entityId);

    /// <summary>
    /// Clears the disengage status for an entity (end of turn).
    /// </summary>
    /// <param name="entityId">The entity to clear.</param>
    void ClearDisengaging(Guid entityId);

    /// <summary>
    /// Clears all disengage statuses (end of round).
    /// </summary>
    void ClearAllDisengaging();
}

/// <summary>
/// Result of checking whether opportunity attacks will trigger.
/// </summary>
/// <param name="TriggersOpportunityAttacks">Whether any OAs will trigger.</param>
/// <param name="AttackingEntities">List of entities that will make opportunity attacks.</param>
/// <param name="IsDisengaging">Whether the moving entity is disengaging.</param>
/// <param name="Message">Human-readable message describing the result.</param>
public readonly record struct OpportunityAttackCheckResult(
    bool TriggersOpportunityAttacks,
    IReadOnlyList<Guid> AttackingEntities,
    bool IsDisengaging,
    string Message)
{
    /// <summary>
    /// Creates a result indicating no opportunity attacks will trigger.
    /// </summary>
    public static OpportunityAttackCheckResult None(string message = "No opportunity attacks triggered.") =>
        new(false, Array.Empty<Guid>(), false, message);

    /// <summary>
    /// Creates a result indicating the entity is disengaging.
    /// </summary>
    public static OpportunityAttackCheckResult Disengaging() =>
        new(false, Array.Empty<Guid>(), true, "You are disengaging - no opportunity attacks.");
}
