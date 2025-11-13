using Spectre.Console;
using RuneAndRust.Core;
using RuneAndRust.Engine;

namespace RuneAndRust.ConsoleApp;

/// <summary>
/// v0.19: UI for specialization browsing and unlocking
/// Provides visual interface for data-driven specialization system
/// </summary>
public class SpecializationUI
{
    private readonly SpecializationService _specializationService;
    private readonly AbilityService _abilityService;

    public SpecializationUI(string connectionString)
    {
        _specializationService = new SpecializationService(connectionString);
        _abilityService = new AbilityService(connectionString);
    }

    /// <summary>
    /// Display specialization browser and handle user interaction
    /// Returns true if specialization was unlocked, false if cancelled
    /// </summary>
    public bool ShowSpecializationBrowser(PlayerCharacter character)
    {
        while (true)
        {
            AnsiConsole.Clear();
            AnsiConsole.WriteLine();

            // Get available specializations for character's archetype
            var result = _specializationService.GetAvailableSpecializations(character);

            if (!result.Success || result.Specializations == null || result.Specializations.Count == 0)
            {
                AnsiConsole.MarkupLine("[red]No specializations available for your archetype.[/]");
                AnsiConsole.MarkupLine("[dim]Press any key to return...[/]");
                Console.ReadKey();
                return false;
            }

            // Display header
            var archetypeName = character.Class.ToString().ToUpper();
            var rule = new Rule($"[bold yellow]{archetypeName} SPECIALIZATIONS[/]")
            {
                Justification = Justify.Center
            };
            AnsiConsole.Write(rule);
            AnsiConsole.WriteLine();

            // Display current PP
            AnsiConsole.MarkupLine($"[dim]Progression Points Available:[/] [bold green]{character.ProgressionPoints} PP[/]");
            AnsiConsole.WriteLine();

            // Display specializations table
            var table = new Table()
                .Border(TableBorder.Rounded)
                .BorderColor(Color.Yellow);

            table.AddColumn(new TableColumn("[bold]Status[/]").Centered());
            table.AddColumn(new TableColumn("[bold]Specialization[/]"));
            table.AddColumn(new TableColumn("[bold]Role[/]"));
            table.AddColumn(new TableColumn("[bold]Requirements[/]"));

            foreach (var spec in result.Specializations)
            {
                // Check unlock status
                bool isUnlocked = _specializationService.HasUnlocked(character, spec.SpecializationID);
                var canUnlockResult = _specializationService.CanUnlock(character, spec.SpecializationID);
                bool canUnlock = canUnlockResult.Success;

                // Status icon
                string statusIcon;
                Color statusColor;
                if (isUnlocked)
                {
                    statusIcon = "✓";
                    statusColor = Color.Green;
                }
                else if (canUnlock)
                {
                    statusIcon = "○";
                    statusColor = Color.Yellow;
                }
                else
                {
                    statusIcon = "✗";
                    statusColor = Color.Red;
                }

                // Format specialization name with icon and tagline
                var specName = $"[bold]{spec.IconEmoji} {spec.Name}[/]\n[dim]{spec.Tagline}[/]";

                // Format role with path type and trauma risk
                var pathColor = spec.PathType == "Coherent" ? "cyan" : "purple";
                var traumaRiskColor = spec.TraumaRisk switch
                {
                    "None" => "green",
                    "Low" => "yellow",
                    "Medium" => "orange1",
                    "High" => "red",
                    "Extreme" => "purple",
                    _ => "grey"
                };
                var roleText = $"{spec.MechanicalRole}\n" +
                              $"[{pathColor}]{spec.PathType}[/] | [{traumaRiskColor}]{spec.TraumaRisk} Risk[/]";

                // Format requirements
                string requirementText;
                if (isUnlocked)
                {
                    var ppSpent = _specializationService.GetPPSpentInTree(character, spec.SpecializationID);
                    requirementText = $"[green]UNLOCKED[/]\n[dim]{ppSpent} PP spent[/]";
                }
                else if (canUnlock)
                {
                    requirementText = $"[yellow]{spec.PPCostToUnlock} PP[/]\n[dim]Can unlock[/]";
                }
                else
                {
                    requirementText = $"[red]{spec.PPCostToUnlock} PP[/]\n[dim]{canUnlockResult.Message}[/]";
                }

                table.AddRow(
                    $"[{statusColor}]{statusIcon}[/]",
                    specName,
                    roleText,
                    requirementText
                );
            }

            AnsiConsole.Write(table);
            AnsiConsole.WriteLine();

            // Build choice list
            var choices = new List<string>();
            foreach (var spec in result.Specializations)
            {
                var isUnlocked = _specializationService.HasUnlocked(character, spec.SpecializationID);
                var statusTag = isUnlocked ? "[green](Unlocked)[/]" : "";
                choices.Add($"{spec.IconEmoji} {spec.Name} {statusTag}");
            }
            choices.Add("← Back");

            // Prompt for selection
            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[yellow]Select a specialization to view details:[/]")
                    .PageSize(15)
                    .AddChoices(choices)
            );

            // Handle back
            if (choice == "← Back")
            {
                return false;
            }

            // Extract selected specialization
            var selectedSpecName = choice.Replace("[green](Unlocked)[/]", "").Trim();
            // Remove emoji by taking everything after the emoji
            selectedSpecName = selectedSpecName.Substring(selectedSpecName.IndexOf(' ') + 1).Trim();

            var selectedSpec = result.Specializations.FirstOrDefault(s => s.Name == selectedSpecName);
            if (selectedSpec == null) continue;

            // Show specialization details
            bool unlocked = ShowSpecializationDetails(character, selectedSpec);
            if (unlocked)
            {
                return true; // Return to caller - specialization was unlocked
            }
        }
    }

    /// <summary>
    /// Display detailed view of a specialization with ability tree
    /// Returns true if specialization was unlocked
    /// </summary>
    private bool ShowSpecializationDetails(PlayerCharacter character, SpecializationData spec)
    {
        AnsiConsole.Clear();
        AnsiConsole.WriteLine();

        // Header with specialization name
        var figlet = new FigletText(spec.Name)
            .Centered()
            .Color(Color.Yellow);
        AnsiConsole.Write(figlet);
        AnsiConsole.WriteLine();

        // Check unlock status
        bool isUnlocked = _specializationService.HasUnlocked(character, spec.SpecializationID);

        // Specialization overview panel
        var pathColor = spec.PathType == "Coherent" ? "cyan" : "purple";
        var traumaRiskColor = spec.TraumaRisk switch
        {
            "None" => "green",
            "Low" => "yellow",
            "Medium" => "orange1",
            "High" => "red",
            "Extreme" => "purple",
            _ => "grey"
        };

        var overviewMarkup = $"[bold]{spec.Tagline}[/]\n\n" +
                            $"{spec.Description}\n\n" +
                            $"[bold]Role:[/] {spec.MechanicalRole}\n" +
                            $"[bold]Path:[/] [{pathColor}]{spec.PathType}[/]\n" +
                            $"[bold]Primary Attribute:[/] {spec.PrimaryAttribute}\n" +
                            $"[bold]Secondary Attribute:[/] {spec.SecondaryAttribute}\n" +
                            $"[bold]Resource System:[/] {spec.ResourceSystem}\n" +
                            $"[bold]Trauma Risk:[/] [{traumaRiskColor}]{spec.TraumaRisk}[/]";

        var overviewPanel = new Panel(overviewMarkup)
        {
            Border = BoxBorder.Rounded,
            Header = new PanelHeader("[bold]SPECIALIZATION OVERVIEW[/]"),
            Padding = new Padding(2, 1)
        };
        overviewPanel.BorderColor(Color.Yellow);
        AnsiConsole.Write(overviewPanel);
        AnsiConsole.WriteLine();

        // Display ability tree preview
        DisplayAbilityTreePreview(spec);

        // Display unlock status / options
        if (isUnlocked)
        {
            var ppSpent = _specializationService.GetPPSpentInTree(character, spec.SpecializationID);
            AnsiConsole.MarkupLine($"[green]✓ This specialization is unlocked![/] [dim]({ppSpent} PP spent in tree)[/]");
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[dim]Press any key to return...[/]");
            Console.ReadKey();
            return false;
        }
        else
        {
            var canUnlockResult = _specializationService.CanUnlock(character, spec.SpecializationID);

            if (canUnlockResult.Success)
            {
                AnsiConsole.MarkupLine($"[yellow]Unlock Cost:[/] [bold]{spec.PPCostToUnlock} PP[/] [dim](you have {character.ProgressionPoints} PP)[/]");
                AnsiConsole.WriteLine();

                if (AnsiConsole.Confirm($"[yellow]Unlock {spec.Name} for {spec.PPCostToUnlock} PP?[/]"))
                {
                    var unlockResult = _specializationService.UnlockSpecialization(character, spec.SpecializationID);

                    if (unlockResult.Success)
                    {
                        AnsiConsole.MarkupLine($"[green]✓ {unlockResult.Message}[/]");
                        AnsiConsole.WriteLine();
                        AnsiConsole.MarkupLine("[dim]Press any key to continue...[/]");
                        Console.ReadKey();
                        return true;
                    }
                    else
                    {
                        AnsiConsole.MarkupLine($"[red]✗ Failed to unlock: {unlockResult.Message}[/]");
                        AnsiConsole.WriteLine();
                        AnsiConsole.MarkupLine("[dim]Press any key to return...[/]");
                        Console.ReadKey();
                        return false;
                    }
                }
            }
            else
            {
                AnsiConsole.MarkupLine($"[red]Cannot unlock:[/] {canUnlockResult.Message}");
                AnsiConsole.WriteLine();
                AnsiConsole.MarkupLine("[dim]Press any key to return...[/]");
                Console.ReadKey();
                return false;
            }
        }

        return false;
    }

    /// <summary>
    /// Display ability tree preview (abilities organized by tier)
    /// </summary>
    private void DisplayAbilityTreePreview(SpecializationData spec)
    {
        var abilitiesResult = _abilityService.GetAbilitiesForSpecialization(spec.SpecializationID);

        if (!abilitiesResult.Success || abilitiesResult.Abilities == null)
        {
            return;
        }

        var abilities = abilitiesResult.Abilities;

        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Cyan1);

        table.AddColumn(new TableColumn("[bold]Tier[/]").Centered());
        table.AddColumn(new TableColumn("[bold]Abilities (9 Total)[/]"));
        table.AddColumn(new TableColumn("[bold]Cost[/]").Centered());

        // Group abilities by tier
        var tier1 = abilities.Where(a => a.TierLevel == 1).ToList();
        var tier2 = abilities.Where(a => a.TierLevel == 2).ToList();
        var tier3 = abilities.Where(a => a.TierLevel == 3).ToList();
        var capstone = abilities.Where(a => a.TierLevel == 4).ToList();

        // Display Tier 1
        AddTierRow(table, "Tier 1", tier1, "Entry abilities");

        // Display Tier 2
        AddTierRow(table, "Tier 2", tier2, "Requires 8 PP in tree");

        // Display Tier 3
        AddTierRow(table, "Tier 3", tier3, "Requires 16 PP in tree");

        // Display Capstone
        AddTierRow(table, "Capstone", capstone, "Requires 24 PP + Tier 3 abilities");

        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();
    }

    /// <summary>
    /// Add a tier row to the ability tree table
    /// </summary>
    private void AddTierRow(Table table, string tierName, List<AbilityData> abilities, string requirement)
    {
        var abilityNames = string.Join("\n", abilities.Select(a =>
        {
            var typeIcon = a.AbilityType switch
            {
                "Active" => "⚔",
                "Passive" => "◈",
                "Reaction" => "⚡",
                _ => "•"
            };
            return $"{typeIcon} [yellow]{a.Name}[/] [dim]- {a.MechanicalSummary}[/]";
        }));

        var costText = abilities.Count > 0
            ? $"{abilities.Sum(a => a.PPCost)} PP total\n[dim]{requirement}[/]"
            : "[dim]No abilities[/]";

        table.AddRow(
            $"[bold cyan]{tierName}[/]",
            abilityNames,
            costText
        );
    }

    /// <summary>
    /// Display compact specialization summary (for character sheet integration)
    /// </summary>
    public void DisplaySpecializationSummary(PlayerCharacter character)
    {
        var unlockedSpecs = _specializationService.GetUnlockedSpecializations(character);

        if (unlockedSpecs.Count == 0)
        {
            AnsiConsole.MarkupLine("[dim]No specializations unlocked. Use 'specialize' to browse available specializations.[/]");
            return;
        }

        AnsiConsole.MarkupLine("[bold yellow]SPECIALIZATIONS[/]");

        foreach (var charSpec in unlockedSpecs)
        {
            var specResult = _specializationService.GetSpecialization(charSpec.SpecializationID);
            if (!specResult.Success || specResult.Specialization == null) continue;

            var spec = specResult.Specialization;
            var ppSpent = charSpec.PPSpentInTree;

            AnsiConsole.MarkupLine($"{spec.IconEmoji} [yellow]{spec.Name}[/] [dim]({ppSpent} PP spent)[/]");

            // Show learned abilities count
            var learnedAbilities = _abilityService.GetLearnedAbilities(character);
            var abilitiesForSpec = learnedAbilities.Where(ca =>
            {
                var abilityResult = _abilityService.GetAbility(ca.AbilityID);
                return abilityResult.Success &&
                       abilityResult.Ability != null &&
                       abilityResult.Ability.SpecializationID == spec.SpecializationID;
            }).ToList();

            AnsiConsole.MarkupLine($"  [dim]Abilities Learned: {abilitiesForSpec.Count}/9[/]");
        }

        AnsiConsole.WriteLine();
    }
}
