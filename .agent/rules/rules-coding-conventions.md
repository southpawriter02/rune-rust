---
trigger: always_on
---

# Coding Conventions

## 1.1 General Style (from .editorconfig)
- Use **4-space indentation** for C# files
- Use **2-space indentation** for JSON, YAML, and XML files
- Use **LF** line endings
- Use **file-scoped namespaces** (`namespace X;` not `namespace X { }`)
- Use **`var`** when type is apparent or for built-in types
- Use **primary constructors** when appropriate
- Use **expression-bodied members** for single-line methods and properties

## 1.2 Naming Conventions
- **PascalCase** for public members, types, and methods
- **camelCase** for private fields and local variables
- **kebab-case** for JSON file names and string IDs (e.g., `"shield-bash"`, `config/abilities.json`)
- Prefix interfaces with `I` (e.g., `IGameRepository`, `IEntity`)
- Suffix DTOs with `Dto` (e.g., `PlayerDto`, `AbilityDefinitionDto`)
- Suffix services with `Service` (e.g., `GameSessionService`, `CombatService`)

## 1.3 Code Organization
- Sort `using` directives with `System` namespaces first
- Group related properties together
- Place private constructors for EF Core before public constructors
- Use `#region` sparingly and only for large files