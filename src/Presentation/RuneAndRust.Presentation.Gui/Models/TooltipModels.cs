namespace RuneAndRust.Presentation.Gui.Models;

using Avalonia.Media;

/// <summary>
/// A single label/value line in a tooltip section.
/// </summary>
/// <param name="Label">The label text.</param>
/// <param name="Value">The value text.</param>
/// <param name="ValueColor">Optional color for the value.</param>
public record TooltipLine(string Label, string Value, Color? ValueColor = null);

/// <summary>
/// A section within a tooltip containing related information.
/// </summary>
/// <param name="Header">Optional section header.</param>
/// <param name="Lines">Lines in this section.</param>
public record TooltipSection(string? Header, IReadOnlyList<TooltipLine> Lines);

/// <summary>
/// Rich tooltip content with title, sections, and footer.
/// </summary>
/// <param name="Title">The main title.</param>
/// <param name="Subtitle">Optional subtitle.</param>
/// <param name="Sections">Content sections.</param>
/// <param name="Description">Optional description text.</param>
/// <param name="Footer">Optional footer text.</param>
public record TooltipContent(
    string Title,
    string? Subtitle,
    IReadOnlyList<TooltipSection> Sections,
    string? Description,
    string? Footer)
{
    /// <summary>Creates a simple text tooltip.</summary>
    public static TooltipContent Simple(string text) =>
        new(text, null, [], null, null);

    /// <summary>Creates an item tooltip.</summary>
    public static TooltipContent ForItem(string name, string type, string description, int value,
        IEnumerable<(string label, string value)>? stats = null)
    {
        var lines = (stats ?? []).Select(s => new TooltipLine(s.label, s.value)).ToList();
        var sections = lines.Count > 0 ? new[] { new TooltipSection(null, lines) } : Array.Empty<TooltipSection>();
        return new TooltipContent($"⚔ {name.ToUpperInvariant()}", type, sections, description, $"Value: {value} gold");
    }

    /// <summary>Creates an ability tooltip.</summary>
    public static TooltipContent ForAbility(string name, string school, int level, string description,
        string? cost = null, int? cooldown = null, string? hotkey = null)
    {
        var lines = new List<TooltipLine>();
        if (cost is not null) lines.Add(new TooltipLine("Cost", cost));
        if (cooldown.HasValue) lines.Add(new TooltipLine("Cooldown", $"{cooldown} turns"));
        var sections = lines.Count > 0 ? new[] { new TooltipSection(null, lines) } : Array.Empty<TooltipSection>();
        var footer = hotkey is not null ? $"[Press {hotkey} to use]" : null;
        return new TooltipContent($"✨ {name.ToUpperInvariant()}", $"{school} • Level {level}", sections, description, footer);
    }

    /// <summary>Creates a status effect tooltip.</summary>
    public static TooltipContent ForStatusEffect(string name, string effectType, int duration, string description,
        int? damagePerTurn = null, string? source = null)
    {
        var lines = new List<TooltipLine>();
        if (damagePerTurn.HasValue)
            lines.Add(new TooltipLine("Effect", $"{damagePerTurn} damage/turn", Colors.Red));
        lines.Add(new TooltipLine("Duration", $"{duration} turns"));
        if (source is not null) lines.Add(new TooltipLine("Source", source));
        var sections = lines.Count > 0 ? new[] { new TooltipSection(null, lines) } : Array.Empty<TooltipSection>();
        var icon = effectType switch { "Buff" => "✨", "Debuff" => "🩸", "Poison" => "☠", "Burning" => "🔥", _ => "•" };
        return new TooltipContent($"{icon} {name.ToUpperInvariant()}", effectType, sections, description, null);
    }
}
