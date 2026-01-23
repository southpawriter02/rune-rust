namespace RuneAndRust.Domain.UnitTests.ValueObjects;

using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Unit tests for <see cref="SkillDescriptor"/> value object.
/// </summary>
[TestFixture]
public class SkillDescriptorTests
{
    [Test]
    public void Empty_CreatesDescriptorWithNoContent()
    {
        // Act
        var descriptor = SkillDescriptor.Empty("lockpicking", DescriptorCategory.Competent);

        // Assert
        descriptor.SkillId.Should().Be("lockpicking");
        descriptor.Category.Should().Be(DescriptorCategory.Competent);
        descriptor.Text.Should().BeEmpty();
        descriptor.HasContent.Should().BeFalse();
    }

    [Test]
    public void GenericFallback_CreatesDescriptorForCategory()
    {
        // Act
        var descriptor = SkillDescriptor.GenericFallback(DescriptorCategory.Masterful);

        // Assert
        descriptor.SkillId.Should().Be("generic");
        descriptor.Category.Should().Be(DescriptorCategory.Masterful);
        descriptor.HasContent.Should().BeTrue();
        descriptor.Text.Should().Contain("masterful");
    }

    [Test]
    [TestCase(DescriptorCategory.Catastrophic, "terribly wrong")]
    [TestCase(DescriptorCategory.Failed, "fails")]
    [TestCase(DescriptorCategory.Marginal, "barely")]
    [TestCase(DescriptorCategory.Competent, "succeed")]
    [TestCase(DescriptorCategory.Impressive, "impressively")]
    [TestCase(DescriptorCategory.Masterful, "masterful")]
    public void GenericFallback_ProvidesAppropriateText(DescriptorCategory category, string expectedSubstring)
    {
        // Act
        var descriptor = SkillDescriptor.GenericFallback(category);

        // Assert
        descriptor.Text.Should().ContainEquivalentOf(expectedSubstring);
    }

    [Test]
    public void IsFallback_TrueWhenNotContextual()
    {
        // Arrange
        var descriptor = new SkillDescriptor(
            SkillId: "lockpicking",
            Category: DescriptorCategory.Competent,
            Text: "The lock opens.");

        // Assert
        descriptor.IsFallback.Should().BeTrue();
        descriptor.IsContextual.Should().BeFalse();
    }

    [Test]
    public void IsFallback_FalseWhenContextual()
    {
        // Arrange
        var descriptor = new SkillDescriptor(
            SkillId: "lockpicking",
            Category: DescriptorCategory.Catastrophic,
            Text: "Reality shudders as the lock phases...",
            IsContextual: true,
            ContextType: "glitched");

        // Assert
        descriptor.IsFallback.Should().BeFalse();
        descriptor.IsContextual.Should().BeTrue();
        descriptor.ContextType.Should().Be("glitched");
    }

    [Test]
    public void ToString_ReturnsText()
    {
        // Arrange
        var descriptor = new SkillDescriptor(
            SkillId: "persuasion",
            Category: DescriptorCategory.Competent,
            Text: "Your argument resonates.");

        // Act
        var text = descriptor.ToString();

        // Assert
        text.Should().Be("Your argument resonates.");
    }
}
