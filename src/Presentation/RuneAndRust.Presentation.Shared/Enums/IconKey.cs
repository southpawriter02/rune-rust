namespace RuneAndRust.Presentation.Shared.Enums;

/// <summary>
/// Keys for accessing icons from the theme.
/// </summary>
/// <remarks>
/// <para>Provides type-safe access to icons defined in
/// <see cref="ValueObjects.IconSet"/>.</para>
/// <para>Icons have both Unicode and ASCII fallback representations
/// for terminal compatibility.</para>
/// </remarks>
public enum IconKey
{
    // ═══════════════════════════════════════════════════════════════════════════
    // Stats
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>Health/HP icon (♥ / [HP]).</summary>
    Health,

    /// <summary>Mana/MP icon (✦ / [MP]).</summary>
    Mana,

    /// <summary>Attack/damage icon (⚔ / [ATK]).</summary>
    Attack,

    /// <summary>Defense/armor icon (🛡 / [DEF]).</summary>
    Defense,

    /// <summary>Speed/agility icon (⚡ / [SPD]).</summary>
    Speed,

    /// <summary>Luck/fortune icon (☘ / [LCK]).</summary>
    Luck,

    // ═══════════════════════════════════════════════════════════════════════════
    // Status Effects
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>Buff/positive effect icon (↑ / [+]).</summary>
    Buff,

    /// <summary>Debuff/negative effect icon (↓ / [-]).</summary>
    Debuff,

    /// <summary>Fire effect icon (🔥 / [F]).</summary>
    Fire,

    /// <summary>Ice/cold effect icon (❄ / [I]).</summary>
    Ice,

    /// <summary>Poison effect icon (☠ / [P]).</summary>
    Poison,

    /// <summary>Lightning/shock effect icon (⚡ / [L]).</summary>
    Lightning,

    /// <summary>Stun/daze effect icon (★ / [*]).</summary>
    Stun,

    /// <summary>Shield/protection effect icon (🛡 / [S]).</summary>
    Shield,

    // ═══════════════════════════════════════════════════════════════════════════
    // Resources/Materials
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>Ore/metal resource icon.</summary>
    Ore,

    /// <summary>Herb/plant resource icon.</summary>
    Herb,

    /// <summary>Leather/hide resource icon.</summary>
    Leather,

    /// <summary>Gem/crystal resource icon.</summary>
    Gem,

    /// <summary>Wood/lumber resource icon.</summary>
    Wood,

    // ═══════════════════════════════════════════════════════════════════════════
    // Navigation
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>Up arrow (↑ / ^).</summary>
    ArrowUp,

    /// <summary>Down arrow (↓ / v).</summary>
    ArrowDown,

    /// <summary>Left arrow (← / &lt;).</summary>
    ArrowLeft,

    /// <summary>Right arrow (→ / &gt;).</summary>
    ArrowRight,

    // ═══════════════════════════════════════════════════════════════════════════
    // UI Indicators
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>Checkmark/success (✓ / [X]).</summary>
    Check,

    /// <summary>X/cross/failure (✗ / [ ]).</summary>
    Cross,

    /// <summary>Warning/caution (⚠ / [!]).</summary>
    Warning,

    /// <summary>Information (ⓘ / [?]).</summary>
    Info,

    /// <summary>Locked state (🔒 / [L]).</summary>
    Lock,

    /// <summary>Unlocked state (🔓 / [U]).</summary>
    Unlock,

    /// <summary>Filled star/rating (★ / *).</summary>
    Star,

    /// <summary>Empty star/rating (☆ / .).</summary>
    StarEmpty,

    // ═══════════════════════════════════════════════════════════════════════════
    // Entities
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>Player character (@ / @).</summary>
    Player,

    /// <summary>Enemy/monster (M / M).</summary>
    Enemy,

    /// <summary>Boss enemy (B / B).</summary>
    Boss,

    /// <summary>NPC/friendly (N / N).</summary>
    Npc,

    // ═══════════════════════════════════════════════════════════════════════════
    // Dice
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>D20 die (🎲 / [D20]).</summary>
    D20,

    /// <summary>Critical success/natural 20 (★ / [20!]).</summary>
    CriticalSuccess,

    /// <summary>Critical failure/natural 1 (✗ / [1!]).</summary>
    CriticalFailure
}
