// ------------------------------------------------------------------------------
// <copyright file="ArgumentAlignmentTests.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Unit tests for ArgumentAlignment value object.
// Part of v0.15.3b Persuasion System implementation.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Unit tests for <see cref="ArgumentAlignment"/> value object.
/// </summary>
[TestFixture]
public class ArgumentAlignmentTests
{
    #region Aligned Tests

    /// <summary>
    /// Verifies that aligned arguments provide +1d10 modifier.
    /// </summary>
    [Test]
    public void Evaluate_WhenArgumentAligned_ProvidesPlusOneDice()
    {
        // Arrange
        var npcValues = new[] { "loyalty", "profit", "honor" };
        var npcDislikes = new[] { "cowardice", "betrayal" };
        var argumentThemes = new[] { "loyalty", "duty" };

        // Act
        var alignment = ArgumentAlignment.Evaluate(argumentThemes, npcValues, npcDislikes);

        // Assert
        alignment.IsAligned.Should().BeTrue();
        alignment.IsContradicting.Should().BeFalse();
        alignment.DiceModifier.Should().Be(1);
        alignment.MatchedValue.Should().Be("loyalty");
    }

    #endregion

    #region Contradicting Tests

    /// <summary>
    /// Verifies that contradicting arguments provide -1d10 modifier.
    /// </summary>
    [Test]
    public void Evaluate_WhenArgumentContradicts_ProvidesMinusOneDice()
    {
        // Arrange
        var npcValues = new[] { "loyalty", "profit" };
        var npcDislikes = new[] { "cowardice", "betrayal" };
        var argumentThemes = new[] { "betrayal", "self-interest" };

        // Act
        var alignment = ArgumentAlignment.Evaluate(argumentThemes, npcValues, npcDislikes);

        // Assert
        alignment.IsContradicting.Should().BeTrue();
        alignment.IsAligned.Should().BeFalse();
        alignment.DiceModifier.Should().Be(-1);
        alignment.ContradictedValue.Should().Be("betrayal");
    }

    #endregion

    #region Neutral Tests

    /// <summary>
    /// Verifies that neutral arguments provide 0 modifier.
    /// </summary>
    [Test]
    public void Evaluate_WhenArgumentNeutral_ProvidesZeroDice()
    {
        // Arrange
        var npcValues = new[] { "loyalty", "profit" };
        var npcDislikes = new[] { "cowardice", "betrayal" };
        var argumentThemes = new[] { "weather", "time" };

        // Act
        var alignment = ArgumentAlignment.Evaluate(argumentThemes, npcValues, npcDislikes);

        // Assert
        alignment.IsAligned.Should().BeFalse();
        alignment.IsContradicting.Should().BeFalse();
        alignment.DiceModifier.Should().Be(0);
        alignment.HasModifier.Should().BeFalse();
    }

    #endregion

    #region Priority Tests

    /// <summary>
    /// Verifies that contradictions take priority over alignments.
    /// </summary>
    [Test]
    public void Evaluate_WhenBothAlignedAndContradicting_ContradictionTakesPriority()
    {
        // Arrange
        var npcValues = new[] { "loyalty", "profit" };
        var npcDislikes = new[] { "betrayal" };
        var argumentThemes = new[] { "loyalty", "betrayal" }; // Both match

        // Act
        var alignment = ArgumentAlignment.Evaluate(argumentThemes, npcValues, npcDislikes);

        // Assert - Contradiction should win
        alignment.IsContradicting.Should().BeTrue();
        alignment.IsAligned.Should().BeFalse();
        alignment.DiceModifier.Should().Be(-1);
    }

    #endregion

    #region Factory Method Tests

    /// <summary>
    /// Verifies CreateNeutral factory method.
    /// </summary>
    [Test]
    public void CreateNeutral_ReturnsNeutralAlignment()
    {
        // Act
        var alignment = ArgumentAlignment.CreateNeutral();

        // Assert
        alignment.IsAligned.Should().BeFalse();
        alignment.IsContradicting.Should().BeFalse();
        alignment.DiceModifier.Should().Be(0);
        alignment.Reason.Should().NotBeNullOrWhiteSpace();
    }

    /// <summary>
    /// Verifies CreateAligned factory method.
    /// </summary>
    [Test]
    public void CreateAligned_ReturnsAlignedWithValue()
    {
        // Act
        var alignment = ArgumentAlignment.CreateAligned("honor");

        // Assert
        alignment.IsAligned.Should().BeTrue();
        alignment.MatchedValue.Should().Be("honor");
        alignment.DiceModifier.Should().Be(1);
    }

    /// <summary>
    /// Verifies CreateContradicting factory method.
    /// </summary>
    [Test]
    public void CreateContradicting_ReturnsContradictingWithValue()
    {
        // Act
        var alignment = ArgumentAlignment.CreateContradicting("betrayal");

        // Assert
        alignment.IsContradicting.Should().BeTrue();
        alignment.ContradictedValue.Should().Be("betrayal");
        alignment.DiceModifier.Should().Be(-1);
    }

    #endregion
}
