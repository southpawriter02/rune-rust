// ═══════════════════════════════════════════════════════════════════════════════
// ArchetypeApplicationService.cs
// Service that orchestrates applying archetype bonuses, starting abilities,
// and specialization mappings to characters during creation Step 4.
// Version: 0.17.3f
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Application.Services;

using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Application.ValueObjects;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Applies archetypes to characters during creation, granting resource bonuses,
/// starting abilities, and setting available specializations.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="ArchetypeApplicationService"/> is the primary orchestrator for applying
/// all archetype components to a <see cref="Player"/> entity during character creation
/// Step 4 (Archetype Selection). It retrieves archetype data from
/// <see cref="IArchetypeProvider"/> and applies them in the following order:
/// </para>
/// <list type="number">
///   <item><description>Validate preconditions (no existing archetype, valid enum, definition exists)</description></item>
///   <item><description>Retrieve archetype definition, resource bonuses, abilities, and specializations from provider</description></item>
///   <item><description>Apply resource bonuses (HP, Aether Pool, Movement) to the character's derived stats</description></item>
///   <item><description>Grant 3 starting abilities to the character</description></item>
///   <item><description>Set the archetype identifier on the character entity</description></item>
/// </list>
/// <para>
/// <strong>Resource Bonus Application:</strong> Resource bonuses are applied via existing
/// <see cref="Player"/> modifier methods (<see cref="Player.ModifyMaxHp"/>,
/// <see cref="Player.ModifyMaxAp"/>, <see cref="Player.ModifyMovement"/>). Stamina bonuses
/// are tracked in the result for future application when the Player entity gains a
/// dedicated stamina modifier method.
/// </para>
/// <para>
/// <strong>Ability Granting:</strong> Starting abilities are created as <see cref="PlayerAbility"/>
/// instances via <see cref="PlayerAbility.Create(string, bool)"/> and added to the character's
/// ability collection via <see cref="Player.AddAbility(PlayerAbility)"/>. All starting abilities
/// are granted as unlocked.
/// </para>
/// <para>
/// <strong>Permanent Choice:</strong> Archetype selection is permanent — once applied, it cannot
/// be changed. The service validates this precondition and rejects attempts to apply a second
/// archetype to a character that already has one.
/// </para>
/// <para>
/// <strong>Idempotency:</strong> The service checks for existing archetype assignment before
/// applying. Attempting to apply an archetype to a character that already has one returns a
/// failure result without modifying the character.
/// </para>
/// </remarks>
/// <seealso cref="IArchetypeApplicationService"/>
/// <seealso cref="IArchetypeProvider"/>
/// <seealso cref="ArchetypeApplicationResult"/>
public class ArchetypeApplicationService : IArchetypeApplicationService
{
    // ═══════════════════════════════════════════════════════════════════════════
    // PRIVATE FIELDS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Provider for accessing archetype definitions, bonuses, abilities, and
    /// specialization mappings from configuration.
    /// </summary>
    private readonly IArchetypeProvider _archetypeProvider;

    /// <summary>
    /// Logger for structured diagnostic output.
    /// </summary>
    private readonly ILogger<ArchetypeApplicationService> _logger;

    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTRUCTOR
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Initializes a new instance of the <see cref="ArchetypeApplicationService"/> class.
    /// </summary>
    /// <param name="archetypeProvider">
    /// The archetype provider for accessing definitions, resource bonuses, starting
    /// abilities, and specialization mappings. Must not be null.
    /// </param>
    /// <param name="logger">
    /// The logger for diagnostic output. Must not be null.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="archetypeProvider"/> or <paramref name="logger"/> is null.
    /// </exception>
    public ArchetypeApplicationService(
        IArchetypeProvider archetypeProvider,
        ILogger<ArchetypeApplicationService> logger)
    {
        ArgumentNullException.ThrowIfNull(archetypeProvider, nameof(archetypeProvider));
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));

        _archetypeProvider = archetypeProvider;
        _logger = logger;

        _logger.LogDebug(
            "ArchetypeApplicationService initialized with IArchetypeProvider");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // PUBLIC METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <inheritdoc />
    public ArchetypeApplicationResult ApplyArchetype(
        Player character,
        Archetype archetype)
    {
        _logger.LogInformation(
            "Beginning archetype application. Archetype={Archetype}, " +
            "CharacterId={CharacterId}, CharacterName={CharacterName}",
            archetype,
            character?.Id.ToString() ?? "null",
            character?.Name ?? "null");

        // Step 1: Validate preconditions
        var validation = CanApplyArchetype(character, archetype);
        if (!validation.IsValid)
        {
            var reason = string.Join("; ", validation.Issues);
            _logger.LogWarning(
                "Archetype application failed: validation failed. " +
                "Archetype={Archetype}, Reason={Reason}",
                archetype,
                reason);
            return ArchetypeApplicationResult.Failed(reason);
        }

        // Step 2: Retrieve archetype data from provider
        var definition = _archetypeProvider.GetArchetype(archetype);
        var bonuses = _archetypeProvider.GetResourceBonuses(archetype);
        var abilities = _archetypeProvider.GetStartingAbilities(archetype);
        var specializations = _archetypeProvider.GetSpecializationMapping(archetype);

        if (definition == null)
        {
            _logger.LogError(
                "Archetype application failed: definition not found in provider. " +
                "Archetype={Archetype}, CharacterId={CharacterId}",
                archetype,
                character!.Id);
            return ArchetypeApplicationResult.Failed(
                $"Archetype '{archetype}' not found in provider.");
        }

        _logger.LogDebug(
            "Retrieved archetype data. Archetype={Archetype}, " +
            "DisplayName={DisplayName}, CombatRole={CombatRole}, " +
            "PrimaryResource={PrimaryResource}, " +
            "AbilityCount={AbilityCount}, SpecializationCount={SpecializationCount}",
            archetype,
            definition.DisplayName,
            definition.CombatRole,
            definition.PrimaryResource,
            abilities.Count,
            specializations.Count);

        // Step 3: Apply resource bonuses to character's derived stats
        ApplyResourceBonuses(character!, bonuses);

        // Step 4: Grant starting abilities to the character
        GrantAbilities(character!, abilities);

        // Step 5: Set the archetype identifier on the character entity
        // Uses the existing SetClass method which accepts archetype and class IDs
        var archetypeId = archetype.ToString().ToLowerInvariant();
        character!.SetClass(archetypeId, archetypeId);

        _logger.LogDebug(
            "Set archetype on character. CharacterId={CharacterId}, " +
            "ArchetypeId={ArchetypeId}, HasClass={HasClass}",
            character.Id,
            archetypeId,
            character.HasClass);

        _logger.LogInformation(
            "Successfully applied archetype {Archetype} to character {CharacterName}. " +
            "ResourceBonuses=HP+{HpBonus}/Stam+{StaminaBonus}/AP+{ApBonus}/Mov+{MovBonus}, " +
            "AbilitiesGranted={AbilityCount}, " +
            "SpecializationsAvailable={SpecCount} (recommended: {RecommendedSpec}), " +
            "CharacterId={CharacterId}",
            archetype,
            character.Name,
            bonuses.MaxHpBonus,
            bonuses.MaxStaminaBonus,
            bonuses.MaxAetherPoolBonus,
            bonuses.MovementBonus,
            abilities.Count,
            specializations.Count,
            specializations.RecommendedFirst,
            character.Id);

        return ArchetypeApplicationResult.Succeeded(
            archetype, bonuses, abilities, specializations);
    }

    /// <inheritdoc />
    public ArchetypeValidationResult CanApplyArchetype(
        Player? character,
        Archetype archetype)
    {
        _logger.LogDebug(
            "Validating archetype application. Archetype={Archetype}, " +
            "CharacterIsNull={CharacterIsNull}",
            archetype,
            character == null);

        var issues = new List<string>();

        // Check 1: Character must not be null
        if (character == null)
        {
            issues.Add("A character is required.");

            _logger.LogDebug(
                "Validation failed: character is null. Archetype={Archetype}",
                archetype);

            return ArchetypeValidationResult.Invalid(issues.ToArray());
        }

        // Check 2: Character must not already have an archetype (permanent choice)
        if (character.HasClass)
        {
            issues.Add(
                $"Character already has archetype: {character.ArchetypeId}. " +
                "Archetype is a permanent choice.");

            _logger.LogDebug(
                "Validation failed: character already has archetype. " +
                "ExistingArchetypeId={ExistingArchetypeId}, " +
                "RequestedArchetype={RequestedArchetype}, " +
                "CharacterId={CharacterId}",
                character.ArchetypeId,
                archetype,
                character.Id);
        }

        // Check 3: Archetype must be a defined enum value
        if (!Enum.IsDefined(archetype))
        {
            issues.Add($"Invalid archetype value: {archetype}.");

            _logger.LogDebug(
                "Validation failed: invalid archetype enum value. " +
                "Archetype={Archetype}, IntValue={IntValue}",
                archetype,
                (int)archetype);
        }

        // Check 4: Archetype definition must exist in the provider
        if (Enum.IsDefined(archetype) && _archetypeProvider.GetArchetype(archetype) == null)
        {
            issues.Add($"Archetype '{archetype}' is not configured.");

            _logger.LogDebug(
                "Validation failed: archetype not configured in provider. " +
                "Archetype={Archetype}",
                archetype);
        }

        if (issues.Count > 0)
        {
            _logger.LogDebug(
                "Archetype validation completed with {IssueCount} issues. " +
                "Archetype={Archetype}, Issues=[{Issues}]",
                issues.Count,
                archetype,
                string.Join(", ", issues));

            return ArchetypeValidationResult.Invalid(issues.ToArray());
        }

        _logger.LogDebug(
            "Archetype validation passed. Archetype={Archetype}, " +
            "CharacterId={CharacterId}, CharacterName={CharacterName}",
            archetype,
            character.Id,
            character.Name);

        return ArchetypeValidationResult.Valid;
    }

    /// <inheritdoc />
    public ArchetypeApplicationPreview GetApplicationPreview(Archetype archetype)
    {
        _logger.LogDebug(
            "Generating archetype application preview. Archetype={Archetype}",
            archetype);

        var definition = _archetypeProvider.GetArchetype(archetype);
        var bonuses = _archetypeProvider.GetResourceBonuses(archetype);
        var abilities = _archetypeProvider.GetStartingAbilities(archetype);
        var specializations = _archetypeProvider.GetSpecializationMapping(archetype);

        var displayName = definition?.DisplayName ?? archetype.ToString();

        _logger.LogDebug(
            "Generated archetype application preview. Archetype={Archetype}, " +
            "DisplayName={DisplayName}, " +
            "ResourceBonusSummary={ResourceBonusSummary}, " +
            "AbilityCount={AbilityCount}, SpecializationCount={SpecializationCount}",
            archetype,
            displayName,
            bonuses.GetDisplaySummary(),
            abilities.Count,
            specializations.Count);

        return new ArchetypeApplicationPreview(
            archetype,
            displayName,
            bonuses,
            abilities,
            specializations);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // PRIVATE METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Applies archetype resource bonuses to the character's derived stats.
    /// </summary>
    /// <param name="character">The player character to modify.</param>
    /// <param name="bonuses">The resource bonuses to apply.</param>
    /// <remarks>
    /// <para>
    /// Applies bonuses using existing <see cref="Player"/> modifier methods:
    /// <list type="bullet">
    ///   <item><description><see cref="Player.ModifyMaxHp"/>: Adds HP bonus to lineage HP modifier</description></item>
    ///   <item><description><see cref="Player.ModifyMaxAp"/>: Adds Aether Pool bonus to lineage AP modifier</description></item>
    ///   <item><description><see cref="Player.ModifyMovement"/>: Adds Movement bonus to base movement speed</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// <strong>Note:</strong> Stamina bonuses are tracked in the result but not currently
    /// applied to the Player entity, as no dedicated <c>ModifyMaxStamina</c> method exists.
    /// This will be addressed when the Player entity is extended for the Attribute System
    /// (v0.17.2) derived stat integration.
    /// </para>
    /// <para>
    /// <strong>Note:</strong> Special bonuses (e.g., Adept's +20% Consumable Effectiveness)
    /// are tracked in the result but require combat system integration (v0.17.5+) for
    /// mechanical application.
    /// </para>
    /// </remarks>
    private void ApplyResourceBonuses(Player character, ArchetypeResourceBonuses bonuses)
    {
        _logger.LogDebug(
            "Applying resource bonuses to {CharacterName}. " +
            "HP+{HpBonus}, Stamina+{StaminaBonus}, AP+{ApBonus}, Movement+{MovementBonus}, " +
            "HasSpecial={HasSpecial}",
            character.Name,
            bonuses.MaxHpBonus,
            bonuses.MaxStaminaBonus,
            bonuses.MaxAetherPoolBonus,
            bonuses.MovementBonus,
            bonuses.HasSpecialBonus);

        // Apply HP bonus via existing modifier method
        if (bonuses.MaxHpBonus > 0)
        {
            character.ModifyMaxHp(bonuses.MaxHpBonus);

            _logger.LogDebug(
                "Applied HP bonus. CharacterId={CharacterId}, " +
                "HpBonus=+{HpBonus}",
                character.Id,
                bonuses.MaxHpBonus);
        }

        // Apply Aether Pool bonus via existing modifier method
        if (bonuses.MaxAetherPoolBonus > 0)
        {
            character.ModifyMaxAp(bonuses.MaxAetherPoolBonus);

            _logger.LogDebug(
                "Applied Aether Pool bonus. CharacterId={CharacterId}, " +
                "ApBonus=+{ApBonus}",
                character.Id,
                bonuses.MaxAetherPoolBonus);
        }

        // Apply Movement bonus via existing modifier method
        if (bonuses.MovementBonus > 0)
        {
            character.ModifyMovement(bonuses.MovementBonus);

            _logger.LogDebug(
                "Applied Movement bonus. CharacterId={CharacterId}, " +
                "MovementBonus=+{MovementBonus}",
                character.Id,
                bonuses.MovementBonus);
        }

        // Stamina bonus is tracked in the result but not currently applied to Player entity
        // The Player entity does not yet have a ModifyMaxStamina method; this will be
        // integrated when derived stats are fully connected in v0.17.5
        if (bonuses.MaxStaminaBonus > 0)
        {
            _logger.LogDebug(
                "Stamina bonus recorded but deferred for Player entity integration. " +
                "CharacterId={CharacterId}, StaminaBonus=+{StaminaBonus}",
                character.Id,
                bonuses.MaxStaminaBonus);
        }

        // Special bonuses are tracked in the result for future combat system integration
        if (bonuses.HasSpecialBonus)
        {
            _logger.LogDebug(
                "Special bonus recorded for combat system integration. " +
                "CharacterId={CharacterId}, BonusType={BonusType}, " +
                "BonusValue={BonusValue}, Description={Description}",
                character.Id,
                bonuses.SpecialBonus!.Value.BonusType,
                bonuses.SpecialBonus.Value.BonusValue,
                bonuses.SpecialBonus.Value.Description);
        }

        _logger.LogDebug(
            "Completed resource bonus application. CharacterId={CharacterId}, " +
            "BonusSummary={BonusSummary}",
            character.Id,
            bonuses.GetDisplaySummary());
    }

    /// <summary>
    /// Grants starting abilities to the character.
    /// </summary>
    /// <param name="character">The player character to modify.</param>
    /// <param name="abilities">The starting abilities to grant.</param>
    /// <remarks>
    /// <para>
    /// Creates <see cref="PlayerAbility"/> instances for each starting ability and
    /// adds them to the character's ability collection via <see cref="Player.AddAbility"/>.
    /// All starting abilities are granted as unlocked (<c>isUnlocked: true</c>).
    /// </para>
    /// <para>
    /// Each archetype provides exactly 3 starting abilities with varying types:
    /// Active (requires activation), Passive (always active), or Stance (toggle state).
    /// </para>
    /// </remarks>
    private void GrantAbilities(
        Player character,
        IReadOnlyList<ArchetypeAbilityGrant> abilities)
    {
        _logger.LogDebug(
            "Granting {AbilityCount} starting abilities to {CharacterName}. " +
            "CharacterId={CharacterId}",
            abilities.Count,
            character.Name,
            character.Id);

        foreach (var grant in abilities)
        {
            // Create a PlayerAbility instance from the grant's ability ID
            var playerAbility = PlayerAbility.Create(
                grant.AbilityId,
                isUnlocked: true);

            character.AddAbility(playerAbility);

            _logger.LogDebug(
                "Granted ability {AbilityId} ({AbilityType}) to {CharacterName}. " +
                "AbilityName={AbilityName}, IsPassive={IsPassive}, " +
                "RequiresActivation={RequiresActivation}, " +
                "CharacterId={CharacterId}",
                grant.AbilityId,
                grant.AbilityType,
                character.Name,
                grant.AbilityName,
                grant.IsPassive,
                grant.RequiresActivation,
                character.Id);
        }

        _logger.LogInformation(
            "Granted {Count} starting abilities to {CharacterName}. " +
            "Abilities=[{AbilityList}], CharacterId={CharacterId}",
            abilities.Count,
            character.Name,
            string.Join(", ", abilities.Select(a => $"{a.AbilityName} ({a.AbilityType})")),
            character.Id);
    }
}
