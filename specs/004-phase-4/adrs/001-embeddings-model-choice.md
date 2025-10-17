# ADR 001: Choice of Embeddings Model

## Status
Accepted

## Context
The AI-RAG system requires an embeddings model to convert text chunks into vectors for semantic search. The system must support local, open-source, CPU-friendly options to align with zero-cost, privacy-focused deployment.

## Decision
We selected `sentence-transformers` (specifically `all-MiniLM-L6-v2`) as the default embeddings model for the following reasons:
- **Local and Open-Source**: Runs entirely on CPU without external API calls, ensuring privacy and no costs.
- **Performance**: Lightweight model (~23MB) that fits in minimal RAM (tested on 4GB systems). Provides good semantic similarity for retrieval tasks.
- **Ease of Integration**: Python-based with a simple API via Hugging Face Transformers. Easy to containerize in the embeddings service.
- **Quality**: Achieves high accuracy on benchmarks like STS tasks, suitable for document retrieval.
- **Alternatives Considered**:
  - OpenAI `text-embedding-ada-002`: Requires API key and costs; not local-first.
  - Larger models (e.g., `all-mpnet-base-v2`): Higher accuracy but increased RAM/CPU usage, not suitable for low-resource environments.
  - Custom training: Overkill for this phase; would delay development.

## Consequences
- Embeddings are computed locally in the Python service, reducing latency and ensuring offline capability.
- CI uses precomputed embeddings from `samples/` to avoid runtime model loading.
- Users can swap models via configuration if needed (e.g., for better accuracy on domain-specific data).
- Documentation must include setup instructions for model download and caching.

## References
- Hugging Face Model Hub: https://huggingface.co/sentence-transformers/all-MiniLM-L6-v2
- Architecture.md: Embeddings Adapter section
- Quickstart.md: Local setup guide