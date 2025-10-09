# Tasks: CI-safe embedding & retrieval (Phase 1)

**Input**: Design docs from `/root/code/ai-rag/specs/002-high-level-goal/`
**Prerequisites**: plan.md, research.md, data-model.md, contracts/ingest_openapi.yaml

## Execution Flow (applied)
- Setup → Tests (TDD) → Models → Services → Endpoints → Integration → Polish

## Phase 3.1: Setup
- T001 Setup .NET project structure and solution files (src/AiRag.Api)
  - Path: `/root/code/ai-rag/src/AiRag.Api/AiRag.Api.csproj`
  - Notes: Ensure solution `AiRag.sln` contains the API project; restore packages.

- T002 [P] Add development sample artifacts to samples/ and verify paths
  - Path: `/root/code/ai-rag/samples/sample1.embeddings.json`, `/root/code/ai-rag/samples/sample1.md`
  - Notes: Confirm sample files exist and vector length == 384.

- T003 Configure formatting & linting (dotnet format)
  - Path: repo root
  - Notes: Add any tooling steps to CI if missing.

## Phase 3.2: Tests First (TDD) ⚠️ MUST COMPLETE BEFORE IMPLEMENTATION
- T004 [P] Contract test: POST /ingest (failing) in `tests/contract/TestIngest.cs`
  - Path: `/root/code/ai-rag/tests/contract/TestIngest.cs`
  - Description: Create xUnit contract test that POSTs to `/ingest` per `specs/002-high-level-goal/contracts/ingest_openapi.yaml` and asserts 202 Accepted. Configure test host to use PrecomputedEmbeddingProvider (or mock) so it fails initially.

- T005 [P] Contract test: POST /query (failing) in `tests/contract/TestQuery.cs`
  - Path: `/root/code/ai-rag/tests/contract/TestQuery.cs`
  - Description: Create xUnit contract test that POSTs to `/query` with `text` and asserts response schema contains `results` array. Use PrecomputedEmbeddingProvider for deterministic output; write expected top result ids from sample file (test should fail before implementation).

- T006 [P] Integration test scenario: ingest -> index -> query in `tests/integration/TestIngestQuery.cs`
  - Path: `/root/code/ai-rag/tests/integration/TestIngestQuery.cs`
  - Description: Integration test following Quickstart scenario: POST sample document to `/ingest`, wait for indexing (or call synchronous endpoint), then POST to `/query` and assert top result includes the new document chunk. Use DI override to inject PrecomputedEmbeddingProvider or a deterministic mock.

## Phase 3.3: Core Implementation (ONLY after tests are failing)
- T007 [P] Create model: Document (src/AiRag.Api/Models/Document.cs)
  - Path: `/root/code/ai-rag/src/AiRag.Api/Models/Document.cs`
  - Description: Implement Document entity per data-model (UUID id, text, metadata, createdAt, chunks list).

- T008 [P] Create model: Chunk (src/AiRag.Api/Models/Chunk.cs)
  - Path: `/root/code/ai-rag/src/AiRag.Api/Models/Chunk.cs`
  - Description: Implement Chunk entity (UUID id, documentId, text, offsets, metadata).

- T009 [P] Create model: EmbeddingVector (src/AiRag.Api/Models/EmbeddingVector.cs)
  - Path: `/root/code/ai-rag/src/AiRag.Api/Models/EmbeddingVector.cs`
  - Description: Implement embedding vector model with vector float[] and dimension validation.

- T010 [P] Implement interface: IEmbeddingProvider (verify existing file) in `src/AiRag.Api/Adapters/IEmbeddingProvider.cs`
  - Path: `/root/code/ai-rag/src/AiRag.Api/Adapters/IEmbeddingProvider.cs`
  - Description: Ensure interface exists; otherwise create with `Task<float[]> GetEmbeddingAsync(string text)` signature and provider metadata.

- T011 [P] Implement PrecomputedEmbeddingProvider scaffold (src/AiRag.Api/Services/PrecomputedEmbeddingProvider.cs)
  - Path: `/root/code/ai-rag/src/AiRag.Api/Services/PrecomputedEmbeddingProvider.cs`
  - Description: Constructor loads `Embedding:PrecomputedPath` (default `samples/sample1.embeddings.json`), validates vector dimension==384, and exposes GetEmbeddingAsync that returns deterministic embeddings for known inputs. Must never perform network calls.

- T012 [P] Implement LiveEmbeddingProvider scaffold (src/AiRag.Api/Services/LiveEmbeddingProvider.cs)
  - Path: `/root/code/ai-rag/src/AiRag.Api/Services/LiveEmbeddingProvider.cs`
  - Description: Minimal implementation that calls local embedding HTTP service; include guard so it cannot be constructed in Precomputed mode.

- T013 [P] Implement InMemoryVectorStore (src/AiRag.Api/Services/InMemoryVectorStore.cs)
  - Path: `/root/code/ai-rag/src/AiRag.Api/Services/InMemoryVectorStore.cs`
  - Description: Provide Insert, Query (cosine similarity deterministic), Remove, Clear. Use stable sorting for tie-breaking.

- T014 [ ] Add EmbeddingOptions config model and register DI wiring (src/AiRag.Api/Models/EmbeddingOptions.cs + Program.cs changes)
  - Path: `/root/code/ai-rag/src/AiRag.Api/Models/EmbeddingOptions.cs`
  - Description: Add `Embedding:Mode`, `Embedding:PrecomputedPath`, `Embedding:Dimension` defaults and update DI registration to pick provider by Embedding:Mode.

- T015 Implement ingestion endpoint scaffold (src/AiRag.Api/Controllers/IngestController.cs) (no [P] if file shared)
  - Path: `/root/code/ai-rag/src/AiRag.Api/Controllers/IngestController.cs`
  - Description: POST /ingest accepts document payload, creates chunks, requests embeddings, stores vectors. Initially minimal to satisfy contract tests.

- T016 Implement query endpoint scaffold (src/AiRag.Api/Controllers/QueryController.cs)
  - Path: `/root/code/ai-rag/src/AiRag.Api/Controllers/QueryController.cs`
  - Description: POST /query returns top-K results. Use InMemoryVectorStore for lookup.

## Phase 3.4: Integration
- T017 [P] DI registration tests: verify Precomputed provider injected when `Embedding__Mode=Precomputed` (tests/di/TestDiConfig.cs)
  - Path: `/root/code/ai-rag/tests/di/TestDiConfig.cs`
  - Description: Unit test that builds ServiceCollection from Program configuration and asserts the IEmbeddingProvider type.

- T018 [ ] Startup fail-fast checks (Program.cs)
  - Path: `/root/code/ai-rag/src/AiRag.Api/Program.cs`
  - Description: If `ASPNETCORE_ENVIRONMENT==CI` and `Embedding:Mode!=Precomputed` then throw; also Precomputed provider validates file and dimension at startup.

- T019 [ ] Logging & request instrumentation (src/AiRag.Api/Services/RequestLoggingMiddleware.cs)
  - Path: `/root/code/ai-rag/src/AiRag.Api/Services/RequestLoggingMiddleware.cs`
  - Description: Ensure ingestion and query requests are logged with chunk ids and timings for observability.

## Phase 3.5: Polish
- T020 [P] Unit tests for vector similarity and deterministic ordering (tests/unit/TestVectorStore.cs)
  - Path: `/root/code/ai-rag/tests/unit/TestVectorStore.cs`
  - Description: Validate cosine similarity, tie-breakers, and deterministic top-K output.

- T021 [P] Update docs: quickstart.md and context/projects/ai-rag/quickstart.md
  - Path: `/root/code/ai-rag/specs/002-high-level-goal/quickstart.md` and `/root/code/ai-rag/context/projects/ai-rag/quickstart.md`
  - Description: Ensure instructions for Embedding__Mode env var and local embedding service startup are accurate.

- T022 [ ] Performance smoke test (manual) — ensure simple queries complete under target (deferred to Phase 5)
  - Path: repo root

## Dependencies & Ordering Notes
- Setup tasks (T001-T003) must run first.
- T004-T006 (contract/integration tests) must be authored and fail before implementing T007-T016.
- Model tasks (T007-T009) must be done before services (T011-T013).
- DI wiring (T014) must exist before starting endpoints (T015-T016).
- DI registration tests (T017) validate config mapping and can run in parallel with model implementation tasks.

## Parallel Execution Examples
- Parallel group A ([P]): T004, T005, T006 (contract & integration tests creation)
- Parallel group B ([P]): T007, T008, T009, T011, T012, T013 (models and provider scaffolds) — these touch distinct files and can proceed concurrently

## Task Execution Agent Commands (examples)
- Task agent command to create contract test file T004:
  - /task run --id T004 --file `/root/code/ai-rag/tests/contract/TestIngest.cs` --action "Create xUnit contract test for POST /ingest asserting 202"

- Task agent command to implement Precomputed provider T011:
  - /task run --id T011 --file `/root/code/ai-rag/src/AiRag.Api/Services/PrecomputedEmbeddingProvider.cs` --action "Implement provider that reads samples/sample1.embeddings.json and validates dimension=384"

## Validation Checklist (gates)
- [ ] All contract tests exist (T004,T005)
- [ ] All entity model tasks created (T007-T009)
- [ ] Tests written before implementation (T004-T006 must be committed before T015-T016 code merges)
- [ ] Each task lists exact file path
- [ ] No parallel tasks modify the same file

