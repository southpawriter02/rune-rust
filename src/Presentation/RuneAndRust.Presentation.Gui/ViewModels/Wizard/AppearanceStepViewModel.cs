namespace RuneAndRust.Presentation.Gui.ViewModels.Wizard;

using CommunityToolkit.Mvvm.ComponentModel;
using RuneAndRust.Presentation.Gui.Models;
using System.Collections.ObjectModel;

/// <summary>
/// ViewModel for the appearance/portrait selection step.
/// </summary>
public partial class AppearanceStepViewModel : WizardStepViewModelBase
{
    /// <inheritdoc />
    public override string StepTitle => "Choose Appearance";

    /// <summary>
    /// Gets the available portraits.
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<PortraitOption> _portraits = [];

    /// <summary>
    /// Gets or sets the selected portrait.
    /// </summary>
    [ObservableProperty]
    private PortraitOption? _selectedPortrait;

    /// <summary>
    /// Creates a new appearance step ViewModel.
    /// </summary>
    /// <param name="data">The character creation data.</param>
    public AppearanceStepViewModel(CharacterCreationData data) : base(data)
    {
        AddSamplePortraits();
        IsValid = true; // Portrait is optional
    }

    /// <inheritdoc />
    public override bool Validate()
    {
        Data.PortraitId = SelectedPortrait?.Id ?? "default";
        return IsValid = true;
    }

    partial void OnSelectedPortraitChanged(PortraitOption? value) => Validate();

    private void AddSamplePortraits()
    {
        Portraits.Add(new PortraitOption("warrior-1", "⚔️", "Warrior 1"));
        Portraits.Add(new PortraitOption("warrior-2", "🛡", "Warrior 2"));
        Portraits.Add(new PortraitOption("mage-1", "🧙", "Mage 1"));
        Portraits.Add(new PortraitOption("mage-2", "✨", "Mage 2"));
        Portraits.Add(new PortraitOption("rogue-1", "🗡", "Rogue 1"));
        Portraits.Add(new PortraitOption("rogue-2", "🎭", "Rogue 2"));

        SelectedPortrait = Portraits[0];
    }
}

/// <summary>
/// Represents a portrait option.
/// </summary>
public class PortraitOption
{
    /// <summary>
    /// Gets the portrait ID.
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// Gets the icon.
    /// </summary>
    public string Icon { get; }

    /// <summary>
    /// Gets the display name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Creates a new portrait option.
    /// </summary>
    public PortraitOption(string id, string icon, string name)
    {
        Id = id;
        Icon = icon;
        Name = name;
    }
}
