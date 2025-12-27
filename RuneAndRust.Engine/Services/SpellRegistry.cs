using System.Collections.Generic;
using RuneAndRust.Core.Models.Magic;

namespace RuneAndRust.Engine.Services
{
    public interface ISpellRegistry
    {
        Spell GetSpell(string spellId);
        IEnumerable<Spell> GetAllSpells();
        void RegisterSpell(Spell spell);
    }

    public class SpellRegistry : ISpellRegistry
    {
        private readonly Dictionary<string, Spell> _spells = new Dictionary<string, Spell>();

        public Spell GetSpell(string spellId)
        {
            if (_spells.TryGetValue(spellId, out var spell))
            {
                return spell;
            }
            return null;
        }

        public IEnumerable<Spell> GetAllSpells()
        {
            return _spells.Values;
        }

        public void RegisterSpell(Spell spell)
        {
            if (!_spells.ContainsKey(spell.Id))
            {
                _spells.Add(spell.Id, spell);
            }
        }
    }
}
