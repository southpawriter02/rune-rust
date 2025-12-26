# v0.43.21: UI Testing, Optimization & Bug Fixes

Type: Technical
Description: Final polish specification: comprehensive UI testing suite (80%+ coverage), performance profiling and optimization (60 FPS target), bug fixes, consistency pass on styling, accessibility improvements. 6-10 hours.
Priority: Must-Have
Status: In Design
Target Version: Alpha
Dependencies: v0.43.1-v0.43.20 (All UI implementations)
Implementation Difficulty: Medium
Balance Validated: No
Parent item: v0.43: Avalonia Desktop UI Implementation (v0%2043%20Avalonia%20Desktop%20UI%20Implementation%20331e1dc7608c4e9f8a029a22c1124c22.md)
Proof-of-Concept Flag: No
Template Validated: No
Voice Validated: No

**Status:** Not Started

**Prerequisites:** v0.43.1-v0.43.20 (All UI implementations)

**Estimated Time:** 6-10 hours

**Group:** Systems & Polish

**Deliverable:** Fully tested, optimized, and polished UI

---

## Executive Summary

v0.43.21 is the final polish specification for the Avalonia UI implementation. This focuses on comprehensive testing, performance optimization, bug fixes, and UI consistency across all 20 previous specifications.

**What This Delivers:**

- Comprehensive UI testing suite
- Performance profiling and optimization
- Bug fixes across all UI components
- Consistency pass on styling
- Accessibility improvements
- Final polish and refinements
- Integration testing with all v0.1-v0.42 systems

**Success Metric:** All UI components tested, performant (60 FPS), bug-free, and consistent.

---

## Testing Requirements

### Unit Tests (Target: 80%+ coverage across all ViewModels)

```csharp
// Example test suite structure
namespace RuneAndRust.DesktopUI.Tests;

public class NavigationServiceTests
{
    [Fact]
    public void NavigateTo_ChangesCurrentView()
    {
        var service = new NavigationService(CreateLogger());
        var viewModel = new TestViewModel();
        
        service.NavigateTo(viewModel);
        
        Assert.Same(viewModel, service.CurrentView);
    }
    
    [Fact]
    public void NavigateBack_RestoresPreviousView()
    {
        var service = new NavigationService(CreateLogger());
        var view1 = new TestViewModel();
        var view2 = new TestViewModel();
        
        service.NavigateTo(view1);
        service.NavigateTo(view2);
        service.NavigateBack();
        
        Assert.Same(view1, service.CurrentView);
    }
}

public class CombatViewModelTests
{
    [Fact]
    public async Task AttackAction_ExecutesCorrectly()
    {
        var engine = CreateMockCombatEngine();
        var vm = new CombatViewModel(engine, CreateMockDialogService());
        
        await vm.AttackAsync();
        
        Assert.Equal(TargetingMode.AttackTarget, vm.TargetingMode);
    }
    
    [Fact]
    public void CombatLog_AddsMessages()
    {
        var vm = CreateCombatViewModel();
        
        vm.AddToCombatLog("Test message");
        
        Assert.Single(vm.CombatLog);
        Assert.Contains("Test message", vm.CombatLog[0]);
    }
}

public class InventoryViewModelTests
{
    [Fact]
    public void EquipItem_UpdatesEquipmentSlot()
    {
        var vm = CreateInventoryViewModel();
        var weapon = new Weapon { Name = "Sword" };
        
        vm.EquipItem(new ItemViewModel(weapon));
        
        var weaponSlot = vm.EquipmentSlots.First(s => s.SlotType == EquipmentSlotType.Weapon);
        Assert.NotNull(weaponSlot.EquippedItem);
        Assert.Equal("Sword", [weaponSlot.EquippedItem.Name](http://weaponSlot.EquippedItem.Name));
    }
}
```

---

## Performance Optimization

### Animation Performance

```csharp
// Optimize animation service for 60 FPS target
public class AnimationService
{
    private const int TargetFPS = 60;
    private const double FrameTime = 1000.0 / TargetFPS;
    
    public void Update()
    {
        var sw = Stopwatch.StartNew();
        
        // Update all active animations
        lock (_lockObject)
        {
            foreach (var animation in _activeAnimations)
            {
                animation.Update(sw.Elapsed);
            }
            
            _activeAnimations.RemoveAll(a => a.IsComplete);
        }
        
        sw.Stop();
        
        // Log performance warning if frame time exceeded
        if (sw.ElapsedMilliseconds > FrameTime)
        {
            _logger.Warning($"Animation update took {sw.ElapsedMilliseconds}ms (target: {FrameTime}ms)");
        }
    }
}
```

### Combat Grid Rendering Optimization

```csharp
// Use dirty rectangle tracking to minimize redraws
public class CombatGridControl
{
    private SKRect _dirtyRect = SKRect.Empty;
    private bool _needsFullRedraw = true;
    
    public void InvalidateCell(GridPosition pos)
    {
        var cellRect = new SKRect(
            pos.X * CellSize,
            pos.Y * CellSize,
            (pos.X + 1) * CellSize,
            (pos.Y + 1) * CellSize);
        
        if (_dirtyRect.IsEmpty)
            _dirtyRect = cellRect;
        else
            _dirtyRect.Union(cellRect);
        
        InvalidateVisual();
    }
    
    public override void Render(DrawingContext context)
    {
        if (_needsFullRedraw)
        {
            DrawFullGrid(context);
            _needsFullRedraw = false;
            _dirtyRect = SKRect.Empty;
        }
        else if (!_dirtyRect.IsEmpty)
        {
            DrawDirtyRegion(context, _dirtyRect);
            _dirtyRect = SKRect.Empty;
        }
    }
}
```

### Memory Optimization

```csharp
// Implement sprite caching with size limits
public class SpriteService
{
    private const int MaxCacheSize = 100 * 1024 * 1024; // 100 MB
    private long _currentCacheSize = 0;
    
    public SKBitmap? GetSpriteBitmap(string name, int scale)
    {
        var cacheKey = $"{name}_{scale}";
        
        if (_cache.TryGetValue(cacheKey, out var bitmap))
            return bitmap;
        
        // Load and cache
        bitmap = LoadAndScaleSprite(name, scale);
        
        if (bitmap != null)
        {
            var bitmapSize = bitmap.ByteCount;
            
            // Evict old entries if cache full
            while (_currentCacheSize + bitmapSize > MaxCacheSize)
            {
                EvictOldestCacheEntry();
            }
            
            _cache[cacheKey] = bitmap;
            _currentCacheSize += bitmapSize;
        }
        
        return bitmap;
    }
}
```

---

## Bug Fix Checklist

### High Priority

- [ ]  Combat grid cells not highlighting correctly
- [ ]  Inventory drag-and-drop dropping items
- [ ]  Navigation back button not working
- [ ]  Save/load corrupting game state
- [ ]  Status effect icons not displaying
- [ ]  Keyboard shortcuts conflicting
- [ ]  Memory leaks in animation system
- [ ]  Boss phase transitions freezing

### Medium Priority

- [ ]  Tooltips not appearing on first hover
- [ ]  Minimap not centering on player
- [ ]  Achievement progress bars incorrect
- [ ]  Settings not persisting
- [ ]  Search functionality case-sensitive
- [ ]  HP bars not updating in real-time
- [ ]  Ability tree prerequisite validation

### Low Priority

- [ ]  Minor text alignment issues
- [ ]  Color inconsistencies
- [ ]  Animation timing tweaks
- [ ]  Tooltip text wrapping
- [ ]  Button hover effects

---

## Consistency Pass

### Color Palette Standardization

```csharp
public static class UIColors
{
    // Primary Colors
    public static Color Gold = Color.Parse("#FFD700");
    public static Color DarkRed = Color.Parse("#DC143C");
    public static Color Blue = Color.Parse("#4A90E2");
    public static Color Purple = Color.Parse("#9400D3");
    public static Color Green = Color.Parse("#228B22");
    
    // Backgrounds
    public static Color BackgroundDark = Color.Parse("#1C1C1C");
    public static Color BackgroundMedium = Color.Parse("#2C2C2C");
    public static Color BackgroundLight = Color.Parse("#3C3C3C");
    
    // Text
    public static Color TextPrimary = Colors.White;
    public static Color TextSecondary = Color.Parse("#CCCCCC");
    public static Color TextTertiary = Color.Parse("#888888");
    
    // Status
    public static Color Success = Color.Parse("#228B22");
    public static Color Warning = Color.Parse("#FFA500");
    public static Color Error = Color.Parse("#DC143C");
    public static Color Info = Color.Parse("#4A90E2");
}
```

### Font Size Standardization

- **Headings:** 24px (H1), 20px (H2), 16px (H3)
- **Body:** 14px
- **Small:** 12px
- **Tiny:** 10px

### Spacing Standardization

- **Margins:** 10px, 15px, 20px
- **Padding:** 10px, 15px, 20px
- **Gaps:** 5px, 10px, 15px

---

## Accessibility Improvements

- [ ]  All interactive elements keyboard-accessible
- [ ]  Tab order logical throughout
- [ ]  Focus indicators visible
- [ ]  Color contrast meets WCAG AA
- [ ]  Screen reader friendly labels
- [ ]  Keyboard shortcuts documented
- [ ]  High contrast mode support
- [ ]  Scalable UI (80%-150%)

---

## Integration Testing

### End-to-End Scenarios

**Complete Combat Scenario:**

1. Start new game
2. Navigate dungeon
3. Trigger combat encounter
4. Execute all action types
5. Win combat
6. Collect loot
7. Level up
8. Continue exploration

**Character Progression Scenario:**

1. View character sheet
2. Allocate attribute points
3. Open specialization tree
4. Unlock abilities
5. Rank up abilities
6. Equip items
7. Verify stat bonuses

**Endgame Scenario:**

1. Complete dungeon run
2. View achievements
3. Check meta-progression
4. Select NG+ tier
5. Start new run with modifiers
6. Verify difficulty changes

---

## Performance Benchmarks

### Target Metrics

- **Frame Rate:** 60 FPS sustained
- **Combat Grid Render:** < 16ms per frame
- **Animation Update:** < 5ms per frame
- **Memory Usage:** < 500 MB total
- **Startup Time:** < 3 seconds
- **Save/Load Time:** < 1 second

### Profiling Tools

- Use Avalonia DevTools for UI debugging
- Use dotTrace for performance profiling
- Use dotMemory for memory profiling
- Monitor frame times with built-in diagnostics

---

## Success Criteria

**v0.43.21 is DONE when:**

### ✅ Testing

- [ ]  80%+ unit test coverage
- [ ]  All integration tests pass
- [ ]  Manual testing checklist complete
- [ ]  No critical bugs

### ✅ Performance

- [ ]  60 FPS in all scenarios
- [ ]  Memory usage within budget
- [ ]  No frame drops during combat
- [ ]  Fast startup and save/load

### ✅ Polish

- [ ]  Consistent styling throughout
- [ ]  All colors from palette
- [ ]  All fonts standardized
- [ ]  Smooth animations

### ✅ Accessibility

- [ ]  Full keyboard navigation
- [ ]  Screen reader support
- [ ]  High contrast mode
- [ ]  Scalable UI

---

**v0.43: Avalonia Desktop UI Implementation COMPLETE!**

**Total Implementation:** 21 specifications, 115-155 hours

**Ready for:** Production deployment and player testing