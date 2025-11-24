using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Skia;
using RuneAndRust.Core;
using RuneAndRust.DesktopUI.Services;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;

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

    public static readonly StyledProperty<Dictionary<GridPosition, Combatant>?> UnitDataProperty =
        AvaloniaProperty.Register<CombatGridControl, Dictionary<GridPosition, Combatant>?>(nameof(UnitData));

    public static readonly StyledProperty<IStatusEffectIconService?> StatusEffectIconServiceProperty =
        AvaloniaProperty.Register<CombatGridControl, IStatusEffectIconService?>(nameof(StatusEffectIconService));

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

    public Dictionary<GridPosition, Combatant>? UnitData
    {
        get => GetValue(UnitDataProperty);
        set => SetValue(UnitDataProperty, value);
    }

    public IStatusEffectIconService? StatusEffectIconService
    {
        get => GetValue(StatusEffectIconServiceProperty);
        set => SetValue(StatusEffectIconServiceProperty, value);
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
            UnitSpritesProperty,
            UnitDataProperty,
            StatusEffectIconServiceProperty);

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

            // Draw HP bar and status effects if unit data is available
            if (UnitData != null && UnitData.TryGetValue(pos, out var unit))
            {
                DrawHPBar(canvas, cellRect, unit);
                DrawStatusEffects(canvas, cellRect, unit);
            }
        }
    }

    private void DrawHPBar(SKCanvas canvas, SKRect cellRect, Combatant unit)
    {
        const float hpBarHeight = 6.0f;
        const float hpBarWidth = 48.0f;
        const float hpBarYOffset = 8.0f; // Above the unit sprite

        // Calculate HP bar position (centered horizontally, above sprite)
        var barX = cellRect.Left + (cellRect.Width - hpBarWidth) / 2;
        var barY = cellRect.Top + hpBarYOffset;

        // HP percentage
        var hpPercent = unit.Character.CurrentHP / (float)unit.Character.MaxHP;
        var fillWidth = hpBarWidth * hpPercent;

        // Background (black border)
        using var borderPaint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            Color = SKColors.Black,
            StrokeWidth = 1.0f,
            IsAntialias = true
        };
        canvas.DrawRect(barX, barY, hpBarWidth, hpBarHeight, borderPaint);

        // HP bar fill color based on percentage
        var fillColor = hpPercent switch
        {
            > 0.6f => new SKColor(50, 205, 50),   // Green (healthy)
            > 0.3f => new SKColor(255, 165, 0),   // Orange (wounded)
            _ => new SKColor(220, 20, 60)         // Crimson (critical)
        };

        // Fill
        using var fillPaint = new SKPaint
        {
            Style = SKPaintStyle.Fill,
            Color = fillColor,
            IsAntialias = true
        };
        canvas.DrawRect(barX, barY, fillWidth, hpBarHeight, fillPaint);

        // HP text (small numbers)
        using var textPaint = new SKPaint
        {
            Color = SKColors.White,
            TextSize = 9.0f,
            IsAntialias = true,
            Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyle.Bold),
            TextAlign = SKTextAlign.Center
        };

        var hpText = $"{unit.Character.CurrentHP}/{unit.Character.MaxHP}";
        var textX = barX + hpBarWidth / 2;
        var textY = barY + hpBarHeight + 10.0f; // Below the bar

        // Text shadow for readability
        using var shadowPaint = new SKPaint
        {
            Color = SKColors.Black,
            TextSize = 9.0f,
            IsAntialias = true,
            Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyle.Bold),
            TextAlign = SKTextAlign.Center
        };
        canvas.DrawText(hpText, textX + 1, textY + 1, shadowPaint);
        canvas.DrawText(hpText, textX, textY, textPaint);
    }

    private void DrawStatusEffects(SKCanvas canvas, SKRect cellRect, Combatant unit)
    {
        if (StatusEffectIconService == null || unit.ActiveEffects == null || unit.ActiveEffects.Count == 0)
            return;

        const int iconSize = 16;
        const int iconSpacing = 2;
        const int maxVisibleIcons = 6;

        // Position icons in bottom-left corner of cell
        var startX = cellRect.Left + 4;
        var startY = cellRect.Bottom - iconSize - 4;

        var visibleEffects = unit.ActiveEffects.Take(maxVisibleIcons).ToList();
        var hasMoreEffects = unit.ActiveEffects.Count > maxVisibleIcons;

        for (int i = 0; i < visibleEffects.Count; i++)
        {
            var effect = visibleEffects[i];
            var icon = StatusEffectIconService.GetStatusEffectIcon(effect.EffectType);

            if (icon != null)
            {
                var iconX = startX + i * (iconSize + iconSpacing);
                var iconY = startY;

                // Draw icon
                var destRect = new SKRect(iconX, iconY, iconX + iconSize, iconY + iconSize);
                canvas.DrawBitmap(icon, destRect);

                // Draw duration number overlay
                if (effect.DurationRemaining > 0)
                {
                    using var durationPaint = new SKPaint
                    {
                        Color = SKColors.White,
                        TextSize = 10.0f,
                        IsAntialias = true,
                        Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyle.Bold),
                        TextAlign = SKTextAlign.Center
                    };

                    // Black shadow for readability
                    using var shadowPaint = new SKPaint
                    {
                        Color = SKColors.Black,
                        TextSize = 10.0f,
                        IsAntialias = true,
                        Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyle.Bold),
                        TextAlign = SKTextAlign.Center
                    };

                    var durationText = effect.DurationRemaining.ToString();
                    var textX = iconX + iconSize / 2.0f;
                    var textY = iconY + iconSize - 2.0f;

                    canvas.DrawText(durationText, textX + 0.5f, textY + 0.5f, shadowPaint);
                    canvas.DrawText(durationText, textX, textY, durationPaint);
                }

                // Draw category color border
                var categoryColor = StatusEffectIconService.GetCategoryColor(effect.Category);
                using var borderPaint = new SKPaint
                {
                    Style = SKPaintStyle.Stroke,
                    Color = categoryColor,
                    StrokeWidth = 1.5f,
                    IsAntialias = true
                };
                canvas.DrawRect(destRect, borderPaint);
            }
        }

        // Draw "+" indicator if more effects exist
        if (hasMoreEffects)
        {
            var moreIconX = startX + maxVisibleIcons * (iconSize + iconSpacing);
            var moreIconY = startY;

            // Background circle
            using var circlePaint = new SKPaint
            {
                Style = SKPaintStyle.Fill,
                Color = new SKColor(64, 64, 64, 200),
                IsAntialias = true
            };
            canvas.DrawCircle(moreIconX + iconSize / 2.0f, moreIconY + iconSize / 2.0f, iconSize / 2.0f, circlePaint);

            // "+" text
            using var plusPaint = new SKPaint
            {
                Color = SKColors.White,
                TextSize = 12.0f,
                IsAntialias = true,
                Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyle.Bold),
                TextAlign = SKTextAlign.Center
            };
            canvas.DrawText("+", moreIconX + iconSize / 2.0f, moreIconY + iconSize / 2.0f + 4.0f, plusPaint);
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
