---
name: abp-ddd-quality
description: Review and refactor CIMA (.NET 9, ABP 9.3, Blazor/MudBlazor) code to enforce ABP and DDD principles across all layers (Domain, Application, HttpApi, Blazor). Use when asked to review components, view models, or layers for DDD/ABP violations, to harden code quality, or to refactor for stronger boundaries, mapping, validation, and application service usage.
---
# ABP DDD Quality

## Overview
Ensure ABP and DDD principles are respected across the whole codebase, with a focus on solid boundaries, correct layering, and resilient implementation.

## Workflow
1. Confirm scope and intent (review only vs refactor) and identify target files and layers.
2. Load only the relevant context:
   - For project conventions: `agents/AGENTS.md`
   - For DDD context: `docs/DDD_REFACTORING_SUMMARY.md`
   - For code style: `conductor/code_styleguides/csharp.md`, `conductor/code_styleguides/html-css.md`
3. Check code against the guardrails in `references/guardrails.md`.
4. Provide findings or implement minimal, safe refactors that preserve behavior while improving boundaries and quality.

## Output Format
- For reviews: list violations ordered by severity, cite file paths, explain why it is a violation, and propose a concrete fix.
- For refactors: outline a short plan, implement focused changes, and call out any follow-up work.
- Always mention missing tests or verification steps when risk is non-trivial.

## Notes for This Repo
- Avoid edits to files marked as critical in `agents/AGENTS.md` unless explicitly requested.
- Prefer application services and DTOs; do not let UI touch domain entities or persistence details.

## References
- `references/guardrails.md` for detailed ABP/DDD checks and common violations.
