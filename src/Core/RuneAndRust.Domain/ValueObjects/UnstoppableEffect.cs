using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents the Unstoppable active ability effect for a Berserkr character.
/// Grants immunity to all movement penalties for its duration (2 turns).
/// </summary>
/// <remarks>
/// <para>Introduced in v0.20.5b as part of Berserkr Tier 2 Abilities.</para>
/// <para>Unstoppable costs 1 AP and 15 Rage to activate. While active:</para>
/// <list type="bullet">
/// <item>Ignores all <see cref="MovementPenaltyType"/> penalties</item>
/// <item>Grants advantage on saves against forced movement</item>
/// <item>Lasts for 2 turns, automatically expiring</item>
/// <item>Cannot be used again until the current effect expires</item>
/// </list>
/// <para>If activated at 80+ Rage, triggers +1 Corruption.</para>
/// <para>Uses <c>private set</c> for <see cref="TurnsRemaining"/> to allow
/// turn-by-turn countdown via <see cref="Tick"/>.</para>
/// </remarks>
public sealed record UnstoppableEffect
{
    /// <summary>
    /// Default duration of the Unstoppable effect in turns.
    /// </summary>
    private const int DefaultDuration = 2;

    /// <summary>
    /// Unique identifier for this effect instance.
    /// </summary>
    public Guid EffectId { get; init; } = Guid.NewGuid();

    /// <summary>
    /// Character affected by the Unstoppable effect.
    /// </summary>
    public Guid CharacterId { get; init; }

    /// <summary>
    /// UTC timestamp when the effect was activated.
    /// </summary>
    public DateTime StartedAt { get; init; }

    /// <summary>
    /// Turns remaining before the effect expires.
    /// Decremented each turn via <see cref="Tick"/>.
    /// Starts at <see cref="DefaultDuration"/> (2).
    /// </summary>
    public int TurnsRemaining { get; private set; } = DefaultDuration;

    /// <summary>
    /// List of movement penalty types this effect ignores.
    /// Includes all defined <see cref="MovementPenaltyType"/> values.
    /// </summary>
    public IReadOnlyList<MovementPenaltyType> MovementPenaltiesIgnored { get; init; }
        = new[]
        {
            MovementPenaltyType.DifficultTerrain,
            MovementPenaltyType.Slow,
            MovementPenaltyType.Root,
            MovementPenaltyType.Water,
            MovementPenaltyType.Entangle,
            MovementPenaltyType.ForcedMovement
        };

    /// <summary>
    /// Creates a new <see cref="UnstoppableEffect"/> for the specified character.
    /// </summary>
    /// <param name="characterId">The character activating Unstoppable.</param>
    /// <returns>A new effect initialized with 2 turns remaining.</returns>
    public static UnstoppableEffect Create(Guid characterId)
    {
        return new UnstoppableEffect
        {
            CharacterId = characterId,
            StartedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Decrements the remaining duration by one turn.
    /// Duration cannot go below zero.
    /// </summary>
    public void Tick()
    {
        TurnsRemaining = Math.Max(0, TurnsRemaining - 1);
    }

    /// <summary>
    /// Checks if the effect is still active (turns remaining greater than zero).
    /// </summary>
    /// <returns>True if the effect has not yet expired.</returns>
    public bool IsActive() => TurnsRemaining > 0;

    /// <summary>
    /// Checks if a specific movement penalty type is ignored by this effect.
    /// </summary>
    /// <param name="penalty">The movement penalty type to check.</param>
    /// <returns>True if the penalty is in the ignored list.</returns>
    public bool IgnoresPenalty(MovementPenaltyType penalty)
        => MovementPenaltiesIgnored.Contains(penalty);

    /// <summary>
    /// Gets a human-readable description of the effect and remaining duration.
    /// </summary>
    /// <returns>A formatted string listing turns remaining and penalties ignored.</returns>
    public string GetDescription()
    {
        var penalties = string.Join(", ", MovementPenaltiesIgnored);
        return $"Unstoppable ({TurnsRemaining} turns): Ignores {penalties}";
    }
}
