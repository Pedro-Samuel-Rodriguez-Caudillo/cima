# C# & Razor Code Style Guide

## General C# Conventions
- **Naming:** PascalCase for classes, methods, and public properties. camelCase for local variables and private fields (with `_` prefix for fields).
- **Formatting:** Use K&R braces (on new line).
- **Async/Await:** Use `async` and `await` for I/O-bound operations. Avoid `.Result` or `.Wait()`.

## Razor / Blazor
- **Components:** PascalCase for component names (e.g., `UserProfile.razor`).
- **Directives:** Place `@page`, `@using`, and `@inject` directives at the top of the file.
- **Code Block:** Use `@code { ... }` blocks at the bottom of the file for logic.

## Control Flow & Complexity
- **Nesting Limit:** **Avoid `if` nesting levels greater than 3.**
    - *Reason:* Deep nesting reduces readability and increases cognitive load.
    - *Refactoring:* Use guard clauses (early returns), extract methods, or use switch expressions to flatten logic.
    - *Exception:* Deep nesting is permitted ONLY if there is a compelling algorithmic reason or if flattening would significantly obscure the logic.
