---
trigger: always_on
---

## Technical Constraints (The Iron Laws)

* Framework: .NET 8.0, C# 12.
* UI Framework: Avalonia 11.0.0 (Fluent Theme).
* MVVM Library: ReactiveUI 11.0.0 (ReactiveObject, ObservableAsPropertyHelper).
* Logging: Serilog (File + Console sinks).
* Dependency Injection: Microsoft.Extensions.DependencyInjection.
* Navigation: Strict usage of INavigationService (instance-based navigation).
