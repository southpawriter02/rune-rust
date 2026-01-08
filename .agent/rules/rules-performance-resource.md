---
trigger: always_on
---

# Performance & Resource Rules

## 1.1 Collections
- Use `IReadOnlyList<T>` for return types when modification is not intended
- Use `Dictionary<string, T>` for lookups by ID
- Normalize IDs to lowercase for case-insensitive comparisons: `id.ToLowerInvariant()`

## 1.2 Async/Await
- Use `async/await` for I/O operations
- Accept `CancellationToken` for cancellable operations
- Name parameter `ct` or `cancellationToken`