using RuneAndRust.Core.Models.Magic;

namespace RuneAndRust.Engine.Services;

public class MagicService : IMagicService
{
    private readonly ILogger<MagicService> _logger;

    public MagicService(ILogger<MagicService> logger)
    {
        _logger = logger;
    }

    public CastResult Cast(Character caster, Spell spell, string targetContext = "Unspecified")
    {
        _logger.LogTrace("Attempting to cast {SpellName} for {CasterName} (Cost: {Cost}, Target: {TargetContext})",
            spell.Name, caster.Name, spell.Cost, targetContext);

        if (caster.Aether.Current < spell.Cost)
        {
            _logger.LogWarning("Cast failed: Insufficient Aether for {CasterName} (Has: {CurrentAether}, Needs: {Cost})",
                caster.Name, caster.Aether.Current, spell.Cost);
            return CastResult.InsufficientAether;
        }

        caster.Aether.Modify(-spell.Cost);
        _logger.LogTrace("Deducted {Cost} Aether from {CasterName}. New Balance: {CurrentAether}",
            spell.Cost, caster.Name, caster.Aether.Current);

        if (spell.CastTime == CastTimeType.Instant)
        {
            // TODO: Apply effect
            _logger.LogDebug("Instant cast {SpellName} successful for {CasterName} on {TargetContext}",
                spell.Name, caster.Name, targetContext);
            return CastResult.Success;
        }
        else
        {
            // TODO: Implement Chanting
            _logger.LogDebug("Chant started for {SpellName} by {CasterName} ({Turns} turns)",
                spell.Name, caster.Name, spell.ChantTurns);
            return CastResult.StartedChant;
        }
    }
}
