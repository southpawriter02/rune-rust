using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Core.Models.Combat;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace RuneAndRust.Terminal.Rendering;

/// <summary>
/// Static helper for rendering spell lists in the combat HUD.
/// Displays available spells grouped by school with cost and effect information.
/// </summary>
/// <remarks>
/// See: v0.4.3e (The Resonance) for implementation details.
/// </remarks>
public static class SpellListRenderer
{
    /// <summary>
    /// Renders the full spell list grouped by school.
    /// </summary>
    /// <param name="spells">Available spells to display.</param>
    /// <param name="caster">The caster for AP availability checks.</param>
    /// <param name="aetherService">Aether service for current flux.</param>
    /// <param name="themeService">Theme service for colors.</param>
    /// <returns>A Panel containing the formatted spell list.</returns>
    public static Panel Render(
        IEnumerable<Spell> spells,
        Combatant caster,
        IAetherService aetherService,
        IThemeService themeService)
    {
        var spellList = spells.ToList();

        if (spellList.Count == 0)
        {
            return new Panel(new Markup("[grey]No spells known.[/]"))
                .Header("[bold]KNOWN SPELLS[/]")
                .Border(BoxBorder.Rounded);
        }

        var rows = new List<IRenderable>();

        // Group by school
        var grouped = spellList
            .GroupBy(s => s.School)
            .OrderBy(g => (int)g.Key);

        var spellNumber = 1;

        foreach (var group in grouped)
        {
            // School header
            var schoolColor = GetSchoolColor(group.Key, themeService);
            rows.Add(new Text(""));
            rows.Add(new Markup($"[bold {schoolColor.ToMarkup()}]{group.Key.ToString().ToUpper()}[/]"));

            // Spells in school
            var schoolSpells = group.OrderBy(s => s.ApCost).ToList();
            for (var i = 0; i < schoolSpells.Count; i++)
            {
                var spell = schoolSpells[i];
                var isLast = i == schoolSpells.Count - 1;
                var prefix = isLast ? "└─" : "├─";

                var entry = FormatSpellEntry(
                    spell,
                    spellNumber++,
                    prefix,
                    caster,
                    schoolColor,
                    themeService);

                rows.Add(new Markup(entry));
            }
        }

        // Footer with current resources
        rows.Add(new Text(""));
        rows.Add(new Rule().RuleStyle(Style.Parse(themeService.GetColor("DimColor"))));
        rows.Add(CreateFooter(caster, aetherService, themeService));

        var panel = new Panel(new Rows(rows))
            .Header("[bold]KNOWN SPELLS[/]")
            .Border(BoxBorder.Rounded)
            .BorderStyle(Style.Parse(themeService.GetColor("BorderColor")))
            .Padding(1, 0);

        return panel;
    }

    /// <summary>
    /// Formats a single spell entry line.
    /// </summary>
    private static string FormatSpellEntry(
        Spell spell,
        int number,
        string prefix,
        Combatant caster,
        Color schoolColor,
        IThemeService themeService)
    {
        // Determine if affordable
        var canAfford = caster.CurrentAp >= spell.ApCost;
        var nameColor = canAfford ? schoolColor.ToMarkup() : "grey";
        var costColor = canAfford ? "white" : "grey dim";

        // Format costs
        var apCost = spell.ApCost.ToString().PadLeft(2);
        var fluxCost = spell.FluxCost.ToString().PadLeft(2);

        // Get effect summary
        var effectSummary = GetEffectSummary(spell);

        // Build entry
        var numberStr = $"[{number}]".PadRight(4);
        var nameStr = Markup.Escape(spell.Name).PadRight(16);

        return $"  {prefix} [{nameColor}]{numberStr}{nameStr}[/] [{costColor}]{apCost} AP │ {fluxCost} Flux │ {effectSummary}[/]";
    }

    /// <summary>
    /// Gets a brief summary of the spell's effect.
    /// </summary>
    private static string GetEffectSummary(Spell spell)
    {
        if (string.IsNullOrEmpty(spell.EffectScript))
            return "Special";

        // Parse effect script for summary
        if (spell.EffectScript.StartsWith("DAMAGE:"))
        {
            var parts = spell.EffectScript.Split(':');
            if (parts.Length >= 2)
            {
                return $"{parts[1]} damage";
            }
        }

        if (spell.EffectScript.StartsWith("HEAL:"))
        {
            return "Healing";
        }

        if (spell.EffectScript.StartsWith("STATUS:"))
        {
            var parts = spell.EffectScript.Split(':');
            if (parts.Length >= 2)
            {
                return $"{parts[1]} effect";
            }
        }

        if (spell.EffectScript.Contains(';'))
        {
            return "Multiple effects";
        }

        return "Special";
    }

    /// <summary>
    /// Creates the footer with current resources.
    /// </summary>
    private static Markup CreateFooter(
        Combatant caster,
        IAetherService aetherService,
        IThemeService themeService)
    {
        var flux = aetherService.CurrentFlux;
        var risk = Math.Max(0, flux - 50);
        var (thresholdLabel, thresholdColor) = FluxWidget.GetThresholdLabel(flux, themeService);

        var apColor = StatusWidget.GetApColor(caster.CurrentAp, caster.MaxAp, themeService);
        var fluxColor = FluxWidget.GetFluxColor(flux, themeService);
        var riskColor = risk > 0 ? "red" : "green";

        return new Markup(
            $"  [{apColor.ToMarkup()}]AP: {caster.CurrentAp}/{caster.MaxAp}[/]  │  " +
            $"[{fluxColor.ToMarkup()}]Flux: {flux}[/] [{thresholdColor.ToMarkup()}]{thresholdLabel}[/]  │  " +
            $"[{riskColor}]Risk: {risk}%[/]");
    }

    /// <summary>
    /// Gets the color for a spell school.
    /// </summary>
    /// <param name="school">The spell school.</param>
    /// <param name="themeService">Theme service for color lookup.</param>
    /// <returns>The color for this spell school.</returns>
    public static Color GetSchoolColor(SpellSchool school, IThemeService themeService)
    {
        var role = school switch
        {
            SpellSchool.Destruction => "SpellDestruction",
            SpellSchool.Restoration => "SpellRestoration",
            SpellSchool.Alteration => "SpellAlteration",
            SpellSchool.Divination => "SpellDivination",
            _ => "NeutralColor"
        };

        return ParseColor(themeService.GetColor(role));
    }

    /// <summary>
    /// Renders a compact spell list for limited space.
    /// </summary>
    /// <param name="spells">Available spells to display.</param>
    /// <param name="caster">The caster for AP availability checks.</param>
    /// <param name="themeService">Theme service for colors.</param>
    /// <returns>A Table with compact spell information.</returns>
    public static Table RenderCompact(
        IEnumerable<Spell> spells,
        Combatant caster,
        IThemeService themeService)
    {
        var table = new Table()
            .Border(TableBorder.None)
            .HideHeaders();

        table.AddColumn("Spell");
        table.AddColumn("Cost");

        foreach (var spell in spells.OrderBy(s => s.ApCost))
        {
            var canAfford = caster.CurrentAp >= spell.ApCost;
            var color = canAfford
                ? GetSchoolColor(spell.School, themeService).ToMarkup()
                : "grey";

            table.AddRow(
                new Markup($"[{color}]{Markup.Escape(spell.Name)}[/]"),
                new Markup($"[grey]{spell.ApCost} AP[/]"));
        }

        return table;
    }

    /// <summary>
    /// Parses a color string to a Spectre.Console Color object.
    /// </summary>
    private static Color ParseColor(string colorString)
    {
        // Strip "bold " prefix if present
        var cleanColor = colorString.StartsWith("bold ", StringComparison.OrdinalIgnoreCase)
            ? colorString[5..]
            : colorString;

        return cleanColor.ToLowerInvariant() switch
        {
            "red" => Color.Red,
            "red1" => Color.Red1,
            "orangered" => Color.OrangeRed1,
            "orange" => Color.Orange1,
            "orange1" => Color.Orange1,
            "green" => Color.Green,
            "lime" => Color.Lime,
            "limegreen" => Color.Green,
            "cyan" => Color.Cyan1,
            "cyan1" => Color.Cyan1,
            "darkcyan" => Color.DarkCyan,
            "purple" => Color.Purple,
            "mediumpurple" => Color.MediumPurple,
            "grey" => Color.Grey,
            "gray" => Color.Grey,
            _ => Color.Grey
        };
    }
}
