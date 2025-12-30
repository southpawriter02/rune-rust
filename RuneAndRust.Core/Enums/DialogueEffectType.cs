namespace RuneAndRust.Core.Enums;

/// <summary>
/// Discriminator for polymorphic dialogue effect types.
/// Determines what happens when a dialogue option is selected.
/// </summary>
/// <remarks>See: v0.4.2b (The Lexicon) for Dialogue System implementation.</remarks>
public enum DialogueEffectType
{
    /// <summary>Modify faction reputation (e.g., +10 with Iron-Banes).</summary>
    ModifyReputation = 0,

    /// <summary>Give item to character (e.g., Receive: Rusty Key x1).</summary>
    GiveItem = 1,

    /// <summary>Remove item from character (e.g., Remove: Gold x50).</summary>
    RemoveItem = 2,

    /// <summary>Set a game flag (e.g., Set: MetOldScavenger = true).</summary>
    SetFlag = 3,

    /// <summary>Start a quest (e.g., Start: quest_iron_bane_intro).</summary>
    StartQuest = 4,

    /// <summary>Trigger combat encounter (e.g., Combat: ambush_bound_cultist).</summary>
    TriggerCombat = 5,

    /// <summary>Heal the character (e.g., Heal: 10 HP).</summary>
    Heal = 6,

    /// <summary>Grant experience/legend points (e.g., Grant: 50 Legend).</summary>
    GiveXP = 7
}
