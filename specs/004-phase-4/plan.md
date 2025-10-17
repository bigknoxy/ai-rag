# Phase 4 Plan: UI, Tests, CI, Docs

## Tasks
1. **ADR Documentation**: Create ADRs for embeddings model (sentence-transformers), vector store (InMemory/Faiss), and local-first rationale.
2. **Blazor Demo UI Setup**: Add Blazor Server project to src/ with pages for ingest, query, and streaming.
3. **UI Components & Styling**: Implement responsive components with basic styling.
4. **Enhanced Unit Tests**: Add tests for UI components.
5. **Integration & E2E Tests**: Expand tests and add Playwright E2E.
6. **CI/CD Enhancements**: Update workflows for Docker builds and coverage.
7. **Docker Setup**: Add Dockerfiles for API and embeddings.
8. **Documentation Updates**: Update README and quickstart with UI/Docker info.
9. **Final Review**: Run tests, lint, build.

## Timeline
2-4 days, iterative with TDD.

## Constraints
- Use Playwright/chrome-devtools for UI testing.
- No secrets in CI.
- Local-first focus.