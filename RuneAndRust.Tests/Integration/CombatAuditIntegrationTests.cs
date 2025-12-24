using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Tests.Infrastructure;
using Xunit;

namespace RuneAndRust.Tests.Integration;

/// <summary>
/// Integration tests for the CombatAuditService Monte Carlo simulation.
/// These tests validate the combat simulation's ability to produce meaningful balance metrics.
/// </summary>
public class CombatAuditIntegrationTests : IDisposable
{
    private readonly TestGameHost _host;

    public CombatAuditIntegrationTests()
    {
        // Create a test host with seeded RNG for deterministic combat outcomes
        _host = TestGameHost.Create(seed: 42, script: new List<string>());
    }

    public void Dispose()
    {
        _host.Dispose();
    }

    #region Sanity Check Tests

    [Fact]
    public async Task Audit_SanityCheck_ProducesResults()
    {
        // Arrange
        using var scope = _host.Services.CreateScope();
        var auditService = scope.ServiceProvider.GetRequiredService<ICombatAuditService>();
        var config = new CombatAuditConfiguration(
            Iterations: 10,
            PlayerArchetype: ArchetypeType.Warrior,
            EnemyTemplateId: "und_draugr_01",
            PlayerLevel: 1);

        // Act
        var report = await auditService.RunAuditAsync(config);

        // Assert
        report.Statistics.TotalEncounters.Should().Be(10);
        report.Statistics.TotalRounds.Should().BeGreaterThan(0);
        report.MarkdownReport.Should().NotBeNullOrEmpty();
        report.Flags.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Audit_WarriorVsMinion_HighWinRate()
    {
        // Arrange - Warrior vs Utility Servitor (Minion tier, very weak)
        using var scope = _host.Services.CreateScope();
        var auditService = scope.ServiceProvider.GetRequiredService<ICombatAuditService>();
        var config = new CombatAuditConfiguration(
            Iterations: 50,
            PlayerArchetype: ArchetypeType.Warrior,
            EnemyTemplateId: "mec_serv_01", // Utility Servitor - Minion tier
            PlayerLevel: 1);

        // Act
        var report = await auditService.RunAuditAsync(config);

        // Assert - Warrior should demolish minions
        report.Statistics.WinRate.Should().BeGreaterThan(85.0,
            "Warriors should win most fights against Minion-tier enemies");
    }

    [Fact]
    public async Task Audit_WarriorVsStandard_CombatFunctional()
    {
        // Arrange - Warrior vs Rusted Draugr (Standard tier)
        using var scope = _host.Services.CreateScope();
        var auditService = scope.ServiceProvider.GetRequiredService<ICombatAuditService>();
        var config = new CombatAuditConfiguration(
            Iterations: 50,
            PlayerArchetype: ArchetypeType.Warrior,
            EnemyTemplateId: "und_draugr_01", // Standard DPS enemy
            PlayerLevel: 1);

        // Act
        var report = await auditService.RunAuditAsync(config);

        // Assert - Combat should be functional (rounds occur, attacks happen)
        // The Draugr (60 HP, Soak 2, 6 Might) is a tough Standard enemy.
        // Due to deterministic seeding (seed=42), results may vary.
        report.Statistics.TotalEncounters.Should().Be(50);
        report.Statistics.TotalRounds.Should().BeGreaterThan(0,
            "Combat should process at least some rounds");
        (report.Statistics.TotalPlayerHits + report.Statistics.TotalPlayerMisses).Should().BeGreaterThan(0,
            "Player should attempt attacks");
    }

    #endregion

    #region Report Generation Tests

    [Fact]
    public async Task Audit_GeneratesValidMarkdownReport()
    {
        // Arrange
        using var scope = _host.Services.CreateScope();
        var auditService = scope.ServiceProvider.GetRequiredService<ICombatAuditService>();
        var config = new CombatAuditConfiguration(
            Iterations: 20,
            PlayerArchetype: ArchetypeType.Skirmisher,
            EnemyTemplateId: "und_draugr_01",
            PlayerLevel: 1);

        // Act
        var report = await auditService.RunAuditAsync(config);

        // Assert - Report contains expected sections
        report.MarkdownReport.Should().Contain("# Combat Audit Report");
        report.MarkdownReport.Should().Contain("**Player:** Skirmisher");
        report.MarkdownReport.Should().Contain("**Enemy:** und_draugr_01");
        report.MarkdownReport.Should().Contain("## Summary");
        report.MarkdownReport.Should().Contain("## Detailed Statistics");
        report.MarkdownReport.Should().Contain("### Player Performance");
        report.MarkdownReport.Should().Contain("### Enemy Performance");
        report.MarkdownReport.Should().Contain("## Balance Assessment");
        report.MarkdownReport.Should().Contain("## Anomalies");
        report.MarkdownReport.Should().Contain("CombatAuditService v0.3.13b");
    }

    [Fact]
    public async Task Audit_ReportIncludesMetricTables()
    {
        // Arrange
        using var scope = _host.Services.CreateScope();
        var auditService = scope.ServiceProvider.GetRequiredService<ICombatAuditService>();
        var config = new CombatAuditConfiguration(
            Iterations: 20,
            PlayerArchetype: ArchetypeType.Warrior,
            EnemyTemplateId: "und_draugr_01",
            PlayerLevel: 1);

        // Act
        var report = await auditService.RunAuditAsync(config);

        // Assert - Tables should have proper headers
        report.MarkdownReport.Should().Contain("| Metric | Value |");
        report.MarkdownReport.Should().Contain("| Metric | Actual | Expected Range | Status |");
        report.MarkdownReport.Should().Contain("**Win Rate**");
        report.MarkdownReport.Should().Contain("**Average Rounds**");
    }

    #endregion

    #region Archetype Comparison Tests

    [Fact]
    public async Task Audit_DifferentArchetypes_ProduceDifferentResults()
    {
        // Arrange
        using var scope = _host.Services.CreateScope();
        var auditService = scope.ServiceProvider.GetRequiredService<ICombatAuditService>();

        var warriorConfig = new CombatAuditConfiguration(50, ArchetypeType.Warrior, "und_draugr_01", 1);
        var adeptConfig = new CombatAuditConfiguration(50, ArchetypeType.Adept, "und_draugr_01", 1);

        // Act
        var warriorReport = await auditService.RunAuditAsync(warriorConfig);
        var adeptReport = await auditService.RunAuditAsync(adeptConfig);

        // Assert - Different archetypes should have different performance characteristics
        // This test just verifies the system produces different results for different configs
        // Note: Due to archetype bonuses, Warriors and Adepts have different combat profiles
        warriorReport.Statistics.Should().NotBe(adeptReport.Statistics,
            "Different archetypes should produce different combat statistics");
    }

    [Fact]
    public async Task Audit_AllArchetypes_ProduceValidReports()
    {
        // Arrange
        using var scope = _host.Services.CreateScope();
        var auditService = scope.ServiceProvider.GetRequiredService<ICombatAuditService>();

        // Act & Assert - Each archetype should produce a valid report
        foreach (var archetype in Enum.GetValues<ArchetypeType>())
        {
            var config = new CombatAuditConfiguration(10, archetype, "mec_serv_01", 1);
            var report = await auditService.RunAuditAsync(config);

            report.Statistics.TotalEncounters.Should().Be(10);
            report.MarkdownReport.Should().Contain(archetype.ToString());
            report.Flags.Should().NotBeEmpty();
        }
    }

    #endregion

    #region Enemy Template Tests

    [Fact]
    public async Task Audit_AllBuiltInEnemies_ProduceValidReports()
    {
        // Arrange
        using var scope = _host.Services.CreateScope();
        var auditService = scope.ServiceProvider.GetRequiredService<ICombatAuditService>();
        var enemyFactory = scope.ServiceProvider.GetRequiredService<IEnemyFactory>();

        // Act & Assert - Each built-in enemy template should work
        foreach (var templateId in enemyFactory.GetTemplateIds())
        {
            var config = new CombatAuditConfiguration(10, ArchetypeType.Warrior, templateId, 1);
            var report = await auditService.RunAuditAsync(config);

            report.Statistics.TotalEncounters.Should().Be(10,
                $"Audit with {templateId} should complete 10 encounters");
            report.MarkdownReport.Should().Contain(templateId);
        }
    }

    [Fact]
    public async Task Audit_WithInvalidEnemy_UsesFallback()
    {
        // Arrange - Use a non-existent template ID
        using var scope = _host.Services.CreateScope();
        var auditService = scope.ServiceProvider.GetRequiredService<ICombatAuditService>();
        var config = new CombatAuditConfiguration(
            Iterations: 10,
            PlayerArchetype: ArchetypeType.Warrior,
            EnemyTemplateId: "nonexistent_enemy_xyz",
            PlayerLevel: 1);

        // Act - Should not throw, should use fallback (und_draugr_01)
        var report = await auditService.RunAuditAsync(config);

        // Assert - Audit still completes successfully
        report.Statistics.TotalEncounters.Should().Be(10);
        report.Statistics.TotalRounds.Should().BeGreaterThan(0);
    }

    #endregion

    #region Variance Flag Tests

    [Fact]
    public async Task Audit_GeneratesVarianceFlags()
    {
        // Arrange
        using var scope = _host.Services.CreateScope();
        var auditService = scope.ServiceProvider.GetRequiredService<ICombatAuditService>();
        var config = new CombatAuditConfiguration(50, ArchetypeType.Warrior, "und_draugr_01", 1);

        // Act
        var report = await auditService.RunAuditAsync(config);

        // Assert - Should have flags for key metrics
        report.Flags.Should().Contain(f => f.Metric == "Win Rate");
        report.Flags.Should().Contain(f => f.Metric == "Avg Rounds");
        report.Flags.Should().Contain(f => f.Metric == "Player Hit Rate");
        report.Flags.Should().Contain(f => f.Metric == "Enemy Hit Rate");
    }

    [Fact]
    public async Task Audit_ClassifiesVarianceCorrectly()
    {
        // Arrange
        using var scope = _host.Services.CreateScope();
        var auditService = scope.ServiceProvider.GetRequiredService<ICombatAuditService>();
        var config = new CombatAuditConfiguration(50, ArchetypeType.Warrior, "und_draugr_01", 1);

        // Act
        var report = await auditService.RunAuditAsync(config);

        // Assert - All flags should have valid severity
        report.Flags.Should().OnlyContain(f =>
            f.Severity == VarianceSeverity.Ok ||
            f.Severity == VarianceSeverity.Warning ||
            f.Severity == VarianceSeverity.Critical);
    }

    [Fact]
    public async Task VarianceFlag_IsWithinBounds_WorksCorrectly()
    {
        // Arrange
        using var scope = _host.Services.CreateScope();
        var auditService = scope.ServiceProvider.GetRequiredService<ICombatAuditService>();
        var config = new CombatAuditConfiguration(100, ArchetypeType.Warrior, "und_draugr_01", 1);

        // Act
        var report = await auditService.RunAuditAsync(config);

        // Assert - Verify IsWithinBounds logic
        foreach (var flag in report.Flags)
        {
            if (flag.ActualValue >= flag.ExpectedMin && flag.ActualValue <= flag.ExpectedMax)
            {
                flag.IsWithinBounds.Should().BeTrue();
                flag.Deviation.Should().Be(0);
            }
            else
            {
                flag.IsWithinBounds.Should().BeFalse();
                flag.Deviation.Should().BeGreaterThan(0);
            }
        }
    }

    #endregion

    #region Edge Case Tests

    [Fact]
    public async Task Audit_WithZeroIterations_ReturnsEmptyReport()
    {
        // Arrange
        using var scope = _host.Services.CreateScope();
        var auditService = scope.ServiceProvider.GetRequiredService<ICombatAuditService>();
        var config = new CombatAuditConfiguration(0, ArchetypeType.Warrior, "und_draugr_01", 1);

        // Act
        var report = await auditService.RunAuditAsync(config);

        // Assert
        report.Statistics.TotalEncounters.Should().Be(0);
        report.Statistics.WinRate.Should().Be(0);
        report.MarkdownReport.Should().Contain("# Combat Audit Report");
    }

    [Fact]
    public async Task Audit_WithHigherLevel_ScalesEnemy()
    {
        // Arrange - Higher level should mean stronger enemy, potentially lower win rate
        using var scope = _host.Services.CreateScope();
        var auditService = scope.ServiceProvider.GetRequiredService<ICombatAuditService>();

        var level1Config = new CombatAuditConfiguration(50, ArchetypeType.Warrior, "und_draugr_01", 1);
        var level3Config = new CombatAuditConfiguration(50, ArchetypeType.Warrior, "und_draugr_01", 3);

        // Act
        var level1Report = await auditService.RunAuditAsync(level1Config);
        var level3Report = await auditService.RunAuditAsync(level3Config);

        // Assert - Higher level enemies should result in longer fights or lower win rate
        // Due to scaling, level 3 enemies have more HP/damage
        (level3Report.Statistics.AvgRoundsPerEncounter >= level1Report.Statistics.AvgRoundsPerEncounter ||
         level3Report.Statistics.WinRate <= level1Report.Statistics.WinRate)
            .Should().BeTrue("Higher level enemies should be harder to defeat");
    }

    #endregion

    #region Configuration Tests

    [Fact]
    public void CombatAuditConfiguration_Default_HasReasonableValues()
    {
        // Act
        var config = CombatAuditConfiguration.Default;

        // Assert
        config.Iterations.Should().Be(1000);
        config.PlayerArchetype.Should().Be(ArchetypeType.Warrior);
        config.EnemyTemplateId.Should().Be("und_draugr_01");
        config.PlayerLevel.Should().Be(1);
        config.Seed.Should().BeNull();
    }

    [Fact]
    public void CombatAuditConfiguration_RecordEquality_Works()
    {
        // Arrange
        var config1 = new CombatAuditConfiguration(100, ArchetypeType.Warrior, "und_draugr_01", 1);
        var config2 = new CombatAuditConfiguration(100, ArchetypeType.Warrior, "und_draugr_01", 1);
        var config3 = new CombatAuditConfiguration(200, ArchetypeType.Warrior, "und_draugr_01", 1);

        // Assert
        config1.Should().Be(config2);
        config1.Should().NotBe(config3);
    }

    #endregion
}
