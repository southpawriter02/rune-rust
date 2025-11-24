using RuneAndRust.Core;
using Serilog;

namespace RuneAndRust.Engine;

/// <summary>
/// v0.30.2: Slippery Terrain Service
/// Handles [Slippery Terrain] hazard for Niflheim biome.
/// Processes FINESSE checks, knockdown risk, and forced movement amplification.
///
/// Responsibilities:
/// - FINESSE DC 12 checks for movement on ice
/// - Apply [Knocked Down] status and fall damage (1d4) on failure
/// - Amplify forced movement effects (+1 tile distance)
/// - Handle immunity bypass for [Knocked Down]-immune characters
/// </summary>
public class SlipperyTerrainService
{
    private static readonly ILogger _log = Log.ForContext<SlipperyTerrainService>();
    private readonly DiceService _diceService;

    /// <summary>
    /// v0.30.2 canonical: FINESSE check DC for [Slippery Terrain]
    /// </summary>
    private const int SLIPPERY_TERRAIN_DC = 12;

    /// <summary>
    /// v0.30.2 canonical: Fall damage dice on check failure (1d4)
    /// </summary>
    private const int FALL_DAMAGE_DICE_COUNT = 1;
    private const int FALL_DAMAGE_DIE_SIZE = 4;

    /// <summary>
    /// v0.30.2 canonical: Forced movement amplification on slippery terrain
    /// </summary>
    private const int FORCED_MOVEMENT_AMPLIFY = 1;

    public SlipperyTerrainService(DiceService diceService)
    {
        _diceService = diceService;
        _log.Information("SlipperyTerrainService initialized");
    }

    #region Movement Checks

    /// <summary>
    /// Process movement through slippery terrain for a character.
    /// FINESSE DC 12 check or [Knocked Down] + 1d4 Physical damage.
    /// </summary>
    public SlipperyTerrainMovementResult ProcessMovement(PlayerCharacter character)
    {
        _log.Information("{Character} attempting movement on [Slippery Terrain]",
            character.Name);

        var result = new SlipperyTerrainMovementResult
        {
            CharacterName = character.Name,
            FinesseValue = character.Attributes.Finesse
        };

        // Check if character is immune to [Knocked Down]
        // TODO: Implement when status immunity system is complete
        bool isImmuneToKnockdown = CheckKnockdownImmunity(character);

        if (isImmuneToKnockdown)
        {
            result.ImmuneToKnockdown = true;
            result.CheckPassed = true;
            result.WasKnockedDown = false;
            result.DamageDealt = 0;
            result.Message = $"✓ {character.Name} navigates [Slippery Terrain] safely (immune to [Knocked Down])";

            _log.Information("{Character} is immune to [Knocked Down], bypasses [Slippery Terrain] hazard",
                character.Name);

            return result;
        }

        // Make FINESSE check (DC 12)
        var rollResult = _diceService.Roll(character.Attributes.Finesse);
        result.SuccessesRolled = rollResult.Successes;
        result.CheckPassed = rollResult.Successes >= SLIPPERY_TERRAIN_DC;

        if (result.CheckPassed)
        {
            // Success: Movement succeeds
            result.WasKnockedDown = false;
            result.DamageDealt = 0;
            result.Message = $"✓ {character.Name} navigates [Slippery Terrain] safely (FINESSE check passed)\n" +
                           $"   Rolled {rollResult.Successes} successes (DC {SLIPPERY_TERRAIN_DC})";

            _log.Information("{Character} passed [Slippery Terrain] check: {Successes} successes (DC {DC})",
                character.Name, rollResult.Successes, SLIPPERY_TERRAIN_DC);
        }
        else
        {
            // Failure: [Knocked Down] + fall damage
            result.WasKnockedDown = true;

            // Roll 1d4 fall damage
            int fallDamage = _diceService.RollDamage(FALL_DAMAGE_DICE_COUNT, FALL_DAMAGE_DIE_SIZE);
            character.HP = Math.Max(0, character.HP - fallDamage);

            result.DamageDealt = fallDamage;
            result.Message = $"✗ {character.Name} slips on [Slippery Terrain] ({rollResult.Successes} successes, needed {SLIPPERY_TERRAIN_DC})\n" +
                           $"   ❄️ You lose your footing on the ice!\n" +
                           $"   [Knocked Down] + Fall Damage: {FALL_DAMAGE_DICE_COUNT}d{FALL_DAMAGE_DIE_SIZE} = {fallDamage}\n" +
                           $"   HP: {character.HP}/{character.MaxHP}";

            _log.Information("{Character} failed [Slippery Terrain] check: {Successes} successes, [Knocked Down] + {Damage} fall damage (HP: {HP}/{MaxHP})",
                character.Name, rollResult.Successes, fallDamage, character.HP, character.MaxHP);

            // TODO: Apply [Knocked Down] status when status system is implemented
        }

        return result;
    }

    /// <summary>
    /// Process movement through slippery terrain for an enemy.
    /// </summary>
    public SlipperyTerrainMovementResult ProcessMovement(Enemy enemy, int finesseValue)
    {
        _log.Information("{Enemy} attempting movement on [Slippery Terrain]",
            enemy.Name);

        var result = new SlipperyTerrainMovementResult
        {
            CharacterName = enemy.Name,
            FinesseValue = finesseValue
        };

        // Check for Ice-Walker passive (immunity to slippery terrain)
        // TODO: Implement tag-based immunity check when tag system is complete
        bool hasIceWalker = CheckIceWalkerPassive(enemy);

        if (hasIceWalker)
        {
            result.ImmuneToKnockdown = true;
            result.CheckPassed = true;
            result.WasKnockedDown = false;
            result.DamageDealt = 0;
            result.Message = $"✓ {enemy.Name} navigates [Slippery Terrain] safely ([Ice-Walker] passive)";

            _log.Information("{Enemy} has [Ice-Walker] passive, ignores [Slippery Terrain]",
                enemy.Name);

            return result;
        }

        // Make FINESSE check (DC 12)
        var rollResult = _diceService.Roll(finesseValue);
        result.SuccessesRolled = rollResult.Successes;
        result.CheckPassed = rollResult.Successes >= SLIPPERY_TERRAIN_DC;

        if (result.CheckPassed)
        {
            result.WasKnockedDown = false;
            result.DamageDealt = 0;
            result.Message = $"✓ {enemy.Name} navigates [Slippery Terrain] safely";

            _log.Information("{Enemy} passed [Slippery Terrain] check: {Successes} successes",
                enemy.Name, rollResult.Successes);
        }
        else
        {
            result.WasKnockedDown = true;

            // Roll 1d4 fall damage
            int fallDamage = _diceService.RollDamage(FALL_DAMAGE_DICE_COUNT, FALL_DAMAGE_DIE_SIZE);
            enemy.HP = Math.Max(0, enemy.HP - fallDamage);

            result.DamageDealt = fallDamage;
            result.Message = $"✗ {enemy.Name} slips on [Slippery Terrain]\n" +
                           $"   [Knocked Down] + {fallDamage} fall damage";

            _log.Information("{Enemy} failed [Slippery Terrain] check: [Knocked Down] + {Damage} fall damage",
                enemy.Name, fallDamage);
        }

        return result;
    }

    #endregion

    #region Forced Movement Amplification

    /// <summary>
    /// Calculate amplified forced movement distance on slippery terrain.
    /// Forced movement effects gain +1 tile distance.
    /// </summary>
    public int AmplifyForcedMovement(int baseDistance)
    {
        int amplifiedDistance = baseDistance + FORCED_MOVEMENT_AMPLIFY;

        _log.Debug("Forced movement amplified on [Slippery Terrain]: {Base} → {Amplified} tiles",
            baseDistance, amplifiedDistance);

        return amplifiedDistance;
    }

    /// <summary>
    /// Process forced movement on slippery terrain.
    /// Amplifies distance and may cause additional effects.
    /// </summary>
    public SlipperyTerrainForcedMovementResult ProcessForcedMovement(
        PlayerCharacter character,
        int baseDistance,
        string sourceName)
    {
        int amplifiedDistance = AmplifyForcedMovement(baseDistance);

        var result = new SlipperyTerrainForcedMovementResult
        {
            CharacterName = character.Name,
            SourceName = sourceName,
            BaseDistance = baseDistance,
            AmplifiedDistance = amplifiedDistance,
            BonusDistance = FORCED_MOVEMENT_AMPLIFY
        };

        result.Message = $"⚡ {character.Name} is pushed {baseDistance} tiles by {sourceName}\n" +
                       $"   [Slippery Terrain]: Forced movement amplified to {amplifiedDistance} tiles (+{FORCED_MOVEMENT_AMPLIFY})";

        _log.Information("{Character} forced movement amplified: {Base} → {Amplified} tiles (source: {Source})",
            character.Name, baseDistance, amplifiedDistance, sourceName);

        return result;
    }

    #endregion

    #region Immunity Checks

    /// <summary>
    /// Check if character is immune to [Knocked Down] status.
    /// </summary>
    private bool CheckKnockdownImmunity(PlayerCharacter character)
    {
        // TODO: Implement when status immunity system is complete
        // This would check:
        // - Character abilities/passives
        // - Equipment bonuses
        // - Active buffs
        return false;
    }

    /// <summary>
    /// Check if enemy has [Ice-Walker] passive (immunity to slippery terrain).
    /// </summary>
    private bool CheckIceWalkerPassive(Enemy enemy)
    {
        // TODO: Implement when enemy tag system is complete
        // This would check enemy tags for "ice_walker"
        return false;
    }

    #endregion

    #region Status Messages

    /// <summary>
    /// Get flavor text for [Slippery Terrain] hazard.
    /// </summary>
    public string GetSlipperyTerrainStatusMessage()
    {
        return $"❄️ [Slippery Terrain] - Environmental Hazard\n" +
               $"   FINESSE check (DC {SLIPPERY_TERRAIN_DC}) required for movement\n" +
               $"   Failure: [Knocked Down] + {FALL_DAMAGE_DICE_COUNT}d{FALL_DAMAGE_DIE_SIZE} Physical damage\n" +
               $"   Forced movement effects gain +{FORCED_MOVEMENT_AMPLIFY} tile distance\n" +
               $"   Characters immune to [Knocked Down] ignore this hazard";
    }

    /// <summary>
    /// Get warning message for slippery terrain area.
    /// </summary>
    public string GetWarningMessage(double coveragePercent)
    {
        return $"⚠️ Entering [Slippery Terrain] area ({coveragePercent:F0}% floor coverage)\n" +
               $"   Ice sheet covering the floor. Movement requires careful footing.\n" +
               $"   Recommendation: Characters with FINESSE < {SLIPPERY_TERRAIN_DC} are at high risk.";
    }

    #endregion
}

#region Result Data Transfer Objects

public class SlipperyTerrainMovementResult
{
    public string CharacterName { get; set; } = string.Empty;
    public int FinesseValue { get; set; }
    public int SuccessesRolled { get; set; }
    public bool CheckPassed { get; set; }
    public bool WasKnockedDown { get; set; }
    public int DamageDealt { get; set; }
    public bool ImmuneToKnockdown { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class SlipperyTerrainForcedMovementResult
{
    public string CharacterName { get; set; } = string.Empty;
    public string SourceName { get; set; } = string.Empty;
    public int BaseDistance { get; set; }
    public int AmplifiedDistance { get; set; }
    public int BonusDistance { get; set; }
    public string Message { get; set; } = string.Empty;
}

#endregion
