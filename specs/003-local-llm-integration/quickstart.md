# Quickstart: Phase 3 Local LLM Integration

## Setup Ollama (Recommended)
1. Install Ollama: [https://ollama.ai/](https://ollama.ai/)
2. Pull a model: `ollama pull llama2:7b` (or `mistral:7b` for variety)
3. Run Ollama: `ollama serve` (runs on http://localhost:11434)
4. Set config: `export LLM__Provider=Ollama` `export LLM__Model=llama2:7b` `export LLM__Enabled=true`
5. Run API: `dotnet run --project src/AiRag.Api`
6. Query with LLM: `curl -X POST "http://localhost:5000/api/query" -H "Content-Type: application/json" -d '{"text":"Explain RAG","useLlm":true}'`

## Setup llama.cpp (Alternative)
1. Install llama.cpp: [https://github.com/ggerganov/llama.cpp](https://github.com/ggerganov/llama.cpp)
2. Download quantized model (e.g., llama-2-7b.Q4_0.gguf from Hugging Face).
3. Run server: `./llama-server -m llama-2-7b.Q4_0.gguf -c 512` (runs on http://localhost:8080)
4. Set config: `export LLM__Provider=LlamaCpp` `export LLM__Model=llama-2-7b` `export LLM__Enabled=true` `export LLM__BaseUrl=http://localhost:8080`
5. Run API: `dotnet run --project src/AiRag.Api`
6. Query with LLM: `curl -X POST "http://localhost:5000/api/query" -H "Content-Type: application/json" -d '{"text":"Explain RAG","useLlm":true}'`

## Streaming Responses
- Use `/api/query/stream` for real-time token streaming: `curl -X POST "http://localhost:5000/api/query/stream" -H "Content-Type: application/json" -d '{"text":"Explain RAG","useLlm":true}'`

## Configuration Options
- `LLM:Enabled`: true/false (default: false for retrieval-only)
- `LLM:Provider`: Ollama/LlamaCpp/Mock (default: Mock)
- `LLM:Model`: Model name (e.g., llama2:7b)
- `LLM:BaseUrl`: LLM service URL (default: http://localhost:11434 for Ollama)

## Troubleshooting
- Ensure LLM service is running on expected port (11434 for Ollama, 8080 for llama.cpp).
- Check logs for connection errors or model loading issues.
- For CI, LLM is disabled by default; tests use MockLLMAdapter.
- Models should be <8GB RAM; use quantized versions for CPU-only machines.

(End of quickstart)