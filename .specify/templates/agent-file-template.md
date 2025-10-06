# [PROJECT NAME] Development Guidelines

Auto-generated from all feature plans. Last updated: [DATE]

## Active Technologies
[EXTRACTED FROM ALL PLAN.MD FILES]

## Project Structure
```
[ACTUAL STRUCTURE FROM PLANS]
```

## Commands
[ONLY COMMANDS FOR ACTIVE TECHNOLOGIES]

## Code Style
[LANGUAGE-SPECIFIC, ONLY FOR LANGUAGES IN USE]

## Recent Changes
[LAST 3 FEATURES AND WHAT THEY ADDED]

<!-- MANUAL ADDITIONS START -->
## Governance & Contributor Rules
- Constitution: See `.specify/memory/constitution.md` — Version: v2.2.0 (Last Amended: 2025-10-06). RATIFICATION_DATE: TODO.
- Mandatory TDD: Tests MUST be authored and committed before implementation; tests MUST initially fail (red) on CI and use only local mocks or precomputed artifacts.
- CI Safety: CI MUST NOT call external paid services or require secrets. Optional LLM usage must be explicitly opt-in and excluded from CI runs.
- Update process: When updating this section, also update the `Last updated` date at the top of this file.
<!-- MANUAL ADDITIONS END -->