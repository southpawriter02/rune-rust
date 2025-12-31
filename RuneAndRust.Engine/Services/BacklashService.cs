using Microsoft.Extensions.Logging;
using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Events;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Core.Models.Combat;
using RuneAndRust.Core.Models.Magic;

namespace RuneAndRust.Engine.Services;

/// <summary>
/// Manages magical backlash mechanics and corruption tracking.
/// The price of power—when the weave unravels, the caster pays in blood and sanity.
/// </summary>
/// <remarks>
/// See: v0.4.3d (The Backlash) for implementation details.
/// </remarks>
public class BacklashService : IBacklashService
{
    private readonly IAetherService _aetherService;
    private readonly IDiceService _diceService;
    private readonly IStatusEffectService _statusEffects;
    private readonly IEventBus _eventBus;
    private readonly ILogger<BacklashService> _logger;

    /// <summary>
    /// Flux level above which backlash risk begins.
    /// </summary>
    private const int CriticalThreshold = 50;

    /// <summary>
    /// Severity boundaries for fail margin.
    /// </summary>
    private const int MinorMaxMargin = 10;
    private const int MajorMaxMargin = 25;

    /// <summary>
    /// Corruption level thresholds.
    /// </summary>
    private const int TaintedThreshold = 10;
    private const int AfflictedThreshold = 25;
    private const int BlightedThreshold = 50;
    private const int LostThreshold = 75;

    /// <summary>
    /// Aether Sickness durations by severity.
    /// </summary>
    private const int MajorSicknessDuration = 2;
    private const int CatastrophicSicknessDuration = 5;

    public BacklashService(
        IAetherService aetherService,
        IDiceService diceService,
        IStatusEffectService statusEffects,
        IEventBus eventBus,
        ILogger<BacklashService> logger)
    {
        _aetherService = aetherService;
        _diceService = diceService;
        _statusEffects = statusEffects;
        _eventBus = eventBus;
        _logger = logger;

        _logger.LogDebug("[Backlash] BacklashService initialized. Critical threshold: {Threshold}",
            CriticalThreshold);
    }

    /// <inheritdoc/>
    public BacklashResult CheckBacklash(Combatant caster, string? spellName = null)
    {
        var flux = _aetherService.CurrentFlux;
        _logger.LogDebug("[Backlash] Checking backlash for {Caster}. Flux: {Flux}, Spell: {Spell}",
            caster.Name, flux, spellName ?? "(unknown)");

        // Calculate risk
        var risk = GetCurrentRisk();

        if (risk <= 0)
        {
            _logger.LogDebug("[Backlash] Flux {Flux} below critical ({Threshold}). No risk.",
                flux, CriticalThreshold);
            return BacklashResult.NoBacklash();
        }

        _logger.LogInformation("[Backlash] {Caster} casting at {Risk}% risk (Flux: {Flux})",
            caster.Name, risk, flux);

        // Roll for backlash (d100, need to roll > risk to avoid backlash)
        var roll = _diceService.RollSingle(100, $"Backlash check for {caster.Name}");

        if (roll > risk)
        {
            _logger.LogDebug("[Backlash] Roll {Roll} > Risk {Risk}. Safe cast.",
                roll, risk);
            return BacklashResult.NoBacklash(risk, roll);
        }

        // BACKLASH TRIGGERED
        _logger.LogWarning("[Backlash] BACKLASH! {Caster} rolled {Roll} <= {Risk}",
            caster.Name, roll, risk);

        // Determine severity based on how badly the roll failed
        var margin = risk - roll;
        var severity = DetermineSeverity(margin);

        _logger.LogInformation("[Backlash] Severity: {Severity} (margin: {Margin})",
            severity, margin);

        // Apply backlash effects
        var result = ApplyBacklashEffects(caster, severity, risk, roll, flux, spellName);

        return result;
    }

    /// <inheritdoc/>
    public int GetCurrentRisk()
    {
        var flux = _aetherService.CurrentFlux;
        return Math.Max(0, flux - CriticalThreshold);
    }

    /// <inheritdoc/>
    public bool IsAtRisk()
    {
        return GetCurrentRisk() > 0;
    }

    /// <inheritdoc/>
    public CorruptionLevel GetCorruptionLevel(Character character)
    {
        var corruption = character.Corruption;
        return MapCorruptionToLevel(corruption);
    }

    /// <inheritdoc/>
    public CorruptionLevel GetCorruptionLevel(Combatant combatant)
    {
        if (combatant.CharacterSource == null)
        {
            return CorruptionLevel.Untouched;
        }

        return GetCorruptionLevel(combatant.CharacterSource);
    }

    /// <inheritdoc/>
    public CorruptionPenalties GetCorruptionPenalties(Character character)
    {
        var level = GetCorruptionLevel(character);
        return BuildPenalties(level, character.Corruption);
    }

    /// <inheritdoc/>
    public CorruptionPenalties GetCorruptionPenalties(Combatant combatant)
    {
        if (combatant.CharacterSource == null)
        {
            return CorruptionPenalties.Untouched(0);
        }

        return GetCorruptionPenalties(combatant.CharacterSource);
    }

    /// <inheritdoc/>
    public void AddCorruption(Character character, int amount, string source)
    {
        var previousCorruption = character.Corruption;
        var previousLevel = GetCorruptionLevel(character);

        character.Corruption = Math.Clamp(character.Corruption + amount, 0, 100);

        var newLevel = GetCorruptionLevel(character);

        _logger.LogWarning("[Corruption] {Character} gained {Amount} corruption from {Source}. " +
            "Total: {Total} ({Level})",
            character.Name, amount, source, character.Corruption, newLevel);

        // Publish event
        var corruptionEvent = new CorruptionChangedEvent(
            character.Id,
            character.Name,
            previousCorruption,
            character.Corruption,
            previousLevel,
            newLevel,
            source);

        _eventBus.Publish(corruptionEvent);

        // Log tier change
        if (previousLevel != newLevel)
        {
            _logger.LogWarning("[Corruption] TIER CHANGE: {Character} is now {Level}",
                character.Name, newLevel);

            if (newLevel == CorruptionLevel.Lost)
            {
                _logger.LogError("[Corruption] {Character} has been LOST to corruption! " +
                    "Soul consumed. Cannot cast spells.",
                    character.Name);
            }
        }
    }

    /// <inheritdoc/>
    public bool PurgeCorruption(Character character, int amount, string source)
    {
        if (character.Corruption <= 0)
        {
            _logger.LogDebug("[Corruption] {Character} has no corruption to purge",
                character.Name);
            return false;
        }

        var previousCorruption = character.Corruption;
        var previousLevel = GetCorruptionLevel(character);

        character.Corruption = Math.Max(0, character.Corruption - amount);

        var newLevel = GetCorruptionLevel(character);

        _logger.LogInformation("[Corruption] {Character} purged {Amount} corruption via {Source}. " +
            "Total: {Total} ({Level})",
            character.Name, amount, source, character.Corruption, newLevel);

        // Publish event
        var corruptionEvent = new CorruptionChangedEvent(
            character.Id,
            character.Name,
            previousCorruption,
            character.Corruption,
            previousLevel,
            newLevel,
            source);

        _eventBus.Publish(corruptionEvent);

        if (previousLevel != newLevel)
        {
            _logger.LogInformation("[Corruption] TIER IMPROVEMENT: {Character} recovered to {Level}",
                character.Name, newLevel);
        }

        return true;
    }

    /// <inheritdoc/>
    public bool CanCastSpells(Character character)
    {
        var level = GetCorruptionLevel(character);
        var canCast = level != CorruptionLevel.Lost;

        if (!canCast)
        {
            _logger.LogDebug("[Corruption] {Character} is Lost - cannot cast spells",
                character.Name);
        }

        return canCast;
    }

    /// <inheritdoc/>
    public string GetRiskWarning()
    {
        var risk = GetCurrentRisk();

        if (risk <= 0)
        {
            return string.Empty;
        }

        return risk switch
        {
            <= 10 => $"[yellow]Caution: {risk}% backlash risk[/]",
            <= 25 => $"[orange1]Warning: {risk}% backlash risk[/]",
            <= 40 => $"[red]Danger: {risk}% backlash risk![/]",
            _ => $"[red bold]CRITICAL: {risk}% backlash risk! Consider waiting.[/]"
        };
    }

    #region Private Methods

    /// <summary>
    /// Maps fail margin to severity level.
    /// </summary>
    private static BacklashSeverity DetermineSeverity(int margin)
    {
        return margin switch
        {
            <= MinorMaxMargin => BacklashSeverity.Minor,      // 1-10
            <= MajorMaxMargin => BacklashSeverity.Major,      // 11-25
            _ => BacklashSeverity.Catastrophic                 // 26+
        };
    }

    /// <summary>
    /// Applies backlash effects based on severity.
    /// </summary>
    private BacklashResult ApplyBacklashEffects(
        Combatant caster,
        BacklashSeverity severity,
        int risk,
        int roll,
        int flux,
        string? spellName)
    {
        // Determine damage dice count
        var damageDice = severity switch
        {
            BacklashSeverity.Minor => 1,
            BacklashSeverity.Major => 2,
            BacklashSeverity.Catastrophic => 3,
            _ => 0
        };

        // Roll damage
        var damage = 0;
        for (var i = 0; i < damageDice; i++)
        {
            damage += _diceService.RollSingle(6, $"Backlash damage {i + 1}/{damageDice}");
        }

        // Apply damage to caster
        var previousHp = caster.CurrentHp;
        caster.CurrentHp -= damage;
        var wasLethal = caster.CurrentHp <= 0;

        _logger.LogWarning("[Backlash] {Caster} takes {Damage} backlash damage ({Dice}d6). HP: {Previous} -> {Current}",
            caster.Name, damage, damageDice, previousHp, caster.CurrentHp);

        // Determine Aether Sickness duration
        var sicknessDuration = severity switch
        {
            BacklashSeverity.Major => MajorSicknessDuration,
            BacklashSeverity.Catastrophic => CatastrophicSicknessDuration,
            _ => 0
        };

        // Apply Aether Sickness
        if (sicknessDuration > 0)
        {
            _statusEffects.ApplyEffect(caster, StatusEffectType.AetherSickness, sicknessDuration, caster.Id);
            _logger.LogWarning("[Backlash] {Caster} afflicted with Aether Sickness for {Duration} turns",
                caster.Name, sicknessDuration);
        }

        // Apply Corruption (Catastrophic only)
        var corruption = 0;
        if (severity == BacklashSeverity.Catastrophic && caster.CharacterSource != null)
        {
            corruption = 1;
            AddCorruption(caster.CharacterSource, corruption, "Catastrophic Backlash");
        }

        // Build narrative message
        var message = BuildBacklashMessage(caster, severity, damage, sicknessDuration, corruption);

        // Publish BacklashEvent
        var backlashEvent = new BacklashEvent(
            caster.Id,
            caster.Name,
            severity,
            damage,
            sicknessDuration,
            corruption,
            risk,
            roll,
            flux,
            spellName)
        {
            WasLethal = wasLethal
        };

        _eventBus.Publish(backlashEvent);

        _logger.LogDebug("[Backlash] Published BacklashEvent: {Severity} for {Caster}",
            severity, caster.Name);

        // Log if caster was knocked out
        if (wasLethal)
        {
            _logger.LogError("[Backlash] {Caster} was knocked unconscious by backlash damage!",
                caster.Name);
        }

        return BacklashResult.Backlash(
            severity,
            damage,
            sicknessDuration,
            corruption,
            message,
            risk,
            roll);
    }

    /// <summary>
    /// Builds Domain 4 compliant narrative message for backlash.
    /// </summary>
    private static string BuildBacklashMessage(
        Combatant caster,
        BacklashSeverity severity,
        int damage,
        int sicknessDuration,
        int corruption)
    {
        var baseMessage = severity switch
        {
            BacklashSeverity.Minor =>
                $"The weave snaps back, stinging {caster.Name} for {damage} damage!",

            BacklashSeverity.Major =>
                $"Arcane energies tear through {caster.Name}, dealing {damage} damage " +
                $"and leaving them reeling with Aether Sickness for {sicknessDuration} turns!",

            BacklashSeverity.Catastrophic =>
                $"Reality itself rejects the spell! {caster.Name} screams as {damage} damage " +
                $"tears through their body. Aether Sickness consumes them for {sicknessDuration} turns " +
                $"as corruption takes root in their soul!",

            _ => string.Empty
        };

        return baseMessage;
    }

    /// <summary>
    /// Maps corruption value to level enum.
    /// </summary>
    private static CorruptionLevel MapCorruptionToLevel(int corruption)
    {
        return corruption switch
        {
            >= LostThreshold => CorruptionLevel.Lost,           // 75-100
            >= BlightedThreshold => CorruptionLevel.Blighted,   // 50-74
            >= AfflictedThreshold => CorruptionLevel.Afflicted, // 25-49
            >= TaintedThreshold => CorruptionLevel.Tainted,     // 10-24
            _ => CorruptionLevel.Untouched                       // 0-9
        };
    }

    /// <summary>
    /// Builds penalties record for a corruption level.
    /// </summary>
    private static CorruptionPenalties BuildPenalties(CorruptionLevel level, int corruption)
    {
        return level switch
        {
            CorruptionLevel.Untouched => CorruptionPenalties.Untouched(corruption),
            CorruptionLevel.Tainted => CorruptionPenalties.Tainted(corruption),
            CorruptionLevel.Afflicted => CorruptionPenalties.Afflicted(corruption),
            CorruptionLevel.Blighted => CorruptionPenalties.Blighted(corruption),
            CorruptionLevel.Lost => CorruptionPenalties.Lost(corruption),
            _ => CorruptionPenalties.Untouched(corruption)
        };
    }

    #endregion
}
