---
trigger: always_on
---

# Testing Rules

## 1.1 Test Organization
- Place unit tests in `tests/RuneAndRust.Domain.UnitTests/` or `tests/RuneAndRust.Application.UnitTests/`
- Mirror the source directory structure in test projects
- Use `[TestFixture]` attribute for test classes
- Use `[Test]` attribute for test methods

## 1.2 Test Naming
Use the pattern: `MethodName_Scenario_ExpectedBehavior`
```csharp
[Test]
public void TakeDamage_WhenDamageLessThanHealth_ReducesHealth()
```

## 1.3 Test Structure
- **ALWAYS** use AAA pattern: Arrange, Act, Assert
- **ALWAYS** add comments marking each section
- **ALWAYS** use FluentAssertions for assertions (`.Should().Be()`, `.Should().Throw<>()`)

```csharp
[Test]
public void Constructor_WithValidName_CreatesPlayer()
{
    // Arrange & Act
    var player = new Player("TestHero");

    // Assert
    player.Name.Should().Be("TestHero");
    player.Health.Should().Be(player.Stats.MaxHealth);
    player.IsAlive.Should().BeTrue();
}
```

## 1.4 Test Coverage Requirements
- **ALWAYS** test:
  - Happy path scenarios
  - Edge cases (null, empty, max values)
  - Exception scenarios with `act.Should().Throw<ExceptionType>()`
  - Boundary conditions

## 1.5 Running Tests

```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test tests/RuneAndRust.Domain.UnitTests

# Run with verbosity
dotnet test --verbosity normal
```