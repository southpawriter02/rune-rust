using NUnit.Framework;
using RuneAndRust.Core;
using RuneAndRust.Engine;

namespace RuneAndRust.Tests;

/// <summary>
/// v0.23: Integration tests for boss encounter system
/// Tests multi-phase bosses, telegraphed abilities, and legendary loot
/// </summary>
[TestFixture]
public class BossEncounterTests
{
    private DiceService _diceService = null!;
    private BossEncounterService _bossEncounterService = null!;
    private TelegraphedAbilityService _telegraphedAbilityService = null!;
    private BossLootService _bossLootService = null!;
    private EquipmentDatabase _equipmentDatabase = null!;

    [SetUp]
    public void Setup()
    {
        _diceService = new DiceService(seed: 42); // Fixed seed for deterministic tests
        _bossEncounterService = new BossEncounterService(_diceService, new EnemyFactory());
        _telegraphedAbilityService = new TelegraphedAbilityService(_diceService);
        _equipmentDatabase = new EquipmentDatabase();
        _bossLootService = new BossLootService(_equipmentDatabase);
    }

    #region Phase Transition Tests

    [Test]
    public void BossPhaseTransition_TriggersAtCorrectHPThreshold()
    {
        // Arrange
        var boss = CreateTestBoss(maxHP: 100, currentHP: 100);
        var phases = BossDatabase.GetBossPhases(EnemyType.RuinWarden);

        _bossEncounterService.InitializeBossEncounter(boss, phases);

        // Act - Reduce boss to 74% HP (should trigger Phase 2 at 75% threshold)
        boss.HP = 74;
        var phaseTransition = _bossEncounterService.CheckPhaseTransition(boss, phases);

        // Assert
        Assert.That(phaseTransition, Is.Not.Null);
        Assert.That(phaseTransition!.PhaseNumber, Is.EqualTo(2));
        Assert.That(phaseTransition.HPPercentageThreshold, Is.EqualTo(75));
    }

    [Test]
    public void BossPhaseTransition_AppliesStatModifiers()
    {
        // Arrange
        var boss = CreateTestBoss(maxHP: 100, currentHP: 74);
        var phases = BossDatabase.GetBossPhases(EnemyType.RuinWarden);
        var combatState = new CombatState();

        _bossEncounterService.InitializeBossEncounter(boss, phases);

        var initialDefense = boss.DefenseBonus;

        // Act
        var phaseTransition = _bossEncounterService.CheckPhaseTransition(boss, phases);
        if (phaseTransition != null)
        {
            _bossEncounterService.ExecutePhaseTransition(boss, phaseTransition, combatState);
        }

        // Assert
        Assert.That(boss.Phase, Is.EqualTo(2));
        Assert.That(boss.DefenseBonus, Is.GreaterThan(initialDefense)); // Phase 2 grants +2 Defense
    }

    [Test]
    public void BossPhaseTransition_GrantsInvulnerability()
    {
        // Arrange
        var boss = CreateTestBoss(maxHP: 100, currentHP: 74);
        var phases = BossDatabase.GetBossPhases(EnemyType.RuinWarden);
        var combatState = new CombatState();

        _bossEncounterService.InitializeBossEncounter(boss, phases);

        // Act
        var phaseTransition = _bossEncounterService.CheckPhaseTransition(boss, phases);
        if (phaseTransition != null)
        {
            _bossEncounterService.ExecutePhaseTransition(boss, phaseTransition, combatState);
        }

        // Assert
        Assert.That(_bossEncounterService.IsBossInvulnerable(boss), Is.True);
        Assert.That(_bossEncounterService.GetInvulnerabilityTurns(boss), Is.EqualTo(1));
    }

    [Test]
    public void BossInvulnerability_ExpiresAfterTurns()
    {
        // Arrange
        var boss = CreateTestBoss(maxHP: 100, currentHP: 74);
        var phases = BossDatabase.GetBossPhases(EnemyType.RuinWarden);
        var combatState = new CombatState();

        _bossEncounterService.InitializeBossEncounter(boss, phases);

        var phaseTransition = _bossEncounterService.CheckPhaseTransition(boss, phases);
        if (phaseTransition != null)
        {
            _bossEncounterService.ExecutePhaseTransition(boss, phaseTransition, combatState);
        }

        // Act - Process end of turn
        _bossEncounterService.ProcessEndOfTurn(boss);

        // Assert
        Assert.That(_bossEncounterService.IsBossInvulnerable(boss), Is.False);
        Assert.That(_bossEncounterService.GetInvulnerabilityTurns(boss), Is.EqualTo(0));
    }

    [Test]
    public void BossPhaseTransition_CanSpawnAdds()
    {
        // Arrange
        var boss = CreateTestBoss(maxHP: 100, currentHP: 74);
        var phases = BossDatabase.GetBossPhases(EnemyType.RuinWarden);
        var combatState = new CombatState();

        _bossEncounterService.InitializeBossEncounter(boss, phases);

        // Act
        var phaseTransition = _bossEncounterService.CheckPhaseTransition(boss, phases);
        if (phaseTransition?.AddWave != null)
        {
            var (spawnedEnemies, logMessage) = _bossEncounterService.SpawnAddWave(phaseTransition.AddWave);

            // Assert
            Assert.That(spawnedEnemies.Count, Is.EqualTo(2)); // Ruin-Warden spawns 2 Corrupted Servitors
            Assert.That(logMessage, Does.Contain("reinforcements"));
        }
    }

    #endregion

    #region Enrage Mechanics Tests

    [Test]
    public void BossEnrage_TriggersAfterTimerExpires()
    {
        // Arrange
        var boss = CreateTestBoss(maxHP: 100, currentHP: 100);
        var phases = BossDatabase.GetBossPhases(EnemyType.RuinWarden);

        _bossEncounterService.InitializeBossEncounter(boss, phases, enrageTimer: 3);

        var initialDamageBonus = boss.DamageBonus;

        // Act - Process 3 turns
        _bossEncounterService.ProcessEndOfTurn(boss); // Turn 1
        _bossEncounterService.ProcessEndOfTurn(boss); // Turn 2
        var logMessage = _bossEncounterService.ProcessEndOfTurn(boss); // Turn 3 - Enrage!

        // Assert
        Assert.That(boss.DamageBonus, Is.GreaterThan(initialDamageBonus));
        Assert.That(logMessage, Does.Contain("ENRAGE"));
    }

    [Test]
    public void BossEnrage_ShowsWarning()
    {
        // Arrange
        var boss = CreateTestBoss(maxHP: 100, currentHP: 100);
        var phases = BossDatabase.GetBossPhases(EnemyType.RuinWarden);

        _bossEncounterService.InitializeBossEncounter(boss, phases, enrageTimer: 3);

        // Act - Process 1 turn
        _bossEncounterService.ProcessEndOfTurn(boss);
        var logMessage = _bossEncounterService.ProcessEndOfTurn(boss); // 1 turn remaining

        // Assert
        Assert.That(logMessage, Does.Contain("WARNING"));
        Assert.That(logMessage, Does.Contain("enrages in"));
    }

    #endregion

    #region Telegraphed Ability Tests

    [Test]
    public void TelegraphedAbility_StartsCharging()
    {
        // Arrange
        var boss = CreateTestBoss(maxHP: 100, currentHP: 100);
        var abilities = BossDatabase.GetBossAbilities(EnemyType.RuinWarden);
        var systemOverload = abilities.First(a => a.Id == "system_overload");

        _telegraphedAbilityService.InitializeCombat();

        // Act
        var logMessage = _telegraphedAbilityService.StartChargingAbility(boss, systemOverload);

        // Assert
        Assert.That(logMessage, Does.Contain("WARNING"));
        Assert.That(logMessage, Does.Contain("charging"));
        Assert.That(_telegraphedAbilityService.IsBossChargingAbility(boss.Id), Is.True);
    }

    [Test]
    public void TelegraphedAbility_ExecutesAfterChargeTurns()
    {
        // Arrange
        var boss = CreateTestBoss(maxHP: 100, currentHP: 100);
        var player = CreateTestPlayer();
        var abilities = BossDatabase.GetBossAbilities(EnemyType.RuinWarden);
        var systemOverload = abilities.First(a => a.Id == "system_overload");

        _telegraphedAbilityService.InitializeCombat();
        _telegraphedAbilityService.StartChargingAbility(boss, systemOverload);

        // Act - Process end of turn (charge time = 1)
        var readyToExecute = _telegraphedAbilityService.ProcessEndOfTurn(new List<Enemy> { boss });

        // Assert
        Assert.That(readyToExecute.Count, Is.EqualTo(1));
        Assert.That(readyToExecute[0].ability.Id, Is.EqualTo("system_overload"));
    }

    [Test]
    public void TelegraphedAbility_CanBeInterrupted()
    {
        // Arrange
        var boss = CreateTestBoss(maxHP: 100, currentHP: 100);
        var abilities = BossDatabase.GetBossAbilities(EnemyType.RuinWarden);
        var systemOverload = abilities.First(a => a.Id == "system_overload");

        _telegraphedAbilityService.InitializeCombat();
        _telegraphedAbilityService.StartChargingAbility(boss, systemOverload);

        // Act
        var logMessage = _telegraphedAbilityService.InterruptAbility(boss, systemOverload.Id);

        // Assert
        Assert.That(logMessage, Does.Contain("INTERRUPTED"));
        Assert.That(_telegraphedAbilityService.IsBossChargingAbility(boss.Id), Is.False);
    }

    [Test]
    public void UltimateAbility_GrantsVulnerabilityWindow()
    {
        // Arrange
        var boss = CreateTestBoss(maxHP: 100, currentHP: 40); // Below 50% HP
        var player = CreateTestPlayer();
        var abilities = BossDatabase.GetBossAbilities(EnemyType.RuinWarden);
        var totalSystemFailure = abilities.First(a => a.Id == "total_system_failure");

        _telegraphedAbilityService.InitializeCombat();

        var initialVulnerable = boss.VulnerableTurnsRemaining;

        // Act
        var logMessage = _telegraphedAbilityService.ExecuteTelegraphedAbility(boss, totalSystemFailure, player);

        // Assert
        Assert.That(boss.VulnerableTurnsRemaining, Is.GreaterThan(initialVulnerable));
        Assert.That(logMessage, Does.Contain("VULNERABLE"));
    }

    [Test]
    public void TelegraphedAbility_StartsCooldown()
    {
        // Arrange
        var boss = CreateTestBoss(maxHP: 100, currentHP: 100);
        var player = CreateTestPlayer();
        var abilities = BossDatabase.GetBossAbilities(EnemyType.RuinWarden);
        var systemOverload = abilities.First(a => a.Id == "system_overload");

        _telegraphedAbilityService.InitializeCombat();

        // Act
        _telegraphedAbilityService.ExecuteTelegraphedAbility(boss, systemOverload, player);

        // Assert
        Assert.That(_telegraphedAbilityService.IsAbilityOnCooldown(boss.Id, systemOverload.Id), Is.True);
        Assert.That(_telegraphedAbilityService.GetCooldownRemaining(boss.Id, systemOverload.Id), Is.EqualTo(3));
    }

    #endregion

    #region Boss Loot Tests

    [Test]
    public void BossLoot_GuaranteesMinimumQuality()
    {
        // Arrange
        var boss = CreateTestBoss(maxHP: 100, currentHP: 0, enemyType: EnemyType.RuinWarden);
        var lootTable = BossDatabase.GetBossLootTable(EnemyType.RuinWarden);
        var player = CreateTestPlayer();

        // Act
        var (equipment, currency, materials) = _bossLootService.GenerateBossLoot(boss, lootTable, player, tdr: 5);

        // Assert
        Assert.That(equipment.Count, Is.GreaterThan(0));
        Assert.That(equipment[0].Quality, Is.GreaterThanOrEqualTo(lootTable.GuaranteedQuality));
    }

    [Test]
    public void BossLoot_GeneratesCurrency()
    {
        // Arrange
        var boss = CreateTestBoss(maxHP: 100, currentHP: 0, enemyType: EnemyType.RuinWarden);
        var lootTable = BossDatabase.GetBossLootTable(EnemyType.RuinWarden);
        var player = CreateTestPlayer();

        // Act
        var (equipment, currency, materials) = _bossLootService.GenerateBossLoot(boss, lootTable, player, tdr: 5);

        // Assert
        Assert.That(currency, Is.GreaterThanOrEqualTo(lootTable.CurrencyDrop.Min));
        Assert.That(currency, Is.LessThanOrEqualTo(lootTable.CurrencyDrop.Max));
    }

    [Test]
    public void BossLoot_CanDropArtifacts()
    {
        // Arrange - Run multiple times to test probabilistic artifact drops
        var boss = CreateTestBoss(maxHP: 100, currentHP: 0, enemyType: EnemyType.RuinWarden);
        var lootTable = BossDatabase.GetBossLootTable(EnemyType.RuinWarden);
        var player = CreateTestPlayer();

        int artifactDrops = 0;

        // Act - Simulate 100 boss kills
        for (int i = 0; i < 100; i++)
        {
            var (equipment, _, _) = _bossLootService.GenerateBossLoot(boss, lootTable, player, tdr: 10);

            // Check if any equipment is an artifact (should have special effects matching artifact definitions)
            if (equipment.Any(e => e.Quality == QualityTier.MythForged && !string.IsNullOrEmpty(e.SpecialEffect)))
            {
                artifactDrops++;
            }
        }

        // Assert - With 10% base + TDR scaling, we should see some artifacts
        // (exact probability is hard to predict due to scaling, but should be > 0)
        Assert.That(artifactDrops, Is.GreaterThan(0), "No artifacts dropped in 100 attempts");
    }

    [Test]
    public void BossLoot_TDRScalesArtifactChance()
    {
        // Arrange
        var boss = CreateTestBoss(maxHP: 100, currentHP: 0, enemyType: EnemyType.RuinWarden);
        var lootTable = BossDatabase.GetBossLootTable(EnemyType.RuinWarden);
        var player = CreateTestPlayer();

        int lowTDRArtifacts = 0;
        int highTDRArtifacts = 0;

        // Act - Test with low TDR vs high TDR
        for (int i = 0; i < 100; i++)
        {
            var (equipmentLow, _, _) = _bossLootService.GenerateBossLoot(boss, lootTable, player, tdr: 0);
            if (equipmentLow.Any(e => e.Quality == QualityTier.MythForged))
                lowTDRArtifacts++;

            var (equipmentHigh, _, _) = _bossLootService.GenerateBossLoot(boss, lootTable, player, tdr: 20);
            if (equipmentHigh.Any(e => e.Quality == QualityTier.MythForged))
                highTDRArtifacts++;
        }

        // Assert - High TDR should produce more legendary/artifact drops
        Assert.That(highTDRArtifacts, Is.GreaterThan(lowTDRArtifacts),
            "High TDR should increase legendary drop rates");
    }

    #endregion

    #region Boss Database Tests

    [Test]
    public void BossDatabase_ReturnsValidPhases()
    {
        // Act
        var phases = BossDatabase.GetBossPhases(EnemyType.RuinWarden);

        // Assert
        Assert.That(phases.Count, Is.GreaterThan(0));
        Assert.That(phases.All(p => p.PhaseNumber > 0), Is.True);
        Assert.That(phases.All(p => p.HPPercentageThreshold > 0 && p.HPPercentageThreshold <= 100), Is.True);
    }

    [Test]
    public void BossDatabase_ReturnsValidAbilities()
    {
        // Act
        var abilities = BossDatabase.GetBossAbilities(EnemyType.RuinWarden);

        // Assert
        Assert.That(abilities.Count, Is.GreaterThan(0));
        Assert.That(abilities.All(a => !string.IsNullOrEmpty(a.Id)), Is.True);
        Assert.That(abilities.All(a => !string.IsNullOrEmpty(a.Name)), Is.True);
    }

    [Test]
    public void BossDatabase_TelegraphedAbilitiesHaveChargeTime()
    {
        // Act
        var abilities = BossDatabase.GetBossAbilities(EnemyType.RuinWarden);
        var telegraphed = abilities.Where(a => a.Type == BossAbilityType.Telegraphed || a.Type == BossAbilityType.Ultimate);

        // Assert
        Assert.That(telegraphed.All(a => a.ChargeTurns > 0), Is.True);
        Assert.That(telegraphed.All(a => !string.IsNullOrEmpty(a.ChargeMessage)), Is.True);
        Assert.That(telegraphed.All(a => !string.IsNullOrEmpty(a.ExecuteMessage)), Is.True);
    }

    [Test]
    public void BossDatabase_UltimateAbilitiesHaveVulnerabilityWindow()
    {
        // Act
        var abilities = BossDatabase.GetBossAbilities(EnemyType.RuinWarden);
        var ultimates = abilities.Where(a => a.Type == BossAbilityType.Ultimate);

        // Assert
        Assert.That(ultimates.All(a => a.TriggersVulnerability), Is.True);
        Assert.That(ultimates.All(a => a.VulnerabilityDuration > 0), Is.True);
    }

    [Test]
    public void BossDatabase_ReturnsValidLootTable()
    {
        // Act
        var lootTable = BossDatabase.GetBossLootTable(EnemyType.RuinWarden);

        // Assert
        Assert.That(lootTable, Is.Not.Null);
        Assert.That(lootTable.GuaranteedQuality, Is.GreaterThanOrEqualTo(QualityTier.ClanForged));
        Assert.That(lootTable.CurrencyDrop.Min, Is.GreaterThan(0));
        Assert.That(lootTable.CurrencyDrop.Max, Is.GreaterThan(lootTable.CurrencyDrop.Min));
    }

    #endregion

    #region Helper Methods

    private Enemy CreateTestBoss(int maxHP = 100, int currentHP = 100, EnemyType enemyType = EnemyType.RuinWarden)
    {
        return new Enemy
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Test Boss",
            Type = enemyType,
            MaxHP = maxHP,
            HP = currentHP,
            IsBoss = true,
            Phase = 1,
            BaseDamageDice = 2,
            DamageBonus = 1,
            Attributes = new Attributes(might: 5, finesse: 3, wits: 2, will: 2, sturdiness: 5)
        };
    }

    private PlayerCharacter CreateTestPlayer()
    {
        return new PlayerCharacter
        {
            Name = "Test Hero",
            CharacterClass = CharacterClass.Warrior,
            MaxHP = 50,
            HP = 50,
            Attributes = new Attributes(might: 4, finesse: 3, wits: 2, will: 2, sturdiness: 4)
        };
    }

    #endregion
}
