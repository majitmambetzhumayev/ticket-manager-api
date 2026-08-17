from fastapi import FastAPI

from app.graph.graph import ticket_graph
from app.schemas import ClassifyRequest, ClassifyResponse

app = FastAPI(title="Ticket AI Service")


@app.post("/classify", response_model=ClassifyResponse)
async def classify_ticket(request: ClassifyRequest) -> ClassifyResponse:
    result = await ticket_graph.ainvoke(
        {
            "title": request.title,
            "description": request.description,
            "category": "",
            "similar_tickets": [],
            "suggested_response": None,
        }
    )

    return ClassifyResponse(
        priority=result["priority"],
        category=result["category"],
        suggested_response=result["suggested_response"],
        grounded_in_history=bool(result["similar_tickets"]),
    )


@app.get("/health")
async def health() -> dict:
    return {"status": "ok"}
