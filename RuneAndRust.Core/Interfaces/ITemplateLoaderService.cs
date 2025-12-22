namespace RuneAndRust.Core.Interfaces;

/// <summary>
/// Service for loading room templates and biome definitions from JSON files into the database.
/// Part of the Dynamic Room Engine (v0.4.0).
/// </summary>
public interface ITemplateLoaderService
{
    /// <summary>
    /// Loads all room templates and biome definitions from the data directory.
    /// Scans data/templates/ for room template JSONs and data/biomes/ for biome JSONs.
    /// Performs upsert operations (updates if TemplateId/BiomeId exists, inserts if new).
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task LoadAllTemplatesAsync();

    /// <summary>
    /// Loads room template JSON files from a specified directory.
    /// Each JSON file is deserialized into a RoomTemplate entity and persisted to the database.
    /// </summary>
    /// <param name="directoryPath">The absolute path to the directory containing template JSON files.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task LoadRoomTemplatesFromDirectoryAsync(string directoryPath);

    /// <summary>
    /// Loads a biome definition JSON file from a specified path.
    /// Deserializes the biome data including nested BiomeElements array.
    /// </summary>
    /// <param name="filePath">The absolute path to the biome JSON file.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task LoadBiomeDefinitionAsync(string filePath);
}
