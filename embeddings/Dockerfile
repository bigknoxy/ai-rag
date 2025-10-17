# Use Python 3.11 slim image
FROM python:3.11-slim

# Set working directory
WORKDIR /app

# Copy requirements and install dependencies
COPY embeddings/requirements.txt .
RUN pip install --no-cache-dir -r requirements.txt

# Copy the service code
COPY embeddings/service.py .

# Expose the port
EXPOSE 8001

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 CMD python -c "import requests; requests.get('http://localhost:8001/health')" || exit 1

# Run the service
CMD ["uvicorn", "service:app", "--host", "0.0.0.0", "--port", "8001"]