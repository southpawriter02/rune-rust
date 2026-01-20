using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;
using RuneAndRust.Presentation.Tui.Configuration;
using RuneAndRust.Presentation.Tui.DTOs;

namespace RuneAndRust.Presentation.Tui.Renderers;

/// <summary>
/// Renders status effect icons, colors, and formatted text.
/// </summary>
/// <remarks>
/// <para>Handles mapping from effect data to visual representation.</para>
/// <para>Supports both Unicode and ASCII fallback based on terminal capabilities.</para>
/// </remarks>
public class StatusEffectRenderer
{
    private readonly ITerminalService _terminal;
    private readonly StatusEffectDisplayConfig _config;
    private readonly ILogger<StatusEffectRenderer>? _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="StatusEffectRenderer"/>.
    /// </summary>
    /// <param name="terminal">Terminal service for capability detection.</param>
    /// <param name="config">Display configuration options.</param>
    /// <param name="logger">Optional logger for diagnostics.</param>
    public StatusEffectRenderer(
        ITerminalService terminal,
        IOptions<StatusEffectDisplayConfig>? config = null,
        ILogger<StatusEffectRenderer>? logger = null)
    {
        _terminal = terminal ?? throw new ArgumentNullException(nameof(terminal));
        _config = config?.Value ?? StatusEffectDisplayConfig.CreateDefault();
        _logger = logger;
    }

    /// <summary>
    /// Gets the icon character for an effect type.
    /// </summary>
    /// <param name="effectType">The type of effect.</param>
    /// <returns>Single character icon representing the effect type.</returns>
    public char GetEffectIcon(StatusEffectType effectType)
    {
        var icon = effectType switch
        {
            StatusEffectType.StatModifier => _config.Icons.StatModifier,
            StatusEffectType.DamageOverTime => _config.Icons.DamageOverTime,
            StatusEffectType.HealOverTime => _config.Icons.HealOverTime,
            StatusEffectType.ActionPrevention => _config.Icons.ActionPrevention,
            StatusEffectType.Movement => _config.Icons.Movement,
            StatusEffectType.Special => _config.Icons.Special,
            _ => '?'
        };

        _logger?.LogDebug("Mapped effect type {EffectType} to icon '{Icon}'", effectType, icon);
        return icon;
    }

    /// <summary>
    /// Gets the damage type icon for DoT effects.
    /// </summary>
    /// <param name="damageType">The damage type string (e.g., "fire", "poison").</param>
    /// <returns>Single character icon for the damage type.</returns>
    /// <remarks>
    /// Maps common damage types to single-character icons:
    /// Fire=F, Ice=I, Poison=P, Lightning=L, Acid=A, Necrotic=N, Bleed=B.
    /// </remarks>
    public char GetDamageTypeIcon(string? damageType)
    {
        if (string.IsNullOrEmpty(damageType))
        {
            return 'D'; // Default damage icon
        }

        var icon = damageType.ToLowerInvariant() switch
        {
            "fire" => 'F',
            "ice" or "cold" or "frost" => 'I',
            "poison" => 'P',
            "lightning" or "electric" or "shock" => 'L',
            "acid" => 'A',
            "necrotic" or "dark" or "shadow" => 'N',
            "bleed" or "bleeding" or "physical" => 'B',
            "holy" or "radiant" or "light" => 'R',
            _ => 'D'
        };

        if (icon == 'D' && !string.IsNullOrEmpty(damageType))
        {
            _logger?.LogWarning("Unknown damage type '{DamageType}', using default icon 'D'", damageType);
        }
        else
        {
            _logger?.LogDebug("Mapped damage type '{DamageType}' to icon '{Icon}'", damageType, icon);
        }

        return icon;
    }

    /// <summary>
    /// Gets the console color for an effect category.
    /// </summary>
    /// <param name="category">The effect category (Buff/Debuff/Environmental).</param>
    /// <returns>Console color for rendering the effect.</returns>
    public ConsoleColor GetEffectColor(EffectCategory category)
    {
        return category switch
        {
            EffectCategory.Buff => _config.Colors.Buff,
            EffectCategory.Debuff => _config.Colors.Debuff,
            EffectCategory.Environmental => _config.Colors.Environmental,
            _ => ConsoleColor.Gray
        };
    }

    /// <summary>
    /// Gets color based on remaining duration indicating urgency.
    /// </summary>
    /// <param name="remaining">Remaining turns.</param>
    /// <param name="baseColor">Base category color to fall back to.</param>
    /// <returns>Color indicating urgency level.</returns>
    /// <remarks>
    /// Returns ExpiringUrgent (dark red) for 1 turn remaining,
    /// ExpiringSoon (yellow) for 2 turns remaining,
    /// otherwise returns the base color.
    /// </remarks>
    public ConsoleColor GetDurationColor(int? remaining, ConsoleColor baseColor)
    {
        if (!remaining.HasValue)
        {
            return baseColor;
        }

        return remaining.Value switch
        {
            <= 1 => _config.Colors.ExpiringUrgent,  // About to expire
            <= 2 => _config.Colors.ExpiringSoon,    // Expiring soon
            _ => baseColor
        };
    }

    /// <summary>
    /// Formats duration for display.
    /// </summary>
    /// <param name="remaining">Remaining duration value.</param>
    /// <param name="durationType">Type of duration tracking.</param>
    /// <returns>Formatted duration string (e.g., "5t", "", "?", "[50]").</returns>
    public string FormatDuration(int? remaining, DurationType durationType)
    {
        return durationType switch
        {
            DurationType.Turns => remaining.HasValue ? $"{remaining}{_config.Symbols.TurnSuffix}" : "",
            DurationType.Permanent => "",
            DurationType.Triggered => "?",
            DurationType.ResourceBased => remaining.HasValue ? $"[{remaining}]" : "[?]",
            _ => ""
        };
    }

    /// <summary>
    /// Formats stack count for display.
    /// </summary>
    /// <param name="current">Current stack count.</param>
    /// <param name="max">Maximum stack count.</param>
    /// <returns>Formatted stack string (e.g., "", "3x", "3x!").</returns>
    /// <remarks>
    /// Returns empty string for single stack.
    /// Appends MaxStackIndicator when at maximum stacks.
    /// </remarks>
    public string FormatStacks(int current, int max)
    {
        // Don't show stack count for single-stack effects (unless configured)
        if (current <= 1 && !_config.Display.ShowStacksWhenSingle)
        {
            return "";
        }

        var atMax = current >= max && max > 1 ? _config.Symbols.MaxStackIndicator.ToString() : "";
        return $"{current}{_config.Symbols.StackIndicator}{atMax}";
    }

    /// <summary>
    /// Gets the immunity indicator symbol.
    /// </summary>
    /// <returns>Immunity indicator string based on terminal Unicode support.</returns>
    public string GetImmunityIndicator()
    {
        return _terminal.SupportsUnicode
            ? _config.Symbols.ImmunityUnicode
            : _config.Symbols.ImmunityAscii;
    }

    /// <summary>
    /// Formats a complete effect display string.
    /// </summary>
    /// <param name="dto">Effect display data.</param>
    /// <returns>Formatted effect string like "[P3x5t]" or "[+3t]".</returns>
    /// <remarks>
    /// Format: [IconStacksDuration]
    /// - Icon: Single character based on effect type or damage type
    /// - Stacks: Stack count with 'x' suffix (only if > 1)
    /// - Duration: Remaining turns with 't' suffix
    /// </remarks>
    public string FormatEffectDisplay(StatusEffectDisplayDto dto)
    {
        // For DoT effects, use damage type icon; otherwise use effect type icon
        var icon = dto.EffectType == StatusEffectType.DamageOverTime
            ? GetDamageTypeIcon(dto.DamageType)
            : GetEffectIcon(dto.EffectType);

        var stacks = FormatStacks(dto.CurrentStacks, dto.MaxStacks);
        var duration = FormatDuration(dto.RemainingDuration, dto.DurationType);

        // Build the display: [IconStacksDuration] format
        var content = $"{icon}{stacks}{duration}";

        _logger?.LogDebug(
            "Formatted effect {EffectId}: [{Content}] (type={EffectType}, stacks={Stacks}, duration={Duration})",
            dto.EffectId, content, dto.EffectType, dto.CurrentStacks, dto.RemainingDuration);

        return $"[{content}]";
    }

    /// <summary>
    /// Renders a tooltip for an effect.
    /// </summary>
    /// <param name="tooltip">Tooltip data to render.</param>
    /// <returns>List of lines for the tooltip panel.</returns>
    public IReadOnlyList<string> RenderTooltip(EffectTooltipDto tooltip)
    {
        var lines = new List<string>();
        var width = _config.Display.TooltipWidth;
        var border = new string('─', width - 2);

        // Header with effect name
        lines.Add($"┌{border}┐");
        lines.Add(FormatTooltipLine(tooltip.Name.ToUpperInvariant(), width));
        lines.Add($"├{border}┤");

        // Description (word-wrapped)
        var descLines = WrapText(tooltip.Description, width - 4);
        foreach (var line in descLines)
        {
            lines.Add(FormatTooltipLine(line, width));
        }
        lines.Add(FormatEmptyLine(width));

        // Duration information
        var durationText = FormatDurationText(tooltip.RemainingDuration, tooltip.DurationType);
        if (!string.IsNullOrEmpty(durationText))
        {
            lines.Add(FormatTooltipLine(durationText, width));
        }

        // Stack information (only if stackable)
        if (tooltip.IsStackable)
        {
            var stackText = $"Stacks: {tooltip.CurrentStacks} / {tooltip.MaxStacks}";
            if (tooltip.IsAtMaxStacks)
            {
                stackText += " (MAX)";
            }
            lines.Add(FormatTooltipLine(stackText, width));
        }

        // Damage/Healing per turn
        if (tooltip.HasDamageOverTime)
        {
            var dmgText = FormatDamageText(tooltip);
            lines.Add(FormatTooltipLine(dmgText, width));
        }

        if (tooltip.HasHealingOverTime)
        {
            var healText = $"Healing: {tooltip.HealingPerTurn} per turn";
            lines.Add(FormatTooltipLine(healText, width));
        }

        // Stat modifiers
        if (tooltip.HasStatModifiers)
        {
            lines.Add(FormatTooltipLine("Stat Changes:", width));
            foreach (var mod in tooltip.StatModifiers!.Take(4))
            {
                var modText = FormatStatModifier(mod, tooltip.CurrentStacks);
                lines.Add(FormatTooltipLine($"  {modText}", width));
            }
            if (tooltip.StatModifiers!.Count > 4)
            {
                lines.Add(FormatTooltipLine("  ...", width));
            }
        }

        // Source information
        if (_config.Display.ShowSourceInTooltip && !string.IsNullOrEmpty(tooltip.SourceName))
        {
            lines.Add(FormatEmptyLine(width));
            lines.Add(FormatTooltipLine($"Source: {tooltip.SourceName}", width));
        }

        // Footer
        lines.Add($"└{border}┘");

        _logger?.LogDebug("Rendered tooltip for effect {EffectName} with {LineCount} lines",
            tooltip.Name, lines.Count);

        return lines;
    }

    #region Private Helpers

    /// <summary>
    /// Formats a line within the tooltip box.
    /// </summary>
    private string FormatTooltipLine(string content, int width)
    {
        var maxContent = width - 4; // Account for "│ " prefix and " │" suffix
        var truncated = content.Length > maxContent
            ? content[..(maxContent - 3)] + "..."
            : content;
        return $"│ {truncated.PadRight(maxContent)} │";
    }

    /// <summary>
    /// Formats an empty line within the tooltip box.
    /// </summary>
    private static string FormatEmptyLine(int width)
    {
        return $"│{new string(' ', width - 2)}│";
    }

    /// <summary>
    /// Formats duration text for the tooltip.
    /// </summary>
    private static string FormatDurationText(int? remaining, DurationType durationType)
    {
        return durationType switch
        {
            DurationType.Turns => $"Duration: {remaining} turns remaining",
            DurationType.Permanent => "Duration: Permanent",
            DurationType.Triggered => "Duration: Until triggered",
            DurationType.ResourceBased => $"Resource: {remaining} remaining",
            _ => ""
        };
    }

    /// <summary>
    /// Formats damage text for DoT effects.
    /// </summary>
    private static string FormatDamageText(EffectTooltipDto tooltip)
    {
        var damageType = tooltip.DamageType ?? "damage";

        if (tooltip.CurrentStacks > 1)
        {
            var total = tooltip.DamagePerTurn!.Value * tooltip.CurrentStacks;
            return $"Damage: {tooltip.DamagePerTurn} {damageType}/turn x{tooltip.CurrentStacks} = {total}";
        }

        return $"Damage: {tooltip.DamagePerTurn} {damageType} per turn";
    }

    /// <summary>
    /// Formats a stat modifier for the tooltip.
    /// </summary>
    private static string FormatStatModifier(StatModifier mod, int stacks)
    {
        var effectiveValue = mod.ModifierType switch
        {
            StatModifierType.Flat => (int)(mod.Value * stacks),
            StatModifierType.Percentage => (int)(mod.Value * 100),
            _ => (int)mod.Value
        };

        var sign = effectiveValue >= 0 ? "+" : "";
        var suffix = mod.ModifierType == StatModifierType.Percentage ? "%" : "";

        return $"{sign}{effectiveValue}{suffix} {mod.StatId}";
    }

    /// <summary>
    /// Wraps text to fit within a maximum width.
    /// </summary>
    private static IReadOnlyList<string> WrapText(string text, int maxWidth)
    {
        var lines = new List<string>();
        var words = text.Split(' ');
        var currentLine = "";

        foreach (var word in words)
        {
            // Check if adding this word would exceed the line width
            if (currentLine.Length + word.Length + 1 <= maxWidth)
            {
                currentLine += (currentLine.Length > 0 ? " " : "") + word;
            }
            else
            {
                // Save current line and start a new one
                if (currentLine.Length > 0)
                {
                    lines.Add(currentLine);
                }
                currentLine = word;
            }
        }

        // Don't forget the last line
        if (currentLine.Length > 0)
        {
            lines.Add(currentLine);
        }

        return lines;
    }

    #endregion
}
