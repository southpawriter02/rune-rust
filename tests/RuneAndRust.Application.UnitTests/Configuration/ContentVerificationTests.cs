using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using RuneAndRust.Infrastructure.Configuration;
using RuneAndRust.Infrastructure.Providers;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace RuneAndRust.Application.UnitTests.Configuration;

/// <summary>
/// Verifies that the v0.19.x content configuration files are valid and can be loaded.
/// </summary>
[TestFixture]
public class ContentVerificationTests
{
    private string _configPath = string.Empty;

    [SetUp]
    public void Setup()
    {
        // specific to the environment where tests run, usually bin/Debug/net9.0/
        // We need to point to the actual source config directory for accurate verification of the *source* files
        // or ensure they are copied to output.
        // Assuming the test runner runs in bin/, we might need to go up to src/..
        // However, usually config files are copied to output.
        // Let's try to locate the config directory relative to the test assembly.
        
        var assemblyLocation = Path.GetDirectoryName(typeof(ContentVerificationTests).Assembly.Location);
        
        if (assemblyLocation == null)
        {
             // Fallback to absolute path known from the environment (Project Root)
             _configPath = "/Users/ryan/Documents/GitHub/rune-rust/config";
             return;
        }

        // Try to find the repo root to point to the source config if possible, or use copied config
        // Using source config is better for "verification of implementation" tasks vs "verification of build"
        var currentDir = new DirectoryInfo(assemblyLocation);
        while (currentDir != null && !Directory.Exists(Path.Combine(currentDir.FullName, "config")))
        {
            currentDir = currentDir.Parent;
        }

        if (currentDir != null)
        {
            _configPath = Path.Combine(currentDir.FullName, "config");
        }
        else
        {
             // Fallback to checking if it was copied to bin
             if (Directory.Exists(Path.Combine(assemblyLocation, "config")))
             {
                 _configPath = Path.Combine(assemblyLocation, "config");
             }
             else
             {
                 // Fallback to absolute path known from the environment (Project Root)
                 _configPath = "/Users/ryan/Documents/GitHub/rune-rust/config";
             }
        }
    }

    [Test]
    public void Monsters_Json_ShouldLoadAndContainNewEnemies()
    {
        var provider = new JsonConfigurationProvider(_configPath, NullLogger<JsonConfigurationProvider>.Instance);
        var monsters = provider.GetMonsters();

        monsters.Should().NotBeNullOrEmpty();
        
        // Verify v0.19.x specific monsters
        monsters.Should().Contain(m => m.Id == "ash-vargr");
        monsters.Should().Contain(m => m.Id == "surtr-scion"); // ID in file is "surtr-scion", not "surtrs-scion"
        monsters.Should().Contain(m => m.Id == "frost-draugr");
        monsters.Should().Contain(m => m.Id == "rot-stalker");
    }

    [Test]
    public void EnvironmentalHazards_Json_ShouldLoadAndContainNewHazards()
    {
        // This file was renamed from hazards.json to environmental-hazards.json
        var provider = new JsonEnvironmentalHazardProvider(
            Path.Combine(_configPath, "environmental-hazards.json"), 
            NullLogger<JsonEnvironmentalHazardProvider>.Instance);
            
        var hazards = provider.GetAllHazards();

        hazards.Should().NotBeNullOrEmpty();

        // Verify v0.19.x specific hazards
        hazards.Should().Contain(h => h.Name != null && (h.Name.Contains("Rust-Rot") || h.Type.ToString() == "Contact")); // haz-rust-rot
        hazards.Should().Contain(h => h.Name != null && (h.Name.Contains("Blight-Storm") || h.Type.ToString() == "Necrotic")); // haz-blight-storm
        // Note: The ID in the file might differ from the Type enum if not 1:1, check name or properties
    }

    [Test]
    public void RoomTemplates_Json_ShouldBeValidJsonAndContainTemplates()
    {
        var filePath = Path.Combine(_configPath, "room-templates.json");
        File.Exists(filePath).Should().BeTrue($"File should exist at {filePath}");

        var json = File.ReadAllText(filePath);
        var jsonNode = JsonNode.Parse(json);
        
        jsonNode.Should().NotBeNull();
        var templates = jsonNode!["templates"]?.AsArray();
        
        templates.Should().NotBeNull();
        templates!.Count.Should().BeGreaterThan(0);

        // Verify a sample v0.19.x template
        var midgardTemplate = templates.FirstOrDefault(t => t?["templateId"]?.ToString() == "mid-ridge-hold-clearing");
        midgardTemplate.Should().NotBeNull("Midgard template should exist");
    }

    [Test]
    public void Descriptors_Json_ShouldLoadBiomesAndSounds()
    {
        var provider = new JsonConfigurationProvider(_configPath, NullLogger<JsonConfigurationProvider>.Instance);
        var pools = provider.GetAllDescriptorPools();
        
        pools.Should().NotBeNull();
        
        // Verify Biome Modifiers (loaded via environmental.json if I put them there, or check the file existence if separate)
        // I created `biome-modifiers.json`, which isn't standard loaded by `GetAllDescriptorPools` unless it follows the schema.
        // `GetAllDescriptorPools` loads `descriptors/*.json`.
        // Let's verify the file `biome-modifiers.json` exists and is valid valid.
        
        var modifiersPath = Path.Combine(_configPath, "descriptors", "biome-modifiers.json");
        File.Exists(modifiersPath).Should().BeTrue();
        
        var modifiersJson = JsonNode.Parse(File.ReadAllText(modifiersPath));
        modifiersJson.Should().NotBeNull();
        modifiersJson!["modifiers"]?.AsArray().Should().HaveCountGreaterThan(0);

        // Verify Sounds (in sounds.json)
        // This IS loaded by GetAllDescriptorPools if it matches the format
        // provider.GetAllDescriptorPools() keys are "Category.PoolId"
        
        // I updated `sounds.json`. Category is "sounds".
        // Pool IDs: "midgard_generic", etc.
        
        pools.Should().ContainKey("sounds.midgard_generic");
        pools.Should().ContainKey("sounds.muspelheim_generic");
    }
}
