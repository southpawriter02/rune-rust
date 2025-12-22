using Microsoft.Extensions.Logging;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Core.ViewModels;
using RuneAndRust.Terminal.Rendering;
using Spectre.Console;

namespace RuneAndRust.Terminal.Services;

/// <summary>
/// Renders the full-screen crafting UI using Spectre.Console Layout (v0.3.7b).
/// Displays recipe list, details panel, and trade filter navigation.
/// </summary>
public class CraftingScreenRenderer : ICraftingScreenRenderer
{
    private readonly ILogger<CraftingScreenRenderer> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="CraftingScreenRenderer"/> class.
    /// </summary>
    /// <param name="logger">The logger for traceability.</param>
    public CraftingScreenRenderer(ILogger<CraftingScreenRenderer> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public void Render(CraftingViewModel vm)
    {
        _logger.LogTrace("[Crafting] Rendering screen for {Character}", vm.CharacterName);

        // Build layout
        var rootLayout = new Layout("Root")
            .SplitRows(
                new Layout("Header").Size(3),
                new Layout("Body").SplitColumns(
                    new Layout("RecipeList").Ratio(4),
                    new Layout("Details").Ratio(6)
                ),
                new Layout("Footer").Size(1)
            );

        // 1. Header - Title with trade indicator
        rootLayout["Header"].Update(CreateHeader(vm.CharacterName, vm.SelectedTrade));

        // 2. Recipe List Panel (left 40%)
        rootLayout["RecipeList"].Update(CreateRecipeListPanel(vm.Recipes, vm.SelectedRecipeIndex, vm.SelectedTrade));

        // 3. Details Panel (right 60%)
        rootLayout["Details"].Update(CreateDetailsPanel(vm.SelectedRecipeDetails));

        // 4. Footer - Trade filter and navigation keys
        rootLayout["Footer"].Update(CreateFooter(vm.SelectedTrade));

        // Clear and render
        AnsiConsole.Clear();
        AnsiConsole.Write(rootLayout);

        _logger.LogTrace("[Crafting] Render complete");
    }

    /// <summary>
    /// Creates the header panel with character name and current trade.
    /// </summary>
    private Panel CreateHeader(string characterName, CraftingTrade trade)
    {
        var tradeColor = CraftingViewHelper.GetTradeColor(trade);
        var tradeIcon = CraftingViewHelper.GetTradeIcon(trade);
        var tradeName = CraftingViewHelper.GetTradeDisplayName(trade);

        var title = new Rule($"[bold gold1]THE BENCH - {Markup.Escape(characterName)}[/] [{tradeColor}]{tradeIcon} {tradeName}[/]")
        {
            Justification = Justify.Center,
            Style = Style.Parse("gold1")
        };
        return new Panel(title).Border(BoxBorder.None);
    }

    /// <summary>
    /// Creates the recipe list panel showing available recipes for the selected trade.
    /// </summary>
    private Panel CreateRecipeListPanel(List<RecipeView> recipes, int selectedIndex, CraftingTrade trade)
    {
        var tradeColor = CraftingViewHelper.GetTradeColor(trade);

        if (recipes.Count == 0)
        {
            return new Panel(new Markup($"[grey](No recipes for {CraftingViewHelper.GetTradeDisplayName(trade)})[/]"))
            {
                Header = new PanelHeader($"[bold {tradeColor}]RECIPES[/]"),
                Border = BoxBorder.Rounded,
                BorderStyle = Style.Parse(tradeColor)
            };
        }

        var rows = new List<Markup>();
        for (int i = 0; i < recipes.Count; i++)
        {
            var recipe = recipes[i];
            var isSelected = i == selectedIndex;
            var selector = isSelected ? "[bold yellow]>[/] " : "  ";
            var availability = CraftingViewHelper.GetAvailabilityIndicator(recipe.CanCraft);
            var diffColor = CraftingViewHelper.GetDifficultyColor(recipe.DifficultyClass, 5); // Assume avg WITS for list

            var nameColor = recipe.CanCraft ? "white" : "grey";
            rows.Add(new Markup($"{selector}{recipe.Index,2}. [{nameColor}]{Markup.Escape(recipe.Name)}[/] [{diffColor}]DC{recipe.DifficultyClass}[/] {availability}"));
        }

        return new Panel(new Rows(rows))
        {
            Header = new PanelHeader($"[bold {tradeColor}]RECIPES[/]"),
            Border = BoxBorder.Rounded,
            BorderStyle = Style.Parse(tradeColor)
        };
    }

    /// <summary>
    /// Creates the details panel showing selected recipe information.
    /// </summary>
    private Panel CreateDetailsPanel(RecipeDetailsView? details)
    {
        if (details == null)
        {
            return new Panel(new Markup("[grey](Select a recipe to view details)[/]"))
            {
                Header = new PanelHeader("[bold white]DETAILS[/]"),
                Border = BoxBorder.Rounded,
                BorderStyle = Style.Parse("grey")
            };
        }

        var tradeColor = CraftingViewHelper.GetTradeColor(details.Trade);
        var tradeIcon = CraftingViewHelper.GetTradeIcon(details.Trade);
        var tradeName = CraftingViewHelper.GetTradeDisplayName(details.Trade);
        var difficultyStr = CraftingViewHelper.FormatDifficultyWithChance(details.DifficultyClass, details.CrafterWits);
        var diffColor = CraftingViewHelper.GetDifficultyColor(details.DifficultyClass, details.CrafterWits);

        var content = new List<Markup>
        {
            new Markup($"[bold white]{Markup.Escape(details.Name)}[/]"),
            new Markup($"[grey]{Markup.Escape(details.Description)}[/]"),
            new Markup(""),
            new Markup($"[bold]Trade:[/] [{tradeColor}]{tradeIcon} {tradeName}[/]"),
            new Markup($"[bold]Difficulty:[/] [{diffColor}]{difficultyStr}[/]"),
            new Markup($"[bold]Your WITS:[/] {details.CrafterWits}"),
            new Markup(""),
            new Markup("[bold]Ingredients:[/]")
        };

        // Add ingredient lines
        foreach (var ingredient in details.Ingredients)
        {
            var line = CraftingViewHelper.FormatIngredientLine(
                ingredient.ItemName,
                ingredient.RequiredQuantity,
                ingredient.AvailableQuantity,
                ingredient.IsSatisfied);
            content.Add(new Markup(line));
        }

        // Add output
        content.Add(new Markup(""));
        var outputColor = details.CanCraft ? "green" : "grey";
        content.Add(new Markup($"[bold]Output:[/] [{outputColor}]{details.OutputQuantity}x {Markup.Escape(details.OutputItemName)}[/]"));

        // Add craft availability status
        content.Add(new Markup(""));
        if (details.CanCraft)
        {
            content.Add(new Markup("[bold green]Ready to craft! Press ENTER to begin.[/]"));
        }
        else
        {
            content.Add(new Markup("[bold red]Missing ingredients.[/]"));
        }

        return new Panel(new Rows(content))
        {
            Header = new PanelHeader("[bold white]DETAILS[/]"),
            Border = BoxBorder.Rounded,
            BorderStyle = Style.Parse("white")
        };
    }

    /// <summary>
    /// Creates the footer showing trade filter keys and navigation controls.
    /// </summary>
    private static Markup CreateFooter(CraftingTrade currentTrade)
    {
        // Highlight the currently selected trade
        var bColor = currentTrade == CraftingTrade.Bodging ? "bold orange1" : "grey";
        var aColor = currentTrade == CraftingTrade.Alchemy ? "bold green" : "grey";
        var rColor = currentTrade == CraftingTrade.Runeforging ? "bold magenta1" : "grey";
        var mColor = currentTrade == CraftingTrade.FieldMedicine ? "bold cyan" : "grey";

        return new Markup(
            $"[{bColor}][[B]]odge[/]  [{aColor}][[A]]lch[/]  [{rColor}][[R]]une[/]  [{mColor}][[M]]ed[/]  " +
            "[grey]|[/]  [grey][[\u2191/\u2193]] Navigate  [[ENTER]] Craft  [[ESC]] Close[/]"
        );
    }
}
