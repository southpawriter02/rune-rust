namespace RuneAndRust.Core.Interfaces;

/// <summary>
/// Service for generating external Markdown documentation from [GameDocument] attributes.
/// Part of v0.3.11b: The Developer's Handbook.
/// </summary>
public interface IDocGenService
{
    /// <summary>
    /// Generates Markdown documentation files from system entries.
    /// Creates one file per EntryCategory plus an index README.md.
    /// </summary>
    /// <param name="outputPath">The directory path where generated files will be written.</param>
    /// <returns>A task representing the async operation.</returns>
    Task GenerateDocsAsync(string outputPath);
}
