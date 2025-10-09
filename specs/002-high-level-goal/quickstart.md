# Quickstart: AI-RAG — CI-safe embeddings & retrieval

This quickstart shows how to run the AI-RAG demo locally (Live mode) and in CI (Precomputed mode), with formatting and test checks.

## Prerequisites
- .NET SDK 8 (or as specified by repo)
- Python 3.8+ and `pip` (for local embedding service)
- Git

## Local Development (Live Embeddings)
1. (Optional) Start local embeddings service:
   - `python -m embeddings.service --host 127.0.0.1 --port 8001`
2. Set config to live mode:
   - `export Embedding__Mode=Live`
3. Run the API locally:
   - `dotnet run --project src/AiRag.Api`
4. Ingest sample documents:
   - `curl -X POST "http://localhost:5000/api/ingest" -F "file=@samples/sample1.md" -H "x-api-key: demo-key"`
5. Query the corpus:
    - `curl -X POST "http://localhost:5000/api/query" -H "Content-Type: application/json" -H "x-api-key: demo-key" -d '{"text":"What is retrieval-augmented generation?","topK":3}'`
    - Response includes `results` array and `assembled` passages for easy reading.

## CI / Tests (Precomputed Embeddings)
1. Set environment:
   - `export Embedding__Mode=Precomputed`
   - `export ASPNETCORE_ENVIRONMENT=CI`
2. Run formatting check:
   - `dotnet tool restore`
   - `dotnet format --verify-no-changes`
3. Run tests:
   - `dotnet test`

> CI workflow runs these steps automatically. See `.github/workflows/format-and-tests.yml`.

## Formatting
- To check and fix formatting locally:
  - `dotnet tool restore`
  - `dotnet format`
- To verify formatting only (CI):
  - `dotnet format --verify-no-changes`
- See `scripts/dotnet-format.md` for details.

## Developer Notes
- Tests must use precomputed embeddings in CI. No external network calls allowed.
- For Live embedding tests, override DI or set `Embedding__Mode=Live` and run the local service.
- Follow TDD: write failing contract/integration tests before implementing endpoints.

---
