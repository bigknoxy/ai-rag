<!--
Sync Impact Report
- Version change: v2.1.1 → v2.2.0
- Modified / Added principles:
  - I. Local-First & Zero-Cost (refined from general defaults)
  - II. Modular Adapter-Driven Architecture (explicit)
  - III. Test-First (TDD) (NEW: made non-negotiable and prescriptive)
  - IV. Retrieval-First & Safety (clarified default runtime behavior)
  - V. Observability, Versioning & Simplicity (combined concerns, clarified semantic versioning guidance)
- Added sections:
  - Development Workflow (explicit TDD + gating + CI safety)
- Removed sections: none
- Templates requiring updates (status):
  - .specify/templates/plan-template.md: ⚠ pending — update constitution version reference and Constitution Check gates to reference TDD / CI-safety specifics.
  - .specify/templates/spec-template.md: ⚠ pending — ensure "Mandatory sections" and "Acceptance Scenarios" require testable criteria and TDD markers.
  - .specify/templates/tasks-template.md: ⚠ pending — align the "Tests First (TDD)" section with the constitution's mandatory TDD phrasing and ensure the gating language is enforced.
  - .specify/templates/agent-file-template.md: ⚠ pending — update "Last updated" + guidance to include TDD & CI-safety.
- Runtime docs review (recommended edits):
  - context/projects/ai-rag/README.md: ⚠ pending — add a short governance note about TDD and CI safety, and link to the constitution.
  - context/projects/ai-rag/quickstart.md: ✅ largely aligned (CI notes present); recommend adding a short note about writing failing tests before implementation for contributors.
- Deferred / TODO items:
  - RATIFICATION_DATE: TODO(RATIFICATION_DATE): original ratification/adoption date not present in repo — maintainer must set.
  - If the project maintains a `MAINTAINERS.md`, confirm the maintainer approval rule (the constitution requires approvals by maintainers; file missing => manual follow-up).
-->

# AI-RAG Constitution

## Core Principles

### I. Local-First & Zero-Cost
The AI‑RAG project MUST remain runnable in its default configuration without payment, accounts, or external paid services. The default demo and CI flows MUST use local, CPU-friendly components (small sentence-transformer embeddings, in-memory or local vector store). CI runs MUST use mocks or precomputed artifacts and MUST NOT call external APIs or require secrets.

Rationale: Ensures the project is reproducible and accessible to anyone with modest hardware (~8GB RAM), and prevents CI from flakiness or cost incursions.

### II. Modular Adapter-Driven Architecture
All externally-facing subsystems (Embedding Provider, Vector Store, LLM Adapter, Conversation Store, etc.) MUST be implemented behind small, well-documented interfaces that support multiple implementations. New functionality MUST prefer creating or extending an adapter over modifying core logic directly. Dependency Injection (DI) is required for service wiring.

Rationale: Keeps components pluggable for testing and optional vendor integration while preserving the local-first default. Encourages small, single-responsibility services.

### III. Test-First (TDD) — NON-NEGOTIABLE
TDD is mandatory for feature work:
- Tests (unit, contract, or integration) MUST be authored and committed before implementation code that satisfies them.
- Tests MUST initially fail (red) on CI; implementation follows to make tests pass (green), then refactor.
- All new features and changes MUST include tests which can run in the CI environment without external service calls (use mocks or precomputed artifacts).
- Pull requests that add implementation without first adding failing tests WILL be rejected.

Rationale: Guarantees behavior-driven development and prevents regressions. Enforces CI-safety and maintainability.

### IV. Retrieval-First & Safety
The default runtime mode MUST be retrieval-only (assemble passages from retrieved chunks with citations). RAG (generation with an LLM) is explicitly opt‑in and MUST be disabled by default. When RAG is enabled, prompts and outputs MUST include safety filters and clear source citations. Any path that invokes an LLM MUST be documented, must be optional, and must not be exercised by CI.

Rationale: Prioritizes factuality and avoids surprise external model calls. Keeps the demo deterministic by default.

### V. Observability, Versioning & Simplicity
- Observability: Components MUST emit structured logs and basic metrics useful for local debugging (e.g., request/response times, ingestion metrics). Logs MUST make it straightforward to map responses back to source chunks.
- Versioning: Project artifacts and public adapters MUST follow semantic versioning. Governance-level versioning for constitution changes follows MAJOR.MINOR.PATCH semantics (see Governance).
- Simplicity: Prefer the simplest solution that meets requirements (YAGNI). Complexity must be justified and documented.

Rationale: Enables debuggability, clear upgrade paths, and prevents overengineering.

## Development Workflow, Review & Quality Gates
Development work MUST follow this flow:
- Write failing tests first (unit/contract/integration as appropriate).
- Add minimal implementation to make the tests pass.
- Refactor with tests passing.
- Submit a PR with:
  - Failing tests demonstrating the required behavior (in history or branch).
  - Passing CI tests (CI must run with mocks/precomputed artifacts).
  - A short migration or compatibility note if a change affects consumers.
- Code review: At least one approving reviewer is required for routine changes; governance amendments require additional approvals (see Governance).

Contributors MUST follow repository formatting and linting rules. Large or risky changes MUST include an ADR (Architecture Decision Record) and a migration/comms plan.

## Additional Constraints
- Technology Defaults: The canonical demo uses .NET (ASP.NET Core) for the API and Python FastAPI for a local embeddings service (`all-MiniLM-L6-v2` recommended). These defaults may be changed in a feature spec but must preserve the zero-cost CI constraints.
- CI Safety: All CI workflows MUST avoid network calls to paid services; tests MUST use `Mock` adapters or precomputed embeddings present in the repo for deterministic runs.
- Resource Guidance: Optional LLM usage MUST include explicit resource and license notices.

## Governance
- Amendment Procedure:
  - Amendments to this constitution MUST be proposed via a repository PR that:
    - States the proposed changes and rationale.
    - Includes a migration plan for any breaking governance changes.
    - Adds or updates tests or gating checks that enforce new mandatory requirements (e.g., TDD gating).
    - Identifies at least one maintainer sponsor.
  - For administrative/governance amendments, acceptance requires approval from at least two core maintainers or the approvers listed in `MAINTAINERS.md` (if present). If `MAINTAINERS.md` is absent, the PR must obtain at least two maintainers' approvals as recorded in PR reviewers.
  - For backcompat-breaking governance (redefinitions or removals of core principles) the change MUST be treated as MAJOR (see Versioning rules) and include a plan to migrate existing work and tooling.
- Versioning Policy (Constitution-level):
  - MAJOR version increase: Backward-incompatible principle removal or redefinition that affects contributor obligations or enforcement.
  - MINOR version increase: Addition of a new principle or material expansion of guidance (e.g., adding new mandatory workflow requirements).
  - PATCH version increase: Clarifications, wording fixes, or non-semantic refinements.
  - The constitution file MUST include the version, the ratification date (ISO YYYY-MM-DD), and last amended date (ISO YYYY-MM-DD).
- Compliance Reviews:
  - All plans (see `.specify/templates/plan-template.md`) MUST include an explicit "Constitution Check" that evaluates proposed work against these principles.
  - Any work that cannot be made compliant MUST be explicitly documented in the plan's Complexity Tracking and escalate to maintainers.

**Version**: v2.2.0 | **Ratified**: 2025-10-06 | **Last Amended**: 2025-10-06
