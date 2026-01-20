using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Infrastructure.Providers;

/// <summary>
/// Provides default combat animation definitions.
/// </summary>
public class AnimationProvider : IAnimationProvider
{
    private readonly Dictionary<AnimationType, AnimationDefinition> _animations = new();
    private readonly ILogger<AnimationProvider>? _logger;
    
    /// <inheritdoc/>
    public IReadOnlyList<AnimationType> AvailableAnimations => _animations.Keys.ToList();
    
    /// <summary>
    /// Initializes a new instance of <see cref="AnimationProvider"/>.
    /// </summary>
    public AnimationProvider(ILogger<AnimationProvider>? logger = null)
    {
        _logger = logger;
        InitializeDefaultAnimations();
        _logger?.LogInformation("AnimationProvider initialized with {Count} animations", _animations.Count);
    }
    
    /// <inheritdoc/>
    public AnimationDefinition? GetAnimation(AnimationType type)
    {
        if (_animations.TryGetValue(type, out var animation))
        {
            return animation;
        }
        
        _logger?.LogDebug("No animation found for type {Type}", type);
        return null;
    }
    
    private void InitializeDefaultAnimations()
    {
        // Attack Hit
        _animations[AnimationType.AttackHit] = new AnimationDefinition(
            AnimationType.AttackHit,
            new List<AnimationFrame>
            {
                new("{actor} strikes {target}!", 100),
                new("* {verb} *", 150, ConsoleColor.Red, Center: true),
                new("-{value}", 200, ConsoleColor.DarkRed, Center: true),
                new("{target} reels from the blow!", 100)
            },
            Verbs: new[] { "SLASH", "STRIKE", "HIT", "BASH" });
        
        // Attack Miss
        _animations[AnimationType.AttackMiss] = new AnimationDefinition(
            AnimationType.AttackMiss,
            new List<AnimationFrame>
            {
                new("{actor} swings at {target}...", 100),
                new("* WHOOSH *", 150, ConsoleColor.Gray, Center: true),
                new("MISS!", 200, ConsoleColor.DarkGray, Center: true),
                new("{target} dodges the attack!", 100)
            });
        
        // Critical Hit
        _animations[AnimationType.CriticalHit] = new AnimationDefinition(
            AnimationType.CriticalHit,
            new List<AnimationFrame>
            {
                new("{actor} strikes {target}!", 100),
                new("* CRITICAL HIT! *", 200, ConsoleColor.Magenta, Center: true),
                new("-{value}!", 150, ConsoleColor.Red, Center: true),
                new("DEVASTATING BLOW!", 100, ConsoleColor.Yellow, Center: true)
            });
        
        // Ability Cast
        _animations[AnimationType.AbilityCast] = new AnimationDefinition(
            AnimationType.AbilityCast,
            new List<AnimationFrame>
            {
                new("{actor} casts {ability}!", 150, ConsoleColor.Cyan),
                new("*", 100, ConsoleColor.Yellow, Center: true),
                new("***", 100, ConsoleColor.Yellow, Center: true),
                new("*****", 100, ConsoleColor.Yellow, Center: true)
            });
        
        // Ability Effect
        _animations[AnimationType.AbilityEffect] = new AnimationDefinition(
            AnimationType.AbilityEffect,
            new List<AnimationFrame>
            {
                new("{ability} hits {target}!", 100),
                new("-{value} {damageType}!", 200, ConsoleColor.Cyan, Center: true)
            });
        
        // Heal
        _animations[AnimationType.Heal] = new AnimationDefinition(
            AnimationType.Heal,
            new List<AnimationFrame>
            {
                new("{actor} heals!", 100, ConsoleColor.Green),
                new("+{value} HP", 200, ConsoleColor.Green, Center: true),
                new("{actor} feels restored!", 100, ConsoleColor.Green)
            });
        
        // Damage Number
        _animations[AnimationType.DamageNumber] = new AnimationDefinition(
            AnimationType.DamageNumber,
            new List<AnimationFrame>
            {
                new("-{value}", 100, ConsoleColor.Red, PositionY: 0),
                new("-{value}", 100, ConsoleColor.Red, PositionY: -1),
                new("-{value}", 100, ConsoleColor.DarkRed, PositionY: -2)
            });
        
        // Death
        _animations[AnimationType.Death] = new AnimationDefinition(
            AnimationType.Death,
            new List<AnimationFrame>
            {
                new("{target} collapses!", 200),
                new("X", 300, ConsoleColor.DarkGray, Center: true),
                new("[DEFEATED]", 200, ConsoleColor.DarkRed, Center: true)
            });
        
        // Status Applied
        _animations[AnimationType.StatusApplied] = new AnimationDefinition(
            AnimationType.StatusApplied,
            new List<AnimationFrame>
            {
                new("{target} is now [{status}]!", 200, ConsoleColor.Yellow)
            });
        
        // Status Removed
        _animations[AnimationType.StatusRemoved] = new AnimationDefinition(
            AnimationType.StatusRemoved,
            new List<AnimationFrame>
            {
                new("[{status}] wore off from {target}!", 200, ConsoleColor.Gray)
            });
    }
}
