// ═══════════════════════════════════════════════════════════════════════════════
// IComponentLifecycleTests.cs
// Unit tests for IComponentLifecycle interface contract verification.
// Version: 0.13.5c
// ═══════════════════════════════════════════════════════════════════════════════

using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Presentation.Interfaces;
using System.Reflection;

namespace RuneAndRust.Application.UnitTests.Presentation.Theme;

/// <summary>
/// Unit tests for <see cref="IComponentLifecycle"/> interface.
/// </summary>
/// <remarks>
/// These tests verify the interface contract is correctly defined,
/// which is important for ensuring consistent behavior across
/// TUI and GUI implementations.
/// </remarks>
[TestFixture]
public class IComponentLifecycleTests
{
    private Type _interfaceType = null!;

    [SetUp]
    public void Setup()
    {
        _interfaceType = typeof(IComponentLifecycle);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // INTERFACE CONTRACT TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that IComponentLifecycle inherits from IDisposable.
    /// </summary>
    [Test]
    public void InterfaceContract_InheritsFromIDisposable()
    {
        // Assert
        typeof(IDisposable).IsAssignableFrom(_interfaceType).Should().BeTrue(
            "IComponentLifecycle must inherit from IDisposable for proper resource cleanup");
    }

    /// <summary>
    /// Verifies that interface defines IsInitialized property.
    /// </summary>
    [Test]
    public void InterfaceContract_DefinesIsInitializedProperty()
    {
        // Arrange
        var property = _interfaceType.GetProperty("IsInitialized");

        // Assert
        property.Should().NotBeNull("interface must define IsInitialized property");
        property!.PropertyType.Should().Be(typeof(bool));
        property.CanRead.Should().BeTrue();
        property.CanWrite.Should().BeFalse("IsInitialized should be read-only");
    }

    /// <summary>
    /// Verifies that interface defines IsActive property.
    /// </summary>
    [Test]
    public void InterfaceContract_DefinesIsActiveProperty()
    {
        // Arrange
        var property = _interfaceType.GetProperty("IsActive");

        // Assert
        property.Should().NotBeNull("interface must define IsActive property");
        property!.PropertyType.Should().Be(typeof(bool));
        property.CanRead.Should().BeTrue();
        property.CanWrite.Should().BeFalse("IsActive should be read-only");
    }

    /// <summary>
    /// Verifies that interface defines Initialize method.
    /// </summary>
    [Test]
    public void InterfaceContract_DefinesInitializeMethod()
    {
        // Arrange
        var method = _interfaceType.GetMethod("Initialize");

        // Assert
        method.Should().NotBeNull("interface must define Initialize method");
        method!.ReturnType.Should().Be(typeof(void));
        method.GetParameters().Should().BeEmpty("Initialize should take no parameters");
    }

    /// <summary>
    /// Verifies that interface defines Activate method.
    /// </summary>
    [Test]
    public void InterfaceContract_DefinesActivateMethod()
    {
        // Arrange
        var method = _interfaceType.GetMethod("Activate");

        // Assert
        method.Should().NotBeNull("interface must define Activate method");
        method!.ReturnType.Should().Be(typeof(void));
        method.GetParameters().Should().BeEmpty("Activate should take no parameters");
    }

    /// <summary>
    /// Verifies that interface defines Deactivate method.
    /// </summary>
    [Test]
    public void InterfaceContract_DefinesDeactivateMethod()
    {
        // Arrange
        var method = _interfaceType.GetMethod("Deactivate");

        // Assert
        method.Should().NotBeNull("interface must define Deactivate method");
        method!.ReturnType.Should().Be(typeof(void));
        method.GetParameters().Should().BeEmpty("Deactivate should take no parameters");
    }

    /// <summary>
    /// Verifies complete interface structure.
    /// </summary>
    [Test]
    public void InterfaceContract_HasExpectedMemberCount()
    {
        // Arrange
        var ownProperties = _interfaceType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        var ownMethods = _interfaceType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Where(m => !m.IsSpecialName); // Exclude property accessors

        // Assert
        ownProperties.Should().HaveCount(2, "should have IsInitialized and IsActive");
        ownMethods.Should().HaveCount(3, "should have Initialize, Activate, and Deactivate");
    }
}
