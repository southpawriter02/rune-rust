namespace RuneAndRust.Presentation.Tui.Configuration;

/// <summary>
/// Configuration for status effect display visuals.
/// </summary>
/// <remarks>
/// <para>Loaded from config/status-effect-display.json.</para>
/// <para>Follows the same pattern as <see cref="RuneAndRust.Presentation.Configuration.HealthBarConfig"/>.</para>
/// </remarks>
public record StatusEffectDisplayConfig
{
    /// <summary>
    /// Icon characters for each effect type.
    /// </summary>
    public required IconConfig Icons { get; init; }

    /// <summary>
    /// Console colors for effect categories and urgency states.
    /// </summary>
    public required ColorConfig Colors { get; init; }

    /// <summary>
    /// Symbol characters for indicators (immunity, stacks, etc.).
    /// </summary>
    public required SymbolConfig Symbols { get; init; }

    /// <summary>
    /// Display behavior settings.
    /// </summary>
    public required DisplayConfig Display { get; init; }

    /// <summary>
    /// Creates a default configuration with standard values.
    /// </summary>
    /// <returns>Default configuration instance.</returns>
    public static StatusEffectDisplayConfig CreateDefault() => new()
    {
        Icons = new IconConfig
        {
            StatModifier = '+',
            DamageOverTime = 'D',
            HealOverTime = 'H',
            ActionPrevention = '!',
            Movement = '>',
            Special = '*'
        },
        Colors = new ColorConfig
        {
            Buff = ConsoleColor.Green,
            Debuff = ConsoleColor.Red,
            Environmental = ConsoleColor.Cyan,
            ExpiringSoon = ConsoleColor.Yellow,
            ExpiringUrgent = ConsoleColor.DarkRed
        },
        Symbols = new SymbolConfig
        {
            ImmunityUnicode = "🛡",
            ImmunityAscii = "[X]",
            StackIndicator = 'x',
            MaxStackIndicator = '!',
            TurnSuffix = 't'
        },
        Display = new DisplayConfig
        {
            MaxEffectsPerRow = 6,
            TooltipWidth = 36,
            ShowSourceInTooltip = true,
            ShowStacksWhenSingle = false
        }
    };
}

/// <summary>
/// Icon characters for effect types.
/// </summary>
public record IconConfig
{
    /// <summary>Icon for stat modifier effects (+/- stats).</summary>
    public char StatModifier { get; init; } = '+';

    /// <summary>Icon for damage over time effects.</summary>
    public char DamageOverTime { get; init; } = 'D';

    /// <summary>Icon for healing over time effects.</summary>
    public char HealOverTime { get; init; } = 'H';

    /// <summary>Icon for action prevention effects (stun/freeze).</summary>
    public char ActionPrevention { get; init; } = '!';

    /// <summary>Icon for movement effects (slow/haste).</summary>
    public char Movement { get; init; } = '>';

    /// <summary>Icon for special/custom effects.</summary>
    public char Special { get; init; } = '*';
}

/// <summary>
/// Console colors for effect categories and states.
/// </summary>
public record ColorConfig
{
    /// <summary>Color for buff (beneficial) effects.</summary>
    public ConsoleColor Buff { get; init; } = ConsoleColor.Green;

    /// <summary>Color for debuff (harmful) effects.</summary>
    public ConsoleColor Debuff { get; init; } = ConsoleColor.Red;

    /// <summary>Color for environmental effects.</summary>
    public ConsoleColor Environmental { get; init; } = ConsoleColor.Cyan;

    /// <summary>Warning color for effects expiring in 2 turns.</summary>
    public ConsoleColor ExpiringSoon { get; init; } = ConsoleColor.Yellow;

    /// <summary>Urgent color for effects expiring in 1 turn.</summary>
    public ConsoleColor ExpiringUrgent { get; init; } = ConsoleColor.DarkRed;
}

/// <summary>
/// Symbol characters for indicators.
/// </summary>
public record SymbolConfig
{
    /// <summary>Unicode immunity indicator (shield emoji).</summary>
    public string ImmunityUnicode { get; init; } = "🛡";

    /// <summary>ASCII fallback for immunity indicator.</summary>
    public string ImmunityAscii { get; init; } = "[X]";

    /// <summary>Character between stack count and effect ('x' in "3x").</summary>
    public char StackIndicator { get; init; } = 'x';

    /// <summary>Character appended when at max stacks ('!' in "3x!").</summary>
    public char MaxStackIndicator { get; init; } = '!';

    /// <summary>Turn duration suffix ('t' in "5t").</summary>
    public char TurnSuffix { get; init; } = 't';
}

/// <summary>
/// Display behavior settings.
/// </summary>
public record DisplayConfig
{
    /// <summary>Maximum number of effect icons to display per row.</summary>
    public int MaxEffectsPerRow { get; init; } = 6;

    /// <summary>Width of the tooltip panel in characters.</summary>
    public int TooltipWidth { get; init; } = 36;

    /// <summary>Whether to show the effect source in tooltips.</summary>
    public bool ShowSourceInTooltip { get; init; } = true;

    /// <summary>Whether to show stack count when stacks = 1.</summary>
    public bool ShowStacksWhenSingle { get; init; } = false;
}
