using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Presentation.Gui.Services;

namespace RuneAndRust.Application.UnitTests.Presentation.Gui.Services;

/// <summary>
/// Unit tests for <see cref="MenuActions"/>.
/// </summary>
[TestFixture]
public class MenuActionsTests
{
    /// <summary>
    /// Verifies that Examine formats correctly.
    /// </summary>
    [Test]
    public void Examine_ReturnsFormattedString()
    {
        // Act
        var result = MenuActions.Examine();

        // Assert
        result.Should().Be("🔍 Examine");
    }

    /// <summary>
    /// Verifies that Examine with custom label formats correctly.
    /// </summary>
    [Test]
    public void Examine_WithCustomLabel_ReturnsFormattedString()
    {
        // Act
        var result = MenuActions.Examine("Inspect");

        // Assert
        result.Should().Be("🔍 Inspect");
    }

    /// <summary>
    /// Verifies that all action methods return non-empty strings.
    /// </summary>
    [Test]
    [TestCase("Examine")]
    [TestCase("Use")]
    [TestCase("Equip")]
    [TestCase("Unequip")]
    [TestCase("Drop")]
    [TestCase("Take")]
    [TestCase("Attack")]
    [TestCase("Talk")]
    [TestCase("Cancel")]
    public void AllActions_ReturnNonEmptyStrings(string actionName)
    {
        // Arrange & Act
        var result = actionName switch
        {
            "Examine" => MenuActions.Examine(),
            "Use" => MenuActions.Use(),
            "Equip" => MenuActions.Equip(),
            "Unequip" => MenuActions.Unequip(),
            "Drop" => MenuActions.Drop(),
            "Take" => MenuActions.Take(),
            "Attack" => MenuActions.Attack(),
            "Talk" => MenuActions.Talk(),
            "Cancel" => MenuActions.Cancel(),
            _ => ""
        };

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().Contain(actionName);
    }

    /// <summary>
    /// Verifies all icon constants are set.
    /// </summary>
    [Test]
    public void AllIconConstants_AreNotEmpty()
    {
        // Assert
        MenuActions.ExamineIcon.Should().NotBeNullOrEmpty();
        MenuActions.UseIcon.Should().NotBeNullOrEmpty();
        MenuActions.EquipIcon.Should().NotBeNullOrEmpty();
        MenuActions.UnequipIcon.Should().NotBeNullOrEmpty();
        MenuActions.DropIcon.Should().NotBeNullOrEmpty();
        MenuActions.TakeIcon.Should().NotBeNullOrEmpty();
        MenuActions.AttackIcon.Should().NotBeNullOrEmpty();
        MenuActions.TalkIcon.Should().NotBeNullOrEmpty();
        MenuActions.CancelIcon.Should().NotBeNullOrEmpty();
    }
}
