using System.Reflection;
using FluentAssertions;
using NetArchTest.Rules;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Infrastructure.Persistence;

namespace RuneAndRust.Architecture.Tests;

[TestFixture]
public class LayerDependencyTests
{
    private static readonly Assembly DomainAssembly = typeof(Player).Assembly;
    private static readonly Assembly ApplicationAssembly = typeof(IGameRepository).Assembly;
    private static readonly Assembly InfrastructureAssembly = typeof(GameDbContext).Assembly;

    [Test]
    public void Domain_ShouldNot_DependOn_Application()
    {
        // Arrange & Act
        var result = Types.InAssembly(DomainAssembly)
            .ShouldNot()
            .HaveDependencyOn("RuneAndRust.Application")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            because: "Domain layer should not depend on Application layer. " +
                     $"Failing types: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }

    [Test]
    public void Domain_ShouldNot_DependOn_Infrastructure()
    {
        // Arrange & Act
        var result = Types.InAssembly(DomainAssembly)
            .ShouldNot()
            .HaveDependencyOn("RuneAndRust.Infrastructure")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            because: "Domain layer should not depend on Infrastructure layer. " +
                     $"Failing types: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }

    [Test]
    public void Domain_ShouldNot_DependOn_Presentation()
    {
        // Arrange & Act
        var result = Types.InAssembly(DomainAssembly)
            .ShouldNot()
            .HaveDependencyOn("RuneAndRust.Presentation")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            because: "Domain layer should not depend on Presentation layer. " +
                     $"Failing types: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }

    [Test]
    public void Application_ShouldNot_DependOn_Infrastructure()
    {
        // Arrange & Act
        var result = Types.InAssembly(ApplicationAssembly)
            .ShouldNot()
            .HaveDependencyOn("RuneAndRust.Infrastructure")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            because: "Application layer should not depend on Infrastructure layer. " +
                     $"Failing types: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }

    [Test]
    public void Application_ShouldNot_DependOn_Presentation()
    {
        // Arrange & Act
        var result = Types.InAssembly(ApplicationAssembly)
            .ShouldNot()
            .HaveDependencyOn("RuneAndRust.Presentation")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            because: "Application layer should not depend on Presentation layer. " +
                     $"Failing types: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }

    [Test]
    public void Infrastructure_ShouldNot_DependOn_Presentation()
    {
        // Arrange & Act
        var result = Types.InAssembly(InfrastructureAssembly)
            .ShouldNot()
            .HaveDependencyOn("RuneAndRust.Presentation")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            because: "Infrastructure layer should not depend on Presentation layer. " +
                     $"Failing types: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }

    [Test]
    public void Domain_Entities_ShouldBe_PublicClasses()
    {
        // Arrange & Act
        var result = Types.InAssembly(DomainAssembly)
            .That()
            .ResideInNamespace("RuneAndRust.Domain.Entities")
            .Should()
            .BeClasses()
            .And()
            .BePublic()
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            because: "All domain entities should be public classes. " +
                     $"Failing types: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }

    [Test]
    public void Application_Interfaces_ShouldBe_PublicInterfaces()
    {
        // Arrange & Act
        var result = Types.InAssembly(ApplicationAssembly)
            .That()
            .ResideInNamespace("RuneAndRust.Application.Interfaces")
            .And()
            .AreInterfaces()
            .Should()
            .BePublic()
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            because: "All application interfaces should be public. " +
                     $"Failing types: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }
}
