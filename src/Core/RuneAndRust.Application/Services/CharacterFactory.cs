// ═══════════════════════════════════════════════════════════════════════════════
// CharacterFactory.cs
// Factory service that transforms a completed CharacterCreationState into a fully
// initialized Player entity. Executes the 13-step initialization sequence: state
// validation, Player construction, lineage attribute modifiers, lineage passive
// bonuses, lineage trait registration, trauma baseline, background skills, derived
// stat calculation, resource pool initialization, archetype abilities,
// specialization application, and saga progression initialization.
// Version: 0.17.5e
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Application.Services;

using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Factory service that creates fully initialized <see cref="Player"/> entities
/// from completed <see cref="CharacterCreationState"/>.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="CharacterFactory"/> implements the <see cref="ICharacterFactory"/>
/// contract by coordinating with five providers to assemble a complete Player
/// entity from the character creation workflow selections.
/// </para>
/// <para>
/// <strong>Dependencies:</strong>
/// </para>
/// <list type="bullet">
///   <item><description><see cref="ILineageProvider"/> — Attribute modifiers, passive bonuses, traits, trauma baselines</description></item>
///   <item><description><see cref="IBackgroundProvider"/> — Skill grants</description></item>
///   <item><description><see cref="IArchetypeProvider"/> — Starting abilities, resource bonuses</description></item>
///   <item><description><see cref="ISpecializationProvider"/> — Tier 1 abilities, special resources</description></item>
///   <item><description><see cref="IDerivedStatCalculator"/> — HP, Stamina, Aether Pool calculations</description></item>
/// </list>
/// <para>
/// <strong>13-Step Initialization Sequence:</strong>
/// </para>
/// <list type="number">
///   <item><description>Validate state completeness and consistency</description></item>
///   <item><description>Construct Player entity with base attributes</description></item>
///   <item><description>Set lineage on Player (once-only)</description></item>
///   <item><description>Set background on Player (once-only)</description></item>
///   <item><description>Apply lineage attribute modifiers (including Clan-Born flexible +1)</description></item>
///   <item><description>Apply lineage passive bonuses (HP, AP, Soak, Movement, Skills)</description></item>
///   <item><description>Register lineage trait</description></item>
///   <item><description>Set trauma baseline (Corruption, Stress, resistance modifiers)</description></item>
///   <item><description>Grant background skill bonuses</description></item>
///   <item><description>Set archetype and calculate/apply derived stats</description></item>
///   <item><description>Initialize resource pools (Stamina, Aether Pool) at maximum</description></item>
///   <item><description>Grant archetype starting abilities (3)</description></item>
///   <item><description>Apply specialization (Tier 1 abilities, special resource, progression)</description></item>
/// </list>
/// </remarks>
/// <seealso cref="ICharacterFactory"/>
/// <seealso cref="CharacterCreationState"/>
/// <seealso cref="Player"/>
public class CharacterFactory : ICharacterFactory
{
    // ═══════════════════════════════════════════════════════════════════════════
    // PRIVATE FIELDS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>Provides lineage definitions, attribute modifiers, passive bonuses, traits, and trauma baselines.</summary>
    private readonly ILineageProvider _lineageProvider;

    /// <summary>Provides background definitions, skill grants, and equipment grants.</summary>
    private readonly IBackgroundProvider _backgroundProvider;

    /// <summary>Provides archetype definitions, starting abilities, and resource bonuses.</summary>
    private readonly IArchetypeProvider _archetypeProvider;

    /// <summary>Provides specialization definitions, Tier 1 abilities, and special resources.</summary>
    private readonly ISpecializationProvider _specializationProvider;

    /// <summary>Calculates derived stats (HP, Stamina, Aether Pool) from attributes, archetype, and lineage.</summary>
    private readonly IDerivedStatCalculator _derivedStatCalculator;

    /// <summary>Logger for structured diagnostic output.</summary>
    private readonly ILogger<CharacterFactory> _logger;

    // ═══════════════════════════════════════════════════════════════════════════
    // VALIDATION ERROR MESSAGES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>Error when lineage is not selected.</summary>
    private const string ErrorLineageRequired = "Lineage is required.";

    /// <summary>Error when background is not selected.</summary>
    private const string ErrorBackgroundRequired = "Background is required.";

    /// <summary>Error when attributes are not allocated or incomplete.</summary>
    private const string ErrorAttributesRequired = "Complete attribute allocation is required.";

    /// <summary>Error when archetype is not selected.</summary>
    private const string ErrorArchetypeRequired = "Archetype is required.";

    /// <summary>Error when specialization is not selected.</summary>
    private const string ErrorSpecializationRequired = "Specialization is required.";

    /// <summary>Error when character name is missing.</summary>
    private const string ErrorNameRequired = "Character name is required.";

    /// <summary>Error when Clan-Born lineage lacks flexible attribute bonus.</summary>
    private const string ErrorClanBornFlexibleBonus = "Clan-Born lineage requires flexible attribute selection.";

    /// <summary>Error when specialization does not belong to the selected archetype.</summary>
    private const string ErrorSpecializationArchetypeMismatch =
        "Selected specialization does not belong to the selected archetype.";

    // ═══════════════════════════════════════════════════════════════════════════
    // KEBAB-CASE CONVERSION
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Compiled regex for splitting PascalCase enum names into kebab-case segments.
    /// Used to convert enum values like <c>ClanBorn</c> to string IDs like <c>"clan-born"</c>
    /// for the <see cref="IDerivedStatCalculator"/> and <see cref="Player"/> constructor.
    /// </summary>
    private static readonly Regex PascalCaseSplitter = new(
        @"(?<!^)(?=[A-Z])",
        RegexOptions.Compiled);

    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTRUCTOR
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Initializes a new <see cref="CharacterFactory"/> with all required providers.
    /// </summary>
    /// <param name="lineageProvider">Provider for lineage definitions and modifiers.</param>
    /// <param name="backgroundProvider">Provider for background definitions and skill grants.</param>
    /// <param name="archetypeProvider">Provider for archetype definitions and starting abilities.</param>
    /// <param name="specializationProvider">Provider for specialization definitions and Tier 1 abilities.</param>
    /// <param name="derivedStatCalculator">Calculator for derived stats (HP, Stamina, Aether Pool).</param>
    /// <param name="logger">Logger for structured diagnostic output.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when any required parameter is <c>null</c>.
    /// </exception>
    public CharacterFactory(
        ILineageProvider lineageProvider,
        IBackgroundProvider backgroundProvider,
        IArchetypeProvider archetypeProvider,
        ISpecializationProvider specializationProvider,
        IDerivedStatCalculator derivedStatCalculator,
        ILogger<CharacterFactory> logger)
    {
        ArgumentNullException.ThrowIfNull(lineageProvider);
        ArgumentNullException.ThrowIfNull(backgroundProvider);
        ArgumentNullException.ThrowIfNull(archetypeProvider);
        ArgumentNullException.ThrowIfNull(specializationProvider);
        ArgumentNullException.ThrowIfNull(derivedStatCalculator);
        ArgumentNullException.ThrowIfNull(logger);

        _lineageProvider = lineageProvider;
        _backgroundProvider = backgroundProvider;
        _archetypeProvider = archetypeProvider;
        _specializationProvider = specializationProvider;
        _derivedStatCalculator = derivedStatCalculator;
        _logger = logger;

        _logger.LogDebug(
            "CharacterFactory initialized with all providers");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // ICharacterFactory IMPLEMENTATION
    // ═══════════════════════════════════════════════════════════════════════════

    /// <inheritdoc />
    public FactoryValidationResult ValidateState(CharacterCreationState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        _logger.LogDebug(
            "Validating creation state. SessionId: {SessionId}",
            state.SessionId);

        var errors = new List<string>();

        // Check all required selections are present
        if (state.SelectedLineage == null)
            errors.Add(ErrorLineageRequired);

        if (state.SelectedBackground == null)
            errors.Add(ErrorBackgroundRequired);

        if (!state.Attributes.HasValue || !state.Attributes.Value.IsComplete)
            errors.Add(ErrorAttributesRequired);

        if (state.SelectedArchetype == null)
            errors.Add(ErrorArchetypeRequired);

        if (state.SelectedSpecialization == null)
            errors.Add(ErrorSpecializationRequired);

        if (string.IsNullOrWhiteSpace(state.CharacterName))
            errors.Add(ErrorNameRequired);

        // Clan-Born specific validation
        if (state.SelectedLineage == Lineage.ClanBorn &&
            state.FlexibleAttributeBonus == null)
        {
            errors.Add(ErrorClanBornFlexibleBonus);
        }

        // Specialization-archetype compatibility check
        if (state.SelectedArchetype != null && state.SelectedSpecialization != null)
        {
            var specDef = _specializationProvider.GetBySpecializationId(
                state.SelectedSpecialization.Value);

            if (specDef != null && specDef.ParentArchetype != state.SelectedArchetype.Value)
            {
                errors.Add(ErrorSpecializationArchetypeMismatch);
            }
        }

        if (errors.Count > 0)
        {
            _logger.LogDebug(
                "State validation failed with {ErrorCount} error(s): {Errors}. SessionId: {SessionId}",
                errors.Count, string.Join("; ", errors), state.SessionId);
            return FactoryValidationResult.Invalid(errors);
        }

        _logger.LogDebug(
            "State validation passed. SessionId: {SessionId}",
            state.SessionId);

        return FactoryValidationResult.Valid();
    }

    /// <inheritdoc />
    public Task<Player> CreateCharacterAsync(
        CharacterCreationState state,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(state);

        // ─── Step 1: Validate state ────────────────────────────────────────────
        var validation = ValidateState(state);
        if (!validation.IsValid)
        {
            _logger.LogError(
                "Cannot create character: state validation failed. Errors: {Errors}. SessionId: {SessionId}",
                string.Join(", ", validation.Errors), state.SessionId);
            throw new InvalidOperationException(
                $"State validation failed: {string.Join(", ", validation.Errors)}");
        }

        // Extract selections from state into local variables for clarity
        var lineage = state.SelectedLineage!.Value;
        var background = state.SelectedBackground!.Value;
        var archetype = state.SelectedArchetype!.Value;
        var specialization = state.SelectedSpecialization!.Value;
        var attributes = state.Attributes!.Value;
        var name = state.CharacterName!.Trim();
        var flexibleBonus = state.FlexibleAttributeBonus;

        _logger.LogInformation(
            "Creating character: {Name} ({Lineage} {Archetype} {Specialization}). SessionId: {SessionId}",
            name, lineage, archetype, specialization, state.SessionId);

        // Convert enum values to kebab-case strings for providers and Player constructor
        var lineageKebab = ConvertToKebabCase(lineage.ToString());
        var backgroundKebab = ConvertToKebabCase(background.ToString());
        var archetypeKebab = ConvertToKebabCase(archetype.ToString());

        _logger.LogDebug(
            "Converted IDs — Lineage: '{LineageId}', Background: '{BackgroundId}', Archetype: '{ArchetypeId}'. " +
            "SessionId: {SessionId}",
            lineageKebab, backgroundKebab, archetypeKebab, state.SessionId);

        // ─── Step 2: Construct Player entity with base attributes ──────────────
        // Map AttributeAllocationState to PlayerAttributes
        // Note: CoreAttribute.Sturdiness maps to PlayerAttributes.Fortitude
        var playerAttributes = new PlayerAttributes(
            might: attributes.CurrentMight,
            fortitude: attributes.CurrentSturdiness,
            will: attributes.CurrentWill,
            wits: attributes.CurrentWits,
            finesse: attributes.CurrentFinesse);

        var player = new Player(
            name: name,
            raceId: lineageKebab,
            backgroundId: backgroundKebab,
            attributes: playerAttributes);

        _logger.LogDebug(
            "Player entity created. Id: {PlayerId}, Name: {Name}, " +
            "BaseAttributes: M:{Might} F:{Finesse} W:{Wits} Wi:{Will} S:{Sturdiness}. " +
            "SessionId: {SessionId}",
            player.Id, player.Name,
            attributes.CurrentMight, attributes.CurrentFinesse, attributes.CurrentWits,
            attributes.CurrentWill, attributes.CurrentSturdiness, state.SessionId);

        // ─── Step 3: Set lineage on Player (once-only) ────────────────────────
        player.SetLineage(lineage);

        _logger.LogDebug(
            "Lineage set: {Lineage}. SessionId: {SessionId}",
            lineage, state.SessionId);

        // ─── Step 4: Set background on Player (once-only) ─────────────────────
        player.SetBackground(background);

        _logger.LogDebug(
            "Background set: {Background}. SessionId: {SessionId}",
            background, state.SessionId);

        // ─── Step 5: Apply lineage attribute modifiers ─────────────────────────
        ApplyLineageAttributeModifiers(player, lineage, flexibleBonus, state.SessionId);

        // ─── Step 6: Apply lineage passive bonuses ─────────────────────────────
        ApplyLineagePassiveBonuses(player, lineage, state.SessionId);

        // ─── Step 7: Register lineage trait ────────────────────────────────────
        RegisterLineageTrait(player, lineage, state.SessionId);

        // ─── Step 8: Set trauma baseline ───────────────────────────────────────
        SetTraumaBaseline(player, lineage, state.SessionId);

        // ─── Step 9: Grant background skill bonuses ────────────────────────────
        GrantBackgroundSkills(player, background, state.SessionId);

        // ─── Step 10: Set archetype and calculate derived stats ────────────────
        SetArchetypeAndDerivedStats(player, archetype, lineageKebab, archetypeKebab, state.SessionId);

        // ─── Step 11: Initialize resource pools at maximum ─────────────────────
        InitializeResourcePools(player, archetypeKebab, lineageKebab, state.SessionId);

        // ─── Step 12: Grant archetype starting abilities ───────────────────────
        GrantArchetypeAbilities(player, archetype, state.SessionId);

        // ─── Step 13: Apply specialization and initialize progression ──────────
        ApplySpecialization(player, specialization, state.SessionId);
        InitializeSagaProgression(player, state.SessionId);

        _logger.LogInformation(
            "Character created successfully: {Name} (HP: {HP}/{MaxHP}, " +
            "Lineage: {Lineage}, Archetype: {Archetype}, Specialization: {Specialization}, " +
            "Abilities: {AbilityCount}). SessionId: {SessionId}",
            player.Name, player.Health, player.Stats.MaxHealth,
            lineage, archetype, specialization,
            player.Abilities.Count, state.SessionId);

        return Task.FromResult(player);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // PRIVATE INITIALIZATION STEPS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Step 5: Applies lineage attribute modifiers to the player, including the
    /// Clan-Born flexible +1 bonus if applicable.
    /// </summary>
    /// <param name="player">The player entity to modify.</param>
    /// <param name="lineage">The selected lineage.</param>
    /// <param name="flexibleBonus">The Clan-Born flexible attribute target, or null.</param>
    /// <param name="sessionId">Session ID for log correlation.</param>
    /// <remarks>
    /// Uses <see cref="LineageAttributeModifiers.GetEffectiveModifier"/> which automatically
    /// includes the flexible bonus for Clan-Born when the target attribute matches. For
    /// non-Clan-Born lineages, the flexible bonus parameter is null and ignored.
    /// </remarks>
    private void ApplyLineageAttributeModifiers(
        Player player,
        Lineage lineage,
        CoreAttribute? flexibleBonus,
        string sessionId)
    {
        var lineageAttrMods = _lineageProvider.GetAttributeModifiers(lineage);

        _logger.LogDebug(
            "Applying lineage attribute modifiers for {Lineage}. FlexibleBonus: {FlexibleBonus}. " +
            "SessionId: {SessionId}",
            lineage, flexibleBonus?.ToString() ?? "(none)", sessionId);

        foreach (var attribute in Enum.GetValues<CoreAttribute>())
        {
            var modifier = lineageAttrMods.GetEffectiveModifier(attribute, flexibleBonus);
            if (modifier != 0)
            {
                player.ModifyAttribute(attribute, modifier);
                _logger.LogDebug(
                    "Applied lineage modifier: {Attribute} {Modifier:+#;-#;0}. SessionId: {SessionId}",
                    attribute, modifier, sessionId);
            }
        }
    }

    /// <summary>
    /// Step 6: Applies lineage passive bonuses including Max HP, Max AP, Soak,
    /// Movement, and skill bonuses.
    /// </summary>
    /// <param name="player">The player entity to modify.</param>
    /// <param name="lineage">The selected lineage.</param>
    /// <param name="sessionId">Session ID for log correlation.</param>
    private void ApplyLineagePassiveBonuses(Player player, Lineage lineage, string sessionId)
    {
        var passives = _lineageProvider.GetPassiveBonuses(lineage);

        _logger.LogDebug(
            "Applying lineage passive bonuses for {Lineage}. HasHp: {HasHp}, HasAp: {HasAp}, " +
            "HasSoak: {HasSoak}, HasMovement: {HasMovement}, SkillCount: {SkillCount}. " +
            "SessionId: {SessionId}",
            lineage, passives.HasHpBonus, passives.HasApBonus,
            passives.HasSoakBonus, passives.HasMovementBonus,
            passives.SkillBonuses.Count, sessionId);

        if (passives.MaxHpBonus != 0)
        {
            player.ModifyMaxHp(passives.MaxHpBonus);
            _logger.LogDebug(
                "Applied Max HP bonus: {Bonus:+#;-#;0}. SessionId: {SessionId}",
                passives.MaxHpBonus, sessionId);
        }

        if (passives.MaxApBonus != 0)
        {
            player.ModifyMaxAp(passives.MaxApBonus);
            _logger.LogDebug(
                "Applied Max AP bonus: {Bonus:+#;-#;0}. SessionId: {SessionId}",
                passives.MaxApBonus, sessionId);
        }

        if (passives.SoakBonus != 0)
        {
            player.ModifySoak(passives.SoakBonus);
            _logger.LogDebug(
                "Applied Soak bonus: {Bonus:+#;-#;0}. SessionId: {SessionId}",
                passives.SoakBonus, sessionId);
        }

        if (passives.MovementBonus != 0)
        {
            player.ModifyMovement(passives.MovementBonus);
            _logger.LogDebug(
                "Applied Movement bonus: {Bonus:+#;-#;0}. SessionId: {SessionId}",
                passives.MovementBonus, sessionId);
        }

        foreach (var skillBonus in passives.SkillBonuses)
        {
            player.ModifySkill(skillBonus.SkillId, skillBonus.BonusAmount);
            _logger.LogDebug(
                "Applied lineage skill bonus: {SkillId} {Bonus:+#;-#;0}. SessionId: {SessionId}",
                skillBonus.SkillId, skillBonus.BonusAmount, sessionId);
        }
    }

    /// <summary>
    /// Step 7: Registers the lineage's unique trait on the player.
    /// </summary>
    /// <param name="player">The player entity to modify.</param>
    /// <param name="lineage">The selected lineage.</param>
    /// <param name="sessionId">Session ID for log correlation.</param>
    private void RegisterLineageTrait(Player player, Lineage lineage, string sessionId)
    {
        var trait = _lineageProvider.GetUniqueTrait(lineage);

        player.RegisterLineageTrait(trait);

        _logger.LogDebug(
            "Registered lineage trait: {TraitName}. SessionId: {SessionId}",
            trait.TraitName, sessionId);
    }

    /// <summary>
    /// Step 8: Sets the trauma baseline (Corruption, Stress, and resistance modifiers)
    /// from the lineage's trauma configuration.
    /// </summary>
    /// <param name="player">The player entity to modify.</param>
    /// <param name="lineage">The selected lineage.</param>
    /// <param name="sessionId">Session ID for log correlation.</param>
    /// <remarks>
    /// Rune-Marked starts with 5 permanent Corruption and -1 Corruption resistance.
    /// Iron-Blooded has -1 Stress resistance. All other lineages start at (0, 0, 0, 0).
    /// </remarks>
    private void SetTraumaBaseline(Player player, Lineage lineage, string sessionId)
    {
        var trauma = _lineageProvider.GetTraumaBaseline(lineage);

        player.SetTraumaBaseline(
            trauma.StartingCorruption,
            trauma.StartingStress,
            trauma.CorruptionResistanceModifier,
            trauma.StressResistanceModifier);

        _logger.LogDebug(
            "Trauma baseline set: Corruption={Corruption}, Stress={Stress}, " +
            "CorruptionResist={CorruptionResist}, StressResist={StressResist}. " +
            "SessionId: {SessionId}",
            trauma.StartingCorruption, trauma.StartingStress,
            trauma.CorruptionResistanceModifier, trauma.StressResistanceModifier,
            sessionId);
    }

    /// <summary>
    /// Step 9: Grants skill bonuses from the selected background.
    /// </summary>
    /// <param name="player">The player entity to modify.</param>
    /// <param name="background">The selected background.</param>
    /// <param name="sessionId">Session ID for log correlation.</param>
    private void GrantBackgroundSkills(Player player, Background background, string sessionId)
    {
        var skillGrants = _backgroundProvider.GetSkillGrants(background);

        _logger.LogDebug(
            "Granting background skills for {Background}. GrantCount: {Count}. SessionId: {SessionId}",
            background, skillGrants.Count, sessionId);

        foreach (var grant in skillGrants)
        {
            player.ModifySkill(grant.SkillId, grant.BonusAmount);
            _logger.LogDebug(
                "Granted background skill: {SkillId} {Bonus:+#;-#;0}. SessionId: {SessionId}",
                grant.SkillId, grant.BonusAmount, sessionId);
        }
    }

    /// <summary>
    /// Step 10: Sets the archetype on the player and calculates derived stats
    /// (HP, Stamina, Aether Pool) using the <see cref="IDerivedStatCalculator"/>.
    /// </summary>
    /// <param name="player">The player entity to modify.</param>
    /// <param name="archetype">The selected archetype enum value.</param>
    /// <param name="lineageKebab">The lineage as a kebab-case string for the calculator.</param>
    /// <param name="archetypeKebab">The archetype as a kebab-case string for the calculator.</param>
    /// <param name="sessionId">Session ID for log correlation.</param>
    /// <remarks>
    /// <para>
    /// Sets the archetype via <see cref="Player.SetClass"/> using the kebab-case
    /// archetype ID for both archetypeId and classId parameters.
    /// </para>
    /// <para>
    /// Builds an attribute dictionary from the player's current (post-lineage-modifier)
    /// attributes for the <see cref="IDerivedStatCalculator.CalculateDerivedStats"/> call.
    /// The calculator returns <see cref="DerivedStats"/> which are applied via
    /// <see cref="Player.SetStats"/>. Since the player was at full health after
    /// construction, <see cref="Player.SetStats"/> automatically sets
    /// <see cref="Player.Health"/> to the new <see cref="Stats.MaxHealth"/>.
    /// </para>
    /// </remarks>
    private void SetArchetypeAndDerivedStats(
        Player player,
        Archetype archetype,
        string lineageKebab,
        string archetypeKebab,
        string sessionId)
    {
        // Set archetype/class on the player
        player.SetClass(archetypeId: archetypeKebab, classId: archetypeKebab);

        _logger.LogDebug(
            "Archetype set: {Archetype} (archetypeId: '{ArchetypeId}'). SessionId: {SessionId}",
            archetype, archetypeKebab, sessionId);

        // Build attribute dictionary from current (post-modifier) player attributes
        var attrDict = new Dictionary<CoreAttribute, int>
        {
            { CoreAttribute.Might, player.Attributes.Might },
            { CoreAttribute.Finesse, player.Attributes.Finesse },
            { CoreAttribute.Wits, player.Attributes.Wits },
            { CoreAttribute.Will, player.Attributes.Will },
            { CoreAttribute.Sturdiness, player.Attributes.Fortitude }
        };

        // Calculate derived stats via IDerivedStatCalculator
        var derivedStats = _derivedStatCalculator.CalculateDerivedStats(
            attrDict, archetypeKebab, lineageKebab);

        _logger.LogDebug(
            "Derived stats calculated: MaxHp={MaxHp}, MaxStamina={MaxStamina}, " +
            "MaxAetherPool={MaxAetherPool}, Initiative={Initiative}, Soak={Soak}. " +
            "SessionId: {SessionId}",
            derivedStats.MaxHp, derivedStats.MaxStamina, derivedStats.MaxAetherPool,
            derivedStats.Initiative, derivedStats.Soak, sessionId);

        // Apply derived stats to player via SetStats
        // Stats constructor: (maxHealth, attack, defense, wits)
        // - MaxHealth from derived stats
        // - Attack mapped from player's Might attribute
        // - Defense mapped from derived Soak value
        // - Wits mapped from player's Wits attribute
        var stats = new Stats(
            maxHealth: derivedStats.MaxHp,
            attack: player.Attributes.Might,
            defense: derivedStats.Soak,
            wits: Math.Clamp(player.Attributes.Wits, 1, 20));

        player.SetStats(stats);

        _logger.LogDebug(
            "Stats applied: MaxHealth={MaxHealth}, Attack={Attack}, Defense={Defense}. " +
            "Health set to: {Health}. SessionId: {SessionId}",
            stats.MaxHealth, stats.Attack, stats.Defense, player.Health, sessionId);
    }

    /// <summary>
    /// Step 11: Initializes resource pools (Stamina and Aether Pool) at maximum
    /// values based on derived stat calculations.
    /// </summary>
    /// <param name="player">The player entity to modify.</param>
    /// <param name="archetypeKebab">The archetype as a kebab-case string for the calculator.</param>
    /// <param name="lineageKebab">The lineage as a kebab-case string for the calculator.</param>
    /// <param name="sessionId">Session ID for log correlation.</param>
    /// <remarks>
    /// <para>
    /// Since there is no <c>SetResourcesToMaximum()</c> method on <see cref="Player"/>,
    /// resource pools are initialized individually:
    /// </para>
    /// <list type="bullet">
    ///   <item><description><strong>HP:</strong> Automatically at maximum — <see cref="Player.SetStats"/>
    ///   sets <see cref="Player.Health"/> to <see cref="Stats.MaxHealth"/> when the player
    ///   was at full health (which they are after construction).</description></item>
    ///   <item><description><strong>Stamina:</strong> Initialized via <see cref="Player.InitializeResource"/>
    ///   with <c>startAtZero: false</c> (starts at maximum).</description></item>
    ///   <item><description><strong>Aether Pool:</strong> Initialized via <see cref="Player.InitializeResource"/>
    ///   with <c>startAtZero: false</c> (starts at maximum).</description></item>
    /// </list>
    /// </remarks>
    private void InitializeResourcePools(
        Player player,
        string archetypeKebab,
        string lineageKebab,
        string sessionId)
    {
        // Re-calculate derived stats to get Stamina and Aether Pool values
        // (We need these values again; they were used for Stats in Step 10)
        var attrDict = new Dictionary<CoreAttribute, int>
        {
            { CoreAttribute.Might, player.Attributes.Might },
            { CoreAttribute.Finesse, player.Attributes.Finesse },
            { CoreAttribute.Wits, player.Attributes.Wits },
            { CoreAttribute.Will, player.Attributes.Will },
            { CoreAttribute.Sturdiness, player.Attributes.Fortitude }
        };

        var derivedStats = _derivedStatCalculator.CalculateDerivedStats(
            attrDict, archetypeKebab, lineageKebab);

        // Initialize Stamina pool at maximum
        if (derivedStats.MaxStamina > 0)
        {
            player.InitializeResource("stamina", derivedStats.MaxStamina, startAtZero: false);
            _logger.LogDebug(
                "Stamina pool initialized: {MaxStamina} (at maximum). SessionId: {SessionId}",
                derivedStats.MaxStamina, sessionId);
        }

        // Initialize Aether Pool at maximum
        if (derivedStats.MaxAetherPool > 0)
        {
            player.InitializeResource("aether-pool", derivedStats.MaxAetherPool, startAtZero: false);
            _logger.LogDebug(
                "Aether Pool initialized: {MaxAetherPool} (at maximum). SessionId: {SessionId}",
                derivedStats.MaxAetherPool, sessionId);
        }
    }

    /// <summary>
    /// Step 12: Grants the 3 starting abilities from the selected archetype.
    /// </summary>
    /// <param name="player">The player entity to modify.</param>
    /// <param name="archetype">The selected archetype.</param>
    /// <param name="sessionId">Session ID for log correlation.</param>
    private void GrantArchetypeAbilities(Player player, Archetype archetype, string sessionId)
    {
        var startingAbilities = _archetypeProvider.GetStartingAbilities(archetype);

        _logger.LogDebug(
            "Granting archetype abilities for {Archetype}. AbilityCount: {Count}. SessionId: {SessionId}",
            archetype, startingAbilities.Count, sessionId);

        foreach (var abilityGrant in startingAbilities)
        {
            player.GrantAbility(abilityGrant.AbilityId);
            _logger.LogDebug(
                "Granted archetype ability: {AbilityId} ({AbilityName}). SessionId: {SessionId}",
                abilityGrant.AbilityId, abilityGrant.AbilityName, sessionId);
        }
    }

    /// <summary>
    /// Step 13a: Applies the selected specialization, grants Tier 1 abilities,
    /// and initializes the special resource if applicable.
    /// </summary>
    /// <param name="player">The player entity to modify.</param>
    /// <param name="specialization">The selected specialization ID.</param>
    /// <param name="sessionId">Session ID for log correlation.</param>
    /// <remarks>
    /// <para>
    /// <see cref="Player.AddSpecialization"/> automatically unlocks Tier 1.
    /// The Tier 1 abilities are then individually granted via <see cref="Player.GrantAbility"/>.
    /// If the specialization defines a special resource (e.g., Rage for Berserkr),
    /// it is initialized via <see cref="Player.InitializeSpecialResource"/>.
    /// </para>
    /// </remarks>
    private void ApplySpecialization(Player player, SpecializationId specialization, string sessionId)
    {
        var specDef = _specializationProvider.GetBySpecializationId(specialization);

        if (specDef == null)
        {
            _logger.LogWarning(
                "Specialization definition not found for {Specialization}. " +
                "Skipping specialization application. SessionId: {SessionId}",
                specialization, sessionId);
            return;
        }

        // Add specialization to player (auto-unlocks Tier 1)
        player.AddSpecialization(specialization);

        _logger.LogDebug(
            "Specialization added: {Specialization} ({DisplayName}, {PathType}). " +
            "SessionId: {SessionId}",
            specialization, specDef.DisplayName, specDef.PathType, sessionId);

        // Initialize special resource if present
        if (specDef.HasSpecialResource)
        {
            player.InitializeSpecialResource(specDef.SpecialResource);
            _logger.LogDebug(
                "Special resource initialized: {ResourceId} ({DisplayName}, Max: {MaxValue}). " +
                "SessionId: {SessionId}",
                specDef.SpecialResource.ResourceId, specDef.SpecialResource.DisplayName,
                specDef.SpecialResource.MaxValue, sessionId);
        }

        // Grant Tier 1 abilities
        if (specDef.HasAbilityTiers)
        {
            var tier1 = specDef.GetTier(1);
            if (tier1 != null)
            {
                var tier1Value = tier1.Value;

                _logger.LogDebug(
                    "Granting Tier 1 abilities for {Specialization}. AbilityCount: {Count}. " +
                    "SessionId: {SessionId}",
                    specialization, tier1Value.Abilities.Count, sessionId);

                foreach (var ability in tier1Value.Abilities)
                {
                    player.GrantAbility(ability.AbilityId);
                    _logger.LogDebug(
                        "Granted specialization Tier 1 ability: {AbilityId} ({DisplayName}). " +
                        "SessionId: {SessionId}",
                        ability.AbilityId, ability.DisplayName, sessionId);
                }
            }
        }
    }

    /// <summary>
    /// Step 13b: Initializes saga progression to starting values.
    /// </summary>
    /// <param name="player">The player entity to modify.</param>
    /// <param name="sessionId">Session ID for log correlation.</param>
    /// <remarks>
    /// Sets Progression Points to 0 and Progression Rank to 1, establishing
    /// the starting point for the Saga progression system.
    /// </remarks>
    private void InitializeSagaProgression(Player player, string sessionId)
    {
        player.SetProgressionPoints(0);
        player.SetProgressionRank(1);

        _logger.LogDebug(
            "Saga progression initialized: PP={PP}, Rank={Rank}. SessionId: {SessionId}",
            0, 1, sessionId);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // PRIVATE HELPER METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Converts a PascalCase enum name to a kebab-case string.
    /// </summary>
    /// <param name="pascalCaseName">
    /// The PascalCase name to convert (e.g., "ClanBorn", "IronBlooded", "Warrior").
    /// </param>
    /// <returns>
    /// The kebab-case equivalent (e.g., "clan-born", "iron-blooded", "warrior").
    /// </returns>
    /// <remarks>
    /// Uses a compiled regex to split on PascalCase word boundaries and joins
    /// with hyphens in lowercase. This is the same pattern used by
    /// <see cref="ViewModelBuilder"/> for <see cref="IDerivedStatCalculator"/> calls.
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
