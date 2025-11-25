using System.Text;
using RuneAndRust.Core;
using Serilog;

namespace RuneAndRust.Engine.Commands;

/// <summary>
/// v0.37.4: Saga Command
/// Displays character progression menu (attributes, skills, specializations).
/// Syntax: saga
/// </summary>
public class SagaCommand : ICommand
{
    private static readonly ILogger _log = Log.ForContext<SagaCommand>();

    public CommandResult Execute(GameState state, string[] args)
    {
        if (state.Player == null)
        {
            _log.Warning("Saga command failed: Player is null");
            return CommandResult.CreateFailure("Player not found.");
        }

        _log.Information(
            "Saga command: CharacterId={CharacterId}",
            state.Player.CharacterID);

        var output = new StringBuilder();

        // Header
        output.AppendLine("╔════════════════════════════════════════╗");
        output.AppendLine("║ Character Progression                  ║");
        output.AppendLine("╠════════════════════════════════════════╣");

        // Character Info
        string nameLine = $"Name: {state.Player.Name}";
        if (nameLine.Length > 38)
            nameLine = nameLine.Substring(0, 38);

        output.AppendLine($"║ {nameLine.PadRight(38)} ║");

        // Milestone (level)
        string levelLine = $"Milestone: {state.Player.CurrentMilestone}";
        output.AppendLine($"║ {levelLine.PadRight(38)} ║");

        // Legend progress
        string legendLine = $"Legend: {state.Player.CurrentLegend}/{state.Player.LegendToNextMilestone}";
        output.AppendLine($"║ {legendLine.PadRight(38)} ║");

        output.AppendLine("║                                        ║");

        // Progression Points
        string ppLine = $"Available Points: {state.Player.ProgressionPoints} PP";
        output.AppendLine($"║ {ppLine.PadRight(38)} ║");

        output.AppendLine("║                                        ║");

        // Attributes
        output.AppendLine("║ ATTRIBUTES:                            ║");

        var attributes = new[]
        {
            ("MIGHT", state.Player.Attributes.Might),
            ("FINESSE", state.Player.Attributes.Finesse),
            ("STURDINESS", state.Player.Attributes.Sturdiness),
            ("WITS", state.Player.Attributes.Wits),
            ("WILL", state.Player.Attributes.Will)
        };

        foreach (var (name, value) in attributes)
        {
            string attrLine = $" {name,-12}: {value,2}  [+] (Cost: 1 PP)";
            output.AppendLine($"║ {attrLine.PadRight(38)} ║");
        }

        output.AppendLine("║                                        ║");

        // Skills reference
        output.AppendLine("║ SKILLS: (use 'skills' command)         ║");

        output.AppendLine("║                                        ║");

        // Specialization
        string specializationLine = GetSpecializationDisplay(state.Player);
        output.AppendLine($"║ {specializationLine.PadRight(38)} ║");

        output.AppendLine("╚════════════════════════════════════════╝");
        output.AppendLine();
        output.AppendLine("Commands: spend [attribute], skills");

        _log.Information(
            "Saga displayed: Milestone={Milestone}, PP={PP}, Legend={Legend}",
            state.Player.CurrentMilestone,
            state.Player.ProgressionPoints,
            state.Player.CurrentLegend);

        return CommandResult.CreateSuccess(output.ToString());
    }

    /// <summary>
    /// Get specialization display string
    /// </summary>
    private string GetSpecializationDisplay(PlayerCharacter player)
    {
        if (player.Specialization == Specialization.None)
        {
            return "SPECIALIZATIONS: None";
        }

        // Format specialization name
        string specName = player.Specialization.ToString();

        // Add archetype if available
        if (player.Archetype != null)
        {
            string archetypeName = player.Archetype.ArchetypeType.ToString();
            return $"SPECIALIZATIONS: {archetypeName}";
        }

        return $"SPECIALIZATIONS: {specName}";
    }
}
