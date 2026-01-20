using FluentAssertions;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

/// <summary>
/// Unit tests for the <see cref="RecipeIngredient"/> value object.
/// </summary>
/// <remarks>
/// Tests cover:
/// <list type="bullet">
///   <item><description>Valid construction with property assignment</description></item>
///   <item><description>Resource ID normalization to lowercase</description></item>
///   <item><description>Invalid resource ID validation</description></item>
///   <item><description>Invalid quantity validation</description></item>
///   <item><description>ToString formatting</description></item>
/// </list>
/// </remarks>
[TestFixture]
public class RecipeIngredientTests
{
    // ═══════════════════════════════════════════════════════════════
    // VALID CONSTRUCTION TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that creating an ingredient with valid input sets properties correctly
    /// and normalizes the resource ID to lowercase.
    /// </summary>
    [Test]
    public void Create_WithValidInput_SetsPropertiesAndNormalizesId()
    {
        // Arrange & Act
        var ingredient = new RecipeIngredient("Iron-Ore", 5);

        // Assert
        ingredient.ResourceId.Should().Be("iron-ore");
        ingredient.Quantity.Should().Be(5);
    }

    /// <summary>
    /// Verifies that creating an ingredient with a lowercase ID preserves the ID.
    /// </summary>
    [Test]
    public void Create_WithLowercaseId_PreservesId()
    {
        // Arrange & Act
        var ingredient = new RecipeIngredient("healing-herb", 3);

        // Assert
        ingredient.ResourceId.Should().Be("healing-herb");
        ingredient.Quantity.Should().Be(3);
    }

    /// <summary>
    /// Verifies that creating an ingredient with quantity of 1 works correctly.
    /// </summary>
    [Test]
    public void Create_WithMinimumQuantity_Succeeds()
    {
        // Arrange & Act
        var ingredient = new RecipeIngredient("empty-vial", 1);

        // Assert
        ingredient.ResourceId.Should().Be("empty-vial");
        ingredient.Quantity.Should().Be(1);
    }

    /// <summary>
    /// Verifies that creating an ingredient with a large quantity works correctly.
    /// </summary>
    [Test]
    public void Create_WithLargeQuantity_Succeeds()
    {
        // Arrange & Act
        var ingredient = new RecipeIngredient("iron-ore", 999);

        // Assert
        ingredient.Quantity.Should().Be(999);
    }

    // ═══════════════════════════════════════════════════════════════
    // INVALID RESOURCE ID TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that creating an ingredient with a null resource ID throws ArgumentException.
    /// </summary>
    [Test]
    public void Create_WithNullResourceId_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => new RecipeIngredient(null!, 1);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("resourceId");
    }

    /// <summary>
    /// Verifies that creating an ingredient with an empty resource ID throws ArgumentException.
    /// </summary>
    [Test]
    public void Create_WithEmptyResourceId_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => new RecipeIngredient("", 1);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("resourceId");
    }

    /// <summary>
    /// Verifies that creating an ingredient with a whitespace-only resource ID throws ArgumentException.
    /// </summary>
    [Test]
    public void Create_WithWhitespaceResourceId_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => new RecipeIngredient("   ", 1);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("resourceId");
    }

    // ═══════════════════════════════════════════════════════════════
    // INVALID QUANTITY TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that creating an ingredient with zero quantity throws ArgumentOutOfRangeException.
    /// </summary>
    [Test]
    public void Create_WithZeroQuantity_ThrowsArgumentOutOfRangeException()
    {
        // Arrange & Act
        var act = () => new RecipeIngredient("iron-ore", 0);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("quantity");
    }

    /// <summary>
    /// Verifies that creating an ingredient with negative quantity throws ArgumentOutOfRangeException.
    /// </summary>
    [Test]
    public void Create_WithNegativeQuantity_ThrowsArgumentOutOfRangeException()
    {
        // Arrange & Act
        var act = () => new RecipeIngredient("iron-ore", -1);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("quantity");
    }

    /// <summary>
    /// Verifies that creating an ingredient with very negative quantity throws ArgumentOutOfRangeException.
    /// </summary>
    [Test]
    public void Create_WithVeryNegativeQuantity_ThrowsArgumentOutOfRangeException()
    {
        // Arrange & Act
        var act = () => new RecipeIngredient("iron-ore", -100);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("quantity");
    }

    // ═══════════════════════════════════════════════════════════════
    // TOSTRING TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that ToString returns the expected format.
    /// </summary>
    [Test]
    public void ToString_ReturnsExpectedFormat()
    {
        // Arrange
        var ingredient = new RecipeIngredient("iron-ore", 5);

        // Act
        var result = ingredient.ToString();

        // Assert
        result.Should().Be("iron-ore x5");
    }

    /// <summary>
    /// Verifies that ToString with quantity of 1 formats correctly.
    /// </summary>
    [Test]
    public void ToString_WithQuantityOne_ReturnsExpectedFormat()
    {
        // Arrange
        var ingredient = new RecipeIngredient("empty-vial", 1);

        // Act
        var result = ingredient.ToString();

        // Assert
        result.Should().Be("empty-vial x1");
    }

    // ═══════════════════════════════════════════════════════════════
    // EQUALITY TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that two ingredients with same values are equal (record equality).
    /// </summary>
    [Test]
    public void Equals_SameValues_ReturnsTrue()
    {
        // Arrange
        var ingredient1 = new RecipeIngredient("iron-ore", 5);
        var ingredient2 = new RecipeIngredient("iron-ore", 5);

        // Assert
        ingredient1.Should().Be(ingredient2);
    }

    /// <summary>
    /// Verifies that two ingredients with different quantities are not equal.
    /// </summary>
    [Test]
    public void Equals_DifferentQuantity_ReturnsFalse()
    {
        // Arrange
        var ingredient1 = new RecipeIngredient("iron-ore", 5);
        var ingredient2 = new RecipeIngredient("iron-ore", 3);

        // Assert
        ingredient1.Should().NotBe(ingredient2);
    }

    /// <summary>
    /// Verifies that two ingredients with different resource IDs are not equal.
    /// </summary>
    [Test]
    public void Equals_DifferentResourceId_ReturnsFalse()
    {
        // Arrange
        var ingredient1 = new RecipeIngredient("iron-ore", 5);
        var ingredient2 = new RecipeIngredient("copper-ore", 5);

        // Assert
        ingredient1.Should().NotBe(ingredient2);
    }
}
