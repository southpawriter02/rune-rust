// ═══════════════════════════════════════════════════════════════════════════════
// TuiServiceCollectionExtensionsTests.cs
// Unit tests for TUI service registration extension methods.
// Version: 0.13.5c
// ═══════════════════════════════════════════════════════════════════════════════

using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using RuneAndRust.Presentation.Extensions;
using RuneAndRust.Presentation.Shared.Enums;
using RuneAndRust.Presentation.Shared.Services;
using RuneAndRust.Presentation.Shared.ValueObjects;
using RuneAndRust.Presentation.Tui.Services;

namespace RuneAndRust.Application.UnitTests.Presentation.Theme;

/// <summary>
/// Unit tests for <see cref="TuiServiceCollectionExtensions"/>.
/// </summary>
/// <remarks>
/// Tests verify that the extension methods correctly register
/// services with the DI container.
/// </remarks>
[TestFixture]
public class TuiServiceCollectionExtensionsTests
{
    private ServiceCollection _services = null!;

    [SetUp]
    public void Setup()
    {
        _services = new ServiceCollection();

        // Add logging (required for services)
        _services.AddLogging(builder => builder.SetMinimumLevel(LogLevel.Debug));
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // SERVICE REGISTRATION TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that AddRuneRustTuiServices registers IThemeService.
    /// </summary>
    [Test]
    public void AddRuneRustTuiServices_RegistersIThemeService()
    {
        // Arrange
        _services.AddRuneRustTuiServices();
        var provider = _services.BuildServiceProvider();

        // Act
        var themeService = provider.GetService<IThemeService>();

        // Assert
        themeService.Should().NotBeNull("IThemeService should be registered");
        themeService.Should().BeOfType<TuiThemeAdapter>();
    }

    /// <summary>
    /// Verifies that IThemeService is registered as singleton.
    /// </summary>
    [Test]
    public void AddRuneRustTuiServices_RegistersThemeServiceAsSingleton()
    {
        // Arrange
        _services.AddRuneRustTuiServices();
        var provider = _services.BuildServiceProvider();

        // Act
        var service1 = provider.GetRequiredService<IThemeService>();
        var service2 = provider.GetRequiredService<IThemeService>();

        // Assert
        service1.Should().BeSameAs(service2, "singleton should return same instance");
    }

    /// <summary>
    /// Verifies that default theme definition is used when not specified.
    /// </summary>
    [Test]
    public void AddRuneRustTuiServices_WithoutTheme_UsesDefaultTheme()
    {
        // Arrange
        _services.AddRuneRustTuiServices();
        var provider = _services.BuildServiceProvider();

        // Act
        var themeService = provider.GetRequiredService<IThemeService>();

        // Assert
        themeService.ThemeDefinition.Should().NotBeNull();
        themeService.ThemeDefinition.Name.Should().Be("Dark Fantasy");
    }

    /// <summary>
    /// Verifies that custom theme definition can be provided.
    /// </summary>
    [Test]
    public void AddRuneRustTuiServices_WithCustomTheme_UsesProvidedTheme()
    {
        // Arrange
        var customTheme = new ThemeDefinition(
            "Custom Test Theme",
            ColorPalette.CreateDefault(),
            IconSet.CreateDefault(),
            AnimationTimings.CreateDefault());
        _services.AddRuneRustTuiServices(customTheme);
        var provider = _services.BuildServiceProvider();

        // Act
        var themeService = provider.GetRequiredService<IThemeService>();

        // Assert
        themeService.ThemeDefinition.Name.Should().Be("Custom Test Theme");
    }

    /// <summary>
    /// Verifies that theme adapter can convert colors correctly.
    /// </summary>
    [Test]
    public void ResolvedThemeService_CanConvertColors()
    {
        // Arrange
        _services.AddRuneRustTuiServices();
        var provider = _services.BuildServiceProvider();
        var themeService = provider.GetRequiredService<IThemeService>();

        // Act
        var healthColor = themeService.GetHealthColor(0.90);

        // Assert
        healthColor.Should().NotBe(default(ThemeColor));
    }

    /// <summary>
    /// Verifies that theme adapter can provide icons.
    /// </summary>
    [Test]
    public void ResolvedThemeService_CanProvideIcons()
    {
        // Arrange
        _services.AddRuneRustTuiServices();
        var provider = _services.BuildServiceProvider();
        var themeService = provider.GetRequiredService<IThemeService>();

        // Act
        var healthIcon = themeService.GetIcon(IconKey.Health);

        // Assert
        healthIcon.Should().NotBeNullOrEmpty();
    }

    /// <summary>
    /// Verifies method chaining works correctly.
    /// </summary>
    [Test]
    public void AddRuneRustTuiServices_ReturnsServiceCollectionForChaining()
    {
        // Act
        var result = _services.AddRuneRustTuiServices();

        // Assert
        result.Should().BeSameAs(_services);
    }

    /// <summary>
    /// Verifies that null theme throws ArgumentNullException.
    /// </summary>
    [Test]
    public void AddRuneRustTuiServices_WithNullTheme_ThrowsArgumentNullException()
    {
        // Act
        var act = () => _services.AddRuneRustTuiServices(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }
}
