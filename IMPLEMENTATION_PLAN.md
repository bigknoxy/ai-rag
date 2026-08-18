# AI-RAG Implementation Plan - From Broken to Working

## Critical Issues Identified

### 1. **Zero Vector Problem** 
- `sample1.embeddings.json` contains all zeros (384 dimensions)
- Cosine similarity always 0.0 or undefined
- No meaningful semantic search possible

### 2. **API Contract Mismatch**
- Quickstart shows file upload but controller expects JSON
- Missing multipart form handling

### 3. **Missing Configuration**
- No `appsettings.json` in API project
- LLM options not configured
- Default mode broken

### 4. **Incomplete Embeddings Service**
- Python service returns dummy zero vectors
- No sentence transformers integration

### 5. **No Document Processing**
- No file upload handling
- No text chunking logic
- No markdown parsing

## Implementation Plan

### Phase 1: Fix Core Infrastructure (IMMEDIATE)

#### 1.1 Fix Python Embeddings Service
- [ ] Install sentence-transformers
- [ ] Implement real embedding generation
- [ ] Add health check endpoint
- [ ] Generate real embeddings for samples

#### 1.2 Fix API Configuration
- [ ] Create `appsettings.json` for API
- [ ] Create `appsettings.Development.json`
- [ ] Configure proper defaults
- [ ] Set up LLM options

#### 1.3 Fix Ingest Controller
- [ ] Add file upload support (`IFormFile`)
- [ ] Add document parsing (markdown, text)
- [ ] Implement text chunking with overlap
- [ ] Fix API contract to match quickstart

### Phase 2: Complete Document Pipeline

#### 2.1 Document Processing Service
- [ ] Create `DocumentProcessor` class
- [ ] Add file type detection
- [ ] Implement text extraction
- [ ] Add intelligent chunking (200-500 tokens with overlap)

#### 2.2 Vector Store Improvements
- [ ] Add persistence options
- [ ] Better error handling
- [ ] Metadata storage improvements
- [ ] Add file-based vector store option

#### 2.3 Query Pipeline Fixes
- [ ] Fix query response format
- [ ] Add proper relevance scoring
- [ ] Improve passage assembly
- [ ] Add source citation formatting

### Phase 3: LLM Integration

#### 3.1 Complete LLM Adapters
- [ ] Finish Ollama integration
- [ ] Add proper prompt templates
- [ ] Implement streaming responses
- [ ] Add error handling and fallbacks

#### 3.2 Prompt Engineering
- [ ] Better context assembly
- [ ] RAG-specific prompts
- [ ] Source citation formatting
- [ ] Token budget management

### Phase 4: Testing & Validation

#### 4.1 Comprehensive Tests
- [ ] Unit tests for chunking
- [ ] Integration tests for full pipeline
- [ ] End-to-end tests with real documents
- [ ] Performance benchmarks

#### 4.2 Performance Optimization
- [ ] Embedding caching
- [ ] Vector search optimization
- [ ] Memory usage improvements
- [ ] Async processing

## Implementation Order

1. **FIRST**: Fix embeddings service (real vectors)
2. **SECOND**: Fix API configuration and ingest
3. **THIRD**: Test basic ingest/query flow
4. **FOURTH**: Add document processing
5. **FIFTH**: Complete LLM integration
6. **SIXTH**: Comprehensive testing

## Success Criteria

- [ ] Can ingest markdown files via file upload
- [ ] Can query and get relevant passages
- [ ] Embeddings are real (non-zero vectors)
- [ ] Cosine similarity works correctly
- [ ] API matches quickstart documentation
- [ ] All tests pass
- [ ] System works end-to-end

## Files to Create/Modify

### New Files
- `src/AiRag.Api/appsettings.json`
- `src/AiRag.Api/appsettings.Development.json`
- `src/AiRag.Api/Services/DocumentProcessor.cs`
- `src/AiRag.Api/Services/ChunkingService.cs`

### Modified Files
- `embeddings/service.py` (real embeddings)
- `samples/sample1.embeddings.json` (real vectors)
- `src/AiRag.Api/Controllers/IngestController.cs` (file upload)
- `src/AiRag.Api/Controllers/QueryController.cs` (response format)
- `src/AiRag.Api/Services/ServiceCollectionExtensions.cs` (new services)

## Testing Strategy

1. **Unit Tests**: Test individual components
2. **Integration Tests**: Test full pipeline
3. **Manual Tests**: Use curl commands from quickstart
4. **Performance Tests**: Ensure reasonable response times

## Timeline Estimate

- Phase 1: 2-3 hours (critical fixes)
- Phase 2: 3-4 hours (document pipeline)
- Phase 3: 2-3 hours (LLM integration)
- Phase 4: 2-3 hours (testing/validation)

**Total: 9-13 hours for fully working system**