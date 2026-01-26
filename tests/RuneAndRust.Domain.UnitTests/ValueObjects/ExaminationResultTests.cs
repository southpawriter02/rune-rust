using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

/// <summary>
/// Unit tests for <see cref="ExaminationResult"/> value object.
/// </summary>
[TestFixture]
public class ExaminationResultTests
{
    #region CursoryOnly Tests

    [Test]
    public void CursoryOnly_WithValidDescription_CreatesLayer1Result()
    {
        // Arrange & Act
        var result = ExaminationResult.CursoryOnly(
            objectId: "door-01",
            objectName: "Iron Door",
            cursoryDescription: "A heavy iron door.");

        // Assert
        result.ObjectId.Should().Be("door-01");
        result.ObjectName.Should().Be("Iron Door");
        result.HighestLayerUnlocked.Should().Be(1);
        result.UnlockedLayer.Should().Be(ExaminationLayer.Cursory);
        result.WitsCheckResult.Should().Be(0);
        result.CompositeDescription.Should().Be("A heavy iron door.");
        result.LayerCount.Should().Be(1);
        result.HasDetailedKnowledge.Should().BeFalse();
        result.HasExpertKnowledge.Should().BeFalse();
        result.RevealedHint.Should().BeFalse();
        result.HasNewInteractions.Should().BeFalse();
    }

    #endregion

    #region Create Tests

    [Test]
    public void Create_Layer2Unlocked_IncludesDetailedKnowledge()
    {
        // Arrange
        var layerTexts = new[]
        {
            "Layer 1 text.",
            "Layer 2 text.",
            "Layer 3 text."
        };

        // Act
        var result = ExaminationResult.Create(
            "obj-01", "Object", 14, 2, layerTexts);

        // Assert
        result.HighestLayerUnlocked.Should().Be(2);
        result.UnlockedLayer.Should().Be(ExaminationLayer.Detailed);
        result.HasDetailedKnowledge.Should().BeTrue();
        result.HasExpertKnowledge.Should().BeFalse();
        result.LayerCount.Should().Be(2);
        result.CompositeDescription.Should().Contain("Layer 1");
        result.CompositeDescription.Should().Contain("Layer 2");
        result.CompositeDescription.Should().NotContain("Layer 3");
    }

    [Test]
    public void Create_Layer3Unlocked_IncludesExpertKnowledge()
    {
        // Arrange
        var layerTexts = new[]
        {
            "Layer 1 text.",
            "Layer 2 text.",
            "Layer 3 text."
        };

        // Act
        var result = ExaminationResult.Create(
            "obj-01", "Object", 20, 3, layerTexts);

        // Assert
        result.HighestLayerUnlocked.Should().Be(3);
        result.UnlockedLayer.Should().Be(ExaminationLayer.Expert);
        result.HasDetailedKnowledge.Should().BeTrue();
        result.HasExpertKnowledge.Should().BeTrue();
        result.LayerCount.Should().Be(3);
        result.CompositeDescription.Should().Contain("Layer 3");
    }

    [Test]
    public void Create_WithHintAndInteractions_TracksRevealedContent()
    {
        // Arrange
        var layerTexts = new[] { "Layer 1.", "Layer 2.", "Layer 3." };
        var interactions = new[] { "bypass", "disarm" };

        // Act
        var result = ExaminationResult.Create(
            "obj-01", "Object", 20, 3, layerTexts,
            revealedHint: true,
            solutionId: "puzzle-solution-01",
            interactions: interactions);

        // Assert
        result.RevealedHint.Should().BeTrue();
        result.RevealedSolutionId.Should().Be("puzzle-solution-01");
        result.HasNewInteractions.Should().BeTrue();
        result.RevealedInteractions.Should().Contain("bypass");
        result.RevealedInteractions.Should().Contain("disarm");
    }

    #endregion

    #region CompositeDescription Tests

    [Test]
    public void CompositeDescription_MultipleUnlockedLayers_CombinesCorrectly()
    {
        // Arrange
        var layerTexts = new[]
        {
            "A heavy iron door.",
            "Reinforced with Jötun metalwork.",
            "Bears the seal of Level 7 Security."
        };

        // Act
        var result = ExaminationResult.Create(
            "door-01", "Iron Door", 18, 3, layerTexts);

        // Assert
        result.CompositeDescription.Should().Contain("heavy iron door");
        result.CompositeDescription.Should().Contain("Jötun metalwork");
        result.CompositeDescription.Should().Contain("Level 7 Security");
    }

    [Test]
    public void CompositeDescription_LayersSeparatedByDoubleNewlines()
    {
        // Arrange
        var layerTexts = new[] { "First layer.", "Second layer." };

        // Act
        var result = ExaminationResult.Create(
            "obj", "Object", 14, 2, layerTexts);

        // Assert
        result.CompositeDescription.Should().Be("First layer.\n\nSecond layer.");
    }

    #endregion

    #region ToSummaryString Tests

    [Test]
    public void ToSummaryString_CursoryOnly_FormatsCorrectly()
    {
        // Arrange
        var result = ExaminationResult.CursoryOnly(
            "door-01", "Iron Door", "A door.");

        // Act
        var summary = result.ToSummaryString();

        // Assert
        summary.Should().Contain("[Iron Door]");
        summary.Should().Contain("Cursory Knowledge");
        summary.Should().NotContain("WITS check");
    }

    [Test]
    public void ToSummaryString_WithWitsCheck_IncludesSuccesses()
    {
        // Arrange
        var layerTexts = new[] { "Layer 1.", "Layer 2." };
        var result = ExaminationResult.Create(
            "obj", "Object", 14, 2, layerTexts);

        // Act
        var summary = result.ToSummaryString();

        // Assert
        summary.Should().Contain("Detailed Knowledge");
        summary.Should().Contain("WITS check: 14 successes");
    }

    [Test]
    public void ToSummaryString_WithHint_IncludesHintIndicator()
    {
        // Arrange
        var layerTexts = new[] { "L1", "L2", "L3" };
        var result = ExaminationResult.Create(
            "obj", "Object", 20, 3, layerTexts, revealedHint: true);

        // Act
        var summary = result.ToSummaryString();

        // Assert
        summary.Should().Contain("[Hint revealed!]");
    }

    [Test]
    public void ToSummaryString_WithInteractions_ListsCommands()
    {
        // Arrange
        var layerTexts = new[] { "L1", "L2", "L3" };
        var interactions = new[] { "bypass", "unlock" };
        var result = ExaminationResult.Create(
            "obj", "Object", 20, 3, layerTexts, interactions: interactions);

        // Act
        var summary = result.ToSummaryString();

        // Assert
        summary.Should().Contain("[New commands: bypass, unlock]");
    }

    #endregion

    #region Validation Tests

    [Test]
    public void CursoryOnly_WithNullObjectId_ThrowsArgumentException()
    {
        // Act
        var act = () => ExaminationResult.CursoryOnly(null!, "Name", "Desc");

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Test]
    public void Create_WithInvalidLayer_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var layerTexts = new[] { "L1", "L2", "L3" };

        // Act
        var actLow = () => ExaminationResult.Create("obj", "Name", 0, 0, layerTexts);
        var actHigh = () => ExaminationResult.Create("obj", "Name", 0, 4, layerTexts);

        // Assert
        actLow.Should().Throw<ArgumentOutOfRangeException>();
        actHigh.Should().Throw<ArgumentOutOfRangeException>();
    }

    #endregion
}
