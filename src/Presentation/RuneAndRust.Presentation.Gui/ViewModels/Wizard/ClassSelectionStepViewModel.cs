namespace RuneAndRust.Presentation.Gui.ViewModels.Wizard;

using CommunityToolkit.Mvvm.ComponentModel;
using RuneAndRust.Domain.Definitions;
using RuneAndRust.Domain.ValueObjects;
using RuneAndRust.Presentation.Gui.Models;
using System.Collections.ObjectModel;

/// <summary>
/// ViewModel for the class selection step.
/// </summary>
public partial class ClassSelectionStepViewModel : WizardStepViewModelBase
{
    /// <inheritdoc />
    public override string StepTitle => "Choose Class";

    /// <summary>
    /// Gets the available classes.
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<ClassDefinition> _classes = [];

    /// <summary>
    /// Gets or sets the selected class.
    /// </summary>
    [ObservableProperty]
    private ClassDefinition? _selectedClass;

    /// <summary>
    /// Creates a new class selection step ViewModel.
    /// </summary>
    /// <param name="data">The character creation data.</param>
    public ClassSelectionStepViewModel(CharacterCreationData data) : base(data)
    {
        // Add sample classes for demo
        AddSampleClasses();
    }

    /// <inheritdoc />
    public override bool Validate()
    {
        if (SelectedClass is null)
        {
            ValidationMessage = "Please select a class";
            return IsValid = false;
        }

        Data.ClassId = SelectedClass.Id;
        ValidationMessage = null;
        return IsValid = true;
    }

    partial void OnSelectedClassChanged(ClassDefinition? value) => Validate();

    private void AddSampleClasses()
    {
        Classes.Add(ClassDefinition.Create(
            id: "warrior",
            name: "Warrior",
            description: "A master of martial combat, skilled with weapons and armor.",
            archetypeId: "martial",
            statModifiers: new StatModifiers { Might = 2, Fortitude = 1 },
            growthRates: new StatModifiers { Might = 2, Fortitude = 1 },
            primaryResourceId: "rage"
        ));

        Classes.Add(ClassDefinition.Create(
            id: "mage",
            name: "Mage",
            description: "A student of arcane arts, wielding devastating spells.",
            archetypeId: "caster",
            statModifiers: new StatModifiers { Will = 2, Wits = 1 },
            growthRates: new StatModifiers { Will = 2, Wits = 1 },
            primaryResourceId: "mana"
        ));

        Classes.Add(ClassDefinition.Create(
            id: "rogue",
            name: "Rogue",
            description: "A cunning fighter who uses stealth and precision.",
            archetypeId: "agile",
            statModifiers: new StatModifiers { Finesse = 2, Wits = 1 },
            growthRates: new StatModifiers { Finesse = 2, Wits = 1 },
            primaryResourceId: "energy"
        ));
    }
}
