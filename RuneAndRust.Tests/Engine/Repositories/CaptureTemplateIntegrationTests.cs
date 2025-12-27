using Microsoft.Extensions.Logging;
using NSubstitute;
using RuneAndRust.Engine.Repositories;
using Xunit;

namespace RuneAndRust.Tests.Engine.Repositories;

/// <summary>
/// Integration tests that verify the actual v0.3.25a JSON templates load correctly.
/// v0.3.25b: Data-driven template system integration.
/// </summary>
public class CaptureTemplateIntegrationTests
{
    private readonly ILogger<JsonCaptureTemplateRepository> _logger;
    private readonly string _templatesPath;

    public CaptureTemplateIntegrationTests()
    {
        _logger = Substitute.For<ILogger<JsonCaptureTemplateRepository>>();
        // Path relative to test execution directory
        _templatesPath = Path.Combine(
            Directory.GetCurrentDirectory(),
            "..", "..", "..", "..", "..",
            "data", "capture-templates", "categories"
        );
    }

    [Fact]
    public async Task ActualTemplates_Load19Templates()
    {
        // Skip if templates directory doesn't exist (CI environment)
        if (!Directory.Exists(_templatesPath))
        {
            return;
        }

        // Arrange
        var repo = new JsonCaptureTemplateRepository(_logger, _templatesPath);

        // Act
        var categories = await repo.GetCategoriesAsync();

        // Assert
        Assert.Equal(19, repo.TotalTemplateCount);
        Assert.Equal(6, categories.Count);
    }

    [Fact]
    public async Task ActualTemplates_HaveExpectedCategories()
    {
        // Skip if templates directory doesn't exist
        if (!Directory.Exists(_templatesPath))
        {
            return;
        }

        // Arrange
        var repo = new JsonCaptureTemplateRepository(_logger, _templatesPath);

        // Act
        var categories = await repo.GetCategoriesAsync();

        // Assert
        Assert.Contains("rusted-servitor", categories);
        Assert.Contains("generic-container", categories);
        Assert.Contains("blighted-creature", categories);
        Assert.Contains("industrial-site", categories);
        Assert.Contains("ancient-ruin", categories);
        Assert.Contains("field-guide-triggers", categories);
    }

    [Fact]
    public async Task ActualTemplates_RustedServitorHas4Templates()
    {
        // Skip if templates directory doesn't exist
        if (!Directory.Exists(_templatesPath))
        {
            return;
        }

        // Arrange
        var repo = new JsonCaptureTemplateRepository(_logger, _templatesPath);

        // Act
        var templates = await repo.GetByCategoryAsync("rusted-servitor");

        // Assert
        Assert.Equal(4, templates.Count);
    }

    [Fact]
    public async Task ActualTemplates_CanFindByKnownId()
    {
        // Skip if templates directory doesn't exist
        if (!Directory.Exists(_templatesPath))
        {
            return;
        }

        // Arrange
        var repo = new JsonCaptureTemplateRepository(_logger, _templatesPath);

        // Act
        var template = await repo.GetByIdAsync("servitor-fungal-infection");

        // Assert
        Assert.NotNull(template);
        Assert.Equal("Servitor examination", template.Source);
    }
}
