using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Application.Events;

/// <summary>
/// Event raised when a player learns a new crafting recipe.
/// </summary>
/// <remarks>
/// <para>
/// This event is published when a player successfully learns a recipe they
/// did not previously know. It can be triggered by:
/// <list type="bullet">
///   <item><description>Using a recipe scroll item</description></item>
///   <item><description>Completing a quest that teaches a recipe</description></item>
///   <item><description>Purchasing a recipe from an NPC</description></item>
///   <item><description>Discovering a recipe through gameplay</description></item>
/// </list>
/// </para>
/// <para>
/// The event is NOT raised when:
/// <list type="bullet">
///   <item><description>Default recipes are initialized during player creation</description></item>
///   <item><description>The player already knows the recipe (AlreadyKnown result)</description></item>
///   <item><description>The recipe doesn't exist (NotFound result)</description></item>
/// </list>
/// </para>
/// <para>
/// This event can be used by:
/// <list type="bullet">
///   <item><description>Achievement systems to track recipe collection progress</description></item>
///   <item><description>Statistics tracking to monitor player progression</description></item>
///   <item><description>UI systems to display "Recipe Learned" notifications</description></item>
///   <item><description>Audio systems to play recipe learned sound effects</description></item>
/// </list>
/// </para>
/// </remarks>
/// <param name="PlayerId">The unique identifier of the player who learned the recipe.</param>
/// <param name="RecipeId">The unique identifier of the learned recipe (lowercase, kebab-case).</param>
/// <param name="RecipeName">The display name of the learned recipe.</param>
/// <example>
/// <code>
/// // Subscribe to recipe learning events
/// eventLogger.Subscribe&lt;RecipeLearnedEvent&gt;(e =>
/// {
///     logger.LogInformation(
///         "Player {PlayerId} learned recipe: {RecipeName}",
///         e.PlayerId,
///         e.RecipeName);
///
///     // Update statistics
///     statistics.IncrementRecipesLearned(e.PlayerId);
///
///     // Check for achievements
///     achievements.CheckRecipeCollection(e.PlayerId);
/// });
///
/// // Publish the event after learning
/// eventLogger.Log(new RecipeLearnedEvent(
///     PlayerId: player.Id,
///     RecipeId: "steel-sword",
///     RecipeName: "Steel Sword"));
/// </code>
/// </example>
public record RecipeLearnedEvent(
    Guid PlayerId,
    string RecipeId,
    string RecipeName)
{
    /// <summary>
    /// Gets the timestamp when the event occurred.
    /// </summary>
    /// <remarks>
    /// Automatically set to the current UTC time when the event is created.
    /// </remarks>
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;
}

/// <summary>
/// Event raised when a player discovers a new recipe through exploration.
/// </summary>
/// <remarks>
/// <para>
/// This event is published when a player discovers a recipe through gameplay
/// activities such as using recipe scrolls, completing quests, or experimentation.
/// It provides more context than RecipeLearnedEvent by including the discovery source.
/// </para>
/// <para>
/// Discovery sources:
/// <list type="bullet">
///   <item><description>Scroll - Recipe scroll item used</description></item>
///   <item><description>Trainer - Learned from NPC trainer</description></item>
///   <item><description>Quest - Reward from quest completion</description></item>
///   <item><description>Experimentation - Discovered through crafting attempts</description></item>
///   <item><description>Default - Known at character creation (not typically fired)</description></item>
/// </list>
/// </para>
/// <para>
/// This event can be used by:
/// <list type="bullet">
///   <item><description>Achievement systems to track discovery-specific accomplishments</description></item>
///   <item><description>Statistics tracking to monitor how players discover recipes</description></item>
///   <item><description>UI systems to display discovery-specific notifications</description></item>
///   <item><description>Audio systems to play discovery-specific sound effects</description></item>
/// </list>
/// </para>
/// </remarks>
/// <param name="PlayerId">The unique identifier of the player who discovered the recipe.</param>
/// <param name="RecipeId">The unique identifier of the discovered recipe (lowercase, kebab-case).</param>
/// <param name="RecipeName">The display name of the discovered recipe.</param>
/// <param name="DiscoverySource">How the recipe was discovered.</param>
/// <example>
/// <code>
/// // Subscribe to recipe discovery events
/// eventLogger.Subscribe&lt;RecipeDiscoveredEvent&gt;(e =>
/// {
///     logger.LogInformation(
///         "Player {PlayerId} discovered {RecipeName} via {Source}",
///         e.PlayerId,
///         e.RecipeName,
///         e.DiscoverySource);
///
///     // Check for discovery-specific achievements
///     if (e.DiscoverySource == DiscoverySource.Experimentation)
///     {
///         achievements.CheckExperimenterProgress(e.PlayerId);
///     }
/// });
///
/// // Publish the event after discovering via scroll
/// eventLogger.Log(new RecipeDiscoveredEvent(
///     PlayerId: player.Id,
///     RecipeId: "steel-sword",
///     RecipeName: "Steel Sword",
///     DiscoverySource: DiscoverySource.Scroll));
/// </code>
/// </example>
public record RecipeDiscoveredEvent(
    Guid PlayerId,
    string RecipeId,
    string RecipeName,
    DiscoverySource DiscoverySource)
{
    /// <summary>
    /// Gets the timestamp when the event occurred.
    /// </summary>
    /// <remarks>
    /// Automatically set to the current UTC time when the event is created.
    /// </remarks>
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;
}
