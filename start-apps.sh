#!/bin/bash

# Script to start AI-RAG apps locally on specific ports
# Ports: Embeddings (8001), API (8000), Blazor (5250)

set -e  # Exit on any error

echo "Checking for running processes..."

# Check if embeddings is running on port 8001
if lsof -Pi :8001 -sTCP:LISTEN -t >/dev/null ; then
    echo "Embeddings service is running on port 8001. Killing it..."
    pkill -f "python -m embeddings.service" || true
    sleep 2
fi

# Check if API is running on port 8000
if lsof -Pi :8000 -sTCP:LISTEN -t >/dev/null ; then
    echo "API is running on port 8000. Killing it..."
    pkill -f "dotnet run.*AiRag.Api" || true
    sleep 2
fi

# Check if Blazor is running on port 5250
if lsof -Pi :5250 -sTCP:LISTEN -t >/dev/null ; then
    echo "Blazor is running on port 5250. Killing it..."
    pkill -f "dotnet run.*AiRag.Blazor" || true
    sleep 2
fi

echo "Starting services..."

# Start embeddings service
echo "Starting embeddings service..."
python3 -m venv venv 2>/dev/null || echo "Venv already exists"
. venv/bin/activate
pip install -r embeddings/requirements.txt >/dev/null 2>&1
python -m embeddings.service --host 127.0.0.1 --port 8001 &
EMBEDDINGS_PID=$!
echo "Embeddings service started with PID $EMBEDDINGS_PID on port 8001"

# Start API
echo "Starting API..."
dotnet run --project src/AiRag.Api --urls "http://localhost:8000" &
API_PID=$!
echo "API started with PID $API_PID on port 8000"

# Start Blazor
echo "Starting Blazor..."
dotnet run --project src/AiRag.Blazor --urls "http://localhost:5250" &
BLAZOR_PID=$!
echo "Blazor started with PID $BLAZOR_PID on port 5250"

echo "All services started. Press Ctrl+C to stop."
echo "Embeddings PID: $EMBEDDINGS_PID"
echo "API PID: $API_PID"
echo "Blazor PID: $BLAZOR_PID"

# Wait for all background processes
wait