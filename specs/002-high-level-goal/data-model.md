# Data Model: CI-safe embeddings & retrieval

## Entities

### Document
- id: string (UUID recommended)
- text: string
- metadata: object { title?, source?, author?, url? }
- createdAt: datetime
- chunks: List<Chunk> (one-to-many)

### Chunk
- id: string (UUID recommended)
- documentId: string (FK)
- text: string
- startOffset: int
- endOffset: int
- metadata: object
- createdAt: datetime

### EmbeddingVector
- id: string (maps to chunkId)
- vector: float[] (length = 384)
- dimension: int (validate == 384)
- source: enum { Precomputed, Live }

### VectorStore (interface)
- Insert(EmbeddingVector) -> void
- Query(vector, topK:int) -> List<(chunkId,score)>
- Remove(id) -> void
- Clear() -> void

## Validation Rules
- Precomputed embeddings file must contain vectors where each vector length == 384.
- IDs referenced in embeddings must map to existing chunk ids in sample data in CI tests.

## State Transitions
- Document: Draft -> Ingested (chunks created) -> Indexed (vectors stored)
- Chunk: Created -> Embedded (embedding assigned)

## Notes
- Use UUID format for ids to avoid collisions in tests and CI.
- For Phase 1, an in-memory vector store with deterministic cosine similarity ordering is sufficient.
