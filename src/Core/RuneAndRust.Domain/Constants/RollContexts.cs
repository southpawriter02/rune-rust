namespace RuneAndRust.Domain.Constants;

/// <summary>
/// Standard context strings for categorizing dice rolls.
/// </summary>
/// <remarks>
/// <para>
/// Context strings use a hierarchical format with colon separators:
/// [Category]:[Subcategory]
/// </para>
/// <para>
/// This allows filtering by:
/// <list type="bullet">
///   <item><description>Full context: "Combat:Attack" returns only attack rolls</description></item>
///   <item><description>Category prefix: "Combat:" returns all combat rolls</description></item>
///   <item><description>Empty prefix: "" returns all rolls</description></item>
/// </list>
/// </para>
/// <para>
/// Use the helper methods <see cref="Skill"/>, <see cref="Dialogue"/>, and
/// <see cref="Crafting"/> to generate context strings for dynamic values.
/// </para>
/// </remarks>
public static class RollContexts
{
    #region Combat Contexts

    /// <summary>
    /// Attack roll to determine if a hit lands.
    /// </summary>
    /// <remarks>
    /// Uses success-counting to determine hit quality.
    /// </remarks>
    public const string CombatAttack = "Combat:Attack";

    /// <summary>
    /// Damage roll after a successful hit.
    /// </summary>
    /// <remarks>
    /// Uses sum-based mechanics (RawTotal) for damage value.
    /// </remarks>
    public const string CombatDamage = "Combat:Damage";

    /// <summary>
    /// Defense roll to avoid or mitigate an attack.
    /// </summary>
    public const string CombatDefense = "Combat:Defense";

    /// <summary>
    /// Initiative roll for turn order determination.
    /// </summary>
    public const string CombatInitiative = "Combat:Initiative";

    /// <summary>
    /// Resolution roll for contested combat actions.
    /// </summary>
    /// <remarks>
    /// Used for grapple, shove, and similar maneuvers.
    /// </remarks>
    public const string CombatResolve = "Combat:Resolve";

    /// <summary>
    /// Prefix for all combat-related rolls.
    /// </summary>
    public const string CombatPrefix = "Combat:";

    #endregion

    #region Skill Contexts

    /// <summary>
    /// Prefix for all skill check rolls.
    /// </summary>
    /// <remarks>
    /// Combined with skill ID: "Skill:Acrobatics", "Skill:Stealth", etc.
    /// </remarks>
    public const string SkillPrefix = "Skill:";

    /// <summary>
    /// Generates a skill context string for the given skill ID.
    /// </summary>
    /// <param name="skillId">The skill identifier (e.g., "Acrobatics", "Stealth").</param>
    /// <returns>A context string in format "Skill:{skillId}".</returns>
    /// <example>
    /// RollContexts.Skill("Acrobatics") returns "Skill:Acrobatics"
    /// </example>
    public static string Skill(string skillId) => $"{SkillPrefix}{skillId}";

    #endregion

    #region Dialogue Contexts

    /// <summary>
    /// Prefix for all dialogue/social rolls.
    /// </summary>
    public const string DialoguePrefix = "Dialogue:";

    /// <summary>
    /// Persuasion dialogue roll.
    /// </summary>
    public const string DialoguePersuasion = "Dialogue:Persuasion";

    /// <summary>
    /// Intimidation dialogue roll.
    /// </summary>
    public const string DialogueIntimidation = "Dialogue:Intimidation";

    /// <summary>
    /// Deception dialogue roll.
    /// </summary>
    public const string DialogueDeception = "Dialogue:Deception";

    /// <summary>
    /// Insight (detecting lies/motives) dialogue roll.
    /// </summary>
    public const string DialogueInsight = "Dialogue:Insight";

    /// <summary>
    /// Generates a dialogue context string for the given type.
    /// </summary>
    /// <param name="type">The dialogue type (e.g., "Persuasion", "Barter").</param>
    /// <returns>A context string in format "Dialogue:{type}".</returns>
    /// <example>
    /// RollContexts.Dialogue("Barter") returns "Dialogue:Barter"
    /// </example>
    public static string Dialogue(string type) => $"{DialoguePrefix}{type}";

    #endregion

    #region Crafting Contexts

    /// <summary>
    /// Prefix for all crafting rolls.
    /// </summary>
    public const string CraftingPrefix = "Crafting:";

    /// <summary>
    /// Forge crafting roll.
    /// </summary>
    public const string CraftingForge = "Crafting:Forge";

    /// <summary>
    /// Alchemy crafting roll.
    /// </summary>
    public const string CraftingAlchemy = "Crafting:Alchemy";

    /// <summary>
    /// Enchantment crafting roll.
    /// </summary>
    public const string CraftingEnchant = "Crafting:Enchant";

    /// <summary>
    /// Generates a crafting context string for the given craft type.
    /// </summary>
    /// <param name="craftType">The craft type (e.g., "Forge", "Repair").</param>
    /// <returns>A context string in format "Crafting:{craftType}".</returns>
    /// <example>
    /// RollContexts.Crafting("Repair") returns "Crafting:Repair"
    /// </example>
    public static string Crafting(string craftType) => $"{CraftingPrefix}{craftType}";

    #endregion

    #region Special Contexts

    /// <summary>
    /// Prefix for contested check rolls.
    /// </summary>
    public const string ContestedPrefix = "Contested:";

    /// <summary>
    /// Generates a contested check context.
    /// </summary>
    /// <param name="checkType">Type of contested check.</param>
    /// <returns>A context string in format "Contested:{checkType}".</returns>
    public static string Contested(string checkType) => $"{ContestedPrefix}{checkType}";

    /// <summary>
    /// Prefix for extended check rolls.
    /// </summary>
    public const string ExtendedPrefix = "Extended:";

    /// <summary>
    /// Generates an extended check round context.
    /// </summary>
    /// <param name="checkType">Type of extended check.</param>
    /// <returns>A context string in format "Extended:{checkType}".</returns>
    public static string Extended(string checkType) => $"{ExtendedPrefix}{checkType}";

    /// <summary>
    /// Prefix for random event rolls.
    /// </summary>
    public const string RandomPrefix = "Random:";

    /// <summary>
    /// Random encounter roll.
    /// </summary>
    public const string RandomEncounter = "Random:Encounter";

    /// <summary>
    /// Random weather roll.
    /// </summary>
    public const string RandomWeather = "Random:Weather";

    /// <summary>
    /// Random loot roll.
    /// </summary>
    public const string RandomLoot = "Random:Loot";

    /// <summary>
    /// Default context for unspecified rolls.
    /// </summary>
    public const string Default = "General";

    #endregion

    #region Utility Methods

    /// <summary>
    /// Determines if a context string matches a given prefix.
    /// </summary>
    /// <param name="context">The context string to check.</param>
    /// <param name="prefix">The prefix to match against.</param>
    /// <returns>True if context starts with prefix (case-insensitive).</returns>
    /// <example>
    /// MatchesPrefix("Combat:Attack", "Combat:") returns true
    /// MatchesPrefix("Skill:Stealth", "Combat:") returns false
    /// </example>
    public static bool MatchesPrefix(string context, string prefix)
    {
        if (string.IsNullOrEmpty(prefix))
            return true;

        return context.StartsWith(prefix, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Extracts the category from a context string.
    /// </summary>
    /// <param name="context">The context string.</param>
    /// <returns>The category portion before the colon, or the full string if no colon.</returns>
    /// <example>
    /// GetCategory("Combat:Attack") returns "Combat"
    /// GetCategory("General") returns "General"
    /// </example>
    public static string GetCategory(string context)
    {
        var colonIndex = context.IndexOf(':');
        return colonIndex > 0 ? context[..colonIndex] : context;
    }

    /// <summary>
    /// Extracts the subcategory from a context string.
    /// </summary>
    /// <param name="context">The context string.</param>
    /// <returns>The subcategory portion after the colon, or empty string if no colon.</returns>
    /// <example>
    /// GetSubcategory("Combat:Attack") returns "Attack"
    /// GetSubcategory("General") returns ""
    /// </example>
    public static string GetSubcategory(string context)
    {
        var colonIndex = context.IndexOf(':');
        return colonIndex > 0 && colonIndex < context.Length - 1
            ? context[(colonIndex + 1)..]
            : string.Empty;
    }

    /// <summary>
    /// Validates that a context string follows the expected format.
    /// </summary>
    /// <param name="context">The context string to validate.</param>
    /// <returns>True if the context is valid (non-empty, no leading/trailing whitespace).</returns>
    public static bool IsValidContext(string context)
    {
        if (string.IsNullOrWhiteSpace(context))
            return false;

        return context == context.Trim();
    }

    #endregion
}
