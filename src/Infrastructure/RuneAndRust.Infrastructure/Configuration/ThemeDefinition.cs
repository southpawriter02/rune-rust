using RuneAndRust.Application.Interfaces;

namespace RuneAndRust.Infrastructure.Configuration;

/// <summary>
/// Represents a complete color theme definition.
/// </summary>
/// <param name="Name">Display name of the theme.</param>
/// <param name="Background">Background console color.</param>
/// <param name="Foreground">Default foreground color.</param>
/// <param name="MessageColors">Message type to color mapping.</param>
public record ThemeDefinition(
    string Name,
    ConsoleColor Background,
    ConsoleColor Foreground,
    Dictionary<MessageType, ConsoleColor> MessageColors);
