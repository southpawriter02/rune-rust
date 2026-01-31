// ═══════════════════════════════════════════════════════════════════════════════
// StepHeaderRenderer.cs
// Shared UI component that renders the step header for the character creation
// wizard. Displays the step number, title, thematic description, and permanent
// choice warning (for the Archetype step). Uses Spectre.Console Rule and Panel
// for consistent styling across all six creation steps.
// Version: 0.17.5f
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Presentation.Tui.Components;

using RuneAndRust.Domain.ValueObjects;
using Spectre.Console;

/// <summary>
/// Renders the step header for the character creation wizard.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="StepHeaderRenderer"/> is a static utility class that provides consistent
/// step header rendering across all six creation screens. The header includes:
/// </para>
/// <list type="number">
///   <item><description>A centered rule with the step number and title in uppercase (e.g., "STEP 3 OF 6: ALLOCATE ATTRIBUTES")</description></item>
///   <item><description>An italic thematic description below the rule (e.g., "Define your character's core capabilities.")</description></item>
///   <item><description>A prominent yellow warning panel for permanent choices (Step 4: Archetype only)</description></item>
/// </list>
/// <para>
/// The renderer reads all display data from the <see cref="CharacterCreationViewModel"/>
/// and writes to the provided <see cref="IAnsiConsole"/>. It does not maintain any state.
/// </para>
/// </remarks>
/// <seealso cref="CharacterCreationViewModel"/>
public static class StepHeaderRenderer
{
    // ═══════════════════════════════════════════════════════════════════════════
    // PUBLIC METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Renders the step header including title, description, and optional permanent warning.
    /// </summary>
    /// <param name="console">The Spectre.Console instance to render to.</param>
    /// <param name="viewModel">
    /// The current <see cref="CharacterCreationViewModel"/> containing step display data.
    /// </param>
    /// <remarks>
    /// <para>
    /// The header is rendered in three parts:
    /// </para>
    /// <list type="number">
    ///   <item>
    ///     <description>
    ///       <strong>Step Rule:</strong> A horizontal rule with centered text showing
    ///       "STEP N OF 6: STEP TITLE" in yellow. The title is converted to uppercase.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       <strong>Description:</strong> The thematic step description in grey italic,
    ///       followed by a blank line for visual separation.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       <strong>Permanent Warning:</strong> If <see cref="CharacterCreationViewModel.IsPermanentChoice"/>
    ///       is <c>true</c> and <see cref="CharacterCreationViewModel.PermanentWarning"/>
    ///       is non-null, a yellow bordered panel with the warning text is displayed.
    ///       This only applies to Step 4 (Archetype).
    ///     </description>
    ///   </item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// StepHeaderRenderer.Render(AnsiConsole.Console, viewModel);
    /// // Output:
    /// // ────── STEP 4 OF 6: CHOOSE YOUR ARCHETYPE ──────
    /// //   Your fundamental approach to survival.
    /// //
    /// // ┌─── WARNING ──────────────────────────────────────┐
    /// // │ This choice is PERMANENT and cannot be changed.   │
    /// // └──────────────────────────────────────────────────┘
    /// </code>
    /// </example>
    public static void Render(IAnsiConsole console, CharacterCreationViewModel viewModel)
    {
        ArgumentNullException.ThrowIfNull(console);

        // Step number and title as centered rule
        var stepTitle = $"STEP {viewModel.StepNumber} OF {viewModel.TotalSteps}: " +
                        (viewModel.StepTitle?.ToUpperInvariant() ?? "UNKNOWN STEP");

        console.Write(new Rule($"[yellow]{Markup.Escape(stepTitle)}[/]")
        {
            Justification = Justify.Center,
            Style = Style.Parse("grey")
        });

        // Thematic description
        if (!string.IsNullOrEmpty(viewModel.StepDescription))
        {
            console.MarkupLine($"  [grey italic]{Markup.Escape(viewModel.StepDescription)}[/]");
        }

        console.WriteLine();

        // Permanent choice warning (Step 4: Archetype only)
        if (viewModel.IsPermanentChoice && !string.IsNullOrEmpty(viewModel.PermanentWarning))
        {
            var warningPanel = new Panel($"[bold yellow]{Markup.Escape(viewModel.PermanentWarning)}[/]")
            {
                Border = BoxBorder.Heavy,
                BorderStyle = new Style(Color.Yellow),
                Header = new PanelHeader("[bold yellow]WARNING[/]"),
                Padding = new Padding(1, 0)
            };
            console.Write(warningPanel);
            console.WriteLine();
        }
    }

    /// <summary>
    /// Renders a progress bar showing how far through the creation workflow the user is.
    /// </summary>
    /// <param name="console">The Spectre.Console instance to render to.</param>
    /// <param name="viewModel">
    /// The current <see cref="CharacterCreationViewModel"/> containing step progress data.
    /// </param>
    /// <remarks>
    /// Renders a visual progress indicator using filled and empty characters to show
    /// the current step position relative to total steps (e.g., "■ ■ ■ □ □ □" for step 3).
    /// </remarks>
    public static void RenderProgress(IAnsiConsole console, CharacterCreationViewModel viewModel)
    {
        ArgumentNullException.ThrowIfNull(console);

        var filled = new string('■', viewModel.StepNumber);
        var empty = new string('□', viewModel.TotalSteps - viewModel.StepNumber);
        console.MarkupLine($"  [cyan]{filled}[/][grey]{empty}[/]  {viewModel.ProgressIndicator}");
        console.WriteLine();
    }
}
