#!/usr/bin/env bash
# Basic performance smoke test for AI-RAG API
# Runs API locally, issues 5 queries, measures average response time
# Exits non-zero if average > 1s (default threshold)

set -euo pipefail
THRESHOLD=${1:-1.0}
PORT=5000
API_PID=""

cleanup() {
  if [[ -n "$API_PID" ]]; then
    kill $API_PID 2>/dev/null || true
  fi
}
trap cleanup EXIT

export Embedding__Mode=Precomputed
export ASPNETCORE_ENVIRONMENT=CI

dotnet run --project src/AiRag.Api --no-launch-profile &
API_PID=$!

# Wait for API to start
for i in {1..30}; do
  if curl -s "http://localhost:$PORT/api/health" | grep -q 'Healthy'; then
    break
  fi
  sleep 1
  if [[ $i -eq 30 ]]; then
    echo "API did not start in time" >&2
    exit 2
  fi
done

# Issue 5 queries and measure response times
sum=0
for i in {1..5}; do
  start=$(date +%s%3N)
  curl -s -X POST "http://localhost:$PORT/api/query" \
    -H "Content-Type: application/json" \
    -H "x-api-key: demo-key" \
    -d '{"query":"What is retrieval-augmented generation?","sessionId":"smoke-'$i'"}' > /dev/null
  end=$(date +%s%3N)
  elapsed=$((end - start))
  sum=$((sum + elapsed))
  echo "Query $i: ${elapsed}ms"
done
avg=$(echo "scale=2; $sum / 5 / 1000" | bc)
echo "Average response time: ${avg}s"
if (( $(echo "$avg > $THRESHOLD" | bc -l) )); then
  echo "FAIL: Average response time ${avg}s exceeds threshold ${THRESHOLD}s" >&2
  exit 1
else
  echo "PASS: Average response time ${avg}s within threshold ${THRESHOLD}s"
fi
