// ------------------------------------------------------------------------------
// <copyright file="IceEncounter.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Represents an ICE encounter during terminal hacking, tracking the state
// from activation through resolution.
// Part of v0.15.4c ICE Countermeasures implementation.
// </summary>
// ------------------------------------------------------------------------------

using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents an ICE encounter during terminal hacking, tracking the state
/// from activation through resolution.
/// </summary>
/// <remarks>
/// <para>
/// ICE encounters are created when a character triggers defensive programs
/// during terminal infiltration, typically on Layer 2 (Authentication) failure.
/// </para>
/// <para>
/// The encounter tracks the ICE type, its rating (difficulty), and the
/// resolution outcome. Encounters are immutable—use factory methods and
/// <see cref="WithOutcome"/> to create modified copies.
/// </para>
/// </remarks>
/// <param name="EncounterId">Unique identifier for this ICE encounter.</param>
/// <param name="IceType">The type of ICE encountered (Passive, Active, Lethal).</param>
/// <param name="IceRating">The difficulty rating of the ICE (12-24).</param>
/// <param name="Triggered">Whether the ICE has been activated.</param>
/// <param name="EncounterResult">The outcome of the encounter (Pending, Won, Lost, Evaded).</param>
public readonly record struct IceEncounter(
    string EncounterId,
    IceType IceType,
    int IceRating,
    bool Triggered,
    IceResolutionOutcome EncounterResult)
{
    // -------------------------------------------------------------------------
    // Computed Properties
    // -------------------------------------------------------------------------

    /// <summary>
    /// Whether this encounter is still awaiting resolution.
    /// </summary>
    /// <remarks>
    /// A pending encounter requires the character to make a contested check
    /// or saving throw before proceeding with infiltration.
    /// </remarks>
    public bool IsPending => EncounterResult == IceResolutionOutcome.Pending;

    /// <summary>
    /// Whether the character successfully dealt with this ICE.
    /// </summary>
    /// <remarks>
    /// Success means the character either won the contested check, made the save,
    /// or evaded the ICE (for Passive type). This does not indicate the absence
    /// of consequences—Lethal ICE success still inflicts stress.
    /// </remarks>
    public bool WasSuccessful => EncounterResult is IceResolutionOutcome.CharacterWon
                                                 or IceResolutionOutcome.Evaded;

    /// <summary>
    /// Whether the ICE won this encounter.
    /// </summary>
    /// <remarks>
    /// ICE victory means the character failed the contested check or saving throw.
    /// Consequences vary by ICE type from location reveal (Passive) to neural
    /// strike (Lethal).
    /// </remarks>
    public bool IceWon => EncounterResult == IceResolutionOutcome.IceWon;

    // -------------------------------------------------------------------------
    // Factory Methods
    // -------------------------------------------------------------------------

    /// <summary>
    /// Creates a new triggered ICE encounter with pending resolution.
    /// </summary>
    /// <param name="iceType">The type of ICE.</param>
    /// <param name="iceRating">The ICE's difficulty rating.</param>
    /// <returns>A new IceEncounter in triggered, pending state.</returns>
    /// <remarks>
    /// Use this factory method when ICE first activates. The encounter ID
    /// is generated automatically using a GUID.
    /// </remarks>
    public static IceEncounter CreateTriggered(IceType iceType, int iceRating)
    {
        return new IceEncounter(
            EncounterId: Guid.NewGuid().ToString(),
            IceType: iceType,
            IceRating: iceRating,
            Triggered: true,
            EncounterResult: IceResolutionOutcome.Pending
        );
    }

    /// <summary>
    /// Creates a copy of this encounter with an updated resolution outcome.
    /// </summary>
    /// <param name="outcome">The resolution outcome.</param>
    /// <returns>A new IceEncounter with the specified outcome.</returns>
    /// <remarks>
    /// This method returns a new instance with the updated outcome.
    /// The original encounter remains unchanged (immutability).
    /// </remarks>
    public IceEncounter WithOutcome(IceResolutionOutcome outcome)
    {
        return this with { EncounterResult = outcome };
    }

    // -------------------------------------------------------------------------
    // DC Calculation
    // -------------------------------------------------------------------------

    /// <summary>
    /// Converts the ICE rating to a success-counting DC.
    /// </summary>
    /// <returns>The DC for contested checks against this ICE.</returns>
    /// <remarks>
    /// <para>
    /// Standard conversion: Rating / 6, rounded up, minimum 1.
    /// </para>
    /// <para>
    /// Example conversions:
    /// <list type="bullet">
    ///   <item><description>Rating 12 → DC 2</description></item>
    ///   <item><description>Rating 16 → DC 3</description></item>
    ///   <item><description>Rating 20 → DC 4</description></item>
    ///   <item><description>Rating 24 → DC 4</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Note: Lethal ICE uses a fixed DC 16 for WILL saves, not this calculation.
    /// </para>
    /// </remarks>
    public int GetDc()
    {
        // Standard conversion: Rating / 6, rounded up, minimum 1
        return Math.Max(1, (int)Math.Ceiling(IceRating / 6.0));
    }

    // -------------------------------------------------------------------------
    // Display Methods
    // -------------------------------------------------------------------------

    /// <summary>
    /// Returns a display string for the ICE encounter.
    /// </summary>
    /// <returns>A formatted string describing this ICE encounter.</returns>
    /// <remarks>
    /// Format: "{Type} ICE - Rating {Rating} [{Status}]"
    /// Example: "Active (Attack) ICE - Rating 16 [Pending]"
    /// </remarks>
    public string ToDisplayString()
    {
        var typeStr = IceType switch
        {
            IceType.Passive => "Passive (Trace)",
            IceType.Active => "Active (Attack)",
            IceType.Lethal => "Lethal (Neural)",
            _ => IceType.ToString()
        };

        var statusStr = EncounterResult switch
        {
            IceResolutionOutcome.Pending => "Pending",
            IceResolutionOutcome.CharacterWon => "Defeated",
            IceResolutionOutcome.IceWon => "Activated",
            IceResolutionOutcome.Evaded => "Evaded",
            _ => EncounterResult.ToString()
        };

        return $"{typeStr} ICE - Rating {IceRating} [{statusStr}]";
    }

    /// <summary>
    /// Returns a compact string for logging purposes.
    /// </summary>
    /// <returns>A log-friendly string representation.</returns>
    public string ToLogString()
    {
        return $"ICE[{EncounterId[..8]}] Type={IceType} Rating={IceRating} Result={EncounterResult}";
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return ToDisplayString();
    }
}
