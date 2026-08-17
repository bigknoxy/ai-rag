"""Tests for the deterministic embedding service.

Run from the repo root:  venv/bin/python -m pytest embeddings/test_service.py -q
"""
import math

from fastapi.testclient import TestClient

from embeddings.service import app, generate_deterministic_embedding


def _norm(v):
    return math.sqrt(sum(x * x for x in v))


def test_deterministic_is_stable_and_normalized():
    a = generate_deterministic_embedding("hello world", 384)
    b = generate_deterministic_embedding("hello world", 384)
    assert a == b
    assert len(a) == 384
    assert 1e-6 < _norm(a) < 2.0


def test_different_texts_differ():
    assert generate_deterministic_embedding("a", 64) != generate_deterministic_embedding("b", 64)


def test_dimension_parameter():
    vec = generate_deterministic_embedding("x", 64)
    assert len(vec) == 64


def test_embed_endpoint():
    client = TestClient(app)
    resp = client.post("/embed", json={"text": "the quick brown fox"})
    assert resp.status_code == 200
    body = resp.json()
    assert body["dimension"] == 384
    assert _norm(body["embedding"]) > 1e-6


def test_embed_batch_endpoint():
    client = TestClient(app)
    resp = client.post("/embed-batch", json={"texts": ["one", "two", "three"]})
    assert resp.status_code == 200
    body = resp.json()
    assert len(body["embeddings"]) == 3
    assert all(_norm(v) > 1e-6 for v in body["embeddings"])


def test_embed_batch_empty():
    client = TestClient(app)
    resp = client.post("/embed-batch", json={"texts": []})
    assert resp.status_code == 200
    assert resp.json()["embeddings"] == []


def test_embed_batch_rejects_invalid_input():
    client = TestClient(app)
    resp = client.post("/embed-batch", json=42)
    assert resp.status_code == 422


def test_health_endpoint():
    client = TestClient(app)
    resp = client.get("/health")
    assert resp.status_code == 200
    assert resp.json()["status"] == "healthy"
