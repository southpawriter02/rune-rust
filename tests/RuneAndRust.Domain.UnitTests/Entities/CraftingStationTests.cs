using FluentAssertions;
using RuneAndRust.Domain.Definitions;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.UnitTests.Entities;

/// <summary>
/// Unit tests for the <see cref="CraftingStation"/> entity.
/// </summary>
/// <remarks>
/// Tests cover:
/// <list type="bullet">
///   <item><description>Factory method creation from definitions</description></item>
///   <item><description>Custom description factory method</description></item>
///   <item><description>State management (SetInUse, SetAvailable)</description></item>
///   <item><description>Display methods (GetStatusDescription, GetInteractionPrompt, GetStatusIndicator)</description></item>
///   <item><description>ToString formatting</description></item>
/// </list>
/// </remarks>
[TestFixture]
public class CraftingStationTests
{
    // ═══════════════════════════════════════════════════════════════
    // TEST HELPERS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a valid crafting station definition for testing.
    /// </summary>
    private static CraftingStationDefinition CreateValidDefinition(
        string stationId = "anvil",
        string name = "Anvil",
        string description = "A sturdy anvil for smithing metal items.")
    {
        return CraftingStationDefinition.Create(
            stationId,
            name,
            description,
            new[] { RecipeCategory.Weapon, RecipeCategory.Tool, RecipeCategory.Material },
            "smithing");
    }

    // ═══════════════════════════════════════════════════════════════
    // FACTORY METHOD TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that creating a station from a definition sets all properties correctly.
    /// </summary>
    [Test]
    public void Create_FromDefinition_SetsCorrectProperties()
    {
        // Arrange
        var definition = CreateValidDefinition();

        // Act
        var station = CraftingStation.Create(definition);

        // Assert
        station.Id.Should().NotBe(Guid.Empty);
        station.DefinitionId.Should().Be("anvil");
        station.Name.Should().Be("Anvil");
        station.Description.Should().Be("A sturdy anvil for smithing metal items.");
        station.FeatureType.Should().Be(RoomFeatureType.CraftingStation);
        station.IsInteractable.Should().BeTrue();
        station.InteractionVerb.Should().Be("use");
        station.IsAvailable.Should().BeTrue();
        station.LastUsedAt.Should().BeNull();
    }

    /// <summary>
    /// Verifies that creating a station with a null definition throws ArgumentNullException.
    /// </summary>
    [Test]
    public void Create_WithNullDefinition_ThrowsArgumentNullException()
    {
        // Act
        var act = () => CraftingStation.Create(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("definition");
    }

    /// <summary>
    /// Verifies that creating a station with a custom description sets the custom description.
    /// </summary>
    [Test]
    public void Create_WithCustomDescription_SetsCustomDescription()
    {
        // Arrange
        var definition = CreateValidDefinition();
        var customDescription = "An ancient anvil, its surface scarred by centuries of legendary smithing.";

        // Act
        var station = CraftingStation.Create(definition, customDescription);

        // Assert
        station.DefinitionId.Should().Be("anvil");
        station.Name.Should().Be("Anvil");
        station.Description.Should().Be(customDescription);
        station.IsAvailable.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that creating a station with a null custom description throws ArgumentException.
    /// </summary>
    [Test]
    public void Create_WithNullCustomDescription_ThrowsArgumentException()
    {
        // Arrange
        var definition = CreateValidDefinition();

        // Act
        var act = () => CraftingStation.Create(definition, null!);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("customDescription");
    }

    /// <summary>
    /// Verifies that creating a station with an empty custom description throws ArgumentException.
    /// </summary>
    [Test]
    public void Create_WithEmptyCustomDescription_ThrowsArgumentException()
    {
        // Arrange
        var definition = CreateValidDefinition();

        // Act
        var act = () => CraftingStation.Create(definition, "");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("customDescription");
    }

    /// <summary>
    /// Verifies that creating a station with a whitespace custom description throws ArgumentException.
    /// </summary>
    [Test]
    public void Create_WithWhitespaceCustomDescription_ThrowsArgumentException()
    {
        // Arrange
        var definition = CreateValidDefinition();

        // Act
        var act = () => CraftingStation.Create(definition, "   ");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("customDescription");
    }

    /// <summary>
    /// Verifies that multiple Create calls produce stations with unique IDs.
    /// </summary>
    [Test]
    public void Create_MultipleCalls_ProducesUniqueIds()
    {
        // Arrange
        var definition = CreateValidDefinition();

        // Act
        var station1 = CraftingStation.Create(definition);
        var station2 = CraftingStation.Create(definition);

        // Assert
        station1.Id.Should().NotBe(station2.Id);
        station1.DefinitionId.Should().Be(station2.DefinitionId);
    }

    // ═══════════════════════════════════════════════════════════════
    // STATE MANAGEMENT TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that SetInUse sets IsAvailable to false.
    /// </summary>
    [Test]
    public void SetInUse_SetsIsAvailableToFalse()
    {
        // Arrange
        var definition = CreateValidDefinition();
        var station = CraftingStation.Create(definition);

        // Act
        station.SetInUse();

        // Assert
        station.IsAvailable.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that SetInUse sets LastUsedAt to current time.
    /// </summary>
    [Test]
    public void SetInUse_SetsLastUsedAt()
    {
        // Arrange
        var definition = CreateValidDefinition();
        var station = CraftingStation.Create(definition);
        var beforeTime = DateTime.UtcNow;

        // Act
        station.SetInUse();
        var afterTime = DateTime.UtcNow;

        // Assert
        station.LastUsedAt.Should().NotBeNull();
        station.LastUsedAt.Should().BeOnOrAfter(beforeTime);
        station.LastUsedAt.Should().BeOnOrBefore(afterTime);
    }

    /// <summary>
    /// Verifies that SetAvailable restores IsAvailable to true.
    /// </summary>
    [Test]
    public void SetAvailable_RestoresAvailability()
    {
        // Arrange
        var definition = CreateValidDefinition();
        var station = CraftingStation.Create(definition);
        station.SetInUse();

        // Act
        station.SetAvailable();

        // Assert
        station.IsAvailable.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that SetAvailable preserves LastUsedAt timestamp.
    /// </summary>
    [Test]
    public void SetAvailable_PreservesLastUsedAt()
    {
        // Arrange
        var definition = CreateValidDefinition();
        var station = CraftingStation.Create(definition);
        station.SetInUse();
        var lastUsedAt = station.LastUsedAt;

        // Act
        station.SetAvailable();

        // Assert
        station.LastUsedAt.Should().Be(lastUsedAt);
    }

    /// <summary>
    /// Verifies that calling SetInUse multiple times updates LastUsedAt.
    /// </summary>
    [Test]
    public void SetInUse_CalledMultipleTimes_UpdatesLastUsedAt()
    {
        // Arrange
        var definition = CreateValidDefinition();
        var station = CraftingStation.Create(definition);
        station.SetInUse();
        var firstUsage = station.LastUsedAt;

        // Small delay to ensure time difference
        System.Threading.Thread.Sleep(10);

        // Act
        station.SetAvailable();
        station.SetInUse();

        // Assert
        station.LastUsedAt.Should().BeAfter(firstUsage!.Value);
    }

    // ═══════════════════════════════════════════════════════════════
    // DISPLAY METHOD TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that GetStatusDescription returns "ready" when available.
    /// </summary>
    [Test]
    public void GetStatusDescription_WhenAvailable_ReturnsReadyMessage()
    {
        // Arrange
        var definition = CreateValidDefinition();
        var station = CraftingStation.Create(definition);

        // Act
        var description = station.GetStatusDescription();

        // Assert
        description.Should().Be("The station is ready for use.");
    }

    /// <summary>
    /// Verifies that GetStatusDescription returns "in use" when unavailable.
    /// </summary>
    [Test]
    public void GetStatusDescription_WhenInUse_ReturnsInUseMessage()
    {
        // Arrange
        var definition = CreateValidDefinition();
        var station = CraftingStation.Create(definition);
        station.SetInUse();

        // Act
        var description = station.GetStatusDescription();

        // Assert
        description.Should().Be("The station is currently in use.");
    }

    /// <summary>
    /// Verifies that GetInteractionPrompt returns craft instruction when available.
    /// </summary>
    [Test]
    public void GetInteractionPrompt_WhenAvailable_ReturnsCraftInstruction()
    {
        // Arrange
        var definition = CreateValidDefinition();
        var station = CraftingStation.Create(definition);

        // Act
        var prompt = station.GetInteractionPrompt();

        // Assert
        prompt.Should().Be("Type 'craft <recipe>' to craft at this Anvil.");
    }

    /// <summary>
    /// Verifies that GetInteractionPrompt returns in-use message when unavailable.
    /// </summary>
    [Test]
    public void GetInteractionPrompt_WhenInUse_ReturnsInUseMessage()
    {
        // Arrange
        var definition = CreateValidDefinition();
        var station = CraftingStation.Create(definition);
        station.SetInUse();

        // Act
        var prompt = station.GetInteractionPrompt();

        // Assert
        prompt.Should().Be("The Anvil is currently in use.");
    }

    /// <summary>
    /// Verifies that GetStatusIndicator returns "[Available]" when available.
    /// </summary>
    [Test]
    public void GetStatusIndicator_WhenAvailable_ReturnsAvailableBracket()
    {
        // Arrange
        var definition = CreateValidDefinition();
        var station = CraftingStation.Create(definition);

        // Act
        var indicator = station.GetStatusIndicator();

        // Assert
        indicator.Should().Be("[Available]");
    }

    /// <summary>
    /// Verifies that GetStatusIndicator returns "[In Use]" when unavailable.
    /// </summary>
    [Test]
    public void GetStatusIndicator_WhenInUse_ReturnsInUseBracket()
    {
        // Arrange
        var definition = CreateValidDefinition();
        var station = CraftingStation.Create(definition);
        station.SetInUse();

        // Act
        var indicator = station.GetStatusIndicator();

        // Assert
        indicator.Should().Be("[In Use]");
    }

    /// <summary>
    /// Verifies that ToString returns name with status indicator.
    /// </summary>
    [Test]
    public void ToString_ReturnsNameWithStatusIndicator()
    {
        // Arrange
        var definition = CreateValidDefinition();
        var station = CraftingStation.Create(definition);

        // Act
        var result = station.ToString();

        // Assert
        result.Should().Be("Anvil [Available]");
    }

    /// <summary>
    /// Verifies that ToString reflects in-use state.
    /// </summary>
    [Test]
    public void ToString_WhenInUse_ReflectsState()
    {
        // Arrange
        var definition = CreateValidDefinition();
        var station = CraftingStation.Create(definition);
        station.SetInUse();

        // Act
        var result = station.ToString();

        // Assert
        result.Should().Be("Anvil [In Use]");
    }

    // ═══════════════════════════════════════════════════════════════
    // FEATURE TYPE TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that FeatureType is always set to CraftingStation.
    /// </summary>
    [Test]
    public void FeatureType_IsAlwaysCraftingStation()
    {
        // Arrange
        var definition = CreateValidDefinition();

        // Act
        var station = CraftingStation.Create(definition);

        // Assert
        station.FeatureType.Should().Be(RoomFeatureType.CraftingStation);
    }

    /// <summary>
    /// Verifies that IsInteractable is always true.
    /// </summary>
    [Test]
    public void IsInteractable_IsAlwaysTrue()
    {
        // Arrange
        var definition = CreateValidDefinition();

        // Act
        var station = CraftingStation.Create(definition);

        // Assert
        station.IsInteractable.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that InteractionVerb is always "use".
    /// </summary>
    [Test]
    public void InteractionVerb_IsAlwaysUse()
    {
        // Arrange
        var definition = CreateValidDefinition();

        // Act
        var station = CraftingStation.Create(definition);

        // Assert
        station.InteractionVerb.Should().Be("use");
    }
}
