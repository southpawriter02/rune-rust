// ═══════════════════════════════════════════════════════════════════════════════
// UnifiedDamageHandler.cs
// Application service that coordinates damage processing across all trauma
// economy systems: stress, corruption, specialization resources, traumas.
// Version: 0.18.5b
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Application.Services;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Coordinates damage processing across all trauma economy systems.
/// </summary>
/// <remarks>
/// <para>
/// <strong>Overview:</strong>
/// The UnifiedDamageHandler orchestrates the complete damage flow through
/// all trauma economy subsystems. Instead of calling stress, corruption,
/// and specialization services separately, callers invoke ProcessDamage()
/// once and receive a comprehensive result.
/// </para>
/// <para>
/// <strong>Processing Pipeline:</strong>
/// </para>
/// <list type="number">
///   <item><description>Calculate soak (base armor + Berserker rage bonus)</description></item>
///   <item><description>Apply stress generation formula (damage/10 + bonuses)</description></item>
///   <item><description>Trigger specialization effects (Rage gain, Momentum/Coherence loss)</description></item>
///   <item><description>Check for trauma triggers (near-death, ally death)</description></item>
///   <item><description>Build and return comprehensive result</description></item>
/// </list>
/// <para>
/// <strong>Stress Generation:</strong>
/// </para>
/// <list type="bullet">
///   <item><description>Base: floor(damage after soak / 10)</description></item>
///   <item><description>Critical hit bonus: +5</description></item>
///   <item><description>Near-death bonus: +10 (if HP drops below 25%)</description></item>
/// </list>
/// <para>
/// <strong>Specialization Interactions:</strong>
/// </para>
/// <list type="bullet">
///   <item><description>Berserker: Adds 10% of current rage to soak; gains rage from damage</description></item>
///   <item><description>Storm Blade: Loses momentum on critical hits received</description></item>
///   <item><description>Arcanist: Loses coherence if casting is interrupted</description></item>
/// </list>
/// </remarks>
/// <example>
/// <code>
/// var handler = new UnifiedDamageHandler(
///     stressService, corruptionService, traumaService, specProvider,
///     rageService, momentumService, coherenceService, logger);
///
/// var context = new DamageContext(IsCriticalHit: true);
/// var result = handler.ProcessDamage(characterId, 50, context);
///
/// if (result.TraumaCheckTriggered)
/// {
///     traumaService.PerformTraumaCheck(characterId);
/// }
/// </code>
/// </example>
/// <seealso cref="DamageIntegrationResult"/>
/// <seealso cref="DamageContext"/>
/// <seealso cref="IStressService"/>
public class UnifiedDamageHandler
{
    // ═══════════════════════════════════════════════════════════════════════════
    // Constants
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Default base soak if character armor is not available.
    /// </summary>
    private const int DefaultBaseSoak = 5;

    /// <summary>
    /// Divisor for converting damage to stress.
    /// </summary>
    private const int DamageToStressDivisor = 10;

    /// <summary>
    /// Bonus stress for critical hits.
    /// </summary>
    private const int CriticalHitStressBonus = 5;

    /// <summary>
    /// Bonus stress for near-death events.
    /// </summary>
    private const int NearDeathStressBonus = 10;

    /// <summary>
    /// HP threshold percentage for near-death (25%).
    /// </summary>
    private const double NearDeathThreshold = 0.25;

    /// <summary>
    /// Rage soak multiplier (10% of current rage).
    /// </summary>
    private const double RageSoakMultiplier = 0.1;

    /// <summary>
    /// Rage gain per 5 damage taken.
    /// </summary>
    private const int RagePerDamageDivisor = 5;

    /// <summary>
    /// Momentum lost on critical hit received.
    /// </summary>
    private const int MomentumLostOnCritical = 20;

    /// <summary>
    /// Coherence lost on interrupt.
    /// </summary>
    private const int CoherenceLostOnInterrupt = 15;

    // ═══════════════════════════════════════════════════════════════════════════
    // Dependencies
    // ═══════════════════════════════════════════════════════════════════════════

    private readonly IStressService _stressService;
    private readonly ICorruptionService _corruptionService;
    private readonly IRageService? _rageService;
    private readonly IMomentumService? _momentumService;
    private readonly ICoherenceService? _coherenceService;
    private readonly ITraumaService _traumaService;
    private readonly ISpecializationProvider _specializationProvider;
    private readonly ILogger<UnifiedDamageHandler> _logger;

    // ═══════════════════════════════════════════════════════════════════════════
    // Constructor
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Initializes a new instance of the <see cref="UnifiedDamageHandler"/> class.
    /// </summary>
    /// <param name="stressService">Service for stress management (required).</param>
    /// <param name="corruptionService">Service for corruption management (required).</param>
    /// <param name="traumaService">Service for trauma management (required).</param>
    /// <param name="specializationProvider">Provider for specialization data (required).</param>
    /// <param name="rageService">Service for rage management (optional).</param>
    /// <param name="momentumService">Service for momentum management (optional).</param>
    /// <param name="coherenceService">Service for coherence management (optional).</param>
    /// <param name="logger">Logger instance (optional, uses NullLogger if not provided).</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when any required parameter is null.
    /// </exception>
    public UnifiedDamageHandler(
        IStressService stressService,
        ICorruptionService corruptionService,
        ITraumaService traumaService,
        ISpecializationProvider specializationProvider,
        IRageService? rageService = null,
        IMomentumService? momentumService = null,
        ICoherenceService? coherenceService = null,
        ILogger<UnifiedDamageHandler>? logger = null)
    {
        ArgumentNullException.ThrowIfNull(stressService);
        ArgumentNullException.ThrowIfNull(corruptionService);
        ArgumentNullException.ThrowIfNull(traumaService);
        ArgumentNullException.ThrowIfNull(specializationProvider);

        _stressService = stressService;
        _corruptionService = corruptionService;
        _traumaService = traumaService;
        _specializationProvider = specializationProvider;
        _rageService = rageService;
        _momentumService = momentumService;
        _coherenceService = coherenceService;
        _logger = logger ?? NullLogger<UnifiedDamageHandler>.Instance;

        _logger.LogDebug(
            "UnifiedDamageHandler initialized with specialization services: " +
            "Rage={RageAvailable}, Momentum={MomentumAvailable}, Coherence={CoherenceAvailable}",
            rageService != null,
            momentumService != null,
            coherenceService != null);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Public Methods
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Processes damage through all trauma economy systems.
    /// </summary>
    /// <param name="characterId">The character taking damage.</param>
    /// <param name="damage">Raw damage amount before soak.</param>
    /// <param name="context">Additional damage context (critical, interrupt, etc).</param>
    /// <returns>Complete damage integration result with all effects.</returns>
    /// <remarks>
    /// <para>
    /// <strong>Processing Steps:</strong>
    /// </para>
    /// <list type="number">
    ///   <item><description>Calculate soak (base + Berserker rage bonus if applicable)</description></item>
    ///   <item><description>Calculate damage after soak (minimum 1)</description></item>
    ///   <item><description>Calculate stress gain (damage/10 + bonuses)</description></item>
    ///   <item><description>Apply specialization effects based on character type</description></item>
    ///   <item><description>Check for trauma triggers (near-death, ally death)</description></item>
    ///   <item><description>Build comprehensive result with all messages</description></item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// var context = new DamageContext(IsCriticalHit: true);
    /// var result = handler.ProcessDamage(characterId, 50, context);
    ///
    /// // Apply stress to character
    /// stressService.ApplyStress(characterId, result.StressGained, result.StressSource!.Value);
    ///
    /// // Display messages
    /// foreach (var msg in result.TransitionMessages)
    /// {
    ///     Console.WriteLine(msg);
    /// }
    /// </code>
    /// </example>
    public DamageIntegrationResult ProcessDamage(
        Guid characterId,
        int damage,
        DamageContext context)
    {
        _logger.LogDebug(
            "Processing {Damage} damage for character {CharacterId} with context {@Context}",
            damage,
            characterId,
            context);

        // 1. Calculate soak
        var (baseSoak, bonusSoak) = CalculateSoak(characterId);
        var totalSoak = baseSoak + bonusSoak;
        var damageAfterSoak = Math.Max(1, damage - totalSoak);

        _logger.LogDebug(
            "Soak calculation: Base={BaseSoak} + Bonus={BonusSoak} = {Total}, " +
            "Damage reduced from {Original} to {Final}",
            baseSoak,
            bonusSoak,
            totalSoak,
            damage,
            damageAfterSoak);

        // 2. Calculate stress gain
        var stressGain = CalculateStressGain(damageAfterSoak, context);

        _logger.LogDebug(
            "Stress calculation: {StressGain} from {DamageAfterSoak} damage " +
            "(Critical={IsCritical})",
            stressGain,
            damageAfterSoak,
            context.IsCriticalHit);

        // 3. Apply specialization effects
        var (rageGained, momentumLost, coherenceLost) =
            ApplySpecializationEffects(characterId, damage, context);

        _logger.LogDebug(
            "Specialization effects: Rage +{RageGained}, Momentum -{MomentumLost}, " +
            "Coherence -{CoherenceLost}",
            rageGained ?? 0,
            momentumLost ?? 0,
            coherenceLost ?? 0);

        // 4. Check trauma trigger
        var traumaCheckTriggered = CheckTraumaTrigger(characterId, context, damageAfterSoak);

        if (traumaCheckTriggered)
        {
            _logger.LogWarning(
                "Trauma check triggered for character {CharacterId} — " +
                "near-death or ally death event",
                characterId);
        }

        // 5. Build result
        var result = BuildResult(
            damage,
            damageAfterSoak,
            totalSoak,
            stressGain,
            rageGained,
            momentumLost,
            coherenceLost,
            traumaCheckTriggered,
            context);

        _logger.LogInformation(
            "Damage processing complete for character {CharacterId}: " +
            "{DamageDealt} → {DamageAfterSoak} damage, +{StressGained} stress, " +
            "TraumaCheck={TraumaTriggered}",
            characterId,
            result.DamageDealt,
            result.DamageAfterSoak,
            result.StressGained,
            result.TraumaCheckTriggered);

        return result;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Private Methods - Soak Calculation
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Calculates soak value including specialization bonuses.
    /// </summary>
    /// <param name="characterId">The character to calculate soak for.</param>
    /// <returns>Tuple of base soak and bonus soak.</returns>
    /// <remarks>
    /// <para>
    /// <strong>Soak Components:</strong>
    /// </para>
    /// <list type="bullet">
    ///   <item><description>Base soak: From character armor/resistance (default 5)</description></item>
    ///   <item><description>Berserker bonus: 10% of current rage</description></item>
    /// </list>
    /// </remarks>
    private (int baseSoak, int bonusSoak) CalculateSoak(Guid characterId)
    {
        // Base soak from character armor/resistance
        // TODO: Pull from character equipment when available
        var baseSoak = DefaultBaseSoak;

        var bonusSoak = 0;

        // Berserker rage soak bonus
        if (_rageService != null)
        {
            try
            {
                var rageState = _rageService.GetRageState(characterId);
                if (rageState != null && rageState.CurrentRage > 0)
                {
                    bonusSoak = (int)(rageState.CurrentRage * RageSoakMultiplier);

                    _logger.LogDebug(
                        "Applied Berserker rage soak bonus: {BonusSoak} from {CurrentRage} rage",
                        bonusSoak,
                        rageState.CurrentRage);
                }
            }
            catch (Exception ex)
            {
                _logger.LogDebug(
                    ex,
                    "Could not retrieve rage state for soak calculation — character may not be Berserker");
            }
        }

        return (baseSoak, bonusSoak);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Private Methods - Stress Calculation
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Calculates stress gain from damage.
    /// </summary>
    /// <param name="damageAfterSoak">Damage after soak applied.</param>
    /// <param name="context">Damage context with bonus flags.</param>
    /// <returns>Total stress to apply.</returns>
    /// <remarks>
    /// <para>
    /// <strong>Stress Formula:</strong>
    /// </para>
    /// <list type="bullet">
    ///   <item><description>Base: floor(damage after soak / 10)</description></item>
    ///   <item><description>Critical hit: +5 bonus</description></item>
    ///   <item><description>Near-death: +10 bonus (if HP below 25%)</description></item>
    /// </list>
    /// <para>
    /// Bonuses stack additively.
    /// </para>
    /// </remarks>
    private int CalculateStressGain(int damageAfterSoak, DamageContext context)
    {
        var baseStress = damageAfterSoak / DamageToStressDivisor;

        var bonusStress = 0;

        // Critical hit bonus
        if (context.IsCriticalHit)
        {
            bonusStress += CriticalHitStressBonus;
            _logger.LogDebug("Critical hit stress bonus applied: +{Bonus}", CriticalHitStressBonus);
        }

        // Near-death bonus
        // TODO: Get actual HP from character when available
        // For now, use a placeholder check based on damage magnitude
        var currentHp = 50; // Placeholder
        var maxHp = 100;    // Placeholder

        if (currentHp - damageAfterSoak < (maxHp * NearDeathThreshold))
        {
            bonusStress += NearDeathStressBonus;
            _logger.LogDebug("Near-death stress bonus applied: +{Bonus}", NearDeathStressBonus);
        }

        var totalStress = baseStress + bonusStress;

        _logger.LogDebug(
            "Stress calculation: Base={Base} + Bonus={Bonus} = {Total}",
            baseStress,
            bonusStress,
            totalStress);

        return totalStress;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Private Methods - Specialization Effects
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Applies specialization-specific damage effects.
    /// </summary>
    /// <param name="characterId">The character taking damage.</param>
    /// <param name="damage">Raw damage amount.</param>
    /// <param name="context">Damage context.</param>
    /// <returns>Tuple of optional rage gained, momentum lost, coherence lost.</returns>
    /// <remarks>
    /// <para>
    /// <strong>Specialization Effects:</strong>
    /// </para>
    /// <list type="bullet">
    ///   <item><description>Berserker (rage): Gains 1 rage per 5 damage</description></item>
    ///   <item><description>Storm Blade (momentum): Loses 20 momentum on critical hits</description></item>
    ///   <item><description>Arcanist (coherence): Loses 15 coherence on interrupts</description></item>
    /// </list>
    /// </remarks>
    private (int? rageGained, int? momentumLost, int? coherenceLost) ApplySpecializationEffects(
        Guid characterId,
        int damage,
        DamageContext context)
    {
        int? rageGained = null;
        int? momentumLost = null;
        int? coherenceLost = null;

        // Determine specialization type by checking which services have state for this character
        // The presence of a valid state in a resource service indicates specialization type
        string? specializationType = null;
        
        // Check for Berserker (rage-based)
        if (_rageService != null)
        {
            try
            {
                var rageState = _rageService.GetRageState(characterId);
                if (rageState != null)
                {
                    specializationType = "rage";
                }
            }
            catch
            {
                // Character doesn't have rage state
            }
        }
        
        // Check for Storm Blade (momentum-based)
        if (specializationType == null && _momentumService != null)
        {
            try
            {
                var momentumState = _momentumService.GetMomentumState(characterId);
                if (momentumState != null)
                {
                    specializationType = "momentum";
                }
            }
            catch
            {
                // Character doesn't have momentum state
            }
        }
        
        // Check for Arcanist (coherence-based)
        if (specializationType == null && _coherenceService != null)
        {
            try
            {
                var coherenceState = _coherenceService.GetCoherenceState(characterId);
                if (coherenceState != null)
                {
                    specializationType = "coherence";
                }
            }
            catch
            {
                // Character doesn't have coherence state
            }
        }

        // Berserker: Gain rage from damage taken
        if (specializationType == "rage" && _rageService != null)
        {
            rageGained = damage / RagePerDamageDivisor;
            if (rageGained > 0)
            {
                _logger.LogDebug(
                    "Berserker rage gain: +{RageGained} from {Damage} damage",
                    rageGained,
                    damage);
            }
        }

        // Storm Blade: Lose momentum on critical hits
        if (specializationType == "momentum" && _momentumService != null)
        {
            if (context.IsCriticalHit)
            {
                momentumLost = MomentumLostOnCritical;
                _logger.LogDebug(
                    "Storm Blade momentum loss: -{MomentumLost} from critical hit",
                    momentumLost);
            }
        }

        // Arcanist: Lose coherence if interrupted while casting
        if (specializationType == "coherence" && _coherenceService != null)
        {
            if (context.IsInterrupt)
            {
                coherenceLost = CoherenceLostOnInterrupt;
                _logger.LogDebug(
                    "Arcanist coherence loss: -{CoherenceLost} from interrupt",
                    coherenceLost);
            }
        }

        return (rageGained, momentumLost, coherenceLost);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Private Methods - Trauma Triggers
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Determines if damage should trigger a trauma check.
    /// </summary>
    /// <param name="characterId">The character to check.</param>
    /// <param name="context">Damage context.</param>
    /// <param name="finalDamage">Damage after soak.</param>
    /// <returns>True if trauma check should be triggered.</returns>
    /// <remarks>
    /// <para>
    /// <strong>Trauma Triggers:</strong>
    /// </para>
    /// <list type="bullet">
    ///   <item><description>Near-death: Damage would reduce HP to 0 or below</description></item>
    ///   <item><description>Ally death: Witnessing a party member fall</description></item>
    /// </list>
    /// </remarks>
    private bool CheckTraumaTrigger(Guid characterId, DamageContext context, int finalDamage)
    {
        // Near-death event: HP would drop to 0 or below
        // TODO: Get actual HP from character when available
        var currentHp = 50; // Placeholder
        if (currentHp - finalDamage <= 0)
        {
            _logger.LogDebug(
                "Trauma trigger: Near-death (HP would drop from {CurrentHp} to {NewHp})",
                currentHp,
                currentHp - finalDamage);
            return true;
        }

        // Ally death witnessed
        if (context.IsAllyDeathEvent)
        {
            _logger.LogDebug("Trauma trigger: Ally death witnessed");
            return true;
        }

        return false;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Private Methods - Result Building
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Builds the complete result object with all messages and thresholds.
    /// </summary>
    private DamageIntegrationResult BuildResult(
        int damage,
        int damageAfterSoak,
        int soak,
        int stressGain,
        int? rageGained,
        int? momentumLost,
        int? coherenceLost,
        bool traumaCheckTriggered,
        DamageContext context)
    {
        var thresholds = new List<string>();
        var messages = new List<string>();

        // Base damage message
        messages.Add($"Took {damageAfterSoak} damage (soak {soak}).");

        // Critical hit
        if (context.IsCriticalHit)
        {
            messages.Add("Critical hit—mind reels from impact!");
            thresholds.Add("CriticalHit");
        }

        // Near-death message
        // TODO: Get actual HP percentage when available
        var currentHp = 50;
        var maxHp = 100;
        if (currentHp - damageAfterSoak < (maxHp * NearDeathThreshold))
        {
            messages.Add("near-death grasp tightens...");
            thresholds.Add("NearDeath");
        }

        // Rage gained message
        if (rageGained.HasValue && rageGained > 0)
        {
            messages.Add($"Fury surges (+{rageGained} rage).");
        }

        // Momentum lost message
        if (momentumLost.HasValue && momentumLost > 0)
        {
            messages.Add($"Your momentum shatters! (-{momentumLost} momentum)");
            thresholds.Add("MomentumLost");
        }

        // Coherence lost message
        if (coherenceLost.HasValue && coherenceLost > 0)
        {
            messages.Add($"Your focus wavers! (-{coherenceLost} coherence)");
            thresholds.Add("CoherenceLost");
        }

        // Trauma check message
        if (traumaCheckTriggered)
        {
            messages.Add("A scar is etched into your mind...");
            thresholds.Add("TraumaCheck");
        }

        // Determine stress source (Combat covers all physical damage)
        var stressSource = StressSource.Combat;

        return DamageIntegrationResult.Create(
            damageDealt: damage,
            damageAfterSoak: damageAfterSoak,
            soakApplied: soak,
            stressGained: stressGain,
            stressSource: stressSource,
            rageGained: rageGained,
            momentumLost: momentumLost,
            coherenceLost: coherenceLost,
            traumaCheckTriggered: traumaCheckTriggered,
            thresholdsCrossed: thresholds.AsReadOnly(),
            transitionMessages: messages.AsReadOnly()
        );
    }
}

// ═══════════════════════════════════════════════════════════════════════════════
// DamageContext Record
// ═══════════════════════════════════════════════════════════════════════════════

/// <summary>
/// Context information for damage events.
/// </summary>
/// <remarks>
/// <para>
/// Provides flags and metadata about a damage event that affect how the
/// damage is processed through trauma economy systems.
/// </para>
/// </remarks>
/// <param name="IsCriticalHit">Whether the damage is from a critical hit.</param>
/// <param name="IsInterrupt">Whether the damage interrupted a casting action.</param>
/// <param name="IsAllyDeathEvent">Whether this is stress from witnessing ally death.</param>
/// <param name="AttackerId">Optional ID of the attacker for tracking.</param>
/// <example>
/// <code>
/// // Normal damage
/// var context = new DamageContext();
///
/// // Critical hit damage
/// var critContext = new DamageContext(IsCriticalHit: true);
///
/// // Interrupt during casting
/// var interruptContext = new DamageContext(IsInterrupt: true, AttackerId: enemyId);
/// </code>
/// </example>
public record DamageContext(
    bool IsCriticalHit = false,
    bool IsInterrupt = false,
    bool IsAllyDeathEvent = false,
    Guid? AttackerId = null);
