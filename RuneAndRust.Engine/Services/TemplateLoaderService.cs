using System.Text.Json;
using Microsoft.Extensions.Logging;
using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Interfaces;

namespace RuneAndRust.Engine.Services;

/// <summary>
/// Service for loading room templates and biome definitions from JSON files into the database.
/// Part of the Dynamic Room Engine (v0.4.0).
/// </summary>
public class TemplateLoaderService : ITemplateLoaderService
{
    private readonly IRoomTemplateRepository _roomTemplateRepo;
    private readonly IBiomeDefinitionRepository _biomeDefRepo;
    private readonly IRepository<BiomeElement> _biomeElementRepo;
    private readonly ILogger<TemplateLoaderService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="TemplateLoaderService"/> class.
    /// </summary>
    /// <param name="roomTemplateRepo">Repository for room templates.</param>
    /// <param name="biomeDefRepo">Repository for biome definitions.</param>
    /// <param name="biomeElementRepo">Repository for biome elements.</param>
    /// <param name="logger">Logger instance.</param>
    public TemplateLoaderService(
        IRoomTemplateRepository roomTemplateRepo,
        IBiomeDefinitionRepository biomeDefRepo,
        IRepository<BiomeElement> biomeElementRepo,
        ILogger<TemplateLoaderService> logger)
    {
        _roomTemplateRepo = roomTemplateRepo;
        _biomeDefRepo = biomeDefRepo;
        _biomeElementRepo = biomeElementRepo;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task LoadAllTemplatesAsync()
    {
        _logger.LogInformation("[TemplateLoader] Loading all templates from data directory");

        var baseDir = AppDomain.CurrentDomain.BaseDirectory;
        var templatesPath = Path.Combine(baseDir, "data", "templates");
        var biomesPath = Path.Combine(baseDir, "data", "biomes");

        if (!Directory.Exists(templatesPath))
        {
            _logger.LogWarning("[TemplateLoader] Templates directory not found: {Path}", templatesPath);
            return;
        }

        if (!Directory.Exists(biomesPath))
        {
            _logger.LogWarning("[TemplateLoader] Biomes directory not found: {Path}", biomesPath);
            return;
        }

        // Load room templates
        await LoadRoomTemplatesFromDirectoryAsync(templatesPath);

        // Load biome definitions (currently just the_roots.json)
        var theRootsBiome = Path.Combine(biomesPath, "the_roots.json");
        if (File.Exists(theRootsBiome))
        {
            await LoadBiomeDefinitionAsync(theRootsBiome);
        }
        else
        {
            _logger.LogWarning("[TemplateLoader] the_roots.json not found in: {Path}", biomesPath);
        }

        _logger.LogInformation("[TemplateLoader] Template loading complete");
    }

    /// <inheritdoc/>
    public async Task LoadRoomTemplatesFromDirectoryAsync(string directoryPath)
    {
        _logger.LogInformation("[TemplateLoader] Scanning directory for room templates: {Path}", directoryPath);

        var jsonFiles = Directory.GetFiles(directoryPath, "*.json", SearchOption.TopDirectoryOnly);
        var loadedCount = 0;
        var updatedCount = 0;

        foreach (var filePath in jsonFiles)
        {
            try
            {
                var json = await File.ReadAllTextAsync(filePath);
                var template = JsonSerializer.Deserialize<RoomTemplate>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (template == null || string.IsNullOrWhiteSpace(template.TemplateId))
                {
                    _logger.LogWarning("[TemplateLoader] Invalid template JSON in file: {FileName}", Path.GetFileName(filePath));
                    continue;
                }

                // Upsert template (insert or update)
                var existing = await _roomTemplateRepo.GetByTemplateIdAsync(template.TemplateId);

                if (existing != null)
                {
                    updatedCount++;
                    _logger.LogDebug("[TemplateLoader] Updating existing template: {TemplateId}", template.TemplateId);
                }
                else
                {
                    loadedCount++;
                    _logger.LogDebug("[TemplateLoader] Loading new template: {TemplateId}", template.TemplateId);
                }

                await _roomTemplateRepo.UpsertAsync(template);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[TemplateLoader] Failed to load template from file: {FileName}", Path.GetFileName(filePath));
            }
        }

        await _roomTemplateRepo.SaveChangesAsync();

        _logger.LogInformation(
            "[TemplateLoader] Room template loading complete. Loaded: {Loaded}, Updated: {Updated}, Total files: {Total}",
            loadedCount, updatedCount, jsonFiles.Length);
    }

    /// <inheritdoc/>
    public async Task LoadBiomeDefinitionAsync(string filePath)
    {
        _logger.LogInformation("[TemplateLoader] Loading biome definition from: {Path}", filePath);

        try
        {
            var json = await File.ReadAllTextAsync(filePath);

            // Deserialize into DTO that matches JSON structure
            var biomeDto = JsonSerializer.Deserialize<BiomeDefinitionDto>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (biomeDto == null || string.IsNullOrWhiteSpace(biomeDto.BiomeId))
            {
                _logger.LogWarning("[TemplateLoader] Invalid biome JSON in file: {FileName}", Path.GetFileName(filePath));
                return;
            }

            // Convert DTO to entity
            var biomeDefinition = new BiomeDefinition
            {
                BiomeId = biomeDto.BiomeId,
                Name = biomeDto.Name,
                Description = biomeDto.Description,
                AvailableTemplates = biomeDto.AvailableTemplates,
                DescriptorCategories = biomeDto.DescriptorCategories,
                MinRoomCount = biomeDto.MinRoomCount,
                MaxRoomCount = biomeDto.MaxRoomCount,
                BranchingProbability = biomeDto.BranchingProbability,
                SecretRoomProbability = biomeDto.SecretRoomProbability
            };

            // Upsert biome definition (insert or update)
            var existing = await _biomeDefRepo.GetByBiomeIdAsync(biomeDto.BiomeId);

            if (existing != null)
            {
                _logger.LogInformation("[TemplateLoader] Updating existing biome: {BiomeId}", biomeDto.BiomeId);
            }
            else
            {
                _logger.LogInformation("[TemplateLoader] Loading new biome: {BiomeId}", biomeDto.BiomeId);
            }

            await _biomeDefRepo.UpsertAsync(biomeDefinition);
            await _biomeDefRepo.SaveChangesAsync();

            // Load biome elements
            await LoadBiomeElementsAsync(biomeDto);

            // Validate template references
            await ValidateTemplateReferencesAsync(biomeDto.BiomeId, biomeDto.AvailableTemplates);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[TemplateLoader] Failed to load biome definition from: {FileName}", Path.GetFileName(filePath));
        }
    }

    /// <summary>
    /// Loads biome elements from the DTO and persists them to the database.
    /// </summary>
    private async Task LoadBiomeElementsAsync(BiomeDefinitionDto biomeDto)
    {
        if (biomeDto.Elements?.Elements == null || biomeDto.Elements.Elements.Count == 0)
        {
            _logger.LogWarning("[TemplateLoader] No elements found for biome: {BiomeId}", biomeDto.BiomeId);
            return;
        }

        _logger.LogInformation("[TemplateLoader] Loading {Count} elements for biome: {BiomeId}",
            biomeDto.Elements.Elements.Count, biomeDto.BiomeId);

        // Remove existing elements for this biome (for clean upsert)
        var existingElements = await _biomeElementRepo.GetAllAsync();
        var elementsToRemove = existingElements.Where(e => e.BiomeId == biomeDto.BiomeId).ToList();

        foreach (var element in elementsToRemove)
        {
            await _biomeElementRepo.DeleteAsync(element.Id);
        }

        // Insert new elements
        foreach (var elementDto in biomeDto.Elements.Elements)
        {
            var element = new BiomeElement
            {
                BiomeId = biomeDto.BiomeId,
                ElementName = elementDto.ElementName,
                ElementType = elementDto.ElementType,
                Weight = elementDto.Weight,
                SpawnCost = elementDto.SpawnCost,
                AssociatedDataId = elementDto.AssociatedDataId,
                SpawnRules = elementDto.SpawnRules ?? new ElementSpawnRules()
            };

            await _biomeElementRepo.AddAsync(element);
        }

        await _biomeElementRepo.SaveChangesAsync();

        _logger.LogInformation("[TemplateLoader] Loaded {Count} elements for biome: {BiomeId}",
            biomeDto.Elements.Elements.Count, biomeDto.BiomeId);
    }

    /// <summary>
    /// Validates that all AvailableTemplates in a biome reference existing RoomTemplates.
    /// </summary>
    private async Task ValidateTemplateReferencesAsync(string biomeId, List<string> availableTemplates)
    {
        var allTemplates = await _roomTemplateRepo.GetAllAsync();
        var existingTemplateIds = allTemplates.Select(t => t.TemplateId).ToHashSet();

        var missingTemplates = availableTemplates
            .Where(t => !existingTemplateIds.Contains(t))
            .ToList();

        if (missingTemplates.Any())
        {
            _logger.LogWarning(
                "[TemplateLoader] Biome '{BiomeId}' references {Count} missing templates: {Missing}",
                biomeId, missingTemplates.Count, string.Join(", ", missingTemplates));
        }
        else
        {
            _logger.LogInformation(
                "[TemplateLoader] Biome '{BiomeId}' template references validated ({Count} templates)",
                biomeId, availableTemplates.Count);
        }
    }
}

/// <summary>
/// DTO for deserializing biome JSON (matches JSON structure with nested Elements object).
/// </summary>
internal class BiomeDefinitionDto
{
    public string BiomeId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> AvailableTemplates { get; set; } = new();
    public BiomeDescriptorCategories DescriptorCategories { get; set; } = new();
    public int MinRoomCount { get; set; }
    public int MaxRoomCount { get; set; }
    public float BranchingProbability { get; set; }
    public float SecretRoomProbability { get; set; }
    public BiomeElementsWrapper? Elements { get; set; }
}

/// <summary>
/// Wrapper class for the nested "Elements" object in biome JSON.
/// </summary>
internal class BiomeElementsWrapper
{
    public List<BiomeElementDto> Elements { get; set; } = new();
}

/// <summary>
/// DTO for deserializing biome element JSON.
/// </summary>
internal class BiomeElementDto
{
    public string ElementName { get; set; } = string.Empty;
    public string ElementType { get; set; } = string.Empty;
    public float Weight { get; set; }
    public int SpawnCost { get; set; }
    public string AssociatedDataId { get; set; } = string.Empty;
    public ElementSpawnRules? SpawnRules { get; set; }
}
