// ═══════════════════════════════════════════════════════════════════════════════
// TraumaEconomyState.cs
// Aggregate root representing the unified trauma economy state for a character.
// Aggregates all trauma economy component states and provides computed
// cross-system properties for integrated effect calculations.
// Version: 0.18.5a
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.Entities;

using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Aggregate root representing the unified trauma economy state for a character.
/// </summary>
/// <remarks>
/// <para>
/// TraumaEconomyState aggregates all trauma economy component states and provides
/// computed cross-system properties. It serves as the primary query interface for
/// trauma economy information and is used by services to calculate integrated effects.
/// </para>
/// <para>
/// The entity does not persist individual states; instead, it composes them from
/// the individual sub-systems (StressService, CorruptionService, TraumaService, etc).
/// </para>
/// <para>
/// Computed properties are calculated on-demand and not cached, ensuring consistency
/// with underlying state systems.
/// </para>
/// <para>
/// <strong>Aggregated States:</strong>
/// </para>
/// <list type="bullet">
///   <item><description><see cref="StressState"/>: Psychic stress (0-100) with defense penalties</description></item>
///   <item><description><see cref="CorruptionState"/>: Runic Blight corruption (0-100) with HP/AP penalties</description></item>
///   <item><description><see cref="CpsState"/>: Cognitive Paradox Syndrome derived from stress</description></item>
///   <item><description><see cref="Traumas"/>: List of permanent character traumas</description></item>
///   <item><description><see cref="SpecializationResource"/>: Polymorphic specialization resource (Rage/Momentum/Coherence)</description></item>
/// </list>
/// </remarks>
/// <example>
/// <code>
/// var state = TraumaEconomyState.Create(
///     characterId,
///     stressState,
///     corruptionState,
///     cpsState,
///     traumas,
///     specializationResource: rageState,
///     specializationType: "rage"
/// );
/// 
/// var effectiveHp = state.EffectiveMaxHp; // Applies corruption penalty
/// var warnings = state.ActiveWarnings;     // Lists all active system warnings
/// var level = state.GetWarningLevel();    // Overall severity assessment
/// </code>
/// </example>
/// <seealso cref="StressState"/>
/// <seealso cref="CorruptionState"/>
/// <seealso cref="CpsState"/>
/// <seealso cref="CharacterTrauma"/>
/// <seealso cref="TraumaEconomySnapshot"/>
public class TraumaEconomyState
{
    // ═══════════════════════════════════════════════════════════════════════════
    // Constants
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Default base hit points used for effective HP calculations.
    /// </summary>
    /// <remarks>
    /// TODO: Inject from character stats in v0.18.5e service layer.
    /// </remarks>
    private const int DefaultBaseHp = 100;

    /// <summary>
    /// Default base action points used for effective AP calculations.
    /// </summary>
    /// <remarks>
    /// TODO: Inject from character stats in v0.18.5e service layer.
    /// </remarks>
    private const int DefaultBaseAp = 50;

    /// <summary>
    /// Default base Resolve dice used for effective Resolve calculations.
    /// </summary>
    /// <remarks>
    /// TODO: Inject from character attributes in v0.18.5e service layer.
    /// </remarks>
    private const int DefaultBaseResolve = 10;

    /// <summary>
    /// Threshold at which a system is considered in warning state.
    /// </summary>
    private const int WarningThreshold = 70;

    /// <summary>
    /// Threshold at which a system is considered in critical state.
    /// </summary>
    private const int CriticalThreshold = 80;

    /// <summary>
    /// Threshold at which a system is considered in terminal state.
    /// </summary>
    private const int TerminalThreshold = 100;

    // ═══════════════════════════════════════════════════════════════════════════
    // Properties — Stored
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the character ID for this trauma economy state.
    /// </summary>
    public Guid CharacterId { get; private init; }

    /// <summary>
    /// Gets the current psychic stress state.
    /// </summary>
    /// <remarks>
    /// Contains stress value (0-100) and derived properties including
    /// defense penalty, skill disadvantage, and trauma check requirement.
    /// </remarks>
    public StressState StressState { get; private init; }

    /// <summary>
    /// Gets the current corruption state.
    /// </summary>
    /// <remarks>
    /// Contains corruption value (0-100) and severity-based penalties
    /// including HP/AP percentage reductions and Resolve dice penalty.
    /// </remarks>
    public CorruptionState CorruptionState { get; private init; }

    /// <summary>
    /// Gets the current Cognitive Paradox Syndrome state.
    /// </summary>
    /// <remarks>
    /// Derived from <see cref="StressState"/>. Includes CPS stage
    /// and panic check requirement.
    /// </remarks>
    public CpsState CpsState { get; private init; }

    /// <summary>
    /// Gets the immutable list of active traumas for this character.
    /// </summary>
    /// <remarks>
    /// Traumas are acquired through near-death and ally death events.
    /// Each trauma has ongoing mechanical effects tracked via
    /// <see cref="ActiveTraumaIds"/>.
    /// </remarks>
    public IReadOnlyList<CharacterTrauma> Traumas { get; private init; }

    /// <summary>
    /// Gets the specialization-specific resource (polymorphic).
    /// </summary>
    /// <remarks>
    /// <para>
    /// May be one of:
    /// </para>
    /// <list type="bullet">
    ///   <item><description><c>RageState</c> for Berserker specialization</description></item>
    ///   <item><description><c>MomentumState</c> for Storm Blade specialization</description></item>
    ///   <item><description><c>CoherenceState</c> for Arcanist specialization</description></item>
    /// </list>
    /// <para>
    /// Null if no specialization is active or character has no specialization.
    /// </para>
    /// </remarks>
    public object? SpecializationResource { get; private init; }

    /// <summary>
    /// Gets the specialization type name.
    /// </summary>
    /// <remarks>
    /// Examples: "rage", "momentum", "coherence", or null if no specialization.
    /// Used for type discrimination when processing <see cref="SpecializationResource"/>.
    /// </remarks>
    public string? SpecializationType { get; private init; }

    // ═══════════════════════════════════════════════════════════════════════════
    // Properties — Computed (Cross-System)
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the effective maximum hit points after corruption penalty.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Formula: BaseHP × (1 - (StageOrdinal × 0.05))
    /// Each corruption stage reduces max HP by 5%.
    /// </para>
    /// <para>
    /// At Consumed stage (5), effective HP is reduced by 25%.
    /// </para>
    /// </remarks>
    /// <value>The effective max HP, always ≥ 0.</value>
    public int EffectiveMaxHp =>
        (int)(DefaultBaseHp * (1 - (int)CorruptionState.Stage * 0.05));

    /// <summary>
    /// Gets the effective maximum action points after corruption penalty.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Formula: BaseAP × (1 - (StageOrdinal × 0.05))
    /// Each corruption stage reduces max AP by 5%.
    /// </para>
    /// <para>
    /// Action point reduction limits tactical options in combat.
    /// </para>
    /// </remarks>
    /// <value>The effective max AP, always ≥ 0.</value>
    public int EffectiveMaxAp =>
        (int)(DefaultBaseAp * (1 - (int)CorruptionState.Stage * 0.05));

    /// <summary>
    /// Gets the effective Resolve dice after corruption penalty.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Formula: BaseResolve - StageOrdinal
    /// Each corruption stage reduces Resolve by 1 die.
    /// </para>
    /// <para>
    /// Result is clamped to minimum of 1 to ensure at least one die
    /// can be rolled for Resolve checks.
    /// </para>
    /// </remarks>
    /// <value>The effective Resolve dice count, always ≥ 1.</value>
    public int EffectiveResolve =>
        Math.Max(1, DefaultBaseResolve - (int)CorruptionState.Stage);

    /// <summary>
    /// Gets the total defense penalty from stress system.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Derived from stress threshold. Higher stress = higher penalty.
    /// </para>
    /// <list type="bullet">
    ///   <item><description>Calm (0-19): 0 penalty</description></item>
    ///   <item><description>Uneasy (20-39): -1 penalty</description></item>
    ///   <item><description>Anxious (40-59): -2 penalty</description></item>
    ///   <item><description>Panicked (60-79): -3 penalty</description></item>
    ///   <item><description>Breaking (80-99): -4 penalty</description></item>
    ///   <item><description>Trauma (100): -5 penalty</description></item>
    /// </list>
    /// </remarks>
    /// <value>Defense penalty as a positive integer (0-5).</value>
    public int TotalDefensePenalty => StressState.DefensePenalty;

    /// <summary>
    /// Gets the total skill penalty from stress and CPS combined.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Formula: StressSkillDisadvantage (1 if Breaking/Trauma, else 0) + CpsLogicDisadvantage
    /// </para>
    /// <para>
    /// Penalties stack additively as both systems contribute to cognitive impairment.
    /// CPS logic disadvantage ranges from 0 (None) to 3 (HollowShell).
    /// </para>
    /// </remarks>
    /// <value>Combined skill penalty as a positive integer.</value>
    public int TotalSkillPenalty => (StressState.HasSkillDisadvantage ? 1 : 0) + GetCpsLogicDisadvantage();

    /// <summary>
    /// Gets whether the character is in a critical system state.
    /// </summary>
    /// <remarks>
    /// True when any system (Stress or Corruption) is at 80 or higher.
    /// Critical state indicates immediate danger and need for recovery.
    /// </remarks>
    /// <value><c>true</c> if any system ≥ 80; otherwise, <c>false</c>.</value>
    public bool IsCriticalState =>
        StressState.CurrentStress >= CriticalThreshold ||
        CorruptionState.CurrentCorruption >= CriticalThreshold;

    /// <summary>
    /// Gets whether the character is in a terminal system state.
    /// </summary>
    /// <remarks>
    /// <para>
    /// True when any system (Stress or Corruption) reaches 100.
    /// </para>
    /// <para>
    /// Character in terminal state requires immediate resolution:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>Stress 100: Trauma Check required</description></item>
    ///   <item><description>Corruption 100: Terminal Error (character may be lost)</description></item>
    /// </list>
    /// </remarks>
    /// <value><c>true</c> if any system reached 100; otherwise, <c>false</c>.</value>
    public bool IsTerminalState =>
        StressState.CurrentStress >= TerminalThreshold ||
        CorruptionState.CurrentCorruption >= TerminalThreshold;

    /// <summary>
    /// Gets the count of active traumas.
    /// </summary>
    /// <value>Number of traumas in the <see cref="Traumas"/> list.</value>
    public int TraumaCount => Traumas?.Count ?? 0;

    /// <summary>
    /// Gets whether multiple trauma economy systems are simultaneously in critical state.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Occurs when 2+ of (Stress, Corruption) are at 80+. Indicates compound danger
    /// where the character faces multiple simultaneous crises.
    /// </para>
    /// <para>
    /// This is a particularly dangerous situation as recovery from one system
    /// may worsen another.
    /// </para>
    /// </remarks>
    /// <value><c>true</c> if 2+ systems are at ≥80; otherwise, <c>false</c>.</value>
    public bool HasMultipleCriticalSystems =>
        (StressState.CurrentStress >= CriticalThreshold ? 1 : 0) +
        (CorruptionState.CurrentCorruption >= CriticalThreshold ? 1 : 0) >= 2;

    // ═══════════════════════════════════════════════════════════════════════════
    // Warning System
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the list of active warning messages for this character's trauma economy state.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Warnings indicate systems approaching or exceeding critical thresholds.
    /// Used for UI display and narrative prompting.
    /// </para>
    /// <para>
    /// Messages are generated based on current system values:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>Stress 90+: Mind spiraling toward collapse</description></item>
    ///   <item><description>Stress 80+: Sanity fracturing</description></item>
    ///   <item><description>Stress 70+: Thoughts scattered</description></item>
    ///   <item><description>Corruption 90+: Blight consuming essence</description></item>
    ///   <item><description>Corruption 80+: Corruption ravaging form</description></item>
    ///   <item><description>Corruption 70+: Taint spreading deeper</description></item>
    ///   <item><description>CPS panic: Grip on reality slipping</description></item>
    ///   <item><description>Multiple critical: Body and mind failing</description></item>
    ///   <item><description>Traumas: Carrying unhealed trauma count</description></item>
    /// </list>
    /// </remarks>
    /// <value>Read-only list of warning message strings.</value>
    public IReadOnlyList<string> ActiveWarnings
    {
        get
        {
            var warnings = new List<string>();

            // Stress warnings (higher severity first)
            if (StressState.CurrentStress >= 90)
            {
                warnings.Add("Your mind spirals toward total collapse.");
            }
            else if (StressState.CurrentStress >= CriticalThreshold)
            {
                warnings.Add("Your sanity fractures under the weight of reality.");
            }
            else if (StressState.CurrentStress >= WarningThreshold)
            {
                warnings.Add("Your thoughts grow increasingly scattered.");
            }

            // Corruption warnings (higher severity first)
            if (CorruptionState.CurrentCorruption >= 90)
            {
                warnings.Add("The Blight consumes your very essence.");
            }
            else if (CorruptionState.CurrentCorruption >= CriticalThreshold)
            {
                warnings.Add("Corruption ravages your form.");
            }
            else if (CorruptionState.CurrentCorruption >= WarningThreshold)
            {
                warnings.Add("The taint spreads deeper within you.");
            }

            // CPS warnings
            if (CpsState.RequiresPanicCheck)
            {
                warnings.Add("Your grip on reality is slipping—Panic Table active.");
            }

            // Combined system warnings
            if (HasMultipleCriticalSystems)
            {
                warnings.Add("Your body and mind fail in tandem.");
            }

            // Trauma count warning
            if (TraumaCount > 0)
            {
                var plural = TraumaCount > 1 ? "s" : "";
                warnings.Add($"You carry {TraumaCount} unhealed trauma{plural}.");
            }

            return warnings.AsReadOnly();
        }
    }

    /// <summary>
    /// Gets the list of active trauma definition IDs for this character.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Each trauma is identified by its definition ID. This property aggregates
    /// all active trauma IDs for unified processing and effect resolution.
    /// </para>
    /// <para>
    /// The actual effects are resolved via TraumaDefinition lookups in the service layer.
    /// </para>
    /// </remarks>
    /// <value>Read-only list of trauma definition IDs (e.g., "survivors-guilt").</value>
    public IReadOnlyList<string> ActiveTraumaIds =>
        Traumas?.Where(t => t.IsActive).Select(t => t.TraumaDefinitionId).ToList().AsReadOnly() ??
        new List<string>().AsReadOnly();

    // ═══════════════════════════════════════════════════════════════════════════
    // Query Methods
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Determines the overall warning level for UI/narrative purposes.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Returns the highest severity level across all systems:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>Terminal: Any system at 100</description></item>
    ///   <item><description>Critical: Any system at 80+ OR CPS requires panic check</description></item>
    ///   <item><description>Warning: Any system at 70+</description></item>
    ///   <item><description>None: All systems below 70</description></item>
    /// </list>
    /// </remarks>
    /// <returns>The <see cref="WarningLevel"/> for the current state.</returns>
    /// <example>
    /// <code>
    /// var level = state.GetWarningLevel();
    /// if (level >= WarningLevel.Critical)
    /// {
    ///     ShowUrgentNotification("Character in danger!");
    /// }
    /// </code>
    /// </example>
    public WarningLevel GetWarningLevel()
    {
        if (IsTerminalState)
        {
            return WarningLevel.Terminal;
        }

        if (IsCriticalState || CpsState.RequiresPanicCheck)
        {
            return WarningLevel.Critical;
        }

        if (StressState.CurrentStress >= WarningThreshold ||
            CorruptionState.CurrentCorruption >= WarningThreshold)
        {
            return WarningLevel.Warning;
        }

        return WarningLevel.None;
    }

    /// <summary>
    /// Gets the highest stress level percentage (0-1) across all systems.
    /// </summary>
    /// <remarks>
    /// Returns the maximum of Stress/100 and Corruption/100.
    /// Useful for progress bar rendering or priority calculations.
    /// </remarks>
    /// <returns>A double in range [0.0, 1.0] representing the highest system fill.</returns>
    /// <example>
    /// <code>
    /// var highest = state.GetHighestSystemPercentage();
    /// // If stress=60, corruption=80 -> returns 0.8
    /// </code>
    /// </example>
    public double GetHighestSystemPercentage() =>
        Math.Max(
            StressState.CurrentStress / 100.0,
            CorruptionState.CurrentCorruption / 100.0
        );

    // ═══════════════════════════════════════════════════════════════════════════
    // Factory Methods
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a new TraumaEconomyState from component states.
    /// </summary>
    /// <param name="characterId">The character's unique identifier. Cannot be empty.</param>
    /// <param name="stressState">The psychic stress state.</param>
    /// <param name="corruptionState">The corruption state.</param>
    /// <param name="cpsState">The CPS state (derived from stress).</param>
    /// <param name="traumas">The list of active traumas. Null is treated as empty list.</param>
    /// <param name="specializationResource">Optional specialization resource (polymorphic).</param>
    /// <param name="specializationType">Optional specialization type name.</param>
    /// <returns>A new TraumaEconomyState with all component states aggregated.</returns>
    /// <exception cref="ArgumentException">Thrown when characterId is empty.</exception>
    /// <example>
    /// <code>
    /// // Basic creation with required states
    /// var state = TraumaEconomyState.Create(
    ///     characterId,
    ///     StressState.Create(45),
    ///     CorruptionState.Create(30),
    ///     CpsState.Create(45),
    ///     traumas: null
    /// );
    /// 
    /// // With specialization resource
    /// var stateWithSpec = TraumaEconomyState.Create(
    ///     characterId,
    ///     stressState,
    ///     corruptionState,
    ///     cpsState,
    ///     traumas,
    ///     specializationResource: rageState,
    ///     specializationType: "rage"
    /// );
    /// </code>
    /// </example>
    public static TraumaEconomyState Create(
        Guid characterId,
        StressState stressState,
        CorruptionState corruptionState,
        CpsState cpsState,
        IReadOnlyList<CharacterTrauma>? traumas,
        object? specializationResource = null,
        string? specializationType = null)
    {
        if (characterId == Guid.Empty)
        {
            throw new ArgumentException("CharacterId cannot be empty.", nameof(characterId));
        }

        return new TraumaEconomyState
        {
            CharacterId = characterId,
            StressState = stressState,
            CorruptionState = corruptionState,
            CpsState = cpsState,
            Traumas = traumas ?? new List<CharacterTrauma>().AsReadOnly(),
            SpecializationResource = specializationResource,
            SpecializationType = specializationType?.ToLowerInvariant()
        };
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Private Helpers
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Calculates the logic disadvantage penalty from CPS stage.
    /// </summary>
    /// <remarks>
    /// CPS stage contributes to skill penalty via logic impairment:
    /// <list type="bullet">
    ///   <item><description>None/WeightOfKnowing: 0 disadvantage</description></item>
    ///   <item><description>GlimmerMadness: 1 disadvantage</description></item>
    ///   <item><description>RuinMadness: 2 disadvantage</description></item>
    ///   <item><description>HollowShell: 3 disadvantage</description></item>
    /// </list>
    /// </remarks>
    /// <returns>Logic disadvantage penalty (0-3).</returns>
    private int GetCpsLogicDisadvantage() =>
        CpsState.Stage switch
        {
            CpsStage.None => 0,
            CpsStage.WeightOfKnowing => 0,
            CpsStage.GlimmerMadness => 1,
            CpsStage.RuinMadness => 2,
            CpsStage.HollowShell => 3,
            _ => 0
        };

    // ═══════════════════════════════════════════════════════════════════════════
    // Display
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Returns a string representation of the trauma economy state for debugging and logging.
    /// </summary>
    /// <returns>
    /// A formatted string showing character ID, stress, corruption, and trauma count.
    /// </returns>
    /// <example>
    /// <code>
    /// var state = TraumaEconomyState.Create(...);
    /// Console.WriteLine(state.ToString());
    /// // Output: "TraumaEconomy[abc123...]: Stress=45 Corruption=30 Traumas=2"
    /// </code>
    /// </example>
    public override string ToString() =>
        $"TraumaEconomy[{CharacterId}]: " +
        $"Stress={StressState.CurrentStress} " +
        $"Corruption={CorruptionState.CurrentCorruption} " +
        $"Traumas={TraumaCount}";

    // ═══════════════════════════════════════════════════════════════════════════
    // Constructors
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Private constructor for EF Core and factory method use.
    /// </summary>
    private TraumaEconomyState()
    {
        Traumas = new List<CharacterTrauma>().AsReadOnly();
    }
}
