namespace RuneAndRust.DesktopUI.Styles;

/// <summary>
/// v0.43.21: Standardized UI constants for consistent sizing and spacing.
/// All UI components should use these values for a cohesive layout.
/// </summary>
public static class UIConstants
{
    #region Font Sizes

    /// <summary>
    /// Extra large heading (H1) - 24px
    /// </summary>
    public const double FontSizeH1 = 24;

    /// <summary>
    /// Large heading (H2) - 20px
    /// </summary>
    public const double FontSizeH2 = 20;

    /// <summary>
    /// Medium heading (H3) - 16px
    /// </summary>
    public const double FontSizeH3 = 16;

    /// <summary>
    /// Body text - 14px
    /// </summary>
    public const double FontSizeBody = 14;

    /// <summary>
    /// Small text - 12px
    /// </summary>
    public const double FontSizeSmall = 12;

    /// <summary>
    /// Tiny text (tooltips, labels) - 10px
    /// </summary>
    public const double FontSizeTiny = 10;

    /// <summary>
    /// Extra large display text - 32px
    /// </summary>
    public const double FontSizeDisplay = 32;

    #endregion

    #region Spacing

    /// <summary>
    /// Extra small spacing - 2px
    /// </summary>
    public const double SpacingXS = 2;

    /// <summary>
    /// Small spacing - 5px
    /// </summary>
    public const double SpacingS = 5;

    /// <summary>
    /// Medium spacing - 10px
    /// </summary>
    public const double SpacingM = 10;

    /// <summary>
    /// Large spacing - 15px
    /// </summary>
    public const double SpacingL = 15;

    /// <summary>
    /// Extra large spacing - 20px
    /// </summary>
    public const double SpacingXL = 20;

    /// <summary>
    /// Extra extra large spacing - 30px
    /// </summary>
    public const double SpacingXXL = 30;

    #endregion

    #region Margins

    /// <summary>
    /// Small margin - 10px
    /// </summary>
    public const double MarginSmall = 10;

    /// <summary>
    /// Medium margin - 15px
    /// </summary>
    public const double MarginMedium = 15;

    /// <summary>
    /// Large margin - 20px
    /// </summary>
    public const double MarginLarge = 20;

    #endregion

    #region Padding

    /// <summary>
    /// Small padding - 10px
    /// </summary>
    public const double PaddingSmall = 10;

    /// <summary>
    /// Medium padding - 15px
    /// </summary>
    public const double PaddingMedium = 15;

    /// <summary>
    /// Large padding - 20px
    /// </summary>
    public const double PaddingLarge = 20;

    #endregion

    #region Gaps (Grid/StackPanel)

    /// <summary>
    /// Extra small gap - 2px
    /// </summary>
    public const double GapXS = 2;

    /// <summary>
    /// Small gap - 5px
    /// </summary>
    public const double GapS = 5;

    /// <summary>
    /// Medium gap - 10px
    /// </summary>
    public const double GapM = 10;

    /// <summary>
    /// Large gap - 15px
    /// </summary>
    public const double GapL = 15;

    #endregion

    #region Border Radius

    /// <summary>
    /// Small border radius - 3px
    /// </summary>
    public const double BorderRadiusSmall = 3;

    /// <summary>
    /// Medium border radius - 5px
    /// </summary>
    public const double BorderRadiusMedium = 5;

    /// <summary>
    /// Large border radius - 8px
    /// </summary>
    public const double BorderRadiusLarge = 8;

    /// <summary>
    /// Rounded border radius - 15px
    /// </summary>
    public const double BorderRadiusRounded = 15;

    #endregion

    #region Border Thickness

    /// <summary>
    /// Thin border - 1px
    /// </summary>
    public const double BorderThin = 1;

    /// <summary>
    /// Medium border - 2px
    /// </summary>
    public const double BorderMedium = 2;

    /// <summary>
    /// Thick border - 3px
    /// </summary>
    public const double BorderThick = 3;

    #endregion

    #region Button Sizes

    /// <summary>
    /// Small button min width - 60px
    /// </summary>
    public const double ButtonMinWidthSmall = 60;

    /// <summary>
    /// Medium button min width - 100px
    /// </summary>
    public const double ButtonMinWidthMedium = 100;

    /// <summary>
    /// Large button min width - 150px
    /// </summary>
    public const double ButtonMinWidthLarge = 150;

    /// <summary>
    /// Button height - 32px
    /// </summary>
    public const double ButtonHeight = 32;

    /// <summary>
    /// Icon button size - 32px
    /// </summary>
    public const double IconButtonSize = 32;

    #endregion

    #region Icon Sizes

    /// <summary>
    /// Small icon - 16px
    /// </summary>
    public const double IconSizeSmall = 16;

    /// <summary>
    /// Medium icon - 24px
    /// </summary>
    public const double IconSizeMedium = 24;

    /// <summary>
    /// Large icon - 32px
    /// </summary>
    public const double IconSizeLarge = 32;

    /// <summary>
    /// Extra large icon - 48px
    /// </summary>
    public const double IconSizeXL = 48;

    #endregion

    #region Grid Sizes

    /// <summary>
    /// Combat grid cell size - 80px
    /// </summary>
    public const double CombatCellSize = 80;

    /// <summary>
    /// Minimap cell size - 8px
    /// </summary>
    public const double MinimapCellSize = 8;

    /// <summary>
    /// Inventory slot size - 64px
    /// </summary>
    public const double InventorySlotSize = 64;

    /// <summary>
    /// Equipment slot size - 72px
    /// </summary>
    public const double EquipmentSlotSize = 72;

    #endregion

    #region Panel Widths

    /// <summary>
    /// Side panel width - 300px
    /// </summary>
    public const double SidePanelWidth = 300;

    /// <summary>
    /// Wide side panel width - 400px
    /// </summary>
    public const double WideSidePanelWidth = 400;

    /// <summary>
    /// Narrow panel width - 250px
    /// </summary>
    public const double NarrowPanelWidth = 250;

    /// <summary>
    /// Dialog min width - 400px
    /// </summary>
    public const double DialogMinWidth = 400;

    /// <summary>
    /// Dialog max width - 800px
    /// </summary>
    public const double DialogMaxWidth = 800;

    #endregion

    #region Animation Durations (milliseconds)

    /// <summary>
    /// Fast animation - 100ms
    /// </summary>
    public const int AnimationFast = 100;

    /// <summary>
    /// Normal animation - 200ms
    /// </summary>
    public const int AnimationNormal = 200;

    /// <summary>
    /// Slow animation - 300ms
    /// </summary>
    public const int AnimationSlow = 300;

    /// <summary>
    /// Very slow animation - 500ms
    /// </summary>
    public const int AnimationVerySlow = 500;

    #endregion

    #region Performance Targets

    /// <summary>
    /// Target frame rate - 60 FPS
    /// </summary>
    public const int TargetFPS = 60;

    /// <summary>
    /// Target frame time in milliseconds - ~16.67ms
    /// </summary>
    public const double TargetFrameTimeMs = 1000.0 / TargetFPS;

    /// <summary>
    /// Max animation update time - 5ms
    /// </summary>
    public const double MaxAnimationUpdateMs = 5;

    /// <summary>
    /// Max cache size in bytes - 100MB
    /// </summary>
    public const long MaxCacheSizeBytes = 100 * 1024 * 1024;

    #endregion

    #region Accessibility

    /// <summary>
    /// Minimum touch target size - 44px (WCAG 2.1)
    /// </summary>
    public const double MinTouchTargetSize = 44;

    /// <summary>
    /// Focus outline width - 2px
    /// </summary>
    public const double FocusOutlineWidth = 2;

    /// <summary>
    /// Minimum contrast ratio (WCAG AA) - 4.5:1
    /// </summary>
    public const double MinContrastRatio = 4.5;

    #endregion

    #region Z-Index Layers

    /// <summary>
    /// Base content layer.
    /// </summary>
    public const int ZIndexBase = 0;

    /// <summary>
    /// Floating elements (dropdowns, tooltips).
    /// </summary>
    public const int ZIndexFloating = 100;

    /// <summary>
    /// Overlays (modals, dialogs).
    /// </summary>
    public const int ZIndexOverlay = 200;

    /// <summary>
    /// Top-most elements (notifications, loading).
    /// </summary>
    public const int ZIndexTop = 300;

    #endregion
}
