namespace RuneAndRust.Infrastructure.Services;

using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;

/// <summary>
/// Triggers sound effects for UI interactions.
/// </summary>
/// <remarks>
/// <para>
/// Features:
/// <list type="bullet">
///   <item><description>Button click and hover sounds</description></item>
///   <item><description>Menu and dialog sounds</description></item>
///   <item><description>Notification and error sounds</description></item>
/// </list>
/// </para>
/// </remarks>
public class UISoundService : IUISoundService
{
    private readonly ISoundEffectService _sfx;
    private readonly ILogger<UISoundService> _logger;

    /// <summary>
    /// Creates a new UI sound service.
    /// </summary>
    /// <param name="sfx">Sound effect service for playback.</param>
    /// <param name="logger">Logger for UI sounds.</param>
    public UISoundService(ISoundEffectService sfx, ILogger<UISoundService> logger)
    {
        _sfx = sfx;
        _logger = logger;
        _logger.LogDebug("UISoundService initialized");
    }

    /// <inheritdoc />
    public void PlayButtonClick()
    {
        _sfx.Play("button-click");
        _logger.LogDebug("UI sound: button click");
    }

    /// <inheritdoc />
    public void PlayButtonHover(float volume = 0.5f)
    {
        _sfx.Play("button-hover", volume);
        _logger.LogDebug("UI sound: button hover");
    }

    /// <inheritdoc />
    public void PlayMenuOpen()
    {
        _sfx.Play("menu-open");
        _logger.LogDebug("UI sound: menu open");
    }

    /// <inheritdoc />
    public void PlayMenuClose()
    {
        _sfx.Play("menu-close");
        _logger.LogDebug("UI sound: menu close");
    }

    /// <inheritdoc />
    public void PlayNotification()
    {
        _sfx.Play("notification");
        _logger.LogDebug("UI sound: notification");
    }

    /// <inheritdoc />
    public void PlayError()
    {
        _sfx.Play("error");
        _logger.LogDebug("UI sound: error");
    }

    /// <inheritdoc />
    public void PlayTabChanged()
    {
        _sfx.Play("button-click", volume: 0.7f);
        _logger.LogDebug("UI sound: tab changed");
    }
}
