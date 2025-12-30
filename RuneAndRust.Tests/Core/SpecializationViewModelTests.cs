using FluentAssertions;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.ViewModels;
using Xunit;

namespace RuneAndRust.Tests.Core;

/// <summary>
/// Tests for the SpecializationViewModel and related record types.
/// Validates immutability, record equality, and proper initialization.
/// </summary>
/// <remarks>See: v0.4.1c (The Tree of Runes) for implementation.</remarks>
public class SpecializationViewModelTests
{
    #region SpecializationViewModel Tests

    [Fact]
    public void SpecializationViewModel_CanBeCreated_WithAllRequiredParameters()
    {
        // Arrange
        var specs = new List<SpecializationListItem>();
        var tree = new SpecializationTreeView(Guid.NewGuid(), "Test Tree", new List<NodeDisplayItem>());

        // Act
        var viewModel = new SpecializationViewModel(
            "TestCharacter",
            15,
            SpecializationViewMode.SpecList,
            specs,
            0,
            tree,
            0);

        // Assert
        viewModel.CharacterName.Should().Be("TestCharacter");
        viewModel.AvailableProgressionPoints.Should().Be(15);
        viewModel.ViewMode.Should().Be(SpecializationViewMode.SpecList);
        viewModel.Specializations.Should().BeSameAs(specs);
        viewModel.SelectedSpecIndex.Should().Be(0);
        viewModel.CurrentTree.Should().BeSameAs(tree);
        viewModel.SelectedNodeIndex.Should().Be(0);
        viewModel.StatusMessage.Should().BeNull();
    }

    [Fact]
    public void SpecializationViewModel_StatusMessage_CanBeSetOptionally()
    {
        // Arrange & Act
        var viewModel = new SpecializationViewModel(
            "Hero",
            10,
            SpecializationViewMode.TreeDetail,
            new List<SpecializationListItem>(),
            0,
            null,
            0,
            "Unlocked successfully!");

        // Assert
        viewModel.StatusMessage.Should().Be("Unlocked successfully!");
    }

    [Fact]
    public void SpecializationViewModel_RecordEquality_WorksCorrectly()
    {
        // Arrange
        var specs = new List<SpecializationListItem>();
        var vm1 = new SpecializationViewModel("Hero", 10, SpecializationViewMode.SpecList, specs, 0, null, 0);
        var vm2 = new SpecializationViewModel("Hero", 10, SpecializationViewMode.SpecList, specs, 0, null, 0);
        var vm3 = new SpecializationViewModel("Hero", 15, SpecializationViewMode.SpecList, specs, 0, null, 0);

        // Assert
        vm1.Should().Be(vm2);
        vm1.Should().NotBe(vm3);
    }

    [Fact]
    public void SpecializationViewModel_With_CreatesModifiedCopy()
    {
        // Arrange
        var original = new SpecializationViewModel(
            "Hero", 10, SpecializationViewMode.SpecList,
            new List<SpecializationListItem>(), 0, null, 0);

        // Act
        var modified = original with { AvailableProgressionPoints = 5 };

        // Assert
        modified.AvailableProgressionPoints.Should().Be(5);
        original.AvailableProgressionPoints.Should().Be(10); // Original unchanged
    }

    [Fact]
    public void SpecializationViewModel_With_CanChangeViewMode()
    {
        // Arrange
        var original = new SpecializationViewModel(
            "Hero", 10, SpecializationViewMode.SpecList,
            new List<SpecializationListItem>(), 0, null, 0);

        // Act
        var modified = original with { ViewMode = SpecializationViewMode.TreeDetail };

        // Assert
        modified.ViewMode.Should().Be(SpecializationViewMode.TreeDetail);
        original.ViewMode.Should().Be(SpecializationViewMode.SpecList);
    }

    #endregion

    #region SpecializationListItem Tests

    [Fact]
    public void SpecializationListItem_CanBeCreated_WithAllParameters()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var item = new SpecializationListItem(
            id,
            "Iron Warden",
            "Tank specialization",
            true,
            true,
            10,
            5,
            12);

        // Assert
        item.Id.Should().Be(id);
        item.Name.Should().Be("Iron Warden");
        item.Description.Should().Be("Tank specialization");
        item.IsUnlocked.Should().BeTrue();
        item.CanUnlock.Should().BeTrue();
        item.UnlockCost.Should().Be(10);
        item.NodesUnlocked.Should().Be(5);
        item.TotalNodes.Should().Be(12);
    }

    [Fact]
    public void SpecializationListItem_RecordEquality_WorksCorrectly()
    {
        // Arrange
        var id = Guid.NewGuid();
        var item1 = new SpecializationListItem(id, "Iron Warden", "Desc", true, true, 10, 5, 12);
        var item2 = new SpecializationListItem(id, "Iron Warden", "Desc", true, true, 10, 5, 12);
        var item3 = new SpecializationListItem(id, "Iron Warden", "Desc", false, true, 10, 5, 12);

        // Assert
        item1.Should().Be(item2);
        item1.Should().NotBe(item3);
    }

    [Fact]
    public void SpecializationListItem_With_CreatesModifiedCopy()
    {
        // Arrange
        var original = new SpecializationListItem(
            Guid.NewGuid(), "Iron Warden", "Desc", false, true, 10, 0, 12);

        // Act
        var modified = original with { IsUnlocked = true, NodesUnlocked = 3 };

        // Assert
        modified.IsUnlocked.Should().BeTrue();
        modified.NodesUnlocked.Should().Be(3);
        original.IsUnlocked.Should().BeFalse();
        original.NodesUnlocked.Should().Be(0);
    }

    #endregion

    #region SpecializationTreeView Tests

    [Fact]
    public void SpecializationTreeView_CanBeCreated_WithAllParameters()
    {
        // Arrange
        var specId = Guid.NewGuid();
        var nodes = new List<NodeDisplayItem>
        {
            CreateNodeDisplayItem("Node1", 1)
        };

        // Act
        var tree = new SpecializationTreeView(specId, "Iron Warden", nodes);

        // Assert
        tree.SpecializationId.Should().Be(specId);
        tree.SpecializationName.Should().Be("Iron Warden");
        tree.Nodes.Should().HaveCount(1);
    }

    [Fact]
    public void SpecializationTreeView_Nodes_IsReadOnly()
    {
        // Arrange
        var nodes = new List<NodeDisplayItem>();
        var tree = new SpecializationTreeView(Guid.NewGuid(), "Test", nodes);

        // Assert
        tree.Nodes.Should().BeAssignableTo<IReadOnlyList<NodeDisplayItem>>();
    }

    #endregion

    #region NodeDisplayItem Tests

    [Fact]
    public void NodeDisplayItem_CanBeCreated_WithAllParameters()
    {
        // Arrange
        var nodeId = Guid.NewGuid();
        var parentIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };

        // Act
        var node = new NodeDisplayItem(
            nodeId,
            "Shield Bash",
            "Shield Bash",
            "Deal damage with shield",
            2,
            2,
            NodeStatus.Available,
            0,
            1,
            false,
            parentIds);

        // Assert
        node.NodeId.Should().Be(nodeId);
        node.NodeName.Should().Be("Shield Bash");
        node.AbilityName.Should().Be("Shield Bash");
        node.AbilityDescription.Should().Be("Deal damage with shield");
        node.Tier.Should().Be(2);
        node.CostPP.Should().Be(2);
        node.Status.Should().Be(NodeStatus.Available);
        node.PositionX.Should().Be(0);
        node.PositionY.Should().Be(1);
        node.IsCapstone.Should().BeFalse();
        node.ParentNodeIds.Should().HaveCount(2);
    }

    [Fact]
    public void NodeDisplayItem_CapstoneNode_HasTier4()
    {
        // Arrange & Act
        var node = CreateNodeDisplayItem("Ultimate Power", 4, isCapstone: true);

        // Assert
        node.Tier.Should().Be(4);
        node.IsCapstone.Should().BeTrue();
    }

    [Theory]
    [InlineData(NodeStatus.Locked)]
    [InlineData(NodeStatus.InsufficientPP)]
    [InlineData(NodeStatus.Available)]
    [InlineData(NodeStatus.Unlocked)]
    public void NodeDisplayItem_CanHaveAnyNodeStatus(NodeStatus status)
    {
        // Arrange & Act
        var node = new NodeDisplayItem(
            Guid.NewGuid(), "Test", "Test", "Desc",
            1, 1, status, 0, 0, false, new List<Guid>());

        // Assert
        node.Status.Should().Be(status);
    }

    [Fact]
    public void NodeDisplayItem_ParentNodeIds_CanBeEmpty()
    {
        // Arrange & Act
        var node = CreateNodeDisplayItem("Root Node", 1);

        // Assert - Tier 1 nodes typically have no parents
        node.ParentNodeIds.Should().BeEmpty();
    }

    [Fact]
    public void NodeDisplayItem_RecordEquality_WorksCorrectly()
    {
        // Arrange
        var nodeId = Guid.NewGuid();
        var parentIds = new List<Guid>();

        var node1 = new NodeDisplayItem(nodeId, "Test", "Test", "Desc", 1, 1, NodeStatus.Available, 0, 0, false, parentIds);
        var node2 = new NodeDisplayItem(nodeId, "Test", "Test", "Desc", 1, 1, NodeStatus.Available, 0, 0, false, parentIds);
        var node3 = new NodeDisplayItem(nodeId, "Test", "Test", "Desc", 1, 1, NodeStatus.Unlocked, 0, 0, false, parentIds);

        // Assert
        node1.Should().Be(node2);
        node1.Should().NotBe(node3);
    }

    [Fact]
    public void NodeDisplayItem_With_CanChangeStatus()
    {
        // Arrange
        var original = CreateNodeDisplayItem("Test", 1, NodeStatus.Available);

        // Act
        var modified = original with { Status = NodeStatus.Unlocked };

        // Assert
        modified.Status.Should().Be(NodeStatus.Unlocked);
        original.Status.Should().Be(NodeStatus.Available);
    }

    #endregion

    #region Helper Methods

    private static NodeDisplayItem CreateNodeDisplayItem(
        string name,
        int tier,
        NodeStatus status = NodeStatus.Available,
        bool isCapstone = false)
    {
        var cost = tier switch
        {
            1 => 1,
            2 => 2,
            3 => 3,
            4 => 5,
            _ => 1
        };

        return new NodeDisplayItem(
            Guid.NewGuid(),
            name,
            name,
            $"Description for {name}",
            tier,
            cost,
            status,
            0,
            tier - 1,
            isCapstone || tier == 4,
            new List<Guid>());
    }

    #endregion
}
