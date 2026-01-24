// ------------------------------------------------------------------------------
// <copyright file="ImprovisedTool.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Represents an improvised tool crafted from salvaged components.
// These tools provide bonuses to specific bypass operations, reflecting
// Aethelgard's cargo-cult approach to Old World technology.
// Part of v0.15.4g Improvised Tool Crafting System implementation.
// </summary>
// ------------------------------------------------------------------------------

using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents an improvised tool crafted from salvaged components.
/// These tools provide bonuses to specific bypass operations.
/// </summary>
/// <remarks>
/// <para>
/// Improvised tools are fragile compared to proper equipment. They have
/// limited uses before breaking and can be destroyed prematurely on fumbles.
/// Quality tools (from critical crafting) have better durability and bonuses.
/// </para>
/// <para>
/// Tool characteristics:
/// <list type="bullet">
///   <item><description>Standard: +1d10 bonus, 3 uses before breaking</description></item>
///   <item><description>Quality: +2d10 bonus, 5 uses before breaking</description></item>
///   <item><description>Some tools have special effects beyond dice bonuses</description></item>
/// </list>
/// </para>
/// </remarks>
/// <param name="ToolId">Unique identifier for this tool instance.</param>
/// <param name="ToolName">Display name of the tool.</param>
/// <param name="ToolType">Classification determining what bypass types it aids.</param>
/// <param name="BonusAmount">Dice pool bonus when used (typically 1d10 or 2d10).</param>
/// <param name="UsesRemaining">Number of uses before the tool breaks.</param>
/// <param name="IsQuality">True if crafted with a critical success (better stats).</param>
/// <param name="SpecialEffect">Optional special effect (e.g., "Force Glitch State").</param>
public readonly record struct ImprovisedTool(
    string ToolId,
    string ToolName,
    ImprovisedToolType ToolType,
    int BonusAmount,
    int UsesRemaining,
    bool IsQuality,
    string? SpecialEffect = null)
{
    // -------------------------------------------------------------------------
    // Constants
    // -------------------------------------------------------------------------

    /// <summary>
    /// Standard tool durability (uses before breaking).
    /// </summary>
    public const int StandardDurability = 3;

    /// <summary>
    /// Quality tool durability (uses before breaking).
    /// </summary>
    public const int QualityDurability = 5;

    /// <summary>
    /// Standard dice bonus for improvised tools.
    /// </summary>
    public const int StandardBonus = 1;

    /// <summary>
    /// Enhanced dice bonus for quality improvised tools.
    /// </summary>
    public const int QualityBonus = 2;

    // -------------------------------------------------------------------------
    // Computed Properties
    // -------------------------------------------------------------------------

    /// <summary>
    /// Gets a value indicating whether this tool is still usable.
    /// </summary>
    /// <remarks>
    /// A tool is usable if it has at least one use remaining.
    /// </remarks>
    public bool IsUsable => UsesRemaining > 0;

    /// <summary>
    /// Gets a description of the tool's current condition.
    /// </summary>
    /// <remarks>
    /// Condition descriptions help players understand tool durability at a glance.
    /// </remarks>
    public string Condition => UsesRemaining switch
    {
        0 => "Broken",
        1 => "Nearly broken",
        2 => "Worn",
        _ => IsQuality ? "Good condition (Quality)" : "Serviceable"
    };

    /// <summary>
    /// Gets the bypass type this tool assists with.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Tool-to-bypass type mappings:
    /// <list type="bullet">
    ///   <item><description>Shim Picks → Lockpicking</description></item>
    ///   <item><description>Wire Probe → Terminal Hacking</description></item>
    ///   <item><description>Glitch Trigger → Glitch Exploitation</description></item>
    ///   <item><description>Bypass Clamps → Terminal Hacking</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    public BypassType AssistsWithBypassType => ToolType switch
    {
        ImprovisedToolType.ShimPicks => BypassType.Lockpicking,
        ImprovisedToolType.WireProbe => BypassType.TerminalHacking,
        ImprovisedToolType.GlitchTrigger => BypassType.GlitchExploitation,
        ImprovisedToolType.BypassClamps => BypassType.TerminalHacking,
        _ => BypassType.None
    };

    /// <summary>
    /// Gets a value indicating whether this tool has a special effect beyond dice bonus.
    /// </summary>
    /// <remarks>
    /// Special effects:
    /// <list type="bullet">
    ///   <item><description>Glitch Trigger: Force mechanism into [Glitched] state</description></item>
    ///   <item><description>Bypass Clamps: Skip Layer 1 of terminal infiltration</description></item>
    /// </list>
    /// </remarks>
    public bool HasSpecialEffect => !string.IsNullOrEmpty(SpecialEffect);

    /// <summary>
    /// Gets the maximum durability for this tool type.
    /// </summary>
    public int MaxDurability => IsQuality ? QualityDurability : StandardDurability;

    // -------------------------------------------------------------------------
    // Factory Methods
    // -------------------------------------------------------------------------

    /// <summary>
    /// Creates a standard Shim Picks tool.
    /// </summary>
    /// <param name="isQuality">True if crafted with critical success.</param>
    /// <returns>A new Shim Picks tool.</returns>
    /// <remarks>
    /// Shim Picks provide +1d10 (standard) or +2d10 (quality) to lockpicking attempts.
    /// </remarks>
    public static ImprovisedTool CreateShimPicks(bool isQuality = false)
    {
        return new ImprovisedTool(
            ToolId: Guid.NewGuid().ToString(),
            ToolName: isQuality ? "Quality Shim Picks" : "Shim Picks",
            ToolType: ImprovisedToolType.ShimPicks,
            BonusAmount: isQuality ? QualityBonus : StandardBonus,
            UsesRemaining: isQuality ? QualityDurability : StandardDurability,
            IsQuality: isQuality,
            SpecialEffect: null);
    }

    /// <summary>
    /// Creates a standard Wire Probe tool.
    /// </summary>
    /// <param name="isQuality">True if crafted with critical success.</param>
    /// <returns>A new Wire Probe tool.</returns>
    /// <remarks>
    /// Wire Probe provides +1d10 (standard) or +2d10 (quality) to terminal hacking attempts.
    /// </remarks>
    public static ImprovisedTool CreateWireProbe(bool isQuality = false)
    {
        return new ImprovisedTool(
            ToolId: Guid.NewGuid().ToString(),
            ToolName: isQuality ? "Quality Wire Probe" : "Wire Probe",
            ToolType: ImprovisedToolType.WireProbe,
            BonusAmount: isQuality ? QualityBonus : StandardBonus,
            UsesRemaining: isQuality ? QualityDurability : StandardDurability,
            IsQuality: isQuality,
            SpecialEffect: null);
    }

    /// <summary>
    /// Creates a Glitch Trigger tool.
    /// </summary>
    /// <param name="isQuality">True if crafted with critical success.</param>
    /// <returns>A new Glitch Trigger tool.</returns>
    /// <remarks>
    /// <para>
    /// Glitch Trigger provides +1d10 (standard) or +2d10 (quality) to glitch exploitation attempts.
    /// </para>
    /// <para>
    /// Special Effect: Can force a non-glitched mechanism into [Glitched] state,
    /// enabling the use of Glitch Exploitation bypass method.
    /// </para>
    /// </remarks>
    public static ImprovisedTool CreateGlitchTrigger(bool isQuality = false)
    {
        return new ImprovisedTool(
            ToolId: Guid.NewGuid().ToString(),
            ToolName: isQuality ? "Quality Glitch Trigger" : "Glitch Trigger",
            ToolType: ImprovisedToolType.GlitchTrigger,
            BonusAmount: isQuality ? QualityBonus : StandardBonus,
            UsesRemaining: isQuality ? QualityDurability : StandardDurability,
            IsQuality: isQuality,
            SpecialEffect: "Force mechanism into [Glitched] state");
    }

    /// <summary>
    /// Creates a Bypass Clamps tool.
    /// </summary>
    /// <param name="isQuality">True if crafted with critical success.</param>
    /// <returns>A new Bypass Clamps tool.</returns>
    /// <remarks>
    /// <para>
    /// Bypass Clamps provide +1d10 (standard) or +2d10 (quality) to terminal hacking attempts.
    /// </para>
    /// <para>
    /// Special Effect: Skip Layer 1 (Access) of terminal infiltration, starting at Layer 2.
    /// </para>
    /// </remarks>
    public static ImprovisedTool CreateBypassClamps(bool isQuality = false)
    {
        return new ImprovisedTool(
            ToolId: Guid.NewGuid().ToString(),
            ToolName: isQuality ? "Quality Bypass Clamps" : "Bypass Clamps",
            ToolType: ImprovisedToolType.BypassClamps,
            BonusAmount: isQuality ? QualityBonus : StandardBonus,
            UsesRemaining: isQuality ? QualityDurability : StandardDurability,
            IsQuality: isQuality,
            SpecialEffect: "Skip Layer 1 of terminal infiltration");
    }

    // -------------------------------------------------------------------------
    // Instance Methods
    // -------------------------------------------------------------------------

    /// <summary>
    /// Consumes one use of the tool.
    /// </summary>
    /// <returns>A new tool with decremented uses, or null if broken.</returns>
    /// <remarks>
    /// <para>
    /// When the last use is consumed, the tool breaks and null is returned.
    /// </para>
    /// <para>
    /// This is an immutable operation - a new ImprovisedTool instance is returned.
    /// </para>
    /// </remarks>
    public ImprovisedTool? UseOnce()
    {
        if (UsesRemaining <= 1)
        {
            return null; // Tool breaks
        }

        return this with { UsesRemaining = UsesRemaining - 1 };
    }

    /// <summary>
    /// Forces the tool to break (e.g., from a fumble).
    /// </summary>
    /// <returns>A broken tool with 0 uses.</returns>
    /// <remarks>
    /// This represents a fumble during bypass that destroys the tool prematurely.
    /// </remarks>
    public ImprovisedTool Break()
    {
        return this with { UsesRemaining = 0 };
    }

    // -------------------------------------------------------------------------
    // Display Methods
    // -------------------------------------------------------------------------

    /// <summary>
    /// Creates a display string for the tool.
    /// </summary>
    /// <returns>A formatted multi-line description of the tool.</returns>
    public string ToDisplayString()
    {
        var lines = new List<string>
        {
            $"[{ToolName}]",
            $"Type: {ToolType}",
            $"Bonus: +{BonusAmount}d10 to {AssistsWithBypassType}",
            $"Uses: {UsesRemaining}/{MaxDurability}",
            $"Condition: {Condition}"
        };

        if (HasSpecialEffect)
        {
            lines.Add($"Special: {SpecialEffect}");
        }

        return string.Join(Environment.NewLine, lines);
    }

    /// <summary>
    /// Returns a compact string for logging purposes.
    /// </summary>
    /// <returns>A log-friendly string representation.</returns>
    public string ToLogString()
    {
        return $"ImprovisedTool[Name={ToolName} Type={ToolType} " +
               $"Bonus=+{BonusAmount}d10 Uses={UsesRemaining}/{MaxDurability} " +
               $"Quality={IsQuality} Special={HasSpecialEffect}]";
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return ToDisplayString();
    }
}
