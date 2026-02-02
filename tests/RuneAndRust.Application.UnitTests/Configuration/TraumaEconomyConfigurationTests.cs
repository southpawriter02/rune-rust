// ═══════════════════════════════════════════════════════════════════════════════
// TraumaEconomyConfigurationTests.cs
// Unit tests for TraumaEconomyConfiguration class.
// Validates configuration loading, default values, and validation logic.
// Version: 0.18.5e
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Application.UnitTests.Configuration;

using FluentAssertions;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using RuneAndRust.Application.Configuration;

/// <summary>
/// Unit tests for the <see cref="TraumaEconomyConfiguration"/> class.
/// </summary>
[TestFixture]
public class TraumaEconomyConfigurationTests
{
    // ═══════════════════════════════════════════════════════════════════════════
    // Default Value Tests
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void Constructor_WithEmptyConfiguration_UsesDefaultValues()
    {
        // Arrange
        var configurationBuilder = new ConfigurationBuilder();
        var configuration = configurationBuilder.Build();

        // Act
        var traumaConfig = new TraumaEconomyConfiguration(configuration);

        // Assert - Damage Integration defaults
        traumaConfig.DamageToStressEnabled.Should().BeTrue();
        traumaConfig.DamageToStressFormula.Should().Be("floor(damage / 10)");
        traumaConfig.CriticalHitStressBonus.Should().Be(5);
        traumaConfig.NearDeathStressBonus.Should().Be(10);
        traumaConfig.AllyDeathStressBonus.Should().Be(15);

        // Assert - Rest Recovery defaults
        traumaConfig.ShortRestRageReset.Should().Be(0);
        traumaConfig.ShortRestMomentumReset.Should().Be(0);
        traumaConfig.LongRestCoherenceRestore.Should().Be(50);

        // Assert - Turn Effects defaults
        traumaConfig.RageDecayOutOfCombat.Should().Be(10);
        traumaConfig.MomentumDecayIdle.Should().Be(15);
        traumaConfig.ApotheosisStressCost.Should().Be(10);
        traumaConfig.MaxEnvironmentalStress.Should().Be(5);

        // Assert - Threshold defaults
        traumaConfig.CriticalWarningThreshold.Should().Be(80);
        traumaConfig.TerminalTriggerThreshold.Should().Be(100);
    }

    [Test]
    public void Constructor_WithEmptyConfiguration_UsesDefaultWarningMessages()
    {
        // Arrange
        var configurationBuilder = new ConfigurationBuilder();
        var configuration = configurationBuilder.Build();

        // Act
        var traumaConfig = new TraumaEconomyConfiguration(configuration);

        // Assert - Messages are thematic rather than literal
        traumaConfig.StressHighMessage.Should().NotBeNullOrEmpty();
        traumaConfig.CorruptionRisingMessage.Should().Contain("corruption");
        traumaConfig.RageOverflowWarning.Should().Contain("rage");
        traumaConfig.MomentumCriticalMessage.Should().Contain("momentum");
        traumaConfig.CoherenceCriticalMessage.Should().NotBeNullOrEmpty();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Configuration Override Tests
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void Constructor_WithCustomDamageIntegration_OverridesDefaults()
    {
        // Arrange
        var inMemorySettings = new Dictionary<string, string?>
        {
            { "traumaEconomy:integration:damageToStress:enabled", "false" },
            { "traumaEconomy:integration:damageToStress:criticalBonus", "10" },
            { "traumaEconomy:integration:damageToStress:nearDeathBonus", "20" }
        };
        
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        // Act
        var traumaConfig = new TraumaEconomyConfiguration(configuration);

        // Assert
        traumaConfig.DamageToStressEnabled.Should().BeFalse();
        traumaConfig.CriticalHitStressBonus.Should().Be(10);
        traumaConfig.NearDeathStressBonus.Should().Be(20);
    }

    [Test]
    public void Constructor_WithCustomThresholds_OverridesDefaults()
    {
        // Arrange
        var inMemorySettings = new Dictionary<string, string?>
        {
            { "traumaEconomy:thresholds:criticalWarning", "75" },
            { "traumaEconomy:thresholds:terminalTrigger", "95" }
        };
        
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        // Act
        var traumaConfig = new TraumaEconomyConfiguration(configuration);

        // Assert
        traumaConfig.CriticalWarningThreshold.Should().Be(75);
        traumaConfig.TerminalTriggerThreshold.Should().Be(95);
    }

    [Test]
    public void Constructor_WithCustomWarningMessages_OverridesDefaults()
    {
        // Arrange
        var customMessage = "Custom high stress warning!";
        var inMemorySettings = new Dictionary<string, string?>
        {
            { "traumaEconomy:warningMessages:stressHigh", customMessage }
        };
        
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        // Act
        var traumaConfig = new TraumaEconomyConfiguration(configuration);

        // Assert
        traumaConfig.StressHighMessage.Should().Be(customMessage);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Validation Tests
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void Constructor_WithValidConfiguration_DoesNotThrow()
    {
        // Arrange
        var configurationBuilder = new ConfigurationBuilder();

        // Act
        Action act = () => new TraumaEconomyConfiguration(configurationBuilder.Build());

        // Assert
        act.Should().NotThrow();
    }

    [Test]
    public void Constructor_WithInvalidCriticalThreshold_ThrowsInvalidOperationException()
    {
        // Arrange
        var inMemorySettings = new Dictionary<string, string?>
        {
            { "traumaEconomy:thresholds:criticalWarning", "150" } // Over 100
        };
        
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        // Act
        Action act = () => new TraumaEconomyConfiguration(configuration);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*CriticalWarningThreshold*");
    }

    [Test]
    public void Constructor_WithInvalidTerminalThreshold_ThrowsInvalidOperationException()
    {
        // Arrange
        var inMemorySettings = new Dictionary<string, string?>
        {
            { "traumaEconomy:thresholds:terminalTrigger", "-5" } // Negative
        };
        
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        // Act
        Action act = () => new TraumaEconomyConfiguration(configuration);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*TerminalTriggerThreshold*");
    }

    [Test]
    public void Constructor_WithNegativeRageDecay_ThrowsInvalidOperationException()
    {
        // Arrange
        var inMemorySettings = new Dictionary<string, string?>
        {
            { "traumaEconomy:integration:turnEffects:rageDecayOutOfCombat", "-10" } // Negative
        };
        
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        // Act
        Action act = () => new TraumaEconomyConfiguration(configuration);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*RageDecayOutOfCombat*");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Rest Recovery Tests
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void Constructor_WithCustomRestRecovery_OverridesDefaults()
    {
        // Arrange
        var inMemorySettings = new Dictionary<string, string?>
        {
            { "traumaEconomy:integration:restRecovery:shortRest:rageReset", "25" },
            { "traumaEconomy:integration:restRecovery:shortRest:momentumReset", "30" },
            { "traumaEconomy:integration:restRecovery:longRest:coherenceRestore", "75" }
        };
        
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        // Act
        var traumaConfig = new TraumaEconomyConfiguration(configuration);

        // Assert
        traumaConfig.ShortRestRageReset.Should().Be(25);
        traumaConfig.ShortRestMomentumReset.Should().Be(30);
        traumaConfig.LongRestCoherenceRestore.Should().Be(75);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Turn Effects Tests
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void Constructor_WithCustomTurnEffects_OverridesDefaults()
    {
        // Arrange
        var inMemorySettings = new Dictionary<string, string?>
        {
            { "traumaEconomy:integration:turnEffects:rageDecayOutOfCombat", "15" },
            { "traumaEconomy:integration:turnEffects:momentumDecayIdle", "20" },
            { "traumaEconomy:integration:turnEffects:apotheosisStressCost", "15" },
            { "traumaEconomy:integration:turnEffects:maxEnvironmentalStress", "10" }
        };
        
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        // Act
        var traumaConfig = new TraumaEconomyConfiguration(configuration);

        // Assert
        traumaConfig.RageDecayOutOfCombat.Should().Be(15);
        traumaConfig.MomentumDecayIdle.Should().Be(20);
        traumaConfig.ApotheosisStressCost.Should().Be(15);
        traumaConfig.MaxEnvironmentalStress.Should().Be(10);
    }
}
