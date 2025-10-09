# Phase 3 Data Model: Local LLM Integration

## Key Entities

### LLMAdapter
- **Purpose**: Abstraction for LLM providers.
- **Fields**:
  - ProviderName: string (e.g., "Ollama", "LlamaCpp", "Mock")
  - ModelName: string (e.g., "llama2:7b")
  - IsStreamingSupported: bool
- **Relationships**: Used by QueryController for response generation.

### Query (Extended)
- **Purpose**: User query with LLM option.
- **Fields** (new):
  - UseLlm: bool? (optional, defaults to config)
- **Relationships**: Inherits from existing Query model.

### Response (Extended)
- **Purpose**: Query response with LLM metadata.
- **Fields** (new):
  - LlmUsed: bool
  - ResponseFormat: string (e.g., "markdown")
- **Relationships**: Includes existing results and assembled fields.

## Validation Rules
- LLMAdapter: ProviderName must be one of supported values.
- Query: UseLlm optional, validated against config.
- Response: LlmUsed indicates if LLM was used.

## State Transitions
- Query → If UseLlm=true and config enabled → Use LLMAdapter → Generate response.
- Query → If UseLlm=false or LLM fails → Use PromptBuilder for retrieval-only.

(End of data model)