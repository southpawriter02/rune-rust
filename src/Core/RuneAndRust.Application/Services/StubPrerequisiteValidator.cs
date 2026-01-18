using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Definitions;
using RuneAndRust.Domain.Entities;

namespace RuneAndRust.Application.Services;

/// <summary>
/// Stub implementation of IPrerequisiteValidator that always returns valid.
/// </summary>
/// <remarks>
/// <para>This is a temporary stub implementation for v0.10.2b that allows
/// the TalentPointService to function without full prerequisite validation.</para>
/// <para>Full prerequisite validation will be implemented in v0.10.2c.</para>
/// <para>Current behavior:</para>
/// <list type="bullet">
///   <item><description>ValidatePrerequisites: Always returns Valid()</description></item>
///   <item><description>MeetsNodePrerequisites: Always returns true</description></item>
///   <item><description>MeetsStatPrerequisites: Always returns true</description></item>
/// </list>
/// </remarks>
public class StubPrerequisiteValidator : IPrerequisiteValidator
{
    // ═══════════════════════════════════════════════════════════════
    // DEPENDENCIES
    // ═══════════════════════════════════════════════════════════════

    private readonly ILogger<StubPrerequisiteValidator> _logger;

    // ═══════════════════════════════════════════════════════════════
    // CONSTRUCTOR
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Initializes a new instance of the StubPrerequisiteValidator class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    public StubPrerequisiteValidator(ILogger<StubPrerequisiteValidator> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _logger.LogDebug(
            "StubPrerequisiteValidator initialized - all prerequisites will pass. " +
            "Full validation will be implemented in v0.10.2c");
    }

    // ═══════════════════════════════════════════════════════════════
    // IPrerequisiteValidator IMPLEMENTATION
    // ═══════════════════════════════════════════════════════════════

    /// <inheritdoc />
    /// <remarks>
    /// Stub implementation: Always returns Valid(). Full validation in v0.10.2c.
    /// </remarks>
    public PrerequisiteResult ValidatePrerequisites(Player player, AbilityTreeNode node)
    {
        ArgumentNullException.ThrowIfNull(player, nameof(player));
        ArgumentNullException.ThrowIfNull(node, nameof(node));

        _logger.LogTrace(
            "Stub validation: Player {PlayerName} prerequisites for node {NodeId} - " +
            "returning Valid (stub)",
            player.Name,
            node.NodeId);

        // Stub: Always return valid
        return PrerequisiteResult.Valid();
    }

    /// <inheritdoc />
    /// <remarks>
    /// Stub implementation: Always returns true. Full validation in v0.10.2c.
    /// </remarks>
    public bool MeetsNodePrerequisites(Player player, AbilityTreeNode node)
    {
        ArgumentNullException.ThrowIfNull(player, nameof(player));
        ArgumentNullException.ThrowIfNull(node, nameof(node));

        _logger.LogTrace(
            "Stub validation: Player {PlayerName} node prerequisites for {NodeId} - " +
            "returning true (stub)",
            player.Name,
            node.NodeId);

        // Stub: Always return true
        return true;
    }

    /// <inheritdoc />
    /// <remarks>
    /// Stub implementation: Always returns true. Full validation in v0.10.2c.
    /// </remarks>
    public bool MeetsStatPrerequisites(Player player, AbilityTreeNode node)
    {
        ArgumentNullException.ThrowIfNull(player, nameof(player));
        ArgumentNullException.ThrowIfNull(node, nameof(node));

        _logger.LogTrace(
            "Stub validation: Player {PlayerName} stat prerequisites for {NodeId} - " +
            "returning true (stub)",
            player.Name,
            node.NodeId);

        // Stub: Always return true
        return true;
    }
}
