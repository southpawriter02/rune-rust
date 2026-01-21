namespace RuneAndRust.Presentation.Shared.Enums;

/// <summary>
/// Keys for accessing colors from the theme palette.
/// </summary>
/// <remarks>
/// <para>Provides type-safe access to colors defined in
/// <see cref="ValueObjects.ColorPalette"/>.</para>
/// <para>Colors are organized into logical categories:</para>
/// <list type="bullet">
///   <item><description>Core UI Colors - Primary, Secondary, Accent, etc.</description></item>
///   <item><description>Health Colors - Full, Good, Low, Critical thresholds</description></item>
///   <item><description>Resource Colors - Mana, Rage, Energy, Focus, Stamina</description></item>
///   <item><description>Status Effect Colors - Buff, Debuff, Fire, Ice, etc.</description></item>
///   <item><description>Terrain Colors - Floor, Wall, Water, Lava, Grass, Door</description></item>
///   <item><description>Entity Colors - Player, Enemy, NPC, Boss, Ally</description></item>
///   <item><description>UI State Colors - Selected, Disabled, Border</description></item>
/// </list>
/// </remarks>
public enum ColorKey
{
    // ═══════════════════════════════════════════════════════════════════════════
    // Core UI Colors
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>Primary brand/accent color.</summary>
    Primary,

    /// <summary>Secondary accent color.</summary>
    Secondary,

    /// <summary>Highlight/action color.</summary>
    Accent,

    /// <summary>Warning/caution color.</summary>
    Warning,

    /// <summary>Error/danger color.</summary>
    Error,

    /// <summary>Success/confirmation color.</summary>
    Success,

    /// <summary>Background color.</summary>
    Background,

    /// <summary>Surface/panel color.</summary>
    Surface,

    /// <summary>Primary text color.</summary>
    Text,

    /// <summary>Muted/secondary text color.</summary>
    Muted,

    // ═══════════════════════════════════════════════════════════════════════════
    // Health Colors
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>Full health (76-100%).</summary>
    HealthFull,

    /// <summary>Good health (51-75%).</summary>
    HealthGood,

    /// <summary>Low health (26-50%).</summary>
    HealthLow,

    /// <summary>Critical health (0-25%).</summary>
    HealthCritical,

    // ═══════════════════════════════════════════════════════════════════════════
    // Resource Colors
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>Mana resource color.</summary>
    Mana,

    /// <summary>Rage resource color.</summary>
    Rage,

    /// <summary>Energy resource color.</summary>
    Energy,

    /// <summary>Focus resource color.</summary>
    Focus,

    /// <summary>Stamina resource color.</summary>
    Stamina,

    // ═══════════════════════════════════════════════════════════════════════════
    // Status Effect Colors
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>Buff effect color.</summary>
    Buff,

    /// <summary>Debuff effect color.</summary>
    Debuff,

    /// <summary>Fire damage/effect color.</summary>
    Fire,

    /// <summary>Ice/cold damage/effect color.</summary>
    Ice,

    /// <summary>Poison damage/effect color.</summary>
    Poison,

    /// <summary>Lightning damage/effect color.</summary>
    Lightning,

    /// <summary>Holy/divine damage/effect color.</summary>
    Holy,

    /// <summary>Shadow/dark damage/effect color.</summary>
    Shadow,

    // ═══════════════════════════════════════════════════════════════════════════
    // Terrain Colors
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>Floor/walkable terrain color.</summary>
    Floor,

    /// <summary>Wall/obstacle terrain color.</summary>
    Wall,

    /// <summary>Water terrain color.</summary>
    Water,

    /// <summary>Lava/hazard terrain color.</summary>
    Lava,

    /// <summary>Grass/vegetation terrain color.</summary>
    Grass,

    /// <summary>Door/portal terrain color.</summary>
    Door,

    // ═══════════════════════════════════════════════════════════════════════════
    // Entity Colors
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>Player character color.</summary>
    Player,

    /// <summary>Enemy/monster color.</summary>
    Enemy,

    /// <summary>NPC (non-hostile) color.</summary>
    Npc,

    /// <summary>Boss enemy color.</summary>
    Boss,

    /// <summary>Ally/companion color.</summary>
    Ally,

    // ═══════════════════════════════════════════════════════════════════════════
    // UI State Colors
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>Selected/highlighted item color.</summary>
    Selected,

    /// <summary>Disabled/inactive color.</summary>
    Disabled,

    /// <summary>Border/outline color.</summary>
    Border
}
