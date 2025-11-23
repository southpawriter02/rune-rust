using Microsoft.VisualStudio.TestTools.UnitTesting;
using RuneAndRust.Core.Spatial;

namespace RuneAndRust.Tests.Spatial;

/// <summary>
/// Unit tests for VerticalLayer enum and extensions (v0.39.1)
/// </summary>
[TestClass]
public class VerticalLayerTests
{
    [TestMethod]
    public void GetApproximateDepth_DeepRoots_ReturnsNegative300()
    {
        // Arrange
        var layer = VerticalLayer.DeepRoots;

        // Act
        var depth = layer.GetApproximateDepth();

        // Assert
        Assert.AreEqual(-300, depth);
    }

    [TestMethod]
    public void GetApproximateDepth_GroundLevel_ReturnsZero()
    {
        // Arrange
        var layer = VerticalLayer.GroundLevel;

        // Act
        var depth = layer.GetApproximateDepth();

        // Assert
        Assert.AreEqual(0, depth);
    }

    [TestMethod]
    public void GetApproximateDepth_Canopy_ReturnsPositive300()
    {
        // Arrange
        var layer = VerticalLayer.Canopy;

        // Act
        var depth = layer.GetApproximateDepth();

        // Assert
        Assert.AreEqual(300, depth);
    }

    [TestMethod]
    public void GetLayerDescription_AllLayers_ReturnsNonEmptyDescriptions()
    {
        // Arrange
        var layers = new[]
        {
            VerticalLayer.DeepRoots,
            VerticalLayer.LowerRoots,
            VerticalLayer.UpperRoots,
            VerticalLayer.GroundLevel,
            VerticalLayer.LowerTrunk,
            VerticalLayer.UpperTrunk,
            VerticalLayer.Canopy
        };

        // Act & Assert
        foreach (var layer in layers)
        {
            var description = layer.GetLayerDescription();
            Assert.IsFalse(string.IsNullOrEmpty(description),
                $"Layer {layer} should have a description");
            Assert.IsTrue(description.Length > 20,
                $"Layer {layer} description should be descriptive");
        }
    }

    [TestMethod]
    public void GetTypicalBiomes_GroundLevel_IncludesMultipleBiomes()
    {
        // Arrange
        var layer = VerticalLayer.GroundLevel;

        // Act
        var biomes = layer.GetTypicalBiomes();

        // Assert
        Assert.IsTrue(biomes.Count >= 3,
            "Ground level should support multiple biomes");
        Assert.IsTrue(biomes.Contains("The_Roots"));
    }

    [TestMethod]
    public void GetTypicalBiomes_DeepRoots_HasSpecificBiomes()
    {
        // Arrange
        var layer = VerticalLayer.DeepRoots;

        // Act
        var biomes = layer.GetTypicalBiomes();

        // Assert
        Assert.IsTrue(biomes.Count > 0);
        Assert.IsTrue(biomes.Contains("The_Roots") || biomes.Contains("Jotunheim"));
    }

    [TestMethod]
    public void GetCharacteristics_AllLayers_ReturnsNonEmpty()
    {
        // Arrange
        var layers = Enum.GetValues<VerticalLayer>();

        // Act & Assert
        foreach (var layer in layers)
        {
            var characteristics = layer.GetCharacteristics();
            Assert.IsFalse(string.IsNullOrEmpty(characteristics),
                $"Layer {layer} should have characteristics");
        }
    }

    [TestMethod]
    public void FromZCoordinate_NegativeThree_ReturnsDeepRoots()
    {
        // Arrange & Act
        var layer = VerticalLayerExtensions.FromZCoordinate(-3);

        // Assert
        Assert.AreEqual(VerticalLayer.DeepRoots, layer);
    }

    [TestMethod]
    public void FromZCoordinate_Zero_ReturnsGroundLevel()
    {
        // Arrange & Act
        var layer = VerticalLayerExtensions.FromZCoordinate(0);

        // Assert
        Assert.AreEqual(VerticalLayer.GroundLevel, layer);
    }

    [TestMethod]
    public void FromZCoordinate_PositiveThree_ReturnsCanopy()
    {
        // Arrange & Act
        var layer = VerticalLayerExtensions.FromZCoordinate(3);

        // Assert
        Assert.AreEqual(VerticalLayer.Canopy, layer);
    }

    [TestMethod]
    public void FromZCoordinate_OutOfBounds_ReturnsGroundLevel()
    {
        // Arrange & Act
        var layerTooLow = VerticalLayerExtensions.FromZCoordinate(-10);
        var layerTooHigh = VerticalLayerExtensions.FromZCoordinate(10);

        // Assert
        Assert.AreEqual(VerticalLayer.GroundLevel, layerTooLow);
        Assert.AreEqual(VerticalLayer.GroundLevel, layerTooHigh);
    }

    [TestMethod]
    public void IsBelowGround_NegativeLayers_ReturnsTrue()
    {
        // Arrange & Act & Assert
        Assert.IsTrue(VerticalLayer.DeepRoots.IsBelowGround());
        Assert.IsTrue(VerticalLayer.LowerRoots.IsBelowGround());
        Assert.IsTrue(VerticalLayer.UpperRoots.IsBelowGround());
    }

    [TestMethod]
    public void IsBelowGround_GroundLevel_ReturnsFalse()
    {
        // Arrange & Act
        var result = VerticalLayer.GroundLevel.IsBelowGround();

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void IsBelowGround_PositiveLayers_ReturnsFalse()
    {
        // Arrange & Act & Assert
        Assert.IsFalse(VerticalLayer.LowerTrunk.IsBelowGround());
        Assert.IsFalse(VerticalLayer.UpperTrunk.IsBelowGround());
        Assert.IsFalse(VerticalLayer.Canopy.IsBelowGround());
    }

    [TestMethod]
    public void IsAboveGround_PositiveLayers_ReturnsTrue()
    {
        // Arrange & Act & Assert
        Assert.IsTrue(VerticalLayer.LowerTrunk.IsAboveGround());
        Assert.IsTrue(VerticalLayer.UpperTrunk.IsAboveGround());
        Assert.IsTrue(VerticalLayer.Canopy.IsAboveGround());
    }

    [TestMethod]
    public void IsAboveGround_GroundLevel_ReturnsFalse()
    {
        // Arrange & Act
        var result = VerticalLayer.GroundLevel.IsAboveGround();

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void DistanceTo_SameLayer_ReturnsZero()
    {
        // Arrange
        var layer = VerticalLayer.GroundLevel;

        // Act
        var distance = layer.DistanceTo(VerticalLayer.GroundLevel);

        // Assert
        Assert.AreEqual(0, distance);
    }

    [TestMethod]
    public void DistanceTo_AdjacentLayers_ReturnsOne()
    {
        // Arrange
        var layer1 = VerticalLayer.GroundLevel;
        var layer2 = VerticalLayer.UpperRoots;

        // Act
        var distance = layer1.DistanceTo(layer2);

        // Assert
        Assert.AreEqual(1, distance);
    }

    [TestMethod]
    public void DistanceTo_OppositeEnds_ReturnsSix()
    {
        // Arrange
        var deepest = VerticalLayer.DeepRoots;
        var highest = VerticalLayer.Canopy;

        // Act
        var distance = deepest.DistanceTo(highest);

        // Assert
        Assert.AreEqual(6, distance); // -3 to +3 = 6 layers apart
    }

    [TestMethod]
    public void GetDepthNarrative_BelowGround_IncludesBeneath()
    {
        // Arrange
        var layer = VerticalLayer.LowerRoots;

        // Act
        var narrative = layer.GetDepthNarrative();

        // Assert
        Assert.IsTrue(narrative.Contains("beneath"));
        Assert.IsTrue(narrative.Contains("200"));
    }

    [TestMethod]
    public void GetDepthNarrative_AboveGround_IncludesAbove()
    {
        // Arrange
        var layer = VerticalLayer.UpperTrunk;

        // Act
        var narrative = layer.GetDepthNarrative();

        // Assert
        Assert.IsTrue(narrative.Contains("above"));
        Assert.IsTrue(narrative.Contains("200"));
    }

    [TestMethod]
    public void GetDepthNarrative_GroundLevel_IncludesGroundLevel()
    {
        // Arrange
        var layer = VerticalLayer.GroundLevel;

        // Act
        var narrative = layer.GetDepthNarrative();

        // Assert
        Assert.IsTrue(narrative.Contains("ground level"));
    }

    [TestMethod]
    public void EnumValues_MatchZCoordinates()
    {
        // Verify that enum int values match Z coordinates
        Assert.AreEqual(-3, (int)VerticalLayer.DeepRoots);
        Assert.AreEqual(-2, (int)VerticalLayer.LowerRoots);
        Assert.AreEqual(-1, (int)VerticalLayer.UpperRoots);
        Assert.AreEqual(0, (int)VerticalLayer.GroundLevel);
        Assert.AreEqual(1, (int)VerticalLayer.LowerTrunk);
        Assert.AreEqual(2, (int)VerticalLayer.UpperTrunk);
        Assert.AreEqual(3, (int)VerticalLayer.Canopy);
    }

    [TestMethod]
    public void AllLayers_HaveConsistentZMapping()
    {
        // Arrange
        var layers = new[]
        {
            VerticalLayer.DeepRoots,
            VerticalLayer.LowerRoots,
            VerticalLayer.UpperRoots,
            VerticalLayer.GroundLevel,
            VerticalLayer.LowerTrunk,
            VerticalLayer.UpperTrunk,
            VerticalLayer.Canopy
        };

        // Act & Assert - Verify round-trip conversion
        foreach (var layer in layers)
        {
            var z = (int)layer;
            var convertedBack = VerticalLayerExtensions.FromZCoordinate(z);
            Assert.AreEqual(layer, convertedBack,
                $"Layer {layer} should round-trip through Z coordinate {z}");
        }
    }
}
