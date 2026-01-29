// ═══════════════════════════════════════════════════════════════════════════════
// BackgroundDefinition.cs
// Entity defining a background with its associated metadata, profession context,
// social standing, and narrative hooks for quest/dialogue integration.
// Version: 0.17.1a
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.Entities;

using Microsoft.Extensions.Logging;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Interfaces;

/// <summary>
/// Represents a complete background definition including all properties and narrative context.
/// </summary>
/// <remarks>
/// <para>
/// BackgroundDefinition is an immutable entity loaded from configuration that contains
/// all data needed to present background options during character creation and to
/// provide narrative context throughout gameplay.
/// </para>
/// <para>
/// Unlike <see cref="LineageDefinition"/> which represents inherited bloodline traits,
/// BackgroundDefinition represents learned skills and accumulated knowledge from the
/// character's pre-Silence profession. Each definition contains:
/// </para>
/// <list type="bullet">
///   <item><description>Display metadata (name, description, selection text)</description></item>
///   <item><description>Profession context (what the character did before the Great Silence)</description></item>
///   <item><description>Social standing (how post-Silence society views this background)</description></item>
///   <item><description>Narrative hooks (quest and dialogue trigger strings)</description></item>
/// </list>
/// <para>
/// This entity is extended by subsequent phases:
/// </para>
/// <list type="bullet">
///   <item><description>v0.17.1b adds SkillGrants (skill bonuses from profession)</description></item>
///   <item><description>v0.17.1c adds EquipmentGrants (starting items from profession)</description></item>
/// </list>
/// <para>
/// Instances are typically loaded from configuration (backgrounds.json) via
/// the IBackgroundProvider (v0.17.1d).
/// </para>
/// </remarks>
/// <seealso cref="Background"/>
public sealed class BackgroundDefinition : IEntity
{
    // ═══════════════════════════════════════════════════════════════════════════
    // PRIVATE FIELDS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Logger for detailed diagnostic output during definition creation.
    /// </summary>
    private static ILogger<BackgroundDefinition>? _logger;

    // ═══════════════════════════════════════════════════════════════════════════
    // PROPERTIES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the unique identifier for this background definition.
    /// </summary>
    /// <value>A GUID that uniquely identifies this definition instance.</value>
    public Guid Id { get; private set; }

    /// <summary>
    /// Gets the background enum value this definition describes.
    /// </summary>
    /// <value>The <see cref="Background"/> enum value for this definition.</value>
    public Background BackgroundId { get; private set; }

    /// <summary>
    /// Gets the display name shown to players in UI.
    /// </summary>
    /// <value>A player-friendly name such as "Village Smith" or "Ruin Delver".</value>
    /// <example>"Village Smith", "Traveling Healer", "Ruin Delver", "Clan Guard", "Wandering Skald", "Outcast Scavenger"</example>
    public string DisplayName { get; private set; }

    /// <summary>
    /// Gets the full description of the background for detail views.
    /// </summary>
    /// <value>A multi-sentence description of the background's history and nature.</value>
    /// <remarks>
    /// This text provides the full lore description of the background, suitable
    /// for detail panels and character sheet views.
    /// </remarks>
    public string Description { get; private set; }

    /// <summary>
    /// Gets the flavor text shown during background selection in character creation.
    /// </summary>
    /// <value>An evocative, second-person narrative describing the character's past.</value>
    /// <remarks>
    /// Selection text is written in second person ("You worked...", "You learned...")
    /// to help players connect with the character's backstory. It should evoke the
    /// atmosphere and experiences of the profession before the Great Silence.
    /// </remarks>
    public string SelectionText { get; private set; }

    /// <summary>
    /// Gets a brief description of what the character did before the Great Silence.
    /// </summary>
    /// <value>A concise profession label (e.g., "Blacksmith and metalworker").</value>
    /// <remarks>
    /// This is a short profession description used in character sheets, summaries,
    /// and compact UI displays. It describes the character's pre-Silence occupation
    /// in a few words.
    /// </remarks>
    /// <example>"Blacksmith and metalworker", "Itinerant medicine practitioner", "Scavenger and explorer"</example>
    public string ProfessionBefore { get; private set; }

    /// <summary>
    /// Gets how society typically views characters with this background.
    /// </summary>
    /// <value>A description of the general social perception of this profession.</value>
    /// <remarks>
    /// <para>
    /// Social standing affects NPC reactions and may unlock or restrict certain
    /// dialogue options. It represents the general perception of the profession
    /// in post-Silence society.
    /// </para>
    /// <para>
    /// Social standing ranges from high (Village Smith, Clan Guard) through
    /// moderate (Traveling Healer, Wandering Skald) to low (Ruin Delver,
    /// Outcast Scavenger).
    /// </para>
    /// </remarks>
    /// <example>"Respected craftsperson, essential to any settlement", "Pariah, viewed with suspicion"</example>
    public string SocialStanding { get; private set; }

    /// <summary>
    /// Gets narrative hooks that can trigger special dialogue or quest options.
    /// </summary>
    /// <value>A read-only list of string descriptions for narrative event matching.</value>
    /// <remarks>
    /// <para>
    /// Narrative hooks are string descriptions that the narrative system can match
    /// against event types to provide background-specific interactions. Each
    /// background typically has 3 narrative hooks that provide:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>Knowledge-based insights (e.g., "Know ruin layouts and hazards")</description></item>
    ///   <item><description>Social interactions (e.g., "Other guards trust you more quickly")</description></item>
    ///   <item><description>Story seeds (e.g., "May have enemies among rival delvers")</description></item>
    /// </list>
    /// <para>
    /// Hooks are matched using case-insensitive keyword searching via
    /// <see cref="HasNarrativeHookContaining"/>.
    /// </para>
    /// </remarks>
    /// <seealso cref="HasNarrativeHooks"/>
    /// <seealso cref="GetNarrativeHookCount"/>
    /// <seealso cref="HasNarrativeHookContaining"/>
    public IReadOnlyList<string> NarrativeHooks { get; private set; }

    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTRUCTORS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Private constructor for EF Core materialization.
    /// </summary>
    private BackgroundDefinition()
    {
        DisplayName = null!;
        Description = null!;
        SelectionText = null!;
        ProfessionBefore = null!;
        SocialStanding = null!;
        NarrativeHooks = Array.Empty<string>();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // FACTORY METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a new BackgroundDefinition with the specified properties.
    /// </summary>
    /// <param name="backgroundId">The background enum value this definition represents.</param>
    /// <param name="displayName">The display name shown in UI (e.g., "Village Smith").</param>
    /// <param name="description">The full lore description of the background.</param>
    /// <param name="selectionText">The flavor text shown during character creation selection.</param>
    /// <param name="professionBefore">A brief description of the pre-Silence profession.</param>
    /// <param name="socialStanding">How society views characters with this background.</param>
    /// <param name="narrativeHooks">Optional list of quest/dialogue trigger strings.</param>
    /// <param name="logger">Optional logger for diagnostic output during creation.</param>
    /// <returns>A new <see cref="BackgroundDefinition"/> instance.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="displayName"/>, <paramref name="description"/>,
    /// <paramref name="selectionText"/>, <paramref name="professionBefore"/>,
    /// or <paramref name="socialStanding"/> is null or whitespace.
    /// </exception>
    /// <example>
    /// <code>
    /// var villageSmith = BackgroundDefinition.Create(
    ///     Background.VillageSmith,
    ///     "Village Smith",
    ///     "You worked the forge, shaping metal into tools of war and peace.",
    ///     "The ring of hammer on anvil was your morning song...",
    ///     "Blacksmith and metalworker",
    ///     "Respected craftsperson, essential to any settlement",
    ///     new List&lt;string&gt;
    ///     {
    ///         "Recognize craftsmanship in ruins",
    ///         "Repair broken equipment more easily",
    ///         "Clan smiths may offer discounts or quests"
    ///     }
    /// );
    /// </code>
    /// </example>
    public static BackgroundDefinition Create(
        Background backgroundId,
        string displayName,
        string description,
        string selectionText,
        string professionBefore,
        string socialStanding,
        IReadOnlyList<string>? narrativeHooks = null,
        ILogger<BackgroundDefinition>? logger = null)
    {
        // Store logger for this creation context
        _logger = logger;

        _logger?.LogDebug(
            "Creating BackgroundDefinition for background {BackgroundId} with display name '{DisplayName}'",
            backgroundId,
            displayName);

        // Validate required string parameters
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName, nameof(displayName));
        ArgumentException.ThrowIfNullOrWhiteSpace(description, nameof(description));
        ArgumentException.ThrowIfNullOrWhiteSpace(selectionText, nameof(selectionText));
        ArgumentException.ThrowIfNullOrWhiteSpace(professionBefore, nameof(professionBefore));
        ArgumentException.ThrowIfNullOrWhiteSpace(socialStanding, nameof(socialStanding));

        _logger?.LogDebug(
            "Validation passed for background {BackgroundId}. " +
            "ProfessionBefore='{ProfessionBefore}', SocialStanding='{SocialStanding}', " +
            "NarrativeHookCount={NarrativeHookCount}",
            backgroundId,
            professionBefore,
            socialStanding,
            narrativeHooks?.Count ?? 0);

        var definition = new BackgroundDefinition
        {
            Id = Guid.NewGuid(),
            BackgroundId = backgroundId,
            DisplayName = displayName.Trim(),
            Description = description.Trim(),
            SelectionText = selectionText.Trim(),
            ProfessionBefore = professionBefore.Trim(),
            SocialStanding = socialStanding.Trim(),
            NarrativeHooks = narrativeHooks ?? Array.Empty<string>()
        };

        _logger?.LogInformation(
            "Created BackgroundDefinition '{DisplayName}' (ID: {Id}) for background {BackgroundId}. " +
            "Profession: '{ProfessionBefore}', Standing: '{SocialStanding}', " +
            "NarrativeHooks: {NarrativeHookCount}",
            definition.DisplayName,
            definition.Id,
            definition.BackgroundId,
            definition.ProfessionBefore,
            definition.SocialStanding,
            definition.NarrativeHooks.Count);

        return definition;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // HELPER METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets whether this background has any narrative hooks defined.
    /// </summary>
    /// <returns>
    /// <c>true</c> if the background has one or more narrative hooks;
    /// otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// All standard backgrounds have 3 narrative hooks. This method is
    /// primarily useful for validation and edge-case handling.
    /// </remarks>
    public bool HasNarrativeHooks() => NarrativeHooks.Count > 0;

    /// <summary>
    /// Gets the number of narrative hooks for this background.
    /// </summary>
    /// <returns>The count of narrative hooks defined for this background.</returns>
    /// <remarks>
    /// Standard backgrounds have 3 narrative hooks each, covering knowledge,
    /// social interactions, and story seeds.
    /// </remarks>
    public int GetNarrativeHookCount() => NarrativeHooks.Count;

    /// <summary>
    /// Checks if this background has a specific narrative hook by keyword.
    /// </summary>
    /// <param name="keyword">The keyword to search for (case-insensitive).</param>
    /// <returns>
    /// <c>true</c> if any narrative hook contains the specified keyword;
    /// otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method performs a case-insensitive substring search across all
    /// narrative hooks. It is used by the narrative event system to determine
    /// whether a background-specific interaction should be triggered.
    /// </para>
    /// <para>
    /// Returns <c>false</c> if the keyword is null or whitespace.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var definition = BackgroundDefinition.Create(...);
    /// // Ruin Delver has hook "Know ruin layouts and hazards"
    /// bool hasRuinHook = definition.HasNarrativeHookContaining("ruin"); // true
    /// bool hasCombatHook = definition.HasNarrativeHookContaining("combat"); // false
    /// </code>
    /// </example>
    public bool HasNarrativeHookContaining(string keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword))
            return false;

        return NarrativeHooks.Any(hook =>
            hook.Contains(keyword, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Gets a summary string suitable for character creation preview.
    /// </summary>
    /// <returns>
    /// A formatted string containing the profession and social standing,
    /// separated by a newline.
    /// </returns>
    /// <remarks>
    /// This summary is used in the character creation UI to provide a
    /// compact overview of the background's key attributes.
    /// </remarks>
    /// <example>
    /// Returns: "Profession: Blacksmith and metalworker\nStanding: Respected craftsperson, essential to any settlement"
    /// </example>
    public string GetCreationSummary() =>
        $"Profession: {ProfessionBefore}\nStanding: {SocialStanding}";

    // ═══════════════════════════════════════════════════════════════════════════
    // DEBUGGING
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Returns a string representation of this background definition.
    /// </summary>
    /// <returns>
    /// A formatted string containing the display name, background ID,
    /// profession, and narrative hook count.
    /// </returns>
    public override string ToString() =>
        $"{DisplayName} ({BackgroundId}): Profession={ProfessionBefore}, Hooks={NarrativeHooks.Count}";
}
