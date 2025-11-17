using System.Text;
using RuneAndRust.Core;
using RuneAndRust.Core.Quests;
using Serilog;

namespace RuneAndRust.Engine.Commands;

/// <summary>
/// v0.37.4: Journal Command
/// Displays Scavenger's Journal with active quests and lore.
/// Syntax: journal (aliases: j)
/// </summary>
public class JournalCommand : ICommand
{
    private static readonly ILogger _log = Log.ForContext<JournalCommand>();

    public CommandResult Execute(GameState state, string[] args)
    {
        if (state.Player == null)
        {
            _log.Warning("Journal command failed: Player is null");
            return CommandResult.Failure("Player not found.");
        }

        _log.Information(
            "Journal command: CharacterId={CharacterId}",
            state.Player.CharacterID);

        var output = new StringBuilder();

        // Header
        output.AppendLine("╔════════════════════════════════════════╗");
        output.AppendLine("║ Scavenger's Journal                    ║");
        output.AppendLine("╠════════════════════════════════════════╣");

        // Active Quests
        var activeQuests = state.Player.ActiveQuests
            .Where(q => q.Status == QuestStatus.Active || q.Status == QuestStatus.Complete)
            .OrderBy(q => q.Type)  // Main quests first
            .ThenBy(q => q.Title)
            .ToList();

        if (activeQuests.Any())
        {
            output.AppendLine("║ ACTIVE QUESTS:                         ║");
            output.AppendLine("║                                        ║");

            foreach (var quest in activeQuests)
            {
                // Quest type tag
                string typeTag = quest.Type switch
                {
                    QuestType.Main => "[Main Quest]",
                    QuestType.Side => "[Side Quest]",
                    QuestType.Dynamic => "[Dynamic]",
                    QuestType.Repeatable => "[Repeatable]",
                    _ => ""
                };

                // Title line
                string titleLine = $"{typeTag} {quest.Title}";
                output.AppendLine($"║ {titleLine.PadRight(38)} ║");

                // Show first active objective
                var activeObjective = quest.Objectives.FirstOrDefault(o => !o.IsComplete);
                if (activeObjective != null)
                {
                    string objectiveLine = $" → {activeObjective.Description}";
                    if (objectiveLine.Length > 38)
                        objectiveLine = objectiveLine.Substring(0, 35) + "...";

                    output.AppendLine($"║ {objectiveLine.PadRight(38)} ║");
                }

                // Show progress for objectives with counters
                var objectiveWithProgress = quest.Objectives.FirstOrDefault(o => o.TargetCount > 1);
                if (objectiveWithProgress != null)
                {
                    string progressLine = $" Progress: {objectiveWithProgress.CurrentCount}/{objectiveWithProgress.TargetCount}";
                    output.AppendLine($"║ {progressLine.PadRight(38)} ║");
                }

                output.AppendLine("║                                        ║");
            }
        }
        else
        {
            output.AppendLine("║ ACTIVE QUESTS: None                    ║");
            output.AppendLine("║                                        ║");
        }

        // Completed Quests Count
        int completedCount = state.Player.CompletedQuests.Count;
        string completedLine = $"COMPLETED QUESTS: {completedCount}";
        output.AppendLine($"║ {completedLine.PadRight(38)} ║");

        output.AppendLine("║                                        ║");

        // Lore Entries (placeholder - not yet implemented in player)
        // For now, show a static message
        output.AppendLine("║ DISCOVERED LORE: (feature pending)     ║");

        output.AppendLine("╚════════════════════════════════════════╝");

        _log.Information(
            "Journal displayed: ActiveQuests={ActiveCount}, CompletedQuests={CompletedCount}",
            activeQuests.Count,
            completedCount);

        return CommandResult.Success(output.ToString());
    }
}
