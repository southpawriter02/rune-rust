using Microsoft.Extensions.Logging;
using NSubstitute;
using RuneAndRust.Core.Enums;
using RuneAndRust.Engine.Repositories;
using Xunit;

namespace RuneAndRust.Tests.Engine.Repositories;

/// <summary>
/// Unit tests for JsonCaptureTemplateRepository.
/// v0.3.25b: Data-driven template system tests.
/// </summary>
public class JsonCaptureTemplateRepositoryTests : IDisposable
{
    private readonly string _testDir;
    private readonly ILogger<JsonCaptureTemplateRepository> _logger;

    public JsonCaptureTemplateRepositoryTests()
    {
        _testDir = Path.Combine(Path.GetTempPath(), $"capture-templates-{Guid.NewGuid()}");
        Directory.CreateDirectory(_testDir);
        _logger = Substitute.For<ILogger<JsonCaptureTemplateRepository>>();
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDir))
            Directory.Delete(_testDir, recursive: true);
    }

    [Fact]
    public async Task GetByCategoryAsync_ValidCategory_ReturnsTemplates()
    {
        // Arrange
        CreateTestFile("test-category.json", """
        {
            "category": "test-category",
            "version": "1.0.0",
            "matchKeywords": ["test"],
            "templates": [
                {
                    "id": "test-template-1",
                    "type": "TextFragment",
                    "fragmentContent": "This is a test fragment with enough content for validation.",
                    "source": "Test source"
                }
            ]
        }
        """);

        var repo = new JsonCaptureTemplateRepository(_logger, _testDir);

        // Act
        var templates = await repo.GetByCategoryAsync("test-category");

        // Assert
        Assert.Single(templates);
        Assert.Equal("test-template-1", templates[0].Id);
        Assert.Equal(CaptureType.TextFragment, templates[0].Type);
        Assert.Equal("test-category", templates[0].Category);
    }

    [Fact]
    public async Task GetByCategoryAsync_InvalidCategory_ReturnsEmpty()
    {
        // Arrange
        var repo = new JsonCaptureTemplateRepository(_logger, _testDir);

        // Act
        var templates = await repo.GetByCategoryAsync("nonexistent");

        // Assert
        Assert.Empty(templates);
    }

    [Fact]
    public async Task GetRandomAsync_ValidCategory_ReturnsTemplate()
    {
        // Arrange
        CreateTestFile("random-test.json", """
        {
            "category": "random-test",
            "version": "1.0.0",
            "matchKeywords": ["test"],
            "templates": [
                { "id": "t1", "type": "Specimen", "fragmentContent": "Content one with sufficient length for tests.", "source": "Test" },
                { "id": "t2", "type": "Specimen", "fragmentContent": "Content two with sufficient length for tests.", "source": "Test" }
            ]
        }
        """);

        var repo = new JsonCaptureTemplateRepository(_logger, _testDir);

        // Act
        var template = await repo.GetRandomAsync("random-test");

        // Assert
        Assert.NotNull(template);
        Assert.Contains(template.Id, new[] { "t1", "t2" });
    }

    [Fact]
    public async Task GetRandomAsync_EmptyCategory_ReturnsNull()
    {
        // Arrange
        var repo = new JsonCaptureTemplateRepository(_logger, _testDir);

        // Act
        var template = await repo.GetRandomAsync("nonexistent");

        // Assert
        Assert.Null(template);
    }

    [Fact]
    public async Task GetByIdAsync_ValidId_ReturnsTemplate()
    {
        // Arrange
        CreateTestFile("id-test.json", """
        {
            "category": "id-test",
            "version": "1.0.0",
            "matchKeywords": ["test"],
            "templates": [
                { "id": "specific-id", "type": "OralHistory", "fragmentContent": "Specific content with enough text for validation.", "source": "Test" }
            ]
        }
        """);

        var repo = new JsonCaptureTemplateRepository(_logger, _testDir);

        // Act
        var template = await repo.GetByIdAsync("specific-id");

        // Assert
        Assert.NotNull(template);
        Assert.Equal("specific-id", template.Id);
        Assert.Equal(CaptureType.OralHistory, template.Type);
    }

    [Fact]
    public async Task GetByIdAsync_InvalidId_ReturnsNull()
    {
        // Arrange
        var repo = new JsonCaptureTemplateRepository(_logger, _testDir);

        // Act
        var template = await repo.GetByIdAsync("nonexistent-id");

        // Assert
        Assert.Null(template);
    }

    [Fact]
    public async Task GetCategoriesAsync_MultipleFiles_ReturnsAllCategories()
    {
        // Arrange
        CreateTestFile("cat-1.json", """
        { "category": "cat-1", "version": "1.0.0", "matchKeywords": ["a"], "templates": [{ "id": "t1", "type": "TextFragment", "fragmentContent": "Content for category one test here.", "source": "Test" }] }
        """);
        CreateTestFile("cat-2.json", """
        { "category": "cat-2", "version": "1.0.0", "matchKeywords": ["b"], "templates": [{ "id": "t2", "type": "TextFragment", "fragmentContent": "Content for category two test here.", "source": "Test" }] }
        """);

        var repo = new JsonCaptureTemplateRepository(_logger, _testDir);

        // Act
        var categories = await repo.GetCategoriesAsync();

        // Assert
        Assert.Equal(2, categories.Count);
        Assert.Contains("cat-1", categories);
        Assert.Contains("cat-2", categories);
    }

    [Fact]
    public async Task TotalTemplateCount_AfterLoad_ReturnsCorrectCount()
    {
        // Arrange
        CreateTestFile("count-test.json", """
        {
            "category": "count-test",
            "version": "1.0.0",
            "matchKeywords": ["test"],
            "templates": [
                { "id": "t1", "type": "TextFragment", "fragmentContent": "First template content here for test.", "source": "Test" },
                { "id": "t2", "type": "TextFragment", "fragmentContent": "Second template content here for test.", "source": "Test" },
                { "id": "t3", "type": "TextFragment", "fragmentContent": "Third template content here for test.", "source": "Test" }
            ]
        }
        """);

        var repo = new JsonCaptureTemplateRepository(_logger, _testDir);

        // Act
        await repo.GetCategoriesAsync(); // Trigger load

        // Assert
        Assert.Equal(3, repo.TotalTemplateCount);
    }

    [Fact]
    public async Task LoadTemplates_InvalidType_SkipsTemplate()
    {
        // Arrange
        CreateTestFile("invalid-type.json", """
        {
            "category": "invalid-type",
            "version": "1.0.0",
            "matchKeywords": ["test"],
            "templates": [
                { "id": "valid", "type": "TextFragment", "fragmentContent": "Valid template content here for test.", "source": "Test" },
                { "id": "invalid", "type": "NotARealType", "fragmentContent": "Invalid template content here.", "source": "Test" }
            ]
        }
        """);

        var repo = new JsonCaptureTemplateRepository(_logger, _testDir);

        // Act
        var templates = await repo.GetByCategoryAsync("invalid-type");

        // Assert
        Assert.Single(templates);
        Assert.Equal("valid", templates[0].Id);
    }

    [Fact]
    public async Task ReloadAsync_ClearsAndReloads()
    {
        // Arrange
        CreateTestFile("reload-test.json", """
        {
            "category": "reload-test",
            "version": "1.0.0",
            "matchKeywords": ["test"],
            "templates": [{ "id": "original", "type": "TextFragment", "fragmentContent": "Original content here for test validation.", "source": "Test" }]
        }
        """);

        var repo = new JsonCaptureTemplateRepository(_logger, _testDir);
        await repo.GetCategoriesAsync(); // Initial load

        // Modify file
        File.WriteAllText(Path.Combine(_testDir, "reload-test.json"), """
        {
            "category": "reload-test",
            "version": "1.0.0",
            "matchKeywords": ["test"],
            "templates": [{ "id": "updated", "type": "TextFragment", "fragmentContent": "Updated content here for test validation.", "source": "Test" }]
        }
        """);

        // Act
        await repo.ReloadAsync();
        var templates = await repo.GetByCategoryAsync("reload-test");

        // Assert
        Assert.Single(templates);
        Assert.Equal("updated", templates[0].Id);
    }

    [Fact]
    public async Task TemplateKeywords_InheritsFromCategory_WhenNotSpecified()
    {
        // Arrange
        CreateTestFile("keywords-test.json", """
        {
            "category": "keywords-test",
            "version": "1.0.0",
            "matchKeywords": ["category", "level", "keywords"],
            "templates": [
                { "id": "inherits", "type": "TextFragment", "fragmentContent": "Template without keywords inherits from category.", "source": "Test" }
            ]
        }
        """);

        var repo = new JsonCaptureTemplateRepository(_logger, _testDir);

        // Act
        var template = await repo.GetByIdAsync("inherits");

        // Assert
        Assert.NotNull(template);
        Assert.Equal(new[] { "category", "level", "keywords" }, template.MatchKeywords);
    }

    [Fact]
    public async Task TemplateKeywords_OverridesCategory_WhenSpecified()
    {
        // Arrange
        CreateTestFile("override-keywords.json", """
        {
            "category": "override-keywords",
            "version": "1.0.0",
            "matchKeywords": ["category", "default"],
            "templates": [
                {
                    "id": "overrides",
                    "type": "TextFragment",
                    "fragmentContent": "Template with custom keywords overrides category.",
                    "source": "Test",
                    "matchKeywords": ["custom", "keywords", "here"]
                }
            ]
        }
        """);

        var repo = new JsonCaptureTemplateRepository(_logger, _testDir);

        // Act
        var template = await repo.GetByIdAsync("overrides");

        // Assert
        Assert.NotNull(template);
        Assert.Equal(new[] { "custom", "keywords", "here" }, template.MatchKeywords);
    }

    [Fact]
    public async Task Quality_DefaultsTo15_WhenNotSpecified()
    {
        // Arrange
        CreateTestFile("quality-default.json", """
        {
            "category": "quality-default",
            "version": "1.0.0",
            "matchKeywords": ["test"],
            "templates": [
                { "id": "default-quality", "type": "TextFragment", "fragmentContent": "Template without explicit quality value.", "source": "Test" }
            ]
        }
        """);

        var repo = new JsonCaptureTemplateRepository(_logger, _testDir);

        // Act
        var template = await repo.GetByIdAsync("default-quality");

        // Assert
        Assert.NotNull(template);
        Assert.Equal(15, template.Quality);
    }

    [Fact]
    public async Task Quality_UsesSpecifiedValue_WhenProvided()
    {
        // Arrange
        CreateTestFile("quality-custom.json", """
        {
            "category": "quality-custom",
            "version": "1.0.0",
            "matchKeywords": ["test"],
            "templates": [
                { "id": "custom-quality", "type": "TextFragment", "fragmentContent": "Template with custom quality value.", "source": "Test", "quality": 75 }
            ]
        }
        """);

        var repo = new JsonCaptureTemplateRepository(_logger, _testDir);

        // Act
        var template = await repo.GetByIdAsync("custom-quality");

        // Assert
        Assert.NotNull(template);
        Assert.Equal(75, template.Quality);
    }

    [Fact]
    public async Task AllCaptureTypes_ParseCorrectly()
    {
        // Arrange
        CreateTestFile("all-types.json", """
        {
            "category": "all-types",
            "version": "1.0.0",
            "matchKeywords": ["test"],
            "templates": [
                { "id": "text", "type": "TextFragment", "fragmentContent": "TextFragment type content here.", "source": "Test" },
                { "id": "echo", "type": "EchoRecording", "fragmentContent": "EchoRecording type content here.", "source": "Test" },
                { "id": "visual", "type": "VisualRecord", "fragmentContent": "VisualRecord type content here.", "source": "Test" },
                { "id": "specimen", "type": "Specimen", "fragmentContent": "Specimen type content here.", "source": "Test" },
                { "id": "oral", "type": "OralHistory", "fragmentContent": "OralHistory type content here.", "source": "Test" },
                { "id": "runic", "type": "RunicTrace", "fragmentContent": "RunicTrace type content here.", "source": "Test" }
            ]
        }
        """);

        var repo = new JsonCaptureTemplateRepository(_logger, _testDir);

        // Act
        var templates = await repo.GetByCategoryAsync("all-types");

        // Assert
        Assert.Equal(6, templates.Count);
        Assert.Equal(CaptureType.TextFragment, templates.First(t => t.Id == "text").Type);
        Assert.Equal(CaptureType.EchoRecording, templates.First(t => t.Id == "echo").Type);
        Assert.Equal(CaptureType.VisualRecord, templates.First(t => t.Id == "visual").Type);
        Assert.Equal(CaptureType.Specimen, templates.First(t => t.Id == "specimen").Type);
        Assert.Equal(CaptureType.OralHistory, templates.First(t => t.Id == "oral").Type);
        Assert.Equal(CaptureType.RunicTrace, templates.First(t => t.Id == "runic").Type);
    }

    [Fact]
    public async Task MissingDirectory_ReturnsEmptyCollections()
    {
        // Arrange
        var nonExistentPath = Path.Combine(Path.GetTempPath(), $"nonexistent-{Guid.NewGuid()}");
        var repo = new JsonCaptureTemplateRepository(_logger, nonExistentPath);

        // Act
        var categories = await repo.GetCategoriesAsync();
        var templates = await repo.GetByCategoryAsync("any");

        // Assert
        Assert.Empty(categories);
        Assert.Empty(templates);
        Assert.Equal(0, repo.TotalTemplateCount);
    }

    private void CreateTestFile(string filename, string content)
    {
        File.WriteAllText(Path.Combine(_testDir, filename), content);
    }
}
