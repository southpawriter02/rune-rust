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
/// Tests for the DocGenService class (v0.3.11b - Developer's Handbook).
/// Validates Markdown documentation generation from system entries.
/// </summary>
public class DocGenServiceTests : IDisposable
{
    private readonly Mock<ILogger<DocGenService>> _mockLogger;
    private readonly Mock<ILibraryService> _mockLibraryService;
    private readonly DocGenService _sut;
    private readonly string _testOutputPath;

    public DocGenServiceTests()
    {
        _mockLogger = new Mock<ILogger<DocGenService>>();
        _mockLibraryService = new Mock<ILibraryService>();
        _sut = new DocGenService(_mockLogger.Object, _mockLibraryService.Object);

        // Create unique temp directory for each test
        _testOutputPath = Path.Combine(Path.GetTempPath(), $"DocGenTest_{Guid.NewGuid():N}");
    }

    public void Dispose()
    {
        // Cleanup temp directory after test
        if (Directory.Exists(_testOutputPath))
        {
            Directory.Delete(_testOutputPath, recursive: true);
        }
    }

    #region GenerateDocsAsync Tests

    [Fact]
    public async Task GenerateDocsAsync_WithEntries_CreatesFiles()
    {
        // Arrange
        var entries = CreateTestEntries(EntryCategory.FieldGuide, 3);
        _mockLibraryService.Setup(x => x.GetSystemEntries()).Returns(entries);

        // Act
        await _sut.GenerateDocsAsync(_testOutputPath);

        // Assert
        Directory.Exists(_testOutputPath).Should().BeTrue();
        File.Exists(Path.Combine(_testOutputPath, "fieldguide.md")).Should().BeTrue();
        File.Exists(Path.Combine(_testOutputPath, "README.md")).Should().BeTrue();
    }

    [Fact]
    public async Task GenerateDocsAsync_WithNoEntries_LogsWarning()
    {
        // Arrange
        _mockLibraryService.Setup(x => x.GetSystemEntries()).Returns(new List<CodexEntry>());

        // Act
        await _sut.GenerateDocsAsync(_testOutputPath);

        // Assert - No files should be created
        if (Directory.Exists(_testOutputPath))
        {
            Directory.GetFiles(_testOutputPath).Should().BeEmpty();
        }

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("No system entries found")),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task GenerateDocsAsync_GroupsByCategory_CreatesMultipleFiles()
    {
        // Arrange
        var fieldGuideEntries = CreateTestEntries(EntryCategory.FieldGuide, 2);
        var bestiaryEntries = CreateTestEntries(EntryCategory.Bestiary, 2);
        var allEntries = fieldGuideEntries.Concat(bestiaryEntries).ToList();
        _mockLibraryService.Setup(x => x.GetSystemEntries()).Returns(allEntries);

        // Act
        await _sut.GenerateDocsAsync(_testOutputPath);

        // Assert
        File.Exists(Path.Combine(_testOutputPath, "fieldguide.md")).Should().BeTrue();
        File.Exists(Path.Combine(_testOutputPath, "bestiary.md")).Should().BeTrue();
        File.Exists(Path.Combine(_testOutputPath, "README.md")).Should().BeTrue();
    }

    [Fact]
    public async Task GenerateDocsAsync_EscapesPipesInContent()
    {
        // Arrange
        var entries = new List<CodexEntry>
        {
            new CodexEntry
            {
                Id = Guid.NewGuid(),
                Title = "Test|Entry",
                Category = EntryCategory.FieldGuide,
                FullText = "Description with | pipe character",
                TotalFragments = 1
            }
        };
        _mockLibraryService.Setup(x => x.GetSystemEntries()).Returns(entries);

        // Act
        await _sut.GenerateDocsAsync(_testOutputPath);

        // Assert
        var content = await File.ReadAllTextAsync(Path.Combine(_testOutputPath, "fieldguide.md"));
        content.Should().Contain("Test\\|Entry");
        content.Should().Contain("Description with \\| pipe character");
    }

    [Fact]
    public async Task GenerateDocsAsync_CreatesReadmeIndex()
    {
        // Arrange
        var entries = CreateTestEntries(EntryCategory.FieldGuide, 5);
        _mockLibraryService.Setup(x => x.GetSystemEntries()).Returns(entries);

        // Act
        await _sut.GenerateDocsAsync(_testOutputPath);

        // Assert
        var readmePath = Path.Combine(_testOutputPath, "README.md");
        File.Exists(readmePath).Should().BeTrue();

        var content = await File.ReadAllTextAsync(readmePath);
        content.Should().Contain("# Generated Documentation");
        content.Should().Contain("[FieldGuide](fieldguide.md)");
        content.Should().Contain("(5 entries)");
        content.Should().Contain("--docgen");
    }

    [Fact]
    public async Task GenerateDocsAsync_GeneratesValidMarkdownTable()
    {
        // Arrange
        var entries = new List<CodexEntry>
        {
            new CodexEntry
            {
                Id = Guid.NewGuid(),
                Title = "TestTitle",
                Category = EntryCategory.FieldGuide,
                FullText = "TestDescription",
                TotalFragments = 1
            }
        };
        _mockLibraryService.Setup(x => x.GetSystemEntries()).Returns(entries);

        // Act
        await _sut.GenerateDocsAsync(_testOutputPath);

        // Assert
        var content = await File.ReadAllTextAsync(Path.Combine(_testOutputPath, "fieldguide.md"));
        content.Should().Contain("| Title | Description |");
        content.Should().Contain("|-------|-------------|");
        content.Should().Contain("| **TestTitle** | TestDescription |");
    }

    [Fact]
    public async Task GenerateDocsAsync_SortsEntriesAlphabetically()
    {
        // Arrange
        var entries = new List<CodexEntry>
        {
            new CodexEntry { Id = Guid.NewGuid(), Title = "Zebra", Category = EntryCategory.FieldGuide, FullText = "Z desc", TotalFragments = 1 },
            new CodexEntry { Id = Guid.NewGuid(), Title = "Alpha", Category = EntryCategory.FieldGuide, FullText = "A desc", TotalFragments = 1 },
            new CodexEntry { Id = Guid.NewGuid(), Title = "Beta", Category = EntryCategory.FieldGuide, FullText = "B desc", TotalFragments = 1 }
        };
        _mockLibraryService.Setup(x => x.GetSystemEntries()).Returns(entries);

        // Act
        await _sut.GenerateDocsAsync(_testOutputPath);

        // Assert
        var content = await File.ReadAllTextAsync(Path.Combine(_testOutputPath, "fieldguide.md"));
        var alphaIndex = content.IndexOf("Alpha");
        var betaIndex = content.IndexOf("Beta");
        var zebraIndex = content.IndexOf("Zebra");

        alphaIndex.Should().BeLessThan(betaIndex);
        betaIndex.Should().BeLessThan(zebraIndex);
    }

    [Fact]
    public async Task GenerateDocsAsync_HandlesNewlinesInDescription()
    {
        // Arrange
        var entries = new List<CodexEntry>
        {
            new CodexEntry
            {
                Id = Guid.NewGuid(),
                Title = "MultiLine",
                Category = EntryCategory.FieldGuide,
                FullText = "Line one\nLine two\r\nLine three",
                TotalFragments = 1
            }
        };
        _mockLibraryService.Setup(x => x.GetSystemEntries()).Returns(entries);

        // Act
        await _sut.GenerateDocsAsync(_testOutputPath);

        // Assert
        var content = await File.ReadAllTextAsync(Path.Combine(_testOutputPath, "fieldguide.md"));
        // Newlines should be replaced with spaces
        content.Should().Contain("Line one Line two Line three");
    }

    [Fact]
    public async Task GenerateDocsAsync_CreatesOutputDirectory()
    {
        // Arrange
        var entries = CreateTestEntries(EntryCategory.FieldGuide, 1);
        _mockLibraryService.Setup(x => x.GetSystemEntries()).Returns(entries);

        // Ensure path doesn't exist
        if (Directory.Exists(_testOutputPath))
        {
            Directory.Delete(_testOutputPath, recursive: true);
        }

        // Act
        await _sut.GenerateDocsAsync(_testOutputPath);

        // Assert
        Directory.Exists(_testOutputPath).Should().BeTrue();
    }

    [Fact]
    public async Task GenerateDocsAsync_LogsStartAndComplete()
    {
        // Arrange
        var entries = CreateTestEntries(EntryCategory.FieldGuide, 1);
        _mockLibraryService.Setup(x => x.GetSystemEntries()).Returns(entries);

        // Act
        await _sut.GenerateDocsAsync(_testOutputPath);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Starting generation")),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Completed successfully")),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion

    #region Helper Methods

    private static List<CodexEntry> CreateTestEntries(EntryCategory category, int count)
    {
        var entries = new List<CodexEntry>();
        for (int i = 0; i < count; i++)
        {
            entries.Add(new CodexEntry
            {
                Id = Guid.NewGuid(),
                Title = $"TestEntry{i}",
                Category = category,
                FullText = $"Description for test entry {i}",
                TotalFragments = 1,
                CreatedAt = DateTime.UtcNow,
                LastModified = DateTime.UtcNow
            });
        }
        return entries;
    }

    #endregion
}
