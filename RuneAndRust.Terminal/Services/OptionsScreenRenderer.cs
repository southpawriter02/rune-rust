using Microsoft.Extensions.Logging;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Core.ViewModels;
using RuneAndRust.Terminal.Rendering;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace RuneAndRust.Terminal.Services;

/// <summary>
/// Renders the full-screen Options UI using Spectre.Console Layout (v0.3.10b, extended v0.3.10c).
/// Displays tabbed navigation, settings list with visual controls, and command legend.
/// </summary>
public class OptionsScreenRenderer : IOptionsScreenRenderer
{
    private readonly ILogger<OptionsScreenRenderer> _logger;
    private readonly IThemeService _themeService;

    /// <summary>
    /// Initializes a new instance of the <see cref="OptionsScreenRenderer"/> class.
    /// </summary>
    /// <param name="logger">The logger for traceability.</param>
    /// <param name="themeService">The theme service for color resolution.</param>
    public OptionsScreenRenderer(ILogger<OptionsScreenRenderer> logger, IThemeService themeService)
    {
        _logger = logger;
        _themeService = themeService;
    }

    /// <inheritdoc />
    public void Render(OptionsViewModel vm)
    {
        _logger.LogTrace("[Options] Rendering screen, ActiveTab: {Tab}, SelectedIndex: {Index}",
            vm.ActiveTab, vm.SelectedIndex);

        // Build layout
        var rootLayout = new Layout("Root")
            .SplitRows(
                new Layout("Header").Size(3),
                new Layout("Tabs").Size(3),
                new Layout("Content"),
                new Layout("Footer").Size(3)
            );

        // Populate sections
        rootLayout["Header"].Update(CreateHeader());
        rootLayout["Tabs"].Update(CreateTabBar(vm.ActiveTab));
        rootLayout["Content"].Update(
            vm.ActiveTab == OptionsTab.Controls
                ? CreateControlsPanel(vm)
                : CreateSettingsPanel(vm));
        rootLayout["Footer"].Update(CreateFooter(vm.ActiveTab));

        // Clear and render
        AnsiConsole.Clear();
        AnsiConsole.Write(rootLayout);

        _logger.LogTrace("[Options] Render complete");
    }

    /// <summary>
    /// Creates the header panel with the OPTIONS title.
    /// </summary>
    private Panel CreateHeader()
    {
        var title = new Rule("[bold gold1]OPTIONS[/]")
        {
            Justification = Justify.Center,
            Style = Style.Parse("gold1")
        };
        return new Panel(title).Border(BoxBorder.None);
    }

    /// <summary>
    /// Creates the tab bar showing all available tabs with the active one highlighted.
    /// </summary>
    private Panel CreateTabBar(OptionsTab activeTab)
    {
        var tabs = Enum.GetValues<OptionsTab>();
        var tabStrings = new List<string>();

        foreach (var tab in tabs)
        {
            var name = OptionsViewHelper.GetTabDisplayName(tab);
            if (tab == activeTab)
            {
                tabStrings.Add($"[bold gold1][ {name} ][/]");
            }
            else
            {
                tabStrings.Add($"[grey]  {name}  [/]");
            }
        }

        var tabLine = string.Join("  ", tabStrings);
        var markup = new Markup(tabLine);

        return new Panel(markup)
        {
            Border = BoxBorder.None
        };
    }

    /// <summary>
    /// Creates the main settings panel with the list of settings for the active tab.
    /// </summary>
    private Panel CreateSettingsPanel(OptionsViewModel vm)
    {
        if (vm.CurrentItems.Count == 0)
        {
            return new Panel(new Markup("[grey](No settings in this tab)[/]"))
            {
                Header = new PanelHeader($"[bold white]{OptionsViewHelper.GetTabDisplayName(vm.ActiveTab)} Settings[/]"),
                Border = BoxBorder.Rounded,
                BorderStyle = Style.Parse("grey")
            };
        }

        var rows = new List<Markup>();

        for (int i = 0; i < vm.CurrentItems.Count; i++)
        {
            var item = vm.CurrentItems[i];
            var isSelected = i == vm.SelectedIndex;
            var selector = isSelected ? "[bold yellow]>[/] " : "  ";
            var nameColor = isSelected ? "white" : "grey";
            var valueColor = isSelected ? "white" : "grey";

            // Format based on setting type
            var valueDisplay = item.Type switch
            {
                SettingType.Toggle => item.ValueDisplay,
                SettingType.Slider when item.MinValue.HasValue && item.MaxValue.HasValue =>
                    FormatSliderWithBar(item, isSelected),
                SettingType.Enum => $"[{valueColor}]< {item.ValueDisplay} >[/]",
                SettingType.Action => $"[{valueColor}][ Press Enter ][/]",
                _ => $"[{valueColor}]{item.ValueDisplay}[/]"
            };

            var namePadded = item.Name.PadRight(25);
            rows.Add(new Markup($"{selector}[{nameColor}]{namePadded}[/] {valueDisplay}"));
        }

        return new Panel(new Rows(rows))
        {
            Header = new PanelHeader($"[bold white]{OptionsViewHelper.GetTabDisplayName(vm.ActiveTab)} Settings[/]"),
            Border = BoxBorder.Rounded,
            BorderStyle = Style.Parse("white"),
            Padding = new Padding(1, 1)
        };
    }

    /// <summary>
    /// Formats a slider setting with a visual bar and value.
    /// </summary>
    private static string FormatSliderWithBar(SettingItemView item, bool isSelected)
    {
        // Parse the current value from ValueDisplay (which might include units)
        var valueStr = item.ValueDisplay.Replace("%", "").Replace(" min", "").Trim();
        if (!int.TryParse(valueStr, out var value))
        {
            return item.ValueDisplay;
        }

        var bar = OptionsViewHelper.RenderSlider(value, item.MinValue!.Value, item.MaxValue!.Value, 15);
        var color = isSelected ? "white" : "grey";
        return $"{bar} [{color}]{item.ValueDisplay}[/]";
    }

    /// <summary>
    /// Creates the Controls panel showing key bindings grouped by category (v0.3.10c).
    /// </summary>
    private Panel CreateControlsPanel(OptionsViewModel vm)
    {
        if (vm.Bindings.Count == 0)
        {
            return new Panel(new Markup("[grey](No bindings defined)[/]"))
            {
                Header = new PanelHeader("[bold white]Key Bindings[/]"),
                Border = BoxBorder.Rounded,
                BorderStyle = Style.Parse("grey")
            };
        }

        var rows = new List<IRenderable>();
        string? currentCategory = null;

        for (int i = 0; i < vm.Bindings.Count; i++)
        {
            var binding = vm.Bindings[i];
            var isSelected = i == vm.SelectedIndex;

            // Add category header if changed
            if (binding.Category != currentCategory)
            {
                if (currentCategory != null)
                {
                    rows.Add(new Text("")); // Spacer
                }
                rows.Add(new Markup($"[bold grey]{binding.Category}[/]"));
                currentCategory = binding.Category;
            }

            var selector = isSelected ? "[bold yellow]>[/] " : "  ";
            var nameColor = isSelected ? "white" : "grey";
            var namePadded = binding.ActionName.PadRight(20);

            rows.Add(new Markup($"{selector}[{nameColor}]{namePadded}[/] {binding.KeyDisplay}"));
        }

        return new Panel(new Rows(rows))
        {
            Header = new PanelHeader("[bold white]Key Bindings[/]"),
            Border = BoxBorder.Rounded,
            BorderStyle = Style.Parse("white"),
            Padding = new Padding(1, 1)
        };
    }

    /// <summary>
    /// Creates the footer showing available commands.
    /// </summary>
    private static Panel CreateFooter(OptionsTab activeTab)
    {
        var commandText = activeTab == OptionsTab.Controls
            ? "[grey][[Tab]] Switch Tab  [[↑↓]] Navigate  [[Enter]] Rebind  [[ESC]] Save & Close[/]"
            : "[grey][[Tab]] Switch Tab  [[↑↓]] Navigate  [[←→]] Adjust  [[Enter]] Toggle/Action  [[ESC]] Save & Close[/]";

        var commands = new Markup(commandText);
        return new Panel(commands).Border(BoxBorder.None);
    }
}
