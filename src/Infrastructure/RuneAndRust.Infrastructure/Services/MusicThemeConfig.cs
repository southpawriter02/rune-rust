namespace RuneAndRust.Infrastructure.Services;

using Microsoft.Extensions.Logging;
using RuneAndRust.Application.DTOs;
using RuneAndRust.Application.Enums;
using RuneAndRust.Application.Interfaces;

/// <summary>
/// Implementation of music theme configuration.
/// </summary>
/// <remarks>
/// <para>
/// Features:
/// <list type="bullet">
///   <item><description>Shuffle or sequential track selection</description></item>
///   <item><description>Intro tracks for themes</description></item>
///   <item><description>Per-theme volume and loop settings</description></item>
///   <item><description>Stinger lookup</description></item>
/// </list>
/// </para>
/// </remarks>
public class MusicThemeConfig : IMusicThemeConfig
{
    private readonly Dictionary<MusicTheme, ThemeDefinition> _themes;
    private readonly Dictionary<string, StingerDefinition> _stingers;
    private readonly Dictionary<MusicTheme, int> _trackIndices = new();
    private readonly Random _random = new();
    private readonly ILogger<MusicThemeConfig> _logger;

    /// <summary>
    /// Creates a new music theme configuration from a config object.
    /// </summary>
    /// <param name="config">The music themes configuration.</param>
    /// <param name="logger">Logger for configuration operations.</param>
    public MusicThemeConfig(MusicThemesConfig config, ILogger<MusicThemeConfig> logger)
    {
        _logger = logger;
        _themes = config.Themes.ToDictionary(t => t.Theme, t => t);
        _stingers = config.Stingers.ToDictionary(s => s.Name.ToLowerInvariant(), s => s);
        _logger.LogInformation("Loaded {ThemeCount} themes, {StingerCount} stingers",
            _themes.Count, _stingers.Count);
    }

    /// <summary>
    /// Creates a new music theme configuration with default values.
    /// </summary>
    /// <param name="logger">Logger for configuration operations.</param>
    public MusicThemeConfig(ILogger<MusicThemeConfig> logger) : this(CreateDefaultConfig(), logger)
    {
    }

    /// <inheritdoc />
    public string? GetTrackForTheme(MusicTheme theme)
    {
        if (!_themes.TryGetValue(theme, out var def) || def.Tracks.Count == 0)
        {
            _logger.LogDebug("No tracks defined for theme {Theme}", theme);
            return null;
        }

        if (def.Shuffle)
        {
            var index = _random.Next(def.Tracks.Count);
            _logger.LogDebug("Shuffle selected track {Index} for {Theme}", index, theme);
            return def.Tracks[index];
        }

        // Sequential playback
        if (!_trackIndices.TryGetValue(theme, out var trackIndex))
        {
            trackIndex = 0;
        }

        var track = def.Tracks[trackIndex % def.Tracks.Count];
        _trackIndices[theme] = trackIndex + 1;
        _logger.LogDebug("Sequential track {Index} for {Theme}: {Track}", trackIndex, theme, track);
        return track;
    }

    /// <inheritdoc />
    public IReadOnlyList<string> GetAllTracksForTheme(MusicTheme theme)
    {
        if (!_themes.TryGetValue(theme, out var def))
        {
            return Array.Empty<string>();
        }
        return def.Tracks;
    }

    /// <inheritdoc />
    public string? GetIntroTrack(MusicTheme theme)
    {
        if (!_themes.TryGetValue(theme, out var def))
        {
            return null;
        }
        return def.IntroTrack;
    }

    /// <inheritdoc />
    public float GetThemeVolume(MusicTheme theme)
    {
        if (!_themes.TryGetValue(theme, out var def))
        {
            return 1.0f;
        }
        return def.Volume;
    }

    /// <inheritdoc />
    public bool ShouldShuffle(MusicTheme theme)
    {
        if (!_themes.TryGetValue(theme, out var def))
        {
            return false;
        }
        return def.Shuffle;
    }

    /// <inheritdoc />
    public bool ShouldLoop(MusicTheme theme)
    {
        if (!_themes.TryGetValue(theme, out var def))
        {
            return true;
        }
        return def.Loop;
    }

    /// <inheritdoc />
    public string? GetStingerTrack(string stingerName)
    {
        var key = stingerName.ToLowerInvariant();
        if (!_stingers.TryGetValue(key, out var def))
        {
            _logger.LogDebug("Stinger not found: {Name}", stingerName);
            return null;
        }
        return def.Track;
    }

    /// <inheritdoc />
    public float GetStingerVolume(string stingerName)
    {
        var key = stingerName.ToLowerInvariant();
        if (!_stingers.TryGetValue(key, out var def))
        {
            return 1.0f;
        }
        return def.Volume;
    }

    /// <inheritdoc />
    public IReadOnlyList<string> GetAvailableStingers() => _stingers.Keys.ToList();

    /// <summary>
    /// Creates a default configuration for testing and fallback.
    /// </summary>
    private static MusicThemesConfig CreateDefaultConfig() => new()
    {
        Themes = new List<ThemeDefinition>
        {
            new()
            {
                Theme = MusicTheme.MainMenu,
                Tracks = new List<string> { "audio/music/menu_theme.ogg" },
                Volume = 0.7f,
                Loop = true,
                Shuffle = false
            },
            new()
            {
                Theme = MusicTheme.Exploration,
                Tracks = new List<string>
                {
                    "audio/music/dungeon_ambient_01.ogg",
                    "audio/music/dungeon_ambient_02.ogg",
                    "audio/music/dungeon_ambient_03.ogg"
                },
                Volume = 0.6f,
                Loop = true,
                Shuffle = true
            },
            new()
            {
                Theme = MusicTheme.Combat,
                Tracks = new List<string> { "audio/music/combat_intense.ogg" },
                IntroTrack = "audio/music/combat_intro.ogg",
                Volume = 0.8f,
                Loop = true,
                Shuffle = false
            },
            new()
            {
                Theme = MusicTheme.BossCombat,
                Tracks = new List<string>
                {
                    "audio/music/boss_epic.ogg",
                    "audio/music/boss_epic_phase2.ogg"
                },
                IntroTrack = "audio/music/boss_intro.ogg",
                Volume = 0.9f,
                Loop = true,
                Shuffle = false
            },
            new()
            {
                Theme = MusicTheme.SafeArea,
                Tracks = new List<string>
                {
                    "audio/music/town_peaceful.ogg",
                    "audio/music/tavern_music.ogg"
                },
                Volume = 0.5f,
                Loop = true,
                Shuffle = true
            }
        },
        Stingers = new List<StingerDefinition>
        {
            new() { Name = "victory", Track = "audio/music/victory_fanfare.ogg", Volume = 1.0f },
            new() { Name = "defeat", Track = "audio/music/defeat_somber.ogg", Volume = 0.8f },
            new() { Name = "level-up", Track = "audio/music/levelup_jingle.ogg", Volume = 1.0f },
            new() { Name = "quest-complete", Track = "audio/music/quest_complete.ogg", Volume = 0.9f }
        },
        Transitions = new TransitionSettings
        {
            DefaultCrossfade = 2.0f,
            CombatCrossfade = 0.5f,
            StingerFadeOut = 1.0f,
            StingerResumeDelay = 0.5f
        }
    };
}
