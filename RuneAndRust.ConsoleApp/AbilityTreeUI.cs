using Spectre.Console;
using RuneAndRust.Core;
using RuneAndRust.Engine;

namespace RuneAndRust.ConsoleApp;

/// <summary>
/// v0.19: UI for ability tree browsing and learning
/// Provides visual interface for ability progression
/// </summary>
public class AbilityTreeUI
{
    private readonly AbilityService _abilityService;
    private readonly SpecializationService _specializationService;

    public AbilityTreeUI(string connectionString)
    {
        _abilityService = new AbilityService(connectionString);
        _specializationService = new SpecializationService(connectionString);
    }

    /// <summary>
    /// Display ability tree browser for a specialization
    /// Allows learning abilities and ranking them up
    /// </summary>
    public void ShowAbilityTree(PlayerCharacter character, int specializationId)
    {
        while (true)
        {
            AnsiConsole.Clear();
            AnsiConsole.WriteLine();

            // Get specialization info
            var specResult = _specializationService.GetSpecialization(specializationId);
            if (!specResult.Success || specResult.Specialization == null)
            {
                AnsiConsole.MarkupLine("[red]Specialization not found.[/]");
                AnsiConsole.MarkupLine("[dim]Press any key to return...[/]");
                Console.ReadKey();
                return;
            }

            var spec = specResult.Specialization;

            // Header
            var rule = new Rule($"[bold yellow]{spec.IconEmoji} {spec.Name.ToUpper()} - ABILITY TREE[/]")
            {
                Justification = Justify.Center
            };
            AnsiConsole.Write(rule);
            AnsiConsole.WriteLine();

            // Display character resources
            var ppSpent = _specializationService.GetPPSpentInTree(character, specializationId);
            AnsiConsole.MarkupLine($"[dim]Progression Points Available:[/] [bold green]{character.ProgressionPoints} PP[/]");
            AnsiConsole.MarkupLine($"[dim]PP Spent in this tree:[/] [bold cyan]{ppSpent} PP[/]");
            AnsiConsole.WriteLine();

            // Get all abilities for this specialization
            var abilitiesResult = _abilityService.GetAbilitiesForSpecialization(specializationId);
            if (!abilitiesResult.Success || abilitiesResult.Abilities == null)
            {
                AnsiConsole.MarkupLine("[red]Failed to load abilities.[/]");
                AnsiConsole.MarkupLine("[dim]Press any key to return...[/]");
                Console.ReadKey();
                return;
            }

            var abilities = abilitiesResult.Abilities;

            // Display abilities organized by tier
            DisplayAbilityTreeTable(character, abilities, ppSpent);

            // Build selection choices
            var choices = new List<string>();

            foreach (var ability in abilities.OrderBy(a => a.TierLevel).ThenBy(a => a.AbilityID))
            {
                bool isLearned = _abilityService.HasLearned(character, ability.AbilityID);
                var canLearnResult = _abilityService.CanLearn(character, ability.AbilityID);

                string statusTag;
                if (isLearned)
                {
                    var currentRank = _abilityService.GetCurrentRank(character, ability.AbilityID);
                    statusTag = $"[green]✓ Rank {currentRank}[/]";
                }
                else if (canLearnResult.Success)
                {
                    statusTag = $"[yellow]○ {ability.PPCost} PP[/]";
                }
                else
                {
                    statusTag = "[red]✗ Locked[/]";
                }

                var typeIcon = ability.AbilityType switch
                {
                    "Active" => "⚔",
                    "Passive" => "◈",
                    "Reaction" => "⚡",
                    _ => "•"
                };

                choices.Add($"{typeIcon} {ability.Name} {statusTag}");
            }

            choices.Add("← Back");

            // Prompt for selection
            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[yellow]Select an ability to view details:[/]")
                    .PageSize(15)
                    .AddChoices(choices)
            );

            // Handle back
            if (choice == "← Back")
            {
                return;
            }

            // Extract ability name (remove icons and status tags)
            var abilityName = ExtractAbilityName(choice);
            var selectedAbility = abilities.FirstOrDefault(a => a.Name == abilityName);

            if (selectedAbility != null)
            {
                ShowAbilityDetails(character, selectedAbility, ppSpent);
            }
        }
    }

    /// <summary>
    /// Display ability tree in table format
    /// </summary>
    private void DisplayAbilityTreeTable(PlayerCharacter character, List<AbilityData> abilities, int ppSpent)
    {
        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Cyan1);

        table.AddColumn(new TableColumn("[bold]Tier[/]").Centered());
        table.AddColumn(new TableColumn("[bold]Ability[/]"));
        table.AddColumn(new TableColumn("[bold]Status[/]").Centered());
        table.AddColumn(new TableColumn("[bold]Cost[/]").Centered());

        // Group by tier
        foreach (var tierLevel in new[] { 1, 2, 3, 4 })
        {
            var tierAbilities = abilities.Where(a => a.TierLevel == tierLevel).OrderBy(a => a.AbilityID).ToList();
            if (tierAbilities.Count == 0) continue;

            var tierName = tierLevel == 4 ? "Capstone" : $"Tier {tierLevel}";
            var tierRequirement = tierLevel switch
            {
                1 => "Entry",
                2 => "8 PP",
                3 => "16 PP",
                4 => "24 PP",
                _ => ""
            };

            bool firstInTier = true;
            foreach (var ability in tierAbilities)
            {
                bool isLearned = _abilityService.HasLearned(character, ability.AbilityID);
                var canLearnResult = _abilityService.CanLearn(character, ability.AbilityID);

                // Type icon
                var typeIcon = ability.AbilityType switch
                {
                    "Active" => "⚔",
                    "Passive" => "◈",
                    "Reaction" => "⚡",
                    _ => "•"
                };

                // Ability name with summary
                var abilityText = $"{typeIcon} [yellow]{ability.Name}[/]\n[dim]{ability.MechanicalSummary}[/]";

                // Status
                string statusText;
                Color statusColor;
                if (isLearned)
                {
                    var currentRank = _abilityService.GetCurrentRank(character, ability.AbilityID);
                    statusText = $"✓ Rank {currentRank}";
                    statusColor = Color.Green;
                }
                else if (canLearnResult.Success)
                {
                    statusText = "Available";
                    statusColor = Color.Yellow;
                }
                else
                {
                    statusText = "Locked";
                    statusColor = Color.Red;
                }

                // Cost
                var costText = isLearned ? "[dim]Learned[/]" : $"[yellow]{ability.PPCost} PP[/]";

                // Tier name (only for first ability in tier)
                var tierCell = firstInTier ? $"[bold cyan]{tierName}[/]\n[dim]{tierRequirement}[/]" : "";

                table.AddRow(
                    tierCell,
                    abilityText,
                    $"[{statusColor}]{statusText}[/]",
                    costText
                );

                firstInTier = false;
            }
        }

        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();
    }

    /// <summary>
    /// Show detailed view of an ability with learn/rank-up options
    /// </summary>
    private void ShowAbilityDetails(PlayerCharacter character, AbilityData ability, int ppSpent)
    {
        AnsiConsole.Clear();
        AnsiConsole.WriteLine();

        // Header
        var typeIcon = ability.AbilityType switch
        {
            "Active" => "⚔",
            "Passive" => "◈",
            "Reaction" => "⚡",
            _ => "•"
        };

        var figlet = new FigletText(ability.Name)
            .Centered()
            .Color(Color.Yellow);
        AnsiConsole.Write(figlet);
        AnsiConsole.WriteLine();

        // Ability details panel
        var detailsMarkup = $"[bold]{ability.Description}[/]\n\n" +
                           $"[bold]Type:[/] {typeIcon} {ability.AbilityType} ({ability.ActionType})\n" +
                           $"[bold]Target:[/] {ability.TargetType}\n" +
                           $"[bold]Tier:[/] {ability.TierLevel}\n\n";

        // Resource costs
        if (ability.ResourceCost.Stamina > 0)
            detailsMarkup += $"[bold]Stamina Cost:[/] {ability.ResourceCost.Stamina}\n";
        if (ability.ResourceCost.Stress > 0)
            detailsMarkup += $"[bold]Stress Cost:[/] {ability.ResourceCost.Stress}\n";
        if (ability.ResourceCost.HP > 0)
            detailsMarkup += $"[bold]HP Cost:[/] {ability.ResourceCost.HP}\n";

        // Mechanical details
        if (ability.AttributeUsed != string.Empty)
        {
            detailsMarkup += $"\n[bold]Roll:[/] {ability.AttributeUsed.ToUpper()} + {ability.BonusDice}d vs {ability.SuccessThreshold} successes\n";
        }

        if (ability.DamageDice > 0)
            detailsMarkup += $"[bold]Damage:[/] {ability.DamageDice}d6{(ability.IgnoresArmor ? " (ignores armor)" : "")}\n";
        if (ability.HealingDice > 0)
            detailsMarkup += $"[bold]Healing:[/] {ability.HealingDice}d6\n";

        // Status effects
        if (ability.StatusEffectsApplied.Count > 0)
        {
            detailsMarkup += $"[bold]Applies:[/] {string.Join(", ", ability.StatusEffectsApplied.Select(s => $"[{s}]"))}\n";
        }
        if (ability.StatusEffectsRemoved.Count > 0)
        {
            detailsMarkup += $"[bold]Removes:[/] {string.Join(", ", ability.StatusEffectsRemoved.Select(s => $"[{s}]"))}\n";
        }

        // Cooldown
        if (ability.CooldownType != "None")
        {
            detailsMarkup += $"\n[bold]Cooldown:[/] {ability.CooldownType}\n";
        }

        var detailsPanel = new Panel(detailsMarkup)
        {
            Border = BoxBorder.Rounded,
            Header = new PanelHeader("[bold]ABILITY DETAILS[/]"),
            Padding = new Padding(2, 1)
        };
        detailsPanel.BorderColor(Color.Yellow);
        AnsiConsole.Write(detailsPanel);
        AnsiConsole.WriteLine();

        // Check learn/rank status
        bool isLearned = _abilityService.HasLearned(character, ability.AbilityID);

        if (isLearned)
        {
            HandleLearnedAbility(character, ability);
        }
        else
        {
            HandleUnlearnedAbility(character, ability, ppSpent);
        }
    }

    /// <summary>
    /// Handle interactions for a learned ability (rank up)
    /// </summary>
    private void HandleLearnedAbility(PlayerCharacter character, AbilityData ability)
    {
        var currentRank = _abilityService.GetCurrentRank(character, ability.AbilityID);

        AnsiConsole.MarkupLine($"[green]✓ Ability learned (Rank {currentRank}/{ability.MaxRank})[/]");

        // Check if can rank up
        if (currentRank < ability.MaxRank)
        {
            int rankUpCost = currentRank == 1 ? ability.CostToRank2 : ability.CostToRank3;

            if (rankUpCost > 0)
            {
                AnsiConsole.MarkupLine($"[yellow]Rank {currentRank + 1} available for {rankUpCost} PP[/] [dim](you have {character.ProgressionPoints} PP)[/]");
                AnsiConsole.WriteLine();

                if (character.ProgressionPoints >= rankUpCost)
                {
                    if (AnsiConsole.Confirm($"[yellow]Rank up to Rank {currentRank + 1} for {rankUpCost} PP?[/]"))
                    {
                        var result = _abilityService.RankUpAbility(character, ability.AbilityID);

                        if (result.Success)
                        {
                            AnsiConsole.MarkupLine($"[green]✓ {result.Message}[/]");
                        }
                        else
                        {
                            AnsiConsole.MarkupLine($"[red]✗ {result.Message}[/]");
                        }

                        AnsiConsole.WriteLine();
                        AnsiConsole.MarkupLine("[dim]Press any key to continue...[/]");
                        Console.ReadKey();
                    }
                }
                else
                {
                    AnsiConsole.MarkupLine($"[red]Insufficient PP (need {rankUpCost}, have {character.ProgressionPoints})[/]");
                    AnsiConsole.WriteLine();
                    AnsiConsole.MarkupLine("[dim]Press any key to return...[/]");
                    Console.ReadKey();
                }
            }
            else
            {
                AnsiConsole.MarkupLine($"[dim]Rank {currentRank + 1} not yet available[/]");
                AnsiConsole.WriteLine();
                AnsiConsole.MarkupLine("[dim]Press any key to return...[/]");
                Console.ReadKey();
            }
        }
        else
        {
            AnsiConsole.MarkupLine($"[cyan]Maximum rank reached![/]");
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[dim]Press any key to return...[/]");
            Console.ReadKey();
        }
    }

    /// <summary>
    /// Handle interactions for an unlearned ability (learn)
    /// </summary>
    private void HandleUnlearnedAbility(PlayerCharacter character, AbilityData ability, int ppSpent)
    {
        var canLearnResult = _abilityService.CanLearn(character, ability.AbilityID);

        if (canLearnResult.Success)
        {
            AnsiConsole.MarkupLine($"[yellow]Learn Cost:[/] [bold]{ability.PPCost} PP[/] [dim](you have {character.ProgressionPoints} PP)[/]");
            AnsiConsole.WriteLine();

            if (AnsiConsole.Confirm($"[yellow]Learn {ability.Name} for {ability.PPCost} PP?[/]"))
            {
                var result = _abilityService.LearnAbility(character, ability.AbilityID);

                if (result.Success)
                {
                    AnsiConsole.MarkupLine($"[green]✓ {result.Message}[/]");
                }
                else
                {
                    AnsiConsole.MarkupLine($"[red]✗ {result.Message}[/]");
                }

                AnsiConsole.WriteLine();
                AnsiConsole.MarkupLine("[dim]Press any key to continue...[/]");
                Console.ReadKey();
            }
        }
        else
        {
            AnsiConsole.MarkupLine($"[red]Cannot learn:[/] {canLearnResult.Message}");

            // Show prerequisite details if available
            if (ability.Prerequisites.RequiredPPInTree > 0)
            {
                AnsiConsole.MarkupLine($"[dim]Requires {ability.Prerequisites.RequiredPPInTree} PP in tree (you have {ppSpent})[/]");
            }

            if (ability.Prerequisites.RequiredAbilityIDs.Count > 0)
            {
                AnsiConsole.MarkupLine($"[dim]Requires {ability.Prerequisites.RequiredAbilityIDs.Count} prerequisite abilities[/]");
            }

            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[dim]Press any key to return...[/]");
            Console.ReadKey();
        }
    }

    /// <summary>
    /// Extract ability name from formatted choice string
    /// </summary>
    private string ExtractAbilityName(string choice)
    {
        // Remove status tags
        var cleaned = choice
            .Replace("[green]✓ Rank 1[/]", "")
            .Replace("[green]✓ Rank 2[/]", "")
            .Replace("[green]✓ Rank 3[/]", "")
            .Replace("[red]✗ Locked[/]", "");

        // Remove PP cost tags
        var ppIndex = cleaned.IndexOf("[yellow]○");
        if (ppIndex >= 0)
        {
            cleaned = cleaned.Substring(0, ppIndex);
        }

        // Remove type icons (they're at the start)
        if (cleaned.Length > 2 && (cleaned[0] == '⚔' || cleaned[0] == '◈' || cleaned[0] == '⚡' || cleaned[0] == '•'))
        {
            cleaned = cleaned.Substring(2);
        }

        return cleaned.Trim();
    }
}
