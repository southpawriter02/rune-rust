using ReactiveUI;
using RuneAndRust.Core;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace RuneAndRust.DesktopUI.ViewModels;

/// <summary>
/// View model for displaying loot items found during search.
/// </summary>
public class LootItemViewModel : ViewModelBase
{
    /// <summary>
    /// Gets or sets the item name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the item description.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the item type display string.
    /// </summary>
    public string TypeDisplay { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the quality color for display.
    /// </summary>
    public string QualityColor { get; set; } = "#FFFFFF";

    /// <summary>
    /// Gets or sets the quality name.
    /// </summary>
    public string QualityName { get; set; } = "Common";

    /// <summary>
    /// Gets or sets the icon character for the item.
    /// </summary>
    public string Icon { get; set; } = "*";

    /// <summary>
    /// Gets or sets the underlying equipment (if applicable).
    /// </summary>
    public Equipment? Equipment { get; set; }

    /// <summary>
    /// Creates a LootItemViewModel from an Equipment.
    /// </summary>
    public static LootItemViewModel FromEquipment(Equipment equipment)
    {
        return new LootItemViewModel
        {
            Name = equipment.GetDisplayName(),
            Description = equipment.Description,
            TypeDisplay = equipment.Type.ToString(),
            QualityColor = GetQualityColor(equipment.Quality),
            QualityName = equipment.GetQualityName(),
            Icon = GetTypeIcon(equipment.Type),
            Equipment = equipment
        };
    }

    /// <summary>
    /// Creates a LootItemViewModel for currency.
    /// </summary>
    public static LootItemViewModel ForCurrency(string currencyType, int amount)
    {
        return new LootItemViewModel
        {
            Name = $"{amount} {currencyType}",
            Description = $"Found {amount} {currencyType.ToLower()}.",
            TypeDisplay = "Currency",
            QualityColor = currencyType == "Aether Shards" ? "#9400D3" : "#FFD700",
            QualityName = "Currency",
            Icon = "$"
        };
    }

    /// <summary>
    /// Creates a LootItemViewModel for a generic item.
    /// </summary>
    public static LootItemViewModel ForMaterial(string name, string description, int quantity = 1)
    {
        return new LootItemViewModel
        {
            Name = quantity > 1 ? $"{name} x{quantity}" : name,
            Description = description,
            TypeDisplay = "Material",
            QualityColor = "#808080",
            QualityName = "Material",
            Icon = "#"
        };
    }

    private static string GetQualityColor(QualityTier quality)
    {
        return quality switch
        {
            QualityTier.JuryRigged => "#808080",   // Gray
            QualityTier.Scavenged => "#FFFFFF",    // White
            QualityTier.ClanForged => "#4A90E2",   // Blue
            QualityTier.Optimized => "#9400D3",    // Purple
            QualityTier.MythForged => "#FFD700",   // Gold
            _ => "#FFFFFF"
        };
    }

    private static string GetTypeIcon(EquipmentType type)
    {
        return type switch
        {
            EquipmentType.Weapon => "W",
            EquipmentType.Armor => "A",
            EquipmentType.Accessory => "R",
            EquipmentType.Material => "#",
            _ => "*"
        };
    }
}

/// <summary>
/// View model for search results from exploring a room.
/// </summary>
public class SearchResultViewModel : ViewModelBase
{
    private bool _isCollected = false;

    /// <summary>
    /// Whether loot was found.
    /// </summary>
    public bool FoundLoot { get; set; }

    /// <summary>
    /// Collection of loot items found.
    /// </summary>
    public ObservableCollection<LootItemViewModel> LootItems { get; } = new();

    /// <summary>
    /// List of discovered secrets/environmental details.
    /// </summary>
    public ObservableCollection<string> DiscoveredSecrets { get; } = new();

    /// <summary>
    /// Whether an encounter was triggered.
    /// </summary>
    public bool TriggeredEncounter { get; set; }

    /// <summary>
    /// Description of the triggered encounter.
    /// </summary>
    public string EncounterDescription { get; set; } = string.Empty;

    /// <summary>
    /// Whether the search found nothing of interest.
    /// </summary>
    public bool NothingFound => !FoundLoot && DiscoveredSecrets.Count == 0 && !TriggeredEncounter;

    /// <summary>
    /// Whether the loot has been collected.
    /// </summary>
    public bool IsCollected
    {
        get => _isCollected;
        set => this.RaiseAndSetIfChanged(ref _isCollected, value);
    }

    /// <summary>
    /// Gets the result title based on what was found.
    /// </summary>
    public string ResultTitle
    {
        get
        {
            if (TriggeredEncounter) return "Encounter!";
            if (FoundLoot) return "Loot Found!";
            if (DiscoveredSecrets.Count > 0) return "Discovery!";
            return "Nothing Found";
        }
    }

    /// <summary>
    /// Gets the title color based on result type.
    /// </summary>
    public string TitleColor
    {
        get
        {
            if (TriggeredEncounter) return "#FF6B6B";
            if (FoundLoot) return "#FFD700";
            if (DiscoveredSecrets.Count > 0) return "#9400D3";
            return "#888888";
        }
    }

    /// <summary>
    /// Creates a search result with loot.
    /// </summary>
    public static SearchResultViewModel WithLoot(IEnumerable<LootItemViewModel> items, IEnumerable<string>? secrets = null)
    {
        var result = new SearchResultViewModel
        {
            FoundLoot = true
        };

        foreach (var item in items)
        {
            result.LootItems.Add(item);
        }

        if (secrets != null)
        {
            foreach (var secret in secrets)
            {
                result.DiscoveredSecrets.Add(secret);
            }
        }

        return result;
    }

    /// <summary>
    /// Creates a search result with an encounter.
    /// </summary>
    public static SearchResultViewModel WithEncounter(string description, IEnumerable<LootItemViewModel>? items = null)
    {
        var result = new SearchResultViewModel
        {
            TriggeredEncounter = true,
            EncounterDescription = description
        };

        if (items != null)
        {
            result.FoundLoot = true;
            foreach (var item in items)
            {
                result.LootItems.Add(item);
            }
        }

        return result;
    }

    /// <summary>
    /// Creates a search result with secrets only.
    /// </summary>
    public static SearchResultViewModel WithSecrets(IEnumerable<string> secrets)
    {
        var result = new SearchResultViewModel();

        foreach (var secret in secrets)
        {
            result.DiscoveredSecrets.Add(secret);
        }

        return result;
    }

    /// <summary>
    /// Creates an empty search result.
    /// </summary>
    public static SearchResultViewModel Empty()
    {
        return new SearchResultViewModel();
    }
}

/// <summary>
/// View model for rest confirmation dialog.
/// </summary>
public class RestConfirmationViewModel : ViewModelBase
{
    /// <summary>
    /// Whether this is a sanctuary rest.
    /// </summary>
    public bool IsSanctuary { get; set; }

    /// <summary>
    /// HP recovery amount.
    /// </summary>
    public string HPRecovery { get; set; } = string.Empty;

    /// <summary>
    /// Stamina recovery amount.
    /// </summary>
    public string StaminaRecovery { get; set; } = string.Empty;

    /// <summary>
    /// Current psychic stress level.
    /// </summary>
    public int CurrentStress { get; set; }

    /// <summary>
    /// Potential stress increase (for non-sanctuary rest).
    /// </summary>
    public int PotentialStressIncrease { get; set; }

    /// <summary>
    /// Warning messages for rest.
    /// </summary>
    public ObservableCollection<string> Warnings { get; } = new();

    /// <summary>
    /// Dialog title.
    /// </summary>
    public string Title => IsSanctuary ? "Sanctuary Rest" : "Rest";

    /// <summary>
    /// Title color.
    /// </summary>
    public string TitleColor => IsSanctuary ? "#4CAF50" : "#4A90E2";

    /// <summary>
    /// Creates a rest confirmation for sanctuary.
    /// </summary>
    public static RestConfirmationViewModel ForSanctuary(int currentHP, int maxHP, int currentStamina, int maxStamina)
    {
        return new RestConfirmationViewModel
        {
            IsSanctuary = true,
            HPRecovery = $"Full recovery ({currentHP} → {maxHP})",
            StaminaRecovery = $"Full recovery ({currentStamina} → {maxStamina})",
            CurrentStress = 0,
            PotentialStressIncrease = 0
        };
    }

    /// <summary>
    /// Creates a rest confirmation for regular rest.
    /// </summary>
    public static RestConfirmationViewModel ForRegularRest(
        int currentHP, int maxHP,
        int currentStamina, int maxStamina,
        int currentStress,
        bool hasEnemiesNearby = false)
    {
        var hpRecovery = Math.Min(maxHP, currentHP + maxHP / 4);
        var staminaRecovery = Math.Min(maxStamina, currentStamina + maxStamina / 2);

        var result = new RestConfirmationViewModel
        {
            IsSanctuary = false,
            HPRecovery = $"+25% ({currentHP} → {hpRecovery})",
            StaminaRecovery = $"+50% ({currentStamina} → {staminaRecovery})",
            CurrentStress = currentStress,
            PotentialStressIncrease = 5
        };

        result.Warnings.Add("Resting may increase Psychic Stress (+5)");
        result.Warnings.Add("Random encounters can occur while resting");

        if (hasEnemiesNearby)
        {
            result.Warnings.Add("Enemies detected nearby - higher encounter risk!");
        }

        if (currentStress >= 80)
        {
            result.Warnings.Add("High stress level - consider finding a sanctuary!");
        }

        return result;
    }
}
