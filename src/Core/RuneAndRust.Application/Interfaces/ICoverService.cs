using RuneAndRust.Domain.Definitions;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Service for cover calculations and management.
/// </summary>
/// <remarks>
/// <para>
/// Provides methods for calculating cover between attackers and targets,
/// managing cover objects on the grid, and handling destructible cover.
/// </para>
/// <para>
/// Cover mechanics:
/// <list type="bullet">
///   <item><description>Partial: +2 defense (configurable), target can still be attacked.</description></item>
///   <item><description>Full: Target cannot be attacked from the blocked angle.</description></item>
///   <item><description>Destructible: Can be damaged and removed when HP reaches 0.</description></item>
/// </list>
/// </para>
/// </remarks>
public interface ICoverService
{
    /// <summary>
    /// Gets the cover between two entities.
    /// </summary>
    /// <param name="attackerId">The attacker's entity ID.</param>
    /// <param name="targetId">The target's entity ID.</param>
    /// <returns>A result describing the cover situation.</returns>
    CoverCheckResult GetCoverBetween(Guid attackerId, Guid targetId);

    /// <summary>
    /// Gets the cover between two positions.
    /// </summary>
    /// <param name="attacker">The attacker's position.</param>
    /// <param name="target">The target's position.</param>
    /// <returns>A result describing the cover situation.</returns>
    CoverCheckResult GetCoverBetween(GridPosition attacker, GridPosition target);

    /// <summary>
    /// Gets the defense bonus provided by cover between two entities.
    /// </summary>
    /// <param name="attackerId">The attacker's entity ID.</param>
    /// <param name="targetId">The target's entity ID.</param>
    /// <returns>The defense bonus (0 if no cover or full cover).</returns>
    int GetDefenseBonus(Guid attackerId, Guid targetId);

    /// <summary>
    /// Checks if the target has full cover from the attacker.
    /// </summary>
    /// <param name="attackerId">The attacker's entity ID.</param>
    /// <param name="targetId">The target's entity ID.</param>
    /// <returns><c>true</c> if the target has full cover; otherwise, <c>false</c>.</returns>
    bool HasFullCover(Guid attackerId, Guid targetId);

    /// <summary>
    /// Adds cover to the grid at a position.
    /// </summary>
    /// <param name="definition">The cover definition to use.</param>
    /// <param name="position">The position for the cover.</param>
    /// <returns><c>true</c> if cover was added; otherwise, <c>false</c>.</returns>
    bool AddCover(CoverDefinition definition, GridPosition position);

    /// <summary>
    /// Removes cover from a position.
    /// </summary>
    /// <param name="position">The position to clear.</param>
    /// <returns><c>true</c> if cover was removed; otherwise, <c>false</c>.</returns>
    bool RemoveCover(GridPosition position);

    /// <summary>
    /// Applies damage to cover at a position.
    /// </summary>
    /// <param name="position">The position of the cover.</param>
    /// <param name="damage">The amount of damage to apply.</param>
    /// <returns>A result describing the damage outcome.</returns>
    CoverDamageResult DamageCover(GridPosition position, int damage);

    /// <summary>
    /// Gets the cover object at a position.
    /// </summary>
    /// <param name="position">The position to query.</param>
    /// <returns>The cover object, or null if none.</returns>
    CoverObject? GetCover(GridPosition position);

    /// <summary>
    /// Gets all cover objects protecting a position from an attacker.
    /// </summary>
    /// <param name="position">The position being protected.</param>
    /// <param name="attackerPosition">The attacker's position.</param>
    /// <returns>An enumerable of cover objects providing protection.</returns>
    IEnumerable<CoverObject> GetProtectingCover(GridPosition position, GridPosition attackerPosition);

    /// <summary>
    /// Gets all cover definitions.
    /// </summary>
    /// <returns>An enumerable of all loaded cover definitions.</returns>
    IEnumerable<CoverDefinition> GetAllCoverDefinitions();
}

/// <summary>
/// Result of checking cover between attacker and target.
/// </summary>
/// <param name="CoverType">The type of cover (None, Partial, Full).</param>
/// <param name="DefenseBonus">The defense bonus granted (0 for None or Full).</param>
/// <param name="CoverObject">The cover object providing protection, if any.</param>
/// <param name="IsBlocked">True if the attack is completely blocked (Full cover).</param>
/// <param name="Message">A descriptive message about the cover situation.</param>
public readonly record struct CoverCheckResult(
    CoverType CoverType,
    int DefenseBonus,
    CoverObject? CoverObject,
    bool IsBlocked,
    string Message)
{
    /// <summary>
    /// Gets a result indicating no cover.
    /// </summary>
    public static CoverCheckResult None { get; } = new(CoverType.None, 0, null, false, "No cover.");
}

/// <summary>
/// Result of damaging cover.
/// </summary>
/// <param name="DamageDealt">True if damage was applied.</param>
/// <param name="DamageAmount">The amount of damage dealt.</param>
/// <param name="RemainingHp">The remaining HP of the cover.</param>
/// <param name="Destroyed">True if the cover was destroyed.</param>
/// <param name="CoverName">The name of the cover object.</param>
/// <param name="Message">A descriptive message about the damage outcome.</param>
public readonly record struct CoverDamageResult(
    bool DamageDealt,
    int DamageAmount,
    int RemainingHp,
    bool Destroyed,
    string CoverName,
    string Message)
{
    /// <summary>
    /// Gets a result indicating no damage was dealt.
    /// </summary>
    public static CoverDamageResult None { get; } = new(false, 0, 0, false, "", "No cover at that position.");
}
