using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

/// <summary>
/// Unit tests for <see cref="ExaminationPuzzleHint"/> value object.
/// </summary>
[TestFixture]
public class ExaminationPuzzleHintTests
{
    [Test]
    public void Constructor_WithAllParameters_SetsProperties()
    {
        // Arrange & Act
        var hint = new ExaminationPuzzleHint(
            HintId: "hint-console-override",
            PuzzleId: "puzzle-level7-door",
            HintLevel: 2,
            RevealedBy: "examination",
            HintText: "The console appears to have override capabilities.",
            UnlocksInteraction: "override console");

        // Assert
        hint.HintId.Should().Be("hint-console-override");
        hint.PuzzleId.Should().Be("puzzle-level7-door");
        hint.HintLevel.Should().Be(2);
        hint.HintText.Should().Contain("override");
        hint.UnlocksInteraction.Should().Be("override console");
    }

    [Test]
    public void HasInteractionUnlock_WithUnlock_ReturnsTrue()
    {
        // Arrange
        var hint = new ExaminationPuzzleHint("id", "puzzle", 2, "examination", "text", "use lever");

        // Assert
        hint.HasInteractionUnlock.Should().BeTrue();
    }

    [Test]
    public void HasInteractionUnlock_WithoutUnlock_ReturnsFalse()
    {
        // Arrange
        var hint = new ExaminationPuzzleHint("id", "puzzle", 2, "examination", "text", null);

        // Assert
        hint.HasInteractionUnlock.Should().BeFalse();
    }

    [Test]
    public void IsSubtle_Level1_ReturnsTrue()
    {
        // Arrange
        var hint = new ExaminationPuzzleHint("id", "puzzle", 1, "examination", "text", null);

        // Assert
        hint.IsSubtle.Should().BeTrue();
        hint.IsObvious.Should().BeFalse();
    }

    [Test]
    public void IsObvious_Level3_ReturnsTrue()
    {
        // Arrange
        var hint = new ExaminationPuzzleHint("id", "puzzle", 3, "examination", "text", null);

        // Assert
        hint.IsObvious.Should().BeTrue();
        hint.IsSubtle.Should().BeFalse();
    }

    [Test]
    public void GetLevelDescription_ReturnsCorrectDescription()
    {
        // Arrange & Act & Assert
        new ExaminationPuzzleHint("", "", 1, "", "", null).GetLevelDescription().Should().Be("Cryptic");
        new ExaminationPuzzleHint("", "", 2, "", "", null).GetLevelDescription().Should().Be("Moderate");
        new ExaminationPuzzleHint("", "", 3, "", "", null).GetLevelDescription().Should().Be("Direct");
    }
}

/// <summary>
/// Unit tests for <see cref="ExaminationHintLink"/> value object.
/// </summary>
[TestFixture]
public class ExaminationHintLinkTests
{
    [Test]
    public void IsLayerSatisfied_WhenAchievedMeetsRequirement_ReturnsTrue()
    {
        // Arrange
        var link = new ExaminationHintLink("obj-1", 3, "puzzle-1", "hint-1", null);

        // Assert
        link.IsLayerSatisfied(3).Should().BeTrue();
        link.IsLayerSatisfied(4).Should().BeTrue();
    }

    [Test]
    public void IsLayerSatisfied_WhenAchievedBelowRequirement_ReturnsFalse()
    {
        // Arrange
        var link = new ExaminationHintLink("obj-1", 3, "puzzle-1", "hint-1", null);

        // Assert
        link.IsLayerSatisfied(2).Should().BeFalse();
    }

    [Test]
    public void HasCondition_WithCondition_ReturnsTrue()
    {
        // Arrange
        var link = new ExaminationHintLink("obj-1", 3, "puzzle-1", "hint-1", "item-key");

        // Assert
        link.HasCondition.Should().BeTrue();
    }

    [Test]
    public void HasCondition_WithoutCondition_ReturnsFalse()
    {
        // Arrange
        var link = new ExaminationHintLink("obj-1", 3, "puzzle-1", "hint-1", null);

        // Assert
        link.HasCondition.Should().BeFalse();
    }

    [Test]
    public void RequiresExpertExamination_Layer3_ReturnsTrue()
    {
        // Arrange
        var link = new ExaminationHintLink("obj-1", 3, "puzzle-1", "hint-1", null);

        // Assert
        link.RequiresExpertExamination.Should().BeTrue();
    }
}

/// <summary>
/// Unit tests for <see cref="HintRevealResult"/> value object.
/// </summary>
[TestFixture]
public class HintRevealResultTests
{
    [Test]
    public void Empty_CreatesEmptyResult()
    {
        // Arrange & Act
        var result = HintRevealResult.Empty("obj-1");

        // Assert
        result.ObjectId.Should().Be("obj-1");
        result.HasRevealedHints.Should().BeFalse();
        result.HasUnlockedInteractions.Should().BeFalse();
        result.TotalHintsRevealed.Should().Be(0);
    }

    [Test]
    public void HasRevealedHints_WithHints_ReturnsTrue()
    {
        // Arrange
        var hints = new List<ExaminationPuzzleHint>
        {
            new("hint-1", "puzzle-1", 2, "examination", "A hint.", null)
        };
        var result = new HintRevealResult("obj-1", hints, Array.Empty<string>(), 0);

        // Assert
        result.HasRevealedHints.Should().BeTrue();
        result.TotalHintsRevealed.Should().Be(1);
    }
}

/// <summary>
/// Unit tests for <see cref="UnlockedInteraction"/> value object.
/// </summary>
[TestFixture]
public class UnlockedInteractionTests
{
    [Test]
    public void MatchesCommand_ExactMatch_ReturnsTrue()
    {
        // Arrange
        var interaction = new UnlockedInteraction(
            "int-1", "override console", "console-1", "hint-1", "Override the console");

        // Assert
        interaction.MatchesCommand("override console").Should().BeTrue();
        interaction.MatchesCommand("OVERRIDE CONSOLE").Should().BeTrue();
        interaction.MatchesCommand("  override console  ").Should().BeTrue();
    }

    [Test]
    public void MatchesCommand_NoMatch_ReturnsFalse()
    {
        // Arrange
        var interaction = new UnlockedInteraction(
            "int-1", "override console", "console-1", "hint-1", "Override the console");

        // Assert
        interaction.MatchesCommand("use lever").Should().BeFalse();
    }

    [Test]
    public void ToHelpString_FormatsCorrectly()
    {
        // Arrange
        var interaction = new UnlockedInteraction(
            "int-1", "override console", "console-1", "hint-1", "Override the console");

        // Act
        var help = interaction.ToHelpString();

        // Assert
        help.Should().Be("override console - Override the console");
    }
}
