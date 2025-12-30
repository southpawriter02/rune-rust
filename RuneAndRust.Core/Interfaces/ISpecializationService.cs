using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Models;

namespace RuneAndRust.Core.Interfaces;

/// <summary>
/// Service for managing specialization unlocks and node purchases.
/// Handles PP spending, prerequisite validation, and event publishing.
/// </summary>
/// <remarks>See: v0.4.1b (The Unlock) for implementation.</remarks>
public interface ISpecializationService
{
    // ═══════════════════════════════════════════════════════════════════════
    // Specialization Unlock Operations
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Attempts to unlock a specialization for a character (10 PP cost).
    /// Validates archetype match, level requirements, and PP availability.
    /// </summary>
    /// <param name="character">The character attempting to unlock.</param>
    /// <param name="specId">The specialization ID to unlock.</param>
    /// <returns>Result indicating success/failure with details.</returns>
    Task<SpecializationUnlockResult> UnlockSpecializationAsync(Entities.Character character, Guid specId);

    /// <summary>
    /// Checks if a character can unlock a specific specialization.
    /// Does not modify state; used for UI pre-validation.
    /// </summary>
    /// <param name="character">The character to check.</param>
    /// <param name="specId">The specialization ID to check.</param>
    /// <returns>True if all requirements are met; otherwise false.</returns>
    Task<bool> CanUnlockSpecializationAsync(Entities.Character character, Guid specId);

    /// <summary>
    /// Gets the PP cost to unlock a specialization (constant: 10).
    /// </summary>
    /// <returns>The specialization unlock cost in PP.</returns>
    int GetSpecializationUnlockCost();

    // ═══════════════════════════════════════════════════════════════════════
    // Node Unlock Operations
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Attempts to unlock a specialization node by spending PP.
    /// Validates spec ownership, prerequisites, and PP availability.
    /// </summary>
    /// <param name="character">The character attempting to unlock.</param>
    /// <param name="nodeId">The node ID to unlock.</param>
    /// <returns>Result indicating success/failure with details.</returns>
    Task<NodeUnlockResult> UnlockNodeAsync(Entities.Character character, Guid nodeId);

    /// <summary>
    /// Checks if a character can unlock a specific node.
    /// Does not modify state; used for UI pre-validation.
    /// </summary>
    /// <param name="character">The character to check.</param>
    /// <param name="nodeId">The node ID to check.</param>
    /// <returns>True if all requirements are met; otherwise false.</returns>
    Task<bool> CanUnlockNodeAsync(Entities.Character character, Guid nodeId);

    /// <summary>
    /// Validates prerequisites for unlocking a node.
    /// Capstone (Tier 4) requires ALL Tier 3 nodes; others require parent nodes.
    /// </summary>
    /// <param name="character">The character to validate against.</param>
    /// <param name="node">The node to validate prerequisites for.</param>
    /// <returns>Tuple of (IsValid, FailureReason if invalid).</returns>
    Task<(bool IsValid, string? FailureReason)> ValidatePrerequisitesAsync(
        Entities.Character character, SpecializationNode node);

    // ═══════════════════════════════════════════════════════════════════════
    // Query Operations
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets all specializations available to a character based on archetype.
    /// </summary>
    /// <param name="character">The character to get specializations for.</param>
    /// <returns>All specializations matching the character's archetype.</returns>
    Task<IEnumerable<Specialization>> GetAvailableSpecializationsAsync(Entities.Character character);

    /// <summary>
    /// Gets all nodes for a specialization with unlock status.
    /// </summary>
    /// <param name="character">The character to check unlock status against.</param>
    /// <param name="specId">The specialization ID to get nodes for.</param>
    /// <returns>All nodes in the specialization tree.</returns>
    Task<IEnumerable<SpecializationNode>> GetNodesWithStatusAsync(Entities.Character character, Guid specId);
}
