// ------------------------------------------------------------------------------
// <copyright file="PsychologicalDescriptorSchemaTests.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Unit tests for psychological-descriptors.schema.json validation.
// Verifies schema structure, stress descriptor validation, trauma descriptor validation,
// corruption descriptor validation, biome pressure validation, and recovery validation.
// Part of v0.14.9d implementation.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Infrastructure.IntegrationTests.Schemas;

using FluentAssertions;
using NJsonSchema;
using NUnit.Framework;

/// <summary>
/// Unit tests validating the psychological-descriptors.schema.json JSON Schema.
/// Tests ensure the schema correctly validates psychological state configuration files
/// including stress, trauma, corruption, biome pressure, and recovery descriptors.
/// Validates 5 stress thresholds, 5 trauma types, 6 corruption thresholds, 6 biomes, and 6 recovery types.
/// </summary>
/// <remarks>
/// <para>
/// The Psychological descriptor schema provides 12 definitions:
/// <list type="bullet">
/// <item><description>StressDescriptor - Stress manifestation descriptions</description></item>
/// <item><description>TraumaDescriptor - Trauma effect descriptions</description></item>
/// <item><description>CorruptionDescriptor - Corruption progression descriptions</description></item>
/// <item><description>BiomePressureDescriptor - Environmental pressure descriptions</description></item>
/// <item><description>RecoveryDescriptor - Recovery moment descriptions</description></item>
/// <item><description>StressThreshold - 5 stress thresholds enum</description></item>
/// <item><description>StressManifestation - 3 manifestation types enum</description></item>
/// <item><description>TraumaType - 5 trauma types enum</description></item>
/// <item><description>TraumaIntensity - 3 intensity levels enum</description></item>
/// <item><description>CorruptionThreshold - 6 corruption thresholds enum</description></item>
/// <item><description>CorruptionManifestation - 5 manifestation types enum</description></item>
/// <item><description>Biome - 6 biomes enum</description></item>
/// <item><description>RecoveryType - 6 recovery types enum</description></item>
/// <item><description>Intensity - 3 intensity levels enum</description></item>
/// </list>
/// </para>
/// </remarks>
[TestFixture]
public class PsychologicalDescriptorSchemaTests
{
    /// <summary>
    /// Path to the psychological descriptor schema file relative to the test execution directory.
    /// </summary>
    private const string SchemaPath = "../../../../../config/schemas/psychological-descriptors.schema.json";

    /// <summary>
    /// Path to the stress-descriptors.json configuration file for validation.
    /// </summary>
    private const string StressDescriptorsPath = "../../../../../config/psychological-descriptors/stress-descriptors.json";

    /// <summary>
    /// Path to the trauma-descriptors.json configuration file for validation.
    /// </summary>
    private const string TraumaDescriptorsPath = "../../../../../config/psychological-descriptors/trauma-descriptors.json";

    /// <summary>
    /// Path to the corruption-descriptors.json configuration file for validation.
    /// </summary>
    private const string CorruptionDescriptorsPath = "../../../../../config/psychological-descriptors/corruption-descriptors.json";

    /// <summary>
    /// Path to the biome-pressure-descriptors.json configuration file for validation.
    /// </summary>
    private const string BiomePressureDescriptorsPath = "../../../../../config/psychological-descriptors/biome-pressure-descriptors.json";

    /// <summary>
    /// Path to the recovery-descriptors.json configuration file for validation.
    /// </summary>
    private const string RecoveryDescriptorsPath = "../../../../../config/psychological-descriptors/recovery-descriptors.json";

    /// <summary>
    /// Loaded JSON Schema for psychological descriptor definitions.
    /// </summary>
    private JsonSchema _schema = null!;

    /// <summary>
    /// Sets up the test fixture by loading the psychological descriptor schema.
    /// </summary>
    [SetUp]
    public async Task SetUp()
    {
        // Arrange: Load the schema from the file system with full path for reference resolution
        var fullSchemaPath = Path.GetFullPath(SchemaPath);
        _schema = await JsonSchema.FromFileAsync(fullSchemaPath);
    }

    #region PSY-001: Schema File Loads and Contains Expected Definitions

    /// <summary>
    /// PSY-001: Verifies that psychological-descriptors.schema.json loads and parses as valid JSON Schema.
    /// </summary>
    [Test]
    public void PsychologicalDescriptorSchema_LoadsAndParses_Successfully()
    {
        // Assert: Schema should parse successfully
        _schema.Should().NotBeNull("schema file should parse successfully");
        _schema.Title.Should().Be("Psychological Descriptor Configuration", "schema title should match");
        _schema.Type.Should().Be(JsonObjectType.Object, "schema type should be object");

        // Assert: All definitions should be present
        _schema.Definitions.Should().ContainKey("StressDescriptor", "should define StressDescriptor");
        _schema.Definitions.Should().ContainKey("TraumaDescriptor", "should define TraumaDescriptor");
        _schema.Definitions.Should().ContainKey("CorruptionDescriptor", "should define CorruptionDescriptor");
        _schema.Definitions.Should().ContainKey("BiomePressureDescriptor", "should define BiomePressureDescriptor");
        _schema.Definitions.Should().ContainKey("RecoveryDescriptor", "should define RecoveryDescriptor");
        _schema.Definitions.Should().ContainKey("StressThreshold", "should define StressThreshold");
        _schema.Definitions.Should().ContainKey("TraumaType", "should define TraumaType");
        _schema.Definitions.Should().ContainKey("CorruptionThreshold", "should define CorruptionThreshold");
        _schema.Definitions.Should().ContainKey("Biome", "should define Biome");
        _schema.Definitions.Should().ContainKey("RecoveryType", "should define RecoveryType");
        _schema.Definitions.Should().ContainKey("Intensity", "should define Intensity");
    }

    #endregion

    #region PSY-002: Existing Configuration Files Pass Validation

    /// <summary>
    /// PSY-002: Verifies that stress-descriptors.json passes schema validation.
    /// </summary>
    [Test]
    public async Task PsychologicalDescriptorSchema_StressDescriptors_PassesValidation()
    {
        // Arrange: Load the actual stress-descriptors.json file
        var jsonContent = await File.ReadAllTextAsync(StressDescriptorsPath);

        // Act: Validate the JSON content against the schema
        var errors = _schema.Validate(jsonContent);

        // Assert: Schema should validate successfully
        errors.Should().BeEmpty(
            "existing stress-descriptors.json should validate against schema without errors");
    }

    /// <summary>
    /// PSY-002: Verifies that trauma-descriptors.json passes schema validation.
    /// </summary>
    [Test]
    public async Task PsychologicalDescriptorSchema_TraumaDescriptors_PassesValidation()
    {
        // Arrange: Load the actual trauma-descriptors.json file
        var jsonContent = await File.ReadAllTextAsync(TraumaDescriptorsPath);

        // Act: Validate the JSON content against the schema
        var errors = _schema.Validate(jsonContent);

        // Assert: Schema should validate successfully
        errors.Should().BeEmpty(
            "existing trauma-descriptors.json should validate against schema without errors");
    }

    /// <summary>
    /// PSY-002: Verifies that corruption-descriptors.json passes schema validation.
    /// </summary>
    [Test]
    public async Task PsychologicalDescriptorSchema_CorruptionDescriptors_PassesValidation()
    {
        // Arrange: Load the actual corruption-descriptors.json file
        var jsonContent = await File.ReadAllTextAsync(CorruptionDescriptorsPath);

        // Act: Validate the JSON content against the schema
        var errors = _schema.Validate(jsonContent);

        // Assert: Schema should validate successfully
        errors.Should().BeEmpty(
            "existing corruption-descriptors.json should validate against schema without errors");
    }

    /// <summary>
    /// PSY-002: Verifies that biome-pressure-descriptors.json passes schema validation.
    /// </summary>
    [Test]
    public async Task PsychologicalDescriptorSchema_BiomePressureDescriptors_PassesValidation()
    {
        // Arrange: Load the actual biome-pressure-descriptors.json file
        var jsonContent = await File.ReadAllTextAsync(BiomePressureDescriptorsPath);

        // Act: Validate the JSON content against the schema
        var errors = _schema.Validate(jsonContent);

        // Assert: Schema should validate successfully
        errors.Should().BeEmpty(
            "existing biome-pressure-descriptors.json should validate against schema without errors");
    }

    /// <summary>
    /// PSY-002: Verifies that recovery-descriptors.json passes schema validation.
    /// </summary>
    [Test]
    public async Task PsychologicalDescriptorSchema_RecoveryDescriptors_PassesValidation()
    {
        // Arrange: Load the actual recovery-descriptors.json file
        var jsonContent = await File.ReadAllTextAsync(RecoveryDescriptorsPath);

        // Act: Validate the JSON content against the schema
        var errors = _schema.Validate(jsonContent);

        // Assert: Schema should validate successfully
        errors.Should().BeEmpty(
            "existing recovery-descriptors.json should validate against schema without errors");
    }

    #endregion

    #region PSY-003: StressDescriptor Validation

    /// <summary>
    /// PSY-003: Verifies that minimal valid stress descriptor passes validation.
    /// </summary>
    [Test]
    public void StressDescriptor_MinimalValid_PassesValidation()
    {
        // Arrange: Minimal valid stress configuration
        var validJson = """
        {
            "version": "1.0.0",
            "category": "stress",
            "pools": {
                "stress_critical_physical": [
                    {
                        "id": "test_001",
                        "text": "Your heart pounds in your ears.",
                        "threshold": "Critical",
                        "manifestation": "Physical"
                    }
                ]
            }
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(validJson);

        // Assert: Validation should pass
        errors.Should().BeEmpty(
            "minimal stress descriptor should validate successfully");
    }

    /// <summary>
    /// PSY-003: Verifies that complete stress descriptor passes validation.
    /// </summary>
    [Test]
    public void StressDescriptor_Complete_PassesValidation()
    {
        // Arrange: Complete stress configuration with all optional fields
        var validJson = """
        {
            "version": "1.0.0",
            "category": "stress",
            "pools": {
                "stress_critical_physical": [
                    {
                        "id": "stress_critical_physical_001",
                        "text": "Your heart pounds in your ears, drowning out everything else.",
                        "weight": 10,
                        "threshold": "Critical",
                        "manifestation": "Physical",
                        "intensity": "Oppressive",
                        "tags": ["Panic", "Heartbeat"]
                    }
                ]
            }
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(validJson);

        // Assert: Validation should pass
        errors.Should().BeEmpty(
            "complete stress descriptor should validate successfully");
    }

    /// <summary>
    /// PSY-003: Verifies that all valid stress thresholds pass validation.
    /// </summary>
    [Test]
    [TestCase("Minimal")]
    [TestCase("Mounting")]
    [TestCase("Critical")]
    [TestCase("Breaking")]
    [TestCase("Broken")]
    public void StressDescriptor_ValidThreshold_PassesValidation(string threshold)
    {
        // Arrange: Descriptor with valid threshold
        var validJson = $$"""
        {
            "version": "1.0.0",
            "category": "stress",
            "pools": {
                "test_pool": [
                    {
                        "id": "test_001",
                        "text": "Test...",
                        "threshold": "{{threshold}}",
                        "manifestation": "Physical"
                    }
                ]
            }
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(validJson);

        // Assert: Validation should pass
        errors.Should().BeEmpty(
            $"threshold '{threshold}' should pass validation");
    }

    /// <summary>
    /// PSY-003: Verifies that all valid stress manifestations pass validation.
    /// </summary>
    [Test]
    [TestCase("Physical")]
    [TestCase("Mental")]
    [TestCase("Behavioral")]
    public void StressDescriptor_ValidManifestation_PassesValidation(string manifestation)
    {
        // Arrange: Descriptor with valid manifestation
        var validJson = $$"""
        {
            "version": "1.0.0",
            "category": "stress",
            "pools": {
                "test_pool": [
                    {
                        "id": "test_001",
                        "text": "Test...",
                        "threshold": "Critical",
                        "manifestation": "{{manifestation}}"
                    }
                ]
            }
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(validJson);

        // Assert: Validation should pass
        errors.Should().BeEmpty(
            $"manifestation '{manifestation}' should pass validation");
    }

    /// <summary>
    /// PSY-003: Verifies that invalid threshold fails validation.
    /// </summary>
    [Test]
    public void StressDescriptor_InvalidThreshold_FailsValidation()
    {
        // Arrange: Descriptor with invalid threshold
        var invalidJson = """
        {
            "version": "1.0.0",
            "category": "stress",
            "pools": {
                "test_pool": [
                    {
                        "id": "test_001",
                        "text": "Test...",
                        "threshold": "InvalidThreshold",
                        "manifestation": "Physical"
                    }
                ]
            }
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail
        errors.Should().NotBeEmpty(
            "invalid threshold should fail validation");
    }

    /// <summary>
    /// PSY-003: Verifies that invalid manifestation fails validation.
    /// </summary>
    [Test]
    public void StressDescriptor_InvalidManifestation_FailsValidation()
    {
        // Arrange: Descriptor with invalid manifestation
        var invalidJson = """
        {
            "version": "1.0.0",
            "category": "stress",
            "pools": {
                "test_pool": [
                    {
                        "id": "test_001",
                        "text": "Test...",
                        "threshold": "Critical",
                        "manifestation": "InvalidManifestation"
                    }
                ]
            }
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail
        errors.Should().NotBeEmpty(
            "invalid manifestation should fail validation");
    }

    #endregion

    #region PSY-004: TraumaDescriptor Validation

    /// <summary>
    /// PSY-004: Verifies that minimal valid trauma descriptor passes validation.
    /// </summary>
    [Test]
    public void TraumaDescriptor_MinimalValid_PassesValidation()
    {
        // Arrange: Minimal valid trauma configuration
        var validJson = """
        {
            "version": "1.0.0",
            "category": "trauma",
            "pools": {
                "trauma_flashback_severe": [
                    {
                        "id": "test_001",
                        "text": "You are there again. Not here. THERE.",
                        "traumaType": "Flashback",
                        "intensity": "Severe"
                    }
                ]
            }
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(validJson);

        // Assert: Validation should pass
        errors.Should().BeEmpty(
            "minimal trauma descriptor should validate successfully");
    }

    /// <summary>
    /// PSY-004: Verifies that all valid trauma types pass validation.
    /// </summary>
    [Test]
    [TestCase("Flashback")]
    [TestCase("Panic")]
    [TestCase("Dissociation")]
    [TestCase("Hypervigilance")]
    [TestCase("Avoidance")]
    public void TraumaDescriptor_ValidTraumaType_PassesValidation(string traumaType)
    {
        // Arrange: Descriptor with valid trauma type
        var validJson = $$"""
        {
            "version": "1.0.0",
            "category": "trauma",
            "pools": {
                "test_pool": [
                    {
                        "id": "test_001",
                        "text": "Test...",
                        "traumaType": "{{traumaType}}",
                        "intensity": "Moderate"
                    }
                ]
            }
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(validJson);

        // Assert: Validation should pass
        errors.Should().BeEmpty(
            $"traumaType '{traumaType}' should pass validation");
    }

    /// <summary>
    /// PSY-004: Verifies that all valid trauma intensities pass validation.
    /// </summary>
    [Test]
    [TestCase("Mild")]
    [TestCase("Moderate")]
    [TestCase("Severe")]
    public void TraumaDescriptor_ValidIntensity_PassesValidation(string intensity)
    {
        // Arrange: Descriptor with valid intensity
        var validJson = $$"""
        {
            "version": "1.0.0",
            "category": "trauma",
            "pools": {
                "test_pool": [
                    {
                        "id": "test_001",
                        "text": "Test...",
                        "traumaType": "Flashback",
                        "intensity": "{{intensity}}"
                    }
                ]
            }
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(validJson);

        // Assert: Validation should pass
        errors.Should().BeEmpty(
            $"intensity '{intensity}' should pass validation");
    }

    /// <summary>
    /// PSY-004: Verifies that trauma descriptor missing required intensity fails validation.
    /// </summary>
    [Test]
    public void TraumaDescriptor_MissingIntensity_FailsValidation()
    {
        // Arrange: Descriptor missing required 'intensity'
        var invalidJson = """
        {
            "version": "1.0.0",
            "category": "trauma",
            "pools": {
                "test_pool": [
                    {
                        "id": "test_001",
                        "text": "Test...",
                        "traumaType": "Flashback"
                    }
                ]
            }
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail
        errors.Should().NotBeEmpty(
            "trauma descriptor missing 'intensity' should fail validation");
    }

    /// <summary>
    /// PSY-004: Verifies that invalid trauma type fails validation.
    /// </summary>
    [Test]
    public void TraumaDescriptor_InvalidTraumaType_FailsValidation()
    {
        // Arrange: Descriptor with invalid trauma type
        var invalidJson = """
        {
            "version": "1.0.0",
            "category": "trauma",
            "pools": {
                "test_pool": [
                    {
                        "id": "test_001",
                        "text": "Test...",
                        "traumaType": "InvalidType",
                        "intensity": "Moderate"
                    }
                ]
            }
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail
        errors.Should().NotBeEmpty(
            "invalid traumaType should fail validation");
    }

    #endregion

    #region PSY-005: CorruptionDescriptor Validation

    /// <summary>
    /// PSY-005: Verifies that minimal valid corruption descriptor passes validation.
    /// </summary>
    [Test]
    public void CorruptionDescriptor_MinimalValid_PassesValidation()
    {
        // Arrange: Minimal valid corruption configuration
        var validJson = """
        {
            "version": "1.0.0",
            "category": "corruption",
            "pools": {
                "corruption_tainted_physical": [
                    {
                        "id": "test_001",
                        "text": "Veins show through the skin, darker than they should be.",
                        "threshold": "Tainted",
                        "manifestation": "Physical"
                    }
                ]
            }
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(validJson);

        // Assert: Validation should pass
        errors.Should().BeEmpty(
            "minimal corruption descriptor should validate successfully");
    }

    /// <summary>
    /// PSY-005: Verifies that all valid corruption thresholds pass validation.
    /// </summary>
    [Test]
    [TestCase("Clean")]
    [TestCase("Touched")]
    [TestCase("Tainted")]
    [TestCase("Corrupted")]
    [TestCase("Lost")]
    [TestCase("Forlorn")]
    public void CorruptionDescriptor_ValidThreshold_PassesValidation(string threshold)
    {
        // Arrange: Descriptor with valid threshold
        var validJson = $$"""
        {
            "version": "1.0.0",
            "category": "corruption",
            "pools": {
                "test_pool": [
                    {
                        "id": "test_001",
                        "text": "Test...",
                        "threshold": "{{threshold}}",
                        "manifestation": "Physical"
                    }
                ]
            }
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(validJson);

        // Assert: Validation should pass
        errors.Should().BeEmpty(
            $"threshold '{threshold}' should pass validation");
    }

    /// <summary>
    /// PSY-005: Verifies that all valid corruption manifestations pass validation.
    /// </summary>
    [Test]
    [TestCase("Physical")]
    [TestCase("Mental")]
    [TestCase("Social")]
    [TestCase("Urgent")]
    [TestCase("Final")]
    public void CorruptionDescriptor_ValidManifestation_PassesValidation(string manifestation)
    {
        // Arrange: Descriptor with valid manifestation
        var validJson = $$"""
        {
            "version": "1.0.0",
            "category": "corruption",
            "pools": {
                "test_pool": [
                    {
                        "id": "test_001",
                        "text": "Test...",
                        "threshold": "Tainted",
                        "manifestation": "{{manifestation}}"
                    }
                ]
            }
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(validJson);

        // Assert: Validation should pass
        errors.Should().BeEmpty(
            $"manifestation '{manifestation}' should pass validation");
    }

    /// <summary>
    /// PSY-005: Verifies that invalid corruption threshold fails validation.
    /// </summary>
    [Test]
    public void CorruptionDescriptor_InvalidThreshold_FailsValidation()
    {
        // Arrange: Descriptor with invalid threshold
        var invalidJson = """
        {
            "version": "1.0.0",
            "category": "corruption",
            "pools": {
                "test_pool": [
                    {
                        "id": "test_001",
                        "text": "Test...",
                        "threshold": "InvalidThreshold",
                        "manifestation": "Physical"
                    }
                ]
            }
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail
        errors.Should().NotBeEmpty(
            "invalid threshold should fail validation");
    }

    #endregion

    #region PSY-006: BiomePressureDescriptor Validation

    /// <summary>
    /// PSY-006: Verifies that minimal valid biome pressure descriptor passes validation.
    /// </summary>
    [Test]
    public void BiomePressureDescriptor_MinimalValid_PassesValidation()
    {
        // Arrange: Minimal valid biome pressure configuration
        var validJson = """
        {
            "version": "1.0.0",
            "category": "biome-pressure",
            "pools": {
                "the_roots_isolation": [
                    {
                        "id": "test_001",
                        "text": "How long since you have seen another person?",
                        "biome": "The_Roots",
                        "pressureType": "Isolation"
                    }
                ]
            }
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(validJson);

        // Assert: Validation should pass
        errors.Should().BeEmpty(
            "minimal biome pressure descriptor should validate successfully");
    }

    /// <summary>
    /// PSY-006: Verifies that all valid biomes pass validation.
    /// </summary>
    [Test]
    [TestCase("The_Roots")]
    [TestCase("Muspelheim")]
    [TestCase("Niflheim")]
    [TestCase("Svartalfheim")]
    [TestCase("Vanaheim")]
    [TestCase("Jotunheim")]
    public void BiomePressureDescriptor_ValidBiome_PassesValidation(string biome)
    {
        // Arrange: Descriptor with valid biome
        var validJson = $$"""
        {
            "version": "1.0.0",
            "category": "biome-pressure",
            "pools": {
                "test_pool": [
                    {
                        "id": "test_001",
                        "text": "Test...",
                        "biome": "{{biome}}",
                        "pressureType": "TestPressure"
                    }
                ]
            }
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(validJson);

        // Assert: Validation should pass
        errors.Should().BeEmpty(
            $"biome '{biome}' should pass validation");
    }

    /// <summary>
    /// PSY-006: Verifies that biome pressure descriptor missing pressureType fails validation.
    /// </summary>
    [Test]
    public void BiomePressureDescriptor_MissingPressureType_FailsValidation()
    {
        // Arrange: Descriptor missing required 'pressureType'
        var invalidJson = """
        {
            "version": "1.0.0",
            "category": "biome-pressure",
            "pools": {
                "test_pool": [
                    {
                        "id": "test_001",
                        "text": "Test...",
                        "biome": "The_Roots"
                    }
                ]
            }
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail
        errors.Should().NotBeEmpty(
            "biome pressure descriptor missing 'pressureType' should fail validation");
    }

    /// <summary>
    /// PSY-006: Verifies that invalid biome fails validation.
    /// </summary>
    [Test]
    public void BiomePressureDescriptor_InvalidBiome_FailsValidation()
    {
        // Arrange: Descriptor with invalid biome
        var invalidJson = """
        {
            "version": "1.0.0",
            "category": "biome-pressure",
            "pools": {
                "test_pool": [
                    {
                        "id": "test_001",
                        "text": "Test...",
                        "biome": "InvalidBiome",
                        "pressureType": "Isolation"
                    }
                ]
            }
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail
        errors.Should().NotBeEmpty(
            "invalid biome should fail validation");
    }

    #endregion

    #region PSY-007: RecoveryDescriptor Validation

    /// <summary>
    /// PSY-007: Verifies that minimal valid recovery descriptor passes validation.
    /// </summary>
    [Test]
    public void RecoveryDescriptor_MinimalValid_PassesValidation()
    {
        // Arrange: Minimal valid recovery configuration
        var validJson = """
        {
            "version": "1.0.0",
            "category": "recovery",
            "pools": {
                "recovery_rest": [
                    {
                        "id": "test_001",
                        "text": "Breathing slows. The shaking stops.",
                        "recoveryType": "Rest"
                    }
                ]
            }
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(validJson);

        // Assert: Validation should pass
        errors.Should().BeEmpty(
            "minimal recovery descriptor should validate successfully");
    }

    /// <summary>
    /// PSY-007: Verifies that all valid recovery types pass validation.
    /// </summary>
    [Test]
    [TestCase("Rest")]
    [TestCase("Social")]
    [TestCase("Grounding")]
    [TestCase("Support")]
    [TestCase("Milestone")]
    [TestCase("Acceptance")]
    public void RecoveryDescriptor_ValidRecoveryType_PassesValidation(string recoveryType)
    {
        // Arrange: Descriptor with valid recovery type
        var validJson = $$"""
        {
            "version": "1.0.0",
            "category": "recovery",
            "pools": {
                "test_pool": [
                    {
                        "id": "test_001",
                        "text": "Test...",
                        "recoveryType": "{{recoveryType}}"
                    }
                ]
            }
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(validJson);

        // Assert: Validation should pass
        errors.Should().BeEmpty(
            $"recoveryType '{recoveryType}' should pass validation");
    }

    /// <summary>
    /// PSY-007: Verifies that recovery descriptor missing recoveryType fails validation.
    /// </summary>
    [Test]
    public void RecoveryDescriptor_MissingRecoveryType_FailsValidation()
    {
        // Arrange: Descriptor missing required 'recoveryType'
        var invalidJson = """
        {
            "version": "1.0.0",
            "category": "recovery",
            "pools": {
                "test_pool": [
                    {
                        "id": "test_001",
                        "text": "Test..."
                    }
                ]
            }
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail
        errors.Should().NotBeEmpty(
            "recovery descriptor missing 'recoveryType' should fail validation");
    }

    /// <summary>
    /// PSY-007: Verifies that invalid recovery type fails validation.
    /// </summary>
    [Test]
    public void RecoveryDescriptor_InvalidRecoveryType_FailsValidation()
    {
        // Arrange: Descriptor with invalid recovery type
        var invalidJson = """
        {
            "version": "1.0.0",
            "category": "recovery",
            "pools": {
                "test_pool": [
                    {
                        "id": "test_001",
                        "text": "Test...",
                        "recoveryType": "InvalidRecovery"
                    }
                ]
            }
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail
        errors.Should().NotBeEmpty(
            "invalid recoveryType should fail validation");
    }

    #endregion

    #region PSY-008: Configuration Requirements Validation

    /// <summary>
    /// PSY-008: Verifies that all valid categories pass validation.
    /// </summary>
    [Test]
    [TestCase("stress")]
    [TestCase("trauma")]
    [TestCase("corruption")]
    [TestCase("biome-pressure")]
    [TestCase("recovery")]
    public void Configuration_ValidCategory_PassesValidation(string category)
    {
        // Arrange: Configuration with valid category (using stress descriptor as general test)
        var validJson = $$"""
        {
            "version": "1.0.0",
            "category": "{{category}}",
            "pools": {
                "test_pool": [
                    {
                        "id": "test_001",
                        "text": "Test...",
                        "threshold": "Critical",
                        "manifestation": "Physical"
                    }
                ]
            }
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(validJson);

        // Assert: Validation should pass
        errors.Should().BeEmpty(
            $"category '{category}' should pass validation");
    }

    /// <summary>
    /// PSY-008: Verifies that invalid category fails validation.
    /// </summary>
    [Test]
    public void Configuration_InvalidCategory_FailsValidation()
    {
        // Arrange: Configuration with invalid category
        var invalidJson = """
        {
            "version": "1.0.0",
            "category": "invalid-category",
            "pools": {
                "test_pool": [
                    {
                        "id": "test_001",
                        "text": "Test...",
                        "threshold": "Critical",
                        "manifestation": "Physical"
                    }
                ]
            }
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail
        errors.Should().NotBeEmpty(
            "invalid category should fail validation");
    }

    /// <summary>
    /// PSY-008: Verifies that empty pool fails validation.
    /// </summary>
    [Test]
    public void Configuration_EmptyPool_FailsValidation()
    {
        // Arrange: Configuration with empty pool
        var invalidJson = """
        {
            "version": "1.0.0",
            "category": "stress",
            "pools": {
                "test_pool": []
            }
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail
        errors.Should().NotBeEmpty(
            "empty pool should fail validation (minItems: 1)");
    }

    #endregion

    #region PSY-009: Intensity Enum Validation

    /// <summary>
    /// PSY-009: Verifies that all valid intensity levels pass validation.
    /// </summary>
    [Test]
    [TestCase("Subtle")]
    [TestCase("Moderate")]
    [TestCase("Oppressive")]
    public void Intensity_ValidLevel_PassesValidation(string intensity)
    {
        // Arrange: Descriptor with valid intensity
        var validJson = $$"""
        {
            "version": "1.0.0",
            "category": "stress",
            "pools": {
                "test_pool": [
                    {
                        "id": "test_001",
                        "text": "Test...",
                        "threshold": "Critical",
                        "manifestation": "Physical",
                        "intensity": "{{intensity}}"
                    }
                ]
            }
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(validJson);

        // Assert: Validation should pass
        errors.Should().BeEmpty(
            $"intensity '{intensity}' should pass validation");
    }

    /// <summary>
    /// PSY-009: Verifies that null intensity passes validation (optional field).
    /// </summary>
    [Test]
    public void Intensity_Null_PassesValidation()
    {
        // Arrange: Descriptor with null intensity
        var validJson = """
        {
            "version": "1.0.0",
            "category": "stress",
            "pools": {
                "test_pool": [
                    {
                        "id": "test_001",
                        "text": "Test...",
                        "threshold": "Critical",
                        "manifestation": "Physical",
                        "intensity": null
                    }
                ]
            }
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(validJson);

        // Assert: Validation should pass
        errors.Should().BeEmpty(
            "null intensity should pass validation");
    }

    #endregion
}
