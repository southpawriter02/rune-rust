using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Skia;
using RuneAndRust.Core;
using SkiaSharp;
using System;
using System.Collections.Generic;

namespace RuneAndRust.DesktopUI.Controls;

/// <summary>
/// Custom Avalonia control that renders the tactical combat grid.
/// Maps BattlefieldGrid's Zone/Row/Column structure to visual coordinates.
/// </summary>
public class CombatGridControl : Control
{
    // Styled properties
    public static readonly StyledProperty<int> ColumnsProperty =
        AvaloniaProperty.Register<CombatGridControl, int>(nameof(Columns), defaultValue: 3);

    public static readonly StyledProperty<double> CellSizeProperty =
        AvaloniaProperty.Register<CombatGridControl, double>(nameof(CellSize), defaultValue: 80.0);

    public static readonly StyledProperty<GridPosition?> SelectedPositionProperty =
        AvaloniaProperty.Register<CombatGridControl, GridPosition?>(nameof(SelectedPosition));

    public static readonly StyledProperty<GridPosition?> HoveredPositionProperty =
        AvaloniaProperty.Register<CombatGridControl, GridPosition?>(nameof(HoveredPosition));

    public static readonly StyledProperty<IReadOnlyCollection<GridPosition>?> HighlightedPositionsProperty =
        AvaloniaProperty.Register<CombatGridControl, IReadOnlyCollection<GridPosition>?>(nameof(HighlightedPositions));

    public static readonly StyledProperty<Dictionary<GridPosition, SKBitmap>?> UnitSpritesProperty =
        AvaloniaProperty.Register<CombatGridControl, Dictionary<GridPosition, SKBitmap>?>(nameof(UnitSprites));

    // Properties
    public int Columns
    {
        get => GetValue(ColumnsProperty);
        set => SetValue(ColumnsProperty, value);
    }

    public double CellSize
    {
        get => GetValue(CellSizeProperty);
        set => SetValue(CellSizeProperty, value);
    }

    public GridPosition? SelectedPosition
    {
        get => GetValue(SelectedPositionProperty);
        set => SetValue(SelectedPositionProperty, value);
    }

    public GridPosition? HoveredPosition
    {
        get => GetValue(HoveredPositionProperty);
        set => SetValue(HoveredPositionProperty, value);
    }

    public IReadOnlyCollection<GridPosition>? HighlightedPositions
    {
        get => GetValue(HighlightedPositionsProperty);
        set => SetValue(HighlightedPositionsProperty, value);
    }

    public Dictionary<GridPosition, SKBitmap>? UnitSprites
    {
        get => GetValue(UnitSpritesProperty);
        set => SetValue(UnitSpritesProperty, value);
    }

    // Events
    public event EventHandler<GridPosition>? CellClicked;
    public event EventHandler<GridPosition>? CellHovered;

    // Visual layout constants
    private const double GridLineWidth = 2.0;
    private const double CenterLineWidth = 4.0;
    private const int SpriteScale = 3; // 16×16 sprites scaled 3x = 48×48

    static CombatGridControl()
    {
        AffectsRender<CombatGridControl>(
            ColumnsProperty,
            CellSizeProperty,
            SelectedPositionProperty,
            HoveredPositionProperty,
            HighlightedPositionsProperty,
            UnitSpritesProperty);

        AffectsMeasure<CombatGridControl>(
            ColumnsProperty,
            CellSizeProperty);
    }

    public CombatGridControl()
    {
        ClipToBounds = false;
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        // Grid layout: 4 rows (2 zones × 2 rows) × N columns
        var width = Columns * CellSize;
        var height = 4 * CellSize; // Enemy Back, Enemy Front, Player Front, Player Back
        return new Size(width, height);
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        if (context.PlatformImpl.GetFeature<ISkiaSharpApiLeaseFeature>() is { } skia)
        {
            using var lease = skia.Lease();
            var canvas = lease.SkCanvas;

            canvas.Clear(SKColors.Transparent);

            // Render grid elements
            RenderGridBackground(canvas);
            RenderGridLines(canvas);
            RenderCenterLine(canvas);
            RenderCellHighlights(canvas);
            RenderUnitSprites(canvas);
        }
    }

    private void RenderGridBackground(SKCanvas canvas)
    {
        var paint = new SKPaint
        {
            Style = SKPaintStyle.Fill,
            IsAntialias = true
        };

        // Render each cell with appropriate background color
        for (int col = 0; col < Columns; col++)
        {
            foreach (Zone zone in Enum.GetValues(typeof(Zone)))
            {
                foreach (Row row in Enum.GetValues(typeof(Row)))
                {
                    var pos = new GridPosition(zone, row, col);
                    var rect = GetCellRect(pos);

                    // Determine cell color
                    var color = GetCellBackgroundColor(pos);
                    paint.Color = color;
                    canvas.DrawRect(rect, paint);
                }
            }
        }

        paint.Dispose();
    }

    private void RenderGridLines(SKCanvas canvas)
    {
        var paint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            Color = new SKColor(100, 100, 100, 180),
            StrokeWidth = (float)GridLineWidth,
            IsAntialias = true
        };

        var width = Columns * CellSize;
        var height = 4 * CellSize;

        // Vertical lines (columns)
        for (int col = 0; col <= Columns; col++)
        {
            var x = col * CellSize;
            canvas.DrawLine((float)x, 0, (float)x, (float)height, paint);
        }

        // Horizontal lines (rows)
        for (int row = 0; row <= 4; row++)
        {
            if (row == 2) continue; // Skip center line (drawn separately)
            var y = row * CellSize;
            canvas.DrawLine(0, (float)y, (float)width, (float)y, paint);
        }

        paint.Dispose();
    }

    private void RenderCenterLine(SKCanvas canvas)
    {
        var paint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            Color = new SKColor(200, 200, 0, 220),
            StrokeWidth = (float)CenterLineWidth,
            IsAntialias = true
        };

        var width = Columns * CellSize;
        var y = 2 * CellSize; // Between Player and Enemy zones

        canvas.DrawLine(0, (float)y, (float)width, (float)y, paint);
        paint.Dispose();
    }

    private void RenderCellHighlights(SKCanvas canvas)
    {
        var paint = new SKPaint
        {
            Style = SKPaintStyle.Fill,
            IsAntialias = true
        };

        // Render highlighted positions (e.g., valid movement/attack targets)
        if (HighlightedPositions != null)
        {
            paint.Color = new SKColor(100, 200, 255, 60);
            foreach (var pos in HighlightedPositions)
            {
                var rect = GetCellRect(pos);
                canvas.DrawRect(rect, paint);
            }
        }

        // Render hovered position
        if (HoveredPosition.HasValue)
        {
            paint.Color = new SKColor(255, 255, 100, 80);
            var rect = GetCellRect(HoveredPosition.Value);
            canvas.DrawRect(rect, paint);
        }

        // Render selected position
        if (SelectedPosition.HasValue)
        {
            paint.Color = new SKColor(100, 255, 100, 100);
            var rect = GetCellRect(SelectedPosition.Value);
            canvas.DrawRect(rect, paint);

            // Draw selection border
            paint.Style = SKPaintStyle.Stroke;
            paint.Color = new SKColor(0, 255, 0, 255);
            paint.StrokeWidth = 3.0f;
            canvas.DrawRect(rect, paint);
        }

        paint.Dispose();
    }

    private void RenderUnitSprites(SKCanvas canvas)
    {
        if (UnitSprites == null || UnitSprites.Count == 0)
            return;

        foreach (var (pos, sprite) in UnitSprites)
        {
            var cellRect = GetCellRect(pos);

            // Center sprite in cell
            var spriteWidth = sprite.Width;
            var spriteHeight = sprite.Height;
            var x = cellRect.Left + (cellRect.Width - spriteWidth) / 2;
            var y = cellRect.Top + (cellRect.Height - spriteHeight) / 2;

            canvas.DrawBitmap(sprite, x, y);
        }
    }

    private SKRect GetCellRect(GridPosition pos)
    {
        // Map Zone/Row/Column to visual Y coordinate
        // Layout (from top to bottom):
        // Row 0: Enemy Back
        // Row 1: Enemy Front
        // --- CENTER LINE ---
        // Row 2: Player Front
        // Row 3: Player Back

        int visualRow = pos.Zone switch
        {
            Zone.Enemy => pos.Row == Row.Back ? 0 : 1,
            Zone.Player => pos.Row == Row.Front ? 2 : 3,
            _ => 0
        };

        var x = pos.Column * CellSize;
        var y = visualRow * CellSize;

        return new SKRect(
            (float)x,
            (float)y,
            (float)(x + CellSize),
            (float)(y + CellSize));
    }

    private SKColor GetCellBackgroundColor(GridPosition pos)
    {
        // Alternating checkerboard pattern
        bool isDark = (pos.Column + (int)pos.Zone + (int)pos.Row) % 2 == 0;

        // Zone-based tint
        if (pos.Zone == Zone.Enemy)
        {
            return isDark ? new SKColor(60, 40, 40) : new SKColor(80, 50, 50);
        }
        else
        {
            return isDark ? new SKColor(40, 50, 60) : new SKColor(50, 65, 80);
        }
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);

        var point = e.GetPosition(this);
        var gridPos = GetGridPositionFromPoint(point);

        if (gridPos.HasValue && gridPos != HoveredPosition)
        {
            HoveredPosition = gridPos;
            CellHovered?.Invoke(this, gridPos.Value);
            InvalidateVisual();
        }
        else if (!gridPos.HasValue && HoveredPosition.HasValue)
        {
            HoveredPosition = null;
            InvalidateVisual();
        }
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);

        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            var point = e.GetPosition(this);
            var gridPos = GetGridPositionFromPoint(point);

            if (gridPos.HasValue)
            {
                SelectedPosition = gridPos;
                CellClicked?.Invoke(this, gridPos.Value);
                InvalidateVisual();
            }
        }
    }

    protected override void OnPointerExited(PointerEventArgs e)
    {
        base.OnPointerExited(e);

        if (HoveredPosition.HasValue)
        {
            HoveredPosition = null;
            InvalidateVisual();
        }
    }

    private GridPosition? GetGridPositionFromPoint(Point point)
    {
        // Convert screen coordinates to grid position
        var col = (int)(point.X / CellSize);
        var visualRow = (int)(point.Y / CellSize);

        // Bounds check
        if (col < 0 || col >= Columns || visualRow < 0 || visualRow >= 4)
            return null;

        // Map visual row back to Zone/Row
        return visualRow switch
        {
            0 => new GridPosition(Zone.Enemy, Row.Back, col),
            1 => new GridPosition(Zone.Enemy, Row.Front, col),
            2 => new GridPosition(Zone.Player, Row.Front, col),
            3 => new GridPosition(Zone.Player, Row.Back, col),
            _ => null
        };
    }
}
