using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

/// <summary>
/// Unit tests for <see cref="Clue"/> value object.
/// </summary>
[TestFixture]
public class ClueTests
{
    [Test]
    public void Create_SetsAllProperties()
    {
        // Arrange & Act
        var clue = Clue.Create(
            "clue-blood-pattern",
            InvestigationTarget.CrimeScene,
            "scene-001",
            ClueCategory.Method,
            12,
            "Blood Pattern",
            "The blood splatter indicates a single blow from above.",
            "You notice the blood pattern tells a story...");

        // Assert
        clue.ClueId.Should().Be("clue-blood-pattern");
        clue.SourceType.Should().Be(InvestigationTarget.CrimeScene);
        clue.Category.Should().Be(ClueCategory.Method);
        clue.DiscoveryDc.Should().Be(12);
        clue.ClueName.Should().Be("Blood Pattern");
    }

    [Test]
    public void HasTags_WithTags_ReturnsTrue()
    {
        // Arrange
        var clue = new Clue
        {
            ClueId = "clue-1",
            SourceType = InvestigationTarget.Remains,
            SourceTargetId = "corpse-1",
            Category = ClueCategory.Identity,
            DiscoveryDc = 8,
            ClueName = "Guild Mark",
            Description = "A tattoo of the Thieves Guild.",
            DiscoveryText = "You notice a distinctive tattoo...",
            Tags = new[] { "guild", "identity" }
        };

        // Assert
        clue.HasTags.Should().BeTrue();
        clue.Tags.Should().Contain("guild");
    }
}

/// <summary>
/// Unit tests for <see cref="Deduction"/> value object.
/// </summary>
[TestFixture]
public class DeductionTests
{
    [Test]
    public void CanDeduce_WithAllClues_ReturnsTrue()
    {
        // Arrange
        var deduction = Deduction.Create(
            "ded-killer-identity",
            "Killer Identity",
            new[] { "clue-weapon", "clue-motive", "clue-witness" },
            "The killer was the butler.",
            "Piecing together the evidence...");

        var gatheredClues = new[] { "clue-weapon", "clue-motive", "clue-witness", "clue-extra" };

        // Act & Assert
        deduction.CanDeduce(gatheredClues).Should().BeTrue();
    }

    [Test]
    public void CanDeduce_MissingClues_ReturnsFalse()
    {
        // Arrange
        var deduction = Deduction.Create(
            "ded-killer-identity",
            "Killer Identity",
            new[] { "clue-weapon", "clue-motive", "clue-witness" },
            "The killer was the butler.",
            "Piecing together the evidence...");

        var gatheredClues = new[] { "clue-weapon", "clue-motive" };

        // Act & Assert
        deduction.CanDeduce(gatheredClues).Should().BeFalse();
        deduction.GetMissingClueCount(gatheredClues).Should().Be(1);
    }

    [Test]
    public void MinimumCluesRequired_ReturnsCorrectCount()
    {
        // Arrange
        var deduction = Deduction.Create(
            "ded-1",
            "Test",
            new[] { "a", "b", "c" },
            "Conclusion",
            "Narrative");

        // Assert
        deduction.MinimumCluesRequired.Should().Be(3);
    }
}

/// <summary>
/// Unit tests for <see cref="InvestigationResult"/> value object.
/// </summary>
[TestFixture]
public class InvestigationResultTests
{
    [Test]
    public void Failed_CreatesFailedResult()
    {
        // Arrange & Act
        var result = InvestigationResult.Failed(
            "corpse-1",
            InvestigationTarget.Remains,
            checkResult: 6,
            dc: 12,
            timeSpent: 10,
            "You couldn't find any useful information.");

        // Assert
        result.Success.Should().BeFalse();
        result.HasDiscoveries.Should().BeFalse();
        result.TotalDiscoveries.Should().Be(0);
    }

    [Test]
    public void HasDiscoveries_WithClues_ReturnsTrue()
    {
        // Arrange
        var clue = Clue.Create("c-1", InvestigationTarget.CrimeScene, "s-1", ClueCategory.Cause, 8, "Test", "Desc", "Text");
        var result = new InvestigationResult
        {
            TargetId = "scene-1",
            TargetType = InvestigationTarget.CrimeScene,
            Success = true,
            CheckResult = 15,
            DifficultyClass = 12,
            CluesDiscovered = new[] { clue },
            DeductionsMade = Array.Empty<Deduction>(),
            NarrativeText = "Investigation narrative.",
            TimeSpent = 15
        };

        // Assert
        result.HasClues.Should().BeTrue();
        result.HasDeductions.Should().BeFalse();
        result.TotalDiscoveries.Should().Be(1);
    }
}

/// <summary>
/// Unit tests for <see cref="InvestigationDifficulty"/> static class.
/// </summary>
[TestFixture]
public class InvestigationDifficultyTests
{
    [Test]
    public void Constants_HaveCorrectValues()
    {
        // Assert
        InvestigationDifficulty.Obvious.Should().Be(8);
        InvestigationDifficulty.Standard.Should().Be(12);
        InvestigationDifficulty.Complex.Should().Be(16);
        InvestigationDifficulty.Obscured.Should().Be(20);
        InvestigationDifficulty.Ancient.Should().Be(24);
    }

    [Test]
    public void GetDescription_ReturnsCorrectDescription()
    {
        // Assert
        InvestigationDifficulty.GetDescription(8).Should().Be("Obvious");
        InvestigationDifficulty.GetDescription(12).Should().Be("Standard");
        InvestigationDifficulty.GetDescription(16).Should().Be("Complex");
        InvestigationDifficulty.GetDescription(20).Should().Be("Obscured");
        InvestigationDifficulty.GetDescription(25).Should().Be("Ancient");
    }
}
