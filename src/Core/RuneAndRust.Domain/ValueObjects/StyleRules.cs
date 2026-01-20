namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Construction rules for an architectural style.
/// </summary>
public class StyleRules
{
    /// <summary>
    /// Gets the minimum room size for this style.
    /// </summary>
    public int MinRoomSize { get; init; } = 3;

    /// <summary>
    /// Gets the maximum room size for this style.
    /// </summary>
    public int MaxRoomSize { get; init; } = 10;

    /// <summary>
    /// Gets whether this style prefers regular geometric shapes.
    /// </summary>
    public bool PrefersRegularShapes { get; init; } = true;

    /// <summary>
    /// Gets the preferred room shapes for this style.
    /// </summary>
    public IReadOnlyList<string> PreferredShapes { get; init; } = ["rectangular", "square"];

    /// <summary>
    /// Gets common features for this style.
    /// </summary>
    public IReadOnlyList<string> CommonFeatures { get; init; } = [];

    /// <summary>
    /// Gets the base weight for style selection.
    /// </summary>
    public int BaseWeight { get; init; } = 100;

    /// <summary>
    /// Default rules.
    /// </summary>
    public static StyleRules Default => new();

    /// <summary>
    /// Creates rules for cramped tunnel style.
    /// </summary>
    public static StyleRules Cramped => new()
    {
        MinRoomSize = 2,
        MaxRoomSize = 5,
        PrefersRegularShapes = false,
        PreferredShapes = ["irregular", "narrow"],
        CommonFeatures = ["low-ceiling", "tight-squeeze"],
        BaseWeight = 80
    };

    /// <summary>
    /// Creates rules for grand hall style.
    /// </summary>
    public static StyleRules Grand => new()
    {
        MinRoomSize = 6,
        MaxRoomSize = 15,
        PrefersRegularShapes = true,
        PreferredShapes = ["rectangular", "circular", "octagonal"],
        CommonFeatures = ["pillars", "raised-dais", "gallery"],
        BaseWeight = 50
    };

    /// <summary>
    /// Checks if a room size is valid for this style.
    /// </summary>
    public bool IsValidSize(int size) => size >= MinRoomSize && size <= MaxRoomSize;
}
