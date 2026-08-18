"""Live-service integration test for the embeddings API.

Skipped automatically when the service is not running on 127.0.0.1:8001.
For offline unit coverage see embeddings/test_service.py.
"""
import os

import requests

import pytest

SERVICE_URL = os.environ.get("EMBEDDINGS_SERVICE_URL", "http://127.0.0.1:8001")


def _service_up() -> bool:
    try:
        requests.get(f"{SERVICE_URL}/health", timeout=1)
        return True
    except Exception:
        return False


live = pytest.mark.skipif(not _service_up(), reason="embedding service not running on 127.0.0.1:8001")


@live
def test_embed_returns_vector_length_384():
    resp = requests.post(f"{SERVICE_URL}/embed", json={"text": "Hello world"}, timeout=5)
    assert resp.status_code == 200
    body = resp.json()
    assert body["dimension"] == 384
    assert len(body["embedding"]) == 384


@live
def test_embed_is_deterministic():
    a = requests.post(f"{SERVICE_URL}/embed", json={"text": "stability check"}, timeout=5).json()
    b = requests.post(f"{SERVICE_URL}/embed", json={"text": "stability check"}, timeout=5).json()
    assert a["embedding"] == b["embedding"]
