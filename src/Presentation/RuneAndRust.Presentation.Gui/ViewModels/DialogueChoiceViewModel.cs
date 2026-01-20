namespace RuneAndRust.Presentation.Gui.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using RuneAndRust.Presentation.Gui.Models;

/// <summary>
/// ViewModel for a dialogue choice.
/// </summary>
public partial class DialogueChoiceViewModel : ViewModelBase
{
    /// <summary>Gets the choice index (1-9).</summary>
    public int Index { get; }

    /// <summary>Gets the choice text.</summary>
    public string Text { get; }

    /// <summary>Gets the action to trigger.</summary>
    public DialogueAction Action { get; }

    /// <summary>Gets the action data (quest ID, shop ID, etc.).</summary>
    public string? ActionData { get; }

    /// <summary>Gets the next node ID.</summary>
    public string? NextNodeId { get; }

    /// <summary>Gets the formatted display text.</summary>
    public string DisplayText => $"{Index}. \"{Text}\"";

    /// <summary>Gets the action badge text.</summary>
    public string? ActionBadge => Action switch
    {
        DialogueAction.AcceptQuest => "[ACCEPT QUEST]",
        DialogueAction.DeclineQuest => "[DECLINE]",
        DialogueAction.OpenShop => "[SHOP]",
        DialogueAction.Leave => "[LEAVE]",
        DialogueAction.GiveItem => "[RECEIVE]",
        DialogueAction.TakeItem => "[GIVE]",
        _ => null
    };

    /// <summary>Gets whether the choice has an action badge.</summary>
    public bool HasActionBadge => ActionBadge is not null;

    /// <summary>Creates a dialogue choice ViewModel.</summary>
    public DialogueChoiceViewModel(int index, DialogueChoiceData data)
    {
        Index = index;
        Text = data.Text;
        Action = data.Action;
        ActionData = data.ActionData;
        NextNodeId = data.NextNodeId;
    }
}
