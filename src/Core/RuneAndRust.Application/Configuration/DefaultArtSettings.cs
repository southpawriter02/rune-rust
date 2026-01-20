namespace RuneAndRust.Application.Configuration;

/// <summary>
/// Settings for default room art rendering.
/// </summary>
/// <param name="Template">Template style (simple-box, ornate, minimal).</param>
/// <param name="ShowExits">Whether to show exit directions.</param>
/// <param name="ShowContents">Whether to show room contents.</param>
public record DefaultArtSettings(
    string Template = "simple-box",
    bool ShowExits = true,
    bool ShowContents = true);
