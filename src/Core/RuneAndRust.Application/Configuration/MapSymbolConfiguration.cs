namespace RuneAndRust.Application.Configuration;

/// <summary>
/// Configuration for map display symbols.
/// </summary>
public class MapSymbolConfiguration
{
    /// <summary>
    /// Player position symbol.
    /// </summary>
    public char PlayerSymbol { get; init; } = '@';

    /// <summary>
    /// Unexplored room symbol.
    /// </summary>
    public char UnexploredSymbol { get; init; } = '?';

    /// <summary>
    /// Room type symbols keyed by room type name.
    /// </summary>
    public IReadOnlyDictionary<string, char> RoomTypeSymbols { get; init; } =
        new Dictionary<string, char>
        {
            ["standard"] = '#',
            ["treasure"] = '$',
            ["trap"] = '!',
            ["boss"] = 'B',
            ["safe"] = '+',
            ["shrine"] = '*'
        };

    /// <summary>
    /// Connection symbols keyed by direction.
    /// </summary>
    public IReadOnlyDictionary<string, char> ConnectionSymbols { get; init; } =
        new Dictionary<string, char>
        {
            ["horizontal"] = '─',
            ["vertical"] = '│',
            ["stairs_up"] = '↑',
            ["stairs_down"] = '↓',
            ["ladder"] = '≡'
        };
}
