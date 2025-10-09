# AI-RAG: Open-Source Retrieval-Augmented Generation

[![CI](https://github.com/bigknoxy/ai-rag/workflows/Format%20and%20Test/badge.svg)](https://github.com/bigknoxy/ai-rag/actions)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

AI-RAG is a modular, open-source Retrieval-Augmented Generation (RAG) service built with ASP.NET Core and local tooling. Designed for zero-cost, CPU-friendly operation on modest hardware (~8GB RAM), it demonstrates production-ready backend engineering with a focus on testability, CI-safety, and extensibility.

## 🚀 Features

- **Local-First Embeddings**: Uses a small SentenceTransformers model via Python FastAPI (no external APIs required).
- **Pluggable Architecture**: Modular adapters for embeddings, vector stores, and LLMs.
- **Retrieval-Only Default**: Assembles answers from passages without LLM calls for lightweight operation.
- **Optional Local LLM**: Integrate Ollama or llama.cpp for full RAG with streaming responses.
- **CI-Safe Testing**: All tests use mocks and precomputed artifacts; no network dependencies.
- **Streaming Support**: Real-time token streaming for LLM responses.
- **Configurable Modes**: Switch between Precomputed (CI), Live (dev), and LLM-enabled modes via config.
- **Comprehensive Testing**: Unit, integration, and contract tests ensure reliability.

## 📋 Table of Contents

- [Quick Start](#quick-start)
- [Architecture](#architecture)
- [API Reference](#api-reference)
- [Configuration](#configuration)
- [Development](#development)
- [Contributing](#contributing)
- [License](#license)

## 🚀 Quick Start

### Prerequisites
- .NET 8 SDK
- Python 3.8+ (for embeddings service)
- Git

### 1. Clone and Setup
```bash
git clone https://github.com/bigknoxy/ai-rag.git
cd ai-rag
```

### 2. Install Dependencies
```bash
# .NET dependencies
dotnet restore

# Python dependencies for embeddings
pip install -r context/projects/ai-rag/embeddings/requirements.txt
```

### 3. Start Services
```bash
# Terminal 1: Embeddings service (Python)
python -m embeddings.service --host 127.0.0.1 --port 8001

# Terminal 2: API server (.NET)
dotnet run --project src/AiRag.Api
```

### 4. Ingest and Query
```bash
# Ingest sample document
curl -X POST "http://localhost:5000/api/ingest" \
  -F "file=@samples/sample1.md" \
  -H "x-api-key: demo-key"

# Query in retrieval-only mode
curl -X POST "http://localhost:5000/api/query" \
  -H "Content-Type: application/json" \
  -d '{"text":"What is retrieval-augmented generation?","topK":3}'

# Query with optional LLM (if enabled)
curl -X POST "http://localhost:5000/api/query" \
  -H "Content-Type: application/json" \
  -d '{"text":"Explain RAG","useLlm":true}'
```

For detailed setup including LLM integration, see [specs/002-high-level-goal/quickstart.md](specs/002-high-level-goal/quickstart.md) or [specs/003-local-llm-integration/quickstart.md](specs/003-local-llm-integration/quickstart.md).

## 🏗️ Architecture

AI-RAG follows a modular adapter pattern for flexibility:

- **Embedding Provider**: Local SentenceTransformers via Python service.
- **Vector Store**: In-memory or FAISS for similarity search.
- **LLM Adapter**: Optional Ollama/llama.cpp with Mock for CI.
- **Prompt Builder**: Assembles context for retrieval or LLM generation.
- **Controllers**: REST endpoints for ingestion and querying.

See [specs/002-high-level-goal/](specs/002-high-level-goal/) for architecture details and diagrams.

## 📖 API Reference

### Endpoints

#### Ingest
- **POST** `/api/ingest` - Upload documents for embedding and storage.
- Body: Multipart form with `file` (markdown/text).

#### Query
- **POST** `/api/query` - Retrieve and optionally generate responses.
- Body: `{"text": "query", "topK": 5, "useLlm": false}`
- Response: `{"results": [...], "response": "...", "llmUsed": false}`

- **POST** `/api/query/stream` - Streaming version for real-time responses.

#### Health
- **GET** `/health` - Service health check.

All endpoints support optional `x-api-key` header (set to `demo-key` for local testing).

## ⚙️ Configuration

Configure via `appsettings.json` or environment variables:

```json
{
  "Embedding": {
    "Mode": "Precomputed", // Precomputed | Live
    "QueryTopK": 5
  },
  "LLM": {
    "Enabled": false, // true for RAG mode
    "Provider": "Mock", // Mock | Ollama | LlamaCpp
    "Model": "llama2:7b",
    "BaseUrl": "http://localhost:11434"
  }
}
```

Environment overrides: `Embedding__Mode=Live`, `LLM__Enabled=true`.

## 🛠️ Development

### Running Tests
```bash
dotnet test # All tests (unit, integration, contract)
```

### Code Quality
```bash
dotnet format # Format code
dotnet format --verify-no-changes # CI check
```

### Project Structure
```
src/AiRag.Api/          # Main API project
  ├── Adapters/         # Pluggable providers (embeddings, LLM, vector store)
  ├── Controllers/      # REST endpoints
  ├── Models/           # Data models
  ├── Services/         # Business logic and adapters
tests/                  # Test suites
  ├── unit/             # Unit tests
  ├── integration/      # Integration tests
  ├── contract/         # Contract tests
 specs/                  # Feature specifications and plans
   ├── 001-create-a-feature/
   ├── 002-high-level-goal/
   ├── 003-local-llm-integration/
```

### Adding Features
Follow the developer flow (see [specs/001-create-a-feature/](specs/001-create-a-feature/) for details):
1. Create spec in `specs/###-feature-name/`
2. Plan with `/plan` command
3. Implement with TDD
4. Test, lint, and PR

## 🤝 Contributing

Contributions welcome! Please follow these guidelines:

1. **Read the Constitution**: See `.specify/memory/constitution.md` for governance rules.
2. **TDD First**: Write failing tests before implementation.
3. **CI-Safe**: No external APIs in tests; use mocks/precomputed data.
4. **PR Process**: Create branch, implement, test, then open PR with description.
5. **Code Style**: Follow `.editorconfig` and use `dotnet format`.

See [CONTRIBUTING.md](CONTRIBUTING.md) for details (create if needed).

## 📜 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

- Built with ASP.NET Core, Python SentenceTransformers, and Ollama/llama.cpp.
- Inspired by open-source RAG research and production patterns.
- Thanks to contributors and the AI-RAG community.

---

**Status**: Active development | **Version**: 1.0.0 | **Last Updated**: 2025-10-10