using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;
using RuneAndRust.DesktopUI.ViewModels;
using SkiaSharp;
using System;

namespace RuneAndRust.DesktopUI.Controls;

/// <summary>
/// Custom control for rendering the dungeon minimap using SkiaSharp.
/// Displays explored rooms, connections, fog-of-war, and vertical layer indicators.
/// </summary>
public class MinimapControl : Control
{
    // Styled properties
    public static readonly StyledProperty<MinimapViewModel?> ViewModelProperty =
        AvaloniaProperty.Register<MinimapControl, MinimapViewModel?>(nameof(ViewModel));

    public static readonly StyledProperty<double> RoomSizeProperty =
        AvaloniaProperty.Register<MinimapControl, double>(nameof(RoomSize), 40.0);

    /// <summary>
    /// The minimap view model containing room and connection data.
    /// </summary>
    public MinimapViewModel? ViewModel
    {
        get => GetValue(ViewModelProperty);
        set => SetValue(ViewModelProperty, value);
    }

    /// <summary>
    /// Size of room squares on the minimap.
    /// </summary>
    public double RoomSize
    {
        get => GetValue(RoomSizeProperty);
        set => SetValue(RoomSizeProperty, value);
    }

    // Panning state
    private bool _isPanning;
    private Point _lastPanPosition;

    public MinimapControl()
    {
        ClipToBounds = true;
    }

    static MinimapControl()
    {
        AffectsRender<MinimapControl>(ViewModelProperty, RoomSizeProperty);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == ViewModelProperty)
        {
            // Subscribe to ViewModel changes
            if (change.OldValue is MinimapViewModel oldVm)
            {
                oldVm.PropertyChanged -= OnViewModelPropertyChanged;
            }
            if (change.NewValue is MinimapViewModel newVm)
            {
                newVm.PropertyChanged += OnViewModelPropertyChanged;
            }
            InvalidateVisual();
        }
    }

    private void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        InvalidateVisual();
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        if (ViewModel == null) return;

        // Use custom drawing operation for SkiaSharp
        var bounds = new Rect(0, 0, Bounds.Width, Bounds.Height);
        context.Custom(new MinimapDrawOperation(bounds, ViewModel, RoomSize));
    }

    #region Mouse Handling for Pan

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);

        if (e.GetCurrentPoint(this).Properties.IsMiddleButtonPressed ||
            (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed && e.KeyModifiers.HasFlag(KeyModifiers.Control)))
        {
            _isPanning = true;
            _lastPanPosition = e.GetPosition(this);
            e.Handled = true;
        }
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);

        if (_isPanning && ViewModel != null)
        {
            var currentPos = e.GetPosition(this);
            var delta = currentPos - _lastPanPosition;
            ViewModel.PanOffset = new Point(
                ViewModel.PanOffset.X + delta.X,
                ViewModel.PanOffset.Y + delta.Y);
            _lastPanPosition = currentPos;
            e.Handled = true;
        }
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);

        if (_isPanning)
        {
            _isPanning = false;
            e.Handled = true;
        }
    }

    protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
    {
        base.OnPointerWheelChanged(e);

        if (ViewModel != null)
        {
            if (e.Delta.Y > 0)
            {
                ViewModel.ZoomLevel += 0.1f;
            }
            else if (e.Delta.Y < 0)
            {
                ViewModel.ZoomLevel -= 0.1f;
            }
            e.Handled = true;
        }
    }

    #endregion

    /// <summary>
    /// Custom drawing operation for SkiaSharp rendering.
    /// </summary>
    private class MinimapDrawOperation : ICustomDrawOperation
    {
        private readonly Rect _bounds;
        private readonly MinimapViewModel _viewModel;
        private readonly double _roomSize;

        // Colors
        private static readonly SKColor BackgroundColor = new SKColor(0x1C, 0x1C, 0x1C);
        private static readonly SKColor GridColor = new SKColor(0x2C, 0x2C, 0x2C);
        private static readonly SKColor ConnectionColor = new SKColor(0x88, 0x88, 0x88);
        private static readonly SKColor CurrentRoomBorderColor = new SKColor(0xFF, 0xD7, 0x00); // Gold
        private static readonly SKColor FogColor = new SKColor(0x33, 0x33, 0x33, 0x80);
        private static readonly SKColor UpConnectionColor = new SKColor(0xFF, 0xFF, 0x00); // Yellow
        private static readonly SKColor DownConnectionColor = new SKColor(0x00, 0xFF, 0xFF); // Cyan

        // Room type colors
        private static readonly SKColor StartRoomColor = new SKColor(0x4C, 0xAF, 0x50); // Green
        private static readonly SKColor BossRoomColor = new SKColor(0x8B, 0x00, 0x00); // Dark red
        private static readonly SKColor SanctuaryColor = new SKColor(0x2E, 0x4A, 0x2E); // Dark green
        private static readonly SKColor CombatRoomColor = new SKColor(0xFF, 0x6B, 0x6B); // Light red
        private static readonly SKColor PuzzleRoomColor = new SKColor(0x9C, 0x27, 0xB0); // Purple

        public MinimapDrawOperation(Rect bounds, MinimapViewModel viewModel, double roomSize)
        {
            _bounds = bounds;
            _viewModel = viewModel;
            _roomSize = roomSize;
        }

        public Rect Bounds => _bounds;

        public void Dispose() { }

        public bool Equals(ICustomDrawOperation? other)
        {
            return other is MinimapDrawOperation op &&
                   op._bounds == _bounds &&
                   op._viewModel == _viewModel;
        }

        public bool HitTest(Point p) => _bounds.Contains(p);

        public void Render(ImmediateDrawingContext context)
        {
            var leaseFeature = context.TryGetFeature<ISkiaSharpApiLeaseFeature>();
            if (leaseFeature == null) return;

            using var lease = leaseFeature.Lease();
            var canvas = lease.SkCanvas;

            // Clear background
            canvas.Clear(BackgroundColor);

            // Draw subtle grid
            DrawGrid(canvas);

            // Apply zoom and pan transformations
            canvas.Save();
            canvas.Translate((float)_bounds.Width / 2, (float)_bounds.Height / 2);
            canvas.Scale(_viewModel.ZoomLevel, _viewModel.ZoomLevel);
            canvas.Translate((float)_viewModel.PanOffset.X - (float)_bounds.Width / 2,
                           (float)_viewModel.PanOffset.Y - (float)_bounds.Height / 2);

            // Draw connections first (under rooms)
            DrawConnections(canvas);

            // Draw rooms
            DrawRooms(canvas);

            canvas.Restore();

            // Draw compass indicator
            DrawCompass(canvas);
        }

        private void DrawGrid(SKCanvas canvas)
        {
            using var paint = new SKPaint
            {
                Color = GridColor,
                StrokeWidth = 1,
                IsStroke = true
            };

            var gridSpacing = 60f * _viewModel.ZoomLevel;
            var offsetX = (float)(_viewModel.PanOffset.X * _viewModel.ZoomLevel + _bounds.Width / 2) % gridSpacing;
            var offsetY = (float)(_viewModel.PanOffset.Y * _viewModel.ZoomLevel + _bounds.Height / 2) % gridSpacing;

            // Vertical lines
            for (float x = offsetX; x < _bounds.Width; x += gridSpacing)
            {
                canvas.DrawLine(x, 0, x, (float)_bounds.Height, paint);
            }

            // Horizontal lines
            for (float y = offsetY; y < _bounds.Height; y += gridSpacing)
            {
                canvas.DrawLine(0, y, (float)_bounds.Width, y, paint);
            }
        }

        private void DrawConnections(SKCanvas canvas)
        {
            using var paint = new SKPaint
            {
                Color = ConnectionColor,
                StrokeWidth = 3,
                IsStroke = true,
                IsAntialias = true,
                StrokeCap = SKStrokeCap.Round
            };

            foreach (var connection in _viewModel.Connections)
            {
                canvas.DrawLine(
                    (float)connection.StartPos.X,
                    (float)connection.StartPos.Y,
                    (float)connection.EndPos.X,
                    (float)connection.EndPos.Y,
                    paint);
            }
        }

        private void DrawRooms(SKCanvas canvas)
        {
            var roomSize = (float)_roomSize;

            foreach (var roomNode in _viewModel.RoomNodes)
            {
                var x = (float)roomNode.Position.X;
                var y = (float)roomNode.Position.Y;

                if (!roomNode.IsExplored)
                {
                    // Draw fog-of-war room (unexplored hint)
                    DrawFogRoom(canvas, x, y, roomSize);
                    continue;
                }

                // Get room color based on type
                var roomColor = GetRoomColor(roomNode);

                // Draw room fill
                using var fillPaint = new SKPaint
                {
                    Color = roomColor,
                    IsAntialias = true
                };
                canvas.DrawRoundRect(x, y, roomSize, roomSize, 4, 4, fillPaint);

                // Draw biome tint overlay
                if (!string.IsNullOrEmpty(roomNode.BiomeColor))
                {
                    var biomeColor = SKColor.Parse(roomNode.BiomeColor).WithAlpha(80);
                    using var biomePaint = new SKPaint
                    {
                        Color = biomeColor,
                        IsAntialias = true
                    };
                    canvas.DrawRoundRect(x + 2, y + 2, roomSize - 4, roomSize - 4, 2, 2, biomePaint);
                }

                // Draw border
                using var borderPaint = new SKPaint
                {
                    Color = roomNode.IsCurrentRoom ? CurrentRoomBorderColor : new SKColor(0x00, 0x00, 0x00),
                    StrokeWidth = roomNode.IsCurrentRoom ? 3 : 1,
                    IsStroke = true,
                    IsAntialias = true
                };
                canvas.DrawRoundRect(x, y, roomSize, roomSize, 4, 4, borderPaint);

                // Draw current room indicator (pulsing effect simulated with larger border)
                if (roomNode.IsCurrentRoom)
                {
                    using var glowPaint = new SKPaint
                    {
                        Color = CurrentRoomBorderColor.WithAlpha(100),
                        StrokeWidth = 6,
                        IsStroke = true,
                        IsAntialias = true
                    };
                    canvas.DrawRoundRect(x - 2, y - 2, roomSize + 4, roomSize + 4, 6, 6, glowPaint);
                }

                // Draw vertical connection indicators
                if (roomNode.HasUpConnection)
                {
                    using var upPaint = new SKPaint
                    {
                        Color = UpConnectionColor,
                        IsAntialias = true
                    };
                    // Up arrow triangle
                    var path = new SKPath();
                    path.MoveTo(x + roomSize / 2, y + 4);
                    path.LineTo(x + roomSize / 2 - 6, y + 12);
                    path.LineTo(x + roomSize / 2 + 6, y + 12);
                    path.Close();
                    canvas.DrawPath(path, upPaint);
                }

                if (roomNode.HasDownConnection)
                {
                    using var downPaint = new SKPaint
                    {
                        Color = DownConnectionColor,
                        IsAntialias = true
                    };
                    // Down arrow triangle
                    var path = new SKPath();
                    path.MoveTo(x + roomSize / 2, y + roomSize - 4);
                    path.LineTo(x + roomSize / 2 - 6, y + roomSize - 12);
                    path.LineTo(x + roomSize / 2 + 6, y + roomSize - 12);
                    path.Close();
                    canvas.DrawPath(path, downPaint);
                }

                // Draw room type icon
                DrawRoomTypeIcon(canvas, roomNode, x, y, roomSize);
            }
        }

        private void DrawFogRoom(SKCanvas canvas, float x, float y, float roomSize)
        {
            using var fogPaint = new SKPaint
            {
                Color = FogColor,
                IsAntialias = true
            };
            canvas.DrawRoundRect(x, y, roomSize, roomSize, 4, 4, fogPaint);

            // Draw question mark
            using var textPaint = new SKPaint
            {
                Color = new SKColor(0x66, 0x66, 0x66),
                TextSize = roomSize * 0.5f,
                IsAntialias = true,
                TextAlign = SKTextAlign.Center
            };
            canvas.DrawText("?", x + roomSize / 2, y + roomSize / 2 + roomSize * 0.15f, textPaint);
        }

        private void DrawRoomTypeIcon(SKCanvas canvas, RoomNodeViewModel roomNode, float x, float y, float roomSize)
        {
            var centerX = x + roomSize / 2;
            var centerY = y + roomSize / 2;

            using var iconPaint = new SKPaint
            {
                Color = SKColors.White,
                TextSize = roomSize * 0.4f,
                IsAntialias = true,
                TextAlign = SKTextAlign.Center,
                FakeBoldText = true
            };

            string icon = roomNode.RoomType switch
            {
                "Start" => "S",
                "Boss" => "B",
                "Sanctuary" => "+",
                "Combat" => "!",
                "Puzzle" => "?",
                _ => ""
            };

            if (!string.IsNullOrEmpty(icon))
            {
                canvas.DrawText(icon, centerX, centerY + roomSize * 0.12f, iconPaint);
            }
        }

        private static SKColor GetRoomColor(RoomNodeViewModel roomNode)
        {
            return roomNode.RoomType switch
            {
                "Start" => StartRoomColor,
                "Boss" => BossRoomColor,
                "Sanctuary" => SanctuaryColor,
                "Combat" => CombatRoomColor,
                "Puzzle" => PuzzleRoomColor,
                _ => new SKColor(0x4A, 0x4A, 0x4A) // Default gray
            };
        }

        private void DrawCompass(SKCanvas canvas)
        {
            var compassX = (float)_bounds.Width - 50;
            var compassY = 50;
            var compassSize = 30f;

            // Draw compass background
            using var bgPaint = new SKPaint
            {
                Color = new SKColor(0x2C, 0x2C, 0x2C, 0xCC),
                IsAntialias = true
            };
            canvas.DrawCircle(compassX, compassY, compassSize, bgPaint);

            // Draw compass border
            using var borderPaint = new SKPaint
            {
                Color = new SKColor(0x66, 0x66, 0x66),
                StrokeWidth = 2,
                IsStroke = true,
                IsAntialias = true
            };
            canvas.DrawCircle(compassX, compassY, compassSize, borderPaint);

            // Draw N indicator
            using var textPaint = new SKPaint
            {
                Color = new SKColor(0xFF, 0xD7, 0x00), // Gold
                TextSize = 14,
                IsAntialias = true,
                TextAlign = SKTextAlign.Center,
                FakeBoldText = true
            };
            canvas.DrawText("N", compassX, compassY - compassSize + 18, textPaint);

            // Draw other directions
            textPaint.Color = new SKColor(0xAA, 0xAA, 0xAA);
            textPaint.TextSize = 10;
            canvas.DrawText("S", compassX, compassY + compassSize - 4, textPaint);
            canvas.DrawText("W", compassX - compassSize + 10, compassY + 4, textPaint);
            canvas.DrawText("E", compassX + compassSize - 10, compassY + 4, textPaint);
        }
    }
}
