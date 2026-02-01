// ═══════════════════════════════════════════════════════════════════════════════
// CpsUiEffects.cs
// Represents the UI distortion effects for a CPS stage. As CPS severity
// increases, the game's UI becomes increasingly distorted to reflect the
// character's deteriorating perception of reality. This record defines all
// visual effect parameters for consistent rendering across the application.
// Version: 0.18.2b
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.ValueObjects;

using RuneAndRust.Domain.Enums;

/// <summary>
/// Represents the UI distortion effects for a CPS stage.
/// </summary>
/// <remarks>
/// <para>
/// As CPS severity increases, the game's UI becomes increasingly distorted
/// to reflect the character's deteriorating perception of reality.
/// This record defines all visual effect parameters.
/// </para>
/// <para>
/// Effect Progression:
/// <list type="bullet">
/// <item>None: Clean UI, no distortion</item>
/// <item>WeightOfKnowing: Subtle peripheral static</item>
/// <item>GlimmerMadness: Text glitching, moderate distortion</item>
/// <item>RuinMadness: Heavy distortion, leetspeak corruption</item>
/// <item>HollowShell: Maximum distortion or blackout</item>
/// </list>
/// </para>
/// </remarks>
/// <param name="Stage">The CPS stage these effects apply to.</param>
/// <param name="DistortionIntensity">Overall distortion intensity (0.0-1.0).</param>
/// <param name="PeripheralStatic">Whether screen edges show static noise.</param>
/// <param name="TextGlitching">Whether text randomly glitches/scrambles.</param>
/// <param name="LeetSpeakLevel">Level of text-to-leetspeak corruption (0-4).</param>
/// <param name="SystemWarnings">Whether to show system warning overlays.</param>
/// <param name="ColorTint">Hex color tint to apply to screen.</param>
/// <param name="ScreenLag">Whether to apply input/visual lag effects.</param>
/// <seealso cref="CpsStage"/>
/// <seealso cref="CpsState"/>
public readonly record struct CpsUiEffects(
    CpsStage Stage,
    float DistortionIntensity,
    bool PeripheralStatic,
    bool TextGlitching,
    int LeetSpeakLevel,
    bool SystemWarnings,
    string ColorTint,
    bool ScreenLag)
{
    // ═══════════════════════════════════════════════════════════════════════════
    // ARROW-EXPRESSION PROPERTIES — Effect Indicators
    // ═══════════════════════════════════════════════════════════════════════════

    #region Arrow-Expression Properties

    /// <summary>
    /// Gets whether any visual distortion effects are active.
    /// </summary>
    /// <value>
    /// <c>true</c> if DistortionIntensity is greater than 0.0; otherwise, <c>false</c>.
    /// </value>
    public bool HasDistortion => DistortionIntensity > 0.0f;

    /// <summary>
    /// Gets whether text corruption effects are active.
    /// </summary>
    /// <value>
    /// <c>true</c> if TextGlitching is enabled or LeetSpeakLevel is greater than 0;
    /// otherwise, <c>false</c>.
    /// </value>
    public bool HasTextCorruption => TextGlitching || LeetSpeakLevel > 0;

    #endregion

    // ═══════════════════════════════════════════════════════════════════════════
    // FACTORY METHODS — Stage-Specific Effects
    // ═══════════════════════════════════════════════════════════════════════════

    #region Factory Methods

    /// <summary>
    /// Creates UI effects for the None stage (clean UI).
    /// </summary>
    /// <returns>A CpsUiEffects with no distortion.</returns>
    /// <remarks>
    /// Clear-minded state. UI renders normally with no visual effects.
    /// </remarks>
    public static CpsUiEffects ForNone() => new(
        Stage: CpsStage.None,
        DistortionIntensity: 0.0f,
        PeripheralStatic: false,
        TextGlitching: false,
        LeetSpeakLevel: 0,
        SystemWarnings: false,
        ColorTint: "#FFFFFF",
        ScreenLag: false);

    /// <summary>
    /// Creates UI effects for the WeightOfKnowing stage.
    /// </summary>
    /// <returns>A CpsUiEffects with subtle distortion.</returns>
    /// <remarks>
    /// Reality feels slightly "off". Subtle peripheral static and slight
    /// color desaturation. No text corruption yet.
    /// </remarks>
    public static CpsUiEffects ForWeightOfKnowing() => new(
        Stage: CpsStage.WeightOfKnowing,
        DistortionIntensity: 0.1f,
        PeripheralStatic: true,
        TextGlitching: false,
        LeetSpeakLevel: 0,
        SystemWarnings: false,
        ColorTint: "#E8E8E8",
        ScreenLag: false);

    /// <summary>
    /// Creates UI effects for the GlimmerMadness stage.
    /// </summary>
    /// <returns>A CpsUiEffects with moderate distortion.</returns>
    /// <remarks>
    /// Reality actively flickers. Text begins glitching and mild leetspeak
    /// corruption appears. Warm color tint suggests unease.
    /// </remarks>
    public static CpsUiEffects ForGlimmerMadness() => new(
        Stage: CpsStage.GlimmerMadness,
        DistortionIntensity: 0.4f,
        PeripheralStatic: true,
        TextGlitching: true,
        LeetSpeakLevel: 2,
        SystemWarnings: false,
        ColorTint: "#FFE0B0",
        ScreenLag: false);

    /// <summary>
    /// Creates UI effects for the RuinMadness stage.
    /// </summary>
    /// <returns>A CpsUiEffects with heavy distortion.</returns>
    /// <remarks>
    /// Mind fracturing. Heavy distortion, maximum leetspeak corruption,
    /// system warnings appear, and input lag is noticeable.
    /// Red color tint indicates danger.
    /// </remarks>
    public static CpsUiEffects ForRuinMadness() => new(
        Stage: CpsStage.RuinMadness,
        DistortionIntensity: 0.7f,
        PeripheralStatic: true,
        TextGlitching: true,
        LeetSpeakLevel: 4,
        SystemWarnings: true,
        ColorTint: "#FF8080",
        ScreenLag: true);

    /// <summary>
    /// Creates UI effects for the HollowShell stage.
    /// </summary>
    /// <returns>A CpsUiEffects with maximum/blackout distortion.</returns>
    /// <remarks>
    /// Mind shattered. Screen goes dark (no static or glitching because
    /// the character can barely perceive anything). System warnings remain
    /// for dramatic effect. Black color tint indicates terminal state.
    /// </remarks>
    public static CpsUiEffects ForHollowShell() => new(
        Stage: CpsStage.HollowShell,
        DistortionIntensity: 1.0f,
        PeripheralStatic: false, // Screen goes dark
        TextGlitching: false,
        LeetSpeakLevel: 0,
        SystemWarnings: true,
        ColorTint: "#000000",
        ScreenLag: true);

    /// <summary>
    /// Creates UI effects for a given CPS stage.
    /// </summary>
    /// <param name="stage">The CPS stage.</param>
    /// <returns>A CpsUiEffects appropriate for the specified stage.</returns>
    /// <remarks>
    /// Dispatcher method that returns the appropriate preset for each stage.
    /// Unknown stages default to ForNone().
    /// </remarks>
    public static CpsUiEffects ForStage(CpsStage stage) => stage switch
    {
        CpsStage.None => ForNone(),
        CpsStage.WeightOfKnowing => ForWeightOfKnowing(),
        CpsStage.GlimmerMadness => ForGlimmerMadness(),
        CpsStage.RuinMadness => ForRuinMadness(),
        CpsStage.HollowShell => ForHollowShell(),
        _ => ForNone()
    };

    #endregion

    // ═══════════════════════════════════════════════════════════════════════════
    // DISPLAY — String Representation
    // ═══════════════════════════════════════════════════════════════════════════

    #region Display

    /// <summary>
    /// Returns a string representation of the UI effects for debugging and logging.
    /// </summary>
    /// <returns>
    /// A formatted string showing stage, distortion, glitch, and leet level.
    /// </returns>
    /// <example>
    /// <code>
    /// var effects = CpsUiEffects.ForGlimmerMadness();
    /// // Returns "CpsUiEffects[GlimmerMadness]: Distortion=40%, Glitch=True, Leet=2"
    /// </code>
    /// </example>
    public override string ToString() =>
        $"CpsUiEffects[{Stage}]: Distortion={DistortionIntensity:P0}, " +
        $"Glitch={TextGlitching}, Leet={LeetSpeakLevel}";

    #endregion
}
