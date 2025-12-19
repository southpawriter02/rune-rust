using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Engine.Services;
using Xunit;

namespace RuneAndRust.Tests.Engine;

/// <summary>
/// Tests for the JournalService class.
/// Validates journal display formatting, entry detail views, and fragment lists.
/// </summary>
public class JournalServiceTests
{
    private readonly Mock<ILogger<JournalService>> _mockLogger;
    private readonly Mock<IDataCaptureService> _mockCaptureService;
    private readonly Mock<IDataCaptureRepository> _mockCaptureRepository;
    private readonly Mock<ICodexEntryRepository> _mockCodexRepository;
    private readonly JournalService _sut;
    private readonly Guid _testCharacterId;

    public JournalServiceTests()
    {
        _mockLogger = new Mock<ILogger<JournalService>>();
        _mockCaptureService = new Mock<IDataCaptureService>();
        _mockCaptureRepository = new Mock<IDataCaptureRepository>();
        _mockCodexRepository = new Mock<ICodexEntryRepository>();
        _testCharacterId = Guid.NewGuid();

        _sut = new JournalService(
            _mockLogger.Object,
            _mockCaptureService.Object,
            _mockCaptureRepository.Object,
            _mockCodexRepository.Object);
    }

    #region FormatJournalListAsync Tests

    [Fact]
    public async Task FormatJournalListAsync_NoEntries_ReturnsEmptyMessage()
    {
        // Arrange
        _mockCaptureService
            .Setup(x => x.GetDiscoveredEntriesAsync(_testCharacterId))
            .ReturnsAsync(Enumerable.Empty<(CodexEntry, int)>());

        // Act
        var result = await _sut.FormatJournalListAsync(_testCharacterId);

        // Assert
        result.Should().Contain("No discoveries recorded yet");
        result.Should().Contain("SCAVENGER'S JOURNAL");
    }

    [Fact]
    public async Task FormatJournalListAsync_WithEntries_GroupsByCategory()
    {
        // Arrange
        var entries = new List<(CodexEntry Entry, int CompletionPercent)>
        {
            (CreateCodexEntry("Rusted Servitor", CodexCategory.Bestiary), 50),
            (CreateCodexEntry("Combat Basics", CodexCategory.FieldGuide), 100),
            (CreateCodexEntry("Iron-Husk", CodexCategory.Bestiary), 25)
        };

        _mockCaptureService
            .Setup(x => x.GetDiscoveredEntriesAsync(_testCharacterId))
            .ReturnsAsync(entries);

        // Act
        var result = await _sut.FormatJournalListAsync(_testCharacterId);

        // Assert
        result.Should().Contain("Bestiary");
        result.Should().Contain("FieldGuide");
        result.Should().Contain("Rusted Servitor");
        result.Should().Contain("Combat Basics");
    }

    [Fact]
    public async Task FormatJournalListAsync_ShowsCompletionPercentage()
    {
        // Arrange
        var entries = new List<(CodexEntry Entry, int CompletionPercent)>
        {
            (CreateCodexEntry("Test Entry", CodexCategory.Bestiary), 75)
        };

        _mockCaptureService
            .Setup(x => x.GetDiscoveredEntriesAsync(_testCharacterId))
            .ReturnsAsync(entries);

        // Act
        var result = await _sut.FormatJournalListAsync(_testCharacterId);

        // Assert
        result.Should().Contain("(75%)");
    }

    [Fact]
    public async Task FormatJournalListAsync_CompletedEntry_ShowsStarIcon()
    {
        // Arrange
        var entries = new List<(CodexEntry Entry, int CompletionPercent)>
        {
            (CreateCodexEntry("Complete Entry", CodexCategory.FieldGuide), 100)
        };

        _mockCaptureService
            .Setup(x => x.GetDiscoveredEntriesAsync(_testCharacterId))
            .ReturnsAsync(entries);

        // Act
        var result = await _sut.FormatJournalListAsync(_testCharacterId);

        // Assert
        result.Should().Contain("[green]★[/]");
        result.Should().Contain("(100%)");
    }

    [Fact]
    public async Task FormatJournalListAsync_IncompleteEntry_ShowsCircleIcon()
    {
        // Arrange
        var entries = new List<(CodexEntry Entry, int CompletionPercent)>
        {
            (CreateCodexEntry("Incomplete Entry", CodexCategory.FieldGuide), 50)
        };

        _mockCaptureService
            .Setup(x => x.GetDiscoveredEntriesAsync(_testCharacterId))
            .ReturnsAsync(entries);

        // Act
        var result = await _sut.FormatJournalListAsync(_testCharacterId);

        // Assert
        result.Should().Contain("[grey]●[/]");
    }

    [Fact]
    public async Task FormatJournalListAsync_ShowsUsageHint()
    {
        // Arrange
        var entries = new List<(CodexEntry Entry, int CompletionPercent)>
        {
            (CreateCodexEntry("Test", CodexCategory.FieldGuide), 50)
        };

        _mockCaptureService
            .Setup(x => x.GetDiscoveredEntriesAsync(_testCharacterId))
            .ReturnsAsync(entries);

        // Act
        var result = await _sut.FormatJournalListAsync(_testCharacterId);

        // Assert
        result.Should().Contain("codex <name>");
    }

    #endregion

    #region FormatEntryDetailAsync Tests

    [Fact]
    public async Task FormatEntryDetailAsync_EntryNotFound_ReturnsError()
    {
        // Arrange
        _mockCodexRepository
            .Setup(x => x.GetByTitleAsync("Unknown Entry"))
            .ReturnsAsync((CodexEntry?)null);

        // Act
        var result = await _sut.FormatEntryDetailAsync(_testCharacterId, "Unknown Entry");

        // Assert
        result.Should().Contain("No entry found");
        result.Should().Contain("Unknown Entry");
        result.Should().Contain("[red]");
    }

    [Fact]
    public async Task FormatEntryDetailAsync_IncompleteEntry_RedactsText()
    {
        // Arrange
        var entry = CreateCodexEntry("Test Entry", CodexCategory.Bestiary);
        entry.FullText = "This is the full description of the test entry.";

        _mockCodexRepository
            .Setup(x => x.GetByTitleAsync("Test Entry"))
            .ReturnsAsync(entry);
        _mockCaptureService
            .Setup(x => x.GetCompletionPercentageAsync(entry.Id, _testCharacterId))
            .ReturnsAsync(30);
        _mockCaptureService
            .Setup(x => x.GetUnlockedThresholdsAsync(entry.Id, _testCharacterId))
            .ReturnsAsync(Enumerable.Empty<string>());

        // Act
        var result = await _sut.FormatEntryDetailAsync(_testCharacterId, "Test Entry");

        // Assert
        result.Should().Contain("[grey]");  // Redacted blocks use grey markup
        result.Should().Contain("30%");
    }

    [Fact]
    public async Task FormatEntryDetailAsync_CompleteEntry_ShowsFullText()
    {
        // Arrange
        var entry = CreateCodexEntry("Complete Entry", CodexCategory.FieldGuide);
        entry.FullText = "This is the complete text without any redaction.";

        _mockCodexRepository
            .Setup(x => x.GetByTitleAsync("Complete Entry"))
            .ReturnsAsync(entry);
        _mockCaptureService
            .Setup(x => x.GetCompletionPercentageAsync(entry.Id, _testCharacterId))
            .ReturnsAsync(100);
        _mockCaptureService
            .Setup(x => x.GetUnlockedThresholdsAsync(entry.Id, _testCharacterId))
            .ReturnsAsync(new[] { "WEAKNESS_REVEALED" });

        // Act
        var result = await _sut.FormatEntryDetailAsync(_testCharacterId, "Complete Entry");

        // Assert
        result.Should().Contain("This is the complete text without any redaction.");
        result.Should().Contain("100%");
    }

    [Fact]
    public async Task FormatEntryDetailAsync_ShowsUnlockedThresholds()
    {
        // Arrange
        var entry = CreateCodexEntry("Entry With Tags", CodexCategory.Bestiary);

        _mockCodexRepository
            .Setup(x => x.GetByTitleAsync("Entry With Tags"))
            .ReturnsAsync(entry);
        _mockCaptureService
            .Setup(x => x.GetCompletionPercentageAsync(entry.Id, _testCharacterId))
            .ReturnsAsync(75);
        _mockCaptureService
            .Setup(x => x.GetUnlockedThresholdsAsync(entry.Id, _testCharacterId))
            .ReturnsAsync(new[] { "WEAKNESS_REVEALED", "HABITAT_REVEALED" });

        // Act
        var result = await _sut.FormatEntryDetailAsync(_testCharacterId, "Entry With Tags");

        // Assert
        result.Should().Contain("Discoveries:");
        result.Should().Contain("Weakness Revealed");
        result.Should().Contain("Habitat Revealed");
        result.Should().Contain("[green]✓[/]");
    }

    [Fact]
    public async Task FormatEntryDetailAsync_ShowsCategoryAndTitle()
    {
        // Arrange
        var entry = CreateCodexEntry("Iron-Husk", CodexCategory.Bestiary);

        _mockCodexRepository
            .Setup(x => x.GetByTitleAsync("Iron-Husk"))
            .ReturnsAsync(entry);
        _mockCaptureService
            .Setup(x => x.GetCompletionPercentageAsync(entry.Id, _testCharacterId))
            .ReturnsAsync(50);
        _mockCaptureService
            .Setup(x => x.GetUnlockedThresholdsAsync(entry.Id, _testCharacterId))
            .ReturnsAsync(Enumerable.Empty<string>());

        // Act
        var result = await _sut.FormatEntryDetailAsync(_testCharacterId, "Iron-Husk");

        // Assert
        result.Should().Contain("IRON-HUSK");  // Title in uppercase
        result.Should().Contain("Category:");
        result.Should().Contain("Bestiary");
        result.Should().Contain("Completion:");
    }

    #endregion

    #region FormatUnassignedCapturesAsync Tests

    [Fact]
    public async Task FormatUnassignedCapturesAsync_NoCaptures_ReturnsEmpty()
    {
        // Arrange
        _mockCaptureRepository
            .Setup(x => x.GetUnassignedAsync(_testCharacterId))
            .ReturnsAsync(Enumerable.Empty<DataCapture>());

        // Act
        var result = await _sut.FormatUnassignedCapturesAsync(_testCharacterId);

        // Assert
        result.Should().Contain("No unassigned fragments");
    }

    [Fact]
    public async Task FormatUnassignedCapturesAsync_WithCaptures_ShowsDetails()
    {
        // Arrange
        var captures = new List<DataCapture>
        {
            new()
            {
                Id = Guid.NewGuid(),
                CharacterId = _testCharacterId,
                Type = CaptureType.TextFragment,
                Source = "Old Container",
                FragmentContent = "A mysterious inscription was found here...",
                DiscoveredAt = DateTime.UtcNow
            }
        };

        _mockCaptureRepository
            .Setup(x => x.GetUnassignedAsync(_testCharacterId))
            .ReturnsAsync(captures);

        // Act
        var result = await _sut.FormatUnassignedCapturesAsync(_testCharacterId);

        // Assert
        result.Should().Contain("Unassigned Fragments");
        result.Should().Contain("TextFragment");
        result.Should().Contain("Old Container");
        result.Should().Contain("A mysterious inscription");
    }

    [Fact]
    public async Task FormatUnassignedCapturesAsync_TruncatesLongContent()
    {
        // Arrange
        var longContent = new string('x', 100); // 100 characters
        var captures = new List<DataCapture>
        {
            new()
            {
                Id = Guid.NewGuid(),
                CharacterId = _testCharacterId,
                Type = CaptureType.TextFragment,
                Source = "Source",
                FragmentContent = longContent,
                DiscoveredAt = DateTime.UtcNow
            }
        };

        _mockCaptureRepository
            .Setup(x => x.GetUnassignedAsync(_testCharacterId))
            .ReturnsAsync(captures);

        // Act
        var result = await _sut.FormatUnassignedCapturesAsync(_testCharacterId);

        // Assert
        result.Should().Contain("...");  // Truncation indicator
        result.Should().NotContain(longContent);  // Full content should not appear
    }

    #endregion

    #region Helper Methods

    private static CodexEntry CreateCodexEntry(string title, CodexCategory category)
    {
        return new CodexEntry
        {
            Id = Guid.NewGuid(),
            Title = title,
            Category = category,
            FullText = $"Full text for {title}",
            Thresholds = new List<ThresholdDefinition>(),
            Rarity = DataRarity.Common
        };
    }

    #endregion
}
