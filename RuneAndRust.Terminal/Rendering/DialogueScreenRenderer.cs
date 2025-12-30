using Microsoft.Extensions.Logging;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Core.ViewModels;
using Spectre.Console;

namespace RuneAndRust.Terminal.Rendering;

/// <summary>
/// Renders the dialogue TUI using Spectre.Console (v0.4.2d).
/// Displays NPC speaker, dialogue text, and selectable options with keyboard navigation.
/// </summary>
/// <remarks>See: v0.4.2d (The Parley) for Dialogue TUI implementation.</remarks>
public class DialogueScreenRenderer : IDialogueScreenRenderer
{
    private readonly IThemeService _theme;
    private readonly ILogger<DialogueScreenRenderer> _logger;

    /// <summary>
    /// Maximum width for dialogue text before wrapping.
    /// </summary>
    private const int MaxTextWidth = 70;

    /// <summary>
    /// Initializes a new instance of the <see cref="DialogueScreenRenderer"/> class.
    /// </summary>
    /// <param name="theme">The theme service for color configuration.</param>
    /// <param name="logger">The logger for traceability.</param>
    public DialogueScreenRenderer(IThemeService theme, ILogger<DialogueScreenRenderer> logger)
    {
        _theme = theme ?? throw new ArgumentNullException(nameof(theme));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public void Render(DialogueTuiViewModel vm)
    {
        AnsiConsole.Clear();

        // Header - NPC name and title
        RenderHeader(vm.SpeakerDisplay);

        // Dialogue text panel
        RenderDialogueText(vm.Text, vm.SpeakerName);

        // Options list with selection
        RenderOptions(vm.Options, vm.SelectedIndex);

        // Footer - Control hints
        RenderFooter();

        _logger.LogTrace("[Dialogue TUI] Rendered. Selected: {Index}, Options: {Count}",
            vm.SelectedIndex, vm.OptionCount);
    }

    /// <inheritdoc/>
    public void PlayLockedFeedback(string lockReason)
    {
        // Brief visual feedback for attempting to select locked option
        AnsiConsole.MarkupLine($"\n[red]Cannot select: {Markup.Escape(lockReason)}[/]");
        Thread.Sleep(500); // Brief pause for user to see message
    }

    // ═══════════════════════════════════════════════════════════════════════
    // Private Render Methods
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Renders the header with speaker name and decorative rule.
    /// </summary>
    private void RenderHeader(string speakerDisplay)
    {
        var speakerColor = _theme.GetColor("DialogueSpeakerColor") ?? "cyan";

        AnsiConsole.Write(new Rule($"[bold {speakerColor}]\u2726 {Markup.Escape(speakerDisplay)} \u2726[/]")
            .RuleStyle(speakerColor));
        AnsiConsole.WriteLine();
    }

    /// <summary>
    /// Renders the dialogue text in a panel with word wrapping.
    /// </summary>
    private void RenderDialogueText(string text, string speakerName)
    {
        var textColor = _theme.GetColor("DialogueTextColor") ?? "white";

        // Word-wrap the text
        var wrappedText = WrapText(text, MaxTextWidth);

        // Create dialogue panel
        var panel = new Panel(new Markup($"[{textColor}]{Markup.Escape(wrappedText)}[/]"))
            .Header($"[grey]{Markup.Escape(speakerName)} says:[/]")
            .Border(BoxBorder.Rounded)
            .BorderColor(Color.Grey)
            .Padding(1, 0, 1, 0)
            .Expand();

        AnsiConsole.Write(panel);
        AnsiConsole.WriteLine();
    }

    /// <summary>
    /// Renders the options list with selection indicator and lock status.
    /// </summary>
    private void RenderOptions(IReadOnlyList<DialogueOptionTuiViewModel> options, int selectedIndex)
    {
        if (options.Count == 0)
        {
            AnsiConsole.MarkupLine("[grey](No responses available)[/]");
            return;
        }

        var selectedColor = _theme.GetColor("DialogueSelectedColor") ?? "yellow";
        var availableColor = _theme.GetColor("DialogueOptionColor") ?? "white";
        var lockedColor = _theme.GetColor("DialogueLockedColor") ?? "dim grey";

        for (int i = 0; i < options.Count; i++)
        {
            var option = options[i];
            var isSelected = i == selectedIndex;

            // Selection indicator
            var indicator = isSelected ? ">" : " ";

            // Determine color based on state
            string color;

            if (!option.IsAvailable)
            {
                color = lockedColor;
            }
            else if (isSelected)
            {
                color = selectedColor;
            }
            else
            {
                color = availableColor;
            }

            // Build the option line
            var numberLabel = i + 1; // 1-indexed for display
            var optionText = Markup.Escape(option.Text);

            if (option.IsAvailable)
            {
                if (isSelected)
                {
                    AnsiConsole.MarkupLine($"[bold {color}]{indicator} {numberLabel}. {optionText}[/]");
                }
                else
                {
                    AnsiConsole.MarkupLine($"[{color}]  {numberLabel}. {optionText}[/]");
                }
            }
            else
            {
                // Locked option with hint - use [[ ]] to escape literal brackets in Spectre.Console
                var lockReason = Markup.Escape(option.LockReason ?? "LOCKED");
                AnsiConsole.MarkupLine(
                    $"[{color}]  {numberLabel}. [[{lockReason}]] {optionText}[/]");
            }
        }

        AnsiConsole.WriteLine();
    }

    /// <summary>
    /// Renders the footer with control hints.
    /// </summary>
    private static void RenderFooter()
    {
        AnsiConsole.MarkupLine("[dim]\u2191\u2193 Navigate  \u2502  Enter: Select  \u2502  1-9: Quick Select  \u2502  Esc: Exit[/]");
    }

    /// <summary>
    /// Wraps text to the specified width.
    /// </summary>
    private static string WrapText(string text, int maxWidth)
    {
        if (string.IsNullOrEmpty(text) || text.Length <= maxWidth)
            return text;

        var words = text.Split(' ');
        var lines = new List<string>();
        var currentLine = "";

        foreach (var word in words)
        {
            if (currentLine.Length + word.Length + 1 <= maxWidth)
            {
                currentLine += (currentLine.Length > 0 ? " " : "") + word;
            }
            else
            {
                if (currentLine.Length > 0)
                    lines.Add(currentLine);
                currentLine = word;
            }
        }

        if (currentLine.Length > 0)
            lines.Add(currentLine);

        return string.Join("\n", lines);
    }
}
