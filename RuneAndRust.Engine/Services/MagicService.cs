using System.Text;
using Microsoft.Extensions.Logging;
using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Events;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Core.Models.Combat;
using RuneAndRust.Core.Models.Magic;

namespace RuneAndRust.Engine.Services;

/// <summary>
/// Implements spell casting mechanics and validation.
/// Mirrors the AbilityService pattern but adds Flux integration via IAetherService.
/// </summary>
/// <remarks>
/// See: v0.4.3c (The Incantation) for implementation details.
///
/// Validation Pipeline (7 checks in order):
/// 1. Magic eligibility (Archetype: Adept or Mystic)
/// 2. AP cost affordability
/// 3. Target type compatibility
/// 4. Range validation
/// 5. Target alive check
/// 6. Silenced status check
/// 7. Concentration conflict check
/// </remarks>
public class MagicService : IMagicService
{
    private readonly IAetherService _aetherService;
    private readonly IStatusEffectService _statusEffects;
    private readonly EffectScriptExecutor _scriptExecutor;
    private readonly IEventBus _eventBus;
    private readonly ILogger<MagicService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="MagicService"/> class.
    /// </summary>
    /// <param name="aetherService">Service for flux management.</param>
    /// <param name="statusEffects">Service for status effect management.</param>
    /// <param name="scriptExecutor">Shared utility for effect script execution.</param>
    /// <param name="eventBus">Event bus for publishing SpellCastEvent.</param>
    /// <param name="logger">Logger for traceability.</param>
    public MagicService(
        IAetherService aetherService,
        IStatusEffectService statusEffects,
        EffectScriptExecutor scriptExecutor,
        IEventBus eventBus,
        ILogger<MagicService> logger)
    {
        _aetherService = aetherService;
        _statusEffects = statusEffects;
        _scriptExecutor = scriptExecutor;
        _eventBus = eventBus;
        _logger = logger;

        _logger.LogInformation("[Magic] MagicService initialized");
    }

    /// <inheritdoc/>
    public bool CanCast(Combatant caster, Spell spell, Combatant? target)
    {
        var reason = GetFailureReason(caster, spell, target);
        return reason == CastFailureReason.None;
    }

    /// <inheritdoc/>
    public CastFailureReason GetFailureReason(Combatant caster, Spell spell, Combatant? target)
    {
        _logger.LogDebug(
            "[Magic] {Caster} checking if can cast {Spell}",
            caster.Name, spell.Name);

        // 1. Magic eligibility - must be Adept or Mystic
        if (!IsMagicUser(caster))
        {
            _logger.LogDebug(
                "[Magic] {Caster} cannot cast: not a magic user (Archetype: {Archetype})",
                caster.Name, caster.CharacterSource?.Archetype);
            return CastFailureReason.NotMagicUser;
        }

        // 2. AP cost affordability
        if (caster.CurrentAp < spell.ApCost)
        {
            _logger.LogDebug(
                "[Magic] {Caster} cannot cast {Spell}: insufficient AP ({Current}/{Required})",
                caster.Name, spell.Name, caster.CurrentAp, spell.ApCost);
            return CastFailureReason.InsufficientAP;
        }

        // 3. Target type compatibility
        if (target != null && !ValidateTargetType(caster, spell, target))
        {
            _logger.LogDebug(
                "[Magic] {Caster} cannot cast {Spell}: invalid target type",
                caster.Name, spell.Name);
            return CastFailureReason.InvalidTarget;
        }

        // 4. Range validation (skip for self-targeting spells)
        if (target != null && target != caster && !ValidateRange(caster, spell, target))
        {
            _logger.LogDebug(
                "[Magic] {Caster} cannot cast {Spell}: target out of range",
                caster.Name, spell.Name);
            return CastFailureReason.OutOfRange;
        }

        // 5. Target alive check (skip for resurrection spells in future)
        if (target != null && target.CurrentHp <= 0)
        {
            _logger.LogDebug(
                "[Magic] {Caster} cannot cast {Spell}: target is dead",
                caster.Name, spell.Name);
            return CastFailureReason.TargetDead;
        }

        // 6. Silenced status check
        if (_statusEffects.HasEffect(caster, StatusEffectType.Silenced))
        {
            _logger.LogDebug(
                "[Magic] {Caster} cannot cast {Spell}: silenced",
                caster.Name, spell.Name);
            return CastFailureReason.Silenced;
        }

        // 7. Concentration conflict check
        if (spell.RequiresConcentration &&
            _statusEffects.HasEffect(caster, StatusEffectType.Concentrating))
        {
            _logger.LogDebug(
                "[Magic] {Caster} cannot cast {Spell}: already concentrating",
                caster.Name, spell.Name);
            return CastFailureReason.ConcentrationConflict;
        }

        _logger.LogDebug(
            "[Magic] {Caster} can cast {Spell}",
            caster.Name, spell.Name);

        return CastFailureReason.None;
    }

    /// <inheritdoc/>
    public MagicResult CastSpell(Combatant caster, Spell spell, Combatant target)
    {
        _logger.LogInformation(
            "[Magic] {Caster} casts {Spell} on {Target}",
            caster.Name, spell.Name, target.Name);

        // Validate cast
        var failureReason = GetFailureReason(caster, spell, target);
        if (failureReason != CastFailureReason.None)
        {
            _logger.LogWarning(
                "[Magic] {Caster} failed to cast {Spell}: {Reason}",
                caster.Name, spell.Name, failureReason);
            return MagicResult.Failure(failureReason);
        }

        // Check if this is a charged spell initiation
        if (spell.IsChargedSpell && !_statusEffects.HasEffect(caster, StatusEffectType.Chanting))
        {
            return InitiateCharge(caster, spell);
        }

        // Check if this is a charge release (caster is chanting)
        if (_statusEffects.HasEffect(caster, StatusEffectType.Chanting))
        {
            return ReleaseCharge(caster, spell, target);
        }

        // Instant cast - deduct AP and generate flux
        DeductApCost(caster, spell);
        var fluxGenerated = GenerateFlux(spell);

        // Apply concentration if required
        if (spell.RequiresConcentration)
        {
            _statusEffects.ApplyEffect(caster, StatusEffectType.Concentrating, 99, caster.Id);
            _logger.LogDebug("[Magic] {Caster} is now concentrating on {Spell}", caster.Name, spell.Name);
        }

        // Execute effect script
        var result = ExecuteSpellEffect(caster, target, spell, fluxGenerated);

        // Publish spell cast event
        PublishSpellCastEvent(caster, spell, target, result, fluxGenerated, isChargeInitiation: false);

        return result;
    }

    /// <inheritdoc/>
    public MagicResult InitiateCharge(Combatant caster, Spell spell)
    {
        _logger.LogInformation(
            "[Magic] {Caster} begins charging {Spell}",
            caster.Name, spell.Name);

        // Deduct AP immediately (committed to the action)
        DeductApCost(caster, spell);

        // Generate flux on initiation
        var fluxGenerated = GenerateFlux(spell);

        // Apply Chanting status with duration = ChargeTurns
        _statusEffects.ApplyEffect(caster, StatusEffectType.Chanting, spell.ChargeTurns, caster.Id);

        // Store which spell is being channeled (using ChanneledAbilityId for now)
        caster.ChanneledSpellId = spell.Id;

        // Build telegraph message
        var message = spell.TelegraphMessage ?? $"{caster.Name} begins channeling {spell.Name}!";
        var result = MagicResult.ChargeInitiated($"⚠ {message}", fluxGenerated);

        // Publish spell cast event for audio/visual feedback
        PublishSpellCastEvent(caster, spell, caster, result, fluxGenerated, isChargeInitiation: true);

        _logger.LogDebug(
            "[Magic] {Caster} initiated charge for {Spell}. Duration: {Turns} turns",
            caster.Name, spell.Name, spell.ChargeTurns);

        return result;
    }

    /// <inheritdoc/>
    public MagicResult ReleaseCharge(Combatant caster, Spell spell, Combatant target)
    {
        _logger.LogInformation(
            "[Magic] {Caster} releases {Spell}!",
            caster.Name, spell.Name);

        // Clear channeling state
        caster.ChanneledSpellId = null;
        _statusEffects.RemoveEffect(caster, StatusEffectType.Chanting);

        // Apply concentration if required
        if (spell.RequiresConcentration)
        {
            _statusEffects.ApplyEffect(caster, StatusEffectType.Concentrating, 99, caster.Id);
        }

        // Execute the actual effect (flux was generated on initiation)
        var result = ExecuteSpellEffect(caster, target, spell, fluxGenerated: 0);

        // Build release message
        var releaseMessage = $"{caster.Name} unleashes {spell.Name}! {result.Message}";
        var finalResult = MagicResult.Ok(
            releaseMessage,
            result.TotalDamage,
            result.TotalHealing,
            result.StatusesApplied,
            result.FluxGenerated);

        // Publish spell cast event
        PublishSpellCastEvent(caster, spell, target, finalResult, 0, isChargeInitiation: false);

        return finalResult;
    }

    /// <inheritdoc/>
    public bool ValidateTarget(Combatant caster, Spell spell, Combatant target)
    {
        return ValidateTargetType(caster, spell, target) &&
               (target == caster || ValidateRange(caster, spell, target));
    }

    #region Private Methods

    /// <summary>
    /// Checks if the caster's archetype allows magic use.
    /// </summary>
    private static bool IsMagicUser(Combatant caster)
    {
        if (caster.CharacterSource == null) return false;

        var archetype = caster.CharacterSource.Archetype;
        return archetype == ArchetypeType.Adept || archetype == ArchetypeType.Mystic;
    }

    /// <summary>
    /// Validates that the target matches the spell's target type.
    /// </summary>
    private bool ValidateTargetType(Combatant caster, Spell spell, Combatant target)
    {
        return spell.TargetType switch
        {
            SpellTargetType.Self => target.Id == caster.Id,
            SpellTargetType.SingleEnemy => !target.IsPlayer && target.Id != caster.Id,
            SpellTargetType.SingleAlly => target.IsPlayer && target.Id != caster.Id,
            SpellTargetType.SingleAny => true,
            SpellTargetType.AllEnemies => !target.IsPlayer,
            SpellTargetType.AllAllies => target.IsPlayer,
            SpellTargetType.Area => true, // Area spells can target any location
            _ => false
        };
    }

    /// <summary>
    /// Validates that the target is within the spell's range.
    /// For now, uses simplified row-based range checking.
    /// </summary>
    private bool ValidateRange(Combatant caster, Spell spell, Combatant target)
    {
        // Touch spells require same row or adjacent
        if (spell.Range == SpellRange.Touch)
        {
            // Same side, same row is always valid
            if (caster.IsPlayer == target.IsPlayer && caster.Row == target.Row)
                return true;

            // Different sides - caster must be Front row or target must be Front row
            if (caster.IsPlayer != target.IsPlayer)
                return caster.Row == RowPosition.Front || target.Row == RowPosition.Front;

            return true;
        }

        // All other ranges are valid in current combat implementation
        // Future: Add proper distance calculation with spatial grid
        return true;
    }

    /// <summary>
    /// Deducts the AP cost from the caster.
    /// </summary>
    private void DeductApCost(Combatant caster, Spell spell)
    {
        caster.CurrentAp -= spell.ApCost;
        _logger.LogDebug(
            "[Magic] {Caster} spent {Cost} AP. Remaining: {Current}",
            caster.Name, spell.ApCost, caster.CurrentAp);
    }

    /// <summary>
    /// Generates environmental flux from the spell cast.
    /// </summary>
    private int GenerateFlux(Spell spell)
    {
        if (spell.FluxCost <= 0) return 0;

        var newFlux = _aetherService.AddFlux(spell.FluxCost);
        _logger.LogDebug(
            "[Magic] Generated {Flux} flux from {Spell}. Total: {Total}",
            spell.FluxCost, spell.Name, newFlux);

        return spell.FluxCost;
    }

    /// <summary>
    /// Executes the spell's effect script and builds the result.
    /// </summary>
    private MagicResult ExecuteSpellEffect(
        Combatant caster,
        Combatant target,
        Spell spell,
        int fluxGenerated)
    {
        if (string.IsNullOrWhiteSpace(spell.EffectScript))
        {
            _logger.LogWarning("[Magic] {Spell} has no EffectScript", spell.Name);
            return MagicResult.Ok(
                $"{caster.Name} casts {spell.Name}, but nothing happens.",
                flux: fluxGenerated);
        }

        // Delegate to shared executor
        var scriptResult = _scriptExecutor.Execute(
            spell.EffectScript,
            target,
            spell.Name,
            caster.Id);

        // Build combined narrative
        var message = new StringBuilder();
        message.Append($"{caster.Name} casts {spell.Name}");

        if (target != caster)
        {
            message.Append($" on {target.Name}");
        }

        message.Append('!');

        if (!string.IsNullOrEmpty(scriptResult.Narrative))
        {
            message.Append(' ');
            message.Append(scriptResult.Narrative);
        }

        var statusTypes = scriptResult.StatusesApplied.Count > 0
            ? scriptResult.StatusesApplied
                .Select(s => Enum.TryParse<StatusEffectType>(s, out var t) ? t : (StatusEffectType?)null)
                .Where(t => t.HasValue)
                .Select(t => t!.Value)
                .ToList()
            : null;

        return MagicResult.Ok(
            message.ToString(),
            scriptResult.TotalDamage,
            scriptResult.TotalHealing,
            statusTypes,
            fluxGenerated);
    }

    /// <summary>
    /// Publishes a SpellCastEvent for audio/visual feedback.
    /// </summary>
    private void PublishSpellCastEvent(
        Combatant caster,
        Spell spell,
        Combatant target,
        MagicResult result,
        int fluxGenerated,
        bool isChargeInitiation)
    {
        var spellEvent = new SpellCastEvent(
            CasterId: caster.Id,
            CasterName: caster.Name,
            SpellId: spell.Id,
            SpellName: spell.Name,
            School: spell.School,
            TargetId: target.Id,
            TargetName: target.Name,
            FluxGenerated: fluxGenerated,
            IsChargeInitiation: isChargeInitiation,
            DamageDealt: result.TotalDamage,
            HealingDone: result.TotalHealing);

        _eventBus.Publish(spellEvent);

        _logger.LogDebug(
            "[Magic] Published SpellCastEvent: {Spell} by {Caster}",
            spell.Name, caster.Name);
    }

    #endregion
}
