# Data Model: Phase 0

Path: /root/code/ai-rag/specs/001-create-a-feature/data-model.md

## Entities

1. Document
- id: string (optional, recommended uuid)
- source: string (optional)
- text: string (required)
- metadata: object (optional)

2. Chunk
- id: string (uuid)
- documentId: string (ref)
- text: string
- startOffset: int
- endOffset: int
- metadata: object

3. EmbeddingRecord
- id: string (uuid)
- chunkId: string (ref)
- vector: array[float] (length = 384)
- source: object { documentId, chunkId }
- createdAt: ISO8601 timestamp

## Validation Rules
- Document.text: required, non-empty
- EmbeddingRecord.vector: length must equal 384

## Storage
- Phase 0 persistence: file-backed JSON at `/root/code/ai-rag/samples/vectors.json`

## Relationships
- Document 1..* Chunk
- Chunk 1..1 EmbeddingRecord

