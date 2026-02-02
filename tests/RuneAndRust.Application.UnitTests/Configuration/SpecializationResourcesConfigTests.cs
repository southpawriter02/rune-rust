using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Application.Configuration;

namespace RuneAndRust.Application.UnitTests.Configuration;

/// <summary>
/// Unit tests for specialization resource configuration loading and behavior.
/// </summary>
[TestFixture]
public class SpecializationResourcesConfigTests
{
    // ===== Default Value Tests =====

    [Test]
    public void RageConfigurationSection_DefaultValues_AreCorrect()
    {
        // Arrange & Act
        var config = new RageConfigurationSection();

        // Assert
        config.MaxValue.Should().Be(100);
        config.MinValue.Should().Be(0);
        config.DecayPerTurn.Should().Be(10);
        config.DecayMinutesBeforeNonCombat.Should().Be(1);
        config.Thresholds.Should().BeEmpty();
        config.Sources.Should().BeEmpty();
    }

    [Test]
    public void MomentumConfigurationSection_DefaultValues_AreCorrect()
    {
        // Arrange & Act
        var config = new MomentumConfigurationSection();

        // Assert
        config.MaxValue.Should().Be(100);
        config.MinValue.Should().Be(0);
        config.DecayOnMiss.Should().Be(25);
        config.DecayOnStun.Should().Be(100);
        config.DecayOnIdleTurn.Should().Be(15);
        config.Thresholds.Should().BeEmpty();
        config.Sources.Should().BeEmpty();
    }

    [Test]
    public void CoherenceConfigurationSection_DefaultValues_AreCorrect()
    {
        // Arrange & Act
        var config = new CoherenceConfigurationSection();

        // Assert
        config.MaxValue.Should().Be(100);
        config.MinValue.Should().Be(0);
        config.DefaultValue.Should().Be(50);
        config.MeditationGain.Should().Be(20);
        config.CastGain.Should().Be(5);
        config.ChannelGainPerTurn.Should().Be(3);
        config.ApotheosisStressCost.Should().Be(10);
        config.Thresholds.Should().BeEmpty();
        config.Sources.Should().BeEmpty();
    }

    // ===== Threshold Lookup Tests =====

    [Test]
    public void RageConfigurationSection_GetThresholdForValue_ReturnsCorrectThreshold()
    {
        // Arrange
        var config = new RageConfigurationSection
        {
            Thresholds =
            [
                new RageThresholdConfig { Name = "Calm", MinValue = 0, MaxValue = 20, DamageBonus = 0 },
                new RageThresholdConfig { Name = "Simmering", MinValue = 21, MaxValue = 40, DamageBonus = 3 },
                new RageThresholdConfig { Name = "Burning", MinValue = 41, MaxValue = 60, DamageBonus = 5 }
            ]
        };

        // Act & Assert
        config.GetThresholdForValue(10)?.Name.Should().Be("Calm");
        config.GetThresholdForValue(35)?.Name.Should().Be("Simmering");
        config.GetThresholdForValue(55)?.Name.Should().Be("Burning");
        config.GetThresholdForValue(100).Should().BeNull(); // Outside defined thresholds
    }

    [Test]
    public void MomentumConfigurationSection_GetThresholdForValue_ReturnsCorrectThreshold()
    {
        // Arrange
        var config = new MomentumConfigurationSection
        {
            Thresholds =
            [
                new MomentumThresholdConfig { Name = "Stationary", MinValue = 0, MaxValue = 20, BonusAttacks = 0 },
                new MomentumThresholdConfig { Name = "Moving", MinValue = 21, MaxValue = 40, BonusAttacks = 0 },
                new MomentumThresholdConfig { Name = "Flowing", MinValue = 41, MaxValue = 60, BonusAttacks = 1 }
            ]
        };

        // Act & Assert
        config.GetThresholdForValue(15)?.Name.Should().Be("Stationary");
        config.GetThresholdForValue(30)?.Name.Should().Be("Moving");
        config.GetThresholdForValue(50)?.Name.Should().Be("Flowing");
    }

    [Test]
    public void CoherenceConfigurationSection_GetThresholdForValue_ReturnsCorrectThreshold()
    {
        // Arrange
        var config = new CoherenceConfigurationSection
        {
            Thresholds =
            [
                new CoherenceThresholdConfig { Name = "Destabilized", MinValue = 0, MaxValue = 20, CascadeRisk = 25 },
                new CoherenceThresholdConfig { Name = "Balanced", MinValue = 41, MaxValue = 60, CascadeRisk = 0 },
                new CoherenceThresholdConfig { Name = "Apotheosis", MinValue = 81, MaxValue = 100, UltimateAbilitiesEnabled = true }
            ]
        };

        // Act & Assert
        config.GetThresholdForValue(10)?.Name.Should().Be("Destabilized");
        config.GetThresholdForValue(10)?.CascadeRisk.Should().Be(25);
        config.GetThresholdForValue(50)?.Name.Should().Be("Balanced");
        config.GetThresholdForValue(90)?.Name.Should().Be("Apotheosis");
        config.GetThresholdForValue(90)?.UltimateAbilitiesEnabled.Should().BeTrue();
    }

    // ===== Root Configuration Tests =====

    [Test]
    public void SpecializationResourceConfiguration_DefaultSections_AreNotNull()
    {
        // Arrange & Act
        var config = new SpecializationResourceConfiguration();

        // Assert
        config.Rage.Should().NotBeNull();
        config.Momentum.Should().NotBeNull();
        config.Coherence.Should().NotBeNull();
        config.Version.Should().BeEmpty();
    }

    [Test]
    public void ResourceSourceConfig_FlatType_HasValue()
    {
        // Arrange & Act
        var source = new ResourceSourceConfig
        {
            Type = "flat",
            Value = 10,
            Description = "Flat gain"
        };

        // Assert
        source.Type.Should().Be("flat");
        source.Value.Should().Be(10);
        source.Formula.Should().BeNull();
    }

    [Test]
    public void ResourceSourceConfig_FormulaType_HasFormula()
    {
        // Arrange & Act
        var source = new ResourceSourceConfig
        {
            Type = "formula",
            Formula = "floor(damage / 5)",
            Description = "Rage from damage"
        };

        // Assert
        source.Type.Should().Be("formula");
        source.Formula.Should().Be("floor(damage / 5)");
        source.Value.Should().BeNull();
    }

    [Test]
    public void RageThresholdConfig_BerserkerFury_HasExpectedConstraints()
    {
        // Arrange & Act
        var threshold = new RageThresholdConfig
        {
            Name = "BerserkFury",
            MinValue = 61,
            MaxValue = 80,
            DamageBonus = 7,
            SoakBonus = 3,
            FearImmune = false,
            MustAttackNearest = true
        };

        // Assert
        threshold.MustAttackNearest.Should().BeTrue("BerserkFury forces attacking nearest target");
        threshold.FearImmune.Should().BeFalse("Fear immunity only at FrenzyBeyondReason");
        threshold.DamageBonus.Should().Be(7);
    }

    [Test]
    public void CoherenceThresholdConfig_Apotheosis_EnablesUltimateAbilities()
    {
        // Arrange & Act
        var threshold = new CoherenceThresholdConfig
        {
            Name = "Apotheosis",
            MinValue = 81,
            MaxValue = 100,
            SpellPowerBonus = 5,
            CriticalCastChance = 20,
            CascadeRisk = 0,
            UltimateAbilitiesEnabled = true
        };

        // Assert
        threshold.UltimateAbilitiesEnabled.Should().BeTrue();
        threshold.CascadeRisk.Should().Be(0, "Apotheosis has no cascade risk");
        threshold.SpellPowerBonus.Should().Be(5);
    }
}
