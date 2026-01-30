// ═══════════════════════════════════════════════════════════════════════════════
// SpecializationAbilityTests.cs
// Unit tests for the SpecializationAbility value object, verifying factory method
// validation, computed properties, normalization, and display formatting.
// Version: 0.17.4c
// ═══════════════════════════════════════════════════════════════════════════════

using FluentAssertions;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

[TestFixture]
public class SpecializationAbilityTests
{
    // ═══════════════════════════════════════════════════════════════════════════
    // CREATE — VALID PARAMETERS
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void Create_WithValidActiveAbility_CreatesAbility()
    {
        // Arrange & Act
        var ability = SpecializationAbility.Create(
            "rage-strike",
            "Rage Strike",
            "Channel rage into a devastating blow.",
            isPassive: false,
            resourceCost: 20,
            resourceType: "rage",
            cooldown: 0,
            corruptionRisk: 5);

        // Assert
        ability.AbilityId.Should().Be("rage-strike");
        ability.DisplayName.Should().Be("Rage Strike");
        ability.Description.Should().Be("Channel rage into a devastating blow.");
        ability.IsPassive.Should().BeFalse();
        ability.ResourceCost.Should().Be(20);
        ability.ResourceType.Should().Be("rage");
        ability.Cooldown.Should().Be(0);
        ability.CorruptionRisk.Should().Be(5);
    }

    [Test]
    public void Create_WithValidPassiveAbility_CreatesAbility()
    {
        // Arrange & Act
        var ability = SpecializationAbility.Create(
            "blood-frenzy",
            "Blood Frenzy",
            "When below 50% health, deal 10% increased damage.",
            isPassive: true);

        // Assert
        ability.AbilityId.Should().Be("blood-frenzy");
        ability.DisplayName.Should().Be("Blood Frenzy");
        ability.IsPassive.Should().BeTrue();
        ability.ResourceCost.Should().Be(0);
        ability.ResourceType.Should().BeEmpty();
        ability.Cooldown.Should().Be(0);
        ability.CorruptionRisk.Should().Be(0);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // CREATE ACTIVE — CONVENIENCE FACTORY
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void CreateActive_WithValidParameters_CreatesActiveAbility()
    {
        // Arrange & Act
        var ability = SpecializationAbility.CreateActive(
            "rage-strike",
            "Rage Strike",
            "Channel rage into a devastating blow.",
            resourceCost: 20,
            resourceType: "rage",
            cooldown: 0,
            corruptionRisk: 5);

        // Assert
        ability.IsPassive.Should().BeFalse();
        ability.IsActive.Should().BeTrue();
        ability.AbilityId.Should().Be("rage-strike");
        ability.ResourceCost.Should().Be(20);
        ability.ResourceType.Should().Be("rage");
        ability.CorruptionRisk.Should().Be(5);
    }

    [Test]
    public void CreateActive_WithCooldown_CreatesAbilityWithCooldown()
    {
        // Arrange & Act
        var ability = SpecializationAbility.CreateActive(
            "berserker-charge",
            "Berserker Charge",
            "Rush toward an enemy.",
            resourceCost: 30,
            resourceType: "rage",
            cooldown: 4,
            corruptionRisk: 10);

        // Assert
        ability.Cooldown.Should().Be(4);
        ability.HasCooldown.Should().BeTrue();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // CREATE PASSIVE — CONVENIENCE FACTORY
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void CreatePassive_WithValidParameters_CreatesPassiveAbility()
    {
        // Arrange & Act
        var ability = SpecializationAbility.CreatePassive(
            "blood-frenzy",
            "Blood Frenzy",
            "When below 50% health, deal 10% increased damage.");

        // Assert
        ability.IsPassive.Should().BeTrue();
        ability.IsActive.Should().BeFalse();
        ability.ResourceCost.Should().Be(0);
        ability.ResourceType.Should().BeEmpty();
        ability.Cooldown.Should().Be(0);
        ability.CorruptionRisk.Should().Be(0);
    }

    [Test]
    public void CreatePassive_WithCorruptionRisk_CreatesPassiveWithRisk()
    {
        // Arrange & Act
        var ability = SpecializationAbility.CreatePassive(
            "deaths-door-defiance",
            "Death's Door Defiance",
            "Survive a fatal blow with 1 HP.",
            corruptionRisk: 15);

        // Assert
        ability.IsPassive.Should().BeTrue();
        ability.CorruptionRisk.Should().Be(15);
        ability.RisksCorruption.Should().BeTrue();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // VALIDATION — NULL / WHITESPACE STRINGS
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void Create_WithNullAbilityId_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => SpecializationAbility.Create(
            null!,
            "Rage Strike",
            "A powerful blow.",
            isPassive: false);

        // Assert
        act.Should().Throw<ArgumentException>()
            .And.ParamName.Should().Be("abilityId");
    }

    [Test]
    public void Create_WithEmptyAbilityId_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => SpecializationAbility.Create(
            "",
            "Rage Strike",
            "A powerful blow.",
            isPassive: false);

        // Assert
        act.Should().Throw<ArgumentException>()
            .And.ParamName.Should().Be("abilityId");
    }

    [Test]
    public void Create_WithWhitespaceAbilityId_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => SpecializationAbility.Create(
            "   ",
            "Rage Strike",
            "A powerful blow.",
            isPassive: false);

        // Assert
        act.Should().Throw<ArgumentException>()
            .And.ParamName.Should().Be("abilityId");
    }

    [Test]
    public void Create_WithNullDisplayName_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => SpecializationAbility.Create(
            "rage-strike",
            null!,
            "A powerful blow.",
            isPassive: false);

        // Assert
        act.Should().Throw<ArgumentException>()
            .And.ParamName.Should().Be("displayName");
    }

    [Test]
    public void Create_WithNullDescription_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => SpecializationAbility.Create(
            "rage-strike",
            "Rage Strike",
            null!,
            isPassive: false);

        // Assert
        act.Should().Throw<ArgumentException>()
            .And.ParamName.Should().Be("description");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // VALIDATION — NEGATIVE NUMERIC VALUES
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void Create_WithNegativeResourceCost_ThrowsArgumentOutOfRangeException()
    {
        // Arrange & Act
        var act = () => SpecializationAbility.Create(
            "rage-strike",
            "Rage Strike",
            "A powerful blow.",
            isPassive: false,
            resourceCost: -1);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .And.ParamName.Should().Be("resourceCost");
    }

    [Test]
    public void Create_WithNegativeCooldown_ThrowsArgumentOutOfRangeException()
    {
        // Arrange & Act
        var act = () => SpecializationAbility.Create(
            "rage-strike",
            "Rage Strike",
            "A powerful blow.",
            isPassive: false,
            cooldown: -1);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .And.ParamName.Should().Be("cooldown");
    }

    [Test]
    public void Create_WithNegativeCorruptionRisk_ThrowsArgumentOutOfRangeException()
    {
        // Arrange & Act
        var act = () => SpecializationAbility.Create(
            "rage-strike",
            "Rage Strike",
            "A powerful blow.",
            isPassive: false,
            corruptionRisk: -1);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .And.ParamName.Should().Be("corruptionRisk");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // VALIDATION — PASSIVE COOLDOWN CONSTRAINT
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void Create_PassiveWithCooldown_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => SpecializationAbility.Create(
            "blood-frenzy",
            "Blood Frenzy",
            "When below 50% health, deal 10% increased damage.",
            isPassive: true,
            cooldown: 3);

        // Assert
        act.Should().Throw<ArgumentException>()
            .And.ParamName.Should().Be("cooldown");
    }

    [Test]
    public void Create_ActiveWithCooldown_DoesNotThrow()
    {
        // Arrange & Act
        var act = () => SpecializationAbility.Create(
            "berserker-charge",
            "Berserker Charge",
            "Rush toward an enemy.",
            isPassive: false,
            cooldown: 4);

        // Assert
        act.Should().NotThrow();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // NORMALIZATION
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void Create_NormalizesAbilityIdToLowercase()
    {
        // Arrange & Act
        var ability = SpecializationAbility.Create(
            "Rage-Strike",
            "Rage Strike",
            "A powerful blow.",
            isPassive: false);

        // Assert
        ability.AbilityId.Should().Be("rage-strike");
    }

    [Test]
    public void Create_NormalizesResourceTypeToLowercase()
    {
        // Arrange & Act
        var ability = SpecializationAbility.Create(
            "rage-strike",
            "Rage Strike",
            "A powerful blow.",
            isPassive: false,
            resourceCost: 20,
            resourceType: "RAGE");

        // Assert
        ability.ResourceType.Should().Be("rage");
    }

    [Test]
    public void Create_TrimsDisplayName()
    {
        // Arrange & Act
        var ability = SpecializationAbility.Create(
            "rage-strike",
            "  Rage Strike  ",
            "A powerful blow.",
            isPassive: false);

        // Assert
        ability.DisplayName.Should().Be("Rage Strike");
    }

    [Test]
    public void Create_TrimsDescription()
    {
        // Arrange & Act
        var ability = SpecializationAbility.Create(
            "rage-strike",
            "Rage Strike",
            "  A powerful blow.  ",
            isPassive: false);

        // Assert
        ability.Description.Should().Be("A powerful blow.");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // COMPUTED PROPERTIES
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void HasResourceCost_WithCostAndType_ReturnsTrue()
    {
        // Arrange
        var ability = SpecializationAbility.CreateActive(
            "rage-strike", "Rage Strike", "A blow.", 20, "rage");

        // Assert
        ability.HasResourceCost.Should().BeTrue();
    }

    [Test]
    public void HasResourceCost_WithZeroCost_ReturnsFalse()
    {
        // Arrange
        var ability = SpecializationAbility.CreatePassive(
            "blood-frenzy", "Blood Frenzy", "Bonus damage.");

        // Assert
        ability.HasResourceCost.Should().BeFalse();
    }

    [Test]
    public void HasResourceCost_WithCostButNoType_ReturnsFalse()
    {
        // Arrange
        var ability = SpecializationAbility.Create(
            "test-ability", "Test", "Test description.",
            isPassive: false, resourceCost: 10, resourceType: "");

        // Assert
        ability.HasResourceCost.Should().BeFalse();
    }

    [Test]
    public void HasCooldown_WithPositiveCooldown_ReturnsTrue()
    {
        // Arrange
        var ability = SpecializationAbility.CreateActive(
            "berserker-charge", "Berserker Charge", "Rush forward.",
            30, "rage", cooldown: 4);

        // Assert
        ability.HasCooldown.Should().BeTrue();
    }

    [Test]
    public void HasCooldown_WithZeroCooldown_ReturnsFalse()
    {
        // Arrange
        var ability = SpecializationAbility.CreateActive(
            "rage-strike", "Rage Strike", "A blow.", 20, "rage");

        // Assert
        ability.HasCooldown.Should().BeFalse();
    }

    [Test]
    public void RisksCorruption_WithPositiveRisk_ReturnsTrue()
    {
        // Arrange
        var ability = SpecializationAbility.CreateActive(
            "rage-strike", "Rage Strike", "A blow.", 20, "rage",
            corruptionRisk: 5);

        // Assert
        ability.RisksCorruption.Should().BeTrue();
    }

    [Test]
    public void RisksCorruption_WithZeroRisk_ReturnsFalse()
    {
        // Arrange
        var ability = SpecializationAbility.CreateActive(
            "rage-strike", "Rage Strike", "A blow.", 20, "rage");

        // Assert
        ability.RisksCorruption.Should().BeFalse();
    }

    [Test]
    public void IsActive_ForActiveAbility_ReturnsTrue()
    {
        // Arrange
        var ability = SpecializationAbility.CreateActive(
            "rage-strike", "Rage Strike", "A blow.", 20, "rage");

        // Assert
        ability.IsActive.Should().BeTrue();
        ability.IsPassive.Should().BeFalse();
    }

    [Test]
    public void IsActive_ForPassiveAbility_ReturnsFalse()
    {
        // Arrange
        var ability = SpecializationAbility.CreatePassive(
            "blood-frenzy", "Blood Frenzy", "Bonus damage.");

        // Assert
        ability.IsActive.Should().BeFalse();
        ability.IsPassive.Should().BeTrue();
    }

    [Test]
    public void TypeDisplay_ForActive_ReturnsActiveTag()
    {
        // Arrange
        var ability = SpecializationAbility.CreateActive(
            "rage-strike", "Rage Strike", "A blow.", 20, "rage");

        // Assert
        ability.TypeDisplay.Should().Be("[ACTIVE]");
    }

    [Test]
    public void TypeDisplay_ForPassive_ReturnsPassiveTag()
    {
        // Arrange
        var ability = SpecializationAbility.CreatePassive(
            "blood-frenzy", "Blood Frenzy", "Bonus damage.");

        // Assert
        ability.TypeDisplay.Should().Be("[PASSIVE]");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // DISPLAY METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void GetShortDisplay_Active_ReturnsFormattedString()
    {
        // Arrange
        var ability = SpecializationAbility.CreateActive(
            "rage-strike", "Rage Strike", "A blow.", 20, "rage");

        // Assert
        ability.GetShortDisplay().Should().Be("Rage Strike [ACTIVE]");
    }

    [Test]
    public void GetShortDisplay_Passive_ReturnsFormattedString()
    {
        // Arrange
        var ability = SpecializationAbility.CreatePassive(
            "blood-frenzy", "Blood Frenzy", "Bonus damage.");

        // Assert
        ability.GetShortDisplay().Should().Be("Blood Frenzy [PASSIVE]");
    }

    [Test]
    public void ToString_ActiveWithCostAndCooldown_ReturnsFormattedString()
    {
        // Arrange
        var ability = SpecializationAbility.CreateActive(
            "berserker-charge", "Berserker Charge", "Rush forward.",
            30, "rage", cooldown: 4);

        // Assert
        ability.ToString().Should().Be("Berserker Charge [Active, 30 rage, CD: 4]");
    }

    [Test]
    public void ToString_ActiveWithCostOnly_ReturnsFormattedString()
    {
        // Arrange
        var ability = SpecializationAbility.CreateActive(
            "rage-strike", "Rage Strike", "A blow.", 20, "rage");

        // Assert
        ability.ToString().Should().Be("Rage Strike [Active, 20 rage]");
    }

    [Test]
    public void ToString_Passive_ReturnsFormattedString()
    {
        // Arrange
        var ability = SpecializationAbility.CreatePassive(
            "blood-frenzy", "Blood Frenzy", "Bonus damage.");

        // Assert
        ability.ToString().Should().Be("Blood Frenzy [Passive]");
    }

    [Test]
    public void GetDetailDisplay_ActiveWithAllProperties_ReturnsMultiLineString()
    {
        // Arrange
        var ability = SpecializationAbility.CreateActive(
            "berserker-charge", "Berserker Charge", "Rush forward.",
            30, "rage", cooldown: 4, corruptionRisk: 10);

        // Act
        var display = ability.GetDetailDisplay();

        // Assert
        display.Should().Contain("Berserker Charge [ACTIVE]");
        display.Should().Contain("Cost: 30 rage");
        display.Should().Contain("Cooldown: 4 turns");
        display.Should().Contain("Corruption Risk: 10");
        display.Should().Contain("Rush forward.");
    }

    [Test]
    public void GetDetailDisplay_Passive_OmitsCostAndCooldown()
    {
        // Arrange
        var ability = SpecializationAbility.CreatePassive(
            "blood-frenzy", "Blood Frenzy", "Bonus damage.");

        // Act
        var display = ability.GetDetailDisplay();

        // Assert
        display.Should().Contain("Blood Frenzy [PASSIVE]");
        display.Should().Contain("Bonus damage.");
        display.Should().NotContain("Cost:");
        display.Should().NotContain("Cooldown:");
        display.Should().NotContain("Corruption Risk:");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // EDGE CASES
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void Create_WithZeroResourceCostAndType_HasResourceCostReturnsFalse()
    {
        // Arrange
        var ability = SpecializationAbility.Create(
            "test", "Test", "Test.",
            isPassive: false,
            resourceCost: 0,
            resourceType: "rage");

        // Assert
        ability.HasResourceCost.Should().BeFalse();
    }

    [Test]
    public void Create_ActiveWithDefaultOptionalParams_CreatesWithZeroes()
    {
        // Arrange & Act
        var ability = SpecializationAbility.Create(
            "test-ability",
            "Test Ability",
            "Test description.",
            isPassive: false);

        // Assert
        ability.ResourceCost.Should().Be(0);
        ability.ResourceType.Should().BeEmpty();
        ability.Cooldown.Should().Be(0);
        ability.CorruptionRisk.Should().Be(0);
    }
}
