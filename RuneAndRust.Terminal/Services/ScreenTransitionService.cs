using Microsoft.Extensions.Logging;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Core.Settings;
using Spectre.Console;
using System.Text;

namespace RuneAndRust.Terminal.Services;

/// <summary>
/// Renders ASCII transition animations between game phases (v0.3.14b).
/// Uses Spectre.Console.Live for frame-based rendering.
/// Respects GameSettings.ReduceMotion for accessibility.
/// </summary>
public class ScreenTransitionService : IScreenTransitionService
{
    private readonly ILogger<ScreenTransitionService> _logger;
    private readonly IThemeService _theme;

    private static readonly char[] ShardChars = { '/', '\\', '#', 'X', '%', '&', '█', '▓', '▒' };
    private static readonly char[] GlitchChars = { '░', '▒', '▓', '█', '▀', '▄', '▌', '▐' };
    private const int FrameCount = 10;
    private const int FrameDelayMs = 50;
    private const int MaxHeight = 24;

    /// <summary>
    /// Initializes a new instance of the <see cref="ScreenTransitionService"/> class.
    /// </summary>
    /// <param name="logger">The logger for traceability.</param>
    /// <param name="theme">The theme service for semantic color lookup.</param>
    public ScreenTransitionService(
        ILogger<ScreenTransitionService> logger,
        IThemeService theme)
    {
        _logger = logger;
        _theme = theme;
    }

    /// <inheritdoc/>
    public async Task PlayAsync(TransitionType type)
    {
        // Accessibility guard - skip all animations when ReduceMotion is enabled
        if (GameSettings.ReduceMotion)
        {
            _logger.LogDebug("[VFX] Transition skipped (ReduceMotion: ON)");
            AnsiConsole.Clear();
            return;
        }

        if (type == TransitionType.None)
        {
            AnsiConsole.Clear();
            return;
        }

        _logger.LogTrace("[VFX] Playing transition: {Type}", type);

        switch (type)
        {
            case TransitionType.Shatter:
                await PlayShatterEffectAsync();
                break;
            case TransitionType.Dissolve:
                await PlayDissolveEffectAsync();
                break;
            case TransitionType.GlitchDecay:
                await PlayGlitchDecayEffectAsync();
                break;
            default:
                AnsiConsole.Clear();
                break;
        }

        AnsiConsole.Clear();
        _logger.LogTrace("[VFX] Transition complete");
    }

    /// <summary>
    /// Plays the Shatter effect - screen fills with noise (Combat start).
    /// Density increases from 10% to 100% over 10 frames.
    /// </summary>
    private async Task PlayShatterEffectAsync()
    {
        var enemyColor = _theme.GetColor("EnemyColor");

        await AnsiConsole.Live(new Text(""))
            .AutoClear(false)
            .StartAsync(async ctx =>
            {
                var width = Console.WindowWidth;
                var height = Math.Min(Console.WindowHeight, MaxHeight);
                var random = new Random();

                for (int frame = 1; frame <= FrameCount; frame++)
                {
                    var density = frame * 10; // 10% to 100%
                    var sb = new StringBuilder();

                    for (int y = 0; y < height; y++)
                    {
                        for (int x = 0; x < width; x++)
                        {
                            if (random.Next(100) < density)
                            {
                                var shard = ShardChars[random.Next(ShardChars.Length)];
                                sb.Append($"[{enemyColor}]{shard}[/]");
                            }
                            else
                            {
                                sb.Append(' ');
                            }
                        }
                        if (y < height - 1) sb.AppendLine();
                    }

                    ctx.UpdateTarget(new Markup(sb.ToString()));
                    await Task.Delay(FrameDelayMs);

                    _logger.LogTrace("[VFX] Shatter frame {Frame}/{Total}", frame, FrameCount);
                }
            });
    }

    /// <summary>
    /// Plays the Dissolve effect - screen fades out with dots (Combat victory).
    /// Density decreases from 100% to 0% over 10 frames.
    /// </summary>
    private async Task PlayDissolveEffectAsync()
    {
        var successColor = _theme.GetColor("SuccessColor");

        await AnsiConsole.Live(new Text(""))
            .AutoClear(false)
            .StartAsync(async ctx =>
            {
                var width = Console.WindowWidth;
                var height = Math.Min(Console.WindowHeight, MaxHeight);
                var random = new Random();

                // Dissolve goes from 100% to 0% (reverse of shatter)
                for (int frame = FrameCount; frame >= 1; frame--)
                {
                    var density = frame * 10;
                    var sb = new StringBuilder();

                    for (int y = 0; y < height; y++)
                    {
                        for (int x = 0; x < width; x++)
                        {
                            if (random.Next(100) < density)
                            {
                                sb.Append($"[{successColor}]·[/]");
                            }
                            else
                            {
                                sb.Append(' ');
                            }
                        }
                        if (y < height - 1) sb.AppendLine();
                    }

                    ctx.UpdateTarget(new Markup(sb.ToString()));
                    await Task.Delay(FrameDelayMs);

                    _logger.LogTrace("[VFX] Dissolve frame {Frame}/{Total}", FrameCount - frame + 1, FrameCount);
                }
            });
    }

    /// <summary>
    /// Plays the GlitchDecay effect - text decays to garbage then black (Game Over).
    /// First 7 frames show glitch characters, last 3 frames fade to black.
    /// </summary>
    private async Task PlayGlitchDecayEffectAsync()
    {
        var stressColor = _theme.GetColor("StressHigh");

        await AnsiConsole.Live(new Text(""))
            .AutoClear(false)
            .StartAsync(async ctx =>
            {
                var width = Console.WindowWidth;
                var height = Math.Min(Console.WindowHeight, MaxHeight);
                var random = new Random();

                for (int frame = 1; frame <= FrameCount; frame++)
                {
                    var sb = new StringBuilder();
                    var fadeOut = frame > 7; // Last 3 frames fade to black

                    for (int y = 0; y < height; y++)
                    {
                        for (int x = 0; x < width; x++)
                        {
                            if (fadeOut && random.Next(100) < (frame - 7) * 40)
                            {
                                // Fade to black - increasing empty space
                                sb.Append(' ');
                            }
                            else if (random.Next(100) < 60)
                            {
                                // Glitch characters
                                var glyph = GlitchChars[random.Next(GlitchChars.Length)];
                                sb.Append($"[{stressColor}]{glyph}[/]");
                            }
                            else
                            {
                                sb.Append(' ');
                            }
                        }
                        if (y < height - 1) sb.AppendLine();
                    }

                    ctx.UpdateTarget(new Markup(sb.ToString()));
                    await Task.Delay(FrameDelayMs + 20); // Slightly slower for dramatic effect

                    _logger.LogTrace("[VFX] GlitchDecay frame {Frame}/{Total}", frame, FrameCount);
                }
            });
    }
}
