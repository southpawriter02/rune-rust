// ═══════════════════════════════════════════════════════════════════════════════
// CharacterCreationViewModel.cs
// Read-only presentation data for the character creation wizard. Contains all
// information needed to render the current step, including step display info,
// navigation state, current selection names, preview data (derived stats,
// abilities, equipment), and validation status. Rebuilt by the ViewModelBuilder
// whenever the creation state changes.
// Version: 0.17.5b
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.ValueObjects;

using RuneAndRust.Domain.Enums;

/// <summary>
/// Read-only presentation data for the character creation wizard.
/// Contains all information needed to render the current step.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="CharacterCreationViewModel"/> is an immutable value object that provides
/// a clean separation between state management (<see cref="RuneAndRust.Domain.Entities.CharacterCreationState"/>)
/// and presentation (TUI screens). It is rebuilt by the <c>IViewModelBuilder</c>
/// service whenever the creation state changes.
/// </para>
/// <para>
/// The ViewModel contains five categories of data:
/// </para>
/// <list type="number">
///   <item>
///     <description>
///       <strong>Step Information:</strong> Current step enum, display title,
///       thematic description, step number (1-6), and total steps (6).
///     </description>
///   </item>
///   <item>
///     <description>
///       <strong>Navigation State:</strong> Whether the user can go back,
///       forward, and whether the current step is a permanent choice.
///     </description>
///   </item>
///   <item>
///     <description>
///       <strong>Current Selections:</strong> Display names of all selections
///       made so far (lineage, background, archetype, specialization, name).
///     </description>
///   </item>
///   <item>
///     <description>
///       <strong>Preview Data:</strong> Derived stats, abilities, and equipment
///       previews based on current selections. Nullable — null means "not yet
///       applicable" for the current step.
///     </description>
///   </item>
///   <item>
///     <description>
///       <strong>Validation:</strong> Error messages and step validity status.
///     </description>
///   </item>
/// </list>
/// <para>
/// <strong>Lifecycle:</strong> The ViewModel is rebuilt on every state change.
/// There is no caching — the ViewModelBuilder constructs a fresh instance each
/// time <c>Build(state)</c> is called. This ensures the ViewModel always reflects
/// the current state without stale data.
/// </para>
/// </remarks>
/// <seealso cref="RuneAndRust.Domain.Entities.CharacterCreationState"/>
/// <seealso cref="DerivedStatsPreview"/>
/// <seealso cref="AbilitiesPreview"/>
/// <seealso cref="EquipmentPreview"/>
public readonly record struct CharacterCreationViewModel
{
    // ═══════════════════════════════════════════════════════════════════════════
    // STEP INFORMATION
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the current step in the creation workflow.
    /// </summary>
    /// <value>
    /// The <see cref="CharacterCreationStep"/> enum value for the current step.
    /// Ranges from <see cref="CharacterCreationStep.Lineage"/> (0) to
    /// <see cref="CharacterCreationStep.Summary"/> (5).
    /// </value>
    public CharacterCreationStep CurrentStep { get; init; }

    /// <summary>
    /// Gets the display title for the current step.
    /// </summary>
    /// <value>
    /// A human-readable step title from the extension method
    /// <c>CharacterCreationStep.GetDisplayName()</c>.
    /// Examples: "Choose Your Lineage", "Allocate Attributes", "Confirm Your Survivor".
    /// </value>
    public string StepTitle { get; init; }

    /// <summary>
    /// Gets the thematic description for the current step.
    /// </summary>
    /// <value>
    /// An atmospheric description from <c>CharacterCreationStep.GetDescription()</c>.
    /// Example: "Your bloodline carries echoes of the world before."
    /// </value>
    public string StepDescription { get; init; }

    /// <summary>
    /// Gets the 1-based step number for display (1-6).
    /// </summary>
    /// <value>
    /// The step number suitable for "Step N of 6" display.
    /// Lineage = 1, Background = 2, Attributes = 3, Archetype = 4,
    /// Specialization = 5, Summary = 6.
    /// </value>
    public int StepNumber { get; init; }

    /// <summary>
    /// Gets the total number of steps in the workflow (always 6).
    /// </summary>
    /// <value>
    /// Fixed at 6. Used alongside <see cref="StepNumber"/> for progress display.
    /// </value>
    public int TotalSteps { get; init; }

    // ═══════════════════════════════════════════════════════════════════════════
    // NAVIGATION STATE
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets whether the user can navigate to the previous step.
    /// </summary>
    /// <value>
    /// <c>false</c> for Step 1 (Lineage); <c>true</c> for all other steps.
    /// </value>
    public bool CanGoBack { get; init; }

    /// <summary>
    /// Gets whether the user can navigate to the next step.
    /// </summary>
    /// <value>
    /// <c>true</c> when the current step's requirements are met (as determined
    /// by <c>CharacterCreationState.IsStepComplete()</c>); <c>false</c> otherwise.
    /// </value>
    /// <remarks>
    /// This is a computed value based on step completion status. For example,
    /// at Step 1 it becomes <c>true</c> when a lineage is selected (and for
    /// Clan-Born, when the flexible attribute bonus is also selected).
    /// </remarks>
    public bool CanGoForward { get; init; }

    /// <summary>
    /// Gets whether the current step represents a permanent, irreversible choice.
    /// </summary>
    /// <value>
    /// <c>true</c> only for Step 4 (Archetype); <c>false</c> for all other steps.
    /// </value>
    /// <remarks>
    /// When this is <c>true</c>, the TUI displays a prominent warning before
    /// the player confirms their selection. The warning text is available in
    /// <see cref="PermanentWarning"/>.
    /// </remarks>
    public bool IsPermanentChoice { get; init; }

    /// <summary>
    /// Gets the warning message for permanent choices, if applicable.
    /// </summary>
    /// <value>
    /// The permanent choice warning text for the Archetype step, or <c>null</c>
    /// for all other steps. When non-null, the TUI renders this text prominently.
    /// </value>
    public string? PermanentWarning { get; init; }

    // ═══════════════════════════════════════════════════════════════════════════
    // CURRENT SELECTIONS DISPLAY
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the display name of the selected lineage, if any.
    /// </summary>
    /// <value>
    /// The lineage display name (e.g., "Clan-Born", "Rune-Marked"), or <c>null</c>
    /// if no lineage has been selected yet.
    /// </value>
    public string? SelectedLineageName { get; init; }

    /// <summary>
    /// Gets the display name of the selected background, if any.
    /// </summary>
    /// <value>
    /// The background display name (e.g., "Village Smith", "Clan Guard"), or <c>null</c>
    /// if no background has been selected yet.
    /// </value>
    public string? SelectedBackgroundName { get; init; }

    /// <summary>
    /// Gets the display name of the selected archetype, if any.
    /// </summary>
    /// <value>
    /// The archetype display name (e.g., "Warrior", "Mystic"), or <c>null</c>
    /// if no archetype has been selected yet.
    /// </value>
    public string? SelectedArchetypeName { get; init; }

    /// <summary>
    /// Gets the display name of the selected specialization, if any.
    /// </summary>
    /// <value>
    /// The specialization display name (e.g., "Berserkr", "Seiðkona"), or <c>null</c>
    /// if no specialization has been selected yet.
    /// </value>
    public string? SelectedSpecializationName { get; init; }

    /// <summary>
    /// Gets the character name entered in Step 6, if any.
    /// </summary>
    /// <value>
    /// The character name string, or <c>null</c> if no name has been entered.
    /// Subject to validation rules (2-20 characters, ASCII letters/spaces/hyphens).
    /// </value>
    public string? CharacterName { get; init; }

    // ═══════════════════════════════════════════════════════════════════════════
    // PREVIEW DATA
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the derived stats preview based on current selections.
    /// </summary>
    /// <value>
    /// A <see cref="DerivedStatsPreview"/> with calculated HP, Stamina, and AP values,
    /// or <c>null</c> when no attributes have been allocated yet.
    /// </value>
    /// <remarks>
    /// Updated in real-time during Step 3 (Attribute Allocation) as the player
    /// adjusts attribute values. Shows the impact of lineage and archetype bonuses
    /// on resource pools.
    /// </remarks>
    public DerivedStatsPreview? DerivedStatsPreview { get; init; }

    /// <summary>
    /// Gets the abilities preview based on archetype and specialization.
    /// </summary>
    /// <value>
    /// An <see cref="AbilitiesPreview"/> listing archetype and specialization abilities,
    /// or <c>null</c> when no archetype has been selected yet.
    /// </value>
    /// <remarks>
    /// Populated in two stages: archetype abilities from Step 4, then specialization
    /// Tier 1 abilities from Step 5.
    /// </remarks>
    public AbilitiesPreview? AbilitiesPreview { get; init; }

    /// <summary>
    /// Gets the equipment preview based on background selection.
    /// </summary>
    /// <value>
    /// An <see cref="EquipmentPreview"/> listing starting equipment items,
    /// or <c>null</c> when no background has been selected yet.
    /// </value>
    public EquipmentPreview? EquipmentPreview { get; init; }

    // ═══════════════════════════════════════════════════════════════════════════
    // VALIDATION
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the list of validation errors for the current step.
    /// </summary>
    /// <value>
    /// A read-only list of error message strings. Empty when no errors exist.
    /// Never null — defaults to an empty array for <see cref="Empty"/>.
    /// </value>
    public IReadOnlyList<string> ValidationErrors { get; init; }

    /// <summary>
    /// Gets whether the current step has passed validation.
    /// </summary>
    /// <value>
    /// <c>true</c> when the current step is complete (all required selections
    /// present) and there are no validation errors; <c>false</c> otherwise.
    /// </value>
    public bool IsCurrentStepValid { get; init; }

    // ═══════════════════════════════════════════════════════════════════════════
    // COMPUTED PROPERTIES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets a progress indicator string for the step header.
    /// </summary>
    /// <value>
    /// A string in the format "Step N of 6" (e.g., "Step 3 of 6").
    /// </value>
    public string ProgressIndicator => $"Step {StepNumber} of {TotalSteps}";

    /// <summary>
    /// Gets whether any selections have been made in the workflow.
    /// </summary>
    /// <value>
    /// <c>true</c> if any selection display name is non-null; <c>false</c> otherwise.
    /// </value>
    public bool HasAnySelections =>
        SelectedLineageName != null ||
        SelectedBackgroundName != null ||
        SelectedArchetypeName != null ||
        SelectedSpecializationName != null ||
        CharacterName != null;

    /// <summary>
    /// Gets whether any preview data is available.
    /// </summary>
    /// <value>
    /// <c>true</c> if any of the three preview objects are non-null;
    /// <c>false</c> otherwise.
    /// </value>
    public bool HasPreviewData =>
        DerivedStatsPreview != null ||
        AbilitiesPreview != null ||
        EquipmentPreview != null;

    // ═══════════════════════════════════════════════════════════════════════════
    // STATIC PROPERTIES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates an empty ViewModel for initialization.
    /// </summary>
    /// <value>
    /// A <see cref="CharacterCreationViewModel"/> initialized to the Lineage step
    /// with empty display fields, disabled navigation, no preview data, and
    /// no validation errors.
    /// </value>
    /// <remarks>
    /// Used as a default value when the ViewModelBuilder has not yet been called
    /// or as a fallback for error scenarios.
    /// </remarks>
    public static CharacterCreationViewModel Empty => new()
    {
        CurrentStep = CharacterCreationStep.Lineage,
        StepTitle = string.Empty,
        StepDescription = string.Empty,
        StepNumber = 1,
        TotalSteps = 6,
        CanGoBack = false,
        CanGoForward = false,
        IsPermanentChoice = false,
        PermanentWarning = null,
        SelectedLineageName = null,
        SelectedBackgroundName = null,
        SelectedArchetypeName = null,
        SelectedSpecializationName = null,
        CharacterName = null,
        DerivedStatsPreview = null,
        AbilitiesPreview = null,
        EquipmentPreview = null,
        ValidationErrors = Array.Empty<string>(),
        IsCurrentStepValid = false
    };

    // ═══════════════════════════════════════════════════════════════════════════
    // DEBUGGING
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Returns a formatted string representation of the ViewModel for debugging.
    /// </summary>
    /// <returns>
    /// A string showing step, navigation state, and selection summary.
    /// </returns>
    public override string ToString() =>
        $"ViewModel [Step {StepNumber}/{TotalSteps}: {StepTitle} | " +
        $"Back={CanGoBack} Fwd={CanGoForward} Perm={IsPermanentChoice} | " +
        $"Valid={IsCurrentStepValid} Errors={ValidationErrors?.Count ?? 0}]";
}
