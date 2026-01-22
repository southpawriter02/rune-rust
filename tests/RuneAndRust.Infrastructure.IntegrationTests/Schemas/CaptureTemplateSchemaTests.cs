// ------------------------------------------------------------------------------
// <copyright file="CaptureTemplateSchemaTests.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Unit tests for capture-templates.schema.json validation.
// Verifies schema structure, capture template validation, capture type enum validation,
// content length constraints, quality range validation, and keyword inheritance.
// Part of v0.14.9e implementation.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Infrastructure.IntegrationTests.Schemas;

using FluentAssertions;
using NJsonSchema;
using NUnit.Framework;

/// <summary>
/// Unit tests validating the capture-templates.schema.json JSON Schema.
/// Tests ensure the schema correctly validates Data Capture template configuration files
/// including capture types, content constraints, quality values, and keyword inheritance.
/// Validates 6 capture types with Domain 4 compliance requirements.
/// </summary>
/// <remarks>
/// <para>
/// The Capture Template schema provides 2 definitions:
/// <list type="bullet">
/// <item><description>CaptureTemplate - Individual template definition</description></item>
/// <item><description>CaptureType - 6 capture types enum</description></item>
/// </list>
/// </para>
/// </remarks>
[TestFixture]
public class CaptureTemplateSchemaTests
{
    /// <summary>
    /// Path to the capture template schema file relative to the test execution directory.
    /// </summary>
    private const string SchemaPath = "../../../../../config/schemas/capture-templates.schema.json";

    /// <summary>
    /// Path to the rusted-servitor.json configuration file for validation.
    /// </summary>
    private const string RustedServitorPath = "../../../../../config/capture-templates/rusted-servitor.json";

    /// <summary>
    /// Path to the blighted-creature.json configuration file for validation.
    /// </summary>
    private const string BlightedCreaturePath = "../../../../../config/capture-templates/blighted-creature.json";

    /// <summary>
    /// Path to the ancient-ruin.json configuration file for validation.
    /// </summary>
    private const string AncientRuinPath = "../../../../../config/capture-templates/ancient-ruin.json";

    /// <summary>
    /// Path to the industrial-site.json configuration file for validation.
    /// </summary>
    private const string IndustrialSitePath = "../../../../../config/capture-templates/industrial-site.json";

    /// <summary>
    /// Path to the generic-container.json configuration file for validation.
    /// </summary>
    private const string GenericContainerPath = "../../../../../config/capture-templates/generic-container.json";

    /// <summary>
    /// Path to the field-guide-triggers.json configuration file for validation.
    /// </summary>
    private const string FieldGuideTriggersPath = "../../../../../config/capture-templates/field-guide-triggers.json";

    /// <summary>
    /// Loaded JSON Schema for capture template definitions.
    /// </summary>
    private JsonSchema _schema = null!;

    /// <summary>
    /// Sets up the test fixture by loading the capture template schema.
    /// </summary>
    [SetUp]
    public async Task SetUp()
    {
        // Arrange: Load the schema from the file system with full path for reference resolution
        var fullSchemaPath = Path.GetFullPath(SchemaPath);
        _schema = await JsonSchema.FromFileAsync(fullSchemaPath);
    }

    #region CAP-001: Schema File Loads and Contains Expected Definitions

    /// <summary>
    /// CAP-001: Verifies that capture-templates.schema.json loads and parses as valid JSON Schema.
    /// </summary>
    [Test]
    public void CaptureTemplateSchema_LoadsAndParses_Successfully()
    {
        // Assert: Schema should parse successfully
        _schema.Should().NotBeNull("schema file should parse successfully");
        _schema.Title.Should().Be("Capture Template Collection", "schema title should match");
        _schema.Type.Should().Be(JsonObjectType.Object, "schema type should be object");

        // Assert: All definitions should be present
        _schema.Definitions.Should().ContainKey("CaptureTemplate", "should define CaptureTemplate");
        _schema.Definitions.Should().ContainKey("CaptureType", "should define CaptureType");
    }

    #endregion

    #region CAP-002: Existing Configuration Files Pass Validation

    /// <summary>
    /// CAP-002: Verifies that rusted-servitor.json passes schema validation.
    /// </summary>
    [Test]
    public async Task CaptureTemplateSchema_RustedServitor_PassesValidation()
    {
        // Arrange: Load the actual rusted-servitor.json file
        var jsonContent = await File.ReadAllTextAsync(RustedServitorPath);

        // Act: Validate the JSON content against the schema
        var errors = _schema.Validate(jsonContent);

        // Assert: Schema should validate successfully
        errors.Should().BeEmpty(
            "existing rusted-servitor.json should validate against schema without errors");
    }

    /// <summary>
    /// CAP-002: Verifies that blighted-creature.json passes schema validation.
    /// </summary>
    [Test]
    public async Task CaptureTemplateSchema_BlightedCreature_PassesValidation()
    {
        // Arrange: Load the actual blighted-creature.json file
        var jsonContent = await File.ReadAllTextAsync(BlightedCreaturePath);

        // Act: Validate the JSON content against the schema
        var errors = _schema.Validate(jsonContent);

        // Assert: Schema should validate successfully
        errors.Should().BeEmpty(
            "existing blighted-creature.json should validate against schema without errors");
    }

    /// <summary>
    /// CAP-002: Verifies that ancient-ruin.json passes schema validation.
    /// </summary>
    [Test]
    public async Task CaptureTemplateSchema_AncientRuin_PassesValidation()
    {
        // Arrange: Load the actual ancient-ruin.json file
        var jsonContent = await File.ReadAllTextAsync(AncientRuinPath);

        // Act: Validate the JSON content against the schema
        var errors = _schema.Validate(jsonContent);

        // Assert: Schema should validate successfully
        errors.Should().BeEmpty(
            "existing ancient-ruin.json should validate against schema without errors");
    }

    /// <summary>
    /// CAP-002: Verifies that industrial-site.json passes schema validation.
    /// </summary>
    [Test]
    public async Task CaptureTemplateSchema_IndustrialSite_PassesValidation()
    {
        // Arrange: Load the actual industrial-site.json file
        var jsonContent = await File.ReadAllTextAsync(IndustrialSitePath);

        // Act: Validate the JSON content against the schema
        var errors = _schema.Validate(jsonContent);

        // Assert: Schema should validate successfully
        errors.Should().BeEmpty(
            "existing industrial-site.json should validate against schema without errors");
    }

    /// <summary>
    /// CAP-002: Verifies that generic-container.json passes schema validation.
    /// </summary>
    [Test]
    public async Task CaptureTemplateSchema_GenericContainer_PassesValidation()
    {
        // Arrange: Load the actual generic-container.json file
        var jsonContent = await File.ReadAllTextAsync(GenericContainerPath);

        // Act: Validate the JSON content against the schema
        var errors = _schema.Validate(jsonContent);

        // Assert: Schema should validate successfully
        errors.Should().BeEmpty(
            "existing generic-container.json should validate against schema without errors");
    }

    /// <summary>
    /// CAP-002: Verifies that field-guide-triggers.json passes schema validation.
    /// </summary>
    [Test]
    public async Task CaptureTemplateSchema_FieldGuideTriggers_PassesValidation()
    {
        // Arrange: Load the actual field-guide-triggers.json file
        var jsonContent = await File.ReadAllTextAsync(FieldGuideTriggersPath);

        // Act: Validate the JSON content against the schema
        var errors = _schema.Validate(jsonContent);

        // Assert: Schema should validate successfully
        errors.Should().BeEmpty(
            "existing field-guide-triggers.json should validate against schema without errors");
    }

    #endregion

    #region CAP-003: CaptureTemplate Validation

    /// <summary>
    /// CAP-003: Verifies that minimal valid capture template passes validation.
    /// </summary>
    [Test]
    public void CaptureTemplate_MinimalValid_PassesValidation()
    {
        // Arrange: Minimal valid capture template configuration
        var validJson = """
        {
            "category": "test-category",
            "version": "1.0.0",
            "templates": [
                {
                    "id": "test-template-id",
                    "type": "TextFragment",
                    "fragmentContent": "This is valid fragment content that meets the minimum length requirement.",
                    "source": "Test source"
                }
            ]
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(validJson);

        // Assert: Validation should pass
        errors.Should().BeEmpty(
            "minimal capture template should validate successfully");
    }

    /// <summary>
    /// CAP-003: Verifies that complete capture template passes validation.
    /// </summary>
    [Test]
    public void CaptureTemplate_Complete_PassesValidation()
    {
        // Arrange: Complete capture template with all optional fields
        var validJson = """
        {
            "category": "test-category",
            "version": "1.0.0",
            "description": "Test category description",
            "matchKeywords": ["test", "keyword"],
            "templates": [
                {
                    "id": "test-template-id",
                    "type": "TextFragment",
                    "fragmentContent": "This is valid fragment content that meets the minimum length requirement and demonstrates a complete capture.",
                    "source": "Test source context",
                    "matchKeywords": ["override", "keywords"],
                    "quality": 50,
                    "tags": ["tag1", "tag2"],
                    "domain4Notes": "Test compliance notes"
                }
            ]
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(validJson);

        // Assert: Validation should pass
        errors.Should().BeEmpty(
            "complete capture template should validate successfully");
    }

    /// <summary>
    /// CAP-003: Verifies that template missing required 'id' fails validation.
    /// </summary>
    [Test]
    public void CaptureTemplate_MissingId_FailsValidation()
    {
        // Arrange: Template missing required 'id'
        var invalidJson = """
        {
            "category": "test-category",
            "version": "1.0.0",
            "templates": [
                {
                    "type": "TextFragment",
                    "fragmentContent": "This is valid fragment content that meets the minimum length requirement.",
                    "source": "Test source"
                }
            ]
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail
        errors.Should().NotBeEmpty(
            "template missing 'id' should fail validation");
    }

    /// <summary>
    /// CAP-003: Verifies that template missing required 'type' fails validation.
    /// </summary>
    [Test]
    public void CaptureTemplate_MissingType_FailsValidation()
    {
        // Arrange: Template missing required 'type'
        var invalidJson = """
        {
            "category": "test-category",
            "version": "1.0.0",
            "templates": [
                {
                    "id": "test-id",
                    "fragmentContent": "This is valid fragment content that meets the minimum length requirement.",
                    "source": "Test source"
                }
            ]
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail
        errors.Should().NotBeEmpty(
            "template missing 'type' should fail validation");
    }

    /// <summary>
    /// CAP-003: Verifies that template with uppercase ID fails validation.
    /// </summary>
    [Test]
    public void CaptureTemplate_UppercaseId_FailsValidation()
    {
        // Arrange: Template with uppercase ID
        var invalidJson = """
        {
            "category": "test-category",
            "version": "1.0.0",
            "templates": [
                {
                    "id": "Test-Id-Uppercase",
                    "type": "TextFragment",
                    "fragmentContent": "This is valid fragment content that meets the minimum length requirement.",
                    "source": "Test source"
                }
            ]
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail
        errors.Should().NotBeEmpty(
            "template with uppercase ID should fail validation");
    }

    #endregion

    #region CAP-004: CaptureType Enum Validation

    /// <summary>
    /// CAP-004: Verifies that all valid capture types pass validation.
    /// </summary>
    [Test]
    [TestCase("TextFragment")]
    [TestCase("EchoRecording")]
    [TestCase("VisualRecord")]
    [TestCase("Specimen")]
    [TestCase("OralHistory")]
    [TestCase("RunicTrace")]
    public void CaptureType_ValidType_PassesValidation(string captureType)
    {
        // Arrange: Template with valid capture type
        var validJson = $$"""
        {
            "category": "test-category",
            "version": "1.0.0",
            "templates": [
                {
                    "id": "test-id",
                    "type": "{{captureType}}",
                    "fragmentContent": "This is valid fragment content that meets the minimum length requirement.",
                    "source": "Test source"
                }
            ]
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(validJson);

        // Assert: Validation should pass
        errors.Should().BeEmpty(
            $"captureType '{captureType}' should pass validation");
    }

    /// <summary>
    /// CAP-004: Verifies that invalid capture type fails validation.
    /// </summary>
    [Test]
    [TestCase("InvalidType")]
    [TestCase("textfragment")]
    [TestCase("TEXTFRAGMENT")]
    [TestCase("Text_Fragment")]
    public void CaptureType_InvalidType_FailsValidation(string captureType)
    {
        // Arrange: Template with invalid capture type
        var invalidJson = $$"""
        {
            "category": "test-category",
            "version": "1.0.0",
            "templates": [
                {
                    "id": "test-id",
                    "type": "{{captureType}}",
                    "fragmentContent": "This is valid fragment content that meets the minimum length requirement.",
                    "source": "Test source"
                }
            ]
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail
        errors.Should().NotBeEmpty(
            $"captureType '{captureType}' should fail validation");
    }

    #endregion

    #region CAP-005: Content Length Validation

    /// <summary>
    /// CAP-005: Verifies that fragmentContent at minimum length passes validation.
    /// </summary>
    [Test]
    public void ContentLength_FragmentContentMinimum_PassesValidation()
    {
        // Arrange: Fragment content at minimum length (20 chars)
        var validJson = """
        {
            "category": "test-category",
            "version": "1.0.0",
            "templates": [
                {
                    "id": "test-id",
                    "type": "TextFragment",
                    "fragmentContent": "12345678901234567890",
                    "source": "Test source"
                }
            ]
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(validJson);

        // Assert: Validation should pass
        errors.Should().BeEmpty(
            "fragmentContent at minimum 20 chars should pass validation");
    }

    /// <summary>
    /// CAP-005: Verifies that fragmentContent below minimum length fails validation.
    /// </summary>
    [Test]
    public void ContentLength_FragmentContentBelowMinimum_FailsValidation()
    {
        // Arrange: Fragment content below minimum length (19 chars)
        var invalidJson = """
        {
            "category": "test-category",
            "version": "1.0.0",
            "templates": [
                {
                    "id": "test-id",
                    "type": "TextFragment",
                    "fragmentContent": "1234567890123456789",
                    "source": "Test source"
                }
            ]
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail
        errors.Should().NotBeEmpty(
            "fragmentContent below 20 chars should fail validation");
    }

    /// <summary>
    /// CAP-005: Verifies that source at minimum length passes validation.
    /// </summary>
    [Test]
    public void ContentLength_SourceMinimum_PassesValidation()
    {
        // Arrange: Source at minimum length (5 chars)
        var validJson = """
        {
            "category": "test-category",
            "version": "1.0.0",
            "templates": [
                {
                    "id": "test-id",
                    "type": "TextFragment",
                    "fragmentContent": "This is valid fragment content that meets the minimum length requirement.",
                    "source": "12345"
                }
            ]
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(validJson);

        // Assert: Validation should pass
        errors.Should().BeEmpty(
            "source at minimum 5 chars should pass validation");
    }

    /// <summary>
    /// CAP-005: Verifies that source below minimum length fails validation.
    /// </summary>
    [Test]
    public void ContentLength_SourceBelowMinimum_FailsValidation()
    {
        // Arrange: Source below minimum length (4 chars)
        var invalidJson = """
        {
            "category": "test-category",
            "version": "1.0.0",
            "templates": [
                {
                    "id": "test-id",
                    "type": "TextFragment",
                    "fragmentContent": "This is valid fragment content that meets the minimum length requirement.",
                    "source": "1234"
                }
            ]
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail
        errors.Should().NotBeEmpty(
            "source below 5 chars should fail validation");
    }

    #endregion

    #region CAP-006: Quality Range Validation

    /// <summary>
    /// CAP-006: Verifies that quality at minimum value passes validation.
    /// </summary>
    [Test]
    public void QualityRange_Minimum_PassesValidation()
    {
        // Arrange: Quality at minimum value (1)
        var validJson = """
        {
            "category": "test-category",
            "version": "1.0.0",
            "templates": [
                {
                    "id": "test-id",
                    "type": "TextFragment",
                    "fragmentContent": "This is valid fragment content that meets the minimum length requirement.",
                    "source": "Test source",
                    "quality": 1
                }
            ]
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(validJson);

        // Assert: Validation should pass
        errors.Should().BeEmpty(
            "quality at minimum 1 should pass validation");
    }

    /// <summary>
    /// CAP-006: Verifies that quality at maximum value passes validation.
    /// </summary>
    [Test]
    public void QualityRange_Maximum_PassesValidation()
    {
        // Arrange: Quality at maximum value (100)
        var validJson = """
        {
            "category": "test-category",
            "version": "1.0.0",
            "templates": [
                {
                    "id": "test-id",
                    "type": "TextFragment",
                    "fragmentContent": "This is valid fragment content that meets the minimum length requirement.",
                    "source": "Test source",
                    "quality": 100
                }
            ]
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(validJson);

        // Assert: Validation should pass
        errors.Should().BeEmpty(
            "quality at maximum 100 should pass validation");
    }

    /// <summary>
    /// CAP-006: Verifies that quality below minimum fails validation.
    /// </summary>
    [Test]
    public void QualityRange_BelowMinimum_FailsValidation()
    {
        // Arrange: Quality below minimum (0)
        var invalidJson = """
        {
            "category": "test-category",
            "version": "1.0.0",
            "templates": [
                {
                    "id": "test-id",
                    "type": "TextFragment",
                    "fragmentContent": "This is valid fragment content that meets the minimum length requirement.",
                    "source": "Test source",
                    "quality": 0
                }
            ]
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail
        errors.Should().NotBeEmpty(
            "quality below 1 should fail validation");
    }

    /// <summary>
    /// CAP-006: Verifies that quality above maximum fails validation.
    /// </summary>
    [Test]
    public void QualityRange_AboveMaximum_FailsValidation()
    {
        // Arrange: Quality above maximum (101)
        var invalidJson = """
        {
            "category": "test-category",
            "version": "1.0.0",
            "templates": [
                {
                    "id": "test-id",
                    "type": "TextFragment",
                    "fragmentContent": "This is valid fragment content that meets the minimum length requirement.",
                    "source": "Test source",
                    "quality": 101
                }
            ]
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail
        errors.Should().NotBeEmpty(
            "quality above 100 should fail validation");
    }

    /// <summary>
    /// CAP-006: Verifies that quality without value uses default.
    /// </summary>
    [Test]
    public void QualityRange_NoValue_PassesValidation()
    {
        // Arrange: Template without quality (uses default 15)
        var validJson = """
        {
            "category": "test-category",
            "version": "1.0.0",
            "templates": [
                {
                    "id": "test-id",
                    "type": "TextFragment",
                    "fragmentContent": "This is valid fragment content that meets the minimum length requirement.",
                    "source": "Test source"
                }
            ]
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(validJson);

        // Assert: Validation should pass
        errors.Should().BeEmpty(
            "template without quality should pass validation (uses default)");
    }

    #endregion

    #region CAP-007: Category Pattern Validation

    /// <summary>
    /// CAP-007: Verifies that valid category patterns pass validation.
    /// </summary>
    [Test]
    [TestCase("test-category")]
    [TestCase("rusted-servitor")]
    [TestCase("ancient-ruin")]
    [TestCase("generic123")]
    public void CategoryPattern_ValidPattern_PassesValidation(string category)
    {
        // Arrange: Configuration with valid category pattern
        var validJson = $$"""
        {
            "category": "{{category}}",
            "version": "1.0.0",
            "templates": [
                {
                    "id": "test-id",
                    "type": "TextFragment",
                    "fragmentContent": "This is valid fragment content that meets the minimum length requirement.",
                    "source": "Test source"
                }
            ]
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(validJson);

        // Assert: Validation should pass
        errors.Should().BeEmpty(
            $"category '{category}' should pass validation");
    }

    /// <summary>
    /// CAP-007: Verifies that invalid category patterns fail validation.
    /// </summary>
    [Test]
    [TestCase("Test-Category")]
    [TestCase("UPPERCASE")]
    [TestCase("123-starts-with-number")]
    [TestCase("has_underscore")]
    public void CategoryPattern_InvalidPattern_FailsValidation(string category)
    {
        // Arrange: Configuration with invalid category pattern
        var invalidJson = $$"""
        {
            "category": "{{category}}",
            "version": "1.0.0",
            "templates": [
                {
                    "id": "test-id",
                    "type": "TextFragment",
                    "fragmentContent": "This is valid fragment content that meets the minimum length requirement.",
                    "source": "Test source"
                }
            ]
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail
        errors.Should().NotBeEmpty(
            $"category '{category}' should fail validation");
    }

    #endregion

    #region CAP-008: Root Configuration Validation

    /// <summary>
    /// CAP-008: Verifies that configuration missing 'category' fails validation.
    /// </summary>
    [Test]
    public void Configuration_MissingCategory_FailsValidation()
    {
        // Arrange: Configuration missing required 'category'
        var invalidJson = """
        {
            "version": "1.0.0",
            "templates": [
                {
                    "id": "test-id",
                    "type": "TextFragment",
                    "fragmentContent": "This is valid fragment content that meets the minimum length requirement.",
                    "source": "Test source"
                }
            ]
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail
        errors.Should().NotBeEmpty(
            "configuration missing 'category' should fail validation");
    }

    /// <summary>
    /// CAP-008: Verifies that configuration missing 'version' fails validation.
    /// </summary>
    [Test]
    public void Configuration_MissingVersion_FailsValidation()
    {
        // Arrange: Configuration missing required 'version'
        var invalidJson = """
        {
            "category": "test-category",
            "templates": [
                {
                    "id": "test-id",
                    "type": "TextFragment",
                    "fragmentContent": "This is valid fragment content that meets the minimum length requirement.",
                    "source": "Test source"
                }
            ]
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail
        errors.Should().NotBeEmpty(
            "configuration missing 'version' should fail validation");
    }

    /// <summary>
    /// CAP-008: Verifies that empty templates array fails validation.
    /// </summary>
    [Test]
    public void Configuration_EmptyTemplates_FailsValidation()
    {
        // Arrange: Configuration with empty templates array
        var invalidJson = """
        {
            "category": "test-category",
            "version": "1.0.0",
            "templates": []
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail
        errors.Should().NotBeEmpty(
            "empty templates array should fail validation (minItems: 1)");
    }

    /// <summary>
    /// CAP-008: Verifies that invalid version format fails validation.
    /// </summary>
    [Test]
    [TestCase("1.0")]
    [TestCase("v1.0.0")]
    [TestCase("1.0.0.0")]
    [TestCase("1")]
    public void Configuration_InvalidVersionFormat_FailsValidation(string version)
    {
        // Arrange: Configuration with invalid version format
        var invalidJson = $$"""
        {
            "category": "test-category",
            "version": "{{version}}",
            "templates": [
                {
                    "id": "test-id",
                    "type": "TextFragment",
                    "fragmentContent": "This is valid fragment content that meets the minimum length requirement.",
                    "source": "Test source"
                }
            ]
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail
        errors.Should().NotBeEmpty(
            $"version '{version}' should fail validation (not semver)");
    }

    #endregion

    #region CAP-009: Keyword Validation

    /// <summary>
    /// CAP-009: Verifies that category matchKeywords with valid keywords passes validation.
    /// </summary>
    [Test]
    public void Keywords_CategoryKeywords_PassesValidation()
    {
        // Arrange: Category with valid matchKeywords
        var validJson = """
        {
            "category": "test-category",
            "version": "1.0.0",
            "matchKeywords": ["keyword1", "keyword2", "longer-keyword"],
            "templates": [
                {
                    "id": "test-id",
                    "type": "TextFragment",
                    "fragmentContent": "This is valid fragment content that meets the minimum length requirement.",
                    "source": "Test source"
                }
            ]
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(validJson);

        // Assert: Validation should pass
        errors.Should().BeEmpty(
            "category with valid matchKeywords should pass validation");
    }

    /// <summary>
    /// CAP-009: Verifies that template matchKeywords override passes validation.
    /// </summary>
    [Test]
    public void Keywords_TemplateOverride_PassesValidation()
    {
        // Arrange: Template with matchKeywords override
        var validJson = """
        {
            "category": "test-category",
            "version": "1.0.0",
            "matchKeywords": ["category-keyword"],
            "templates": [
                {
                    "id": "test-id",
                    "type": "TextFragment",
                    "fragmentContent": "This is valid fragment content that meets the minimum length requirement.",
                    "source": "Test source",
                    "matchKeywords": ["template-keyword", "override-keyword"]
                }
            ]
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(validJson);

        // Assert: Validation should pass
        errors.Should().BeEmpty(
            "template with matchKeywords override should pass validation");
    }

    /// <summary>
    /// CAP-009: Verifies that keywords below minimum length fail validation.
    /// </summary>
    [Test]
    public void Keywords_BelowMinimumLength_FailsValidation()
    {
        // Arrange: Category with keyword below minimum length (1 char)
        var invalidJson = """
        {
            "category": "test-category",
            "version": "1.0.0",
            "matchKeywords": ["a"],
            "templates": [
                {
                    "id": "test-id",
                    "type": "TextFragment",
                    "fragmentContent": "This is valid fragment content that meets the minimum length requirement.",
                    "source": "Test source"
                }
            ]
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail
        errors.Should().NotBeEmpty(
            "keyword below 2 chars should fail validation");
    }

    #endregion
}
