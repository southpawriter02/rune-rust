using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;
using Xunit;

namespace RuneAndRust.Tests.Integration.PostgreSQL;

/// <summary>
/// Tests for PostgreSQL transaction handling including commits, rollbacks,
/// and isolation behavior.
/// </summary>
[Collection("PostgreSQL")]
[Trait("Category", "PostgreSQL")]
public class TransactionTests
{
    private readonly PostgreSqlTestFixture _fixture;

    public TransactionTests(PostgreSqlTestFixture fixture)
    {
        _fixture = fixture;
    }

    #region Transaction Commit Tests

    [Fact]
    public async Task Transaction_Commit_PersistsAllChanges()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        var character = new Character
        {
            Name = $"TxCommit_{Guid.NewGuid():N}",
            Lineage = LineageType.Human,
            Archetype = ArchetypeType.Warrior
        };

        var item = new Item
        {
            Name = "Transaction Item",
            ItemType = ItemType.Material,
            Description = "Created in transaction"
        };

        // Act - Use explicit transaction
        using var transaction = await context.Database.BeginTransactionAsync();

        context.Characters.Add(character);
        context.Items.Add(item);
        await context.SaveChangesAsync();

        await transaction.CommitAsync();

        // Assert - Both should exist
        using var freshContext = _fixture.CreateContext();
        var loadedChar = await freshContext.Characters.FindAsync(character.Id);
        var loadedItem = await freshContext.Items.FindAsync(item.Id);

        loadedChar.Should().NotBeNull();
        loadedItem.Should().NotBeNull();
    }

    #endregion

    #region Transaction Rollback Tests

    [Fact]
    public async Task Transaction_Rollback_DiscardsAllChanges()
    {
        // Arrange
        var characterId = Guid.NewGuid();
        var itemId = Guid.NewGuid();

        // Act
        using (var context = _fixture.CreateContext())
        using (var transaction = await context.Database.BeginTransactionAsync())
        {
            var character = new Character
            {
                Id = characterId,
                Name = $"TxRollback_{Guid.NewGuid():N}",
                Lineage = LineageType.Human,
                Archetype = ArchetypeType.Warrior
            };

            var item = new Item
            {
                Id = itemId,
                Name = "Rollback Item",
                ItemType = ItemType.Material,
                Description = "Will be rolled back"
            };

            context.Characters.Add(character);
            context.Items.Add(item);
            await context.SaveChangesAsync();

            // Explicitly rollback
            await transaction.RollbackAsync();
        }

        // Assert - Neither should exist
        using var freshContext = _fixture.CreateContext();
        var loadedChar = await freshContext.Characters.FindAsync(characterId);
        var loadedItem = await freshContext.Items.FindAsync(itemId);

        loadedChar.Should().BeNull();
        loadedItem.Should().BeNull();
    }

    [Fact]
    public async Task Transaction_Dispose_WithoutCommit_RollsBack()
    {
        // Arrange
        var characterId = Guid.NewGuid();

        // Act - Transaction disposes without commit
        using (var context = _fixture.CreateContext())
        using (var transaction = await context.Database.BeginTransactionAsync())
        {
            var character = new Character
            {
                Id = characterId,
                Name = $"TxDispose_{Guid.NewGuid():N}",
                Lineage = LineageType.Human,
                Archetype = ArchetypeType.Warrior
            };

            context.Characters.Add(character);
            await context.SaveChangesAsync();

            // Transaction disposes without commit - implicit rollback
        }

        // Assert
        using var freshContext = _fixture.CreateContext();
        var loaded = await freshContext.Characters.FindAsync(characterId);
        loaded.Should().BeNull();
    }

    #endregion

    #region Partial Failure Tests

    [Fact]
    public async Task Transaction_PartialFailure_RollsBackAll()
    {
        // Arrange
        var validCharacterId = Guid.NewGuid();

        // First create a character with a specific name that we'll try to duplicate
        var existingName = $"Existing_{Guid.NewGuid():N}";
        using (var setupContext = _fixture.CreateContext())
        {
            var existingChar = new Character
            {
                Name = existingName,
                Lineage = LineageType.Human,
                Archetype = ArchetypeType.Warrior
            };

            setupContext.Characters.Add(existingChar);
            await setupContext.SaveChangesAsync();
        }

        // Act - Try to insert valid char then duplicate (should fail)
        using var freshContext = _fixture.CreateContext();
        using var transaction = await freshContext.Database.BeginTransactionAsync();

        try
        {
            // Valid insert
            var validChar = new Character
            {
                Id = validCharacterId,
                Name = $"Valid_{Guid.NewGuid():N}",
                Lineage = LineageType.IronBlooded,
                Archetype = ArchetypeType.Adept
            };
            freshContext.Characters.Add(validChar);
            await freshContext.SaveChangesAsync();

            // Invalid insert - duplicate name
            var duplicateChar = new Character
            {
                Name = existingName, // Will violate unique constraint
                Lineage = LineageType.Human,
                Archetype = ArchetypeType.Warrior
            };
            freshContext.Characters.Add(duplicateChar);
            await freshContext.SaveChangesAsync(); // This should fail

            await transaction.CommitAsync();
        }
        catch (DbUpdateException)
        {
            await transaction.RollbackAsync();
        }

        // Assert - Valid character should NOT exist due to rollback
        using var verifyContext = _fixture.CreateContext();
        var loaded = await verifyContext.Characters.FindAsync(validCharacterId);
        loaded.Should().BeNull();
    }

    #endregion

    #region Savepoint Tests

    [Fact]
    public async Task Transaction_Savepoint_AllowsPartialRollback()
    {
        // Arrange
        var firstCharId = Guid.NewGuid();
        var secondCharId = Guid.NewGuid();

        using var freshContext = _fixture.CreateContext();
        using var transaction = await freshContext.Database.BeginTransactionAsync();

        // Act
        // First insert - will keep
        var firstChar = new Character
        {
            Id = firstCharId,
            Name = $"Savepoint_First_{Guid.NewGuid():N}",
            Lineage = LineageType.Human,
            Archetype = ArchetypeType.Warrior
        };
        freshContext.Characters.Add(firstChar);
        await freshContext.SaveChangesAsync();

        // Create savepoint
        await transaction.CreateSavepointAsync("AfterFirstInsert");

        // Second insert - will rollback
        var secondChar = new Character
        {
            Id = secondCharId,
            Name = $"Savepoint_Second_{Guid.NewGuid():N}",
            Lineage = LineageType.IronBlooded,
            Archetype = ArchetypeType.Adept
        };
        freshContext.Characters.Add(secondChar);
        await freshContext.SaveChangesAsync();

        // Rollback to savepoint
        await transaction.RollbackToSavepointAsync("AfterFirstInsert");

        // Commit transaction
        await transaction.CommitAsync();

        // Assert
        using var verifyContext = _fixture.CreateContext();
        var first = await verifyContext.Characters.FindAsync(firstCharId);
        var second = await verifyContext.Characters.FindAsync(secondCharId);

        first.Should().NotBeNull(); // Kept
        second.Should().BeNull();   // Rolled back
    }

    #endregion

    #region Concurrent Transaction Tests

    [Fact]
    public async Task Transaction_ConcurrentReads_SeeCommittedData()
    {
        // Arrange
        Guid characterId;
        using (var setupContext = _fixture.CreateContext())
        {
            var character = new Character
            {
                Name = $"ConcurrentRead_{Guid.NewGuid():N}",
                Lineage = LineageType.Human,
                Archetype = ArchetypeType.Warrior,
                CurrentHP = 50
            };

            setupContext.Characters.Add(character);
            await setupContext.SaveChangesAsync();
            characterId = character.Id;
        }

        // Act - Start transaction and modify
        using var context1 = _fixture.CreateContext();
        using var context2 = _fixture.CreateContext();

        using var transaction1 = await context1.Database.BeginTransactionAsync();

        var charInTx = await context1.Characters.FindAsync(characterId);
        charInTx!.CurrentHP = 25;
        await context1.SaveChangesAsync();

        // Before commit, context2 should see old value (read committed isolation)
        var charBeforeCommit = await context2.Characters.FindAsync(characterId);
        charBeforeCommit!.CurrentHP.Should().Be(50);

        await transaction1.CommitAsync();

        // After commit, fresh query should see new value
        using var context3 = _fixture.CreateContext();
        var charAfterCommit = await context3.Characters.FindAsync(characterId);
        charAfterCommit!.CurrentHP.Should().Be(25);
    }

    #endregion

    #region Large Transaction Tests

    [Fact]
    public async Task Transaction_BulkInsert_HandlesLargeDataset()
    {
        // Arrange
        const int itemCount = 100;
        var itemIds = new List<Guid>();

        using var freshContext = _fixture.CreateContext();
        using var transaction = await freshContext.Database.BeginTransactionAsync();

        // Act - Insert many items in one transaction
        for (int i = 0; i < itemCount; i++)
        {
            var item = new Item
            {
                Name = $"BulkItem_{i}_{Guid.NewGuid():N}",
                ItemType = ItemType.Material,
                Description = $"Bulk test item {i}"
            };
            freshContext.Items.Add(item);
            itemIds.Add(item.Id);
        }

        await freshContext.SaveChangesAsync();
        await transaction.CommitAsync();

        // Assert - All items should exist
        using var verifyContext = _fixture.CreateContext();
        var loadedCount = await verifyContext.Items
            .Where(i => itemIds.Contains(i.Id))
            .CountAsync();

        loadedCount.Should().Be(itemCount);
    }

    #endregion
}
