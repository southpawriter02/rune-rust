using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using RuneAndRust.Domain.Definitions;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Infrastructure.Providers;

#pragma warning disable NUnit2045 // Use Assert.Multiple

namespace RuneAndRust.Application.UnitTests.Providers;

/// <summary>
/// Unit tests for JsonComboProvider and related combo definition entities.
/// </summary>
/// <remarks>
/// <para>Tests cover:</para>
/// <list type="bullet">
///   <item><description>ComboDefinition creation and validation</description></item>
///   <item><description>ComboStep matching logic</description></item>
///   <item><description>ComboBonusEffect description generation</description></item>
///   <item><description>Provider loading and querying functionality</description></item>
/// </list>
/// </remarks>
[TestFixture]
public class JsonComboProviderTests
{
    // ═══════════════════════════════════════════════════════════════
    // TEST FIXTURES
    // ═══════════════════════════════════════════════════════════════

    private Mock<ILogger<JsonComboProvider>> _mockLogger = null!;
    private string _testConfigPath = null!;

    /// <summary>
    /// Sets up test dependencies before each test.
    /// </summary>
    [SetUp]
    public void SetUp()
    {
        _mockLogger = new Mock<ILogger<JsonComboProvider>>();

        // Use the actual config path for integration-style tests
        var baseDir = TestContext.CurrentContext.TestDirectory;
        _testConfigPath = Path.Combine(baseDir, "..", "..", "..", "..", "..", "config", "combos.json");
    }

    // ═══════════════════════════════════════════════════════════════
    // COMBO DEFINITION TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that ComboDefinition.Create creates a valid combo with correct properties.
    /// </summary>
    [Test]
    public void Create_ValidCombo_ReturnsComboDefinition()
    {
        // Arrange
        var steps = new[]
        {
            new ComboStep { StepNumber = 1, AbilityId = "fire-bolt" },
            new ComboStep { StepNumber = 2, AbilityId = "ice-shard" }
        };

        var bonusEffects = new[]
        {
            new ComboBonusEffect
            {
                EffectType = ComboBonusType.DamageMultiplier,
                Value = "2.0",
                Target = ComboBonusTarget.LastTarget
            }
        };

        // Act
        var combo = ComboDefinition.Create(
            comboId: "test-combo",
            name: "Test Combo",
            description: "A test combo",
            windowTurns: 3,
            steps: steps,
            bonusEffects: bonusEffects,
            requiredClassIds: new[] { "mage" },
            iconPath: "icons/test.png");

        // Assert
        combo.Should().NotBeNull();
        combo.ComboId.Should().Be("test-combo");
        combo.Name.Should().Be("Test Combo");
        combo.Description.Should().Be("A test combo");
        combo.WindowTurns.Should().Be(3);
        combo.StepCount.Should().Be(2);
        combo.Steps.Should().HaveCount(2);
        combo.BonusEffects.Should().HaveCount(1);
        combo.RequiredClassIds.Should().ContainSingle().Which.Should().Be("mage");
        combo.IconPath.Should().Be("icons/test.png");
        combo.FirstAbilityId.Should().Be("fire-bolt");
    }

    /// <summary>
    /// Verifies that ComboDefinition.Create throws when fewer than 2 steps are provided.
    /// </summary>
    [Test]
    public void Create_LessThanTwoSteps_ThrowsArgumentException()
    {
        // Arrange
        var steps = new[]
        {
            new ComboStep { StepNumber = 1, AbilityId = "fire-bolt" }
        };

        // Act
        var act = () => ComboDefinition.Create(
            comboId: "test-combo",
            name: "Test Combo",
            description: "A test combo",
            windowTurns: 3,
            steps: steps);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*at least 2 steps*");
    }

    /// <summary>
    /// Verifies that StepCount returns the correct number of steps.
    /// </summary>
    [Test]
    public void StepCount_ReturnsCorrectCount()
    {
        // Arrange
        var steps = new[]
        {
            new ComboStep { StepNumber = 1, AbilityId = "ability-1" },
            new ComboStep { StepNumber = 2, AbilityId = "ability-2" },
            new ComboStep { StepNumber = 3, AbilityId = "ability-3" }
        };

        var combo = ComboDefinition.Create(
            comboId: "test",
            name: "Test",
            description: "Test",
            windowTurns: 3,
            steps: steps);

        // Act & Assert
        combo.StepCount.Should().Be(3);
    }

    /// <summary>
    /// Verifies that GetAbilityForStep returns the correct ability ID.
    /// </summary>
    [Test]
    public void GetAbilityForStep_ValidStep_ReturnsAbilityId()
    {
        // Arrange
        var steps = new[]
        {
            new ComboStep { StepNumber = 1, AbilityId = "first-ability" },
            new ComboStep { StepNumber = 2, AbilityId = "second-ability" }
        };

        var combo = ComboDefinition.Create(
            comboId: "test",
            name: "Test",
            description: "Test",
            windowTurns: 2,
            steps: steps);

        // Act & Assert
        combo.GetAbilityForStep(1).Should().Be("first-ability");
        combo.GetAbilityForStep(2).Should().Be("second-ability");
        combo.GetAbilityForStep(99).Should().BeNull();
    }

    /// <summary>
    /// Verifies that ContainsAbility returns true when the ability is present.
    /// </summary>
    [Test]
    public void ContainsAbility_AbilityPresent_ReturnsTrue()
    {
        // Arrange
        var steps = new[]
        {
            new ComboStep { StepNumber = 1, AbilityId = "fire-bolt" },
            new ComboStep { StepNumber = 2, AbilityId = "ice-shard" }
        };

        var combo = ComboDefinition.Create(
            comboId: "test",
            name: "Test",
            description: "Test",
            windowTurns: 2,
            steps: steps);

        // Act & Assert
        combo.ContainsAbility("fire-bolt").Should().BeTrue();
        combo.ContainsAbility("FIRE-BOLT").Should().BeTrue(); // Case-insensitive
        combo.ContainsAbility("ice-shard").Should().BeTrue();
    }

    /// <summary>
    /// Verifies that ContainsAbility returns false when the ability is not present.
    /// </summary>
    [Test]
    public void ContainsAbility_AbilityMissing_ReturnsFalse()
    {
        // Arrange
        var steps = new[]
        {
            new ComboStep { StepNumber = 1, AbilityId = "fire-bolt" },
            new ComboStep { StepNumber = 2, AbilityId = "ice-shard" }
        };

        var combo = ComboDefinition.Create(
            comboId: "test",
            name: "Test",
            description: "Test",
            windowTurns: 2,
            steps: steps);

        // Act & Assert
        combo.ContainsAbility("lightning").Should().BeFalse();
        combo.ContainsAbility("").Should().BeFalse();
    }

    /// <summary>
    /// Verifies that IsAvailableForClass returns true when the class matches.
    /// </summary>
    [Test]
    public void IsAvailableForClass_MatchingClass_ReturnsTrue()
    {
        // Arrange
        var steps = new[]
        {
            new ComboStep { StepNumber = 1, AbilityId = "ability-1" },
            new ComboStep { StepNumber = 2, AbilityId = "ability-2" }
        };

        var combo = ComboDefinition.Create(
            comboId: "test",
            name: "Test",
            description: "Test",
            windowTurns: 2,
            steps: steps,
            requiredClassIds: new[] { "mage", "sorcerer" });

        // Act & Assert
        combo.IsAvailableForClass("mage").Should().BeTrue();
        combo.IsAvailableForClass("MAGE").Should().BeTrue(); // Case-insensitive
        combo.IsAvailableForClass("sorcerer").Should().BeTrue();
        combo.IsAvailableForClass("warrior").Should().BeFalse();
    }

    /// <summary>
    /// Verifies that IsAvailableForClass returns true for all classes when RequiredClassIds is empty.
    /// </summary>
    [Test]
    public void IsAvailableForClass_EmptyClassList_ReturnsTrue()
    {
        // Arrange
        var steps = new[]
        {
            new ComboStep { StepNumber = 1, AbilityId = "ability-1" },
            new ComboStep { StepNumber = 2, AbilityId = "ability-2" }
        };

        var combo = ComboDefinition.Create(
            comboId: "test",
            name: "Test",
            description: "Test",
            windowTurns: 2,
            steps: steps,
            requiredClassIds: null); // No class restrictions

        // Act & Assert
        combo.IsAvailableForClass("mage").Should().BeTrue();
        combo.IsAvailableForClass("warrior").Should().BeTrue();
        combo.IsAvailableForClass("rogue").Should().BeTrue();
    }

    // ═══════════════════════════════════════════════════════════════
    // COMBO STEP TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that ComboStep.Matches correctly checks ability ID.
    /// </summary>
    [Test]
    public void ComboStep_Matches_ChecksAbilityId()
    {
        // Arrange
        var step = new ComboStep
        {
            StepNumber = 1,
            AbilityId = "fire-bolt",
            TargetRequirement = ComboTargetRequirement.Any
        };

        // Act & Assert
        step.Matches("fire-bolt", isSameTarget: true, isSelfTarget: false).Should().BeTrue();
        step.Matches("FIRE-BOLT", isSameTarget: true, isSelfTarget: false).Should().BeTrue(); // Case-insensitive
        step.Matches("ice-shard", isSameTarget: true, isSelfTarget: false).Should().BeFalse();
    }

    /// <summary>
    /// Verifies that ComboStep.Matches correctly validates target requirements.
    /// </summary>
    [Test]
    public void ComboStep_Matches_ChecksTargetRequirement()
    {
        // Arrange - Same target requirement
        var sameTargetStep = new ComboStep
        {
            StepNumber = 2,
            AbilityId = "ice-shard",
            TargetRequirement = ComboTargetRequirement.SameTarget
        };

        // Act & Assert - SameTarget
        sameTargetStep.Matches("ice-shard", isSameTarget: true, isSelfTarget: false).Should().BeTrue();
        sameTargetStep.Matches("ice-shard", isSameTarget: false, isSelfTarget: false).Should().BeFalse();

        // Arrange - Different target requirement
        var differentTargetStep = new ComboStep
        {
            StepNumber = 2,
            AbilityId = "cleave",
            TargetRequirement = ComboTargetRequirement.DifferentTarget
        };

        // Act & Assert - DifferentTarget
        differentTargetStep.Matches("cleave", isSameTarget: false, isSelfTarget: false).Should().BeTrue();
        differentTargetStep.Matches("cleave", isSameTarget: true, isSelfTarget: false).Should().BeFalse();

        // Arrange - Self target requirement
        var selfTargetStep = new ComboStep
        {
            StepNumber = 2,
            AbilityId = "vanish",
            TargetRequirement = ComboTargetRequirement.Self
        };

        // Act & Assert - Self
        selfTargetStep.Matches("vanish", isSameTarget: false, isSelfTarget: true).Should().BeTrue();
        selfTargetStep.Matches("vanish", isSameTarget: false, isSelfTarget: false).Should().BeFalse();
    }

    // ═══════════════════════════════════════════════════════════════
    // COMBO BONUS EFFECT TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that ComboBonusEffect.GetDescription returns correct descriptions.
    /// </summary>
    [Test]
    public void ComboBonusEffect_GetDescription_ReturnsCorrectDescription()
    {
        // ExtraDamage
        var extraDamage = new ComboBonusEffect
        {
            EffectType = ComboBonusType.ExtraDamage,
            Value = "4d6",
            DamageType = "piercing"
        };
        extraDamage.GetDescription().Should().Be("+4d6 piercing damage");

        // DamageMultiplier
        var damageMultiplier = new ComboBonusEffect
        {
            EffectType = ComboBonusType.DamageMultiplier,
            Value = "2.0"
        };
        damageMultiplier.GetDescription().Should().Be("x2.0 damage");

        // ApplyStatus
        var applyStatus = new ComboBonusEffect
        {
            EffectType = ComboBonusType.ApplyStatus,
            Value = "",
            StatusEffectId = "stunned"
        };
        applyStatus.GetDescription().Should().Be("Apply stunned");

        // Heal
        var heal = new ComboBonusEffect
        {
            EffectType = ComboBonusType.Heal,
            Value = "2d8"
        };
        heal.GetDescription().Should().Be("Heal 2d8");

        // ResetCooldown
        var resetCooldown = new ComboBonusEffect
        {
            EffectType = ComboBonusType.ResetCooldown,
            Value = "vanish"
        };
        resetCooldown.GetDescription().Should().Be("Reset vanish cooldown");

        // AreaEffect
        var areaEffect = new ComboBonusEffect
        {
            EffectType = ComboBonusType.AreaEffect,
            Value = "2"
        };
        areaEffect.GetDescription().Should().Be("Expand to 2 cell radius");
    }

    // ═══════════════════════════════════════════════════════════════
    // JSON COMBO PROVIDER TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that the provider loads all combos from the config file.
    /// </summary>
    [Test]
    public void GetAllCombos_ReturnsAllLoadedCombos()
    {
        // Skip if config file doesn't exist (CI environment)
        if (!File.Exists(_testConfigPath))
        {
            Assert.Ignore("Config file not found at expected path. Skipping integration test.");
            return;
        }

        // Arrange
        var provider = new JsonComboProvider(_testConfigPath, _mockLogger.Object);

        // Act
        var combos = provider.GetAllCombos();

        // Assert
        combos.Should().NotBeEmpty();
        combos.Should().HaveCountGreaterOrEqualTo(4); // We defined 4 combos in config
        combos.Should().Contain(c => c.ComboId == "elemental-burst");
        combos.Should().Contain(c => c.ComboId == "warriors-onslaught");
        combos.Should().Contain(c => c.ComboId == "assassins-dance");
        combos.Should().Contain(c => c.ComboId == "divine-judgment");
    }

    /// <summary>
    /// Verifies that GetCombosForClass filters combos correctly.
    /// </summary>
    [Test]
    public void GetCombosForClass_FiltersCorrectly()
    {
        // Skip if config file doesn't exist (CI environment)
        if (!File.Exists(_testConfigPath))
        {
            Assert.Ignore("Config file not found at expected path. Skipping integration test.");
            return;
        }

        // Arrange
        var provider = new JsonComboProvider(_testConfigPath, _mockLogger.Object);

        // Act
        var mageCombos = provider.GetCombosForClass("mage");
        var warriorCombos = provider.GetCombosForClass("warrior");
        var paladinCombos = provider.GetCombosForClass("paladin");

        // Assert
        mageCombos.Should().Contain(c => c.ComboId == "elemental-burst");
        mageCombos.Should().NotContain(c => c.ComboId == "warriors-onslaught");

        warriorCombos.Should().Contain(c => c.ComboId == "warriors-onslaught");
        warriorCombos.Should().NotContain(c => c.ComboId == "elemental-burst");

        // Paladin can use both warrior's onslaught and divine judgment
        paladinCombos.Should().Contain(c => c.ComboId == "warriors-onslaught");
        paladinCombos.Should().Contain(c => c.ComboId == "divine-judgment");
    }

    /// <summary>
    /// Verifies that GetCombosStartingWith returns combos indexed by first ability.
    /// </summary>
    [Test]
    public void GetCombosStartingWith_ReturnsIndexedCombos()
    {
        // Skip if config file doesn't exist (CI environment)
        if (!File.Exists(_testConfigPath))
        {
            Assert.Ignore("Config file not found at expected path. Skipping integration test.");
            return;
        }

        // Arrange
        var provider = new JsonComboProvider(_testConfigPath, _mockLogger.Object);

        // Act
        var fireStartCombos = provider.GetCombosStartingWith("fire-bolt");
        var chargeStartCombos = provider.GetCombosStartingWith("charge");
        var unknownStartCombos = provider.GetCombosStartingWith("unknown-ability");

        // Assert
        fireStartCombos.Should().Contain(c => c.ComboId == "elemental-burst");
        chargeStartCombos.Should().Contain(c => c.ComboId == "warriors-onslaught");
        unknownStartCombos.Should().BeEmpty();
    }

    /// <summary>
    /// Verifies that GetCombo returns the correct combo by ID.
    /// </summary>
    [Test]
    public void GetCombo_ExistingId_ReturnsCombo()
    {
        // Skip if config file doesn't exist (CI environment)
        if (!File.Exists(_testConfigPath))
        {
            Assert.Ignore("Config file not found at expected path. Skipping integration test.");
            return;
        }

        // Arrange
        var provider = new JsonComboProvider(_testConfigPath, _mockLogger.Object);

        // Act
        var combo = provider.GetCombo("elemental-burst");

        // Assert
        combo.Should().NotBeNull();
        combo!.Name.Should().Be("Elemental Burst");
        combo.WindowTurns.Should().Be(3);
        combo.StepCount.Should().Be(3);
        combo.BonusEffects.Should().HaveCount(2);
    }

    /// <summary>
    /// Verifies that GetCombo returns null for unknown combo IDs.
    /// </summary>
    [Test]
    public void GetCombo_UnknownId_ReturnsNull()
    {
        // Skip if config file doesn't exist (CI environment)
        if (!File.Exists(_testConfigPath))
        {
            Assert.Ignore("Config file not found at expected path. Skipping integration test.");
            return;
        }

        // Arrange
        var provider = new JsonComboProvider(_testConfigPath, _mockLogger.Object);

        // Act
        var combo = provider.GetCombo("nonexistent-combo");

        // Assert
        combo.Should().BeNull();
    }
}
