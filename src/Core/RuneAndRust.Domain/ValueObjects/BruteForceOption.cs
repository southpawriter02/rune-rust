// ------------------------------------------------------------------------------
// <copyright file="BruteForceOption.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Represents the brute force approach to bypassing a physical obstacle.
// When lockpicking or other finesse-based bypass isn't available,
// characters can attempt to force their way through using raw MIGHT.
// Part of v0.15.4h Alternative Bypass Methods implementation.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Domain.ValueObjects;

// =============================================================================
// BRUTE FORCE TARGET TYPE ENUM
// =============================================================================

/// <summary>
/// Classification of physical obstacles that can be forced through.
/// </summary>
/// <remarks>
/// <para>
/// Each target type has different base DC, maximum attempts, retry penalty,
/// consequences, and available tool modifiers. The difficulty progression
/// reflects the increasing strength of construction.
/// </para>
/// </remarks>
public enum BruteForceTargetType
{
    /// <summary>
    /// Standard wooden or light metal door (DC 12).
    /// </summary>
    /// <remarks>
    /// Simple doors are the easiest to force. Allows 5 attempts with +1 DC
    /// per retry. Primary consequence is noise.
    /// </remarks>
    SimpleDoor,

    /// <summary>
    /// Metal-reinforced door with heavy construction (DC 16).
    /// </summary>
    /// <remarks>
    /// Reinforced doors require significant strength. Allows 3 attempts with
    /// +2 DC per retry. Consequences include noise and content damage risk.
    /// </remarks>
    ReinforcedDoor,

    /// <summary>
    /// Heavy vault door, often Jötun-forged (DC 22).
    /// </summary>
    /// <remarks>
    /// Vaults are extremely difficult to force. Allows only 2 attempts with
    /// +3 DC per retry. Consequences include exhaustion, noise, and content damage.
    /// </remarks>
    Vault,

    /// <summary>
    /// Locked container, chest, or safe (DC 10-16 based on strength).
    /// </summary>
    /// <remarks>
    /// Containers vary in strength from 1-5, affecting DC (8 + strength × 2).
    /// Primary concerns are noise and content damage.
    /// </remarks>
    Container
}

// =============================================================================
// TOOL MODIFIER VALUE OBJECT
// =============================================================================

/// <summary>
/// A tool that modifies the DC of a brute force attempt.
/// </summary>
/// <remarks>
/// <para>
/// Tools provide DC modifiers (negative values make attempts easier) and
/// may also grant bonus dice to the roll. Using a tool risks breakage on fumble.
/// </para>
/// </remarks>
/// <param name="ToolName">Display name of the tool.</param>
/// <param name="DcModifier">DC modifier (negative = easier, positive = harder).</param>
/// <param name="BonusDice">Number of bonus d10s added to the dice pool.</param>
public readonly record struct ToolModifier(
    string ToolName,
    int DcModifier,
    int BonusDice = 0)
{
    /// <summary>
    /// Creates a display string for the tool modifier.
    /// </summary>
    /// <returns>A formatted string showing the tool's effects.</returns>
    /// <example>
    /// <code>
    /// var crowbar = new ToolModifier("Crowbar", -2, 1);
    /// Console.WriteLine(crowbar.ToDisplayString());
    /// // Output: "Crowbar: -2 DC, +1d10"
    /// </code>
    /// </example>
    public string ToDisplayString()
    {
        var dcSign = DcModifier <= 0 ? string.Empty : "+";
        var diceBonus = BonusDice > 0 ? $", +{BonusDice}d10" : string.Empty;
        return $"{ToolName}: {dcSign}{DcModifier} DC{diceBonus}";
    }
}

// =============================================================================
// BRUTE FORCE OPTION VALUE OBJECT
// =============================================================================

/// <summary>
/// Represents the brute force approach to bypassing a physical obstacle.
/// </summary>
/// <remarks>
/// <para>
/// When lockpicking or other finesse-based bypass isn't available,
/// characters can attempt to force their way through using raw MIGHT.
/// Brute force is always an option but comes with consequences:
/// <list type="bullet">
///   <item><description>Noise alerts nearby enemies</description></item>
///   <item><description>Contents may be damaged</description></item>
///   <item><description>Multiple attempts cause exhaustion</description></item>
///   <item><description>Structural damage may trigger security systems</description></item>
/// </list>
/// </para>
/// <para>
/// <b>Design Principle:</b> Every locked obstacle should be forceable,
/// but the consequences should make players consider whether the noise
/// and potential damage are acceptable.
/// </para>
/// </remarks>
/// <param name="TargetType">Classification of the physical obstacle.</param>
/// <param name="BaseDc">Base difficulty class for MIGHT check.</param>
/// <param name="Consequences">List of consequences that may apply on success.</param>
/// <param name="ToolModifiers">Available tool modifiers for the attempt.</param>
/// <param name="MaxAttempts">Maximum number of attempts before obstacle is impassable.</param>
/// <param name="RetryPenaltyPerAttempt">DC increase per failed attempt.</param>
/// <param name="TargetHitPoints">HP for the obstacle (for damage-based bypass).</param>
public readonly record struct BruteForceOption(
    BruteForceTargetType TargetType,
    int BaseDc,
    IReadOnlyList<BruteForceConsequence> Consequences,
    IReadOnlyList<ToolModifier> ToolModifiers,
    int MaxAttempts,
    int RetryPenaltyPerAttempt,
    int TargetHitPoints)
{
    // =========================================================================
    // STATIC FACTORY PROPERTIES
    // =========================================================================

    /// <summary>
    /// Creates a brute force option for a simple wooden door.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Simple Door Statistics:
    /// <list type="bullet">
    ///   <item><description>MIGHT DC: 12</description></item>
    ///   <item><description>Max Attempts: 5</description></item>
    ///   <item><description>Retry Penalty: +1 DC per failure</description></item>
    ///   <item><description>Hit Points: 20</description></item>
    ///   <item><description>Consequences: Loud noise</description></item>
    ///   <item><description>Tools: Crowbar (-2 DC), Sledgehammer (-4 DC)</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    public static BruteForceOption SimpleDoor => new(
        TargetType: BruteForceTargetType.SimpleDoor,
        BaseDc: 12,
        Consequences: new List<BruteForceConsequence>
        {
            BruteForceConsequence.LoudNoise
        }.AsReadOnly(),
        ToolModifiers: new List<ToolModifier>
        {
            new("Crowbar", -2, 1),
            new("Sledgehammer", -4, 2)
        }.AsReadOnly(),
        MaxAttempts: 5,
        RetryPenaltyPerAttempt: 1,
        TargetHitPoints: 20);

    /// <summary>
    /// Creates a brute force option for a reinforced door.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Reinforced Door Statistics:
    /// <list type="bullet">
    ///   <item><description>MIGHT DC: 16</description></item>
    ///   <item><description>Max Attempts: 3</description></item>
    ///   <item><description>Retry Penalty: +2 DC per failure</description></item>
    ///   <item><description>Hit Points: 40</description></item>
    ///   <item><description>Consequences: Very loud noise, content damage risk</description></item>
    ///   <item><description>Tools: Crowbar (-2), Sledgehammer (-3), Breaching Charge (-6)</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    public static BruteForceOption ReinforcedDoor => new(
        TargetType: BruteForceTargetType.ReinforcedDoor,
        BaseDc: 16,
        Consequences: new List<BruteForceConsequence>
        {
            BruteForceConsequence.VeryLoudNoise,
            BruteForceConsequence.ContentDamageRisk
        }.AsReadOnly(),
        ToolModifiers: new List<ToolModifier>
        {
            new("Crowbar", -2, 1),
            new("Sledgehammer", -3, 2),
            new("Breaching Charge", -6, 2)
        }.AsReadOnly(),
        MaxAttempts: 3,
        RetryPenaltyPerAttempt: 2,
        TargetHitPoints: 40);

    /// <summary>
    /// Creates a brute force option for a vault.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Vault Statistics:
    /// <list type="bullet">
    ///   <item><description>MIGHT DC: 22</description></item>
    ///   <item><description>Max Attempts: 2</description></item>
    ///   <item><description>Retry Penalty: +3 DC per failure</description></item>
    ///   <item><description>Hit Points: 80</description></item>
    ///   <item><description>Consequences: Very loud noise, exhaustion, content damage risk</description></item>
    ///   <item><description>Tools: Sledgehammer (-2), Breaching Charge (-4), Industrial Cutter (-6)</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    public static BruteForceOption Vault => new(
        TargetType: BruteForceTargetType.Vault,
        BaseDc: 22,
        Consequences: new List<BruteForceConsequence>
        {
            BruteForceConsequence.VeryLoudNoise,
            BruteForceConsequence.Exhausting,
            BruteForceConsequence.ContentDamageRisk
        }.AsReadOnly(),
        ToolModifiers: new List<ToolModifier>
        {
            new("Sledgehammer", -2, 2),
            new("Breaching Charge", -4, 2),
            new("Industrial Cutter", -6, 2)
        }.AsReadOnly(),
        MaxAttempts: 2,
        RetryPenaltyPerAttempt: 3,
        TargetHitPoints: 80);

    /// <summary>
    /// Creates a brute force option for a container with variable strength.
    /// </summary>
    /// <param name="containerStrength">Strength rating 1-5 affecting DC and HP.</param>
    /// <returns>A BruteForceOption configured for the specified container strength.</returns>
    /// <remarks>
    /// <para>
    /// Container Statistics (by strength):
    /// <list type="bullet">
    ///   <item><description>Strength 1 (flimsy): DC 10, HP 15</description></item>
    ///   <item><description>Strength 2 (normal): DC 12, HP 20</description></item>
    ///   <item><description>Strength 3 (sturdy): DC 14, HP 25</description></item>
    ///   <item><description>Strength 4 (reinforced): DC 16, HP 30</description></item>
    ///   <item><description>Strength 5 (heavy): DC 18, HP 35</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// All containers have:
    /// <list type="bullet">
    ///   <item><description>Max Attempts: 3</description></item>
    ///   <item><description>Retry Penalty: +1 DC per failure</description></item>
    ///   <item><description>Consequences: Loud noise, content damage risk</description></item>
    ///   <item><description>Tools: Crowbar (-2), Knife (-1)</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    public static BruteForceOption Container(int containerStrength = 3)
    {
        // Clamp strength to valid range
        var strength = Math.Clamp(containerStrength, 1, 5);

        return new BruteForceOption(
            TargetType: BruteForceTargetType.Container,
            BaseDc: 8 + (strength * 2), // DC 10-18
            Consequences: new List<BruteForceConsequence>
            {
                BruteForceConsequence.LoudNoise,
                BruteForceConsequence.ContentDamageRisk
            }.AsReadOnly(),
            ToolModifiers: new List<ToolModifier>
            {
                new("Crowbar", -2, 1),
                new("Knife", -1, 0)
            }.AsReadOnly(),
            MaxAttempts: 3,
            RetryPenaltyPerAttempt: 1,
            TargetHitPoints: 10 + (strength * 5)); // HP 15-35
    }

    // =========================================================================
    // INSTANCE METHODS
    // =========================================================================

    /// <summary>
    /// Gets the effective DC for an attempt, accounting for previous failures and fumbles.
    /// </summary>
    /// <param name="previousAttempts">Number of failed attempts.</param>
    /// <param name="hasFumbled">Whether a fumble occurred in previous attempts.</param>
    /// <returns>The adjusted DC for the current attempt.</returns>
    /// <remarks>
    /// <para>
    /// DC calculation: BaseDc + (previousAttempts × RetryPenalty) + (fumble ? 2 : 0)
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var door = BruteForceOption.SimpleDoor;
    /// var dc1 = door.GetEffectiveDc(0, false); // 12 (base)
    /// var dc2 = door.GetEffectiveDc(1, false); // 13 (base + 1 retry)
    /// var dc3 = door.GetEffectiveDc(1, true);  // 15 (base + 1 retry + fumble)
    /// </code>
    /// </example>
    public int GetEffectiveDc(int previousAttempts, bool hasFumbled)
    {
        var dc = BaseDc + (previousAttempts * RetryPenaltyPerAttempt);

        if (hasFumbled)
        {
            dc += 2; // Fumble adds permanent +2 DC
        }

        return dc;
    }

    /// <summary>
    /// Gets the DC modifier from a specific tool.
    /// </summary>
    /// <param name="toolName">Name of the tool being used.</param>
    /// <returns>DC modifier (negative means easier), or 0 if tool not found.</returns>
    /// <remarks>
    /// Tool names are matched case-insensitively.
    /// </remarks>
    public int GetToolDcModifier(string toolName)
    {
        var modifier = ToolModifiers.FirstOrDefault(t =>
            t.ToolName.Equals(toolName, StringComparison.OrdinalIgnoreCase));
        return modifier.DcModifier;
    }

    /// <summary>
    /// Gets the bonus dice from a specific tool.
    /// </summary>
    /// <param name="toolName">Name of the tool being used.</param>
    /// <returns>Number of bonus d10s, or 0 if tool not found.</returns>
    public int GetToolBonusDice(string toolName)
    {
        var modifier = ToolModifiers.FirstOrDefault(t =>
            t.ToolName.Equals(toolName, StringComparison.OrdinalIgnoreCase));
        return modifier.BonusDice;
    }

    /// <summary>
    /// Determines if more attempts are possible.
    /// </summary>
    /// <param name="attemptsMade">Number of attempts already made.</param>
    /// <returns>True if the character can retry.</returns>
    public bool CanRetry(int attemptsMade) => attemptsMade < MaxAttempts;

    /// <summary>
    /// Gets the narrative description of this target.
    /// </summary>
    /// <returns>A descriptive string for the obstacle.</returns>
    public string GetDescription() => TargetType switch
    {
        BruteForceTargetType.SimpleDoor => "a wooden door with a simple lock",
        BruteForceTargetType.ReinforcedDoor => "a reinforced door with metal bracing",
        BruteForceTargetType.Vault => "a heavy vault door of Old World construction",
        BruteForceTargetType.Container => "a locked container",
        _ => "a physical obstacle"
    };

    /// <summary>
    /// Creates a display string for the brute force option.
    /// </summary>
    /// <returns>A formatted multi-line string showing all option details.</returns>
    /// <example>
    /// <code>
    /// var door = BruteForceOption.SimpleDoor;
    /// Console.WriteLine(door.ToDisplayString());
    /// // Output:
    /// // [Brute Force: SimpleDoor]
    /// // MIGHT DC: 12
    /// // Max Attempts: 5
    /// // Retry Penalty: +1 DC per failure
    /// //
    /// // Consequences:
    /// //   - Loud noise (avoided on critical)
    /// //
    /// // Tool Modifiers:
    /// //   - Crowbar: -2 DC, +1d10
    /// //   - Sledgehammer: -4 DC, +2d10
    /// </code>
    /// </example>
    public string ToDisplayString()
    {
        var lines = new List<string>
        {
            $"[Brute Force: {TargetType}]",
            $"MIGHT DC: {BaseDc}",
            $"Max Attempts: {MaxAttempts}",
            $"Retry Penalty: +{RetryPenaltyPerAttempt} DC per failure",
            string.Empty,
            "Consequences:"
        };

        foreach (var consequence in Consequences)
        {
            lines.Add($"  - {consequence.ToDisplayString()}");
        }

        if (ToolModifiers.Count > 0)
        {
            lines.Add(string.Empty);
            lines.Add("Tool Modifiers:");
            foreach (var tool in ToolModifiers)
            {
                lines.Add($"  - {tool.ToDisplayString()}");
            }
        }

        return string.Join(Environment.NewLine, lines);
    }
}
