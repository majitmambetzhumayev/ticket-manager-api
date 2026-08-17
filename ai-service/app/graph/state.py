from typing import TypedDict, Literal

Priority = Literal["Low", "Medium", "High", "Critical"]


class SimilarTicket(TypedDict):
    title: str
    description: str
    resolution_notes: str


class GraphState(TypedDict):
    title: str
    description: str
    priority: Priority
    category: str
    similar_tickets: list[SimilarTicket]
    suggested_response: str | None
