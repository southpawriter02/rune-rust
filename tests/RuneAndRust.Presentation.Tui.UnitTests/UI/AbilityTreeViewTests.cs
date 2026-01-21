// ═══════════════════════════════════════════════════════════════════════════════
// AbilityTreeViewTests.cs
// Unit tests for AbilityTreeView.
// Version: 0.13.2a
// ═══════════════════════════════════════════════════════════════════════════════

using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Presentation.Tui.Configuration;
using RuneAndRust.Presentation.Tui.DTOs;
using RuneAndRust.Presentation.Tui.Renderers;
using RuneAndRust.Presentation.Tui.UI;

namespace RuneAndRust.Presentation.Tui.UnitTests.UI;

/// <summary>
/// Unit tests for <see cref="AbilityTreeView"/>.
/// </summary>
[TestFixture]
public class AbilityTreeViewTests
{
    private Mock<ITerminalService> _mockTerminal = null!;
    private AbilityTreeDisplayConfig _config = null!;
    private TreeLayoutRenderer _renderer = null!;
    private AbilityTreeView _treeView = null!;

    [SetUp]
    public void SetUp()
    {
        _mockTerminal = new Mock<ITerminalService>();
        _mockTerminal.Setup(t => t.SupportsUnicode).Returns(true);
        _mockTerminal.Setup(t => t.SupportsColor).Returns(true);

        _config = AbilityTreeDisplayConfig.CreateDefault();
        _renderer = new TreeLayoutRenderer(_config, NullLogger<TreeLayoutRenderer>.Instance);

        _treeView = new AbilityTreeView(
            _renderer,
            _mockTerminal.Object,
            _config,
            NullLogger<AbilityTreeView>.Instance);
    }

    // ═══════════════════════════════════════════════════════════════
    // RENDER TREE TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void RenderTree_WithValidDto_RendersHeaderAndTiers()
    {
        // Arrange
        var nodes = new List<NodeLayoutDto>
        {
            new("power-strike", "Power Strike", 1, 0, 0, "berserker")
        };

        var tiers = new List<TierDisplayDto>
        {
            new(1, "TIER 1", nodes)
        };

        var dto = new TreeDisplayDto(
            TreeId: "warrior-tree",
            TreeName: "Warrior Ability Tree",
            ClassId: "warrior",
            Tiers: tiers,
            Branches: new List<BranchDisplayDto>(),
            MaxTier: 1);

        // Act
        _treeView.RenderTree(dto);

        // Assert
        _mockTerminal.Verify(t => t.WriteColoredAt(
            It.IsAny<int>(),
            It.IsAny<int>(),
            It.Is<string>(s => s.Contains("WARRIOR ABILITY TREE")),
            _config.Colors.HeaderColor), Times.Once);
    }

    [Test]
    public void RenderTree_WithMultipleBranches_RendersBranchHeaders()
    {
        // Arrange
        var nodes = new List<NodeLayoutDto>
        {
            new("frenzy", "Frenzy", 1, 0, 0, "berserker"),
            new("iron-skin", "Iron Skin", 1, 0, 1, "guardian")
        };

        var tiers = new List<TierDisplayDto>
        {
            new(1, "TIER 1", nodes)
        };

        var branches = new List<BranchDisplayDto>
        {
            new("berserker", "Berserker", "Offensive abilities", 3),
            new("guardian", "Guardian", "Defensive abilities", 3)
        };

        var dto = new TreeDisplayDto(
            TreeId: "warrior-tree",
            TreeName: "Warrior Ability Tree",
            ClassId: "warrior",
            Tiers: tiers,
            Branches: branches,
            MaxTier: 1);

        // Act
        _treeView.RenderTree(dto);

        // Assert
        _mockTerminal.Verify(t => t.WriteColoredAt(
            It.IsAny<int>(),
            It.IsAny<int>(),
            It.Is<string>(s => s.Contains("BERSERKER")),
            _config.Colors.BranchHeaderColor), Times.Once);
    }

    // ═══════════════════════════════════════════════════════════════
    // HANDLE SELECTION TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void HandleSelection_WithValidPosition_UpdatesSelection()
    {
        // Arrange
        var nodes = new List<NodeLayoutDto>
        {
            new("node-1", "Node 1", 1, 0, 0, "branch"),
            new("node-2", "Node 2", 1, 0, 1, "branch")
        };

        var tiers = new List<TierDisplayDto>
        {
            new(1, "TIER 1", nodes)
        };

        var dto = new TreeDisplayDto(
            TreeId: "test-tree",
            TreeName: "Test Tree",
            ClassId: "test",
            Tiers: tiers,
            Branches: new List<BranchDisplayDto>(),
            MaxTier: 1);

        _treeView.RenderTree(dto);

        // Act
        _treeView.HandleSelection(0, 1);

        // Assert
        _treeView.GetSelectedNodeId().Should().Be("node-2");
        _treeView.GetSelectedPosition().Should().Be((0, 1));
    }

    [Test]
    public void HandleSelection_WithInvalidTierIndex_DoesNotUpdateSelection()
    {
        // Arrange
        var nodes = new List<NodeLayoutDto>
        {
            new("node-1", "Node 1", 1, 0, 0, "branch")
        };

        var tiers = new List<TierDisplayDto>
        {
            new(1, "TIER 1", nodes)
        };

        var dto = new TreeDisplayDto(
            TreeId: "test-tree",
            TreeName: "Test Tree",
            ClassId: "test",
            Tiers: tiers,
            Branches: new List<BranchDisplayDto>(),
            MaxTier: 1);

        _treeView.RenderTree(dto);

        // Act
        _treeView.HandleSelection(5, 0); // Invalid tier index

        // Assert - selection should remain at default (0, 0)
        _treeView.GetSelectedPosition().Should().Be((0, 0));
    }

    // ═══════════════════════════════════════════════════════════════
    // CLEAR TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void Clear_AfterRendering_ResetsState()
    {
        // Arrange
        var nodes = new List<NodeLayoutDto>
        {
            new("node-1", "Node 1", 1, 0, 0, "branch")
        };

        var tiers = new List<TierDisplayDto>
        {
            new(1, "TIER 1", nodes)
        };

        var dto = new TreeDisplayDto(
            TreeId: "test-tree",
            TreeName: "Test Tree",
            ClassId: "test",
            Tiers: tiers,
            Branches: new List<BranchDisplayDto>(),
            MaxTier: 1);

        _treeView.RenderTree(dto);

        // Act
        _treeView.Clear();

        // Assert
        _treeView.GetSelectedNodeId().Should().BeNull();
        _treeView.GetSelectedPosition().Should().Be((0, 0));
    }
}
