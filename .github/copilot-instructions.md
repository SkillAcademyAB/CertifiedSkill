# Copilot instructions for CertifiedSkill

## Project overview

CertifiedSkill is a .NET-based application for managing verifiable educational records with strong focus on identity, consent, course results, and certificates.

The system should act as a system of record for:
- persons
- participant identities
- consent records
- course results
- certificates
- future external identities such as BankID

## Technical direction

Prefer the current .NET stack already used in this repository:

- ASP.NET Core
- Blazor Web App
- Entity Framework Core
- SQL Server
- ASP.NET Core Identity

Do not introduce Python or a parallel platform for core application behavior unless explicitly requested.

## Security and privacy constraints

This repository handles highly sensitive personal data.

Always follow these rules:
- Never store or expose PNR in plaintext outside tightly controlled flows.
- Normalize PNR before validation, encryption, and hashing.
- Use hashed representations for lookup and deduplication.
- Keep encryption keys and hashing secrets outside normal app configuration.
- Do not log plaintext PNR.
- Do not expose sensitive personal data in UI, API responses, errors, or logs.
- Apply least-privilege and minimal-data principles in all features.

## Domain expectations

Important domain concepts:
- **Person** is the primary legal and operational identity.
- **Consent** must be traceable, versionable, auditable, and revocable.
- **ExternalIdentity** must be provider-neutral and link to exactly one Person.
- Participant-facing access must only reveal that participant’s own data.

## Coding expectations

When making changes:
- Prefer small, focused changes over broad refactors.
- Preserve current architecture unless there is a strong reason to change it.
- Keep domain rules explicit in code.
- Favor clear names over clever abstractions.
- Add or update tests when business logic changes.
- Avoid introducing hidden coupling between UI, auth, and persistence logic.

## Data and persistence expectations

When working with EF Core or persistence:
- Keep sensitive-field handling explicit.
- Separate normalized, encrypted, and hashed values conceptually and structurally.
- Model auditability deliberately.
- Be careful with migrations that affect identity, consent, or certificate-related records.

## UI expectations

For Blazor/UI work:
- Show the minimum data needed.
- Do not display sensitive identity data unless explicitly required and approved.
- Prefer clear empty states and error states.
- Participant views must be isolated to the authenticated person only.

## Authentication and external identity

When working on authentication:
- Internal users and participants are different concerns.
- Magic link flows must be single-use, time-limited, and abuse-resistant.
- Future BankID integration should be prepared through abstraction, not provider-specific coupling.
- External identity linking must be auditable and conflict-aware.

## Output style for Copilot

When proposing code:
- Explain assumptions briefly.
- Call out security-sensitive decisions.
- Mention where additional tests should be added.
- If repository structure is still evolving, prefer pragmatic placement over premature layering.
