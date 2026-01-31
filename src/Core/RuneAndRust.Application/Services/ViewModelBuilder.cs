// ═══════════════════════════════════════════════════════════════════════════════
// ViewModelBuilder.cs
// Builds CharacterCreationViewModel from CharacterCreationState by coordinating
// with lineage, background, archetype, specialization, and derived stat providers.
// Assembles step display info, navigation state, selection display names, preview
// data (derived stats, abilities, equipment), and validation status into an
// immutable ViewModel for the character creation TUI.
// Version: 0.17.5b
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Application.Services;

using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Extensions;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Builds <see cref="CharacterCreationViewModel"/> from <see cref="CharacterCreationState"/>.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="ViewModelBuilder"/> is the application service that transforms raw creation
/// state into display-ready presentation data. It coordinates with five providers to
/// assemble complete ViewModel instances:
/// </para>
/// <list type="bullet">
///   <item><description><see cref="ILineageProvider"/> — lineage display names, HP/AP bonuses</description></item>
///   <item><description><see cref="IBackgroundProvider"/> — background display names, equipment grants</description></item>
///   <item><description><see cref="IArchetypeProvider"/> — archetype display names, resource bonuses, starting abilities</description></item>
///   <item><description><see cref="ISpecializationProvider"/> — specialization display names, Tier 1 abilities</description></item>
///   <item><description><see cref="IDerivedStatCalculator"/> — derived stat calculation from attribute allocation</description></item>
/// </list>
/// <para>
/// <strong>Rebuild Strategy:</strong> A fresh ViewModel is constructed on every call to
/// <see cref="Build"/>. There is no caching — this ensures the ViewModel always reflects
/// the current state. For real-time stat preview during attribute allocation,
/// <see cref="BuildDerivedStatsPreview"/> provides a lightweight alternative that
/// calculates only the resource pool preview.
/// </para>
/// <para>
/// <strong>Error Handling:</strong> Missing provider data (e.g., lineage not found) is
/// handled gracefully by returning null display names and empty previews. Warnings are
/// logged for missing data. The builder never throws for missing definitions — it
/// degrades gracefully so the TUI can still render partial information.
/// </para>
/// </remarks>
/// <seealso cref="IViewModelBuilder"/>
/// <seealso cref="CharacterCreationViewModel"/>
/// <seealso cref="CharacterCreationState"/>
public class ViewModelBuilder : IViewModelBuilder
{
    // ═══════════════════════════════════════════════════════════════════════════
    // PRIVATE FIELDS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Provider for lineage definitions, display names, and bonus methods.
    /// </summary>
    private readonly ILineageProvider _lineageProvider;

    /// <summary>
    /// Provider for background definitions, display names, and equipment grants.
    /// </summary>
    private readonly IBackgroundProvider _backgroundProvider;

    /// <summary>
    /// Provider for archetype definitions, resource bonuses, and starting abilities.
    /// </summary>
    private readonly IArchetypeProvider _archetypeProvider;

    /// <summary>
    /// Provider for specialization definitions and Tier 1 abilities.
    /// </summary>
    private readonly ISpecializationProvider _specializationProvider;

    /// <summary>
    /// Calculator for derived statistics from attribute allocation state.
    /// </summary>
    private readonly IDerivedStatCalculator _derivedStatCalculator;

    /// <summary>
    /// Logger for diagnostic output during ViewModel construction.
    /// </summary>
    private readonly ILogger<ViewModelBuilder> _logger;

    /// <summary>
    /// Warning text displayed on the Archetype step (Step 4) to indicate a permanent choice.
    /// </summary>
    private const string PermanentWarningText =
        "This choice is PERMANENT and cannot be changed after character creation.";

    /// <summary>
    /// Compiled regex for splitting PascalCase enum names into kebab-case segments.
    /// Used to convert enum values like <c>ClanBorn</c> to string IDs like <c>"clan-born"</c>
    /// for the <see cref="IDerivedStatCalculator"/>.
    /// </summary>
    private static readonly Regex PascalCaseSplitter = new(
        @"(?<!^)(?=[A-Z])",
        RegexOptions.Compiled);

    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTRUCTOR
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Initializes a new <see cref="ViewModelBuilder"/> with all required providers.
    /// </summary>
    /// <param name="lineageProvider">Provider for lineage definitions and bonuses.</param>
    /// <param name="backgroundProvider">Provider for background definitions and equipment grants.</param>
    /// <param name="archetypeProvider">Provider for archetype definitions, resource bonuses, and abilities.</param>
    /// <param name="specializationProvider">Provider for specialization definitions and abilities.</param>
    /// <param name="derivedStatCalculator">Calculator for derived statistics from attributes.</param>
    /// <param name="logger">Optional logger for diagnostic output. Falls back to <see cref="NullLogger{T}"/>.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when any required provider is null.
    /// </exception>
    public ViewModelBuilder(
        ILineageProvider lineageProvider,
        IBackgroundProvider backgroundProvider,
        IArchetypeProvider archetypeProvider,
        ISpecializationProvider specializationProvider,
        IDerivedStatCalculator derivedStatCalculator,
        ILogger<ViewModelBuilder>? logger = null)
    {
        ArgumentNullException.ThrowIfNull(lineageProvider);
        ArgumentNullException.ThrowIfNull(backgroundProvider);
        ArgumentNullException.ThrowIfNull(archetypeProvider);
        ArgumentNullException.ThrowIfNull(specializationProvider);
        ArgumentNullException.ThrowIfNull(derivedStatCalculator);

        _lineageProvider = lineageProvider;
        _backgroundProvider = backgroundProvider;
        _archetypeProvider = archetypeProvider;
        _specializationProvider = specializationProvider;
        _derivedStatCalculator = derivedStatCalculator;
        _logger = logger ?? NullLogger<ViewModelBuilder>.Instance;

        _logger.LogDebug(
            "ViewModelBuilder initialized with all 5 providers. " +
            "LineageProvider={LineageProvider}, BackgroundProvider={BackgroundProvider}, " +
            "ArchetypeProvider={ArchetypeProvider}, SpecializationProvider={SpecializationProvider}, " +
            "DerivedStatCalculator={DerivedStatCalculator}",
            lineageProvider.GetType().Name,
            backgroundProvider.GetType().Name,
            archetypeProvider.GetType().Name,
            specializationProvider.GetType().Name,
            derivedStatCalculator.GetType().Name);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // PUBLIC METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <inheritdoc />
    public CharacterCreationViewModel Build(CharacterCreationState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        var step = state.CurrentStep;

        _logger.LogDebug(
            "Building ViewModel for step {Step} (Step {StepNumber} of {TotalSteps}). " +
            "SessionId: {SessionId}",
            step, step.GetStepNumber(), CharacterCreationStepExtensions.GetTotalSteps(),
            state.SessionId);

        // Build selection display names from providers
        var selectedLineageName = GetLineageDisplayName(state.SelectedLineage);
        var selectedBackgroundName = GetBackgroundDisplayName(state.SelectedBackground);
        var selectedArchetypeName = GetArchetypeDisplayName(state.SelectedArchetype);
        var selectedSpecializationName = GetSpecializationDisplayName(state.SelectedSpecialization);

        _logger.LogDebug(
            "Selection display names resolved. Lineage={Lineage}, Background={Background}, " +
            "Archetype={Archetype}, Specialization={Specialization}, Name={CharacterName}. " +
            "SessionId: {SessionId}",
            selectedLineageName ?? "(none)", selectedBackgroundName ?? "(none)",
            selectedArchetypeName ?? "(none)", selectedSpecializationName ?? "(none)",
            state.CharacterName ?? "(none)", state.SessionId);

        // Build preview data from providers
        var derivedStatsPreview = BuildDerivedStatsPreviewInternal(state);
        var abilitiesPreview = BuildAbilitiesPreview(state);
        var equipmentPreview = BuildEquipmentPreview(state);

        _logger.LogDebug(
            "Preview data built. HasDerivedStats={HasDerivedStats}, HasAbilities={HasAbilities}, " +
            "HasEquipment={HasEquipment}. SessionId: {SessionId}",
            derivedStatsPreview?.HasValues ?? false,
            abilitiesPreview?.HasAbilities ?? false,
            equipmentPreview?.HasItems ?? false,
            state.SessionId);

        // Determine navigation and validation state
        var isStepComplete = state.IsStepComplete(step);
        var hasValidationErrors = state.ValidationErrors.Count > 0;
        var isCurrentStepValid = isStepComplete && !hasValidationErrors;

        _logger.LogDebug(
            "Navigation state calculated. CanGoBack={CanGoBack}, CanGoForward={CanGoForward}, " +
            "IsPermanent={IsPermanent}, IsStepComplete={IsStepComplete}, " +
            "ValidationErrors={ValidationErrorCount}, IsCurrentStepValid={IsValid}. " +
            "SessionId: {SessionId}",
            step.CanGoBack(), isStepComplete, step.IsPermanentChoice(),
            isStepComplete, state.ValidationErrors.Count, isCurrentStepValid,
            state.SessionId);

        var viewModel = new CharacterCreationViewModel
        {
            // Step Information
            CurrentStep = step,
            StepTitle = step.GetDisplayName(),
            StepDescription = step.GetDescription(),
            StepNumber = step.GetStepNumber(),
            TotalSteps = CharacterCreationStepExtensions.GetTotalSteps(),

            // Navigation
            CanGoBack = step.CanGoBack(),
            CanGoForward = isStepComplete,
            IsPermanentChoice = step.IsPermanentChoice(),
            PermanentWarning = step.IsPermanentChoice() ? PermanentWarningText : null,

            // Current Selections
            SelectedLineageName = selectedLineageName,
            SelectedBackgroundName = selectedBackgroundName,
            SelectedArchetypeName = selectedArchetypeName,
            SelectedSpecializationName = selectedSpecializationName,
            CharacterName = state.CharacterName,

            // Preview Data
            DerivedStatsPreview = derivedStatsPreview,
            AbilitiesPreview = abilitiesPreview,
            EquipmentPreview = equipmentPreview,

            // Validation
            ValidationErrors = state.ValidationErrors.AsReadOnly(),
            IsCurrentStepValid = isCurrentStepValid
        };

        _logger.LogDebug(
            "ViewModel build complete for step {Step}. {ViewModel}. SessionId: {SessionId}",
            step, viewModel, state.SessionId);

        return viewModel;
    }

    /// <inheritdoc />
    public DerivedStatsPreview BuildDerivedStatsPreview(CharacterCreationState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        _logger.LogDebug(
            "Building derived stats preview (public). HasAttributes={HasAttributes}, " +
            "HasLineage={HasLineage}, HasArchetype={HasArchetype}. SessionId: {SessionId}",
            state.Attributes.HasValue, state.SelectedLineage.HasValue,
            state.SelectedArchetype.HasValue, state.SessionId);

        return BuildDerivedStatsPreviewInternal(state) ?? DerivedStatsPreview.Empty;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // PRIVATE METHODS — PREVIEW BUILDERS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Builds the derived stats preview from the current creation state.
    /// Returns null if no attributes have been allocated.
    /// </summary>
    /// <param name="state">The current character creation state.</param>
    /// <returns>
    /// A <see cref="DerivedStatsPreview"/> with calculated stats and source breakdown,
    /// or <c>null</c> if attributes have not been allocated yet.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Uses <see cref="IDerivedStatCalculator.GetPreview"/> with string-based archetype
    /// and lineage IDs (kebab-case) to calculate the full derived stats. The individual
    /// bonus values (HpFromLineage, HpFromArchetype, etc.) are retrieved separately
    /// from the lineage definition and archetype resource bonuses.
    /// </para>
    /// </remarks>
    private DerivedStatsPreview? BuildDerivedStatsPreviewInternal(CharacterCreationState state)
    {
        // No attributes allocated yet — cannot calculate derived stats
        if (!state.Attributes.HasValue)
        {
            _logger.LogDebug(
                "Derived stats preview skipped — no attributes allocated. SessionId: {SessionId}",
                state.SessionId);
            return null;
        }

        // Get lineage bonuses via LineageDefinition helper methods
        var lineageDef = state.SelectedLineage.HasValue
            ? _lineageProvider.GetLineage(state.SelectedLineage.Value)
            : null;

        if (state.SelectedLineage.HasValue && lineageDef == null)
        {
            _logger.LogWarning(
                "Lineage definition not found for {Lineage}. Using zero bonuses for preview. " +
                "SessionId: {SessionId}",
                state.SelectedLineage.Value, state.SessionId);
        }

        var hpFromLineage = lineageDef?.GetHpBonus() ?? 0;
        var apFromLineage = lineageDef?.GetApBonus() ?? 0;

        // Get archetype bonuses via IArchetypeProvider.GetResourceBonuses()
        var archetypeBonuses = state.SelectedArchetype.HasValue
            ? _archetypeProvider.GetResourceBonuses(state.SelectedArchetype.Value)
            : ArchetypeResourceBonuses.None;

        var hpFromArchetype = archetypeBonuses.MaxHpBonus;
        var staminaFromArchetype = archetypeBonuses.MaxStaminaBonus;
        var apFromArchetype = archetypeBonuses.MaxAetherPoolBonus;

        _logger.LogDebug(
            "Bonus sources resolved. HpFromLineage={HpFromLineage}, ApFromLineage={ApFromLineage}, " +
            "HpFromArchetype={HpFromArchetype}, StaminaFromArchetype={StaminaFromArchetype}, " +
            "ApFromArchetype={ApFromArchetype}. SessionId: {SessionId}",
            hpFromLineage, apFromLineage, hpFromArchetype, staminaFromArchetype,
            apFromArchetype, state.SessionId);

        // Convert enum values to kebab-case string IDs for IDerivedStatCalculator
        var archetypeId = state.SelectedArchetype.HasValue
            ? ConvertToKebabCase(state.SelectedArchetype.Value.ToString())
            : null;
        var lineageId = state.SelectedLineage.HasValue
            ? ConvertToKebabCase(state.SelectedLineage.Value.ToString())
            : null;

        // Calculate derived stats via IDerivedStatCalculator.GetPreview()
        var derivedStats = _derivedStatCalculator.GetPreview(
            state.Attributes.Value, archetypeId, lineageId);

        _logger.LogDebug(
            "Derived stats preview calculated. MaxHp={MaxHp}, MaxStamina={MaxStamina}, " +
            "MaxAetherPool={MaxAetherPool}. ArchetypeId='{ArchetypeId}', LineageId='{LineageId}'. " +
            "SessionId: {SessionId}",
            derivedStats.MaxHp, derivedStats.MaxStamina, derivedStats.MaxAetherPool,
            archetypeId ?? "(none)", lineageId ?? "(none)", state.SessionId);

        return new DerivedStatsPreview
        {
            MaxHp = derivedStats.MaxHp,
            MaxStamina = derivedStats.MaxStamina,
            MaxAp = derivedStats.MaxAetherPool, // Domain uses MaxAetherPool; preview uses MaxAp
            HpFromLineage = hpFromLineage,
            HpFromArchetype = hpFromArchetype,
            ApFromLineage = apFromLineage,
            ApFromArchetype = apFromArchetype,
            StaminaFromArchetype = staminaFromArchetype
        };
    }

    /// <summary>
    /// Builds the abilities preview from archetype and specialization selections.
    /// Returns null if no archetype has been selected.
    /// </summary>
    /// <param name="state">The current character creation state.</param>
    /// <returns>
    /// An <see cref="AbilitiesPreview"/> with archetype and specialization ability names,
    /// or <c>null</c> if no archetype has been selected yet.
    /// </returns>
    private AbilitiesPreview? BuildAbilitiesPreview(CharacterCreationState state)
    {
        var archetypeAbilities = new List<string>();
        var specAbilities = new List<string>();

        // Get archetype starting abilities (3 abilities per archetype)
        if (state.SelectedArchetype.HasValue)
        {
            var abilities = _archetypeProvider.GetStartingAbilities(state.SelectedArchetype.Value);

            _logger.LogDebug(
                "Archetype abilities retrieved for {Archetype}. Count={Count}. SessionId: {SessionId}",
                state.SelectedArchetype.Value, abilities.Count, state.SessionId);

            archetypeAbilities.AddRange(abilities.Select(a => a.AbilityName));
        }

        // Get specialization Tier 1 abilities (typically 2-4 abilities)
        if (state.SelectedSpecialization.HasValue)
        {
            var specDef = _specializationProvider.GetBySpecializationId(state.SelectedSpecialization.Value);
            if (specDef != null)
            {
                // Get Tier 1 from the specialization's ability tiers
                var tier1 = specDef.GetTier(1);
                if (tier1.HasValue && tier1.Value.Abilities != null)
                {
                    var tier1Names = tier1.Value.Abilities.Select(a => a.DisplayName).ToList();

                    _logger.LogDebug(
                        "Specialization Tier 1 abilities retrieved for {Specialization}. Count={Count}. " +
                        "SessionId: {SessionId}",
                        state.SelectedSpecialization.Value, tier1Names.Count, state.SessionId);

                    specAbilities.AddRange(tier1Names);
                }
                else
                {
                    _logger.LogWarning(
                        "Specialization {Specialization} has no Tier 1 abilities. SessionId: {SessionId}",
                        state.SelectedSpecialization.Value, state.SessionId);
                }
            }
            else
            {
                _logger.LogWarning(
                    "Specialization definition not found for {SpecializationId}. " +
                    "Using empty abilities for preview. SessionId: {SessionId}",
                    state.SelectedSpecialization.Value, state.SessionId);
            }
        }

        // Return null if no abilities from either source
        if (archetypeAbilities.Count == 0 && specAbilities.Count == 0)
        {
            _logger.LogDebug(
                "Abilities preview skipped — no archetype or specialization selected. " +
                "SessionId: {SessionId}",
                state.SessionId);
            return null;
        }

        return new AbilitiesPreview
        {
            ArchetypeAbilities = archetypeAbilities.AsReadOnly(),
            SpecializationAbilities = specAbilities.AsReadOnly()
        };
    }

    /// <summary>
    /// Builds the equipment preview from the selected background.
    /// Returns null if no background has been selected.
    /// </summary>
    /// <param name="state">The current character creation state.</param>
    /// <returns>
    /// An <see cref="EquipmentPreview"/> with starting equipment items,
    /// or <c>null</c> if no background has been selected yet.
    /// </returns>
    private EquipmentPreview? BuildEquipmentPreview(CharacterCreationState state)
    {
        if (!state.SelectedBackground.HasValue)
        {
            _logger.LogDebug(
                "Equipment preview skipped — no background selected. SessionId: {SessionId}",
                state.SessionId);
            return null;
        }

        var backgroundDef = _backgroundProvider.GetBackground(state.SelectedBackground.Value);
        if (backgroundDef == null)
        {
            _logger.LogWarning(
                "Background definition not found for {Background}. " +
                "Using empty equipment preview. SessionId: {SessionId}",
                state.SelectedBackground.Value, state.SessionId);
            return null;
        }

        // Transform BackgroundEquipmentGrant items into display-ready EquipmentPreviewItems
        var items = backgroundDef.EquipmentGrants
            .Select(grant => new EquipmentPreviewItem
            {
                Name = FormatItemIdAsDisplayName(grant.ItemId),
                Quantity = grant.Quantity,
                ItemType = DeriveItemType(grant)
            })
            .ToList();

        _logger.LogDebug(
            "Equipment preview built for background {Background}. ItemCount={ItemCount}. " +
            "Items: [{Items}]. SessionId: {SessionId}",
            backgroundDef.DisplayName, items.Count,
            string.Join(", ", items.Select(i => i.ToString())),
            state.SessionId);

        return new EquipmentPreview
        {
            Items = items.AsReadOnly(),
            FromBackground = backgroundDef.DisplayName
        };
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // PRIVATE METHODS — DISPLAY NAME HELPERS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the display name for a lineage from the provider.
    /// </summary>
    /// <param name="lineage">The lineage enum value, or null if not selected.</param>
    /// <returns>
    /// The lineage display name (e.g., "Clan-Born"), or <c>null</c> if the lineage
    /// is not selected or not found in the provider.
    /// </returns>
    private string? GetLineageDisplayName(Lineage? lineage)
    {
        if (!lineage.HasValue) return null;

        var definition = _lineageProvider.GetLineage(lineage.Value);
        if (definition == null)
        {
            _logger.LogWarning(
                "Lineage definition not found for {Lineage}. Returning null display name.",
                lineage.Value);
        }

        return definition?.DisplayName;
    }

    /// <summary>
    /// Gets the display name for a background from the provider.
    /// </summary>
    /// <param name="background">The background enum value, or null if not selected.</param>
    /// <returns>
    /// The background display name (e.g., "Village Smith"), or <c>null</c> if the
    /// background is not selected or not found in the provider.
    /// </returns>
    private string? GetBackgroundDisplayName(Background? background)
    {
        if (!background.HasValue) return null;

        var definition = _backgroundProvider.GetBackground(background.Value);
        if (definition == null)
        {
            _logger.LogWarning(
                "Background definition not found for {Background}. Returning null display name.",
                background.Value);
        }

        return definition?.DisplayName;
    }

    /// <summary>
    /// Gets the display name for an archetype from the provider.
    /// </summary>
    /// <param name="archetype">The archetype enum value, or null if not selected.</param>
    /// <returns>
    /// The archetype display name (e.g., "Warrior"), or <c>null</c> if the archetype
    /// is not selected or not found in the provider.
    /// </returns>
    private string? GetArchetypeDisplayName(Archetype? archetype)
    {
        if (!archetype.HasValue) return null;

        var definition = _archetypeProvider.GetArchetype(archetype.Value);
        if (definition == null)
        {
            _logger.LogWarning(
                "Archetype definition not found for {Archetype}. Returning null display name.",
                archetype.Value);
        }

        return definition?.DisplayName;
    }

    /// <summary>
    /// Gets the display name for a specialization from the provider.
    /// </summary>
    /// <param name="specId">The specialization ID, or null if not selected.</param>
    /// <returns>
    /// The specialization display name (e.g., "Berserkr"), or <c>null</c> if the
    /// specialization is not selected or not found in the provider.
    /// </returns>
    private string? GetSpecializationDisplayName(SpecializationId? specId)
    {
        if (!specId.HasValue) return null;

        var definition = _specializationProvider.GetBySpecializationId(specId.Value);
        if (definition == null)
        {
            _logger.LogWarning(
                "Specialization definition not found for {SpecializationId}. Returning null display name.",
                specId.Value);
        }

        return definition?.DisplayName;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // PRIVATE METHODS — DATA TRANSFORMATION HELPERS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Converts a kebab-case item ID to a display-friendly name.
    /// </summary>
    /// <param name="itemId">
    /// The kebab-case item identifier from <see cref="BackgroundEquipmentGrant.ItemId"/>
    /// (e.g., "smiths-hammer", "healers-kit", "leather-apron").
    /// </param>
    /// <returns>
    /// A title-cased display name with hyphens replaced by spaces
    /// (e.g., "Smiths Hammer", "Healers Kit", "Leather Apron").
    /// </returns>
    /// <remarks>
    /// <para>
    /// This is a simple transformation that capitalizes the first letter of each
    /// hyphen-separated word. It does not handle apostrophes (e.g., "smiths-hammer"
    /// becomes "Smiths Hammer", not "Smith's Hammer") because the underlying item
    /// IDs do not encode apostrophe information.
    /// </para>
    /// </remarks>
    private static string FormatItemIdAsDisplayName(string itemId)
    {
        if (string.IsNullOrWhiteSpace(itemId))
            return string.Empty;

        return string.Join(' ', itemId.Split('-')
            .Where(word => word.Length > 0)
            .Select(word => char.ToUpperInvariant(word[0]) + word[1..]));
    }

    /// <summary>
    /// Derives an item type string from equipment grant properties.
    /// </summary>
    /// <param name="grant">The background equipment grant to derive the type from.</param>
    /// <returns>
    /// A string describing the item type:
    /// <list type="bullet">
    ///   <item><description>Equipped items with a slot → the slot name (e.g., "MainHand", "Body")</description></item>
    ///   <item><description>Non-equipped items with Quantity &gt; 1 → "Consumable"</description></item>
    ///   <item><description>All other items → "Item"</description></item>
    /// </list>
    /// </returns>
    /// <remarks>
    /// <para>
    /// <see cref="BackgroundEquipmentGrant"/> does not have a direct <c>ItemType</c>
    /// property. This method derives the type from the grant's <c>IsEquipped</c>,
    /// <c>Slot</c>, and <c>Quantity</c> properties for presentation purposes.
    /// </para>
    /// </remarks>
    private static string DeriveItemType(BackgroundEquipmentGrant grant)
    {
        // Equipped items use their equipment slot as the type
        if (grant.IsEquipped && grant.Slot.HasValue)
            return grant.Slot.Value.ToString();

        // Multi-quantity non-equipped items are consumables
        if (grant.Quantity > 1)
            return "Consumable";

        // Default for single non-equipped items
        return "Item";
    }

    /// <summary>
    /// Converts a PascalCase enum name to a kebab-case string identifier.
    /// </summary>
    /// <param name="pascalCaseName">
    /// The PascalCase string (e.g., "ClanBorn", "IronBlooded", "Warrior").
    /// </param>
    /// <returns>
    /// A lowercase kebab-case string (e.g., "clan-born", "iron-blooded", "warrior").
    /// </returns>
    /// <remarks>
    /// <para>
    /// Used to convert <see cref="Lineage"/> and <see cref="Archetype"/> enum values
    /// to the kebab-case string IDs expected by <see cref="IDerivedStatCalculator.GetPreview"/>.
    /// For example, <c>Lineage.ClanBorn.ToString()</c> returns "ClanBorn", which is
    /// converted to "clan-born".
    /// </para>
    /// <para>
    /// The conversion uses a compiled regex to split on PascalCase word boundaries
    /// and joins with hyphens in lowercase.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// ConvertToKebabCase("ClanBorn");    // "clan-born"
    /// ConvertToKebabCase("IronBlooded"); // "iron-blooded"
    /// ConvertToKebabCase("Warrior");     // "warrior"
    /// ConvertToKebabCase("VargrKin");    // "vargr-kin"
    /// </code>
    /// </example>
    private static string ConvertToKebabCase(string pascalCaseName)
    {
        return string.Join("-", PascalCaseSplitter.Split(pascalCaseName))
            .ToLowerInvariant();
    }
}
