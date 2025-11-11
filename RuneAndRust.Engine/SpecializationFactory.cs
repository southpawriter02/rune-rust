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

        // TIER 2 - Advanced treatment abilities

        // Anatomical Insight (Active)
        character.Abilities.Add(new Ability
        {
            Name = "Anatomical Insight",
            Description = "Apply [Vulnerable] status to organic enemy. Target takes +25% damage for 3 turns. WITS check vs target's defense.",
            StaminaCost = 25,
            Type = AbilityType.Control,
            AttributeUsed = "wits",
            BonusDice = 2,
            SuccessThreshold = 3,
            // Special: Applies [Vulnerable] status (handled in CombatEngine)
            CurrentRank = 1,
            MaxRank = 3
        });

        // Administer Antidote (Active)
        character.Abilities.Add(new Ability
        {
            Name = "Administer Antidote",
            Description = "Remove [Poisoned] and [Disease] status effects from ally. Crafted antidotes are more effective.",
            StaminaCost = 15,
            Type = AbilityType.Utility,
            AttributeUsed = "wits",
            BonusDice = 2,
            SuccessThreshold = 2,
            // Special: Removes poison/disease (handled in CombatEngine)
            CurrentRank = 1,
            MaxRank = 3
        });

        // Triage (Passive)
        character.Abilities.Add(new Ability
        {
            Name = "Triage",
            Description = "[PASSIVE] All healing you provide to bloodied allies (HP < 50%) gains +25% effectiveness.",
            StaminaCost = 0,
            Type = AbilityType.Utility,
            AttributeUsed = "wits",
            BonusDice = 0,
            SuccessThreshold = 0,
            // Special: Passive healing bonus (handled in CombatEngine)
            CurrentRank = 1,
            MaxRank = 1
        });

        // TIER 3 - Mastery abilities

        // Cognitive Realignment (Active)
        character.Abilities.Add(new Ability
        {
            Name = "Cognitive Realignment",
            Description = "Remove [Feared] and [Disoriented] status effects from ally. Restore 2d6 Psychic Stress. Your presence anchors their sanity.",
            StaminaCost = 30,
            Type = AbilityType.Utility,
            AttributeUsed = "will",  // Uses WILL for mental stabilization
            BonusDice = 2,
            SuccessThreshold = 2,
            // Special: Removes mental debuffs + restores Stress (handled in CombatEngine)
            CurrentRank = 1,
            MaxRank = 3
        });

        // "First, Do No Harm" (Passive)
        character.Abilities.Add(new Ability
        {
            Name = "First, Do No Harm",
            Description = "[PASSIVE] After healing an ally, gain +2 Defense until your next turn. Helping others keeps you vigilant.",
            StaminaCost = 0,
            Type = AbilityType.Defense,
            AttributeUsed = "wits",
            BonusDice = 0,
            SuccessThreshold = 0,
            DefensePercent = 0,  // Flat +2 defense bonus
            DefenseDuration = 1,
            // Special: Triggered after healing (handled in CombatEngine)
            CurrentRank = 1,
            MaxRank = 1
        });

        // CAPSTONE - Ultimate healing ability

        // Miracle Worker (Active)
        character.Abilities.Add(new Ability
        {
            Name = "Miracle Worker",
            Description = "⭐ CAPSTONE: Massive heal (4d6 + WITS) and remove ALL physical debuffs ([Bleeding], [Poisoned], [Disease], [Vulnerable]). Limited uses per expedition.",
            StaminaCost = 60,
            Type = AbilityType.Utility,
            AttributeUsed = "wits",
            BonusDice = 3,
            SuccessThreshold = 2,
            DamageDice = 4,  // Used for healing dice
            // Special: Massive heal + cleanse all debuffs (handled in CombatEngine)
            CurrentRank = 1,
            MaxRank = 3
        });
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

        // TIER 2 - Advanced tactical support

        // Exploit Design Flaw (Active)
        character.Abilities.Add(new Ability
        {
            Name = "Exploit Design Flaw",
            Description = "Apply [Analyzed] debuff to target. All allies gain +2 Accuracy against [Analyzed] enemies for 3 turns.",
            StaminaCost = 35,
            Type = AbilityType.Control,
            AttributeUsed = "wits",
            BonusDice = 2,
            SuccessThreshold = 2,
            // Special: Applies [Analyzed] status (+2 Accuracy for all allies)
            CurrentRank = 1,
            MaxRank = 3
        });

        // Navigational Bypass (Active)
        character.Abilities.Add(new Ability
        {
            Name = "Navigational Bypass",
            Description = "Grant entire party +2d to resist/avoid trap damage for next 3 rooms. Your knowledge of Jötun-Forged systems protects your allies.",
            StaminaCost = 30,
            Type = AbilityType.Utility,
            AttributeUsed = "wits",
            BonusDice = 3,
            SuccessThreshold = 2,
            // Special: Party-wide trap resistance buff (handled in exploration logic)
            CurrentRank = 1,
            MaxRank = 3
        });

        // Structural Insight (Passive)
        character.Abilities.Add(new Ability
        {
            Name = "Structural Insight",
            Description = "[PASSIVE] Automatically detect unstable structures (collapsing floors, weak walls, environmental hazards) before entering a room.",
            StaminaCost = 0,
            Type = AbilityType.Utility,
            AttributeUsed = "wits",
            BonusDice = 0,
            SuccessThreshold = 0,
            // Special: Pre-room hazard detection (handled in exploration logic)
            CurrentRank = 1,
            MaxRank = 1
        });

        // TIER 3 - Mastery abilities

        // Calculated Triage (Passive)
        character.Abilities.Add(new Ability
        {
            Name = "Calculated Triage",
            Description = "[PASSIVE] Allies within 10 feet of you gain +25% effectiveness from consumable healing items. Your analysis optimizes treatment.",
            StaminaCost = 0,
            Type = AbilityType.Utility,
            AttributeUsed = "wits",
            BonusDice = 0,
            SuccessThreshold = 0,
            // Special: Proximity-based healing buff (handled in CombatEngine)
            CurrentRank = 1,
            MaxRank = 1
        });

        // The Unspoken Truth (Active)
        character.Abilities.Add(new Ability
        {
            Name = "The Unspoken Truth",
            Description = "Knowledge attack that inflicts [Disoriented] status. Speak the enemy's true designation - most cannot bear the weight of their own identity.",
            StaminaCost = 40,
            Type = AbilityType.Control,
            AttributeUsed = "will",  // Mental assault using knowledge
            BonusDice = 3,
            SuccessThreshold = 3,
            DamageDice = 2,  // 2d6 psychic damage
            IgnoresArmor = true,
            // Special: Applies [Disoriented] + psychic damage (handled in CombatEngine)
            CurrentRank = 1,
            MaxRank = 3
        });

        // CAPSTONE - Command syntax mastery

        // Architect of the Silence (Active)
        character.Abilities.Add(new Ability
        {
            Name = "Architect of the Silence",
            Description = "⭐ CAPSTONE: Speak original Jötun command syntax. Apply [Seized] status to Jötun-Forged or Undying enemy for 2 turns (cannot take actions). Costs 15 Psychic Stress.",
            StaminaCost = 60,
            Type = AbilityType.Control,
            AttributeUsed = "will",  // WILL vs enemy WILL
            BonusDice = 4,
            SuccessThreshold = 4,  // Difficult but devastating
            // Special: [Seized] status (complete lockdown) + 15 Stress cost (handled in CombatEngine)
            // Only works on Jötun-Forged or Undying enemies
            CurrentRank = 1,
            MaxRank = 3
        });
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

        // TIER 2 - Advanced composition

        // Rousing Verse (Active - NOT a performance)
        character.Abilities.Add(new Ability
        {
            Name = "Rousing Verse",
            Description = "Restore 2d6 Stamina to target ally with a brief, energizing verse. Not a performance - immediate effect.",
            StaminaCost = 20,
            Type = AbilityType.Utility,
            AttributeUsed = "will",
            BonusDice = 2,
            SuccessThreshold = 2,
            DamageDice = 2,  // Used for stamina restoration dice
            // Special: Restores Stamina instead of HP (handled in CombatEngine)
            CurrentRank = 1,
            MaxRank = 3
        });

        // Song of Silence (Active)
        character.Abilities.Add(new Ability
        {
            Name = "Song of Silence",
            Description = "Apply [Silenced] status to enemy caster/channeler. Interrupted performances end immediately. WILL vs enemy WILL.",
            StaminaCost = 35,
            Type = AbilityType.Control,
            AttributeUsed = "will",
            BonusDice = 2,
            SuccessThreshold = 3,
            // Special: Applies [Silenced] status (prevents casting/performances)
            CurrentRank = 1,
            MaxRank = 3
        });

        // Enduring Performance (Passive)
        character.Abilities.Add(new Ability
        {
            Name = "Enduring Performance",
            Description = "[PASSIVE] All your [PERFORMANCE] abilities last +2 additional rounds. Longer sagas, deeper impact.",
            StaminaCost = 0,
            Type = AbilityType.Utility,
            AttributeUsed = "will",
            BonusDice = 0,
            SuccessThreshold = 0,
            // Special: Extends performance duration (handled in CombatEngine)
            CurrentRank = 1,
            MaxRank = 1
        });

        // TIER 3 - Mastery abilities

        // Lay of the Iron Wall (Performance)
        character.Abilities.Add(new Ability
        {
            Name = "Lay of the Iron Wall",
            Description = "[PERFORMANCE] Front row allies gain +2 Soak (damage reduction). Duration: WILL score rounds. Shield-song of ancient defenders.",
            StaminaCost = 50,
            Type = AbilityType.Defense,
            AttributeUsed = "will",
            BonusDice = 2,
            SuccessThreshold = 2,
            DefensePercent = 0,  // Uses Soak instead
            // Special: +2 Soak for front row (handled in CombatEngine)
            CurrentRank = 1,
            MaxRank = 3
        });

        // Heart of the Clan (Passive)
        character.Abilities.Add(new Ability
        {
            Name = "Heart of the Clan",
            Description = "[PASSIVE] Allies in the same row as you gain +1d to defensive Resolve checks (vs fear, stun, etc.). Your presence bolsters their will.",
            StaminaCost = 0,
            Type = AbilityType.Defense,
            AttributeUsed = "will",
            BonusDice = 0,
            SuccessThreshold = 0,
            // Special: Proximity-based defensive buff (handled in CombatEngine)
            CurrentRank = 1,
            MaxRank = 1
        });

        // CAPSTONE - The ultimate saga

        // Saga of the Einherjar (Performance)
        character.Abilities.Add(new Ability
        {
            Name = "Saga of the Einherjar",
            Description = "⭐ CAPSTONE: [PERFORMANCE] All allies gain [Inspired] (+3 damage dice) and 2d6 temporary HP. Duration: WILL score rounds. When performance ends, you suffer 10 Psychic Stress from the effort.",
            StaminaCost = 70,
            Type = AbilityType.Utility,
            AttributeUsed = "will",
            BonusDice = 3,
            SuccessThreshold = 2,
            DamageDice = 2,  // Used for temp HP dice
            // Special: [Inspired] status + temp HP + 10 Stress on end (handled in CombatEngine)
            CurrentRank = 1,
            MaxRank = 3
        });
    }

    #endregion
}
