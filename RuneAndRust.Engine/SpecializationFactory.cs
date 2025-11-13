using RuneAndRust.Core;

namespace RuneAndRust.Engine;

/// <summary>
/// v0.7: Factory for applying specializations to characters
/// Specializations are unlocked with 3 PP during gameplay (v0.18: reduced from 10)
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
                "BONE-SETTER (Healer / Sanity Anchor)\n" +
                "Pragmatic Restoration - Non-magical combat medic who fights entropy.\n" +
                "Primary: WITS | Secondary: FINESSE | Trauma Risk: None (Coherent)\n" +
                "Core Role: Heal both HP and Stress, craft consumables, enable high-risk strategies\n" +
                "Tier 1: Field Medic (3 ranks), Mend Wound (3 ranks), Apply Tourniquet (3 ranks)\n" +
                "Tier 2: Anatomical Insight (3 ranks), Administer Antidote (3 ranks), Triage (3 ranks)\n" +
                "Tier 3: Cognitive Realignment (3 ranks), Defensive Focus (3 ranks)\n" +
                "Capstone: Miracle Worker (emergency massive heal, death protection, once per expedition)",

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
        // v0.19.4: Updated Bone-Setter specialization with full rank progression
        // TIER 1 - Available immediately upon unlocking specialization (3 PP each)

        // Grant starting Field Medicine supplies
        GrantBoneSetterStartingItems(character);

        // Field Medic (Passive) - 3 ranks
        character.Abilities.Add(new Ability
        {
            Name = "Field Medic",
            Description = "You are an expert at preparing medical supplies. Your kit is always ready, your hands always steady.",
            StaminaCost = 0,
            Type = AbilityType.Utility,
            AttributeUsed = "wits",
            BonusDice = 0,
            SuccessThreshold = 0,
            CurrentRank = 1,
            MaxRank = 3,
            CostToRank2 = 20,
            CostToRank3 = 0
        });

        // Mend Wound (Active) - 3 ranks
        character.Abilities.Add(new Ability
        {
            Name = "Mend Wound",
            Description = "You quickly dress the wound, applying poultice with practiced efficiency. The healing begins.",
            StaminaCost = 35,
            Type = AbilityType.Utility,
            AttributeUsed = "wits",
            BonusDice = 0,
            SuccessThreshold = 0,  // Auto-success, consumes poultice
            DamageDice = 3,  // Used for healing dice
            CurrentRank = 1,
            MaxRank = 3,
            CostToRank2 = 20,
            CostToRank3 = 0
        });

        // Apply Tourniquet (Active) - 3 ranks
        character.Abilities.Add(new Ability
        {
            Name = "Apply Tourniquet",
            Description = "With speed and precision, you stop the life-threatening blood loss. They'll live.",
            StaminaCost = 30,
            Type = AbilityType.Utility,
            AttributeUsed = "",
            BonusDice = 0,
            SuccessThreshold = 0,  // Auto-success
            CurrentRank = 1,
            MaxRank = 3,
            CostToRank2 = 20,
            CostToRank3 = 0
        });

        // TIER 2 - Advanced treatment abilities (4 PP each, requires 8 PP in tree)

        // Anatomical Insight (Active) - 3 ranks
        character.Abilities.Add(new Ability
        {
            Name = "Anatomical Insight",
            Description = "You observe their anatomy and recognize the weak points. There—that's where to strike.",
            StaminaCost = 40,
            Type = AbilityType.Control,
            AttributeUsed = "wits",
            BonusDice = 0,
            SuccessThreshold = 0,  // WITS check vs enemy
            // Special: Applies [Vulnerable] status (handled in CombatEngine)
            CurrentRank = 1,
            MaxRank = 3,
            CostToRank2 = 20,
            CostToRank3 = 0
        });

        // Administer Antidote (Active) - 3 ranks
        character.Abilities.Add(new Ability
        {
            Name = "Administer Antidote",
            Description = "You administer the carefully prepared antidote. The toxins are neutralized.",
            StaminaCost = 30,
            Type = AbilityType.Utility,
            AttributeUsed = "",
            BonusDice = 0,
            SuccessThreshold = 0,  // Auto-success, consumes antidote
            // Special: Removes poison/disease (handled in CombatEngine)
            CurrentRank = 1,
            MaxRank = 3,
            CostToRank2 = 20,
            CostToRank3 = 0
        });

        // Triage (Passive) - 3 ranks
        character.Abilities.Add(new Ability
        {
            Name = "Triage",
            Description = "You understand battlefield medicine: treat the most grievous wounds first. Maximum efficiency.",
            StaminaCost = 0,
            Type = AbilityType.Utility,
            AttributeUsed = "wits",
            BonusDice = 0,
            SuccessThreshold = 0,
            // Special: Passive healing bonus (handled in CombatEngine)
            CurrentRank = 1,
            MaxRank = 3,
            CostToRank2 = 20,
            CostToRank3 = 0
        });

        // TIER 3 - Mastery abilities (5 PP each, requires 16 PP in tree)

        // Cognitive Realignment (Active) - 3 ranks
        character.Abilities.Add(new Ability
        {
            Name = "Cognitive Realignment",
            Description = "Calming techniques, pressure points, smelling salts—you reboot their panicked mind.",
            StaminaCost = 45,
            Type = AbilityType.Utility,
            AttributeUsed = "",
            BonusDice = 0,
            SuccessThreshold = 0,  // Auto-success, consumes draught
            // Special: Removes mental debuffs + restores Stress (handled in CombatEngine)
            CurrentRank = 1,
            MaxRank = 3,
            CostToRank2 = 20,
            CostToRank3 = 0
        });

        // Defensive Focus (Passive) - 3 ranks
        character.Abilities.Add(new Ability
        {
            Name = "Defensive Focus",
            Description = "When focused on saving another, you enter heightened awareness. You will not fall.",
            StaminaCost = 0,
            Type = AbilityType.Defense,
            AttributeUsed = "",
            BonusDice = 0,
            SuccessThreshold = 0,
            DefensePercent = 0,  // Flat defense bonus (varies by rank)
            DefenseDuration = 1,
            // Special: Triggered after healing (handled in CombatEngine)
            CurrentRank = 1,
            MaxRank = 3,
            CostToRank2 = 20,
            CostToRank3 = 0
        });

        // CAPSTONE - Ultimate healing ability (6 PP, requires 24 PP + both Tier 3)

        // Miracle Worker (Active) - 3 ranks
        character.Abilities.Add(new Ability
        {
            Name = "Miracle Worker",
            Description = "⭐ CAPSTONE: A complex procedure—stimulants, field surgery, sheer will. You bring them back from the brink.",
            StaminaCost = 50,
            Type = AbilityType.Utility,
            AttributeUsed = "wits",
            BonusDice = 0,
            SuccessThreshold = 0,  // Auto-success, consumes Miracle Tincture
            DamageDice = 8,  // Used for healing dice
            // Special: Massive heal + cleanse all debuffs (handled in CombatEngine)
            CurrentRank = 1,
            MaxRank = 3,
            CostToRank2 = 20,
            CostToRank3 = 0
        });
    }

    #endregion

    #region Jötun-Reader Abilities

    private static void AddJotunReaderAbilities(PlayerCharacter character)
    {
        // v0.19.7: Updated Jötun-Reader specialization with full rank progression
        // TIER 1 - Available immediately upon unlocking specialization (3 PP each)

        // Scholarly Acumen I (Passive) - 3 ranks
        character.Abilities.Add(new Ability
        {
            Name = "Scholarly Acumen I",
            Description = "Your mind is a finely honed instrument, constantly processing layers of forgotten history.",
            StaminaCost = 0,
            Type = AbilityType.Utility,
            AttributeUsed = "wits",
            BonusDice = 0,
            SuccessThreshold = 0,
            CurrentRank = 1,
            MaxRank = 3,
            CostToRank2 = 20,
            CostToRank3 = 0
        });

        // Analyze Weakness (Active) - 3 ranks
        character.Abilities.Add(new Ability
        {
            Name = "Analyze Weakness",
            Description = "Clinical observation reveals structural flaws. You document weakness like a pathologist identifies cause of death.",
            StaminaCost = 30,
            Type = AbilityType.Utility,
            AttributeUsed = "wits",
            BonusDice = 0,
            SuccessThreshold = 0,
            // Special: Reveals enemy stats + 5 Stress cost (handled in CombatEngine)
            CurrentRank = 1,
            MaxRank = 3,
            CostToRank2 = 20,
            CostToRank3 = 0
        });

        // Runic Linguistics (Passive) - 3 ranks
        character.Abilities.Add(new Ability
        {
            Name = "Runic Linguistics",
            Description = "You read the grammar of reality's operating system. You understand error messages in a dead language.",
            StaminaCost = 0,
            Type = AbilityType.Utility,
            AttributeUsed = "wits",
            BonusDice = 0,
            SuccessThreshold = 0,
            CurrentRank = 1,
            MaxRank = 3,
            CostToRank2 = 20,
            CostToRank3 = 0
        });

        // TIER 2 - Advanced tactical support (4 PP each, requires 8 PP in tree)

        // Exploit Design Flaw (Active) - 3 ranks
        character.Abilities.Add(new Ability
        {
            Name = "Exploit Design Flaw",
            Description = "Strike the left knee joint—actuator is damaged. Your tactical guidance turns allies into precision instruments.",
            StaminaCost = 35,
            Type = AbilityType.Control,
            AttributeUsed = "wits",
            BonusDice = 0,
            SuccessThreshold = 0,
            // Special: Applies [Analyzed] status (+2/+3/+4 Accuracy for all allies)
            CurrentRank = 1,
            MaxRank = 3,
            CostToRank2 = 20,
            CostToRank3 = 0
        });

        // Navigational Bypass (Active) - 3 ranks
        character.Abilities.Add(new Ability
        {
            Name = "Navigational Bypass",
            Description = "The trigger mechanism is corroded on the western edge. Distribute weight evenly—sensor won't register threshold pressure.",
            StaminaCost = 30,
            Type = AbilityType.Utility,
            AttributeUsed = "wits",
            BonusDice = 0,
            SuccessThreshold = 0,
            // Special: Party-wide trap resistance buff (handled in exploration logic)
            CurrentRank = 1,
            MaxRank = 3,
            CostToRank2 = 20,
            CostToRank3 = 0
        });

        // Structural Insight (Passive) - 3 ranks
        character.Abilities.Add(new Ability
        {
            Name = "Structural Insight",
            Description = "Support beams compromised. Eastern wall provides solid cover—load-bearing, reinforced. Center of room will collapse.",
            StaminaCost = 0,
            Type = AbilityType.Utility,
            AttributeUsed = "wits",
            BonusDice = 0,
            SuccessThreshold = 0,
            // Special: Pre-room hazard detection (handled in exploration logic)
            CurrentRank = 1,
            MaxRank = 3,
            CostToRank2 = 20,
            CostToRank3 = 0
        });

        // TIER 3 - Mastery abilities (5 PP each, requires 16 PP in tree)

        // Calculated Triage (Passive) - 3 ranks
        character.Abilities.Add(new Ability
        {
            Name = "Calculated Triage",
            Description = "Apply pressure to brachial artery first. Follow wound track with the applicator. Your clinical guidance optimizes treatment.",
            StaminaCost = 0,
            Type = AbilityType.Utility,
            AttributeUsed = "wits",
            BonusDice = 0,
            SuccessThreshold = 0,
            // Special: Proximity-based healing buff (handled in CombatEngine)
            CurrentRank = 1,
            MaxRank = 3,
            CostToRank2 = 20,
            CostToRank3 = 0
        });

        // The Unspoken Truth (Active) - 3 ranks
        character.Abilities.Add(new Ability
        {
            Name = "The Unspoken Truth",
            Description = "Your 'god' is ERROR CODE 0x4A7F. You worship a crash log. The truth shatters their worldview.",
            StaminaCost = 40,
            Type = AbilityType.Control,
            AttributeUsed = "wits",  // Opposed WITS vs WILL
            BonusDice = 0,
            SuccessThreshold = 0,
            DamageDice = 0,
            IgnoresArmor = true,
            // Special: Applies [Disoriented] + psychic stress to target (handled in CombatEngine)
            CurrentRank = 1,
            MaxRank = 3,
            CostToRank2 = 20,
            CostToRank3 = 0
        });

        // CAPSTONE - Command syntax mastery (6 PP, requires 24 PP + both Tier 3)

        // Architect of the Silence (Active) - 3 ranks
        character.Abilities.Add(new Ability
        {
            Name = "Architect of the Silence",
            Description = "⭐ CAPSTONE: PRIORITY OVERRIDE: TIWAZ PROTOCOL ALPHA. CEASE HOSTILE OPERATIONS. The machine's logic wars with corrupted directives.",
            StaminaCost = 60,
            Type = AbilityType.Control,
            AttributeUsed = "will",  // WILL vs enemy WILL
            BonusDice = 0,
            SuccessThreshold = 0,
            // Special: [Seized] status (complete lockdown) + 15-20 Stress cost (handled in CombatEngine)
            // Only works on Jötun-Forged or Undying enemies
            CurrentRank = 1,
            MaxRank = 3,
            CostToRank2 = 20,
            CostToRank3 = 0
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

    #region Helper Methods

    /// <summary>
    /// Grant Bone-Setter starting items (3 Healing Poultices + crafting components)
    /// </summary>
    private static void GrantBoneSetterStartingItems(PlayerCharacter character)
    {
        // Add 3 Standard Healing Poultices
        for (int i = 0; i < 3; i++)
        {
            character.Consumables.Add(new Consumable
            {
                Name = "Healing Poultice",
                Description = "A compress of medicinal herbs that restores vitality.",
                Type = ConsumableType.Medicine,
                Quality = CraftQuality.Standard,
                HPRestore = 15,
                MasterworkBonusHP = 5
            });
        }

        // Add starting crafting components (enough for a few crafts)
        character.CraftingComponents[ComponentType.CommonHerb] = 8;
        character.CraftingComponents[ComponentType.CleanCloth] = 4;
        character.CraftingComponents[ComponentType.Antiseptic] = 2;
        character.CraftingComponents[ComponentType.Suture] = 2;
    }

    #endregion
}
