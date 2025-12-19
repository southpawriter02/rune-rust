using System.Text;
using Microsoft.Extensions.Logging;
using RuneAndRust.Core.Interfaces;

namespace RuneAndRust.Engine.Services;

/// <summary>
/// Service for formatting Scavenger's Journal display output.
/// Provides formatted views for journal list, entry details, and unassigned fragments.
/// </summary>
public class JournalService : IJournalService
{
    private readonly ILogger<JournalService> _logger;
    private readonly IDataCaptureService _captureService;
    private readonly IDataCaptureRepository _captureRepository;
    private readonly ICodexEntryRepository _codexRepository;
    private readonly TextRedactor _redactor;

    /// <summary>
    /// Initializes a new instance of the <see cref="JournalService"/> class.
    /// </summary>
    /// <param name="logger">The logger for traceability.</param>
    /// <param name="captureService">The data capture service for completion data.</param>
    /// <param name="captureRepository">The data capture repository for fragment queries.</param>
    /// <param name="codexRepository">The codex entry repository for entry lookups.</param>
    public JournalService(
        ILogger<JournalService> logger,
        IDataCaptureService captureService,
        IDataCaptureRepository captureRepository,
        ICodexEntryRepository codexRepository)
    {
        _logger = logger;
        _captureService = captureService;
        _captureRepository = captureRepository;
        _codexRepository = codexRepository;
        _redactor = new TextRedactor();
    }

    /// <inheritdoc/>
    public async Task<string> FormatJournalListAsync(Guid characterId)
    {
        _logger.LogDebug("Formatting journal list for Character {CharacterId}", characterId);

        var entries = await _captureService.GetDiscoveredEntriesAsync(characterId);
        var entryList = entries.ToList();

        if (!entryList.Any())
        {
            _logger.LogDebug("No discovered entries for Character {CharacterId}", characterId);
            return "[yellow]═══ SCAVENGER'S JOURNAL ═══[/]\n\nNo discoveries recorded yet.\nExamine objects and search containers to gather knowledge.";
        }

        var sb = new StringBuilder();
        sb.AppendLine("[yellow]═══ SCAVENGER'S JOURNAL ═══[/]");
        sb.AppendLine();

        // Group by category and sort by category enum value
        var grouped = entryList
            .GroupBy(e => e.Entry.Category)
            .OrderBy(g => g.Key);

        foreach (var group in grouped)
        {
            _logger.LogTrace("Processing category {Category} with {Count} entries", group.Key, group.Count());

            sb.AppendLine($"[cyan]── {group.Key} ──[/]");

            foreach (var (entry, pct) in group.OrderBy(e => e.Entry.Title))
            {
                var statusIcon = pct >= 100 ? "[green]★[/]" : "[grey]●[/]";
                var pctColor = pct >= 100 ? "green" : pct >= 50 ? "yellow" : "grey";
                sb.AppendLine($"  {statusIcon} {entry.Title} [{pctColor}]({pct}%)[/]");
            }
            sb.AppendLine();
        }

        sb.AppendLine("[grey]Use 'codex <name>' to view entry details.[/]");

        _logger.LogDebug("Formatted {Count} entries for journal list", entryList.Count);
        return sb.ToString();
    }

    /// <inheritdoc/>
    public async Task<string> FormatEntryDetailAsync(Guid characterId, string entryTitle)
    {
        _logger.LogDebug("Formatting entry detail for '{EntryTitle}'", entryTitle);

        var entry = await _codexRepository.GetByTitleAsync(entryTitle);
        if (entry == null)
        {
            _logger.LogDebug("Entry '{EntryTitle}' not found", entryTitle);
            return $"[red]No entry found matching '{entryTitle}'.[/]";
        }

        var pct = await _captureService.GetCompletionPercentageAsync(entry.Id, characterId);
        var thresholds = await _captureService.GetUnlockedThresholdsAsync(entry.Id, characterId);
        var thresholdList = thresholds.ToList();

        _logger.LogTrace("Entry {EntryTitle} has {Pct}% completion and {ThresholdCount} unlocked thresholds",
            entry.Title, pct, thresholdList.Count);

        var sb = new StringBuilder();
        sb.AppendLine($"[yellow]═══ {entry.Title.ToUpperInvariant()} ═══[/]");
        sb.AppendLine($"[cyan]Category:[/] {entry.Category}");
        sb.AppendLine($"[cyan]Completion:[/] {pct}%");
        sb.AppendLine();

        // Show redacted content
        var displayText = _redactor.RedactText(entry.FullText, pct);
        sb.AppendLine(displayText);

        // Show unlocked threshold tags if any
        if (thresholdList.Any())
        {
            sb.AppendLine();
            sb.AppendLine("[cyan]Discoveries:[/]");
            foreach (var tag in thresholdList)
            {
                sb.AppendLine($"  [green]✓[/] {FormatTag(tag)}");
            }
        }

        _logger.LogDebug("Formatted entry detail for '{EntryTitle}' at {Pct}%", entryTitle, pct);
        return sb.ToString();
    }

    /// <inheritdoc/>
    public async Task<string> FormatUnassignedCapturesAsync(Guid characterId)
    {
        _logger.LogDebug("Formatting unassigned captures for Character {CharacterId}", characterId);

        var unassigned = await _captureRepository.GetUnassignedAsync(characterId);
        var captureList = unassigned.ToList();

        if (!captureList.Any())
        {
            _logger.LogDebug("No unassigned captures for Character {CharacterId}", characterId);
            return "[grey]No unassigned fragments.[/]";
        }

        var sb = new StringBuilder();
        sb.AppendLine("[yellow]── Unassigned Fragments ──[/]");
        sb.AppendLine("[grey]These fragments don't match any known entries yet.[/]");
        sb.AppendLine();

        foreach (var capture in captureList.OrderByDescending(c => c.DiscoveredAt))
        {
            _logger.LogTrace("Formatting unassigned capture {CaptureId}: {CaptureType}", capture.Id, capture.Type);

            sb.AppendLine($"  [cyan]{capture.Type}[/] - {capture.Source}");
            sb.AppendLine($"    \"{Truncate(capture.FragmentContent, 60)}\"");
        }

        _logger.LogDebug("Formatted {Count} unassigned captures", captureList.Count);
        return sb.ToString();
    }

    #region Private Methods

    /// <summary>
    /// Converts a threshold tag from SCREAMING_SNAKE_CASE to Title Case.
    /// </summary>
    /// <param name="tag">The tag to format (e.g., "WEAKNESS_REVEALED").</param>
    /// <returns>Formatted tag (e.g., "Weakness Revealed").</returns>
    private static string FormatTag(string tag)
    {
        if (string.IsNullOrEmpty(tag))
        {
            return tag;
        }

        return string.Join(" ", tag.Split('_').Select(word =>
            word.Length > 0
                ? char.ToUpper(word[0]) + word[1..].ToLower()
                : word));
    }

    /// <summary>
    /// Truncates text to a maximum length with ellipsis.
    /// </summary>
    /// <param name="text">The text to truncate.</param>
    /// <param name="maxLength">Maximum length including ellipsis.</param>
    /// <returns>Truncated text with "..." if it exceeded maxLength.</returns>
    private static string Truncate(string text, int maxLength)
    {
        if (string.IsNullOrEmpty(text) || text.Length <= maxLength)
        {
            return text;
        }

        return text[..(maxLength - 3)] + "...";
    }

    #endregion
}
