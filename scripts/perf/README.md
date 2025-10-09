# Performance Smoke Test

To run a basic performance smoke test:

    scripts/perf/smoke_test.sh

This will start the API locally (in CI-safe mode), issue 5 queries, and check that average response time is below 1s (default threshold).

You can change the threshold by passing a value:

    scripts/perf/smoke_test.sh 2.0

The script uses only local precomputed embeddings and does not require external services.

