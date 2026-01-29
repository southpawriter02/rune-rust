using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

/// <summary>
/// Unit tests for <see cref="LootDrop"/> smart loot metadata properties (v0.16.3d).
/// </summary>
/// <remarks>
/// Tests validate the CreateFrom and CreateRandom factory methods,
/// as well as the smart loot metadata properties.
/// </remarks>
[TestFixture]
public class LootDropSmartLootTests
{
    #region Test Data

    private static readonly LootEntry TestSword = LootEntry.Create("iron-sword", "swords");
    private static readonly LootEntry TestShield = LootEntry.Create("wooden-shield", "shields");

    #endregion

    #region CreateFrom Tests

    /// <summary>
    /// Verifies that CreateFrom with a class-appropriate result sets all metadata correctly.
    /// </summary>
    [Test]
    public void CreateFrom_WithClassAppropriateResult_SetsAllProperties()
    {
        // Arrange
        var result = SmartLootResult.CreateClassAppropriate(
            TestSword,
            biasRoll: 42,
            filteredPoolSize: 5,
            totalPoolSize: 10);
        const string archetypeId = "warrior";

        // Act
        var loot = LootDrop.CreateFrom(result, archetypeId, "Goblin");

        // Assert
        loot.HasItems.Should().BeTrue();
        loot.WasClassAppropriate.Should().BeTrue();
        loot.PlayerArchetypeId.Should().Be(archetypeId);
        loot.SelectionReason.Should().Contain("Class-appropriate");
        loot.BiasRoll.Should().Be(42);
        loot.HasPlayerContext.Should().BeTrue();
        loot.SourceMonster.Should().Be("Goblin");
    }

    /// <summary>
    /// Verifies that CreateFrom with a random selection result sets WasClassAppropriate to false.
    /// </summary>
    [Test]
    public void CreateFrom_WithRandomResult_SetsWasClassAppropriateFalse()
    {
        // Arrange
        var result = SmartLootResult.CreateRandom(
            TestShield,
            biasRoll: 75,
            filteredPoolSize: 3,
            totalPoolSize: 10);
        const string archetypeId = "mystic";

        // Act
        var loot = LootDrop.CreateFrom(result, archetypeId, "Skeleton");

        // Assert
        loot.WasClassAppropriate.Should().BeFalse();
        loot.BiasRoll.Should().Be(75);
        loot.PlayerArchetypeId.Should().Be(archetypeId);
        loot.SelectionReason.Should().Contain("Random");
    }

    /// <summary>
    /// Verifies that CreateFrom with an empty result returns LootDrop.Empty.
    /// </summary>
    [Test]
    public void CreateFrom_WithEmptyResult_ReturnsEmpty()
    {
        // Arrange
        var emptyResult = SmartLootResult.Empty;

        // Act
        var loot = LootDrop.CreateFrom(emptyResult, "warrior");

        // Assert
        loot.IsEmpty.Should().BeTrue();
        loot.HasItems.Should().BeFalse();
    }

    #endregion

    #region CreateRandom Tests

    /// <summary>
    /// Verifies that CreateRandom sets default smart loot metadata for context-less drops.
    /// </summary>
    [Test]
    public void CreateRandom_SetsRandomSelectionDefaults()
    {
        // Arrange & Act
        var loot = LootDrop.CreateRandom(TestSword, "Orc");

        // Assert
        loot.HasItems.Should().BeTrue();
        loot.WasClassAppropriate.Should().BeFalse();
        loot.PlayerArchetypeId.Should().BeNull();
        loot.BiasRoll.Should().Be(-1);
        loot.HasPlayerContext.Should().BeFalse();
        loot.SelectionReason.Should().Contain("no player context");
        loot.SourceMonster.Should().Be("Orc");
    }

    #endregion

    #region HasPlayerContext Tests

    /// <summary>
    /// Verifies that HasPlayerContext is true when PlayerArchetypeId is set.
    /// </summary>
    [Test]
    public void HasPlayerContext_WhenArchetypeSet_ReturnsTrue()
    {
        // Arrange
        var result = SmartLootResult.CreateClassAppropriate(
            TestSword,
            biasRoll: 50,
            filteredPoolSize: 5,
            totalPoolSize: 10);

        // Act
        var loot = LootDrop.CreateFrom(result, "stalker");

        // Assert
        loot.HasPlayerContext.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that HasPlayerContext is false when PlayerArchetypeId is null.
    /// </summary>
    [Test]
    public void HasPlayerContext_WhenNoArchetype_ReturnsFalse()
    {
        // Arrange & Act
        var loot = LootDrop.CreateRandom(TestShield);

        // Assert
        loot.HasPlayerContext.Should().BeFalse();
    }

    #endregion

    #region Empty Property Tests

    /// <summary>
    /// Verifies that LootDrop.Empty has appropriate smart loot defaults.
    /// </summary>
    [Test]
    public void Empty_HasSmartLootDefaults()
    {
        // Arrange & Act
        var empty = LootDrop.Empty;

        // Assert
        empty.WasClassAppropriate.Should().BeFalse();
        empty.PlayerArchetypeId.Should().BeNull();
        empty.BiasRoll.Should().Be(-1);
        empty.SelectionReason.Should().BeEmpty();
        empty.HasPlayerContext.Should().BeFalse();
    }

    #endregion
}
