// ═══════════════════════════════════════════════════════════════════════════════
// ProficiencyCheckService.cs
// Service implementation providing proficiency-based combat modifiers and
// experience recording.
// Version: 0.16.1f
// ═══════════════════════════════════════════════════════════════════════════════

using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Services;

/// <summary>
/// Provides proficiency-based combat modifiers and experience recording.
/// </summary>
/// <remarks>
/// <para>
/// ProficiencyCheckService is the centralized service for all proficiency-based
/// combat calculations. It queries proficiency levels from <see cref="CharacterProficiencies"/>,
/// retrieves effect data from <see cref="IProficiencyEffectProvider"/>, and delegates
/// experience recording to <see cref="IProficiencyAcquisitionService"/>.
/// </para>
/// <para>
/// This service is designed for combat integration and follows these patterns:
/// </para>
/// <list type="bullet">
///   <item><description>Single retrieval: Call <see cref="GetCombatModifiers"/> once per attack</description></item>
///   <item><description>Centralized calculation: All modifier logic is here, not in combat service</description></item>
///   <item><description>Automatic recording: Combat usage is tracked for proficiency advancement</description></item>
/// </list>
/// <para>
/// Logging behavior:
/// </para>
/// <list type="bullet">
///   <item><description>Debug: Modifier calculations, technique checks</description></item>
///   <item><description>Information: Proficiency level advancements</description></item>
/// </list>
/// </remarks>
/// <seealso cref="IProficiencyCheckService"/>
/// <seealso cref="CombatProficiencyModifiers"/>
/// <seealso cref="IProficiencyEffectProvider"/>
/// <seealso cref="IProficiencyAcquisitionService"/>
public class ProficiencyCheckService : IProficiencyCheckService
{
    // ═══════════════════════════════════════════════════════════════════════════
    // Dependencies
    // ═══════════════════════════════════════════════════════════════════════════

    private readonly IProficiencyEffectProvider _effectProvider;
    private readonly IProficiencyAcquisitionService _acquisitionService;
    private readonly ILogger<ProficiencyCheckService> _logger;

    // ═══════════════════════════════════════════════════════════════════════════
    // Constructor
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Initializes a new instance of the <see cref="ProficiencyCheckService"/> class.
    /// </summary>
    /// <param name="effectProvider">Provider for proficiency effect data.</param>
    /// <param name="acquisitionService">Service for recording combat experience.</param>
    /// <param name="logger">Logger for diagnostic output.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="effectProvider"/>, <paramref name="acquisitionService"/>,
    /// or <paramref name="logger"/> is null.
    /// </exception>
    public ProficiencyCheckService(
        IProficiencyEffectProvider effectProvider,
        IProficiencyAcquisitionService acquisitionService,
        ILogger<ProficiencyCheckService> logger)
    {
        ArgumentNullException.ThrowIfNull(effectProvider);
        ArgumentNullException.ThrowIfNull(acquisitionService);
        ArgumentNullException.ThrowIfNull(logger);

        _effectProvider = effectProvider;
        _acquisitionService = acquisitionService;
        _logger = logger;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Primary Combat Integration Methods
    // ═══════════════════════════════════════════════════════════════════════════

    /// <inheritdoc/>
    public CombatProficiencyModifiers GetCombatModifiers(
        CharacterProficiencies proficiencies,
        WeaponCategory weaponCategory)
    {
        ArgumentNullException.ThrowIfNull(proficiencies);

        // Get current proficiency level for this weapon category
        var level = proficiencies.GetLevel(weaponCategory);

        // Get the effect configuration for this level
        var effect = _effectProvider.GetEffect(level);

        // Create combat modifiers from the effect
        var modifiers = CombatProficiencyModifiers.FromEffect(level, effect);

        _logger.LogDebug(
            "Combat modifiers calculated for {WeaponCategory}: {ProficiencyLevel} " +
            "(Attack: {AttackModifier:+0;-#}, Damage: {DamageModifier:+0;-#}, " +
            "SpecialProps: {CanUseSpecialProperties}, Techniques: {UnlockedTechniques})",
            weaponCategory,
            level,
            modifiers.AttackModifier,
            modifiers.DamageModifier,
            modifiers.CanUseSpecialProperties,
            modifiers.UnlockedTechniqueLevel);

        return modifiers;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Individual Modifier Accessors
    // ═══════════════════════════════════════════════════════════════════════════

    /// <inheritdoc/>
    public int GetAttackModifier(CharacterProficiencies proficiencies, WeaponCategory weaponCategory)
    {
        ArgumentNullException.ThrowIfNull(proficiencies);

        var level = proficiencies.GetLevel(weaponCategory);
        var attackModifier = _effectProvider.GetAttackModifier(level);

        _logger.LogDebug(
            "Attack modifier for {WeaponCategory}: {ProficiencyLevel} = {AttackModifier:+0;-#}",
            weaponCategory,
            level,
            attackModifier);

        return attackModifier;
    }

    /// <inheritdoc/>
    public int GetDamageModifier(CharacterProficiencies proficiencies, WeaponCategory weaponCategory)
    {
        ArgumentNullException.ThrowIfNull(proficiencies);

        var level = proficiencies.GetLevel(weaponCategory);
        var damageModifier = _effectProvider.GetDamageModifier(level);

        _logger.LogDebug(
            "Damage modifier for {WeaponCategory}: {ProficiencyLevel} = {DamageModifier:+0;-#}",
            weaponCategory,
            level,
            damageModifier);

        return damageModifier;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Capability Check Methods
    // ═══════════════════════════════════════════════════════════════════════════

    /// <inheritdoc/>
    public bool CanUseSpecialProperties(CharacterProficiencies proficiencies, WeaponCategory weaponCategory)
    {
        ArgumentNullException.ThrowIfNull(proficiencies);

        var level = proficiencies.GetLevel(weaponCategory);
        var canUse = _effectProvider.CanUseSpecialProperties(level);

        _logger.LogDebug(
            "Special property check for {WeaponCategory}: {ProficiencyLevel} = {CanUseSpecialProperties}",
            weaponCategory,
            level,
            canUse);

        return canUse;
    }

    /// <inheritdoc/>
    public bool CanUseTechnique(
        CharacterProficiencies proficiencies,
        WeaponCategory weaponCategory,
        TechniqueAccess techniqueLevel)
    {
        ArgumentNullException.ThrowIfNull(proficiencies);

        var level = proficiencies.GetLevel(weaponCategory);
        var unlockedTechniques = _effectProvider.GetTechniqueAccess(level);

        // Character can use the technique if their unlocked level is >= required level
        var canUse = unlockedTechniques >= techniqueLevel;

        _logger.LogDebug(
            "Technique check for {WeaponCategory}: {ProficiencyLevel} has {UnlockedTechniques}, " +
            "required {RequiredTechniques} = {CanUseTechnique}",
            weaponCategory,
            level,
            unlockedTechniques,
            techniqueLevel,
            canUse);

        return canUse;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Experience Recording Methods
    // ═══════════════════════════════════════════════════════════════════════════

    /// <inheritdoc/>
    public async Task<ProficiencyGainResult> RecordCombatUsageAsync(
        CharacterProficiencies proficiencies,
        WeaponCategory weaponCategory,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(proficiencies);

        _logger.LogDebug(
            "Recording combat usage for {WeaponCategory}",
            weaponCategory);

        // Delegate to the acquisition service
        var result = await _acquisitionService.RecordCombatExperienceAsync(
            proficiencies,
            weaponCategory,
            ct);

        // Log level advancement if it occurred
        if (result.LevelChanged)
        {
            _logger.LogInformation(
                "Proficiency advanced for {WeaponCategory}: {OldLevel} -> {NewLevel}",
                weaponCategory,
                result.OldLevel,
                result.NewLevel);
        }

        return result;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Display Methods
    // ═══════════════════════════════════════════════════════════════════════════

    /// <inheritdoc/>
    public string GetProficiencyDescription(WeaponProficiencyLevel level)
    {
        var effect = _effectProvider.GetEffect(level);

        // Build description string
        var techniqueLabel = effect.UnlockedTechniques switch
        {
            TechniqueAccess.None => "no techniques",
            TechniqueAccess.Basic => "basic techniques",
            TechniqueAccess.Advanced => "advanced techniques",
            TechniqueAccess.Signature => "signature techniques",
            _ => "unknown techniques"
        };

        var specialPropsLabel = effect.CanUseSpecialProperties
            ? "special properties allowed"
            : "special properties blocked";

        return $"{effect.DisplayName}: {effect.FormatAttackModifier()} attack, " +
               $"{effect.FormatDamageModifier()} damage, {techniqueLabel}, {specialPropsLabel}";
    }
}
