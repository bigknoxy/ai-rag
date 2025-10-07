import requests


def test_embed_returns_vector_length_384():
    # Phase 0: call local service if available; otherwise assert sample embedding exists
    try:
        resp = requests.post("http://127.0.0.1:8001/embed", json={"text": "Hello world"}, timeout=1)
        assert resp.status_code == 200
        vec = resp.json()
        assert isinstance(vec, list)
        assert len(vec) == 384
    except Exception:
        # Fallback: use samples/sample1.embeddings.json
        import json
        with open("samples/sample1.embeddings.json") as f:
            data = json.load(f)
        # sample contains tiny vector, this test will fail until proper embedding is provided
        assert False, "Embeddings service not available and no valid sample provided"
