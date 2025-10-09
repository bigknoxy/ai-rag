# AI-RAG - Agents Guidance
### IMPORTANT - ALWAYS CHECK 'context/projects/ai-rag' folder for context and use it for decisions

- **Purpose:**: Guidance for autonomous agents operating in this repository (build, test, lint, style, safety).
- **Build:**: `dotnet build` (from repo root) or `dotnet build src/AiRag.Api` for the API project.
- **Run API:**: `dotnet run --project src/AiRag.Api` (defaults to Development settings).
- **Embeddings service:**: `python -m embeddings.service --host 127.0.0.1 --port 8001` (install with `pip install -r context/projects/ai-rag/embeddings/requirements.txt`).
- **Tests (all):**: `dotnet test` (from repo root; CI uses mocks/precomputed embeddings).
- **Single test:**: `dotnet test --filter "FullyQualifiedName=Namespace.ClassName.TestMethod"` or use `--filter "DisplayName=TestName"` to run one test.
- **Format & lint:**: `dotnet format` (or `dotnet tool run dotnet-format`), follow any `.editorconfig` if present.
- **CI-safety:**: Tests and CI must not call external services; use mocks and precomputed embeddings in `context/projects/ai-rag/samples`.
- **Imports/usings:**: `System.*` first, then third-party, then project usings; prefer file-scoped namespaces for C# 10+.
- **Naming:**: Public types/members PascalCase; interfaces `I*`; private fields `_camelCase`; local variables camelCase.
- **Types:**: Use explicit types when it improves clarity; `var` is allowed when the type is obvious from the initializer.
- **DI & interfaces:**: Favor small, single-responsibility interfaces; register services in `Program.cs` via DI.
- **Async & blocking:**: Use `async/await` for I/O; avoid `.Result` or `.Wait()` to prevent deadlocks.
- **Error handling:**: Throw typed exceptions; avoid swallowing exceptions; controllers should return proper HTTP ProblemDetails or `IActionResult` with clear error messages.
- **Logging & observability:**: Use structured logs (ILogger) and include contextual metadata for requests/jobs.
- **Security:**: Never commit secrets or model weights; CI must use mocks/precomputed artifacts only.
- **Docs & tests:**: Add unit tests for business logic and document public APIs with XML comments where appropriate.
- **Cursor/Copilot rules:**: No `.cursor/`, `.cursorrules`, or `.github/copilot-instructions.md` found in this repo — follow general repo rules above.
- **Commit/PR policy for agents:**: Do not push commits or open PRs without human review; include test results and a short rationale when proposing changes.

### Documentation & Search Comparison

**Use `webfetch` for:**
- Official documentation (Bun, React, Next.js, etc.)
- Direct URLs to specific docs pages
- Most reliable for authoritative information
- Latest feature documentation

**Use `exa_web_search` for:**
- Finding official documentation when you don't have the URL
- Searching for specific features/topics
- Getting multiple authoritative sources
- Latest releases and announcements

**Use `exa_get_code_context` for:**
- Real-world code examples and patterns
- Library/SDK usage from GitHub repos
- Community best practices
- Finding how others solve similar problems

**Priority Order:**
1. `webfetch` (if you have the exact URL)
2. `exa_web_search` (to find official docs)
3. `exa_get_code_context` (for code examples)

**Key Findings:**
- `exa_web_search` reliably finds official documentation
- `exa_get_code_context` often misses official docs, returns examples from other frameworks
- For documentation queries, `exa_web_search` > `exa_get_code_context


# MCP Usage Guide for Opencode

Below is a concise, LLM-friendly guide to using the MCPs (Multi-Context Providers) enabled in your `opencode.json` file. This integrates with the existing tool priorities and best practices outlined above, emphasizing tool-first problem-solving and efficient usage.

### Key Principles for MCP Usage (Aligned with Existing Guidance)
- **Tool-First Mindset**: Always prioritize existing tools/MCPs over building from scratch. Ask: "Which MCP solves this?" before coding or manual work.
- **Priority Order for Documentation/Search** (Integrated with Existing Rules):
  1. **webfetch** (if you have an exact URL for official docs—fastest and most reliable).
  2. **exa_web_search** or **Reference** (for finding/searching docs when URL is unknown).
  3. **exa_get_code_context** (for code examples/community patterns).
  4. **brave-search** (as a fallback for general web queries or private searches).
- **Maximize Usage**: Use MCPs proactively for tasks like research, automation, testing, or content retrieval. Chain them (e.g., search with one, then fetch with another) for complex queries. Avoid redundancy—pick the most targeted MCP first.
- **When to Use MCPs**: For web-related tasks (search, scraping, browser automation), documentation, or code context. Fall back to non-MCP tools (e.g., `read`, `grep`) for local file operations.
- **General Rules**: Respect API limits (e.g., don't spam searches). Use MCPs for efficiency—e.g., automate browser testing instead of manual checks. If a task doesn't fit an MCP, default to core tools like `bash` or `edit`.

### Enabled MCPs in Your Config
Here's a breakdown of each, including purpose, use cases, examples, and priority integration.

1. **context7** (Local: `@upstash/context7-mcp`)
   - **Purpose**: Local code and documentation search for your codebase. Indexes local files for fast, context-aware queries (e.g., finding functions or patterns in your repo).
   - **When to Use**: For local code exploration, debugging, or understanding your project's structure. Ideal for quick lookups without external calls. Prioritize over web searches for internal code.
   - **Examples**:
     - Query: "Find where errors are handled in my API." → Use context7 to search local files for error-handling patterns.
     - When: Before editing code—search for existing implementations to avoid duplication.
   - **Priority**: High for local tasks (e.g., before `grep` or `read`). Use early in development to understand your codebase. Not for web/external docs—defer to `Reference` or `exa`.
   - **Maximize Tip**: Run searches proactively during feature planning to reuse existing code.

2. **chrome-devtools** (Local: `chrome-devtools-mcp@latest`)
   - **Purpose**: Browser automation, performance tracing, screenshots, and page inspection. Automates interactions like clicking, filling forms, or analyzing load times.
   - **When to Use**: For web app testing, debugging UI issues, or performance checks. Great for dynamic tasks (e.g., simulating user interactions). Prioritize over manual browser testing.
   - **Examples**:
     - Query: "Test if my login form works." → Use chrome-devtools to automate clicks, fills, and checks.
     - When: During QA or debugging—e.g., capture screenshots of a broken page or trace slow loads.
   - **Priority**: High for browser-specific tasks (e.g., after `playwright` for E2E). Use for real-time automation; not for static searches (defer to `brave-search` or `exa`).
   - **Maximize Tip**: Integrate with performance tracing for optimization tasks—e.g., identify bottlenecks in your app.

3. **playwright** (Local: `@playwright/mcp@latest`)
   - **Purpose**: Browser automation and end-to-end (E2E) testing. Simulates user interactions, handles dialogs, and runs tests in a headless browser.
   - **When to Use**: For comprehensive testing of web apps (e.g., full user journeys). Best for repeatable, scripted tests. Prioritize for E2E over manual checks.
   - **Examples**:
     - Query: "Run E2E tests for my app's checkout flow." → Use playwright to automate navigation, clicks, and assertions.
     - When: After code changes—e.g., verify a new feature doesn't break existing flows.
   - **Priority**: High for testing workflows (e.g., alongside `chrome-devtools`). Use for structured tests; not for quick searches (defer to `brave-search`).
   - **Maximize Tip**: Combine with `chrome-devtools` for hybrid testing—e.g., playwright for E2E, devtools for performance.

4. **brave-search** (Local: `@modelcontextprotocol/server-brave-search`)
   - **Purpose**: Private web search for general queries, news, or content. Returns diverse sources with filtering options.
   - **When to Use**: For broad web research, current events, or when you need private/uncensored results. Use as a fallback for non-official docs.
   - **Examples**:
     - Query: "Latest trends in AI web apps." → Use brave-search for diverse articles and news.
     - When: For ideation or research—e.g., finding alternatives to a library.
   - **Priority**: Medium (per existing rules: after `webfetch`/`exa` for docs). Use for general queries; prefer `Reference` or `exa` for technical docs.
   - **Maximize Tip**: Leverage for private searches (e.g., sensitive topics) where public tools might not suffice.

5. **exa** (Remote: `https://mcp.exa.ai/mcp`)
   - **Purpose**: Web search and content scraping. Retrieves real-time results from websites, ideal for finding docs or examples.
   - **When to Use**: For searching official docs, tutorials, or code examples when URLs are unknown. Aligns with existing priority for `exa_web_search`.
   - **Examples**:
     - Query: "Find React 19 docs on hooks." → Use exa to search and scrape relevant pages.
     - When: For documentation research—e.g., before implementing a feature.
   - **Priority**: High (per existing rules: #2 after `webfetch`). Use for web-based searches; chain with `webfetch` if you get a URL.
   - **Maximize Tip**: Use for code context (e.g., `exa_get_code_context`) to find real-world examples.

6. **Reference** (Remote: `https://api.ref.tools/mcp`)
   - **Purpose**: Documentation search and reading for up-to-date technical info (e.g., APIs, libraries). Optimized for docs with smart filtering.
   - **When to Use**: For official docs, best practices, or API lookups. Matches existing emphasis on `ref_search_documentation`.
   - **Examples**:
     - Query: "Get Stripe API docs for webhooks." → Use Reference to search and read relevant sections.
     - When: Starting a project—e.g., check latest framework features.
   - **Priority**: High (per existing rules: #2 after `webfetch`). Use for docs; prefer over `brave-search` for technical queries.
   - **Maximize Tip**: Always check here for coding tasks to ensure best practices.

### How to Maximize MCP Usage
- **Chain Tools**: E.g., Use `Reference` or `exa` to find a URL, then `webfetch` to read it. For testing, combine `playwright` with `chrome-devtools`.
- **Proactive Integration**: In development workflows (e.g., per `developer-flow.md`), use MCPs for research (`Reference`/`exa`), testing (`playwright`/`chrome-devtools`), and search (`brave-search`/`context7`).
- **Avoid Overlap**: Don't use `brave-search` for docs if `Reference`/`exa` fits better. Test locally with `context7` before web searches.
- **Best Practices**: Follow existing rules—e.g., use `webfetch` first for known URLs. Monitor for errors (e.g., API limits) and fall back gracefully.
- **When Not to Use**: For purely local tasks (e.g., file edits), stick to `read`/`edit`/`grep`. MCPs are for web/code context—don't force them for everything.

This setup enhances your existing tool ecosystem for efficient, integrated workflows. Reference the full AGENTS.md for overarching guidance!

(End of AGENTS guidance)
