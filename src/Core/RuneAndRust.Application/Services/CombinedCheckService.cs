using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Interfaces;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Services;

/// <summary>
/// Implements combined skill checks for the Exploration Synergies system.
/// </summary>
/// <remarks>
/// <para>
/// The CombinedCheckService handles multi-skill exploration actions:
/// <list type="bullet">
///   <item><description><b>Find Hidden Path</b>: Navigation → Acrobatics (traverse)</description></item>
///   <item><description><b>Track to Lair</b>: Tracking → System Bypass (enter)</description></item>
///   <item><description><b>Avoid Patrol</b>: Hazard Detection → Acrobatics (stealth)</description></item>
///   <item><description><b>Find and Loot</b>: Foraging → System Bypass (lockpick)</description></item>
/// </list>
/// </para>
/// <para>
/// Each synergy follows the pattern:
/// <list type="number">
///   <item><description>Execute primary Wasteland Survival check</description></item>
///   <item><description>If primary succeeds, execute secondary skill check</description></item>
///   <item><description>Combine results with appropriate narrative</description></item>
/// </list>
/// </para>
/// </remarks>
public class CombinedCheckService : ICombinedCheckService
{
    private readonly ISkillService _skillService;
    private readonly ILogger<CombinedCheckService> _logger;

    private readonly Dictionary<SynergyType, SkillSynergyDefinition> _synergies;

    /// <summary>
    /// Initializes a new instance of the <see cref="CombinedCheckService"/> class.
    /// </summary>
    /// <param name="skillService">Service for performing skill checks.</param>
    /// <param name="logger">Logger for diagnostic output.</param>
    /// <exception cref="ArgumentNullException">Thrown when a required dependency is null.</exception>
    public CombinedCheckService(
        ISkillService skillService,
        ILogger<CombinedCheckService> logger)
    {
        ArgumentNullException.ThrowIfNull(skillService);
        ArgumentNullException.ThrowIfNull(logger);

        _skillService = skillService;
        _logger = logger;

        _synergies = InitializeSynergies();
    }

    private static Dictionary<SynergyType, SkillSynergyDefinition> InitializeSynergies()
    {
        return new Dictionary<SynergyType, SkillSynergyDefinition>
        {
            [SynergyType.FindHiddenPath] = SkillSynergyDefinition.FindHiddenPath(),
            [SynergyType.TrackToLair] = SkillSynergyDefinition.TrackToLair(),
            [SynergyType.AvoidPatrol] = SkillSynergyDefinition.AvoidPatrol(),
            [SynergyType.FindAndLoot] = SkillSynergyDefinition.FindAndLoot()
        };
    }

    /// <inheritdoc />
    public CombinedCheckResult ExecuteCombinedCheck(
        Player player,
        SynergyType synergyType,
        CombinedCheckContext context)
    {
        ArgumentNullException.ThrowIfNull(player);

        _logger.LogInformation(
            "Executing combined check {SynergyType} for player {PlayerId}",
            synergyType, player.Id);

        var definition = GetSynergyDefinition(synergyType);

        // Execute primary check
        var primaryResult = ExecutePrimaryCheck(player, synergyType, context);

        _logger.LogDebug(
            "Primary check result: Succeeded={Succeeded}, Fumble={Fumble}, Margin={Margin}",
            primaryResult.IsSuccess, primaryResult.IsFumble, primaryResult.Margin);

        // Determine if secondary should execute
        if (!ShouldExecuteSecondary(synergyType, primaryResult))
        {
            _logger.LogDebug("Secondary check skipped: primary did not meet timing requirement");

            var narrative = GenerateNarrative(synergyType, primaryResult, null);
            return CombinedCheckResult.PrimaryFailed(synergyType, primaryResult, narrative);
        }

        // Execute secondary check
        var secondaryResult = ExecuteSecondaryCheck(player, synergyType, context);

        _logger.LogDebug(
            "Secondary check result: Succeeded={Succeeded}, Fumble={Fumble}, Margin={Margin}",
            secondaryResult.IsSuccess, secondaryResult.IsFumble, secondaryResult.Margin);

        // Build combined result
        var combinedNarrative = GenerateNarrative(synergyType, primaryResult, secondaryResult);

        if (secondaryResult.IsSuccess)
        {
            _logger.LogInformation(
                "Combined check {SynergyType} FULL SUCCESS for player {PlayerId}",
                synergyType, player.Id);

            return CombinedCheckResult.FullSuccess(
                synergyType, primaryResult, secondaryResult, combinedNarrative);
        }
        else
        {
            _logger.LogInformation(
                "Combined check {SynergyType} PARTIAL SUCCESS for player {PlayerId}",
                synergyType, player.Id);

            return CombinedCheckResult.PartialSuccess(
                synergyType, primaryResult, secondaryResult, combinedNarrative);
        }
    }

    private SimpleCheckOutcome ExecutePrimaryCheck(
        Player player,
        SynergyType synergyType,
        CombinedCheckContext context)
    {
        var definition = GetSynergyDefinition(synergyType);
        var skillId = GetPrimarySkillId(definition.PrimarySkill);
        var dc = context.PrimaryDc ?? GetDefaultPrimaryDc(synergyType);

        _logger.LogDebug(
            "Executing primary check for {SynergyType}: skill={Skill}, DC={Dc}",
            synergyType, skillId, dc);

        var outcome = _skillService.PerformSkillCheck(player, skillId, dc);
        return ConvertToSimpleOutcome(outcome);
    }

    private SimpleCheckOutcome ExecuteSecondaryCheck(
        Player player,
        SynergyType synergyType,
        CombinedCheckContext context)
    {
        var definition = GetSynergyDefinition(synergyType);
        var skillId = definition.SecondarySkillId;
        var dc = context.SecondaryDc ?? GetDefaultSecondaryDc(synergyType);

        _logger.LogDebug(
            "Executing secondary check for {SynergyType}: skill={Skill}, DC={Dc}",
            synergyType, skillId, dc);

        var outcome = _skillService.PerformSkillCheck(player, skillId, dc);
        return ConvertToSimpleOutcome(outcome);
    }

    private static SimpleCheckOutcome ConvertToSimpleOutcome(SkillCheckOutcome outcome)
    {
        // Determine if fumble (margin <= -5) or critical (margin >= 5)
        var isFumble = outcome.Margin <= -5;
        var isCritical = outcome.Margin >= 5;

        return new SimpleCheckOutcome(
            outcome.Success,
            outcome.Margin,
            isFumble,
            isCritical,
            outcome.Message);
    }


    private static string GetPrimarySkillId(WastelandSurvivalCheckType checkType)
    {
        return checkType switch
        {
            WastelandSurvivalCheckType.Navigation => "wasteland-survival",
            WastelandSurvivalCheckType.Tracking => "wasteland-survival",
            WastelandSurvivalCheckType.HazardDetection => "wasteland-survival",
            WastelandSurvivalCheckType.Foraging => "wasteland-survival",
            _ => "wasteland-survival"
        };
    }

    private static int GetDefaultPrimaryDc(SynergyType synergyType)
    {
        // Default DCs from design specification
        return synergyType switch
        {
            SynergyType.FindHiddenPath => 4,      // Hidden path navigation DC (base + 4)
            SynergyType.TrackToLair => 3,         // Standard tracking DC
            SynergyType.AvoidPatrol => 3,         // Ambush detection DC (16 in sum-based = ~3 in success-counting)
            SynergyType.FindAndLoot => 4,         // Cache discovery DC (22 in sum-based = ~4 in success-counting)
            _ => 3
        };
    }

    private static int GetDefaultSecondaryDc(SynergyType synergyType)
    {
        // Default DCs from design specification (mid-tier)
        return synergyType switch
        {
            SynergyType.FindHiddenPath => 3,      // Collapsed tunnel traversal
            SynergyType.TrackToLair => 2,         // Burrow entry
            SynergyType.AvoidPatrol => 3,         // Alert patrol evasion
            SynergyType.FindAndLoot => 3,         // Combination lock
            _ => 3
        };
    }

    /// <inheritdoc />
    public SkillSynergyDefinition GetSynergyDefinition(SynergyType synergyType)
    {
        if (_synergies.TryGetValue(synergyType, out var definition))
        {
            return definition;
        }

        _logger.LogWarning("Unknown synergy type requested: {SynergyType}", synergyType);
        throw new ArgumentException($"Unknown synergy type: {synergyType}", nameof(synergyType));
    }

    /// <inheritdoc />
    public IReadOnlyList<SynergyType> GetAvailableSynergies(ExplorationContext context)
    {
        var available = new List<SynergyType>();

        // FindHiddenPath available when navigating
        if (context.AllowsNavigation)
        {
            available.Add(SynergyType.FindHiddenPath);
        }

        // TrackToLair available when tracking
        if (context.HasActiveTracking)
        {
            available.Add(SynergyType.TrackToLair);
        }

        // AvoidPatrol available when patrols present
        if (context.HasPatrols)
        {
            available.Add(SynergyType.AvoidPatrol);
        }

        // FindAndLoot available when foraging
        if (context.AllowsForaging)
        {
            available.Add(SynergyType.FindAndLoot);
        }

        _logger.LogDebug(
            "Available synergies for location {LocationId}: {Synergies}",
            context.LocationId, string.Join(", ", available));

        return available;
    }

    /// <inheritdoc />
    public bool ShouldExecuteSecondary(SynergyType synergyType, SimpleCheckOutcome primaryResult)
    {
        var definition = GetSynergyDefinition(synergyType);

        return definition.SecondaryTiming switch
        {
            SecondaryCheckTiming.Always => true,
            SecondaryCheckTiming.OnPrimarySuccess => primaryResult.IsSuccess,
            SecondaryCheckTiming.OnPrimaryCritical => primaryResult.IsCriticalSuccess,
            _ => false
        };
    }

    /// <inheritdoc />
    public WastelandSurvivalCheckType GetPrimarySkill(SynergyType synergyType)
    {
        return GetSynergyDefinition(synergyType).PrimarySkill;
    }

    /// <inheritdoc />
    public string GetSecondarySkillId(SynergyType synergyType)
    {
        return GetSynergyDefinition(synergyType).SecondarySkillId;
    }

    /// <inheritdoc />
    public string GenerateNarrative(
        SynergyType synergyType,
        SimpleCheckOutcome primaryResult,
        SimpleCheckOutcome? secondaryResult)
    {
        return synergyType switch
        {
            SynergyType.FindHiddenPath =>
                GenerateFindPathNarrative(primaryResult, secondaryResult),

            SynergyType.TrackToLair =>
                GenerateTrackToLairNarrative(primaryResult, secondaryResult),

            SynergyType.AvoidPatrol =>
                GenerateAvoidPatrolNarrative(primaryResult, secondaryResult),

            SynergyType.FindAndLoot =>
                GenerateFindAndLootNarrative(primaryResult, secondaryResult),

            _ => "The exploration attempt concludes."
        };
    }

    private static string GenerateFindPathNarrative(
        SimpleCheckOutcome primary,
        SimpleCheckOutcome? secondary)
    {
        if (!primary.IsSuccess)
        {
            return primary.IsFumble
                ? "You become hopelessly lost searching for a hidden path."
                : "You search thoroughly but find no hidden passages.";
        }

        if (secondary == null)
        {
            return "You spot a hidden path but cannot reach it.";
        }

        if (secondary.Value.IsSuccess)
        {
            return "You discover a hidden path and traverse it successfully, " +
                   "reaching your destination unseen.";
        }

        return secondary.Value.IsFumble
            ? "You find the hidden path but slip during traversal, taking damage."
            : "You find the hidden path but cannot navigate the difficult terrain.";
    }

    private static string GenerateTrackToLairNarrative(
        SimpleCheckOutcome primary,
        SimpleCheckOutcome? secondary)
    {
        if (!primary.IsSuccess)
        {
            return primary.IsFumble
                ? "The trail leads you into a dead end—you've been misled."
                : "The trail goes cold. You'll need to find it again.";
        }

        if (secondary == null)
        {
            return "You track the creature to its lair but the entrance is sealed.";
        }

        if (secondary.Value.IsSuccess)
        {
            return "You follow the trail to the creature's lair and slip " +
                   "inside undetected.";
        }

        return secondary.Value.IsFumble
            ? "You find the lair but trigger its defenses while entering!"
            : "You find the lair but cannot bypass its entrance. The creature stirs within.";
    }

    private static string GenerateAvoidPatrolNarrative(
        SimpleCheckOutcome primary,
        SimpleCheckOutcome? secondary)
    {
        if (!primary.IsSuccess)
        {
            return primary.IsFumble
                ? "You walk directly into the patrol's path!"
                : "You don't notice the patrol until it's too late.";
        }

        if (secondary == null)
        {
            return "You spot the patrol but have nowhere to hide.";
        }

        if (secondary.Value.IsSuccess)
        {
            return "You spot the patrol and slip into the shadows, " +
                   "evading them completely.";
        }

        return secondary.Value.IsFumble
            ? "You spot the patrol but knock something over while hiding—they surround you!"
            : "You spot the patrol but they catch a glimpse of you—the chase is on!";
    }

    private static string GenerateFindAndLootNarrative(
        SimpleCheckOutcome primary,
        SimpleCheckOutcome? secondary)
    {
        if (!primary.IsSuccess)
        {
            return primary.IsFumble
                ? "Your search disturbs something dangerous."
                : "You find nothing of particular value.";
        }

        if (secondary == null)
        {
            return "You discover a locked cache but lack the tools to open it.";
        }

        if (secondary.Value.IsSuccess)
        {
            return "You discover a locked cache and deftly bypass its " +
                   "mechanism, claiming its contents.";
        }

        return secondary.Value.IsFumble
            ? "You find a cache but jam the lock permanently while attempting to open it."
            : "You find a cache but cannot bypass its lock. It remains sealed.";
    }
}
