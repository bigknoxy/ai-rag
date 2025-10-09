# Implementation Plan: Optional Local LLM Integration (Phase 3)

**Branch**: `003-local-llm-integration` | **Date**: 2025-10-09 | **Spec**: specs/003-local-llm-integration/spec.md

## Execution Flow (/plan command scope)
```
1. Load feature spec from Input path
   → If not found: ERROR "No feature spec at {path}"
2. Fill Technical Context (scan for NEEDS CLARIFICATION)
   → Detect Project Type from file system structure or context (web=frontend+backend, mobile=app+api)
   → Set Structure Decision based on project type
3. Fill the Constitution Check section based on the content of the constitution document.
4. Evaluate Constitution Check section below
   → If violations exist: Document in Complexity Tracking
   → If no justification possible: ERROR "Simplify approach first"
   → Update Progress Tracking: Initial Constitution Check
5. Execute Phase 0 → research.md
   → If NEEDS CLARIFICATION remain: ERROR "Resolve unknowns"
6. Execute Phase 1 → contracts, data-model.md, quickstart.md, agent-specific template file (e.g., `CLAUDE.md` for Claude Code, `.github/copilot-instructions.md` for GitHub Copilot, `GEMINI.md` for Gemini CLI, `QWEN.md` for Qwen Code, or `AGENTS.md` for all other agents).
7. Re-evaluate Constitution Check section
   → If new violations: Refactor design, return to Phase 1
   → Update Progress Tracking: Post-Design Constitution Check
8. Plan Phase 2 → Describe task generation approach (DO NOT create tasks.md)
9. STOP - Ready for /tasks command
```

**IMPORTANT**: The /plan command STOPS at step 7. Phases 2-4 are executed by other commands:
- Phase 2: /tasks command creates tasks.md
- Phase 3-4: Implementation execution (manual or via tools)

## Summary
Implement optional local LLM integration for AI-RAG, adding pluggable adapters for Ollama and llama.cpp with streaming support, MockLLM for CI, and markdown response formatting. Builds on Phase 2's retrieval pipeline.

## Technical Context
**Language/Version**: C# / .NET 8
**Primary Dependencies**: Existing AI-RAG codebase, HTTP clients for local LLM services
**Storage**: N/A (leverages existing vector store)
**Testing**: xUnit with mocks for LLM calls
**Target Platform**: Linux/Windows servers, local dev
**Project Type**: Web application (backend API)
**Performance Goals**: <500ms for LLM responses, CPU-only
**Constraints**: Local LLMs only, no external APIs
**Scale/Scope**: Single API instance, optional per user

## Constitution Check
*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

[Gates determined based on constitution file]

## Project Structure

### Documentation (this feature)
```
specs/003-local-llm-integration/
├── plan.md              # This file (/plan command output)
├── research.md          # Phase 0 output (/plan command)
├── data-model.md        # Phase 1 output (/plan command)
├── quickstart.md        # Phase 1 output (/plan command)
├── contracts/           # Phase 1 output (/plan command)
└── tasks.md             # Phase 2 output (/tasks command - NOT created by /plan)
```

### Source Code (repository root)
```
src/AiRag.Api/
├── Adapters/
│   └── ILLMAdapter.cs   # New interface
├── Services/
│   ├── OllamaAdapter.cs # New
│   ├── LlamaCppAdapter.cs # New
│   └── MockLLMAdapter.cs # New
└── Controllers/
    └── QueryController.cs # Updated for LLM integration
```

**Structure Decision**: Extend existing web application structure with new adapters and controller updates.

## Phase 0: Outline & Research
1. **Extract unknowns from Technical Context** above:
   - Research local LLM integration patterns for .NET
   - Model selection for CPU-friendliness
   - Streaming implementation details

2. **Generate and dispatch research agents**:
   - Task: "Research Ollama .NET integration and CPU model options"
   - Task: "Research llama.cpp .NET bindings and quantization"

3. **Consolidate findings** in `research.md` using format:
   - Decision: [what was chosen]
   - Rationale: [why chosen]
   - Alternatives considered: [what else evaluated]

**Output**: research.md with all NEEDS CLARIFICATION resolved

## Phase 1: Design & Contracts
*Prerequisites: research.md complete*

1. **Extract entities from feature spec** → `data-model.md`:
   - LLMAdapter entities, config models

2. **Generate API contracts** from functional requirements:
   - Update query endpoint for LLM options

3. **Generate contract tests** from contracts:
   - Tests for LLM vs. retrieval modes

4. **Extract test scenarios** from user stories:
   - Integration tests for streaming and fallbacks

5. **Update agent file incrementally** (O(1) operation):
   - Run `.specify/scripts/bash/update-agent-context.sh opencode`

**Output**: data-model.md, /contracts/*, failing tests, quickstart.md, agent-specific file

## Phase 2: Task Planning Approach
*This section describes what the /tasks command will do - DO NOT execute during /plan*

**Task Generation Strategy**:
- Load `.specify/templates/tasks-template.md` as base
- Generate tasks from Phase 1 design docs (contracts, data model, quickstart)
- Each adapter → implementation task
- Each requirement → test task

**Ordering Strategy**:
- TDD order: Tests before implementation
- Dependency order: Interfaces before adapters

**Estimated Output**: 20-25 numbered, ordered tasks in tasks.md

**IMPORTANT**: This phase is executed by the /tasks command, NOT by /plan

## Phase 3+: Future Implementation
*These phases are beyond the scope of the /plan command*

**Phase 3**: Task execution (/tasks command creates tasks.md)
**Phase 4**: Implementation (execute tasks.md following constitutional principles)
**Phase 5**: Validation (run tests, execute quickstart.md, performance validation)

## Complexity Tracking
*Fill ONLY if Constitution Check has violations that must be justified*

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| [None] | N/A | N/A |

## Progress Tracking
*This checklist is updated during execution flow*

**Phase Status**:
- [x] Phase 0: Research complete (/plan command)
- [x] Phase 1: Design complete (/plan command)
- [ ] Phase 2: Task planning complete (/plan command - describe approach only)
- [ ] Phase 3: Tasks generated (/tasks command)
- [ ] Phase 4: Implementation complete
- [ ] Phase 5: Validation passed

**Gate Status**:
- [x] Initial Constitution Check: PASS
- [x] Post-Design Constitution Check: PASS
- [x] All NEEDS CLARIFICATION resolved
- [ ] Complexity deviations documented

---
*Based on Constitution v2.2.0 - See `/memory/constitution.md`*