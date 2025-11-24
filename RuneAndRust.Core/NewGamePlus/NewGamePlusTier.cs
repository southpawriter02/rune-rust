namespace RuneAndRust.Core.NewGamePlus;

/// <summary>
/// v0.40.1: New Game+ difficulty tiers
/// Represents the escalating difficulty levels for replay campaigns
/// </summary>
public enum NewGamePlusTier
{
    /// <summary>First playthrough (no NG+ modifiers)</summary>
    None = 0,

    /// <summary>NG+1: +50% difficulty (1.5x HP/Damage)</summary>
    PlusOne = 1,

    /// <summary>NG+2: +100% difficulty (2.0x HP/Damage)</summary>
    PlusTwo = 2,

    /// <summary>NG+3: +150% difficulty (2.5x HP/Damage)</summary>
    PlusThree = 3,

    /// <summary>NG+4: +200% difficulty (3.0x HP/Damage)</summary>
    PlusFour = 4,

    /// <summary>NG+5: +250% difficulty (3.5x HP/Damage, maximum tier)</summary>
    PlusFive = 5
}
