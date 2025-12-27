using System;
using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Models.Magic;
using RuneAndRust.Core.Enums;

namespace RuneAndRust.Engine.Services
{
    public interface IMagicService
    {
        CastResult Cast(Character caster, Spell spell, Character target);
        Spell GetSpell(string spellId);
    }

    public enum CastResult
    {
        Success,
        InsufficientAether,
        StartedChant,
        InvalidTarget,
        UnknownSpell
    }

    public class MagicService : IMagicService
    {
        private readonly ISpellRegistry _spellRegistry;

        public MagicService(ISpellRegistry spellRegistry)
        {
            _spellRegistry = spellRegistry;
        }

        public CastResult Cast(Character caster, Spell spell, Character target)
        {
            if (caster.Aether.Current < spell.Cost)
            {
                return CastResult.InsufficientAether;
            }

            caster.Aether.Modify(-spell.Cost);

            // TODO: Apply flux mechanics if applicable

            if (spell.CastTimeType == CastTimeType.Instant)
            {
                // TODO: Apply effect using EffectService/EffectScriptExecutor
                // For now, we assume success as effect application is part of v0.4.4c
                return CastResult.Success;
            }
            else
            {
                // Start Chant (v0.4.4b)
                return CastResult.StartedChant;
            }
        }

        public Spell GetSpell(string spellId)
        {
            return _spellRegistry.GetSpell(spellId);
        }
    }
}
