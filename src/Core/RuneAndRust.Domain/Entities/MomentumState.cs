namespace RuneAndRust.Domain.Entities;

using RuneAndRust.Domain.Enums;

/// <summary>
/// Represents the momentum state of a Storm Blade character.
/// </summary>
/// <remarks>
/// <para>
/// MomentumState is the core domain entity for the Momentum system.
/// It represents a flow-state resource earned through successful attacks
/// and lost through misses, stuns, or lack of action. Momentum rewards
/// sustained aggression and punishes interruption.
/// </para>
/// <para>
/// Key Properties:
/// <list type="bullet">
/// <item>CurrentMomentum: 0-100 integer</item>
/// <item>Threshold: Computed from CurrentMomentum</item>
/// <item>BonusAttacks: 0/0/1/1/2 depending on threshold</item>
/// <item>MovementBonus: floor(CurrentMomentum / 20)</item>
/// <item>AttackBonus: 0-4 by threshold</item>
/// <item>DefenseBonus: 0-4 by threshold</item>
/// <item>CriticalChance: +10% only at Unstoppable</item>
/// <item>ConsecutiveHits: Tracks chain for bonus calculation</item>
/// </list>
/// </para>
/// </remarks>
public class MomentumState
{
    #region Constants

    /// <summary>Minimum momentum value.</summary>
    public const int MinMomentum = 0;

    /// <summary>Maximum momentum value.</summary>
    public const int MaxMomentum = 100;

    /// <summary>Momentum threshold for Moving state (>20).</summary>
    public const int MovingThreshold = 20;

    /// <summary>Momentum threshold for Flowing state (>40).</summary>
    public const int FlowingThreshold = 40;

    /// <summary>Momentum threshold for Surging state (>60).</summary>
    public const int SurgingThreshold = 60;

    /// <summary>Momentum threshold for Unstoppable state (>80).</summary>
    public const int UnstoppableThreshold = 80;

    /// <summary>Momentum decay on missed attack.</summary>
    public const int DecayOnMiss = 25;

    /// <summary>Momentum decay on stun (full reset).</summary>
    public const int DecayOnStun = 100;

    /// <summary>Momentum decay on idle turn (no attack action).</summary>
    public const int DecayOnIdleTurn = 15;

    /// <summary>Momentum gain from successful attack.</summary>
    public const int GainPerSuccessfulAttack = 10;

    /// <summary>Momentum gain from killing blow.</summary>
    public const int GainPerKill = 20;

    /// <summary>Chain bonus per consecutive hit (multiplier × ConsecutiveHits).</summary>
    public const int ChainBonusPerHit = 5;

    /// <summary>Critical hit chance percentage bonus at Unstoppable threshold.</summary>
    public const int UnstoppableCritBonus = 10;

    #endregion

    #region Properties

    /// <summary>
    /// Gets the character ID this momentum state belongs to.
    /// </summary>
    public Guid CharacterId { get; private init; }

    /// <summary>
    /// Gets the current momentum level (0-100).
    /// </summary>
    public int CurrentMomentum { get; private init; }

    /// <summary>
    /// Gets the current momentum threshold.
    /// </summary>
    public MomentumThreshold Threshold { get; private init; }

    /// <summary>
    /// Gets the number of bonus attacks from momentum.
    /// </summary>
    /// <remarks>
    /// Scaling by threshold:
    /// <list type="bullet">
    /// <item>Stationary/Moving: 0 bonus attacks</item>
    /// <item>Flowing/Surging: 1 bonus attack</item>
    /// <item>Unstoppable: 2 bonus attacks</item>
    /// </list>
    /// </remarks>
    public int BonusAttacks { get; private init; }

    /// <summary>
    /// Gets the movement bonus from momentum.
    /// </summary>
    /// <remarks>
    /// Calculated as floor(CurrentMomentum / 20).
    /// Ranges from 0 to 5 extra squares of movement.
    /// </remarks>
    public int MovementBonus { get; private init; }

    /// <summary>
    /// Gets the defense bonus from momentum.
    /// </summary>
    /// <remarks>
    /// Scales with threshold: 0/1/2/3/4
    /// </remarks>
    public int DefenseBonus { get; private init; }

    /// <summary>
    /// Gets the attack bonus from momentum.
    /// </summary>
    /// <remarks>
    /// Scales with threshold: 0/1/2/3/4
    /// </remarks>
    public int AttackBonus { get; private init; }

    /// <summary>
    /// Gets the critical hit chance bonus.
    /// </summary>
    /// <remarks>
    /// +10% only at Unstoppable threshold.
    /// Null at all other thresholds.
    /// </remarks>
    public int? CriticalChance { get; private init; }

    /// <summary>
    /// Gets the count of consecutive successful attacks.
    /// </summary>
    /// <remarks>
    /// Resets to 0 on miss, stun, or end of turn with no attack.
    /// Used to calculate chain bonuses (ConsecutiveHits × 5).
    /// </remarks>
    public int ConsecutiveHits { get; private init; }

    /// <summary>
    /// Gets the time of last action (attack or movement).
    /// </summary>
    /// <remarks>
    /// Used to determine idle decay eligibility.
    /// </remarks>
    public DateTime? LastActionTime { get; private init; }

    #endregion

    #region Constructors

    /// <summary>
    /// Private constructor for EF Core.
    /// </summary>
    private MomentumState() { }

    #endregion

    #region Factory Methods

    /// <summary>
    /// Creates a new MomentumState for a character.
    /// </summary>
    /// <param name="characterId">The character ID.</param>
    /// <param name="initialMomentum">Initial momentum value (default 0).</param>
    /// <param name="consecutiveHits">Initial consecutive hits count (default 0).</param>
    /// <param name="lastActionTime">Optional last action time for decay tracking.</param>
    /// <returns>A new MomentumState instance.</returns>
    /// <exception cref="ArgumentException">Thrown when characterId is empty.</exception>
    public static MomentumState Create(
        Guid characterId,
        int initialMomentum = 0,
        int consecutiveHits = 0,
        DateTime? lastActionTime = null)
    {
        if (characterId == Guid.Empty)
        {
            throw new ArgumentException("CharacterId cannot be empty.", nameof(characterId));
        }

        var clampedMomentum = Math.Clamp(initialMomentum, MinMomentum, MaxMomentum);
        var clampedHits = Math.Max(0, consecutiveHits);
        var threshold = DetermineThreshold(clampedMomentum);

        return new MomentumState
        {
            CharacterId = characterId,
            CurrentMomentum = clampedMomentum,
            Threshold = threshold,
            BonusAttacks = CalculateBonusAttacks(threshold),
            MovementBonus = CalculateMovementBonus(clampedMomentum),
            DefenseBonus = CalculateDefenseBonus(threshold),
            AttackBonus = CalculateAttackBonus(threshold),
            CriticalChance = threshold == MomentumThreshold.Unstoppable ? UnstoppableCritBonus : null,
            ConsecutiveHits = clampedHits,
            LastActionTime = lastActionTime
        };
    }

    /// <summary>
    /// Determines the momentum threshold for a given momentum value.
    /// </summary>
    /// <param name="momentum">The momentum value (0-100).</param>
    /// <returns>The corresponding MomentumThreshold.</returns>
    public static MomentumThreshold DetermineThreshold(int momentum) =>
        momentum switch
        {
            > UnstoppableThreshold => MomentumThreshold.Unstoppable,
            > SurgingThreshold => MomentumThreshold.Surging,
            > FlowingThreshold => MomentumThreshold.Flowing,
            > MovingThreshold => MomentumThreshold.Moving,
            _ => MomentumThreshold.Stationary
        };

    #endregion

    #region Private Calculation Methods

    /// <summary>
    /// Calculates bonus attacks from threshold.
    /// </summary>
    /// <param name="threshold">Current momentum threshold.</param>
    /// <returns>Number of bonus attacks (0-2).</returns>
    private static int CalculateBonusAttacks(MomentumThreshold threshold) =>
        threshold switch
        {
            MomentumThreshold.Stationary => 0,
            MomentumThreshold.Moving => 0,
            MomentumThreshold.Flowing => 1,
            MomentumThreshold.Surging => 1,
            MomentumThreshold.Unstoppable => 2,
            _ => 0
        };

    /// <summary>
    /// Calculates movement bonus from momentum.
    /// </summary>
    /// <param name="momentum">Current momentum value.</param>
    /// <returns>Movement bonus (0-5).</returns>
    private static int CalculateMovementBonus(int momentum) =>
        momentum / 20;

    /// <summary>
    /// Calculates defense bonus from threshold.
    /// </summary>
    /// <param name="threshold">Current momentum threshold.</param>
    /// <returns>Defense bonus (0-4).</returns>
    private static int CalculateDefenseBonus(MomentumThreshold threshold) =>
        threshold switch
        {
            MomentumThreshold.Stationary => 0,
            MomentumThreshold.Moving => 1,
            MomentumThreshold.Flowing => 2,
            MomentumThreshold.Surging => 3,
            MomentumThreshold.Unstoppable => 4,
            _ => 0
        };

    /// <summary>
    /// Calculates attack bonus from threshold.
    /// </summary>
    /// <param name="threshold">Current momentum threshold.</param>
    /// <returns>Attack bonus (0-4).</returns>
    private static int CalculateAttackBonus(MomentumThreshold threshold) =>
        threshold switch
        {
            MomentumThreshold.Stationary => 0,
            MomentumThreshold.Moving => 1,
            MomentumThreshold.Flowing => 2,
            MomentumThreshold.Surging => 3,
            MomentumThreshold.Unstoppable => 4,
            _ => 0
        };

    #endregion

    #region Display

    /// <inheritdoc/>
    public override string ToString() =>
        $"Momentum[{Threshold}]: {CurrentMomentum}/100 " +
        $"(ATK +{AttackBonus}, DEF +{DefenseBonus}, Extra Atks {BonusAttacks})" +
        (CriticalChance.HasValue ? $" [+{CriticalChance}% CRIT]" : "") +
        (ConsecutiveHits > 0 ? $" [Chain x{ConsecutiveHits}]" : "");

    #endregion
}
