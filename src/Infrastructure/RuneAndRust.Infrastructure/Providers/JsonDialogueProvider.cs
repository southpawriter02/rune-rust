using System.Text.Json;
using Microsoft.Extensions.Logging;
using RuneAndRust.Application.DTOs;
using RuneAndRust.Application.Interfaces;

namespace RuneAndRust.Infrastructure.Providers;

/// <summary>
/// JSON-based provider for dialogue trees (v0.21.0a — Narrative Infrastructure).
/// Loads dialogue trees from config/dialogues.json keyed by root dialogue ID.
/// </summary>
public class JsonDialogueProvider : IDialogueProvider
{
    // ═══════════════════════════════════════════════════════════════
    // FIELDS
    // ═══════════════════════════════════════════════════════════════

    private readonly Dictionary<string, List<DialogueNodeDto>> _dialogueTrees;
    private readonly ILogger<JsonDialogueProvider> _logger;
    private readonly string _configPath;

    // ═══════════════════════════════════════════════════════════════
    // CONSTRUCTOR
    // ═══════════════════════════════════════════════════════════════

    public JsonDialogueProvider(string configPath, ILogger<JsonDialogueProvider> logger)
    {
        _configPath = configPath ?? throw new ArgumentNullException(nameof(configPath));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _dialogueTrees = new Dictionary<string, List<DialogueNodeDto>>(StringComparer.OrdinalIgnoreCase);

        _logger.LogDebug("Initializing dialogue provider from: {ConfigPath}", configPath);
        LoadDialogues();
        _logger.LogInformation(
            "Dialogue provider initialized: {TreeCount} dialogue trees, {TotalNodes} total nodes",
            _dialogueTrees.Count,
            _dialogueTrees.Values.Sum(t => t.Count));
    }

    // ═══════════════════════════════════════════════════════════════
    // INTERFACE IMPLEMENTATION
    // ═══════════════════════════════════════════════════════════════

    /// <inheritdoc />
    public IReadOnlyList<DialogueNodeDto>? GetDialogueTree(string rootDialogueId)
    {
        ArgumentNullException.ThrowIfNull(rootDialogueId);

        if (_dialogueTrees.TryGetValue(rootDialogueId, out var tree))
            return tree;

        _logger.LogDebug("Dialogue tree not found: {RootId}", rootDialogueId);
        return null;
    }

    /// <inheritdoc />
    public DialogueNodeDto? GetNode(string rootDialogueId, string nodeId)
    {
        ArgumentNullException.ThrowIfNull(rootDialogueId);
        ArgumentNullException.ThrowIfNull(nodeId);

        if (!_dialogueTrees.TryGetValue(rootDialogueId, out var tree))
        {
            _logger.LogDebug("Dialogue tree not found: {RootId}", rootDialogueId);
            return null;
        }

        var node = tree.FirstOrDefault(n =>
            n.Id.Equals(nodeId, StringComparison.OrdinalIgnoreCase));

        if (node == null)
        {
            _logger.LogWarning(
                "Dialogue node '{NodeId}' not found in tree '{RootId}'",
                nodeId, rootDialogueId);
        }

        return node;
    }

    /// <inheritdoc />
    public bool HasDialogue(string rootDialogueId)
    {
        ArgumentNullException.ThrowIfNull(rootDialogueId);
        return _dialogueTrees.ContainsKey(rootDialogueId);
    }

    /// <inheritdoc />
    public IReadOnlyList<string> GetAllDialogueIds()
    {
        return _dialogueTrees.Keys.ToList();
    }

    /// <inheritdoc />
    public void Reload()
    {
        _logger.LogInformation("Reloading dialogue trees from {ConfigPath}", _configPath);

        _dialogueTrees.Clear();
        LoadDialogues();

        _logger.LogInformation(
            "Dialogue trees reloaded: {TreeCount} trees",
            _dialogueTrees.Count);
    }

    // ═══════════════════════════════════════════════════════════════
    // PRIVATE METHODS
    // ═══════════════════════════════════════════════════════════════

    private void LoadDialogues()
    {
        if (!File.Exists(_configPath))
        {
            _logger.LogWarning(
                "Dialogue configuration file not found: {Path}. No dialogues will be available.",
                _configPath);
            return;
        }

        var json = File.ReadAllText(_configPath);

        _logger.LogDebug("Read {Length} bytes from dialogue configuration file", json.Length);

        var config = JsonSerializer.Deserialize<DialoguesConfigDto>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (config?.Dialogues is null || config.Dialogues.Count == 0)
        {
            _logger.LogWarning("Dialogue configuration is empty or contains no dialogue trees");
            return;
        }

        _logger.LogDebug("Parsing {Count} dialogue trees", config.Dialogues.Count);

        foreach (var (rootId, nodes) in config.Dialogues)
        {
            if (string.IsNullOrWhiteSpace(rootId))
            {
                _logger.LogWarning("Skipping dialogue tree with empty root ID");
                continue;
            }

            if (nodes == null || nodes.Count == 0)
            {
                _logger.LogWarning("Skipping empty dialogue tree: {RootId}", rootId);
                continue;
            }

            _dialogueTrees[rootId] = nodes;

            _logger.LogDebug(
                "Loaded dialogue tree: {RootId} - {NodeCount} nodes",
                rootId, nodes.Count);
        }
    }

    // ═══════════════════════════════════════════════════════════════
    // INTERNAL DTO CLASSES
    // ═══════════════════════════════════════════════════════════════

    private sealed class DialoguesConfigDto
    {
        public Dictionary<string, List<DialogueNodeDto>>? Dialogues { get; set; }
    }
}
