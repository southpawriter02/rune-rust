namespace RuneAndRust.Core;

/// <summary>
/// v0.41: Cosmetic type categories
/// </summary>
public enum CosmeticType
{
    Title,              // Display title (e.g., "Gauntlet Champion")
    Portrait,           // Character portrait image
    UITheme,            // Interface color scheme
    AbilityVFX,         // Ability visual effect variant
    CombatLogStyle,     // Combat log formatting style
    CharacterFrame,     // Portrait frame decoration
    Emblem              // Account emblem/badge
}

/// <summary>
/// v0.41: Cosmetic customization item
/// Visual customization with zero gameplay impact
/// </summary>
public class Cosmetic
{
    public string CosmeticId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public CosmeticType Type { get; set; }
    public string Description { get; set; } = string.Empty;
    public string PreviewImageUrl { get; set; } = string.Empty;

    // Unlock requirements
    public string UnlockRequirement { get; set; } = string.Empty;

    // Application data (JSON parameters for cosmetic application)
    public Dictionary<string, string> Parameters { get; set; } = new();
}

/// <summary>
/// v0.41: Cosmetic unlock progress for a specific account
/// </summary>
public class CosmeticProgress
{
    public int ProgressId { get; set; }
    public int AccountId { get; set; }
    public string CosmeticId { get; set; } = string.Empty;
    public bool IsUnlocked { get; set; } = false;
    public DateTime? UnlockedAt { get; set; }
}

/// <summary>
/// v0.41: Cosmetic loadout (player's selected cosmetics)
/// </summary>
public class CosmeticLoadout
{
    public int LoadoutId { get; set; }
    public int AccountId { get; set; }
    public string LoadoutName { get; set; } = "Default";

    // Selected cosmetics
    public string? SelectedTitle { get; set; }
    public string? SelectedPortrait { get; set; }
    public string? SelectedUITheme { get; set; }
    public string? SelectedCharacterFrame { get; set; }
    public string? SelectedEmblem { get; set; }

    // Ability VFX overrides (JSON dictionary: ability name -> VFX cosmetic ID)
    public Dictionary<string, string> AbilityVFXOverrides { get; set; } = new();

    // Combat log style
    public string? CombatLogStyle { get; set; }

    // Active loadout flag
    public bool IsActive { get; set; } = false;
}
