using RuneAndRust.Presentation.UI;

namespace RuneAndRust.Presentation.Configuration;

/// <summary>
/// Configuration for health bar rendering.
/// </summary>
public record HealthBarConfig(
    Dictionary<BarType, List<ColorThreshold>> Thresholds,
    BarCharacters Characters,
    bool EnableBlinking,
    int BlinkThreshold)
{
    /// <summary>
    /// Gets thresholds for a bar type.
    /// </summary>
    public IReadOnlyList<ColorThreshold> GetThresholds(BarType type)
    {
        return Thresholds.TryGetValue(type, out var list) 
            ? list 
            : new List<ColorThreshold> { new(100, ConsoleColor.Gray) };
    }
    
    /// <summary>
    /// Creates default configuration with standard thresholds.
    /// </summary>
    public static HealthBarConfig CreateDefault()
    {
        return new HealthBarConfig(
            Thresholds: new Dictionary<BarType, List<ColorThreshold>>
            {
                [BarType.Health] = new()
                {
                    new(100, ConsoleColor.Green),
                    new(75, ConsoleColor.Green),
                    new(50, ConsoleColor.Yellow),
                    new(25, ConsoleColor.Red),
                    new(10, ConsoleColor.DarkRed)
                },
                [BarType.Mana] = new()
                {
                    new(100, ConsoleColor.Blue)
                },
                [BarType.Experience] = new()
                {
                    new(100, ConsoleColor.Magenta)
                },
                [BarType.Stamina] = new()
                {
                    new(100, ConsoleColor.Yellow),
                    new(50, ConsoleColor.DarkYellow)
                }
            },
            Characters: new BarCharacters(),
            EnableBlinking: true,
            BlinkThreshold: 10);
    }
}
