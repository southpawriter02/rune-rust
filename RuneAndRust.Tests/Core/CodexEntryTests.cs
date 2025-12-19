using FluentAssertions;
using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;
using Xunit;

namespace RuneAndRust.Tests.Core;

/// <summary>
/// Tests for the CodexEntry entity.
/// Validates entry creation, properties, defaults, and unlock threshold operations.
/// </summary>
public class CodexEntryTests
{
    #region Identity Tests

    [Fact]
    public void CodexEntry_NewEntry_HasUniqueId()
    {
        // Arrange & Act
        var entry1 = new CodexEntry();
        var entry2 = new CodexEntry();

        // Assert
        entry1.Id.Should().NotBeEmpty();
        entry2.Id.Should().NotBeEmpty();
        entry1.Id.Should().NotBe(entry2.Id, "each entry should have a unique Id");
    }

    [Fact]
    public void CodexEntry_Title_DefaultsToEmptyString()
    {
        // Arrange & Act
        var entry = new CodexEntry();

        // Assert
        entry.Title.Should().BeEmpty();
    }

    [Fact]
    public void CodexEntry_Title_CanBeSet()
    {
        // Arrange
        var entry = new CodexEntry();

        // Act
        entry.Title = "The Ginnungagap Glitch";

        // Assert
        entry.Title.Should().Be("The Ginnungagap Glitch");
    }

    #endregion

    #region Classification Tests

    [Fact]
    public void CodexEntry_Category_DefaultsToFieldGuide()
    {
        // Arrange & Act
        var entry = new CodexEntry();

        // Assert
        entry.Category.Should().Be(EntryCategory.FieldGuide);
    }

    [Theory]
    [InlineData(EntryCategory.FieldGuide)]
    [InlineData(EntryCategory.BlightOrigin)]
    [InlineData(EntryCategory.Bestiary)]
    [InlineData(EntryCategory.Factions)]
    [InlineData(EntryCategory.Technical)]
    [InlineData(EntryCategory.Geography)]
    public void CodexEntry_Category_CanBeSetToAllCategories(EntryCategory category)
    {
        // Arrange
        var entry = new CodexEntry();

        // Act
        entry.Category = category;

        // Assert
        entry.Category.Should().Be(category);
    }

    #endregion

    #region Content Tests

    [Fact]
    public void CodexEntry_FullText_DefaultsToEmptyString()
    {
        // Arrange & Act
        var entry = new CodexEntry();

        // Assert
        entry.FullText.Should().BeEmpty();
    }

    [Fact]
    public void CodexEntry_FullText_CanBeSet()
    {
        // Arrange
        var entry = new CodexEntry();
        var loreText = "A humanoid automaton of ancient Aesir design, now corrupted by the Blight.";

        // Act
        entry.FullText = loreText;

        // Assert
        entry.FullText.Should().Be(loreText);
    }

    [Fact]
    public void CodexEntry_TotalFragments_DefaultsToOne()
    {
        // Arrange & Act
        var entry = new CodexEntry();

        // Assert
        entry.TotalFragments.Should().Be(1);
    }

    [Fact]
    public void CodexEntry_TotalFragments_CanBeSet()
    {
        // Arrange
        var entry = new CodexEntry();

        // Act
        entry.TotalFragments = 4;

        // Assert
        entry.TotalFragments.Should().Be(4);
    }

    #endregion

    #region Unlock Threshold Tests

    [Fact]
    public void CodexEntry_UnlockThresholds_DefaultsToEmptyDictionary()
    {
        // Arrange & Act
        var entry = new CodexEntry();

        // Assert
        entry.UnlockThresholds.Should().NotBeNull();
        entry.UnlockThresholds.Should().BeEmpty();
    }

    [Fact]
    public void CodexEntry_UnlockThresholds_CanAddEntries()
    {
        // Arrange
        var entry = new CodexEntry();

        // Act
        entry.UnlockThresholds[25] = "WEAKNESS_REVEALED";
        entry.UnlockThresholds[50] = "HABITAT_REVEALED";
        entry.UnlockThresholds[100] = "FULL_ENTRY";

        // Assert
        entry.UnlockThresholds.Should().HaveCount(3);
        entry.UnlockThresholds[25].Should().Be("WEAKNESS_REVEALED");
        entry.UnlockThresholds[50].Should().Be("HABITAT_REVEALED");
        entry.UnlockThresholds[100].Should().Be("FULL_ENTRY");
    }

    [Fact]
    public void CodexEntry_UnlockThresholds_CanBeInitializedInline()
    {
        // Arrange & Act
        var entry = new CodexEntry
        {
            UnlockThresholds = new Dictionary<int, string>
            {
                { 25, "WEAKNESS_REVEALED" },
                { 50, "HABITAT_REVEALED" },
                { 75, "BEHAVIOR_REVEALED" },
                { 100, "FULL_ENTRY" }
            }
        };

        // Assert
        entry.UnlockThresholds.Should().HaveCount(4);
        entry.UnlockThresholds.Should().ContainKey(75);
    }

    [Fact]
    public void CodexEntry_UnlockThresholds_CanCheckForThreshold()
    {
        // Arrange
        var entry = new CodexEntry
        {
            UnlockThresholds = new Dictionary<int, string>
            {
                { 25, "WEAKNESS_REVEALED" },
                { 50, "HABITAT_REVEALED" }
            }
        };

        // Act & Assert
        entry.UnlockThresholds.ContainsKey(25).Should().BeTrue();
        entry.UnlockThresholds.ContainsKey(75).Should().BeFalse();
    }

    #endregion

    #region Navigation Tests

    [Fact]
    public void CodexEntry_Fragments_DefaultsToEmptyCollection()
    {
        // Arrange & Act
        var entry = new CodexEntry();

        // Assert
        entry.Fragments.Should().NotBeNull();
        entry.Fragments.Should().BeEmpty();
    }

    [Fact]
    public void CodexEntry_Fragments_CanAddDataCaptures()
    {
        // Arrange
        var entry = new CodexEntry { Title = "Test Entry" };
        var capture = new DataCapture
        {
            CharacterId = Guid.NewGuid(),
            FragmentContent = "A fragment of lore...",
            Source = "Found on corpse"
        };

        // Act
        entry.Fragments.Add(capture);

        // Assert
        entry.Fragments.Should().HaveCount(1);
        entry.Fragments.Should().Contain(capture);
    }

    #endregion

    #region Metadata Tests

    [Fact]
    public void CodexEntry_CreatedAt_IsSetAutomatically()
    {
        // Arrange
        var before = DateTime.UtcNow;

        // Act
        var entry = new CodexEntry();

        // Assert
        var after = DateTime.UtcNow;
        entry.CreatedAt.Should().BeOnOrAfter(before);
        entry.CreatedAt.Should().BeOnOrBefore(after);
    }

    [Fact]
    public void CodexEntry_LastModified_IsSetAutomatically()
    {
        // Arrange
        var before = DateTime.UtcNow;

        // Act
        var entry = new CodexEntry();

        // Assert
        var after = DateTime.UtcNow;
        entry.LastModified.Should().BeOnOrAfter(before);
        entry.LastModified.Should().BeOnOrBefore(after);
    }

    #endregion

    #region Full Entry Creation Tests

    [Fact]
    public void CodexEntry_FullCreation_AllPropertiesSet()
    {
        // Arrange & Act
        var entry = new CodexEntry
        {
            Title = "Rusted Servitor",
            Category = EntryCategory.Bestiary,
            FullText = "A humanoid automaton of ancient Aesir design, corrupted by centuries of Blight exposure.",
            TotalFragments = 4,
            UnlockThresholds = new Dictionary<int, string>
            {
                { 25, "WEAKNESS_REVEALED" },
                { 50, "HABITAT_REVEALED" },
                { 75, "BEHAVIOR_REVEALED" },
                { 100, "FULL_ENTRY" }
            }
        };

        // Assert
        entry.Id.Should().NotBeEmpty();
        entry.Title.Should().Be("Rusted Servitor");
        entry.Category.Should().Be(EntryCategory.Bestiary);
        entry.FullText.Should().Contain("automaton");
        entry.TotalFragments.Should().Be(4);
        entry.UnlockThresholds.Should().HaveCount(4);
        entry.Fragments.Should().BeEmpty();
        entry.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    #endregion
}
