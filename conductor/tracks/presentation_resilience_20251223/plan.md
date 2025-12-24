# Plan - Presentation Layer Resilience

## Phase 1: Infrastructure and Global Handling
Implement the foundational structures for catching and displaying errors globally.

- [x] Task: Create custom `MainLayout` error handling logic to differentiate between Public and Admin users. c2afd99
- [x] Task: Implement a reusable `ErrorDetail` component that shows friendly messages to all, but an expandable stack trace for Admins. 3c02826
- [~] Task: Configure the Blazor `ErrorBoundary` at the root level to catch unhandled lifecycle exceptions.
- [ ] Task: Conductor - User Manual Verification 'Infrastructure and Global Handling' (Protocol in workflow.md)

## Phase 2: Component-Level Isolation
Isolate UI modules to prevent cascading failures.

- [ ] Task: Wrap `PropertyList` and `PropertyDetail` components in specific `ErrorBoundary` instances.
- [ ] Task: Create a `RetryBoundary` component that provides a "Reload" button to reset the ErrorBoundary state.
- [ ] Task: Conductor - User Manual Verification 'Component-Level Isolation' (Protocol in workflow.md)

## Phase 3: API & Form Resilience
Improve the robustness of data mutations and API interactions.

- [ ] Task: Create a `LoadingButton` component that manages its own disabled state and spinner during `Task` execution.
- [ ] Task: Implement a standard "API Error" handler for Admin forms that prevents data loss on failure and allows retry.
- [ ] Task: Conductor - User Manual Verification 'API & Form Resilience' (Protocol in workflow.md)

## Phase 4: Standardized Feedback System
Finalize the UI elements for user notifications.

- [ ] Task: Implement a `ToastService` and container for floating notifications using Tailwind CSS.
- [ ] Task: Create a reusable `Alert` component for inline contextual warnings/info.
- [ ] Task: Update Admin flows to use `Modal` dialogs for critical session or authorization errors.
- [ ] Task: Conductor - User Manual Verification 'Standardized Feedback System' (Protocol in workflow.md)
