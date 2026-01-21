// ═══════════════════════════════════════════════════════════════════════════════
// AchievementsCommand.cs
// Command handler for opening the achievement panel.
// Version: 0.13.4a
// ═══════════════════════════════════════════════════════════════════════════════

using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Presentation.Tui.DTOs;
using RuneAndRust.Presentation.Tui.UI;

namespace RuneAndRust.Presentation.Tui.Commands;

/// <summary>
/// Command handler for opening the achievement panel.
/// </summary>
/// <remarks>
/// <para>
/// This command handles the "achievements" input (and aliases "achieve", "ach")
/// to retrieve and display all achievements for the current player.
/// </para>
/// <para>
/// The command:
/// </para>
/// <list type="bullet">
///   <item><description>Retrieves achievement definitions from <see cref="IAchievementService"/></description></item>
///   <item><description>Gets player progress for each achievement</description></item>
///   <item><description>Maps data to display DTOs</description></item>
///   <item><description>Renders the achievement panel</description></item>
/// </list>
/// </remarks>
public class AchievementsCommand
{
    private readonly IAchievementService _achievementService;
    private readonly AchievementPanel _achievementPanel;
    private readonly ILogger<AchievementsCommand> _logger;

    /// <summary>
    /// Creates a new instance of the AchievementsCommand.
    /// </summary>
    /// <param name="achievementService">The achievement service for data retrieval.</param>
    /// <param name="achievementPanel">The achievement panel for rendering.</param>
    /// <param name="logger">Logger for diagnostic output.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when required parameters are null.
    /// </exception>
    public AchievementsCommand(
        IAchievementService achievementService,
        AchievementPanel achievementPanel,
        ILogger<AchievementsCommand>? logger = null)
    {
        _achievementService = achievementService ?? throw new ArgumentNullException(nameof(achievementService));
        _achievementPanel = achievementPanel ?? throw new ArgumentNullException(nameof(achievementPanel));
        _logger = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<AchievementsCommand>.Instance;
    }

    /// <summary>
    /// Executes the achievements command for the specified player.
    /// </summary>
    /// <param name="player">The player whose achievements to display.</param>
    /// <remarks>
    /// This method uses synchronous calls to <see cref="IAchievementService"/>
    /// as the service API is synchronous.
    /// </remarks>
    public void Execute(Player player)
    {
        ArgumentNullException.ThrowIfNull(player);

        _logger.LogDebug("Executing achievements command for player {PlayerName}", player.Name);

        // Get achievement progress from the service
        var progress = _achievementService.GetProgress(player);
        var totalPoints = _achievementService.GetTotalPoints(player);
        var unlockedCount = _achievementService.GetUnlockedCount(player);
        var totalCount = progress.Count;

        // Map to display DTOs
        var displayDtos = MapToDisplayDtos(progress);

        // Set panel position and render
        _achievementPanel.SetPosition(2, 2);
        _achievementPanel.RenderAchievements(displayDtos, totalPoints, unlockedCount, totalCount);

        _logger.LogInformation(
            "Achievement panel opened: {UnlockedCount}/{TotalCount} achievements, {TotalPoints} points",
            unlockedCount, totalCount, totalPoints);
    }

    /// <summary>
    /// Maps achievement progress records to display DTOs.
    /// </summary>
    /// <param name="progress">The achievement progress records.</param>
    /// <returns>A list of display DTOs for the UI.</returns>
    private static IReadOnlyList<AchievementDisplayDto> MapToDisplayDtos(
        IReadOnlyList<Application.Models.AchievementProgress> progress)
    {
        return progress.Select(p => new AchievementDisplayDto(
            Id: p.Definition.AchievementId,
            Name: p.DisplayName,
            Description: p.DisplayDescription,
            Category: p.Definition.Category,
            Tier: p.Definition.Tier,
            TargetValue: GetTargetValue(p),
            CurrentValue: GetCurrentValue(p),
            IsUnlocked: p.IsUnlocked,
            IsSecret: p.Definition.IsSecret,
            PointValue: (int)p.Definition.Tier, // Tier enum value is the point value
            UnlockedAt: null // Not tracked in current model
        )).ToList();
    }

    /// <summary>
    /// Gets the target value from progress (first condition target or 1).
    /// </summary>
    private static int GetTargetValue(Application.Models.AchievementProgress progress)
    {
        // Use first condition's target, or 1 for single-condition achievements
        var firstCondition = progress.ConditionProgress.FirstOrDefault();
        return firstCondition != null ? (int)firstCondition.TargetValue : 1;
    }

    /// <summary>
    /// Gets the current value from progress (first condition current or unlock status).
    /// </summary>
    private static int GetCurrentValue(Application.Models.AchievementProgress progress)
    {
        if (progress.IsUnlocked)
        {
            return GetTargetValue(progress); // Full completion
        }

        // Use first condition's current value
        var firstCondition = progress.ConditionProgress.FirstOrDefault();
        return firstCondition != null ? (int)firstCondition.CurrentValue : 0;
    }
}
