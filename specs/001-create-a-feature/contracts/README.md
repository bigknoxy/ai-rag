# Contracts for Phase 0

This folder contains OpenAPI contracts and notes for contract tests.

Primary contract:
- `ingest_openapi.yaml` — defines `POST /api/ingest` accepting batch JSON `{ "documents": [...] }`.

Contract Test Guidance:
- Create a C# xUnit test at `tests/contract/TestIngest.cs` that POSTs to `/api/ingest` and asserts the request returns the expected response shape (or, for Phase 0 TDD, that the endpoint is missing and the test fails).
- Tests must be committed before implementation to satisfy TDD constitutional gates.

