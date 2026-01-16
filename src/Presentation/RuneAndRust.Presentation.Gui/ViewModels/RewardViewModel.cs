namespace RuneAndRust.Presentation.Gui.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;

/// <summary>
/// ViewModel for a quest reward.
/// </summary>
public partial class RewardViewModel : ViewModelBase
{
    /// <summary>Gets the reward type (XP, Gold, Item, Unlock).</summary>
    [ObservableProperty] private string _rewardType;

    /// <summary>Gets the reward description.</summary>
    [ObservableProperty] private string _description;

    /// <summary>Gets the reward icon.</summary>
    public string Icon => RewardType switch
    {
        "Experience" or "XP" => "✨",
        "Gold" => "💰",
        "Item" => "📦",
        "Unlock" => "🔓",
        _ => "•"
    };

    /// <summary>Creates a reward ViewModel.</summary>
    public RewardViewModel(string rewardType, string description)
    {
        _rewardType = rewardType;
        _description = description;
    }
}
