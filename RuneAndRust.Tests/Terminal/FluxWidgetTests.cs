using FluentAssertions;
using NSubstitute;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Terminal.Rendering;
using Spectre.Console;

namespace RuneAndRust.Tests.Terminal;

/// <summary>
/// Unit tests for FluxWidget (v0.4.3e - The Resonance).
/// </summary>
public class FluxWidgetTests
{
    private readonly IThemeService _mockTheme;

    public FluxWidgetTests()
    {
        _mockTheme = Substitute.For<IThemeService>();
        _mockTheme.GetColor("FluxSafe").Returns("green");
        _mockTheme.GetColor("FluxElevated").Returns("yellow");
        _mockTheme.GetColor("FluxCritical").Returns("red");
        _mockTheme.GetColor("FluxOverload").Returns("red1");
    }

    #region GetThreshold Tests

    [Theory]
    [InlineData(0, FluxThreshold.Safe)]
    [InlineData(24, FluxThreshold.Safe)]
    [InlineData(25, FluxThreshold.Elevated)]
    [InlineData(49, FluxThreshold.Elevated)]
    [InlineData(50, FluxThreshold.Critical)]
    [InlineData(74, FluxThreshold.Critical)]
    [InlineData(75, FluxThreshold.Overload)]
    [InlineData(100, FluxThreshold.Overload)]
    public void GetThreshold_ReturnsCorrectThreshold(int flux, FluxThreshold expected)
    {
        // Act
        var result = FluxWidget.GetThreshold(flux);

        // Assert
        result.Should().Be(expected);
    }

    #endregion

    #region GetFluxColor Tests

    [Fact]
    public void GetFluxColor_WhenSafe_ReturnsGreen()
    {
        // Act
        var result = FluxWidget.GetFluxColor(10, _mockTheme);

        // Assert
        result.Should().Be(Color.Green);
    }

    [Fact]
    public void GetFluxColor_WhenElevated_ReturnsYellow()
    {
        // Act
        var result = FluxWidget.GetFluxColor(30, _mockTheme);

        // Assert
        result.Should().Be(Color.Yellow);
    }

    [Fact]
    public void GetFluxColor_WhenCritical_ReturnsRed()
    {
        // Act
        var result = FluxWidget.GetFluxColor(55, _mockTheme);

        // Assert
        result.Should().Be(Color.Red);
    }

    [Fact]
    public void GetFluxColor_WhenOverload_ReturnsRed1()
    {
        // Act
        var result = FluxWidget.GetFluxColor(80, _mockTheme);

        // Assert
        result.Should().Be(Color.Red1);
    }

    #endregion

    #region GetThresholdLabel Tests

    [Fact]
    public void GetThresholdLabel_WhenSafe_ReturnsSafeLabel()
    {
        // Act
        var (label, _) = FluxWidget.GetThresholdLabel(10, _mockTheme);

        // Assert
        label.Should().Be("[SAFE]");
    }

    [Fact]
    public void GetThresholdLabel_WhenElevated_ReturnsElevatedLabel()
    {
        // Act
        var (label, _) = FluxWidget.GetThresholdLabel(30, _mockTheme);

        // Assert
        label.Should().Be("[ELEVATED]");
    }

    [Fact]
    public void GetThresholdLabel_WhenCritical_ReturnsCriticalLabel()
    {
        // Act
        var (label, _) = FluxWidget.GetThresholdLabel(55, _mockTheme);

        // Assert
        label.Should().Be("[CRITICAL]");
    }

    [Fact]
    public void GetThresholdLabel_WhenOverload_ReturnsOverloadLabel()
    {
        // Act
        var (label, _) = FluxWidget.GetThresholdLabel(80, _mockTheme);

        // Assert
        label.Should().Be("[OVERLOAD]");
    }

    #endregion

    #region RenderCompact Tests

    [Fact]
    public void RenderCompact_WhenSafe_ReturnsNoIndicator()
    {
        // Act
        var result = FluxWidget.RenderCompact(10, _mockTheme);

        // Assert
        result.Should().NotContain("!");
    }

    [Fact]
    public void RenderCompact_WhenElevated_ReturnsSingleExclamation()
    {
        // Act
        var result = FluxWidget.RenderCompact(30, _mockTheme);

        // Assert
        result.Should().Contain("!");
        result.Should().NotContain("!!");
    }

    [Fact]
    public void RenderCompact_WhenCritical_ReturnsDoubleExclamation()
    {
        // Act
        var result = FluxWidget.RenderCompact(55, _mockTheme);

        // Assert
        result.Should().Contain("!!");
    }

    [Fact]
    public void RenderCompact_WhenOverload_ReturnsTripleExclamation()
    {
        // Act
        var result = FluxWidget.RenderCompact(80, _mockTheme);

        // Assert
        result.Should().Contain("!!!");
    }

    [Fact]
    public void RenderCompact_IncludesFluxValue()
    {
        // Act
        var result = FluxWidget.RenderCompact(42, _mockTheme);

        // Assert
        result.Should().Contain("42");
    }

    #endregion
}
