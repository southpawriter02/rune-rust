using Microsoft.Extensions.Logging;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Core.ViewModels;
using RuneAndRust.Terminal.Rendering;
using Spectre.Console;

namespace RuneAndRust.Terminal.Services;

/// <summary>
/// Renders the full-screen Journal UI using Spectre.Console Layout (v0.3.7c).
/// Displays tabbed navigation, entry list, and detail panel with text redaction.
/// </summary>
public class JournalScreenRenderer : IJournalScreenRenderer
{
    private readonly ILogger<JournalScreenRenderer> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="JournalScreenRenderer"/> class.
    /// </summary>
    /// <param name="logger">The logger for traceability.</param>
    public JournalScreenRenderer(ILogger<JournalScreenRenderer> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public void Render(JournalViewModel vm)
    {
        _logger.LogTrace("[Journal] Rendering screen for {Character}", vm.CharacterName);

        // Build layout
        var rootLayout = new Layout("Root")
            .SplitRows(
                new Layout("Header").Size(3),
                new Layout("TabBar").Size(1),
                new Layout("Body").SplitColumns(
                    new Layout("EntryList").Ratio(4),
                    new Layout("Details").Ratio(6)
                ),
                new Layout("Footer").Size(1)
            );

        // 1. Header - Title
        rootLayout["Header"].Update(CreateHeader(vm.CharacterName));

        // 2. Tab Bar
        rootLayout["TabBar"].Update(CreateTabBar(vm.ActiveTab));

        // 3. Entry List Panel (left 40%)
        rootLayout["EntryList"].Update(CreateEntryListPanel(vm.Entries, vm.SelectedEntryIndex, vm.ActiveTab));

        // 4. Details Panel (right 60%)
        rootLayout["Details"].Update(CreateDetailsPanel(vm.SelectedDetail, vm.StressLevel, vm.ActiveTab));

        // 5. Footer - Navigation help
        rootLayout["Footer"].Update(CreateFooter());

        // Clear and render
        AnsiConsole.Clear();
        AnsiConsole.Write(rootLayout);

        _logger.LogTrace("[Journal] Render complete");
    }

    /// <summary>
    /// Creates the header panel with character name title.
    /// </summary>
    private Panel CreateHeader(string characterName)
    {
        var title = new Rule($"[bold gold1]THE ARCHIVE - {Markup.Escape(characterName)}[/]")
        {
            Justification = Justify.Center,
            Style = Style.Parse("gold1")
        };
        return new Panel(title).Border(BoxBorder.None);
    }

    /// <summary>
    /// Creates the tab bar showing all available tabs with active highlight.
    /// </summary>
    private static Markup CreateTabBar(JournalTab activeTab)
    {
        var tabs = new List<string>();

        foreach (var tab in Enum.GetValues<JournalTab>())
        {
            var isActive = tab == activeTab;
            var color = JournalViewHelper.GetTabColor(tab, isActive);
            var hotkey = JournalViewHelper.GetTabHotkey(tab);
            var name = JournalViewHelper.GetTabDisplayName(tab);

            var format = isActive
                ? $"[bold {color}][[{hotkey}]]{name}[/]"
                : $"[{color}][[{hotkey}]]{name.ToLower()}[/]";

            tabs.Add(format);
        }

        return new Markup("  " + string.Join("  ", tabs));
    }

    /// <summary>
    /// Creates the entry list panel showing discovered entries for the active tab.
    /// </summary>
    private Panel CreateEntryListPanel(List<JournalEntryView> entries, int selectedIndex, JournalTab tab)
    {
        var tabColor = JournalViewHelper.GetTabColor(tab, true);
        var tabName = JournalViewHelper.GetTabDisplayName(tab).ToUpperInvariant();

        if (entries.Count == 0)
        {
            var emptyMessage = tab == JournalTab.Contracts
                ? "[grey](Contracts coming soon...)[/]"
                : "[grey](No entries discovered)[/]";

            return new Panel(new Markup(emptyMessage))
            {
                Header = new PanelHeader($"[bold {tabColor}]{tabName}[/]"),
                Border = BoxBorder.Rounded,
                BorderStyle = Style.Parse(tabColor)
            };
        }

        var rows = new List<Markup>();
        for (int i = 0; i < entries.Count; i++)
        {
            var entry = entries[i];
            var isSelected = i == selectedIndex;
            var selector = isSelected ? "[bold yellow]>[/] " : "  ";
            var completionIndicator = JournalViewHelper.GetCompletionIndicator(entry.IsComplete);
            var completionPct = JournalViewHelper.FormatCompletionPercent(entry.CompletionPercent);

            var titleColor = entry.IsComplete ? "white" : "grey";
            rows.Add(new Markup($"{selector}{entry.Index,2}. {completionIndicator} [{titleColor}]{Markup.Escape(entry.Title)}[/] {completionPct}"));
        }

        return new Panel(new Rows(rows))
        {
            Header = new PanelHeader($"[bold {tabColor}]{tabName}[/]"),
            Border = BoxBorder.Rounded,
            BorderStyle = Style.Parse(tabColor)
        };
    }

    /// <summary>
    /// Creates the details panel showing selected entry information with redacted content.
    /// </summary>
    private Panel CreateDetailsPanel(JournalEntryDetailView? details, int stressLevel, JournalTab tab)
    {
        if (details == null)
        {
            var emptyMessage = tab == JournalTab.Contracts
                ? "[grey](Select a contract to view details)[/]"
                : "[grey](Select an entry to view details)[/]";

            return new Panel(new Markup(emptyMessage))
            {
                Header = new PanelHeader("[bold white]DETAILS[/]"),
                Border = BoxBorder.Rounded,
                BorderStyle = Style.Parse("grey")
            };
        }

        var categoryColor = JournalViewHelper.GetCategoryColor(details.Category);
        var categoryIcon = JournalViewHelper.GetCategoryIcon(details.Category);
        var categoryName = JournalViewHelper.GetCategoryDisplayName(details.Category);
        var completionColor = JournalViewHelper.GetCompletionColor(details.CompletionPercent);
        var fragmentProgress = JournalViewHelper.FormatFragmentProgress(details.FragmentsCollected, details.FragmentsRequired);

        var content = new List<Markup>
        {
            new Markup($"[bold white]{Markup.Escape(details.Title.ToUpperInvariant())}[/]"),
            new Markup(""),
            new Markup($"[bold]Category:[/] [{categoryColor}]{categoryIcon} {categoryName}[/]"),
            new Markup($"[bold]Progress:[/] {fragmentProgress} [{completionColor}]({details.CompletionPercent}%)[/]"),
            new Markup("")
        };

        // Add redacted content (apply glitch effect if stressed)
        var displayContent = stressLevel > 50
            ? JournalViewHelper.ApplyGlitchEffect(details.RedactedContent, stressLevel)
            : details.RedactedContent;

        // Split content into lines for proper display
        var contentLines = displayContent.Split('\n');
        foreach (var line in contentLines)
        {
            content.Add(new Markup(Markup.Escape(line)));
        }

        // Add unlocked thresholds if any
        if (details.UnlockedThresholds.Count > 0)
        {
            content.Add(new Markup(""));
            content.Add(new Markup("[bold cyan]Discoveries:[/]"));
            foreach (var threshold in details.UnlockedThresholds)
            {
                var formattedThreshold = JournalViewHelper.FormatThreshold(threshold);
                content.Add(new Markup($"  [green]\u2713[/] {formattedThreshold}"));
            }
        }

        return new Panel(new Rows(content))
        {
            Header = new PanelHeader("[bold white]DETAILS[/]"),
            Border = BoxBorder.Rounded,
            BorderStyle = Style.Parse("white")
        };
    }

    /// <summary>
    /// Creates the footer showing available commands.
    /// </summary>
    private static Markup CreateFooter()
    {
        return new Markup("[grey][[\u2191/\u2193]] Navigate  [[C/B/F/Q]] Switch Tab  [[ESC]] Close[/]");
    }
}
