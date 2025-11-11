using RuneAndRust.Core;
using Serilog;

namespace RuneAndRust.Engine;

/// <summary>
/// Aethelgard Saga System - Manages Legend, Milestones, and Progression Points
/// </summary>
public class SagaService
{
    private static readonly ILogger _log = Log.ForContext<SagaService>();
    private const int MaxMilestone = 3; // For v0.1
    private const int AttributeCap = 6;

    /// <summary>
    /// Award Legend based on the Aethelgard formula: BLV × DM × TM
    /// </summary>
    public void AwardLegend(PlayerCharacter player, int baseLegendValue, float difficultyMod = 1.0f, float traumaMod = 1.0f)
    {
        if (player.CurrentMilestone >= MaxMilestone)
        {
            _log.Debug("Legend award skipped: Character={CharacterName}, Reason=MaxMilestoneReached, CurrentMilestone={Milestone}",
                player.Name, player.CurrentMilestone);
            return; // Already at max milestone for v0.1
        }

        int oldLegend = player.CurrentLegend;
        int legendAwarded = (int)(baseLegendValue * difficultyMod * traumaMod);
        player.CurrentLegend += legendAwarded;

        _log.Information("Legend awarded: Character={CharacterName}, Amount={Amount}, BaseValue={Base}, DifficultyMod={DM}, TraumaMod={TM}, OldLegend={Old}, NewLegend={New}, ToNextMilestone={ToNext}",
            player.Name, legendAwarded, baseLegendValue, difficultyMod, traumaMod, oldLegend, player.CurrentLegend, player.LegendToNextMilestone);
    }

    /// <summary>
    /// Check if player can reach the next milestone
    /// </summary>
    public bool CanReachMilestone(PlayerCharacter player)
    {
        if (player.CurrentMilestone >= MaxMilestone)
        {
            return false;
        }

        return player.CurrentLegend >= player.LegendToNextMilestone;
    }

    /// <summary>
    /// Advance player to next milestone and grant rewards
    /// </summary>
    public void ReachMilestone(PlayerCharacter player)
    {
        if (!CanReachMilestone(player))
        {
            throw new InvalidOperationException("Player cannot reach milestone yet.");
        }

        int oldMilestone = player.CurrentMilestone;
        int oldMaxHP = player.MaxHP;
        int oldMaxStamina = player.MaxStamina;
        int oldPP = player.ProgressionPoints;

        // Increase milestone
        player.CurrentMilestone++;

        // Update Legend to next milestone
        player.LegendToNextMilestone = CalculateLegendToNextMilestone(player.CurrentMilestone);

        // Grant milestone rewards
        player.ProgressionPoints += 1;
        player.MaxHP += 10;
        player.MaxStamina += 5;

        // Full heal on milestone
        player.HP = player.MaxHP;
        player.Stamina = player.MaxStamina;

        _log.Information("Milestone reached: Character={CharacterName}, OldMilestone={OldMilestone}, NewMilestone={NewMilestone}, PPGained={PPGained}, TotalPP={TotalPP}, MaxHPIncrease={HPIncrease}, NewMaxHP={MaxHP}, MaxStaminaIncrease={StaminaIncrease}, NewMaxStamina={MaxStamina}, LegendToNext={LegendToNext}",
            player.Name, oldMilestone, player.CurrentMilestone, 1, player.ProgressionPoints, 10, player.MaxHP, 5, player.MaxStamina, player.LegendToNextMilestone);
    }

    /// <summary>
    /// Calculate Legend required to reach next milestone
    /// Adjusted formula for v0.1: (CurrentMilestone × 50) + 100
    /// </summary>
    public int CalculateLegendToNextMilestone(int currentMilestone)
    {
        if (currentMilestone >= MaxMilestone)
        {
            return 0; // At max milestone
        }

        // Adjusted formula for v0.1's short playtime
        return (currentMilestone * 50) + 100;
    }

    /// <summary>
    /// Spend PP to increase an attribute
    /// </summary>
    public bool SpendPPOnAttribute(PlayerCharacter player, string attributeName)
    {
        if (player.ProgressionPoints < 1)
        {
            _log.Warning("PP spend failed: Character={CharacterName}, Attribute={Attribute}, Reason=InsufficientPP, CurrentPP={PP}",
                player.Name, attributeName, player.ProgressionPoints);
            return false; // Not enough PP
        }

        // Get current attribute value
        int currentValue = player.GetAttributeValue(attributeName);
        if (currentValue >= AttributeCap)
        {
            _log.Warning("PP spend failed: Character={CharacterName}, Attribute={Attribute}, Reason=AttributeAtCap, CurrentValue={Value}, Cap={Cap}",
                player.Name, attributeName, currentValue, AttributeCap);
            return false; // Already at cap
        }

        // Spend PP and increase attribute
        int oldPP = player.ProgressionPoints;
        player.ProgressionPoints -= 1;

        switch (attributeName.ToLower())
        {
            case "might":
                player.Attributes.Might++;
                break;
            case "finesse":
                player.Attributes.Finesse++;
                break;
            case "wits":
                player.Attributes.Wits++;
                break;
            case "will":
                player.Attributes.Will++;
                break;
            case "sturdiness":
                player.Attributes.Sturdiness++;
                break;
            default:
                // Refund PP if invalid attribute
                player.ProgressionPoints += 1;
                _log.Error("Invalid attribute name: Character={CharacterName}, Attribute={Attribute}",
                    player.Name, attributeName);
                throw new ArgumentException($"Invalid attribute name: {attributeName}");
        }

        int newValue = player.GetAttributeValue(attributeName);
        _log.Information("PP spent on attribute: Character={CharacterName}, Attribute={Attribute}, PPSpent={Cost}, OldValue={OldValue}, NewValue={NewValue}, RemainingPP={PP}",
            player.Name, attributeName, 1, currentValue, newValue, player.ProgressionPoints);

        return true;
    }

    /// <summary>
    /// Advance an ability to the next rank
    /// </summary>
    public bool AdvanceAbilityRank(PlayerCharacter player, Ability ability)
    {
        if (ability.CurrentRank >= 2)
        {
            _log.Warning("Ability rank advance failed: Character={CharacterName}, Ability={Ability}, Reason=AlreadyAtMaxRank, CurrentRank={Rank}",
                player.Name, ability.Name, ability.CurrentRank);
            return false; // Rank 3 locked until v0.5+
        }

        if (player.ProgressionPoints < ability.CostToRank2)
        {
            _log.Warning("Ability rank advance failed: Character={CharacterName}, Ability={Ability}, Reason=InsufficientPP, RequiredPP={Required}, CurrentPP={Current}",
                player.Name, ability.Name, ability.CostToRank2, player.ProgressionPoints);
            return false; // Not enough PP
        }

        int oldRank = ability.CurrentRank;
        int ppCost = ability.CostToRank2;

        // Spend PP and advance rank
        player.ProgressionPoints -= ability.CostToRank2;
        ability.CurrentRank++;

        // Apply rank 2 improvements (will be customized per ability)
        ApplyRank2Improvements(ability);

        _log.Information("Ability rank advanced: Character={CharacterName}, Ability={Ability}, PPSpent={Cost}, OldRank={OldRank}, NewRank={NewRank}, RemainingPP={PP}",
            player.Name, ability.Name, ppCost, oldRank, ability.CurrentRank, player.ProgressionPoints);

        return true;
    }

    /// <summary>
    /// Apply Rank 2 improvements to an ability
    /// </summary>
    private void ApplyRank2Improvements(Ability ability)
    {
        switch (ability.Name)
        {
            // Warrior abilities
            case "Power Strike":
                ability.BonusDice = 3; // Was +2, now +3
                ability.SuccessThreshold = 2; // Was 2, now easier to trigger double damage
                ability.StaminaCost = 4; // Was 5, now cheaper
                break;

            case "Shield Wall":
                ability.BonusDice = 2; // Was +1, now +2
                ability.DefensePercent = 75; // Was 50%, now 75%
                ability.DefenseDuration = 3; // Was 2, now 3
                break;

            // Scavenger abilities
            case "Quick Dodge":
                ability.BonusDice = 2; // Was +1, now +2
                ability.StaminaCost = 2; // Was 3, now cheaper
                break;

            case "Precision Strike":
                ability.BonusDice = 2; // Improved dice
                ability.SuccessThreshold = 2; // Easier to trigger bleeding
                break;

            // Mystic abilities
            case "Aetheric Bolt":
                ability.BonusDice = 3; // Was +2, now +3
                ability.DamageDice = 2; // Was 1d6, now 2d6
                break;

            case "Aetheric Shield":
                ability.BonusDice = 2; // Was +1, now +2
                // Shield absorption is handled in combat logic
                break;

            // Level 3 abilities (if unlocked)
            case "Cleaving Strike":
                ability.BonusDice = 3;
                ability.DamageDice = 2; // Improved damage
                break;

            case "Survivalist":
                ability.BonusDice = 2;
                // Heal amount improved in combat logic
                break;

            case "Chain Lightning":
                ability.BonusDice = 3;
                ability.SuccessThreshold = 3; // Easier to trigger 2d6 damage
                break;

            // Level 5 abilities (if unlocked)
            case "Battle Rage":
                ability.BonusDice = 2;
                ability.StaminaCost = 8; // Cheaper
                break;

            case "Exploit Weakness":
                ability.NextAttackBonusDice = 4; // Was +3, now +4
                break;

            case "Disrupt":
                ability.BonusDice = 2;
                ability.SuccessThreshold = 2; // Easier to stun
                break;

            // v0.7: Bone-Setter abilities
            case "Mend Wound":
                ability.BonusDice = 2; // Was +1, now +2
                ability.StaminaCost = 3; // Was 5, now cheaper
                break;

            case "Apply Tourniquet":
                ability.BonusDice = 3; // Was +2, now +3 (easier success)
                break;

            case "Anatomical Insight":
                ability.BonusDice = 3; // Was +2, now +3
                ability.SuccessThreshold = 2; // Was 3, easier to apply
                break;

            case "Administer Antidote":
                ability.BonusDice = 3; // Was +2, now +3
                ability.StaminaCost = 10; // Was 15, now cheaper
                break;

            case "Cognitive Realignment":
                ability.BonusDice = 3; // Was +2, now +3
                ability.StaminaCost = 20; // Was 30, now cheaper
                ability.DamageDice = 3; // Was 2d6 stress restoration, now 3d6
                break;

            case "Miracle Worker":
                ability.BonusDice = 4; // Was +3, now +4
                ability.DamageDice = 5; // Was 4d6 healing, now 5d6
                ability.StaminaCost = 50; // Was 60, now cheaper
                break;

            // v0.7: Jötun-Reader abilities
            case "Analyze Weakness":
                ability.BonusDice = 4; // Was +3, now +4
                ability.StaminaCost = 25; // Was 30, now cheaper (but still 5 Stress cost)
                break;

            case "Exploit Design Flaw":
                ability.BonusDice = 3; // Was +2, now +3
                ability.StaminaCost = 30; // Was 35, now cheaper
                break;

            case "Navigational Bypass":
                ability.BonusDice = 4; // Was +3, now +4
                ability.StaminaCost = 25; // Was 30, now cheaper
                break;

            case "The Unspoken Truth":
                ability.BonusDice = 4; // Was +3, now +4
                ability.DamageDice = 3; // Was 2d6, now 3d6 psychic damage
                break;

            case "Architect of the Silence":
                ability.BonusDice = 5; // Was +4, now +5 (extremely powerful)
                ability.SuccessThreshold = 3; // Was 4, slightly easier
                ability.StaminaCost = 50; // Was 60, now cheaper (still 15 Stress)
                break;

            // v0.7: Skald abilities
            case "Saga of Courage":
                ability.BonusDice = 3; // Was +2, now +3
                ability.StaminaCost = 35; // Was 40, now cheaper
                break;

            case "Dirge of Defeat":
                ability.BonusDice = 3; // Was +2, now +3
                ability.StaminaCost = 35; // Was 40, now cheaper
                break;

            case "Rousing Verse":
                ability.BonusDice = 3; // Was +2, now +3
                ability.DamageDice = 3; // Was 2d6 stamina, now 3d6
                ability.StaminaCost = 15; // Was 20, now cheaper
                break;

            case "Song of Silence":
                ability.BonusDice = 3; // Was +2, now +3
                ability.SuccessThreshold = 2; // Was 3, easier to silence
                break;

            case "Lay of the Iron Wall":
                ability.BonusDice = 3; // Was +2, now +3
                ability.StaminaCost = 45; // Was 50, now cheaper
                // Soak bonus increased in CombatEngine logic
                break;

            case "Saga of the Einherjar":
                ability.BonusDice = 4; // Was +3, now +4
                ability.DamageDice = 3; // Was 2d6 temp HP, now 3d6
                ability.StaminaCost = 60; // Was 70, now cheaper
                break;
        }
    }

    /// <summary>
    /// v0.7: Unlock a specialization for 10 PP
    /// </summary>
    public bool UnlockSpecialization(PlayerCharacter player, Specialization specialization)
    {
        const int SpecializationCost = 10;

        // Check if player has enough PP
        if (player.ProgressionPoints < SpecializationCost)
        {
            _log.Warning("Specialization unlock failed: Character={CharacterName}, Specialization={Specialization}, Reason=InsufficientPP, RequiredPP={Required}, CurrentPP={Current}",
                player.Name, specialization, SpecializationCost, player.ProgressionPoints);
            return false;
        }

        // Check if player already has a specialization
        if (player.Specialization != Specialization.None)
        {
            _log.Warning("Specialization unlock failed: Character={CharacterName}, Specialization={Specialization}, Reason=AlreadyHasSpecialization, CurrentSpecialization={Current}",
                player.Name, specialization, player.Specialization);
            return false;
        }

        // Check if specialization is valid for this archetype
        if (!SpecializationFactory.CanChooseSpecialization(player, specialization))
        {
            _log.Warning("Specialization unlock failed: Character={CharacterName}, Specialization={Specialization}, Reason=InvalidForArchetype, Class={Class}",
                player.Name, specialization, player.Class);
            return false;
        }

        // Spend PP
        player.ProgressionPoints -= SpecializationCost;

        // Apply specialization (adds Tier 1 abilities)
        SpecializationFactory.ApplySpecialization(player, specialization);

        _log.Information("Specialization unlocked: Character={CharacterName}, Specialization={Specialization}, PPSpent={Cost}, RemainingPP={PP}, Class={Class}",
            player.Name, specialization, SpecializationCost, player.ProgressionPoints, player.Class);

        return true;
    }

    /// <summary>
    /// v0.7: Check if player can unlock a specialization
    /// </summary>
    public bool CanUnlockSpecialization(PlayerCharacter player)
    {
        const int SpecializationCost = 10;
        return player.ProgressionPoints >= SpecializationCost &&
               player.Specialization == Specialization.None;
    }

    /// <summary>
    /// Get list of abilities that are usable (unlocked and available)
    /// For now, returns all abilities. Full specialization system comes in v0.5+
    /// </summary>
    public List<Ability> GetUsableAbilities(PlayerCharacter player)
    {
        // For v0.2.1, all abilities in the list are usable
        // This will be expanded in v0.5+ with specialization trees
        return player.Abilities;
    }

    /// <summary>
    /// Calculate trauma modifier based on encounter context
    /// </summary>
    public float CalculateTraumaMod(string encounterType)
    {
        return encounterType.ToLower() switch
        {
            "normal" => 1.0f,           // Coherent act
            "puzzle" => 1.25f,          // Taxing act (interfacing with corrupted systems)
            "boss" => 1.25f,            // Taxing act (Blight-corrupted enemy)
            "blight" => 1.25f,          // Taxing act (Blight-corrupted area)
            _ => 1.0f
        };
    }
}
