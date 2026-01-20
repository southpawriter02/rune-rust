using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Infrastructure.Persistence.Seeders;

/// <summary>
/// Seeds Tier 2 thematic modifiers for biome-specific variations.
/// </summary>
public static class ThematicModifierSeeder
{
    /// <summary>
    /// Gets all thematic modifiers indexed by biome.
    /// </summary>
    public static IReadOnlyDictionary<Biome, ThematicModifier> GetAllModifiers()
    {
        return new Dictionary<Biome, ThematicModifier>
        {
            [Biome.TheRoots] = new ThematicModifier
            {
                Name = "Rusted",
                PrimaryBiome = Biome.TheRoots,
                Adjective = "corroded",
                DetailFragment = "shows centuries of oxidation and decay",
                ColorPalette = "rust-brown-grey",
                AmbientSounds = ["creaking metal", "dripping water", "groaning pipes"],
                HpMultiplier = 0.7,
                IsBrittle = true
            },

            [Biome.Muspelheim] = new ThematicModifier
            {
                Name = "Scorched",
                PrimaryBiome = Biome.Muspelheim,
                Adjective = "scorched",
                DetailFragment = "radiates intense, oppressive heat",
                ColorPalette = "orange-red-black",
                AmbientSounds = ["crackling flames", "hissing steam", "rumbling magma"],
                DamagePerTurn = 2,
                DamageType = "fire"
            },

            [Biome.Niflheim] = new ThematicModifier
            {
                Name = "Frozen",
                PrimaryBiome = Biome.Niflheim,
                Adjective = "ice-covered",
                DetailFragment = "is encased in thick, ancient frost",
                ColorPalette = "white-blue-silver",
                AmbientSounds = ["howling wind", "cracking ice", "crystalline chimes"],
                IsSlippery = true,
                DamagePerTurn = 1,
                DamageType = "cold"
            },

            [Biome.Alfheim] = new ThematicModifier
            {
                Name = "Crystalline",
                PrimaryBiome = Biome.Alfheim,
                Adjective = "crystalline",
                DetailFragment = "defies physics with impossible formations",
                ColorPalette = "prismatic-iridescent",
                AmbientSounds = ["harmonic resonance", "tinkling crystals", "ethereal whispers"],
                IsLightSource = true,
                CanDazzle = true
            },

            [Biome.Jotunheim] = new ThematicModifier
            {
                Name = "Monolithic",
                PrimaryBiome = Biome.Jotunheim,
                Adjective = "monolithic",
                DetailFragment = "towers at a massive, inhuman scale",
                ColorPalette = "grey-stone-shadow",
                AmbientSounds = ["echoing footsteps", "rumbling stone", "distant thunder"],
                ScaleMultiplier = 2.0
            },

            [Biome.Citadel] = new ThematicModifier
            {
                Name = "Ancient",
                PrimaryBiome = Biome.Citadel,
                Adjective = "weathered",
                DetailFragment = "bears the weight of forgotten ages",
                ColorPalette = "grey-brown-faded",
                AmbientSounds = ["distant echoes", "settling stone", "whispered memories"]
            },

            [Biome.Surface] = new ThematicModifier
            {
                Name = "Blighted",
                PrimaryBiome = Biome.Surface,
                Adjective = "blighted",
                DetailFragment = "shows signs of the Glitch's corruption",
                ColorPalette = "sickly-green-purple",
                AmbientSounds = ["static crackle", "warped sounds", "digital screech"],
                DamagePerTurn = 1,
                DamageType = "corruption"
            }
        };
    }

    /// <summary>
    /// Gets a specific modifier by biome, with fallback to Citadel.
    /// </summary>
    public static ThematicModifier GetModifier(Biome biome)
    {
        var modifiers = GetAllModifiers();
        return modifiers.TryGetValue(biome, out var modifier)
            ? modifier
            : modifiers[Biome.Citadel];
    }
}
