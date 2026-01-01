using Microsoft.Extensions.Logging;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Core.Models;
using RuneAndRust.Core.Models.Combat;
using CharacterAttribute = RuneAndRust.Core.Enums.Attribute;
using CharacterEntity = RuneAndRust.Core.Entities.Character;

namespace RuneAndRust.Engine.Services;

/// <summary>
/// Generates contextual help tips based on game state (v0.3.9c).
/// Analyzes player status effects, resource levels, enemy types, and environment
/// to provide relevant, prioritized gameplay advice.
/// </summary>
public class ContextHelpService : IContextHelpService
{
    private readonly ILogger<ContextHelpService> _logger;

    /// <summary>
    /// WITS attribute threshold required to reveal enemy tactical tips.
    /// Tips are shown if WITS >= this value OR target has Analyzed status.
    /// </summary>
    public int WitsThreshold => 3;

    /// <summary>
    /// Maximum number of tips returned from analysis methods.
    /// </summary>
    public int MaxTips => 3;

    /// <summary>
    /// Initializes a new instance of the <see cref="ContextHelpService"/> class.
    /// </summary>
    /// <param name="logger">The logger for traceability.</param>
    public ContextHelpService(ILogger<ContextHelpService> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc/>
    public List<HelpTip> Analyze(GameState state)
    {
        var tips = new List<HelpTip>();

        if (state.CurrentCharacter == null)
        {
            _logger.LogTrace("[Help] No active character, returning empty tips");
            return tips;
        }

        var character = state.CurrentCharacter;

        // Status Effect Tips
        AnalyzeStatusEffects(character.ActiveStatusEffects, tips);

        // Resource Level Tips
        AnalyzeResources(character, tips);

        // Sort by priority (highest first) and limit
        var result = tips
            .OrderByDescending(t => t.Priority)
            .Take(MaxTips)
            .ToList();

        _logger.LogTrace("[Help] Generated {Count} tips for exploration state", result.Count);
        return result;
    }

    /// <inheritdoc/>
    public List<HelpTip> AnalyzeCombat(CombatState combatState)
    {
        var tips = new List<HelpTip>();

        if (combatState == null)
        {
            _logger.LogTrace("[Help] No combat state, returning empty tips");
            return tips;
        }

        // Find the player combatant
        var player = combatState.TurnOrder.FirstOrDefault(c => c.IsPlayer);
        if (player == null)
        {
            _logger.LogTrace("[Help] No player in combat, returning empty tips");
            return tips;
        }

        // Player Status Effect Tips
        AnalyzeCombatantStatusEffects(player.StatusEffects, tips);

        // Player Resource Level Tips (combat-specific thresholds)
        AnalyzeCombatResources(player, tips);

        // Enemy Tactical Tips (WITS-gated)
        var playerWits = player.CharacterSource?.GetEffectiveAttribute(CharacterAttribute.Wits) ?? 0;
        var enemies = combatState.TurnOrder.Where(c => !c.IsPlayer && c.CurrentHp > 0).ToList();

        foreach (var enemy in enemies)
        {
            AnalyzeEnemyTactics(enemy, playerWits, tips);
        }

        // Stunned check - highest priority
        if (player.StatusEffects.Any(e => e.Type == StatusEffectType.Stunned))
        {
            tips.Add(HelpTip.Critical("Stunned", "You cannot act this turn!"));
        }

        // Sort by priority (highest first) and limit
        var result = tips
            .OrderByDescending(t => t.Priority)
            .Take(MaxTips)
            .ToList();

        _logger.LogTrace("[Help] Generated {Count} tips for combat state", result.Count);
        return result;
    }

    /// <inheritdoc/>
    public List<HelpTip> AnalyzeMagic(CombatState combatState, IAetherService aetherService)
    {
        var tips = new List<HelpTip>();

        if (combatState?.ActiveCombatant == null)
        {
            _logger.LogTrace("[Help] No active combatant for magic analysis");
            return tips;
        }

        var caster = combatState.ActiveCombatant;
        var flux = aetherService.CurrentFlux;
        var risk = Math.Max(0, flux - 50);

        // High risk warning (>=40%)
        if (risk >= 40)
        {
            tips.Add(HelpTip.Critical("HIGH RISK",
                $"Backlash chance is {risk}%! Consider physical attacks or waiting."));
            _logger.LogTrace("[Help] Added tip: HIGH RISK ({Risk}%)", risk);
        }
        // Elevated risk (20-39%)
        else if (risk >= 20)
        {
            tips.Add(HelpTip.Warning("Elevated Risk",
                $"Backlash chance: {risk}%. Cast with caution."));
            _logger.LogTrace("[Help] Added tip: Elevated Risk ({Risk}%)", risk);
        }
        // Rising flux (25-49, no risk yet)
        else if (flux >= 25)
        {
            tips.Add(HelpTip.Info("Rising Flux",
                "Flux is elevated. Powerful spells increase risk."));
            _logger.LogTrace("[Help] Added tip: Rising Flux");
        }

        // AP warnings
        if (caster.MaxAp > 0)
        {
            var apPct = (double)caster.CurrentAp / caster.MaxAp * 100;
            if (apPct <= 20)
            {
                tips.Add(HelpTip.Warning("Low Aether",
                    "AP nearly depleted. Use items or rest to recover."));
                _logger.LogTrace("[Help] Added tip: Low Aether");
            }
            else if (apPct <= 40)
            {
                tips.Add(HelpTip.Info("Aether Reserves",
                    "AP running low. Conserve for critical moments."));
                _logger.LogTrace("[Help] Added tip: Aether Reserves");
            }
        }

        // Aether Sickness warning
        if (caster.StatusEffects.Any(e => e.Type == StatusEffectType.AetherSickness))
        {
            var sickness = caster.StatusEffects.First(e => e.Type == StatusEffectType.AetherSickness);
            tips.Add(HelpTip.Warning("Aether Sickness",
                $"-2 WILL, -1 WITS. Cannot concentrate. ({sickness.DurationRemaining} turns)"));
            _logger.LogTrace("[Help] Added tip: Aether Sickness");
        }

        // Corruption warning
        var corruption = caster.CharacterSource?.Corruption ?? 0;
        if (corruption >= 50)
        {
            tips.Add(HelpTip.Critical("Soul Corruption",
                $"Corruption at {corruption}. Seek purification."));
            _logger.LogTrace("[Help] Added tip: Soul Corruption");
        }
        else if (corruption >= 25)
        {
            tips.Add(HelpTip.Warning("Corruption",
                $"Corruption at {corruption}. Avoid Catastrophic backlash."));
            _logger.LogTrace("[Help] Added tip: Corruption");
        }

        var result = tips.Take(MaxTips).ToList();
        _logger.LogTrace("[Help] Generated {Count} magic tips", result.Count);
        return result;
    }

    /// <summary>
    /// Analyzes character status effects and adds relevant tips.
    /// </summary>
    private void AnalyzeStatusEffects(List<StatusEffectType> effects, List<HelpTip> tips)
    {
        if (effects == null) return;

        foreach (var effect in effects)
        {
            var tip = GetStatusEffectTip(effect);
            if (tip != null)
            {
                tips.Add(tip);
                _logger.LogTrace("[Help] Added tip: {Title}", tip.Title);
            }
        }
    }

    /// <summary>
    /// Analyzes combatant active status effects and adds relevant tips.
    /// </summary>
    private void AnalyzeCombatantStatusEffects(List<ActiveStatusEffect> effects, List<HelpTip> tips)
    {
        if (effects == null) return;

        foreach (var effect in effects)
        {
            var tip = GetStatusEffectTip(effect.Type);
            if (tip != null)
            {
                tips.Add(tip);
                _logger.LogTrace("[Help] Added tip: {Title}", tip.Title);
            }
        }
    }

    /// <summary>
    /// Gets the help tip for a given status effect type.
    /// </summary>
    private static HelpTip? GetStatusEffectTip(StatusEffectType type)
    {
        return type switch
        {
            StatusEffectType.Bleeding => HelpTip.Warning("Bleeding",
                "Use [green]Bandage[/] or [green]Tourniquet[/] to stop HP loss."),

            StatusEffectType.Poisoned => HelpTip.Warning("Poisoned",
                "Use [green]Antidote[/] or wait for it to wear off. Armor reduces damage."),

            StatusEffectType.Stunned => HelpTip.Critical("Stunned",
                "You cannot act this turn!"),

            StatusEffectType.Vulnerable => HelpTip.Warning("Vulnerable",
                "Taking +50% damage from all sources. Use defensive abilities."),

            StatusEffectType.Disoriented => HelpTip.Warning("Disoriented",
                "-1 penalty to all dice pools. Focus and steady yourself."),

            StatusEffectType.Exhausted => HelpTip.Info("Exhausted",
                "Rest recovery is halved. Reach a [cyan]Sanctuary[/] for full recovery."),

            StatusEffectType.Analyzed => HelpTip.Info("Analyzed",
                "Enemy intent is revealed. Use this intel wisely."),

            // Buffs don't need tips - they're already helpful
            _ => null
        };
    }

    /// <summary>
    /// Analyzes character resource levels and adds relevant tips.
    /// </summary>
    private void AnalyzeResources(CharacterEntity character, List<HelpTip> tips)
    {
        // Low HP (<25%)
        if (character.CurrentHP <= character.MaxHP / 4)
        {
            tips.Add(HelpTip.Critical("Low HP",
                "HP critical! Use healing items or find a safe place to rest."));
            _logger.LogTrace("[Help] Added tip: Low HP");
        }
        // Medium HP (25-50%)
        else if (character.CurrentHP <= character.MaxHP / 2)
        {
            tips.Add(HelpTip.Warning("Wounded",
                "HP below half. Consider using [green]Bandage[/] or [green]Potion[/]."));
            _logger.LogTrace("[Help] Added tip: Wounded");
        }

        // Low Stamina (<20%)
        if (character.CurrentStamina <= character.MaxStamina / 5)
        {
            tips.Add(HelpTip.Warning("Exhausted Stamina",
                "Stamina nearly depleted. Consider resting or passing turns."));
            _logger.LogTrace("[Help] Added tip: Exhausted Stamina");
        }

        // High Stress (>80%)
        if (character.PsychicStress >= 80)
        {
            tips.Add(HelpTip.Critical("Breaking Point",
                "Stress critical! Breaking Point imminent. Seek [cyan]Sanctuary[/]."));
            _logger.LogTrace("[Help] Added tip: Breaking Point");
        }
        // Medium Stress (60-80%)
        else if (character.PsychicStress >= 60)
        {
            tips.Add(HelpTip.Warning("High Stress",
                "Stress elevated. Avoid further horror. Rest at anchor when able."));
            _logger.LogTrace("[Help] Added tip: High Stress");
        }

        // High Corruption (>80%)
        if (character.Corruption >= 80)
        {
            tips.Add(HelpTip.Critical("Terminal Blight",
                "Corruption critical! Terminal Error approaches. Seek purification."));
            _logger.LogTrace("[Help] Added tip: Terminal Blight");
        }
    }

    /// <summary>
    /// Analyzes combatant resources during combat and adds relevant tips.
    /// </summary>
    private void AnalyzeCombatResources(Combatant player, List<HelpTip> tips)
    {
        // Low HP (<25%)
        if (player.CurrentHp <= player.MaxHp / 4)
        {
            tips.Add(HelpTip.Critical("Critical HP",
                "HP critical! Use healing items or consider fleeing."));
            _logger.LogTrace("[Help] Added tip: Critical HP");
        }

        // Low Stamina (<20%)
        if (player.CurrentStamina <= player.MaxStamina / 5)
        {
            tips.Add(HelpTip.Warning("Low Stamina",
                "Stamina nearly depleted. Use [yellow]Wait[/] to recover or flee."));
            _logger.LogTrace("[Help] Added tip: Low Stamina");
        }

        // High Stress (>80%)
        if (player.CurrentStress >= 80)
        {
            tips.Add(HelpTip.Critical("Stressed",
                "Breaking Point imminent! Finish combat quickly or flee."));
            _logger.LogTrace("[Help] Added tip: Stressed");
        }
    }

    /// <summary>
    /// Analyzes enemy tags and adds tactical tips if WITS check passes.
    /// </summary>
    private void AnalyzeEnemyTactics(Combatant enemy, int playerWits, List<HelpTip> tips)
    {
        // Check WITS threshold OR Analyzed status
        var isAnalyzed = enemy.StatusEffects.Any(e => e.Type == StatusEffectType.Analyzed);
        var canSeeTactics = playerWits >= WitsThreshold || isAnalyzed;

        if (!canSeeTactics)
        {
            _logger.LogTrace("[Help] WITS too low ({Wits}) and enemy not Analyzed, skipping tactics for {Enemy}",
                playerWits, enemy.Name);
            return;
        }

        // Check enemy tags for tactical advice
        foreach (var tag in enemy.Tags)
        {
            var tip = GetEnemyTagTip(tag, enemy.Name);
            if (tip != null)
            {
                // Avoid duplicate tips for same tag type
                if (!tips.Any(t => t.Title == tip.Title))
                {
                    tips.Add(tip);
                    _logger.LogTrace("[Help] Enemy tag tip: {Tag} -> {Title}", tag, tip.Title);
                }
            }
        }

        // Check armor level
        if (enemy.ArmorSoak >= 5)
        {
            var armorTip = HelpTip.Tactical("Armored",
                $"[yellow]{enemy.Name}[/] has heavy armor. Use [blue]Sunder[/] or bypass with magic.");
            if (!tips.Any(t => t.Title == "Armored"))
            {
                tips.Add(armorTip);
                _logger.LogTrace("[Help] Enemy armor tip for {Enemy}", enemy.Name);
            }
        }
    }

    /// <summary>
    /// Gets the tactical tip for a given enemy tag.
    /// </summary>
    private static HelpTip? GetEnemyTagTip(string tag, string enemyName)
    {
        return tag.ToLowerInvariant() switch
        {
            "mechanical" => HelpTip.Tactical("Mechanical",
                $"[yellow]{enemyName}[/] is mechanical. Weak to [blue]Shock[/] damage."),

            "flying" => HelpTip.Tactical("Flying",
                $"[yellow]{enemyName}[/] is airborne. Melee attacks have reduced accuracy."),

            "undead" => HelpTip.Tactical("Undead",
                $"[yellow]{enemyName}[/] is undead. Immune to poison, weak to [yellow]fire[/]."),

            "beast" => HelpTip.Tactical("Beast",
                $"[yellow]{enemyName}[/] is a beast. May flee when wounded."),

            "corrupted" => HelpTip.Tactical("Corrupted",
                $"[yellow]{enemyName}[/] is blight-touched. Attacks may inflict corruption."),

            "swarm" => HelpTip.Tactical("Swarm",
                $"[yellow]{enemyName}[/] is a swarm. Use area attacks for bonus damage."),

            "armored" => HelpTip.Tactical("Armored",
                $"[yellow]{enemyName}[/] has armor. Use [blue]Sunder[/] or bypass with magic."),

            _ => null
        };
    }
}
