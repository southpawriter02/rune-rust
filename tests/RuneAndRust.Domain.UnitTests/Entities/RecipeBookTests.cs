using FluentAssertions;
using RuneAndRust.Domain.Entities;

namespace RuneAndRust.Domain.UnitTests.Entities;

/// <summary>
/// Unit tests for the RecipeBook entity.
/// </summary>
/// <remarks>
/// Tests cover:
/// <list type="bullet">
///   <item><description>Factory creation with player ID</description></item>
///   <item><description>Learning new recipes</description></item>
///   <item><description>Duplicate recipe handling</description></item>
///   <item><description>Case-insensitive lookups</description></item>
///   <item><description>Default recipe initialization</description></item>
///   <item><description>Learning timestamp tracking</description></item>
/// </list>
/// </remarks>
[TestFixture]
public class RecipeBookTests
{
    // ═══════════════════════════════════════════════════════════════
    // FACTORY TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void Create_WithPlayerId_CreatesEmptyBook()
    {
        // Arrange
        var playerId = Guid.NewGuid();

        // Act
        var recipeBook = RecipeBook.Create(playerId);

        // Assert
        recipeBook.Should().NotBeNull();
        recipeBook.Id.Should().NotBe(Guid.Empty);
        recipeBook.PlayerId.Should().Be(playerId);
        recipeBook.KnownRecipeIds.Should().BeEmpty();
        recipeBook.KnownCount.Should().Be(0);
    }

    [Test]
    public void Create_GeneratesUniqueIds()
    {
        // Arrange
        var playerId = Guid.NewGuid();

        // Act
        var book1 = RecipeBook.Create(playerId);
        var book2 = RecipeBook.Create(playerId);

        // Assert
        book1.Id.Should().NotBe(book2.Id);
    }

    // ═══════════════════════════════════════════════════════════════
    // LEARNING TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void Learn_NewRecipe_ReturnsTrue()
    {
        // Arrange
        var recipeBook = RecipeBook.Create(Guid.NewGuid());

        // Act
        var result = recipeBook.Learn("iron-sword");

        // Assert
        result.Should().BeTrue();
        recipeBook.KnownCount.Should().Be(1);
        recipeBook.IsKnown("iron-sword").Should().BeTrue();
    }

    [Test]
    public void Learn_AlreadyKnownRecipe_ReturnsFalse()
    {
        // Arrange
        var recipeBook = RecipeBook.Create(Guid.NewGuid());
        recipeBook.Learn("iron-sword");

        // Act
        var result = recipeBook.Learn("iron-sword");

        // Assert
        result.Should().BeFalse();
        recipeBook.KnownCount.Should().Be(1);
    }

    [Test]
    public void Learn_MultipleRecipes_TracksAll()
    {
        // Arrange
        var recipeBook = RecipeBook.Create(Guid.NewGuid());

        // Act
        recipeBook.Learn("iron-sword");
        recipeBook.Learn("steel-sword");
        recipeBook.Learn("healing-potion");

        // Assert
        recipeBook.KnownCount.Should().Be(3);
        recipeBook.IsKnown("iron-sword").Should().BeTrue();
        recipeBook.IsKnown("steel-sword").Should().BeTrue();
        recipeBook.IsKnown("healing-potion").Should().BeTrue();
    }

    [Test]
    public void Learn_WithNullId_ThrowsArgumentException()
    {
        // Arrange
        var recipeBook = RecipeBook.Create(Guid.NewGuid());

        // Act
        var act = () => recipeBook.Learn(null!);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Test]
    public void Learn_WithEmptyId_ThrowsArgumentException()
    {
        // Arrange
        var recipeBook = RecipeBook.Create(Guid.NewGuid());

        // Act
        var act = () => recipeBook.Learn(string.Empty);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Test]
    public void Learn_WithWhitespaceId_ThrowsArgumentException()
    {
        // Arrange
        var recipeBook = RecipeBook.Create(Guid.NewGuid());

        // Act
        var act = () => recipeBook.Learn("   ");

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    // ═══════════════════════════════════════════════════════════════
    // CASE-INSENSITIVITY TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void IsKnown_IsCaseInsensitive()
    {
        // Arrange
        var recipeBook = RecipeBook.Create(Guid.NewGuid());
        recipeBook.Learn("Iron-Sword");

        // Act & Assert
        recipeBook.IsKnown("iron-sword").Should().BeTrue();
        recipeBook.IsKnown("IRON-SWORD").Should().BeTrue();
        recipeBook.IsKnown("Iron-Sword").Should().BeTrue();
        recipeBook.IsKnown("iRoN-sWoRd").Should().BeTrue();
    }

    [Test]
    public void Learn_IsCaseInsensitive_PreventsDuplicates()
    {
        // Arrange
        var recipeBook = RecipeBook.Create(Guid.NewGuid());

        // Act
        var result1 = recipeBook.Learn("Iron-Sword");
        var result2 = recipeBook.Learn("iron-sword");
        var result3 = recipeBook.Learn("IRON-SWORD");

        // Assert
        result1.Should().BeTrue();
        result2.Should().BeFalse();
        result3.Should().BeFalse();
        recipeBook.KnownCount.Should().Be(1);
    }

    [Test]
    public void IsKnown_UnknownRecipe_ReturnsFalse()
    {
        // Arrange
        var recipeBook = RecipeBook.Create(Guid.NewGuid());
        recipeBook.Learn("iron-sword");

        // Act & Assert
        recipeBook.IsKnown("steel-sword").Should().BeFalse();
        recipeBook.IsKnown("unknown-recipe").Should().BeFalse();
    }

    [Test]
    public void IsKnown_WithNullId_ReturnsFalse()
    {
        // Arrange
        var recipeBook = RecipeBook.Create(Guid.NewGuid());
        recipeBook.Learn("iron-sword");

        // Act & Assert
        recipeBook.IsKnown(null!).Should().BeFalse();
    }

    [Test]
    public void IsKnown_WithEmptyId_ReturnsFalse()
    {
        // Arrange
        var recipeBook = RecipeBook.Create(Guid.NewGuid());
        recipeBook.Learn("iron-sword");

        // Act & Assert
        recipeBook.IsKnown(string.Empty).Should().BeFalse();
    }

    // ═══════════════════════════════════════════════════════════════
    // TIMESTAMP TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void Learn_TracksTimestamp()
    {
        // Arrange
        var recipeBook = RecipeBook.Create(Guid.NewGuid());
        var beforeLearn = DateTime.UtcNow;

        // Act
        recipeBook.Learn("iron-sword");
        var afterLearn = DateTime.UtcNow;

        // Assert
        var learnedDate = recipeBook.GetLearnedDate("iron-sword");
        learnedDate.Should().NotBeNull();
        learnedDate!.Value.Should().BeOnOrAfter(beforeLearn);
        learnedDate!.Value.Should().BeOnOrBefore(afterLearn);
    }

    [Test]
    public void GetLearnedDate_UnknownRecipe_ReturnsNull()
    {
        // Arrange
        var recipeBook = RecipeBook.Create(Guid.NewGuid());
        recipeBook.Learn("iron-sword");

        // Act
        var learnedDate = recipeBook.GetLearnedDate("unknown-recipe");

        // Assert
        learnedDate.Should().BeNull();
    }

    [Test]
    public void GetLearnedDate_IsCaseInsensitive()
    {
        // Arrange
        var recipeBook = RecipeBook.Create(Guid.NewGuid());
        recipeBook.Learn("iron-sword");

        // Act & Assert
        recipeBook.GetLearnedDate("iron-sword").Should().NotBeNull();
        recipeBook.GetLearnedDate("IRON-SWORD").Should().NotBeNull();
        recipeBook.GetLearnedDate("Iron-Sword").Should().NotBeNull();
    }

    [Test]
    public void GetLearnedDate_WithNullId_ReturnsNull()
    {
        // Arrange
        var recipeBook = RecipeBook.Create(Guid.NewGuid());
        recipeBook.Learn("iron-sword");

        // Act & Assert
        recipeBook.GetLearnedDate(null!).Should().BeNull();
    }

    // ═══════════════════════════════════════════════════════════════
    // DEFAULT INITIALIZATION TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void InitializeDefaults_AddsAllRecipes()
    {
        // Arrange
        var recipeBook = RecipeBook.Create(Guid.NewGuid());
        var defaultRecipes = new[] { "iron-sword", "healing-potion", "leather-armor" };

        // Act
        recipeBook.InitializeDefaults(defaultRecipes);

        // Assert
        recipeBook.KnownCount.Should().Be(3);
        recipeBook.IsKnown("iron-sword").Should().BeTrue();
        recipeBook.IsKnown("healing-potion").Should().BeTrue();
        recipeBook.IsKnown("leather-armor").Should().BeTrue();
    }

    [Test]
    public void InitializeDefaults_SkipsAlreadyKnownRecipes()
    {
        // Arrange
        var recipeBook = RecipeBook.Create(Guid.NewGuid());
        recipeBook.Learn("iron-sword");
        var defaultRecipes = new[] { "iron-sword", "healing-potion" };

        // Act
        recipeBook.InitializeDefaults(defaultRecipes);

        // Assert
        recipeBook.KnownCount.Should().Be(2);
    }

    [Test]
    public void InitializeDefaults_WithEmptyList_DoesNothing()
    {
        // Arrange
        var recipeBook = RecipeBook.Create(Guid.NewGuid());
        recipeBook.Learn("iron-sword");

        // Act
        recipeBook.InitializeDefaults(Array.Empty<string>());

        // Assert
        recipeBook.KnownCount.Should().Be(1);
    }

    [Test]
    public void InitializeDefaults_WithNullEnumerable_ThrowsArgumentNullException()
    {
        // Arrange
        var recipeBook = RecipeBook.Create(Guid.NewGuid());

        // Act
        var act = () => recipeBook.InitializeDefaults(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    // ═══════════════════════════════════════════════════════════════
    // KNOWN RECIPE IDS COLLECTION TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void KnownRecipeIds_ReturnsReadOnlySet()
    {
        // Arrange
        var recipeBook = RecipeBook.Create(Guid.NewGuid());
        recipeBook.Learn("iron-sword");
        recipeBook.Learn("healing-potion");

        // Act
        var knownIds = recipeBook.KnownRecipeIds;

        // Assert
        knownIds.Should().HaveCount(2);
        knownIds.Should().Contain("iron-sword");
        knownIds.Should().Contain("healing-potion");
    }

    [Test]
    public void KnownRecipeIds_IsImmutable()
    {
        // Arrange
        var recipeBook = RecipeBook.Create(Guid.NewGuid());
        recipeBook.Learn("iron-sword");

        // Act
        var knownIds = recipeBook.KnownRecipeIds;

        // Assert
        // The collection should be read-only; verify it's the expected type
        knownIds.Should().BeAssignableTo<IReadOnlySet<string>>();
    }
}
