"""Local deterministic embedding service.

Provides /embed and /embed-batch endpoints that produce unit-normalized
384-dimension vectors using a SHA-256 counter-based stretch of the input
text. The algorithm intentionally mirrors
`AiRag.Api.Services.DeterministicEmbedding` (C#) so that vectors generated
here are interchangeable with the .NET deterministic fallback vectors.
"""
import hashlib
import struct

from fastapi import FastAPI
from pydantic import BaseModel

DEFAULT_DIMENSION = 384
DEFAULT_SALT = ""

app = FastAPI()


class EmbedRequest(BaseModel):
    text: str
    dimension: int = DEFAULT_DIMENSION


class EmbedResponse(BaseModel):
    embedding: list[float]
    dimension: int


class EmbedBatchRequest(BaseModel):
    texts: list[str]
    dimension: int = DEFAULT_DIMENSION


class EmbedBatchResponse(BaseModel):
    embeddings: list[list[float]]
    dimension: int


def generate_deterministic_embedding(
    text: str, dimension: int = DEFAULT_DIMENSION, salt: str = DEFAULT_SALT
) -> list[float]:
    """Generate a deterministic, unit-normalized embedding for the given text.

    Mirrors AiRag.Api.Services.DeterministicEmbedding.Generate: a SHA-256
    counter-based stretch of (salt, text) is reinterpreted as little-endian
    unsigned 32-bit integers mapped to [-1, 1], then L2-normalized.
    """
    if dimension <= 0:
        raise ValueError("dimension must be positive")

    data = (text or "").encode("utf-8")
    salt_bytes = (salt or "").encode("utf-8")
    bytes_needed = dimension * 4
    buffer = bytearray()
    counter = 0
    sha = hashlib.sha256()
    while len(buffer) < bytes_needed:
        counter_bytes = struct.pack("<I", counter)
        sha = hashlib.sha256()
        sha.update(salt_bytes)
        sha.update(counter_bytes)
        sha.update(data)
        buffer += sha.digest()
        counter += 1

    vector = []
    for i in range(dimension):
        u = struct.unpack_from("<I", bytes(buffer), i * 4)[0]
        v01 = u / 4294967295.0
        vector.append(v01 * 2.0 - 1.0)

    norm = sum(x * x for x in vector) ** 0.5
    if norm < 1e-12:
        vector[0] = 1.0
        return vector
    return [x / norm for x in vector]


@app.get("/health")
async def health():
    return {"status": "healthy", "model": "deterministic-hash", "dimension": DEFAULT_DIMENSION}


@app.post("/embed", response_model=EmbedResponse)
async def embed(req: EmbedRequest) -> EmbedResponse:
    """Generate embedding for the given text"""
    embedding = generate_deterministic_embedding(req.text, req.dimension)
    return EmbedResponse(embedding=embedding, dimension=len(embedding))


@app.post("/embed-batch", response_model=EmbedBatchResponse)
async def embed_batch(req: EmbedBatchRequest) -> EmbedBatchResponse:
    """Generate embeddings for multiple texts"""
    embeddings = [generate_deterministic_embedding(t, req.dimension) for t in req.texts]
    return EmbedBatchResponse(embeddings=embeddings, dimension=req.dimension)


if __name__ == "__main__":
    import uvicorn

    uvicorn.run(app, host="127.0.0.1", port=8001)
