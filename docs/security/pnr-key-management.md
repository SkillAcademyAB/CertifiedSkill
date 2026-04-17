# PNR key management

This document defines how CertifiedSkill handles encryption keys and hash secrets for Swedish personal identity numbers (PNR).

## Scope

- PNR must be normalized before validation, encryption, and hashing.
- Plaintext PNR must only exist inside tightly controlled application flows.
- Database persistence, lookup, deduplication, logs, UI, and troubleshooting must avoid plaintext PNR exposure.

## Configuration ownership

- Application code owns the contract for required key material through the `PnrProtection` options model.
- Environment owners are responsible for supplying key material outside the repository.
- Production secrets must be stored in a managed secret store such as Azure Key Vault.
- Development secrets may use .NET user secrets or environment variables, but never checked-in configuration files.

## Required configuration contract

The `PnrProtection` configuration supports:

- `Enabled` to explicitly turn on PNR encryption and hashing features
- `ActiveEncryptionKeyVersion` for the key version used for new encryption operations
- `ActiveHashKeyVersion` for the secret version used for new hash operations
- `EncryptionKeys` for versioned encryption key material
- `HashKeys` for versioned hashing secret material

Key material must be base64-encoded and at least 32 bytes long. When `Enabled` is `true`, both active versions must point to configured key entries. Missing or malformed configuration is treated as invalid.

## Environment rules

### Development

- Keep `Enabled` off until the required secrets are configured locally.
- Store key material in user secrets or environment variables only.
- Use synthetic test identities only; never use real PNR in local setup data.

### Test and CI

- Inject non-production key material through secure pipeline secrets.
- Keep test keys isolated from development and production keys.
- Ensure automated tests never print configured secret values.

### Production

- Store secrets in Azure Key Vault or equivalent managed secret storage.
- Restrict write access to a small operational group and read access to the runtime identity only.
- Audit secret reads, writes, and version changes.

## Rotation strategy

- Rotate by adding a new version to `EncryptionKeys` and `HashKeys` while keeping the previous versions available.
- Move `ActiveEncryptionKeyVersion` and `ActiveHashKeyVersion` to the new versions only after the new material is deployed.
- Re-encrypt or re-hash existing records in a controlled migration process before removing old versions.
- Do not delete retired versions until dependent records have been migrated and verified.

## Fallback and incident handling

- If required key material is missing or invalid, fail closed and block PNR-related operations.
- Do not silently fall back to plaintext storage, plaintext lookup, or implicit key generation.
- Treat key validation failures as operational incidents that require secure remediation.
- Restore service by supplying valid key material or by disabling the PNR feature explicitly until remediation is complete.

## Monitoring and traceability

- Log only high-level failure reasons, never key values or plaintext PNR.
- Alert on startup validation failures, secret retrieval failures, and unexpected key version mismatches.
- Record which key version was used for protected writes so rotation and reprocessing remain auditable.
