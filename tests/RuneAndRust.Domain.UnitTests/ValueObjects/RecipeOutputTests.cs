using FluentAssertions;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

/// <summary>
/// Unit tests for the <see cref="RecipeOutput"/> value object.
/// </summary>
/// <remarks>
/// Tests cover:
/// <list type="bullet">
///   <item><description>Valid construction with property assignment</description></item>
///   <item><description>Item ID normalization to lowercase</description></item>
///   <item><description>Quality formula handling and HasQualityScaling property</description></item>
///   <item><description>Invalid item ID validation</description></item>
///   <item><description>Invalid quantity validation</description></item>
///   <item><description>ToString formatting with and without quality scaling</description></item>
/// </list>
/// </remarks>
[TestFixture]
public class RecipeOutputTests
{
    // ═══════════════════════════════════════════════════════════════
    // VALID CONSTRUCTION TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that creating an output with valid input sets properties correctly
    /// and normalizes the item ID to lowercase.
    /// </summary>
    [Test]
    public void Create_WithValidInput_SetsPropertiesAndNormalizesId()
    {
        // Arrange & Act
        var output = new RecipeOutput("Iron-Sword", 1);

        // Assert
        output.ItemId.Should().Be("iron-sword");
        output.Quantity.Should().Be(1);
        output.QualityFormula.Should().BeNull();
        output.HasQualityScaling.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that creating an output with a lowercase ID preserves the ID.
    /// </summary>
    [Test]
    public void Create_WithLowercaseId_PreservesId()
    {
        // Arrange & Act
        var output = new RecipeOutput("healing-potion", 2);

        // Assert
        output.ItemId.Should().Be("healing-potion");
        output.Quantity.Should().Be(2);
    }

    /// <summary>
    /// Verifies that creating an output with quantity of 1 works correctly.
    /// </summary>
    [Test]
    public void Create_WithMinimumQuantity_Succeeds()
    {
        // Arrange & Act
        var output = new RecipeOutput("iron-ingot", 1);

        // Assert
        output.ItemId.Should().Be("iron-ingot");
        output.Quantity.Should().Be(1);
    }

    /// <summary>
    /// Verifies that creating an output with a large quantity works correctly.
    /// </summary>
    [Test]
    public void Create_WithLargeQuantity_Succeeds()
    {
        // Arrange & Act
        var output = new RecipeOutput("arrow", 50);

        // Assert
        output.Quantity.Should().Be(50);
    }

    // ═══════════════════════════════════════════════════════════════
    // QUALITY FORMULA TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that creating an output with a quality formula sets HasQualityScaling to true.
    /// </summary>
    [Test]
    public void Create_WithQualityFormula_SetsHasQualityScalingTrue()
    {
        // Arrange & Act
        var output = new RecipeOutput(
            "mithril-blade",
            1,
            "roll >= 20 ? 'Legendary' : roll >= 15 ? 'Epic' : 'Rare'");

        // Assert
        output.QualityFormula.Should().Be("roll >= 20 ? 'Legendary' : roll >= 15 ? 'Epic' : 'Rare'");
        output.HasQualityScaling.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that creating an output without a quality formula sets HasQualityScaling to false.
    /// </summary>
    [Test]
    public void Create_WithoutQualityFormula_SetsHasQualityScalingFalse()
    {
        // Arrange & Act
        var output = new RecipeOutput("iron-sword", 1);

        // Assert
        output.QualityFormula.Should().BeNull();
        output.HasQualityScaling.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that creating an output with null quality formula sets HasQualityScaling to false.
    /// </summary>
    [Test]
    public void Create_WithNullQualityFormula_SetsHasQualityScalingFalse()
    {
        // Arrange & Act
        var output = new RecipeOutput("iron-sword", 1, null);

        // Assert
        output.QualityFormula.Should().BeNull();
        output.HasQualityScaling.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that creating an output with empty quality formula sets HasQualityScaling to false.
    /// </summary>
    [Test]
    public void Create_WithEmptyQualityFormula_SetsHasQualityScalingFalse()
    {
        // Arrange & Act
        var output = new RecipeOutput("iron-sword", 1, "");

        // Assert
        output.QualityFormula.Should().Be("");
        output.HasQualityScaling.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that creating an output with whitespace-only quality formula
    /// preserves the formula and treats it as having quality scaling.
    /// </summary>
    /// <remarks>
    /// While whitespace-only formulas are not valid in practice, the implementation
    /// uses IsNullOrEmpty (not IsNullOrWhiteSpace), so whitespace is treated as
    /// a non-empty formula. This is intentional - validation of formula content
    /// is expected to happen at a higher level (e.g., during parsing/evaluation).
    /// </remarks>
    [Test]
    public void Create_WithWhitespaceQualityFormula_PreservesFormula()
    {
        // Arrange & Act
        var output = new RecipeOutput("iron-sword", 1, "   ");

        // Assert
        output.QualityFormula.Should().Be("   ");
        // Whitespace is not null or empty, so HasQualityScaling returns true
        output.HasQualityScaling.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that a simple quality formula is preserved.
    /// </summary>
    [Test]
    public void Create_WithSimpleQualityFormula_PreservesFormula()
    {
        // Arrange & Act
        var output = new RecipeOutput("steel-sword", 1, "roll >= 18 ? 'Fine' : 'Common'");

        // Assert
        output.QualityFormula.Should().Be("roll >= 18 ? 'Fine' : 'Common'");
        output.HasQualityScaling.Should().BeTrue();
    }

    // ═══════════════════════════════════════════════════════════════
    // INVALID ITEM ID TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that creating an output with a null item ID throws ArgumentException.
    /// </summary>
    [Test]
    public void Create_WithNullItemId_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => new RecipeOutput(null!, 1);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("itemId");
    }

    /// <summary>
    /// Verifies that creating an output with an empty item ID throws ArgumentException.
    /// </summary>
    [Test]
    public void Create_WithEmptyItemId_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => new RecipeOutput("", 1);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("itemId");
    }

    /// <summary>
    /// Verifies that creating an output with a whitespace-only item ID throws ArgumentException.
    /// </summary>
    [Test]
    public void Create_WithWhitespaceItemId_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => new RecipeOutput("   ", 1);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("itemId");
    }

    // ═══════════════════════════════════════════════════════════════
    // INVALID QUANTITY TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that creating an output with zero quantity throws ArgumentOutOfRangeException.
    /// </summary>
    [Test]
    public void Create_WithZeroQuantity_ThrowsArgumentOutOfRangeException()
    {
        // Arrange & Act
        var act = () => new RecipeOutput("iron-sword", 0);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("quantity");
    }

    /// <summary>
    /// Verifies that creating an output with negative quantity throws ArgumentOutOfRangeException.
    /// </summary>
    [Test]
    public void Create_WithNegativeQuantity_ThrowsArgumentOutOfRangeException()
    {
        // Arrange & Act
        var act = () => new RecipeOutput("iron-sword", -1);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("quantity");
    }

    /// <summary>
    /// Verifies that creating an output with very negative quantity throws ArgumentOutOfRangeException.
    /// </summary>
    [Test]
    public void Create_WithVeryNegativeQuantity_ThrowsArgumentOutOfRangeException()
    {
        // Arrange & Act
        var act = () => new RecipeOutput("iron-sword", -100);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("quantity");
    }

    // ═══════════════════════════════════════════════════════════════
    // TOSTRING TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that ToString returns the expected format without quality scaling.
    /// </summary>
    [Test]
    public void ToString_WithoutQualityScaling_ReturnsExpectedFormat()
    {
        // Arrange
        var output = new RecipeOutput("iron-sword", 1);

        // Act
        var result = output.ToString();

        // Assert
        result.Should().Be("iron-sword x1");
    }

    /// <summary>
    /// Verifies that ToString returns the expected format with quality scaling.
    /// </summary>
    [Test]
    public void ToString_WithQualityScaling_ReturnsExpectedFormat()
    {
        // Arrange
        var output = new RecipeOutput("mithril-blade", 1, "roll >= 20 ? 'Legendary' : 'Rare'");

        // Act
        var result = output.ToString();

        // Assert
        result.Should().Be("mithril-blade x1 (quality scaling)");
    }

    /// <summary>
    /// Verifies that ToString with quantity greater than 1 formats correctly.
    /// </summary>
    [Test]
    public void ToString_WithQuantityGreaterThanOne_ReturnsExpectedFormat()
    {
        // Arrange
        var output = new RecipeOutput("arrow", 20);

        // Act
        var result = output.ToString();

        // Assert
        result.Should().Be("arrow x20");
    }

    /// <summary>
    /// Verifies that ToString with quantity greater than 1 and quality scaling formats correctly.
    /// </summary>
    [Test]
    public void ToString_WithQuantityAndQualityScaling_ReturnsExpectedFormat()
    {
        // Arrange
        var output = new RecipeOutput("enchanted-arrow", 10, "roll >= 15 ? 'Fine' : 'Common'");

        // Act
        var result = output.ToString();

        // Assert
        result.Should().Be("enchanted-arrow x10 (quality scaling)");
    }

    // ═══════════════════════════════════════════════════════════════
    // EQUALITY TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that two outputs with same values are equal (record equality).
    /// </summary>
    [Test]
    public void Equals_SameValues_ReturnsTrue()
    {
        // Arrange
        var output1 = new RecipeOutput("iron-sword", 1);
        var output2 = new RecipeOutput("iron-sword", 1);

        // Assert
        output1.Should().Be(output2);
    }

    /// <summary>
    /// Verifies that two outputs with same values including quality formula are equal.
    /// </summary>
    [Test]
    public void Equals_SameValuesWithQualityFormula_ReturnsTrue()
    {
        // Arrange
        var output1 = new RecipeOutput("mithril-blade", 1, "roll >= 20 ? 'Legendary' : 'Rare'");
        var output2 = new RecipeOutput("mithril-blade", 1, "roll >= 20 ? 'Legendary' : 'Rare'");

        // Assert
        output1.Should().Be(output2);
    }

    /// <summary>
    /// Verifies that two outputs with different item IDs are not equal.
    /// </summary>
    [Test]
    public void Equals_DifferentItemId_ReturnsFalse()
    {
        // Arrange
        var output1 = new RecipeOutput("iron-sword", 1);
        var output2 = new RecipeOutput("steel-sword", 1);

        // Assert
        output1.Should().NotBe(output2);
    }

    /// <summary>
    /// Verifies that two outputs with different quantities are not equal.
    /// </summary>
    [Test]
    public void Equals_DifferentQuantity_ReturnsFalse()
    {
        // Arrange
        var output1 = new RecipeOutput("iron-sword", 1);
        var output2 = new RecipeOutput("iron-sword", 2);

        // Assert
        output1.Should().NotBe(output2);
    }

    /// <summary>
    /// Verifies that two outputs with different quality formulas are not equal.
    /// </summary>
    [Test]
    public void Equals_DifferentQualityFormula_ReturnsFalse()
    {
        // Arrange
        var output1 = new RecipeOutput("mithril-blade", 1, "roll >= 20 ? 'Legendary' : 'Rare'");
        var output2 = new RecipeOutput("mithril-blade", 1, "roll >= 15 ? 'Epic' : 'Common'");

        // Assert
        output1.Should().NotBe(output2);
    }

    /// <summary>
    /// Verifies that outputs with and without quality formula are not equal.
    /// </summary>
    [Test]
    public void Equals_WithAndWithoutQualityFormula_ReturnsFalse()
    {
        // Arrange
        var output1 = new RecipeOutput("mithril-blade", 1);
        var output2 = new RecipeOutput("mithril-blade", 1, "roll >= 20 ? 'Legendary' : 'Rare'");

        // Assert
        output1.Should().NotBe(output2);
    }
}
