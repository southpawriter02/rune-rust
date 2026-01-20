---
trigger: always_on
---

# Service Design Rules

## 1.1 Application Services
- **ALWAYS** accept dependencies via constructor injection
- **ALWAYS** use interfaces for dependencies (`ILogger<T>`, `IGameRepository`)
- **ALWAYS** mark optional dependencies with `?` and provide defaults
- Use `Async` suffix for async methods
- Return result objects (e.g., `AbilityResult`, `MoveResult`) instead of throwing exceptions for expected failures

```csharp
public class GameSessionService
{
    private readonly IGameRepository _repository;
    private readonly ILogger<GameSessionService> _logger;
    private readonly IExaminationService? _examinationService;
    
    public GameSessionService(
        IGameRepository repository,
        ILogger<GameSessionService> logger,
        IExaminationService? examinationService = null)
    {
        _repository = repository;
        _logger = logger;
        _examinationService = examinationService;
    }
}
```

## 1.2 Domain Services
- Keep domain services pure (no external dependencies)
- Focus on business logic that doesn't belong to a single entity