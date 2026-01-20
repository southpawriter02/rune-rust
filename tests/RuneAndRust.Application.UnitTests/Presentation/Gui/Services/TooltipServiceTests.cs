namespace RuneAndRust.Application.UnitTests.Presentation.Gui.Services;

using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Presentation.Gui.Models;
using RuneAndRust.Presentation.Gui.Services;

[TestFixture]
public class TooltipServiceTests
{
    // ============================================================================
    // TooltipContent Tests
    // ============================================================================

    [Test]
    public void TooltipContent_Simple_CreatesBasicTooltip()
    {
        // Arrange & Act
        var content = TooltipContent.Simple("Hello World");

        // Assert
        content.Title.Should().Be("Hello World");
        content.Subtitle.Should().BeNull();
        content.Sections.Should().BeEmpty();
    }

    [Test]
    public void TooltipContent_ForItem_CreatesItemTooltip()
    {
        // Arrange & Act
        var content = TooltipContent.ForItem("Iron Sword", "Weapon", "A sturdy blade", 100,
            [("Damage", "1d6+2"), ("Weight", "3 lbs")]);

        // Assert
        content.Title.Should().Contain("IRON SWORD");
        content.Subtitle.Should().Be("Weapon");
        content.Footer.Should().Contain("100 gold");
        content.Sections.Should().HaveCount(1);
        content.Sections[0].Lines.Should().HaveCount(2);
    }

    [Test]
    public void TooltipContent_ForAbility_CreatesAbilityTooltip()
    {
        // Arrange & Act
        var content = TooltipContent.ForAbility("Fireball", "Fire", 3, "Launches a ball of fire",
            cost: "10 Mana", cooldown: 2, hotkey: "F");

        // Assert
        content.Title.Should().Contain("FIREBALL");
        content.Subtitle.Should().Be("Fire • Level 3");
        content.Footer.Should().Be("[Press F to use]");
        content.Sections.Should().HaveCount(1);
    }

    // ============================================================================
    // TooltipService Tests
    // ============================================================================

    [Test]
    public void TooltipService_ShowTextTooltip_FiresEvent()
    {
        // Arrange
        var service = new TooltipService { ShowDelayMs = 0 };
        TooltipContent? receivedContent = null;
        service.OnTooltipChanged += c => receivedContent = c;

        // Act
        service.ShowTextTooltip("Test Tooltip");
        Thread.Sleep(50); // Allow async to complete

        // Assert
        receivedContent.Should().NotBeNull();
        receivedContent!.Title.Should().Be("Test Tooltip");
    }

    [Test]
    public void TooltipService_HideTooltip_ClearsContent()
    {
        // Arrange
        var service = new TooltipService { ShowDelayMs = 0 };
        TooltipContent? receivedContent = null;
        service.OnTooltipChanged += c => receivedContent = c;

        // Act
        service.ShowTextTooltip("Test");
        Thread.Sleep(50);
        service.HideTooltip();

        // Assert
        receivedContent.Should().BeNull();
        service.CurrentContent.Should().BeNull();
    }

    [Test]
    public void TooltipService_WhenDisabled_DoesNotShow()
    {
        // Arrange
        var service = new TooltipService { ShowDelayMs = 0, IsEnabled = false };
        TooltipContent? receivedContent = null;
        service.OnTooltipChanged += c => receivedContent = c;

        // Act
        service.ShowTextTooltip("Test");
        Thread.Sleep(50);

        // Assert
        receivedContent.Should().BeNull();
    }

    [Test]
    public void TooltipService_ShowTooltip_SetsCurrentContent()
    {
        // Arrange
        var service = new TooltipService { ShowDelayMs = 0 };
        var content = TooltipContent.Simple("Test");

        // Act
        service.ShowTooltip(content);
        Thread.Sleep(50);

        // Assert
        service.CurrentContent.Should().Be(content);
    }

    [Test]
    public void TooltipService_DefaultDelay_Is500ms()
    {
        // Arrange & Act
        var service = new TooltipService();

        // Assert
        service.ShowDelayMs.Should().Be(500);
    }

    [Test]
    public void TooltipService_DefaultEnabled_IsTrue()
    {
        // Arrange & Act
        var service = new TooltipService();

        // Assert
        service.IsEnabled.Should().BeTrue();
    }
}
