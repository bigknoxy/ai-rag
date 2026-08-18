# ADR 003: Local-First Design Rationale

## Status
Accepted

## Context
The AI-RAG system must prioritize privacy, zero cost, and ease of deployment. External APIs introduce costs, latency, and data privacy risks, which conflict with open-source goals.

## Decision
Adopt a local-first architecture for all core components:
- **Embeddings**: Run `sentence-transformers` locally via Python service.
- **LLM (Optional)**: Use Ollama or llama.cpp for local inference; default to retrieval-only mode.
- **Vector Store**: In-memory or FAISS on local disk.
- **Deployment**: Docker-based for easy local setup; no cloud dependencies.
Reasons:
- **Privacy**: No data sent to external services; all processing on user's machine.
- **Cost**: Zero API costs; works offline.
- **Simplicity**: Single Docker Compose setup; no account signups or keys.
- **Performance**: Local inference reduces latency for small-scale use.
- **CI/CD**: Tests run with mocks/precomputed data; no external calls.
- **Alternatives Considered**:
  - Hybrid (local + cloud): Increases complexity and costs; not zero-cost.
  - Cloud-Only: Violates privacy and cost goals; requires internet.

## Consequences
- Users must have sufficient hardware (e.g., 4GB RAM for models).
- Documentation emphasizes local setup with cache warm-up instructions.
- Future enhancements (e.g., GPU support) can build on this foundation.
- CI ensures no secrets or external dependencies in builds.

## References
- Plan.md: Zero-cost, local-first constraints
- Architecture.md: Deployment Modes section
- Quickstart.md: Local setup guide