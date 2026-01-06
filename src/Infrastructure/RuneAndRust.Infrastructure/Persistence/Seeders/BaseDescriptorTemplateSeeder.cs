using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Infrastructure.Persistence.Seeders;

/// <summary>
/// Seeds Tier 1 base descriptor templates - biome-agnostic archetypes with placeholder tokens.
/// 18+ templates covering all room archetypes.
/// </summary>
public static class BaseDescriptorTemplateSeeder
{
    public static IEnumerable<BaseDescriptorTemplate> GetAllTemplates()
    {
        // Corridor templates
        yield return new BaseDescriptorTemplate(
            "Corridor_Base",
            "The {Modifier} Corridor",
            "{Article_Cap} {Modifier_Adj} corridor stretches {Direction_Descriptor}. {Architectural_Feature}. {Spatial_Descriptor}. {Detail_1}. {Modifier_Detail}.",
            RoomArchetype.Corridor,
            RoomSize.Small,
            minExits: 2,
            maxExits: 2,
            spawnBudgetMultiplier: 0.8,
            weight: 3);

        yield return new BaseDescriptorTemplate(
            "Corridor_Narrow",
            "The Narrow {Modifier} Passage",
            "A narrow, {Modifier_Adj} passage winds {Direction_Descriptor}. {Spatial_Descriptor}. {Architectural_Feature}. {Atmospheric_Detail}. {Modifier_Detail}.",
            RoomArchetype.Corridor,
            RoomSize.Small,
            minExits: 2,
            maxExits: 2,
            spawnBudgetMultiplier: 0.6,
            weight: 2);

        // Chamber templates
        yield return new BaseDescriptorTemplate(
            "Chamber_Base",
            "The {Modifier} Chamber",
            "You enter {Article} {Modifier_Adj} chamber. {Spatial_Descriptor}. {Architectural_Feature}. {Detail_1}. {Detail_2}. {Atmospheric_Detail}.",
            RoomArchetype.Chamber,
            RoomSize.Large,
            minExits: 1,
            maxExits: 4,
            spawnBudgetMultiplier: 1.2,
            weight: 3);

        yield return new BaseDescriptorTemplate(
            "Chamber_Grand",
            "The Grand {Modifier} Hall",
            "{Article_Cap} grand, {Modifier_Adj} hall opens before you. {Spatial_Descriptor}. {Architectural_Feature}. The space {Modifier_Detail}. {Detail_1}. {Atmospheric_Detail}.",
            RoomArchetype.Chamber,
            RoomSize.Large,
            minExits: 2,
            maxExits: 4,
            spawnBudgetMultiplier: 1.5,
            weight: 2);

        yield return new BaseDescriptorTemplate(
            "Chamber_Storage",
            "The {Modifier} Storage Bay",
            "{Article_Cap} {Modifier_Adj} storage area sprawls around you. {Spatial_Descriptor}. Containers and crates line the walls. {Detail_1}. {Modifier_Detail}.",
            RoomArchetype.Chamber,
            RoomSize.Large,
            minExits: 1,
            maxExits: 2,
            spawnBudgetMultiplier: 0.9,
            weight: 2);

        // Junction templates
        yield return new BaseDescriptorTemplate(
            "Junction_Base",
            "The {Modifier} Junction",
            "You stand at {Article} {Modifier_Adj} junction. Passages lead off in multiple directions. {Spatial_Descriptor}. {Architectural_Feature}. {Detail_1}.",
            RoomArchetype.Junction,
            RoomSize.Medium,
            minExits: 3,
            maxExits: 4,
            spawnBudgetMultiplier: 1.0,
            weight: 3);

        yield return new BaseDescriptorTemplate(
            "Junction_Hub",
            "The {Modifier} Hub",
            "{Article_Cap} {Modifier_Adj} hub serves as a crossroads. {Spatial_Descriptor}. Multiple paths converge here. {Architectural_Feature}. {Atmospheric_Detail}.",
            RoomArchetype.Junction,
            RoomSize.Medium,
            minExits: 3,
            maxExits: 4,
            spawnBudgetMultiplier: 1.1,
            weight: 2);

        // Dead End templates
        yield return new BaseDescriptorTemplate(
            "DeadEnd_Base",
            "The {Modifier} Alcove",
            "{Article_Cap} {Modifier_Adj} alcove terminates abruptly. {Spatial_Descriptor}. {Detail_1}. {Modifier_Detail}. {Atmospheric_Detail}.",
            RoomArchetype.DeadEnd,
            RoomSize.Small,
            minExits: 1,
            maxExits: 1,
            spawnBudgetMultiplier: 0.7,
            weight: 2);

        yield return new BaseDescriptorTemplate(
            "DeadEnd_Secret",
            "The Hidden {Modifier} Nook",
            "A concealed, {Modifier_Adj} nook opens before you. {Spatial_Descriptor}. {Detail_1}. This place seems forgotten. {Modifier_Detail}.",
            RoomArchetype.DeadEnd,
            RoomSize.Small,
            minExits: 1,
            maxExits: 1,
            spawnBudgetMultiplier: 0.5,
            weight: 1);

        // Stairwell templates
        yield return new BaseDescriptorTemplate(
            "Stairwell_Base",
            "The {Modifier} Stairwell",
            "{Article_Cap} {Modifier_Adj} stairwell spirals {Direction_Descriptor}. {Architectural_Feature}. {Spatial_Descriptor}. {Atmospheric_Detail}. {Modifier_Detail}.",
            RoomArchetype.Stairwell,
            RoomSize.Medium,
            minExits: 2,
            maxExits: 3,
            spawnBudgetMultiplier: 0.8,
            weight: 2);

        yield return new BaseDescriptorTemplate(
            "Stairwell_Shaft",
            "The Vertical {Modifier} Shaft",
            "A vertical, {Modifier_Adj} shaft extends {Direction_Descriptor}. {Spatial_Descriptor}. {Architectural_Feature}. Climbing here would be perilous. {Modifier_Detail}.",
            RoomArchetype.Stairwell,
            RoomSize.Medium,
            minExits: 2,
            maxExits: 2,
            spawnBudgetMultiplier: 0.6,
            weight: 1);

        // Boss Arena templates
        yield return new BaseDescriptorTemplate(
            "BossArena_Base",
            "The {Modifier} Sanctum",
            "{Article_Cap} {Modifier_Adj} arena of terrible purpose spreads before you. {Spatial_Descriptor}. {Architectural_Feature}. The air feels {Atmospheric_Detail}. {Modifier_Detail}. Something awaits.",
            RoomArchetype.BossArena,
            RoomSize.XLarge,
            minExits: 1,
            maxExits: 1,
            spawnBudgetMultiplier: 2.0,
            weight: 2);

        yield return new BaseDescriptorTemplate(
            "BossArena_Throne",
            "The {Modifier} Throne Room",
            "{Article_Cap} vast, {Modifier_Adj} throne room dominates this level. {Spatial_Descriptor}. A throne of impossible design commands attention. {Architectural_Feature}. {Modifier_Detail}.",
            RoomArchetype.BossArena,
            RoomSize.XLarge,
            minExits: 1,
            maxExits: 2,
            spawnBudgetMultiplier: 2.5,
            weight: 1);

        // Specialized templates for functional rooms
        yield return new BaseDescriptorTemplate(
            "Chamber_Lab",
            "The {Modifier} Laboratory",
            "{Article_Cap} {Modifier_Adj} laboratory clutters the space. {Spatial_Descriptor}. Equipment and apparatus fill every surface. {Detail_1}. {Modifier_Detail}.",
            RoomArchetype.Chamber,
            RoomSize.Medium,
            minExits: 1,
            maxExits: 2,
            spawnBudgetMultiplier: 1.0,
            weight: 1);

        yield return new BaseDescriptorTemplate(
            "Chamber_Forge",
            "The {Modifier} Forge",
            "{Article_Cap} {Modifier_Adj} forge dominates this chamber. {Spatial_Descriptor}. {Architectural_Feature}. Heat emanates from dormant furnaces. {Modifier_Detail}.",
            RoomArchetype.Chamber,
            RoomSize.Large,
            minExits: 1,
            maxExits: 2,
            spawnBudgetMultiplier: 1.3,
            weight: 1);

        yield return new BaseDescriptorTemplate(
            "Chamber_Barracks",
            "The {Modifier} Barracks",
            "{Article_Cap} {Modifier_Adj} barracks stretches before you. {Spatial_Descriptor}. Rows of bunks line the walls. {Detail_1}. {Modifier_Detail}.",
            RoomArchetype.Chamber,
            RoomSize.Medium,
            minExits: 1,
            maxExits: 2,
            spawnBudgetMultiplier: 1.4,
            weight: 1);

        yield return new BaseDescriptorTemplate(
            "Junction_Operations",
            "The {Modifier} Operations Center",
            "{Article_Cap} {Modifier_Adj} operations center serves as a nerve hub. {Spatial_Descriptor}. Consoles and displays surround you. {Architectural_Feature}. {Modifier_Detail}.",
            RoomArchetype.Junction,
            RoomSize.Medium,
            minExits: 3,
            maxExits: 4,
            spawnBudgetMultiplier: 1.2,
            weight: 1);

        yield return new BaseDescriptorTemplate(
            "Chamber_Observation",
            "The {Modifier} Observation Platform",
            "{Article_Cap} {Modifier_Adj} observation platform offers a vantage point. {Spatial_Descriptor}. {Architectural_Feature}. {Atmospheric_Detail}. {Modifier_Detail}.",
            RoomArchetype.Chamber,
            RoomSize.Medium,
            minExits: 1,
            maxExits: 2,
            spawnBudgetMultiplier: 0.8,
            weight: 1);
    }

    /// <summary>
    /// Gets templates filtered by archetype.
    /// </summary>
    public static IEnumerable<BaseDescriptorTemplate> GetTemplatesByArchetype(RoomArchetype archetype) =>
        GetAllTemplates().Where(t => t.Archetype == archetype);
}
