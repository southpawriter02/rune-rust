using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using RuneAndRust.Core.Entities;
using RuneAndRust.Persistence.Data;
using RuneAndRust.Persistence.Repositories;
using Xunit;

namespace RuneAndRust.Tests.Integration;

/// <summary>
/// Integration tests for the persistence layer using InMemory database.
/// </summary>
public class PersistenceTests
{
    private RuneAndRustDbContext GetContext()
    {
        var options = new DbContextOptionsBuilder<RuneAndRustDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new RuneAndRustDbContext(options);
    }

    private SaveGameRepository GetSaveGameRepository(RuneAndRustDbContext context)
    {
        var genericLogger = Mock.Of<ILogger<GenericRepository<SaveGame>>>();
        var saveGameLogger = Mock.Of<ILogger<SaveGameRepository>>();
        return new SaveGameRepository(context, genericLogger, saveGameLogger);
    }

    [Fact]
    public async Task AddAsync_ShouldPersistSaveGame()
    {
        // Arrange
        var context = GetContext();
        var repo = GetSaveGameRepository(context);
        var save = new SaveGame { SlotNumber = 1, CharacterName = "TestChar" };

        // Act
        await repo.AddAsync(save);
        await repo.SaveChangesAsync();

        // Assert
        var result = await context.SaveGames.FirstOrDefaultAsync(s => s.SlotNumber == 1);
        result.Should().NotBeNull();
        result!.CharacterName.Should().Be("TestChar");
    }

    [Fact]
    public async Task GetByIdAsync_ExistingSaveGame_ReturnsSaveGame()
    {
        // Arrange
        var context = GetContext();
        var repo = GetSaveGameRepository(context);
        var save = new SaveGame { SlotNumber = 1, CharacterName = "FindMe" };
        await repo.AddAsync(save);
        await repo.SaveChangesAsync();

        // Act
        var result = await repo.GetByIdAsync(save.Id);

        // Assert
        result.Should().NotBeNull();
        result!.CharacterName.Should().Be("FindMe");
    }

    [Fact]
    public async Task GetByIdAsync_NonExistentId_ReturnsNull()
    {
        // Arrange
        var context = GetContext();
        var repo = GetSaveGameRepository(context);

        // Act
        var result = await repo.GetByIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllAsync_MultipleSaveGames_ReturnsAll()
    {
        // Arrange
        var context = GetContext();
        var repo = GetSaveGameRepository(context);

        await repo.AddAsync(new SaveGame { SlotNumber = 1, CharacterName = "Char1" });
        await repo.AddAsync(new SaveGame { SlotNumber = 2, CharacterName = "Char2" });
        await repo.AddAsync(new SaveGame { SlotNumber = 3, CharacterName = "Char3" });
        await repo.SaveChangesAsync();

        // Act
        var results = await repo.GetAllAsync();

        // Assert
        results.Should().HaveCount(3);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateExistingSaveGame()
    {
        // Arrange
        var context = GetContext();
        var repo = GetSaveGameRepository(context);
        var save = new SaveGame { SlotNumber = 1, CharacterName = "OriginalName" };
        await repo.AddAsync(save);
        await repo.SaveChangesAsync();

        // Act
        save.CharacterName = "UpdatedName";
        await repo.UpdateAsync(save);
        await repo.SaveChangesAsync();

        // Assert
        var result = await context.SaveGames.FirstOrDefaultAsync(s => s.SlotNumber == 1);
        result!.CharacterName.Should().Be("UpdatedName");
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveSaveGame()
    {
        // Arrange
        var context = GetContext();
        var repo = GetSaveGameRepository(context);
        var save = new SaveGame { SlotNumber = 1, CharacterName = "DeleteMe" };
        await repo.AddAsync(save);
        await repo.SaveChangesAsync();

        // Act
        await repo.DeleteAsync(save.Id);
        await repo.SaveChangesAsync();

        // Assert
        var result = await context.SaveGames.FirstOrDefaultAsync(s => s.SlotNumber == 1);
        result.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_NonExistentId_DoesNotThrow()
    {
        // Arrange
        var context = GetContext();
        var repo = GetSaveGameRepository(context);

        // Act
        var act = async () =>
        {
            await repo.DeleteAsync(Guid.NewGuid());
            await repo.SaveChangesAsync();
        };

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task GetBySlotAsync_ExistingSlot_ReturnsSaveGame()
    {
        // Arrange
        var context = GetContext();
        var repo = GetSaveGameRepository(context);
        await repo.AddAsync(new SaveGame { SlotNumber = 2, CharacterName = "SlotChar" });
        await repo.SaveChangesAsync();

        // Act
        var result = await repo.GetBySlotAsync(2);

        // Assert
        result.Should().NotBeNull();
        result!.CharacterName.Should().Be("SlotChar");
    }

    [Fact]
    public async Task GetBySlotAsync_NonExistentSlot_ReturnsNull()
    {
        // Arrange
        var context = GetContext();
        var repo = GetSaveGameRepository(context);

        // Act
        var result = await repo.GetBySlotAsync(99);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task SlotExistsAsync_ExistingSlot_ReturnsTrue()
    {
        // Arrange
        var context = GetContext();
        var repo = GetSaveGameRepository(context);
        await repo.AddAsync(new SaveGame { SlotNumber = 1, CharacterName = "ExistingChar" });
        await repo.SaveChangesAsync();

        // Act
        var exists = await repo.SlotExistsAsync(1);

        // Assert
        exists.Should().BeTrue();
    }

    [Fact]
    public async Task SlotExistsAsync_NonExistentSlot_ReturnsFalse()
    {
        // Arrange
        var context = GetContext();
        var repo = GetSaveGameRepository(context);

        // Act
        var exists = await repo.SlotExistsAsync(99);

        // Assert
        exists.Should().BeFalse();
    }

    [Fact]
    public async Task GetAllOrderedByLastPlayedAsync_ReturnsOrderedByMostRecent()
    {
        // Arrange
        var context = GetContext();
        var repo = GetSaveGameRepository(context);

        var oldest = new SaveGame
        {
            SlotNumber = 1,
            CharacterName = "Oldest",
            LastPlayed = DateTime.UtcNow.AddDays(-3)
        };
        var middle = new SaveGame
        {
            SlotNumber = 2,
            CharacterName = "Middle",
            LastPlayed = DateTime.UtcNow.AddDays(-2)
        };
        var newest = new SaveGame
        {
            SlotNumber = 3,
            CharacterName = "Newest",
            LastPlayed = DateTime.UtcNow.AddDays(-1)
        };

        await repo.AddAsync(oldest);
        await repo.AddAsync(middle);
        await repo.AddAsync(newest);
        await repo.SaveChangesAsync();

        // Act
        var results = (await repo.GetAllOrderedByLastPlayedAsync()).ToList();

        // Assert
        results.Should().HaveCount(3);
        results[0].CharacterName.Should().Be("Newest");
        results[1].CharacterName.Should().Be("Middle");
        results[2].CharacterName.Should().Be("Oldest");
    }

    [Fact]
    public async Task SaveGame_SerializedState_CanStoreLargeJson()
    {
        // Arrange
        var context = GetContext();
        var repo = GetSaveGameRepository(context);
        var largeJson = new string('x', 10000); // 10KB of data
        var save = new SaveGame
        {
            SlotNumber = 1,
            CharacterName = "LargeData",
            SerializedState = $"{{\"data\":\"{largeJson}\"}}"
        };

        // Act
        await repo.AddAsync(save);
        await repo.SaveChangesAsync();

        // Assert
        var result = await repo.GetBySlotAsync(1);
        result!.SerializedState.Length.Should().BeGreaterThan(10000);
    }

    [Fact]
    public void SaveGame_DefaultValues_AreSetCorrectly()
    {
        // Arrange
        var save = new SaveGame();

        // Assert
        save.Id.Should().NotBeEmpty();
        save.SlotNumber.Should().Be(0);
        save.CharacterName.Should().BeEmpty();
        save.SerializedState.Should().Be("{}");
        save.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        save.LastPlayed.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task SaveGame_Timestamps_ArePreservedOnSave()
    {
        // Arrange
        var context = GetContext();
        var repo = GetSaveGameRepository(context);
        var specificTime = new DateTime(2024, 6, 15, 12, 0, 0, DateTimeKind.Utc);
        var save = new SaveGame
        {
            SlotNumber = 1,
            CharacterName = "TimeTest",
            CreatedAt = specificTime,
            LastPlayed = specificTime
        };

        // Act
        await repo.AddAsync(save);
        await repo.SaveChangesAsync();

        // Assert
        var result = await repo.GetBySlotAsync(1);
        result!.CreatedAt.Should().Be(specificTime);
        result.LastPlayed.Should().Be(specificTime);
    }
}
