// ═══════════════════════════════════════════════════════════════════════════════
// IProficiencyEffectProvider.cs
// Interface defining the contract for accessing proficiency effect data.
// Version: 0.16.1a
// ═══════════════════════════════════════════════════════════════════════════════

using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Provides access to weapon proficiency effect data.
/// </summary>
/// <remarks>
/// <para>
/// This interface abstracts the source of proficiency effect data, allowing
/// it to be loaded from configuration files, databases, or hardcoded defaults.
/// </para>
/// <para>
/// Implementations should:
/// </para>
/// <list type="bullet">
///   <item><description>Cache loaded effects for performance</description></item>
///   <item><description>Validate that all proficiency levels have corresponding effect entries</description></item>
///   <item><description>Provide fallback behavior when configuration is missing</description></item>
///   <item><description>Log configuration loading and validation results</description></item>
/// </list>
/// <para>
/// Usage example:
/// </para>
/// <code>
/// // Inject via DI
/// public class CombatService(IProficiencyEffectProvider proficiencyProvider)
/// {
///     public int CalculateAttackBonus(WeaponProficiencyLevel level)
///     {
///         var effect = proficiencyProvider.GetEffect(level);
///         return effect.AttackModifier;
///     }
/// }
/// </code>
/// </remarks>
/// <seealso cref="ProficiencyEffect"/>
/// <seealso cref="WeaponProficiencyLevel"/>
/// <seealso cref="TechniqueAccess"/>
public interface IProficiencyEffectProvider
{
    // ═══════════════════════════════════════════════════════════════════════════
    // Core Effect Access Methods
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the combat effects for a specific proficiency level.
    /// </summary>
    /// <param name="level">The proficiency level to get effects for.</param>
    /// <returns>
    /// The <see cref="ProficiencyEffect"/> containing all combat modifiers and
    /// capability flags for the specified level.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when the effect for the specified level is not found in configuration.
    /// </exception>
    /// <remarks>
    /// <para>
    /// This is the primary method for retrieving proficiency effects. It returns
    /// a complete effect object that can be used to:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>Apply attack and damage modifiers</description></item>
    ///   <item><description>Check special property access</description></item>
    ///   <item><description>Determine available techniques</description></item>
    ///   <item><description>Display proficiency information in UI</description></item>
    /// </list>
    /// <para>
    /// Implementations should cache effects after initial load for performance.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var masterEffect = provider.GetEffect(WeaponProficiencyLevel.Master);
    /// Console.WriteLine($"Attack: {masterEffect.FormatAttackModifier()}"); // "+2"
    /// Console.WriteLine($"Damage: {masterEffect.FormatDamageModifier()}"); // "+1"
    /// </code>
    /// </example>
    ProficiencyEffect GetEffect(WeaponProficiencyLevel level);

    /// <summary>
    /// Gets effects for all proficiency levels.
    /// </summary>
    /// <returns>
    /// A read-only list of all proficiency effects, ordered by level ascending
    /// (NonProficient → Proficient → Expert → Master).
    /// </returns>
    /// <remarks>
    /// <para>
    /// Useful for:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>Populating UI elements showing all proficiency levels</description></item>
    ///   <item><description>Validating configuration completeness</description></item>
    ///   <item><description>Generating comparison displays</description></item>
    /// </list>
    /// <para>
    /// The returned list should always contain exactly 4 elements, one for each
    /// <see cref="WeaponProficiencyLevel"/>.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Display all proficiency levels in UI
    /// foreach (var effect in provider.GetAllEffects())
    /// {
    ///     Console.WriteLine($"{effect.DisplayName}: Atk {effect.FormatAttackModifier()}");
    /// }
    /// // Output:
    /// // Non-Proficient: Atk -3
    /// // Proficient: Atk +0
    /// // Expert: Atk +1
    /// // Master: Atk +2
    /// </code>
    /// </example>
    IReadOnlyList<ProficiencyEffect> GetAllEffects();

    // ═══════════════════════════════════════════════════════════════════════════
    // Convenience Modifier Accessors
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the attack modifier for a specific proficiency level.
    /// </summary>
    /// <param name="level">The proficiency level.</param>
    /// <returns>
    /// The attack modifier value (-3 for NonProficient, 0 for Proficient,
    /// +1 for Expert, +2 for Master by default).
    /// </returns>
    /// <remarks>
    /// <para>
    /// Convenience method that extracts only the attack modifier from the full effect.
    /// Equivalent to calling <c>GetEffect(level).AttackModifier</c>.
    /// </para>
    /// <para>
    /// Use this method when you only need the attack modifier, not the full effect object.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// int attackMod = provider.GetAttackModifier(WeaponProficiencyLevel.NonProficient);
    /// Console.WriteLine($"Attack penalty: {attackMod}"); // -3
    /// </code>
    /// </example>
    int GetAttackModifier(WeaponProficiencyLevel level);

    /// <summary>
    /// Gets the damage modifier for a specific proficiency level.
    /// </summary>
    /// <param name="level">The proficiency level.</param>
    /// <returns>
    /// The damage modifier value (-2 for NonProficient, 0 for Proficient,
    /// 0 for Expert, +1 for Master by default).
    /// </returns>
    /// <remarks>
    /// <para>
    /// Convenience method that extracts only the damage modifier from the full effect.
    /// Equivalent to calling <c>GetEffect(level).DamageModifier</c>.
    /// </para>
    /// <para>
    /// Use this method when you only need the damage modifier, not the full effect object.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// int damageMod = provider.GetDamageModifier(WeaponProficiencyLevel.Master);
    /// Console.WriteLine($"Damage bonus: {damageMod}"); // +1
    /// </code>
    /// </example>
    int GetDamageModifier(WeaponProficiencyLevel level);

    // ═══════════════════════════════════════════════════════════════════════════
    // Capability Check Methods
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Checks if a proficiency level allows special property usage.
    /// </summary>
    /// <param name="level">The proficiency level.</param>
    /// <returns>
    /// <c>true</c> if characters with this proficiency level can activate
    /// weapon special properties (lifesteal, cleave, etc.); otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Special properties are typically blocked for <see cref="WeaponProficiencyLevel.NonProficient"/>
    /// and available for all other levels.
    /// </para>
    /// <para>
    /// Convenience method equivalent to calling <c>GetEffect(level).CanUseSpecialProperties</c>.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// if (!provider.CanUseSpecialProperties(level))
    /// {
    ///     Logger.LogWarning("Cannot activate lifesteal - not proficient with weapon!");
    /// }
    /// </code>
    /// </example>
    bool CanUseSpecialProperties(WeaponProficiencyLevel level);

    /// <summary>
    /// Gets the technique access tier for a proficiency level.
    /// </summary>
    /// <param name="level">The proficiency level.</param>
    /// <returns>
    /// The highest <see cref="TechniqueAccess"/> tier available at this
    /// proficiency level (None, Basic, Advanced, or Signature).
    /// </returns>
    /// <remarks>
    /// <para>
    /// Technique access is cumulative:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>NonProficient → None</description></item>
    ///   <item><description>Proficient → Basic</description></item>
    ///   <item><description>Expert → Advanced (includes Basic)</description></item>
    ///   <item><description>Master → Signature (includes Basic and Advanced)</description></item>
    /// </list>
    /// <para>
    /// Convenience method equivalent to calling <c>GetEffect(level).UnlockedTechniques</c>.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var access = provider.GetTechniqueAccess(characterProficiency);
    /// if (access >= TechniqueAccess.Advanced)
    /// {
    ///     // Enable advanced technique buttons in combat UI
    /// }
    /// </code>
    /// </example>
    TechniqueAccess GetTechniqueAccess(WeaponProficiencyLevel level);

    /// <summary>
    /// Gets the display name for a proficiency level.
    /// </summary>
    /// <param name="level">The proficiency level.</param>
    /// <returns>
    /// The localized display name for UI presentation (e.g., "Non-Proficient",
    /// "Proficient", "Expert", "Master").
    /// </returns>
    /// <remarks>
    /// <para>
    /// Display names are loaded from configuration and may be localized.
    /// </para>
    /// <para>
    /// Convenience method equivalent to calling <c>GetEffect(level).DisplayName</c>.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// string displayName = provider.GetDisplayName(WeaponProficiencyLevel.Expert);
    /// proficiencyLabel.Text = displayName; // "Expert"
    /// </code>
    /// </example>
    string GetDisplayName(WeaponProficiencyLevel level);

    // ═══════════════════════════════════════════════════════════════════════════
    // Validation Methods
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Validates that all required proficiency levels have effect entries.
    /// </summary>
    /// <returns>
    /// <c>true</c> if all four proficiency levels (NonProficient, Proficient,
    /// Expert, Master) have corresponding effect entries; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method should be called during application startup to verify that
    /// the configuration is complete. Missing entries may indicate a configuration
    /// file error or version mismatch.
    /// </para>
    /// <para>
    /// Implementations should log warnings or errors for missing entries to
    /// help diagnose configuration issues.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // During application startup
    /// if (!proficiencyProvider.ValidateConfiguration())
    /// {
    ///     Logger.LogError("Proficiency configuration incomplete - some levels missing!");
    ///     // Use fallback defaults or abort startup
    /// }
    /// </code>
    /// </example>
    bool ValidateConfiguration();
}
