namespace RuneAndRust.Domain.Entities;

using RuneAndRust.Domain.Enums;

/// <summary>
/// Represents the coherence state of an Arcanist character.
/// </summary>
/// <remarks>
/// <para>
/// CoherenceState is the core domain entity for the Coherence system.
/// It represents magical reality stability and provides spell bonuses
/// or cascading failures based on coherence level.
/// </para>
/// <para>
/// Key Properties:
/// <list type="bullet">
/// <item>CurrentCoherence: 0-100 integer (default 50 for Balanced state)</item>
/// <item>Threshold: Computed from CurrentCoherence</item>
/// <item>SpellPowerBonus: -2 to +5 by threshold</item>
/// <item>CriticalCastChance: 0% to 20% by threshold</item>
/// <item>CascadeRisk: 0%, 10%, or 25% at low thresholds</item>
/// <item>InApotheosis: true only at Apotheosis threshold</item>
/// <item>ApotheosisStressCost: 10 per turn during Apotheosis</item>
/// <item>CanMeditate: false during combat</item>
/// </list>
/// </para>
/// </remarks>
public class CoherenceState
{
    #region Constants

    /// <summary>
    /// Minimum coherence value.
    /// </summary>
    public const int MinCoherence = 0;

    /// <summary>
    /// Maximum coherence value.
    /// </summary>
    public const int MaxCoherence = 100;

    /// <summary>
    /// Default starting coherence (Balanced state).
    /// </summary>
    public const int DefaultCoherence = 50;

    /// <summary>
    /// Coherence at or below which Destabilized state applies.
    /// </summary>
    /// <remarks>
    /// Coherence 0-20 = Destabilized.
    /// </remarks>
    public const int DestabilizedThreshold = 20;

    /// <summary>
    /// Coherence at or below which Unstable state applies (above Destabilized).
    /// </summary>
    /// <remarks>
    /// Coherence 21-40 = Unstable.
    /// </remarks>
    public const int UnstableThreshold = 40;

    /// <summary>
    /// Coherence at or below which Balanced state applies (above Unstable).
    /// </summary>
    /// <remarks>
    /// Coherence 41-60 = Balanced.
    /// </remarks>
    public const int BalancedThreshold = 60;

    /// <summary>
    /// Coherence at or below which Focused state applies (above Balanced).
    /// </summary>
    /// <remarks>
    /// Coherence 61-80 = Focused.
    /// </remarks>
    public const int FocusedThreshold = 80;

    /// <summary>
    /// Cascade risk percentage at Destabilized state.
    /// </summary>
    public const int CascadeRiskDestabilized = 25;

    /// <summary>
    /// Cascade risk percentage at Unstable state.
    /// </summary>
    public const int CascadeRiskUnstable = 10;

    /// <summary>
    /// Stress cost per turn during Apotheosis.
    /// </summary>
    public const int ApotheosisStressCostPerTurn = 10;

    /// <summary>
    /// Coherence gained per meditation action.
    /// </summary>
    public const int MeditationGain = 20;

    /// <summary>
    /// Coherence gained per successful cast.
    /// </summary>
    public const int CastGain = 5;

    /// <summary>
    /// Coherence gained per turn of controlled channel.
    /// </summary>
    public const int ChannelGainPerTurn = 3;

    /// <summary>
    /// Critical cast chance at Balanced threshold (percentage).
    /// </summary>
    public const int BalancedCritChance = 5;

    /// <summary>
    /// Critical cast chance at Focused threshold (percentage).
    /// </summary>
    public const int FocusedCritChance = 10;

    /// <summary>
    /// Critical cast chance at Apotheosis threshold (percentage).
    /// </summary>
    public const int ApotheosisCritChance = 20;

    #endregion

    #region Properties

    /// <summary>
    /// Gets the character ID this coherence state belongs to.
    /// </summary>
    public Guid CharacterId { get; private init; }

    /// <summary>
    /// Gets the current coherence level (0-100).
    /// </summary>
    public int CurrentCoherence { get; private init; }

    /// <summary>
    /// Gets the current coherence threshold.
    /// </summary>
    public CoherenceThreshold Threshold { get; private init; }

    /// <summary>
    /// Gets the spell power bonus from coherence.
    /// </summary>
    /// <remarks>
    /// Ranges from -2 (Destabilized) to +5 (Apotheosis).
    /// </remarks>
    public int SpellPowerBonus { get; private init; }

    /// <summary>
    /// Gets the critical casting chance bonus from coherence.
    /// </summary>
    /// <remarks>
    /// Ranges from 0% (Destabilized/Unstable) to 20% (Apotheosis).
    /// Null only for thresholds without crit bonus.
    /// </remarks>
    public int? CriticalCastChance { get; private init; }

    /// <summary>
    /// Gets the cascade risk percentage at current threshold.
    /// </summary>
    /// <remarks>
    /// 0% at Balanced and above.
    /// 10% at Unstable.
    /// 25% at Destabilized.
    /// </remarks>
    public int CascadeRisk { get; private init; }

    /// <summary>
    /// Gets whether the character is in Apotheosis state.
    /// </summary>
    /// <remarks>
    /// True only at Apotheosis threshold (coherence 81+).
    /// </remarks>
    public bool InApotheosis { get; private init; }

    /// <summary>
    /// Gets whether Apotheosis abilities are unlocked.
    /// </summary>
    /// <remarks>
    /// True once first entered Apotheosis threshold.
    /// Persists even if coherence drops below 81.
    /// </remarks>
    public bool ApotheosisAbilitiesUnlocked { get; private init; }

    /// <summary>
    /// Gets the number of turns spent in Apotheosis.
    /// </summary>
    /// <remarks>
    /// Tracks duration for effect calculations and stress accumulation.
    /// </remarks>
    public int TurnsInApotheosis { get; private init; }

    /// <summary>
    /// Gets the time of last spell cast.
    /// </summary>
    /// <remarks>
    /// Used for cascade tracking and cooldown calculations.
    /// </remarks>
    public DateTime? LastCastTime { get; private init; }

    /// <summary>
    /// Gets whether the character is currently in combat.
    /// </summary>
    /// <remarks>
    /// Affects meditation availability. Cannot meditate during combat.
    /// </remarks>
    public bool IsCombat { get; private init; }

    #endregion

    #region Computed Properties

    /// <summary>
    /// Gets whether meditation is available.
    /// </summary>
    /// <remarks>
    /// Can only meditate outside of combat.
    /// </remarks>
    public bool CanMeditate => !IsCombat;

    /// <summary>
    /// Gets the stress cost per turn during Apotheosis.
    /// </summary>
    /// <remarks>
    /// Returns 10 if in Apotheosis, 0 otherwise.
    /// </remarks>
    public int ApotheosisStressCost => InApotheosis ? ApotheosisStressCostPerTurn : 0;

    #endregion

    #region Constructors

    /// <summary>
    /// Private constructor for EF Core.
    /// </summary>
    private CoherenceState()
    {
    }

    #endregion

    #region Factory Methods

    /// <summary>
    /// Creates a new CoherenceState for a character.
    /// </summary>
    /// <param name="characterId">The character ID. Cannot be empty.</param>
    /// <param name="initialCoherence">Initial coherence value (default 50 for Balanced). Clamped to 0-100.</param>
    /// <param name="isInCombat">Whether character is in combat. Affects meditation availability.</param>
    /// <param name="turnsInApotheosis">Number of turns already spent in Apotheosis (default 0).</param>
    /// <param name="apotheosisAbilitiesUnlocked">Whether Apotheosis abilities have been previously unlocked.</param>
    /// <param name="lastCastTime">Time of last spell cast for tracking purposes.</param>
    /// <returns>A new CoherenceState instance.</returns>
    /// <exception cref="ArgumentException">Thrown when characterId is empty.</exception>
    public static CoherenceState Create(
        Guid characterId,
        int initialCoherence = DefaultCoherence,
        bool isInCombat = false,
        int turnsInApotheosis = 0,
        bool apotheosisAbilitiesUnlocked = false,
        DateTime? lastCastTime = null)
    {
        if (characterId == Guid.Empty)
        {
            throw new ArgumentException("CharacterId cannot be empty.", nameof(characterId));
        }

        var clampedCoherence = Math.Clamp(initialCoherence, MinCoherence, MaxCoherence);
        var clampedTurns = Math.Max(0, turnsInApotheosis);
        var threshold = DetermineThreshold(clampedCoherence);
        var inApotheosis = threshold == CoherenceThreshold.Apotheosis;

        return new CoherenceState
        {
            CharacterId = characterId,
            CurrentCoherence = clampedCoherence,
            Threshold = threshold,
            SpellPowerBonus = CalculateSpellPowerBonus(threshold),
            CriticalCastChance = CalculateCriticalCastChance(threshold),
            CascadeRisk = CalculateCascadeRisk(threshold),
            InApotheosis = inApotheosis,
            ApotheosisAbilitiesUnlocked = apotheosisAbilitiesUnlocked || inApotheosis,
            TurnsInApotheosis = clampedTurns,
            LastCastTime = lastCastTime,
            IsCombat = isInCombat
        };
    }

    /// <summary>
    /// Determines the coherence threshold for a given coherence value.
    /// </summary>
    /// <param name="coherence">The coherence value (0-100). Values outside range are handled gracefully.</param>
    /// <returns>The corresponding CoherenceThreshold.</returns>
    /// <remarks>
    /// Threshold ranges:
    /// <list type="bullet">
    /// <item>0-20: Destabilized</item>
    /// <item>21-40: Unstable</item>
    /// <item>41-60: Balanced</item>
    /// <item>61-80: Focused</item>
    /// <item>81+: Apotheosis</item>
    /// </list>
    /// </remarks>
    public static CoherenceThreshold DetermineThreshold(int coherence) =>
        coherence switch
        {
            > FocusedThreshold => CoherenceThreshold.Apotheosis,
            > BalancedThreshold => CoherenceThreshold.Focused,
            > UnstableThreshold => CoherenceThreshold.Balanced,
            > DestabilizedThreshold => CoherenceThreshold.Unstable,
            _ => CoherenceThreshold.Destabilized
        };

    #endregion

    #region Private Calculation Methods

    /// <summary>
    /// Calculates spell power bonus from threshold.
    /// </summary>
    /// <param name="threshold">The coherence threshold.</param>
    /// <returns>Spell power bonus: -2 (Destabilized), -1 (Unstable), 0 (Balanced), +2 (Focused), +5 (Apotheosis).</returns>
    private static int CalculateSpellPowerBonus(CoherenceThreshold threshold) =>
        threshold switch
        {
            CoherenceThreshold.Destabilized => -2,
            CoherenceThreshold.Unstable => -1,
            CoherenceThreshold.Balanced => 0,
            CoherenceThreshold.Focused => 2,
            CoherenceThreshold.Apotheosis => 5,
            _ => 0
        };

    /// <summary>
    /// Calculates critical casting chance from threshold.
    /// </summary>
    /// <param name="threshold">The coherence threshold.</param>
    /// <returns>Critical cast chance percentage: 0 (Destabilized/Unstable), 5 (Balanced), 10 (Focused), 20 (Apotheosis).</returns>
    private static int? CalculateCriticalCastChance(CoherenceThreshold threshold) =>
        threshold switch
        {
            CoherenceThreshold.Destabilized => 0,
            CoherenceThreshold.Unstable => 0,
            CoherenceThreshold.Balanced => BalancedCritChance,
            CoherenceThreshold.Focused => FocusedCritChance,
            CoherenceThreshold.Apotheosis => ApotheosisCritChance,
            _ => 0
        };

    /// <summary>
    /// Calculates cascade risk from threshold.
    /// </summary>
    /// <param name="threshold">The coherence threshold.</param>
    /// <returns>Cascade risk percentage: 25 (Destabilized), 10 (Unstable), 0 (Balanced/Focused/Apotheosis).</returns>
    private static int CalculateCascadeRisk(CoherenceThreshold threshold) =>
        threshold switch
        {
            CoherenceThreshold.Destabilized => CascadeRiskDestabilized,
            CoherenceThreshold.Unstable => CascadeRiskUnstable,
            _ => 0
        };

    #endregion

    #region Display

    /// <inheritdoc/>
    public override string ToString()
    {
        var result = $"Coherence[{Threshold}]: {CurrentCoherence}/100 " +
                     $"(Power {SpellPowerBonus:+#;-#;+0}, Crit {CriticalCastChance}%)";

        if (CascadeRisk > 0)
        {
            result += $" [CASCADE RISK {CascadeRisk}%]";
        }

        if (InApotheosis)
        {
            result += $" [APOTHEOSIS Turn {TurnsInApotheosis}]";
        }

        if (!CanMeditate)
        {
            result += " [COMBAT]";
        }

        return result;
    }

    #endregion
}
