# Research: Phase 0 — Scaffolding & Local-First Research

Path: /root/code/ai-rag/specs/001-create-a-feature/research.md

## Context
The feature implements scaffolding for AI-RAG Phase 0: a minimal ASP.NET Core API using .NET 8 and a Python FastAPI embeddings service. All development must follow the constitution's TDD and CI-safety rules.

## Decisions
- Decision: Use .NET 8 (ASP.NET Core) for the API.
  - Rationale: Repository default and constitution guidance specify .NET for API projects. The user explicitly indicated the application uses .NET 8.
  - Alternatives considered: .NET 7, Node.js, Python for API. Rejected due to repository conventions and maintainers' familiarity.

- Decision: Use Python 3.11 + FastAPI for the embeddings service.
  - Rationale: Light-weight, matches existing project guidance, easy to run locally.

- Decision: Embedding dimensionality = 384 (all-MiniLM-L6-v2 precomputed).
  - Rationale: Matches constitution and spec notes.

- Decision: Vector persistence: file-backed JSON at `/root/code/ai-rag/samples/vectors.json` for Phase 0.
  - Rationale: CI-safety and reproducibility.

## Risks & Mitigations
- Risk: Contributors may have different .NET SDK versions installed. Mitigation: Document prerequisites and `dotnet --version` check in `quickstart.md`.
- Risk: CI environment lacking Python. Mitigation: Use precomputed embeddings and mock the embeddings provider in CI; include python-based tests only in local dev workflows.

## Next Steps (Phase 1 inputs)
- Produce `data-model.md`, `contracts/ingest_openapi.yaml`, failing contract tests, and `quickstart.md`.

