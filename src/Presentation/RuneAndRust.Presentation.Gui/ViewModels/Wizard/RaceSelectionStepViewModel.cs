namespace RuneAndRust.Presentation.Gui.ViewModels.Wizard;

using CommunityToolkit.Mvvm.ComponentModel;
using RuneAndRust.Domain.Definitions;
using RuneAndRust.Presentation.Gui.Models;
using System.Collections.ObjectModel;

/// <summary>
/// ViewModel for the race selection step.
/// </summary>
public partial class RaceSelectionStepViewModel : WizardStepViewModelBase
{
    /// <inheritdoc />
    public override string StepTitle => "Choose Race";

    /// <summary>
    /// Gets the available races.
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<RaceDefinition> _races = [];

    /// <summary>
    /// Gets or sets the selected race.
    /// </summary>
    [ObservableProperty]
    private RaceDefinition? _selectedRace;

    /// <summary>
    /// Creates a new race selection step ViewModel.
    /// </summary>
    /// <param name="data">The character creation data.</param>
    public RaceSelectionStepViewModel(CharacterCreationData data) : base(data)
    {
        // Add sample races for demo
        AddSampleRaces();
    }

    /// <inheritdoc />
    public override bool Validate()
    {
        if (SelectedRace is null)
        {
            ValidationMessage = "Please select a race";
            return IsValid = false;
        }

        Data.RaceId = SelectedRace.Id;
        ValidationMessage = null;
        return IsValid = true;
    }

    partial void OnSelectedRaceChanged(RaceDefinition? value) => Validate();

    private void AddSampleRaces()
    {
        Races.Add(new RaceDefinition
        {
            Id = "human",
            Name = "Human",
            Description = "Versatile and adaptable, humans are the most common race.",
            AttributeModifiers = new Dictionary<string, int>
            {
                ["Strength"] = 1,
                ["Dexterity"] = 1,
                ["Constitution"] = 1
            },
            TraitName = "Adaptable",
            TraitDescription = "Gain +1 to any three attributes."
        });

        Races.Add(new RaceDefinition
        {
            Id = "elf",
            Name = "Elf",
            Description = "Graceful beings with keen senses and natural affinity for magic.",
            AttributeModifiers = new Dictionary<string, int>
            {
                ["Dexterity"] = 2,
                ["Intelligence"] = 1
            },
            TraitName = "Darkvision",
            TraitDescription = "See in darkness up to 60 feet."
        });

        Races.Add(new RaceDefinition
        {
            Id = "dwarf",
            Name = "Dwarf",
            Description = "Stout and resilient, dwarves are master craftsmen.",
            AttributeModifiers = new Dictionary<string, int>
            {
                ["Constitution"] = 2,
                ["Strength"] = 1
            },
            TraitName = "Stonecunning",
            TraitDescription = "Advantage on checks related to stonework."
        });
    }
}
