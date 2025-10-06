# Feature Specification: Phase 0 — Scaffolding & Local-First Research

**Feature Branch**: `001-create-a-feature`  
**Created**: 2025-10-06  
**Status**: Draft  
**Input**: User description: "Create a feature specification for \"Phase 0 — Scaffolding & Local-First Research\" for the AI-RAG repo..."

## Execution Flow (main)
```
1. Parse user description from Input
   → If empty: ERROR "No feature description provided"
2. Extract key concepts from description
   → Identify: actors, actions, data, constraints
3. For each unclear aspect:
   → Mark with [NEEDS CLARIFICATION: specific question]
4. Fill User Scenarios & Testing section
   → If no clear user flow: ERROR "Cannot determine user scenarios"
5. Generate Functional Requirements
   → Each requirement must be testable
   → Mark ambiguous requirements
6. Identify Key Entities (if data involved)
7. Run Review Checklist
   → If any [NEEDS CLARIFICATION]: WARN "Spec has uncertainties"
   → If implementation details found: ERROR "Remove tech details"
8. Return: SUCCESS (spec ready for planning)
```

---

## ⚡ Quick Guidelines
- ✅ Focus on WHAT this phase delivers and WHY
- ❌ Avoid unnecessary implementation decisions beyond skeletal choices required for scaffolding
- 👥 Written for developers preparing to run `/plan` and generate tasks

### Section Requirements
- **Mandatory sections**: Must be completed for Phase 0 and must result in at least one failing, automated testable acceptance scenario.
- **Optional sections**: Include only when relevant to Phase 0.

Note: Requirements MUST be written to be testable and the plan MUST include the tests-first step; unresolved [NEEDS CLARIFICATION] items block implementation until resolved.

### For AI Generation
1. **Mark all ambiguities**: Use [NEEDS CLARIFICATION: specific question] for any assumption you'd need to make
2. **Don't guess**: If the prompt doesn't specify something, mark it
3. **Think like a tester**: Every vague requirement should fail the "testable and unambiguous" checklist item

---

## User Scenarios & Testing *(mandatory)*

### Primary User Story
As a developer, I want a minimal, zero-cost scaffolding for AI-RAG so I can run the project locally, author tests first, and iterate on ingestion and retrieval functionality without requiring paid services.

### Acceptance Scenarios
1. **Given** a fresh checkout of the repository, **When** I run the commands to start the embeddings service and build the solution, **Then** the repository contains the required folders (`src/`, `tests/`, `embeddings/`, `samples/`) and the `embeddings` service starts with the expected command.
2. **Given** the pre-initialized test files in `tests/contract/` and `embeddings/tests/`, **When** CI runs, **Then** the tests fail initially (red) because they assert endpoints or behaviors that are not yet implemented.

### Edge Cases
- What happens if the developer's machine lacks Python or .NET SDK? => Document clear prerequisites and commands to verify environment.
- What if the embeddings model download fails in local mode? => Document cache pre-download and advise using precomputed embeddings for demo.

## Requirements *(mandatory)*

### Functional Requirements
- **FR-001**: Repository MUST include a .NET solution skeleton in `src/` and test directories `tests/unit/`, `tests/contract/`, `tests/integration/`.
- **FR-002**: Repository MUST include an `embeddings/` folder with a Python FastAPI skeleton and `requirements.txt` that includes `sentence-transformers`.
- **FR-003**: `embeddings` service MUST be runnable via `python -m embeddings.service --host 127.0.0.1 --port 8001` (document this in README).
- **FR-004**: Repository MUST include `samples/sample1.md` and `samples/sample1.embeddings.json` (precomputed embeddings) used by CI.
- **FR-005**: Provide at least two failing automated tests committed before implementation:
  - **TST-001**: Contract/integration test asserting `POST /api/ingest` endpoint exists (path: `tests/contract/test_ingest.py` or `tests/contract/TestIngest.cs`).
  - **TST-002**: Test asserting `POST /embeddings` on the embeddings service returns an embeddings array shape (path: `embeddings/tests/test_embeddings_service.py`).
- **FR-006**: All tests must run in CI without network calls (use mocks or precomputed embeddings available in `samples/`).

### Key Entities
- **Document**: uploaded doc for ingestion (metadata: id, source, text)
- **Chunk**: chunked passage with metadata and embedding vector
- **EmbeddingRecord**: storage representation of vector and source link

---

## Constitution Check
- **TDD (mandatory)**: PASS — Phase 0 requires committed failing tests (TST-001, TST-002) before implementation.
- **CI-Safety**: PASS — Tests must run using precomputed embeddings and mocks; CI must not call external APIs.
- **Local-First**: PASS — Default runtime is retrieval-only; embeddings run locally via Python FastAPI.
- **Adapter/DI requirement**: PASS — Scaffolding must include interface stubs for EmbeddingProvider, VectorStore, LLMAdapter (Mock), and DI registration placeholders.

If any gate cannot be satisfied (e.g., environment lacks Python/.NET), the mitigation is to provide documentation and precomputed artifacts; list is below.

## Deliverables (explicit file list and paths)
- `specs/phase-0/spec.md` (this file)
- `src/` (solution skeleton placeholder)
- `tests/unit/` (placeholder failing tests)
- `tests/contract/` (placeholder failing tests e.g., `tests/contract/test_ingest.py`)
- `tests/integration/` (placeholder failing tests)
- `embeddings/`
  - `embeddings/service.py` (FastAPI skeleton)
  - `embeddings/requirements.txt` (includes `sentence-transformers`)
  - `embeddings/tests/test_embeddings_service.py` (failing test)
- `samples/sample1.md`
- `samples/sample1.embeddings.json` (precomputed embeddings for CI)
- `README.phase-0.md` (notes for running local demo and CI requirements)

---

## Phase 0 Ordered Plan (estimated effort: 1 day)
1. Create solution & project skeleton (2-3 hours)
   - `src/` solution and placeholder projects, DI registration stubs, adapter interfaces
2. Add `embeddings/` Python FastAPI skeleton + `requirements.txt` (1 hour)
   - Provide `embeddings/service.py` FastAPI app with `/embeddings` POST stub
3. Add `samples/` with `sample1.md` and `sample1.embeddings.json` precomputed (30 minutes)
4. Add placeholder failing tests (1 hour)
   - `tests/contract/test_ingest.py` asserts `POST /api/ingest` exists
   - `embeddings/tests/test_embeddings_service.py` asserts `/embeddings` returns array shape
5. Add README.phase-0.md with local verification commands and CI notes (30 minutes)

## Acceptance Test Cases (Gherkin)

Scenario: Embeddings service endpoint returns embeddings shape
  Given the embeddings FastAPI service is started with `python -m embeddings.service --host 127.0.0.1 --port 8001`
  When a client POSTs text to `/embeddings` with payload `{ "texts": ["hello"] }`
  Then the service responds with a JSON array `[[float, ...]]` where the inner array length equals the embedding dimension (or >0)

Scenario: Ingest endpoint exists (contract)
  Given the API server is not yet implemented
  When CI runs the contract test for `POST /api/ingest`
  Then the test fails (red) asserting the endpoint is missing — satisfying the tests-first requirement for Phase 0

## Exact Commands for Local Verification
- `python -m embeddings.service --host 127.0.0.1 --port 8001`
- `dotnet build`
- `dotnet test --filter "DisplayName=TestName"`

## Non-functional Notes
- Resource guidance: default components assume CPU-only execution on a modest machine (~8GB RAM). The `sentence-transformers` model recommended (`all-MiniLM-L6-v2`) is ~80–100MB and CPU friendly.
- CI: Tests must run with precomputed embeddings (in `samples/`) and mocks. CI workflows must avoid network calls and paid services.

## Open Questions / [NEEDS CLARIFICATION]
- [NEEDS CLARIFICATION: Preferred language for placeholder contract tests — Python (pytest) or C# (xUnit)? The repo uses .NET for API and Python for embeddings; recommend: contract tests in the primary API language (C# xUnit) and embeddings tests in Python pytest.]
- [NEEDS CLARIFICATION: Desired embedding vector dimensionality for precomputed sample? Recommend 384 (typical for all-MiniLM-L6-v2).]

---

## Execution Status
- [x] User description parsed
- [x] Key concepts extracted
- [x] Ambiguities marked
- [x] User scenarios defined
- [x] Requirements generated
- [x] Entities identified
- [ ] Review checklist passed (to be completed before /plan)

---

*Generated by /specify via create-new-feature script — Branch: `001-create-a-feature`*
