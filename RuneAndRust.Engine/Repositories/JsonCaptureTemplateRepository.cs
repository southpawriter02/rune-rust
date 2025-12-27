using System.Collections.Concurrent;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Core.Models;

namespace RuneAndRust.Engine.Repositories;

/// <summary>
/// Repository that loads capture templates from JSON files.
/// v0.3.25b: Data-driven template system.
/// </summary>
/// <remarks>
/// See: SPEC-CODEX-001 for Data Capture System design.
/// Templates are loaded lazily on first access and cached in memory.
/// </remarks>
public class JsonCaptureTemplateRepository : ICaptureTemplateRepository
{
    private readonly ILogger<JsonCaptureTemplateRepository> _logger;
    private readonly string _templatesPath;
    private readonly Random _random;
    private readonly ConcurrentDictionary<string, List<CaptureTemplateDto>> _categoryCache;
    private readonly ConcurrentDictionary<string, CaptureTemplateDto> _idCache;
    private bool _isLoaded;
    private readonly object _loadLock = new();

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    /// <summary>
    /// Initializes a new instance of the repository.
    /// </summary>
    /// <param name="logger">Logger for diagnostics.</param>
    /// <param name="templatesPath">Base path to template directory. Defaults to "data/capture-templates/categories".</param>
    public JsonCaptureTemplateRepository(
        ILogger<JsonCaptureTemplateRepository> logger,
        string? templatesPath = null)
    {
        _logger = logger;
        _templatesPath = templatesPath ?? Path.Combine("data", "capture-templates", "categories");
        _random = new Random();
        _categoryCache = new ConcurrentDictionary<string, List<CaptureTemplateDto>>();
        _idCache = new ConcurrentDictionary<string, CaptureTemplateDto>();

        _logger.LogDebug("[Templates] Repository initialized with path: '{Path}'", _templatesPath);
    }

    /// <inheritdoc/>
    public int TotalTemplateCount => _idCache.Count;

    /// <inheritdoc/>
    public async Task<IReadOnlyList<CaptureTemplateDto>> GetByCategoryAsync(string category)
    {
        await EnsureLoadedAsync();

        if (_categoryCache.TryGetValue(category, out var templates))
        {
            _logger.LogTrace("[Templates] Retrieved {Count} templates for category '{Category}'",
                templates.Count, category);
            return templates.AsReadOnly();
        }

        _logger.LogDebug("[Templates] Category '{Category}' not found", category);
        return Array.Empty<CaptureTemplateDto>();
    }

    /// <inheritdoc/>
    public async Task<CaptureTemplateDto?> GetRandomAsync(string category)
    {
        var templates = await GetByCategoryAsync(category);

        if (templates.Count == 0)
        {
            _logger.LogDebug("[Templates] No templates found for category '{Category}'", category);
            return null;
        }

        var index = _random.Next(templates.Count);
        var template = templates[index];

        _logger.LogTrace("[Templates] Selected random template '{Id}' from category '{Category}'",
            template.Id, category);

        return template;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<string>> GetCategoriesAsync()
    {
        await EnsureLoadedAsync();
        var categories = _categoryCache.Keys.ToList().AsReadOnly();
        _logger.LogTrace("[Templates] Retrieved {Count} categories", categories.Count);
        return categories;
    }

    /// <inheritdoc/>
    public async Task<CaptureTemplateDto?> GetByIdAsync(string templateId)
    {
        await EnsureLoadedAsync();

        if (_idCache.TryGetValue(templateId, out var template))
        {
            _logger.LogTrace("[Templates] Found template by ID: '{Id}'", templateId);
            return template;
        }

        _logger.LogDebug("[Templates] Template ID '{Id}' not found", templateId);
        return null;
    }

    /// <inheritdoc/>
    public async Task ReloadAsync()
    {
        _logger.LogInformation("[Templates] Reloading all templates from '{Path}'", _templatesPath);

        _categoryCache.Clear();
        _idCache.Clear();
        _isLoaded = false;

        await EnsureLoadedAsync();

        _logger.LogInformation("[Templates] Reload complete. {Count} templates loaded across {CategoryCount} categories.",
            TotalTemplateCount, _categoryCache.Count);
    }

    /// <summary>
    /// Ensures templates are loaded, using double-checked locking for thread safety.
    /// </summary>
    private Task EnsureLoadedAsync()
    {
        if (_isLoaded) return Task.CompletedTask;

        lock (_loadLock)
        {
            if (_isLoaded) return Task.CompletedTask;
            LoadTemplatesSync();
            _isLoaded = true;
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Synchronously loads all template files.
    /// </summary>
    private void LoadTemplatesSync()
    {
        _logger.LogDebug("[Templates] Loading templates from '{Path}'", _templatesPath);

        if (!Directory.Exists(_templatesPath))
        {
            _logger.LogWarning("[Templates] Templates directory not found: '{Path}'", _templatesPath);
            return;
        }

        var jsonFiles = Directory.GetFiles(_templatesPath, "*.json");
        _logger.LogDebug("[Templates] Found {Count} JSON files", jsonFiles.Length);

        foreach (var filePath in jsonFiles)
        {
            try
            {
                LoadCategoryFile(filePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Templates] Failed to load file: '{Path}'", filePath);
            }
        }

        _logger.LogInformation("[Templates] Loaded {TemplateCount} templates across {CategoryCount} categories",
            TotalTemplateCount, _categoryCache.Count);
    }

    /// <summary>
    /// Loads a single category JSON file.
    /// </summary>
    private void LoadCategoryFile(string filePath)
    {
        var fileName = Path.GetFileName(filePath);
        _logger.LogTrace("[Templates] Loading file: '{FileName}'", fileName);

        var json = File.ReadAllText(filePath);
        var collection = JsonSerializer.Deserialize<CaptureTemplateCollection>(json, JsonOptions);

        if (collection == null)
        {
            _logger.LogWarning("[Templates] Failed to deserialize: '{FileName}'", fileName);
            return;
        }

        var templates = new List<CaptureTemplateDto>();

        foreach (var jsonTemplate in collection.Templates)
        {
            // Parse CaptureType enum
            if (!Enum.TryParse<CaptureType>(jsonTemplate.Type, out var captureType))
            {
                _logger.LogWarning("[Templates] Invalid type '{Type}' in template '{Id}' (file: {FileName})",
                    jsonTemplate.Type, jsonTemplate.Id, fileName);
                continue;
            }

            // Use template keywords, fallback to category keywords
            var keywords = jsonTemplate.MatchKeywords?.Length > 0
                ? jsonTemplate.MatchKeywords
                : collection.MatchKeywords;

            var dto = new CaptureTemplateDto
            {
                Id = jsonTemplate.Id,
                Type = captureType,
                FragmentContent = jsonTemplate.FragmentContent,
                Source = jsonTemplate.Source,
                MatchKeywords = keywords,
                Quality = jsonTemplate.Quality,
                Tags = jsonTemplate.Tags ?? Array.Empty<string>(),
                Category = collection.Category
            };

            templates.Add(dto);

            if (!_idCache.TryAdd(dto.Id, dto))
            {
                _logger.LogWarning("[Templates] Duplicate template ID '{Id}' in file '{FileName}'",
                    dto.Id, fileName);
            }
        }

        if (!_categoryCache.TryAdd(collection.Category, templates))
        {
            _logger.LogWarning("[Templates] Duplicate category '{Category}' from file '{FileName}'",
                collection.Category, fileName);
        }
        else
        {
            _logger.LogDebug("[Templates] Loaded category '{Category}' with {Count} templates",
                collection.Category, templates.Count);
        }
    }
}
