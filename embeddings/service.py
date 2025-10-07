from fastapi import FastAPI
from pydantic import BaseModel
from typing import List

app = FastAPI()

class EmbedRequest(BaseModel):
    text: str

@app.post("/embed")
async def embed(req: EmbedRequest) -> List[float]:
    # Phase 0: return a deterministic dummy 384-d vector (zeros)
    return [0.0] * 384

if __name__ == "__main__":
    import uvicorn
    uvicorn.run(app, host="127.0.0.1", port=8001)
