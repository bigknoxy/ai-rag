# AI-RAG - Agents Guidance

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

(End of AGENTS guidance)