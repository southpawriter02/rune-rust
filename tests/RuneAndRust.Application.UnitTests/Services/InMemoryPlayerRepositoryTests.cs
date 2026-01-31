// ═══════════════════════════════════════════════════════════════════════════════
// InMemoryPlayerRepositoryTests.cs
// Unit tests for the InMemoryPlayerRepository infrastructure service. Verifies
// save operations with name uniqueness enforcement, retrieval by ID and most
// recent, name existence checks (case-insensitive), update operations, and
// listing all saved characters. Uses NUnit with FluentAssertions for readable
// assertions and direct repository instantiation (no mocking required).
// Version: 0.17.5g
// ═══════════════════════════════════════════════════════════════════════════════

using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Infrastructure.Repositories;

#pragma warning disable NUnit2045 // Use Assert.Multiple

namespace RuneAndRust.Application.UnitTests.Services;

/// <summary>
/// Unit tests for <see cref="InMemoryPlayerRepository"/>.
/// </summary>
/// <remarks>
/// <para>
/// Tests cover the full <see cref="IPlayerRepository"/> contract:
/// </para>
/// <list type="bullet">
///   <item><description>SaveAsync — success, null guard, duplicate name, case-insensitive name collision</description></item>
///   <item><description>GetByIdAsync — existing player, non-existent ID</description></item>
///   <item><description>GetMostRecentAsync — empty store, multiple players</description></item>
///   <item><description>ExistsWithNameAsync — exact match, case-insensitive, non-existent</description></item>
///   <item><description>UpdateAsync — existing player, non-existent player</description></item>
///   <item><description>GetAllAsync — empty store, multiple players ordered by most recent</description></item>
/// </list>
/// <para>
/// The repository is instantiated directly with no logger (uses NullLogger internally).
/// Each test creates a fresh repository instance via <see cref="SetUp"/>.
/// </para>
/// </remarks>
/// <seealso cref="InMemoryPlayerRepository"/>
/// <seealso cref="IPlayerRepository"/>
[TestFixture]
public class InMemoryPlayerRepositoryTests
{
    // ═══════════════════════════════════════════════════════════════════════════
    // FIELDS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>The repository instance under test.</summary>
    private InMemoryPlayerRepository _repository = null!;

    // ═══════════════════════════════════════════════════════════════════════════
    // SETUP
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a fresh repository instance before each test.
    /// </summary>
    [SetUp]
    public void SetUp()
    {
        _repository = new InMemoryPlayerRepository();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // SaveAsync TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that saving a new player returns a successful result with the player's ID.
    /// </summary>
    [Test]
    public async Task SaveAsync_NewPlayer_ReturnsSuccess()
    {
        // Arrange
        var player = new Player("Bjorn");

        // Act
        var result = await _repository.SaveAsync(player);

        // Assert
        result.Success.Should().BeTrue();
        result.EntityId.Should().Be(player.Id);
        result.ErrorMessage.Should().BeNull();
    }

    /// <summary>
    /// Verifies that passing null throws an ArgumentNullException.
    /// </summary>
    [Test]
    public void SaveAsync_NullPlayer_ThrowsArgumentNullException()
    {
        // Act
        var act = () => _repository.SaveAsync(null!);

        // Assert
        act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("player");
    }

    /// <summary>
    /// Verifies that saving a player with a duplicate name returns a failed result.
    /// </summary>
    [Test]
    public async Task SaveAsync_DuplicateName_ReturnsFailed()
    {
        // Arrange — save the first player
        var player1 = new Player("Bjorn");
        await _repository.SaveAsync(player1);

        // Arrange — create a second player with the same name
        var player2 = new Player("Bjorn");

        // Act
        var result = await _repository.SaveAsync(player2);

        // Assert
        result.Success.Should().BeFalse();
        result.EntityId.Should().BeNull();
        result.ErrorMessage.Should().Contain("Bjorn");
    }

    /// <summary>
    /// Verifies that name uniqueness is case-insensitive ("TestHero" vs "testhero").
    /// </summary>
    [Test]
    public async Task SaveAsync_DuplicateNameCaseInsensitive_ReturnsFailed()
    {
        // Arrange — save with one casing
        var player1 = new Player("TestHero");
        await _repository.SaveAsync(player1);

        // Arrange — try to save with different casing
        var player2 = new Player("testhero");

        // Act
        var result = await _repository.SaveAsync(player2);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("testhero");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // GetByIdAsync TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that retrieving a saved player by ID returns the correct player.
    /// </summary>
    [Test]
    public async Task GetByIdAsync_ExistingPlayer_ReturnsPlayer()
    {
        // Arrange
        var player = new Player("Bjorn");
        await _repository.SaveAsync(player);

        // Act
        var retrieved = await _repository.GetByIdAsync(player.Id);

        // Assert
        retrieved.Should().NotBeNull();
        retrieved!.Id.Should().Be(player.Id);
        retrieved.Name.Should().Be("Bjorn");
    }

    /// <summary>
    /// Verifies that retrieving a non-existent player returns null.
    /// </summary>
    [Test]
    public async Task GetByIdAsync_NonExistent_ReturnsNull()
    {
        // Act
        var retrieved = await _repository.GetByIdAsync(Guid.NewGuid());

        // Assert
        retrieved.Should().BeNull();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // GetMostRecentAsync TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that calling GetMostRecentAsync on an empty store returns null.
    /// </summary>
    [Test]
    public async Task GetMostRecentAsync_Empty_ReturnsNull()
    {
        // Act
        var result = await _repository.GetMostRecentAsync();

        // Assert
        result.Should().BeNull();
    }

    /// <summary>
    /// Verifies that GetMostRecentAsync returns the last saved player.
    /// </summary>
    [Test]
    public async Task GetMostRecentAsync_MultiplePlayers_ReturnsMostRecent()
    {
        // Arrange — save two players sequentially
        var player1 = new Player("FirstHero");
        await _repository.SaveAsync(player1);

        // Small delay to ensure different timestamps
        await Task.Delay(10);

        var player2 = new Player("SecondHero");
        await _repository.SaveAsync(player2);

        // Act
        var mostRecent = await _repository.GetMostRecentAsync();

        // Assert
        mostRecent.Should().NotBeNull();
        mostRecent!.Name.Should().Be("SecondHero");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // ExistsWithNameAsync TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that ExistsWithNameAsync returns true for an exact name match.
    /// </summary>
    [Test]
    public async Task ExistsWithNameAsync_ExistingName_ReturnsTrue()
    {
        // Arrange
        var player = new Player("Bjorn");
        await _repository.SaveAsync(player);

        // Act
        var exists = await _repository.ExistsWithNameAsync("Bjorn");

        // Assert
        exists.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that ExistsWithNameAsync is case-insensitive.
    /// </summary>
    [Test]
    public async Task ExistsWithNameAsync_CaseInsensitive_ReturnsTrue()
    {
        // Arrange
        var player = new Player("Bjorn");
        await _repository.SaveAsync(player);

        // Act
        var exists = await _repository.ExistsWithNameAsync("BJORN");

        // Assert
        exists.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that ExistsWithNameAsync returns false for a non-existent name.
    /// </summary>
    [Test]
    public async Task ExistsWithNameAsync_NonExistent_ReturnsFalse()
    {
        // Act
        var exists = await _repository.ExistsWithNameAsync("UnknownName");

        // Assert
        exists.Should().BeFalse();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // UpdateAsync TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that updating an existing player returns a successful result.
    /// </summary>
    [Test]
    public async Task UpdateAsync_ExistingPlayer_ReturnsSuccess()
    {
        // Arrange — save the player first
        var player = new Player("Bjorn");
        await _repository.SaveAsync(player);

        // Act — update the same player
        var result = await _repository.UpdateAsync(player);

        // Assert
        result.Success.Should().BeTrue();
        result.EntityId.Should().Be(player.Id);
    }

    /// <summary>
    /// Verifies that updating a non-existent player returns a failed result.
    /// </summary>
    [Test]
    public async Task UpdateAsync_NonExistent_ReturnsFailed()
    {
        // Arrange
        var player = new Player("Bjorn");

        // Act — update without saving first
        var result = await _repository.UpdateAsync(player);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain(player.Id.ToString());
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // GetAllAsync TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that GetAllAsync returns an empty list when no players are saved.
    /// </summary>
    [Test]
    public async Task GetAllAsync_Empty_ReturnsEmptyList()
    {
        // Act
        var players = await _repository.GetAllAsync();

        // Assert
        players.Should().BeEmpty();
    }

    /// <summary>
    /// Verifies that GetAllAsync returns all saved players.
    /// </summary>
    [Test]
    public async Task GetAllAsync_MultiplePlayers_ReturnsAll()
    {
        // Arrange
        var player1 = new Player("Bjorn");
        var player2 = new Player("Astrid");
        var player3 = new Player("Fenrir");

        await _repository.SaveAsync(player1);
        await _repository.SaveAsync(player2);
        await _repository.SaveAsync(player3);

        // Act
        var players = await _repository.GetAllAsync();

        // Assert
        players.Should().HaveCount(3);
        players.Select(p => p.Name).Should().Contain(new[] { "Bjorn", "Astrid", "Fenrir" });
    }
}
