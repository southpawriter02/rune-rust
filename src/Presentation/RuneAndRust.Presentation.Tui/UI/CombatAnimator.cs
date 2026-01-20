using Microsoft.Extensions.Logging;
using RuneAndRust.Application.DTOs;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Presentation.UI;

/// <summary>
/// Plays text-based combat animations.
/// </summary>
/// <remarks>
/// Features:
/// - Frame-based animation with configurable timing
/// - Animation queue for sequential playback
/// - Template-based text with variable substitution
/// - Speed multiplier and skip controls
/// </remarks>
public class CombatAnimator : ICombatAnimator
{
    private readonly ITerminalService _terminal;
    private readonly IAnimationProvider _animationProvider;
    private readonly ScreenLayout _layout;
    private readonly ILogger<CombatAnimator>? _logger;
    
    private readonly Queue<(AnimationType Type, AnimationContext Context)> _animationQueue = new();
    private readonly Random _random = new();
    
    private bool _skipRequested;
    private int _animationAreaY;
    private int _animationAreaX;
    private int _animationAreaWidth;
    
    /// <inheritdoc/>
    public bool AnimationsEnabled { get; set; } = true;
    
    /// <inheritdoc/>
    public float SpeedMultiplier { get; set; } = 1.0f;
    
    /// <inheritdoc/>
    public bool IsPlaying { get; private set; }
    
    /// <inheritdoc/>
    public int QueueCount => _animationQueue.Count;
    
    /// <summary>
    /// Initializes a new instance of <see cref="CombatAnimator"/>.
    /// </summary>
    public CombatAnimator(
        ITerminalService terminal,
        IAnimationProvider animationProvider,
        ScreenLayout layout,
        ILogger<CombatAnimator>? logger = null)
    {
        _terminal = terminal;
        _animationProvider = animationProvider;
        _layout = layout;
        _logger = logger;
    }
    
    /// <inheritdoc/>
    public void QueueAnimation(AnimationType type, AnimationContext context)
    {
        if (!AnimationsEnabled) return;
        
        _animationQueue.Enqueue((type, context));
        _logger?.LogDebug("Queued animation {Type} for {Actor}", type, context.ActorName);
    }
    
    /// <inheritdoc/>
    public async Task PlayQueuedAnimationsAsync(CancellationToken ct = default)
    {
        if (!AnimationsEnabled || _animationQueue.Count == 0) return;
        
        IsPlaying = true;
        SetupAnimationArea();
        
        _logger?.LogDebug("Playing {Count} queued animations", _animationQueue.Count);
        
        try
        {
            while (_animationQueue.TryDequeue(out var item) && !ct.IsCancellationRequested)
            {
                await PlayAnimationAsync(item.Type, item.Context, ct);
                _skipRequested = false;
            }
        }
        finally
        {
            IsPlaying = false;
            ClearAnimationArea();
        }
    }
    
    /// <inheritdoc/>
    public void SkipCurrent()
    {
        _skipRequested = true;
        _logger?.LogDebug("Skip requested for current animation");
    }
    
    /// <inheritdoc/>
    public void ClearQueue()
    {
        _animationQueue.Clear();
        _logger?.LogDebug("Animation queue cleared");
    }
    
    private void SetupAnimationArea()
    {
        var panel = _layout.GetPanel(PanelPosition.MainContent);
        if (panel != null)
        {
            var area = panel.Value.ContentArea;
            _animationAreaX = area.X;
            _animationAreaY = area.Y + area.Height - 5;
            _animationAreaWidth = area.Width;
        }
        else
        {
            // Fallback defaults
            _animationAreaX = 0;
            _animationAreaY = 20;
            _animationAreaWidth = 60;
        }
    }
    
    private async Task PlayAnimationAsync(
        AnimationType type,
        AnimationContext context,
        CancellationToken ct)
    {
        var definition = _animationProvider.GetAnimation(type);
        if (definition == null)
        {
            _logger?.LogWarning("No animation definition found for {Type}", type);
            return;
        }
        
        foreach (var frame in definition.Frames)
        {
            if (_skipRequested || ct.IsCancellationRequested) break;
            
            RenderFrame(frame, context, definition);
            
            var delayMs = (int)(frame.DurationMs / SpeedMultiplier);
            try
            {
                await Task.Delay(delayMs, ct);
            }
            catch (TaskCanceledException)
            {
                break;
            }
            
            if (frame.ClearPrevious)
            {
                ClearAnimationLine();
            }
        }
    }
    
    private void RenderFrame(
        AnimationFrame frame,
        AnimationContext context,
        AnimationDefinition definition)
    {
        var text = SubstituteVariables(frame.TextTemplate, context, definition);
        
        var x = _animationAreaX;
        if (frame.Center)
        {
            x = _animationAreaX + (_animationAreaWidth - text.Length) / 2;
        }
        else if (frame.PositionX.HasValue)
        {
            x = _animationAreaX + frame.PositionX.Value;
        }
        
        var y = frame.PositionY.HasValue
            ? _animationAreaY + frame.PositionY.Value
            : _animationAreaY;
        
        _terminal.SetCursorPosition(Math.Max(0, x), Math.Max(0, y));
        
        if (frame.Color.HasValue)
        {
            WriteColored(text, frame.Color.Value);
        }
        else
        {
            _terminal.Write(text);
        }
    }
    
    private string SubstituteVariables(
        string template,
        AnimationContext context,
        AnimationDefinition definition)
    {
        var result = template
            .Replace("{actor}", context.ActorName)
            .Replace("{target}", context.TargetName ?? "target")
            .Replace("{value}", context.Value?.ToString() ?? "0")
            .Replace("{ability}", context.AbilityName ?? "ability")
            .Replace("{damageType}", context.DamageType ?? "damage")
            .Replace("{status}", context.StatusName ?? "effect");
        
        // Handle {verb} with random selection
        if (result.Contains("{verb}") && definition.Verbs?.Count > 0)
        {
            var verb = definition.Verbs[_random.Next(definition.Verbs.Count)];
            result = result.Replace("{verb}", verb);
        }
        
        return result;
    }
    
    private void ClearAnimationLine()
    {
        _terminal.SetCursorPosition(_animationAreaX, _animationAreaY);
        _terminal.Write(new string(' ', _animationAreaWidth));
    }
    
    private void ClearAnimationArea()
    {
        for (var i = 0; i < 4; i++)
        {
            _terminal.SetCursorPosition(_animationAreaX, _animationAreaY + i);
            _terminal.Write(new string(' ', _animationAreaWidth));
        }
    }
    
    private void WriteColored(string text, ConsoleColor color)
    {
        var prevColor = Console.ForegroundColor;
        Console.ForegroundColor = color;
        _terminal.Write(text);
        Console.ForegroundColor = prevColor;
    }
}
