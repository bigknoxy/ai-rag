# Phase 3 Research: Local LLM Integration

## Decisions
- **Ollama Prioritization**: Chosen for primary documentation due to ease of use (Docker, model management). Llama.cpp as alternative for advanced users.
- **Rationale**: Aligns with project's user-friendly, zero-cost goals.
- **Alternatives Considered**: OpenAI API (rejected - not local/zero-cost), Hugging Face local (more complex setup).

## Model Selection
- **Recommended Models**: Llama 2 7B Q4_0 (~4GB) for Ollama; Mistral 7B for llama.cpp.
- **Rationale**: CPU-friendly, good RAG performance, <8GB RAM.
- **Alternatives Considered**: Larger models (rejected - resource intensive).

## Streaming Implementation
- **Protocol**: Server-Sent Events (SSE) for .NET API.
- **Rationale**: Standard for real-time web responses.
- **Alternatives Considered**: WebSockets (overkill for this use case).

## Error Handling
- **Fallback Strategy**: Retrieval-only mode on LLM failure.
- **Rationale**: Maintains functionality without external dependencies.
- **Alternatives Considered**: Retry logic (not needed for local services).

(End of research - to be updated with findings)