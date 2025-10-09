# Research: CI-safe embeddings & retrieval (Phase 0)

## Decision: Canonical selection for precomputed embeddings
- Chosen: Use canonical config key `Embedding:Mode` with values `Precomputed` or `Live`.
- CI enforcement: CI workflows MUST set `Embedding__Mode=Precomputed` (env var mapping for .NET config).

## Rationale
- Explicit configuration follows .NET conventions and is auditable in CI runs.
- Prevents accidental live embedding calls in automated environments (safety).
- Easy to override in tests and DI for WebApplicationFactory-based integration tests.
- Aligned with Constitution: local-first, CI-safe, and TDD principles.

## Precomputed artifacts
- Repository includes precomputed sample artifacts used for CI and tests at:
  - `/root/code/ai-rag/samples/sample1.embeddings.json` (primary sample)
  - `/root/code/ai-rag/samples/sample1.md` (document sample)
- Provider MUST validate file presence and vector dimensionality at startup.

## Embedding vector dimension
- Default chosen: 384 (compatible with many small sentence-transformer models).
- Validation: Precomputed provider validates vector length == 384 on load; fail-fast otherwise.

## Candidate approaches considered
- Env-var boolean (e.g., USE_PRECOMPUTED_EMBEDDINGS=true): Simple but less explicit and harder to audit across systems.
- Auto-detection by file presence: fragile (file may be present locally but not desired), ambiguous.
- CI-specific environment (ASPNETCORE_ENVIRONMENT=CI): convenient but brittle if other CI jobs run with different envs.
- Canonical config key (chosen): explicit, auditable, DI-friendly.

## Alternatives / deferred
- Add a `PrecomputedManifest` file to map queries->vectors for larger sample sets (deferred to Phase 2 if needed).
- Support multiple precomputed sample sets via `Embedding:PrecomputedPath` and a manifest (deferred).

## Constitution check (summary)
- TDD: Mandatory — plan requires creating failing tests for contracts before implementation.
- CI-Safety: Satisfied — `Embedding:Mode=Precomputed` enforcement in CI prevents external calls.
- Adapter-driven: Satisfied — strategy encourages implementing `IEmbeddingProvider` adapters.

## Output
- All NEEDS_CLARIFICATION items resolved for Phase 0 (precomputed mode selection).

