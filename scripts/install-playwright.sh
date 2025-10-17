#!/usr/bin/env bash
set -euo pipefail

# Installs dotnet tools and Playwright browsers for local development and CI
# Usage: ./scripts/install-playwright.sh

# Restore dotnet tools
dotnet tool restore

# Install playwright browsers (with dependencies)
# --with-deps will attempt to install additional system packages on Linux
dotnet tool run playwright install --with-deps
