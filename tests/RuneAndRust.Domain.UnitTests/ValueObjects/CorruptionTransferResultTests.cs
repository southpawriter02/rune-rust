namespace RuneAndRust.Domain.UnitTests.ValueObjects;

using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Unit tests for <see cref="CorruptionTransferResult"/> value object.
/// Tests cover the Create factory method, all stored properties, the TargetTerminalError
/// flag, successful and failed transfer scenarios, and ToString formatting.
/// </summary>
[TestFixture]
public class CorruptionTransferResultTests
{
    // -------------------------------------------------------------------------
    // Factory — Create (Successful Transfer)
    // -------------------------------------------------------------------------

    /// <summary>
    /// Verifies that Create stores all properties correctly for a successful transfer.
    /// </summary>
    [Test]
    public void Create_SuccessfulTransfer_StoresAllProperties()
    {
        // Arrange & Act
        var result = CorruptionTransferResult.Create(
            success: true,
            amountTransferred: 15,
            sourceNewCorruption: 25,
            targetNewCorruption: 60,
            targetTerminalError: false);

        // Assert
        result.Success.Should().BeTrue();
        result.AmountTransferred.Should().Be(15);
        result.SourceNewCorruption.Should().Be(25);
        result.TargetNewCorruption.Should().Be(60);
        result.TargetTerminalError.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that a successful transfer with Terminal Error correctly
    /// sets the TargetTerminalError flag.
    /// </summary>
    [Test]
    public void Create_SuccessfulTransferCausingTerminalError_SetsFlag()
    {
        // Arrange & Act
        var result = CorruptionTransferResult.Create(
            success: true,
            amountTransferred: 20,
            sourceNewCorruption: 10,
            targetNewCorruption: 100,
            targetTerminalError: true);

        // Assert
        result.Success.Should().BeTrue();
        result.TargetTerminalError.Should().BeTrue(
            because: "the target reached 100 corruption from the transfer");
        result.TargetNewCorruption.Should().Be(100);
    }

    // -------------------------------------------------------------------------
    // Factory — Create (Failed Transfer)
    // -------------------------------------------------------------------------

    /// <summary>
    /// Verifies that Create stores correct properties for a failed transfer.
    /// </summary>
    [Test]
    public void Create_FailedTransfer_StoresZeroAmountTransferred()
    {
        // Arrange & Act
        var result = CorruptionTransferResult.Create(
            success: false,
            amountTransferred: 0,
            sourceNewCorruption: 40,
            targetNewCorruption: 50,
            targetTerminalError: false);

        // Assert
        result.Success.Should().BeFalse();
        result.AmountTransferred.Should().Be(0,
            because: "a failed transfer moves no corruption");
        result.SourceNewCorruption.Should().Be(40,
            because: "source corruption should be unchanged on failure");
        result.TargetNewCorruption.Should().Be(50,
            because: "target corruption should be unchanged on failure");
        result.TargetTerminalError.Should().BeFalse();
    }

    // -------------------------------------------------------------------------
    // TargetTerminalError — Boundary Cases
    // -------------------------------------------------------------------------

    /// <summary>
    /// Verifies TargetTerminalError with various target corruption levels.
    /// </summary>
    [Test]
    [TestCase(99, false, Description = "Target at 99 — no Terminal Error")]
    [TestCase(100, true, Description = "Target at 100 — Terminal Error")]
    [TestCase(50, false, Description = "Target at 50 — no Terminal Error")]
    public void Create_TargetTerminalError_MatchesExpected(
        int targetCorruption, bool expectedTerminalError)
    {
        // Arrange & Act
        var result = CorruptionTransferResult.Create(
            success: true,
            amountTransferred: 10,
            sourceNewCorruption: 20,
            targetNewCorruption: targetCorruption,
            targetTerminalError: expectedTerminalError);

        // Assert
        result.TargetTerminalError.Should().Be(expectedTerminalError);
    }

    // -------------------------------------------------------------------------
    // AmountTransferred — Various Amounts
    // -------------------------------------------------------------------------

    /// <summary>
    /// Verifies that AmountTransferred correctly captures various transfer sizes.
    /// </summary>
    [Test]
    [TestCase(1, Description = "Minimal transfer of 1 corruption")]
    [TestCase(10, Description = "Moderate transfer of 10 corruption")]
    [TestCase(50, Description = "Large transfer of 50 corruption")]
    [TestCase(100, Description = "Maximum possible transfer of 100 corruption")]
    public void Create_VariousAmounts_StoresCorrectly(int amount)
    {
        // Act
        var result = CorruptionTransferResult.Create(
            success: true,
            amountTransferred: amount,
            sourceNewCorruption: 0,
            targetNewCorruption: amount,
            targetTerminalError: amount >= 100);

        // Assert
        result.AmountTransferred.Should().Be(amount);
    }

    // -------------------------------------------------------------------------
    // ToString — Successful Transfer
    // -------------------------------------------------------------------------

    /// <summary>
    /// Verifies ToString for a successful transfer without Terminal Error.
    /// </summary>
    [Test]
    public void ToString_SuccessfulTransfer_FormatsCorrectly()
    {
        // Arrange
        var result = CorruptionTransferResult.Create(
            success: true,
            amountTransferred: 15,
            sourceNewCorruption: 25,
            targetNewCorruption: 60,
            targetTerminalError: false);

        // Act
        var display = result.ToString();

        // Assert
        display.Should().Contain("Transfer SUCCESS");
        display.Should().Contain("15 corruption moved");
        display.Should().Contain("Source: 25");
        display.Should().Contain("Target: 60");
        display.Should().NotContain("TERMINAL ERROR");
    }

    /// <summary>
    /// Verifies ToString for a successful transfer that caused Terminal Error.
    /// </summary>
    [Test]
    public void ToString_SuccessfulTransferWithTerminalError_IncludesWarning()
    {
        // Arrange
        var result = CorruptionTransferResult.Create(
            success: true,
            amountTransferred: 20,
            sourceNewCorruption: 10,
            targetNewCorruption: 100,
            targetTerminalError: true);

        // Act
        var display = result.ToString();

        // Assert
        display.Should().Contain("Transfer SUCCESS");
        display.Should().Contain("TARGET TERMINAL ERROR");
    }

    // -------------------------------------------------------------------------
    // ToString — Failed Transfer
    // -------------------------------------------------------------------------

    /// <summary>
    /// Verifies ToString for a failed transfer.
    /// </summary>
    [Test]
    public void ToString_FailedTransfer_FormatsCorrectly()
    {
        // Arrange
        var result = CorruptionTransferResult.Create(
            success: false,
            amountTransferred: 0,
            sourceNewCorruption: 40,
            targetNewCorruption: 50,
            targetTerminalError: false);

        // Act
        var display = result.ToString();

        // Assert
        display.Should().Contain("Transfer FAILED");
        display.Should().Contain("No corruption moved");
        display.Should().Contain("Source: 40");
        display.Should().Contain("Target: 50");
    }

    // -------------------------------------------------------------------------
    // Record Equality
    // -------------------------------------------------------------------------

    /// <summary>
    /// Verifies that two results with identical properties are equal (record semantics).
    /// </summary>
    [Test]
    public void Equality_SameValues_AreEqual()
    {
        // Arrange
        var result1 = CorruptionTransferResult.Create(true, 10, 30, 60, false);
        var result2 = CorruptionTransferResult.Create(true, 10, 30, 60, false);

        // Assert
        result1.Should().Be(result2);
    }

    /// <summary>
    /// Verifies that two results with different properties are not equal.
    /// </summary>
    [Test]
    public void Equality_DifferentValues_AreNotEqual()
    {
        // Arrange
        var result1 = CorruptionTransferResult.Create(true, 10, 30, 60, false);
        var result2 = CorruptionTransferResult.Create(true, 15, 25, 60, false);

        // Assert
        result1.Should().NotBe(result2);
    }
}
