# ADR 002: Choice of Vector Store

## Status
Accepted

## Context
The system needs a vector store to index and search embeddings for semantic retrieval. It must be local-first, support similarity search, and work in CI without external dependencies.

## Decision
We implemented two vector store options:
- **Primary: InMemoryVectorStore** - For development and small-scale demos. Stores vectors in memory with cosine similarity search.
- **Optional: FaissVectorStore** - For production-like scenarios. Uses FAISS for efficient similarity search on disk.
Reasons:
- **Local-First**: No external databases required; runs in-process or via simple file storage.
- **Performance**: InMemory is fast for small datasets; FAISS scales better for larger indexes with minimal overhead.
- **CI-Safety**: Tests use InMemory with precomputed vectors; no persistent state needed.
- **Ease of Use**: Simple abstractions via `IVectorStore` interface. Easy to swap implementations.
- **Alternatives Considered**:
  - Weaviate (Docker): Requires container setup; overkill for local-first.
  - Pinecone/Chroma: External services with costs and dependencies.
  - SQLite with custom vectors: More complex; FAISS provides better performance out-of-the-box.

## Consequences
- Default config uses InMemory for simplicity; users can opt into Faiss for persistence.
- Embeddings are stored with metadata (chunk ID, source) for citation support.
- Documentation includes setup for Faiss (Python package) and index management.
- Future: Could add more stores (e.g., Qdrant) via the adapter pattern.

## References
- Architecture.md: Vector Store Adapter section
- src/AiRag.Api/Services/InMemoryVectorStore.cs and FaissVectorStore.cs
- Quickstart.md: Vector store configuration