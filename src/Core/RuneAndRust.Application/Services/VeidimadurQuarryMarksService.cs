using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Services;

/// <summary>
/// Service handling Veiðimaðr (Hunter) Quarry Marks resource management.
/// Manages mark creation, removal, querying, and lifecycle operations.
/// </summary>
/// <remarks>
/// <para>Named <c>VeidimadurQuarryMarksService</c> to coexist alongside other specialization
/// resource services (e.g., <see cref="BoneSetterMedicalSuppliesService"/>,
/// <see cref="BerserkrRageService"/>).</para>
/// <para>Key differences from other resource services:</para>
/// <list type="bullet">
/// <item>Mutable model — marks are added/removed in-place on the <see cref="QuarryMarksResource"/>,
/// similar to <see cref="BerserkrRageService"/> and unlike the immutable
/// <see cref="BoneSetterMedicalSuppliesService"/></item>
/// <item>Target-tracking — marks are discrete objects tracking specific enemies, not a numeric meter</item>
/// <item>FIFO replacement — when at max capacity (3), oldest mark is automatically replaced</item>
/// <item>Encounter-scoped — all marks clear at encounter end</item>
/// <item>No Corruption interaction — the Veiðimaðr follows the Coherent path</item>
/// </list>
/// <para>Introduced in v0.20.7a as part of the Veiðimaðr specialization framework.</para>
/// </remarks>
public class VeidimadurQuarryMarksService : IVeidimadurQuarryMarksService
{
    /// <summary>
    /// The specialization ID string for Veiðimaðr (Hunter).
    /// </summary>
    private const string VeidimadurSpecId = "veidimadur";

    private readonly ILogger<VeidimadurQuarryMarksService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="VeidimadurQuarryMarksService"/> class.
    /// </summary>
    /// <param name="logger">Logger for quarry marks management events.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="logger"/> is null.</exception>
    public VeidimadurQuarryMarksService(ILogger<VeidimadurQuarryMarksService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public void InitializeQuarryMarks(Player player)
    {
        ArgumentNullException.ThrowIfNull(player);

        player.InitializeQuarryMarks();

        _logger.LogInformation(
            "Quarry Marks initialized: {Player} ({PlayerId}) resource set to " +
            "{Current}/{Max} (empty)",
            player.Name, player.Id, 0, QuarryMarksResource.DefaultMaxMarks);
    }

    /// <inheritdoc />
    public QuarryMarksResource? GetQuarryMarks(Player player)
    {
        ArgumentNullException.ThrowIfNull(player);
        return player.QuarryMarks;
    }

    /// <inheritdoc />
    public QuarryMark? AddMark(Player player, QuarryMark mark)
    {
        ArgumentNullException.ThrowIfNull(player);
        ArgumentNullException.ThrowIfNull(mark);

        if (player.QuarryMarks == null)
        {
            _logger.LogWarning(
                "Add mark failed: {Player} ({PlayerId}) has no Quarry Marks resource initialized",
                player.Name, player.Id);
            return null;
        }

        var previousCount = player.QuarryMarks.CurrentMarkCount;
        var replacedMark = player.QuarryMarks.AddMark(mark);

        if (replacedMark != null)
        {
            _logger.LogInformation(
                "Quarry Mark replaced (FIFO): {Player} ({PlayerId}) mark on {OldTarget} ({OldTargetId}) " +
                "replaced by new mark on {NewTarget} ({NewTargetId}). " +
                "Marks: {PreviousCount}/{MaxMarks} -> {CurrentCount}/{MaxMarks}",
                player.Name, player.Id,
                replacedMark.TargetName, replacedMark.TargetId,
                mark.TargetName, mark.TargetId,
                previousCount, player.QuarryMarks.MaxMarks,
                player.QuarryMarks.CurrentMarkCount, player.QuarryMarks.MaxMarks);
        }
        else
        {
            _logger.LogInformation(
                "Quarry Mark added: {Player} ({PlayerId}) marked {Target} ({TargetId}). " +
                "Marks: {PreviousCount}/{MaxMarks} -> {CurrentCount}/{MaxMarks}",
                player.Name, player.Id,
                mark.TargetName, mark.TargetId,
                previousCount, player.QuarryMarks.MaxMarks,
                player.QuarryMarks.CurrentMarkCount, player.QuarryMarks.MaxMarks);
        }

        return replacedMark;
    }

    /// <inheritdoc />
    public bool RemoveMark(Player player, Guid targetId)
    {
        ArgumentNullException.ThrowIfNull(player);

        if (player.QuarryMarks == null)
        {
            _logger.LogWarning(
                "Remove mark failed: {Player} ({PlayerId}) has no Quarry Marks resource initialized",
                player.Name, player.Id);
            return false;
        }

        var mark = player.QuarryMarks.GetMark(targetId);
        var removed = player.QuarryMarks.RemoveMark(targetId);

        if (removed)
        {
            _logger.LogInformation(
                "Quarry Mark removed: {Player} ({PlayerId}) removed mark on {Target} ({TargetId}). " +
                "Marks remaining: {CurrentCount}/{MaxMarks}",
                player.Name, player.Id,
                mark?.TargetName ?? "Unknown", targetId,
                player.QuarryMarks.CurrentMarkCount, player.QuarryMarks.MaxMarks);
        }
        else
        {
            _logger.LogWarning(
                "Remove mark failed: {Player} ({PlayerId}) has no mark on target {TargetId}",
                player.Name, player.Id, targetId);
        }

        return removed;
    }

    /// <inheritdoc />
    public bool HasActiveMark(Player player, Guid targetId)
    {
        ArgumentNullException.ThrowIfNull(player);
        return player.QuarryMarks?.HasMark(targetId) ?? false;
    }

    /// <inheritdoc />
    public QuarryMark? GetMarkFor(Player player, Guid targetId)
    {
        ArgumentNullException.ThrowIfNull(player);
        return player.QuarryMarks?.GetMark(targetId);
    }

    /// <inheritdoc />
    public bool CanAddMark(Player player)
    {
        ArgumentNullException.ThrowIfNull(player);
        return player.QuarryMarks?.CanAddMark() ?? false;
    }

    /// <inheritdoc />
    public void ClearAllMarks(Player player)
    {
        ArgumentNullException.ThrowIfNull(player);

        if (player.QuarryMarks == null)
        {
            _logger.LogWarning(
                "Clear marks failed: {Player} ({PlayerId}) has no Quarry Marks resource initialized",
                player.Name, player.Id);
            return;
        }

        var previousCount = player.QuarryMarks.CurrentMarkCount;
        player.QuarryMarks.ClearAllMarks();

        _logger.LogInformation(
            "Quarry Marks cleared: {Player} ({PlayerId}) cleared {ClearedCount} marks. " +
            "Marks: {CurrentCount}/{MaxMarks}",
            player.Name, player.Id, previousCount,
            player.QuarryMarks.CurrentMarkCount, player.QuarryMarks.MaxMarks);
    }

    /// <inheritdoc />
    public int GetMarkCount(Player player)
    {
        ArgumentNullException.ThrowIfNull(player);
        return player.QuarryMarks?.CurrentMarkCount ?? 0;
    }
}
