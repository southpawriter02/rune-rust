using RuneAndRust.Core;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RuneAndRust.DesktopUI.Services;

/// <summary>
/// Service for managing combat animations and visual feedback.
/// Handles attack animations, damage numbers, healing effects, and status indicators.
/// </summary>
public interface IAnimationService
{
    Task PlayAttackAnimationAsync(GridPosition attacker, GridPosition target, bool hit, int damage, bool critical);
    Task PlayDamageNumberAsync(int damage, GridPosition position, string damageType);
    Task PlayHealingEffectAsync(int healing, GridPosition position);
    Task PlayStatusEffectAnimationAsync(string effectType, GridPosition position);
    Task PlayMissIndicatorAsync(GridPosition position);
    Task PlayCriticalHitAsync(GridPosition position);
    Task PlayUnitFlashAsync(GridPosition position, SKColor color, int durationMs = 200);
    void Update();
    IEnumerable<ActiveAnimation> GetActiveAnimations();
}

public class AnimationService : IAnimationService
{
    private readonly List<ActiveAnimation> _activeAnimations = new();
    private readonly object _lockObject = new();

    public async Task PlayAttackAnimationAsync(GridPosition attacker, GridPosition target, bool hit, int damage, bool critical)
    {
        // Create projectile animation from attacker to target
        var animation = new ProjectileAnimation
        {
            StartPosition = attacker,
            EndPosition = target,
            StartTime = DateTime.UtcNow,
            DurationMs = 300,
            ProjectileColor = SKColors.White
        };

        AddAnimation(animation);
        await Task.Delay(animation.DurationMs);

        // Flash target on hit
        if (hit)
        {
            await PlayUnitFlashAsync(target, SKColors.Red, 150);

            if (damage > 0)
            {
                await PlayDamageNumberAsync(damage, target, "Physical");
            }

            if (critical)
            {
                await PlayCriticalHitAsync(target);
            }
        }
        else
        {
            await PlayMissIndicatorAsync(target);
        }
    }

    public async Task PlayDamageNumberAsync(int damage, GridPosition position, string damageType)
    {
        var color = damageType.ToLowerInvariant() switch
        {
            "physical" => new SKColor(220, 20, 60),      // Crimson
            "fire" => new SKColor(255, 69, 0),            // Orange Red
            "ice" => new SKColor(0, 255, 255),            // Cyan
            "lightning" or "electrical" or "electric" => new SKColor(255, 255, 0), // Yellow
            "poison" or "toxic" => new SKColor(160, 32, 240), // Purple
            "psychic" or "darkness" => new SKColor(255, 0, 255), // Magenta
            "acid" => new SKColor(50, 205, 50),           // Lime Green
            "corruption" => new SKColor(139, 0, 139),     // Dark Magenta
            _ => SKColors.White
        };

        var animation = new DamageNumberAnimation
        {
            Damage = damage,
            Position = position,
            Color = color,
            StartTime = DateTime.UtcNow,
            DurationMs = 800,
            IsHealing = false
        };

        AddAnimation(animation);
        await Task.Delay(animation.DurationMs);
    }

    public async Task PlayHealingEffectAsync(int healing, GridPosition position)
    {
        // Green particle effect rising
        var particleAnimation = new ParticleAnimation
        {
            Position = position,
            ParticleCount = 10,
            Color = new SKColor(50, 205, 50), // Lime Green
            StartTime = DateTime.UtcNow,
            DurationMs = 600,
            Direction = ParticleDirection.Up
        };

        AddAnimation(particleAnimation);

        // Healing number
        var numberAnimation = new DamageNumberAnimation
        {
            Damage = healing,
            Position = position,
            Color = new SKColor(0, 255, 0), // Green
            StartTime = DateTime.UtcNow,
            DurationMs = 800,
            IsHealing = true
        };

        AddAnimation(numberAnimation);
        await Task.Delay(Math.Max(particleAnimation.DurationMs, numberAnimation.DurationMs));
    }

    public async Task PlayStatusEffectAnimationAsync(string effectType, GridPosition position)
    {
        var color = effectType.ToLowerInvariant() switch
        {
            "blessed" or "inspired" => new SKColor(255, 215, 0),  // Gold
            "poisoned" => new SKColor(160, 32, 240),               // Purple
            "burning" or "bleeding" => new SKColor(255, 69, 0),   // Orange Red
            "frozen" or "slowed" => new SKColor(0, 255, 255),     // Cyan
            "stunned" => new SKColor(255, 255, 0),                 // Yellow
            "vulnerable" => new SKColor(255, 165, 0),              // Orange
            "corroded" => new SKColor(50, 205, 50),                // Lime Green
            _ => SKColors.White
        };

        var animation = new StatusEffectAnimation
        {
            Position = position,
            EffectType = effectType,
            Color = color,
            StartTime = DateTime.UtcNow,
            DurationMs = 500
        };

        AddAnimation(animation);
        await Task.Delay(animation.DurationMs);
    }

    public async Task PlayMissIndicatorAsync(GridPosition position)
    {
        var animation = new TextAnimation
        {
            Text = "MISS",
            Position = position,
            Color = new SKColor(128, 128, 128), // Gray
            StartTime = DateTime.UtcNow,
            DurationMs = 600,
            Scale = 1.0f
        };

        AddAnimation(animation);
        await Task.Delay(animation.DurationMs);
    }

    public async Task PlayCriticalHitAsync(GridPosition position)
    {
        var animation = new TextAnimation
        {
            Text = "CRITICAL!",
            Position = position,
            Color = new SKColor(255, 255, 0), // Yellow
            StartTime = DateTime.UtcNow,
            DurationMs = 700,
            Scale = 1.5f
        };

        AddAnimation(animation);
        await Task.Delay(animation.DurationMs);
    }

    public async Task PlayUnitFlashAsync(GridPosition position, SKColor color, int durationMs = 200)
    {
        var animation = new FlashAnimation
        {
            Position = position,
            Color = color,
            StartTime = DateTime.UtcNow,
            DurationMs = durationMs
        };

        AddAnimation(animation);
        await Task.Delay(durationMs);
    }

    private void AddAnimation(ActiveAnimation animation)
    {
        lock (_lockObject)
        {
            _activeAnimations.Add(animation);
        }
    }

    public void Update()
    {
        lock (_lockObject)
        {
            var now = DateTime.UtcNow;
            _activeAnimations.RemoveAll(a => (now - a.StartTime).TotalMilliseconds > a.DurationMs);
        }
    }

    public IEnumerable<ActiveAnimation> GetActiveAnimations()
    {
        lock (_lockObject)
        {
            return _activeAnimations.ToList();
        }
    }
}

/// <summary>
/// Base class for all combat animations.
/// </summary>
public abstract class ActiveAnimation
{
    public DateTime StartTime { get; set; }
    public int DurationMs { get; set; }

    /// <summary>
    /// Gets the animation progress from 0.0 to 1.0.
    /// </summary>
    public float Progress => Math.Clamp(
        (float)(DateTime.UtcNow - StartTime).TotalMilliseconds / DurationMs,
        0f, 1f);
}

/// <summary>
/// Projectile animation traveling from attacker to target.
/// </summary>
public class ProjectileAnimation : ActiveAnimation
{
    public GridPosition StartPosition { get; set; }
    public GridPosition EndPosition { get; set; }
    public SKColor ProjectileColor { get; set; }
}

/// <summary>
/// Damage or healing number floating upward and fading.
/// </summary>
public class DamageNumberAnimation : ActiveAnimation
{
    public int Damage { get; set; }
    public GridPosition Position { get; set; }
    public SKColor Color { get; set; }
    public bool IsHealing { get; set; }
}

/// <summary>
/// Particle effect with multiple particles moving outward or upward.
/// </summary>
public class ParticleAnimation : ActiveAnimation
{
    public GridPosition Position { get; set; }
    public int ParticleCount { get; set; }
    public SKColor Color { get; set; }
    public ParticleDirection Direction { get; set; }
}

/// <summary>
/// Status effect application animation.
/// </summary>
public class StatusEffectAnimation : ActiveAnimation
{
    public GridPosition Position { get; set; }
    public string EffectType { get; set; } = string.Empty;
    public SKColor Color { get; set; }
}

/// <summary>
/// Text overlay animation (MISS, CRITICAL, etc.).
/// </summary>
public class TextAnimation : ActiveAnimation
{
    public string Text { get; set; } = string.Empty;
    public GridPosition Position { get; set; }
    public SKColor Color { get; set; }
    public float Scale { get; set; } = 1.0f;
}

/// <summary>
/// Flash overlay on unit sprite.
/// </summary>
public class FlashAnimation : ActiveAnimation
{
    public GridPosition Position { get; set; }
    public SKColor Color { get; set; }
}

/// <summary>
/// Direction for particle animations.
/// </summary>
public enum ParticleDirection
{
    Up,
    Down,
    Radial
}
