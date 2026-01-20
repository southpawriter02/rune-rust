namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Configuration for a pattern matching puzzle.
/// </summary>
/// <remarks>
/// <para>
/// Pattern puzzles require matching or reproducing a visual pattern on a grid.
/// Features include:
/// </para>
/// <list type="bullet">
///   <item><description>Grid-based pattern representation</description></item>
///   <item><description>Optional acceptance of rotations (90°/180°/270°)</description></item>
///   <item><description>Optional acceptance of reflections (horizontal/vertical)</description></item>
///   <item><description>Configurable display duration</description></item>
/// </list>
/// </remarks>
public class PatternPuzzle
{
    // ===== Core Properties =====

    /// <summary>
    /// Gets the pattern to match (as string representation).
    /// </summary>
    public string TargetPattern { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the pattern grid width.
    /// </summary>
    public int GridWidth { get; private set; }

    /// <summary>
    /// Gets the pattern grid height.
    /// </summary>
    public int GridHeight { get; private set; }

    /// <summary>
    /// Gets the pattern elements/symbols available.
    /// </summary>
    public IReadOnlyList<string> PatternElements { get; private set; } = Array.Empty<string>();

    /// <summary>
    /// Gets whether the pattern is shown before solving.
    /// </summary>
    public bool ShowTargetPattern { get; private set; } = true;

    /// <summary>
    /// Gets how long the pattern is displayed (turns, 0 = always visible).
    /// </summary>
    public int PatternDisplayDuration { get; private set; }

    /// <summary>
    /// Gets whether rotations of the pattern are accepted.
    /// </summary>
    public bool AcceptRotations { get; private set; }

    /// <summary>
    /// Gets whether reflections of the pattern are accepted.
    /// </summary>
    public bool AcceptReflections { get; private set; }

    // ===== Computed Properties =====

    /// <summary>
    /// Gets the total cells in the pattern grid.
    /// </summary>
    public int TotalCells => GridWidth * GridHeight;

    // ===== Constructors =====

    /// <summary>
    /// Private parameterless constructor for EF Core.
    /// </summary>
    private PatternPuzzle() { }

    // ===== Factory Methods =====

    /// <summary>
    /// Creates a pattern puzzle configuration.
    /// </summary>
    /// <param name="targetPattern">The pattern solution.</param>
    /// <param name="gridWidth">Grid width.</param>
    /// <param name="gridHeight">Grid height.</param>
    /// <param name="elements">Valid pattern symbols.</param>
    /// <param name="showTarget">Whether to show the target.</param>
    /// <param name="displayDuration">How long to show (turns).</param>
    /// <param name="acceptRotations">Accept rotated solutions.</param>
    /// <param name="acceptReflections">Accept reflected solutions.</param>
    /// <returns>A new PatternPuzzle instance.</returns>
    public static PatternPuzzle Create(
        string targetPattern,
        int gridWidth,
        int gridHeight,
        IEnumerable<string>? elements = null,
        bool showTarget = true,
        int displayDuration = 0,
        bool acceptRotations = false,
        bool acceptReflections = false)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(targetPattern);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(gridWidth);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(gridHeight);

        return new PatternPuzzle
        {
            TargetPattern = targetPattern,
            GridWidth = gridWidth,
            GridHeight = gridHeight,
            PatternElements = (IReadOnlyList<string>?)elements?.ToList() ?? Array.Empty<string>(),
            ShowTargetPattern = showTarget,
            PatternDisplayDuration = Math.Max(0, displayDuration),
            AcceptRotations = acceptRotations,
            AcceptReflections = acceptReflections
        };
    }

    // ===== Validation Methods =====

    /// <summary>
    /// Validates if the input pattern matches the target.
    /// </summary>
    /// <param name="inputPattern">The player's pattern input.</param>
    /// <returns>True if the pattern matches.</returns>
    public bool Validate(string inputPattern)
    {
        if (string.IsNullOrEmpty(inputPattern))
            return false;

        // Direct match
        if (inputPattern == TargetPattern)
            return true;

        // Check rotations if enabled
        if (AcceptRotations)
        {
            var rotations = GetRotations(TargetPattern);
            if (rotations.Contains(inputPattern))
                return true;
        }

        // Check reflections if enabled
        if (AcceptReflections)
        {
            var reflections = GetReflections(TargetPattern);
            if (reflections.Contains(inputPattern))
                return true;

            // Also check reflections of rotations if both are enabled
            if (AcceptRotations)
            {
                var rotations = GetRotations(TargetPattern);
                foreach (var rotation in rotations)
                {
                    var rotatedReflections = GetReflections(rotation);
                    if (rotatedReflections.Contains(inputPattern))
                        return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Validates that input contains only valid pattern elements.
    /// </summary>
    /// <param name="input">The input to validate.</param>
    /// <returns>True if all characters are valid elements.</returns>
    public bool IsValidInput(string input)
    {
        if (string.IsNullOrEmpty(input))
            return true;

        if (PatternElements.Count == 0)
            return true; // No restrictions

        var validChars = PatternElements.SelectMany(e => e.ToCharArray()).ToHashSet();
        return input.All(c => validChars.Contains(c));
    }

    // ===== Rendering Methods =====

    /// <summary>
    /// Renders the target pattern as a display grid.
    /// </summary>
    /// <returns>ASCII representation of the pattern grid.</returns>
    public string RenderGrid()
    {
        return RenderPatternAsGrid(TargetPattern);
    }

    /// <summary>
    /// Renders an input pattern as a display grid.
    /// </summary>
    /// <param name="pattern">The pattern to render.</param>
    /// <returns>ASCII representation of the pattern.</returns>
    public string RenderPatternAsGrid(string pattern)
    {
        var lines = new List<string>();

        // Top border
        var topBorder = "  ╔" + string.Concat(Enumerable.Repeat("═══╦", GridWidth - 1)) + "═══╗";
        lines.Add(topBorder);

        for (int y = 0; y < GridHeight; y++)
        {
            var row = "  ║";
            for (int x = 0; x < GridWidth; x++)
            {
                var index = y * GridWidth + x;
                var symbol = index < pattern.Length ? pattern[index].ToString() : " ";
                row += $" {symbol} ║";
            }
            lines.Add(row);

            // Row separator (except for last row)
            if (y < GridHeight - 1)
            {
                var separator = "  ╠" + string.Concat(Enumerable.Repeat("═══╬", GridWidth - 1)) + "═══╣";
                lines.Add(separator);
            }
        }

        // Bottom border
        var bottomBorder = "  ╚" + string.Concat(Enumerable.Repeat("═══╩", GridWidth - 1)) + "═══╝";
        lines.Add(bottomBorder);

        return string.Join(Environment.NewLine, lines);
    }

    // ===== Rotation/Reflection Helpers =====

    private List<string> GetRotations(string pattern)
    {
        var rotations = new List<string>();
        var grid = PatternToGrid(pattern);

        // 90 degrees
        var rotated90 = RotateGrid90(grid);
        rotations.Add(GridToPattern(rotated90));

        // 180 degrees
        var rotated180 = RotateGrid90(rotated90);
        rotations.Add(GridToPattern(rotated180));

        // 270 degrees
        var rotated270 = RotateGrid90(rotated180);
        rotations.Add(GridToPattern(rotated270));

        return rotations;
    }

    private List<string> GetReflections(string pattern)
    {
        var reflections = new List<string>();
        var grid = PatternToGrid(pattern);

        // Horizontal reflection
        var horizontalReflection = ReflectGridHorizontal(grid);
        reflections.Add(GridToPattern(horizontalReflection));

        // Vertical reflection
        var verticalReflection = ReflectGridVertical(grid);
        reflections.Add(GridToPattern(verticalReflection));

        return reflections;
    }

    private char[,] PatternToGrid(string pattern)
    {
        var grid = new char[GridHeight, GridWidth];
        for (int i = 0; i < pattern.Length && i < GridWidth * GridHeight; i++)
        {
            grid[i / GridWidth, i % GridWidth] = pattern[i];
        }
        return grid;
    }

    private string GridToPattern(char[,] grid)
    {
        var height = grid.GetLength(0);
        var width = grid.GetLength(1);
        var chars = new char[height * width];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                chars[y * width + x] = grid[y, x];
            }
        }
        return new string(chars);
    }

    private char[,] RotateGrid90(char[,] grid)
    {
        var height = grid.GetLength(0);
        var width = grid.GetLength(1);
        var rotated = new char[width, height];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                rotated[x, height - 1 - y] = grid[y, x];
            }
        }
        return rotated;
    }

    private char[,] ReflectGridHorizontal(char[,] grid)
    {
        var height = grid.GetLength(0);
        var width = grid.GetLength(1);
        var reflected = new char[height, width];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                reflected[y, width - 1 - x] = grid[y, x];
            }
        }
        return reflected;
    }

    private char[,] ReflectGridVertical(char[,] grid)
    {
        var height = grid.GetLength(0);
        var width = grid.GetLength(1);
        var reflected = new char[height, width];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                reflected[height - 1 - y, x] = grid[y, x];
            }
        }
        return reflected;
    }

    /// <summary>
    /// Returns a string representation of this pattern puzzle.
    /// </summary>
    public override string ToString() =>
        $"PatternPuzzle({GridWidth}x{GridHeight}, Rotations={AcceptRotations}, Reflections={AcceptReflections})";
}
