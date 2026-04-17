# AGENTS.md

## Scope

This agent applies to authentication, participant identity, magic link flows, external identity linking, and future BankID-related integration services.

## Use this agent for

- login and participant identity flows
- magic link token behavior
- participant-to-person linking
- external identity abstraction
- provider callback handling
- session/state handling for integrations

## Rules

- Treat internal user auth and participant auth as separate concerns.
- Magic links must be single-use, time-limited, and abuse-resistant.
- Identity-to-person linking must be explicit, auditable, and safe.
- Never grant access when identity resolution is ambiguous.
- Design BankID support through provider abstraction, not hardcoded assumptions.

## Integration guidance

- Keep provider-specific details behind interfaces.
- Separate domain logic from external-provider protocol details.
- Model callback and session state carefully.
- Make retry, idempotency, and failure handling explicit.

## Security guidance

- Do not leak token values, personal identifiers, or provider secrets.
- Be careful with logs around auth and callback flows.
- Prefer fail-closed behavior when validation or linkage is uncertain.

## Testing guidance

Add tests for:
- token expiration
- single-use token rules
- invalid/expired login attempts
- ambiguous identity linkage
- idempotent linking behavior
- callback and state validation
