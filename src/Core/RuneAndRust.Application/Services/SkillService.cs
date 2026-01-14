using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Definitions;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Application.Services;

/// <summary>
/// Service for managing player skills and skill checks.
/// </summary>
/// <remarks>
/// <para>
/// Handles skill system operations:
/// <list type="bullet">
///   <item><description>Skill checks with configurable dice pools</description></item>
///   <item><description>Experience-based skill progression</description></item>
///   <item><description>Proficiency bonus calculations</description></item>
///   <item><description>Usage tracking and statistics</description></item>
/// </list>
/// </para>
/// </remarks>
public class SkillService : ISkillService
{
    private readonly ILogger<SkillService> _logger;

    /// <summary>
    /// In-memory skill definitions for v0.4.3c.
    /// </summary>
    private readonly Dictionary<string, SkillDefinition> _skillDefinitions;

    /// <summary>
    /// Creates a new skill service.
    /// </summary>
    /// <param name="logger">Optional logger for diagnostics.</param>
    public SkillService(ILogger<SkillService>? logger = null)
    {
        _logger = logger ?? NullLogger<SkillService>.Instance;
        _skillDefinitions = InitializeDefaultSkills();
        _logger.LogDebug("SkillService initialized with {Count} skills", _skillDefinitions.Count);
    }

    /// <inheritdoc />
    public SkillCheckOutcome PerformSkillCheck(Player player, string skillId, int dc)
    {
        ArgumentNullException.ThrowIfNull(player);
        ArgumentException.ThrowIfNullOrWhiteSpace(skillId);

        var normalizedId = skillId.ToLowerInvariant();

        if (!_skillDefinitions.TryGetValue(normalizedId, out var definition))
        {
            return new SkillCheckOutcome(skillId, false, 0, 0, 0, dc, -dc, "Unknown skill.");
        }

        var playerSkill = player.GetSkill(normalizedId);

        // Check if untrained use is allowed
        if (playerSkill == null || playerSkill.Proficiency == SkillProficiency.Untrained)
        {
            if (!definition.AllowUntrained)
            {
                return new SkillCheckOutcome(skillId, false, 0, 0, 0, dc, -dc,
                    $"You need training in {definition.Name} to attempt this.");
            }
        }

        var bonus = GetSkillBonus(player, normalizedId);

        // Simple 2d6 roll simulation
        var random = new Random();
        var roll = random.Next(1, 7) + random.Next(1, 7);

        var total = roll + bonus;
        var success = total >= dc;
        var margin = total - dc;

        // Record usage
        playerSkill?.RecordUsage(success);

        _logger.LogInformation(
            "Skill check: {Skill} - Roll {Roll} + {Bonus} = {Total} vs DC {DC} = {Result}",
            definition.Name, roll, bonus, total, dc, success ? "Success" : "Failure");

        var message = success
            ? $"Success! ({total} vs DC {dc})"
            : $"Failed. ({total} vs DC {dc})";

        return new SkillCheckOutcome(normalizedId, success, roll, bonus, total, dc, margin, message);
    }

    /// <inheritdoc />
    public int GetSkillBonus(Player player, string skillId)
    {
        ArgumentNullException.ThrowIfNull(player);

        var normalizedId = skillId?.ToLowerInvariant() ?? string.Empty;

        if (!_skillDefinitions.TryGetValue(normalizedId, out var definition))
            return 0;

        var playerSkill = player.GetSkill(normalizedId);
        if (playerSkill == null)
            return 0;

        return definition.GetProficiencyBonus(playerSkill.Proficiency);
    }

    /// <inheritdoc />
    public SkillExperienceResult AwardSkillExperience(Player player, string skillId, int amount)
    {
        ArgumentNullException.ThrowIfNull(player);
        ArgumentException.ThrowIfNullOrWhiteSpace(skillId);

        var normalizedId = skillId.ToLowerInvariant();

        if (!_skillDefinitions.TryGetValue(normalizedId, out var definition))
        {
            return new SkillExperienceResult(skillId, 0, 0, false,
                SkillProficiency.Untrained, "Unknown skill.");
        }

        var playerSkill = player.GetSkill(normalizedId);
        if (playerSkill == null)
        {
            playerSkill = PlayerSkill.Create(normalizedId, player.Id);
            player.AddSkill(playerSkill);
        }

        var leveledUp = playerSkill.AddExperience(amount, definition);

        _logger.LogInformation(
            "Skill experience: {Skill} +{Exp} (Total: {Total}, Level: {Level})",
            definition.Name, amount, playerSkill.Experience, playerSkill.Proficiency);

        var message = leveledUp
            ? $"{definition.Name} improved to {playerSkill.Proficiency}!"
            : $"+{amount} {definition.Name} experience";

        return new SkillExperienceResult(
            normalizedId, amount, playerSkill.Experience,
            leveledUp, playerSkill.Proficiency, message);
    }

    /// <inheritdoc />
    public IEnumerable<PlayerSkillInfo> GetPlayerSkills(Player player)
    {
        ArgumentNullException.ThrowIfNull(player);

        foreach (var definition in _skillDefinitions.Values)
        {
            var playerSkill = player.GetSkill(definition.Id);
            var proficiency = playerSkill?.Proficiency ?? SkillProficiency.Untrained;
            var experience = playerSkill?.Experience ?? 0;
            var bonus = definition.GetProficiencyBonus(proficiency);

            var expToNext = GetExperienceToNextLevel(definition, experience);

            yield return new PlayerSkillInfo(
                definition.Id,
                definition.Name,
                proficiency,
                experience,
                expToNext,
                bonus,
                playerSkill?.TimesUsed ?? 0,
                playerSkill?.GetSuccessRate() ?? 0);
        }
    }

    /// <inheritdoc />
    public IEnumerable<SkillDefinition> GetAllSkillDefinitions() =>
        _skillDefinitions.Values;

    /// <inheritdoc />
    public void InitializePlayerSkills(Player player)
    {
        ArgumentNullException.ThrowIfNull(player);

        foreach (var definition in _skillDefinitions.Values)
        {
            if (!player.HasSkill(definition.Id))
            {
                var skill = PlayerSkill.Create(definition.Id, player.Id,
                    SkillProficiency.Untrained, 0);
                player.AddSkill(skill);

                _logger.LogDebug("Initialized skill {Skill} for player {Player}",
                    definition.Name, player.Name);
            }
        }
    }

    /// <summary>
    /// Gets experience needed for the next proficiency level.
    /// </summary>
    private static int GetExperienceToNextLevel(SkillDefinition definition, int currentExp)
    {
        var thresholds = definition.ExperienceThresholds.Values.OrderBy(v => v).ToList();
        foreach (var threshold in thresholds)
        {
            if (currentExp < threshold)
                return threshold - currentExp;
        }
        return 0; // Already at max
    }

    /// <summary>
    /// Initializes the default skill definitions.
    /// </summary>
    private static Dictionary<string, SkillDefinition> InitializeDefaultSkills()
    {
        var skills = new[]
        {
            SkillDefinition.Create("perception", "Perception",
                "Noticing details, spotting hidden objects, detecting traps.",
                "wits", category: "Mental"),
            SkillDefinition.Create("stealth", "Stealth",
                "Moving silently, hiding in shadows, avoiding detection.",
                "finesse", category: "Physical"),
            SkillDefinition.Create("lockpicking", "Lockpicking",
                "Opening locks without the proper key.",
                "finesse", category: "Technical", allowUntrained: false),
            SkillDefinition.Create("acrobatics", "Acrobatics",
                "Jumping, climbing, balancing, and tumbling.",
                "finesse", category: "Physical"),
            SkillDefinition.Create("field-medicine", "Field Medicine",
                "Treating wounds, curing ailments, and stabilizing the injured.",
                "wits", category: "Mental"),
            SkillDefinition.Create("rhetoric", "Rhetoric",
                "Persuasion, intimidation, and social manipulation.",
                "will", category: "Social"),
            SkillDefinition.Create("wilderness-survival", "Wilderness Survival",
                "Foraging, tracking, and navigating wild terrain.",
                "wits", category: "Physical")
        };

        return skills.ToDictionary(s => s.Id, StringComparer.OrdinalIgnoreCase);
    }
}
