# Contributing to AI-RAG

We welcome contributions! Please follow these guidelines to ensure a smooth process.

## Getting Started

1. Fork the repository.
2. Clone your fork: `git clone https://github.com/yourusername/ai-rag.git`
3. Create a feature branch: `git checkout -b feature/your-feature-name`
4. Make your changes.
5. Run tests: `dotnet test`
6. Format code: `dotnet format`
7. Commit and push: `git push origin feature/your-feature-name`
8. Open a pull request.

## Guidelines

- **TDD First**: Write failing tests before implementing features.
- **CI-Safe**: No external APIs in tests; use mocks or precomputed data.
- **Code Style**: Follow `.editorconfig` and use `dotnet format`.
- **PR Description**: Include what was changed, why, and how to test.
- **Governance**: Follow the AI-RAG Constitution in `.specify/memory/constitution.md`.

## Reporting Issues

Use GitHub Issues for bugs or feature requests.

## License

By contributing, you agree to license your contributions under the MIT License.