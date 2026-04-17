# AGENTS.md

## Scope

This agent applies to the main ASP.NET Core / Blazor application under `CertifiedSkill/`.

## Responsibilities

Use this agent for:
- application wiring
- dependency injection
- middleware
- startup configuration
- app composition
- feature integration across UI, auth, and data layers

## Rules

- Keep `Program.cs` clean and intentional.
- Register services explicitly and group registrations logically.
- Do not place domain logic in startup or UI composition code.
- Keep infrastructure concerns separated from UI concerns.
- Be careful with authentication and authorization defaults.
- Treat configuration involving secrets, keys, connection strings, and identity providers as security-sensitive.

## Configuration

- Never hardcode secrets.
- Prefer environment-specific configuration for local development only where appropriate.
- Assume production secrets belong in secure secret management, such as Azure Key Vault.

## Implementation style

- Favor straightforward ASP.NET Core patterns over custom frameworks.
- Keep startup behavior understandable for future maintainers.
- If a feature needs new services, define clear interfaces and responsibilities.
