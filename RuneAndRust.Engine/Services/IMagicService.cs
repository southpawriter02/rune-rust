using RuneAndRust.Core.Models;
using RuneAndRust.Core.Models.Magic;

namespace RuneAndRust.Engine.Services;

public interface IMagicService
{
    CastResult Cast(Character caster, Spell spell, string targetContext = "Unspecified");
}
