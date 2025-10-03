---
description: C# team coding standards and conventions
applyTo: '**/*.cs'
---

# C# Team Standards

## Naming Conventions
- **PascalCase**: Classes, interfaces (prefix with `I`), structs, records, methods, properties, events, namespaces
- **camelCase**: Method parameters, local variables, private/internal fields (prefix with `_`)
- **Static fields**: `s_` prefix for static, `t_` for thread-static
- **Constants**: PascalCase for both fields and local constants
- **Type parameters**: Prefix with `T` (e.g., `TSession`, `TInput`)
- Avoid abbreviations except widely known ones (HTTP, ID, SQL)
- Use descriptive names; favor clarity over brevity
- No Hungarian notation, underscores in public members, or single-letter names (except loop counters)

## Code Style
- Use `var` only when type is obvious from right side
- Prefer language keywords (`string`, `int`) over runtime types (`String`, `Int32`)
- Use string interpolation: `$"{lastName}, {firstName}"`
- Use collection initializers and object initializers where appropriate
- Prefer expression-bodied members for simple one-liners
- Use pattern matching and modern C# features (records, primary constructors, etc.)

## Exception Handling
- Catch specific exceptions only; avoid catching `System.Exception` without filters
- Use specific exception types for meaningful error messages
- Let exceptions propagate if you can't handle them properly

## Async/Await
- Use `async`/`await` for I/O-bound operations
- Name async methods with `Async` suffix
- Use `ConfigureAwait(false)` in library code to avoid deadlocks
- Avoid `async void` except for event handlers

## LINQ & Collections
- Use meaningful query variable names
- Use PascalCase for anonymous type properties
- Prefer LINQ methods for collection manipulation
- Use aliases to clarify ambiguous property names in joins

## Modern C# Features
- Use records for immutable data transfer objects
- Use primary constructors where appropriate
- Use file-scoped namespaces to reduce indentation
- Use nullable reference types (`#nullable enable`)
- Use target-typed `new()` expressions
- Prefer `is` patterns over type checks and casts

## Organization
- One class/interface/enum per file (exceptions for nested types)
- File name matches primary type name
- Order members: fields, constructors, properties, methods
- Group interface implementations together
- Use `#region` sparingly; prefer proper class decomposition

## Comments & Documentation
- Use XML documentation (`///`) for public APIs
- Write clear, concise comments explaining "why", not "what"
- Keep comments up-to-date with code changes
- Avoid obvious comments

## Testing
- Follow Arrange-Act-Assert pattern
- One logical assertion per test
- Use descriptive test method names: `MethodName_Scenario_ExpectedBehavior`
- Mock external dependencies

## Performance
- Use `StringBuilder` for string concatenation in loops
- Prefer `List<T>` over `ArrayList`
- Use `readonly` for fields that don't change after initialization
- Dispose `IDisposable` resources (use `using` statements)
- Avoid boxing/unboxing where possible

## Code Quality
- Keep methods small and focused (single responsibility)
- Avoid deep nesting (max 3-4 levels)
- Magic numbers → named constants or enums
- DRY (Don't Repeat Yourself) - extract common logic
- SOLID principles for class design