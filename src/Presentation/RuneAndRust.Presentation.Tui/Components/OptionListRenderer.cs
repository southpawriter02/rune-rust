// ═══════════════════════════════════════════════════════════════════════════════
// OptionListRenderer.cs
// Shared UI component that builds Spectre.Console SelectionPrompt instances
// with consistent styling for the character creation wizard. Provides helpers
// for constructing option prompts with navigation choices (Back/Cancel) and
// parsing selection results back into typed values.
// Version: 0.17.5f
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Presentation.Tui.Components;

using RuneAndRust.Presentation.Tui.Screens;
using Spectre.Console;

/// <summary>
/// Builds Spectre.Console selection prompts with consistent styling for creation screens.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="OptionListRenderer"/> is a static utility class that standardizes the
/// construction of <see cref="SelectionPrompt{T}"/> instances across all creation
/// screens. It ensures consistent styling (highlight color, page size), navigation
/// choices ("&lt; Back", "Cancel"), and result parsing.
/// </para>
/// <para>
/// <strong>Navigation Choices:</strong> All prompts include "Back" and "Cancel"
/// navigation options appended after the content choices. The "Back" option is
/// conditionally included based on the <c>canGoBack</c> parameter (false for
/// Step 1 / Lineage).
/// </para>
/// </remarks>
/// <seealso cref="ICreationScreen"/>
/// <seealso cref="ScreenResult"/>
public static class OptionListRenderer
{
    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTANTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// The display text for the "Back" navigation choice.
    /// </summary>
    public const string BackChoice = "< Back";

    /// <summary>
    /// The display text for the "Cancel" navigation choice.
    /// </summary>
    public const string CancelChoice = "Cancel Creation";

    // ═══════════════════════════════════════════════════════════════════════════
    // PROMPT BUILDING
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a styled selection prompt with navigation choices appended.
    /// </summary>
    /// <param name="title">The prompt title displayed above the choices.</param>
    /// <param name="choices">The content choices to display.</param>
    /// <param name="canGoBack">
    /// Whether to include the "Back" navigation option. <c>false</c> for Step 1 (Lineage).
    /// </param>
    /// <returns>
    /// A configured <see cref="SelectionPrompt{T}"/> with content choices followed
    /// by navigation options, styled with cyan highlighting and gold1 title.
    /// </returns>
    /// <remarks>
    /// <para>
    /// The prompt is configured with:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>Page size of 15 to display more options without scrolling</description></item>
    ///   <item><description>Cyan highlight style for the currently selected option</description></item>
    ///   <item><description>Navigation choices separated visually from content choices</description></item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// var prompt = OptionListRenderer.CreatePrompt(
    ///     "Select your lineage:",
    ///     new[] { "Clan-Born — The Stable Code", "Rune-Marked — The Tainted Aether" },
    ///     canGoBack: false);
    /// var selection = console.Prompt(prompt);
    /// </code>
    /// </example>
    public static SelectionPrompt<string> CreatePrompt(
        string title,
        IReadOnlyList<string> choices,
        bool canGoBack = true)
    {
        var prompt = new SelectionPrompt<string>()
            .Title($"[gold1]{Markup.Escape(title)}[/]")
            .HighlightStyle(new Style(Color.Cyan1, decoration: Decoration.Bold))
            .PageSize(15)
            .MoreChoicesText("[grey](Move up/down to see more)[/]");

        // Add content choices
        foreach (var choice in choices)
        {
            prompt.AddChoice(choice);
        }

        // Add navigation choices
        if (canGoBack)
        {
            prompt.AddChoice(BackChoice);
        }

        prompt.AddChoice(CancelChoice);

        return prompt;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // RESULT PARSING
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Checks whether the selection is the "Back" navigation choice.
    /// </summary>
    /// <param name="selection">The user's selection from the prompt.</param>
    /// <returns><c>true</c> if the selection matches <see cref="BackChoice"/>.</returns>
    public static bool IsBack(string selection) =>
        string.Equals(selection, BackChoice, StringComparison.Ordinal);

    /// <summary>
    /// Checks whether the selection is the "Cancel" navigation choice.
    /// </summary>
    /// <param name="selection">The user's selection from the prompt.</param>
    /// <returns><c>true</c> if the selection matches <see cref="CancelChoice"/>.</returns>
    public static bool IsCancel(string selection) =>
        string.Equals(selection, CancelChoice, StringComparison.Ordinal);

    /// <summary>
    /// Converts a prompt selection into a <see cref="ScreenResult"/> for navigation choices,
    /// or returns <c>null</c> if the selection is a content choice.
    /// </summary>
    /// <param name="selection">The user's selection from the prompt.</param>
    /// <returns>
    /// <see cref="ScreenResult.GoBack"/> if the selection is "Back",
    /// <see cref="ScreenResult.Cancel"/> if "Cancel", or <c>null</c> if the
    /// selection is a content choice that needs further processing.
    /// </returns>
    /// <remarks>
    /// This helper simplifies the common pattern of checking for navigation actions
    /// before processing content selections in each screen implementation.
    /// </remarks>
    /// <example>
    /// <code>
    /// var selection = console.Prompt(prompt);
    /// var navResult = OptionListRenderer.TryParseNavigation(selection);
    /// if (navResult.HasValue)
    ///     return navResult.Value;
    /// // Process content selection...
    /// </code>
    /// </example>
    public static ScreenResult? TryParseNavigation(string selection)
    {
        if (IsBack(selection))
            return ScreenResult.GoBack();
        if (IsCancel(selection))
            return ScreenResult.Cancel();
        return null;
    }

    /// <summary>
    /// Renders validation errors from the ViewModel, if any exist.
    /// </summary>
    /// <param name="console">The Spectre.Console instance to render to.</param>
    /// <param name="errors">The list of validation error messages.</param>
    /// <remarks>
    /// Displays each error in red markup. Call this before presenting the selection
    /// prompt to show any errors from a previous failed attempt.
    /// </remarks>
    public static void RenderValidationErrors(IAnsiConsole console, IReadOnlyList<string> errors)
    {
        if (errors == null || errors.Count == 0)
            return;

        foreach (var error in errors)
        {
            console.MarkupLine($"  [red]{Markup.Escape(error)}[/]");
        }

        console.WriteLine();
    }
}
