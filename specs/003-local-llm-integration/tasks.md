# AI-RAG Phase 3 Tasks: Local LLM Integration

## Phase 3 - Optional Local LLM (2-3 days, opt-in)

- TASK-3-1: Research local LLM options (Ollama, llama.cpp) and .NET integration patterns.
- TASK-3-2: Design ILLMAdapter interface and concrete implementations (OllamaAdapter, LlamaCppAdapter, MockLLMAdapter).
- TASK-3-3: Implement streaming support for LLM responses in query endpoint.
- TASK-3-4: Add configuration for LLM provider selection (Ollama, llama.cpp, Mock).
- TASK-3-5: Create unit tests for LLM adapters and integration.
- TASK-3-6: Update query endpoint to use LLM for generating responses from retrieved chunks.
- TASK-3-7: Ensure CI uses MockLLM and no external LLM services.
- TASK-3-8: Update quickstart and docs for local LLM setup.

## Detailed Task Breakdown

### 1. Research and Planning (1-2 days, Priority: High)
- **Task 3.1.1**: Research Ollama integration for .NET (API client, model serving, CPU requirements). Document pros/cons vs. llama.cpp.
- **Task 3.1.2**: Research llama.cpp .NET bindings (e.g., via llama.cpp-sharp or direct HTTP calls to llama.cpp server). Identify quantization options for small models.
- **Task 3.1.3**: Select recommended models (e.g., Llama 2 7B Q4_0 for Ollama, Mistral 7B for llama.cpp). Ensure <8GB RAM usage.
- **Task 3.1.4**: Analyze streaming protocols (SSE for .NET API) and error handling for local LLM failures.
- **Dependencies**: None. **Output**: Update `research.md` with findings and model recommendations.

### 2. Design and Interface Definition (1 day, Priority: High)
- **Task 3.2.1**: Design `ILLMAdapter` interface (methods: `GenerateAsync`, `GenerateStreamingAsync`, properties for model info).
- **Task 3.2.2**: Design `OllamaAdapter` implementation (HTTP client to Ollama API, model config).
- **Task 3.2.3**: Design `LlamaCppAdapter` implementation (HTTP client to llama.cpp server, quantization support).
- **Task 3.2.4**: Design `MockLLMAdapter` for CI (returns canned responses, simulates streaming).
- **Task 3.2.5**: Design configuration model (e.g., `LLM:Provider`, `LLM:Model`, `LLM:Enabled`).
- **Dependencies**: Existing `IEmbeddingProvider` pattern. **Output**: Interface definitions and class stubs.

### 3. Configuration and DI Setup (0.5 days, Priority: Medium)
- **Task 3.3.1**: Add LLM config section to `appsettings.json` (defaults: `Enabled=false`, `Provider=Mock`).
- **Task 3.3.2**: Update `ServiceCollectionExtensions.cs` to register `ILLMAdapter` with conditional binding (e.g., based on `LLM:Provider`).
- **Task 3.3.3**: Add environment variable support (e.g., `LLM__Provider=Ollama`).
- **Task 3.3.4**: Add per-query option (`useLlm` parameter in query models).
- **Dependencies**: Existing config system. **Output**: Configurable DI registration.

### 4. Implement LLM Adapters (2-3 days, Priority: High)
- **Task 3.4.1**: Implement `MockLLMAdapter` (hardcoded responses, streaming simulation).
- **Task 3.4.2**: Implement `OllamaAdapter` (HTTP calls to localhost:11434, handle model loading errors).
- **Task 3.4.3**: Implement `LlamaCppAdapter` (HTTP calls to llama.cpp server, support quantization params).
- **Task 3.4.4**: Add error handling (e.g., fallback to retrieval-only if LLM unavailable).
- **Task 3.4.5**: Add logging for LLM calls (structured logs for observability).
- **Dependencies**: Task 3.2. **Output**: Working adapters with unit tests.

### 5. Implement Streaming Support (1 day, Priority: Medium)
- **Task 3.5.1**: Update `QueryController` to support streaming responses (IAsyncEnumerable for tokens).
- **Task 3.5.2**: Integrate streaming into `ILLMAdapter` (yield tokens progressively).
- **Task 3.5.3**: Add SSE or chunked transfer encoding for API responses.
- **Task 3.5.4**: Handle streaming errors (e.g., model crash mid-response).
- **Dependencies**: Existing query endpoint. **Output**: Streaming-enabled query API.

### 6. Update Query Endpoint Integration (1-2 days, Priority: High)
- **Task 3.6.1**: Modify `QueryController` to check LLM config and use `ILLMAdapter` when enabled.
- **Task 3.6.2**: Update prompt building to include retrieved chunks as context for LLM.
- **Task 3.6.3**: Ensure fallback to retrieval-only mode if LLM fails or is disabled.
- **Task 3.6.4**: Add response formatting (markdown for LLM, plain for retrieval-only).
- **Task 3.6.5**: Update response models to include `llmUsed` flag and source citations.
- **Dependencies**: Phase 2 query endpoint, `PromptBuilder`. **Output**: Hybrid query mode.

### 7. Testing and Validation (1-2 days, Priority: Medium)
- **Task 3.7.1**: Write unit tests for each adapter (mock HTTP responses).
- **Task 3.7.2**: Write integration tests for query endpoint (LLM vs. retrieval-only modes).
- **Task 3.7.3**: Add contract tests for streaming responses.
- **Task 3.7.4**: Ensure CI uses `MockLLMAdapter` (no real LLM calls).
- **Task 3.7.5**: Test error scenarios (LLM unavailable, invalid config).
- **Dependencies**: Existing test structure. **Output**: Passing tests with mocks.

### 8. Documentation and Quickstart Updates (0.5-1 days, Priority: Low)
- **Task 3.8.1**: Update `quickstart.md` with Ollama setup (prioritize) and llama.cpp as alternative.
- **Task 3.8.2**: Add model download instructions (e.g., `ollama pull llama2:7b`).
- **Task 3.8.3**: Document config options and per-query parameters.
- **Task 3.8.4**: Update `README.md` with Phase 3 features.
- **Task 3.8.5**: Add troubleshooting for common LLM issues (e.g., port conflicts).
- **Dependencies**: Existing docs. **Output**: User-friendly setup guide.

## Total Estimated Effort
- **8-12 developer-days** (depending on testing depth and iterations).
- **Parallel Opportunities**: Research and design can overlap; adapters can be implemented in parallel.
- **Risks**: Local LLM setup variability (model downloads); mitigate with clear docs and fallbacks.
- **Success Criteria**: Query endpoint works in both modes; CI passes with mocks; no external dependencies.

(End of updated tasks)