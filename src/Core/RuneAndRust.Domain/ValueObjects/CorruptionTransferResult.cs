// ═══════════════════════════════════════════════════════════════════════════════
// CorruptionTransferResult.cs
// Immutable value object representing the outcome of transferring corruption
// between two characters. This is the signature mechanic of the Blot-Priest
// specialization, who willingly absorbs Runic Blight from allies at personal
// cost. Captures both characters' resulting corruption values and whether the
// transfer caused a Terminal Error for the receiving character.
// Version: 0.18.1c
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents the result of transferring corruption between two characters.
/// </summary>
/// <remarks>
/// <para>
/// Corruption transfer is the signature ability of the Blot-Priest specialization.
/// The Blot-Priest willingly absorbs Runic Blight from an ally, reducing the
/// ally's corruption at the cost of increasing their own. This record captures
/// the complete outcome of the transfer for both characters.
/// </para>
/// <para>
/// Transfer mechanics:
/// <list type="bullet">
///   <item>
///     <description>
///       The source character (being cleansed) has their corruption reduced
///       by the transfer amount, clamped to a minimum of 0.
///     </description>
///   </item>
///   <item>
///     <description>
///       The target character (absorbing corruption) has their corruption
///       increased by the transfer amount, clamped to a maximum of 100.
///     </description>
///   </item>
///   <item>
///     <description>
///       If the target reaches 100 corruption, a Terminal Error is triggered
///       and the <see cref="TargetTerminalError"/> flag is set.
///     </description>
///   </item>
///   <item>
///     <description>
///       A failed transfer (e.g., amount exceeds source corruption or
///       invalid parameters) returns <see cref="Success"/> = <c>false</c>
///       with no state changes.
///     </description>
///   </item>
/// </list>
/// </para>
/// <para>
/// This result is created by <c>CorruptionService.TransferCorruption</c>
/// (v0.18.1d) and consumed by the UI for dramatic presentation and by the
/// combat log for transfer history tracking.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var result = CorruptionTransferResult.Create(
///     success: true,
///     amountTransferred: 10,
///     sourceNewCorruption: 30,
///     targetNewCorruption: 85,
///     targetTerminalError: false);
///
/// if (result.Success)
///     Console.WriteLine($"Transferred {result.AmountTransferred} corruption");
///
/// if (result.TargetTerminalError)
///     Console.WriteLine("WARNING: Transfer caused Terminal Error for the absorber!");
/// </code>
/// </example>
/// <seealso cref="Entities.CorruptionTracker"/>
/// <seealso cref="CorruptionState"/>
/// <seealso cref="TerminalErrorResult"/>
public readonly record struct CorruptionTransferResult
{
    // ═══════════════════════════════════════════════════════════════════════════
    // PROPERTIES — Stored
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets whether the transfer was successfully executed.
    /// </summary>
    /// <value>
    /// <c>true</c> if the transfer completed and both characters' corruption
    /// values were updated; <c>false</c> if the transfer was invalid (e.g.,
    /// amount exceeded source corruption, invalid character IDs, or same
    /// character specified for both source and target).
    /// </value>
    public bool Success { get; }

    /// <summary>
    /// Gets the amount of corruption that was actually transferred.
    /// </summary>
    /// <value>
    /// The corruption amount moved from source to target. If the transfer
    /// failed (<see cref="Success"/> = <c>false</c>), this is 0. May be
    /// less than the requested amount if the source had insufficient
    /// corruption to fulfill the full request.
    /// </value>
    public int AmountTransferred { get; }

    /// <summary>
    /// Gets the source character's corruption after the transfer.
    /// </summary>
    /// <value>
    /// The corruption level of the character being cleansed after the
    /// transfer amount was subtracted. Range: [0, 100]. If the transfer
    /// failed, this reflects the source's unchanged corruption value.
    /// </value>
    public int SourceNewCorruption { get; }

    /// <summary>
    /// Gets the target character's corruption after the transfer.
    /// </summary>
    /// <value>
    /// The corruption level of the character absorbing the Blight after the
    /// transfer amount was added. Range: [0, 100]. If the transfer failed,
    /// this reflects the target's unchanged corruption value.
    /// </value>
    public int TargetNewCorruption { get; }

    /// <summary>
    /// Gets whether the transfer caused a Terminal Error for the target.
    /// </summary>
    /// <value>
    /// <c>true</c> if the target character's corruption reached 100 as a
    /// result of absorbing the transferred corruption, triggering a Terminal
    /// Error survival check; <c>false</c> otherwise.
    /// </value>
    /// <remarks>
    /// When this is <c>true</c>, the caller should immediately initiate a
    /// Terminal Error check for the target character via
    /// <c>ICorruptionService.PerformTerminalErrorCheck</c>. This represents
    /// the ultimate sacrifice a Blot-Priest can make — risking their own
    /// transformation to save an ally from the Blight.
    /// </remarks>
    public bool TargetTerminalError { get; }

    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTRUCTOR (private — use factory methods)
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Private constructor to enforce factory pattern.
    /// </summary>
    /// <param name="success">Whether the transfer was successful.</param>
    /// <param name="amountTransferred">The amount of corruption transferred.</param>
    /// <param name="sourceNewCorruption">The source character's new corruption value.</param>
    /// <param name="targetNewCorruption">The target character's new corruption value.</param>
    /// <param name="targetTerminalError">Whether the target reached Terminal Error.</param>
    private CorruptionTransferResult(
        bool success,
        int amountTransferred,
        int sourceNewCorruption,
        int targetNewCorruption,
        bool targetTerminalError)
    {
        Success = success;
        AmountTransferred = amountTransferred;
        SourceNewCorruption = sourceNewCorruption;
        TargetNewCorruption = targetNewCorruption;
        TargetTerminalError = targetTerminalError;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // FACTORY METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a corruption transfer result.
    /// </summary>
    /// <param name="success">Whether the transfer was successfully executed.</param>
    /// <param name="amountTransferred">The amount of corruption transferred (0 if failed).</param>
    /// <param name="sourceNewCorruption">The source character's new corruption value.</param>
    /// <param name="targetNewCorruption">The target character's new corruption value.</param>
    /// <param name="targetTerminalError">Whether the target reached Terminal Error (100 corruption).</param>
    /// <returns>
    /// A new <see cref="CorruptionTransferResult"/> with the specified transfer outcome.
    /// </returns>
    /// <example>
    /// <code>
    /// // Successful transfer
    /// var success = CorruptionTransferResult.Create(
    ///     success: true,
    ///     amountTransferred: 15,
    ///     sourceNewCorruption: 25,
    ///     targetNewCorruption: 60,
    ///     targetTerminalError: false);
    ///
    /// // Failed transfer (amount exceeds source corruption)
    /// var failure = CorruptionTransferResult.Create(
    ///     success: false,
    ///     amountTransferred: 0,
    ///     sourceNewCorruption: 10,
    ///     targetNewCorruption: 45,
    ///     targetTerminalError: false);
    /// </code>
    /// </example>
    public static CorruptionTransferResult Create(
        bool success,
        int amountTransferred,
        int sourceNewCorruption,
        int targetNewCorruption,
        bool targetTerminalError) =>
        new(success, amountTransferred, sourceNewCorruption, targetNewCorruption, targetTerminalError);

    // ═══════════════════════════════════════════════════════════════════════════
    // DEBUGGING
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Returns a string representation of the corruption transfer result for debugging and logging.
    /// </summary>
    /// <returns>
    /// A formatted string showing the transfer outcome, amount, both characters'
    /// resulting corruption values, and Terminal Error status if applicable.
    /// </returns>
    /// <example>
    /// <code>
    /// var result = CorruptionTransferResult.Create(true, 15, 25, 60, false);
    /// result.ToString();
    /// // Returns "Transfer SUCCESS: 15 corruption moved (Source: 25, Target: 60)"
    ///
    /// var terminal = CorruptionTransferResult.Create(true, 20, 10, 100, true);
    /// terminal.ToString();
    /// // Returns "Transfer SUCCESS: 20 corruption moved (Source: 10, Target: 100) [TARGET TERMINAL ERROR!]"
    ///
    /// var failure = CorruptionTransferResult.Create(false, 0, 40, 50, false);
    /// failure.ToString();
    /// // Returns "Transfer FAILED: No corruption moved (Source: 40, Target: 50)"
    /// </code>
    /// </example>
    public override string ToString() =>
        Success
            ? $"Transfer SUCCESS: {AmountTransferred} corruption moved (Source: {SourceNewCorruption}, Target: {TargetNewCorruption})" +
              (TargetTerminalError ? " [TARGET TERMINAL ERROR!]" : "")
            : $"Transfer FAILED: No corruption moved (Source: {SourceNewCorruption}, Target: {TargetNewCorruption})";
}
