namespace RuneAndRust.Domain.Extensions;

using RuneAndRust.Domain.Enums;

/// <summary>
/// Extension methods for <see cref="MyrkengrAbility"/> enum.
/// </summary>
/// <remarks>
/// Provides display names, ability types, descriptions, and usage information
/// for all Myrk-gengr specialization abilities.
/// </remarks>
public static class MyrkengrAbilityExtensions
{
    // ═══════════════════════════════════════════════════════════════════════════
    // DISPLAY METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the display name for this ability.
    /// </summary>
    /// <param name="ability">The ability.</param>
    /// <returns>Human-readable display name with brackets.</returns>
    public static string GetDisplayName(this MyrkengrAbility ability)
    {
        return ability switch
        {
            MyrkengrAbility.SlipIntoShadow => "[Slip into Shadow]",
            MyrkengrAbility.GhostlyForm => "[Ghostly Form]",
            MyrkengrAbility.CloakTheParty => "[Cloak the Party]",
            MyrkengrAbility.OneWithTheStatic => "[One with the Static]",
            _ => "[Unknown Myrk-gengr Ability]"
        };
    }

    /// <summary>
    /// Gets the ability type for this ability.
    /// </summary>
    /// <param name="ability">The ability.</param>
    /// <returns>The ability activation type.</returns>
    public static SpecializationAbilityType GetAbilityType(this MyrkengrAbility ability)
    {
        return ability switch
        {
            MyrkengrAbility.SlipIntoShadow => SpecializationAbilityType.Triggered,
            MyrkengrAbility.GhostlyForm => SpecializationAbilityType.Reactive,
            MyrkengrAbility.CloakTheParty => SpecializationAbilityType.Active,
            MyrkengrAbility.OneWithTheStatic => SpecializationAbilityType.Passive,
            _ => SpecializationAbilityType.Passive
        };
    }

    /// <summary>
    /// Gets the description of this ability's effect.
    /// </summary>
    /// <param name="ability">The ability.</param>
    /// <returns>Description of the ability effect.</returns>
    public static string GetDescription(this MyrkengrAbility ability)
    {
        return ability switch
        {
            MyrkengrAbility.SlipIntoShadow =>
                "Enter [Hidden] without using an action when entering shadows.",
            MyrkengrAbility.GhostlyForm =>
                "Once per encounter, stay [Hidden] after making an attack.",
            MyrkengrAbility.CloakTheParty =>
                "Grant party members within 30 ft +2d10 on Passive Stealth (concentration).",
            MyrkengrAbility.OneWithTheStatic =>
                "Automatically enter [Hidden] in [Psychic Resonance] zones.",
            _ => "Unknown ability effect."
        };
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // USAGE METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets whether this ability has limited encounter uses.
    /// </summary>
    /// <param name="ability">The ability.</param>
    /// <returns>True if the ability has encounter use limits.</returns>
    public static bool HasEncounterLimit(this MyrkengrAbility ability)
    {
        return ability == MyrkengrAbility.GhostlyForm;
    }

    /// <summary>
    /// Gets the encounter use limit for this ability.
    /// </summary>
    /// <param name="ability">The ability.</param>
    /// <returns>Number of encounter uses, or -1 for unlimited.</returns>
    public static int GetEncounterUses(this MyrkengrAbility ability)
    {
        return ability switch
        {
            MyrkengrAbility.GhostlyForm => 1,
            _ => -1 // Unlimited
        };
    }

    /// <summary>
    /// Gets whether this ability requires concentration.
    /// </summary>
    /// <param name="ability">The ability.</param>
    /// <returns>True if the ability requires concentration.</returns>
    public static bool RequiresConcentration(this MyrkengrAbility ability)
    {
        return ability == MyrkengrAbility.CloakTheParty;
    }

    /// <summary>
    /// Gets whether this ability requires an action to use.
    /// </summary>
    /// <param name="ability">The ability.</param>
    /// <returns>True if the ability costs an action.</returns>
    public static bool RequiresAction(this MyrkengrAbility ability)
    {
        return ability == MyrkengrAbility.CloakTheParty;
    }

    /// <summary>
    /// Gets the range of the ability effect in feet.
    /// </summary>
    /// <param name="ability">The ability.</param>
    /// <returns>The effect range, or 0 for self-only.</returns>
    public static int GetRangeFeet(this MyrkengrAbility ability)
    {
        return ability switch
        {
            MyrkengrAbility.CloakTheParty => 30,
            _ => 0 // Self only
        };
    }

    /// <summary>
    /// Gets the dice bonus provided by this ability.
    /// </summary>
    /// <param name="ability">The ability.</param>
    /// <returns>The dice bonus, or 0 if no bonus.</returns>
    public static int GetDiceBonus(this MyrkengrAbility ability)
    {
        return ability switch
        {
            MyrkengrAbility.CloakTheParty => 2, // +2d10
            _ => 0
        };
    }

    /// <summary>
    /// Gets the string ID for configuration lookups.
    /// </summary>
    /// <param name="ability">The ability.</param>
    /// <returns>The kebab-case ability ID.</returns>
    public static string GetAbilityId(this MyrkengrAbility ability)
    {
        return ability switch
        {
            MyrkengrAbility.SlipIntoShadow => "slip-into-shadow",
            MyrkengrAbility.GhostlyForm => "ghostly-form",
            MyrkengrAbility.CloakTheParty => "cloak-the-party",
            MyrkengrAbility.OneWithTheStatic => "one-with-the-static",
            _ => "unknown"
        };
    }
}
