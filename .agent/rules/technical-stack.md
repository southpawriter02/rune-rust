---
trigger: always_on
---

## Technical Stack

### Core Technologies

| Component | Technology | Version |
|-----------|------------|---------|
| **Language** | C# | .NET 8 |
| **Database** | PostgreSQL | 16+ |
| **ORM** | Entity Framework Core | 8.x |
| **Unit Testing** | xUnit | Latest |
| **Mocking** | Moq | Latest |

### Architecture

| Layer | Pattern |
|-------|---------|
| **Engine** | Service-oriented, DI-based |
| **Data Access** | Repository + Unit of Work |
| **Game State** | Entity-based with soft deletes |
| **Save System** | Ironman (auto-save, no reload) |

### UI Targets

| Platform | Technology |
|----------|------------|
| **Terminal/CLI** | Spectre.Console (rich terminal UI) |
| **GUI** | Planned: Avalonia or terminal-first |
| **Web** | Not planned |

### Code Conventions

- **Interfaces**: All services have `I{ServiceName}` interfaces
- **Async**: Use `async/await` for I/O operations
- **Nullable**: Enable nullable reference types
- **Records**: Use for immutable data (results, events)

### Example Service Pattern

```csharp
public interface IExampleService
{
    Task<Result> DoSomethingAsync(int id);
}

public class ExampleService : IExampleService
{
    private readonly IUnitOfWork _unitOfWork;
    
    public ExampleService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }
    
    public async Task<Result> DoSomethingAsync(int id)
    {
        // Implementation
    }
}
```