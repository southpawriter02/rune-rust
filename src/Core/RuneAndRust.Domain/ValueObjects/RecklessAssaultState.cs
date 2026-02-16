namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents the Reckless Assault stance state for a Berserkr character.
/// Tracks the duration, bonuses, penalties, and Corruption accrued while
/// the stance is active.
/// </summary>
/// <remarks>
/// <para>Introduced in v0.20.5b as part of Berserkr Tier 2 Abilities.</para>
/// <para>Reckless Assault is a toggle stance that costs 1 AP to enter and can be
/// exited as a free action. While active:</para>
/// <list type="bullet">
/// <item>+4 base Attack bonus (+1 per 20 Rage, max +9 at 100 Rage)</item>
/// <item>-2 Defense penalty</item>
/// <item>+1 Corruption per turn while at 80+ Rage</item>
/// </list>
/// <para>Uses <c>private set</c> for mutable tracking properties to maintain
/// the value object pattern while allowing turn-by-turn state updates.</para>
/// </remarks>
public sealed record RecklessAssaultState
{
    /// <summary>
    /// Base attack bonus granted by the Reckless Assault stance.
    /// </summary>
    private const int BaseAttackBonusValue = 4;

    /// <summary>
    /// Defense penalty applied while in the Reckless Assault stance.
    /// </summary>
    private const int DefensePenaltyValue = -2;

    /// <summary>
    /// Rage threshold at which per-turn Corruption is generated.
    /// </summary>
    private const int EnragedThreshold = 80;

    /// <summary>
    /// Rage divisor for scaling attack bonus (+1 per this many Rage points).
    /// </summary>
    private const int RageScalingDivisor = 20;

    /// <summary>
    /// Unique identifier for this stance instance.
    /// </summary>
    public Guid StateId { get; init; } = Guid.NewGuid();

    /// <summary>
    /// Character who has entered the Reckless Assault stance.
    /// </summary>
    public Guid CharacterId { get; init; }

    /// <summary>
    /// UTC timestamp when the stance was activated.
    /// </summary>
    public DateTime StartedAt { get; init; }

    /// <summary>
    /// Current number of turns the stance has been active.
    /// Incremented each turn via <see cref="Tick"/>.
    /// </summary>
    public int TurnsActive { get; private set; }

    /// <summary>
    /// Gets the base attack bonus from the stance (+4).
    /// Does not include Rage scaling; use <see cref="GetCurrentAttackBonus"/> for total.
    /// </summary>
    public int BaseAttackBonus => BaseAttackBonusValue;

    /// <summary>
    /// Gets the Defense penalty from the stance (-2).
    /// </summary>
    public int DefensePenalty => DefensePenaltyValue;

    /// <summary>
    /// Cumulative Corruption gained while in this stance.
    /// Incremented each turn the character remains at 80+ Rage.
    /// </summary>
    public int CorruptionAccrued { get; private set; }

    /// <summary>
    /// Creates a new <see cref="RecklessAssaultState"/> for the specified character.
    /// </summary>
    /// <param name="characterId">The character entering the stance.</param>
    /// <returns>A new stance state initialized at turn 0 with zero Corruption.</returns>
    public static RecklessAssaultState Create(Guid characterId)
    {
        return new RecklessAssaultState
        {
            CharacterId = characterId,
            StartedAt = DateTime.UtcNow,
            TurnsActive = 0,
            CorruptionAccrued = 0
        };
    }

    /// <summary>
    /// Gets the total attack bonus including Rage scaling.
    /// Formula: BaseAttackBonus (4) + (currentRage / 20).
    /// </summary>
    /// <param name="currentRage">Current Rage value of the character.</param>
    /// <returns>
    /// Total attack bonus (4 at 0 Rage, up to 9 at 100 Rage).
    /// </returns>
    /// <example>
    /// At 40 Rage: 4 + (40/20) = 4 + 2 = +6.
    /// At 80 Rage: 4 + (80/20) = 4 + 4 = +8.
    /// At 100 Rage: 4 + (100/20) = 4 + 5 = +9.
    /// </example>
    public int GetCurrentAttackBonus(int currentRage)
    {
        var rageBonus = currentRage / RageScalingDivisor;
        return BaseAttackBonusValue + rageBonus;
    }

    /// <summary>
    /// Checks if the stance would generate Corruption this turn at the given Rage level.
    /// Corruption is generated each turn while Rage is at or above 80 (Enraged).
    /// </summary>
    /// <param name="currentRage">Current Rage value.</param>
    /// <returns>True if Rage is 80 or above (Enraged threshold).</returns>
    public bool GeneratesCorruptionThisTurn(int currentRage) => currentRage >= EnragedThreshold;

    /// <summary>
    /// Advances the stance by one turn, optionally recording Corruption generated.
    /// Should be called at the end of each turn while the stance is active.
    /// </summary>
    /// <param name="corruptionGenerated">
    /// Corruption generated this turn (0 if Rage is below 80, 1 if at or above 80).
    /// </param>
    public void Tick(int corruptionGenerated = 0)
    {
        TurnsActive++;
        CorruptionAccrued += Math.Max(0, corruptionGenerated);
    }

    /// <summary>
    /// Ends the Reckless Assault stance early.
    /// The stance can be exited as a free action at any time.
    /// </summary>
    /// <remarks>
    /// No special cleanup logic is needed; the state is simply discarded
    /// by the owning service. This method exists for semantic clarity.
    /// </remarks>
    public void End()
    {
        // Stance is ended by the service discarding this state instance.
        // No internal cleanup needed.
    }

    /// <summary>
    /// Checks if the stance is currently active.
    /// Always returns true while the state object exists; the owning service
    /// controls lifecycle by creating/discarding the state.
    /// </summary>
    /// <returns>True (always active while held).</returns>
    public bool IsActive() => true;

    /// <summary>
    /// Gets a human-readable description of the stance and its current effects.
    /// </summary>
    /// <param name="currentRage">Current Rage value for bonus calculation.</param>
    /// <returns>
    /// A formatted string showing attack bonus, defense penalty, and Corruption risk.
    /// </returns>
    public string GetDescription(int currentRage)
    {
        var attackBonus = GetCurrentAttackBonus(currentRage);
        var corruptionRisk = GeneratesCorruptionThisTurn(currentRage) ? " [Corruption Risk]" : "";
        return $"Reckless Assault: +{attackBonus} Attack, {DefensePenaltyValue} Defense{corruptionRisk}";
    }
}
