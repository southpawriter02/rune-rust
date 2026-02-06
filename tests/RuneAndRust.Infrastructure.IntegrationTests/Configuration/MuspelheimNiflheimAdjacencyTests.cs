using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Infrastructure.Services;

namespace RuneAndRust.Infrastructure.IntegrationTests.Configuration;

/// <summary>
/// Integration tests validating the adjacency rules for Muspelheim and Niflheim
/// as defined in the v0.19.3a design specification.
/// </summary>
/// <remarks>
/// <para>
/// These tests verify that the adjacency-matrix.json configuration correctly enforces
/// the critical incompatibilities between fire/ice and fire/bio realms, and that
/// compatible pairings (e.g., Muspelheim ↔ Svartalfheim) are properly recorded.
/// </para>
/// <para>
/// Rules verified:
/// <list type="bullet">
/// <item>Muspelheim (Fire) ↔ Niflheim (Ice): Incompatible</item>
/// <item>Muspelheim (Fire) ↔ Vanaheim (Bio): Incompatible</item>
/// <item>Niflheim ↔ Vanaheim: Compatible (no fire conflict)</item>
/// <item>Muspelheim ↔ Svartalfheim: Compatible (forge adjacency)</item>
/// </list>
/// </para>
/// </remarks>
[TestFixture]
public class MuspelheimNiflheimAdjacencyTests
{
    private BiomeAdjacencyService _adjacencyService = null!;

    [SetUp]
    public void SetUp()
    {
        var logger = new Microsoft.Extensions.Logging.Abstractions.NullLogger<BiomeAdjacencyService>();
        _adjacencyService = new BiomeAdjacencyService(logger);
    }

    [Test]
    public void Muspelheim_CannotNeighbor_Niflheim()
    {
        // Act
        var compatibility = _adjacencyService.GetCompatibility(RealmId.Muspelheim, RealmId.Niflheim);

        // Assert
        compatibility.Should().Be(BiomeCompatibility.Incompatible,
            "Muspelheim (Fire) and Niflheim (Ice) are fundamentally incompatible — extreme thermal differential");

        // Verify bidirectional
        var reverseCompatibility = _adjacencyService.GetCompatibility(RealmId.Niflheim, RealmId.Muspelheim);
        reverseCompatibility.Should().Be(BiomeCompatibility.Incompatible,
            "Incompatibility must be bidirectional");
    }

    [Test]
    public void Muspelheim_CannotNeighbor_Vanaheim()
    {
        // Act
        var compatibility = _adjacencyService.GetCompatibility(RealmId.Muspelheim, RealmId.Vanaheim);

        // Assert
        compatibility.Should().Be(BiomeCompatibility.Incompatible,
            "Muspelheim (Fire) and Vanaheim (Bio) are incompatible — extreme heat destroys organic matter");

        // Verify bidirectional
        var reverseCompatibility = _adjacencyService.GetCompatibility(RealmId.Vanaheim, RealmId.Muspelheim);
        reverseCompatibility.Should().Be(BiomeCompatibility.Incompatible,
            "Incompatibility must be bidirectional");
    }

    [Test]
    public void Niflheim_CanNeighbor_Vanaheim()
    {
        // Act
        var compatibility = _adjacencyService.GetCompatibility(RealmId.Niflheim, RealmId.Vanaheim);

        // Assert
        compatibility.Should().NotBe(BiomeCompatibility.Incompatible,
            "Niflheim (Ice) and Vanaheim (Bio) have no fire-based conflict and can neighbor");
    }

    [Test]
    public void Muspelheim_CanNeighbor_Svartalfheim()
    {
        // Act
        var compatibility = _adjacencyService.GetCompatibility(RealmId.Muspelheim, RealmId.Svartalfheim);

        // Assert
        compatibility.Should().Be(BiomeCompatibility.Compatible,
            "Muspelheim (Fire) and Svartalfheim (Forge) are compatible — volcanic heat feeds the forges");
    }
}
