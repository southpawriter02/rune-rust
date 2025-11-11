using Spectre.Console;
using RuneAndRust.Core;
using RuneAndRust.Engine;

namespace RuneAndRust.ConsoleApp;

public static class UIHelper
{
    public static void DisplayCharacterSheet(PlayerCharacter character)
    {
        AnsiConsole.WriteLine();

        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Grey);

        // v0.7: Show specialization if present
        string classDisplay = character.Specialization != Specialization.None
            ? $"{character.Class} ({character.Specialization})"
            : character.Class.ToString();
        table.AddColumn(new TableColumn($"[bold yellow]{character.Name}[/] - [dim]{classDisplay}[/]").Centered());

        // Milestone and Legend with progress bar
        if (character.CurrentMilestone >= 3)
        {
            table.AddRow($"[yellow]Milestone {character.CurrentMilestone}[/] [dim](MAX)[/] | [dim]Total Legend: {character.CurrentLegend}[/]");
        }
        else
        {
            var legendProgress = (double)character.CurrentLegend / character.LegendToNextMilestone;
            var legendBarWidth = 15;
            var legendFilled = (int)(legendProgress * legendBarWidth);
            var legendEmpty = legendBarWidth - legendFilled;
            var legendBar = new string('█', legendFilled) + new string('░', legendEmpty);
            table.AddRow($"[yellow]Milestone {character.CurrentMilestone}[/] | [cyan]{legendBar}[/] [dim]{character.CurrentLegend}/{character.LegendToNextMilestone} Legend[/]");
        }

        // Progression Points
        if (character.ProgressionPoints > 0)
        {
            table.AddRow($"[bold green]PP Available: {character.ProgressionPoints}[/] [dim](type 'spend' to use)[/]");
        }

        // Resources
        table.AddRow(new Markup(""));  // Spacer
        table.AddRow(new Markup("[bold]RESOURCES[/]"));
        var hpBar = CreateBar("HP", character.HP, character.MaxHP, Color.Red, Color.DarkRed);
        var staminaBar = CreateBar("Stamina", character.Stamina, character.MaxStamina, Color.Green, Color.DarkGreen);
        table.AddRow(hpBar);
        table.AddRow(staminaBar);
        table.AddRow($"[dim]AP:[/] {character.AP}");

        // Status Effects
        var statusEffects = new List<string>();
        if (character.BattleRageTurnsRemaining > 0)
            statusEffects.Add($"[red]Battle Rage ({character.BattleRageTurnsRemaining} turns)[/]");
        if (character.ShieldAbsorptionRemaining > 0)
            statusEffects.Add($"[cyan]Shield ({character.ShieldAbsorptionRemaining} absorption)[/]");
        if (character.DefenseTurnsRemaining > 0)
            statusEffects.Add($"[blue]Defense +{character.DefenseBonus}% ({character.DefenseTurnsRemaining} turns)[/]");

        // v0.7: Adept status effects
        if (character.VulnerableTurnsRemaining > 0)
            statusEffects.Add($"[yellow]Vulnerable ({character.VulnerableTurnsRemaining} turns) - Take +25% damage[/]");
        if (character.AnalyzedTurnsRemaining > 0)
            statusEffects.Add($"[cyan]Analyzed ({character.AnalyzedTurnsRemaining} turns) - Allies +2 Accuracy[/]");
        if (character.SeizedTurnsRemaining > 0)
            statusEffects.Add($"[red]Seized ({character.SeizedTurnsRemaining} turns) - Cannot act![/]");
        if (character.IsPerforming)
            statusEffects.Add($"[magenta]Performing: {character.CurrentPerformance} ({character.PerformingTurnsRemaining} turns)[/]");
        if (character.InspiredTurnsRemaining > 0)
            statusEffects.Add($"[yellow]Inspired ({character.InspiredTurnsRemaining} turns) - +3 damage dice[/]");
        if (character.SilencedTurnsRemaining > 0)
            statusEffects.Add($"[red]Silenced ({character.SilencedTurnsRemaining} turns) - Cannot cast/perform[/]");
        if (character.TempHP > 0)
            statusEffects.Add($"[cyan]Temp HP: {character.TempHP}[/]");

        if (statusEffects.Count > 0)
        {
            table.AddRow(new Markup(""));  // Spacer
            table.AddRow(new Markup("[bold]STATUS EFFECTS[/]"));
            foreach (var effect in statusEffects)
            {
                table.AddRow(new Markup(effect));
            }
        }

        // Trauma Economy (v0.5)
        table.AddRow(new Markup(""));  // Spacer
        table.AddRow(new Markup("[bold]TRAUMA ECONOMY[/]"));

        var traumaService = new TraumaEconomyService();
        var stressThreshold = traumaService.GetStressThreshold(character);
        var corruptionThreshold = traumaService.GetCorruptionThreshold(character);

        // Psychic Stress meter with color coding
        var stressColor = stressThreshold switch
        {
            StressThreshold.Safe => Color.Green,
            StressThreshold.Strained => Color.Yellow,
            StressThreshold.Severe => Color.Orange1,
            StressThreshold.Critical => Color.Red,
            _ => Color.Green
        };
        var stressEmptyColor = stressThreshold switch
        {
            StressThreshold.Safe => Color.DarkGreen,
            StressThreshold.Strained => Color.DarkOrange,
            StressThreshold.Severe => Color.DarkRed,
            StressThreshold.Critical => Color.Maroon,
            _ => Color.DarkGreen
        };
        var stressBar = CreateBar("Psychic Stress", character.PsychicStress, 100, stressColor, stressEmptyColor);
        table.AddRow(stressBar);

        // Corruption meter with color coding
        var corruptionColor = corruptionThreshold switch
        {
            CorruptionThreshold.Minimal => Color.Grey,
            CorruptionThreshold.Low => Color.Yellow,
            CorruptionThreshold.Moderate => Color.Orange1,
            CorruptionThreshold.High => Color.Red,
            CorruptionThreshold.Extreme => Color.Purple,
            _ => Color.Grey
        };
        var corruptionEmptyColor = corruptionThreshold switch
        {
            CorruptionThreshold.Minimal => Color.Grey19,
            CorruptionThreshold.Low => Color.DarkOrange,
            CorruptionThreshold.Moderate => Color.DarkRed,
            CorruptionThreshold.High => Color.Maroon,
            CorruptionThreshold.Extreme => Color.Purple4,
            _ => Color.Grey19
        };
        var corruptionBar = CreateBar("Corruption", character.Corruption, 100, corruptionColor, corruptionEmptyColor);
        table.AddRow(corruptionBar);

        // Threshold warnings
        if (stressThreshold >= StressThreshold.Severe)
        {
            var stressWarning = stressThreshold == StressThreshold.Critical
                ? "[red]⚠️ CRITICAL STRESS - Seek Sanctuary immediately![/]"
                : "[orange1]⚠️ High Stress - Rest at Sanctuary soon[/]";
            table.AddRow(stressWarning);
        }
        if (corruptionThreshold >= CorruptionThreshold.High)
        {
            var corruptionWarning = corruptionThreshold == CorruptionThreshold.Extreme
                ? "[purple]⚠️ EXTREME CORRUPTION - You are becoming something else...[/]"
                : "[red]⚠️ High Corruption - Limit heretical ability use[/]";
            table.AddRow(corruptionWarning);
        }

        // Attributes
        table.AddRow(new Markup(""));  // Spacer
        table.AddRow(new Markup("[bold]ATTRIBUTES[/]"));
        table.AddRow($"MIGHT: {character.Attributes.Might} | FINESSE: {character.Attributes.Finesse} | WITS: {character.Attributes.Wits}");
        table.AddRow($"WILL: {character.Attributes.Will} | STURDINESS: {character.Attributes.Sturdiness}");

        // Equipment (v0.3)
        table.AddRow(new Markup(""));  // Spacer
        table.AddRow(new Markup("[bold]EQUIPMENT[/]"));

        if (character.EquippedWeapon != null)
        {
            var weapon = character.EquippedWeapon;
            table.AddRow($"[yellow]Weapon:[/] {weapon.GetDisplayName()}");
            table.AddRow($"  • {weapon.GetDamageDescription()} damage ({weapon.StaminaCost} Stamina)");
            if (weapon.GetBonusesDescription() != "None")
                table.AddRow($"  • {weapon.GetBonusesDescription()}");
        }
        else if (!string.IsNullOrEmpty(character.WeaponName))
        {
            // Fallback for legacy saves (v0.1/v0.2)
            table.AddRow($"[yellow]Weapon:[/] {character.WeaponName} ([yellow]{character.WeaponAttribute.ToUpper()}[/]-based, {character.BaseDamage}d6)");
        }
        else
        {
            table.AddRow($"[yellow]Weapon:[/] [dim]Unarmed[/]");
        }

        if (character.EquippedArmor != null)
        {
            var armor = character.EquippedArmor;
            table.AddRow($"[yellow]Armor:[/] {armor.GetDisplayName()}");
            table.AddRow($"  • +{armor.HPBonus} HP, -{armor.DefenseBonus} to enemy attacks");
            if (armor.GetBonusesDescription() != "None")
                table.AddRow($"  • {armor.GetBonusesDescription()}");
        }
        else
        {
            table.AddRow($"[yellow]Armor:[/] [dim]None[/]");
        }

        // Abilities (all usable in v0.2.1, ranks shown)
        table.AddRow(new Markup(""));  // Spacer
        table.AddRow(new Markup("[bold]ABILITIES[/]"));

        foreach (var ability in character.Abilities)
        {
            var costColor = character.Stamina >= ability.StaminaCost ? "green" : "red";
            var rankTag = ability.CurrentRank > 1 ? $" [cyan][Rank {ability.CurrentRank}][/]" : "";
            table.AddRow($"[yellow]{ability.Name}[/]{rankTag} ([{costColor}]{ability.StaminaCost} Stamina[/])");
            table.AddRow($"[dim]{ability.Description}[/]");
        }

        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();
    }

    public static void DisplayRoomDescription(Room room, List<string> availableDirections)
    {
        AnsiConsole.WriteLine();

        var panel = new Panel(new Markup($"[bold]{room.Name}[/]\n\n{room.Description}"))
        {
            Border = BoxBorder.Rounded,
            BorderColor = Color.Blue,
            Padding = new Padding(2, 1)
        };

        AnsiConsole.Write(panel);

        // Show exits
        if (availableDirections.Count > 0)
        {
            var exits = string.Join(", ", availableDirections.Select(d => $"[yellow]{d}[/]"));
            AnsiConsole.MarkupLine($"[dim]Exits: {exits}[/]");
        }

        // Show if puzzle is present
        if (room.HasPuzzle && !room.IsPuzzleSolved)
        {
            AnsiConsole.MarkupLine("[yellow]⚠ A puzzle blocks your progress. Use 'solve' to attempt it.[/]");
        }

        // Show if enemies are present (but don't spoil them)
        if (!room.HasBeenCleared && room.Enemies.Count > 0)
        {
            AnsiConsole.MarkupLine("[red]⚠ You sense danger ahead...[/]");
        }

        // Show items on ground (v0.3)
        if (room.ItemsOnGround.Count > 0)
        {
            DisplayGroundItems(room);
        }

        AnsiConsole.WriteLine();
    }

    public static void DisplayCombatStart(List<Enemy> enemies)
    {
        AnsiConsole.WriteLine();

        // Check if boss fight
        var hasBoss = enemies.Any(e => e.IsBoss);
        if (hasBoss)
        {
            DisplayBossArt();
        }

        var rule = new Rule("[bold red]⚔ COMBAT INITIATED ⚔[/]")
        {
            Justification = Justify.Center
        };
        AnsiConsole.Write(rule);
        AnsiConsole.WriteLine();

        AnsiConsole.MarkupLine("[red]Enemies detected:[/]");
        foreach (var enemy in enemies)
        {
            var legendReward = enemy.BaseLegendValue > 0 ? $" [dim]({enemy.BaseLegendValue} Legend)[/]" : "";
            AnsiConsole.MarkupLine($"  • [bold]{enemy.Name}[/] ([dim]HP: {enemy.HP}/{enemy.MaxHP}{legendReward}[/])");
        }
        AnsiConsole.WriteLine();
    }

    private static void DisplayBossArt()
    {
        var bossArt = @"
    ╔════════════════════════════════════════════════╗
    ║                                                ║
    ║       ████████╗  ██╗    ██╗  ██████╗          ║
    ║       ██╔═══██║  ██║    ██║  ██╔══██╗         ║
    ║       ████████╔╝ ██║ █╗ ██║  ██████╔╝         ║
    ║       ██╔═══██╗ ██║███╗██║  ██╔═══╝          ║
    ║       ██║   ██║ ╚███╔███╔╝  ██║              ║
    ║       ╚═╝   ╚═╝  ╚══╝╚══╝   ╚═╝              ║
    ║                                                ║
    ║            ▄████████████████▄                  ║
    ║          ▄█▀▀            ▀▀█▄                ║
    ║         █▀  ████    ████  ▀█                 ║
    ║        █    ████    ████    █                ║
    ║        █                    █                ║
    ║        █    ▄██████████▄    █                ║
    ║         █   █          █   █                 ║
    ║          ▀█▄▄        ▄▄█▀                   ║
    ║            ▀████████████▀                    ║
    ║         ███ ██████████ ███                   ║
    ║        █████████████████████                 ║
    ║       ███████████████████████                ║
    ║                                                ║
    ║          R U I N - W A R D E N                ║
    ║                                                ║
    ╚════════════════════════════════════════════════╝
";

        AnsiConsole.Markup($"[red]{bossArt.EscapeMarkup()}[/]");
        AnsiConsole.WriteLine();
    }

    public static void DisplayPuzzlePrompt(Room room)
    {
        AnsiConsole.WriteLine();
        var rule = new Rule("[bold yellow]⚙ PUZZLE CHAMBER ⚙[/]")
        {
            Justification = Justify.Center
        };
        AnsiConsole.Write(rule);
        AnsiConsole.WriteLine();

        var panel = new Panel(new Markup(room.PuzzleDescription))
        {
            Border = BoxBorder.Double,
            BorderColor = Color.Yellow,
            Header = new PanelHeader("[bold]Environmental Puzzle[/]"),
            Padding = new Padding(2, 1)
        };

        AnsiConsole.Write(panel);
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine($"[dim]This puzzle requires a [yellow]WITS check[/] ({room.PuzzleSuccessThreshold} successes needed).[/]");
        AnsiConsole.MarkupLine($"[dim]Failure will deal [red]{room.PuzzleFailureDamage}d6 damage[/].[/]");
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[yellow]Type 'solve' to attempt the puzzle.[/]");
        AnsiConsole.WriteLine();
    }

    public static void DisplayDiceRoll(DiceResult result, string context = "")
    {
        var contextText = string.IsNullOrEmpty(context) ? "" : $"{context}: ";
        var rollsDisplay = string.Join(" ", result.Rolls.Select(r =>
            r >= 5 ? $"[green]{r}[/]" : $"[dim]{r}[/]"
        ));

        AnsiConsole.MarkupLine($"{contextText}Rolled [yellow]{result.DiceRolled}d6[/]: [{rollsDisplay}] = [bold]{result.Successes}[/] successes");
    }

    public static void DisplayVictory()
    {
        DisplayVictory(null);
    }

    public static void DisplayVictory(PlayerCharacter? player)
    {
        AnsiConsole.Clear();
        AnsiConsole.WriteLine();

        var figlet = new FigletText("VICTORY")
            .Centered()
            .Color(Color.Gold1);
        AnsiConsole.Write(figlet);

        AnsiConsole.WriteLine();

        var storyText = "[bold green]You have defeated the Ruin-Warden and survived the facility.[/]\n\n" +
                       "The corrupted machines fall silent. The hum of residual energy fades.\n" +
                       "You stand alone in the twilight ruins, victorious.\n\n" +
                       "[dim]THE END[/]";

        // Add player stats if available
        if (player != null)
        {
            var healthPercent = (int)((double)player.HP / player.MaxHP * 100);
            storyText += $"\n\n[bold yellow]Final Status:[/]\n" +
                        $"[green]Milestone:[/] {player.CurrentMilestone}\n" +
                        $"[green]Total Legend:[/] {player.CurrentLegend}\n" +
                        $"[green]HP:[/] {player.HP}/{player.MaxHP} ({healthPercent}%)\n" +
                        $"[green]Stamina:[/] {player.Stamina}/{player.MaxStamina}\n" +
                        $"[dim]You survived with {healthPercent}% health remaining.[/]";
        }

        storyText += "\n\n[yellow]Thank you for playing Rune & Rust v0.2![/]";

        var panel = new Panel(storyText)
        {
            Border = BoxBorder.Double,
            BorderColor = Color.Gold1,
            Padding = new Padding(2, 1)
        };
        AnsiConsole.Write(panel);
        AnsiConsole.WriteLine();
    }

    public static void DisplayGameOver()
    {
        AnsiConsole.Clear();
        AnsiConsole.WriteLine();

        var figlet = new FigletText("GAME OVER")
            .Centered()
            .Color(Color.Red);
        AnsiConsole.Write(figlet);

        AnsiConsole.WriteLine();
        var panel = new Panel(
            "[bold red]You have fallen in the depths of the facility.[/]\n\n" +
            "The corrupted machines stand over your broken form.\n" +
            "Your journey ends here, in the twilight ruins.\n\n" +
            "[dim]Perhaps another survivor will fare better...[/]"
        )
        {
            Border = BoxBorder.Double,
            BorderColor = Color.Red,
            Padding = new Padding(2, 1)
        };
        AnsiConsole.Write(panel);
        AnsiConsole.WriteLine();
    }

    public static void DisplayCombatState(CombatState combat)
    {
        AnsiConsole.Clear();
        AnsiConsole.WriteLine();

        var rule = new Rule("[bold red]⚔ COMBAT ⚔[/]")
        {
            Justification = Justify.Center
        };
        AnsiConsole.Write(rule);
        AnsiConsole.WriteLine();

        // Player status
        var playerTable = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Green);

        playerTable.AddColumn($"[bold yellow]{combat.Player.Name}[/] - Milestone {combat.Player.CurrentMilestone}");
        playerTable.AddRow(CreateBar("HP", combat.Player.HP, combat.Player.MaxHP, Color.Red, Color.DarkRed));
        playerTable.AddRow(CreateBar("Stamina", combat.Player.Stamina, combat.Player.MaxStamina, Color.Green, Color.DarkGreen));

        // Player status effects
        var playerEffects = new List<string>();
        if (combat.Player.DefenseTurnsRemaining > 0)
            playerEffects.Add($"Defense: {combat.Player.DefenseBonus}% ({combat.Player.DefenseTurnsRemaining} turns)");
        if (combat.PlayerNextAttackBonusDice > 0)
            playerEffects.Add($"Next attack: +{combat.PlayerNextAttackBonusDice} dice");
        if (combat.PlayerNegateNextAttack)
            playerEffects.Add($"Dodge ready!");
        if (combat.Player.BattleRageTurnsRemaining > 0)
            playerEffects.Add($"Battle Rage ({combat.Player.BattleRageTurnsRemaining} turns)");
        if (combat.Player.ShieldAbsorptionRemaining > 0)
            playerEffects.Add($"Shield ({combat.Player.ShieldAbsorptionRemaining} absorption)");

        // v0.7: Adept status effects
        if (combat.Player.TempHP > 0)
            playerEffects.Add($"Temp HP: {combat.Player.TempHP}");
        if (combat.Player.VulnerableTurnsRemaining > 0)
            playerEffects.Add($"[Vulnerable] +25% damage taken ({combat.Player.VulnerableTurnsRemaining} turns)");
        if (combat.Player.InspiredTurnsRemaining > 0)
            playerEffects.Add($"[Inspired] +3 damage dice ({combat.Player.InspiredTurnsRemaining} turns)");
        if (combat.Player.SeizedTurnsRemaining > 0)
            playerEffects.Add($"[Seized] Cannot act! ({combat.Player.SeizedTurnsRemaining} turns)");
        if (combat.Player.SilencedTurnsRemaining > 0)
            playerEffects.Add($"[Silenced] Cannot perform ({combat.Player.SilencedTurnsRemaining} turns)");
        if (combat.Player.IsPerforming)
            playerEffects.Add($"[Performing] {combat.Player.CurrentPerformance} ({combat.Player.PerformingTurnsRemaining} turns)");

        if (playerEffects.Count > 0)
        {
            foreach (var effect in playerEffects)
            {
                playerTable.AddRow($"[cyan]{effect}[/]");
            }
        }

        AnsiConsole.Write(playerTable);
        AnsiConsole.WriteLine();

        // Enemies status
        var enemyTable = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Red);

        enemyTable.AddColumn("[bold]Enemies[/]");

        var aliveEnemies = combat.Enemies.Where(e => e.IsAlive).ToList();
        for (int i = 0; i < aliveEnemies.Count; i++)
        {
            var enemy = aliveEnemies[i];
            var hpBar = CreateBar($"[{i + 1}] {enemy.Name}", enemy.HP, enemy.MaxHP, Color.Red, Color.DarkRed);

            var statusEffects = new List<string>();
            if (enemy.DefenseTurnsRemaining > 0)
                statusEffects.Add($"DEF:{enemy.DefenseBonus}%");
            if (enemy.IsStunned)
                statusEffects.Add("STUNNED");
            if (enemy.BleedingTurnsRemaining > 0)
                statusEffects.Add($"BLEEDING({enemy.BleedingTurnsRemaining})");
            if (enemy.AnalyzedTurnsRemaining > 0)
                statusEffects.Add($"ANALYZED({enemy.AnalyzedTurnsRemaining})");

            var statusText = statusEffects.Count > 0 ? $" [dim]({string.Join(", ", statusEffects)})[/]" : "";
            enemyTable.AddRow(new Markup($"{hpBar}{statusText}"));
        }

        AnsiConsole.Write(enemyTable);
        AnsiConsole.WriteLine();

        // Initiative order indicator
        var currentParticipant = combat.CurrentParticipant;
        var currentName = currentParticipant.IsPlayer ? combat.Player.Name : ((Enemy)currentParticipant.Character!).Name;
        var turnColor = currentParticipant.IsPlayer ? "green" : "red";
        AnsiConsole.MarkupLine($"[{turnColor}]▶ {currentName}'s turn[/]");
        AnsiConsole.WriteLine();
    }

    public static void DisplayCombatLog(List<string> logEntries, int maxEntries = 10)
    {
        if (logEntries.Count == 0) return;

        var panel = new Panel(string.Join("\n", logEntries.TakeLast(maxEntries)))
        {
            Border = BoxBorder.Rounded,
            BorderColor = Color.Grey,
            Header = new PanelHeader("[dim]Combat Log[/]"),
            Padding = new Padding(1, 0)
        };

        AnsiConsole.Write(panel);
        AnsiConsole.WriteLine();
    }

    public static string PromptCombatAction(CombatState combat)
    {
        var choices = new List<string>
        {
            "Attack - Strike an enemy",
            "Defend - Reduce incoming damage",
            "Ability - Use a special ability",
        };

        if (combat.CanFlee)
        {
            choices.Add("Flee - Attempt to escape");
        }

        choices.Add("Stats - View character sheet");

        var choice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[yellow]Choose your action:[/]")
                .PageSize(10)
                .AddChoices(choices)
        );

        return choice.Split('-')[0].Trim().ToLower();
    }

    public static int PromptEnemyTarget(CombatState combat)
    {
        var aliveEnemies = combat.Enemies.Where(e => e.IsAlive).ToList();

        if (aliveEnemies.Count == 1)
        {
            return 1; // Auto-target if only one enemy
        }

        var choices = new List<string>();
        for (int i = 0; i < aliveEnemies.Count; i++)
        {
            var enemy = aliveEnemies[i];
            choices.Add($"[{i + 1}] {enemy.Name} (HP: {enemy.HP}/{enemy.MaxHP})");
        }

        var choice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[yellow]Select target:[/]")
                .PageSize(10)
                .AddChoices(choices)
        );

        // Extract index from choice
        var indexStr = choice.Substring(choice.IndexOf('[') + 1, choice.IndexOf(']') - 1);
        return int.Parse(indexStr);
    }

    public static string PromptAbilityChoice(PlayerCharacter player)
    {
        var choices = new List<string>();

        // Show all abilities (all are usable in v0.2.1)
        foreach (var ability in player.Abilities)
        {
            var canAfford = player.Stamina >= ability.StaminaCost;
            var staminaColor = canAfford ? "green" : "red";
            var rankTag = ability.CurrentRank > 1 ? $" [Rank {ability.CurrentRank}]" : "";
            choices.Add($"{ability.Name}{rankTag} ([{staminaColor}]{ability.StaminaCost} Stamina[/]) - {ability.Description}");
        }

        choices.Add("Cancel - Go back");

        var choice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[yellow]Choose ability:[/]")
                .PageSize(10)
                .AddChoices(choices)
                .EnableSearch()
        );

        if (choice.StartsWith("Cancel"))
        {
            return "cancel";
        }

        return choice.Split('(')[0].Trim();
    }

    private static Markup CreateBar(string label, int current, int max, Color fillColor, Color emptyColor)
    {
        var percentage = max > 0 ? (double)current / max : 0;
        var barWidth = 20;
        var filledBlocks = (int)(percentage * barWidth);
        var emptyBlocks = barWidth - filledBlocks;

        var filled = new string('█', filledBlocks);
        var empty = new string('░', emptyBlocks);

        return new Markup($"[dim]{label}:[/] [{fillColor}]{filled}[/][{emptyColor}]{empty}[/] [bold]{current}/{max}[/]");
    }

    // ============================================================
    // EQUIPMENT & INVENTORY UI (v0.3)
    // ============================================================

    /// <summary>
    /// Display player inventory and equipped items
    /// </summary>
    public static void DisplayInventory(PlayerCharacter player)
    {
        AnsiConsole.WriteLine();

        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Yellow);

        table.AddColumn(new TableColumn($"[bold yellow]{player.Name}'s Equipment & Inventory[/]").Centered());

        // Equipped Items
        table.AddRow(new Markup("[bold]EQUIPPED[/]"));

        if (player.EquippedWeapon != null)
        {
            var weapon = player.EquippedWeapon;
            table.AddRow($"[yellow]Weapon:[/] {weapon.GetDisplayName()}");
            table.AddRow($"  [dim]• Damage: {weapon.GetDamageDescription()} ({weapon.StaminaCost} Stamina)[/]");
            if (weapon.AccuracyBonus != 0)
                table.AddRow($"  [dim]• Accuracy: {(weapon.AccuracyBonus > 0 ? "+" : "")}{weapon.AccuracyBonus}[/]");
            var bonuses = weapon.GetBonusesDescription();
            if (bonuses != "None")
                table.AddRow($"  [dim]• Bonuses: {bonuses}[/]");
        }
        else
        {
            table.AddRow($"[yellow]Weapon:[/] [dim]None equipped (unarmed)[/]");
        }

        if (player.EquippedArmor != null)
        {
            var armor = player.EquippedArmor;
            table.AddRow($"[yellow]Armor:[/] {armor.GetDisplayName()}");
            table.AddRow($"  [dim]• HP Bonus: +{armor.HPBonus}[/]");
            table.AddRow($"  [dim]• Defense: -{armor.DefenseBonus} to enemy attacks[/]");
            var bonuses = armor.GetBonusesDescription();
            if (bonuses != "None")
                table.AddRow($"  [dim]• Bonuses: {bonuses}[/]");
        }
        else
        {
            table.AddRow($"[yellow]Armor:[/] [dim]None equipped[/]");
        }

        // Inventory
        table.AddRow(new Markup(""));  // Spacer
        table.AddRow(new Markup($"[bold]CARRIED[/] [dim]({player.Inventory.Count}/{player.MaxInventorySize})[/]"));

        if (player.Inventory.Count == 0)
        {
            table.AddRow(new Markup("[dim]Empty[/]"));
        }
        else
        {
            foreach (var item in player.Inventory)
            {
                var itemType = item.Type == EquipmentType.Weapon ? "⚔" : "🛡";
                var qualityColor = GetQualityColor(item.Quality);
                table.AddRow($"[{qualityColor}]{itemType} {item.GetDisplayName()}[/]");

                if (item.Type == EquipmentType.Weapon)
                {
                    table.AddRow($"  [dim]• Damage: {item.GetDamageDescription()} ({item.StaminaCost} Stamina)[/]");
                }
                else
                {
                    table.AddRow($"  [dim]• HP: +{item.HPBonus}, Defense: -{item.DefenseBonus}[/]");
                }
            }
        }

        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[dim]Use 'equip [item]' to equip from inventory, 'compare [item]' to compare items.[/]");
        AnsiConsole.WriteLine();
    }

    /// <summary>
    /// Display items on ground in current room
    /// </summary>
    public static void DisplayGroundItems(Room room)
    {
        if (room.ItemsOnGround.Count == 0) return;

        AnsiConsole.MarkupLine("[yellow]Items on the ground:[/]");
        foreach (var item in room.ItemsOnGround)
        {
            var itemType = item.Type == EquipmentType.Weapon ? "⚔" : "🛡";
            var qualityColor = GetQualityColor(item.Quality);
            AnsiConsole.MarkupLine($"  [{qualityColor}]{itemType} {item.GetDisplayName()}[/] [dim]- Use 'pickup {item.Name}' to take[/]");
        }
        AnsiConsole.WriteLine();
    }

    /// <summary>
    /// Display equipment comparison
    /// </summary>
    public static void DisplayEquipmentComparison(EquipmentComparison comparison)
    {
        AnsiConsole.WriteLine();

        var panel = new Panel(BuildComparisonMarkup(comparison))
        {
            Border = BoxBorder.Double,
            BorderColor = comparison.IsUpgrade ? Color.Green : Color.Red,
            Header = new PanelHeader("[bold]EQUIPMENT COMPARISON[/]"),
            Padding = new Padding(2, 1)
        };

        AnsiConsole.Write(panel);
        AnsiConsole.WriteLine();
    }

    private static string BuildComparisonMarkup(EquipmentComparison comparison)
    {
        var markup = "";

        // Current item
        if (comparison.Current != null)
        {
            markup += $"[bold]CURRENT:[/] {comparison.Current.GetDisplayName()}\n";
            if (comparison.Current.Type == EquipmentType.Weapon)
            {
                markup += $"  • Damage: {comparison.Current.GetDamageDescription()}\n";
                markup += $"  • Stamina Cost: {comparison.Current.StaminaCost}\n";
                if (comparison.Current.AccuracyBonus != 0)
                    markup += $"  • Accuracy: {(comparison.Current.AccuracyBonus > 0 ? "+" : "")}{comparison.Current.AccuracyBonus}\n";
            }
            else
            {
                markup += $"  • HP Bonus: +{comparison.Current.HPBonus}\n";
                markup += $"  • Defense: -{comparison.Current.DefenseBonus}\n";
            }
            var currentBonuses = comparison.Current.GetBonusesDescription();
            if (currentBonuses != "None")
                markup += $"  • Bonuses: {currentBonuses}\n";

            markup += "\n";
        }
        else
        {
            markup += "[bold]CURRENT:[/] [dim]Nothing equipped[/]\n\n";
        }

        // Proposed item
        markup += $"[bold]NEW:[/] {comparison.Proposed.GetDisplayName()}\n";
        if (comparison.Proposed.Type == EquipmentType.Weapon)
        {
            markup += $"  • Damage: {comparison.Proposed.GetDamageDescription()}\n";
            markup += $"  • Stamina Cost: {comparison.Proposed.StaminaCost}\n";
            if (comparison.Proposed.AccuracyBonus != 0)
                markup += $"  • Accuracy: {(comparison.Proposed.AccuracyBonus > 0 ? "+" : "")}{comparison.Proposed.AccuracyBonus}\n";
        }
        else
        {
            markup += $"  • HP Bonus: +{comparison.Proposed.HPBonus}\n";
            markup += $"  • Defense: -{comparison.Proposed.DefenseBonus}\n";
        }
        var proposedBonuses = comparison.Proposed.GetBonusesDescription();
        if (proposedBonuses != "None")
            markup += $"  • Bonuses: {proposedBonuses}\n";

        // Differences
        if (comparison.Differences.Count > 0)
        {
            markup += "\n[bold]CHANGES:[/]\n";
            foreach (var diff in comparison.Differences)
            {
                var color = diff.StartsWith("NEW:") || diff.Contains("+") ? "green" : "red";
                markup += $"  [{color}]• {diff}[/]\n";
            }
        }

        // Verdict
        markup += "\n";
        if (comparison.IsUpgrade)
        {
            markup += "[bold green]⬆ UPGRADE RECOMMENDED[/]";
        }
        else
        {
            markup += "[bold red]⬇ DOWNGRADE WARNING[/]";
        }

        return markup;
    }

    private static string GetQualityColor(QualityTier quality)
    {
        return quality switch
        {
            QualityTier.JuryRigged => "grey",
            QualityTier.Scavenged => "white",
            QualityTier.ClanForged => "blue",
            QualityTier.Optimized => "purple",
            QualityTier.MythForged => "yellow",
            _ => "white"
        };
    }
}
