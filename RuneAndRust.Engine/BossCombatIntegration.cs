using RuneAndRust.Core;
using RuneAndRust.Persistence;
using Serilog;

namespace RuneAndRust.Engine;

/// <summary>
/// v0.23 Boss Combat Integration: Hooks boss mechanics into combat loop
/// Manages boss encounters, phase transitions, telegraphed abilities, and loot
/// </summary>
public class BossCombatIntegration
{
    private static readonly ILogger _log = Log.ForContext<BossCombatIntegration>();

    private readonly BossEncounterRepository _repository;
    private readonly BossEncounterService _encounterService;
    private readonly TelegraphedAbilityService _telegraphService;
    private readonly BossLootService _lootService;
    private readonly DiceService _diceService;

    public BossCombatIntegration(
        BossEncounterRepository repository,
        DiceService diceService)
    {
        _repository = repository;
        _diceService = diceService;
        _encounterService = new BossEncounterService(repository, diceService);
        _telegraphService = new TelegraphedAbilityService(repository, diceService);
        _lootService = new BossLootService(repository, diceService);
    }

    // PPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPP
    // COMBAT INITIALIZATION
    // PPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPP

    /// <summary>
    /// Initialize boss encounters when combat starts
    /// Call this during combat initialization if boss enemies are present
    /// </summary>
    public void InitializeBossEncounters(CombatState combatState)
    {
        var bosses = combatState.Enemies.Where(e => e.IsBoss).ToList();

        if (!bosses.Any())
        {
            return; // No bosses in this combat
        }

        _log.Information("Initializing boss encounters: BossCount={Count}", bosses.Count);

        foreach (var boss in bosses)
        {
            // Determine encounter ID based on boss type
            int encounterId = boss.Type switch
            {
                EnemyType.RuinWarden => 1,
                EnemyType.AethericAberration => 2,
                EnemyType.ForlornArchivist => 3,
                EnemyType.OmegaSentinel => 4,
                _ => 0
            };

            if (encounterId == 0)
            {
                _log.Warning("Unknown boss type: {BossType}", boss.Type);
                continue;
            }

            // Initialize boss encounter
            _encounterService.InitializeBossEncounter(boss, encounterId);

            // Add initialization message to combat log
            var encounterConfig = _repository.GetBossEncounterByEncounterId(encounterId);
            if (encounterConfig != null)
            {
                combatState.AddLogEntry("");
                combatState.AddLogEntry($"TPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPW");
                combatState.AddLogEntry($"Q ”  BOSS ENCOUNTER: {boss.Name.ToUpper()}");
                combatState.AddLogEntry($"ZPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPP]");
                combatState.AddLogEntry($"[bold red]{boss.Name}[/] emerges from the shadows!");
                combatState.AddLogEntry($"[dim]Boss Type: {encounterConfig.BossType}[/]");
                combatState.AddLogEntry($"[dim]Phases: {encounterConfig.TotalPhases}[/]");
                combatState.AddLogEntry("");
            }

            _log.Information("Boss encounter initialized: Boss={BossName}, EncounterId={EncounterId}",
                boss.Name, encounterId);
        }
    }

    // PPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPP
    // TURN PROCESSING
    // PPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPP

    /// <summary>
    /// Process boss mechanics after player/enemy actions
    /// Call this after each action during combat
    /// </summary>
    public void ProcessBossAction(CombatState combatState, Enemy boss)
    {
        if (!boss.IsBoss || boss.HP <= 0)
        {
            return;
        }

        // Check for phase transitions
        var phaseMessage = _encounterService.CheckPhaseTransition(boss);
        if (!string.IsNullOrEmpty(phaseMessage))
        {
            combatState.AddLogEntry(phaseMessage);

            // Process phase transition (spawn adds, etc.)
            var transitionMessage = _encounterService.ProcessPhaseTransition(boss, combatState);
            if (!string.IsNullOrEmpty(transitionMessage))
            {
                combatState.AddLogEntry(transitionMessage);
            }
        }

        // Check for enrage
        var enrageMessage = _encounterService.CheckEnrageCondition(boss);
        if (!string.IsNullOrEmpty(enrageMessage))
        {
            combatState.AddLogEntry(enrageMessage);
        }
    }

    /// <summary>
    /// Process telegraphed abilities at end of turn
    /// Returns list of ready abilities that should execute
    /// </summary>
    public void ProcessEndOfTurn(CombatState combatState, int currentTurn)
    {
        var bosses = combatState.Enemies.Where(e => e.IsBoss && e.HP > 0).ToList();

        if (!bosses.Any())
        {
            return;
        }

        // Process active telegraphs
        var readyAbilities = _telegraphService.ProcessActiveTelegraphs(bosses, currentTurn);

        // Execute ready telegraphed abilities
        foreach (var (boss, ability) in readyAbilities)
        {
            var executeMessage = _telegraphService.ExecuteTelegraphedAbility(
                boss, ability, combatState.Player, combatState);

            combatState.AddLogEntry(executeMessage);
        }

        // Process vulnerability windows
        foreach (var boss in bosses)
        {
            var vulnerabilityMessage = _telegraphService.ProcessVulnerabilityWindow(boss);
            if (!string.IsNullOrEmpty(vulnerabilityMessage))
            {
                combatState.AddLogEntry($"[dim]{vulnerabilityMessage}[/]");
            }
        }
    }

    /// <summary>
    /// Check if player damage interrupted a telegraphed ability
    /// Call this after player deals damage to a boss
    /// </summary>
    public void CheckTelegraphInterrupt(CombatState combatState, Enemy boss, int damageDealt)
    {
        if (!boss.IsBoss || damageDealt <= 0)
        {
            return;
        }

        var interruptMessage = _telegraphService.CheckTelegraphInterrupt(boss, damageDealt);
        if (!string.IsNullOrEmpty(interruptMessage))
        {
            combatState.AddLogEntry(interruptMessage);
        }
    }

    // PPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPP
    // BOSS AI & TELEGRAPHING
    // PPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPP

    /// <summary>
    /// Decide if boss should start telegraphing an ability this turn
    /// Returns ability to telegraph, or null if boss uses standard attack
    /// </summary>
    public BossAbilityData? ShouldBossTelegraph(Enemy boss, int currentTurn)
    {
        if (!boss.IsBoss || boss.BossEncounterId == null)
        {
            return null;
        }

        // Get AI pattern for current phase
        var aiPattern = _repository.GetBossAIPattern(boss.BossEncounterId.Value, boss.CurrentPhase);
        if (aiPattern == null)
        {
            return null;
        }

        // Check if boss is already charging an ability
        if (_telegraphService.IsBossChargingAbility(boss.Id))
        {
            _log.Debug("Boss already charging, skipping new telegraph: Boss={BossId}", boss.Id);
            return null;
        }

        // Roll for telegraph based on frequency
        int telegraphRoll = _diceService.RollD100();
        int telegraphChance = (int)(aiPattern.TelegraphFrequency * 100);

        if (telegraphRoll > telegraphChance)
        {
            _log.Debug("Telegraph roll failed: Boss={BossId}, Roll={Roll}, Chance={Chance}",
                boss.Id, telegraphRoll, telegraphChance);
            return null;
        }

        // Get available telegraphed abilities for current phase
        var abilities = _repository.GetBossAbilities(boss.BossEncounterId.Value);
        var telegraphedAbilities = abilities
            .Where(a => a.IsTelegraphed && a.PhaseNumber == boss.CurrentPhase)
            .ToList();

        if (!telegraphedAbilities.Any())
        {
            _log.Warning("No telegraphed abilities found for phase: Boss={BossId}, Phase={Phase}",
                boss.Id, boss.CurrentPhase);
            return null;
        }

        // Check for ultimate ability (low HP threshold)
        var hpPercent = (float)boss.HP / boss.MaxHP;
        var ultimateAbilities = telegraphedAbilities
            .Where(a => a.IsUltimate && hpPercent <= aiPattern.UltimateHpThreshold)
            .ToList();

        if (ultimateAbilities.Any())
        {
            // Prioritize ultimate when below threshold
            var ultimate = ultimateAbilities[_diceService.RollBetween(0, ultimateAbilities.Count - 1)];
            _log.Information("Boss triggering ultimate ability: Boss={BossId}, Ability={AbilityName}",
                boss.Id, ultimate.AbilityName);
            return ultimate;
        }

        // Select random telegraphed ability
        var selectedAbility = telegraphedAbilities[_diceService.RollBetween(0, telegraphedAbilities.Count - 1)];
        _log.Information("Boss telegraphing ability: Boss={BossId}, Ability={AbilityName}",
            boss.Id, selectedAbility.AbilityName);

        return selectedAbility;
    }

    /// <summary>
    /// Start boss telegraphing an ability
    /// </summary>
    public void BeginBossTelegraph(CombatState combatState, Enemy boss, BossAbilityData ability, int currentTurn)
    {
        var message = _telegraphService.BeginTelegraph(boss, ability, currentTurn);
        combatState.AddLogEntry(message);
    }

    // PPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPP
    // LOOT GENERATION
    // PPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPP

    /// <summary>
    /// Generate boss loot when defeated
    /// Call this instead of regular loot generation for bosses
    /// </summary>
    public void GenerateBossLoot(CombatState combatState, Enemy boss, string characterId)
    {
        if (!boss.IsBoss || boss.BossEncounterId == null)
        {
            _log.Warning("Attempted to generate boss loot for non-boss: EnemyId={EnemyId}", boss.Id);
            return;
        }

        // Determine boss TDR
        int bossTdr = boss.Type switch
        {
            EnemyType.RuinWarden => 50,
            EnemyType.AethericAberration => 55,
            EnemyType.ForlornArchivist => 60,
            EnemyType.OmegaSentinel => 100,
            _ => 50
        };

        // Generate loot
        var lootResult = _lootService.GenerateBossLoot(
            boss.BossEncounterId.Value,
            characterId,
            bossTdr);

        // Add loot to combat log
        combatState.AddLogEntry(lootResult.LogMessage);

        // Apply loot to player
        ApplyBossLootToPlayer(combatState.Player, lootResult);

        _log.Information("Boss loot generated: Boss={BossName}, Items={ItemCount}, Silver={Silver}",
            boss.Name, lootResult.Items.Count, lootResult.SilverMarks);
    }

    /// <summary>
    /// Apply generated boss loot to player
    /// </summary>
    private void ApplyBossLootToPlayer(PlayerCharacter player, LootGenerationResult lootResult)
    {
        // Add silver marks (assuming player has currency property)
        // Note: This may need adjustment based on actual currency system
        if (lootResult.SilverMarks > 0)
        {
            _log.Debug("Adding silver marks to player: Amount={Amount}", lootResult.SilverMarks);
            // TODO: Integrate with actual currency system
            // player.Currency += lootResult.SilverMarks;
        }

        // Add crafting materials
        foreach (var material in lootResult.CraftingMaterials)
        {
            if (player.CraftingComponents.ContainsKey(ComponentType.Unknown))
            {
                // TODO: Map material names to ComponentType enum
                // For now, just log
                _log.Debug("Material dropped: Name={Name}, Count={Count}",
                    material.MaterialName, material.Count);
            }
        }

        // Add items to player inventory
        foreach (var item in lootResult.Items)
        {
            _log.Information("Item generated: Name={Name}, Quality={Quality}, IsArtifact={IsArtifact}",
                item.ItemName, item.QualityTier, item.IsArtifact);

            // TODO: Convert GeneratedItem to Equipment and add to player inventory
            // This requires creating Equipment instances from GeneratedItem data
        }
    }

    // PPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPP
    // UTILITY METHODS
    // PPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPP

    /// <summary>
    /// Check if combat contains any boss enemies
    /// </summary>
    public bool HasBossEncounter(CombatState combatState)
    {
        return combatState.Enemies.Any(e => e.IsBoss);
    }

    /// <summary>
    /// Get all boss enemies in combat
    /// </summary>
    public List<Enemy> GetBosses(CombatState combatState)
    {
        return combatState.Enemies.Where(e => e.IsBoss).ToList();
    }

    /// <summary>
    /// Clear all telegraphs when combat ends
    /// </summary>
    public void ClearBossCombatState(CombatState combatState)
    {
        var bosses = combatState.Enemies.Where(e => e.IsBoss).ToList();

        foreach (var boss in bosses)
        {
            _telegraphService.ClearTelegraphs(boss.Id);
        }

        _log.Debug("Boss combat state cleared: BossCount={Count}", bosses.Count);
    }

    /// <summary>
    /// Get active telegraphs for display
    /// Returns formatted list of active telegraphs
    /// </summary>
    public List<string> GetActiveTelegraphsDisplay(CombatState combatState)
    {
        var messages = new List<string>();
        var bosses = combatState.Enemies.Where(e => e.IsBoss && e.HP > 0).ToList();

        foreach (var boss in bosses)
        {
            var telegraphs = _telegraphService.GetActiveTelegraphs(boss.Id);

            foreach (var telegraph in telegraphs.Where(t => t.IsCharging && !t.IsCompleted))
            {
                var ability = _repository.GetBossAbility(telegraph.BossAbilityId);
                if (ability != null)
                {
                    int turnsRemaining = _telegraphService.GetTelegraphTurnsRemaining(telegraph, telegraph.CurrentTurn);
                    messages.Add($"   {boss.Name}: [yellow]{ability.AbilityName}[/] ({turnsRemaining} turn{(turnsRemaining != 1 ? "s" : "")})");

                    if (telegraph.InterruptDamageThreshold > 0)
                    {
                        int damageNeeded = telegraph.InterruptDamageThreshold - telegraph.AccumulatedInterruptDamage;
                        if (damageNeeded > 0)
                        {
                            messages.Add($"    [dim]=á  {damageNeeded} damage needed to interrupt[/]");
                        }
                    }
                }
            }
        }

        return messages;
    }

    /// <summary>
    /// Get boss status display (phase, enrage, vulnerability)
    /// </summary>
    public List<string> GetBossStatusDisplay(CombatState combatState)
    {
        var messages = new List<string>();
        var bosses = combatState.Enemies.Where(e => e.IsBoss && e.HP > 0).ToList();

        foreach (var boss in bosses)
        {
            var status = new List<string>();

            // Phase
            status.Add($"Phase {boss.CurrentPhase}");

            // Enrage
            if (boss.IsEnraged)
            {
                status.Add("[red]ENRAGED[/]");
            }

            // Vulnerability
            if (boss.VulnerableTurnsRemaining > 0)
            {
                status.Add($"[yellow]VULNERABLE ({boss.VulnerableTurnsRemaining}t)[/]");
            }

            // Stagger
            if (boss.StaggeredTurnsRemaining > 0)
            {
                status.Add($"[cyan]STAGGERED ({boss.StaggeredTurnsRemaining}t)[/]");
            }

            if (status.Any())
            {
                messages.Add($"{boss.Name}: {string.Join(" | ", status)}");
            }
        }

        return messages;
    }
}
