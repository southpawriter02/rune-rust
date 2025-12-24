using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Core.ViewModels;
using RuneAndRust.Engine.Services;
using Xunit;

namespace RuneAndRust.Tests.Engine;

/// <summary>
/// Tests for the JournalService.BuildViewModelAsync method (v0.3.7c).
/// Validates ViewModel construction, tab filtering, and detail building.
/// </summary>
public class JournalViewModelTests
{
    private readonly Mock<ILogger<JournalService>> _mockLogger;
    private readonly Mock<IDataCaptureService> _mockCaptureService;
    private readonly Mock<IDataCaptureRepository> _mockCaptureRepository;
    private readonly Mock<ICodexEntryRepository> _mockCodexRepository;
    private readonly Mock<ILibraryService> _mockLibraryService;
    private readonly JournalService _sut;

    private readonly Guid _testCharacterId = Guid.NewGuid();
    private readonly string _testCharacterName = "Test Scholar";

    public JournalViewModelTests()
    {
        _mockLogger = new Mock<ILogger<JournalService>>();
        _mockCaptureService = new Mock<IDataCaptureService>();
        _mockCaptureRepository = new Mock<IDataCaptureRepository>();
        _mockCodexRepository = new Mock<ICodexEntryRepository>();
        _mockLibraryService = new Mock<ILibraryService>();

        // Default: return empty collection for system entries
        _mockLibraryService
            .Setup(x => x.GetEntriesByCategory(It.IsAny<EntryCategory>()))
            .Returns(Enumerable.Empty<CodexEntry>());

        _sut = new JournalService(
            _mockLogger.Object,
            _mockCaptureService.Object,
            _mockCaptureRepository.Object,
            _mockCodexRepository.Object,
            _mockLibraryService.Object);
    }

    #region Tab Filtering Tests

    [Fact]
    public async Task BuildViewModelAsync_FiltersByTab_Codex()
    {
        // Arrange - Create entries of different categories
        var entries = new List<(CodexEntry Entry, int CompletionPercent)>
        {
            (CreateEntry("Blight Origin Entry", EntryCategory.BlightOrigin), 50),
            (CreateEntry("Factions Entry", EntryCategory.Factions), 75),
            (CreateEntry("Technical Entry", EntryCategory.Technical), 100),
            (CreateEntry("Geography Entry", EntryCategory.Geography), 25),
            (CreateEntry("Bestiary Entry", EntryCategory.Bestiary), 80),
            (CreateEntry("Field Guide Entry", EntryCategory.FieldGuide), 60)
        };

        _mockCaptureService.Setup(s => s.GetDiscoveredEntriesAsync(_testCharacterId))
            .ReturnsAsync(entries);

        // Act
        var vm = await _sut.BuildViewModelAsync(_testCharacterId, _testCharacterName, JournalTab.Codex);

        // Assert - Should only include BlightOrigin, Factions, Technical, Geography
        vm.Entries.Should().HaveCount(4);
        vm.Entries.Should().AllSatisfy(e =>
            e.Category.Should().BeOneOf(
                EntryCategory.BlightOrigin,
                EntryCategory.Factions,
                EntryCategory.Technical,
                EntryCategory.Geography));
    }

    [Fact]
    public async Task BuildViewModelAsync_FiltersByTab_Bestiary()
    {
        // Arrange
        var entries = new List<(CodexEntry Entry, int CompletionPercent)>
        {
            (CreateEntry("Blight Origin Entry", EntryCategory.BlightOrigin), 50),
            (CreateEntry("Hrimthursar", EntryCategory.Bestiary), 80),
            (CreateEntry("Iron-Husk", EntryCategory.Bestiary), 45)
        };

        _mockCaptureService.Setup(s => s.GetDiscoveredEntriesAsync(_testCharacterId))
            .ReturnsAsync(entries);

        // Act
        var vm = await _sut.BuildViewModelAsync(_testCharacterId, _testCharacterName, JournalTab.Bestiary);

        // Assert - Should only include Bestiary entries
        vm.Entries.Should().HaveCount(2);
        vm.Entries.Should().AllSatisfy(e => e.Category.Should().Be(EntryCategory.Bestiary));
    }

    [Fact]
    public async Task BuildViewModelAsync_FiltersByTab_FieldGuide()
    {
        // Arrange
        var entries = new List<(CodexEntry Entry, int CompletionPercent)>
        {
            (CreateEntry("Combat Basics", EntryCategory.FieldGuide), 100),
            (CreateEntry("Crafting Tutorial", EntryCategory.FieldGuide), 50),
            (CreateEntry("Random Lore", EntryCategory.BlightOrigin), 30)
        };

        _mockCaptureService.Setup(s => s.GetDiscoveredEntriesAsync(_testCharacterId))
            .ReturnsAsync(entries);

        // Act
        var vm = await _sut.BuildViewModelAsync(_testCharacterId, _testCharacterName, JournalTab.FieldGuide);

        // Assert - Should only include FieldGuide entries
        vm.Entries.Should().HaveCount(2);
        vm.Entries.Should().AllSatisfy(e => e.Category.Should().Be(EntryCategory.FieldGuide));
    }

    [Fact]
    public async Task BuildViewModelAsync_Contracts_ReturnsEmpty()
    {
        // Arrange - No quest system yet, Contracts should always be empty
        var entries = new List<(CodexEntry Entry, int CompletionPercent)>
        {
            (CreateEntry("Bestiary Entry", EntryCategory.Bestiary), 80)
        };

        _mockCaptureService.Setup(s => s.GetDiscoveredEntriesAsync(_testCharacterId))
            .ReturnsAsync(entries);

        // Act
        var vm = await _sut.BuildViewModelAsync(_testCharacterId, _testCharacterName, JournalTab.Contracts);

        // Assert - Should be empty (no EntryCategory maps to Contracts)
        vm.Entries.Should().BeEmpty();
        vm.SelectedDetail.Should().BeNull();
    }

    #endregion

    #region Completion Tests

    [Fact]
    public async Task BuildViewModelAsync_SetsCompletionPercent()
    {
        // Arrange
        var entries = new List<(CodexEntry Entry, int CompletionPercent)>
        {
            (CreateEntry("Entry 1", EntryCategory.Bestiary), 45),
            (CreateEntry("Entry 2", EntryCategory.Bestiary), 100)
        };

        _mockCaptureService.Setup(s => s.GetDiscoveredEntriesAsync(_testCharacterId))
            .ReturnsAsync(entries);

        // Act
        var vm = await _sut.BuildViewModelAsync(_testCharacterId, _testCharacterName, JournalTab.Bestiary);

        // Assert
        vm.Entries.Should().Contain(e => e.CompletionPercent == 45 && !e.IsComplete);
        vm.Entries.Should().Contain(e => e.CompletionPercent == 100 && e.IsComplete);
    }

    [Fact]
    public async Task BuildViewModelAsync_SetsIsComplete_WhenPercentIs100()
    {
        // Arrange
        var entries = new List<(CodexEntry Entry, int CompletionPercent)>
        {
            (CreateEntry("Complete Entry", EntryCategory.Bestiary), 100)
        };

        _mockCaptureService.Setup(s => s.GetDiscoveredEntriesAsync(_testCharacterId))
            .ReturnsAsync(entries);

        // Act
        var vm = await _sut.BuildViewModelAsync(_testCharacterId, _testCharacterName, JournalTab.Bestiary);

        // Assert
        vm.Entries.Should().HaveCount(1);
        vm.Entries[0].IsComplete.Should().BeTrue();
    }

    #endregion

    #region Details Panel Tests

    [Fact]
    public async Task BuildViewModelAsync_BuildsDetailsForSelectedEntry()
    {
        // Arrange
        var entry = CreateEntry("Selected Entry", EntryCategory.Bestiary);
        var entries = new List<(CodexEntry Entry, int CompletionPercent)>
        {
            (entry, 75)
        };

        _mockCaptureService.Setup(s => s.GetDiscoveredEntriesAsync(_testCharacterId))
            .ReturnsAsync(entries);
        _mockCodexRepository.Setup(r => r.GetByIdAsync(entry.Id))
            .ReturnsAsync(entry);
        _mockCaptureService.Setup(s => s.GetCompletionPercentageAsync(entry.Id, _testCharacterId))
            .ReturnsAsync(75);
        _mockCaptureService.Setup(s => s.GetUnlockedThresholdsAsync(entry.Id, _testCharacterId))
            .ReturnsAsync(new[] { "WEAKNESS_REVEALED" });
        _mockCaptureRepository.Setup(r => r.GetFragmentCountAsync(entry.Id, _testCharacterId))
            .ReturnsAsync(9);

        // Act
        var vm = await _sut.BuildViewModelAsync(_testCharacterId, _testCharacterName, JournalTab.Bestiary, selectedIndex: 0);

        // Assert
        vm.SelectedDetail.Should().NotBeNull();
        vm.SelectedDetail!.Title.Should().Be("Selected Entry");
        vm.SelectedDetail.CompletionPercent.Should().Be(75);
        vm.SelectedDetail.FragmentsCollected.Should().Be(9);
        vm.SelectedDetail.UnlockedThresholds.Should().Contain("WEAKNESS_REVEALED");
    }

    [Fact]
    public async Task BuildViewModelAsync_NullDetails_WhenNoEntries()
    {
        // Arrange
        _mockCaptureService.Setup(s => s.GetDiscoveredEntriesAsync(_testCharacterId))
            .ReturnsAsync(new List<(CodexEntry, int)>());

        // Act
        var vm = await _sut.BuildViewModelAsync(_testCharacterId, _testCharacterName, JournalTab.Codex);

        // Assert
        vm.Entries.Should().BeEmpty();
        vm.SelectedDetail.Should().BeNull();
    }

    [Fact]
    public async Task BuildViewModelAsync_NullDetails_WhenIndexOutOfRange()
    {
        // Arrange
        var entries = new List<(CodexEntry Entry, int CompletionPercent)>
        {
            (CreateEntry("Only Entry", EntryCategory.Bestiary), 50)
        };

        _mockCaptureService.Setup(s => s.GetDiscoveredEntriesAsync(_testCharacterId))
            .ReturnsAsync(entries);

        // Act
        var vm = await _sut.BuildViewModelAsync(_testCharacterId, _testCharacterName, JournalTab.Bestiary, selectedIndex: 999);

        // Assert
        vm.SelectedDetail.Should().BeNull();
    }

    [Fact]
    public async Task BuildViewModelAsync_RedactsContent()
    {
        // Arrange
        var entry = CreateEntry("Partial Entry", EntryCategory.Bestiary);
        entry.FullText = "This is a test entry with multiple words that should be partially redacted.";
        var entries = new List<(CodexEntry Entry, int CompletionPercent)>
        {
            (entry, 50)
        };

        _mockCaptureService.Setup(s => s.GetDiscoveredEntriesAsync(_testCharacterId))
            .ReturnsAsync(entries);
        _mockCodexRepository.Setup(r => r.GetByIdAsync(entry.Id))
            .ReturnsAsync(entry);
        _mockCaptureService.Setup(s => s.GetCompletionPercentageAsync(entry.Id, _testCharacterId))
            .ReturnsAsync(50);
        _mockCaptureService.Setup(s => s.GetUnlockedThresholdsAsync(entry.Id, _testCharacterId))
            .ReturnsAsync(Array.Empty<string>());
        _mockCaptureRepository.Setup(r => r.GetFragmentCountAsync(entry.Id, _testCharacterId))
            .ReturnsAsync(5);

        // Act
        var vm = await _sut.BuildViewModelAsync(_testCharacterId, _testCharacterName, JournalTab.Bestiary, selectedIndex: 0);

        // Assert - RedactedContent should contain some redacted blocks
        vm.SelectedDetail.Should().NotBeNull();
        vm.SelectedDetail!.RedactedContent.Should().Contain("[grey]████[/]");
    }

    [Fact]
    public async Task BuildViewModelAsync_IncludesUnlockedThresholds()
    {
        // Arrange
        var entry = CreateEntry("Complete Entry", EntryCategory.Bestiary);
        entry.UnlockThresholds = new Dictionary<int, string>
        {
            { 25, "WEAKNESS_REVEALED" },
            { 50, "HABITAT_REVEALED" },
            { 100, "FULL_ENTRY" }
        };
        var entries = new List<(CodexEntry Entry, int CompletionPercent)>
        {
            (entry, 100)
        };

        _mockCaptureService.Setup(s => s.GetDiscoveredEntriesAsync(_testCharacterId))
            .ReturnsAsync(entries);
        _mockCodexRepository.Setup(r => r.GetByIdAsync(entry.Id))
            .ReturnsAsync(entry);
        _mockCaptureService.Setup(s => s.GetCompletionPercentageAsync(entry.Id, _testCharacterId))
            .ReturnsAsync(100);
        _mockCaptureService.Setup(s => s.GetUnlockedThresholdsAsync(entry.Id, _testCharacterId))
            .ReturnsAsync(new[] { "WEAKNESS_REVEALED", "HABITAT_REVEALED", "FULL_ENTRY" });
        _mockCaptureRepository.Setup(r => r.GetFragmentCountAsync(entry.Id, _testCharacterId))
            .ReturnsAsync(12);

        // Act
        var vm = await _sut.BuildViewModelAsync(_testCharacterId, _testCharacterName, JournalTab.Bestiary, selectedIndex: 0);

        // Assert
        vm.SelectedDetail.Should().NotBeNull();
        vm.SelectedDetail!.UnlockedThresholds.Should().HaveCount(3);
        vm.SelectedDetail.UnlockedThresholds.Should().Contain("WEAKNESS_REVEALED");
        vm.SelectedDetail.UnlockedThresholds.Should().Contain("HABITAT_REVEALED");
        vm.SelectedDetail.UnlockedThresholds.Should().Contain("FULL_ENTRY");
    }

    #endregion

    #region ViewModel Properties Tests

    [Fact]
    public async Task BuildViewModelAsync_SetsCharacterName()
    {
        // Arrange
        _mockCaptureService.Setup(s => s.GetDiscoveredEntriesAsync(_testCharacterId))
            .ReturnsAsync(new List<(CodexEntry, int)>());

        // Act
        var vm = await _sut.BuildViewModelAsync(_testCharacterId, "Scholar Artemis", JournalTab.Codex);

        // Assert
        vm.CharacterName.Should().Be("Scholar Artemis");
    }

    [Fact]
    public async Task BuildViewModelAsync_SetsActiveTab()
    {
        // Arrange
        _mockCaptureService.Setup(s => s.GetDiscoveredEntriesAsync(_testCharacterId))
            .ReturnsAsync(new List<(CodexEntry, int)>());

        // Act
        var vm = await _sut.BuildViewModelAsync(_testCharacterId, _testCharacterName, JournalTab.Bestiary);

        // Assert
        vm.ActiveTab.Should().Be(JournalTab.Bestiary);
    }

    [Fact]
    public async Task BuildViewModelAsync_SetsStressLevel()
    {
        // Arrange
        _mockCaptureService.Setup(s => s.GetDiscoveredEntriesAsync(_testCharacterId))
            .ReturnsAsync(new List<(CodexEntry, int)>());

        // Act
        var vm = await _sut.BuildViewModelAsync(_testCharacterId, _testCharacterName, JournalTab.Codex, stressLevel: 75);

        // Assert
        vm.StressLevel.Should().Be(75);
    }

    [Fact]
    public async Task BuildViewModelAsync_SetsSelectedEntryIndex()
    {
        // Arrange
        var entries = new List<(CodexEntry Entry, int CompletionPercent)>
        {
            (CreateEntry("Entry 1", EntryCategory.Bestiary), 50),
            (CreateEntry("Entry 2", EntryCategory.Bestiary), 75),
            (CreateEntry("Entry 3", EntryCategory.Bestiary), 100)
        };

        _mockCaptureService.Setup(s => s.GetDiscoveredEntriesAsync(_testCharacterId))
            .ReturnsAsync(entries);

        // Act
        var vm = await _sut.BuildViewModelAsync(_testCharacterId, _testCharacterName, JournalTab.Bestiary, selectedIndex: 2);

        // Assert
        vm.SelectedEntryIndex.Should().Be(2);
    }

    [Fact]
    public async Task BuildViewModelAsync_AssignsOneBasedIndices()
    {
        // Arrange
        var entries = new List<(CodexEntry Entry, int CompletionPercent)>
        {
            (CreateEntry("Entry A", EntryCategory.Bestiary), 50),
            (CreateEntry("Entry B", EntryCategory.Bestiary), 75),
            (CreateEntry("Entry C", EntryCategory.Bestiary), 100)
        };

        _mockCaptureService.Setup(s => s.GetDiscoveredEntriesAsync(_testCharacterId))
            .ReturnsAsync(entries);

        // Act
        var vm = await _sut.BuildViewModelAsync(_testCharacterId, _testCharacterName, JournalTab.Bestiary);

        // Assert - Indices should be 1-based
        vm.Entries[0].Index.Should().Be(1);
        vm.Entries[1].Index.Should().Be(2);
        vm.Entries[2].Index.Should().Be(3);
    }

    #endregion

    #region Helper Methods

    private static CodexEntry CreateEntry(string title, EntryCategory category)
    {
        return new CodexEntry
        {
            Id = Guid.NewGuid(),
            Title = title,
            Category = category,
            FullText = $"Full text content for {title}. This is sample text for testing purposes.",
            TotalFragments = 12,
            UnlockThresholds = new Dictionary<int, string>()
        };
    }

    #endregion
}
