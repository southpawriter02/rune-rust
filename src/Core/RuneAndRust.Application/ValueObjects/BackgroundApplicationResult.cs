// ═══════════════════════════════════════════════════════════════════════════════
// BackgroundApplicationResult.cs
// Value object capturing the result of applying a background to a character,
// including all skills granted, equipment created, and any messages or errors.
// Version: 0.17.1e
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Application.ValueObjects;

using RuneAndRust.Domain.Enums;

/// <summary>
/// Represents the result of applying a background to a character during creation.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="BackgroundApplicationResult"/> captures detailed information about what was
/// granted when a background was applied. This enables UI feedback, logging,
/// and character creation summaries.
/// </para>
/// <para>
/// Use the factory methods <see cref="Succeeded"/> and <see cref="Failed"/> to create
/// instances rather than constructing directly. This ensures consistent initialization
/// of all properties.
/// </para>
/// <para>
/// <strong>Success Result:</strong> Contains populated lists of granted skills and equipment,
/// plus an optional success message. <see cref="Success"/> is <c>true</c>.
/// </para>
/// <para>
/// <strong>Failure Result:</strong> Contains an error message in <see cref="Messages"/>.
/// <see cref="SkillsGranted"/> and <see cref="EquipmentGranted"/> are empty arrays.
/// <see cref="Success"/> is <c>false</c>.
/// </para>
/// </remarks>
/// <seealso cref="GrantedSkill"/>
/// <seealso cref="GrantedEquipment"/>
/// <seealso cref="BackgroundPreview"/>
public readonly record struct BackgroundApplicationResult
{
    // ═══════════════════════════════════════════════════════════════════════════
    // PROPERTIES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets whether the background was applied successfully.
    /// </summary>
    /// <value>
    /// <c>true</c> if all background grants were applied without errors;
    /// <c>false</c> if the background could not be applied (e.g., not found,
    /// character already has a background).
    /// </value>
    public bool Success { get; init; }

    /// <summary>
    /// Gets the background that was applied (or attempted to be applied).
    /// </summary>
    /// <value>
    /// The <see cref="Background"/> enum value identifying which background
    /// was applied or attempted.
    /// </value>
    public Background BackgroundId { get; init; }

    /// <summary>
    /// Gets the list of skills that were granted to the character.
    /// </summary>
    /// <value>
    /// A read-only list of <see cref="GrantedSkill"/> records describing each
    /// skill bonus applied. Empty on failure.
    /// </value>
    /// <remarks>
    /// Each entry includes the skill ID, bonus amount, and grant type.
    /// For example, Village Smith grants: [("craft", 2, Permanent), ("might", 1, Permanent)].
    /// </remarks>
    public IReadOnlyList<GrantedSkill> SkillsGranted { get; init; }

    /// <summary>
    /// Gets the list of equipment items that were granted to the character.
    /// </summary>
    /// <value>
    /// A read-only list of <see cref="GrantedEquipment"/> records describing each
    /// item added to the character's inventory. Empty on failure.
    /// </value>
    /// <remarks>
    /// Each entry includes the item ID, quantity, whether it was auto-equipped,
    /// and the equipment slot (if equipped). For example, Clan Guard grants:
    /// [("shield", 1, true, Shield), ("spear", 1, true, Weapon)].
    /// </remarks>
    public IReadOnlyList<GrantedEquipment> EquipmentGranted { get; init; }

    /// <summary>
    /// Gets informational and error messages from the application process.
    /// </summary>
    /// <value>
    /// A read-only list of human-readable messages. On success, contains a summary
    /// message. On failure, contains the error description.
    /// </value>
    public IReadOnlyList<string> Messages { get; init; }

    // ═══════════════════════════════════════════════════════════════════════════
    // FACTORY METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a successful application result with the granted skills and equipment.
    /// </summary>
    /// <param name="backgroundId">The background that was applied.</param>
    /// <param name="skills">The skills that were granted to the character.</param>
    /// <param name="equipment">The equipment items that were granted to the character.</param>
    /// <param name="message">An optional success message for display.</param>
    /// <returns>
    /// A <see cref="BackgroundApplicationResult"/> with <see cref="Success"/> set to <c>true</c>
    /// and all grant details populated.
    /// </returns>
    /// <example>
    /// <code>
    /// var result = BackgroundApplicationResult.Succeeded(
    ///     Background.VillageSmith,
    ///     new[] { new GrantedSkill("craft", 2, SkillGrantType.Permanent) },
    ///     new[] { new GrantedEquipment("smiths-hammer", 1, true, EquipmentSlot.Weapon) },
    ///     "Applied Village Smith background successfully");
    /// </code>
    /// </example>
    public static BackgroundApplicationResult Succeeded(
        Background backgroundId,
        IReadOnlyList<GrantedSkill> skills,
        IReadOnlyList<GrantedEquipment> equipment,
        string? message = null)
    {
        var messages = new List<string>();
        if (!string.IsNullOrEmpty(message))
            messages.Add(message);

        return new BackgroundApplicationResult
        {
            Success = true,
            BackgroundId = backgroundId,
            SkillsGranted = skills,
            EquipmentGranted = equipment,
            Messages = messages
        };
    }

    /// <summary>
    /// Creates a failed application result with an error message.
    /// </summary>
    /// <param name="backgroundId">The background that was attempted.</param>
    /// <param name="errorMessage">A human-readable description of why the application failed.</param>
    /// <returns>
    /// A <see cref="BackgroundApplicationResult"/> with <see cref="Success"/> set to <c>false</c>
    /// and empty grant lists.
    /// </returns>
    /// <example>
    /// <code>
    /// var result = BackgroundApplicationResult.Failed(
    ///     Background.VillageSmith,
    ///     "Background 'VillageSmith' not found");
    /// </code>
    /// </example>
    public static BackgroundApplicationResult Failed(
        Background backgroundId,
        string errorMessage)
    {
        return new BackgroundApplicationResult
        {
            Success = false,
            BackgroundId = backgroundId,
            SkillsGranted = Array.Empty<GrantedSkill>(),
            EquipmentGranted = Array.Empty<GrantedEquipment>(),
            Messages = new[] { errorMessage }
        };
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // SUMMARY METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets a formatted summary of granted skills for display.
    /// </summary>
    /// <returns>
    /// A comma-separated string of skill grants (e.g., "craft +2, might +1"),
    /// or "No skills granted" if the list is empty.
    /// </returns>
    /// <example>
    /// <code>
    /// var result = BackgroundApplicationResult.Succeeded(
    ///     Background.VillageSmith, skills, equipment);
    /// Console.WriteLine(result.GetSkillSummary());
    /// // Output: "craft +2, might +1"
    /// </code>
    /// </example>
    public string GetSkillSummary() =>
        SkillsGranted.Count > 0
            ? string.Join(", ", SkillsGranted.Select(s => s.ToString()))
            : "No skills granted";

    /// <summary>
    /// Gets a formatted summary of granted equipment for display.
    /// </summary>
    /// <returns>
    /// A comma-separated string of equipment grants (e.g., "shield (Shield), spear (Weapon)"),
    /// or "No equipment granted" if the list is empty.
    /// </returns>
    /// <example>
    /// <code>
    /// var result = BackgroundApplicationResult.Succeeded(
    ///     Background.ClanGuard, skills, equipment);
    /// Console.WriteLine(result.GetEquipmentSummary());
    /// // Output: "shield (Shield), spear (Weapon)"
    /// </code>
    /// </example>
    public string GetEquipmentSummary() =>
        EquipmentGranted.Count > 0
            ? string.Join(", ", EquipmentGranted.Select(e => e.ToString()))
            : "No equipment granted";
}

/// <summary>
/// Represents a skill that was granted from a background during character creation.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="GrantedSkill"/> is a lightweight record used within
/// <see cref="BackgroundApplicationResult"/> to track which skills were applied.
/// It captures the skill identifier, bonus amount, and the type of grant
/// (Permanent, StartingBonus, or Proficiency).
/// </para>
/// </remarks>
/// <param name="SkillId">
/// The lowercase skill identifier (e.g., "craft", "combat", "survival").
/// </param>
/// <param name="Amount">
/// The numeric bonus amount applied (e.g., 2 for primary skill, 1 for secondary).
/// Zero for Proficiency grants.
/// </param>
/// <param name="GrantType">
/// The type of skill grant that was applied (Permanent, StartingBonus, or Proficiency).
/// </param>
/// <seealso cref="BackgroundApplicationResult"/>
/// <seealso cref="SkillGrantType"/>
public readonly record struct GrantedSkill(
    string SkillId,
    int Amount,
    SkillGrantType GrantType)
{
    /// <summary>
    /// Returns a formatted string representation of the granted skill.
    /// </summary>
    /// <returns>
    /// A string in one of the following formats:
    /// <list type="bullet">
    ///   <item><description><c>"craft +2"</c> for Permanent grants</description></item>
    ///   <item><description><c>"craft +2 (starting)"</c> for StartingBonus grants</description></item>
    ///   <item><description><c>"traps (proficient)"</c> for Proficiency grants</description></item>
    /// </list>
    /// </returns>
    public override string ToString() =>
        GrantType switch
        {
            SkillGrantType.Proficiency => $"{SkillId} (proficient)",
            SkillGrantType.StartingBonus => $"{SkillId} +{Amount} (starting)",
            _ => $"{SkillId} +{Amount}"
        };
}

/// <summary>
/// Represents an equipment item that was granted from a background during character creation.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="GrantedEquipment"/> is a lightweight record used within
/// <see cref="BackgroundApplicationResult"/> to track which equipment items were
/// created and added to the character's inventory. Items may also have been
/// auto-equipped to a specific slot.
/// </para>
/// </remarks>
/// <param name="ItemId">
/// The lowercase item identifier (e.g., "smiths-hammer", "shield", "bandages").
/// </param>
/// <param name="Quantity">
/// The number of items granted (e.g., 1 for weapons, 5 for bandages, 3 for rations).
/// </param>
/// <param name="WasEquipped">
/// <c>true</c> if the item was auto-equipped to a slot during creation;
/// <c>false</c> if it was only placed in inventory.
/// </param>
/// <param name="Slot">
/// The equipment slot the item was equipped to, or <c>null</c> if not equipped.
/// Only meaningful when <paramref name="WasEquipped"/> is <c>true</c>.
/// </param>
/// <seealso cref="BackgroundApplicationResult"/>
/// <seealso cref="EquipmentSlot"/>
public readonly record struct GrantedEquipment(
    string ItemId,
    int Quantity,
    bool WasEquipped,
    EquipmentSlot? Slot)
{
    /// <summary>
    /// Returns a formatted string representation of the granted equipment.
    /// </summary>
    /// <returns>
    /// A string in one of the following formats:
    /// <list type="bullet">
    ///   <item><description><c>"spear (Weapon)"</c> for equipped items</description></item>
    ///   <item><description><c>"bandages ×5"</c> for stackable inventory items</description></item>
    ///   <item><description><c>"lockpicks"</c> for single inventory items</description></item>
    /// </list>
    /// </returns>
    public override string ToString()
    {
        var qty = Quantity > 1 ? $" ×{Quantity}" : "";
        var slot = WasEquipped && Slot.HasValue ? $" ({Slot.Value})" : "";
        return $"{ItemId}{qty}{slot}";
    }
}
