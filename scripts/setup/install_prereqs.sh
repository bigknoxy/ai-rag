#!/usr/bin/env bash
# install_prereqs.sh
# Idempotent installer: installs .NET 8 SDK, Python 3.11 + venv/pip, and python requirements.
# Usage: sudo ./install_prereqs.sh
set -euo pipefail

REPO_ROOT="$(cd "$(dirname "$0")/../.." && pwd)"
EMB_REQS="$REPO_ROOT/embeddings/requirements.txt"

confirm() {
  read -r -p "$1 [y/N]: " response
  case "$response" in
    [yY][eE][sS]|[yY]) true ;; *) false ;;
  esac
}

has_cmd() { command -v "$1" >/dev/null 2>&1; }
info(){ printf "\n==> %s\n" "$1"; }
warn(){ printf "\n!! %s\n" "$1"; }

# Detect OS
OS=""
if [[ "$(uname -s)" == "Darwin" ]]; then
  OS="macos"
elif [[ -f /etc/os-release ]]; then
  . /etc/os-release
  case "${ID,,}" in
    ubuntu|debian) OS="debian" ;;
    fedora|rhel|centos) OS="rhel" ;;
    *) OS="debian" ;;
  esac
else
  OS="unknown"
fi
info "Detected platform: $OS"

# Ensure curl or wget
if ! has_cmd curl && ! has_cmd wget; then
  if [ "$OS" = "debian" ]; then
    apt-get update && apt-get install -y curl
  elif [ "$OS" = "rhel" ]; then
    dnf install -y curl
  else
    warn "No curl/wget found and automatic installation not supported for this OS. Please install curl or wget."
  fi
fi

# 1) Ensure .NET 8 SDK
check_dotnet() {
  if has_cmd dotnet; then
    v="$(dotnet --version 2>/dev/null || true)"
    printf "%s" "$v"
  else
    printf ""
  fi
}

dotnet_ver="$(check_dotnet)"
if [[ -n "$dotnet_ver" && "${dotnet_ver%%.*}" -ge 8 ]]; then
  info "dotnet SDK already installed: $(dotnet --version)"
else
  info "dotnet SDK 8 not found (found: ${dotnet_ver:-none})."
  if confirm "Install .NET SDK 8 now?"; then
    if [ "$OS" = "debian" ]; then
      info "Installing .NET SDK 8 for Debian/Ubuntu..."
      # Attempt to add Microsoft package feed for Ubuntu/Debian
      if has_cmd wget; then
        wget -q https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb -O /tmp/packages-microsoft-prod.deb || true
      elif has_cmd curl; then
        curl -fsSL https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb -o /tmp/packages-microsoft-prod.deb || true
      fi
      if [ -f /tmp/packages-microsoft-prod.deb ]; then
        dpkg -i /tmp/packages-microsoft-prod.deb || true
        apt-get update
        apt-get install -y dotnet-sdk-8.0 || { warn "dotnet-sdk-8.0 not available from apt for this distro. Install manually: https://learn.microsoft.com/dotnet/core/install/linux"; }
      else
        warn "Could not fetch Microsoft package feed. Install .NET SDK manually: https://learn.microsoft.com/dotnet/core/install/linux"
      fi
    elif [ "$OS" = "rhel" ]; then
      info "Installing .NET SDK 8 for RHEL/Fedora..."
      dnf install -y dotnet-sdk-8.0 || { warn "Install dotnet manually: https://learn.microsoft.com/dotnet/core/install/linux"; }
    elif [ "$OS" = "macos" ]; then
      if has_cmd brew; then
        info "Installing .NET SDK 8 via Homebrew..."
        brew update
        brew install --cask dotnet-sdk || warn "Homebrew install failed; install from https://learn.microsoft.com/dotnet/core/install/macos"
      else
        warn "Homebrew not found. Install Homebrew or .NET manually: https://learn.microsoft.com/dotnet/core/install/macos"
      fi
    else
      warn "Unsupported OS for automated dotnet install. See https://learn.microsoft.com/dotnet/core/install"
    fi
  else
    warn "Skipping .NET install by user choice."
  fi
fi

# 2) Ensure Python 3.11 (or >=3.11)
PY_CMD=""
if has_cmd python3.11; then
  PY_CMD="python3.11"
elif has_cmd python3; then
  PY_CMD="python3"
else
  PY_CMD=""
fi

py_ok=false
if [ -n "$PY_CMD" ]; then
  if "$PY_CMD" -c 'import sys; v=sys.version_info; print((v.major==3 and v.minor>=11) or v.major>3)' 2>/dev/null | grep -q True; then
    py_ok=true
  fi
fi

if $py_ok; then
  info "Python suitable version available: $($PY_CMD --version 2>&1)"
else
  warn "Python >= 3.11 not found or not the default. Attempt to install."
  if confirm "Install Python 3.11 via package manager?"; then
    if [ "$OS" = "debian" ]; then
      apt-get update
      apt-get install -y software-properties-common || true
      apt-get install -y python3.11 python3.11-venv python3.11-distutils python3-pip || warn "Could not install python3.11 via apt; consider pyenv or manual install."
      PY_CMD="python3.11"
    elif [ "$OS" = "rhel" ]; then
      dnf install -y python3.11 python3.11-venv python3-pip || warn "Could not install python3.11 via dnf; consider pyenv."
      PY_CMD="python3.11"
    elif [ "$OS" = "macos" ]; then
      if has_cmd brew; then
        brew install python@3.11 || warn "Homebrew install failed."
        PY_CMD="python3"
      else
        warn "Install Homebrew or Python manually for macOS."
      fi
    else
      warn "Unsupported OS for automated Python install."
    fi
  else
    warn "Skipping Python install by user choice. You must provide Python >=3.11 to proceed."
  fi
fi

# Ensure pip and venv are available
if [ -n "$PY_CMD" ] && has_cmd "$PY_CMD"; then
  info "Ensuring pip and venv for $PY_CMD"
  "$PY_CMD" -m ensurepip --upgrade >/dev/null 2>&1 || true
  "$PY_CMD" -m pip install --upgrade pip setuptools wheel >/dev/null 2>&1 || true
fi

# 3) Create a virtualenv in repo root and install python requirements
if [ -n "$PY_CMD" ] && has_cmd "$PY_CMD" && [ -f "$EMB_REQS" ]; then
  VENV_DIR="$REPO_ROOT/.venv"
  if [ ! -d "$VENV_DIR" ]; then
    info "Creating virtualenv in $VENV_DIR"
    "$PY_CMD" -m venv "$VENV_DIR"
  else
    info "Virtualenv already exists: $VENV_DIR"
  fi

  PIP="$VENV_DIR/bin/pip"
  if [ -x "$PIP" ]; then
    info "Installing python requirements from $EMB_REQS"
    "$PIP" install --upgrade pip
    "$PIP" install -r "$EMB_REQS"
  else
    warn "pip not found in $VENV_DIR; you can activate venv and run: source $VENV_DIR/bin/activate && pip install -r $EMB_REQS"
  fi
else
  warn "Python or requirements file not available; skipping virtualenv + pip install."
fi

# 4) Final verification
info "Verification summary:"
if has_cmd dotnet; then
  printf " - dotnet: %s\n" "$(dotnet --version 2>/dev/null || 'unknown')"
else
  printf " - dotnet: not installed or not on PATH\n"
fi

if [ -n "$PY_CMD" ] && has_cmd "$PY_CMD"; then
  printf " - python: %s\n" "$($PY_CMD --version 2>&1)"
else
  printf " - python: not installed or not on PATH\n"
fi

if [ -f "$REPO_ROOT/.venv/bin/pip" ]; then
  printf " - venv pip: %s\n" "$($REPO_ROOT/.venv/bin/pip --version 2>/dev/null || 'unknown')"
fi

info "Setup script finished. If any step failed, review the printed warnings and follow the referenced links for manual installation."
