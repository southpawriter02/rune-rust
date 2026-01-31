// ═══════════════════════════════════════════════════════════════════════════════
// SpecializationApplicationService.cs
// Service that orchestrates applying specializations and managing ability tier
// unlocks for characters during creation Step 5 and progression.
// Version: 0.17.4e
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Application.Services;

using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Application.ValueObjects;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Applies specializations to characters during creation and manages ability tier unlocks
/// during progression.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="SpecializationApplicationService"/> is the primary orchestrator for applying
/// specializations to a <see cref="Player"/> entity during character creation Step 5
/// (Specialization Selection) and for unlocking ability tiers during Saga progression.
/// It retrieves specialization data from <see cref="ISpecializationProvider"/> and applies
/// them in the following order:
/// </para>
/// <list type="number">
///   <item><description>Validate preconditions (definition exists, archetype matches, no duplicate, sufficient PP)</description></item>
///   <item><description>Deduct Progression Points (0 for first specialization, 3 for additional)</description></item>
///   <item><description>Register specialization on the player with Tier 1 unlocked</description></item>
///   <item><description>Initialize special resource if the specialization has one</description></item>
///   <item><description>Grant Tier 1 abilities (3 per specialization)</description></item>
/// </list>
/// <para>
/// <strong>Tier Unlock Flow:</strong> For progression-based tier unlocks, the service validates
/// that the previous tier is unlocked, the player meets the rank requirement, and has sufficient
/// PP. It then deducts PP, marks the tier as unlocked, and grants the tier's abilities.
/// </para>
/// <para>
/// <strong>Heretical Path Handling:</strong> When a Heretical specialization is applied, the
/// service logs a warning and includes the Corruption risk information in the result for
/// UI display. No automatic Corruption is applied — individual abilities may trigger
/// Corruption gain during combat.
/// </para>
/// <para>
/// <strong>Special Resource Initialization:</strong> 5 of 17 specializations have unique
/// combat resources (e.g., Rage for Berserkr, Block Charges for Skjaldmaer). When applied,
/// the service initializes the resource pool on the player entity.
/// </para>
/// </remarks>
/// <seealso cref="ISpecializationApplicationService"/>
/// <seealso cref="ISpecializationProvider"/>
/// <seealso cref="SpecializationApplicationResult"/>
/// <seealso cref="TierUnlockResult"/>
public class SpecializationApplicationService : ISpecializationApplicationService
{
    // ═══════════════════════════════════════════════════════════════════════════
    // PRIVATE FIELDS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Provider for accessing specialization definitions, ability tiers,
    /// special resources, and path type classifications from configuration.
    /// </summary>
    private readonly ISpecializationProvider _provider;

    /// <summary>
    /// Logger for structured diagnostic output.
    /// </summary>
    private readonly ILogger<SpecializationApplicationService> _logger;

    /// <summary>
    /// The PP cost for acquiring additional specializations beyond the first.
    /// </summary>
    /// <remarks>
    /// The first specialization is free during character creation (0 PP).
    /// Each additional specialization costs 3 PP.
    /// </remarks>
    private const int AdditionalSpecializationCost = 3;

    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTRUCTOR
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Initializes a new instance of the <see cref="SpecializationApplicationService"/> class.
    /// </summary>
    /// <param name="provider">
    /// The specialization provider for accessing definitions, ability tiers,
    /// special resources, and path type classifications. Must not be null.
    /// </param>
    /// <param name="logger">
    /// The logger for diagnostic output. Must not be null.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="provider"/> or <paramref name="logger"/> is null.
    /// </exception>
    public SpecializationApplicationService(
        ISpecializationProvider provider,
        ILogger<SpecializationApplicationService> logger)
    {
        ArgumentNullException.ThrowIfNull(provider, nameof(provider));
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));

        _provider = provider;
        _logger = logger;

        _logger.LogDebug(
            "SpecializationApplicationService initialized with ISpecializationProvider. " +
            "ProviderSpecializationCount={ProviderCount}",
            _provider.Count);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // PUBLIC METHODS — SPECIALIZATION APPLICATION
    // ═══════════════════════════════════════════════════════════════════════════

    /// <inheritdoc />
    public SpecializationApplicationResult ApplySpecialization(
        Player player,
        SpecializationId specializationId)
    {
        _logger.LogInformation(
            "Beginning specialization application. SpecializationId={SpecializationId}, " +
            "CharacterId={CharacterId}, CharacterName={CharacterName}",
            specializationId,
            player?.Id.ToString() ?? "null",
            player?.Name ?? "null");

        // Step 1: Validate preconditions
        if (player == null)
        {
            _logger.LogWarning(
                "Specialization application failed: character is null. " +
                "SpecializationId={SpecializationId}",
                specializationId);
            return SpecializationApplicationResult.CharacterRequired;
        }

        var (canApply, reason) = CanApplySpecialization(player, specializationId);
        if (!canApply)
        {
            _logger.LogWarning(
                "Specialization application failed: validation failed. " +
                "SpecializationId={SpecializationId}, Reason={Reason}, " +
                "CharacterId={CharacterId}",
                specializationId,
                reason,
                player.Id);
            return SpecializationApplicationResult.Failed(reason!);
        }

        // Step 2: Retrieve specialization definition from provider
        var definition = _provider.GetBySpecializationId(specializationId)!;

        _logger.LogDebug(
            "Retrieved specialization definition. SpecializationId={SpecializationId}, " +
            "DisplayName={DisplayName}, PathType={PathType}, " +
            "ParentArchetype={ParentArchetype}, HasSpecialResource={HasSpecialResource}, " +
            "HasAbilityTiers={HasAbilityTiers}",
            specializationId,
            definition.DisplayName,
            definition.PathType,
            definition.ParentArchetype,
            definition.HasSpecialResource,
            definition.HasAbilityTiers);

        // Step 3: Calculate and deduct PP cost
        var cost = GetNextSpecializationCost(player);

        if (cost > 0)
        {
            player.SpendProgressionPoints(cost);

            _logger.LogDebug(
                "Deducted PP for specialization. CharacterId={CharacterId}, " +
                "PpCost={PpCost}, RemainingPP={RemainingPP}",
                player.Id,
                cost,
                player.ProgressionPoints);
        }
        else
        {
            _logger.LogDebug(
                "First specialization is free. CharacterId={CharacterId}, " +
                "SpecializationId={SpecializationId}",
                player.Id,
                specializationId);
        }

        // Step 4: Register specialization on the player (auto-unlocks Tier 1)
        player.AddSpecialization(specializationId);

        _logger.LogDebug(
            "Registered specialization on player. CharacterId={CharacterId}, " +
            "SpecializationId={SpecializationId}, " +
            "TotalSpecializations={TotalSpecializations}",
            player.Id,
            specializationId,
            player.SpecializationCount);

        // Step 5: Initialize special resource if the specialization has one
        var specialResourceInitialized = false;
        if (definition.HasSpecialResource)
        {
            InitializeSpecialResource(player, definition);
            specialResourceInitialized = true;
        }

        // Step 6: Grant Tier 1 abilities
        var tier1 = definition.GetTier(1);
        var grantedAbilities = new List<SpecializationAbility>();

        if (tier1.HasValue)
        {
            GrantTierAbilities(player, tier1.Value, specializationId);
            grantedAbilities.AddRange(tier1.Value.Abilities);
        }

        // Step 7: Log Heretical warning if applicable
        if (definition.IsHeretical)
        {
            _logger.LogWarning(
                "Heretical specialization applied. CharacterId={CharacterId}, " +
                "SpecializationId={SpecializationId}, " +
                "CorruptionWarning={CorruptionWarning}",
                player.Id,
                specializationId,
                definition.GetCorruptionWarning());
        }

        _logger.LogInformation(
            "Successfully applied specialization {DisplayName} ({PathType}) to " +
            "character {CharacterName}. " +
            "AbilitiesGranted={AbilityCount}, " +
            "SpecialResourceInitialized={ResourceInitialized}, " +
            "PpCost={PpCost}, " +
            "TotalSpecializations={TotalSpecs}, " +
            "IsHeretical={IsHeretical}, " +
            "CharacterId={CharacterId}",
            definition.DisplayName,
            definition.PathType,
            player.Name,
            grantedAbilities.Count,
            specialResourceInitialized,
            cost,
            player.SpecializationCount,
            definition.IsHeretical,
            player.Id);

        return SpecializationApplicationResult.Succeeded(
            definition,
            grantedAbilities,
            specialResourceInitialized,
            cost);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // PUBLIC METHODS — TIER UNLOCK
    // ═══════════════════════════════════════════════════════════════════════════

    /// <inheritdoc />
    public TierUnlockResult UnlockTier(
        Player player,
        SpecializationId specializationId,
        int tierNumber)
    {
        ArgumentNullException.ThrowIfNull(player);
        ArgumentOutOfRangeException.ThrowIfLessThan(tierNumber, 2);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(tierNumber, 3);

        _logger.LogInformation(
            "Attempting to unlock tier {TierNumber} of {SpecializationId} for " +
            "{CharacterName}. CharacterId={CharacterId}",
            tierNumber,
            specializationId,
            player.Name,
            player.Id);

        // Step 1: Validate preconditions
        var (canUnlock, reason) = CanUnlockTier(player, specializationId, tierNumber);
        if (!canUnlock)
        {
            _logger.LogWarning(
                "Cannot unlock tier {TierNumber}: {Reason}. " +
                "SpecializationId={SpecializationId}, CharacterId={CharacterId}",
                tierNumber,
                reason,
                specializationId,
                player.Id);
            return TierUnlockResult.Failed(reason!);
        }

        // Step 2: Retrieve definition and tier from provider
        var definition = _provider.GetBySpecializationId(specializationId)!;
        var tier = definition.GetTier(tierNumber)!.Value;

        _logger.LogDebug(
            "Retrieved tier definition. SpecializationId={SpecializationId}, " +
            "Tier={Tier}, DisplayName={TierDisplayName}, " +
            "UnlockCost={UnlockCost}, AbilityCount={AbilityCount}",
            specializationId,
            tierNumber,
            tier.DisplayName,
            tier.UnlockCost,
            tier.AbilityCount);

        // Step 3: Deduct PP
        player.SpendProgressionPoints(tier.UnlockCost);

        _logger.LogDebug(
            "Deducted PP for tier unlock. CharacterId={CharacterId}, " +
            "PpCost={PpCost}, RemainingPP={RemainingPP}",
            player.Id,
            tier.UnlockCost,
            player.ProgressionPoints);

        // Step 4: Mark tier as unlocked
        player.UnlockSpecializationTier(specializationId, tierNumber);

        _logger.LogDebug(
            "Marked tier as unlocked. CharacterId={CharacterId}, " +
            "SpecializationId={SpecializationId}, Tier={Tier}",
            player.Id,
            specializationId,
            tierNumber);

        // Step 5: Grant tier abilities
        GrantTierAbilities(player, tier, specializationId);

        _logger.LogInformation(
            "Successfully unlocked tier {TierNumber} ({TierDisplayName}) of " +
            "{SpecializationId} for character {CharacterName}. " +
            "AbilitiesGranted={AbilityCount}, PpCost={PpCost}, " +
            "RemainingPP={RemainingPP}, CharacterId={CharacterId}",
            tierNumber,
            tier.DisplayName,
            specializationId,
            player.Name,
            tier.AbilityCount,
            tier.UnlockCost,
            player.ProgressionPoints,
            player.Id);

        return TierUnlockResult.Succeeded(tier);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // PUBLIC METHODS — VALIDATION
    // ═══════════════════════════════════════════════════════════════════════════

    /// <inheritdoc />
    public (bool CanApply, string? Reason) CanApplySpecialization(
        Player player,
        SpecializationId specializationId)
    {
        _logger.LogDebug(
            "Validating specialization application. SpecializationId={SpecializationId}, " +
            "CharacterId={CharacterId}, CharacterName={CharacterName}",
            specializationId,
            player?.Id.ToString() ?? "null",
            player?.Name ?? "null");

        // Check 1: Specialization definition must exist in the provider
        var definition = _provider.GetBySpecializationId(specializationId);
        if (definition == null)
        {
            _logger.LogDebug(
                "Validation failed: specialization not found. " +
                "SpecializationId={SpecializationId}",
                specializationId);
            return (false, $"Specialization '{specializationId}' not found.");
        }

        // Check 2: Player's archetype must match the specialization's parent archetype
        if (player!.Archetype != definition.ParentArchetype)
        {
            _logger.LogDebug(
                "Validation failed: archetype mismatch. " +
                "SpecializationId={SpecializationId}, " +
                "RequiredArchetype={RequiredArchetype}, " +
                "PlayerArchetype={PlayerArchetype}, " +
                "CharacterId={CharacterId}",
                specializationId,
                definition.ParentArchetype,
                player.Archetype?.ToString() ?? player.ArchetypeId ?? "none",
                player.Id);
            return (false, $"Requires {definition.ParentArchetype} archetype.");
        }

        // Check 3: Player must not already have this specialization
        if (player.HasSpecialization(specializationId))
        {
            _logger.LogDebug(
                "Validation failed: already has specialization. " +
                "SpecializationId={SpecializationId}, CharacterId={CharacterId}",
                specializationId,
                player.Id);
            return (false, "Already has this specialization.");
        }

        // Check 4: Player must have sufficient PP
        var cost = GetNextSpecializationCost(player);
        if (player.ProgressionPoints < cost)
        {
            _logger.LogDebug(
                "Validation failed: insufficient PP. " +
                "SpecializationId={SpecializationId}, " +
                "RequiredPP={RequiredPP}, AvailablePP={AvailablePP}, " +
                "CharacterId={CharacterId}",
                specializationId,
                cost,
                player.ProgressionPoints,
                player.Id);
            return (false, $"Requires {cost} PP ({player.ProgressionPoints} available).");
        }

        _logger.LogDebug(
            "Specialization validation passed. SpecializationId={SpecializationId}, " +
            "CharacterId={CharacterId}, PpCost={PpCost}",
            specializationId,
            player.Id,
            cost);

        return (true, null);
    }

    /// <inheritdoc />
    public (bool CanUnlock, string? Reason) CanUnlockTier(
        Player player,
        SpecializationId specializationId,
        int tierNumber)
    {
        _logger.LogDebug(
            "Validating tier unlock. SpecializationId={SpecializationId}, " +
            "Tier={Tier}, CharacterId={CharacterId}",
            specializationId,
            tierNumber,
            player?.Id.ToString() ?? "null");

        // Check 1: Player must have this specialization
        if (!player!.HasSpecialization(specializationId))
        {
            _logger.LogDebug(
                "Validation failed: does not have specialization. " +
                "SpecializationId={SpecializationId}, CharacterId={CharacterId}",
                specializationId,
                player.Id);
            return (false, "Does not have this specialization.");
        }

        // Check 2: Specialization definition must exist
        var definition = _provider.GetBySpecializationId(specializationId);
        if (definition == null)
        {
            _logger.LogDebug(
                "Validation failed: specialization not found. " +
                "SpecializationId={SpecializationId}",
                specializationId);
            return (false, $"Specialization '{specializationId}' not found.");
        }

        // Check 3: Tier must exist in the definition
        var tier = definition.GetTier(tierNumber);
        if (!tier.HasValue)
        {
            _logger.LogDebug(
                "Validation failed: tier not found. " +
                "SpecializationId={SpecializationId}, Tier={Tier}",
                specializationId,
                tierNumber);
            return (false, $"Tier {tierNumber} not found for '{specializationId}'.");
        }

        // Check 4: Tier must not already be unlocked
        if (player.HasUnlockedTier(specializationId, tierNumber))
        {
            _logger.LogDebug(
                "Validation failed: tier already unlocked. " +
                "SpecializationId={SpecializationId}, Tier={Tier}, " +
                "CharacterId={CharacterId}",
                specializationId,
                tierNumber,
                player.Id);
            return (false, $"Tier {tierNumber} is already unlocked.");
        }

        // Check 5: Delegate to tier's CanUnlock for previous tier, rank, and PP checks
        var (canUnlock, unlockReason) = tier.Value.CanUnlock(
            player.ProgressionRank,
            player.HasUnlockedTier(specializationId, tierNumber - 1),
            player.ProgressionPoints);

        if (!canUnlock)
        {
            _logger.LogDebug(
                "Validation failed: tier unlock requirements not met. " +
                "SpecializationId={SpecializationId}, Tier={Tier}, " +
                "Reason={Reason}, Rank={Rank}, PP={PP}, " +
                "PreviousTierUnlocked={PrevUnlocked}, " +
                "CharacterId={CharacterId}",
                specializationId,
                tierNumber,
                unlockReason,
                player.ProgressionRank,
                player.ProgressionPoints,
                player.HasUnlockedTier(specializationId, tierNumber - 1),
                player.Id);
        }
        else
        {
            _logger.LogDebug(
                "Tier unlock validation passed. SpecializationId={SpecializationId}, " +
                "Tier={Tier}, CharacterId={CharacterId}",
                specializationId,
                tierNumber,
                player.Id);
        }

        return (canUnlock, unlockReason);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // PUBLIC METHODS — QUERY
    // ═══════════════════════════════════════════════════════════════════════════

    /// <inheritdoc />
    public IReadOnlyList<SpecializationDefinition> GetAvailableSpecializations(Player player)
    {
        ArgumentNullException.ThrowIfNull(player);

        var archetype = player.Archetype;

        if (archetype == null)
        {
            _logger.LogDebug(
                "No specializations available: player has no archetype. " +
                "CharacterId={CharacterId}",
                player.Id);
            return Array.Empty<SpecializationDefinition>();
        }

        var specializations = _provider.GetByArchetype(archetype.Value);

        _logger.LogDebug(
            "Retrieved available specializations. CharacterId={CharacterId}, " +
            "Archetype={Archetype}, AvailableCount={Count}",
            player.Id,
            archetype.Value,
            specializations.Count);

        return specializations;
    }

    /// <inheritdoc />
    public int GetNextSpecializationCost(Player player)
    {
        ArgumentNullException.ThrowIfNull(player);

        var cost = player.SpecializationCount == 0 ? 0 : AdditionalSpecializationCost;

        _logger.LogDebug(
            "Calculated next specialization cost. CharacterId={CharacterId}, " +
            "CurrentSpecCount={CurrentCount}, NextCost={NextCost}",
            player.Id,
            player.SpecializationCount,
            cost);

        return cost;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // PRIVATE METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Grants all abilities in a tier to the player.
    /// </summary>
    /// <param name="player">The player to grant abilities to.</param>
    /// <param name="tier">The ability tier containing the abilities to grant.</param>
    /// <param name="specializationId">The specialization ID for logging.</param>
    /// <remarks>
    /// Creates <see cref="PlayerAbility"/> instances via <see cref="Player.GrantAbility"/>
    /// for each ability in the tier. All abilities are granted as unlocked.
    /// </remarks>
    private void GrantTierAbilities(
        Player player,
        SpecializationAbilityTier tier,
        SpecializationId specializationId)
    {
        _logger.LogDebug(
            "Granting {AbilityCount} tier {Tier} abilities to {CharacterName}. " +
            "SpecializationId={SpecializationId}, TierDisplayName={TierDisplayName}, " +
            "CharacterId={CharacterId}",
            tier.AbilityCount,
            tier.Tier,
            player.Name,
            specializationId,
            tier.DisplayName,
            player.Id);

        foreach (var ability in tier.Abilities)
        {
            player.GrantAbility(ability.AbilityId);

            _logger.LogDebug(
                "Granted ability {AbilityId} ({AbilityType}) to {CharacterName}. " +
                "AbilityName={AbilityName}, IsPassive={IsPassive}, " +
                "SpecializationId={SpecializationId}, Tier={Tier}, " +
                "CharacterId={CharacterId}",
                ability.AbilityId,
                ability.TypeDisplay,
                player.Name,
                ability.DisplayName,
                ability.IsPassive,
                specializationId,
                tier.Tier,
                player.Id);
        }

        _logger.LogInformation(
            "Granted {Count} tier {Tier} abilities to {CharacterName}. " +
            "Abilities=[{AbilityList}], SpecializationId={SpecializationId}, " +
            "CharacterId={CharacterId}",
            tier.AbilityCount,
            tier.Tier,
            player.Name,
            string.Join(", ", tier.Abilities.Select(a => $"{a.DisplayName} ({a.TypeDisplay})")),
            specializationId,
            player.Id);
    }

    /// <summary>
    /// Initializes the special resource for a specialization on the player.
    /// </summary>
    /// <param name="player">The player to initialize the resource on.</param>
    /// <param name="definition">The specialization definition containing the resource.</param>
    /// <remarks>
    /// <para>
    /// Delegates to <see cref="Player.InitializeSpecialResource"/> which wraps
    /// <see cref="Player.InitializeResource"/>. The resource is initialized with
    /// the max value from the definition, starting at 0 or max depending on
    /// <see cref="SpecialResourceDefinition.StartsAt"/>.
    /// </para>
    /// <para>
    /// Special resources by specialization:
    /// <list type="bullet">
    ///   <item><description>Berserkr: Rage (0-100, starts at 0)</description></item>
    ///   <item><description>Skjaldmaer: Block Charges (0-3, starts at 3)</description></item>
    ///   <item><description>Iron-Bane: Righteous Fervor (0-50, starts at 0)</description></item>
    ///   <item><description>Seiðkona: Aether Resonance (0-10, starts at 0)</description></item>
    ///   <item><description>Echo-Caller: Echoes (0-5, starts at 0)</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    private void InitializeSpecialResource(Player player, SpecializationDefinition definition)
    {
        var resource = definition.SpecialResource;

        _logger.LogDebug(
            "Initializing special resource for {CharacterName}. " +
            "SpecializationId={SpecializationId}, " +
            "ResourceId={ResourceId}, ResourceName={ResourceName}, " +
            "MaxValue={MaxValue}, StartsAt={StartsAt}, " +
            "CharacterId={CharacterId}",
            player.Name,
            definition.SpecializationId,
            resource.ResourceId,
            resource.DisplayName,
            resource.MaxValue,
            resource.StartsAt,
            player.Id);

        player.InitializeSpecialResource(resource);

        _logger.LogInformation(
            "Initialized special resource {ResourceName} ({MinValue}-{MaxValue}, " +
            "starts at {StartsAt}) for {CharacterName}. " +
            "SpecializationId={SpecializationId}, CharacterId={CharacterId}",
            resource.DisplayName,
            resource.MinValue,
            resource.MaxValue,
            resource.StartsAt,
            player.Name,
            definition.SpecializationId,
            player.Id);
    }
}
