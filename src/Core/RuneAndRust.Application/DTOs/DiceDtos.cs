using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.DTOs;

/// <summary>
/// DTO for dice roll results to be rendered in the presentation layer.
/// </summary>
public record DiceRollDto
{
    /// <summary>Dice notation (e.g., "3d6+5").</summary>
    public required string Notation { get; init; }

    /// <summary>Individual dice results.</summary>
    public required IReadOnlyList<int> Rolls { get; init; }

    /// <summary>Explosion roll results.</summary>
    public required IReadOnlyList<int> ExplosionRolls { get; init; }

    /// <summary>Modifier applied to the roll.</summary>
    public required int Modifier { get; init; }

    /// <summary>Total of dice only (before modifier).</summary>
    public required int DiceTotal { get; init; }

    /// <summary>Final total including modifier.</summary>
    public required int Total { get; init; }

    /// <summary>True if first die rolled maximum.</summary>
    public required bool IsNaturalMax { get; init; }

    /// <summary>True if first die rolled 1.</summary>
    public required bool IsNaturalOne { get; init; }

    /// <summary>True if any dice exploded.</summary>
    public required bool HadExplosions { get; init; }

    /// <summary>Advantage type used.</summary>
    public required string AdvantageType { get; init; }

    /// <summary>All roll totals for advantage/disadvantage.</summary>
    public IReadOnlyList<int>? AllRollTotals { get; init; }

    /// <summary>Optional flavor text descriptor.</summary>
    public string? Descriptor { get; init; }

    /// <summary>
    /// Creates a DTO from a domain DiceRollResult.
    /// </summary>
    /// <param name="result">The domain result.</param>
    /// <param name="descriptor">Optional flavor text.</param>
    /// <returns>A DTO for presentation.</returns>
    public static DiceRollDto FromDomainResult(DiceRollResult result, string? descriptor = null)
    {
        // Use new success-counting properties for critical detection
        // but fall back to legacy properties for sum-based display
#pragma warning disable CS0618 // Legacy properties still used for display
        return new DiceRollDto
        {
            Notation = result.Pool.ToString(),
            Rolls = result.Rolls,
            ExplosionRolls = result.ExplosionRolls,
            Modifier = result.Pool.Modifier,
            DiceTotal = result.DiceTotal,
            Total = result.Total,
            IsNaturalMax = result.IsNaturalMax,
            IsNaturalOne = result.IsNaturalOne,
            HadExplosions = result.HadExplosions,
            AdvantageType = result.AdvantageType.ToString(),
            AllRollTotals = result.AdvantageType != Domain.Enums.AdvantageType.Normal ? result.AllRollTotals : null,
            Descriptor = descriptor
        };
#pragma warning restore CS0618
    }
}

/// <summary>
/// DTO for dice pool configuration.
/// </summary>
public record DicePoolDto
{
    /// <summary>Unique identifier for this pool configuration.</summary>
    public required string Id { get; init; }

    /// <summary>Display name.</summary>
    public required string Name { get; init; }

    /// <summary>Dice notation.</summary>
    public required string Notation { get; init; }

    /// <summary>Whether exploding is enabled.</summary>
    public required bool Exploding { get; init; }

    /// <summary>Maximum explosion count.</summary>
    public required int MaxExplosions { get; init; }

    /// <summary>Optional description.</summary>
    public string? Description { get; init; }

    /// <summary>
    /// Creates a DTO from a domain DicePool.
    /// </summary>
    /// <param name="id">Identifier for the pool.</param>
    /// <param name="name">Display name.</param>
    /// <param name="pool">The domain pool.</param>
    /// <param name="description">Optional description.</param>
    /// <returns>A DTO for presentation.</returns>
    public static DicePoolDto FromDomain(string id, string name, DicePool pool, string? description = null)
    {
        return new DicePoolDto
        {
            Id = id,
            Name = name,
            Notation = pool.ToString(),
            Exploding = pool.Exploding,
            MaxExplosions = pool.MaxExplosions,
            Description = description
        };
    }
}
