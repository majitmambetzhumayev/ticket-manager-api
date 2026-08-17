from pydantic import BaseModel


class ClassifyRequest(BaseModel):
    title: str
    description: str


class ClassifyResponse(BaseModel):
    priority: str
    category: str
    suggested_response: str | None
    grounded_in_history: bool
