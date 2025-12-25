using Microsoft.Extensions.Logging;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Core.ViewModels;
using RuneAndRust.Terminal.Rendering;
using Spectre.Console;

namespace RuneAndRust.Terminal.Services;

/// <summary>
/// Renders the full-screen inventory UI using Spectre.Console Layout (v0.3.7a).
/// Displays equipment panel, backpack list, burden bar, and command legend.
/// Updated with IThemeService for accessibility themes in v0.3.14a.
/// </summary>
public class InventoryScreenRenderer : IInventoryScreenRenderer
{
    private readonly ILogger<InventoryScreenRenderer> _logger;
    private readonly IThemeService _theme;

    /// <summary>
    /// Initializes a new instance of the <see cref="InventoryScreenRenderer"/> class.
    /// </summary>
    /// <param name="logger">The logger for traceability.</param>
    /// <param name="theme">The theme service for accessibility colors.</param>
    public InventoryScreenRenderer(ILogger<InventoryScreenRenderer> logger, IThemeService theme)
    {
        _logger = logger;
        _theme = theme;
    }

    /// <inheritdoc />
    public void Render(InventoryViewModel vm)
    {
        _logger.LogTrace("[Inventory] Rendering screen for {Character}", vm.CharacterName);

        // Build layout
        var rootLayout = new Layout("Root")
            .SplitRows(
                new Layout("Header").Size(3),
                new Layout("Body").SplitColumns(
                    new Layout("Equipment").Ratio(3),
                    new Layout("Backpack").Ratio(7)
                ),
                new Layout("BurdenBar").Size(3),
                new Layout("Footer").Size(1)
            );

        // 1. Header - Title
        rootLayout["Header"].Update(CreateHeader(vm.CharacterName));

        // 2. Equipment Panel (left 30%)
        rootLayout["Equipment"].Update(CreateEquipmentPanel(vm.EquippedItems));

        // 3. Backpack Panel (right 70%)
        rootLayout["Backpack"].Update(CreateBackpackPanel(vm.BackpackItems, vm.SelectedIndex));

        // 4. Burden Bar
        rootLayout["BurdenBar"].Update(CreateBurdenPanel(vm));

        // 5. Footer - Commands
        rootLayout["Footer"].Update(CreateFooter());

        // Clear and render
        AnsiConsole.Clear();
        AnsiConsole.Write(rootLayout);

        _logger.LogTrace("[Inventory] Render complete");
    }

    /// <summary>
    /// Creates the header panel with character name title.
    /// Updated with themed colors in v0.3.14a.
    /// </summary>
    private Panel CreateHeader(string characterName)
    {
        var headerColor = _theme.GetColor("HeaderColor");
        var title = new Rule($"[bold {headerColor}]THE PACK - {Markup.Escape(characterName)}[/]")
        {
            Justification = Justify.Center,
            Style = Style.Parse(headerColor)
        };
        return new Panel(title).Border(BoxBorder.None);
    }

    /// <summary>
    /// Creates the equipment panel showing all 7 equipment slots.
    /// Updated with themed colors in v0.3.14a.
    /// </summary>
    private Panel CreateEquipmentPanel(Dictionary<EquipmentSlot, EquippedItemView?> equipped)
    {
        var rows = new List<Markup>();
        var dimColor = _theme.GetColor("DimColor");
        var labelColor = _theme.GetColor("LabelColor");
        var errorColor = _theme.GetColor("ErrorColor");
        var warningColor = _theme.GetColor("WarningColor");
        var playerColor = _theme.GetColor("PlayerColor");

        foreach (var slot in Enum.GetValues<EquipmentSlot>())
        {
            var slotName = InventoryViewHelper.GetSlotDisplayName(slot).PadRight(10);
            var item = equipped.TryGetValue(slot, out var view) ? view : null;

            if (item == null)
            {
                rows.Add(new Markup($"[{dimColor}]{slotName}:[/] [{dimColor}]---[/]"));
            }
            else
            {
                var color = InventoryViewHelper.GetQualityColor(item.Quality, _theme);
                var brokenMarker = item.IsBroken ? $" [{errorColor}](BROKEN)[/]" : "";
                var durability = item.DurabilityPercentage < 50
                    ? $" [{warningColor}]({item.DurabilityPercentage}%)[/]"
                    : "";
                rows.Add(new Markup($"[{labelColor}]{slotName}:[/] [{color}]{Markup.Escape(item.Name)}[/]{durability}{brokenMarker}"));
            }
        }

        return new Panel(new Rows(rows))
        {
            Header = new PanelHeader($"[bold {playerColor}]EQUIPPED[/]"),
            Border = BoxBorder.Rounded,
            BorderStyle = Style.Parse(playerColor)
        };
    }

    /// <summary>
    /// Creates the backpack panel showing non-equipped items.
    /// Updated with themed colors in v0.3.14a.
    /// </summary>
    private Panel CreateBackpackPanel(List<BackpackItemView> items, int selectedIndex)
    {
        var dimColor = _theme.GetColor("DimColor");
        var labelColor = _theme.GetColor("LabelColor");
        var activeColor = _theme.GetColor("ActiveColor");

        if (items.Count == 0)
        {
            return new Panel(new Markup($"[{dimColor}](empty)[/]"))
            {
                Header = new PanelHeader($"[bold {labelColor}]BACKPACK[/]"),
                Border = BoxBorder.Rounded,
                BorderStyle = Style.Parse(dimColor)
            };
        }

        var rows = new List<Markup>();
        for (int i = 0; i < items.Count; i++)
        {
            var item = items[i];
            var isSelected = i == selectedIndex;
            var selector = isSelected ? $"[bold {activeColor}]>[/] " : "  ";
            var color = InventoryViewHelper.GetQualityColor(item.Quality, _theme);
            var icon = InventoryViewHelper.GetItemTypeIcon(item.ItemType, _theme);
            var name = InventoryViewHelper.FormatItemWithQuantity(item.Name, item.Quantity);
            var weight = InventoryViewHelper.FormatWeight(item.WeightGrams);
            var equipMarker = item.IsEquipable ? $"[{dimColor}]E[/]" : " ";

            rows.Add(new Markup($"{selector}{item.Index,2}. {icon} [{color}]{Markup.Escape(name)}[/] [{dimColor}]({weight})[/] {equipMarker}"));
        }

        return new Panel(new Rows(rows))
        {
            Header = new PanelHeader($"[bold {labelColor}]BACKPACK[/]"),
            Border = BoxBorder.Rounded,
            BorderStyle = Style.Parse(labelColor)
        };
    }

    /// <summary>
    /// Creates the burden bar panel showing weight status.
    /// Updated with themed colors in v0.3.14a.
    /// </summary>
    private Panel CreateBurdenPanel(InventoryViewModel vm)
    {
        var burdenColor = InventoryViewHelper.GetBurdenColor(vm.BurdenState, _theme);
        var bar = InventoryViewHelper.RenderBurdenBar(vm.BurdenPercentage);
        var stateLabel = vm.BurdenState.ToString();
        var weightStr = $"{InventoryViewHelper.FormatWeight(vm.CurrentWeight)} / {InventoryViewHelper.FormatWeight(vm.MaxCapacity)}";
        var dimColor = _theme.GetColor("DimColor");

        var content = new Markup(
            $"[bold]BURDEN:[/] [{burdenColor}]{bar}[/] [{burdenColor}]{vm.BurdenPercentage}%[/] [{dimColor}]({stateLabel})[/]\n" +
            $"[{dimColor}]Weight: {weightStr}[/]"
        );

        return new Panel(content).Border(BoxBorder.None);
    }

    /// <summary>
    /// Creates the footer showing available commands.
    /// Updated with themed colors in v0.3.14a.
    /// </summary>
    private Markup CreateFooter()
    {
        var dimColor = _theme.GetColor("DimColor");
        return new Markup($"[{dimColor}][[\u2191/\u2193]] Navigate  [[E]]quip  [[D]]rop  [[I]]nspect  [[ESC]] Close[/]");
    }
}
