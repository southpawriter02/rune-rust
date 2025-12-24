using System.Text;
using Microsoft.Extensions.Logging;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Interfaces;

namespace RuneAndRust.Engine.Services;

/// <summary>
/// Generates external Markdown documentation from [GameDocument] attributes.
/// Part of v0.3.11b: The Developer's Handbook.
/// </summary>
/// <remarks>
/// <para>
/// This service leverages the LibraryService reflection work to export
/// game data into static Markdown files. The generated documentation
/// stays 1:1 synchronized with the codebase.
/// </para>
/// <para>
/// Output format: One file per EntryCategory plus a README.md index.
/// </para>
/// </remarks>
public class DocGenService : IDocGenService
{
    private readonly ILogger<DocGenService> _logger;
    private readonly ILibraryService _libraryService;

    /// <summary>
    /// Initializes a new instance of the <see cref="DocGenService"/> class.
    /// </summary>
    /// <param name="logger">The logger for traceability.</param>
    /// <param name="libraryService">The library service for retrieving system entries.</param>
    public DocGenService(ILogger<DocGenService> logger, ILibraryService libraryService)
    {
        _logger = logger;
        _libraryService = libraryService;
    }

    /// <inheritdoc/>
    public async Task GenerateDocsAsync(string outputPath)
    {
        _logger.LogInformation("[DocGen] Starting generation to {Path}", outputPath);

        var entries = _libraryService.GetSystemEntries().ToList();

        if (entries.Count == 0)
        {
            _logger.LogWarning("[DocGen] No system entries found. No files generated.");
            return;
        }

        // Ensure output directory exists
        Directory.CreateDirectory(outputPath);

        var grouped = entries.GroupBy(e => e.Category).OrderBy(g => g.Key);
        var categoryFiles = new List<(EntryCategory Category, string Filename, int Count)>();

        foreach (var group in grouped)
        {
            var filename = $"{group.Key.ToString().ToLowerInvariant()}.md";
            var filepath = Path.Combine(outputPath, filename);

            try
            {
                var content = GenerateCategoryMarkdown(group.Key, group.ToList());
                await File.WriteAllTextAsync(filepath, content);

                categoryFiles.Add((group.Key, filename, group.Count()));
                _logger.LogInformation("[DocGen] Wrote {Filename} ({Count} entries)", filename, group.Count());
            }
            catch (Exception ex)
            {
                _logger.LogError("[DocGen] Failed to write file {Filename}: {Message}", filename, ex.Message);
            }
        }

        // Generate README index
        try
        {
            var indexPath = Path.Combine(outputPath, "README.md");
            var indexContent = GenerateIndexMarkdown(categoryFiles, entries.Count);
            await File.WriteAllTextAsync(indexPath, indexContent);
            _logger.LogInformation("[DocGen] Wrote README.md (index)");
        }
        catch (Exception ex)
        {
            _logger.LogError("[DocGen] Failed to write README.md: {Message}", ex.Message);
        }

        _logger.LogInformation("[DocGen] Completed successfully. Generated {FileCount} files with {EntryCount} total entries.",
            categoryFiles.Count + 1, entries.Count);
    }

    /// <summary>
    /// Generates Markdown content for a single category.
    /// </summary>
    /// <param name="category">The entry category.</param>
    /// <param name="entries">The entries in this category.</param>
    /// <returns>The Markdown content as a string.</returns>
    private static string GenerateCategoryMarkdown(EntryCategory category, List<Core.Entities.CodexEntry> entries)
    {
        var sb = new StringBuilder();

        // Header
        sb.AppendLine($"# Auto-Generated: {category}");
        sb.AppendLine();
        sb.AppendLine("> Generated from `[GameDocument]` attributes via reflection.");
        sb.AppendLine($"> Last generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
        sb.AppendLine();

        // Table header
        sb.AppendLine("| Title | Description |");
        sb.AppendLine("|-------|-------------|");

        // Table rows
        foreach (var entry in entries.OrderBy(e => e.Title))
        {
            // Escape pipes and newlines to prevent table breakage
            var safeDescription = EscapeMarkdownTableCell(entry.FullText);
            sb.AppendLine($"| **{EscapeMarkdownTableCell(entry.Title)}** | {safeDescription} |");
        }

        sb.AppendLine();
        sb.AppendLine("---");
        sb.AppendLine();
        sb.AppendLine($"*{entries.Count} entries in this category*");

        return sb.ToString();
    }

    /// <summary>
    /// Generates the README.md index linking all category files.
    /// </summary>
    /// <param name="categoryFiles">List of generated category files.</param>
    /// <param name="totalEntries">Total number of entries across all categories.</param>
    /// <returns>The Markdown content as a string.</returns>
    private static string GenerateIndexMarkdown(
        List<(EntryCategory Category, string Filename, int Count)> categoryFiles,
        int totalEntries)
    {
        var sb = new StringBuilder();

        sb.AppendLine("# Generated Documentation");
        sb.AppendLine();
        sb.AppendLine("> Auto-generated from `[GameDocument]` attributes in the RuneAndRust.Core assembly.");
        sb.AppendLine($"> Last generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
        sb.AppendLine();
        sb.AppendLine("## Overview");
        sb.AppendLine();
        sb.AppendLine($"This directory contains **{totalEntries}** system-generated entries across **{categoryFiles.Count}** categories.");
        sb.AppendLine();
        sb.AppendLine("## Categories");
        sb.AppendLine();

        foreach (var (category, filename, count) in categoryFiles.OrderBy(c => c.Category))
        {
            sb.AppendLine($"- [{category}]({filename}) ({count} entries)");
        }

        sb.AppendLine();
        sb.AppendLine("---");
        sb.AppendLine();
        sb.AppendLine("## Usage");
        sb.AppendLine();
        sb.AppendLine("To regenerate this documentation, run:");
        sb.AppendLine();
        sb.AppendLine("```bash");
        sb.AppendLine("dotnet run --project RuneAndRust.Terminal -- --docgen");
        sb.AppendLine("```");
        sb.AppendLine();
        sb.AppendLine("---");
        sb.AppendLine();
        sb.AppendLine("*Generated by The Architect - Rune & Rust v0.3.11b*");

        return sb.ToString();
    }

    /// <summary>
    /// Escapes characters that would break Markdown table cells.
    /// </summary>
    /// <param name="text">The text to escape.</param>
    /// <returns>The escaped text safe for table cells.</returns>
    private static string EscapeMarkdownTableCell(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        return text
            .Replace("|", "\\|")
            .Replace("\r\n", " ")
            .Replace("\n", " ")
            .Replace("\r", " ");
    }
}
