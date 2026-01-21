// ═══════════════════════════════════════════════════════════════════════════════
// GatheringDisplayDtos.cs
// Data transfer objects for gathering display components.
// Version: 0.13.3d
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Presentation.Tui.DTOs;

/// <summary>
/// Data transfer object for harvestable node display.
/// </summary>
/// <param name="NodeId">The unique node identifier.</param>
/// <param name="Name">The display name of the node.</param>
/// <param name="ResourceTypeId">The resource type identifier.</param>
/// <param name="ResourceType">The resource category for display.</param>
/// <param name="Quantity">The available quantity.</param>
/// <param name="DifficultyClass">The gathering DC.</param>
/// <param name="RequiredSkill">The skill required for gathering.</param>
/// <param name="IsAvailable">Whether the node can be gathered.</param>
/// <example>
/// <code>
/// var node = new HarvestableNodeDisplayDto(
///     NodeId: "herbs-1",
///     Name: "Healing Herbs",
///     ResourceTypeId: "herb",
///     ResourceType: ResourceCategory.Herb,
///     Quantity: 3,
///     DifficultyClass: 10,
///     RequiredSkill: "Herbalism",
///     IsAvailable: true);
/// </code>
/// </example>
public record HarvestableNodeDisplayDto(
    string NodeId,
    string Name,
    string ResourceTypeId,
    ResourceCategory ResourceType,
    int Quantity,
    int DifficultyClass,
    string RequiredSkill,
    bool IsAvailable);

/// <summary>
/// Data transfer object for gathering check display.
/// </summary>
/// <param name="NodeId">The target node identifier.</param>
/// <param name="NodeName">The node name for display.</param>
/// <param name="SkillName">The skill being checked.</param>
/// <param name="DifficultyClass">The gathering DC.</param>
/// <param name="SkillModifier">The player's skill modifier.</param>
/// <example>
/// <code>
/// var check = new GatheringCheckDisplayDto(
///     NodeId: "herbs-1",
///     NodeName: "Healing Herbs",
///     SkillName: "Herbalism",
///     DifficultyClass: 10,
///     SkillModifier: 3);
/// </code>
/// </example>
public record GatheringCheckDisplayDto(
    string NodeId,
    string NodeName,
    string SkillName,
    int DifficultyClass,
    int SkillModifier);

/// <summary>
/// Data transfer object for gathering result display.
/// </summary>
/// <param name="Success">Whether the gathering succeeded.</param>
/// <param name="RawRoll">The raw dice roll value.</param>
/// <param name="Modifier">The skill modifier applied.</param>
/// <param name="Total">The total check result.</param>
/// <param name="DifficultyClass">The DC that was checked against.</param>
/// <example>
/// <code>
/// var result = new GatheringResultDto(
///     Success: true,
///     RawRoll: 14,
///     Modifier: 3,
///     Total: 17,
///     DifficultyClass: 10);
/// </code>
/// </example>
public record GatheringResultDto(
    bool Success,
    int RawRoll,
    int Modifier,
    int Total,
    int DifficultyClass);

/// <summary>
/// Data transfer object for gathered resource display.
/// </summary>
/// <param name="ResourceId">The resource identifier.</param>
/// <param name="ResourceName">The display name of the resource.</param>
/// <param name="ResourceType">The resource category.</param>
/// <param name="Quantity">The quantity gathered.</param>
/// <example>
/// <code>
/// var gathered = new GatheredResourceDto(
///     ResourceId: "healing-herb",
///     ResourceName: "Healing Herb",
///     ResourceType: ResourceCategory.Herb,
///     Quantity: 2);
/// </code>
/// </example>
public record GatheredResourceDto(
    string ResourceId,
    string ResourceName,
    ResourceCategory ResourceType,
    int Quantity);
