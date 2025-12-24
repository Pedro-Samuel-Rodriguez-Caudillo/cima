# Plan - Presentation Layer Resilience

## Phase 1: Infrastructure and Global Handling [checkpoint: a2f907d]
Implement the foundational structures for catching and displaying errors globally.

- [x] Task: Create custom `MainLayout` error handling logic to differentiate between Public and Admin users. c2afd99
- [x] Task: Implement a reusable `ErrorDetail` component that shows friendly messages to all, but an expandable stack trace for Admins. 3c02826
- [x] Task: Configure the Blazor `ErrorBoundary` at the root level to catch unhandled lifecycle exceptions. 59ae14a
- [x] Task: Conductor - User Manual Verification 'Infrastructure and Global Handling' (Protocol in workflow.md) a2f907d

## Phase 2: Component-Level Isolation [checkpoint: 034a20e]
Isolate UI modules to prevent cascading failures.

- [x] Task: Fix `MainLayout` top margin to prevent Navbar from obscuring content (User Feedback). 05c399f
- [x] Task: Wrap `PropertyList` and `PropertyDetail` components in specific `ErrorBoundary` instances. 860438e
- [x] Task: Create a `RetryBoundary` component that provides a "Reload" button to reset the ErrorBoundary state. 20850c7
- [x] Task: Conductor - User Manual Verification 'Component-Level Isolation' (Protocol in workflow.md) 034a20e

## Phase 3: API & Form Resilience [checkpoint: 2671010]
Improve the robustness of data mutations and API interactions.

- [x] Task: Create a `LoadingButton` component that manages its own disabled state and spinner during `Task` execution. f5571a4
- [x] Task: Implement a standard "API Error" handler for Admin forms that prevents data loss on failure and allows retry. dd02e20
- [x] Task: Conductor - User Manual Verification 'API & Form Resilience' (Protocol in workflow.md) 2671010

## Phase 4: Standardized Feedback System
Finalize the UI elements for user notifications.

- [x] Task: Implement friendly validation error message parsing for toasts (User Feedback). 4f9e10f
- [x] Task: Implement a `ToastService` and container for floating notifications using Tailwind CSS. 87aeaba
- [~] Task: Create a reusable `Alert` component for inline contextual warnings/info.
- [ ] Task: Update Admin flows to use `Modal` dialogs for critical session or authorization errors.
- [ ] Task: Conductor - User Manual Verification 'Standardized Feedback System' (Protocol in workflow.md)
