#!/usr/bin/env bash
# Perf sanity script: runs 100 ingest requests (placeholder)
set -euo pipefail
for i in $(seq 1 100); do
  echo "Ingest $i"
done
