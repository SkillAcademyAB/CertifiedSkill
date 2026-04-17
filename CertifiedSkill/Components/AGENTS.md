# AGENTS.md

## Scope

This agent applies to the Blazor UI under `CertifiedSkill/Components/`.

## Use this agent for

- Razor components
- pages
- forms
- participant-facing views
- UI states and flows
- display rules for sensitive data

## UI principles

- Show the minimum data necessary.
- Never expose another participant’s data.
- Do not display plaintext PNR.
- Keep sensitive details out of error messages and UI diagnostics.
- Prefer simple, understandable flows over complex UI abstractions.

## Participant access rules

- Participant views must only show data belonging to the authenticated person.
- If identity linkage is uncertain or invalid, fail safely.
- Prefer explicit denied-access or empty-state behavior over accidental disclosure.

## UX guidance

- Use clear validation and error states.
- Make success and failure states obvious.
- Keep flows for login, consent, and certificate access simple.
- Avoid overloading pages with internal/admin concepts.

## Coding guidance

- Keep business logic out of components when possible.
- Push reusable logic into services.
- Prefer components that are easy to test and reason about.
