// ═══════════════════════════════════════════════════════════════════════════════
// LeaderboardDisplayDto.cs
// Data transfer object for displaying leaderboard entries.
// Version: 0.13.4c
// ═══════════════════════════════════════════════════════════════════════════════

using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Presentation.Tui.DTOs;

/// <summary>
/// Data transfer object for displaying a leaderboard entry.
/// </summary>
/// <remarks>
/// <para>
/// Contains all display-ready data for a single leaderboard entry,
/// including rank, player info, score/time/floors, and highlighting flags.
/// </para>
/// <para>Category-specific value fields:</para>
/// <list type="bullet">
///   <item><description>High Score / Achievement Points: Use <see cref="Score"/></description></item>
///   <item><description>Speedrun: Use <see cref="TimeElapsed"/></description></item>
///   <item><description>No Death: Use <see cref="FloorsCleared"/></description></item>
///   <item><description>Boss Slayer: Use <see cref="BossesDefeated"/></description></item>
/// </list>
/// </remarks>
public class LeaderboardDisplayDto
{
    /// <summary>
    /// Gets or sets the rank position (1-based).
    /// </summary>
    public int Rank { get; set; }

    /// <summary>
    /// Gets or sets the player's display name.
    /// </summary>
    public required string PlayerName { get; set; }

    /// <summary>
    /// Gets or sets the character class name.
    /// </summary>
    public required string CharacterClass { get; set; }

    /// <summary>
    /// Gets or sets the character level.
    /// </summary>
    public int Level { get; set; }

    /// <summary>
    /// Gets or sets the score value (for High Score and Achievement Points).
    /// </summary>
    public long Score { get; set; }

    /// <summary>
    /// Gets or sets the achievement date.
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    /// Gets or sets whether this is the current player's entry.
    /// </summary>
    /// <remarks>
    /// When true, the entry is highlighted with a different color
    /// and the rank is prefixed with "&gt;" (e.g., "&gt;5").
    /// </remarks>
    public bool IsCurrentPlayer { get; set; }

    /// <summary>
    /// Gets or sets the leaderboard category.
    /// </summary>
    public LeaderboardCategory Category { get; set; }

    /// <summary>
    /// Gets or sets the time elapsed (for Speedrun category).
    /// </summary>
    public TimeSpan? TimeElapsed { get; set; }

    /// <summary>
    /// Gets or sets the floors cleared (for No Death category).
    /// </summary>
    public int? FloorsCleared { get; set; }

    /// <summary>
    /// Gets or sets the bosses defeated (for Boss Slayer category).
    /// </summary>
    public int? BossesDefeated { get; set; }
}
