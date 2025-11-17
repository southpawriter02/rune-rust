using RuneAndRust.Core;
using RuneAndRust.Core.Descriptors;
using RuneAndRust.Persistence;
using Serilog;

namespace RuneAndRust.Engine;

/// <summary>
/// v0.38.1: Service for generating room descriptions using the descriptor framework
/// Processes base templates + modifiers + fragments into complete room descriptions
/// </summary>
public class RoomDescriptorService
{
    private static readonly ILogger _log = Log.ForContext<RoomDescriptorService>();
    private readonly DescriptorRepository _repository;
    private readonly DescriptorService _descriptorService;
    private readonly Random _random;

    public RoomDescriptorService(
        DescriptorRepository repository,
        DescriptorService descriptorService)
    {
        _repository = repository;
        _descriptorService = descriptorService;
        _random = new Random();
        _log.Information("RoomDescriptorService initialized");
    }

    #region Room Name Generation

    /// <summary>
    /// Generates a room name from archetype and biome
    /// </summary>
    public string GenerateRoomName(RoomArchetype archetype, string biome)
    {
        _log.Debug("Generating room name: {Archetype} in {Biome}", archetype, biome);

        try
        {
            // Get base template
            var baseTemplateName = archetype.GetBaseTemplateName();
            var baseTemplate = _repository.GetBaseTemplateByName(baseTemplateName);

            if (baseTemplate == null)
            {
                _log.Warning("Base template not found: {TemplateName}", baseTemplateName);
                return $"The {archetype} Room";
            }

            // Get modifier for biome
            var modifier = GetModifierForBiome(biome);

            if (modifier == null)
            {
                _log.Warning("Modifier not found for biome: {Biome}", biome);
                return ProcessNameTemplate(baseTemplate.NameTemplate, "Unknown", null);
            }

            // Get function variant (for chambers)
            RoomFunctionVariant? function = null;
            if (archetype == RoomArchetype.Chamber ||
                archetype == RoomArchetype.PowerStation ||
                archetype == RoomArchetype.Laboratory)
            {
                function = _repository.GetRandomFunctionVariant(
                    baseTemplate.Archetype,
                    biome,
                    _random);
            }

            // Process name template
            var name = ProcessNameTemplate(baseTemplate.NameTemplate, modifier.ModifierName, function);

            _log.Debug("Generated room name: {Name}", name);
            return name;
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Error generating room name: {Archetype} in {Biome}", archetype, biome);
            return $"The {archetype} Room";
        }
    }

    #endregion

    #region Room Description Generation

    /// <summary>
    /// Generates a complete room description from archetype and biome
    /// </summary>
    public string GenerateRoomDescription(RoomArchetype archetype, string biome, Room? room = null)
    {
        _log.Debug("Generating room description: {Archetype} in {Biome}", archetype, biome);

        try
        {
            // Get base template
            var baseTemplateName = archetype.GetBaseTemplateName();
            var baseTemplate = _repository.GetBaseTemplateByName(baseTemplateName);

            if (baseTemplate == null)
            {
                _log.Warning("Base template not found: {TemplateName}", baseTemplateName);
                return "An unremarkable room.";
            }

            // Get modifier for biome
            var modifier = GetModifierForBiome(biome);

            if (modifier == null)
            {
                _log.Warning("Modifier not found for biome: {Biome}", biome);
                return "An unremarkable room.";
            }

            // Get function variant (for chambers)
            RoomFunctionVariant? function = null;
            if (archetype == RoomArchetype.Chamber ||
                archetype == RoomArchetype.PowerStation ||
                archetype == RoomArchetype.Laboratory ||
                archetype == RoomArchetype.ForgeCharnber ||
                archetype == RoomArchetype.CryoVault)
            {
                function = _repository.GetRandomFunctionVariant(
                    baseTemplate.Archetype,
                    biome,
                    _random);
            }

            // Process description template
            var description = ProcessDescriptionTemplate(
                baseTemplate.DescriptionTemplate,
                baseTemplate,
                modifier,
                function,
                room);

            _log.Debug("Generated room description: {Length} characters", description.Length);
            return description;
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Error generating room description: {Archetype} in {Biome}", archetype, biome);
            return "An unremarkable room.";
        }
    }

    #endregion

    #region Template Processing

    private string ProcessNameTemplate(
        string template,
        string modifierName,
        RoomFunctionVariant? function)
    {
        var result = template;

        // Replace {Modifier}
        result = result.Replace("{Modifier}", modifierName);

        // Replace {Function} if present
        if (function != null)
        {
            result = result.Replace("{Function}", function.FunctionName);
        }
        else
        {
            result = result.Replace("{Function} ", "");
            result = result.Replace(" {Function}", "");
            result = result.Replace("{Function}", "");
        }

        return result.Trim();
    }

    private string ProcessDescriptionTemplate(
        string template,
        DescriptorBaseTemplate baseTemplate,
        ThematicModifier modifier,
        RoomFunctionVariant? function,
        Room? room)
    {
        var result = template;

        // Get tags for fragment filtering
        var tags = baseTemplate.GetTags();
        tags.Add(modifier.PrimaryBiome);

        // Replace modifier placeholders
        result = result.Replace("{Modifier}", modifier.ModifierName);
        result = result.Replace("{Modifier_Adj}", modifier.Adjective);
        result = result.Replace("{Modifier_Detail}", modifier.DetailFragment);

        // Replace article
        var article = IsVowel(modifier.Adjective[0]) ? "an" : "a";
        result = result.Replace("{Article}", article);
        result = result.Replace("{Article_Cap}", char.ToUpper(article[0]) + article.Substring(1));

        // Replace function placeholders
        if (function != null)
        {
            result = result.Replace("{Function}", function.FunctionName);
            result = result.Replace("{Function_Detail}", function.FunctionDetail);
        }
        else
        {
            result = result.Replace("{Function}", "chamber");
            result = result.Replace("{Function_Detail}", "serves an unknown purpose");
        }

        // Replace spatial descriptor
        var spatialDescriptor = GetRandomFragment("SpatialDescriptor", tags);
        result = result.Replace("{Spatial_Descriptor}", spatialDescriptor);

        // Replace architectural feature
        var archFeature = GetRandomFragment("ArchitecturalFeature", tags);
        result = result.Replace("{Architectural_Feature}", archFeature);

        // Replace details (up to 2)
        var detail1 = GetRandomFragment("Detail", tags);
        var detail2 = GetRandomFragment("Detail", tags, exclude: detail1);
        result = result.Replace("{Detail_1}", detail1);
        result = result.Replace("{Detail_2}", detail2);

        // Replace atmospheric detail
        var atmoDetail = GetRandomFragment("Atmospheric", tags);
        result = result.Replace("{Atmospheric_Detail}", atmoDetail);

        // Replace direction descriptor
        var directionDesc = GetRandomFragment("Direction", tags);
        result = result.Replace("{Direction_Descriptor}", directionDesc);

        // Replace specialized placeholders
        result = ReplaceSpecializedPlaceholders(result, tags, modifier);

        return result.Trim();
    }

    private string ReplaceSpecializedPlaceholders(string template, List<string> tags, ThematicModifier modifier)
    {
        var result = template;

        // Ominous details (for boss arenas)
        if (result.Contains("{Ominous_Detail}"))
        {
            var ominousDetail = GetRandomFragmentBySubcategory("Detail", "Ominous", tags);
            result = result.Replace("{Ominous_Detail}", ominousDetail);
        }

        // Loot hints (for secret rooms)
        if (result.Contains("{Loot_Hint}"))
        {
            var lootHint = GetRandomFragmentBySubcategory("Detail", "Loot", tags);
            result = result.Replace("{Loot_Hint}", lootHint);
        }

        // Exit descriptions (for junctions)
        if (result.Contains("{Exit_Description}"))
        {
            var exitDesc = GetRandomFragmentBySubcategory("Detail", "Exits", tags);
            result = result.Replace("{Exit_Description}", exitDesc);
        }

        // Traversal warnings (for vertical shafts)
        if (result.Contains("{Traversal_Warning}"))
        {
            var warning = GetRandomFragmentBySubcategory("Detail", "Warning", tags);
            result = result.Replace("{Traversal_Warning}", warning);
        }

        // Industrial details
        if (result.Contains("{Industrial_Detail}"))
        {
            var industrial = GetRandomFragmentBySubcategory("Detail", "Industrial", tags);
            result = result.Replace("{Industrial_Detail}", industrial);
        }

        // Storage contents
        if (result.Contains("{Storage_Contents}"))
        {
            var storage = GetRandomFragmentBySubcategory("Detail", "Storage", tags);
            result = result.Replace("{Storage_Contents}", storage);
        }

        // Salvage potential
        if (result.Contains("{Salvage_Potential}"))
        {
            var salvage = GetRandomFragmentBySubcategory("Detail", "Salvage", tags);
            result = result.Replace("{Salvage_Potential}", salvage);
        }

        // Vantage description
        if (result.Contains("{Vantage_Description}"))
        {
            var vantage = GetRandomFragmentBySubcategory("Detail", "Vantage", tags);
            result = result.Replace("{Vantage_Description}", vantage);
        }

        // Visibility detail
        if (result.Contains("{Visibility_Detail}"))
        {
            var visibility = GetRandomFragmentBySubcategory("Detail", "Visibility", tags);
            result = result.Replace("{Visibility_Detail}", visibility);
        }

        // Energy state
        if (result.Contains("{Energy_State}"))
        {
            var energyState = GetRandomFragmentBySubcategory("Detail", "Energy", tags);
            result = result.Replace("{Energy_State}", energyState);
        }

        // Electrical warning
        if (result.Contains("{Electrical_Warning}"))
        {
            var electricalWarning = GetRandomFragmentBySubcategory("Detail", "Warning", tags);
            result = result.Replace("{Electrical_Warning}", electricalWarning);
        }

        // Research equipment
        if (result.Contains("{Research_Equipment}"))
        {
            var researchEquipment = GetRandomFragmentBySubcategory("Detail", "Research", tags);
            result = result.Replace("{Research_Equipment}", researchEquipment);
        }

        // Research focus
        if (result.Contains("{Research_Focus}"))
        {
            var researchFocus = GetRandomFragmentBySubcategory("Detail", "Research", tags);
            result = result.Replace("{Research_Focus}", researchFocus);
        }

        // Military detail
        if (result.Contains("{Military_Detail}"))
        {
            var militaryDetail = GetRandomFragmentBySubcategory("Detail", "Military", tags);
            result = result.Replace("{Military_Detail}", militaryDetail);
        }

        // Occupant description
        if (result.Contains("{Occupant_Description}"))
        {
            var occupant = GetRandomFragmentBySubcategory("Detail", "Occupant", tags);
            result = result.Replace("{Occupant_Description}", occupant);
        }

        // Forge equipment
        if (result.Contains("{Forge_Equipment}"))
        {
            var forgeEquipment = GetRandomFragmentBySubcategory("Detail", "Forge", tags);
            result = result.Replace("{Forge_Equipment}", forgeEquipment);
        }

        // Heat warning
        if (result.Contains("{Heat_Warning}"))
        {
            var heatWarning = GetRandomFragmentBySubcategory("Detail", "Warning", tags);
            result = result.Replace("{Heat_Warning}", heatWarning);
        }

        // Cryo contents
        if (result.Contains("{Cryo_Contents}"))
        {
            var cryoContents = GetRandomFragmentBySubcategory("Detail", "Cryo", tags);
            result = result.Replace("{Cryo_Contents}", cryoContents);
        }

        // Cryo status
        if (result.Contains("{Cryo_Status}"))
        {
            var cryoStatus = GetRandomFragmentBySubcategory("Detail", "Cryo", tags);
            result = result.Replace("{Cryo_Status}", cryoStatus);
        }

        // Cold warning
        if (result.Contains("{Cold_Warning}"))
        {
            var coldWarning = GetRandomFragmentBySubcategory("Detail", "Warning", tags);
            result = result.Replace("{Cold_Warning}", coldWarning);
        }

        return result;
    }

    #endregion

    #region Fragment Selection

    private string GetRandomFragment(string category, List<string> tags, string? exclude = null)
    {
        var fragments = _repository.GetDescriptorFragments(category, tags);

        if (exclude != null)
        {
            fragments = fragments.Where(f => f.FragmentText != exclude).ToList();
        }

        if (fragments.Count == 0)
        {
            _log.Warning("No fragments found for category {Category} with tags {Tags}", category, string.Join(",", tags));
            return GetDefaultFragment(category);
        }

        // Weighted random selection
        var totalWeight = fragments.Sum(f => f.Weight);
        var randomValue = (float)(_random.NextDouble() * totalWeight);
        var cumulativeWeight = 0f;

        foreach (var fragment in fragments)
        {
            cumulativeWeight += fragment.Weight;
            if (randomValue <= cumulativeWeight)
            {
                return fragment.FragmentText;
            }
        }

        // Fallback
        return fragments[0].FragmentText;
    }

    private string GetRandomFragmentBySubcategory(string category, string subcategory, List<string> tags, string? exclude = null)
    {
        var fragments = _repository.GetDescriptorFragmentsBySubcategory(category, subcategory, tags);

        if (exclude != null)
        {
            fragments = fragments.Where(f => f.FragmentText != exclude).ToList();
        }

        if (fragments.Count == 0)
        {
            _log.Debug("No fragments found for {Category}/{Subcategory}", category, subcategory);
            return GetDefaultFragment(category);
        }

        // Weighted random selection
        var totalWeight = fragments.Sum(f => f.Weight);
        var randomValue = (float)(_random.NextDouble() * totalWeight);
        var cumulativeWeight = 0f;

        foreach (var fragment in fragments)
        {
            cumulativeWeight += fragment.Weight;
            if (randomValue <= cumulativeWeight)
            {
                return fragment.FragmentText;
            }
        }

        // Fallback
        return fragments[0].FragmentText;
    }

    private string GetDefaultFragment(string category)
    {
        return category switch
        {
            "SpatialDescriptor" => "The space extends before you",
            "ArchitecturalFeature" => "The architecture is unremarkable",
            "Detail" => "Details are nondescript",
            "Atmospheric" => "is still",
            "Direction" => "ahead",
            _ => ""
        };
    }

    #endregion

    #region Helper Methods

    private ThematicModifier? GetModifierForBiome(string biome)
    {
        var modifierName = biome switch
        {
            "The_Roots" => "Rusted",
            "Muspelheim" => "Scorched",
            "Niflheim" => "Frozen",
            "Alfheim" => "Crystalline",
            "Jotunheim" => "Monolithic",
            _ => null
        };

        if (modifierName == null)
        {
            _log.Warning("Unknown biome: {Biome}", biome);
            return null;
        }

        return _repository.GetModifierByName(modifierName);
    }

    private bool IsVowel(char c)
    {
        return "aeiouAEIOU".Contains(c);
    }

    #endregion
}
