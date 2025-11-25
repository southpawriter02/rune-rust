using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Rendering.SceneGraph;
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

    public static readonly StyledProperty<BattlefieldGrid?> GridProperty =
        AvaloniaProperty.Register<CombatGridControl, BattlefieldGrid?>(nameof(Grid));

    public static readonly StyledProperty<List<EnvironmentalObject>?> EnvironmentalObjectsProperty =
        AvaloniaProperty.Register<CombatGridControl, List<EnvironmentalObject>?>(nameof(EnvironmentalObjects));

    public static readonly StyledProperty<IHazardVisualizationService?> HazardVisualizationServiceProperty =
        AvaloniaProperty.Register<CombatGridControl, IHazardVisualizationService?>(nameof(HazardVisualizationService));

    public static readonly StyledProperty<IAnimationService?> AnimationServiceProperty =
        AvaloniaProperty.Register<CombatGridControl, IAnimationService?>(nameof(AnimationService));

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

    public BattlefieldGrid? Grid
    {
        get => GetValue(GridProperty);
        set => SetValue(GridProperty, value);
    }

    public List<EnvironmentalObject>? EnvironmentalObjects
    {
        get => GetValue(EnvironmentalObjectsProperty);
        set => SetValue(EnvironmentalObjectsProperty, value);
    }

    public IHazardVisualizationService? HazardVisualizationService
    {
        get => GetValue(HazardVisualizationServiceProperty);
        set => SetValue(HazardVisualizationServiceProperty, value);
    }

    public IAnimationService? AnimationService
    {
        get => GetValue(AnimationServiceProperty);
        set => SetValue(AnimationServiceProperty, value);
    }

    // Events
    public event EventHandler<GridPosition>? CellClicked;
    public event EventHandler<GridPosition>? CellHovered;

    // Visual layout constants
    private const double GridLineWidth = 2.0;
    private const double CenterLineWidth = 4.0;
    private const int SpriteScale = 3; // 16×16 sprites scaled 3x = 48×48

    // Animation state
    private float _animationTime = 0f;
    private System.Timers.Timer? _animationTimer;

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
            StatusEffectIconServiceProperty,
            GridProperty,
            EnvironmentalObjectsProperty,
            HazardVisualizationServiceProperty,
            AnimationServiceProperty);

        AffectsMeasure<CombatGridControl>(
            ColumnsProperty,
            CellSizeProperty);
    }

    public CombatGridControl()
    {
        ClipToBounds = false;

        // Setup 60 FPS animation timer
        _animationTimer = new System.Timers.Timer(16.67); // ~60 FPS
        _animationTimer.Elapsed += (s, e) =>
        {
            AnimationService?.Update();
            Avalonia.Threading.Dispatcher.UIThread.Post(InvalidateVisual);
        };
        _animationTimer.Start();
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

        // Use custom draw operation for SkiaSharp rendering
        var bounds = new Rect(0, 0, Bounds.Width, Bounds.Height);
        context.Custom(new CombatGridDrawOperation(this, bounds));
    }

    // Custom draw operation for SkiaSharp rendering
    private class CombatGridDrawOperation : ICustomDrawOperation
    {
        private readonly CombatGridControl _control;
        private readonly Rect _bounds;

        public CombatGridDrawOperation(CombatGridControl control, Rect bounds)
        {
            _control = control;
            _bounds = bounds;
        }

        public Rect Bounds => _bounds;

        public void Dispose() { }

        public bool Equals(ICustomDrawOperation? other) => false;

        public bool HitTest(Point p) => _bounds.Contains(p);

        public void Render(ImmediateDrawingContext context)
        {
            var leaseFeature = context.TryGetFeature<ISkiaSharpApiLeaseFeature>();
            if (leaseFeature == null) return;

            using var lease = leaseFeature.Lease();
            var canvas = lease.SkCanvas;

            canvas.Clear(SKColors.Transparent);

            // Render grid elements (layered from back to front)
            _control.RenderGridBackground(canvas);
            _control.RenderTileTypes(canvas);           // v0.43.7: Elevation tints, terrain types
            _control.RenderCover(canvas);                // v0.43.7: Cover indicators
            _control.RenderHazards(canvas);              // v0.43.7: Environmental hazards
            _control.RenderGridLines(canvas);
            _control.RenderCenterLine(canvas);
            _control.RenderCellHighlights(canvas);
            _control.RenderUnitSprites(canvas);
            _control.RenderActiveAnimations(canvas);     // v0.43.8: Combat animations

            // Increment animation time for animated hazards
            _control._animationTime += 0.05f;
            if (_control._animationTime > Math.PI * 2) _control._animationTime = 0f;
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

    private void RenderTileTypes(SKCanvas canvas)
    {
        if (Grid == null || HazardVisualizationService == null)
            return;

        using var paint = new SKPaint { IsAntialias = true };

        foreach (var (pos, tile) in Grid.Tiles)
        {
            var cellRect = GetCellRect(pos);

            // Draw tile type tint
            if (tile.Type != TileType.Normal)
            {
                paint.Color = HazardVisualizationService.GetTileTypeColor(tile.Type);
                if (paint.Color != SKColors.Transparent)
                {
                    canvas.DrawRect(cellRect, paint);
                }
            }

            // Draw elevation indicator in top-right corner
            if (pos.Elevation != 0)
            {
                using var textPaint = new SKPaint
                {
                    Color = pos.Elevation > 0 ? new SKColor(255, 215, 0) : new SKColor(0, 191, 255),
                    TextSize = 14,
                    IsAntialias = true,
                    TextAlign = SKTextAlign.Right,
                    Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyle.Bold)
                };

                var elevText = pos.Elevation > 0 ? $"+{pos.Elevation}" : pos.Elevation.ToString();
                canvas.DrawText(elevText, cellRect.Right - 5, cellRect.Top + 20, textPaint);
            }
        }
    }

    private void RenderCover(SKCanvas canvas)
    {
        if (Grid == null || HazardVisualizationService == null)
            return;

        foreach (var (pos, tile) in Grid.Tiles)
        {
            if (tile.Cover == CoverType.None)
                continue;

            var cellRect = GetCellRect(pos);
            var coverIcon = HazardVisualizationService.GetCoverIcon(tile.Cover);
            var coverColor = HazardVisualizationService.GetCoverColor(tile.Cover);

            // Draw cover icon in top-left corner
            using var iconPaint = new SKPaint
            {
                Color = coverColor,
                TextSize = 24,
                IsAntialias = true,
                TextAlign = SKTextAlign.Left,
                Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyle.Bold)
            };

            canvas.DrawText(coverIcon, cellRect.Left + 5, cellRect.Top + 25, iconPaint);

            // Draw cover health if available
            if (tile.CoverHealth.HasValue)
            {
                using var healthPaint = new SKPaint
                {
                    Color = SKColors.White,
                    TextSize = 10,
                    IsAntialias = true,
                    TextAlign = SKTextAlign.Left,
                    Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyle.Bold)
                };

                // Black shadow
                using var shadowPaint = new SKPaint
                {
                    Color = SKColors.Black,
                    TextSize = 10,
                    IsAntialias = true,
                    TextAlign = SKTextAlign.Left,
                    Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyle.Bold)
                };

                canvas.DrawText($"HP:{tile.CoverHealth}", cellRect.Left + 6, cellRect.Bottom - 4, shadowPaint);
                canvas.DrawText($"HP:{tile.CoverHealth}", cellRect.Left + 5, cellRect.Bottom - 5, healthPaint);
            }
        }
    }

    private void RenderHazards(SKCanvas canvas)
    {
        if (Grid == null || EnvironmentalObjects == null || HazardVisualizationService == null)
            return;

        foreach (var (pos, tile) in Grid.Tiles)
        {
            // Get hazards at this position
            var hazardsAtTile = EnvironmentalObjects
                .Where(obj => obj.IsHazard &&
                             tile.EnvironmentalObjectIds.Contains(obj.ObjectId))
                .ToList();

            if (!hazardsAtTile.Any())
                continue;

            var cellRect = GetCellRect(pos);

            // Draw first hazard (most prominent)
            var hazard = hazardsAtTile.First();

            // Draw hazard overlay tint
            var overlayColor = HazardVisualizationService.GetHazardOverlayColor(hazard.DamageType ?? "");

            // Animate if appropriate
            if (HazardVisualizationService.ShouldAnimateHazard(hazard.DamageType ?? ""))
            {
                var pulse = (float)Math.Sin(_animationTime) * 0.3f + 0.7f;
                overlayColor = overlayColor.WithAlpha((byte)(overlayColor.Alpha * pulse));
            }

            using var overlayPaint = new SKPaint { Color = overlayColor, IsAntialias = true };
            canvas.DrawRect(cellRect, overlayPaint);

            // Draw hazard sprite
            var hazardSprite = HazardVisualizationService.GetHazardSprite(hazard.DamageType ?? "");
            if (hazardSprite != null)
            {
                var spriteX = cellRect.Left + (cellRect.Width - 48) / 2;
                var spriteY = cellRect.Top + (cellRect.Height - 48) / 2;
                canvas.DrawBitmap(hazardSprite, spriteX, spriteY);
            }

            // Draw hazard count if multiple
            if (hazardsAtTile.Count > 1)
            {
                using var countPaint = new SKPaint
                {
                    Color = SKColors.White,
                    TextSize = 12,
                    IsAntialias = true,
                    TextAlign = SKTextAlign.Right,
                    Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyle.Bold)
                };

                // Black shadow
                using var shadowPaint = new SKPaint
                {
                    Color = SKColors.Black,
                    TextSize = 12,
                    IsAntialias = true,
                    TextAlign = SKTextAlign.Right,
                    Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyle.Bold)
                };

                canvas.DrawText($"×{hazardsAtTile.Count}", cellRect.Right - 4, cellRect.Bottom - 4, shadowPaint);
                canvas.DrawText($"×{hazardsAtTile.Count}", cellRect.Right - 5, cellRect.Bottom - 5, countPaint);
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
        var hpPercent = unit.CurrentHP / (float)unit.MaxHP;
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

        var hpText = $"{unit.CurrentHP}/{unit.MaxHP}";
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

    private void RenderActiveAnimations(SKCanvas canvas)
    {
        if (AnimationService == null)
            return;

        foreach (var animation in AnimationService.GetActiveAnimations())
        {
            switch (animation)
            {
                case ProjectileAnimation proj:
                    RenderProjectile(canvas, proj);
                    break;
                case DamageNumberAnimation dmg:
                    RenderDamageNumber(canvas, dmg);
                    break;
                case ParticleAnimation particle:
                    RenderParticles(canvas, particle);
                    break;
                case TextAnimation text:
                    RenderTextAnimation(canvas, text);
                    break;
                case FlashAnimation flash:
                    RenderFlash(canvas, flash);
                    break;
                case StatusEffectAnimation status:
                    RenderStatusEffectAnimation(canvas, status);
                    break;
            }
        }
    }

    private void RenderProjectile(SKCanvas canvas, ProjectileAnimation anim)
    {
        var startRect = GetCellRect(anim.StartPosition);
        var endRect = GetCellRect(anim.EndPosition);

        var startX = startRect.Left + startRect.Width / 2;
        var startY = startRect.Top + startRect.Height / 2;
        var endX = endRect.Left + endRect.Width / 2;
        var endY = endRect.Top + endRect.Height / 2;

        var currentX = startX + (endX - startX) * anim.Progress;
        var currentY = startY + (endY - startY) * anim.Progress;

        using var paint = new SKPaint
        {
            Color = anim.ProjectileColor,
            IsAntialias = true
        };

        canvas.DrawCircle(currentX, currentY, 5, paint);
    }

    private void RenderDamageNumber(SKCanvas canvas, DamageNumberAnimation anim)
    {
        var cellRect = GetCellRect(anim.Position);
        var x = cellRect.Left + cellRect.Width / 2;
        var y = cellRect.Top + cellRect.Height / 2;

        // Float upward
        y -= 30 * anim.Progress;

        // Fade out
        var alpha = (byte)(255 * (1 - anim.Progress));
        var color = anim.Color.WithAlpha(alpha);

        using var paint = new SKPaint
        {
            Color = color,
            TextSize = anim.IsHealing ? 24 : 20,
            IsAntialias = true,
            TextAlign = SKTextAlign.Center,
            Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyle.Bold)
        };

        var text = anim.IsHealing ? $"+{anim.Damage}" : anim.Damage.ToString();

        // Black shadow for readability
        using var shadowPaint = new SKPaint
        {
            Color = SKColors.Black.WithAlpha(alpha),
            TextSize = paint.TextSize,
            IsAntialias = true,
            TextAlign = SKTextAlign.Center,
            Typeface = paint.Typeface
        };

        canvas.DrawText(text, x + 1, y + 1, shadowPaint);
        canvas.DrawText(text, x, y, paint);
    }

    private void RenderParticles(SKCanvas canvas, ParticleAnimation anim)
    {
        var cellRect = GetCellRect(anim.Position);
        var centerX = cellRect.Left + cellRect.Width / 2;
        var centerY = cellRect.Top + cellRect.Height / 2;

        for (int i = 0; i < anim.ParticleCount; i++)
        {
            var angle = (float)(i * 2 * Math.PI / anim.ParticleCount);
            var distance = 30 * anim.Progress;

            float x, y;
            if (anim.Direction == ParticleDirection.Up)
            {
                // Particles rise upward and spread slightly
                x = centerX + (float)Math.Cos(angle) * distance * 0.3f;
                y = centerY - 40 * anim.Progress;
            }
            else if (anim.Direction == ParticleDirection.Radial)
            {
                // Particles spread radially
                x = centerX + (float)Math.Cos(angle) * distance;
                y = centerY + (float)Math.Sin(angle) * distance;
            }
            else // Down
            {
                x = centerX + (float)Math.Cos(angle) * distance * 0.3f;
                y = centerY + 40 * anim.Progress;
            }

            var alpha = (byte)(255 * (1 - anim.Progress));
            using var paint = new SKPaint
            {
                Color = anim.Color.WithAlpha(alpha),
                IsAntialias = true
            };

            canvas.DrawCircle(x, y, 3, paint);
        }
    }

    private void RenderTextAnimation(SKCanvas canvas, TextAnimation anim)
    {
        var cellRect = GetCellRect(anim.Position);
        var x = cellRect.Left + cellRect.Width / 2;
        var y = cellRect.Top + cellRect.Height / 2 - 40;

        // Scale up then fade
        var scale = 0.5f + anim.Progress * 0.5f * anim.Scale;
        var alpha = (byte)(255 * (1 - anim.Progress));

        using var paint = new SKPaint
        {
            Color = anim.Color.WithAlpha(alpha),
            TextSize = 18 * scale,
            IsAntialias = true,
            TextAlign = SKTextAlign.Center,
            Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyle.Bold)
        };

        // Black shadow
        using var shadowPaint = new SKPaint
        {
            Color = SKColors.Black.WithAlpha(alpha),
            TextSize = paint.TextSize,
            IsAntialias = true,
            TextAlign = SKTextAlign.Center,
            Typeface = paint.Typeface
        };

        canvas.DrawText(anim.Text, x + 2, y + 2, shadowPaint);
        canvas.DrawText(anim.Text, x, y, paint);
    }

    private void RenderFlash(SKCanvas canvas, FlashAnimation anim)
    {
        var cellRect = GetCellRect(anim.Position);

        var alpha = (byte)(150 * (1 - anim.Progress));
        using var paint = new SKPaint
        {
            Color = anim.Color.WithAlpha(alpha),
            IsAntialias = true
        };

        canvas.DrawRect(cellRect, paint);
    }

    private void RenderStatusEffectAnimation(SKCanvas canvas, StatusEffectAnimation anim)
    {
        var cellRect = GetCellRect(anim.Position);
        var centerX = cellRect.Left + cellRect.Width / 2;
        var centerY = cellRect.Top + cellRect.Height / 2;

        // Expanding ring effect
        var radius = 20 * anim.Progress;
        var alpha = (byte)(200 * (1 - anim.Progress));

        using var paint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            Color = anim.Color.WithAlpha(alpha),
            StrokeWidth = 3,
            IsAntialias = true
        };

        canvas.DrawCircle(centerX, centerY, radius, paint);

        // Inner glow
        var glowAlpha = (byte)(100 * (1 - anim.Progress));
        using var glowPaint = new SKPaint
        {
            Style = SKPaintStyle.Fill,
            Color = anim.Color.WithAlpha(glowAlpha),
            IsAntialias = true
        };

        canvas.DrawCircle(centerX, centerY, radius * 0.5f, glowPaint);
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
