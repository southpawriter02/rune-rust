using RuneAndRust.Core;
using Serilog;

namespace RuneAndRust.Engine;

/// <summary>
/// v0.19.8: Service for handling all spellcasting mechanics, AP costs, and magical effects
/// </summary>
public class MagicService
{
    private static readonly ILogger _log = Log.ForContext<MagicService>();

    /// <summary>
    /// Check if a character has enough AP to cast a spell
    /// </summary>
    public bool CanCastSpell(PlayerCharacter caster, Ability spell)
    {
        if (spell.APCost == 0)
        {
            return true; // Free spell or non-magical ability
        }

        if (caster.AP < spell.APCost)
        {
            _log.Warning("Insufficient AP: Character={Name}, Spell={Spell}, Required={Required}, Current={Current}",
                caster.Name, spell.Name, spell.APCost, caster.AP);
            return false;
        }

        return true;
    }

    /// <summary>
    /// Deduct AP cost from caster
    /// </summary>
    public void DeductAPCost(PlayerCharacter caster, Ability spell)
    {
        if (spell.APCost == 0)
        {
            return; // Free spell
        }

        int oldAP = caster.AP;
        caster.AP -= spell.APCost;
        caster.AP = Math.Max(0, caster.AP); // Don't go below 0

        _log.Information("AP cost deducted: Character={Name}, Spell={Spell}, Cost={Cost}, OldAP={OldAP}, NewAP={NewAP}",
            caster.Name, spell.Name, spell.APCost, oldAP, caster.AP);
    }

    /// <summary>
    /// Restore AP to a character
    /// </summary>
    public void RestoreAP(PlayerCharacter character, int amount)
    {
        int oldAP = character.AP;
        character.AP += amount;
        character.AP = Math.Min(character.AP, character.MaxAP); // Don't exceed max

        _log.Information("AP restored: Character={Name}, Amount={Amount}, OldAP={OldAP}, NewAP={NewAP}, MaxAP={MaxAP}",
            character.Name, amount, oldAP, character.AP, character.MaxAP);
    }

    /// <summary>
    /// Calculate spell potency based on WILL attribute
    /// </summary>
    public int CalculateSpellPotency(PlayerCharacter caster, int basePotency)
    {
        // Spells scale with WILL attribute (Primary stat for Mystics)
        int willBonus = caster.Attributes.Will / 2;
        int totalPotency = basePotency + willBonus;

        _log.Debug("Spell potency calculated: Character={Name}, BasePotency={Base}, WillBonus={Bonus}, Total={Total}",
            caster.Name, basePotency, willBonus, totalPotency);

        return totalPotency;
    }

    /// <summary>
    /// Apply self-Corruption for heretical spells (Rust-Witch)
    /// </summary>
    public void ApplyCorruptionCost(PlayerCharacter caster, int corruptionCost, string abilityName)
    {
        if (corruptionCost == 0)
        {
            return; // No Corruption cost
        }

        int oldCorruption = caster.Corruption;
        caster.Corruption += corruptionCost;
        caster.Corruption = Math.Min(100, caster.Corruption); // Cap at 100

        _log.Warning("Heretical spell cast - Corruption inflicted: Character={Name}, Ability={Ability}, Cost={Cost}, OldCorruption={Old}, NewCorruption={New}",
            caster.Name, abilityName, corruptionCost, oldCorruption, caster.Corruption);
    }

    /// <summary>
    /// Handle Focus Aether ability - restore 25 AP (ends turn)
    /// </summary>
    public void FocusAether(PlayerCharacter caster)
    {
        // Base restoration: 25 AP
        // Rank 2: 35 AP
        // Rank 3: 50 AP
        var focusAbility = caster.Abilities.FirstOrDefault(a => a.Name == "Focus Aether");

        int restoreAmount = focusAbility?.CurrentRank switch
        {
            1 => 25,
            2 => 35,
            3 => 50,
            _ => 25
        };

        RestoreAP(caster, restoreAmount);

        _log.Information("Focus Aether used: Character={Name}, Restored={Amount} AP",
            caster.Name, restoreAmount);
    }

    /// <summary>
    /// Handle short rest AP regeneration (25% Max AP)
    /// </summary>
    public void ShortRestRegeneration(PlayerCharacter character)
    {
        if (character.Class != CharacterClass.Mystic)
        {
            return; // Only Mystics use AP
        }

        int regenAmount = (int)(character.MaxAP * 0.25f);
        RestoreAP(character, regenAmount);

        _log.Information("Short rest AP regeneration: Character={Name}, Restored={Amount} AP (25% of {Max})",
            character.Name, regenAmount, character.MaxAP);
    }

    /// <summary>
    /// Handle long rest AP regeneration (100% Max AP)
    /// </summary>
    public void LongRestRegeneration(PlayerCharacter character)
    {
        if (character.Class != CharacterClass.Mystic)
        {
            return; // Only Mystics use AP
        }

        int oldAP = character.AP;
        character.AP = character.MaxAP; // Full restore

        _log.Information("Long rest AP regeneration: Character={Name}, OldAP={Old}, NewAP={New}",
            character.Name, oldAP, character.AP);
    }
}
