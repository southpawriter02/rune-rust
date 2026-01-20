using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Presentation.Tui.DTOs;
using RuneAndRust.Presentation.Tui.Renderers;

namespace RuneAndRust.Presentation.Tui.UI;

/// <summary>
/// UI component for displaying active status effects on a combatant.
/// </summary>
/// <remarks>
/// <para>Shows buff and debuff icons with duration timers and stack counts.</para>
/// <para>Supports keyboard navigation for effect selection and tooltip display.</para>
/// </remarks>
public class StatusEffectDisplay
{
    private readonly ITerminalService _terminal;
    private readonly StatusEffectRenderer _renderer;
    private readonly ILogger<StatusEffectDisplay>? _logger;

    private IReadOnlyList<StatusEffectDisplayDto> _currentBuffs = [];
    private IReadOnlyList<StatusEffectDisplayDto> _currentDebuffs = [];
    private int _selectedIndex = -1;
    private bool _isBuffSelected = true;

    /// <summary>
    /// Maximum effects to display per row before showing overflow indicator.
    /// </summary>
    private const int MaxEffectsPerRow = 6;

    /// <summary>
    /// Initializes a new instance of <see cref="StatusEffectDisplay"/>.
    /// </summary>
    /// <param name="terminal">Terminal service for output.</param>
    /// <param name="renderer">Renderer for effect formatting.</param>
    /// <param name="logger">Optional logger for diagnostics.</param>
    public StatusEffectDisplay(
        ITerminalService terminal,
        StatusEffectRenderer renderer,
        ILogger<StatusEffectDisplay>? logger = null)
    {
        _terminal = terminal ?? throw new ArgumentNullException(nameof(terminal));
        _renderer = renderer ?? throw new ArgumentNullException(nameof(renderer));
        _logger = logger;
    }

    /// <summary>
    /// Renders buffs in a single row.
    /// </summary>
    /// <param name="buffs">Active buff effects to display.</param>
    /// <returns>Formatted string for the buff row (e.g., "BUFFS: [R5t] [+3t]").</returns>
    public string RenderBuffRow(IReadOnlyList<ActiveStatusEffect> buffs)
    {
        ArgumentNullException.ThrowIfNull(buffs);

        _currentBuffs = MapToDisplayDtos(buffs);

        if (_currentBuffs.Count == 0)
        {
            _logger?.LogDebug("Rendered buff row with no buffs");
            return "BUFFS: (none)";
        }

        var effectStrings = _currentBuffs
            .Take(MaxEffectsPerRow)
            .Select((dto, index) => FormatEffectIcon(dto, isSelected: _isBuffSelected && index == _selectedIndex));

        var row = $"BUFFS: {string.Join(" ", effectStrings)}";

        // Show overflow indicator if more effects exist
        if (_currentBuffs.Count > MaxEffectsPerRow)
        {
            row += $" (+{_currentBuffs.Count - MaxEffectsPerRow})";
        }

        _logger?.LogDebug("Rendered buff row with {Count} buffs", _currentBuffs.Count);
        return row;
    }

    /// <summary>
    /// Renders debuffs in a single row.
    /// </summary>
    /// <param name="debuffs">Active debuff effects to display.</param>
    /// <returns>Formatted string for the debuff row (e.g., "DEBUFFS: [P3x5t] [!2t]").</returns>
    public string RenderDebuffRow(IReadOnlyList<ActiveStatusEffect> debuffs)
    {
        ArgumentNullException.ThrowIfNull(debuffs);

        _currentDebuffs = MapToDisplayDtos(debuffs);

        if (_currentDebuffs.Count == 0)
        {
            _logger?.LogDebug("Rendered debuff row with no debuffs");
            return "DEBUFFS: (none)";
        }

        var effectStrings = _currentDebuffs
            .Take(MaxEffectsPerRow)
            .Select((dto, index) => FormatEffectIcon(dto, isSelected: !_isBuffSelected && index == _selectedIndex));

        var row = $"DEBUFFS: {string.Join(" ", effectStrings)}";

        // Show overflow indicator if more effects exist
        if (_currentDebuffs.Count > MaxEffectsPerRow)
        {
            row += $" (+{_currentDebuffs.Count - MaxEffectsPerRow})";
        }

        _logger?.LogDebug("Rendered debuff row with {Count} debuffs", _currentDebuffs.Count);
        return row;
    }

    /// <summary>
    /// Renders all status effects for a combatant.
    /// </summary>
    /// <param name="effects">All active effects on the combatant.</param>
    /// <returns>List of formatted lines for display (buff row, debuff row).</returns>
    public IReadOnlyList<string> RenderAll(IReadOnlyList<ActiveStatusEffect> effects)
    {
        ArgumentNullException.ThrowIfNull(effects);

        // Separate buffs and debuffs by category
        var buffs = effects.Where(e => e.Definition.Category == EffectCategory.Buff).ToList();
        var debuffs = effects.Where(e => e.Definition.Category != EffectCategory.Buff).ToList();

        _logger?.LogInformation(
            "Rendered status effects: {BuffCount} buffs, {DebuffCount} debuffs",
            buffs.Count, debuffs.Count);

        return new List<string>
        {
            RenderBuffRow(buffs),
            RenderDebuffRow(debuffs)
        };
    }

    /// <summary>
    /// Updates duration display for all tracked effects.
    /// </summary>
    /// <remarks>
    /// Called at the start of each turn to signal that a refresh is needed.
    /// Durations are read fresh from ActiveStatusEffect on each render.
    /// </remarks>
    public void UpdateDurations()
    {
        _logger?.LogDebug("Duration update signaled");
    }

    /// <summary>
    /// Shows detailed tooltip for the specified effect.
    /// </summary>
    /// <param name="effect">The active effect to show details for.</param>
    /// <returns>Tooltip content as a list of lines.</returns>
    public IReadOnlyList<string> ShowTooltip(ActiveStatusEffect effect)
    {
        ArgumentNullException.ThrowIfNull(effect);

        var definition = effect.Definition;

        var tooltip = new EffectTooltipDto
        {
            Name = definition.Name,
            Description = definition.Description,
            Category = definition.Category,
            RemainingDuration = effect.RemainingDuration,
            DurationType = definition.DurationType,
            CurrentStacks = effect.Stacks,
            MaxStacks = definition.MaxStacks,
            DamagePerTurn = definition.DamagePerTurn,
            DamageType = definition.DamageType,
            HealingPerTurn = definition.HealingPerTurn,
            StatModifiers = definition.StatModifiers.ToList(),
            SourceName = effect.SourceName
        };

        _logger?.LogDebug("Showing tooltip for effect {EffectName}", definition.Name);
        return _renderer.RenderTooltip(tooltip);
    }

    /// <summary>
    /// Navigates selection to the next effect.
    /// </summary>
    /// <remarks>
    /// Cycles through buffs first, then debuffs, wrapping back to buffs.
    /// </remarks>
    public void SelectNext()
    {
        var currentList = _isBuffSelected ? _currentBuffs : _currentDebuffs;

        if (currentList.Count == 0)
        {
            // Try switching to other list
            _isBuffSelected = !_isBuffSelected;
            currentList = _isBuffSelected ? _currentBuffs : _currentDebuffs;
            if (currentList.Count == 0)
            {
                _selectedIndex = -1;
                return;
            }
            _selectedIndex = 0;
            _logger?.LogDebug("Selection moved to {Category} at index {Index}",
                _isBuffSelected ? "buffs" : "debuffs", _selectedIndex);
            return;
        }

        _selectedIndex++;
        if (_selectedIndex >= Math.Min(currentList.Count, MaxEffectsPerRow))
        {
            // Move to other list or wrap
            _isBuffSelected = !_isBuffSelected;
            _selectedIndex = 0;
            _logger?.LogDebug("Selection wrapped to {Category} at index {Index}",
                _isBuffSelected ? "buffs" : "debuffs", _selectedIndex);
        }
    }

    /// <summary>
    /// Navigates selection to the previous effect.
    /// </summary>
    public void SelectPrevious()
    {
        var currentList = _isBuffSelected ? _currentBuffs : _currentDebuffs;

        if (currentList.Count == 0)
        {
            _isBuffSelected = !_isBuffSelected;
            currentList = _isBuffSelected ? _currentBuffs : _currentDebuffs;
            if (currentList.Count == 0)
            {
                _selectedIndex = -1;
                return;
            }
            _selectedIndex = Math.Min(currentList.Count, MaxEffectsPerRow) - 1;
            return;
        }

        _selectedIndex--;
        if (_selectedIndex < 0)
        {
            _isBuffSelected = !_isBuffSelected;
            currentList = _isBuffSelected ? _currentBuffs : _currentDebuffs;
            _selectedIndex = Math.Min(currentList.Count, MaxEffectsPerRow) - 1;
        }
    }

    /// <summary>
    /// Gets the currently selected effect, if any.
    /// </summary>
    /// <returns>The selected effect DTO, or null if nothing is selected.</returns>
    public StatusEffectDisplayDto? GetSelectedEffect()
    {
        if (_selectedIndex < 0)
        {
            return null;
        }

        var list = _isBuffSelected ? _currentBuffs : _currentDebuffs;
        if (_selectedIndex >= list.Count)
        {
            return null;
        }

        return list[_selectedIndex];
    }

    /// <summary>
    /// Clears the current selection.
    /// </summary>
    public void ClearSelection()
    {
        _selectedIndex = -1;
        _logger?.LogDebug("Selection cleared");
    }

    /// <summary>
    /// Renders an immunity indicator when an effect is blocked.
    /// </summary>
    /// <param name="effectName">Name of the blocked effect.</param>
    /// <param name="immunitySource">Source granting immunity (optional).</param>
    /// <returns>Formatted immunity message (e.g., "🛡 IMMUNE to Poison").</returns>
    public string RenderImmunityIndicator(string effectName, string? immunitySource = null)
    {
        var indicator = _renderer.GetImmunityIndicator();
        var message = $"{indicator} IMMUNE to {effectName}";

        if (!string.IsNullOrEmpty(immunitySource))
        {
            message += $" (from {immunitySource})";
        }

        _logger?.LogDebug("Rendered immunity indicator for effect {EffectName}", effectName);
        return message;
    }

    #region Private Helpers

    /// <summary>
    /// Maps active status effects to display DTOs.
    /// </summary>
    private IReadOnlyList<StatusEffectDisplayDto> MapToDisplayDtos(IReadOnlyList<ActiveStatusEffect> effects)
    {
        return effects.Select(e => new StatusEffectDisplayDto
        {
            EffectId = e.Definition.Id,
            Name = e.Definition.Name,
            Category = e.Definition.Category,
            EffectType = DetermineEffectType(e.Definition),
            RemainingDuration = e.RemainingDuration,
            DurationType = e.Definition.DurationType,
            CurrentStacks = e.Stacks,
            MaxStacks = e.Definition.MaxStacks,
            DamageType = e.Definition.DamageType,
            IconId = e.Definition.IconId,
            SourceName = e.SourceName
        }).ToList();
    }

    /// <summary>
    /// Determines the primary effect type from a definition.
    /// </summary>
    private static StatusEffectType DetermineEffectType(Domain.Definitions.StatusEffectDefinition def)
    {
        // Priority: DoT > HoT > ActionPrevention > StatModifier > Special
        if (def.DamagePerTurn.HasValue && def.DamagePerTurn > 0)
        {
            return StatusEffectType.DamageOverTime;
        }
        if (def.HealingPerTurn.HasValue && def.HealingPerTurn > 0)
        {
            return StatusEffectType.HealOverTime;
        }
        if (def.PreventsActions || def.PreventsMovement || def.PreventsAbilities || def.PreventsAttacking)
        {
            return StatusEffectType.ActionPrevention;
        }
        if (def.StatModifiers.Any())
        {
            return StatusEffectType.StatModifier;
        }

        return StatusEffectType.Special;
    }

    /// <summary>
    /// Formats an effect icon with optional selection indicator.
    /// </summary>
    private string FormatEffectIcon(StatusEffectDisplayDto dto, bool isSelected)
    {
        var icon = _renderer.FormatEffectDisplay(dto);
        return isSelected ? $">{icon}<" : icon;
    }

    #endregion
}
