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
/// Integration tests for CodexEntry and DataCapture entity persistence.
/// Uses InMemory database to test repository operations with JSONB and relationships.
/// </summary>
public class CodexPersistenceTests : IDisposable
{
    private readonly RuneAndRustDbContext _context;
    private readonly CodexEntryRepository _codexRepository;
    private readonly DataCaptureRepository _captureRepository;

    public CodexPersistenceTests()
    {
        var options = new DbContextOptionsBuilder<RuneAndRustDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new RuneAndRustDbContext(options);

        var genericCodexLoggerMock = new Mock<ILogger<GenericRepository<CodexEntry>>>();
        var codexLoggerMock = new Mock<ILogger<CodexEntryRepository>>();
        _codexRepository = new CodexEntryRepository(_context, genericCodexLoggerMock.Object, codexLoggerMock.Object);

        var genericCaptureLoggerMock = new Mock<ILogger<GenericRepository<DataCapture>>>();
        var captureLoggerMock = new Mock<ILogger<DataCaptureRepository>>();
        _captureRepository = new DataCaptureRepository(_context, genericCaptureLoggerMock.Object, captureLoggerMock.Object);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    #region CodexEntry CRUD Tests

    [Fact]
    public async Task AddAsync_ShouldPersistCodexEntry()
    {
        // Arrange
        var entry = CreateTestCodexEntry("Test Entry");

        // Act
        await _codexRepository.AddAsync(entry);
        await _codexRepository.SaveChangesAsync();

        // Assert
        var retrieved = await _codexRepository.GetByIdAsync(entry.Id);
        retrieved.Should().NotBeNull();
        retrieved!.Title.Should().Be("Test Entry");
    }

    [Fact]
    public async Task AddAsync_ShouldPersistAllCodexProperties()
    {
        // Arrange
        var entry = new CodexEntry
        {
            Title = "Rusted Servitor",
            Category = EntryCategory.Bestiary,
            FullText = "A humanoid automaton of ancient Aesir design, corrupted by centuries of Blight exposure.",
            TotalFragments = 4,
            UnlockThresholds = new Dictionary<int, string>
            {
                { 25, "WEAKNESS_REVEALED" },
                { 50, "HABITAT_REVEALED" },
                { 75, "BEHAVIOR_REVEALED" },
                { 100, "FULL_ENTRY" }
            }
        };

        // Act
        await _codexRepository.AddAsync(entry);
        await _codexRepository.SaveChangesAsync();

        // Assert
        var retrieved = await _codexRepository.GetByIdAsync(entry.Id);
        retrieved.Should().NotBeNull();
        retrieved!.Title.Should().Be("Rusted Servitor");
        retrieved.Category.Should().Be(EntryCategory.Bestiary);
        retrieved.FullText.Should().Contain("automaton");
        retrieved.TotalFragments.Should().Be(4);
    }

    [Fact]
    public async Task UpdateAsync_ShouldModifyCodexEntry()
    {
        // Arrange
        var entry = CreateTestCodexEntry("Original Title");
        await _codexRepository.AddAsync(entry);
        await _codexRepository.SaveChangesAsync();

        // Act
        entry.Title = "Updated Title";
        entry.Category = EntryCategory.Technical;
        await _codexRepository.UpdateAsync(entry);
        await _codexRepository.SaveChangesAsync();

        // Assert
        var retrieved = await _codexRepository.GetByIdAsync(entry.Id);
        retrieved!.Title.Should().Be("Updated Title");
        retrieved.Category.Should().Be(EntryCategory.Technical);
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveCodexEntry()
    {
        // Arrange
        var entry = CreateTestCodexEntry("To Delete");
        await _codexRepository.AddAsync(entry);
        await _codexRepository.SaveChangesAsync();

        // Act
        await _codexRepository.DeleteAsync(entry.Id);
        await _codexRepository.SaveChangesAsync();

        // Assert
        var retrieved = await _codexRepository.GetByIdAsync(entry.Id);
        retrieved.Should().BeNull();
    }

    #endregion

    #region GetByCategoryAsync Tests

    [Fact]
    public async Task GetByCategoryAsync_ReturnsEntriesOfCategory()
    {
        // Arrange
        await _codexRepository.AddAsync(CreateTestCodexEntry("Servitor", EntryCategory.Bestiary));
        await _codexRepository.AddAsync(CreateTestCodexEntry("Blight Wolf", EntryCategory.Bestiary));
        await _codexRepository.AddAsync(CreateTestCodexEntry("Psychic Stress", EntryCategory.FieldGuide));
        await _codexRepository.SaveChangesAsync();

        // Act
        var result = await _codexRepository.GetByCategoryAsync(EntryCategory.Bestiary);

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(e => e.Category == EntryCategory.Bestiary);
    }

    [Fact]
    public async Task GetByCategoryAsync_NoMatches_ReturnsEmptyList()
    {
        // Arrange
        await _codexRepository.AddAsync(CreateTestCodexEntry("Test", EntryCategory.Bestiary));
        await _codexRepository.SaveChangesAsync();

        // Act
        var result = await _codexRepository.GetByCategoryAsync(EntryCategory.Geography);

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region GetByTitleAsync Tests

    [Fact]
    public async Task GetByTitleAsync_FindsExactMatch()
    {
        // Arrange
        await _codexRepository.AddAsync(CreateTestCodexEntry("Rusted Servitor"));
        await _codexRepository.SaveChangesAsync();

        // Act
        var result = await _codexRepository.GetByTitleAsync("Rusted Servitor");

        // Assert
        result.Should().NotBeNull();
        result!.Title.Should().Be("Rusted Servitor");
    }

    [Fact]
    public async Task GetByTitleAsync_CaseInsensitive()
    {
        // Arrange
        await _codexRepository.AddAsync(CreateTestCodexEntry("Rusted Servitor"));
        await _codexRepository.SaveChangesAsync();

        // Act
        var result = await _codexRepository.GetByTitleAsync("rusted servitor");

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetByTitleAsync_NotFound_ReturnsNull()
    {
        // Arrange
        await _codexRepository.AddAsync(CreateTestCodexEntry("Rusted Servitor"));
        await _codexRepository.SaveChangesAsync();

        // Act
        var result = await _codexRepository.GetByTitleAsync("Blight Wolf");

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region UnlockThresholds JSONB Tests

    [Fact]
    public async Task UnlockThresholds_SerializesAsJsonb()
    {
        // Arrange
        var entry = new CodexEntry
        {
            Title = "Test Entry",
            UnlockThresholds = new Dictionary<int, string>
            {
                { 25, "WEAKNESS_REVEALED" },
                { 50, "HABITAT_REVEALED" },
                { 100, "FULL_ENTRY" }
            }
        };

        // Act
        await _codexRepository.AddAsync(entry);
        await _codexRepository.SaveChangesAsync();

        // Clear the context to force a fresh read
        _context.ChangeTracker.Clear();

        // Assert
        var retrieved = await _codexRepository.GetByIdAsync(entry.Id);
        retrieved.Should().NotBeNull();
        retrieved!.UnlockThresholds.Should().HaveCount(3);
        retrieved.UnlockThresholds[25].Should().Be("WEAKNESS_REVEALED");
        retrieved.UnlockThresholds[50].Should().Be("HABITAT_REVEALED");
        retrieved.UnlockThresholds[100].Should().Be("FULL_ENTRY");
    }

    [Fact]
    public async Task UnlockThresholds_EmptyDictionary_PersistsCorrectly()
    {
        // Arrange
        var entry = new CodexEntry
        {
            Title = "Empty Thresholds",
            UnlockThresholds = new Dictionary<int, string>()
        };

        // Act
        await _codexRepository.AddAsync(entry);
        await _codexRepository.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Assert
        var retrieved = await _codexRepository.GetByIdAsync(entry.Id);
        retrieved!.UnlockThresholds.Should().NotBeNull();
        retrieved.UnlockThresholds.Should().BeEmpty();
    }

    #endregion

    #region DataCapture CRUD Tests

    [Fact]
    public async Task AddAsync_ShouldPersistDataCapture()
    {
        // Arrange
        var capture = CreateTestDataCapture("Test Fragment");

        // Act
        await _captureRepository.AddAsync(capture);
        await _captureRepository.SaveChangesAsync();

        // Assert
        var retrieved = await _captureRepository.GetByIdAsync(capture.Id);
        retrieved.Should().NotBeNull();
        retrieved!.FragmentContent.Should().Be("Test Fragment");
    }

    [Fact]
    public async Task AddAsync_ShouldPersistAllCaptureProperties()
    {
        // Arrange
        var characterId = Guid.NewGuid();
        var capture = new DataCapture
        {
            CharacterId = characterId,
            Type = CaptureType.Specimen,
            FragmentContent = "The servo-motor shows signs of organic fungal infiltration...",
            Source = "Found on Rusted Servitor corpse",
            Quality = 30,
            IsAnalyzed = true
        };

        // Act
        await _captureRepository.AddAsync(capture);
        await _captureRepository.SaveChangesAsync();

        // Assert
        var retrieved = await _captureRepository.GetByIdAsync(capture.Id);
        retrieved.Should().NotBeNull();
        retrieved!.CharacterId.Should().Be(characterId);
        retrieved.Type.Should().Be(CaptureType.Specimen);
        retrieved.FragmentContent.Should().Contain("servo-motor");
        retrieved.Source.Should().Contain("Servitor");
        retrieved.Quality.Should().Be(30);
        retrieved.IsAnalyzed.Should().BeTrue();
    }

    #endregion

    #region GetByCharacterIdAsync Tests

    [Fact]
    public async Task GetByCharacterIdAsync_ReturnsCharacterCaptures()
    {
        // Arrange
        var characterId = Guid.NewGuid();
        var otherCharacterId = Guid.NewGuid();

        await _captureRepository.AddAsync(CreateTestDataCapture("Fragment 1", characterId));
        await _captureRepository.AddAsync(CreateTestDataCapture("Fragment 2", characterId));
        await _captureRepository.AddAsync(CreateTestDataCapture("Other Fragment", otherCharacterId));
        await _captureRepository.SaveChangesAsync();

        // Act
        var result = await _captureRepository.GetByCharacterIdAsync(characterId);

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(c => c.CharacterId == characterId);
    }

    [Fact]
    public async Task GetByCharacterIdAsync_NoCaptures_ReturnsEmptyList()
    {
        // Arrange
        var characterId = Guid.NewGuid();

        // Act
        var result = await _captureRepository.GetByCharacterIdAsync(characterId);

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region GetFragmentCountAsync Tests

    [Fact]
    public async Task GetFragmentCountAsync_ReturnsCorrectCount()
    {
        // Arrange
        var characterId = Guid.NewGuid();
        var entry = CreateTestCodexEntry("Servitor");
        await _codexRepository.AddAsync(entry);
        await _codexRepository.SaveChangesAsync();

        await _captureRepository.AddAsync(CreateTestDataCapture("Fragment 1", characterId, entry.Id));
        await _captureRepository.AddAsync(CreateTestDataCapture("Fragment 2", characterId, entry.Id));
        await _captureRepository.AddAsync(CreateTestDataCapture("Fragment 3", characterId, entry.Id));
        await _captureRepository.SaveChangesAsync();

        // Act
        var count = await _captureRepository.GetFragmentCountAsync(entry.Id, characterId);

        // Assert
        count.Should().Be(3);
    }

    [Fact]
    public async Task GetFragmentCountAsync_NoFragments_ReturnsZero()
    {
        // Arrange
        var characterId = Guid.NewGuid();
        var entryId = Guid.NewGuid();

        // Act
        var count = await _captureRepository.GetFragmentCountAsync(entryId, characterId);

        // Assert
        count.Should().Be(0);
    }

    #endregion

    #region GetWithFragmentsAsync Tests

    [Fact]
    public async Task GetWithFragmentsAsync_IncludesRelatedCaptures()
    {
        // Arrange
        var characterId = Guid.NewGuid();
        var entry = CreateTestCodexEntry("Servitor");
        await _codexRepository.AddAsync(entry);
        await _codexRepository.SaveChangesAsync();

        await _captureRepository.AddAsync(CreateTestDataCapture("Fragment 1", characterId, entry.Id));
        await _captureRepository.AddAsync(CreateTestDataCapture("Fragment 2", characterId, entry.Id));
        await _captureRepository.SaveChangesAsync();

        // Act
        var result = await _codexRepository.GetWithFragmentsAsync(entry.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Fragments.Should().HaveCount(2);
    }

    #endregion

    #region GetUnassignedAsync Tests

    [Fact]
    public async Task GetUnassignedAsync_ReturnsUnassignedCaptures()
    {
        // Arrange
        var characterId = Guid.NewGuid();
        var entry = CreateTestCodexEntry("Servitor");
        await _codexRepository.AddAsync(entry);
        await _codexRepository.SaveChangesAsync();

        await _captureRepository.AddAsync(CreateTestDataCapture("Assigned", characterId, entry.Id));
        await _captureRepository.AddAsync(CreateTestDataCapture("Unassigned 1", characterId, null));
        await _captureRepository.AddAsync(CreateTestDataCapture("Unassigned 2", characterId, null));
        await _captureRepository.SaveChangesAsync();

        // Act
        var result = await _captureRepository.GetUnassignedAsync(characterId);

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(c => c.CodexEntryId == null);
    }

    #endregion

    #region AddRangeAsync Tests

    [Fact]
    public async Task AddRangeAsync_AddsMultipleCodexEntries()
    {
        // Arrange
        var entries = new[]
        {
            CreateTestCodexEntry("Entry 1"),
            CreateTestCodexEntry("Entry 2"),
            CreateTestCodexEntry("Entry 3")
        };

        // Act
        await _codexRepository.AddRangeAsync(entries);
        await _codexRepository.SaveChangesAsync();

        // Assert
        var allEntries = await _codexRepository.GetAllAsync();
        allEntries.Should().HaveCount(3);
    }

    [Fact]
    public async Task AddRangeAsync_AddsMultipleDataCaptures()
    {
        // Arrange
        var characterId = Guid.NewGuid();
        var captures = new[]
        {
            CreateTestDataCapture("Fragment 1", characterId),
            CreateTestDataCapture("Fragment 2", characterId),
            CreateTestDataCapture("Fragment 3", characterId)
        };

        // Act
        await _captureRepository.AddRangeAsync(captures);
        await _captureRepository.SaveChangesAsync();

        // Assert
        var result = await _captureRepository.GetByCharacterIdAsync(characterId);
        result.Should().HaveCount(3);
    }

    #endregion

    #region Relationship Tests

    [Fact]
    public async Task DataCapture_LinkedToEntry_NavigationPropertyWorks()
    {
        // Arrange
        var characterId = Guid.NewGuid();
        var entry = CreateTestCodexEntry("Servitor");
        await _codexRepository.AddAsync(entry);
        await _codexRepository.SaveChangesAsync();

        var capture = CreateTestDataCapture("Fragment", characterId, entry.Id);
        await _captureRepository.AddAsync(capture);
        await _captureRepository.SaveChangesAsync();

        // Act
        var captures = await _captureRepository.GetByCharacterIdAsync(characterId);
        var retrievedCapture = captures.First();

        // Assert
        retrievedCapture.CodexEntry.Should().NotBeNull();
        retrievedCapture.CodexEntry!.Title.Should().Be("Servitor");
    }

    #endregion

    #region Helper Methods

    private CodexEntry CreateTestCodexEntry(string title, EntryCategory category = EntryCategory.Bestiary)
    {
        return new CodexEntry
        {
            Title = title,
            Category = category,
            FullText = $"Full text for {title}.",
            TotalFragments = 1
        };
    }

    private DataCapture CreateTestDataCapture(string content, Guid? characterId = null, Guid? entryId = null)
    {
        return new DataCapture
        {
            CharacterId = characterId ?? Guid.NewGuid(),
            CodexEntryId = entryId,
            Type = CaptureType.TextFragment,
            FragmentContent = content,
            Source = "Test Source",
            Quality = 15
        };
    }

    #endregion
}
