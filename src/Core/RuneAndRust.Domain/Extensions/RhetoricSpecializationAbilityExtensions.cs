// ------------------------------------------------------------------------------
// <copyright file="RhetoricSpecializationAbilityExtensions.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Extension methods for the RhetoricSpecializationAbility enum providing
// display names, descriptions, archetype mappings, and effect parameters.
// Part of v0.15.3i Specialization Integration implementation.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Domain.Extensions;

using RuneAndRust.Domain.Enums;

/// <summary>
/// Extension methods for the <see cref="RhetoricSpecializationAbility"/> enum providing
/// display names, descriptions, archetype mappings, and effect parameters.
/// </summary>
public static class RhetoricSpecializationAbilityExtensions
{
    /// <summary>
    /// Gets the display name for UI presentation.
    /// </summary>
    /// <param name="ability">The rhetoric specialization ability.</param>
    /// <returns>Human-readable ability name.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when an unknown ability is provided.
    /// </exception>
    public static string GetDisplayName(this RhetoricSpecializationAbility ability)
    {
        return ability switch
        {
            RhetoricSpecializationAbility.VoiceOfReason => "Voice of Reason",
            RhetoricSpecializationAbility.ScholarlyAuthority => "Scholarly Authority",
            RhetoricSpecializationAbility.InspiringWords => "Inspiring Words",
            RhetoricSpecializationAbility.SagaOfHeroes => "Saga of Heroes",
            RhetoricSpecializationAbility.SilverTongue => "Silver Tongue",
            RhetoricSpecializationAbility.SniffOutLies => "Sniff Out Lies",
            RhetoricSpecializationAbility.MaintainCover => "Maintain Cover",
            RhetoricSpecializationAbility.ForgeDocuments => "Forge Documents",
            _ => throw new ArgumentOutOfRangeException(
                nameof(ability),
                ability,
                "Unknown rhetoric specialization ability")
        };
    }

    /// <summary>
    /// Gets a short description of the ability.
    /// </summary>
    /// <param name="ability">The rhetoric specialization ability.</param>
    /// <returns>Brief description suitable for tooltips.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when an unknown ability is provided.
    /// </exception>
    public static string GetDescription(this RhetoricSpecializationAbility ability)
    {
        return ability switch
        {
            RhetoricSpecializationAbility.VoiceOfReason =>
                "Failed persuasion doesn't lock dialogue options",
            RhetoricSpecializationAbility.ScholarlyAuthority =>
                "+2d10 when discussing lore/history topics",
            RhetoricSpecializationAbility.InspiringWords =>
                "Grant allies +1d10 on social checks through oratory",
            RhetoricSpecializationAbility.SagaOfHeroes =>
                "Reduce party Stress through storytelling during rest",
            RhetoricSpecializationAbility.SilverTongue =>
                "Auto-succeed negotiation checks with DC ≤ 12",
            RhetoricSpecializationAbility.SniffOutLies =>
                "+2d10 when detecting NPC deception",
            RhetoricSpecializationAbility.MaintainCover =>
                "+2d10 and reduced stress when cover is challenged",
            RhetoricSpecializationAbility.ForgeDocuments =>
                "Create forged documents as evidence/credentials",
            _ => throw new ArgumentOutOfRangeException(
                nameof(ability),
                ability,
                "Unknown rhetoric specialization ability")
        };
    }

    /// <summary>
    /// Gets the archetype ID that has access to this ability.
    /// </summary>
    /// <param name="ability">The rhetoric specialization ability.</param>
    /// <returns>The archetype ID string.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when an unknown ability is provided.
    /// </exception>
    public static string GetArchetypeId(this RhetoricSpecializationAbility ability)
    {
        return ability switch
        {
            RhetoricSpecializationAbility.VoiceOfReason => "thul",
            RhetoricSpecializationAbility.ScholarlyAuthority => "thul",
            RhetoricSpecializationAbility.InspiringWords => "skald",
            RhetoricSpecializationAbility.SagaOfHeroes => "skald",
            RhetoricSpecializationAbility.SilverTongue => "kupmadr",
            RhetoricSpecializationAbility.SniffOutLies => "kupmadr",
            RhetoricSpecializationAbility.MaintainCover => "myrk-gengr",
            RhetoricSpecializationAbility.ForgeDocuments => "myrk-gengr",
            _ => throw new ArgumentOutOfRangeException(
                nameof(ability),
                ability,
                "Unknown rhetoric specialization ability")
        };
    }

    /// <summary>
    /// Gets the human-readable archetype name for this ability.
    /// </summary>
    /// <param name="ability">The rhetoric specialization ability.</param>
    /// <returns>The archetype display name.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when an unknown ability is provided.
    /// </exception>
    public static string GetArchetypeName(this RhetoricSpecializationAbility ability)
    {
        return ability switch
        {
            RhetoricSpecializationAbility.VoiceOfReason => "Thul (Scholar)",
            RhetoricSpecializationAbility.ScholarlyAuthority => "Thul (Scholar)",
            RhetoricSpecializationAbility.InspiringWords => "Skald (Bard)",
            RhetoricSpecializationAbility.SagaOfHeroes => "Skald (Bard)",
            RhetoricSpecializationAbility.SilverTongue => "Kupmaðr (Merchant)",
            RhetoricSpecializationAbility.SniffOutLies => "Kupmaðr (Merchant)",
            RhetoricSpecializationAbility.MaintainCover => "Myrk-gengr (Infiltrator)",
            RhetoricSpecializationAbility.ForgeDocuments => "Myrk-gengr (Infiltrator)",
            _ => throw new ArgumentOutOfRangeException(
                nameof(ability),
                ability,
                "Unknown rhetoric specialization ability")
        };
    }

    /// <summary>
    /// Gets the dice bonus provided by this ability (if any).
    /// </summary>
    /// <param name="ability">The rhetoric specialization ability.</param>
    /// <returns>The number of bonus dice, or 0 if the ability doesn't provide a dice bonus.</returns>
    public static int GetDiceBonus(this RhetoricSpecializationAbility ability)
    {
        return ability switch
        {
            RhetoricSpecializationAbility.ScholarlyAuthority => 2,
            RhetoricSpecializationAbility.InspiringWords => 1,
            RhetoricSpecializationAbility.SniffOutLies => 2,
            RhetoricSpecializationAbility.MaintainCover => 2,
            _ => 0
        };
    }

    /// <summary>
    /// Gets whether this ability modifies the dice pool.
    /// </summary>
    /// <param name="ability">The rhetoric specialization ability.</param>
    /// <returns>True if the ability adds bonus dice to the pool.</returns>
    public static bool ModifiesDicePool(this RhetoricSpecializationAbility ability)
    {
        return ability.GetDiceBonus() > 0;
    }

    /// <summary>
    /// Gets whether this ability modifies the check outcome.
    /// </summary>
    /// <param name="ability">The rhetoric specialization ability.</param>
    /// <returns>True if the ability modifies outcomes (e.g., prevents locking).</returns>
    public static bool ModifiesOutcome(this RhetoricSpecializationAbility ability)
    {
        return ability switch
        {
            RhetoricSpecializationAbility.VoiceOfReason => true,
            RhetoricSpecializationAbility.MaintainCover => true, // Fumble downgrade
            _ => false
        };
    }

    /// <summary>
    /// Gets whether this ability can bypass checks entirely.
    /// </summary>
    /// <param name="ability">The rhetoric specialization ability.</param>
    /// <returns>True if the ability can auto-succeed under certain conditions.</returns>
    public static bool CanBypassCheck(this RhetoricSpecializationAbility ability)
    {
        return ability == RhetoricSpecializationAbility.SilverTongue;
    }

    /// <summary>
    /// Gets whether this ability affects party members.
    /// </summary>
    /// <param name="ability">The rhetoric specialization ability.</param>
    /// <returns>True if the ability has party-wide effects.</returns>
    public static bool AffectsParty(this RhetoricSpecializationAbility ability)
    {
        return ability switch
        {
            RhetoricSpecializationAbility.InspiringWords => true,
            RhetoricSpecializationAbility.SagaOfHeroes => true,
            _ => false
        };
    }

    /// <summary>
    /// Gets whether this ability creates an asset.
    /// </summary>
    /// <param name="ability">The rhetoric specialization ability.</param>
    /// <returns>True if the ability creates a usable asset (e.g., forged document).</returns>
    public static bool CreatesAsset(this RhetoricSpecializationAbility ability)
    {
        return ability == RhetoricSpecializationAbility.ForgeDocuments;
    }

    /// <summary>
    /// Gets whether this ability requires a skill check to activate.
    /// </summary>
    /// <param name="ability">The rhetoric specialization ability.</param>
    /// <returns>True if the ability requires a skill check.</returns>
    public static bool RequiresActivationCheck(this RhetoricSpecializationAbility ability)
    {
        return ability switch
        {
            RhetoricSpecializationAbility.InspiringWords => true,   // DC 12 Rhetoric
            RhetoricSpecializationAbility.SagaOfHeroes => true,     // DC 10 Rhetoric
            RhetoricSpecializationAbility.ForgeDocuments => true,   // Variable DC
            _ => false
        };
    }

    /// <summary>
    /// Gets the activation DC for abilities that require a skill check.
    /// </summary>
    /// <param name="ability">The rhetoric specialization ability.</param>
    /// <returns>The DC required to activate, or 0 if no check is needed.</returns>
    public static int GetActivationDc(this RhetoricSpecializationAbility ability)
    {
        return ability switch
        {
            RhetoricSpecializationAbility.InspiringWords => 12,
            RhetoricSpecializationAbility.SagaOfHeroes => 10,
            // ForgeDocuments DC varies by document type
            _ => 0
        };
    }

    /// <summary>
    /// Gets the auto-success DC threshold for abilities that can bypass checks.
    /// </summary>
    /// <param name="ability">The rhetoric specialization ability.</param>
    /// <returns>The DC threshold, or 0 if the ability doesn't auto-succeed.</returns>
    public static int GetAutoSuccessThreshold(this RhetoricSpecializationAbility ability)
    {
        return ability switch
        {
            RhetoricSpecializationAbility.SilverTongue => 12,
            _ => 0
        };
    }

    /// <summary>
    /// Gets the stress reduction provided by this ability (if any).
    /// </summary>
    /// <param name="ability">The rhetoric specialization ability.</param>
    /// <returns>The base stress reduction, or 0 if the ability doesn't reduce stress.</returns>
    /// <remarks>
    /// For SagaOfHeroes, the actual reduction varies based on success tier (1-4).
    /// For MaintainCover, the reduction is always 1.
    /// </remarks>
    public static int GetBaseStressReduction(this RhetoricSpecializationAbility ability)
    {
        return ability switch
        {
            RhetoricSpecializationAbility.SagaOfHeroes => 1,  // Base; actual is 1-4
            RhetoricSpecializationAbility.MaintainCover => 1,
            _ => 0
        };
    }

    /// <summary>
    /// Gets the ability type for categorization.
    /// </summary>
    /// <param name="ability">The rhetoric specialization ability.</param>
    /// <returns>A string describing the ability type.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when an unknown ability is provided.
    /// </exception>
    public static string GetAbilityType(this RhetoricSpecializationAbility ability)
    {
        return ability switch
        {
            RhetoricSpecializationAbility.VoiceOfReason => "Outcome Modification",
            RhetoricSpecializationAbility.ScholarlyAuthority => "Dice Bonus",
            RhetoricSpecializationAbility.InspiringWords => "Party Buff",
            RhetoricSpecializationAbility.SagaOfHeroes => "Stress Relief",
            RhetoricSpecializationAbility.SilverTongue => "Auto-Success",
            RhetoricSpecializationAbility.SniffOutLies => "Dice Bonus",
            RhetoricSpecializationAbility.MaintainCover => "Composite",
            RhetoricSpecializationAbility.ForgeDocuments => "Asset Creation",
            _ => throw new ArgumentOutOfRangeException(
                nameof(ability),
                ability,
                "Unknown rhetoric specialization ability")
        };
    }

    /// <summary>
    /// Gets the icon or indicator character for this ability.
    /// </summary>
    /// <param name="ability">The rhetoric specialization ability.</param>
    /// <returns>A unicode character representing the ability.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when an unknown ability is provided.
    /// </exception>
    public static string GetIconHint(this RhetoricSpecializationAbility ability)
    {
        return ability switch
        {
            RhetoricSpecializationAbility.VoiceOfReason => "🗣",    // Speaking head
            RhetoricSpecializationAbility.ScholarlyAuthority => "📚", // Books
            RhetoricSpecializationAbility.InspiringWords => "✨",   // Sparkles
            RhetoricSpecializationAbility.SagaOfHeroes => "🎭",    // Drama masks
            RhetoricSpecializationAbility.SilverTongue => "💰",    // Money bag
            RhetoricSpecializationAbility.SniffOutLies => "👁",    // Eye
            RhetoricSpecializationAbility.MaintainCover => "🎭",   // Mask
            RhetoricSpecializationAbility.ForgeDocuments => "📜",  // Scroll
            _ => throw new ArgumentOutOfRangeException(
                nameof(ability),
                ability,
                "Unknown rhetoric specialization ability")
        };
    }

    /// <summary>
    /// Gets the color hint for UI presentation.
    /// </summary>
    /// <param name="ability">The rhetoric specialization ability.</param>
    /// <returns>A suggested color name for the ability.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when an unknown ability is provided.
    /// </exception>
    /// <remarks>
    /// Colors are grouped by archetype:
    /// <list type="bullet">
    ///   <item><description>Thul: Blue (wisdom, knowledge)</description></item>
    ///   <item><description>Skald: Gold (inspiration, performance)</description></item>
    ///   <item><description>Kupmaðr: Green (commerce, success)</description></item>
    ///   <item><description>Myrk-gengr: Purple (shadows, subterfuge)</description></item>
    /// </list>
    /// </remarks>
    public static string GetColorHint(this RhetoricSpecializationAbility ability)
    {
        return ability switch
        {
            RhetoricSpecializationAbility.VoiceOfReason => "Blue",
            RhetoricSpecializationAbility.ScholarlyAuthority => "Blue",
            RhetoricSpecializationAbility.InspiringWords => "Gold",
            RhetoricSpecializationAbility.SagaOfHeroes => "Gold",
            RhetoricSpecializationAbility.SilverTongue => "Green",
            RhetoricSpecializationAbility.SniffOutLies => "Green",
            RhetoricSpecializationAbility.MaintainCover => "Purple",
            RhetoricSpecializationAbility.ForgeDocuments => "Purple",
            _ => throw new ArgumentOutOfRangeException(
                nameof(ability),
                ability,
                "Unknown rhetoric specialization ability")
        };
    }

    /// <summary>
    /// Gets the flavor text shown when the ability triggers.
    /// </summary>
    /// <param name="ability">The rhetoric specialization ability.</param>
    /// <returns>Narrative flavor text for the ability.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when an unknown ability is provided.
    /// </exception>
    public static string GetFlavorText(this RhetoricSpecializationAbility ability)
    {
        return ability switch
        {
            RhetoricSpecializationAbility.VoiceOfReason =>
                "Your reasoned approach leaves the conversation open for future attempts.",
            RhetoricSpecializationAbility.ScholarlyAuthority =>
                "Your scholarly expertise lends weight to your words.",
            RhetoricSpecializationAbility.InspiringWords =>
                "Your rousing words fill your allies with confidence.",
            RhetoricSpecializationAbility.SagaOfHeroes =>
                "Your tale of heroism soothes troubled minds.",
            RhetoricSpecializationAbility.SilverTongue =>
                "Your mercantile expertise makes this negotiation trivial.",
            RhetoricSpecializationAbility.SniffOutLies =>
                "Your merchant's instincts detect something false.",
            RhetoricSpecializationAbility.MaintainCover =>
                "Your training allows you to stay calm and in character.",
            RhetoricSpecializationAbility.ForgeDocuments =>
                "You craft a convincing forgery.",
            _ => throw new ArgumentOutOfRangeException(
                nameof(ability),
                ability,
                "Unknown rhetoric specialization ability")
        };
    }

    /// <summary>
    /// Gets the abilities for a specific archetype ID.
    /// </summary>
    /// <param name="archetypeId">The archetype identifier.</param>
    /// <returns>Array of abilities available to the archetype, or empty if unknown.</returns>
    public static RhetoricSpecializationAbility[] GetAbilitiesForArchetype(string archetypeId)
    {
        return archetypeId?.ToLowerInvariant() switch
        {
            "thul" => new[]
            {
                RhetoricSpecializationAbility.VoiceOfReason,
                RhetoricSpecializationAbility.ScholarlyAuthority
            },
            "skald" => new[]
            {
                RhetoricSpecializationAbility.InspiringWords,
                RhetoricSpecializationAbility.SagaOfHeroes
            },
            "kupmadr" => new[]
            {
                RhetoricSpecializationAbility.SilverTongue,
                RhetoricSpecializationAbility.SniffOutLies
            },
            "myrk-gengr" => new[]
            {
                RhetoricSpecializationAbility.MaintainCover,
                RhetoricSpecializationAbility.ForgeDocuments
            },
            _ => Array.Empty<RhetoricSpecializationAbility>()
        };
    }
}
