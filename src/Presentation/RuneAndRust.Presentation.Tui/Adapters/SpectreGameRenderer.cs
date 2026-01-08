using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using RuneAndRust.Application.DTOs;
using RuneAndRust.Application.Interfaces;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace RuneAndRust.Presentation.Tui.Adapters;

/// <summary>
/// Spectre.Console-based implementation of <see cref="IGameRenderer"/> for TUI output.
/// </summary>
/// <remarks>
/// SpectreGameRenderer uses Spectre.Console's rich formatting capabilities to render
/// colorful, styled game output including status bars, room descriptions, inventory
/// tables, and combat results.
/// </remarks>
public class SpectreGameRenderer : IGameRenderer
{
    /// <summary>
    /// Logger for rendering operations and diagnostics.
    /// </summary>
    private readonly ILogger<SpectreGameRenderer> _logger;

    /// <summary>
    /// Creates a new Spectre game renderer instance.
    /// </summary>
    /// <param name="logger">Optional logger for diagnostics. If null, a no-op logger is used.</param>
    public SpectreGameRenderer(ILogger<SpectreGameRenderer>? logger = null)
    {
        _logger = logger ?? NullLogger<SpectreGameRenderer>.Instance;
        _logger.LogDebug("SpectreGameRenderer initialized");
    }

    /// <inheritdoc/>
    public Task RenderGameStateAsync(GameStateDto gameState, CancellationToken ct = default)
    {
        _logger.LogDebug(
            "Rendering game state - Player: {PlayerName}, Room: {RoomName}, State: {GameState}",
            gameState.Player.Name,
            gameState.CurrentRoom.Name,
            gameState.State);

        RenderStatusBar(gameState.Player);
        Console.WriteLine();
        RenderRoomInternal(gameState.CurrentRoom);

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task RenderMessageAsync(string message, MessageType type = MessageType.Info, CancellationToken ct = default)
    {
        _logger.LogDebug("Rendering message - Type: {MessageType}, Message: {Message}", type, message);

        var color = type switch
        {
            MessageType.Warning => "yellow",
            MessageType.Error => "red",
            MessageType.Success => "green",
            MessageType.Combat => "orange1",
            MessageType.Narrative => "italic grey",
            _ => "white"
        };

        AnsiConsole.MarkupLine($"[{color}]{Markup.Escape(message)}[/]");
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task RenderRoomAsync(RoomDto room, CancellationToken ct = default)
    {
        _logger.LogDebug(
            "Rendering room: {RoomName}, Items: {ItemCount}, Monsters: {MonsterCount}, Exits: {ExitCount}",
            room.Name,
            room.Items.Count,
            room.Monsters.Count,
            room.Exits.Count);

        RenderRoomInternal(room);
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task RenderInventoryAsync(InventoryDto inventory, CancellationToken ct = default)
    {
        _logger.LogDebug("Rendering inventory: {ItemCount}/{Capacity} items", inventory.Count, inventory.Capacity);

        if (inventory.Items.Count == 0)
        {
            AnsiConsole.MarkupLine("[grey]Your inventory is empty.[/]");
            return Task.CompletedTask;
        }

        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Grey)
            .Title("[yellow]Inventory[/]")
            .AddColumn(new TableColumn("[cyan]Item[/]"))
            .AddColumn(new TableColumn("[grey]Type[/]"))
            .AddColumn(new TableColumn("[grey]Description[/]"));

        foreach (var item in inventory.Items)
        {
            table.AddRow(
                $"[white]{Markup.Escape(item.Name)}[/]",
                $"[grey]{item.Type}[/]",
                $"[grey]{Markup.Escape(item.Description)}[/]"
            );
        }

        AnsiConsole.Write(table);
        AnsiConsole.MarkupLine($"[grey]Capacity: {inventory.Count}/{inventory.Capacity}[/]");

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task RenderCombatResultAsync(string combatDescription, CancellationToken ct = default)
    {
        _logger.LogDebug("Rendering combat result");

        var panel = new Panel(combatDescription)
            .Header("[red]Combat[/]")
            .Border(BoxBorder.Heavy)
            .BorderColor(Color.Red);

        AnsiConsole.Write(panel);
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task RenderExamineResultAsync(ExamineResultDto result, CancellationToken ct = default)
    {
        _logger.LogDebug("Rendering examine result for: {Name} (Type: {Type})", result.Name, result.Type);

        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Cyan1)
            .Title($"[cyan]{Markup.Escape(result.Name)}[/] [grey]({result.Type})[/]")
            .AddColumn(new TableColumn("[grey]Property[/]"))
            .AddColumn(new TableColumn("[white]Value[/]"));

        table.AddRow("[white]Description[/]", Markup.Escape(result.Description));

        foreach (var prop in result.Properties)
        {
            table.AddRow($"[grey]{Markup.Escape(prop.Key)}[/]", Markup.Escape(prop.Value));
        }

        AnsiConsole.Write(table);
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task RenderPlayerStatsAsync(PlayerStatsDto stats, CancellationToken ct = default)
    {
        _logger.LogDebug("Rendering player stats for: {Name}", stats.Name);

        var healthColor = stats.HealthPercentage > 0.5 ? "green" :
                          stats.HealthPercentage > 0.25 ? "yellow" : "red";

        var barColor = stats.HealthPercentage > 0.5 ? Color.Green :
                       stats.HealthPercentage > 0.25 ? Color.Yellow : Color.Red;

        var healthBar = new BarChart()
            .Width(30)
            .AddItem("[white]HP[/]", (int)(stats.HealthPercentage * 100), barColor);

        var panel = new Panel(
            new Rows(
                new Markup($"[bold yellow]{Markup.Escape(stats.Name)}[/]"),
                new Text(""),
                new Markup($"[{healthColor}]Health: {stats.Health}/{stats.MaxHealth}[/]"),
                healthBar,
                new Text(""),
                new Markup($"[cyan]Attack: {stats.Attack}[/]"),
                new Markup($"[blue]Defense: {stats.Defense}[/]"),
                new Text(""),
                new Markup($"[grey]Location: {Markup.Escape(stats.CurrentRoomName)} ({stats.PositionX},{stats.PositionY})[/]"),
                new Markup($"[grey]Inventory: {stats.InventoryCount}/{stats.InventoryCapacity}[/]")
            ))
            .Header("[yellow]Character Status[/]")
            .Border(BoxBorder.Rounded)
            .BorderColor(Color.Yellow);

        AnsiConsole.Write(panel);
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task RenderAbilitiesAsync(IReadOnlyList<PlayerAbilityDto> abilities, CancellationToken ct = default)
    {
        _logger.LogDebug("Rendering abilities: {AbilityCount} abilities", abilities.Count);

        if (abilities.Count == 0)
        {
            AnsiConsole.MarkupLine("[grey]You have no abilities.[/]");
            return Task.CompletedTask;
        }

        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Purple)
            .Title("[purple]Abilities[/]")
            .AddColumn(new TableColumn("[cyan]Name[/]"))
            .AddColumn(new TableColumn("[grey]Cost[/]"))
            .AddColumn(new TableColumn("[grey]Cooldown[/]"))
            .AddColumn(new TableColumn("[grey]Status[/]"))
            .AddColumn(new TableColumn("[grey]Description[/]"));

        foreach (var ability in abilities)
        {
            var statusColor = ability.IsReady ? "green" :
                              ability.IsOnCooldown ? "yellow" :
                              !ability.CanAfford ? "red" :
                              !ability.IsUnlocked ? "grey" : "white";

            var status = ability.IsReady ? "Ready" :
                         ability.IsOnCooldown ? $"CD: {ability.CurrentCooldown}" :
                         !ability.CanAfford ? "No resource" :
                         !ability.IsUnlocked ? $"Lvl {ability.UnlockLevel}" : "Ready";

            var cost = ability.CostAmount > 0
                ? $"{ability.CostAmount} {ability.CostResourceTypeId}"
                : "Free";

            var cooldown = ability.Cooldown > 0 ? $"{ability.Cooldown} turns" : "None";

            table.AddRow(
                $"[white]{Markup.Escape(ability.Name)}[/]",
                $"[cyan]{Markup.Escape(cost)}[/]",
                $"[grey]{cooldown}[/]",
                $"[{statusColor}]{status}[/]",
                $"[grey]{Markup.Escape(ability.Description)}[/]"
            );
        }

        AnsiConsole.Write(table);
        AnsiConsole.MarkupLine("[grey]Use 'cast <ability name>' to use an ability.[/]");

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task RenderTurnEndChangesAsync(TurnEndResult changes, CancellationToken ct = default)
    {
        _logger.LogDebug(
            "Rendering turn-end changes: {ResourceChanges} resource changes, {CooldownChanges} cooldown changes",
            changes.ResourceChanges.Count, changes.CooldownChanges.Count);

        if (!changes.HasChanges)
        {
            return Task.CompletedTask;
        }

        Console.WriteLine();

        // Resource regeneration/decay messages
        foreach (var resourceChange in changes.ResourceChanges)
        {
            var delta = resourceChange.Delta;
            var sign = delta > 0 ? "+" : "";
            var color = delta > 0 ? "green" : "yellow";
            var changeTypeDesc = resourceChange.ChangeType.ToLower();

            AnsiConsole.MarkupLine(
                $"[{color}]{resourceChange.ResourceName} {changeTypeDesc}: {sign}{delta} ({resourceChange.NewValue}/{resourceChange.MaxValue})[/]");
        }

        // Abilities now ready notifications
        foreach (var abilityName in changes.AbilitiesNowReady)
        {
            AnsiConsole.MarkupLine($"[cyan]{Markup.Escape(abilityName)} is now ready![/]");
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task RenderDiceRollAsync(DiceRollDto roll, CancellationToken ct = default)
    {
        _logger.LogDebug("Rendering dice roll: {Notation}, Total: {Total}", roll.Notation, roll.Total);

        // Build roll visualization
        var rollsStr = string.Join(", ", roll.Rolls.Select(r =>
        {
            if (roll.Rolls.Count == 1 && roll.IsNaturalMax) return $"[bold green]{r}[/]";
            if (roll.Rolls.Count == 1 && roll.IsNaturalOne) return $"[bold red]{r}[/]";
            return r.ToString();
        }));

        var explosionsStr = roll.HadExplosions && roll.ExplosionRolls.Count > 0
            ? $" + [yellow]💥{string.Join(", ", roll.ExplosionRolls)}[/]"
            : "";

        var modifierStr = roll.Modifier != 0
            ? $" [grey]{(roll.Modifier > 0 ? "+" : "")}{roll.Modifier}[/]"
            : "";

        var advantageStr = roll.AdvantageType != "Normal"
            ? $" [cyan]({roll.AdvantageType})[/]"
            : "";

        var totalColor = roll.IsNaturalMax ? "bold green" :
                         roll.IsNaturalOne ? "bold red" : "white";

        var content = new Rows(
            new Markup($"[grey]Dice:[/] [cyan]{Markup.Escape(roll.Notation)}[/]{advantageStr}"),
            new Markup($"[grey]Roll:[/] [{rollsStr}]{explosionsStr}{modifierStr}"),
            new Markup($"[grey]Total:[/] [{totalColor}]{roll.Total}[/]")
        );

        var panel = new Panel(content)
            .Header("[cyan]Dice Roll[/]")
            .Border(BoxBorder.Rounded)
            .BorderColor(roll.IsNaturalMax ? Color.Green : roll.IsNaturalOne ? Color.Red : Color.Cyan1);

        AnsiConsole.Write(panel);

        if (!string.IsNullOrEmpty(roll.Descriptor))
        {
            AnsiConsole.MarkupLine($"[italic grey]{Markup.Escape(roll.Descriptor)}[/]");
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task RenderSkillCheckAsync(SkillCheckResultDto result, CancellationToken ct = default)
    {
        _logger.LogDebug("Rendering skill check: {Skill} - {SuccessLevel}", result.SkillName, result.SuccessLevel);

        var resultColor = result.IsSuccess ? "green" : "red";
        var critColor = result.IsCritical ? (result.IsSuccess ? "bold green" : "bold red") : resultColor;

        var rollStr = string.Join(", ", result.DiceRoll.Rolls);
        var bonusStr = result.AttributeBonus >= 0 ? $"+{result.AttributeBonus}" : result.AttributeBonus.ToString();
        var otherBonusStr = result.OtherBonus != 0
            ? (result.OtherBonus > 0 ? $"+{result.OtherBonus}" : result.OtherBonus.ToString())
            : "";

        var marginSign = result.Margin >= 0 ? "+" : "";
        var marginStr = $"({marginSign}{result.Margin})";

        var content = new Rows(
            new Markup($"[grey]Skill:[/] [cyan]{Markup.Escape(result.SkillName)}[/]"),
            new Markup($"[grey]Roll:[/] [{rollStr}] {bonusStr}{otherBonusStr} = [white]{result.TotalResult}[/]"),
            new Markup($"[grey]DC:[/] [yellow]{result.DifficultyClass}[/] ({Markup.Escape(result.DifficultyName)})"),
            new Markup($"[grey]Result:[/] [{critColor}]{result.SuccessLevel}[/] [{resultColor}]{marginStr}[/]")
        );

        var borderColor = result.IsCritical
            ? (result.IsSuccess ? Color.Green : Color.Red)
            : (result.IsSuccess ? Color.Cyan1 : Color.Orange1);

        var panel = new Panel(content)
            .Header($"[{(result.IsSuccess ? "green" : "red")}]Skill Check[/]")
            .Border(BoxBorder.Rounded)
            .BorderColor(borderColor);

        AnsiConsole.Write(panel);

        if (!string.IsNullOrEmpty(result.Descriptor))
        {
            AnsiConsole.MarkupLine($"[italic grey]{Markup.Escape(result.Descriptor)}[/]");
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task RenderCombatRoundAsync(CombatRoundResultDto result, CancellationToken ct = default)
    {
        _logger.LogDebug("Rendering combat round: Hit={IsHit}, Critical={IsCrit}", result.IsHit, result.IsCriticalHit);

        // Player attack section
        var attackRollStr = string.Join(", ", result.AttackRoll.Rolls);
        var attackColor = result.IsCriticalHit ? "bold green" :
                          result.IsCriticalMiss ? "bold red" :
                          result.IsHit ? "green" : "red";

        var rows = new List<IRenderable>
        {
            new Markup($"[bold cyan]Your Attack[/]"),
            new Markup($"[grey]Roll:[/] [{attackRollStr}] = [white]{result.AttackTotal}[/]"),
            new Markup($"[grey]Result:[/] [{attackColor}]{result.AttackSuccessLevel}[/]")
        };

        if (result.IsHit && result.DamageRoll != null)
        {
            var damageRollStr = string.Join(", ", result.DamageRoll.Rolls);
            rows.Add(new Markup($"[grey]Damage:[/] [{damageRollStr}] = [orange1]{result.DamageDealt}[/]"));
        }

        // Monster counterattack section
        if (result.MonsterCounterAttack != null)
        {
            var counter = result.MonsterCounterAttack;
            var counterRollStr = string.Join(", ", counter.AttackRoll.Rolls);
            var counterColor = counter.IsCriticalHit ? "bold red" :
                               counter.IsCriticalMiss ? "bold green" :
                               counter.IsHit ? "red" : "green";

            rows.Add(new Text(""));
            rows.Add(new Markup($"[bold red]Enemy Counterattack[/]"));
            rows.Add(new Markup($"[grey]Roll:[/] [{counterRollStr}] = [white]{counter.AttackTotal}[/]"));
            rows.Add(new Markup($"[grey]Result:[/] [{counterColor}]{counter.AttackSuccessLevel}[/]"));

            if (counter.IsHit && counter.DamageRoll != null)
            {
                var counterDamageStr = string.Join(", ", counter.DamageRoll.Rolls);
                rows.Add(new Markup($"[grey]Damage:[/] [{counterDamageStr}] = [red]{counter.DamageDealt}[/]"));
            }
        }

        // Round summary
        rows.Add(new Text(""));
        if (result.MonsterDefeated)
        {
            rows.Add(new Markup("[bold green]Enemy defeated![/]"));
        }
        else if (result.PlayerDefeated)
        {
            rows.Add(new Markup("[bold red]You have been defeated![/]"));
        }

        var panel = new Panel(new Rows(rows))
            .Header("[red]Combat Round[/]")
            .Border(BoxBorder.Heavy)
            .BorderColor(Color.Red);

        AnsiConsole.Write(panel);

        if (!string.IsNullOrEmpty(result.Descriptor))
        {
            AnsiConsole.MarkupLine($"[italic grey]{Markup.Escape(result.Descriptor)}[/]");
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task ClearScreenAsync(CancellationToken ct = default)
    {
        _logger.LogDebug("Clearing screen");
        AnsiConsole.Clear();
        return Task.CompletedTask;
    }

    // ===== Multi-Monster Combat Display (v0.0.6a) =====

    /// <inheritdoc/>
    public Task RenderCombatStartAsync(int monsterCount, CancellationToken ct = default)
    {
        _logger.LogDebug("Rendering combat start: {MonsterCount} monsters", monsterCount);

        var enemyText = monsterCount == 1 ? "enemy" : "enemies";
        var panel = new Panel($"[white]You are ambushed by {monsterCount} {enemyText}![/]")
            .Header("[red bold]⚔ COMBAT INITIATED ⚔[/]")
            .Border(BoxBorder.Double)
            .BorderColor(Color.Red);

        AnsiConsole.Write(panel);
        Console.WriteLine();

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task RenderInitiativeAsync(IReadOnlyList<CombatantInitiativeDto> initiatives, CancellationToken ct = default)
    {
        _logger.LogDebug("Rendering initiative for {Count} combatants", initiatives.Count);

        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Cyan1)
            .Title("[cyan]⚡ Initiative Order ⚡[/]")
            .AddColumn(new TableColumn("[grey]#[/]").Centered())
            .AddColumn(new TableColumn("[white]Combatant[/]"))
            .AddColumn(new TableColumn("[grey]Roll[/]").Centered())
            .AddColumn(new TableColumn("[grey]Mod[/]").Centered())
            .AddColumn(new TableColumn("[cyan]Total[/]").Centered());

        var position = 1;
        foreach (var init in initiatives)
        {
            var nameColor = init.IsPlayer ? "yellow" : "red";
            var modStr = init.Modifier >= 0 ? $"+{init.Modifier}" : init.Modifier.ToString();

            table.AddRow(
                $"[grey]{position}[/]",
                $"[{nameColor}]{Markup.Escape(init.Name)}[/]",
                $"[white]{init.RollValue}[/]",
                $"[grey]{modStr}[/]",
                $"[cyan bold]{init.Total}[/]"
            );
            position++;
        }

        AnsiConsole.Write(table);
        Console.WriteLine();

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task RenderCombatStateAsync(CombatStateDisplayDto combatState, CancellationToken ct = default)
    {
        _logger.LogDebug("Rendering combat state: Round {Round}", combatState.RoundNumber);

        // Round header
        AnsiConsole.MarkupLine($"[grey]═══[/] [yellow bold]Round {combatState.RoundNumber}[/] [grey]═══[/]");
        Console.WriteLine();

        // Turn order panel
        var turnOrderRows = new List<IRenderable>();
        for (var i = 0; i < combatState.TurnOrder.Count; i++)
        {
            var entry = combatState.TurnOrder[i];
            var marker = entry.IsCurrentTurn ? "►" : " ";
            var nameColor = entry.IsDefeated ? "grey strikethrough" :
                            entry.IsPlayer ? "yellow" : "red";
            var initText = $"[grey]({entry.Initiative})[/]";
            turnOrderRows.Add(new Markup($"[cyan]{marker}[/] [{nameColor}]{Markup.Escape(entry.Name)}[/] {initText}"));
        }

        var turnPanel = new Panel(new Rows(turnOrderRows))
            .Header("[cyan]Turn Order[/]")
            .Border(BoxBorder.Rounded)
            .BorderColor(Color.Cyan1);

        // Enemy status panel
        var enemyRows = new List<IRenderable>();
        foreach (var enemy in combatState.Enemies)
        {
            var healthPct = enemy.MaxHealth > 0 ? (float)enemy.CurrentHealth / enemy.MaxHealth : 0;
            var healthColor = healthPct > 0.5 ? "green" : healthPct > 0.25 ? "yellow" : "red";
            var statusText = enemy.IsDefeated ? "[grey strikethrough]Defeated[/]" :
                $"[{healthColor}]{enemy.CurrentHealth}/{enemy.MaxHealth}[/]";

            enemyRows.Add(new Markup($"[grey][[{enemy.Number}]][/] [red]{Markup.Escape(enemy.DisplayName)}[/] {statusText}"));
        }

        var enemyPanel = new Panel(new Rows(enemyRows))
            .Header("[red]Enemies[/]")
            .Border(BoxBorder.Rounded)
            .BorderColor(Color.Red);

        // Render side by side
        var columns = new Columns(turnPanel, enemyPanel);
        AnsiConsole.Write(columns);

        // Current turn indicator
        if (combatState.IsPlayerTurn)
        {
            AnsiConsole.MarkupLine("[yellow]It's your turn! Choose your action.[/]");
        }
        else
        {
            var currentName = combatState.TurnOrder[combatState.CurrentTurnIndex].Name;
            AnsiConsole.MarkupLine($"[red]{Markup.Escape(currentName)}'s turn...[/]");
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task RenderCombatEndAsync(CombatEndResultDto result, CancellationToken ct = default)
    {
        _logger.LogDebug("Rendering combat end: {State}, Rounds: {Rounds}", result.EndState, result.RoundsElapsed);

        var (headerText, borderColor, messageText) = result.EndState switch
        {
            Domain.Enums.CombatState.Victory => (
                "[green bold]⚔ VICTORY ⚔[/]",
                Color.Green,
                $"[green]You defeated {result.MonstersDefeated} enemies in {result.RoundsElapsed} rounds![/]"
            ),
            Domain.Enums.CombatState.PlayerDefeated => (
                "[red bold]☠ DEFEATED ☠[/]",
                Color.Red,
                "[red]You have fallen in combat...[/]"
            ),
            Domain.Enums.CombatState.Fled => (
                "[yellow bold]🏃 FLED 🏃[/]",
                Color.Yellow,
                "[yellow]You fled from combat, returning to the previous room.[/]"
            ),
            _ => (
                "[grey]Combat Ended[/]",
                Color.Grey,
                "[grey]Combat has ended.[/]"
            )
        };

        var panel = new Panel(messageText)
            .Header(headerText)
            .Border(BoxBorder.Double)
            .BorderColor(borderColor);

        Console.WriteLine();
        AnsiConsole.Write(panel);
        Console.WriteLine();

        return Task.CompletedTask;
    }

    /// <summary>
    /// Renders the player status bar showing name, health, attack, and defense.
    /// </summary>
    /// <param name="player">The player DTO to render.</param>
    private static void RenderStatusBar(PlayerDto player)
    {
        var healthColor = player.Health > player.MaxHealth * 0.5 ? "green" :
                          player.Health > player.MaxHealth * 0.25 ? "yellow" : "red";

        var statusTable = new Table()
            .Border(TableBorder.None)
            .HideHeaders()
            .AddColumn("")
            .AddColumn("")
            .AddColumn("")
            .AddColumn("");

        statusTable.AddRow(
            $"[white]{Markup.Escape(player.Name)}[/]",
            $"[{healthColor}]HP: {player.Health}/{player.MaxHealth}[/]",
            $"[cyan]ATK: {player.Attack}[/]",
            $"[blue]DEF: {player.Defense}[/]"
        );

        AnsiConsole.Write(statusTable);
    }

    /// <summary>
    /// Renders room details including name, description, monsters, items, and exits.
    /// </summary>
    /// <param name="room">The room DTO to render.</param>
    private static void RenderRoomInternal(RoomDto room)
    {
        // Room title
        var rule = new Rule($"[yellow]{Markup.Escape(room.Name)}[/]")
        {
            Justification = Justify.Center,
            Style = Style.Parse("yellow")
        };
        AnsiConsole.Write(rule);

        // Room description
        AnsiConsole.MarkupLine($"[white]{Markup.Escape(room.Description)}[/]");
        Console.WriteLine();

        // Monsters
        var aliveMonsters = room.Monsters.Where(m => m.IsAlive).ToList();
        if (aliveMonsters.Count > 0)
        {
            AnsiConsole.MarkupLine("[red]Enemies here:[/]");
            foreach (var monster in aliveMonsters)
            {
                var healthPct = monster.MaxHealth > 0 ? (double)monster.Health / monster.MaxHealth : 0;
                var healthColor = healthPct > 0.5 ? "green" : healthPct > 0.25 ? "yellow" : "red";
                AnsiConsole.MarkupLine($"  [red]* {Markup.Escape(monster.Name)}[/] [{healthColor}](HP: {monster.Health}/{monster.MaxHealth})[/]");
            }
            Console.WriteLine();
        }

        // Items
        if (room.Items.Count > 0)
        {
            AnsiConsole.MarkupLine("[cyan]Items here:[/]");
            foreach (var item in room.Items)
            {
                AnsiConsole.MarkupLine($"  [cyan]* {Markup.Escape(item.Name)}[/]");
            }
            Console.WriteLine();
        }

        // Exits
        if (room.Exits.Count > 0)
        {
            var exits = string.Join(", ", room.Exits.Select(e => e.ToString().ToLower()));
            AnsiConsole.MarkupLine($"[grey]Exits: {exits}[/]");
        }
        else
        {
            AnsiConsole.MarkupLine("[grey]There are no visible exits.[/]");
        }
    }
}
