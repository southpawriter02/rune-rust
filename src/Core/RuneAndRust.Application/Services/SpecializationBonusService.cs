// ------------------------------------------------------------------------------
// <copyright file="SpecializationBonusService.cs" company="Rune & Rust">
//     Copyright (c) Rune & Rust. All rights reserved.
// </copyright>
// <summary>
// Service implementation for managing Wasteland Survival specialization abilities,
// including passive bonuses, active abilities, and hunting grounds markers.
// Part of v0.15.5h Specialization Integration implementation.
// </summary>
// ------------------------------------------------------------------------------

using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Services;

/// <summary>
/// Service for managing Wasteland Survival specialization bonuses and abilities.
/// </summary>
/// <remarks>
/// <para>
/// This service handles all specialization abilities related to the Wasteland Survival
/// skill system, including passive bonuses, active abilities, and hunting grounds markers.
/// </para>
/// <para>
/// <b>Supported Specializations:</b>
/// <list type="bullet">
///   <item><description><b>Veioimaor (Hunter):</b> Beast Tracker (+2d10), Predator's Eye, Hunting Grounds (+2d10)</description></item>
///   <item><description><b>Myr-Stalker:</b> Swamp Navigator (+1d10), Toxin Resistance (advantage), Mire Knowledge</description></item>
///   <item><description><b>Gantry-Runner:</b> Urban Navigator (+1d10), Rooftop Routes, Scrap Familiar (+1d10)</description></item>
/// </list>
/// </para>
/// </remarks>
public class SpecializationBonusService : ISpecializationBonusService
{
    // =========================================================================
    // CONSTANTS - ABILITY IDS
    // =========================================================================

    /// <summary>Ability ID for Beast Tracker (Veioimaor).</summary>
    private const string BeastTrackerId = "beast-tracker";

    /// <summary>Ability ID for Predator's Eye (Veioimaor).</summary>
    private const string PredatorsEyeId = "predators-eye";

    /// <summary>Ability ID for Hunting Grounds (Veioimaor).</summary>
    private const string HuntingGroundsId = "hunting-grounds";

    /// <summary>Ability ID for Swamp Navigator (Myr-Stalker).</summary>
    private const string SwampNavigatorId = "swamp-navigator";

    /// <summary>Ability ID for Toxin Resistance (Myr-Stalker).</summary>
    private const string ToxinResistanceId = "toxin-resistance";

    /// <summary>Ability ID for Mire Knowledge (Myr-Stalker).</summary>
    private const string MireKnowledgeId = "mire-knowledge";

    /// <summary>Ability ID for Urban Navigator (Gantry-Runner).</summary>
    private const string UrbanNavigatorId = "urban-navigator";

    /// <summary>Ability ID for Rooftop Routes (Gantry-Runner).</summary>
    private const string RooftopRoutesId = "rooftop-routes";

    /// <summary>Ability ID for Scrap Familiar (Gantry-Runner).</summary>
    private const string ScrapFamiliarId = "scrap-familiar";

    // =========================================================================
    // CONSTANTS - BONUS DICE
    // =========================================================================

    /// <summary>Bonus dice for Beast Tracker ability.</summary>
    private const int BeastTrackerBonusDice = 2;

    /// <summary>Bonus dice for Swamp Navigator ability.</summary>
    private const int SwampNavigatorBonusDice = 1;

    /// <summary>Bonus dice for Urban Navigator ability.</summary>
    private const int UrbanNavigatorBonusDice = 1;

    /// <summary>Bonus dice for Scrap Familiar ability.</summary>
    private const int ScrapFamiliarBonusDice = 1;

    /// <summary>Bonus dice for Hunting Grounds ability.</summary>
    private const int HuntingGroundsBonusDice = 2;

    // =========================================================================
    // PRIVATE FIELDS
    // =========================================================================

    /// <summary>Logger for diagnostic output.</summary>
    private readonly ILogger<SpecializationBonusService> _logger;

    /// <summary>Mapping of character IDs to their Wasteland Survival specializations.</summary>
    private readonly Dictionary<string, WastelandSurvivalSpecializationType> _characterSpecializations = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Mapping of character IDs to their active hunting grounds markers.</summary>
    private readonly Dictionary<string, HuntingGroundsMarker> _huntingGrounds = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Mapping of specializations to their ability IDs.</summary>
    private static readonly Dictionary<WastelandSurvivalSpecializationType, HashSet<string>> SpecializationAbilities = new()
    {
        [WastelandSurvivalSpecializationType.None] = new HashSet<string>(StringComparer.OrdinalIgnoreCase),
        [WastelandSurvivalSpecializationType.Veioimaor] = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            BeastTrackerId,
            PredatorsEyeId,
            HuntingGroundsId
        },
        [WastelandSurvivalSpecializationType.MyrStalker] = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            SwampNavigatorId,
            ToxinResistanceId,
            MireKnowledgeId
        },
        [WastelandSurvivalSpecializationType.GantryRunner] = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            UrbanNavigatorId,
            RooftopRoutesId,
            ScrapFamiliarId
        }
    };

    // =========================================================================
    // CONSTRUCTOR
    // =========================================================================

    /// <summary>
    /// Initializes a new instance of the <see cref="SpecializationBonusService"/> class.
    /// </summary>
    /// <param name="logger">Logger for diagnostic output.</param>
    /// <exception cref="ArgumentNullException">Thrown when logger is null.</exception>
    public SpecializationBonusService(ILogger<SpecializationBonusService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _logger.LogDebug("SpecializationBonusService initialized");
    }

    // =========================================================================
    // SPECIALIZATION QUERIES
    // =========================================================================

    /// <inheritdoc/>
    public WastelandSurvivalSpecializationType GetCharacterSpecialization(string characterId)
    {
        if (string.IsNullOrWhiteSpace(characterId))
        {
            _logger.LogWarning("GetCharacterSpecialization called with null or empty characterId");
            return WastelandSurvivalSpecializationType.None;
        }

        if (_characterSpecializations.TryGetValue(characterId, out var specialization))
        {
            _logger.LogDebug(
                "Character {CharacterId} has specialization {Specialization}",
                characterId,
                specialization);
            return specialization;
        }

        _logger.LogDebug(
            "Character {CharacterId} has no registered specialization, returning None",
            characterId);
        return WastelandSurvivalSpecializationType.None;
    }

    /// <inheritdoc/>
    public void RegisterCharacterSpecialization(string characterId, WastelandSurvivalSpecializationType specialization)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(characterId, nameof(characterId));

        _characterSpecializations[characterId] = specialization;

        _logger.LogInformation(
            "Character {CharacterId} registered with specialization {Specialization}",
            characterId,
            specialization);
    }

    /// <inheritdoc/>
    public void UnregisterCharacter(string characterId)
    {
        if (string.IsNullOrWhiteSpace(characterId))
        {
            return;
        }

        var wasRegistered = _characterSpecializations.Remove(characterId);
        var hadHuntingGrounds = _huntingGrounds.Remove(characterId);

        if (wasRegistered || hadHuntingGrounds)
        {
            _logger.LogInformation(
                "Character {CharacterId} unregistered (specialization: {HadSpec}, huntingGrounds: {HadMarker})",
                characterId,
                wasRegistered,
                hadHuntingGrounds);
        }
    }

    // =========================================================================
    // PASSIVE BONUS APPLICATION
    // =========================================================================

    /// <inheritdoc/>
    public IReadOnlyList<SpecializationBonus> GetBonusesForCheck(
        string characterId,
        WastelandSurvivalCheckType checkType,
        NavigationTerrainType terrain,
        TargetType targetType = TargetType.Unknown,
        string? areaId = null)
    {
        if (string.IsNullOrWhiteSpace(characterId))
        {
            _logger.LogWarning("GetBonusesForCheck called with null or empty characterId");
            return Array.Empty<SpecializationBonus>();
        }

        var specialization = GetCharacterSpecialization(characterId);

        _logger.LogDebug(
            "Evaluating bonuses for character {CharacterId} ({Specialization}) - Check: {CheckType}, Terrain: {Terrain}, Target: {TargetType}",
            characterId,
            specialization,
            checkType,
            terrain,
            targetType);

        if (specialization == WastelandSurvivalSpecializationType.None)
        {
            _logger.LogDebug("Character {CharacterId} has no specialization, returning empty bonuses", characterId);
            return Array.Empty<SpecializationBonus>();
        }

        var bonuses = new List<SpecializationBonus>();

        // Evaluate specialization-specific bonuses
        switch (specialization)
        {
            case WastelandSurvivalSpecializationType.Veioimaor:
                EvaluateVeioimaorBonuses(characterId, checkType, targetType, areaId, bonuses);
                break;

            case WastelandSurvivalSpecializationType.MyrStalker:
                EvaluateMyrStalkerBonuses(characterId, checkType, terrain, bonuses);
                break;

            case WastelandSurvivalSpecializationType.GantryRunner:
                EvaluateGantryRunnerBonuses(characterId, checkType, terrain, bonuses);
                break;
        }

        _logger.LogDebug(
            "Found {Count} applicable bonuses for character {CharacterId}",
            bonuses.Count,
            characterId);

        return bonuses;
    }

    /// <inheritdoc/>
    public int GetTotalBonusDice(
        string characterId,
        WastelandSurvivalCheckType checkType,
        NavigationTerrainType terrain,
        TargetType targetType = TargetType.Unknown,
        string? areaId = null)
    {
        var bonuses = GetBonusesForCheck(characterId, checkType, terrain, targetType, areaId);
        var totalDice = bonuses.Sum(b => b.BonusDice);

        _logger.LogDebug(
            "Total bonus dice for character {CharacterId}: {TotalDice}d10",
            characterId,
            totalDice);

        return totalDice;
    }

    /// <inheritdoc/>
    public bool HasAdvantage(string characterId, HazardType hazardType)
    {
        if (string.IsNullOrWhiteSpace(characterId))
        {
            return false;
        }

        var specialization = GetCharacterSpecialization(characterId);

        // Only Myr-Stalker has advantage via Toxin Resistance
        if (specialization != WastelandSurvivalSpecializationType.MyrStalker)
        {
            return false;
        }

        // Toxin Resistance only applies to PoisonGas hazards
        var hasAdvantage = hazardType == HazardType.PoisonGas;

        if (hasAdvantage)
        {
            _logger.LogDebug(
                "Character {CharacterId} has advantage on {HazardType} save via Toxin Resistance",
                characterId,
                hazardType);
        }

        return hasAdvantage;
    }

    // =========================================================================
    // ABILITY QUERIES
    // =========================================================================

    /// <inheritdoc/>
    public bool HasAbility(string characterId, string abilityId)
    {
        if (string.IsNullOrWhiteSpace(characterId) || string.IsNullOrWhiteSpace(abilityId))
        {
            return false;
        }

        var specialization = GetCharacterSpecialization(characterId);

        if (!SpecializationAbilities.TryGetValue(specialization, out var abilities))
        {
            return false;
        }

        return abilities.Contains(abilityId);
    }

    // =========================================================================
    // ACTIVE ABILITY ACTIVATION
    // =========================================================================

    /// <inheritdoc/>
    public AbilityActivation? ActivateAbility(
        string characterId,
        string abilityId,
        IReadOnlyDictionary<string, string> context)
    {
        if (string.IsNullOrWhiteSpace(characterId) || string.IsNullOrWhiteSpace(abilityId))
        {
            _logger.LogWarning(
                "ActivateAbility called with invalid parameters: characterId={CharacterId}, abilityId={AbilityId}",
                characterId ?? "null",
                abilityId ?? "null");
            return null;
        }

        if (!HasAbility(characterId, abilityId))
        {
            _logger.LogWarning(
                "Character {CharacterId} attempted to activate ability {AbilityId} they don't have",
                characterId,
                abilityId);
            return null;
        }

        _logger.LogInformation(
            "Character {CharacterId} activating ability {AbilityId}",
            characterId,
            abilityId);

        return abilityId.ToLowerInvariant() switch
        {
            HuntingGroundsId => ActivateHuntingGrounds(characterId, context),
            PredatorsEyeId => ActivatePredatorsEye(context),
            MireKnowledgeId => ActivateMireKnowledge(context),
            RooftopRoutesId => ActivateRooftopRoutes(context),
            _ => null
        };
    }

    // =========================================================================
    // HUNTING GROUNDS MANAGEMENT
    // =========================================================================

    /// <inheritdoc/>
    public HuntingGroundsMarker? GetActiveHuntingGrounds(string characterId)
    {
        if (string.IsNullOrWhiteSpace(characterId))
        {
            return null;
        }

        if (_huntingGrounds.TryGetValue(characterId, out var marker) && marker.IsActive)
        {
            return marker;
        }

        return null;
    }

    /// <inheritdoc/>
    public HuntingGroundsMarker? MarkHuntingGrounds(string characterId, string areaId, string areaName)
    {
        if (string.IsNullOrWhiteSpace(characterId) ||
            string.IsNullOrWhiteSpace(areaId))
        {
            return null;
        }

        if (!HasAbility(characterId, HuntingGroundsId))
        {
            _logger.LogWarning(
                "Character {CharacterId} cannot mark hunting grounds (lacks ability)",
                characterId);
            return null;
        }

        var marker = HuntingGroundsMarker.Create(characterId, areaId, areaName);
        _huntingGrounds[characterId] = marker;

        _logger.LogInformation(
            "Character {CharacterId} marked area {AreaId} ({AreaName}) as hunting grounds",
            characterId,
            areaId,
            areaName);

        return marker;
    }

    /// <inheritdoc/>
    public void ClearHuntingGrounds(string characterId)
    {
        if (string.IsNullOrWhiteSpace(characterId))
        {
            return;
        }

        if (_huntingGrounds.Remove(characterId))
        {
            _logger.LogDebug("Cleared hunting grounds for character {CharacterId}", characterId);
        }
    }

    // =========================================================================
    // TERRAIN UTILITY METHODS
    // =========================================================================

    /// <inheritdoc/>
    public bool IsSwampTerrain(NavigationTerrainType terrain)
    {
        // Currently no explicit swamp terrain in NavigationTerrainType
        // This is a placeholder for when swamp terrain is added
        // For now, we can map certain terrains or treat this as always false
        // until the terrain system is expanded
        return false;
    }

    /// <inheritdoc/>
    public bool IsRuinsTerrain(NavigationTerrainType terrain)
    {
        return terrain is NavigationTerrainType.ModerateRuins or NavigationTerrainType.DenseRuins;
    }

    // =========================================================================
    // PRIVATE HELPER METHODS - BONUS EVALUATION
    // =========================================================================

    /// <summary>
    /// Evaluates Veioimaor (Hunter) specialization bonuses.
    /// </summary>
    private void EvaluateVeioimaorBonuses(
        string characterId,
        WastelandSurvivalCheckType checkType,
        TargetType targetType,
        string? areaId,
        List<SpecializationBonus> bonuses)
    {
        // Beast Tracker: +2d10 when tracking living creatures or groups
        if (checkType == WastelandSurvivalCheckType.Tracking &&
            (targetType == TargetType.LivingCreature || targetType == TargetType.Group))
        {
            bonuses.Add(SpecializationBonus.DiceBonus(
                BeastTrackerId,
                BeastTrackerBonusDice,
                "Beast Tracker (+2d10 tracking living creatures)"));

            _logger.LogDebug(
                "Beast Tracker bonus applied for character {CharacterId} tracking {TargetType}",
                characterId,
                targetType);
        }

        // Hunting Grounds: +2d10 in marked area
        if (!string.IsNullOrWhiteSpace(areaId))
        {
            var marker = GetActiveHuntingGrounds(characterId);
            if (marker.HasValue && marker.Value.AppliesToArea(areaId))
            {
                bonuses.Add(marker.Value.GetBonus());

                _logger.LogDebug(
                    "Hunting Grounds bonus applied for character {CharacterId} in area {AreaId}",
                    characterId,
                    areaId);
            }
        }
    }

    /// <summary>
    /// Evaluates Myr-Stalker specialization bonuses.
    /// </summary>
    private void EvaluateMyrStalkerBonuses(
        string characterId,
        WastelandSurvivalCheckType checkType,
        NavigationTerrainType terrain,
        List<SpecializationBonus> bonuses)
    {
        // Swamp Navigator: +1d10 in swamp/marsh terrain
        if (IsSwampTerrain(terrain))
        {
            bonuses.Add(SpecializationBonus.DiceBonus(
                SwampNavigatorId,
                SwampNavigatorBonusDice,
                "Swamp Navigator (+1d10 in swamp terrain)"));

            _logger.LogDebug(
                "Swamp Navigator bonus applied for character {CharacterId} in terrain {Terrain}",
                characterId,
                terrain);
        }

        // Note: Toxin Resistance (advantage) is handled separately via HasAdvantage()
    }

    /// <summary>
    /// Evaluates Gantry-Runner specialization bonuses.
    /// </summary>
    private void EvaluateGantryRunnerBonuses(
        string characterId,
        WastelandSurvivalCheckType checkType,
        NavigationTerrainType terrain,
        List<SpecializationBonus> bonuses)
    {
        var isRuins = IsRuinsTerrain(terrain);

        // Urban Navigator: +1d10 in ruins terrain for all WS checks
        if (isRuins)
        {
            bonuses.Add(SpecializationBonus.DiceBonus(
                UrbanNavigatorId,
                UrbanNavigatorBonusDice,
                "Urban Navigator (+1d10 in ruins)"));

            _logger.LogDebug(
                "Urban Navigator bonus applied for character {CharacterId} in terrain {Terrain}",
                characterId,
                terrain);
        }

        // Scrap Familiar: +1d10 for foraging in ruins
        if (checkType == WastelandSurvivalCheckType.Foraging && isRuins)
        {
            bonuses.Add(SpecializationBonus.DiceBonus(
                ScrapFamiliarId,
                ScrapFamiliarBonusDice,
                "Scrap Familiar (+1d10 foraging in ruins)"));

            _logger.LogDebug(
                "Scrap Familiar bonus applied for character {CharacterId} foraging in ruins",
                characterId);
        }
    }

    // =========================================================================
    // PRIVATE HELPER METHODS - ABILITY ACTIVATION
    // =========================================================================

    /// <summary>
    /// Activates the Hunting Grounds ability.
    /// </summary>
    private AbilityActivation? ActivateHuntingGrounds(
        string characterId,
        IReadOnlyDictionary<string, string> context)
    {
        if (!context.TryGetValue("areaId", out var areaId) ||
            string.IsNullOrWhiteSpace(areaId))
        {
            _logger.LogWarning(
                "Hunting Grounds activation failed: missing areaId in context");
            return null;
        }

        context.TryGetValue("areaName", out var areaName);
        areaName ??= "this area";

        var marker = MarkHuntingGrounds(characterId, areaId, areaName);
        if (!marker.HasValue)
        {
            return null;
        }

        return AbilityActivation.HuntingGrounds(areaId, areaName);
    }

    /// <summary>
    /// Activates the Predator's Eye ability.
    /// </summary>
    private static AbilityActivation? ActivatePredatorsEye(IReadOnlyDictionary<string, string> context)
    {
        context.TryGetValue("creatureWeakness", out var weakness);
        context.TryGetValue("behaviorPattern", out var behavior);

        return AbilityActivation.PredatorsEye(weakness ?? "unknown", behavior ?? "unknown");
    }

    /// <summary>
    /// Activates the Mire Knowledge ability.
    /// </summary>
    private static AbilityActivation? ActivateMireKnowledge(IReadOnlyDictionary<string, string> context)
    {
        context.TryGetValue("pathDescription", out var path);
        context.TryGetValue("hazardsAvoided", out var hazards);

        return AbilityActivation.MireKnowledge(path ?? "a safe route through the mire", hazards);
    }

    /// <summary>
    /// Activates the Rooftop Routes ability.
    /// </summary>
    private static AbilityActivation? ActivateRooftopRoutes(IReadOnlyDictionary<string, string> context)
    {
        context.TryGetValue("routeDescription", out var route);
        context.TryGetValue("destination", out var destination);

        return AbilityActivation.RooftopRoutes(route ?? "an elevated route", destination);
    }
}
