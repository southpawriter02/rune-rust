using Spectre.Console;
using RuneAndRust.Core;
using RuneAndRust.Engine;
using RuneAndRust.Persistence;

namespace RuneAndRust.ConsoleApp;

public static class UIHelper
{
    public static void DisplayCharacterSheet(PlayerCharacter character, string? connectionString = null)
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

        // v0.19: Specialization Summary
        if (!string.IsNullOrEmpty(connectionString))
        {
            DisplaySpecializationSummary(table, character, connectionString);
        }

        // Resources
        table.AddRow(new Markup(""));  // Spacer
        table.AddRow(new Markup("[bold]RESOURCES[/]"));
        var hpBar = CreateBar("HP", character.HP, character.MaxHP, Color.Red, Color.DarkRed);
        var staminaBar = CreateBar("Stamina", character.Stamina, character.MaxStamina, Color.Green, Color.DarkGreen);
        table.AddRow(hpBar);
        table.AddRow(staminaBar);
        table.AddRow($"[dim]AP:[/] {character.AP}");
        table.AddRow($"[dim]Currency:[/] [green]{character.Currency} Cogs ⚙[/]"); // v0.9

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

        // v0.20.3: Disoriented status
        if (character.DisorientedTurnsRemaining > 0)
            statusEffects.Add($"[yellow]Disoriented ({character.DisorientedTurnsRemaining} turns) - Gravity/spatial distortion[/]");

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

        // Show NPCs in room (v0.8, v0.9)
        if (room.NPCs.Count > 0)
        {
            var aliveNPCs = room.NPCs.Where(npc => npc.IsAlive).ToList();
            if (aliveNPCs.Count > 0)
            {
                AnsiConsole.MarkupLine("[cyan]⏵ People here:[/]");
                foreach (var npc in aliveNPCs)
                {
                    var hostileTag = npc.IsHostile ? "[red](HOSTILE)[/] " : "";

                    // v0.9: Special display for merchants
                    if (npc is Merchant merchant)
                    {
                        var merchantType = merchant.Type switch
                        {
                            MerchantType.General => "General Merchant",
                            MerchantType.Apothecary => "Apothecary",
                            MerchantType.ScrapTrader => "Scrap Trader",
                            _ => "Merchant"
                        };
                        AnsiConsole.MarkupLine($"  • [yellow]💰[/] [cyan]{npc.Name.EscapeMarkup()}[/] [dim]({merchantType}) (use 'shop' or 'talk')[/]");
                    }
                    else
                    {
                        AnsiConsole.MarkupLine($"  • {hostileTag}[cyan]{npc.Name.EscapeMarkup()}[/] [dim](use 'talk')[/]");
                    }
                }
            }
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

        // [v2.0] Stance indicator - persistent display of current stance
        var stanceService = new StanceService();
        var stanceName = stanceService.GetStanceName(combat.Player.ActiveStance.Type);
        var stanceColor = combat.Player.ActiveStance.Type switch
        {
            StanceType.Aggressive => "red",
            StanceType.Defensive => "blue",
            StanceType.Calculated => "white",
            StanceType.Evasive => "yellow",
            _ => "white"
        };
        var stanceIcon = combat.Player.ActiveStance.Type switch
        {
            StanceType.Aggressive => "⚔",
            StanceType.Defensive => "🛡",
            StanceType.Calculated => "⚖",
            StanceType.Evasive => "💨",
            _ => "•"
        };
        playerEffects.Add($"[{stanceColor}]{stanceIcon} Stance: {stanceName}[/] ({combat.Player.StanceShiftsRemaining} shift{(combat.Player.StanceShiftsRemaining == 1 ? "" : "s")} left)");

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
            playerEffects.Add($"[[Vulnerable]] +25% damage taken ({combat.Player.VulnerableTurnsRemaining} turns)");
        if (combat.Player.InspiredTurnsRemaining > 0)
            playerEffects.Add($"[[Inspired]] +3 damage dice ({combat.Player.InspiredTurnsRemaining} turns)");
        if (combat.Player.SeizedTurnsRemaining > 0)
            playerEffects.Add($"[[Seized]] Cannot act! ({combat.Player.SeizedTurnsRemaining} turns)");
        if (combat.Player.SilencedTurnsRemaining > 0)
            playerEffects.Add($"[[Silenced]] Cannot perform ({combat.Player.SilencedTurnsRemaining} turns)");
        if (combat.Player.IsPerforming)
            playerEffects.Add($"[[Performing]] {combat.Player.CurrentPerformance} ({combat.Player.PerformingTurnsRemaining} turns)");

        if (playerEffects.Count > 0)
        {
            foreach (var effect in playerEffects)
            {
                playerTable.AddRow($"[cyan]{effect}[/]");
            }
        }

        // [v0.20] Grid position display
        if (combat.Grid != null && combat.Player.Position != null)
        {
            var pos = combat.Player.Position.Value;
            var posDisplay = $"Position: {pos.Zone} {pos.Row} (Col {pos.Column})";
            if (combat.Player.KineticEnergy > 0)
            {
                posDisplay += $" | KE: {combat.Player.KineticEnergy}/{combat.Player.MaxKineticEnergy}";
            }
            playerTable.AddRow($"[dim]{posDisplay}[/]");
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
            if (enemy.VulnerableTurnsRemaining > 0)
                statusEffects.Add($"VULNERABLE({enemy.VulnerableTurnsRemaining})");
            if (enemy.SilencedTurnsRemaining > 0)
                statusEffects.Add($"SILENCED({enemy.SilencedTurnsRemaining})");

            // [v0.20] Add position info if grid is active
            if (combat.Grid != null && enemy.Position != null)
            {
                var pos = enemy.Position.Value;
                statusEffects.Add($"{pos.Row}/C{pos.Column}");
            }

            var statusText = statusEffects.Count > 0 ? $" [dim]({string.Join(", ", statusEffects)})[/]" : "";
            enemyTable.AddRow(new Markup($"{hpBar}{statusText}"));
        }

        AnsiConsole.Write(enemyTable);
        AnsiConsole.WriteLine();

        // [v0.20.2] Display battlefield cover locations
        if (combat.Grid != null)
        {
            DisplayBattlefieldCover(combat.Grid);
            DisplayBattlefieldGlitches(combat.Grid); // v0.20.3
        }

        // Initiative order indicator
        var currentParticipant = combat.CurrentParticipant;
        var currentName = currentParticipant.IsPlayer ? combat.Player.Name : ((Enemy)currentParticipant.Character!).Name;
        var turnColor = currentParticipant.IsPlayer ? "green" : "red";
        AnsiConsole.MarkupLine($"[{turnColor}]▶ {currentName}'s turn[/]");
        AnsiConsole.WriteLine();
    }

    /// <summary>
    /// v0.20.2: Displays cover locations on the battlefield
    /// Shows cover type, position, and health status
    /// </summary>
    private static void DisplayBattlefieldCover(BattlefieldGrid grid)
    {
        var coverTiles = grid.Tiles.Values
            .Where(t => t.Cover != CoverType.None)
            .OrderBy(t => t.Position.Zone)
            .ThenBy(t => t.Position.Row)
            .ThenBy(t => t.Position.Column)
            .ToList();

        if (coverTiles.Count == 0)
        {
            return; // No cover to display
        }

        var coverTable = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Cyan);

        coverTable.AddColumn("[bold cyan]Battlefield Cover[/]");

        foreach (var tile in coverTiles)
        {
            string icon = GetCoverIcon(tile.Cover);
            string description = tile.CoverDescription ?? "Cover";
            string position = $"{tile.Position.Zone} {tile.Position.Row} (Col {tile.Position.Column})";
            string health = "";

            if (tile.CoverHealth.HasValue)
            {
                // Color code health: green (16-20), yellow (11-15), orange (6-10), red (1-5)
                string healthColor = tile.CoverHealth >= 16 ? "green" :
                                   tile.CoverHealth >= 11 ? "yellow" :
                                   tile.CoverHealth >= 6 ? "orange" : "red";
                health = $" [{healthColor}]{tile.CoverHealth} HP[/]";
            }

            coverTable.AddRow($"{icon} {description} [dim]at {position}[/]{health}");
        }

        AnsiConsole.Write(coverTable);
        AnsiConsole.WriteLine();
    }

    /// <summary>
    /// v0.20.2: Returns icon/visual representation for cover type
    /// </summary>
    private static string GetCoverIcon(CoverType coverType)
    {
        return coverType switch
        {
            CoverType.Physical => "[blue]█[/]",        // Solid block for physical
            CoverType.Metaphysical => "[magenta]◆[/]", // Diamond for metaphysical
            CoverType.Both => "[cyan]◈[/]",            // Combined symbol
            _ => "[ ]"
        };
    }

    /// <summary>
    /// v0.20.3: Displays glitched tiles on the battlefield
    /// Shows glitch type, position, and DC
    /// </summary>
    private static void DisplayBattlefieldGlitches(BattlefieldGrid grid)
    {
        var glitchedTiles = grid.GetGlitchedTiles()
            .OrderBy(t => t.Position.Zone)
            .ThenBy(t => t.Position.Row)
            .ThenBy(t => t.Position.Column)
            .ToList();

        if (glitchedTiles.Count == 0)
        {
            return; // No glitched tiles to display
        }

        var glitchTable = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Red);

        glitchTable.AddColumn("[bold red]⚠ Reality Corruption Detected[/]");

        foreach (var tile in glitchedTiles)
        {
            if (tile.GlitchType == null)
                continue;

            string icon = GetGlitchIcon(tile.GlitchType.Value);
            string severityColor = GetGlitchSeverityColor(tile.GlitchSeverity);
            string description = GetGlitchDescription(tile.GlitchType.Value);
            string position = $"{tile.Position.Zone} {tile.Position.Row} (Col {tile.Position.Column})";
            int dc = 10 + (tile.GlitchSeverity * 2);

            glitchTable.AddRow($"{icon} [{severityColor}]{description}[/] [dim]at {position}[/] [yellow](DC {dc})[/]");
        }

        AnsiConsole.Write(glitchTable);
        AnsiConsole.WriteLine();
    }

    /// <summary>
    /// v0.20.3: Returns icon/visual representation for glitch type
    /// </summary>
    private static string GetGlitchIcon(Core.GlitchType glitchType)
    {
        return glitchType switch
        {
            Core.GlitchType.Flickering => "[yellow]~[/]",         // Wave for flickering
            Core.GlitchType.InvertedGravity => "[magenta]↕[/]",   // Up-down arrow for gravity
            Core.GlitchType.Looping => "[cyan]∞[/]",              // Infinity for loops
            _ => "[red]?[/]"
        };
    }

    /// <summary>
    /// v0.20.3: Returns color for glitch severity level
    /// </summary>
    private static string GetGlitchSeverityColor(int severity)
    {
        return severity switch
        {
            1 => "yellow",      // DC 12 - Minor hazard
            2 => "darkorange",  // DC 14 - Moderate hazard
            3 => "red",         // DC 16 - Severe hazard
            _ => "grey"
        };
    }

    /// <summary>
    /// v0.20.3: Returns descriptive text for glitch type with attribute requirement
    /// </summary>
    private static string GetGlitchDescription(Core.GlitchType glitchType)
    {
        return glitchType switch
        {
            Core.GlitchType.Flickering => "Flickering Platform (FINESSE)",
            Core.GlitchType.InvertedGravity => "Inverted Gravity (STURDINESS)",
            Core.GlitchType.Looping => "Looping Corridor (WITS)",
            _ => "Unknown Glitch"
        };
    }

    public static void DisplayCombatLog(List<string> logEntries, int maxEntries = 10)
    {
        if (logEntries.Count == 0) return;

        // Escape markup in log entries to prevent Spectre.Console from interpreting square brackets as markup tags
        var escapedEntries = logEntries.TakeLast(maxEntries).Select(entry => entry.EscapeMarkup());
        var panel = new Panel(string.Join("\n", escapedEntries))
        {
            Border = BoxBorder.Rounded,
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
            "Parry - Prepare to counter the next attack", // v0.21.4
            "Ability - Use a special ability",
            "Move - Advanced movement abilities", // v0.20.4
            "Stance - Change combat stance", // v0.21.1
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

    /// <summary>
    /// v0.21.1: Prompt player to select a combat stance
    /// </summary>
    public static string PromptStanceChoice(PlayerCharacter player)
    {
        var stanceService = new StanceService();
        var currentStance = player.ActiveStance?.Type ?? StanceType.Calculated;
        var availableStances = stanceService.GetAvailableStances();

        var choices = new List<string>();
        foreach (var (stanceType, name, description) in availableStances)
        {
            var currentIndicator = stanceType == currentStance ? " [green](CURRENT)[/]" : "";
            choices.Add($"{name}{currentIndicator} - {description}");
        }

        choices.Add("Cancel - Go back");

        var choice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title($"[yellow]Change Stance (Current: {stanceService.GetStanceName(currentStance)}):[/]")
                .PageSize(10)
                .AddChoices(choices)
        );

        if (choice.StartsWith("Cancel"))
        {
            return "cancel";
        }

        // Extract stance name (first word before " - ")
        var stanceName = choice.Split('-')[0].Trim();
        // Remove (CURRENT) marker if present
        stanceName = stanceName.Replace("[green]", "").Replace("(CURRENT)[/]", "").Trim();

        return stanceName.ToLower();
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

    /// <summary>
    /// v0.20.4: Prompt player to choose an advanced movement ability
    /// </summary>
    public static string PromptMovementChoice(PlayerCharacter player, BattlefieldGrid grid)
    {
        var choices = new List<string>();

        // Leap
        if (player.Stamina >= 20)
        {
            choices.Add($"Leap ([green]20 Stamina[/]) - Jump 2-3 tiles, bypass hazards (FINESSE check)");
        }
        else
        {
            choices.Add($"Leap ([red]20 Stamina[/]) - Jump 2-3 tiles, bypass hazards (FINESSE check)");
        }

        // Dash
        if (player.KineticEnergy >= 25 && player.Stamina >= 10)
        {
            choices.Add($"Dash ([green]25 KE + 10 Stamina[/]) - Rapid 3-tile straight movement (+10 KE bonus)");
        }
        else
        {
            choices.Add($"Dash ([red]25 KE + 10 Stamina[/]) - Rapid 3-tile straight movement (+10 KE bonus)");
        }

        // Blink (Mystic ability)
        if (player.AP >= 40)
        {
            choices.Add($"Blink ([green]40 AP[/]) - Teleport 2 tiles, bypass all hazards");
        }
        else
        {
            choices.Add($"Blink ([red]40 AP[/]) - Teleport 2 tiles, bypass all hazards");
        }

        // Climb
        if (player.Stamina >= 15)
        {
            choices.Add($"Climb ([green]15 Stamina[/]) - Reach high ground (FINESSE check, +2 Accuracy/+2 Defense)");
        }
        else
        {
            choices.Add($"Climb ([red]15 Stamina[/]) - Reach high ground (FINESSE check, +2 Accuracy/+2 Defense)");
        }

        // Safe Step
        if (player.Stamina >= 15)
        {
            var autoPass = player.Attributes.Wits >= 5 ? " (auto-pass)" : " (WITS check)";
            choices.Add($"Safe Step ([green]15 Stamina[/]) - Careful 1-tile movement, ignore glitches{autoPass}");
        }
        else
        {
            var autoPass = player.Attributes.Wits >= 5 ? " (auto-pass)" : " (WITS check)";
            choices.Add($"Safe Step ([red]15 Stamina[/]) - Careful 1-tile movement, ignore glitches{autoPass}");
        }

        choices.Add("Cancel - Go back");

        var choice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[yellow]Choose movement:[/]")
                .PageSize(10)
                .AddChoices(choices)
        );

        if (choice.StartsWith("Cancel"))
        {
            return "cancel";
        }

        // Extract movement type from choice
        if (choice.Contains("Leap")) return "leap";
        if (choice.Contains("Dash")) return "dash";
        if (choice.Contains("Blink")) return "blink";
        if (choice.Contains("Climb")) return "climb";
        if (choice.Contains("Safe Step")) return "safestep";

        return "cancel";
    }

    /// <summary>
    /// v0.20.4: Prompt player to select a target tile for movement
    /// </summary>
    public static GridPosition? PromptMovementTarget(BattlefieldGrid grid, GridPosition currentPosition)
    {
        AnsiConsole.MarkupLine("[yellow]Select target tile:[/]");
        AnsiConsole.MarkupLine($"[dim]Current position: {currentPosition}[/]");
        AnsiConsole.MarkupLine($"[dim]Enter target as: zone/row/column (e.g., Player/Front/2)[/]");
        AnsiConsole.MarkupLine($"[dim]Or type 'cancel' to go back[/]");
        AnsiConsole.WriteLine();

        var input = AnsiConsole.Ask<string>("[yellow]Target:[/]").Trim();

        if (input.Equals("cancel", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        // Parse input: zone/row/column
        var parts = input.Split('/');
        if (parts.Length != 3)
        {
            AnsiConsole.MarkupLine("[red]Invalid format! Use: zone/row/column[/]");
            return null;
        }

        // Parse zone
        if (!Enum.TryParse<Zone>(parts[0], true, out var zone))
        {
            AnsiConsole.MarkupLine("[red]Invalid zone! Use: Player or Enemy[/]");
            return null;
        }

        // Parse row
        if (!Enum.TryParse<Row>(parts[1], true, out var row))
        {
            AnsiConsole.MarkupLine("[red]Invalid row! Use: Front or Back[/]");
            return null;
        }

        // Parse column
        if (!int.TryParse(parts[2], out var column))
        {
            AnsiConsole.MarkupLine("[red]Invalid column! Must be a number[/]");
            return null;
        }

        var targetPosition = new GridPosition(zone, row, column, currentPosition.Elevation);

        // Validate position
        if (!grid.IsValidPosition(targetPosition))
        {
            AnsiConsole.MarkupLine("[red]Invalid position! Out of bounds[/]");
            return null;
        }

        return targetPosition;
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
        AnsiConsole.MarkupLine("[dim]Use 'equip [[item]]' to equip from inventory, 'compare [[item]]' to compare items.[/]");
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
            BorderStyle = new Style(comparison.IsUpgrade ? Color.Green : Color.Red),
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

    // ============================================================
    // SPECIALIZATION SUMMARY (v0.19)
    // ============================================================

    /// <summary>
    /// v0.19: Display specialization summary in character sheet
    /// Shows unlocked specs, learned abilities, and PP spending breakdown
    /// </summary>
    private static void DisplaySpecializationSummary(Table table, PlayerCharacter character, string connectionString)
    {
        var specializationService = new SpecializationService(connectionString);
        var abilityService = new AbilityService(connectionString);
        var specializationRepo = new SpecializationRepository(connectionString);

        // Get all unlocked specializations for this character
        var characterId = character.Name.GetHashCode();
        var unlockedSpecs = specializationRepo.GetUnlockedSpecializations(characterId);

        if (unlockedSpecs.Count == 0)
        {
            return; // No specializations unlocked, skip section
        }

        table.AddRow(new Markup(""));  // Spacer
        table.AddRow(new Markup("[bold]SPECIALIZATIONS[/]"));

        foreach (var charSpec in unlockedSpecs)
        {
            // Get the SpecializationData for this character specialization
            var specResult = specializationService.GetSpecialization(charSpec.SpecializationID);
            if (!specResult.Success || specResult.Specialization == null) continue;

            var spec = specResult.Specialization;

            // Get learned abilities count - filter by specialization
            var allLearnedAbilities = abilityService.GetLearnedAbilities(character);
            var specAbilities = abilityService.GetAbilitiesForSpecialization(charSpec.SpecializationID).Abilities ?? new List<AbilityData>();
            var specAbilityIds = specAbilities.Select(a => a.AbilityID).ToHashSet();
            var learnedAbilities = allLearnedAbilities.Where(ca => specAbilityIds.Contains(ca.AbilityID)).ToList();

            var totalAbilities = 9; // All specs have 9 abilities
            var ppSpent = specializationService.GetPPSpentInTree(character, charSpec.SpecializationID);

            // Determine current tier unlocked based on PP spent
            string tierProgress;
            if (ppSpent >= 24)
                tierProgress = "Capstone";
            else if (ppSpent >= 16)
                tierProgress = "Tier 3";
            else if (ppSpent >= 8)
                tierProgress = "Tier 2";
            else
                tierProgress = "Tier 1";

            // Build summary line
            var specLine = $"{spec.IconEmoji} [yellow]{spec.Name}[/] " +
                          $"[dim]({learnedAbilities.Count}/{totalAbilities} abilities)[/] " +
                          $"[cyan]{ppSpent} PP[/] [dim]• {tierProgress}[/]";

            table.AddRow(specLine);
        }

        table.AddRow(new Markup("[dim]Use 'specializations' to manage specializations and abilities[/]"));
    }

    /// <summary>
    /// v0.19: Display quick ability reference for combat
    /// Shows learned abilities with costs and availability
    /// </summary>
    public static void DisplayAbilityQuickReference(PlayerCharacter character, string connectionString)
    {
        var abilityService = new AbilityService(connectionString);
        var learnedAbilities = abilityService.GetLearnedAbilities(character);

        if (learnedAbilities.Count == 0)
        {
            return; // No abilities learned from new system yet
        }

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[bold yellow]SPECIALIZATION ABILITIES[/]");

        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Cyan1)
            .AddColumn("Ability")
            .AddColumn("Type")
            .AddColumn("Cost")
            .AddColumn("Summary");

        foreach (var learnedAbility in learnedAbilities)
        {
            var result = abilityService.GetAbility(learnedAbility.AbilityID);
            if (!result.Success || result.Ability == null) continue;

            var ability = result.Ability;
            var rank = abilityService.GetCurrentRank(character, learnedAbility.AbilityID);

            // Type icon
            var typeIcon = ability.AbilityType switch
            {
                "Active" => "⚔",
                "Passive" => "◈",
                "Reaction" => "⚡",
                _ => "•"
            };

            // Name with rank
            var nameDisplay = rank > 1
                ? $"[yellow]{ability.Name}[/] [cyan][R{rank}][/]"
                : $"[yellow]{ability.Name}[/]";

            // Resource cost
            var costDisplay = ability.AbilityType == "Passive"
                ? "[dim]Passive[/]"
                : $"[green]{ability.ResourceCost.Stamina} STA[/]";

            // Can afford check
            var canAfford = character.Stamina >= ability.ResourceCost.Stamina;
            if (!canAfford && ability.AbilityType != "Passive")
            {
                costDisplay = $"[red]{ability.ResourceCost.Stamina} STA[/]";
            }

            table.AddRow(
                nameDisplay,
                $"{typeIcon} {ability.AbilityType}",
                costDisplay,
                $"[dim]{ability.MechanicalSummary}[/]"
            );
        }

        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();
    }

    /// <summary>
    /// v0.19: Display PP spending breakdown across all specializations
    /// Useful for planning progression
    /// </summary>
    public static void DisplayPPBreakdown(PlayerCharacter character, string connectionString)
    {
        var specializationService = new SpecializationService(connectionString);
        var abilityService = new AbilityService(connectionString);
        var specializationRepo = new SpecializationRepository(connectionString);

        AnsiConsole.WriteLine();
        var rule = new Rule("[bold yellow]PROGRESSION POINT BREAKDOWN[/]")
        {
            Justification = Justify.Center
        };
        AnsiConsole.Write(rule);
        AnsiConsole.WriteLine();

        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Yellow)
            .AddColumn("Category")
            .AddColumn("PP Spent", c => c.RightAligned())
            .AddColumn("Details");

        // Get all unlocked specializations
        var characterId = character.Name.GetHashCode();
        var unlockedSpecs = specializationRepo.GetUnlockedSpecializations(characterId);

        int totalSpent = 0;

        // Specialization unlocks
        int unlockCost = unlockedSpecs.Count * 3; // 3 PP per specialization
        totalSpent += unlockCost;
        table.AddRow(
            "[yellow]Specialization Unlocks[/]",
            $"[red]{unlockCost}[/]",
            $"[dim]{unlockedSpecs.Count} × 3 PP[/]"
        );

        // Per-specialization breakdown
        foreach (var charSpec in unlockedSpecs)
        {
            // Get the SpecializationData for this character specialization
            var specResult = specializationService.GetSpecialization(charSpec.SpecializationID);
            if (!specResult.Success || specResult.Specialization == null) continue;

            var spec = specResult.Specialization;

            var ppSpent = specializationService.GetPPSpentInTree(character, charSpec.SpecializationID);
            // Filter learned abilities by specialization
            var allLearnedAbilities = abilityService.GetLearnedAbilities(character);
            var specAbilities = abilityService.GetAbilitiesForSpecialization(charSpec.SpecializationID).Abilities ?? new List<AbilityData>();
            var specAbilityIds = specAbilities.Select(a => a.AbilityID).ToHashSet();
            var learnedCount = allLearnedAbilities.Count(ca => specAbilityIds.Contains(ca.AbilityID));

            totalSpent += ppSpent;

            table.AddRow(
                $"{spec.IconEmoji} [yellow]{spec.Name}[/]",
                $"[red]{ppSpent}[/]",
                $"[dim]{learnedCount} abilities learned[/]"
            );
        }

        // Summary
        table.AddRow("", "", "");
        table.AddRow(
            "[bold]TOTAL SPENT[/]",
            $"[bold red]{totalSpent}[/]",
            ""
        );
        table.AddRow(
            "[bold]AVAILABLE[/]",
            $"[bold green]{character.ProgressionPoints}[/]",
            ""
        );

        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[dim]Progression Points are earned by completing milestones and achieving Legend.[/]");
        AnsiConsole.WriteLine();
    }

    // ============================================================
    // v0.20.5: Party Formation UI
    // ============================================================

    /// <summary>
    /// Displays formation selection options to the player
    /// </summary>
    public static FormationType PromptFormationSelection()
    {
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[bold yellow]═══ Formation Selection ═══[/]");
        AnsiConsole.WriteLine();

        var choices = new List<string>
        {
            "Line Formation (Defensive) - +1 Defense for front row",
            "Wedge Formation (Aggressive) - +1 Accuracy for front row",
            "Scattered Formation (Tactical) - Harder to flank",
            "Protect Formation (Support) - +2 Defense for back-row center"
        };

        var choice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[yellow]Choose your formation:[/]")
                .PageSize(10)
                .AddChoices(choices)
        );

        if (choice.Contains("Line"))
            return FormationType.Line;
        if (choice.Contains("Wedge"))
            return FormationType.Wedge;
        if (choice.Contains("Scattered"))
            return FormationType.Scattered;
        if (choice.Contains("Protect"))
            return FormationType.Protect;

        return FormationType.Line; // Default
    }

    /// <summary>
    /// Displays the current party formation on the grid
    /// </summary>
    public static void DisplayPartyFormation(List<object> party, BattlefieldGrid grid)
    {
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[bold yellow]═══ Party Formation ═══[/]");
        AnsiConsole.WriteLine();

        // Display grid visualization
        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Blue);

        table.AddColumn(new TableColumn("[bold]Zone[/]").Centered());
        table.AddColumn(new TableColumn("[bold]Row[/]").Centered());

        for (int col = 0; col < grid.Columns; col++)
        {
            table.AddColumn(new TableColumn($"[bold]Col {col}[/]").Centered());
        }

        // Display Player zone
        for (int rowIndex = 0; rowIndex < 2; rowIndex++)
        {
            var row = (Row)rowIndex;
            var rowCells = new List<string>
            {
                rowIndex == 0 ? "[cyan]Player[/]" : "",
                $"[dim]{row}[/]"
            };

            for (int col = 0; col < grid.Columns; col++)
            {
                var pos = new GridPosition(Zone.Player, row, col);
                var tile = grid.GetTile(pos);

                if (tile != null && tile.IsOccupied)
                {
                    // Find the party member at this position
                    var member = party.FirstOrDefault(m =>
                    {
                        var memberPos = m switch
                        {
                            PlayerCharacter player => player.Position,
                            Enemy enemy => enemy.Position,
                            _ => null
                        };
                        return memberPos.HasValue && memberPos.Value == pos;
                    });

                    if (member != null)
                    {
                        var icon = GetFormationIcon(member, row);
                        var name = member switch
                        {
                            PlayerCharacter player => player.Name.Substring(0, Math.Min(3, player.Name.Length)),
                            Enemy enemy => enemy.Name.Substring(0, Math.Min(3, enemy.Name.Length)),
                            _ => "?"
                        };
                        rowCells.Add($"[green]{icon}:{name}[/]");
                    }
                    else
                    {
                        rowCells.Add("[dim][ ][/]");
                    }
                }
                else
                {
                    rowCells.Add("[dim][ ][/]");
                }
            }

            table.AddRow(rowCells.ToArray());
        }

        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[dim]T=Tank/Front, S=Support/Back, R=Ranged, D=Damage[/]");
        AnsiConsole.WriteLine();
    }

    /// <summary>
    /// Gets a formation icon for a party member based on their position
    /// </summary>
    private static string GetFormationIcon(object combatant, Row row)
    {
        if (row == Row.Front)
        {
            return "T"; // Tank/Front-line
        }
        else
        {
            // Back row
            if (combatant is PlayerCharacter player)
            {
                // Check class/archetype
                if (player.Class == CharacterClass.Mystic)
                    return "S"; // Support
                else
                    return "R"; // Ranged
            }
            return "R"; // Default to Ranged
        }
    }

    /// <summary>
    /// Displays formation bonuses applied to party members
    /// </summary>
    public static void DisplayFormationBonuses(List<FormationBonus> bonuses, FormationType formation)
    {
        if (bonuses == null || bonuses.Count == 0)
            return;

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine($"[bold yellow]Formation Bonuses ({formation}):[/]");

        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Green);

        table.AddColumn(new TableColumn("[bold]Member[/]"));
        table.AddColumn(new TableColumn("[bold]Bonus Type[/]"));
        table.AddColumn(new TableColumn("[bold]Amount[/]"));
        table.AddColumn(new TableColumn("[bold]Description[/]"));

        foreach (var bonus in bonuses)
        {
            var memberName = bonus.Target switch
            {
                PlayerCharacter player => player.Name,
                Enemy enemy => enemy.Name,
                _ => "Unknown"
            };

            var bonusColor = bonus.Type switch
            {
                BonusType.Defense => "blue",
                BonusType.Accuracy => "yellow",
                BonusType.Damage => "red",
                BonusType.AntiFlanking => "cyan",
                _ => "white"
            };

            table.AddRow(
                $"[green]{memberName}[/]",
                $"[{bonusColor}]{bonus.Type}[/]",
                $"[bold]+{bonus.Amount}[/]",
                $"[dim]{bonus.Description}[/]"
            );
        }

        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();
    }

    /// <summary>
    /// Displays tactical grid with party and enemy positions (v0.20.5 enhanced)
    /// </summary>
    public static void DisplayTacticalGrid(BattlefieldGrid grid, List<object> party, List<Enemy> enemies)
    {
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[bold yellow]═══ Tactical Grid ═══[/]");
        AnsiConsole.WriteLine();

        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Grey);

        table.AddColumn(new TableColumn("[bold]Zone[/]").Centered());
        table.AddColumn(new TableColumn("[bold]Row[/]").Centered());

        for (int col = 0; col < grid.Columns; col++)
        {
            table.AddColumn(new TableColumn($"[bold]{col}[/]").Centered());
        }

        // Display both zones
        foreach (Zone zone in new[] { Zone.Enemy, Zone.Player })
        {
            for (int rowIndex = 0; rowIndex < 2; rowIndex++)
            {
                var row = (Row)rowIndex;
                var rowCells = new List<string>
                {
                    rowIndex == 0 ? (zone == Zone.Player ? "[cyan]Player[/]" : "[red]Enemy[/]") : "",
                    $"[dim]{row}[/]"
                };

                for (int col = 0; col < grid.Columns; col++)
                {
                    var pos = new GridPosition(zone, row, col);
                    var tile = grid.GetTile(pos);

                    if (tile != null)
                    {
                        var cellContent = GetGridCellDisplay(tile, party, enemies, pos);
                        rowCells.Add(cellContent);
                    }
                    else
                    {
                        rowCells.Add("[dim][ ][/]");
                    }
                }

                table.AddRow(rowCells.ToArray());
            }

            // Add separator between zones
            if (zone == Zone.Enemy)
            {
                var separator = Enumerable.Repeat("[dim]───[/]", grid.Columns + 2).ToArray();
                table.AddRow(separator);
            }
        }

        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();
    }

    /// <summary>
    /// Gets the display content for a grid cell
    /// </summary>
    private static string GetGridCellDisplay(BattlefieldTile tile, List<object> party, List<Enemy> enemies, GridPosition pos)
    {
        var content = new List<string>();

        // Check for occupant
        if (tile.IsOccupied)
        {
            // Check party members
            var partyMember = party.FirstOrDefault(m =>
            {
                var memberPos = m switch
                {
                    PlayerCharacter player => player.Position,
                    Enemy enemy => enemy.Position,
                    _ => null
                };
                return memberPos.HasValue && memberPos.Value == pos;
            });

            if (partyMember != null)
            {
                var name = partyMember switch
                {
                    PlayerCharacter player => player.Name.Substring(0, Math.Min(1, player.Name.Length)),
                    _ => "P"
                };
                content.Add($"[green]{name}[/]");
            }
            else
            {
                // Check enemies
                var enemy = enemies.FirstOrDefault(e => e.Position.HasValue && e.Position.Value == pos);
                if (enemy != null)
                {
                    var name = enemy.Name.Substring(0, Math.Min(1, enemy.Name.Length));
                    content.Add($"[red]{name}[/]");
                }
                else
                {
                    content.Add("[yellow]?[/]");
                }
            }
        }

        // Check for cover
        if (tile.Cover != CoverType.None)
        {
            var coverIcon = tile.Cover switch
            {
                CoverType.Physical => "▓",
                CoverType.Metaphysical => "▒",
                CoverType.Both => "█",
                _ => ""
            };
            content.Add($"[blue]{coverIcon}[/]");
        }

        // Check for glitch
        if (tile.Type == TileType.Glitched)
        {
            content.Add("[magenta]⚠[/]");
        }

        // Check for high ground
        if (tile.Type == TileType.HighGround)
        {
            content.Add("[cyan]↑[/]");
        }

        if (content.Count == 0)
        {
            return "[dim][ ][/]";
        }

        return string.Join("", content);
    }
}
