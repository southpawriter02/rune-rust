namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Defines ASCII art for a room type.
/// </summary>
/// <param name="RoomId">The room type ID this art is for.</param>
/// <param name="ArtLines">Lines of ASCII art.</param>
/// <param name="Legend">Symbol to description mapping.</param>
/// <param name="SymbolColors">Optional symbol to color mapping.</param>
/// <param name="ShowLegend">Whether to display the legend.</param>
public record RoomArtDefinition(
    string RoomId,
    IReadOnlyList<string> ArtLines,
    IReadOnlyDictionary<char, string> Legend,
    IReadOnlyDictionary<char, ConsoleColor>? SymbolColors = null,
    bool ShowLegend = true)
{
    /// <summary>
    /// Gets the maximum width of the art.
    /// </summary>
    public int MaxWidth => ArtLines.Count > 0 ? ArtLines.Max(line => line.Length) : 0;
    
    /// <summary>
    /// Gets the height of the art in lines.
    /// </summary>
    public int Height => ArtLines.Count;
    
    /// <summary>
    /// Gets all unique symbols used in the art.
    /// </summary>
    public IReadOnlySet<char> UsedSymbols => 
        ArtLines.SelectMany(line => line).ToHashSet();
    
    /// <summary>
    /// Creates an empty art definition for a room ID.
    /// </summary>
    public static RoomArtDefinition Empty(string roomId) => new(
        roomId,
        Array.Empty<string>(),
        new Dictionary<char, string>());
}
