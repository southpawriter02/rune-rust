namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents the Death Defiance passive ability state for a Berserkr character.
/// Tracks whether the once-per-combat death prevention has been triggered,
/// when it was triggered, and the amount of damage prevented.
/// </summary>
/// <remarks>
/// <para>Introduced in v0.20.5c as part of Berserkr Tier 3 Abilities.</para>
/// <para>Death Defiance is a passive ability that is always active once learned:</para>
/// <list type="bullet">
/// <item>When damage would reduce HP to 0 or below, the character stays at 1 HP instead</item>
/// <item>Grants +20 Rage when triggered (primal survival instinct fuels fury)</item>
/// <item>Can only trigger once per combat encounter</item>
/// <item>Resets on rest or new combat encounter</item>
/// <item>No Corruption risk — represents survival instinct, not Heretical power</item>
/// </list>
/// <para>Uses <c>private set</c> for mutable tracking properties to maintain
/// the value object pattern while allowing per-combat state updates.</para>
/// </remarks>
public sealed record DeathDefianceState
{
    /// <summary>
    /// Rage granted when Death Defiance triggers.
    /// The near-death experience fuels a surge of desperate fury.
    /// </summary>
    public const int RageGainOnTrigger = 20;

    /// <summary>
    /// Unique identifier for this state instance.
    /// </summary>
    public Guid StateId { get; init; } = Guid.NewGuid();

    /// <summary>
    /// Character who has the Death Defiance passive ability.
    /// </summary>
    public Guid CharacterId { get; init; }

    /// <summary>
    /// Whether Death Defiance has already been triggered this combat encounter.
    /// Once triggered, it cannot activate again until the next combat or rest.
    /// </summary>
    public bool HasTriggeredThisCombat { get; private set; }

    /// <summary>
    /// UTC timestamp when Death Defiance was triggered.
    /// Null if it has not yet triggered this combat.
    /// </summary>
    public DateTime? TriggeredAt { get; private set; }

    /// <summary>
    /// Amount of lethal damage that was prevented when Death Defiance triggered.
    /// Zero if the ability has not yet triggered this combat.
    /// </summary>
    public int DamagePreventedOnTrigger { get; private set; }

    /// <summary>
    /// Creates a new <see cref="DeathDefianceState"/> for the specified character.
    /// The state is initialized as ready (not yet triggered).
    /// </summary>
    /// <param name="characterId">The character gaining the Death Defiance passive.</param>
    /// <returns>A new state instance initialized and ready to trigger.</returns>
    public static DeathDefianceState Create(Guid characterId)
    {
        return new DeathDefianceState
        {
            CharacterId = characterId,
            HasTriggeredThisCombat = false,
            TriggeredAt = null,
            DamagePreventedOnTrigger = 0
        };
    }

    /// <summary>
    /// Checks if Death Defiance can still trigger this combat.
    /// Returns true only if the ability has not yet been used this encounter.
    /// </summary>
    /// <returns>True if the ability is available to prevent death; false if already used.</returns>
    public bool CanTrigger() => !HasTriggeredThisCombat;

    /// <summary>
    /// Records Death Defiance activation with the amount of damage that would have killed
    /// the character. Marks the ability as used for this combat encounter.
    /// </summary>
    /// <param name="damageWouldHaveCaused">
    /// The total damage that would have reduced the character to 0 or below HP.
    /// </param>
    /// <remarks>
    /// After triggering, <see cref="HasTriggeredThisCombat"/> is set to true and
    /// the ability cannot trigger again until <see cref="ResetForNewCombat"/> is called.
    /// </remarks>
    public void Trigger(int damageWouldHaveCaused)
    {
        HasTriggeredThisCombat = true;
        TriggeredAt = DateTime.UtcNow;
        DamagePreventedOnTrigger = damageWouldHaveCaused;
    }

    /// <summary>
    /// Resets Death Defiance for a new combat encounter or after resting.
    /// Clears the triggered state, allowing the ability to activate again.
    /// </summary>
    public void ResetForNewCombat()
    {
        HasTriggeredThisCombat = false;
        TriggeredAt = null;
        DamagePreventedOnTrigger = 0;
    }

    /// <summary>
    /// Gets a human-readable description of the Death Defiance state.
    /// </summary>
    /// <returns>
    /// A formatted string indicating whether the ability is ready, has been used,
    /// or has been reset for a new combat.
    /// </returns>
    public string GetDescription()
    {
        if (HasTriggeredThisCombat)
        {
            return $"Death Defiance (USED) - Triggered at {TriggeredAt:HH:mm:ss}, " +
                   $"prevented {DamagePreventedOnTrigger} damage";
        }

        return "Death Defiance (Ready) - Will prevent death once this combat";
    }
}
