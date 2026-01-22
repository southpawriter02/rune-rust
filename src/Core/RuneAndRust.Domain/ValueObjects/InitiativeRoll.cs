namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents the result of an initiative roll used to determine turn order in combat.
/// </summary>
/// <remarks>
/// <para>Initiative determines when a combatant acts in combat turn order.</para>
/// <para>Formula: 1d10 + modifier (Finesse for players, InitiativeModifier for monsters).</para>
/// <para>Higher initiative acts first. Ties are broken by highest Finesse/InitiativeModifier.</para>
/// </remarks>
/// <param name="DiceResult">The underlying dice roll result from the DiceService.</param>
/// <param name="Modifier">The modifier applied (Finesse for players, InitiativeModifier for monsters).</param>
public readonly record struct InitiativeRoll(
    DiceRollResult DiceResult,
    int Modifier)
{
    /// <summary>
    /// Gets the total initiative value (dice roll + modifier).
    /// </summary>
    /// <remarks>
    /// This value is used for sorting turn order. Higher values act first.
    /// </remarks>
    public int Total => DiceResult.Total + Modifier;

    /// <summary>
    /// Gets the raw dice result before modifier.
    /// </summary>
    public int RollValue => DiceResult.Total;

    /// <summary>
    /// Gets whether this was a natural maximum roll (10 on d10).
    /// </summary>
    /// <remarks>
    /// Initiative uses sum-based mechanics; intentionally uses legacy property.
    /// </remarks>
#pragma warning disable CS0618 // IsNaturalMax intentionally used for sum-based initiative
    public bool IsNaturalMax => DiceResult.IsNaturalMax;
#pragma warning restore CS0618

    /// <summary>
    /// Gets whether this was a natural minimum roll (1 on d10).
    /// </summary>
    /// <remarks>
    /// Initiative uses sum-based mechanics; intentionally uses legacy property.
    /// </remarks>
#pragma warning disable CS0618 // IsNaturalOne intentionally used for sum-based initiative
    public bool IsNaturalOne => DiceResult.IsNaturalOne;
#pragma warning restore CS0618

    /// <summary>
    /// Creates a display string showing the initiative breakdown.
    /// </summary>
    /// <returns>A formatted string like "[8] +3 = 11".</returns>
    /// <example>
    /// <code>
    /// var roll = new InitiativeRoll(diceResult, 3);
    /// Console.WriteLine(roll.ToDisplayString()); // "[8] +3 = 11"
    /// </code>
    /// </example>
    public string ToDisplayString()
    {
        var modStr = Modifier >= 0 ? $"+{Modifier}" : Modifier.ToString();
        return $"[{RollValue}] {modStr} = {Total}";
    }

    /// <inheritdoc />
    public override string ToString() => ToDisplayString();
}
