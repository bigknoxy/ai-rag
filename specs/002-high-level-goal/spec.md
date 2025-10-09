# Feature Specification: CI-safe embedding & retrieval stack (Phase 1)

**Feature Branch**: `002-high-level-goal`
**Created**: 2025-10-07
**Status**: Draft
**Input**: User description: "High-level goal: Implement a CI-safe embedding and retrieval stack that uses precomputed embeddings during tests while keeping code paths ready for a live embeddings service in development. - read @context/projects/ai-rag/plan.md and the rest of @context/projects/ai-rag/ to implement phase 1"

## Clarifications

### Session 2025-10-07
- Q: Which method should be used to select precomputed mode in CI? → A: D (Require explicit config key, e.g., `Embedding:Mode=Precomputed`)

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
- ✅ Focus on WHAT users need and WHY
- ❌ Avoid low-level HOW where possible, but include necessary operational constraints (CI-safety)
- 👥 Written for stakeholders and developer teams responsible for delivery

### Section Requirements
- **Mandatory sections**: Completed below. Each acceptance scenario is testable and automatable.

---

## User Scenarios & Testing *(mandatory)*

### Primary User Story
As a repository maintainer or developer, I want the system's embedding and retrieval stack to be fully testable in CI without network access or secrets, using precomputed embeddings so that unit and integration tests run deterministically; in local development I want the same code paths to support a live, local embeddings service for ingestion and reindexing.

### Acceptance Scenarios
1. **Given** CI or test environment with only repository files available, **When** the test suite runs, **Then** the embedding provider MUST return precomputed embeddings sourced from repository sample files and all embedding-dependent tests MUST pass without network access.

2. **Given** a developer running locally with the optional local embeddings service started, **When** the ingestion endpoint is called with a new document, **Then** the system MUST obtain embeddings from the local service and populate the vector store so subsequent queries can retrieve the document.

3. **Given** a populated vector store using precomputed embeddings (CI) or live embeddings (dev), **When** a client POSTs a query to the `/query` endpoint in tests, **Then** the response MUST include top-K ranked chunks with stable scores and source metadata; acceptance tests assert expected top result ids for the sample queries.

4. **Given** malformed or missing precomputed embedding files in CI, **When** tests run, **Then** the test setup MUST fail fast with a clear error indicating the missing or invalid sample artifacts (failing CI), not by attempting external network calls.

### Edge Cases
- What happens when sample precomputed embeddings exist but vector dimensionality differs from the runtime model? → Tests should fail with clear validation error.
- How does the system behave when the local embeddings service is unreachable in dev? → Ingest/Live paths must return a deterministic error and not fallback to network calls.
- Empty query text or very short queries → Query endpoint should return empty results or a clear validation error depending on policy.

## Requirements *(mandatory)*

### Functional Requirements
- **FR-001**: System MUST provide a deterministic, precomputed-embeddings mode that is used during tests and CI so that no external network calls are performed.
- **FR-002**: System MUST provide a local-first, optional live-embeddings mode for developer workflows where a local embedding service can supply vectors.
- **FR-003**: System MUST expose an ingestion API that accepts documents, produces/stores embeddings, and supports both precomputed and live embedding providers (precomputed used by CI/tests; live used in dev when enabled).
- **FR-004**: System MUST include an in-memory or file-based vector store usable in tests with deterministic retrieval semantics (stable top-K ordering for given inputs).
- **FR-005**: System MUST expose a `/query` endpoint (retrieval-only for Phase 1) that returns top-K chunks with scores and source metadata; behavior MUST be consistent between precomputed and live modes.
- **FR-006**: Tests and CI MUST run without any external network access or secrets; all artifacts required for tests (sample documents and precomputed embeddings) MUST be included in the repository and versioned.
- **FR-007**: The system MUST validate precomputed embeddings at startup or test fixture setup (check dimension, count, and ids) and fail early on mismatch.
- **FR-008**: Configuration toggles MUST allow selecting precomputed-vs-live embedding provider via the canonical config key `Embedding:Mode` (values: `Precomputed` | `Live`); CI MUST set `Embedding:Mode=Precomputed`.
- **FR-009**: Acceptance tests MUST include at least one failing automated test scenario until implementation is complete (tests-first approach).

### Non-Functional Requirements
- **NFR-001**: CI runs MUST complete in reasonable time (goal: < 3 minutes for unit/integration subset); using precomputed vectors supports this.
- **NFR-002**: Test determinism: Given the same sample files, tests MUST be deterministic across runs and platforms.
- **NFR-003**: No secrets or external service credentials are stored in the repository.

### Key Entities
- **Document**: Represents an input document with attributes `id`, `text`, `metadata` (author, source, url), and `chunks` (derived passages).
- **Chunk**: A passage derived from a Document with attributes `id`, `documentId`, `text`, `startOffset`, `endOffset`, and `metadata`.
- **EmbeddingVector**: Represents an embedding with attributes `id` (chunk or document id), numeric `vector` values, and `dimension`.
- **VectorStore**: Logical store of EmbeddingVectors supporting top-K similarity lookup and insertion.
- **Query**: Represents a user's query with `text`, optional `topK` (default 5), and returned `results` list with `chunkId`, `score`, and `metadata`.

---

## Review & Acceptance Checklist

### Content Quality
- [x] No low-level implementation details required by stakeholders (operational constraints are included).
- [x] Focused on user value and CI-safety needs.
- [x] Written for both stakeholders and developers to act on.
- [x] All mandatory sections completed.

### Requirement Completeness
- [ ] No [NEEDS CLARIFICATION] markers remain.
- [x] Requirements are testable and unambiguous for the majority of scope.
- [x] Success criteria are measurable (precomputed mode, deterministic top-K, CI-only tests).
- [x] Scope is bounded to Phase 1 (embeddings provider, vector store, ingestion, query endpoint).
- [x] Dependencies and assumptions identified (sample artifacts present in repo, dev can run local embedding service).

---

## Execution Status
- [x] User description parsed
- [x] Key concepts extracted
- [x] Ambiguities marked when necessary
- [x] User scenarios defined
- [x] Requirements generated
- [x] Entities identified
- [ ] Review checklist passed (final validation required by owner)

---

Notes and Next Steps
- Phase 1 implementation tasks (implementation-ready): implement `LocalEmbeddingProvider`, `PrecomputedEmbeddingProvider` usage in tests, `InMemoryVectorStore`, ingestion endpoint + background worker, `/query` retrieval endpoint, and tests that inject precomputed embeddings via DI fixtures.
- CI must set the canonical config key to select precomputed-embeddings mode (example: `Embedding:Mode=Precomputed` via env `Embedding__Mode=Precomputed`) and run `dotnet test` with no network access.
- Documentation: Update `context/projects/ai-rag/quickstart.md` with local dev steps to run the embeddings service and how to switch modes.

