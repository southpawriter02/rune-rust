namespace RuneAndRust.Domain.UnitTests.ValueObjects;

using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Unit tests for <see cref="TerminalErrorResult"/> value object.
/// Tests cover Success/Failure factory methods, WasCriticalSuccess detection,
/// BecameForlorn/Survived relationship, FinalCorruption values, and ToString formatting.
/// </summary>
[TestFixture]
public class TerminalErrorResultTests
{
    // -------------------------------------------------------------------------
    // Factory — Success
    // -------------------------------------------------------------------------

    /// <summary>
    /// Verifies that Success creates a result with correct survival properties.
    /// </summary>
    [Test]
    public void Success_CreatesCorrectResult()
    {
        // Arrange & Act
        var result = TerminalErrorResult.Success(successes: 4, dc: 3);

        // Assert
        result.Survived.Should().BeTrue();
        result.BecameForlorn.Should().BeFalse();
        result.FinalCorruption.Should().Be(99,
            because: "surviving Terminal Error sets corruption to 99");
        result.SuccessesRolled.Should().Be(4);
        result.RequiredDc.Should().Be(3);
    }

    /// <summary>
    /// Verifies that Success uses default DC of 3 when not specified.
    /// </summary>
    [Test]
    public void Success_DefaultDc_IsThree()
    {
        // Act
        var result = TerminalErrorResult.Success(successes: 3);

        // Assert
        result.RequiredDc.Should().Be(3);
        result.Survived.Should().BeTrue();
    }

    // -------------------------------------------------------------------------
    // Factory — Failure
    // -------------------------------------------------------------------------

    /// <summary>
    /// Verifies that Failure creates a result with correct Forlorn properties.
    /// </summary>
    [Test]
    public void Failure_CreatesCorrectResult()
    {
        // Arrange & Act
        var result = TerminalErrorResult.Failure(successes: 2, dc: 3);

        // Assert
        result.Survived.Should().BeFalse();
        result.BecameForlorn.Should().BeTrue(
            because: "failing Terminal Error means the character becomes Forlorn");
        result.FinalCorruption.Should().Be(100,
            because: "Forlorn characters remain at maximum corruption");
        result.SuccessesRolled.Should().Be(2);
        result.RequiredDc.Should().Be(3);
    }

    /// <summary>
    /// Verifies that Failure uses default DC of 3 when not specified.
    /// </summary>
    [Test]
    public void Failure_DefaultDc_IsThree()
    {
        // Act
        var result = TerminalErrorResult.Failure(successes: 1);

        // Assert
        result.RequiredDc.Should().Be(3);
        result.BecameForlorn.Should().BeTrue();
    }

    // -------------------------------------------------------------------------
    // Arrow — WasCriticalSuccess
    // -------------------------------------------------------------------------

    /// <summary>
    /// Verifies WasCriticalSuccess detects when successes >= 2x DC.
    /// </summary>
    [Test]
    [TestCase(6, 3, true)]      // 6 >= 3*2 = 6 — critical
    [TestCase(7, 3, true)]      // 7 >= 6 — critical
    [TestCase(5, 3, false)]     // 5 < 6 — not critical
    [TestCase(4, 3, false)]     // Normal success, not critical
    [TestCase(3, 3, false)]     // Exactly at DC, not critical
    [TestCase(4, 2, true)]      // 4 >= 2*2 = 4 — critical (custom DC)
    public void WasCriticalSuccess_DetectsCorrectly(int successes, int dc, bool expectedCritical)
    {
        // Act
        var result = TerminalErrorResult.Success(successes, dc);

        // Assert
        result.WasCriticalSuccess.Should().Be(expectedCritical);
    }

    /// <summary>
    /// Verifies that WasCriticalSuccess is always false for failures
    /// (even with high successes, since Survived is false).
    /// </summary>
    [Test]
    public void WasCriticalSuccess_OnFailure_AlwaysFalse()
    {
        // Act — failure result with high successes (edge case)
        var result = TerminalErrorResult.Failure(successes: 10, dc: 3);

        // Assert
        result.WasCriticalSuccess.Should().BeFalse(
            because: "WasCriticalSuccess requires Survived to be true");
    }

    // -------------------------------------------------------------------------
    // Survived / BecameForlorn Relationship
    // -------------------------------------------------------------------------

    /// <summary>
    /// Verifies that BecameForlorn is always the inverse of Survived.
    /// </summary>
    [Test]
    public void BecameForlorn_AlwaysInverseOfSurvived()
    {
        // Arrange
        var success = TerminalErrorResult.Success(successes: 3, dc: 3);
        var failure = TerminalErrorResult.Failure(successes: 2, dc: 3);

        // Assert
        success.BecameForlorn.Should().Be(!success.Survived);
        failure.BecameForlorn.Should().Be(!failure.Survived);
    }

    // -------------------------------------------------------------------------
    // ToString
    // -------------------------------------------------------------------------

    /// <summary>
    /// Verifies ToString for a normal survival (non-critical).
    /// </summary>
    [Test]
    public void ToString_NormalSuccess_FormatsCorrectly()
    {
        // Arrange
        var result = TerminalErrorResult.Success(successes: 4, dc: 3);

        // Act
        var display = result.ToString();

        // Assert
        display.Should().Contain("SURVIVED");
        display.Should().Contain("4/3 successes");
        display.Should().Contain("corruption -> 99");
        display.Should().NotContain("[CRITICAL]");
    }

    /// <summary>
    /// Verifies ToString for a critical survival.
    /// </summary>
    [Test]
    public void ToString_CriticalSuccess_IncludesCriticalFlag()
    {
        // Arrange
        var result = TerminalErrorResult.Success(successes: 6, dc: 3);

        // Act
        var display = result.ToString();

        // Assert
        display.Should().Contain("SURVIVED");
        display.Should().Contain("[CRITICAL]");
    }

    /// <summary>
    /// Verifies ToString for a failure.
    /// </summary>
    [Test]
    public void ToString_Failure_IncludesForlornIndicator()
    {
        // Arrange
        var result = TerminalErrorResult.Failure(successes: 2, dc: 3);

        // Act
        var display = result.ToString();

        // Assert
        display.Should().Contain("FAILED");
        display.Should().Contain("2/3 successes");
        display.Should().Contain("CHARACTER BECAME FORLORN");
    }
}
