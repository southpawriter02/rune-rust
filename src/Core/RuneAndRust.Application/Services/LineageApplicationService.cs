// ═══════════════════════════════════════════════════════════════════════════════
// LineageApplicationService.cs
// Service that orchestrates applying lineage bonuses, traits, and trauma
// baselines to characters during creation.
// Version: 0.17.0f
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Application.Services;

using Microsoft.Extensions.Logging;
using RuneAndRust.Application.DTOs;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Applies lineage components to characters during creation.
/// </summary>
/// <remarks>
/// <para>
/// LineageApplicationService is the primary orchestrator for applying all lineage
/// components to a <see cref="Player"/> entity. It retrieves lineage definitions
/// from <see cref="ILineageProvider"/> and applies them in the following order:
/// </para>
/// <list type="number">
///   <item><description>Validate the lineage selection</description></item>
///   <item><description>Apply attribute modifiers (fixed + flexible bonus)</description></item>
///   <item><description>Apply passive bonuses (HP, AP, Soak, Movement, Skills)</description></item>
///   <item><description>Register the unique lineage trait</description></item>
///   <item><description>Set trauma baseline (Corruption, Stress, resistance modifiers)</description></item>
///   <item><description>Set the lineage on the character</description></item>
/// </list>
/// <para>
/// <strong>Idempotency:</strong> The service prevents duplicate lineage application
/// by checking <see cref="Player.HasLineage"/> before proceeding.
/// </para>
/// <para>
/// <strong>Flexible Bonus:</strong> Clan-Born lineage grants +1 to any attribute
/// of the player's choice. The service validates that:
/// <list type="bullet">
///   <item><description>Clan-Born requires a <c>flexibleBonusAttribute</c> parameter</description></item>
///   <item><description>Non-Clan-Born lineages must NOT provide a <c>flexibleBonusAttribute</c></description></item>
/// </list>
/// </para>
/// </remarks>
/// <seealso cref="ILineageApplicationService"/>
/// <seealso cref="ILineageProvider"/>
/// <seealso cref="LineageApplicationResult"/>
public class LineageApplicationService : ILineageApplicationService
{
    // ═══════════════════════════════════════════════════════════════════════════
    // PRIVATE FIELDS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Provider for accessing lineage definitions from configuration.
    /// </summary>
    private readonly ILineageProvider _lineageProvider;

    /// <summary>
    /// Logger for structured diagnostic output.
    /// </summary>
    private readonly ILogger<LineageApplicationService> _logger;

    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTRUCTOR
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Initializes a new instance of the <see cref="LineageApplicationService"/> class.
    /// </summary>
    /// <param name="lineageProvider">
    /// The lineage provider for accessing lineage definitions.
    /// Must not be null.
    /// </param>
    /// <param name="logger">
    /// The logger for diagnostic output. Must not be null.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="lineageProvider"/> or <paramref name="logger"/> is null.
    /// </exception>
    public LineageApplicationService(
        ILineageProvider lineageProvider,
        ILogger<LineageApplicationService> logger)
    {
        ArgumentNullException.ThrowIfNull(lineageProvider, nameof(lineageProvider));
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));

        _lineageProvider = lineageProvider;
        _logger = logger;

        _logger.LogDebug(
            "LineageApplicationService initialized with ILineageProvider");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // PUBLIC METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <inheritdoc />
    public LineageApplicationResult ApplyLineage(
        Player character,
        Lineage lineage,
        CoreAttribute? flexibleBonusAttribute = null)
    {
        _logger.LogInformation(
            "Beginning lineage application. Lineage={Lineage}, " +
            "FlexibleBonusAttribute={FlexibleBonusAttribute}, " +
            "CharacterId={CharacterId}, CharacterName={CharacterName}",
            lineage,
            flexibleBonusAttribute?.ToString() ?? "None",
            character?.Id.ToString() ?? "null",
            character?.Name ?? "null");

        // Step 1: Validate the lineage selection
        var validation = ValidateLineageSelection(character!, lineage, flexibleBonusAttribute);
        if (!validation.IsValid)
        {
            _logger.LogWarning(
                "Lineage application failed validation. Lineage={Lineage}, " +
                "Reason={Reason}, CharacterId={CharacterId}",
                lineage,
                validation.FailureReason,
                character?.Id.ToString() ?? "null");

            return LineageApplicationResult.Failure(validation.FailureReason!);
        }

        // Step 2: Get the full lineage definition
        var definition = _lineageProvider.GetLineage(lineage)!;

        _logger.LogDebug(
            "Retrieved lineage definition. Lineage={Lineage}, DisplayName={DisplayName}, " +
            "DefinitionId={DefinitionId}",
            lineage,
            definition.DisplayName,
            definition.Id);

        var attributeChanges = new Dictionary<CoreAttribute, int>();

        // Step 3: Apply attribute modifiers (fixed + flexible)
        ApplyAttributeModifiers(character!, definition.AttributeModifiers,
            flexibleBonusAttribute, attributeChanges);

        // Step 4: Apply passive bonuses
        ApplyPassiveBonuses(character!, definition.PassiveBonuses);

        // Step 5: Register unique trait
        RegisterTrait(character!, definition.UniqueTrait);

        // Step 6: Set trauma baseline
        SetTraumaBaseline(character!, definition.TraumaBaseline);

        // Step 7: Set lineage on character
        character!.SetLineage(lineage);

        _logger.LogInformation(
            "Successfully applied lineage to character. " +
            "CharacterId={CharacterId}, CharacterName={CharacterName}, " +
            "Lineage={Lineage}, DisplayName={DisplayName}, " +
            "AttributeChanges={AttributeChangeCount}, " +
            "TraitRegistered={TraitName}, " +
            "StartingCorruption={StartingCorruption}, " +
            "CorruptionResistMod={CorruptionResistMod}, " +
            "StressResistMod={StressResistMod}",
            character.Id,
            character.Name,
            lineage,
            definition.DisplayName,
            attributeChanges.Count,
            definition.UniqueTrait.TraitName,
            definition.TraumaBaseline.StartingCorruption,
            definition.TraumaBaseline.CorruptionResistanceModifier,
            definition.TraumaBaseline.StressResistanceModifier);

        return LineageApplicationResult.Success(
            lineage,
            attributeChanges,
            definition.PassiveBonuses,
            definition.UniqueTrait,
            definition.TraumaBaseline);
    }

    /// <inheritdoc />
    public LineageValidationResult ValidateLineageSelection(
        Player character,
        Lineage lineage,
        CoreAttribute? flexibleBonusAttribute = null)
    {
        _logger.LogDebug(
            "Validating lineage selection. Lineage={Lineage}, " +
            "FlexibleBonusAttribute={FlexibleBonusAttribute}",
            lineage,
            flexibleBonusAttribute?.ToString() ?? "None");

        // Check 1: Character must not be null
        if (character == null)
        {
            _logger.LogWarning(
                "Lineage validation failed: character is null. Lineage={Lineage}",
                lineage);
            return LineageValidationResult.Fail("Character cannot be null");
        }

        // Check 2: Character must not already have a lineage
        if (character.HasLineage)
        {
            _logger.LogWarning(
                "Lineage validation failed: character already has lineage. " +
                "CharacterId={CharacterId}, ExistingLineage={ExistingLineage}, " +
                "AttemptedLineage={AttemptedLineage}",
                character.Id,
                character.SelectedLineage,
                lineage);
            return LineageValidationResult.Fail("Character already has a lineage assigned");
        }

        // Check 3: Lineage must exist in the provider
        var definition = _lineageProvider.GetLineage(lineage);
        if (definition == null)
        {
            _logger.LogWarning(
                "Lineage validation failed: unknown lineage. Lineage={Lineage}, " +
                "CharacterId={CharacterId}",
                lineage,
                character.Id);
            return LineageValidationResult.Fail($"Unknown lineage: {lineage}");
        }

        // Check 4: Flexible bonus validation
        if (definition.AttributeModifiers.HasFlexibleBonus)
        {
            // Clan-Born requires a flexible bonus attribute selection
            if (!flexibleBonusAttribute.HasValue)
            {
                _logger.LogWarning(
                    "Lineage validation failed: Clan-Born requires flexible bonus attribute. " +
                    "CharacterId={CharacterId}",
                    character.Id);
                return LineageValidationResult.Fail(
                    $"{definition.DisplayName} lineage requires a flexible bonus attribute selection");
            }

            _logger.LogDebug(
                "Flexible bonus attribute validated for {Lineage}. " +
                "SelectedAttribute={SelectedAttribute}, " +
                "BonusAmount={BonusAmount}",
                lineage,
                flexibleBonusAttribute.Value,
                definition.AttributeModifiers.FlexibleBonusAmount);
        }
        else
        {
            // Non-Clan-Born lineages must not have a flexible bonus
            if (flexibleBonusAttribute.HasValue)
            {
                _logger.LogWarning(
                    "Lineage validation failed: lineage does not support flexible bonus. " +
                    "Lineage={Lineage}, ProvidedAttribute={ProvidedAttribute}, " +
                    "CharacterId={CharacterId}",
                    lineage,
                    flexibleBonusAttribute.Value,
                    character.Id);
                return LineageValidationResult.Fail(
                    $"{definition.DisplayName} lineage does not support flexible bonus attribute selection");
            }
        }

        _logger.LogDebug(
            "Lineage validation passed. Lineage={Lineage}, CharacterId={CharacterId}",
            lineage,
            character.Id);

        return LineageValidationResult.Valid();
    }

    /// <inheritdoc />
    public IReadOnlyList<Lineage> GetApplicableLineages(Player character)
    {
        _logger.LogDebug(
            "Getting applicable lineages for character. " +
            "CharacterId={CharacterId}, HasLineage={HasLineage}",
            character?.Id.ToString() ?? "null",
            character?.HasLineage.ToString() ?? "null");

        if (character == null || character.HasLineage)
        {
            _logger.LogDebug(
                "Returning empty lineage list. Character is null or already has lineage");
            return Array.Empty<Lineage>();
        }

        var allLineages = _lineageProvider.GetAllLineages()
            .Select(d => d.LineageId)
            .ToList()
            .AsReadOnly();

        _logger.LogDebug(
            "Returning {LineageCount} applicable lineages for character {CharacterId}",
            allLineages.Count,
            character.Id);

        return allLineages;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // PRIVATE METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Applies attribute modifiers from the lineage definition to the character.
    /// </summary>
    /// <param name="character">The character to modify.</param>
    /// <param name="mods">The attribute modifiers from the lineage definition.</param>
    /// <param name="flexibleBonusAttribute">
    /// The attribute to apply the flexible bonus to (Clan-Born only).
    /// </param>
    /// <param name="changes">
    /// Dictionary to record all attribute changes made, for inclusion in the result.
    /// </param>
    private void ApplyAttributeModifiers(
        Player character,
        LineageAttributeModifiers mods,
        CoreAttribute? flexibleBonusAttribute,
        Dictionary<CoreAttribute, int> changes)
    {
        _logger.LogDebug(
            "Applying attribute modifiers. " +
            "Might={MightMod}, Finesse={FinesseMod}, Wits={WitsMod}, " +
            "Will={WillMod}, Sturdiness={SturdinessMod}, " +
            "HasFlexibleBonus={HasFlexibleBonus}, FlexibleAmount={FlexibleAmount}",
            mods.MightModifier,
            mods.FinesseModifier,
            mods.WitsModifier,
            mods.WillModifier,
            mods.SturdinessModifier,
            mods.HasFlexibleBonus,
            mods.FlexibleBonusAmount);

        // Apply fixed attribute modifiers
        ApplyFixedModifier(character, CoreAttribute.Might, mods.MightModifier, changes);
        ApplyFixedModifier(character, CoreAttribute.Finesse, mods.FinesseModifier, changes);
        ApplyFixedModifier(character, CoreAttribute.Wits, mods.WitsModifier, changes);
        ApplyFixedModifier(character, CoreAttribute.Will, mods.WillModifier, changes);
        ApplyFixedModifier(character, CoreAttribute.Sturdiness, mods.SturdinessModifier, changes);

        // Apply flexible bonus (Clan-Born: +1 to chosen attribute)
        if (mods.HasFlexibleBonus && flexibleBonusAttribute.HasValue)
        {
            var attr = flexibleBonusAttribute.Value;
            var amount = mods.FlexibleBonusAmount;

            character.ModifyAttribute(attr, amount);

            // Accumulate into changes dictionary
            if (changes.ContainsKey(attr))
                changes[attr] += amount;
            else
                changes[attr] = amount;

            _logger.LogDebug(
                "Applied flexible bonus. Attribute={Attribute}, Amount={Amount:+#;-#;0}",
                attr,
                amount);
        }

        _logger.LogDebug(
            "Finished applying attribute modifiers. TotalChanges={TotalChanges}: {ChangeDetails}",
            changes.Count,
            string.Join(", ", changes.Select(c => $"{c.Key}:{c.Value:+#;-#;0}")));
    }

    /// <summary>
    /// Applies a single fixed attribute modifier if non-zero.
    /// </summary>
    /// <param name="character">The character to modify.</param>
    /// <param name="attribute">The attribute to modify.</param>
    /// <param name="modifier">The modifier amount (0 means no change).</param>
    /// <param name="changes">Dictionary to track applied changes.</param>
    private void ApplyFixedModifier(
        Player character,
        CoreAttribute attribute,
        int modifier,
        Dictionary<CoreAttribute, int> changes)
    {
        if (modifier == 0) return;

        character.ModifyAttribute(attribute, modifier);
        changes[attribute] = modifier;

        _logger.LogDebug(
            "Applied fixed attribute modifier. Attribute={Attribute}, Modifier={Modifier:+#;-#;0}",
            attribute,
            modifier);
    }

    /// <summary>
    /// Applies passive bonuses from the lineage definition to the character.
    /// </summary>
    /// <param name="character">The character to modify.</param>
    /// <param name="bonuses">The passive bonuses from the lineage definition.</param>
    private void ApplyPassiveBonuses(Player character, LineagePassiveBonuses bonuses)
    {
        _logger.LogDebug(
            "Applying passive bonuses. " +
            "MaxHP={MaxHpBonus}, MaxAP={MaxApBonus}, Soak={SoakBonus}, " +
            "Movement={MovementBonus}, SkillBonusCount={SkillBonusCount}",
            bonuses.MaxHpBonus,
            bonuses.MaxApBonus,
            bonuses.SoakBonus,
            bonuses.MovementBonus,
            bonuses.SkillBonuses.Count);

        if (bonuses.MaxHpBonus != 0)
        {
            character.ModifyMaxHp(bonuses.MaxHpBonus);
            _logger.LogDebug("Applied Max HP bonus: {MaxHpBonus:+#;-#;0}", bonuses.MaxHpBonus);
        }

        if (bonuses.MaxApBonus != 0)
        {
            character.ModifyMaxAp(bonuses.MaxApBonus);
            _logger.LogDebug("Applied Max AP bonus: {MaxApBonus:+#;-#;0}", bonuses.MaxApBonus);
        }

        if (bonuses.SoakBonus != 0)
        {
            character.ModifySoak(bonuses.SoakBonus);
            _logger.LogDebug("Applied Soak bonus: {SoakBonus:+#;-#;0}", bonuses.SoakBonus);
        }

        if (bonuses.MovementBonus != 0)
        {
            character.ModifyMovement(bonuses.MovementBonus);
            _logger.LogDebug("Applied Movement bonus: {MovementBonus:+#;-#;0}", bonuses.MovementBonus);
        }

        foreach (var skillBonus in bonuses.SkillBonuses)
        {
            character.ModifySkill(skillBonus.SkillId, skillBonus.BonusAmount);
            _logger.LogDebug(
                "Applied skill bonus. SkillId={SkillId}, Amount={Amount:+#;-#;0}",
                skillBonus.SkillId,
                skillBonus.BonusAmount);
        }

        _logger.LogDebug("Finished applying passive bonuses");
    }

    /// <summary>
    /// Registers the unique lineage trait on the character.
    /// </summary>
    /// <param name="character">The character to register the trait on.</param>
    /// <param name="trait">The lineage trait to register.</param>
    private void RegisterTrait(Player character, LineageTrait trait)
    {
        character.RegisterLineageTrait(trait);

        _logger.LogDebug(
            "Registered unique lineage trait. TraitId={TraitId}, TraitName={TraitName}, " +
            "EffectType={EffectType}, BonusDice={BonusDice}, PercentModifier={PercentModifier}",
            trait.TraitId,
            trait.TraitName,
            trait.EffectType,
            trait.BonusDice?.ToString() ?? "N/A",
            trait.PercentModifier?.ToString("F2") ?? "N/A");
    }

    /// <summary>
    /// Sets the Trauma Economy baseline values on the character from the lineage definition.
    /// </summary>
    /// <param name="character">The character to configure.</param>
    /// <param name="baseline">The trauma baseline from the lineage definition.</param>
    private void SetTraumaBaseline(Player character, LineageTraumaBaseline baseline)
    {
        character.SetTraumaBaseline(
            baseline.StartingCorruption,
            baseline.StartingStress,
            baseline.CorruptionResistanceModifier,
            baseline.StressResistanceModifier);

        _logger.LogDebug(
            "Set trauma baseline. StartingCorruption={StartingCorruption}, " +
            "StartingStress={StartingStress}, " +
            "CorruptionResistMod={CorruptionResistMod}, " +
            "StressResistMod={StressResistMod}, " +
            "HasPermanentCorruption={HasPermanentCorruption}",
            baseline.StartingCorruption,
            baseline.StartingStress,
            baseline.CorruptionResistanceModifier,
            baseline.StressResistanceModifier,
            baseline.HasPermanentCorruption);
    }
}
