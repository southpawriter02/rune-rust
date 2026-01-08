using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.DTOs;

/// <summary>
/// DTO for displaying experience gain information.
/// </summary>
/// <param name="AmountGained">The XP amount gained.</param>
/// <param name="NewTotal">The player's new total XP.</param>
/// <param name="Source">Description of the XP source.</param>
/// <param name="CurrentLevel">The player's current level.</param>
/// <param name="ExperienceToNextLevel">XP needed for next level.</param>
/// <param name="ProgressPercent">Progress toward next level (0-100).</param>
/// <param name="ExperienceTerminology">The configured term for experience (e.g., "XP", "Glory").</param>
/// <param name="LevelTerminology">The configured term for level (e.g., "Level", "Rank").</param>
public record ExperienceGainDto(
    int AmountGained,
    int NewTotal,
    string Source,
    int CurrentLevel,
    int ExperienceToNextLevel,
    int ProgressPercent,
    string ExperienceTerminology = "XP",
    string LevelTerminology = "Level")
{
    /// <summary>
    /// Creates a DTO from an ExperienceGainResult and player state.
    /// </summary>
    /// <param name="result">The experience gain result.</param>
    /// <param name="currentLevel">The player's current level.</param>
    /// <param name="xpToNextLevel">XP needed for next level.</param>
    /// <param name="progressPercent">Progress percentage.</param>
    /// <param name="experienceTerminology">The term for experience.</param>
    /// <param name="levelTerminology">The term for level.</param>
    /// <returns>A DTO for rendering.</returns>
    public static ExperienceGainDto FromResult(
        ExperienceGainResult result,
        int currentLevel,
        int xpToNextLevel,
        int progressPercent,
        string experienceTerminology = "XP",
        string levelTerminology = "Level") =>
        new(
            result.AmountGained,
            result.NewTotal,
            result.Source,
            currentLevel,
            xpToNextLevel,
            progressPercent,
            experienceTerminology,
            levelTerminology);
}
