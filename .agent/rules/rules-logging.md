---
trigger: always_on
---

# Logging Rules

## 1.1 Structured Logging
- **ALWAYS** use Serilog for logging
- **ALWAYS** use structured logging with message templates
- **NEVER** use string interpolation in log messages

```csharp
// ✅ DO: Use message templates
_logger.LogInformation("Player {PlayerName} dealt {Damage} damage to {Target}", 
    player.Name, damage, monster.Name);

// ❌ DON'T: Use string interpolation
_logger.LogInformation($"Player {player.Name} dealt {damage} damage");
```

## 1.2 Log Levels
- `LogDebug` - Detailed debugging info
- `LogInformation` - General operational info (ability used, level up)
- `LogWarning` - Unexpected but recoverable situations
- `LogError` - Errors that need attention