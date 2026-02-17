# v0.43.8: Combat Animations & Feedback

Type: UI
Description: Implements animations and visual feedback for combat: attack animations, damage number popups, sprite flash on hit/heal, healing effects, ability visualizations, miss/dodge indicators, critical hit effects. 60 FPS target. 6-8 hours.
Priority: Must-Have
Status: In Design
Target Version: Alpha
Dependencies: v0.43.4-v0.43.7
Implementation Difficulty: Medium
Balance Validated: No
Parent item: v0.43: Avalonia Desktop UI Implementation (v0%2043%20Avalonia%20Desktop%20UI%20Implementation%20331e1dc7608c4e9f8a029a22c1124c22.md)
Proof-of-Concept Flag: No
Template Validated: No
Voice Validated: No

**Status:** Not Started

**Prerequisites:** v0.43.4-v0.43.7

**Estimated Time:** 6-8 hours

**Group:** Combat UI

**Deliverable:** Combat animations and visual feedback system

---

## Executive Summary

v0.43.8 implements animations and visual feedback for combat actions, including attack animations, damage numbers, healing effects, hit flashes, and ability visualizations to make combat feel responsive and impactful.

**What This Delivers:**

- Attack animation system
- Damage number popups
- Sprite flash on hit/heal
- Healing effect visualization
- Ability effect animations
- Miss/dodge indicators
- Critical hit effects
- 60 FPS performance target

**Success Metric:** Combat feels responsive with smooth animations at 60 FPS.

---

## Database Schema Changes

No database changes in this specification.

---

## Service Implementation

### AnimationService

```csharp
using Avalonia.Animation;
using RuneAndRust.Core.Combat;
using SkiaSharp;
using System.Diagnostics;

namespace [RuneAndRust.DesktopUI.Services](http://RuneAndRust.DesktopUI.Services);

public interface IAnimationService
{
    Task PlayAttackAnimationAsync(GridPosition attacker, GridPosition target, AttackResult result);
    Task PlayDamageNumberAsync(int damage, GridPosition position, DamageType type);
    Task PlayHealingEffectAsync(int healing, GridPosition position);
    Task PlayStatusEffectAnimationAsync(StatusEffect effect, GridPosition position);
    Task PlayMissIndicatorAsync(GridPosition position);
    Task PlayCriticalHitAsync(GridPosition position);
    Task PlayUnitFlashAsync(GridPosition position, SKColor color, int durationMs = 200);
}

public class AnimationService : IAnimationService
{
    private readonly List<ActiveAnimation> _activeAnimations = new();
    private readonly object _lockObject = new();
    
    public async Task PlayAttackAnimationAsync(GridPosition attacker, GridPosition target, AttackResult result)
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
        if (result.Hit)
        {
            await PlayUnitFlashAsync(target, [SKColors.Red](http://SKColors.Red), 150);
            await PlayDamageNumberAsync(result.DamageDealt, target, result.DamageType);
            
            if (result.Critical)
            {
                await PlayCriticalHitAsync(target);
            }
        }
        else
        {
            await PlayMissIndicatorAsync(target);
        }
    }
    
    public async Task PlayDamageNumberAsync(int damage, GridPosition position, DamageType type)
    {
        var color = type switch
        {
            DamageType.Physical => [SKColors.Red](http://SKColors.Red),
            [DamageType.Fire](http://DamageType.Fire) => SKColors.OrangeRed,
            [DamageType.Ice](http://DamageType.Ice) => SKColors.Cyan,
            DamageType.Lightning => SKColors.Yellow,
            DamageType.Poison => SKColors.Purple,
            DamageType.Psychic => SKColors.Magenta,
            _ => SKColors.White
        };
        
        var animation = new DamageNumberAnimation
        {
            Damage = damage,
            Position = position,
            Color = color,
            StartTime = DateTime.UtcNow,
            DurationMs = 800
        };
        
        AddAnimation(animation);
        await Task.Delay(animation.DurationMs);
    }
    
    public async Task PlayHealingEffectAsync(int healing, GridPosition position)
    {
        // Green particle effect rising
        var animation = new ParticleAnimation
        {
            Position = position,
            ParticleCount = 10,
            Color = SKColors.LimeGreen,
            StartTime = DateTime.UtcNow,
            DurationMs = 600,
            Direction = ParticleDirection.Up
        };
        
        AddAnimation(animation);
        
        // Healing number
        var numberAnimation = new DamageNumberAnimation
        {
            Damage = healing,
            Position = position,
            Color = [SKColors.Green](http://SKColors.Green),
            StartTime = DateTime.UtcNow,
            DurationMs = 800,
            IsHealing = true
        };
        
        AddAnimation(numberAnimation);
        await Task.Delay(Math.Max(animation.DurationMs, numberAnimation.DurationMs));
    }
    
    public async Task PlayStatusEffectAnimationAsync(StatusEffect effect, GridPosition position)
    {
        var color = effect.Type switch
        {
            StatusEffectType.Blessed => [SKColors.Gold](http://SKColors.Gold),
            StatusEffectType.Poisoned => SKColors.Purple,
            StatusEffectType.Burning => SKColors.OrangeRed,
            StatusEffectType.Frozen => SKColors.Cyan,
            _ => SKColors.White
        };
        
        var animation = new StatusEffectAnimation
        {
            Position = position,
            EffectType = effect.Type,
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
            Color = SKColors.Gray,
            StartTime = DateTime.UtcNow,
            DurationMs = 600
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
            Color = SKColors.Yellow,
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

// Animation base class
public abstract class ActiveAnimation
{
    public DateTime StartTime { get; set; }
    public int DurationMs { get; set; }
    
    public float Progress => Math.Clamp(
        (float)(DateTime.UtcNow - StartTime).TotalMilliseconds / DurationMs,
        0f, 1f);
}

public class ProjectileAnimation : ActiveAnimation
{
    public GridPosition StartPosition { get; set; }
    public GridPosition EndPosition { get; set; }
    public SKColor ProjectileColor { get; set; }
}

public class DamageNumberAnimation : ActiveAnimation
{
    public int Damage { get; set; }
    public GridPosition Position { get; set; }
    public SKColor Color { get; set; }
    public bool IsHealing { get; set; }
}

public class ParticleAnimation : ActiveAnimation
{
    public GridPosition Position { get; set; }
    public int ParticleCount { get; set; }
    public SKColor Color { get; set; }
    public ParticleDirection Direction { get; set; }
}

public class StatusEffectAnimation : ActiveAnimation
{
    public GridPosition Position { get; set; }
    public StatusEffectType EffectType { get; set; }
    public SKColor Color { get; set; }
}

public class TextAnimation : ActiveAnimation
{
    public string Text { get; set; } = "";
    public GridPosition Position { get; set; }
    public SKColor Color { get; set; }
    public float Scale { get; set; } = 1.0f;
}

public class FlashAnimation : ActiveAnimation
{
    public GridPosition Position { get; set; }
    public SKColor Color { get; set; }
}

public enum ParticleDirection
{
    Up,
    Down,
    Radial
}
```

### Enhanced CombatGridControl (Animation Rendering)

```csharp
// Add to CombatGridControl from v0.43.4

private readonly IAnimationService _animationService;
private readonly System.Timers.Timer _animationTimer;

public CombatGridControl(ISpriteService spriteService, IAnimationService animationService)
{
    _spriteService = spriteService;
    _animationService = animationService;
    
    // 60 FPS animation timer
    _animationTimer = new System.Timers.Timer(16.67); // ~60 FPS
    _animationTimer.Elapsed += (s, e) =>
    {
        [Dispatcher.UIThread.Post](http://Dispatcher.UIThread.Post)(InvalidateVisual);
    };
    _animationTimer.Start();
}

public override void Render(DrawingContext context)
{
    base.Render(context);
    
    // ... existing rendering code ...
    
    // Render animations last (on top)
    RenderActiveAnimations(canvas);
}

private void RenderActiveAnimations(SKCanvas canvas)
{
    foreach (var animation in _animationService.GetActiveAnimations())
    {
        switch (animation)
        {
            case ProjectileAnimation proj:
                RenderProjectile(canvas, proj);
                break;
            case DamageNumberAnimation dmg:
                RenderDamageNumber(canvas, dmg);
                break;
            case ParticleAnimation particle:
                RenderParticles(canvas, particle);
                break;
            case TextAnimation text:
                RenderText(canvas, text);
                break;
            case FlashAnimation flash:
                RenderFlash(canvas, flash);
                break;
        }
    }
}

private void RenderProjectile(SKCanvas canvas, ProjectileAnimation anim)
{
    var startX = anim.StartPosition.X * CellSize + CellSize / 2;
    var startY = anim.StartPosition.Y * CellSize + CellSize / 2;
    var endX = anim.EndPosition.X * CellSize + CellSize / 2;
    var endY = anim.EndPosition.Y * CellSize + CellSize / 2;
    
    var currentX = startX + (endX - startX) * anim.Progress;
    var currentY = startY + (endY - startY) * anim.Progress;
    
    using var paint = new SKPaint
    {
        Color = anim.ProjectileColor,
        IsAntialias = true
    };
    
    canvas.DrawCircle(currentX, currentY, 5, paint);
}

private void RenderDamageNumber(SKCanvas canvas, DamageNumberAnimation anim)
{
    var x = anim.Position.X * CellSize + CellSize / 2;
    var y = anim.Position.Y * CellSize + CellSize / 2;
    
    // Float upward
    y -= 30 * anim.Progress;
    
    // Fade out
    var alpha = (byte)(255 * (1 - anim.Progress));
    var color = anim.Color.WithAlpha(alpha);
    
    using var paint = new SKPaint
    {
        Color = color,
        TextSize = anim.IsHealing ? 24 : 20,
        IsAntialias = true,
        TextAlign = [SKTextAlign.Center](http://SKTextAlign.Center),
        FakeBoldText = true
    };
    
    var text = anim.IsHealing ? $"+{anim.Damage}" : anim.Damage.ToString();
    canvas.DrawText(text, x, y, paint);
}

private void RenderParticles(SKCanvas canvas, ParticleAnimation anim)
{
    var centerX = anim.Position.X * CellSize + CellSize / 2;
    var centerY = anim.Position.Y * CellSize + CellSize / 2;
    
    for (int i = 0; i < anim.ParticleCount; i++)
    {
        var angle = (float)(i * 2 * Math.PI / anim.ParticleCount);
        var distance = 30 * anim.Progress;
        
        var x = centerX + (float)Math.Cos(angle) * distance;
        var y = centerY + (float)Math.Sin(angle) * distance - (anim.Direction == ParticleDirection.Up ? 20 * anim.Progress : 0);
        
        var alpha = (byte)(255 * (1 - anim.Progress));
        using var paint = new SKPaint
        {
            Color = anim.Color.WithAlpha(alpha),
            IsAntialias = true
        };
        
        canvas.DrawCircle(x, y, 3, paint);
    }
}

private void RenderText(SKCanvas canvas, TextAnimation anim)
{
    var x = anim.Position.X * CellSize + CellSize / 2;
    var y = anim.Position.Y * CellSize + CellSize / 2 - 40;
    
    // Scale up then fade
    var scale = 0.5f + anim.Progress * 0.5f * anim.Scale;
    var alpha = (byte)(255 * (1 - anim.Progress));
    
    using var paint = new SKPaint
    {
        Color = anim.Color.WithAlpha(alpha),
        TextSize = 18 * scale,
        IsAntialias = true,
        TextAlign = [SKTextAlign.Center](http://SKTextAlign.Center),
        FakeBoldText = true
    };
    
    canvas.DrawText(anim.Text, x, y, paint);
}

private void RenderFlash(SKCanvas canvas, FlashAnimation anim)
{
    var x = anim.Position.X * CellSize;
    var y = anim.Position.Y * CellSize;
    
    var alpha = (byte)(150 * (1 - anim.Progress));
    using var paint = new SKPaint
    {
        Color = anim.Color.WithAlpha(alpha)
    };
    
    canvas.DrawRect(x, y, CellSize, CellSize, paint);
}
```

---

## Integration Points

**With v0.43.5 (Combat Actions):**

- Play animations when actions execute
- Update CombatViewModel to trigger animations

**With v0.1 (Combat System):**

- Use `AttackResult` for animation parameters
- Critical hits, misses trigger specific effects

---

## Success Criteria

**v0.43.8 is DONE when:**

### ✅ Attack Animations

- [ ]  Projectile travels from attacker to target
- [ ]  Hit flash on target
- [ ]  Damage numbers appear
- [ ]  Miss indicator shows
- [ ]  Critical hit effect displays

### ✅ Effect Animations

- [ ]  Healing particles rise
- [ ]  Status effects have visual
- [ ]  Smooth 60 FPS performance

### ✅ Performance

- [ ]  No frame drops during combat
- [ ]  Multiple animations simultaneously
- [ ]  Clean animation cleanup

---

**Combat UI complete! Ready for character management in v0.43.9.**