using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;
using RuneAndRust.Persistence.Data;
using RuneAndRust.Persistence.Repositories;
using Xunit;

namespace RuneAndRust.Tests.Integration;

/// <summary>
/// Integration tests for Character entity persistence.
/// Uses InMemory database to test CharacterRepository operations.
/// </summary>
public class CharacterPersistenceTests : IDisposable
{
    private readonly RuneAndRustDbContext _context;
    private readonly CharacterRepository _repository;

    public CharacterPersistenceTests()
    {
        var options = new DbContextOptionsBuilder<RuneAndRustDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new RuneAndRustDbContext(options);

        var genericLoggerMock = new Mock<ILogger<GenericRepository<Character>>>();
        var characterLoggerMock = new Mock<ILogger<CharacterRepository>>();

        _repository = new CharacterRepository(_context, genericLoggerMock.Object, characterLoggerMock.Object);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    #region AddAsync Tests

    [Fact]
    public async Task AddAsync_ShouldPersistCharacter()
    {
        // Arrange
        var character = new Character
        {
            Name = "Test Hero",
            Lineage = LineageType.Human,
            Archetype = ArchetypeType.Warrior
        };

        // Act
        await _repository.AddAsync(character);
        await _repository.SaveChangesAsync();

        // Assert
        var retrieved = await _repository.GetByIdAsync(character.Id);
        retrieved.Should().NotBeNull();
        retrieved!.Name.Should().Be("Test Hero");
    }

    [Fact]
    public async Task AddAsync_ShouldPersistLineage()
    {
        // Arrange
        var character = new Character
        {
            Name = "Rune Bearer",
            Lineage = LineageType.RuneMarked
        };

        // Act
        await _repository.AddAsync(character);
        await _repository.SaveChangesAsync();

        // Assert
        var retrieved = await _repository.GetByIdAsync(character.Id);
        retrieved!.Lineage.Should().Be(LineageType.RuneMarked);
    }

    [Fact]
    public async Task AddAsync_ShouldPersistArchetype()
    {
        // Arrange
        var character = new Character
        {
            Name = "Shadow",
            Archetype = ArchetypeType.Skirmisher
        };

        // Act
        await _repository.AddAsync(character);
        await _repository.SaveChangesAsync();

        // Assert
        var retrieved = await _repository.GetByIdAsync(character.Id);
        retrieved!.Archetype.Should().Be(ArchetypeType.Skirmisher);
    }

    [Fact]
    public async Task AddAsync_ShouldPersistAttributes()
    {
        // Arrange
        var character = new Character
        {
            Name = "Stat Test",
            Sturdiness = 8,
            Might = 7,
            Wits = 6,
            Will = 5,
            Finesse = 4
        };

        // Act
        await _repository.AddAsync(character);
        await _repository.SaveChangesAsync();

        // Assert
        var retrieved = await _repository.GetByIdAsync(character.Id);
        retrieved!.Sturdiness.Should().Be(8);
        retrieved.Might.Should().Be(7);
        retrieved.Wits.Should().Be(6);
        retrieved.Will.Should().Be(5);
        retrieved.Finesse.Should().Be(4);
    }

    [Fact]
    public async Task AddAsync_ShouldPersistDerivedStats()
    {
        // Arrange
        var character = new Character
        {
            Name = "Derived Test",
            MaxHP = 150,
            CurrentHP = 120,
            MaxStamina = 80,
            CurrentStamina = 60,
            ActionPoints = 4
        };

        // Act
        await _repository.AddAsync(character);
        await _repository.SaveChangesAsync();

        // Assert
        var retrieved = await _repository.GetByIdAsync(character.Id);
        retrieved!.MaxHP.Should().Be(150);
        retrieved.CurrentHP.Should().Be(120);
        retrieved.MaxStamina.Should().Be(80);
        retrieved.CurrentStamina.Should().Be(60);
        retrieved.ActionPoints.Should().Be(4);
    }

    [Fact]
    public async Task AddAsync_ShouldPersistProgression()
    {
        // Arrange
        var character = new Character
        {
            Name = "Veteran",
            Level = 5,
            Legend = 2500
        };

        // Act
        await _repository.AddAsync(character);
        await _repository.SaveChangesAsync();

        // Assert
        var retrieved = await _repository.GetByIdAsync(character.Id);
        retrieved!.Level.Should().Be(5);
        retrieved.Legend.Should().Be(2500);
    }

    #endregion

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_ExistingCharacter_ReturnsCharacter()
    {
        // Arrange
        var character = new Character { Name = "Find Me" };
        await _repository.AddAsync(character);
        await _repository.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(character.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Find Me");
    }

    [Fact]
    public async Task GetByIdAsync_NonExistentId_ReturnsNull()
    {
        // Act
        var result = await _repository.GetByIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetByNameAsync Tests

    [Fact]
    public async Task GetByNameAsync_ExistingName_ReturnsCharacter()
    {
        // Arrange
        await _repository.AddAsync(new Character { Name = "Unique Name" });
        await _repository.SaveChangesAsync();

        // Act
        var result = await _repository.GetByNameAsync("Unique Name");

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Unique Name");
    }

    [Fact]
    public async Task GetByNameAsync_NonExistentName_ReturnsNull()
    {
        // Act
        var result = await _repository.GetByNameAsync("Does Not Exist");

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region NameExistsAsync Tests

    [Fact]
    public async Task NameExistsAsync_ExistingName_ReturnsTrue()
    {
        // Arrange
        await _repository.AddAsync(new Character { Name = "Taken Name" });
        await _repository.SaveChangesAsync();

        // Act
        var result = await _repository.NameExistsAsync("Taken Name");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task NameExistsAsync_NonExistentName_ReturnsFalse()
    {
        // Act
        var result = await _repository.NameExistsAsync("Available Name");

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region GetAllOrderedByCreationAsync Tests

    [Fact]
    public async Task GetAllOrderedByCreationAsync_MultipleCharacters_ReturnsOrderedByCreationDesc()
    {
        // Arrange
        var char1 = new Character { Name = "First", CreatedAt = DateTime.UtcNow.AddDays(-2) };
        var char2 = new Character { Name = "Second", CreatedAt = DateTime.UtcNow.AddDays(-1) };
        var char3 = new Character { Name = "Third", CreatedAt = DateTime.UtcNow };

        await _repository.AddAsync(char1);
        await _repository.AddAsync(char2);
        await _repository.AddAsync(char3);
        await _repository.SaveChangesAsync();

        // Act
        var result = (await _repository.GetAllOrderedByCreationAsync()).ToList();

        // Assert
        result.Should().HaveCount(3);
        result[0].Name.Should().Be("Third");
        result[1].Name.Should().Be("Second");
        result[2].Name.Should().Be("First");
    }

    [Fact]
    public async Task GetAllOrderedByCreationAsync_EmptyDatabase_ReturnsEmpty()
    {
        // Act
        var result = await _repository.GetAllOrderedByCreationAsync();

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region GetMostRecentAsync Tests

    [Fact]
    public async Task GetMostRecentAsync_WithCharacters_ReturnsMostRecentlyModified()
    {
        // Arrange
        var char1 = new Character { Name = "Old", LastModified = DateTime.UtcNow.AddHours(-2) };
        var char2 = new Character { Name = "Newest", LastModified = DateTime.UtcNow };
        var char3 = new Character { Name = "Middle", LastModified = DateTime.UtcNow.AddHours(-1) };

        await _repository.AddAsync(char1);
        await _repository.AddAsync(char2);
        await _repository.AddAsync(char3);
        await _repository.SaveChangesAsync();

        // Act
        var result = await _repository.GetMostRecentAsync();

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Newest");
    }

    [Fact]
    public async Task GetMostRecentAsync_NoCharacters_ReturnsNull()
    {
        // Act
        var result = await _repository.GetMostRecentAsync();

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region UpdateAsync Tests

    [Fact]
    public async Task UpdateAsync_UpdatesCharacterProperties()
    {
        // Arrange
        var character = new Character { Name = "Original" };
        await _repository.AddAsync(character);
        await _repository.SaveChangesAsync();

        // Act
        character.Name = "Updated";
        character.Level = 10;
        await _repository.UpdateAsync(character);
        await _repository.SaveChangesAsync();

        // Assert
        var retrieved = await _repository.GetByIdAsync(character.Id);
        retrieved!.Name.Should().Be("Updated");
        retrieved.Level.Should().Be(10);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesAttributes()
    {
        // Arrange
        var character = new Character { Name = "Leveling Up" };
        await _repository.AddAsync(character);
        await _repository.SaveChangesAsync();

        // Act
        character.Sturdiness = 10;
        character.MaxHP = 150;
        await _repository.UpdateAsync(character);
        await _repository.SaveChangesAsync();

        // Assert
        var retrieved = await _repository.GetByIdAsync(character.Id);
        retrieved!.Sturdiness.Should().Be(10);
        retrieved.MaxHP.Should().Be(150);
    }

    #endregion

    #region DeleteAsync Tests

    [Fact]
    public async Task DeleteAsync_RemovesCharacter()
    {
        // Arrange
        var character = new Character { Name = "ToDelete" };
        await _repository.AddAsync(character);
        await _repository.SaveChangesAsync();

        // Act
        await _repository.DeleteAsync(character.Id);
        await _repository.SaveChangesAsync();

        // Assert
        var result = await _repository.GetByIdAsync(character.Id);
        result.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_NonExistentId_DoesNotThrow()
    {
        // Act
        var action = async () =>
        {
            await _repository.DeleteAsync(Guid.NewGuid());
            await _repository.SaveChangesAsync();
        };

        // Assert
        await action.Should().NotThrowAsync();
    }

    #endregion

    #region Character Entity Defaults Tests

    [Fact]
    public void Character_DefaultValues_AreSetCorrectly()
    {
        // Arrange & Act
        var character = new Character();

        // Assert
        character.Id.Should().NotBe(Guid.Empty);
        character.Name.Should().BeEmpty();
        character.Lineage.Should().Be(LineageType.Human);
        character.Archetype.Should().Be(ArchetypeType.Warrior);
        character.Sturdiness.Should().Be(5);
        character.Might.Should().Be(5);
        character.Wits.Should().Be(5);
        character.Will.Should().Be(5);
        character.Finesse.Should().Be(5);
        character.Level.Should().Be(1);
        character.Legend.Should().Be(0);
    }

    #endregion
}
