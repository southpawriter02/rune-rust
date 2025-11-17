// ==============================================================================
// v0.38.7: Galdr & Ability Flavor Text Integration
// CombatEngine_GaldrFlavorExtensions.cs
// ==============================================================================
// Purpose: Extends CombatEngine with Galdr flavor text generation
// Pattern: Follows CombatFlavorTextService integration pattern
// Integration: Called during ability execution (PlayerUseAbility)
// ==============================================================================

using RuneAndRust.Core;
using RuneAndRust.Core.GaldrFlavor;
using Serilog;

namespace RuneAndRust.Engine;

/// <summary>
/// Galdr flavor text extensions for CombatEngine.
/// Generates dynamic flavor text for Galdr casting, ability outcomes, and miscasts.
/// </summary>
public partial class CombatEngine
{
    private static readonly ILogger _galdrLog = Log.ForContext<CombatEngine>();

    /// <summary>
    /// Galdr flavor text service (optional, null-safe).
    /// Set in constructor if flavor text is enabled.
    /// </summary>
    private readonly GaldrFlavorTextService? _galdrFlavorTextService;

    #region Galdr Flavor Text Generation

    /// <summary>
    /// Generates Galdr casting flavor text for magical abilities.
    /// Called after successful ability roll.
    /// </summary>
    /// <param name="ability">The ability being cast</param>
    /// <param name="successCount">Number of successes rolled</param>
    /// <param name="targetName">Name of the target</param>
    /// <param name="casterName">Name of the caster (default: "You")</param>
    /// <param name="casterArchetype">Caster archetype for voice profile (optional)</param>
    /// <returns>Galdr casting flavor text or empty string if service unavailable</returns>
    private string GenerateGaldrCastingFlavor(
        Ability ability,
        int successCount,
        string targetName,
        string casterName = "You",
        string? casterArchetype = null)
    {
        // Null-safe check
        if (_galdrFlavorTextService == null)
            return string.Empty;

        // Only generate Galdr flavor for magical abilities
        if (!ability.IsGaldr || string.IsNullOrEmpty(ability.RuneSchool))
            return string.Empty;

        try
        {
            var biomeName = GetCurrentBiomeName(); // Implement based on your biome system
            var castingText = _galdrFlavorTextService.GenerateGaldrCastingText(
                runeSchool: ability.RuneSchool,
                abilityName: ability.Name,
                successCount: successCount,
                targetName: targetName,
                casterName: casterName,
                biomeName: biomeName,
                casterArchetype: casterArchetype
            );

            return castingText;
        }
        catch (Exception ex)
        {
            _galdrLog.Warning(ex, "Failed to generate Galdr casting flavor for {Ability}", ability.Name);
            return string.Empty;
        }
    }

    /// <summary>
    /// Generates Galdr outcome flavor text (damage, healing, effects).
    /// Called after ability effects are applied.
    /// </summary>
    /// <param name="ability">The ability that was cast</param>
    /// <param name="outcomeType">Type of outcome (Hit, CriticalHit, Miss, etc.)</param>
    /// <param name="targetName">Name of the target</param>
    /// <param name="damageOrHealing">Damage dealt or healing provided</param>
    /// <param name="enemyArchetype">Enemy archetype (optional)</param>
    /// <returns>Outcome flavor text or empty string</returns>
    private string GenerateGaldrOutcomeFlavor(
        Ability ability,
        string outcomeType,
        string targetName,
        int? damageOrHealing = null,
        string? enemyArchetype = null)
    {
        if (_galdrFlavorTextService == null)
            return string.Empty;

        if (!ability.IsGaldr)
            return string.Empty;

        try
        {
            var effectCategory = DetermineEffectCategory(ability);
            var outcomeText = _galdrFlavorTextService.GenerateGaldrOutcomeText(
                abilityName: ability.Name,
                outcomeType: outcomeType,
                targetName: targetName,
                damageOrHealing: damageOrHealing,
                effectCategory: effectCategory,
                enemyArchetype: enemyArchetype
            );

            return outcomeText;
        }
        catch (Exception ex)
        {
            _galdrLog.Warning(ex, "Failed to generate Galdr outcome flavor for {Ability}", ability.Name);
            return string.Empty;
        }
    }

    /// <summary>
    /// Generates non-Galdr ability flavor text (weapon arts, tactics, etc.).
    /// </summary>
    /// <param name="ability">The ability being used</param>
    /// <param name="weaponName">Weapon name (if applicable)</param>
    /// <param name="targetName">Target name (if applicable)</param>
    /// <param name="successLevel">Success level (MinorSuccess, SolidSuccess, ExceptionalSuccess)</param>
    /// <param name="specialization">Player specialization (optional)</param>
    /// <returns>Ability flavor text or empty string</returns>
    private string GenerateAbilityFlavor(
        Ability ability,
        string? weaponName = null,
        string? targetName = null,
        string? successLevel = null,
        string? specialization = null)
    {
        if (_galdrFlavorTextService == null)
            return string.Empty;

        // Only for non-Galdr abilities
        if (ability.IsGaldr)
            return string.Empty;

        try
        {
            var abilityText = _galdrFlavorTextService.GenerateAbilityFlavorText(
                abilityCategory: ability.AbilityCategory ?? "WeaponArt",
                abilityName: ability.Name,
                weaponName: weaponName,
                weaponType: null, // Could be extracted from weapon if available
                targetName: targetName,
                successLevel: successLevel,
                specialization: specialization
            );

            return abilityText;
        }
        catch (Exception ex)
        {
            _galdrLog.Warning(ex, "Failed to generate ability flavor for {Ability}", ability.Name);
            return string.Empty;
        }
    }

    /// <summary>
    /// Generates miscast flavor text for failed Galdr casting.
    /// </summary>
    /// <param name="ability">The ability that misfired</param>
    /// <param name="severity">Severity of miscast (Minor, Moderate, Severe, Catastrophic)</param>
    /// <returns>Miscast flavor text or empty string</returns>
    private string GenerateMiscastFlavor(
        Ability ability,
        string severity = "Moderate")
    {
        if (_galdrFlavorTextService == null)
            return string.Empty;

        if (!ability.IsGaldr || string.IsNullOrEmpty(ability.RuneSchool))
            return string.Empty;

        try
        {
            var biomeName = GetCurrentBiomeName();
            var miscastType = DetermineMiscastType(biomeName);
            var corruptionSource = DetermineCorruptionSource(biomeName);

            var miscastText = _galdrFlavorTextService.GenerateMiscastText(
                miscastType: miscastType,
                severity: severity,
                runeSchool: ability.RuneSchool,
                abilityName: ability.Name,
                biomeName: biomeName,
                corruptionSource: corruptionSource
            );

            return miscastText;
        }
        catch (Exception ex)
        {
            _galdrLog.Warning(ex, "Failed to generate miscast flavor for {Ability}", ability.Name);
            return string.Empty;
        }
    }

    /// <summary>
    /// Generates environmental reaction to Galdr casting.
    /// Triggered with 30% probability (configurable).
    /// </summary>
    /// <param name="ability">The ability being cast</param>
    /// <returns>Environmental reaction text or empty string</returns>
    private string GenerateEnvironmentalReactionFlavor(Ability ability)
    {
        if (_galdrFlavorTextService == null)
            return string.Empty;

        if (!ability.IsGaldr || string.IsNullOrEmpty(ability.RuneSchool))
            return string.Empty;

        try
        {
            var biomeName = GetCurrentBiomeName();
            var reactionText = _galdrFlavorTextService.GenerateGaldrEnvironmentalReaction(
                biomeName: biomeName,
                runeSchool: ability.RuneSchool,
                element: ability.Element,
                reactionType: null // Let service randomly select based on trigger chance
            );

            return reactionText;
        }
        catch (Exception ex)
        {
            _galdrLog.Warning(ex, "Failed to generate environmental reaction for {Ability}", ability.Name);
            return string.Empty;
        }
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Determines success level based on number of successes.
    /// </summary>
    private string DetermineSuccessLevel(int successCount)
    {
        return successCount switch
        {
            <= 2 => GaldrActionDescriptor.SuccessLevels.MinorSuccess,
            >= 5 => GaldrActionDescriptor.SuccessLevels.ExceptionalSuccess,
            _ => GaldrActionDescriptor.SuccessLevels.SolidSuccess
        };
    }

    /// <summary>
    /// Determines effect category based on ability type.
    /// </summary>
    private string DetermineEffectCategory(Ability ability)
    {
        if (ability.DamageDice > 0)
            return GaldrOutcomeDescriptor.EffectCategories.Damage;
        if (ability.DefensePercent > 0)
            return GaldrOutcomeDescriptor.EffectCategories.Buff;
        if (ability.SkipEnemyTurn)
            return GaldrOutcomeDescriptor.EffectCategories.Control;

        return GaldrOutcomeDescriptor.EffectCategories.Utility;
    }

    /// <summary>
    /// Determines miscast type based on biome.
    /// </summary>
    private string DetermineMiscastType(string? biomeName)
    {
        return biomeName switch
        {
            "Alfheim" => GaldrMiscastDescriptor.MiscastTypes.AlfheimDistortion,
            "The_Roots" => GaldrMiscastDescriptor.MiscastTypes.BlightCorruption,
            _ => GaldrMiscastDescriptor.MiscastTypes.Paradox
        };
    }

    /// <summary>
    /// Determines corruption source based on biome.
    /// </summary>
    private string? DetermineCorruptionSource(string? biomeName)
    {
        return biomeName switch
        {
            "Alfheim" => GaldrMiscastDescriptor.CorruptionSources.AlfheimCursedChoir,
            "The_Roots" or "Muspelheim" or "Niflheim" => GaldrMiscastDescriptor.CorruptionSources.RunicBlight,
            _ => null
        };
    }

    /// <summary>
    /// Gets current biome name.
    /// TODO: Implement based on your biome system (Room.BiomeName or similar).
    /// </summary>
    private string? GetCurrentBiomeName()
    {
        // PLACEHOLDER: Replace with actual biome retrieval
        // Examples:
        // - return _currentRoom?.BiomeName;
        // - return _gameState?.CurrentBiome;
        // - return "The_Roots"; // Default for testing

        return null; // Return null if biome system not yet implemented
    }

    #endregion

    #region Integration Helpers

    /// <summary>
    /// Generates complete ability flavor text (casting + outcome + environmental).
    /// Call this from PlayerUseAbility after ability roll succeeds.
    /// </summary>
    /// <param name="ability">The ability being used</param>
    /// <param name="successCount">Number of successes rolled</param>
    /// <param name="targetName">Target name</param>
    /// <param name="damageOrHealing">Damage/healing amount (optional)</param>
    /// <param name="outcomeType">Outcome type (Hit, CriticalHit, etc.)</param>
    /// <param name="combatState">Combat state to add log entries</param>
    public void GenerateAndLogAbilityFlavor(
        Ability ability,
        int successCount,
        string targetName,
        int? damageOrHealing,
        string outcomeType,
        CombatState combatState)
    {
        if (_galdrFlavorTextService == null)
            return;

        // 1. Casting/Activation Flavor
        string castingFlavor = ability.IsGaldr
            ? GenerateGaldrCastingFlavor(ability, successCount, targetName)
            : GenerateAbilityFlavor(ability, null, targetName, DetermineSuccessLevel(successCount));

        if (!string.IsNullOrEmpty(castingFlavor))
        {
            combatState.AddLogEntry(castingFlavor);
        }

        // 2. Environmental Reaction (Galdr only, chance-based)
        if (ability.IsGaldr)
        {
            string environmentalFlavor = GenerateEnvironmentalReactionFlavor(ability);
            if (!string.IsNullOrEmpty(environmentalFlavor))
            {
                combatState.AddLogEntry(environmentalFlavor);
            }
        }

        // 3. Outcome Flavor
        if (ability.IsGaldr)
        {
            string outcomeFlavor = GenerateGaldrOutcomeFlavor(
                ability,
                outcomeType,
                targetName,
                damageOrHealing
            );

            if (!string.IsNullOrEmpty(outcomeFlavor))
            {
                combatState.AddLogEntry(outcomeFlavor);
            }
        }
    }

    /// <summary>
    /// Generates and logs miscast flavor text.
    /// Call this when ability roll fails or critical failure occurs.
    /// </summary>
    public void GenerateAndLogMiscastFlavor(
        Ability ability,
        string severity,
        CombatState combatState)
    {
        if (_galdrFlavorTextService == null)
            return;

        string miscastFlavor = GenerateMiscastFlavor(ability, severity);
        if (!string.IsNullOrEmpty(miscastFlavor))
        {
            combatState.AddLogEntry(miscastFlavor);
        }
    }

    #endregion
}
