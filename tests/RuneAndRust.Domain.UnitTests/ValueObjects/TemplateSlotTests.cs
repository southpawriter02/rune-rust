using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

/// <summary>
/// Tests for TemplateSlot value object.
/// </summary>
[TestFixture]
public class TemplateSlotTests
{
    [Test]
    public void Monster_CreatesCorrectSlotType()
    {
        // Arrange & Act
        var slot = TemplateSlot.Monster("monster_01", required: false, probability: 0.5f);

        // Assert
        slot.Type.Should().Be(SlotType.Monster);
        slot.SlotId.Should().Be("monster_01");
        slot.IsRequired.Should().BeFalse();
        slot.FillProbability.Should().Be(0.5f);
    }

    [Test]
    public void Description_WithPool_SetsDescriptorPool()
    {
        // Arrange & Act
        var slot = TemplateSlot.Description(
            "adjective",
            "room.adjectives.corridor",
            required: true);

        // Assert
        slot.Type.Should().Be(SlotType.Description);
        slot.SlotId.Should().Be("adjective");
        slot.DescriptorPool.Should().Be("room.adjectives.corridor");
        slot.IsRequired.Should().BeTrue();
    }

    [Test]
    public void GetConstraint_WithExistingKey_ReturnsValue()
    {
        // Arrange
        var constraints = new Dictionary<string, string> { ["maxTier"] = "elite" };
        var slot = TemplateSlot.Monster("monster_01", constraints: constraints);

        // Act & Assert
        slot.GetConstraint("maxTier").Should().Be("elite");
    }

    [Test]
    public void GetConstraint_WithMissingKey_ReturnsNull()
    {
        // Arrange
        var slot = TemplateSlot.Monster("monster_01");

        // Act & Assert
        slot.GetConstraint("missing").Should().BeNull();
    }

    [Test]
    public void EffectiveFillProbability_Required_ReturnsOne()
    {
        // Arrange
        var slot = TemplateSlot.Monster("monster_01", required: true, probability: 0.3f);

        // Act & Assert
        slot.EffectiveFillProbability.Should().Be(1.0f);
    }

    [Test]
    public void EffectiveFillProbability_Optional_ReturnsProbability()
    {
        // Arrange
        var slot = TemplateSlot.Monster("monster_01", required: false, probability: 0.5f);

        // Act & Assert
        slot.EffectiveFillProbability.Should().Be(0.5f);
    }

    [Test]
    public void Item_CreatesCorrectSlotType()
    {
        // Arrange & Act
        var slot = TemplateSlot.Item("item_01", probability: 0.2f);

        // Assert
        slot.Type.Should().Be(SlotType.Item);
        slot.FillProbability.Should().Be(0.2f);
    }

    [Test]
    public void Container_CreatesCorrectSlotType()
    {
        // Arrange & Act
        var constraints = new Dictionary<string, string>
        {
            ["containerType"] = "chest",
            ["lootQuality"] = "high"
        };
        var slot = TemplateSlot.Container("main_container", constraints: constraints);

        // Assert
        slot.Type.Should().Be(SlotType.Container);
        slot.GetConstraint("containerType").Should().Be("chest");
        slot.GetConstraint("lootQuality").Should().Be("high");
    }

    [Test]
    public void Feature_CreatesCorrectSlotType()
    {
        // Arrange & Act
        var slot = TemplateSlot.Feature("altar_01");

        // Assert
        slot.Type.Should().Be(SlotType.Feature);
    }

    [Test]
    public void Hazard_CreatesCorrectSlotType()
    {
        // Arrange & Act
        var slot = TemplateSlot.Hazard("hazard_01", probability: 0.4f);

        // Assert
        slot.Type.Should().Be(SlotType.Hazard);
        slot.FillProbability.Should().Be(0.4f);
    }

    [Test]
    public void Exit_CreatesCorrectSlotType()
    {
        // Arrange & Act
        var constraints = new Dictionary<string, string> { ["direction"] = "north" };
        var slot = TemplateSlot.Exit("exit_north", constraints: constraints);

        // Assert
        slot.Type.Should().Be(SlotType.Exit);
        slot.GetConstraint("direction").Should().Be("north");
    }

    [Test]
    public void HasConstraint_WithMatchingValue_ReturnsTrue()
    {
        // Arrange
        var constraints = new Dictionary<string, string> { ["maxTier"] = "elite" };
        var slot = TemplateSlot.Monster("monster_01", constraints: constraints);

        // Act & Assert
        slot.HasConstraint("maxTier", "elite").Should().BeTrue();
        slot.HasConstraint("maxTier", "standard").Should().BeFalse();
    }

    [Test]
    public void HasConstraint_CaseInsensitive_ReturnsTrue()
    {
        // Arrange
        var constraints = new Dictionary<string, string> { ["maxTier"] = "elite" };
        var slot = TemplateSlot.Monster("monster_01", constraints: constraints);

        // Act & Assert
        slot.HasConstraint("maxTier", "ELITE").Should().BeTrue();
    }
}
