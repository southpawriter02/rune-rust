using FluentAssertions;
using RuneAndRust.Core.ViewModels;
using RuneAndRust.Terminal.Rendering;
using Xunit;

namespace RuneAndRust.Tests.Terminal;

/// <summary>
/// Tests for the TimelineRenderer introduced in v0.3.6b.
/// Validates horizontal timeline rendering and combatant formatting.
/// </summary>
public class TimelineRendererTests
{
    #region Render Tests

    [Fact]
    public void Render_NullProjection_ReturnsPanel()
    {
        // Arrange
        List<TimelineEntryView>? projection = null;

        // Act
        var result = TimelineRenderer.Render(projection, currentRound: 1);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public void Render_EmptyProjection_ReturnsPanel()
    {
        // Arrange
        var projection = new List<TimelineEntryView>();

        // Act
        var result = TimelineRenderer.Render(projection, currentRound: 1);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public void Render_WithEntries_ReturnsPanel()
    {
        // Arrange
        var projection = CreateTestProjection();

        // Act
        var result = TimelineRenderer.Render(projection, currentRound: 1);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public void Render_RespectsWindowSize()
    {
        // Arrange - create more entries than window size
        var projection = new List<TimelineEntryView>();
        for (int i = 0; i < 12; i++)
        {
            projection.Add(CreateTestEntry($"Combatant{i}", isPlayer: i % 2 == 0));
        }

        // Act
        var result = TimelineRenderer.Render(projection, currentRound: 1);

        // Assert - should not throw, panel should be created
        result.Should().NotBeNull();
    }

    #endregion

    #region FormatTimelineEntry Tests

    [Fact]
    public void FormatTimelineEntry_Player_UsesCyan()
    {
        // Arrange
        var entry = CreateTestEntry("Hero", isPlayer: true);

        // Act
        var result = TimelineRenderer.FormatTimelineEntry(entry);

        // Assert
        result.Should().Contain("[cyan]");
    }

    [Fact]
    public void FormatTimelineEntry_Enemy_UsesRed()
    {
        // Arrange
        var entry = CreateTestEntry("Goblin", isPlayer: false);

        // Act
        var result = TimelineRenderer.FormatTimelineEntry(entry);

        // Assert
        result.Should().Contain("[red]");
    }

    [Fact]
    public void FormatTimelineEntry_Active_ShowsMarker()
    {
        // Arrange
        var entry = new TimelineEntryView(
            CombatantId: Guid.NewGuid(),
            Name: "Hero",
            IsPlayer: true,
            IsActive: true,
            Initiative: 15,
            RoundNumber: 1,
            HealthIndicator: "healthy"
        );

        // Act
        var result = TimelineRenderer.FormatTimelineEntry(entry);

        // Assert
        result.Should().Contain("[bold yellow]►[/]");
    }

    [Fact]
    public void FormatTimelineEntry_Inactive_NoMarker()
    {
        // Arrange
        var entry = new TimelineEntryView(
            CombatantId: Guid.NewGuid(),
            Name: "Hero",
            IsPlayer: true,
            IsActive: false,
            Initiative: 15,
            RoundNumber: 1,
            HealthIndicator: "healthy"
        );

        // Act
        var result = TimelineRenderer.FormatTimelineEntry(entry);

        // Assert
        result.Should().NotContain("[bold yellow]►[/]");
        result.Should().StartWith(" ");
    }

    [Fact]
    public void FormatTimelineEntry_CriticalHealth_ShowsExclamation()
    {
        // Arrange
        var entry = new TimelineEntryView(
            CombatantId: Guid.NewGuid(),
            Name: "Enemy",
            IsPlayer: false,
            IsActive: false,
            Initiative: 10,
            RoundNumber: 1,
            HealthIndicator: "critical"
        );

        // Act
        var result = TimelineRenderer.FormatTimelineEntry(entry);

        // Assert
        result.Should().Contain("[red]![/]");
    }

    [Fact]
    public void FormatTimelineEntry_WoundedHealth_ShowsTilde()
    {
        // Arrange
        var entry = new TimelineEntryView(
            CombatantId: Guid.NewGuid(),
            Name: "Enemy",
            IsPlayer: false,
            IsActive: false,
            Initiative: 10,
            RoundNumber: 1,
            HealthIndicator: "wounded"
        );

        // Act
        var result = TimelineRenderer.FormatTimelineEntry(entry);

        // Assert
        result.Should().Contain("[yellow]~[/]");
    }

    [Fact]
    public void FormatTimelineEntry_DeadHealth_ShowsCross()
    {
        // Arrange
        var entry = new TimelineEntryView(
            CombatantId: Guid.NewGuid(),
            Name: "Enemy",
            IsPlayer: false,
            IsActive: false,
            Initiative: 10,
            RoundNumber: 1,
            HealthIndicator: "dead"
        );

        // Act
        var result = TimelineRenderer.FormatTimelineEntry(entry);

        // Assert
        result.Should().Contain("[grey]✗[/]");
    }

    [Fact]
    public void FormatTimelineEntry_HealthyHealth_NoIndicator()
    {
        // Arrange
        var entry = new TimelineEntryView(
            CombatantId: Guid.NewGuid(),
            Name: "Enemy",
            IsPlayer: false,
            IsActive: false,
            Initiative: 10,
            RoundNumber: 1,
            HealthIndicator: "healthy"
        );

        // Act
        var result = TimelineRenderer.FormatTimelineEntry(entry);

        // Assert
        result.Should().NotContain("[red]![/]");
        result.Should().NotContain("[yellow]~[/]");
        result.Should().NotContain("[grey]✗[/]");
    }

    [Fact]
    public void FormatTimelineEntry_LongName_Truncates()
    {
        // Arrange
        var entry = new TimelineEntryView(
            CombatantId: Guid.NewGuid(),
            Name: "VeryLongCombatantName",
            IsPlayer: false,
            IsActive: false,
            Initiative: 10,
            RoundNumber: 1,
            HealthIndicator: "healthy"
        );

        // Act
        var result = TimelineRenderer.FormatTimelineEntry(entry);

        // Assert
        result.Should().Contain("VeryLon…");
        result.Should().NotContain("VeryLongCombatantName");
    }

    [Fact]
    public void FormatTimelineEntry_ShortName_NoTruncation()
    {
        // Arrange
        var entry = new TimelineEntryView(
            CombatantId: Guid.NewGuid(),
            Name: "Kael",
            IsPlayer: true,
            IsActive: false,
            Initiative: 15,
            RoundNumber: 1,
            HealthIndicator: "healthy"
        );

        // Act
        var result = TimelineRenderer.FormatTimelineEntry(entry);

        // Assert
        result.Should().Contain("Kael");
        result.Should().NotContain("…");
    }

    [Fact]
    public void FormatTimelineEntry_NameWithBrackets_EscapesMarkup()
    {
        // Arrange
        var entry = new TimelineEntryView(
            CombatantId: Guid.NewGuid(),
            Name: "[Elite]",
            IsPlayer: false,
            IsActive: false,
            Initiative: 10,
            RoundNumber: 1,
            HealthIndicator: "healthy"
        );

        // Act
        var result = TimelineRenderer.FormatTimelineEntry(entry);

        // Assert
        // Markup.Escape should convert [ and ] to [[ and ]]
        result.Should().Contain("[[Elite]]");
    }

    #endregion

    #region Helper Methods

    private static List<TimelineEntryView> CreateTestProjection()
    {
        return new List<TimelineEntryView>
        {
            CreateTestEntry("Hero", isPlayer: true, isActive: true),
            CreateTestEntry("Goblin", isPlayer: false),
            CreateTestEntry("Orc", isPlayer: false)
        };
    }

    private static TimelineEntryView CreateTestEntry(
        string name,
        bool isPlayer,
        bool isActive = false,
        string healthIndicator = "healthy")
    {
        return new TimelineEntryView(
            CombatantId: Guid.NewGuid(),
            Name: name,
            IsPlayer: isPlayer,
            IsActive: isActive,
            Initiative: isPlayer ? 15 : 10,
            RoundNumber: 1,
            HealthIndicator: healthIndicator
        );
    }

    #endregion
}
