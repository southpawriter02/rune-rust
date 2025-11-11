using RuneAndRust.Core;

namespace RuneAndRust.Engine;

/// <summary>
/// v0.7: Factory for applying specializations to characters
/// Specializations are unlocked with 10 PP during gameplay
/// Each specialization grants access to 9 tiered abilities
/// </summary>
public class SpecializationFactory
{
    /// <summary>
    /// Apply a specialization to a character
    /// Adds tier 1 abilities immediately (unlocked with specialization)
    /// Tier 2 and 3 abilities must still be purchased with PP
    /// </summary>
    public static void ApplySpecialization(PlayerCharacter character, Specialization specialization)
    {
        if (character.Specialization != Specialization.None)
        {
            throw new InvalidOperationException("Character already has a specialization!");
        }

        character.Specialization = specialization;

        // Add specialization-specific abilities based on type
        switch (specialization)
        {
            case Specialization.BoneSetter:
                AddBoneSetterAbilities(character);
                break;
            case Specialization.JotunReader:
                AddJotunReaderAbilities(character);
                break;
            case Specialization.Skald:
                AddSkaldAbilities(character);
                break;
            // Future Warrior specializations
            case Specialization.Berserker:
            case Specialization.ShieldBearer:
            case Specialization.WeaponMaster:
                throw new NotImplementedException($"{specialization} not yet implemented");
            default:
                throw new ArgumentException($"Invalid specialization: {specialization}");
        }
    }

    /// <summary>
    /// Check if a character can choose a specific specialization
    /// </summary>
    public static bool CanChooseSpecialization(PlayerCharacter character, Specialization specialization)
    {
        // Must not already have a specialization
        if (character.Specialization != Specialization.None)
            return false;

        // Check archetype compatibility
        return specialization switch
        {
            // Adept specializations
            Specialization.BoneSetter => character.Class == CharacterClass.Adept,
            Specialization.JotunReader => character.Class == CharacterClass.Adept,
            Specialization.Skald => character.Class == CharacterClass.Adept,

            // Warrior specializations (future)
            Specialization.Berserker => character.Class == CharacterClass.Warrior,
            Specialization.ShieldBearer => character.Class == CharacterClass.Warrior,
            Specialization.WeaponMaster => character.Class == CharacterClass.Warrior,

            _ => false
        };
    }

    /// <summary>
    /// Get list of available specializations for a character's archetype
    /// </summary>
    public static List<Specialization> GetAvailableSpecializations(CharacterClass characterClass)
    {
        return characterClass switch
        {
            CharacterClass.Adept => new List<Specialization>
            {
                Specialization.BoneSetter,
                Specialization.JotunReader,
                Specialization.Skald
            },
            CharacterClass.Warrior => new List<Specialization>
            {
                Specialization.Berserker,
                Specialization.ShieldBearer,
                Specialization.WeaponMaster
            },
            _ => new List<Specialization>() // Scavenger and Mystic don't have specializations yet
        };
    }

    public static string GetSpecializationDescription(Specialization specialization)
    {
        return specialization switch
        {
            Specialization.BoneSetter =>
                "BONE-SETTER (Support/Healer)\n" +
                "Non-magical medic and sanity anchor. Uses Field Medicine crafting to create healing items.\n" +
                "Primary: WITS | Secondary: FINESSE | Trauma Risk: None (Coherent)\n" +
                "Core Role: Keep party alive with healing items, remove debuffs, stabilize sanity\n" +
                "Tier 1: Field Medic I (passive), Mend Wound, Apply Tourniquet\n" +
                "Tier 2: Anatomical Insight, Administer Antidote, Triage (passive)\n" +
                "Tier 3: Cognitive Realignment, \"First, Do No Harm\" (passive)\n" +
                "Capstone: Miracle Worker (massive heal + cleanse)",

            Specialization.JotunReader =>
                "JÖTUN-READER (Utility/Analyst)\n" +
                "Forensic analyst of the apocalypse. Exposes enemy weaknesses for tactical advantage.\n" +
                "Primary: WITS | Secondary: FINESSE | Trauma Risk: High (pays Stress for knowledge)\n" +
                "Core Role: Analyze enemies, apply debuffs, translate runic puzzles, unlock secrets\n" +
                "Tier 1: Scholarly Acumen I (passive), Analyze Weakness, Runic Linguistics (passive)\n" +
                "Tier 2: Exploit Design Flaw, Navigational Bypass, Structural Insight (passive)\n" +
                "Tier 3: Calculated Triage (passive), The Unspoken Truth\n" +
                "Capstone: Architect of the Silence (shut down Undying, 15 Stress cost)",

            Specialization.Skald =>
                "SKALD (Buffer/Debuffer)\n" +
                "Warrior-poet who wields structured narrative as weapon. Maintains performances for battlefield control.\n" +
                "Primary: WILL | Secondary: WITS | Trauma Risk: Low (Coherent with minor costs)\n" +
                "Core Role: Provide party-wide buffs/debuffs through sustained performances\n" +
                "Tier 1: Oral Tradition I (passive), Saga of Courage (performance), Dirge of Defeat (performance)\n" +
                "Tier 2: Rousing Verse, Song of Silence, Enduring Performance (passive)\n" +
                "Tier 3: Lay of the Iron Wall (performance), Heart of the Clan (passive)\n" +
                "Capstone: Saga of the Einherjar (Inspired + temp HP, Stress cost on end)",

            // Future Warrior specializations
            Specialization.Berserker => "BERSERKER - Not yet implemented",
            Specialization.ShieldBearer => "SHIELD-BEARER - Not yet implemented",
            Specialization.WeaponMaster => "WEAPON-MASTER - Not yet implemented",

            _ => "Unknown specialization"
        };
    }

    #region Bone-Setter Abilities

    private static void AddBoneSetterAbilities(PlayerCharacter character)
    {
        // TIER 1 - Available immediately upon unlocking specialization

        // Field Medic I (Passive)
        character.Abilities.Add(new Ability
        {
            Name = "Field Medic I",
            Description = "[PASSIVE] +1d to Field Medicine crafting checks. Start each expedition with 3 free Standard Healing Poultices.",
            StaminaCost = 0,
            Type = AbilityType.Utility,
            AttributeUsed = "wits",
            BonusDice = 0,
            SuccessThreshold = 0,
            CurrentRank = 1,
            MaxRank = 1
        });

        // Mend Wound (Active)
        character.Abilities.Add(new Ability
        {
            Name = "Mend Wound",
            Description = "Consume a healing poultice to restore HP to an ally. Effectiveness based on poultice quality.",
            StaminaCost = 5,
            Type = AbilityType.Utility,
            AttributeUsed = "wits",
            BonusDice = 1,
            SuccessThreshold = 2,
            CurrentRank = 1,
            MaxRank = 3
        });

        // Apply Tourniquet (Active)
        character.Abilities.Add(new Ability
        {
            Name = "Apply Tourniquet",
            Description = "Remove [Bleeding] status from an ally. No stamina cost - emergency field medicine.",
            StaminaCost = 0,
            Type = AbilityType.Utility,
            AttributeUsed = "wits",
            BonusDice = 2,
            SuccessThreshold = 2,
            CurrentRank = 1,
            MaxRank = 3
        });

        // TIER 2 - Must be unlocked with PP (not added automatically)
        // Will be added through progression system

        // TIER 3 - Must be unlocked with PP
        // Will be added through progression system

        // CAPSTONE - Must be unlocked with PP
        // Will be added through progression system
    }

    #endregion

    #region Jötun-Reader Abilities

    private static void AddJotunReaderAbilities(PlayerCharacter character)
    {
        // TIER 1 - Available immediately upon unlocking specialization

        // Scholarly Acumen I (Passive)
        character.Abilities.Add(new Ability
        {
            Name = "Scholarly Acumen I",
            Description = "[PASSIVE] +1d to System Bypass and investigation checks. Knowledge is your weapon.",
            StaminaCost = 0,
            Type = AbilityType.Utility,
            AttributeUsed = "wits",
            BonusDice = 0,
            SuccessThreshold = 0,
            CurrentRank = 1,
            MaxRank = 1
        });

        // Analyze Weakness (Active)
        character.Abilities.Add(new Ability
        {
            Name = "Analyze Weakness",
            Description = "Reveal enemy HP, Resistances, and Vulnerabilities to entire party. Costs 5 Psychic Stress (staring into the crash hurts).",
            StaminaCost = 30,
            Type = AbilityType.Utility,
            AttributeUsed = "wits",
            BonusDice = 3,
            SuccessThreshold = 2,
            // Special: Reveals enemy stats + 5 Stress cost (handled in CombatEngine)
            CurrentRank = 1,
            MaxRank = 3
        });

        // Runic Linguistics (Passive)
        character.Abilities.Add(new Ability
        {
            Name = "Runic Linguistics",
            Description = "[PASSIVE] Automatically translate non-magical runic inscriptions. Bypass puzzle gates that would require difficult WITS checks.",
            StaminaCost = 0,
            Type = AbilityType.Utility,
            AttributeUsed = "wits",
            BonusDice = 0,
            SuccessThreshold = 0,
            CurrentRank = 1,
            MaxRank = 1
        });

        // TIER 2, 3, and CAPSTONE added through progression
    }

    #endregion

    #region Skald Abilities

    private static void AddSkaldAbilities(PlayerCharacter character)
    {
        // TIER 1 - Available immediately upon unlocking specialization

        // Oral Tradition I (Passive)
        character.Abilities.Add(new Ability
        {
            Name = "Oral Tradition I",
            Description = "[PASSIVE] +1d to Rhetoric and lore checks. Your words carry weight.",
            StaminaCost = 0,
            Type = AbilityType.Utility,
            AttributeUsed = "will",
            BonusDice = 0,
            SuccessThreshold = 0,
            CurrentRank = 1,
            MaxRank = 1
        });

        // Saga of Courage (Performance)
        character.Abilities.Add(new Ability
        {
            Name = "Saga of Courage",
            Description = "[PERFORMANCE] All allies immune to [Feared] and gain +1d to WILL Resolve checks. Duration: WILL score rounds. Cannot take other actions while performing.",
            StaminaCost = 40,
            Type = AbilityType.Utility,
            AttributeUsed = "will",
            BonusDice = 2,
            SuccessThreshold = 2,
            // Special: Channeled performance (handled in CombatEngine)
            CurrentRank = 1,
            MaxRank = 3
        });

        // Dirge of Defeat (Performance)
        character.Abilities.Add(new Ability
        {
            Name = "Dirge of Defeat",
            Description = "[PERFORMANCE] All enemies suffer -2 Accuracy penalty. Duration: WILL score rounds. Cannot take other actions while performing.",
            StaminaCost = 40,
            Type = AbilityType.Control,
            AttributeUsed = "will",
            BonusDice = 2,
            SuccessThreshold = 2,
            // Special: Channeled performance (handled in CombatEngine)
            CurrentRank = 1,
            MaxRank = 3
        });

        // TIER 2, 3, and CAPSTONE added through progression
    }

    #endregion
}
