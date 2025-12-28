namespace RuneAndRust.Engine.Services;

using Microsoft.Extensions.Logging;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Interfaces;

/// <summary>
/// Manages screen-to-game coordinate mapping for mouse interaction.
/// </summary>
/// <remarks>
/// See: SPEC-INPUT-002 for Mouse Support System design.
/// v0.3.23c: Initial implementation.
/// </remarks>
public class HitTestService : IHitTestService
{
    private readonly ILogger<HitTestService> _logger;
    private readonly List<ClickableRegion> _regions = new();

    /// <summary>
    /// Internal record for tracking registered clickable regions.
    /// </summary>
    private record ClickableRegion(
        HitTargetType Type,
        int Left, int Top, int Right, int Bottom,
        int? Index, int? Row, int? Column,
        object? Data);

    /// <summary>
    /// Initializes a new instance of the <see cref="HitTestService"/> class.
    /// </summary>
    /// <param name="logger">The logger for traceability.</param>
    public HitTestService(ILogger<HitTestService> logger)
    {
        _logger = logger;
        _logger.LogDebug("[HitTest] HitTestService initialized (v0.3.23c)");
    }

    /// <inheritdoc/>
    public HitTestResult HitTest(int screenX, int screenY, GamePhase currentPhase)
    {
        // Search regions in reverse order (last registered = on top)
        for (int i = _regions.Count - 1; i >= 0; i--)
        {
            var region = _regions[i];
            if (screenX >= region.Left && screenX <= region.Right &&
                screenY >= region.Top && screenY <= region.Bottom)
            {
                _logger.LogTrace("[HitTest] Hit at ({X}, {Y}): {Type}",
                    screenX, screenY, region.Type);

                return new HitTestResult(
                    region.Type,
                    region.Index,
                    region.Row,
                    region.Column,
                    region.Data);
            }
        }

        _logger.LogTrace("[HitTest] Miss at ({X}, {Y})", screenX, screenY);
        return HitTestResult.None;
    }

    /// <inheritdoc/>
    public void RegisterRegion(
        HitTargetType type,
        int left, int top, int right, int bottom,
        int? index = null, int? row = null, int? column = null,
        object? data = null)
    {
        _regions.Add(new ClickableRegion(type, left, top, right, bottom, index, row, column, data));
        _logger.LogTrace("[HitTest] Registered region: {Type} at ({Left},{Top})-({Right},{Bottom})",
            type, left, top, right, bottom);
    }

    /// <inheritdoc/>
    public void ClearRegions()
    {
        var count = _regions.Count;
        _regions.Clear();
        if (count > 0)
        {
            _logger.LogTrace("[HitTest] Cleared {Count} regions", count);
        }
    }
}
