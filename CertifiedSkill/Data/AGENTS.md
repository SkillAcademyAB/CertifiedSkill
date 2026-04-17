# AGENTS.md

## Scope

This agent applies to the `CertifiedSkill/Data/` area and any work related to entities, persistence, identity data, encryption, hashing, consent, and auditability.

## Use this agent for

- EF Core entities and mappings
- persistence design
- handling Person, Consent, ExternalIdentity
- PNR normalization, encryption, hashing
- dedupe and lookup design
- audit-related data behavior

## Critical security rules

- Never design storage around plaintext PNR.
- Normalize PNR before any validation, encryption, or hashing operations.
- Use hashing for lookup and deduplication.
- Keep encryption and hashing concerns explicit in naming and structure.
- Do not expose sensitive fields casually in entities, DTOs, logs, or debug output.

## Domain rules

- A Person is the core identity record.
- ExternalIdentity must link to exactly one Person.
- Consent must be traceable and versioned.
- Identity-linking flows must avoid ambiguous or unsafe matches.
- Conflict scenarios must be handled explicitly and safely.

## Persistence guidance

- Make sensitive fields obvious in the model.
- Prefer auditable timestamps and actor tracking where relevant.
- Be careful with uniqueness constraints around provider identities and dedupe keys.
- Favor migrations that are explicit and reversible when possible.

## Testing guidance

Add tests for:
- normalization rules
- checksum validation
- encryption/decryption boundaries
- hashing consistency
- dedupe conflict behavior
- consent history behavior
