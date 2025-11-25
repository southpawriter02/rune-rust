using System.Text;
using RuneAndRust.Core;
using RuneAndRust.Persistence;
using Serilog;

namespace RuneAndRust.Engine.Commands;

/// <summary>
/// v0.37.4: Talk Command
/// v0.38.11: Integrated with NPCFlavorTextService for dynamic greetings
/// Initiates dialogue with NPCs in the current room.
/// Syntax: talk [to] [npc_name] (aliases: speak)
/// </summary>
public class TalkCommand : ICommand
{
    private static readonly ILogger _log = Log.ForContext<TalkCommand>();
    private readonly NPCFlavorTextService? _npcFlavorService;

    public TalkCommand(DescriptorRepository? repository = null)
    {
        if (repository != null)
        {
            _npcFlavorService = new NPCFlavorTextService(repository);
        }
    }

    public CommandResult Execute(GameState state, string[] args)
    {
        if (state.Player == null)
        {
            _log.Warning("Talk command failed: Player is null");
            return CommandResult.CreateFailure("Player not found.");
        }

        if (state.CurrentRoom == null)
        {
            _log.Warning("Talk command failed: CurrentRoom is null");
            return CommandResult.CreateFailure("You are not in a room.");
        }

        // Check if arguments provided
        if (args.Length == 0)
        {
            // Show available NPCs
            if (state.CurrentRoom.NPCs.Any())
            {
                var npcNames = string.Join(", ", state.CurrentRoom.NPCs.Select(npc => npc.Name));
                return CommandResult.CreateFailure($"Talk to whom? Available NPCs: {npcNames}");
            }

            return CommandResult.CreateFailure("There is no one here to talk to.");
        }

        // Parse NPC name (skip "to" if present)
        string npcName;
        if (args[0].Equals("to", StringComparison.OrdinalIgnoreCase) && args.Length > 1)
        {
            npcName = string.Join(" ", args.Skip(1));
        }
        else
        {
            npcName = string.Join(" ", args);
        }

        _log.Information(
            "Talk command: CharacterId={CharacterId}, NPC={NPCName}, RoomId={RoomId}",
            state.Player.CharacterID,
            npcName,
            state.CurrentRoom.RoomId);

        // Find NPC in current room
        var npc = state.CurrentRoom.NPCs.FirstOrDefault(n =>
            n.Name.Contains(npcName, StringComparison.OrdinalIgnoreCase));

        if (npc == null)
        {
            _log.Warning(
                "Talk failed: NPC not found: NPC={NPCName}, RoomId={RoomId}",
                npcName,
                state.CurrentRoom.RoomId);

            // Show available NPCs
            if (state.CurrentRoom.NPCs.Any())
            {
                var npcNames = string.Join(", ", state.CurrentRoom.NPCs.Select(n => n.Name));
                return CommandResult.CreateFailure($"'{npcName}' is not here. Available NPCs: {npcNames}");
            }

            return CommandResult.CreateFailure($"'{npcName}' is not here.");
        }

        // Check if NPC is hostile
        if (npc.IsHostile)
        {
            _log.Warning(
                "Talk failed: NPC is hostile: NPC={NPCName}",
                npc.Name);

            return CommandResult.CreateFailure($"{npc.Name} is hostile and will not talk to you!");
        }

        // Mark NPC as met
        if (!npc.HasBeenMet)
        {
            npc.HasBeenMet = true;
        }

        // Build dialogue output
        var output = new StringBuilder();

        // NPC greeting
        output.AppendLine($"{npc.Name} looks at you.");
        output.AppendLine();

        // v0.38.11: Generate dynamic greeting based on disposition and archetype
        string greeting;
        if (_npcFlavorService != null && !string.IsNullOrEmpty(npc.Archetype))
        {
            try
            {
                var dispositionTier = npc.GetDispositionTier();
                greeting = _npcFlavorService.GeneratePlayerApproachReaction(
                    npc.Archetype,
                    npc.Subtype,
                    dispositionTier);

                _log.Debug("Generated dynamic greeting for {NpcName}: Archetype={Archetype}, Subtype={Subtype}, Disposition={Disposition}",
                    npc.Name, npc.Archetype, npc.Subtype, dispositionTier);
            }
            catch (Exception ex)
            {
                _log.Warning(ex, "Failed to generate dynamic greeting for {NpcName}, using fallback", npc.Name);
                greeting = !string.IsNullOrEmpty(npc.InitialGreeting) ? npc.InitialGreeting : "Greetings, traveler.";
            }
        }
        else
        {
            // Fallback to static greeting
            greeting = !string.IsNullOrEmpty(npc.InitialGreeting) ? npc.InitialGreeting : "Greetings, traveler.";
        }

        output.AppendLine($"{npc.Name}: \"{greeting}\"");
        output.AppendLine();

        // Dialogue system integration (placeholder)
        // In a full implementation, this would:
        // 1. Check npc.RootDialogueId
        // 2. Load dialogue tree from DialogueService
        // 3. Display dialogue options
        // 4. Track dialogue state

        if (!string.IsNullOrEmpty(npc.RootDialogueId))
        {
            output.AppendLine("[Dialogue Options]");
            output.AppendLine("[1] \"What can you tell me about this place?\"");
            output.AppendLine("[2] \"Do you have any work for me?\"");
            output.AppendLine("[3] (Leave)");
            output.AppendLine();
            output.AppendLine("Note: Full dialogue system integration pending.");
        }
        else
        {
            output.AppendLine($"{npc.Name} has nothing more to say right now.");
        }

        _log.Information(
            "Talk initiated: NPC={NPCName}, HasDialogue={HasDialogue}",
            npc.Name,
            !string.IsNullOrEmpty(npc.RootDialogueId));

        return CommandResult.CreateSuccess(output.ToString());
    }
}
