// ------------------------------------------------------------------------------
// <copyright file="PersuasionRequestTests.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Unit tests for PersuasionRequest enum and extensions.
// Part of v0.15.3b Persuasion System implementation.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Domain.UnitTests.Enums;

using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Extensions;

/// <summary>
/// Unit tests for <see cref="PersuasionRequest"/> enum and extensions.
/// </summary>
[TestFixture]
public class PersuasionRequestTests
{
    #region GetBaseDc Tests

    /// <summary>
    /// Verifies that each request type returns the correct base DC.
    /// </summary>
    [TestCase(PersuasionRequest.Trivial, 8)]
    [TestCase(PersuasionRequest.Simple, 12)]
    [TestCase(PersuasionRequest.Moderate, 16)]
    [TestCase(PersuasionRequest.Major, 20)]
    [TestCase(PersuasionRequest.Extreme, 24)]
    public void GetBaseDc_ReturnsCorrectValue(PersuasionRequest request, int expectedDc)
    {
        // Act
        var baseDc = request.GetBaseDc();

        // Assert
        baseDc.Should().Be(expectedDc, because: $"{request} should have DC {expectedDc}");
    }

    #endregion

    #region GetRequiredSuccesses Tests

    /// <summary>
    /// Verifies that each request type returns the correct required successes.
    /// </summary>
    [TestCase(PersuasionRequest.Trivial, 1)]
    [TestCase(PersuasionRequest.Simple, 2)]
    [TestCase(PersuasionRequest.Moderate, 3)]
    [TestCase(PersuasionRequest.Major, 4)]
    [TestCase(PersuasionRequest.Extreme, 4)]
    public void GetRequiredSuccesses_ReturnsCorrectValue(PersuasionRequest request, int expectedSuccesses)
    {
        // Act
        var requiredSuccesses = request.GetRequiredSuccesses();

        // Assert
        requiredSuccesses.Should().Be(expectedSuccesses);
    }

    #endregion

    #region GetMinimumDisposition Tests

    /// <summary>
    /// Verifies that each request type returns the correct minimum disposition.
    /// </summary>
    [TestCase(PersuasionRequest.Trivial, NpcDisposition.Unfriendly)]
    [TestCase(PersuasionRequest.Simple, NpcDisposition.Neutral)]
    [TestCase(PersuasionRequest.Moderate, NpcDisposition.NeutralPositive)]
    [TestCase(PersuasionRequest.Major, NpcDisposition.Friendly)]
    [TestCase(PersuasionRequest.Extreme, NpcDisposition.Ally)]
    public void GetMinimumDisposition_ReturnsCorrectValue(PersuasionRequest request, NpcDisposition expectedDisposition)
    {
        // Act
        var minimumDisposition = request.GetMinimumDisposition();

        // Assert
        minimumDisposition.Should().Be(expectedDisposition);
    }

    #endregion

    #region MayBeImpossible Tests

    /// <summary>
    /// Verifies that only Extreme requests may be impossible.
    /// </summary>
    [TestCase(PersuasionRequest.Trivial, false)]
    [TestCase(PersuasionRequest.Simple, false)]
    [TestCase(PersuasionRequest.Moderate, false)]
    [TestCase(PersuasionRequest.Major, false)]
    [TestCase(PersuasionRequest.Extreme, true)]
    public void MayBeImpossible_ReturnsCorrectValue(PersuasionRequest request, bool expected)
    {
        // Act
        var mayBeImpossible = request.MayBeImpossible();

        // Assert
        mayBeImpossible.Should().Be(expected);
    }

    #endregion

    #region GetDescription Tests

    /// <summary>
    /// Verifies that descriptions contain the DC value.
    /// </summary>
    [TestCase(PersuasionRequest.Trivial, "DC 8")]
    [TestCase(PersuasionRequest.Simple, "DC 12")]
    [TestCase(PersuasionRequest.Moderate, "DC 16")]
    [TestCase(PersuasionRequest.Major, "DC 20")]
    [TestCase(PersuasionRequest.Extreme, "DC 24")]
    public void GetDescription_ContainsDcValue(PersuasionRequest request, string expectedDcText)
    {
        // Act
        var description = request.GetDescription();

        // Assert
        description.Should().Contain(expectedDcText);
    }

    #endregion
}
