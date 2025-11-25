using RuneAndRust.Core.BossGauntlet;
using RuneAndRust.Core.ChallengeSectors;
using RuneAndRust.Core.NewGamePlus;

namespace RuneAndRust.DesktopUI.Services;

/// <summary>
/// v0.43.16: Endgame mode selection for UI.
/// </summary>
public enum EndgameMode
{
    /// <summary>New Game Plus - replay with increased difficulty and rewards.</summary>
    NGPlus,

    /// <summary>Challenge Sectors - handcrafted extreme difficulty challenges.</summary>
    ChallengeSector,

    /// <summary>Boss Gauntlet - sequential boss fights with limited resources.</summary>
    BossGauntlet,

    /// <summary>Endless Mode - survive waves of increasing difficulty.</summary>
    EndlessMode
}

/// <summary>
/// v0.43.16: Type of difficulty modifier value.
/// </summary>
public enum ModifierType
{
    /// <summary>Percentage-based modifier (e.g., +50% damage).</summary>
    Percentage,

    /// <summary>Flat value modifier (e.g., +2 enemy levels).</summary>
    Flat,

    /// <summary>Multiplier (e.g., 1.5x corruption rate).</summary>
    Multiplier
}

/// <summary>
/// v0.43.16: Represents a difficulty modifier applied to endgame content.
/// </summary>
public class DifficultyModifier
{
    /// <summary>Unique modifier identifier.</summary>
    public string ModifierId { get; set; } = string.Empty;

    /// <summary>Display name for the modifier.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Description of what the modifier does.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Type of modifier value.</summary>
    public ModifierType Type { get; set; }

    /// <summary>Modifier value (positive = harder, negative = easier).</summary>
    public float Value { get; set; }

    /// <summary>Category for grouping modifiers.</summary>
    public string Category { get; set; } = "General";

    /// <summary>Is this modifier detrimental (makes game harder)?</summary>
    public bool IsDetrimental { get; set; } = true;
}

/// <summary>
/// v0.43.16: Represents a reward multiplier for endgame content.
/// </summary>
public class RewardMultiplier
{
    /// <summary>Type of reward (e.g., "Legend Points", "Loot Quality").</summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>Multiplier value (e.g., 1.5 = 50% bonus).</summary>
    public float Multiplier { get; set; } = 1.0f;

    /// <summary>Description of the reward bonus.</summary>
    public string Description { get; set; } = string.Empty;
}

/// <summary>
/// v0.43.16: Configuration for a Challenge Sector run.
/// </summary>
public class ChallengeSectorConfig
{
    /// <summary>Selected challenge sector.</summary>
    public ChallengeSector? Sector { get; set; }

    /// <summary>List of active modifier IDs.</summary>
    public List<string> ActiveModifierIds { get; set; } = new();

    /// <summary>Whether to enable hardcore mode (permadeath).</summary>
    public bool HardcoreMode { get; set; }
}

/// <summary>
/// v0.43.16: Configuration for starting an endgame mode.
/// </summary>
public class EndgameModeConfig
{
    /// <summary>Selected endgame mode.</summary>
    public EndgameMode Mode { get; set; }

    /// <summary>Selected NG+ tier (1-5).</summary>
    public int NGPlusTier { get; set; } = 1;

    /// <summary>Challenge Sector configuration (if applicable).</summary>
    public ChallengeSectorConfig? ChallengeSectorConfig { get; set; }

    /// <summary>Selected Boss Gauntlet sequence (if applicable).</summary>
    public GauntletSequence? GauntletSequence { get; set; }

    /// <summary>Selected character ID.</summary>
    public int CharacterId { get; set; }
}

/// <summary>
/// v0.43.16: Summary of available endgame content for UI display.
/// </summary>
public class EndgameContentAvailability
{
    /// <summary>Maximum NG+ tier unlocked.</summary>
    public int MaxUnlockedNGPlusTier { get; set; } = 1;

    /// <summary>Whether Challenge Sectors are unlocked.</summary>
    public bool ChallengeSectorsUnlocked { get; set; }

    /// <summary>Whether Boss Gauntlet is unlocked.</summary>
    public bool BossGauntletUnlocked { get; set; }

    /// <summary>Whether Endless Mode is unlocked.</summary>
    public bool EndlessModeUnlocked { get; set; }

    /// <summary>Available challenge sectors.</summary>
    public List<ChallengeSector> AvailableSectors { get; set; } = new();

    /// <summary>Available boss gauntlet sequences.</summary>
    public List<GauntletSequence> AvailableGauntlets { get; set; } = new();

    /// <summary>Current endless mode high score (wave reached).</summary>
    public int EndlessModeHighScore { get; set; }
}

/// <summary>
/// v0.43.16: Service interface for endgame mode selection and configuration.
/// Provides UI with information about available endgame content and difficulty modifiers.
/// </summary>
public interface IEndgameService
{
    /// <summary>
    /// Gets the maximum unlocked NG+ tier for the current account.
    /// </summary>
    int GetMaxUnlockedNGPlusTier();

    /// <summary>
    /// Checks if Challenge Sectors are unlocked.
    /// </summary>
    bool IsChallengeSectorUnlocked();

    /// <summary>
    /// Checks if Boss Gauntlet is unlocked.
    /// </summary>
    bool IsBossGauntletUnlocked();

    /// <summary>
    /// Checks if Endless Mode is unlocked.
    /// </summary>
    bool IsEndlessModeUnlocked();

    /// <summary>
    /// Gets all available endgame content for the current account.
    /// </summary>
    EndgameContentAvailability GetAvailableContent();

    /// <summary>
    /// Gets difficulty modifiers for a specific NG+ tier.
    /// </summary>
    List<DifficultyModifier> GetNGPlusModifiers(int tier);

    /// <summary>
    /// Gets difficulty modifiers for a Challenge Sector configuration.
    /// </summary>
    List<DifficultyModifier> GetChallengeSectorModifiers(ChallengeSectorConfig? config);

    /// <summary>
    /// Gets difficulty modifiers for Boss Gauntlet mode.
    /// </summary>
    List<DifficultyModifier> GetBossGauntletModifiers();

    /// <summary>
    /// Gets difficulty modifiers for Endless Mode.
    /// </summary>
    List<DifficultyModifier> GetEndlessModeModifiers();

    /// <summary>
    /// Gets reward multipliers for the specified mode and tier.
    /// </summary>
    List<RewardMultiplier> GetRewardMultipliers(EndgameMode mode, int ngPlusTier = 0);

    /// <summary>
    /// Gets available Challenge Sectors.
    /// </summary>
    List<ChallengeSector> GetAvailableChallengeSectors();

    /// <summary>
    /// Gets available Boss Gauntlet sequences.
    /// </summary>
    List<GauntletSequence> GetAvailableGauntletSequences();

    /// <summary>
    /// Starts an endgame mode with the specified configuration.
    /// </summary>
    Task<bool> StartEndgameModeAsync(EndgameModeConfig config);
}
