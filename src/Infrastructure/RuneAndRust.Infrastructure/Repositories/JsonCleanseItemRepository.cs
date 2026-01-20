using System.Text.Json;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Infrastructure.Repositories;

/// <summary>
/// JSON-based repository for cleanse item definitions.
/// </summary>
public class JsonCleanseItemRepository : ICleanseItemRepository
{
    private readonly Dictionary<string, CleanseItem> _items = new();
    private readonly string _configPath;
    private bool _loaded;

    public JsonCleanseItemRepository(string configPath)
    {
        _configPath = configPath ?? throw new ArgumentNullException(nameof(configPath));
    }

    public CleanseItem? GetById(string itemId)
    {
        EnsureLoaded();
        return _items.TryGetValue(itemId.ToLowerInvariant(), out var item) ? item : null;
    }

    public bool IsCleanseItem(string itemId)
    {
        EnsureLoaded();
        return _items.ContainsKey(itemId.ToLowerInvariant());
    }

    public IReadOnlyList<CleanseItem> GetAll()
    {
        EnsureLoaded();
        return _items.Values.ToList().AsReadOnly();
    }

    private void EnsureLoaded()
    {
        if (_loaded) return;

        if (!File.Exists(_configPath))
        {
            _loaded = true;
            return;
        }

        var json = File.ReadAllText(_configPath);
        var config = JsonSerializer.Deserialize<CleanseItemsConfig>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (config?.Items == null)
        {
            _loaded = true;
            return;
        }

        foreach (var dto in config.Items)
        {
            var cleanseType = Enum.Parse<CleanseType>(dto.CleanseType, ignoreCase: true);
            var item = new CleanseItem(
                dto.Id.ToLowerInvariant(),
                dto.Name,
                dto.Description ?? string.Empty,
                cleanseType,
                dto.SpecificEffect);

            _items[item.Id] = item;
        }

        _loaded = true;
    }

    private class CleanseItemsConfig
    {
        public List<CleanseItemDto>? Items { get; set; }
    }

    private class CleanseItemDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string CleanseType { get; set; } = "Specific";
        public string? SpecificEffect { get; set; }
    }
}
