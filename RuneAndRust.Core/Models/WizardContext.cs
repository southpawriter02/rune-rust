using RuneAndRust.Core.Enums;

namespace RuneAndRust.Core.Models;

/// <summary>
/// Holds the partial selections during the character creation wizard (v0.3.4b).
/// Tracks progress through each step and stores confirmed choices.
/// </summary>
public class WizardContext
{
    /// <summary>
    /// The character's name, entered in step 1.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The selected lineage, confirmed in step 2.
    /// </summary>
    public LineageType? Lineage { get; set; }

    /// <summary>
    /// The selected archetype, confirmed in step 2.
    /// </summary>
    public ArchetypeType? Archetype { get; set; }

    /// <summary>
    /// The selected background, confirmed in step 3 (v0.3.4c).
    /// </summary>
    public BackgroundType? Background { get; set; }

    /// <summary>
    /// The current step index in the wizard (0-based).
    /// </summary>
    public int CurrentStep { get; set; } = 0;

    /// <summary>
    /// Checks if a specific step has been completed.
    /// </summary>
    /// <param name="stepIndex">The 0-based step index.</param>
    /// <returns>True if the step is complete, false otherwise.</returns>
    public bool IsStepComplete(int stepIndex) => stepIndex switch
    {
        0 => !string.IsNullOrWhiteSpace(Name),
        1 => Lineage.HasValue,
        2 => Archetype.HasValue,
        3 => Background.HasValue,
        _ => false
    };

    /// <summary>
    /// Resets the context to a clean state for a new wizard run.
    /// </summary>
    public void Reset()
    {
        Name = string.Empty;
        Lineage = null;
        Archetype = null;
        Background = null;
        CurrentStep = 0;
    }

    /// <summary>
    /// Clears the selection for the current step (for back navigation).
    /// </summary>
    public void ClearCurrentStep()
    {
        switch (CurrentStep)
        {
            case 0:
                Name = string.Empty;
                break;
            case 1:
                Lineage = null;
                break;
            case 2:
                Archetype = null;
                break;
            case 3:
                Background = null;
                break;
        }
    }
}
