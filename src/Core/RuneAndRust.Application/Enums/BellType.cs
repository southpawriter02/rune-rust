namespace RuneAndRust.Application.Enums;

/// <summary>
/// Bell pattern types for different events in TUI mode.
/// </summary>
/// <remarks>
/// <para>
/// Each type has a distinct audio pattern:
/// <list type="bullet">
///   <item><description><see cref="Info"/> — Single short beep (800Hz, 100ms)</description></item>
///   <item><description><see cref="Warning"/> — Double beep (600Hz, 150ms × 2)</description></item>
///   <item><description><see cref="Error"/> — Long beep (400Hz, 500ms)</description></item>
///   <item><description><see cref="Combat"/> — Quick triple (1000Hz, 80ms × 3)</description></item>
///   <item><description><see cref="LevelUp"/> — Ascending tones (600→800→1000Hz)</description></item>
///   <item><description><see cref="QuestComplete"/> — Celebratory pattern</description></item>
///   <item><description><see cref="LowHealth"/> — Danger pattern (400Hz × 2)</description></item>
///   <item><description><see cref="Notification"/> — Simple beep (700Hz, 120ms)</description></item>
/// </list>
/// </para>
/// </remarks>
public enum BellType
{
    /// <summary>
    /// Single short beep for general info.
    /// </summary>
    Info = 0,

    /// <summary>
    /// Double beep for warnings.
    /// </summary>
    Warning = 1,

    /// <summary>
    /// Long beep for errors.
    /// </summary>
    Error = 2,

    /// <summary>
    /// Quick triple beep for combat start.
    /// </summary>
    Combat = 3,

    /// <summary>
    /// Ascending tones for level up.
    /// </summary>
    LevelUp = 4,

    /// <summary>
    /// Celebratory pattern for quest completion.
    /// </summary>
    QuestComplete = 5,

    /// <summary>
    /// Danger pattern for low health.
    /// </summary>
    LowHealth = 6,

    /// <summary>
    /// Simple notification beep.
    /// </summary>
    Notification = 7
}
