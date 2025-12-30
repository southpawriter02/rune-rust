namespace RuneAndRust.Core.Enums;

/// <summary>
/// Discriminator for polymorphic dialogue condition types.
/// Determines what kind of check is required for a dialogue option.
/// </summary>
/// <remarks>See: v0.4.2b (The Lexicon) for Dialogue System implementation.</remarks>
public enum DialogueConditionType
{
    /// <summary>Check a character attribute (e.g., WITS >= 6).</summary>
    Attribute = 0,

    /// <summary>Check character level (e.g., Level >= 5).</summary>
    Level = 1,

    /// <summary>Check faction reputation/disposition (e.g., Iron-Banes: Friendly).</summary>
    Reputation = 2,

    /// <summary>Check a game flag value (e.g., HasCompletedTutorial).</summary>
    Flag = 3,

    /// <summary>Check item possession (e.g., Has: Iron Key x1).</summary>
    Item = 4,

    /// <summary>Check specialization access (e.g., Is: Berserkr).</summary>
    Specialization = 5,

    /// <summary>Check specialization node unlocked (e.g., Has: Rage ability).</summary>
    Node = 6,

    /// <summary>Dice roll skill check (e.g., Roll WITS DC 3).</summary>
    SkillCheck = 7
}
