# Tasks: AI-RAG Phase 0 — Scaffolding & Ingest

**Input**: Design documents from `/root/code/ai-rag/specs/001-create-a-feature/`
**Prerequisites**: `plan.md` (required), `research.md`, `data-model.md`, `contracts/`, `quickstart.md`

## Execution Flow (main)
```
1. Load plan.md from feature directory
   → If not found: ERROR "No implementation plan found"
   → Extract: tech stack, libraries, structure
2. Load optional design documents:
   → data-model.md: Extract entities → model tasks
   → contracts/: Each file → contract test task
   → research.md: Extract decisions → setup tasks
3. Generate tasks by category:
   → Setup: project init, dependencies, linting
   → Tests: contract tests, integration tests
   → Core: models, services, CLI commands, endpoints
   → Integration: DB, middleware, logging
   → Polish: unit tests, performance, docs
4. Apply task rules:
   → Different files = mark [P] for parallel
   → Same file = sequential (no [P])
   → Tests before implementation (TDD)
5. Number tasks sequentially (T001, T002...)
6. Generate dependency graph
7. Create parallel execution examples
8. Validate task completeness:
   → All contracts have tests?
   → All entities have models?
   → All endpoints implemented?
9. Return: SUCCESS (tasks ready for execution)
```

## Format: `[ID] [P?] Description`
- **[P]**: Can run in parallel (different files, no dependencies)
- Includes exact file paths in descriptions

## Phase 3.1: Setup
- [X] T001 Initialize .NET 8 solution skeleton per `plan.md`
  - Create solution and Web API project under `src/`.
  - Files/paths to create: `src/AiRag.sln`, `src/AiRag.Api/AiRag.Api.csproj`, `src/AiRag.Api/Program.cs` (file-scoped namespace), `src/AiRag.Api/Controllers/HealthController.cs`.
  - Dependency notes: `Microsoft.AspNetCore.App` (implicit), `xunit` for tests (dev dependency).
  - Dependency: none

- [X] T002 Create `embeddings` Python package skeleton
  - Files/paths: `embeddings/__init__.py`, `embeddings/service.py` (FastAPI app stub), `embeddings/requirements.txt` (include `fastapi`, `uvicorn`, `pytest`), `embeddings/__main__.py` to support `python -m embeddings.service` entry.
  - Dependency: T001

- [X] T003 [P] Add repository samples and precomputed artifacts
  - Files/paths: `samples/sample1.md`, `samples/sample1.embeddings.json` (precomputed 384-d vectors), `samples/vectors.json` (file-backed vector store sample). Populate with placeholder content matching `data-model.md` schema.
  - Dependency: none

- [X] T004 Configure linting and formatting
  - Files/paths: `.editorconfig` (if missing), add `dotnet format` guidance to `README.md` or `quickstart.md`.
  - Dependency: T001

## Phase 3.2: Tests First (TDD) ⚠️ MUST COMPLETE BEFORE IMPLEMENTATION
**MANDATORY:** Tests MUST be authored and committed before implementation. These tests MUST fail initially on CI using precomputed artifacts/mocks.

- [X] T005 [P] Contract test: POST `/api/ingest` in `tests/contract/TestIngest.cs`
  - Path: `tests/contract/TestIngest.cs`
  - Test specifics: Use xUnit. Send `POST /api/ingest` with body `{ "documents": [ { "id": "doc-1", "text": "Hello world", "metadata": {} } ] }`. Assert response status `200` and JSON `{ "ingested": 1 }` or similar per `contracts/ingest_openapi.yaml`.
  - Notes: Use `WebApplicationFactory` or minimal test host to run API; in CI this test will run against precomputed samples and should fail until implementation exists.
  - Dependency: T001

- [X] T006 [P] Embeddings service contract test: embeddings generation in `embeddings/tests/test_embeddings_service.py`
  - Path: `embeddings/tests/test_embeddings_service.py`
  - Test specifics: Call `embeddings.service` (or mock) to request embedding for `"Hello world"` and assert returned vector length is 384 and numeric.
  - Notes: In CI this may use `samples/sample1.embeddings.json` or a mock; test should be written to fail until embedding endpoint stub exists.
  - Dependency: T002

## Phase 3.3: Core Implementation (ONLY after tests are failing)
- [X] T007 Create `Document` and `Chunk` models per `data-model.md`
  - Files/paths: `src/AiRag.Api/Models/Document.cs`, `src/AiRag.Api/Models/Chunk.cs`, `src/AiRag.Api/Models/EmbeddingRecord.cs`
  - Model specifics: Match fields in `data-model.md` with validation attributes (e.g., `[Required]` on `text`). Ensure `EmbeddingRecord.vector` enforces length 384 at runtime (validation helper).
  - Marked [P] because each model file is separate and can be implemented in parallel with other model files.
  - Dependency: T005, T006

- [X] T008 Create adapter interfaces and DI registration
  - Files/paths: `src/AiRag.Api/Adapters/IEmbeddingProvider.cs`, `src/AiRag.Api/Adapters/IVectorStore.cs`, `src/AiRag.Api/Adapters/ILLMAdapter.cs`, `src/AiRag.Api/Services/ServiceCollectionExtensions.cs`
  - Task: Define small interfaces with required methods (e.g., `Task<float[]> GetEmbeddingAsync(string text)`), and register default file-backed implementations in `Program.cs` for Phase 0.
  - Dependency: T007

- [X] T009 Implement file-backed VectorStore for Phase 0
  - Files/paths: `src/AiRag.Api/Services/FileVectorStore.cs`
  - Behavior: Read/write to `samples/vectors.json`, provide `SaveAsync(EmbeddingRecord)` and `SearchAsync(queryVector, topK)` stub methods.
  - Dependency: T008

- [X] T010 Implement minimal EmbeddingProvider that reads precomputed embeddings
  - Files/paths: `src/AiRag.Api/Services/PrecomputedEmbeddingProvider.cs` or `embeddings/service.py` implementation if calling local FastAPI
  - Behavior: For Phase 0, look up text in `samples/sample1.embeddings.json` or call local embeddings service if available. Return 384-d float array.
  - Dependency: T006, T003

- [X] T011 Implement POST `/api/ingest` endpoint per contract
  - Files/paths: `src/AiRag.Api/Controllers/IngestController.cs`
  - Behavior: Accept batch, validate `text` required, create Document/Chunk/EmbeddingRecord objects, use `IEmbeddingProvider` to get vectors, persist via `IVectorStore`, return `{ "ingested": <count> }`.
  - Notes: Keep implementation minimal to satisfy tests; use async/await and proper error handling.
  - Dependency: T007, T009, T010

## Phase 3.4: Integration
- [X] T012 Connect API to file-backed vector store and ensure thread-safety
  - Files/paths: `src/AiRag.Api/Services/FileVectorStore.cs` (ensure locking) and `src/AiRag.Api/Program.cs` DI registrations
  - Dependency: T009, T011
  - Note: Implemented locking in `FileVectorStore` and verified via unit test in `tests/unit/TestFileVectorStore.cs`.

- [X] T013 Add logging and request/response tracing
  - Files/paths: `src/AiRag.Api/Program.cs`, `src/AiRag.Api/Middleware/RequestLoggingMiddleware.cs`
  - Dependency: T011
  - Note: Added `RequestLoggingMiddleware` in `src/AiRag.Api/Services/RequestLoggingMiddleware.cs` and unit test `tests/unit/TestRequestLoggingMiddleware.cs`.

- [X] T014 Add healthcheck endpoint and update quickstart
  - Files/paths: `src/AiRag.Api/Controllers/HealthController.cs`, update `/specs/001-create-a-feature/quickstart.md` with verification steps
  - Dependency: T001
  - Note: `HealthController` exists and quickstart updated to include verification. 

## Phase 3.5: Polish
- [X] T015 [P] Unit tests for models and validation in `tests/unit/TestModels.cs`
  - Path: `tests/unit/TestModels.cs`
  - Dependency: T007

- [X] T016 [P] Unit tests for FileVectorStore in `tests/unit/TestFileVectorStore.cs`
  - Path: `tests/unit/TestFileVectorStore.cs`
  - Dependency: T009

- [X] T017 [P] Update documentation: `specs/001-create-a-feature/README.md` and API docs
  - Path: `specs/001-create-a-feature/README.md`, `docs/api.md`
  - Dependency: all core tasks

- [X] T018 Performance sanity test (local): `scripts/perf/run_sanity.sh`
  - Path: `scripts/perf/run_sanity.sh` (create script that runs a sample ingest of 100 docs and measures time)
  - Dependency: T011, T009

## Dependencies (summary)
- Setup (T001-T004) before Tests (T005-T006)
- Tests (T005-T006) must be present and failing before Core Implementation (T007-T011)
- Models (T007) before Adapters/DI (T008)
- Adapter/DI (T008) before VectorStore (T009) and EmbeddingProvider (T010)
- VectorStore + EmbeddingProvider before Ingest endpoint (T011)
- Integration tasks (T012-T014) after core endpoint implementation
- Polish (T015-T018) last

## Parallel Execution Examples
```
# Example A: Run independent test tasks in parallel (safe)
run_task_agent --task "T005: tests/contract/TestIngest.cs"
run_task_agent --task "T006: embeddings/tests/test_embeddings_service.py"

# Example B: Run model creation tasks in parallel (each file independent)
run_task_agent --task "T007.1: src/AiRag.Api/Models/Document.cs"
run_task_agent --task "T007.2: src/AiRag.Api/Models/Chunk.cs"
run_task_agent --task "T007.3: src/AiRag.Api/Models/EmbeddingRecord.cs"

# Example C: Sequential execution (same file) - do not parallelize
run_task_agent --task "T011: src/AiRag.Api/Controllers/IngestController.cs"
# Once complete, run dependent tasks T012 and T013
```

## Validation Checklist
- [x] `contracts/ingest_openapi.yaml` → T005 contract test created
- [x] `data-model.md` entities → T007 model tasks created
- [x] `research.md` decisions → setup tasks T001-T004 included
- [x] `quickstart.md` scenarios → healthcheck & verification tasks (T014) included

---
Generated: /root/code/ai-rag/specs/001-create-a-feature/tasks.md
