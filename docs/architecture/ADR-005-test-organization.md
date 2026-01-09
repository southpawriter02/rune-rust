# ADR-005: NUnit Test Organization

**Status:** Accepted
**Date:** 2026-01-06
**Deciders:** Development Team

## Context

Rune and Rust requires comprehensive testing to ensure game mechanics work correctly. The test suite must:

- Cover domain logic (entities, value objects, calculations)
- Cover application services (orchestration, validation)
- Verify architectural constraints (layer dependencies)
- Be maintainable as the codebase grows
- Provide clear failure messages
- Support test-driven development workflow

## Decision

We will use NUnit as the primary test framework with the following organization:

### Test Framework Stack

- **NUnit**: Test framework with rich assertion and parameterized test support
- **FluentAssertions**: Readable assertion syntax
- **Moq**: Mocking framework for interface isolation
- **Microsoft.Extensions.Logging.Testing**: Test logger for verifying log output

### Project Structure

```
tests/
├── RuneAndRust.Domain.UnitTests/       # Domain layer tests
│   ├── Entities/                       # Entity tests
│   ├── ValueObjects/                   # Value object tests
│   ├── Definitions/                    # Definition tests
│   └── Services/                       # Domain service tests
├── RuneAndRust.Application.UnitTests/  # Application layer tests
│   ├── Services/                       # Application service tests
│   └── DTOs/                          # DTO mapping tests
├── RuneAndRust.Architecture.Tests/     # Architectural constraint tests
└── RuneAndRust.TestUtilities/          # Shared test utilities
    ├── Builders/                       # Test data builders
    ├── Mocks/                          # Mock implementations
    ├── Fixtures/                       # Test fixtures
    └── Logging/                        # Test logger utilities
```

### Naming Convention

Test methods follow `MethodName_StateUnderTest_ExpectedBehavior`:

```csharp
[Test]
public void Attack_WhenTargetIsAlive_DealsDamageAndReturnsResult()

[Test]
public void UseAbility_WhenInsufficientResource_ReturnsFailureResult()

[Test]
public void Create_WithEmptyId_ThrowsArgumentException()
```

### Test Fixtures

Base fixture class provides common setup:

```csharp
public abstract class TestFixtureBase
{
    protected MockConfigurationProvider ConfigProvider { get; private set; }
    protected TestLoggerFactory LoggerFactory { get; private set; }
    protected SeededRandom Random { get; private set; }

    [SetUp]
    public virtual void SetUp()
    {
        ConfigProvider = new MockConfigurationProvider();
        LoggerFactory = new TestLoggerFactory();
        Random = new SeededRandom(42);
    }
}
```

### Test Builders

Fluent builders for creating test data:

```csharp
var player = new PlayerBuilder()
    .WithName("TestPlayer")
    .WithHealth(100)
    .WithClass("berserker")
    .WithAbility("rage-strike")
    .Build();

var monster = new MonsterBuilder()
    .WithName("Goblin")
    .WithHealth(30)
    .WithTier("common")
    .Build();
```

## Consequences

### Positive

- **Clarity**: Naming convention makes test purpose obvious
- **Isolation**: Each layer tested independently
- **Maintainability**: Builders reduce test setup duplication
- **Readability**: FluentAssertions provides clear failure messages
- **Architecture Enforcement**: Architecture tests prevent layer violations
- **Parallelization**: Tests run in parallel for faster feedback

### Negative

- **Multiple Projects**: More test projects to maintain
- **Builder Maintenance**: Builders must be updated when entities change
- **Learning Curve**: Team must learn NUnit, FluentAssertions, Moq patterns

### Neutral

- Tests organized by layer, then by type
- Integration tests may be added in future
- Code coverage measured but not enforced

## Implementation Details

### Domain Test Example

```csharp
[TestFixture]
public class PlayerTests : TestFixtureBase
{
    [Test]
    public void TakeDamage_WhenDamageExceedsHealth_SetsHealthToZero()
    {
        // Arrange
        var player = new PlayerBuilder()
            .WithHealth(50)
            .WithDefense(0)
            .Build();

        // Act
        player.TakeDamage(100);

        // Assert
        player.CurrentHealth.Should().Be(0);
        player.IsDead.Should().BeTrue();
    }

    [TestCase(10, 5, 5)]
    [TestCase(10, 10, 0)]
    [TestCase(10, 15, 0)]
    public void TakeDamage_WithVariousDamageAmounts_ReducesHealthCorrectly(
        int health, int damage, int expectedHealth)
    {
        // Arrange
        var player = new PlayerBuilder()
            .WithHealth(health)
            .Build();

        // Act
        player.TakeDamage(damage);

        // Assert
        player.CurrentHealth.Should().Be(expectedHealth);
    }
}
```

### Application Service Test Example

```csharp
[TestFixture]
public class AbilityServiceTests : TestFixtureBase
{
    private AbilityService _sut;
    private Mock<IResourceService> _resourceServiceMock;

    [SetUp]
    public override void SetUp()
    {
        base.SetUp();
        _resourceServiceMock = new Mock<IResourceService>();
        _sut = new AbilityService(ConfigProvider, _resourceServiceMock.Object);
    }

    [Test]
    public void UseAbility_WhenPlayerHasInsufficientResource_ReturnsFailure()
    {
        // Arrange
        var player = new PlayerBuilder()
            .WithResource("mana", current: 5, max: 100)
            .Build();
        var ability = ConfigProvider.GetAbility("fireball"); // costs 20 mana

        _resourceServiceMock
            .Setup(x => x.HasSufficientResource(player, "mana", 20))
            .Returns(false);

        // Act
        var result = _sut.UseAbility(player, ability.Id);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Insufficient");
    }
}
```

### Architecture Test Example

```csharp
[TestFixture]
public class ArchitectureTests
{
    [Test]
    public void DomainLayer_ShouldNotDependOn_ApplicationLayer()
    {
        var domainAssembly = typeof(Player).Assembly;
        var applicationNamespace = "RuneAndRust.Application";

        var violations = domainAssembly.GetTypes()
            .SelectMany(t => t.GetReferencedAssemblies())
            .Where(a => a.Name?.Contains(applicationNamespace) == true);

        violations.Should().BeEmpty(
            "Domain layer should not depend on Application layer");
    }
}
```

## Alternatives Considered

### Alternative 1: xUnit

Use xUnit instead of NUnit.

**Rejected because:**
- Team more experienced with NUnit
- NUnit parameterized tests more intuitive
- Both frameworks are equally capable

### Alternative 2: Single Test Project

One test project for all layers.

**Rejected because:**
- Slower test runs (can't run domain tests alone)
- Less clear organization
- Dependencies between test types

### Alternative 3: No Test Builders

Create test data inline in each test.

**Rejected because:**
- Duplicated setup code
- Harder to maintain when entities change
- Less readable tests

### Alternative 4: Specification Pattern

Use SpecFlow or similar for BDD-style tests.

**Rejected because:**
- Overkill for unit tests
- Additional tooling complexity
- Reserved for potential future integration tests

## Related

- [ADR-001](ADR-001-clean-architecture.md): Clean Architecture (test isolation per layer)
- [v0.0.10b Changelog](../../changelogs/v0.0.10b-changelog.md): Test utilities implementation
