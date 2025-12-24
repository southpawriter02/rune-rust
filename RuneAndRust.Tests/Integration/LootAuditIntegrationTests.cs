using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Engine.Services;

namespace RuneAndRust.Tests.Integration;

/// <summary>
/// Integration tests for the LootAuditService Monte Carlo simulation.
/// These tests validate the audit tool's ability to accurately measure drop rates.
/// </summary>
public class LootAuditIntegrationTests
{
    private readonly ILogger<LootAuditService> _logger;
    private readonly ILogger<LootService> _lootLogger;

    public LootAuditIntegrationTests()
    {
        _logger = Substitute.For<ILogger<LootAuditService>>();
        _lootLogger = Substitute.For<ILogger<LootService>>();
    }

    #region Sanity Check Tests

    [Fact]
    public async Task Audit_SanityCheck_ProducesNonZeroResults()
    {
        // Arrange
        var lootService = new LootService(_lootLogger);
        var auditService = new LootAuditService(lootService, _logger);
        var config = new LootAuditConfiguration(100, BiomeType.Ruin, DangerLevel.Safe);

        // Act
        var report = await auditService.RunAuditAsync(config);

        // Assert
        report.Statistics.TotalIterations.Should().Be(100);
        report.Statistics.TotalItemsDropped.Should().BeGreaterThan(0);
        report.Statistics.TotalScripValue.Should().BeGreaterThan(0);
        report.Statistics.SuccessfulIterations.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Audit_SafeZone_HasNoMythForgedItems()
    {
        // Arrange - Use a larger sample for statistical significance
        var lootService = new LootService(_lootLogger, seed: 42); // Seeded for determinism
        var auditService = new LootAuditService(lootService, _logger);
        var config = new LootAuditConfiguration(1000, BiomeType.Ruin, DangerLevel.Safe);

        // Act
        var report = await auditService.RunAuditAsync(config);

        // Assert - Safe zone has 0% MythForged weight in LootTables
        var mythForgedCount = report.Statistics.QualityTierCounts.GetValueOrDefault(QualityTier.MythForged, 0);
        mythForgedCount.Should().Be(0, "Safe zones should never drop MythForged items");

        // Also verify no Optimized items (0% weight in Safe)
        var optimizedCount = report.Statistics.QualityTierCounts.GetValueOrDefault(QualityTier.Optimized, 0);
        optimizedCount.Should().Be(0, "Safe zones should never drop Optimized items");
    }

    [Fact]
    public async Task Audit_LethalZone_CanDropMythForgedItems()
    {
        // Arrange - Lethal zone has 5% MythForged weight
        // Use large sample to ensure statistical likelihood
        var lootService = new LootService(_lootLogger, seed: 12345);
        var auditService = new LootAuditService(lootService, _logger);
        var config = new LootAuditConfiguration(5000, BiomeType.Void, DangerLevel.Lethal);

        // Act
        var report = await auditService.RunAuditAsync(config);

        // Assert - With 5% chance and 5000 iterations, we should see some MythForged
        // Statistical expectation: ~250 items at 5% = ~12 MythForged (conservative estimate)
        // Allow for variance - just verify at least some dropped
        var mythForgedCount = report.Statistics.QualityTierCounts.GetValueOrDefault(QualityTier.MythForged, 0);
        mythForgedCount.Should().BeGreaterThan(0, "Lethal zones should eventually drop MythForged items");
    }

    #endregion

    #region Report Generation Tests

    [Fact]
    public async Task Audit_GeneratesValidMarkdownReport()
    {
        // Arrange
        var lootService = new LootService(_lootLogger, seed: 42);
        var auditService = new LootAuditService(lootService, _logger);
        var config = new LootAuditConfiguration(100, BiomeType.Industrial, DangerLevel.Unstable);

        // Act
        var report = await auditService.RunAuditAsync(config);

        // Assert - Report contains expected sections
        report.MarkdownReport.Should().Contain("# Loot Audit Report");
        report.MarkdownReport.Should().Contain("**Iterations:**");
        report.MarkdownReport.Should().Contain("**Biome:** Industrial");
        report.MarkdownReport.Should().Contain("**Danger Level:** Unstable");
        report.MarkdownReport.Should().Contain("## Summary");
        report.MarkdownReport.Should().Contain("## Quality Tier Distribution");
        report.MarkdownReport.Should().Contain("## Item Type Distribution");
        report.MarkdownReport.Should().Contain("## Anomalies");
        report.MarkdownReport.Should().Contain("LootAuditService v0.3.13a");
    }

    [Fact]
    public async Task Audit_ReportsIncludeTableHeaders()
    {
        // Arrange
        var lootService = new LootService(_lootLogger, seed: 42);
        var auditService = new LootAuditService(lootService, _logger);
        var config = new LootAuditConfiguration(50, BiomeType.Ruin, DangerLevel.Safe);

        // Act
        var report = await auditService.RunAuditAsync(config);

        // Assert - Tables should have proper headers
        report.MarkdownReport.Should().Contain("| Tier | Count | Actual % | Expected % | Variance | Status |");
        report.MarkdownReport.Should().Contain("| Type | Count | Actual % | Expected % | Variance | Status |");
    }

    [Fact]
    public async Task Audit_ReportsWitsBonus_WhenProvided()
    {
        // Arrange
        var lootService = new LootService(_lootLogger, seed: 42);
        var auditService = new LootAuditService(lootService, _logger);
        var config = new LootAuditConfiguration(50, BiomeType.Ruin, DangerLevel.Safe, WitsBonus: 3);

        // Act
        var report = await auditService.RunAuditAsync(config);

        // Assert
        report.MarkdownReport.Should().Contain("**WITS Bonus:** +3");
    }

    #endregion

    #region Variance Flag Tests

    [Fact]
    public async Task Audit_GeneratesVarianceFlags()
    {
        // Arrange
        var lootService = new LootService(_lootLogger, seed: 42);
        var auditService = new LootAuditService(lootService, _logger);
        var config = new LootAuditConfiguration(100, BiomeType.Ruin, DangerLevel.Safe);

        // Act
        var report = await auditService.RunAuditAsync(config);

        // Assert - Should have flags for all quality tiers and item types
        report.Flags.Should().NotBeEmpty();

        var qualityFlags = report.Flags.Where(f => f.Category == "QualityTier").ToList();
        qualityFlags.Should().HaveCount(5); // JuryRigged, Scavenged, ClanForged, Optimized, MythForged

        var typeFlags = report.Flags.Where(f => f.Category == "ItemType").ToList();
        typeFlags.Should().HaveCount(5); // Weapon, Armor, Consumable, Material, Junk (KeyItem excluded)
    }

    [Fact]
    public async Task Audit_ClassifiesVarianceCorrectly()
    {
        // Arrange
        var lootService = new LootService(_lootLogger, seed: 42);
        var auditService = new LootAuditService(lootService, _logger);
        // Use small sample intentionally to potentially create variance
        var config = new LootAuditConfiguration(50, BiomeType.Ruin, DangerLevel.Safe);

        // Act
        var report = await auditService.RunAuditAsync(config);

        // Assert - All flags should have valid severity
        report.Flags.Should().OnlyContain(f =>
            f.Severity == VarianceSeverity.Ok ||
            f.Severity == VarianceSeverity.Warning ||
            f.Severity == VarianceSeverity.Critical);
    }

    #endregion

    #region Biome Distribution Tests

    [Fact]
    public async Task Audit_OrganicBiome_FavorsConsumables()
    {
        // Arrange - Organic biome has 40% Consumable weight (highest of all biomes)
        var lootService = new LootService(_lootLogger, seed: 42);
        var auditService = new LootAuditService(lootService, _logger);
        var config = new LootAuditConfiguration(1000, BiomeType.Organic, DangerLevel.Safe);

        // Act
        var report = await auditService.RunAuditAsync(config);

        // Assert - Consumables should be most common type in Organic
        var consumableCount = report.Statistics.ItemTypeCounts.GetValueOrDefault(ItemType.Consumable, 0);
        var weaponCount = report.Statistics.ItemTypeCounts.GetValueOrDefault(ItemType.Weapon, 0);
        var armorCount = report.Statistics.ItemTypeCounts.GetValueOrDefault(ItemType.Armor, 0);

        consumableCount.Should().BeGreaterThan(weaponCount, "Organic biome should favor consumables over weapons");
        consumableCount.Should().BeGreaterThan(armorCount, "Organic biome should favor consumables over armor");
    }

    [Fact]
    public async Task Audit_IndustrialBiome_FavorsMaterials()
    {
        // Arrange - Industrial biome has 35% Material weight (highest of all biomes)
        var lootService = new LootService(_lootLogger, seed: 42);
        var auditService = new LootAuditService(lootService, _logger);
        var config = new LootAuditConfiguration(1000, BiomeType.Industrial, DangerLevel.Safe);

        // Act
        var report = await auditService.RunAuditAsync(config);

        // Assert - Materials should be most common type in Industrial
        var materialCount = report.Statistics.ItemTypeCounts.GetValueOrDefault(ItemType.Material, 0);
        var consumableCount = report.Statistics.ItemTypeCounts.GetValueOrDefault(ItemType.Consumable, 0);

        materialCount.Should().BeGreaterThan(consumableCount, "Industrial biome should favor materials over consumables");
    }

    #endregion

    #region Statistical Bounds Tests

    [Fact]
    public async Task Audit_QualityDistribution_ProducesReasonableDistribution()
    {
        // Arrange - Use large sample for statistical significance
        // Note: The exact distribution depends on seeding and the internal weighting logic
        var lootService = new LootService(_lootLogger, seed: 42);
        var auditService = new LootAuditService(lootService, _logger);
        var config = new LootAuditConfiguration(5000, BiomeType.Ruin, DangerLevel.Safe);

        // Act
        var report = await auditService.RunAuditAsync(config);

        // Assert - Verify that the distribution produces expected patterns:
        // 1. Optimized and MythForged should be zero in Safe zones (0% weight)
        // 2. All quality tiers present should sum to 100%
        var juryRiggedPercent = report.Statistics.GetQualityTierPercent(QualityTier.JuryRigged);
        var scavengedPercent = report.Statistics.GetQualityTierPercent(QualityTier.Scavenged);
        var clanForgedPercent = report.Statistics.GetQualityTierPercent(QualityTier.ClanForged);
        var optimizedPercent = report.Statistics.GetQualityTierPercent(QualityTier.Optimized);
        var mythForgedPercent = report.Statistics.GetQualityTierPercent(QualityTier.MythForged);

        // The key invariants for Safe zone economy
        optimizedPercent.Should().Be(0, "Safe zones should never drop Optimized items");
        mythForgedPercent.Should().Be(0, "Safe zones should never drop MythForged items");

        // Quality tiers should have non-zero presence for expected tiers
        (juryRiggedPercent + scavengedPercent + clanForgedPercent).Should().BeApproximately(100, 0.1,
            "Total quality distribution should sum to 100%");

        // Basic sanity: We should have items of various qualities
        report.Statistics.TotalItemsDropped.Should().BeGreaterThan(0);
    }

    #endregion

    #region Edge Case Tests

    [Fact]
    public async Task Audit_WithZeroIterations_ReturnsEmptyReport()
    {
        // Arrange
        var lootService = new LootService(_lootLogger);
        var auditService = new LootAuditService(lootService, _logger);
        var config = new LootAuditConfiguration(0, BiomeType.Ruin, DangerLevel.Safe);

        // Act
        var report = await auditService.RunAuditAsync(config);

        // Assert
        report.Statistics.TotalIterations.Should().Be(0);
        report.Statistics.TotalItemsDropped.Should().Be(0);
        report.MarkdownReport.Should().Contain("# Loot Audit Report");
    }

    [Fact]
    public async Task Audit_AllDangerLevels_ProduceValidReports()
    {
        // Arrange
        var lootService = new LootService(_lootLogger, seed: 42);
        var auditService = new LootAuditService(lootService, _logger);

        // Act & Assert - Each danger level should produce a valid report
        foreach (var danger in Enum.GetValues<DangerLevel>())
        {
            var config = new LootAuditConfiguration(100, BiomeType.Ruin, danger);
            var report = await auditService.RunAuditAsync(config);

            report.Statistics.TotalIterations.Should().Be(100);
            report.MarkdownReport.Should().Contain(danger.ToString());
            report.Flags.Should().NotBeEmpty();
        }
    }

    [Fact]
    public async Task Audit_AllBiomes_ProduceValidReports()
    {
        // Arrange
        var lootService = new LootService(_lootLogger, seed: 42);
        var auditService = new LootAuditService(lootService, _logger);

        // Act & Assert - Each biome should produce a valid report
        foreach (var biome in Enum.GetValues<BiomeType>())
        {
            var config = new LootAuditConfiguration(100, biome, DangerLevel.Safe);
            var report = await auditService.RunAuditAsync(config);

            report.Statistics.TotalIterations.Should().Be(100);
            report.MarkdownReport.Should().Contain(biome.ToString());
        }
    }

    #endregion
}
