namespace RuneAndRust.Domain.Entities;

using RuneAndRust.Domain.Enums;

/// <summary>
/// Represents the rage state of a Berserker character.
/// </summary>
/// <remarks>
/// <para>
/// RageState is the core domain entity for the Rage system.
/// It is derived from character actions and provides bonuses
/// that scale with rage level.
/// </para>
/// <para>
/// Key Properties:
/// <list type="bullet">
/// <item>CurrentRage: 0-100 integer</item>
/// <item>Threshold: Computed from CurrentRage</item>
/// <item>DamageBonus: floor(CurrentRage / 10), ranges 0-10</item>
/// <item>DamageTakenReduction: Computed by threshold</item>
/// <item>SoakBonus: Ranges 0-4 depending on threshold</item>
/// <item>FearImmune: true only at FrenzyBeyondReason</item>
/// <item>MustAttackNearest: true at BerserkFury and above</item>
/// <item>PartyStressReduction: 10 only at FrenzyBeyondReason</item>
/// <item>DecayPerTurn: 10 per non-combat turn</item>
/// </list>
/// </para>
/// </remarks>
public class RageState
{
    #region Constants

    /// <summary>Minimum rage value.</summary>
    public const int MinRage = 0;

    /// <summary>Maximum rage value.</summary>
    public const int MaxRage = 100;

    /// <summary>Rage threshold for Simmering state.</summary>
    public const int SimmeringThreshold = 20;

    /// <summary>Rage threshold for Burning state.</summary>
    public const int BurningThreshold = 40;

    /// <summary>Rage threshold for BerserkFury state.</summary>
    public const int BerserkFuryThreshold = 60;

    /// <summary>Rage threshold for FrenzyBeyondReason state.</summary>
    public const int FrenzyBeyondReasonThreshold = 80;

    /// <summary>Rage decay per non-combat turn.</summary>
    public const int DecayPerNonCombatTurn = 10;

    #endregion

    #region Properties

    /// <summary>
    /// Gets the character ID this rage state belongs to.
    /// </summary>
    public Guid CharacterId { get; private init; }

    /// <summary>
    /// Gets the current rage level (0-100).
    /// </summary>
    public int CurrentRage { get; private init; }

    /// <summary>
    /// Gets the current rage threshold based on rage level.
    /// </summary>
    public RageThreshold Threshold { get; private init; }

    /// <summary>
    /// Gets the damage bonus from rage (floor(CurrentRage / 10)).
    /// </summary>
    /// <remarks>
    /// Ranges from 0 to 10.
    /// </remarks>
    public int DamageBonus { get; private init; }

    /// <summary>
    /// Gets the damage reduction percentage from rage absorption.
    /// </summary>
    /// <remarks>
    /// Calculated as floor(CurrentRage / 20) * 5%.
    /// Ranges from 0% to 25%.
    /// </remarks>
    public int DamageTakenReduction { get; private init; }

    /// <summary>
    /// Gets the soak bonus (defense against damage) from threshold.
    /// </summary>
    /// <remarks>
    /// Ranges 0-4 depending on threshold.
    /// </remarks>
    public int SoakBonus { get; private init; }

    /// <summary>
    /// Gets whether the character is immune to fear effects.
    /// </summary>
    /// <remarks>
    /// True only at FrenzyBeyondReason threshold.
    /// </remarks>
    public bool FearImmune { get; private init; }

    /// <summary>
    /// Gets whether the character must attack the nearest enemy.
    /// </summary>
    /// <remarks>
    /// True at BerserkFury and FrenzyBeyondReason thresholds.
    /// </remarks>
    public bool MustAttackNearest { get; private init; }

    /// <summary>
    /// Gets the party stress reduction at FrenzyBeyondReason.
    /// </summary>
    /// <remarks>
    /// 10 stress reduction for party at rest, or null if not active.
    /// </remarks>
    public int? PartyStressReduction { get; private init; }

    /// <summary>
    /// Gets the time of last combat engagement.
    /// </summary>
    /// <remarks>
    /// Used to determine non-combat decay eligibility.
    /// </remarks>
    public DateTime? LastCombatTime { get; private init; }

    #endregion

    #region Computed Properties

    /// <summary>
    /// Gets the decay per non-combat turn (constant: 10).
    /// </summary>
    public int DecayPerTurn => DecayPerNonCombatTurn;

    #endregion

    #region Constructors

    /// <summary>
    /// Private constructor for EF Core.
    /// </summary>
    private RageState() { }

    #endregion

    #region Factory Methods

    /// <summary>
    /// Creates a new RageState for a character.
    /// </summary>
    /// <param name="characterId">The character ID.</param>
    /// <param name="initialRage">Initial rage value (default 0).</param>
    /// <param name="lastCombatTime">Optional last combat time for decay tracking.</param>
    /// <returns>A new RageState instance.</returns>
    /// <exception cref="ArgumentException">Thrown when characterId is empty.</exception>
    public static RageState Create(Guid characterId, int initialRage = 0, DateTime? lastCombatTime = null)
    {
        if (characterId == Guid.Empty)
        {
            throw new ArgumentException("CharacterId cannot be empty.", nameof(characterId));
        }

        var clampedRage = Math.Clamp(initialRage, MinRage, MaxRage);
        var threshold = DetermineThreshold(clampedRage);

        return new RageState
        {
            CharacterId = characterId,
            CurrentRage = clampedRage,
            Threshold = threshold,
            DamageBonus = CalculateDamageBonus(clampedRage),
            DamageTakenReduction = CalculateDamageTakenReduction(clampedRage),
            SoakBonus = CalculateSoakBonus(threshold),
            FearImmune = threshold == RageThreshold.FrenzyBeyondReason,
            MustAttackNearest = threshold >= RageThreshold.BerserkFury,
            PartyStressReduction = threshold == RageThreshold.FrenzyBeyondReason ? 10 : null,
            LastCombatTime = lastCombatTime
        };
    }

    /// <summary>
    /// Determines the rage threshold for a given rage value.
    /// </summary>
    /// <param name="rage">The rage value (0-100).</param>
    /// <returns>The corresponding RageThreshold.</returns>
    public static RageThreshold DetermineThreshold(int rage) =>
        rage switch
        {
            > FrenzyBeyondReasonThreshold => RageThreshold.FrenzyBeyondReason,
            > BerserkFuryThreshold => RageThreshold.BerserkFury,
            > BurningThreshold => RageThreshold.Burning,
            > SimmeringThreshold => RageThreshold.Simmering,
            _ => RageThreshold.Calm
        };

    #endregion

    #region Private Calculation Methods

    /// <summary>
    /// Calculates damage bonus from rage value.
    /// </summary>
    /// <param name="rage">Current rage value.</param>
    /// <returns>Damage bonus (0-10).</returns>
    private static int CalculateDamageBonus(int rage) =>
        Math.Min(MaxRage, rage) / 10;

    /// <summary>
    /// Calculates damage reduction percentage from rage value.
    /// </summary>
    /// <param name="rage">Current rage value.</param>
    /// <returns>Damage reduction percentage (0-25).</returns>
    private static int CalculateDamageTakenReduction(int rage) =>
        (rage / 20) * 5;

    /// <summary>
    /// Calculates soak bonus from threshold.
    /// </summary>
    /// <param name="threshold">Current rage threshold.</param>
    /// <returns>Soak bonus (0-4).</returns>
    private static int CalculateSoakBonus(RageThreshold threshold) =>
        threshold switch
        {
            RageThreshold.Calm => 0,
            RageThreshold.Simmering => 1,
            RageThreshold.Burning => 2,
            RageThreshold.BerserkFury => 3,
            RageThreshold.FrenzyBeyondReason => 4,
            _ => 0
        };

    #endregion

    #region Display

    /// <inheritdoc/>
    public override string ToString() =>
        $"Rage[{Threshold}]: {CurrentRage}/100 " +
        $"(DMG +{DamageBonus}, SOAK +{SoakBonus})" +
        (FearImmune ? " [FEAR IMMUNE]" : "") +
        (MustAttackNearest ? " [MUST ATTACK]" : "");

    #endregion
}
