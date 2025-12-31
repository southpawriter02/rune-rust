using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Models.Magic;

namespace RuneAndRust.Core.Interfaces;

/// <summary>
/// Service for managing Mystic Aetheric Resonance—personal attunement to ambient Flux.
/// </summary>
public interface IResonanceService
{
    /// <summary>
    /// Modifies a character's resonance by the specified amount.
    /// </summary>
    /// <param name="character">The character whose resonance to modify.</param>
    /// <param name="amount">Amount to add (positive) or subtract (negative).</param>
    /// <param name="source">Description of what caused the change.</param>
    /// <returns>Result containing old/new values and threshold information.</returns>
    ResonanceResult ModifyResonance(Character character, int amount, string source);

    /// <summary>
    /// Gets the current resonance value for a character.
    /// </summary>
    /// <param name="character">The character to query.</param>
    /// <returns>Current resonance (0-100), or 0 if not a Mystic.</returns>
    int GetResonance(Character character);

    /// <summary>
    /// Gets the current resonance threshold for a character.
    /// </summary>
    /// <param name="character">The character to query.</param>
    /// <returns>Current threshold enum value.</returns>
    ResonanceThreshold GetThreshold(Character character);

    /// <summary>
    /// Gets the spell potency modifier based on current resonance.
    /// </summary>
    /// <param name="character">The character to query.</param>
    /// <returns>Multiplier (0.90 to 1.50).</returns>
    decimal GetPotencyModifier(Character character);

    /// <summary>
    /// Gets the modifiers for a specific casting mode.
    /// </summary>
    /// <param name="mode">The casting mode to evaluate.</param>
    /// <returns>Result containing resonance gain, cast time, and flux modifiers.</returns>
    CastingModeResult ApplyCastingModeModifiers(CastingMode mode);

    /// <summary>
    /// Processes resonance decay during rest.
    /// </summary>
    /// <param name="character">The character whose resonance should decay.</param>
    /// <param name="context">The context/reason for the decay.</param>
    /// <returns>Amount of resonance that decayed.</returns>
    int ProcessResonanceDecay(Character character, string context = "Unspecified");

    /// <summary>
    /// Triggers the Overflow state when resonance reaches 100.
    /// </summary>
    /// <param name="character">The character experiencing overflow.</param>
    /// <param name="context">The context triggering the overflow check.</param>
    /// <returns>Result containing potency bonus, duration, and risk information.</returns>
    OverflowResult TriggerOverflow(Character character, string context = "Unspecified");

    /// <summary>
    /// Processes the end of an Overflow state (forced discharge).
    /// </summary>
    /// <param name="character">The character to discharge.</param>
    /// <param name="context">The context/reason for the discharge.</param>
    /// <returns>Amount of resonance discharged.</returns>
    int ProcessOverflowDischarge(Character character, string context = "Unspecified");

    /// <summary>
    /// Resets a character's resonance to zero.
    /// </summary>
    /// <param name="character">The character to reset.</param>
    /// <param name="context">The context/reason for the reset.</param>
    void Reset(Character character, string context = "Unspecified");
}
