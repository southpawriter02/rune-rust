using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.UnitTests.Entities;

/// <summary>
/// Unit tests for Item.CreateRecipeScroll factory method (v0.11.1c).
/// </summary>
/// <remarks>
/// <para>Tests cover:</para>
/// <list type="bullet">
///   <item><description>Recipe scroll creation with valid parameters</description></item>
///   <item><description>Recipe ID normalization to lowercase</description></item>
///   <item><description>Correct effect type assignment</description></item>
///   <item><description>Name and description formatting</description></item>
///   <item><description>Parameter validation</description></item>
/// </list>
/// </remarks>
[TestFixture]
public class ItemRecipeScrollTests
{
    // ═══════════════════════════════════════════════════════════════
    // CreateRecipeScroll TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that CreateRecipeScroll creates a scroll item with correct properties.
    /// </summary>
    [Test]
    public void CreateRecipeScroll_WithValidParameters_ReturnsScrollWithCorrectProperties()
    {
        // Arrange & Act
        var scroll = Item.CreateRecipeScroll(
            recipeId: "steel-sword",
            recipeName: "Steel Sword",
            baseValue: 150);

        // Assert
        scroll.Should().NotBeNull();
        scroll.Name.Should().Be("Recipe Scroll: Steel Sword");
        scroll.Description.Should().Contain("Steel Sword");
        scroll.Description.Should().Contain("recipe");
        scroll.Type.Should().Be(ItemType.Consumable);
        scroll.Effect.Should().Be(ItemEffect.LearnRecipe);
        scroll.EffectValue.Should().Be(0);
        scroll.RecipeId.Should().Be("steel-sword");
        scroll.Value.Should().Be(150);
        scroll.IsRecipeScroll.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that CreateRecipeScroll normalizes recipe ID to lowercase.
    /// </summary>
    [Test]
    public void CreateRecipeScroll_NormalizesRecipeIdToLowercase()
    {
        // Arrange & Act
        var scroll = Item.CreateRecipeScroll(
            recipeId: "STEEL-SWORD",
            recipeName: "Steel Sword");

        // Assert
        scroll.RecipeId.Should().Be("steel-sword");
    }

    /// <summary>
    /// Verifies that CreateRecipeScroll handles mixed case recipe IDs correctly.
    /// </summary>
    [Test]
    public void CreateRecipeScroll_MixedCaseRecipeId_NormalizesToLowercase()
    {
        // Arrange & Act
        var scroll = Item.CreateRecipeScroll(
            recipeId: "Fire-Resistance-Potion",
            recipeName: "Fire Resistance Potion");

        // Assert
        scroll.RecipeId.Should().Be("fire-resistance-potion");
    }

    /// <summary>
    /// Verifies that CreateRecipeScroll uses default base value when not specified.
    /// </summary>
    [Test]
    public void CreateRecipeScroll_WithoutBaseValue_UsesDefaultValue()
    {
        // Arrange & Act
        var scroll = Item.CreateRecipeScroll(
            recipeId: "iron-sword",
            recipeName: "Iron Sword");

        // Assert
        scroll.Value.Should().Be(100); // Default value
    }

    /// <summary>
    /// Verifies that CreateRecipeScroll throws for null recipe ID.
    /// </summary>
    [Test]
    public void CreateRecipeScroll_NullRecipeId_ThrowsArgumentException()
    {
        // Act
        var act = () => Item.CreateRecipeScroll(
            recipeId: null!,
            recipeName: "Test Recipe");

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    /// <summary>
    /// Verifies that CreateRecipeScroll throws for empty recipe ID.
    /// </summary>
    [Test]
    public void CreateRecipeScroll_EmptyRecipeId_ThrowsArgumentException()
    {
        // Act
        var act = () => Item.CreateRecipeScroll(
            recipeId: "",
            recipeName: "Test Recipe");

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    /// <summary>
    /// Verifies that CreateRecipeScroll throws for whitespace recipe ID.
    /// </summary>
    [Test]
    public void CreateRecipeScroll_WhitespaceRecipeId_ThrowsArgumentException()
    {
        // Act
        var act = () => Item.CreateRecipeScroll(
            recipeId: "   ",
            recipeName: "Test Recipe");

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    /// <summary>
    /// Verifies that CreateRecipeScroll throws for null recipe name.
    /// </summary>
    [Test]
    public void CreateRecipeScroll_NullRecipeName_ThrowsArgumentException()
    {
        // Act
        var act = () => Item.CreateRecipeScroll(
            recipeId: "test-recipe",
            recipeName: null!);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    /// <summary>
    /// Verifies that CreateRecipeScroll throws for empty recipe name.
    /// </summary>
    [Test]
    public void CreateRecipeScroll_EmptyRecipeName_ThrowsArgumentException()
    {
        // Act
        var act = () => Item.CreateRecipeScroll(
            recipeId: "test-recipe",
            recipeName: "");

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    // ═══════════════════════════════════════════════════════════════
    // IsRecipeScroll PROPERTY TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that IsRecipeScroll returns true for recipe scroll items.
    /// </summary>
    [Test]
    public void IsRecipeScroll_ForRecipeScrollItem_ReturnsTrue()
    {
        // Arrange
        var scroll = Item.CreateRecipeScroll("test-recipe", "Test Recipe");

        // Act & Assert
        scroll.IsRecipeScroll.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that IsRecipeScroll returns false for non-scroll items.
    /// </summary>
    [Test]
    public void IsRecipeScroll_ForNonScrollItem_ReturnsFalse()
    {
        // Arrange
        var sword = Item.CreateSword();
        var potion = Item.CreateHealthPotion();
        var scroll = Item.CreateScroll(); // Quest scroll, not recipe scroll

        // Act & Assert
        sword.IsRecipeScroll.Should().BeFalse();
        potion.IsRecipeScroll.Should().BeFalse();
        scroll.IsRecipeScroll.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that IsRecipeScroll requires both LearnRecipe effect and RecipeId.
    /// </summary>
    [Test]
    public void IsRecipeScroll_RequiresBothEffectAndRecipeId()
    {
        // Arrange - Item with LearnRecipe effect but no RecipeId
        var itemWithEffectNoId = new Item(
            name: "Test Scroll",
            description: "A test scroll",
            type: ItemType.Consumable,
            effect: ItemEffect.LearnRecipe,
            recipeId: null);

        // Arrange - Item with RecipeId but different effect
        var itemWithIdNoEffect = new Item(
            name: "Test Item",
            description: "A test item",
            type: ItemType.Consumable,
            effect: ItemEffect.Heal,
            recipeId: "some-recipe");

        // Act & Assert
        itemWithEffectNoId.IsRecipeScroll.Should().BeFalse();
        itemWithIdNoEffect.IsRecipeScroll.Should().BeFalse();
    }

    // ═══════════════════════════════════════════════════════════════
    // RecipeId PROPERTY TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that RecipeId is null for non-scroll items.
    /// </summary>
    [Test]
    public void RecipeId_ForNonScrollItem_IsNull()
    {
        // Arrange
        var sword = Item.CreateSword();
        var potion = Item.CreateHealthPotion();

        // Act & Assert
        sword.RecipeId.Should().BeNull();
        potion.RecipeId.Should().BeNull();
    }

    /// <summary>
    /// Verifies that each scroll has a unique ID.
    /// </summary>
    [Test]
    public void CreateRecipeScroll_CreatesUniqueItemId()
    {
        // Arrange & Act
        var scroll1 = Item.CreateRecipeScroll("steel-sword", "Steel Sword");
        var scroll2 = Item.CreateRecipeScroll("steel-sword", "Steel Sword");

        // Assert
        scroll1.Id.Should().NotBe(scroll2.Id);
    }
}
