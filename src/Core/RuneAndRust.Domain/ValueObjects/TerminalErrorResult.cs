// ═══════════════════════════════════════════════════════════════════════════════
// TerminalErrorResult.cs
// Immutable value object representing the outcome of a Terminal Error survival
// check. When corruption reaches 100, the character must make a WILL-based
// check to avoid becoming Forlorn (lost to the Blight). This record captures
// the check outcome, final corruption value, and whether the character survived
// or was transformed into an unplayable NPC.
// Version: 0.18.1b
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents the result of a Terminal Error survival check.
/// </summary>
/// <remarks>
/// <para>
/// When corruption reaches 100, the character must make a Terminal Error check
/// to avoid becoming Forlorn (lost to the Blight). This record captures the
/// outcome of that check.
/// </para>
/// <para>
/// Terminal Error Check Mechanics:
/// <list type="bullet">
///   <item>
///     <description>
///       <strong>Trigger:</strong> Corruption reaches exactly 100.
///     </description>
///   </item>
///   <item>
///     <description>
///       <strong>Roll:</strong> WILL dice pool vs DC (default 3).
///     </description>
///   </item>
///   <item>
///     <description>
///       <strong>Success:</strong> Character survives, corruption set to 99.
///       The character narrowly avoids transformation.
///     </description>
///   </item>
///   <item>
///     <description>
///       <strong>Critical Success:</strong> Successes >= 2x DC. Character survives
///       with additional narrative benefit.
///     </description>
///   </item>
///   <item>
///     <description>
///       <strong>Failure:</strong> Character becomes Forlorn — an unplayable NPC
///       controlled by the GM. The character may appear as an antagonist in
///       future encounters.
///     </description>
///   </item>
/// </list>
/// </para>
/// <para>
/// This result is created by <c>CorruptionService.PerformTerminalErrorCheck</c>
/// (v0.18.1d) and consumed by the UI for dramatic presentation of the outcome.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var result = TerminalErrorResult.Success(successes: 4, dc: 3);
/// if (result.Survived)
///     Console.WriteLine($"Barely escaped! Corruption reduced to {result.FinalCorruption}");
/// else
///     Console.WriteLine("Character lost to the Blight...");
///
/// if (result.WasCriticalSuccess)
///     Console.WriteLine("Critical success — additional narrative benefit!");
/// </code>
/// </example>
/// <seealso cref="Entities.CorruptionTracker"/>
/// <seealso cref="CorruptionState"/>
public readonly record struct TerminalErrorResult
{
    // ═══════════════════════════════════════════════════════════════════════════
    // PROPERTIES — Stored
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets whether the character survived the Terminal Error.
    /// </summary>
    /// <value>
    /// <c>true</c> if the character's WILL check succeeded (successes >= DC),
    /// meaning the character narrowly avoids transformation; <c>false</c> if
    /// the check failed and the character became Forlorn.
    /// </value>
    public bool Survived { get; }

    /// <summary>
    /// Gets the final corruption value after resolution.
    /// </summary>
    /// <value>
    /// If survived: 99 (just below terminal, allowing continued play).
    /// If failed: 100 (character is Forlorn, unplayable).
    /// </value>
    /// <remarks>
    /// On survival, corruption is set to 99 rather than a lower value to
    /// maintain the existential tension — the character is one corruption
    /// point away from another Terminal Error check. This is an intentional
    /// design choice to prevent "safe" corruption levels after survival.
    /// </remarks>
    public int FinalCorruption { get; }

    /// <summary>
    /// Gets whether the character became Forlorn (lost to the Blight).
    /// </summary>
    /// <value>
    /// <c>true</c> if the character failed the Terminal Error check and has
    /// been consumed by the Blight; <c>false</c> if they survived.
    /// Always the inverse of <see cref="Survived"/>.
    /// </value>
    /// <remarks>
    /// A Forlorn character is an unplayable NPC, controlled by the GM.
    /// The character may appear as an antagonist in future encounters,
    /// transformed by the Runic Blight into something inhuman.
    /// </remarks>
    public bool BecameForlorn { get; }

    /// <summary>
    /// Gets the number of successes rolled on the survival check.
    /// </summary>
    /// <value>
    /// The number of successes from the WILL dice pool roll. Compare with
    /// <see cref="RequiredDc"/> to determine the margin of success or failure.
    /// </value>
    public int SuccessesRolled { get; }

    /// <summary>
    /// Gets the difficulty class that was required.
    /// </summary>
    /// <value>
    /// The number of successes needed to survive. Default is 3.
    /// May be modified by game effects or difficulty settings.
    /// </value>
    public int RequiredDc { get; }

    // ═══════════════════════════════════════════════════════════════════════════
    // PROPERTIES — Arrow-Expression (derived from stored properties)
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets whether the check was a critical success (double required successes).
    /// </summary>
    /// <value>
    /// <c>true</c> if the character survived AND rolled at least twice the
    /// required DC in successes; <c>false</c> otherwise.
    /// </value>
    /// <remarks>
    /// A critical success on a Terminal Error check may grant additional
    /// narrative benefits, such as insight into the Blight's nature or
    /// temporary resistance to further corruption.
    /// </remarks>
    public bool WasCriticalSuccess => Survived && SuccessesRolled >= RequiredDc * 2;

    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTRUCTOR (private — use factory methods)
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Private constructor to enforce factory pattern.
    /// </summary>
    /// <param name="survived">Whether the character survived.</param>
    /// <param name="finalCorruption">The final corruption value (99 if survived, 100 if Forlorn).</param>
    /// <param name="successesRolled">Number of successes rolled on the WILL check.</param>
    /// <param name="requiredDc">The DC that was required to survive.</param>
    private TerminalErrorResult(
        bool survived,
        int finalCorruption,
        int successesRolled,
        int requiredDc)
    {
        Survived = survived;
        FinalCorruption = finalCorruption;
        BecameForlorn = !survived;
        SuccessesRolled = successesRolled;
        RequiredDc = requiredDc;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // FACTORY METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a result for a successful survival check.
    /// </summary>
    /// <param name="successes">Number of successes rolled on the WILL check.</param>
    /// <param name="dc">The required DC (default 3).</param>
    /// <returns>
    /// A survival result with <see cref="Survived"/> = <c>true</c>,
    /// <see cref="FinalCorruption"/> = 99, and <see cref="BecameForlorn"/> = <c>false</c>.
    /// </returns>
    /// <example>
    /// <code>
    /// var result = TerminalErrorResult.Success(successes: 4, dc: 3);
    /// // result.Survived == true
    /// // result.FinalCorruption == 99
    /// // result.WasCriticalSuccess == false (4 &lt; 3 * 2)
    /// </code>
    /// </example>
    public static TerminalErrorResult Success(int successes, int dc = 3) =>
        new(survived: true, finalCorruption: 99, successesRolled: successes, requiredDc: dc);

    /// <summary>
    /// Creates a result for a failed survival check.
    /// </summary>
    /// <param name="successes">Number of successes rolled on the WILL check.</param>
    /// <param name="dc">The required DC (default 3).</param>
    /// <returns>
    /// A failure result with <see cref="Survived"/> = <c>false</c>,
    /// <see cref="FinalCorruption"/> = 100, and <see cref="BecameForlorn"/> = <c>true</c>.
    /// </returns>
    /// <example>
    /// <code>
    /// var result = TerminalErrorResult.Failure(successes: 2, dc: 3);
    /// // result.Survived == false
    /// // result.BecameForlorn == true
    /// // result.FinalCorruption == 100
    /// </code>
    /// </example>
    public static TerminalErrorResult Failure(int successes, int dc = 3) =>
        new(survived: false, finalCorruption: 100, successesRolled: successes, requiredDc: dc);

    // ═══════════════════════════════════════════════════════════════════════════
    // DEBUGGING
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Returns a string representation of the Terminal Error result for debugging and logging.
    /// </summary>
    /// <returns>
    /// A formatted string showing the outcome, successes/DC ratio, final corruption,
    /// and critical success indicator if applicable.
    /// </returns>
    /// <example>
    /// <code>
    /// var success = TerminalErrorResult.Success(successes: 6, dc: 3);
    /// success.ToString();
    /// // Returns "Terminal Error SURVIVED: 6/3 successes, corruption -> 99 [CRITICAL]"
    ///
    /// var failure = TerminalErrorResult.Failure(successes: 2, dc: 3);
    /// failure.ToString();
    /// // Returns "Terminal Error FAILED: 2/3 successes - CHARACTER BECAME FORLORN"
    /// </code>
    /// </example>
    public override string ToString() =>
        Survived
            ? $"Terminal Error SURVIVED: {SuccessesRolled}/{RequiredDc} successes, corruption -> {FinalCorruption}" +
              (WasCriticalSuccess ? " [CRITICAL]" : "")
            : $"Terminal Error FAILED: {SuccessesRolled}/{RequiredDc} successes - CHARACTER BECAME FORLORN";
}
