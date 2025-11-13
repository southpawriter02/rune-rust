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
            // Warrior specializations
            case Specialization.SkarHordeAspirant:
            case Specialization.IronBane:
            case Specialization.AtgeirWielder:
                // These are implemented elsewhere
                break;

            // Adept specializations
            case Specialization.BoneSetter:
                AddBoneSetterAbilities(character);
                break;
            case Specialization.ScrapTinker:
                // Implemented elsewhere
                break;
            case Specialization.JotunReader:
                AddJotunReaderAbilities(character);
                break;

            // Mystic specializations (v0.19.8+)
            case Specialization.VardWarden:
                AddVardWardenAbilities(character);
                break;
            case Specialization.RustWitch:
                AddRustWitchAbilities(character);
                break;
            case Specialization.Runasmidr:  // v0.19.10
                AddRunasmidrAbilities(character);
                break;

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
            // Warrior specializations
            Specialization.SkarHordeAspirant => character.Class == CharacterClass.Warrior,
            Specialization.IronBane => character.Class == CharacterClass.Warrior,
            Specialization.AtgeirWielder => character.Class == CharacterClass.Warrior,

            // Adept specializations
            Specialization.BoneSetter => character.Class == CharacterClass.Adept,
            Specialization.ScrapTinker => character.Class == CharacterClass.Adept,
            Specialization.JotunReader => character.Class == CharacterClass.Adept,

            // Mystic specializations
            Specialization.VardWarden => character.Class == CharacterClass.Mystic,
            Specialization.RustWitch => character.Class == CharacterClass.Mystic,
            Specialization.Runasmidr => character.Class == CharacterClass.Mystic,  // v0.19.10

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
            CharacterClass.Warrior => new List<Specialization>
            {
                Specialization.SkarHordeAspirant,
                Specialization.IronBane,
                Specialization.AtgeirWielder
            },
            CharacterClass.Adept => new List<Specialization>
            {
                Specialization.BoneSetter,
                Specialization.ScrapTinker,
                Specialization.JotunReader
            },
            CharacterClass.Mystic => new List<Specialization>
            {
                Specialization.VardWarden,
                Specialization.RustWitch,
                Specialization.Runasmidr  // v0.19.10
            },
            _ => new List<Specialization>() // Scavenger doesn't have specializations yet
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

            // Mystic specializations (v0.19.8)
            Specialization.VardWarden =>
                "VARD-WARDEN (Defensive Caster)\n" +
                "Firewall architect who creates pockets of stable reality. Battlefield controller with runic barriers.\n" +
                "Primary: WILL | Secondary: WITS | Trauma Risk: None (Coherent)\n" +
                "Core Role: Create physical barriers, sanctify zones, protect allies, control battlefield\n" +
                "Tier 1: Sanctified Resolve I (passive +1d WILL vs Push/Pull), Runic Barrier (create wall), Consecrate Ground (healing zone)\n" +
                "Tier 2: Rune of Shielding (buff ally +2 Soak), Reinforce Ward (heal barrier/boost zone), Warden's Vigil (passive Stress resist)\n" +
                "Tier 3: Glyph of Sanctuary (party temp HP + Stress immunity), Aegis of Sanctity (passive barrier reflection + cleanse)\n" +
                "Capstone: Indomitable Bastion (reaction: negate fatal damage, create emergency barrier)",

            Specialization.RustWitch =>
                "RUST-WITCH (Heretical Debuffer)\n" +
                "Agent of entropy who accelerates inevitable collapse. Trades sanity for devastating power.\n" +
                "Primary: WILL | Secondary: WITS | Trauma Risk: EXTREME (every spell inflicts self-Corruption)\n" +
                "Core Role: Apply [Corroded] stacking debuff, shred armor, sustained damage, high risk/reward\n" +
                "Tier 1: Philosopher of Dust (passive +1d vs corrupted), Corrosive Curse (1 stack [Corroded], +2 Corruption), Entropic Field (passive -1 Armor to nearby enemies)\n" +
                "Tier 2: System Shock (2 stacks + [Stunned] vs Mechanical, +3 Corruption), Flash Rust (2 stacks AoE, +4 Corruption), Accelerated Entropy (passive +1d damage per stack)\n" +
                "Tier 3: Unmaking Word (double [Corroded] stacks, +4 Corruption), Cascade Reaction (passive: death spreads [Corroded])\n" +
                "Capstone: Entropic Cascade (execute if >50% Corrupted, +6 Corruption)",

            Specialization.Runasmidr =>
                "RÚNASMIÐR (Arcane Artisan)\n" +
                "System debugger who writes stable code patches into broken reality. Runesmith and battlefield controller.\n" +
                "Primary: WITS | Secondary: WILL | Trauma Risk: Low (Coherent specialization)\n" +
                "Core Role: Craft enchanted gear (out-of-combat), place runic traps (in-combat), force multiplier\n" +
                "Tier 1: Master Carver I (passive +1d to Runeforging), Hagalaz Trap (ice damage trap), Algiz Imbuement (defensive enchantment)\n" +
                "Tier 2: Debug Enemy (reveal stats, +3 Stress), Rune of Disruption ([Disoriented] trap), Uruz Imbuement (offensive enchantment)\n" +
                "Tier 3: Runic Synergy (restore resources when trap triggers), Rune of Isolation ([Rooted] + blocks buffs)\n" +
                "Capstone: Architect of Stability (Masterwork crafting + Saga Properties, Sanctuary zone ability)",

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

    #region Vard-Warden Abilities (v0.19.8)

    private static void AddVardWardenAbilities(PlayerCharacter character)
    {
        // v0.19.8: Vard-Warden specialization - Defensive Caster
        // TIER 1 - Available immediately upon unlocking specialization

        // Sanctified Resolve I (Passive) - Rank 1
        character.Abilities.Add(new Ability
        {
            Name = "Sanctified Resolve I",
            Description = "Passive: +1d to WILL Resolve Checks against [Push] and [Pull] effects. Your connection to stable Aether grounds you.",
            StaminaCost = 0,
            APCost = 0,
            Type = AbilityType.Utility,
            AttributeUsed = "",
            BonusDice = 0,
            SuccessThreshold = 0,
            CurrentRank = 1,
            MaxRank = 1  // Passive, no ranking
        });

        // Runic Barrier (Active) - 3 ranks
        character.Abilities.Add(new Ability
        {
            Name = "Runic Barrier",
            Description = "Create a physical wall of solidified Aether on target row (front/back). Barrier has 30 HP and blocks movement/line-of-sight for 2 turns.",
            StaminaCost = 0,
            APCost = 25,
            Type = AbilityType.Defense,
            AttributeUsed = "will",
            BonusDice = 0,
            SuccessThreshold = 2,
            CurrentRank = 1,
            MaxRank = 3,
            CostToRank2 = 5,
            CostToRank3 = 10
        });

        // Consecrate Ground (Active) - 3 ranks
        character.Abilities.Add(new Ability
        {
            Name = "Consecrate Ground",
            Description = "Target row becomes [Sanctified Ground] for 3 turns. Allies heal 1d6 HP at start of turn. [Blighted]/[Undying] enemies take 1d6 Arcane damage.",
            StaminaCost = 0,
            APCost = 30,
            Type = AbilityType.Utility,
            AttributeUsed = "will",
            BonusDice = 0,
            SuccessThreshold = 2,
            CurrentRank = 1,
            MaxRank = 3,
            CostToRank2 = 5,
            CostToRank3 = 10
        });

        // TIER 2 - Requires Progression Points to unlock

        // Rune of Shielding (Active) - 3 ranks
        character.Abilities.Add(new Ability
        {
            Name = "Rune of Shielding",
            Description = "Inscribe protective rune on ally. Target gains +2 Soak and resistance to Corruption for 3 turns.",
            StaminaCost = 0,
            APCost = 20,
            Type = AbilityType.Defense,
            AttributeUsed = "will",
            BonusDice = 0,
            SuccessThreshold = 2,
            CurrentRank = 1,
            MaxRank = 3,
            CostToRank2 = 5,
            CostToRank3 = 10
        });

        // Reinforce Ward (Active) - 3 ranks
        character.Abilities.Add(new Ability
        {
            Name = "Reinforce Ward",
            Description = "Target your Runic Barrier to heal it for 2d6 HP, OR boost Sanctified Ground to extend duration by 2 turns and increase healing to 2d6.",
            StaminaCost = 0,
            APCost = 15,
            Type = AbilityType.Utility,
            AttributeUsed = "will",
            BonusDice = 0,
            SuccessThreshold = 0,  // Auto-success
            CurrentRank = 1,
            MaxRank = 3,
            CostToRank2 = 5,
            CostToRank3 = 10
        });

        // Warden's Vigil (Passive) - Rank 1
        character.Abilities.Add(new Ability
        {
            Name = "Warden's Vigil",
            Description = "Passive: Allies in the same row as you gain +1d to Resolve Checks against Stress effects. Your presence is calming.",
            StaminaCost = 0,
            APCost = 0,
            Type = AbilityType.Utility,
            AttributeUsed = "",
            BonusDice = 0,
            SuccessThreshold = 0,
            CurrentRank = 1,
            MaxRank = 1  // Passive, no ranking
        });

        // TIER 3 - High-level abilities

        // Glyph of Sanctuary (Active) - 3 ranks
        character.Abilities.Add(new Ability
        {
            Name = "Glyph of Sanctuary",
            Description = "Party-wide protection: All allies gain 2d6 temporary HP and immunity to Stress for 2 turns. Emergency protection.",
            StaminaCost = 0,
            APCost = 40,
            Type = AbilityType.Defense,
            AttributeUsed = "will",
            BonusDice = 0,
            SuccessThreshold = 2,
            CurrentRank = 1,
            MaxRank = 3,
            CostToRank2 = 5,
            CostToRank3 = 10
        });

        // Aegis of Sanctity (Passive) - Rank 1
        character.Abilities.Add(new Ability
        {
            Name = "Aegis of Sanctity",
            Description = "Passive: Your Runic Barriers reflect 25% damage back to attackers. Your Sanctified Ground zones cleanse 1 debuff per turn.",
            StaminaCost = 0,
            APCost = 0,
            Type = AbilityType.Utility,
            AttributeUsed = "",
            BonusDice = 0,
            SuccessThreshold = 0,
            CurrentRank = 1,
            MaxRank = 1  // Passive, no ranking
        });

        // CAPSTONE - Ultimate defensive ability

        // Indomitable Bastion (Reaction) - Rank 1
        character.Abilities.Add(new Ability
        {
            Name = "Indomitable Bastion",
            Description = "CAPSTONE REACTION: When you or an ally would take fatal damage, negate it and create emergency Runic Barrier on their row (40 HP, 3 turns). Once per expedition.",
            StaminaCost = 0,
            APCost = 0,  // Free reaction, once per expedition
            Type = AbilityType.Defense,
            AttributeUsed = "",
            BonusDice = 0,
            SuccessThreshold = 0,  // Auto-success
            CurrentRank = 1,
            MaxRank = 1
        });
    }

    #endregion

    #region Rust-Witch Abilities (v0.19.8)

    private static void AddRustWitchAbilities(PlayerCharacter character)
    {
        // v0.19.8: Rust-Witch specialization - Heretical Debuffer
        // WARNING: ALL abilities inflict self-Corruption
        // TIER 1 - Available immediately upon unlocking specialization

        // Philosopher of Dust (Passive) - Rank 1
        character.Abilities.Add(new Ability
        {
            Name = "Philosopher of Dust",
            Description = "Passive: +1d to analysis checks against corrupted targets. You understand entropy intimately.",
            StaminaCost = 0,
            APCost = 0,
            Type = AbilityType.Utility,
            AttributeUsed = "",
            BonusDice = 0,
            SuccessThreshold = 0,
            CurrentRank = 1,
            MaxRank = 1  // Passive, no ranking
        });

        // Corrosive Curse (Active) - 3 ranks
        character.Abilities.Add(new Ability
        {
            Name = "Corrosive Curse",
            Description = "Apply 1 stack of [Corroded] to target (1d6 damage/turn, -2 Armor, 3 turns). COST: +2 Corruption to self.",
            StaminaCost = 0,
            APCost = 20,
            Type = AbilityType.Attack,
            AttributeUsed = "will",
            BonusDice = 0,
            SuccessThreshold = 2,
            DamageDice = 0,  // [Corroded] handles damage
            CurrentRank = 1,
            MaxRank = 3,
            CostToRank2 = 5,
            CostToRank3 = 10
        });

        // Entropic Field (Passive) - Rank 1
        character.Abilities.Add(new Ability
        {
            Name = "Entropic Field",
            Description = "Passive: Enemies in your row lose 1 Armor. Your presence accelerates decay.",
            StaminaCost = 0,
            APCost = 0,
            Type = AbilityType.Utility,
            AttributeUsed = "",
            BonusDice = 0,
            SuccessThreshold = 0,
            CurrentRank = 1,
            MaxRank = 1  // Passive, no ranking
        });

        // TIER 2 - Requires Progression Points to unlock

        // System Shock (Active) - 3 ranks
        character.Abilities.Add(new Ability
        {
            Name = "System Shock",
            Description = "Apply 2 stacks of [Corroded] to target. If target is [Mechanical], also apply [Stunned] (1 turn). COST: +3 Corruption to self.",
            StaminaCost = 0,
            APCost = 25,
            Type = AbilityType.Control,
            AttributeUsed = "will",
            BonusDice = 0,
            SuccessThreshold = 2,
            SkipEnemyTurn = false,  // [Stunned] handled separately
            CurrentRank = 1,
            MaxRank = 3,
            CostToRank2 = 5,
            CostToRank3 = 10
        });

        // Flash Rust (Active) - 3 ranks
        character.Abilities.Add(new Ability
        {
            Name = "Flash Rust",
            Description = "Apply 2 stacks of [Corroded] to ALL enemies. Instant entropy cascade. COST: +4 Corruption to self.",
            StaminaCost = 0,
            APCost = 35,
            Type = AbilityType.Attack,
            AttributeUsed = "will",
            BonusDice = 0,
            SuccessThreshold = 2,
            CurrentRank = 1,
            MaxRank = 3,
            CostToRank2 = 5,
            CostToRank3 = 10
        });

        // Accelerated Entropy (Passive) - Rank 1
        character.Abilities.Add(new Ability
        {
            Name = "Accelerated Entropy",
            Description = "Passive: [Corroded] damage increases to 2d6 per stack (from 1d6). Your curses are potent.",
            StaminaCost = 0,
            APCost = 0,
            Type = AbilityType.Utility,
            AttributeUsed = "",
            BonusDice = 0,
            SuccessThreshold = 0,
            CurrentRank = 1,
            MaxRank = 1  // Passive, no ranking
        });

        // TIER 3 - High-level abilities

        // Unmaking Word (Active) - 3 ranks
        character.Abilities.Add(new Ability
        {
            Name = "Unmaking Word",
            Description = "Speak the word of dissolution. DOUBLE current [Corroded] stacks on target (max 5 total). COST: +4 Corruption to self.",
            StaminaCost = 0,
            APCost = 30,
            Type = AbilityType.Attack,
            AttributeUsed = "will",
            BonusDice = 0,
            SuccessThreshold = 2,
            CurrentRank = 1,
            MaxRank = 3,
            CostToRank2 = 5,
            CostToRank3 = 10
        });

        // Cascade Reaction (Passive) - Rank 1
        character.Abilities.Add(new Ability
        {
            Name = "Cascade Reaction",
            Description = "Passive: When an enemy with [Corroded] dies, spread 1 stack of [Corroded] to all adjacent enemies. Entropy is contagious.",
            StaminaCost = 0,
            APCost = 0,
            Type = AbilityType.Utility,
            AttributeUsed = "",
            BonusDice = 0,
            SuccessThreshold = 0,
            CurrentRank = 1,
            MaxRank = 1  // Passive, no ranking
        });

        // CAPSTONE - Ultimate execution ability

        // Entropic Cascade (Active) - Rank 1
        character.Abilities.Add(new Ability
        {
            Name = "Entropic Cascade",
            Description = "CAPSTONE: If target has >50% Corruption or 5 [Corroded] stacks, instantly reduce to 0 HP. Otherwise deal 6d6 Arcane damage. COST: +6 Corruption to self.",
            StaminaCost = 0,
            APCost = 50,
            Type = AbilityType.Attack,
            AttributeUsed = "will",
            BonusDice = 0,
            SuccessThreshold = 2,
            DamageDice = 6,  // 6d6 if not execute
            CurrentRank = 1,
            MaxRank = 1
        });
    }

    #endregion

    #region Rúnasmiðr Abilities (v0.19.10)

    private static void AddRunasmidrAbilities(PlayerCharacter character)
    {
        // v0.19.10: Rúnasmiðr specialization - Arcane Artisan (Runeforging + Battlefield Control)
        // Grant starting Runeforging components
        GrantRunasmidrStartingItems(character);

        // TIER 1 - Available immediately upon unlocking specialization (3 PP each)

        // Master Carver I (Passive) - 3 ranks
        character.Abilities.Add(new Ability
        {
            Name = "Master Carver I",
            Description = "Passive: +1d10 to all Runeforging checks. Your runic inscriptions are precise and potent.",
            StaminaCost = 0,
            APCost = 0,
            Type = AbilityType.Utility,
            AttributeUsed = "",
            BonusDice = 0,
            SuccessThreshold = 0,
            CurrentRank = 1,
            MaxRank = 3,
            CostToRank2 = 20,  // Rank 2: +2d10
            CostToRank3 = 40   // Rank 3: +3d10 + Masterwork access
        });

        // Hagalaz Trap (Active) - 3 ranks
        character.Abilities.Add(new Ability
        {
            Name = "Hagalaz Trap",
            Description = "Place invisible ice trap on target tile. When enemy enters: 2d6 Ice damage (AoE row). Lasts 3 turns.",
            StaminaCost = 0,
            APCost = 25,
            Type = AbilityType.Attack,
            AttributeUsed = "",
            BonusDice = 0,
            SuccessThreshold = 0,  // Auto-success placement
            DamageDice = 2,  // 2d6 base
            CurrentRank = 1,
            MaxRank = 3,
            CostToRank2 = 20,  // Rank 2: 3d6 damage
            CostToRank3 = 40   // Rank 3: 4d6 + [Chilled] status
        });

        // Algiz Imbuement (Crafting Ability) - 3 ranks
        character.Abilities.Add(new Ability
        {
            Name = "Algiz Imbuement",
            Description = "Runeforging recipe: Imbue armor with protective wards. Grants 2 [Warding Rune] charges (React: +5 Soak vs one attack).",
            StaminaCost = 0,
            APCost = 0,
            Type = AbilityType.Utility,
            AttributeUsed = "",
            BonusDice = 0,
            SuccessThreshold = 0,
            CurrentRank = 1,
            MaxRank = 3,
            CostToRank2 = 20,  // Rank 2: 3 charges
            CostToRank3 = 40   // Rank 3: 4 charges, +7 Soak
        });

        // TIER 2 - Requires Progression Points to unlock (4 PP each, requires 8 PP in tree)

        // Debug Enemy (Active) - 3 ranks
        character.Abilities.Add(new Ability
        {
            Name = "Debug Enemy",
            Description = "Analyze enemy's code structure. Reveal HP, defenses, resistances, vulnerabilities. COST: +3 Psychic Stress.",
            StaminaCost = 0,
            APCost = 30,
            Type = AbilityType.Utility,
            AttributeUsed = "wits",
            BonusDice = 0,
            SuccessThreshold = 2,  // WITS + WILL check
            CurrentRank = 1,
            MaxRank = 3,
            CostToRank2 = 20,  // Rank 2: Also reveals AI patterns
            CostToRank3 = 40   // Rank 3: No Stress cost, reveals hidden abilities
        });

        // Rune of Disruption (Active) - 3 ranks
        character.Abilities.Add(new Ability
        {
            Name = "Rune of Disruption",
            Description = "Place trap that applies [Disoriented] (-2 to all checks) for 2 turns. WILL save DC 14. Lasts 3 turns.",
            StaminaCost = 0,
            APCost = 35,
            Type = AbilityType.Control,
            AttributeUsed = "",
            BonusDice = 0,
            SuccessThreshold = 0,
            CurrentRank = 1,
            MaxRank = 3,
            CostToRank2 = 20,  // Rank 2: DC 16, status lasts 3 turns
            CostToRank3 = 40   // Rank 3: DC 18, also deals 2d6 Psychic damage
        });

        // Uruz Imbuement (Crafting Ability) - 3 ranks
        character.Abilities.Add(new Ability
        {
            Name = "Uruz Imbuement",
            Description = "Runeforging recipe: Imbue weapon with primal strength. Grants 3 [Bull's Strength] charges (Free Action: +2d10 damage to next attack).",
            StaminaCost = 0,
            APCost = 0,
            Type = AbilityType.Utility,
            AttributeUsed = "",
            BonusDice = 0,
            SuccessThreshold = 0,
            CurrentRank = 1,
            MaxRank = 3,
            CostToRank2 = 20,  // Rank 2: 4 charges
            CostToRank3 = 40   // Rank 3: 5 charges, +3d10 damage
        });

        // TIER 3 - High-level abilities (5 PP each, requires 16 PP in tree)

        // Runic Synergy (Passive) - 3 ranks
        character.Abilities.Add(new Ability
        {
            Name = "Runic Synergy",
            Description = "Passive: When enemy triggers your trap, restore 10 Stamina to yourself. Your traps feed you energy.",
            StaminaCost = 0,
            APCost = 0,
            Type = AbilityType.Utility,
            AttributeUsed = "",
            BonusDice = 0,
            SuccessThreshold = 0,
            CurrentRank = 1,
            MaxRank = 3,
            CostToRank2 = 20,  // Rank 2: Restore 15 Stamina + 5 AP
            CostToRank3 = 40   // Rank 3: Restore 20 Stamina + 10 AP + remove 2 Stress
        });

        // Rune of Isolation (Active) - 3 ranks
        character.Abilities.Add(new Ability
        {
            Name = "Rune of Isolation",
            Description = "Place trap that applies [Rooted] (cannot move) for 2 turns. Target cannot receive external buffs/healing. Lasts 3 turns.",
            StaminaCost = 0,
            APCost = 50,
            Type = AbilityType.Control,
            AttributeUsed = "",
            BonusDice = 0,
            SuccessThreshold = 0,
            CurrentRank = 1,
            MaxRank = 3,
            CostToRank2 = 20,  // Rank 2: Status lasts 3 turns
            CostToRank3 = 40   // Rank 3: Also deals 3d6 Arcane damage
        });

        // CAPSTONE - Ultimate crafting and support ability (6 PP, requires 24 PP + one Tier 3)

        // Architect of Stability (Passive + Active) - Rank 1
        character.Abilities.Add(new Ability
        {
            Name = "Architect of Stability",
            Description = "CAPSTONE: Passive: Masterwork crafting unlocked (critical success: 2x charges + Saga Property). Active (75 AP, once per combat): Create 3x3 [Sanctuary of Order] zone for 3 turns (negates Wild Magic, blocks Reality Glitches, nullifies environmental Stress).",
            StaminaCost = 0,
            APCost = 75,  // For active component
            Type = AbilityType.Utility,
            AttributeUsed = "",
            BonusDice = 0,
            SuccessThreshold = 0,
            CurrentRank = 1,
            MaxRank = 1  // Capstone, no ranking
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

    /// <summary>
    /// Grant Rúnasmiðr starting items (runeforging components)
    /// </summary>
    private static void GrantRunasmidrStartingItems(PlayerCharacter character)
    {
        // Add starting Runeforging components
        character.CraftingComponents[ComponentType.AetherDust] = 10;  // Common fuel
        character.CraftingComponents[ComponentType.AlgizTablet] = 3;  // Defensive enchantments
        character.CraftingComponents[ComponentType.UruzStone] = 3;    // Offensive enchantments
        character.CraftingComponents[ComponentType.HagalazCrystal] = 2;  // Ice trap runes
    }

    #endregion
}
