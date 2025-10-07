# Quickstart: Phase 0 — Local Verification

Path: /root/code/ai-rag/specs/001-create-a-feature/quickstart.md

Prerequisites
- .NET 8 SDK installed (`dotnet --version` should report `8.x`)
- Python 3.11 (optional for local embeddings service; CI uses precomputed embeddings)
- Optional: `pip` and virtualenv for Python tests

Local verification steps
1. Start the embeddings service (optional locally):
   - `python -m embeddings.service --host 127.0.0.1 --port 8001`
2. Build the .NET solution:
   - `dotnet build src/AiRag.Api`
3. Run tests:
   - `dotnet test`
   - To run a single test: `dotnet test --filter "DisplayName=TestName"`

Local verification steps (detailed)
1. Ensure `samples/sample1.embeddings.json` exists and contains a `sample-1` key mapped to a 384-length float array. If missing, create a zeros vector file:
   - `python - <<'PY'\nimport json\nvec=[0.0]*384\nopen('samples/sample1.embeddings.json','w').write(json.dumps({'sample-1':vec}))\nPY`
2. Start the API for manual testing:
   - `dotnet run --project src/AiRag.Api`
3. Healthcheck:
   - `curl -s http://localhost:5000/api/health` should return `{ "status": "healthy" }`.
4. Ingest:
   - `curl -s -X POST http://localhost:5000/api/ingest -H "Content-Type: application/json" -d '{ "documents": [ { "id": "doc-1", "text": "Hello world", "metadata": {} } ] }'`
   - Expect: JSON with `"ingested": 1` and `samples/vectors.json` updated.

CI notes
- CI must run with precomputed embeddings available in `/root/code/ai-rag/samples/` and must not call external APIs.
- Ensure `samples/vectors.json` is present in the repository for deterministic retrieval tests.

