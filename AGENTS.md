# AGENTS.md

## Purpose

This repository builds CertifiedSkill, a system for managing certifiable educational evidence with strong identity, consent, and privacy requirements.

## Use this agent for

- high-level repo guidance
- architecture-aware changes
- cross-cutting planning
- changes that affect multiple parts of the application

## Core rules

- Treat Person as the central system-of-record concept.
- Protect PNR and other sensitive identity data by default.
- Prefer explicit, traceable, auditable solutions.
- Keep implementation aligned with the .NET stack already in the repository.
- Avoid adding new frameworks or platform shifts unless explicitly requested.

## Priorities

1. correctness
2. privacy and security
3. auditability
4. simplicity
5. maintainability

## When touching code

- Check whether the change affects auth, persistence, UI, or domain boundaries.
- Prefer minimal changes with clear reasoning.
- Preserve future support for external identity providers such as BankID.
- Avoid mixing participant-facing access rules with admin/internal logic.

## When uncertain

Default to the safest option for personal data, and make assumptions explicit.
