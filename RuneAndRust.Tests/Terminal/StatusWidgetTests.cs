using RuneAndRust.Terminal.Rendering;
using Spectre.Console;

namespace RuneAndRust.Tests.Terminal;

/// <summary>
/// Unit tests for StatusWidget color logic and bar rendering (v0.3.5a).
/// Validates threshold-based color coding for HP, Stamina, and Stress.
/// </summary>
public class StatusWidgetTests
{
    #region HP Color Tests

    [Fact]
    public void GetHpColor_Critical_ReturnsRed()
    {
        // Arrange - 10% HP (critical threshold: <= 25%)
        int current = 10;
        int max = 100;

        // Act
        var color = StatusWidget.GetHpColor(current, max);

        // Assert
        Assert.Equal(Color.Red1, color);
    }

    [Fact]
    public void GetHpColor_Danger_ReturnsOrange()
    {
        // Arrange - 35% HP (danger threshold: 26-50%)
        int current = 35;
        int max = 100;

        // Act
        var color = StatusWidget.GetHpColor(current, max);

        // Assert
        Assert.Equal(Color.Orange1, color);
    }

    [Fact]
    public void GetHpColor_Wounded_ReturnsYellow()
    {
        // Arrange - 60% HP (wounded threshold: 51-75%)
        int current = 60;
        int max = 100;

        // Act
        var color = StatusWidget.GetHpColor(current, max);

        // Assert
        Assert.Equal(Color.Yellow, color);
    }

    [Fact]
    public void GetHpColor_Healthy_ReturnsGreen()
    {
        // Arrange - 90% HP (healthy threshold: > 75%)
        int current = 90;
        int max = 100;

        // Act
        var color = StatusWidget.GetHpColor(current, max);

        // Assert
        Assert.Equal(Color.Green, color);
    }

    [Fact]
    public void GetHpColor_ZeroMax_ReturnsGrey()
    {
        // Arrange - Edge case: max HP is 0
        int current = 0;
        int max = 0;

        // Act
        var color = StatusWidget.GetHpColor(current, max);

        // Assert
        Assert.Equal(Color.Grey, color);
    }

    #endregion

    #region Stamina Color Tests

    [Fact]
    public void GetStaminaColor_Exhausted_ReturnsGrey()
    {
        // Arrange - 10% stamina (exhausted threshold: < 20%)
        int current = 5;
        int max = 50;

        // Act
        var color = StatusWidget.GetStaminaColor(current, max);

        // Assert
        Assert.Equal(Color.Grey, color);
    }

    [Fact]
    public void GetStaminaColor_Active_ReturnsCyan()
    {
        // Arrange - 60% stamina (active threshold: >= 20%)
        int current = 30;
        int max = 50;

        // Act
        var color = StatusWidget.GetStaminaColor(current, max);

        // Assert
        Assert.Equal(Color.Cyan1, color);
    }

    [Fact]
    public void GetStaminaColor_ExactlyTwentyPercent_ReturnsCyan()
    {
        // Arrange - Exactly 20% stamina (boundary test)
        int current = 20;
        int max = 100;

        // Act
        var color = StatusWidget.GetStaminaColor(current, max);

        // Assert
        Assert.Equal(Color.Cyan1, color);
    }

    #endregion

    #region Stress Color Tests

    [Fact]
    public void GetStressColor_Stable_ReturnsGrey()
    {
        // Arrange - Stress at 10 (stable threshold: < 20)
        int stress = 10;

        // Act
        var color = StatusWidget.GetStressColor(stress);

        // Assert
        Assert.Equal(Color.Grey, color);
    }

    [Fact]
    public void GetStressColor_Unsettled_ReturnsCyan()
    {
        // Arrange - Stress at 25 (unsettled threshold: 20-39)
        int stress = 25;

        // Act
        var color = StatusWidget.GetStressColor(stress);

        // Assert
        Assert.Equal(Color.Cyan1, color);
    }

    [Fact]
    public void GetStressColor_Shaken_ReturnsYellow()
    {
        // Arrange - Stress at 45 (shaken threshold: 40-59)
        int stress = 45;

        // Act
        var color = StatusWidget.GetStressColor(stress);

        // Assert
        Assert.Equal(Color.Yellow, color);
    }

    [Fact]
    public void GetStressColor_Distressed_ReturnsMagenta()
    {
        // Arrange - Stress at 70 (distressed threshold: 60-79)
        int stress = 70;

        // Act
        var color = StatusWidget.GetStressColor(stress);

        // Assert
        Assert.Equal(Color.Magenta1, color);
    }

    [Fact]
    public void GetStressColor_Fractured_ReturnsPurple()
    {
        // Arrange - Stress at 85 (fractured threshold: >= 80)
        int stress = 85;

        // Act
        var color = StatusWidget.GetStressColor(stress);

        // Assert
        Assert.Equal(Color.Purple, color);
    }

    #endregion

    #region RenderBar Tests

    [Fact]
    public void RenderBar_FullHealth_AllFilled()
    {
        // Arrange
        int current = 100;
        int max = 100;
        int width = 20;

        // Act
        var bar = StatusWidget.RenderBar(current, max, width);

        // Assert
        Assert.Equal(new string('█', 20), bar);
    }

    [Fact]
    public void RenderBar_HalfHealth_HalfFilled()
    {
        // Arrange
        int current = 50;
        int max = 100;
        int width = 20;

        // Act
        var bar = StatusWidget.RenderBar(current, max, width);

        // Assert
        Assert.Equal(new string('█', 10) + new string('░', 10), bar);
    }

    [Fact]
    public void RenderBar_Empty_AllEmpty()
    {
        // Arrange
        int current = 0;
        int max = 100;
        int width = 20;

        // Act
        var bar = StatusWidget.RenderBar(current, max, width);

        // Assert
        Assert.Equal(new string('░', 20), bar);
    }

    [Fact]
    public void RenderBar_ZeroMax_AllEmpty()
    {
        // Arrange - Edge case: max is 0
        int current = 0;
        int max = 0;
        int width = 20;

        // Act
        var bar = StatusWidget.RenderBar(current, max, width);

        // Assert
        Assert.Equal(new string('░', 20), bar);
    }

    [Fact]
    public void RenderBar_OverMax_ClampedToFull()
    {
        // Arrange - Edge case: current exceeds max
        int current = 150;
        int max = 100;
        int width = 20;

        // Act
        var bar = StatusWidget.RenderBar(current, max, width);

        // Assert
        Assert.Equal(new string('█', 20), bar);
    }

    #endregion
}
