using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Presentation.UI;

namespace RuneAndRust.Application.UnitTests.Presentation.UI;

/// <summary>
/// Unit tests for <see cref="ColoredText"/>.
/// </summary>
[TestFixture]
public class ColoredTextTests
{
    #region Factory Method Tests

    [Test]
    public void Default_CreatesWithDefaultType()
    {
        // Act
        var text = ColoredText.Default("Test");

        // Assert
        text.Text.Should().Be("Test");
        text.Type.Should().Be(MessageType.Default);
        text.ExplicitColor.Should().BeNull();
    }

    [Test]
    public void Info_CreatesWithInfoType()
    {
        // Act
        var text = ColoredText.Info("Information");

        // Assert
        text.Text.Should().Be("Information");
        text.Type.Should().Be(MessageType.Info);
    }

    [Test]
    public void Warning_CreatesWithWarningType()
    {
        // Act
        var text = ColoredText.Warning("Be careful!");

        // Assert
        text.Text.Should().Be("Be careful!");
        text.Type.Should().Be(MessageType.Warning);
    }

    [Test]
    public void Error_CreatesWithErrorType()
    {
        // Act
        var text = ColoredText.Error("Something went wrong");

        // Assert
        text.Text.Should().Be("Something went wrong");
        text.Type.Should().Be(MessageType.Error);
    }

    [Test]
    public void CombatHit_CreatesWithCombatHitType()
    {
        // Act
        var text = ColoredText.CombatHit("Critical hit!");

        // Assert
        text.Type.Should().Be(MessageType.CombatHit);
    }

    [Test]
    public void CombatDamage_CreatesWithCombatDamageType()
    {
        // Act
        var text = ColoredText.CombatDamage("25 damage dealt");

        // Assert
        text.Type.Should().Be(MessageType.CombatDamage);
    }

    [Test]
    public void CombatHeal_CreatesWithCombatHealType()
    {
        // Act
        var text = ColoredText.CombatHeal("Healed for 30 HP");

        // Assert
        text.Type.Should().Be(MessageType.CombatHeal);
    }

    [Test]
    [TestCase(nameof(ColoredText.LootCommon), MessageType.LootCommon)]
    [TestCase(nameof(ColoredText.LootUncommon), MessageType.LootUncommon)]
    [TestCase(nameof(ColoredText.LootRare), MessageType.LootRare)]
    [TestCase(nameof(ColoredText.LootEpic), MessageType.LootEpic)]
    [TestCase(nameof(ColoredText.LootLegendary), MessageType.LootLegendary)]
    public void LootMethods_CreateCorrectTypes(string methodName, MessageType expectedType)
    {
        // Act
        var text = methodName switch
        {
            nameof(ColoredText.LootCommon) => ColoredText.LootCommon("Item"),
            nameof(ColoredText.LootUncommon) => ColoredText.LootUncommon("Item"),
            nameof(ColoredText.LootRare) => ColoredText.LootRare("Item"),
            nameof(ColoredText.LootEpic) => ColoredText.LootEpic("Item"),
            nameof(ColoredText.LootLegendary) => ColoredText.LootLegendary("Item"),
            _ => throw new ArgumentException($"Unknown method: {methodName}")
        };

        // Assert
        text.Type.Should().Be(expectedType);
    }

    [Test]
    public void Dialogue_CreatesWithDialogueType()
    {
        // Act
        var text = ColoredText.Dialogue("Hello, traveler!");

        // Assert
        text.Type.Should().Be(MessageType.Dialogue);
    }

    [Test]
    public void Description_CreatesWithDescriptionType()
    {
        // Act
        var text = ColoredText.Description("A dark cave stretches before you.");

        // Assert
        text.Type.Should().Be(MessageType.Description);
    }

    [Test]
    public void Success_CreatesWithSuccessType()
    {
        // Act
        var text = ColoredText.Success("You succeeded!");

        // Assert
        text.Type.Should().Be(MessageType.Success);
    }

    [Test]
    public void Failure_CreatesWithFailureType()
    {
        // Act
        var text = ColoredText.Failure("You failed.");

        // Assert
        text.Type.Should().Be(MessageType.Failure);
    }

    #endregion

    #region Explicit Color Tests

    [Test]
    public void ExplicitColor_CanBeSet()
    {
        // Act
        var text = new ColoredText("Custom", MessageType.Default, ConsoleColor.Magenta);

        // Assert
        text.ExplicitColor.Should().Be(ConsoleColor.Magenta);
    }

    [Test]
    public void ExplicitColor_DefaultsToNull()
    {
        // Act
        var text = new ColoredText("Text", MessageType.Info);

        // Assert
        text.ExplicitColor.Should().BeNull();
    }

    #endregion
}
