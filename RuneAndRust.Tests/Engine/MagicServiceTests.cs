using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using RuneAndRust.Engine.Services;
using RuneAndRust.Core.Models;
using RuneAndRust.Core.Models.Magic;
using RuneAndRust.Core.Enums;

namespace RuneAndRust.Tests.Engine;

public class MagicServiceTests
{
    private readonly Mock<ILogger<MagicService>> _loggerMock;
    private readonly MagicService _magicService;
    private readonly Character _character;

    public MagicServiceTests()
    {
        _loggerMock = new Mock<ILogger<MagicService>>();
        _magicService = new MagicService(_loggerMock.Object);
        _character = new Character("TestMage");
        // Reset aether to a known state if needed, though default is 10
    }

    [Fact]
    public void Cast_InstantSpell_WithSufficientAether_Success()
    {
        // Arrange
        var spell = new Spell
        {
            Id = "spell_test",
            Name = "Test Spell",
            Cost = 5,
            CastTime = CastTimeType.Instant
        };

        // Act
        var result = _magicService.Cast(_character, spell);

        // Assert
        Assert.Equal(CastResult.Success, result);
        Assert.Equal(5, _character.Aether.Current); // 10 - 5 = 5
    }

    [Fact]
    public void Cast_WithInsufficientAether_Fails()
    {
        // Arrange
        var spell = new Spell
        {
            Id = "spell_expensive",
            Name = "Expensive Spell",
            Cost = 20,
            CastTime = CastTimeType.Instant
        };

        // Act
        var result = _magicService.Cast(_character, spell);

        // Assert
        Assert.Equal(CastResult.InsufficientAether, result);
        Assert.Equal(10, _character.Aether.Current); // Unchanged
    }

    [Fact]
    public void Cast_ChantSpell_StartsChant()
    {
        // Arrange
        var spell = new Spell
        {
            Id = "spell_chant",
            Name = "Chant Spell",
            Cost = 5,
            CastTime = CastTimeType.Chant,
            ChantTurns = 2
        };

        // Act
        var result = _magicService.Cast(_character, spell);

        // Assert
        Assert.Equal(CastResult.StartedChant, result);
        Assert.Equal(5, _character.Aether.Current); // Deducted immediately
    }
}
