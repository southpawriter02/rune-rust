using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

/// <summary>
/// Unit tests for <see cref="ExaminationContext"/> value object.
/// </summary>
[TestFixture]
public class ExaminationContextTests
{
    #region Factory Method Tests

    [Test]
    public void FirstExamination_WithValidParameters_CreatesContextWithNoHistory()
    {
        // Arrange & Act
        var context = ExaminationContext.FirstExamination(
            objectId: "door-01",
            objectType: "Door",
            characterId: "player-1",
            witsPool: 8,
            biomeId: "citadel");

        // Assert
        context.ObjectId.Should().Be("door-01");
        context.ObjectType.Should().Be("Door");
        context.CharacterId.Should().Be("player-1");
        context.WitsPool.Should().Be(8);
        context.BiomeId.Should().Be("citadel");
        context.PreviousHighestLayer.Should().Be(0);
        context.HasPreviousExamination.Should().BeFalse();
    }

    [Test]
    public void WithHistory_WithLayer2_CreatesContextWithDetailedKnowledge()
    {
        // Arrange & Act
        var context = ExaminationContext.WithHistory(
            objectId: "door-01",
            objectType: "Door",
            characterId: "player-1",
            witsPool: 8,
            biomeId: "citadel",
            previousLayer: 2);

        // Assert
        context.PreviousHighestLayer.Should().Be(2);
        context.HasPreviousExamination.Should().BeTrue();
        context.HasDetailedKnowledge.Should().BeTrue();
        context.HasExpertKnowledge.Should().BeFalse();
    }

    [Test]
    public void WithHistory_WithLayer3_CreatesContextWithExpertKnowledge()
    {
        // Arrange & Act
        var context = ExaminationContext.WithHistory(
            objectId: "door-01",
            objectType: "Door",
            characterId: "player-1",
            witsPool: 8,
            biomeId: "citadel",
            previousLayer: 3);

        // Assert
        context.PreviousHighestLayer.Should().Be(3);
        context.HasExpertKnowledge.Should().BeTrue();
        context.NextLayerDc.Should().Be(-1, "all layers already unlocked");
    }

    [Test]
    public void WithHistory_ClampsLayerToValidRange()
    {
        // Arrange & Act
        var contextHigh = ExaminationContext.WithHistory(
            "obj", "Type", "char", 8, "biome", previousLayer: 10);
        var contextLow = ExaminationContext.WithHistory(
            "obj", "Type", "char", 8, "biome", previousLayer: -5);

        // Assert
        contextHigh.PreviousHighestLayer.Should().Be(3);
        contextLow.PreviousHighestLayer.Should().Be(0);
    }

    #endregion

    #region NextLayerDc Tests

    [Test]
    public void NextLayerDc_FirstExamination_Returns0ForAutoLayer1()
    {
        // Arrange
        var context = ExaminationContext.FirstExamination(
            "obj", "Type", "char", 8, "biome");

        // Act & Assert
        context.NextLayerDc.Should().Be(0);
    }

    [Test]
    public void NextLayerDc_Layer1Unlocked_Returns12ForLayer2()
    {
        // Arrange
        var context = ExaminationContext.WithHistory(
            "obj", "Type", "char", 8, "biome", previousLayer: 1);

        // Act & Assert
        context.NextLayerDc.Should().Be(12);
    }

    [Test]
    public void NextLayerDc_Layer2Unlocked_Returns18ForLayer3()
    {
        // Arrange
        var context = ExaminationContext.WithHistory(
            "obj", "Type", "char", 8, "biome", previousLayer: 2);

        // Act & Assert
        context.NextLayerDc.Should().Be(18);
    }

    [Test]
    public void NextLayerDc_Layer3Unlocked_ReturnsNegative1()
    {
        // Arrange
        var context = ExaminationContext.WithHistory(
            "obj", "Type", "char", 8, "biome", previousLayer: 3);

        // Act & Assert
        context.NextLayerDc.Should().Be(-1);
    }

    #endregion

    #region Validation Tests

    [Test]
    public void FirstExamination_WithNullObjectId_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => ExaminationContext.FirstExamination(
            null!, "Type", "char", 8, "biome");

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Test]
    public void FirstExamination_WithNegativeWitsPool_ThrowsArgumentOutOfRangeException()
    {
        // Arrange & Act
        var act = () => ExaminationContext.FirstExamination(
            "obj", "Type", "char", -1, "biome");

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    #endregion
}
