using NUnit.Framework;
using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Models.Magic;
using RuneAndRust.Engine.Services;

namespace RuneAndRust.Tests.Engine
{
    [TestFixture]
    public class MagicServiceTests
    {
        private MagicService _magicService;
        private Character _character;
        private ISpellRegistry _spellRegistry;

        [SetUp]
        public void Setup()
        {
            _spellRegistry = new SpellRegistry();
            _magicService = new MagicService(_spellRegistry);
            _character = new Character
            {
                Aether = new AetherPool(20)
            };
        }

        [Test]
        public void Cast_DeductsAether_OnSuccess()
        {
            var spell = new Spell
            {
                Id = "spell_test",
                Cost = 5,
                CastTimeType = CastTimeType.Instant
            };

            var result = _magicService.Cast(_character, spell, null);

            Assert.AreEqual(CastResult.Success, result);
            Assert.AreEqual(15, _character.Aether.Current);
        }

        [Test]
        public void Cast_Fails_WhenAetherLow()
        {
            var spell = new Spell
            {
                Id = "spell_expensive",
                Cost = 30,
                CastTimeType = CastTimeType.Instant
            };

            var result = _magicService.Cast(_character, spell, null);

            Assert.AreEqual(CastResult.InsufficientAether, result);
            Assert.AreEqual(20, _character.Aether.Current); // Should not change
        }
    }
}
