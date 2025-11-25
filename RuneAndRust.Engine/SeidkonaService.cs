using RuneAndRust.Core;
using Serilog;
using System.Collections.Generic;

namespace RuneAndRust.Engine;

/// <summary>
/// v0.28.1: Service for Seidkona specialization-specific abilities and mechanics.
/// Handles Echo of Vigor, Echo of Misfortune, Forlorn Communion, Spirit Ward, Ride the Echoes, and Moment of Clarity.
/// The psychic archaeologist who communes with fragmented echoes of crashed reality.
/// </summary>
public class SeidkonaService
{
    private static readonly ILogger _log = Log.ForContext<SeidkonaService>();
    private readonly SpiritBargainService _bargainService;
    private readonly TraumaEconomyService _traumaService;
    private readonly DiceService _diceService;
    private readonly string _connectionString;

    public SeidkonaService(string connectionString)
    {
        _connectionString = connectionString;
        _bargainService = new SpiritBargainService(connectionString);
        _traumaService = new TraumaEconomyService();
        _diceService = new DiceService();
        _log.Debug("SeidkonaService initialized");
    }

    #region Tier 1 Abilities

    /// <summary>
    /// Execute Echo of Vigor (Tier 1)
    /// Healing spell with Spirit Bargain chance to cleanse physical debuffs
    /// </summary>
    public (int healingAmount, bool debuffCleansed, string debuffCleansedName, string message) ExecuteEchoOfVigor(
        PlayerCharacter caster,
        PlayerCharacter target,
        int rank = 1)
    {
        _log.Information("Executing Echo of Vigor: CasterID={CasterId}, TargetID={TargetId}, Rank={Rank}",
            caster.CharacterID, target.CharacterID, rank);

        // Calculate healing based on rank
        var (diceCount, baseChance, canTargetSelf) = rank switch
        {
            1 => (3, 0.25f, false),  // 3d8, 25% bargain
            2 => (4, 0.30f, false),  // 4d8, 30% bargain
            3 => (5, 0.35f, true),   // 5d8, 35% bargain, can self-cast
            _ => (3, 0.25f, false)
        };

        // Check if target is valid (Rank 3 can target self)
        if (target.CharacterID == caster.CharacterID && !canTargetSelf)
        {
            _log.Warning("Echo of Vigor cannot target self at Rank {Rank}", rank);
            return (0, false, "", "Echo of Vigor cannot target self at this rank!");
        }

        // Roll healing: XdY HP
        int healing = 0;
        for (int i = 0; i < diceCount; i++)
        {
            healing += _diceService.RollD8();
        }

        // Apply healing
        int oldHP = target.CurrentHP;
        target.CurrentHP = Math.Min(target.CurrentHP + healing, target.MaxHP);
        int actualHealing = target.CurrentHP - oldHP;

        // Attempt Spirit Bargain
        var bargainResult = _bargainService.AttemptSpiritBargain(caster.CharacterID, "Echo of Vigor", baseChance);

        bool debuffCleansed = false;
        string debuffCleansedName = "";

        if (bargainResult.Success)
        {
            // Attempt to cleanse one minor physical debuff
            var physicalDebuffs = new[] { "Bleeding", "Poisoned", "Disease" };

            // In a real implementation, check target's status effects
            // For now, simulate cleansing the first present debuff
            foreach (var debuff in physicalDebuffs)
            {
                // Placeholder: In real implementation, check if target has this debuff
                bool hasDebuff = false; // Replace with actual check

                if (hasDebuff)
                {
                    debuffCleansed = true;
                    debuffCleansedName = debuff;
                    _log.Information("[Spirit Bargain] Echo of Vigor cleansed [{Debuff}] from {Target}",
                        debuff, target.Name);
                    break;
                }
            }
        }

        _log.Information("Echo of Vigor complete: CasterID={CasterId}, Healing={Healing}, Bargain={Bargain}, DebuffCleansed={Cleansed}",
            caster.CharacterID, actualHealing, bargainResult.Success, debuffCleansed);

        string message = $"Echo of Vigor restores {actualHealing} HP to {target.Name}!";

        if (bargainResult.Success && debuffCleansed)
        {
            message += $" [Spirit Bargain] Also cleanses [{debuffCleansedName}]!";
        }
        else if (bargainResult.Success && !debuffCleansed)
        {
            message += " [Spirit Bargain] Triggered but no debuffs to cleanse.";
        }

        return (actualHealing, debuffCleansed, debuffCleansedName, message);
    }

    /// <summary>
    /// Execute Echo of Misfortune (Tier 1)
    /// Curse enemy with Spirit Bargain chance to spread curse on critical success
    /// </summary>
    public (bool cursed, int duration, bool curseSpread, string message) ExecuteEchoOfMisfortune(
        PlayerCharacter caster,
        Enemy target,
        int rank = 1)
    {
        _log.Information("Executing Echo of Misfortune: CasterID={CasterId}, TargetID={TargetId}, Rank={Rank}",
            caster.CharacterID, target.EnemyID, rank);

        // Calculate curse effects based on rank
        var (duration, missChance, dicePenalty, baseChance) = rank switch
        {
            1 => (2, 0.25f, 1, 0.25f),  // 2 turns, 25% miss, -1 die, Spirit Bargain on crit
            2 => (3, 0.30f, 1, 0.30f),  // 3 turns, 30% miss, -1 die
            3 => (3, 0.30f, 2, 0.35f),  // 3 turns, 30% miss, -2 dice
            _ => (2, 0.25f, 1, 0.25f)
        };

        // Apply [Cursed] status effect
        // In real implementation, apply to target.StatusEffects
        bool cursed = true;

        // Attempt Spirit Bargain
        var bargainResult = _bargainService.AttemptSpiritBargain(caster.CharacterID, "Echo of Misfortune", baseChance);

        bool curseSpread = false;

        if (bargainResult.Success)
        {
            // On critical success (for simplicity, treat all bargain successes as potential spreads)
            // In real implementation, check for adjacent enemies
            curseSpread = true;
            _log.Information("[Spirit Bargain] Echo of Misfortune spreads to adjacent enemy!");
        }

        _log.Information("Echo of Misfortune complete: CasterID={CasterId}, Target={Target}, Duration={Duration}, Spread={Spread}",
            caster.CharacterID, target.Name, duration, curseSpread);

        string message = $"Echo of Misfortune applies [Cursed] to {target.Name} for {duration} turns " +
                        $"({(int)(missChance * 100)}% miss chance, -{dicePenalty} dice penalty)!";

        if (curseSpread)
        {
            message += " [Spirit Bargain] Curse spreads to adjacent enemy for 1 turn!";
        }

        return (cursed, duration, curseSpread, message);
    }

    #endregion

    #region Tier 2 Abilities

    /// <summary>
    /// Execute Forlorn Communion (Tier 2)
    /// WITS check to gain forbidden knowledge from Forlorn entities, with unavoidable Psychic Stress cost
    /// </summary>
    public (bool success, int margin, int stressCost, int questionsAsked, string knowledge, string message) ExecuteForlornCommunion(
        PlayerCharacter character,
        int forlornEntityId,
        int rank = 1)
    {
        _log.Information("Executing Forlorn Communion: CharacterID={CharacterId}, ForlornID={ForlornId}, Rank={Rank}",
            character.CharacterID, forlornEntityId, rank);

        // Check if in Moment of Clarity (auto-success with reduced cost)
        bool inClarity = IsInMomentOfClarity(character.CharacterID);

        // Calculate DC and stress cost based on rank
        var (dcReduction, stressBase, questionsAllowed) = rank switch
        {
            1 => (0, 15, 1),   // DC 12, +15 Stress, 1 question
            2 => (2, 12, 1),   // DC 10, +12 Stress, 1 question
            3 => (4, 10, 2),   // DC 8, +10 Stress, 2 questions
            _ => (0, 15, 1)
        };

        int finalDC = 12 - dcReduction;
        int stressCost = inClarity ? 7 : stressBase;

        bool success;
        int margin;

        if (inClarity)
        {
            // Auto-success during Moment of Clarity
            success = true;
            margin = 10; // Maximum information quality
            _log.Information("[Moment of Clarity] Forlorn Communion auto-succeeds with maximum information");
        }
        else
        {
            // Perform WITS check
            var checkResult = _diceService.Roll(character.WITS);
            success = checkResult.Successes >= finalDC;
            margin = checkResult.Successes - finalDC;

            _log.Information("Forlorn Communion WITS check: CharacterID={CharacterId}, DC={DC}, Successes={Successes}, Success={Success}",
                character.CharacterID, finalDC, checkResult.Successes, success);
        }

        // Generate knowledge based on success margin
        string knowledge = GenerateForlornKnowledge(forlornEntityId, margin, success);

        // Apply unavoidable Psychic Stress cost
        var (stressGained, traumaAcquired) = _traumaService.AddStress(
            character,
            stressCost,
            source: "Forlorn Communion (channeling corrupted consciousness)",
            allowResolveCheck: false  // Stress cannot be resisted
        );

        _log.Information("Forlorn Communion complete: CharacterID={CharacterId}, Success={Success}, Margin={Margin}, Stress={Stress}, Questions={Questions}",
            character.CharacterID, success, margin, stressGained, questionsAllowed);

        string message = success
            ? $"Forlorn Communion succeeds! You glimpse {margin} fragments of truth... (+{stressGained} Psychic Stress, unavoidable)"
            : $"Forlorn Communion fails! Only psychic static and screams... (+{stressGained} Psychic Stress, unavoidable)";

        if (traumaAcquired != null)
        {
            message += $" [BREAKING POINT REACHED - Trauma acquired: {traumaAcquired.Name}]";
        }

        return (success, margin, stressGained, questionsAllowed, knowledge, message);
    }

    /// <summary>
    /// Execute Spiritual Anchor (Tier 2)
    /// Meditate to remove Psychic Stress from self (or ally at Rank 3 during Clarity)
    /// </summary>
    public (int stressRemoved, bool debuffCleansed, string debuffCleansedName, string message) ExecuteSpiritualAnchor(
        PlayerCharacter character,
        int rank = 1,
        PlayerCharacter? targetAlly = null)
    {
        _log.Information("Executing Spiritual Anchor: CharacterID={CharacterId}, Rank={Rank}",
            character.CharacterID, rank);

        // Determine target (self, or ally during Clarity at Rank 3)
        PlayerCharacter target = character;
        bool inClarity = IsInMomentOfClarity(character.CharacterID);

        if (targetAlly != null && inClarity && rank >= 3)
        {
            target = targetAlly;
            _log.Information("[Moment of Clarity] Spiritual Anchor targets ally: {AllyName}", target.Name);
        }

        // Calculate stress removal based on rank
        var (stressRemoval, cleanseMentalDebuff) = rank switch
        {
            1 => (20, false),
            2 => (25, false),
            3 => (30, true),
            _ => (20, false)
        };

        // Remove Psychic Stress
        int oldStress = target.PsychicStress;
        target.PsychicStress = Math.Max(0, target.PsychicStress - stressRemoval);
        int actualRemoval = oldStress - target.PsychicStress;

        bool debuffCleansed = false;
        string debuffCleansedName = "";

        if (cleanseMentalDebuff)
        {
            // Attempt to cleanse one mental debuff
            var mentalDebuffs = new[] { "Fear", "Disoriented", "Charmed" };

            foreach (var debuff in mentalDebuffs)
            {
                // Placeholder: In real implementation, check if target has this debuff
                bool hasDebuff = false; // Replace with actual check

                if (hasDebuff)
                {
                    debuffCleansed = true;
                    debuffCleansedName = debuff;
                    _log.Information("Spiritual Anchor cleansed [{Debuff}] from {Target}",
                        debuff, target.Name);
                    break;
                }
            }
        }

        _log.Information("Spiritual Anchor complete: CharacterID={CharacterId}, StressRemoved={Stress}, DebuffCleansed={Cleansed}",
            character.CharacterID, actualRemoval, debuffCleansed);

        string message = $"Spiritual Anchor removes {actualRemoval} Psychic Stress from {target.Name}.";

        if (debuffCleansed)
        {
            message += $" Also cleanses [{debuffCleansedName}]!";
        }

        return (actualRemoval, debuffCleansed, debuffCleansedName, message);
    }

    #endregion

    #region Tier 3 Abilities

    /// <summary>
    /// Execute Spirit Ward (Tier 3)
    /// Place protective ward on row negating environmental Psychic Stress
    /// </summary>
    public (bool success, int duration, bool dualRow, string message) ExecuteSpiritWard(
        PlayerCharacter caster,
        int targetRow,
        List<PlayerCharacter> rowAllies,
        int rank = 1)
    {
        _log.Information("Executing Spirit Ward: CasterID={CasterId}, TargetRow={Row}, Rank={Rank}",
            caster.CharacterID, targetRow, rank);

        // Calculate ward effects based on rank
        var (baseDuration, diceBonus, baseChance, canPlaceDual) = rank switch
        {
            1 => (3, 1, 0.25f, false),  // 3 turns, +1 die, 25% bargain
            2 => (3, 2, 0.30f, false),  // 3 turns, +2 dice, 30% bargain
            3 => (3, 2, 0.35f, true),   // 3 turns, +2 dice, 35% bargain, can dual-row
            _ => (3, 1, 0.25f, false)
        };

        int finalDuration = baseDuration;
        bool dualRow = false;

        // Attempt Spirit Bargain for extended duration
        var bargainResult = _bargainService.AttemptSpiritBargain(caster.CharacterID, "Spirit Ward", baseChance);

        if (bargainResult.Success)
        {
            finalDuration = 4; // Extended to 4 turns
            _log.Information("[Spirit Bargain] Spirit Ward extended to 4 turns!");
        }

        _log.Information("Spirit Ward placed: CasterID={CasterId}, Row={Row}, Duration={Duration}, Allies={AllyCount}, DiceBonus={Dice}",
            caster.CharacterID, targetRow, finalDuration, rowAllies.Count, diceBonus);

        string message = $"Spirit Ward placed on Row {targetRow} for {finalDuration} turns! " +
                        $"{rowAllies.Count} allies protected: Negate environmental Psychic Stress, " +
                        $"+{diceBonus} dice vs psychic attacks.";

        if (bargainResult.Success)
        {
            message += " [Spirit Bargain] Duration extended to 4 turns!";
        }

        return (true, finalDuration, dualRow, message);
    }

    /// <summary>
    /// Execute Ride the Echoes (Tier 3)
    /// Instant teleportation with Runic Blight Corruption cost
    /// </summary>
    public (bool success, int corruptionGained, bool allyBrought, string message) ExecuteRideTheEchoes(
        PlayerCharacter caster,
        int targetX,
        int targetY,
        int rank = 1,
        PlayerCharacter? adjacentAlly = null)
    {
        _log.Information("Executing Ride the Echoes: CasterID={CasterId}, Target=({X},{Y}), Rank={Rank}",
            caster.CharacterID, targetX, targetY, rank);

        // Calculate corruption cost based on rank
        var (corruptionCost, canBringAlly) = rank switch
        {
            1 => (2, false),  // +2 Corruption
            2 => (1, false),  // +1 Corruption (reduced)
            3 => (1, true),   // +1 Corruption, can bring ally (+1 for ally)
            _ => (2, false)
        };

        bool allyBrought = false;
        int totalCorruption = corruptionCost;

        // Check if bringing an ally
        if (adjacentAlly != null && canBringAlly)
        {
            allyBrought = true;
            totalCorruption += 1; // Ally gains +1 Corruption

            var allyCorruptionResult = _traumaService.AddCorruption(
                adjacentAlly,
                1,
                source: "Ride the Echoes (forced through Blight-space)"
            );

            _log.Information("Ally brought through Blight-space: AllyID={AllyId}, Corruption={Corruption}",
                adjacentAlly.CharacterID, allyCorruptionResult.corruptionGained);
        }

        // Apply Corruption to caster
        var corruptionResult = _traumaService.AddCorruption(
            caster,
            corruptionCost,
            source: "Ride the Echoes (interfacing with Blight nature)"
        );

        _log.Information("Ride the Echoes complete: CasterID={CasterId}, Corruption={Corruption}, AllyBrought={Ally}",
            caster.CharacterID, corruptionResult.corruptionGained, allyBrought);

        string message = $"Ride the Echoes teleports {caster.Name} to ({targetX},{targetY})! " +
                        $"(+{corruptionResult.corruptionGained} Runic Blight Corruption)";

        if (allyBrought)
        {
            message += $" {adjacentAlly!.Name} brought along (+1 Corruption to ally).";
        }

        return (true, corruptionResult.corruptionGained, allyBrought, message);
    }

    #endregion

    #region Capstone: Moment of Clarity

    /// <summary>
    /// Execute Moment of Clarity (Capstone)
    /// Enter state of perfect spirit control with guaranteed Spirit Bargains
    /// </summary>
    public (bool success, int duration, int aftermathStress, string message) ExecuteMomentOfClarity(
        PlayerCharacter caster,
        int rank = 1)
    {
        _log.Information("Attempting to execute Moment of Clarity: CasterID={CasterId}, Rank={Rank}",
            caster.CharacterID, rank);

        // Calculate duration and aftermath stress based on rank
        var (duration, aftermathStress, maxUses) = rank switch
        {
            1 => (2, 20, 1),  // 2 turns, +20 Stress aftermath, 1 use per combat
            2 => (3, 15, 1),  // 3 turns, +15 Stress aftermath, 1 use per combat
            3 => (3, 10, 2),  // 3 turns, +10 Stress aftermath, 2 uses per combat
            _ => (2, 20, 1)
        };

        // Attempt to activate Moment of Clarity
        bool activated = _bargainService.ActivateMomentOfClarity(caster.CharacterID, duration, rank);

        if (!activated)
        {
            return (false, 0, 0, "Moment of Clarity already used maximum times this combat!");
        }

        _log.Information("MOMENT OF CLARITY ACTIVATED: CasterID={CasterId}, Duration={Duration} turns, AftermathStress={Stress}, Rank={Rank}",
            caster.CharacterID, duration, aftermathStress, rank);

        string message = $"Moment of Clarity activated! For {duration} turns: " +
                        $"ALL Spirit Bargains guaranteed (100%), " +
                        $"Forlorn Communion costs 0 Aether + only +7 Stress and auto-succeeds, " +
                        $"Spiritual Anchor can target allies. " +
                        $"[Aftermath: +{aftermathStress} Psychic Stress when Clarity ends]";

        return (true, duration, aftermathStress, message);
    }

    /// <summary>
    /// Apply Moment of Clarity aftermath (Psychic Stress cost when Clarity ends)
    /// </summary>
    public (int stressGained, string message) ApplyClarityAftermath(PlayerCharacter character, int rank)
    {
        int aftermathStress = rank switch
        {
            1 => 20,
            2 => 15,
            3 => 10,
            _ => 20
        };

        var (stressGained, traumaAcquired) = _traumaService.AddStress(
            character,
            aftermathStress,
            source: "Moment of Clarity aftermath (psychic backlash from forced communion)",
            allowResolveCheck: false
        );

        _bargainService.EndMomentOfClarity(character.CharacterID);

        _log.Information("Moment of Clarity ended: CharacterID={CharacterId}, AftermathStress={Stress}",
            character.CharacterID, stressGained);

        string message = $"Moment of Clarity ends. Psychic backlash: +{stressGained} Psychic Stress.";

        if (traumaAcquired != null)
        {
            message += $" [BREAKING POINT - Trauma acquired: {traumaAcquired.Name}]";
        }

        return (stressGained, message);
    }

    #endregion

    #region Helper Methods

    private bool IsInMomentOfClarity(int characterId)
    {
        // Query Spirit Bargain service for Clarity status
        // For now, simplified implementation
        return false; // Placeholder - integrate with SpiritBargainService
    }

    private string GenerateForlornKnowledge(int forlornId, int margin, bool success)
    {
        if (!success) return "Only psychic static and screaming madness...";

        // Generate knowledge quality based on margin
        return margin switch
        {
            >= 8 => "CRITICAL KNOWLEDGE: Exact location of hidden loot, enemy weakness revealed, puzzle solution discovered",
            >= 5 => "STRONG KNOWLEDGE: General direction to objective, enemy resistances partially revealed",
            >= 2 => "BASIC KNOWLEDGE: Vague hints about nearby threats or resources",
            _ => "MINIMAL KNOWLEDGE: Fragmentary whispers of uncertain value"
        };
    }

    #endregion
}
