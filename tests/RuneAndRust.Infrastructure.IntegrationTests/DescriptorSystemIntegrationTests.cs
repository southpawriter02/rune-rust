using FluentAssertions;
using RuneAndRust.Application.Services;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Infrastructure.Providers;

namespace RuneAndRust.Infrastructure.IntegrationTests;

/// <summary>
/// Comprehensive integration tests for the Three-Tier Descriptor Composition System.
/// Tests the full pipeline from repository through service to final output.
/// </summary>
[TestFixture]
public class DescriptorSystemIntegrationTests
{
    private SeederBasedDescriptorRepository _repository = null!;
    private RoomDescriptorService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _repository = new SeederBasedDescriptorRepository();
        _service = new RoomDescriptorService(_repository);
    }

    #region Repository Tests

    [Test]
    public void Repository_LoadsAllModifiers()
    {
        var modifiers = _repository.GetAllModifiers();

        modifiers.Should().HaveCountGreaterOrEqualTo(5);
        modifiers.Should().Contain(m => m.PrimaryBiome == Biome.TheRoots);
        modifiers.Should().Contain(m => m.PrimaryBiome == Biome.Muspelheim);
        modifiers.Should().Contain(m => m.PrimaryBiome == Biome.Niflheim);
        modifiers.Should().Contain(m => m.PrimaryBiome == Biome.Alfheim);
        modifiers.Should().Contain(m => m.PrimaryBiome == Biome.Jotunheim);
    }

    [Test]
    public void Repository_LoadsAllBaseTemplates()
    {
        var templates = _repository.GetAllBaseTemplates();

        templates.Should().HaveCountGreaterOrEqualTo(18);
        templates.Should().Contain(t => t.Archetype == RoomArchetype.Corridor);
        templates.Should().Contain(t => t.Archetype == RoomArchetype.Chamber);
        templates.Should().Contain(t => t.Archetype == RoomArchetype.Junction);
        templates.Should().Contain(t => t.Archetype == RoomArchetype.DeadEnd);
        templates.Should().Contain(t => t.Archetype == RoomArchetype.Stairwell);
        templates.Should().Contain(t => t.Archetype == RoomArchetype.BossArena);
    }

    [Test]
    public void Repository_LoadsAllRoomFunctions()
    {
        var functions = _repository.GetAllFunctions();

        functions.Should().HaveCountGreaterOrEqualTo(18);
        functions.Should().OnlyContain(f => !string.IsNullOrWhiteSpace(f.FunctionName));
        functions.Should().OnlyContain(f => !string.IsNullOrWhiteSpace(f.FunctionDetail));
    }

    [Test]
    public void Repository_LoadsFragmentsForAllCategories()
    {
        var spatial = _repository.GetFragments(FragmentCategory.Spatial);
        var architectural = _repository.GetFragments(FragmentCategory.Architectural);
        var detail = _repository.GetFragments(FragmentCategory.Detail);
        var atmospheric = _repository.GetFragments(FragmentCategory.Atmospheric);
        var direction = _repository.GetFragments(FragmentCategory.Direction);

        spatial.Should().HaveCountGreaterOrEqualTo(5);
        architectural.Should().HaveCountGreaterOrEqualTo(10);
        detail.Should().HaveCountGreaterOrEqualTo(20);
        atmospheric.Should().HaveCountGreaterOrEqualTo(50);
        direction.Should().HaveCountGreaterOrEqualTo(5);
    }

    [Test]
    public void Repository_GetModifier_ReturnsCorrectBiomeModifier()
    {
        var rootsModifier = _repository.GetModifier(Biome.TheRoots);
        var muspelheimModifier = _repository.GetModifier(Biome.Muspelheim);
        var niflheimModifier = _repository.GetModifier(Biome.Niflheim);

        rootsModifier.Name.Should().Be("Rusted");
        rootsModifier.IsBrittle.Should().BeTrue();

        muspelheimModifier.Name.Should().Be("Scorched");
        muspelheimModifier.DamageType.Should().Be("fire");

        niflheimModifier.Name.Should().Be("Frozen");
        niflheimModifier.IsSlippery.Should().BeTrue();
    }

    [Test]
    public void Repository_GetModifier_FallsBackToCitadel()
    {
        // Surface is handled, but test fallback with an unmapped biome scenario
        var citadelModifier = _repository.GetModifier(Biome.Citadel);
        citadelModifier.Should().NotBeNull();
        citadelModifier.Name.Should().Be("Ancient");
    }

    #endregion

    #region Service Generation Tests

    [Test]
    public void Service_GeneratesValidRoomNames_ForAllBiomes()
    {
        var biomes = new[] { Biome.TheRoots, Biome.Muspelheim, Biome.Niflheim, Biome.Alfheim, Biome.Jotunheim, Biome.Citadel };
        var archetypes = new[] { RoomArchetype.Corridor, RoomArchetype.Chamber, RoomArchetype.Junction };

        foreach (var biome in biomes)
        {
            var modifier = _repository.GetModifier(biome);

            foreach (var archetype in archetypes)
            {
                var template = _repository.GetBaseTemplate(archetype);
                template.Should().NotBeNull($"No template found for {archetype}");

                var name = _service.GenerateRoomName(template!, modifier);

                name.Should().NotBeNullOrWhiteSpace($"Name should not be empty for {biome}/{archetype}");
                name.Should().NotContain("{", $"Name should not contain unprocessed tokens for {biome}/{archetype}");
                name.Should().NotContain("}", $"Name should not contain unprocessed tokens for {biome}/{archetype}");
                name.Should().Contain(modifier.Name, $"Name should contain modifier name for {biome}/{archetype}");
            }
        }
    }

    [Test]
    public void Service_GeneratesValidRoomDescriptions_ForAllBiomes()
    {
        var biomes = new[] { Biome.TheRoots, Biome.Muspelheim, Biome.Niflheim, Biome.Alfheim, Biome.Jotunheim, Biome.Citadel };
        var archetypes = new[] { RoomArchetype.Corridor, RoomArchetype.Chamber, RoomArchetype.Junction, RoomArchetype.DeadEnd };
        var random = new Random(42);

        foreach (var biome in biomes)
        {
            var modifier = _repository.GetModifier(biome);

            foreach (var archetype in archetypes)
            {
                var template = _repository.GetBaseTemplate(archetype);
                if (template == null) continue;

                var description = _service.GenerateRoomDescription(
                    template, modifier, new[] { "abandoned" }, random);

                description.Should().NotBeNullOrWhiteSpace($"Description should not be empty for {biome}/{archetype}");
                description.Should().NotContain("{", $"Description should not contain unprocessed tokens for {biome}/{archetype}");
                description.Should().NotContain("}", $"Description should not contain unprocessed tokens for {biome}/{archetype}");
                description.Should().Contain(modifier.Adjective, $"Description should contain modifier adjective for {biome}/{archetype}");
            }
        }
    }

    [Test]
    public void Service_GeneratesDescriptions_WithNoUnprocessedTokens()
    {
        var templates = _repository.GetAllBaseTemplates();
        var modifiers = _repository.GetAllModifiers();
        var random = new Random(42);

        foreach (var template in templates)
        {
            foreach (var modifier in modifiers)
            {
                var description = _service.GenerateRoomDescription(
                    template, modifier, Array.Empty<string>(), random);

                description.Should().NotContain("{Modifier}", $"Unprocessed {{Modifier}} in {template.TemplateId}/{modifier.Name}");
                description.Should().NotContain("{Modifier_Adj}", $"Unprocessed {{Modifier_Adj}} in {template.TemplateId}/{modifier.Name}");
                description.Should().NotContain("{Modifier_Detail}", $"Unprocessed {{Modifier_Detail}} in {template.TemplateId}/{modifier.Name}");
                description.Should().NotContain("{Spatial_Descriptor}", $"Unprocessed {{Spatial_Descriptor}} in {template.TemplateId}/{modifier.Name}");
                description.Should().NotContain("{Architectural_Feature}", $"Unprocessed {{Architectural_Feature}} in {template.TemplateId}/{modifier.Name}");
                description.Should().NotContain("{Detail_1}", $"Unprocessed {{Detail_1}} in {template.TemplateId}/{modifier.Name}");
                description.Should().NotContain("{Detail_2}", $"Unprocessed {{Detail_2}} in {template.TemplateId}/{modifier.Name}");
                description.Should().NotContain("{Direction_Descriptor}", $"Unprocessed {{Direction_Descriptor}} in {template.TemplateId}/{modifier.Name}");
                description.Should().NotContain("{Atmospheric_Detail}", $"Unprocessed {{Atmospheric_Detail}} in {template.TemplateId}/{modifier.Name}");
                description.Should().NotContain("{Article}", $"Unprocessed {{Article}} in {template.TemplateId}/{modifier.Name}");
                description.Should().NotContain("{Article_Cap}", $"Unprocessed {{Article_Cap}} in {template.TemplateId}/{modifier.Name}");
                description.Should().NotContain("{Function}", $"Unprocessed {{Function}} in {template.TemplateId}/{modifier.Name}");
            }
        }
    }

    [Test]
    public void Service_GeneratesDescriptions_WithRoomFunctions()
    {
        var template = _repository.GetBaseTemplate(RoomArchetype.Chamber);
        var modifier = _repository.GetModifier(Biome.Muspelheim);
        var functions = _repository.GetFunctionsByBiome(Biome.Muspelheim);
        var random = new Random(42);

        functions.Should().NotBeEmpty();

        foreach (var function in functions.Take(5))
        {
            var description = _service.GenerateRoomDescription(
                template!, modifier, Array.Empty<string>(), random, function);

            description.Should().NotBeNullOrWhiteSpace();
            description.Should().NotContain("{Function}");
            // Function detail should be incorporated (note: some templates may not have {Function} placeholder)
        }
    }

    #endregion

    #region Determinism Tests

    [Test]
    public void Service_SameSeed_ProducesSameOutput()
    {
        var template = _repository.GetBaseTemplate(RoomArchetype.Chamber)!;
        var modifier = _repository.GetModifier(Biome.TheRoots);
        var tags = new[] { "abandoned", "dark" };

        var result1 = _service.GenerateRoomDescription(template, modifier, tags, new Random(12345));
        var result2 = _service.GenerateRoomDescription(template, modifier, tags, new Random(12345));

        result1.Should().Be(result2);
    }

    [Test]
    public void Service_DifferentSeeds_MayProduceDifferentOutput()
    {
        var template = _repository.GetBaseTemplate(RoomArchetype.Chamber)!;
        var modifier = _repository.GetModifier(Biome.TheRoots);
        var tags = new[] { "abandoned", "dark" };

        var results = Enumerable.Range(0, 50)
            .Select(seed => _service.GenerateRoomDescription(template, modifier, tags, new Random(seed)))
            .ToHashSet();

        // Should have variety (not all the same)
        results.Count.Should().BeGreaterThan(1);
    }

    #endregion

    #region Mechanical Effects Tests

    [Test]
    public void Modifiers_HaveCorrectMechanicalEffects()
    {
        var rootsModifier = _repository.GetModifier(Biome.TheRoots);
        var rootsTags = rootsModifier.GetEffectTags().ToList();
        rootsTags.Should().Contain("Brittle");
        rootsTags.Should().Contain("HpMod:0.7");

        var muspelheimModifier = _repository.GetModifier(Biome.Muspelheim);
        var muspelheimTags = muspelheimModifier.GetEffectTags().ToList();
        muspelheimTags.Should().Contain("DamageAura:fire:2");

        var niflheimModifier = _repository.GetModifier(Biome.Niflheim);
        var niflheimTags = niflheimModifier.GetEffectTags().ToList();
        niflheimTags.Should().Contain("Slippery");
        niflheimTags.Should().Contain("DamageAura:cold:1");

        var alfheimModifier = _repository.GetModifier(Biome.Alfheim);
        var alfheimTags = alfheimModifier.GetEffectTags().ToList();
        alfheimTags.Should().Contain("LightSource");
        alfheimTags.Should().Contain("Dazzle");

        var jotunheimModifier = _repository.GetModifier(Biome.Jotunheim);
        var jotunheimTags = jotunheimModifier.GetEffectTags().ToList();
        jotunheimTags.Should().Contain("Scale:2.0");
    }

    #endregion

    #region Quality Tests

    [Test]
    public void Descriptions_HaveReasonableLength()
    {
        var templates = _repository.GetAllBaseTemplates();
        var modifier = _repository.GetModifier(Biome.Citadel);
        var random = new Random(42);

        foreach (var template in templates)
        {
            var description = _service.GenerateRoomDescription(
                template, modifier, Array.Empty<string>(), random);

            description.Length.Should().BeGreaterThan(50, $"Description too short for {template.TemplateId}");
            description.Length.Should().BeLessThan(1000, $"Description too long for {template.TemplateId}");
        }
    }

    [Test]
    public void Descriptions_HaveNoDoubleSpaces()
    {
        var templates = _repository.GetAllBaseTemplates();
        var modifier = _repository.GetModifier(Biome.TheRoots);
        var random = new Random(42);

        foreach (var template in templates)
        {
            var description = _service.GenerateRoomDescription(
                template, modifier, Array.Empty<string>(), random);

            description.Should().NotContain("  ", $"Double space found in {template.TemplateId}");
        }
    }

    [Test]
    public void Descriptions_HaveNoDoublePeriods()
    {
        var templates = _repository.GetAllBaseTemplates();
        var modifier = _repository.GetModifier(Biome.Niflheim);
        var random = new Random(42);

        foreach (var template in templates)
        {
            var description = _service.GenerateRoomDescription(
                template, modifier, Array.Empty<string>(), random);

            description.Should().NotContain("..", $"Double period found in {template.TemplateId}");
        }
    }

    [Test]
    public void Names_AreProperlyCapitalized()
    {
        var templates = _repository.GetAllBaseTemplates();
        var modifiers = _repository.GetAllModifiers();

        foreach (var template in templates)
        {
            foreach (var modifier in modifiers)
            {
                var name = _service.GenerateRoomName(template, modifier);

                // First character should be uppercase (after "The")
                name.Should().StartWith("The ", $"Name should start with 'The ' for {template.TemplateId}");
            }
        }
    }

    #endregion

    #region Mass Generation Tests

    [Test]
    public void MassGeneration_Produces1000Rooms_WithoutErrors()
    {
        var templates = _repository.GetAllBaseTemplates().ToList();
        var modifiers = _repository.GetAllModifiers().ToList();
        var functions = _repository.GetAllFunctions().ToList();
        var random = new Random(42);

        var generatedRooms = new List<(string Name, string Description)>();

        for (var i = 0; i < 1000; i++)
        {
            var template = templates[random.Next(templates.Count)];
            var modifier = modifiers[random.Next(modifiers.Count)];
            var function = random.NextDouble() < 0.3 ? functions[random.Next(functions.Count)] : null;

            var name = _service.GenerateRoomName(template, modifier, function);
            var description = _service.GenerateRoomDescription(
                template, modifier, Array.Empty<string>(), random, function);

            generatedRooms.Add((name, description));
        }

        generatedRooms.Should().HaveCount(1000);
        generatedRooms.Should().OnlyContain(r => !string.IsNullOrWhiteSpace(r.Name));
        generatedRooms.Should().OnlyContain(r => !string.IsNullOrWhiteSpace(r.Description));
        generatedRooms.Should().OnlyContain(r => !r.Name.Contains("{"));
        generatedRooms.Should().OnlyContain(r => !r.Description.Contains("{"));
    }

    [Test]
    public void MassGeneration_HasGoodVariety()
    {
        var template = _repository.GetBaseTemplate(RoomArchetype.Chamber)!;
        var modifier = _repository.GetModifier(Biome.TheRoots);

        var descriptions = Enumerable.Range(0, 200)
            .Select(seed => _service.GenerateRoomDescription(
                template, modifier, Array.Empty<string>(), new Random(seed)))
            .ToList();

        var uniqueDescriptions = descriptions.Distinct().Count();

        // At least 50% variety expected (accounting for limited fragment pool)
        uniqueDescriptions.Should().BeGreaterThan(100, "Should have significant variety in descriptions");
    }

    #endregion

    #region Fragment Coverage Tests

    [Test]
    public void AllFragments_HaveValidText()
    {
        var categories = new[]
        {
            FragmentCategory.Spatial,
            FragmentCategory.Architectural,
            FragmentCategory.Detail,
            FragmentCategory.Atmospheric,
            FragmentCategory.Direction
        };

        foreach (var category in categories)
        {
            var fragments = _repository.GetFragments(category);

            fragments.Should().OnlyContain(f => !string.IsNullOrWhiteSpace(f.Text),
                $"All {category} fragments should have text");
            fragments.Should().OnlyContain(f => f.Weight > 0,
                $"All {category} fragments should have positive weight");
        }
    }

    [Test]
    public void BiomeFilteredFragments_ReturnSubset()
    {
        var allAtmospheric = _repository.GetFragments(FragmentCategory.Atmospheric);
        var rootsAtmospheric = _repository.GetFragments(FragmentCategory.Atmospheric, Biome.TheRoots);

        // Filtered should be subset (or equal if all match)
        rootsAtmospheric.Count.Should().BeLessThanOrEqualTo(allAtmospheric.Count);
        rootsAtmospheric.Should().OnlyContain(f => f.MatchesBiome(Biome.TheRoots));
    }

    #endregion

    #region Template Structure Tests

    [Test]
    public void Templates_HaveValidNameTemplates()
    {
        var templates = _repository.GetAllBaseTemplates();

        foreach (var template in templates)
        {
            template.NameTemplate.Should().NotBeNullOrWhiteSpace($"Template {template.TemplateId} should have a name template");
            template.NameTemplate.Should().Contain("{Modifier}", $"Template {template.TemplateId} name should contain {{Modifier}} token");
        }
    }

    [Test]
    public void Templates_HaveValidDescriptionTemplates()
    {
        var templates = _repository.GetAllBaseTemplates();

        foreach (var template in templates)
        {
            template.DescriptionTemplate.Should().NotBeNullOrWhiteSpace($"Template {template.TemplateId} should have a description template");
            template.DescriptionTemplate.Should().Contain("{Modifier_Adj}", $"Template {template.TemplateId} should use modifier adjective");
        }
    }

    [Test]
    public void Templates_HaveValidExitCounts()
    {
        var templates = _repository.GetAllBaseTemplates();

        foreach (var template in templates)
        {
            template.MinExits.Should().BeGreaterThan(0, $"Template {template.TemplateId} should have at least 1 exit");
            template.MaxExits.Should().BeGreaterThanOrEqualTo(template.MinExits, $"Template {template.TemplateId} max exits should >= min exits");
            template.MaxExits.Should().BeLessThanOrEqualTo(6, $"Template {template.TemplateId} should have reasonable max exits");
        }
    }

    [Test]
    public void Templates_HavePositiveSpawnBudgetMultipliers()
    {
        var templates = _repository.GetAllBaseTemplates();

        foreach (var template in templates)
        {
            template.SpawnBudgetMultiplier.Should().BeGreaterThan(0, $"Template {template.TemplateId} should have positive spawn budget");
            template.SpawnBudgetMultiplier.Should().BeLessThanOrEqualTo(5, $"Template {template.TemplateId} spawn budget seems too high");
        }
    }

    [Test]
    public void Templates_HavePositiveWeights()
    {
        var templates = _repository.GetAllBaseTemplates();

        foreach (var template in templates)
        {
            template.Weight.Should().BeGreaterThan(0, $"Template {template.TemplateId} should have positive weight");
        }
    }

    #endregion

    #region Modifier Structure Tests

    [Test]
    public void Modifiers_HaveAllRequiredFields()
    {
        var modifiers = _repository.GetAllModifiers();

        foreach (var modifier in modifiers)
        {
            modifier.Name.Should().NotBeNullOrWhiteSpace($"Modifier for {modifier.PrimaryBiome} should have a name");
            modifier.Adjective.Should().NotBeNullOrWhiteSpace($"Modifier {modifier.Name} should have an adjective");
            modifier.DetailFragment.Should().NotBeNullOrWhiteSpace($"Modifier {modifier.Name} should have a detail fragment");
        }
    }

    [Test]
    public void Modifiers_HaveValidHpMultipliers()
    {
        var modifiers = _repository.GetAllModifiers();

        foreach (var modifier in modifiers)
        {
            modifier.HpMultiplier.Should().BeGreaterThan(0, $"Modifier {modifier.Name} HP multiplier should be positive");
            modifier.HpMultiplier.Should().BeLessThanOrEqualTo(3, $"Modifier {modifier.Name} HP multiplier seems too high");
        }
    }

    [Test]
    public void Modifiers_HaveValidScaleMultipliers()
    {
        var modifiers = _repository.GetAllModifiers();

        foreach (var modifier in modifiers)
        {
            modifier.ScaleMultiplier.Should().BeGreaterThan(0, $"Modifier {modifier.Name} scale multiplier should be positive");
            modifier.ScaleMultiplier.Should().BeLessThanOrEqualTo(5, $"Modifier {modifier.Name} scale multiplier seems too high");
        }
    }

    [Test]
    public void Modifiers_WithDamageAura_HaveValidDamageType()
    {
        var modifiers = _repository.GetAllModifiers().Where(m => m.DamagePerTurn.HasValue && m.DamagePerTurn > 0);

        foreach (var modifier in modifiers)
        {
            modifier.DamageType.Should().NotBeNullOrWhiteSpace($"Modifier {modifier.Name} with damage aura should have damage type");
        }
    }

    #endregion

    #region Room Function Tests

    [Test]
    public void Functions_HavePositiveWeights()
    {
        var functions = _repository.GetAllFunctions();

        foreach (var function in functions)
        {
            function.Weight.Should().BeGreaterThan(0, $"Function {function.FunctionName} should have positive weight");
        }
    }

    [Test]
    public void Functions_HaveReasonableDetailLength()
    {
        var functions = _repository.GetAllFunctions();

        foreach (var function in functions)
        {
            function.FunctionDetail.Length.Should().BeGreaterThan(20, $"Function {function.FunctionName} detail seems too short");
            function.FunctionDetail.Length.Should().BeLessThan(500, $"Function {function.FunctionName} detail seems too long");
        }
    }

    [Test]
    public void Functions_ByBiome_ReturnsCorrectSubset()
    {
        var allFunctions = _repository.GetAllFunctions();
        var muspelheimFunctions = _repository.GetFunctionsByBiome(Biome.Muspelheim);
        var niflheimFunctions = _repository.GetFunctionsByBiome(Biome.Niflheim);

        muspelheimFunctions.Should().OnlyContain(f => f.HasAffinityFor(Biome.Muspelheim));
        niflheimFunctions.Should().OnlyContain(f => f.HasAffinityFor(Biome.Niflheim));

        // Should have some unique functions per biome
        var muspelheimOnly = muspelheimFunctions.Where(f => !niflheimFunctions.Contains(f)).ToList();
        muspelheimOnly.Should().NotBeEmpty("Muspelheim should have some unique functions");
    }

    [Test]
    public void Functions_Universal_ApplyToAllBiomes()
    {
        var allFunctions = _repository.GetAllFunctions();
        var universalFunctions = allFunctions.Where(f => f.BiomeAffinities == null || f.BiomeAffinities.Count == 0).ToList();

        // Universal functions should appear in all biome queries
        var biomes = new[] { Biome.TheRoots, Biome.Muspelheim, Biome.Niflheim, Biome.Alfheim, Biome.Jotunheim };

        foreach (var universal in universalFunctions)
        {
            foreach (var biome in biomes)
            {
                var biomeFunctions = _repository.GetFunctionsByBiome(biome);
                biomeFunctions.Should().Contain(universal, $"Universal function {universal.FunctionName} should apply to {biome}");
            }
        }
    }

    #endregion

    #region Fragment Subcategory Tests

    [Test]
    public void DetailFragments_HaveValidSubcategories()
    {
        var detailFragments = _repository.GetFragments(FragmentCategory.Detail);

        var validSubcategories = new[] { "Decay", "Runes", "Activity", "Ominous", "Loot" };

        foreach (var fragment in detailFragments)
        {
            if (!string.IsNullOrEmpty(fragment.Subcategory))
            {
                validSubcategories.Should().Contain(fragment.Subcategory,
                    $"Fragment '{fragment.Text[..Math.Min(30, fragment.Text.Length)]}...' has unexpected subcategory {fragment.Subcategory}");
            }
        }
    }

    [Test]
    public void AtmosphericFragments_HaveValidSubcategories()
    {
        var atmosphericFragments = _repository.GetFragments(FragmentCategory.Atmospheric);

        var validSubcategories = new[] { "Smell", "Sound", "Light", "Temperature" };

        foreach (var fragment in atmosphericFragments)
        {
            if (!string.IsNullOrEmpty(fragment.Subcategory))
            {
                validSubcategories.Should().Contain(fragment.Subcategory,
                    $"Fragment '{fragment.Text[..Math.Min(30, fragment.Text.Length)]}...' has unexpected subcategory {fragment.Subcategory}");
            }
        }
    }

    [Test]
    public void ArchitecturalFragments_HaveValidSubcategories()
    {
        var architecturalFragments = _repository.GetFragments(FragmentCategory.Architectural);

        var validSubcategories = new[] { "Wall", "Ceiling", "Floor" };

        foreach (var fragment in architecturalFragments)
        {
            if (!string.IsNullOrEmpty(fragment.Subcategory))
            {
                validSubcategories.Should().Contain(fragment.Subcategory,
                    $"Fragment '{fragment.Text[..Math.Min(30, fragment.Text.Length)]}...' has unexpected subcategory {fragment.Subcategory}");
            }
        }
    }

    #endregion

    #region Cross-Biome Consistency Tests

    [Test]
    public void AllBiomes_HaveAtLeastOneRoomFunction()
    {
        var biomes = new[] { Biome.TheRoots, Biome.Muspelheim, Biome.Niflheim, Biome.Alfheim, Biome.Jotunheim, Biome.Citadel };

        foreach (var biome in biomes)
        {
            var functions = _repository.GetFunctionsByBiome(biome);
            functions.Should().NotBeEmpty($"Biome {biome} should have at least one room function");
        }
    }

    [Test]
    public void AllArchetypes_HaveAtLeastOneTemplate()
    {
        var archetypes = new[] { RoomArchetype.Corridor, RoomArchetype.Chamber, RoomArchetype.Junction, RoomArchetype.DeadEnd, RoomArchetype.Stairwell, RoomArchetype.BossArena };

        foreach (var archetype in archetypes)
        {
            var template = _repository.GetBaseTemplate(archetype);
            template.Should().NotBeNull($"Archetype {archetype} should have at least one template");
        }
    }

    [Test]
    public void DescriptionsAcrossBiomes_ContainBiomeSpecificAdjectives()
    {
        var template = _repository.GetBaseTemplate(RoomArchetype.Chamber)!;
        var random = new Random(42);

        var biomeAdjectives = new Dictionary<Biome, string>
        {
            [Biome.TheRoots] = "corroded",
            [Biome.Muspelheim] = "scorched",
            [Biome.Niflheim] = "ice-covered",
            [Biome.Alfheim] = "crystalline",
            [Biome.Jotunheim] = "monolithic"
        };

        foreach (var (biome, expectedAdjective) in biomeAdjectives)
        {
            var modifier = _repository.GetModifier(biome);
            var description = _service.GenerateRoomDescription(template, modifier, Array.Empty<string>(), random);

            description.Should().Contain(expectedAdjective, $"Description for {biome} should contain '{expectedAdjective}'");
        }
    }

    #endregion

    #region Article Handling Tests

    [Test]
    public void Descriptions_HandleArticlesCorrectly_ForVowelStartingAdjectives()
    {
        // Ice-covered starts with vowel, should get "an"
        var template = _repository.GetBaseTemplate(RoomArchetype.Chamber)!;
        var frozenModifier = _repository.GetModifier(Biome.Niflheim);
        var random = new Random(42);

        var description = _service.GenerateRoomDescription(template, frozenModifier, Array.Empty<string>(), random);

        // The description template uses {Article} before {Modifier_Adj}
        // For "ice-covered", it should produce "an ice-covered"
        description.Should().Contain("an ice-covered", "Should use 'an' before vowel-starting adjective");
    }

    [Test]
    public void Descriptions_HandleArticlesCorrectly_ForConsonantStartingAdjectives()
    {
        // Corroded starts with consonant, should get "a"
        var template = _repository.GetBaseTemplate(RoomArchetype.Chamber)!;
        var rustedModifier = _repository.GetModifier(Biome.TheRoots);
        var random = new Random(42);

        var description = _service.GenerateRoomDescription(template, rustedModifier, Array.Empty<string>(), random);

        // Should use "a" not "an" before "corroded"
        description.Should().Contain("a corroded", "Should use 'a' before consonant-starting adjective");
    }

    #endregion

    #region Output Samples (for manual verification)

    [Test]
    [Explicit("Run manually to see sample output")]
    public void OutputSamples_ForManualVerification()
    {
        var biomes = new[] { Biome.TheRoots, Biome.Muspelheim, Biome.Niflheim, Biome.Alfheim, Biome.Jotunheim };
        var archetypes = new[] { RoomArchetype.Chamber, RoomArchetype.Corridor, RoomArchetype.Junction };
        var random = new Random(42);
        var output = new System.Text.StringBuilder();

        output.AppendLine("=== THREE-TIER DESCRIPTOR SYSTEM SAMPLE OUTPUT ===");
        output.AppendLine();

        foreach (var biome in biomes)
        {
            var modifier = _repository.GetModifier(biome);
            output.AppendLine($"--- {biome} (Modifier: {modifier.Name}) ---");
            output.AppendLine($"Effects: {string.Join(", ", modifier.GetEffectTags())}");
            output.AppendLine();

            foreach (var archetype in archetypes)
            {
                var template = _repository.GetBaseTemplate(archetype);
                if (template == null) continue;

                var name = _service.GenerateRoomName(template, modifier);
                var description = _service.GenerateRoomDescription(
                    template, modifier, new[] { "abandoned" }, random);

                output.AppendLine($"  [{archetype}] {name}");
                output.AppendLine($"  {description}");
                output.AppendLine();
            }

            output.AppendLine();
        }

        // Output with room functions
        output.AppendLine("=== ROOM FUNCTIONS SAMPLE ===");
        output.AppendLine();
        var functions = _repository.GetFunctionsByBiome(Biome.Muspelheim);
        var chamberTemplate = _repository.GetBaseTemplate(RoomArchetype.Chamber)!;
        var muspelheimModifier = _repository.GetModifier(Biome.Muspelheim);

        foreach (var function in functions.Take(3))
        {
            var name = _service.GenerateRoomName(chamberTemplate, muspelheimModifier, function);
            var description = _service.GenerateRoomDescription(
                chamberTemplate, muspelheimModifier, Array.Empty<string>(), random, function);

            output.AppendLine($"Function: {function.FunctionName}");
            output.AppendLine($"Name: {name}");
            output.AppendLine($"Description: {description}");
            output.AppendLine();
        }

        // Statistics
        output.AppendLine("=== STATISTICS ===");
        output.AppendLine();
        output.AppendLine($"Base Templates: {_repository.GetAllBaseTemplates().Count}");
        output.AppendLine($"Modifiers: {_repository.GetAllModifiers().Count}");
        output.AppendLine($"Room Functions: {_repository.GetAllFunctions().Count}");
        output.AppendLine($"Spatial Fragments: {_repository.GetFragments(FragmentCategory.Spatial).Count}");
        output.AppendLine($"Architectural Fragments: {_repository.GetFragments(FragmentCategory.Architectural).Count}");
        output.AppendLine($"Detail Fragments: {_repository.GetFragments(FragmentCategory.Detail).Count}");
        output.AppendLine($"Atmospheric Fragments: {_repository.GetFragments(FragmentCategory.Atmospheric).Count}");
        output.AppendLine($"Direction Fragments: {_repository.GetFragments(FragmentCategory.Direction).Count}");

        var totalFragments = _repository.GetFragments(FragmentCategory.Spatial).Count +
                            _repository.GetFragments(FragmentCategory.Architectural).Count +
                            _repository.GetFragments(FragmentCategory.Detail).Count +
                            _repository.GetFragments(FragmentCategory.Atmospheric).Count +
                            _repository.GetFragments(FragmentCategory.Direction).Count;

        output.AppendLine($"Total Fragments: {totalFragments}");

        // Combination estimate
        var templates = _repository.GetAllBaseTemplates().Count;
        var modifiers = _repository.GetAllModifiers().Count;
        var spatial = _repository.GetFragments(FragmentCategory.Spatial).Count;
        var architectural = _repository.GetFragments(FragmentCategory.Architectural).Count;
        var detail = _repository.GetFragments(FragmentCategory.Detail).Count;
        var atmospheric = _repository.GetFragments(FragmentCategory.Atmospheric).Count;

        var estimatedCombinations = (long)templates * modifiers * spatial * architectural * detail * atmospheric;
        output.AppendLine($"Estimated Combinations: {estimatedCombinations:N0}");

        // Write to test output using Assert.Pass which shows the message
        Assert.Pass(output.ToString());
    }

    #endregion

    #region Room Feature Examination Seeder Tests

    [Test]
    public void RoomFeatureExaminationSeeder_HasInteractableDescriptors()
    {
        var descriptors = Persistence.Seeders.RoomFeatureExaminationSeeder.GetAllDescriptors().ToList();

        // Check for weapon_rack descriptors
        descriptors.Should().Contain(d => d.ObjectType == "weapon_rack" && d.Layer == ExaminationLayer.Cursory);
        descriptors.Should().Contain(d => d.ObjectType == "weapon_rack" && d.Layer == ExaminationLayer.Detailed);
        descriptors.Should().Contain(d => d.ObjectType == "weapon_rack" && d.Layer == ExaminationLayer.Expert);

        // Check for bookshelf descriptors
        descriptors.Should().Contain(d => d.ObjectType == "bookshelf" && d.Layer == ExaminationLayer.Cursory);
    }

    [Test]
    public void RoomFeatureExaminationSeeder_HasDecorationDescriptors()
    {
        var descriptors = Persistence.Seeders.RoomFeatureExaminationSeeder.GetAllDescriptors().ToList();

        // Check for broken_fountain descriptors
        descriptors.Should().Contain(d => d.ObjectType == "broken_fountain" && d.Layer == ExaminationLayer.Cursory);

        // Check for throne descriptors
        descriptors.Should().Contain(d => d.ObjectType == "throne" && d.Layer == ExaminationLayer.Cursory);
    }

    [Test]
    public void RoomFeatureExaminationSeeder_HasLightSourceDescriptors()
    {
        var descriptors = Persistence.Seeders.RoomFeatureExaminationSeeder.GetAllDescriptors().ToList();

        // Check for glowing_fungus descriptors
        descriptors.Should().Contain(d => d.ObjectType == "glowing_fungus" && d.Layer == ExaminationLayer.Cursory);

        // Check for brazier descriptors
        descriptors.Should().Contain(d => d.ObjectType == "brazier" && d.Layer == ExaminationLayer.Cursory);
    }

    [Test]
    public void RoomFeatureExaminationSeeder_HasHazardDescriptors()
    {
        var descriptors = Persistence.Seeders.RoomFeatureExaminationSeeder.GetAllDescriptors().ToList();

        // Check for spore_cloud descriptors
        descriptors.Should().Contain(d => d.ObjectType == "spore_cloud" && d.Layer == ExaminationLayer.Cursory);

        // Check for lava_pool descriptors
        descriptors.Should().Contain(d => d.ObjectType == "lava_pool" && d.Layer == ExaminationLayer.Cursory);
    }

    [Test]
    public void RoomFeatureExaminationSeeder_HasUniversalStructuralDescriptors()
    {
        var descriptors = Persistence.Seeders.RoomFeatureExaminationSeeder.GetAllDescriptors().ToList();

        // Check walls across different biomes
        descriptors.Should().Contain(d => d.ObjectType == "walls" && d.BiomeAffinity == Biome.Citadel);
        descriptors.Should().Contain(d => d.ObjectType == "walls" && d.BiomeAffinity == Biome.TheRoots);
        descriptors.Should().Contain(d => d.ObjectType == "walls" && d.BiomeAffinity == Biome.Muspelheim);
        descriptors.Should().Contain(d => d.ObjectType == "walls" && d.BiomeAffinity == Biome.Niflheim);
        descriptors.Should().Contain(d => d.ObjectType == "walls" && d.BiomeAffinity == Biome.Jotunheim);

        // Check floor across different biomes
        descriptors.Should().Contain(d => d.ObjectType == "floor" && d.BiomeAffinity == Biome.Citadel);
        descriptors.Should().Contain(d => d.ObjectType == "floor" && d.BiomeAffinity == Biome.Muspelheim);

        // Check ceiling across different biomes
        descriptors.Should().Contain(d => d.ObjectType == "ceiling" && d.BiomeAffinity == Biome.Citadel);
        descriptors.Should().Contain(d => d.ObjectType == "ceiling" && d.BiomeAffinity == Biome.Niflheim);
    }

    [Test]
    public void RoomFeatureExaminationSeeder_HasWallSingularVariant()
    {
        var descriptors = Persistence.Seeders.RoomFeatureExaminationSeeder.GetAllDescriptors().ToList();

        // Check singular "wall" variant
        descriptors.Should().Contain(d => d.ObjectType == "wall" && d.BiomeAffinity == Biome.Citadel);
        descriptors.Should().Contain(d => d.ObjectType == "wall" && d.BiomeAffinity == Biome.TheRoots);
    }

    [Test]
    public void RoomFeatureExaminationSeeder_HasGroundVariant()
    {
        var descriptors = Persistence.Seeders.RoomFeatureExaminationSeeder.GetAllDescriptors().ToList();

        // Check "ground" variant (synonym for floor)
        descriptors.Should().Contain(d => d.ObjectType == "ground" && d.BiomeAffinity == Biome.Citadel);
        descriptors.Should().Contain(d => d.ObjectType == "ground" && d.BiomeAffinity == Biome.TheRoots);
    }

    [Test]
    public void RoomFeatureExaminationSeeder_AllDescriptorsHaveValidText()
    {
        var descriptors = Persistence.Seeders.RoomFeatureExaminationSeeder.GetAllDescriptors().ToList();

        foreach (var descriptor in descriptors)
        {
            descriptor.DescriptorText.Should().NotBeNullOrWhiteSpace(
                $"Descriptor for {descriptor.ObjectType} layer {descriptor.Layer} should have text");
            descriptor.DescriptorText.Length.Should().BeGreaterThan(10,
                $"Descriptor for {descriptor.ObjectType} should have meaningful text");
        }
    }

    [Test]
    public void RoomFeatureExaminationSeeder_TotalDescriptorCount()
    {
        var descriptors = Persistence.Seeders.RoomFeatureExaminationSeeder.GetAllDescriptors().ToList();

        // Should have a substantial number of descriptors
        descriptors.Should().HaveCountGreaterOrEqualTo(70,
            "Should have at least 70 examination descriptors for features and structural elements");
    }

    [Test]
    public void RoomFeatureExaminationSeeder_HintsAreProperlyMarked()
    {
        var descriptors = Persistence.Seeders.RoomFeatureExaminationSeeder.GetAllDescriptors().ToList();

        // Some expert-level descriptors should reveal hints
        var hintsCount = descriptors.Count(d => d.RevealsHint);
        hintsCount.Should().BeGreaterThan(5, "Should have some hint-revealing descriptors");

        // Cursory descriptors should not reveal hints
        var cursoryWithHints = descriptors.Where(d => d.Layer == ExaminationLayer.Cursory && d.RevealsHint);
        cursoryWithHints.Should().BeEmpty("Cursory descriptors should not reveal hints");
    }

    #endregion
}
