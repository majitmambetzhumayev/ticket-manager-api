import asyncio
from typing import Literal

import numpy as np
from langchain_openai import ChatOpenAI, OpenAIEmbeddings
from pydantic import BaseModel, Field

from app.clients.ticket_api_client import fetch_resolved_tickets
from app.graph.state import GraphState

SIMILARITY_THRESHOLD = 0.55
MAX_SIMILAR_TICKETS = 3

# Built once per process and reused across requests: these wrappers own an
# HTTP client each, so constructing them per-invocation would drop connection
# pooling/keep-alive to OpenAI on every single ticket.
_classifier_llm = ChatOpenAI(model="gpt-4o-mini", temperature=0)
_responder_llm = ChatOpenAI(model="gpt-4o-mini", temperature=0.3)
_embeddings = OpenAIEmbeddings(model="text-embedding-3-small")


class ClassificationResult(BaseModel):
    priority: Literal["Low", "Medium", "High", "Critical"] = Field(
        description="Urgency of the ticket for the support team."
    )
    category: str = Field(
        description="Short category label, e.g. Billing, Shipping, Technical, Account."
    )


def _ticket_text(title: str, description: str) -> str:
    return f"{title}\n{description}"


async def classify(state: GraphState) -> dict:
    structured_llm = _classifier_llm.with_structured_output(ClassificationResult)

    result = await structured_llm.ainvoke(
        f"Classify this support ticket.\n\nTitle: {state['title']}\nDescription: {state['description']}"
    )
    return {"priority": result.priority, "category": result.category}


def should_retrieve(state: GraphState) -> Literal["retrieve_similar", "__end__"]:
    return "retrieve_similar" if state["priority"] in ("High", "Critical") else "__end__"


async def retrieve_similar(state: GraphState) -> dict:
    resolved = await fetch_resolved_tickets()
    if not resolved:
        return {"similar_tickets": []}

    query_vector, corpus_vectors = await asyncio.gather(
        _embeddings.aembed_query(_ticket_text(state["title"], state["description"])),
        _embeddings.aembed_documents([_ticket_text(t["title"], t["description"]) for t in resolved]),
    )

    scored = sorted(
        ((ticket, _cosine_similarity(query_vector, vector)) for ticket, vector in zip(resolved, corpus_vectors)),
        key=lambda pair: pair[1],
        reverse=True,
    )
    top_matches = [ticket for ticket, score in scored[:MAX_SIMILAR_TICKETS] if score >= SIMILARITY_THRESHOLD]

    return {"similar_tickets": top_matches}


async def suggest_response(state: GraphState) -> dict:
    similar_tickets = state.get("similar_tickets") or []

    if similar_tickets:
        context = "\n".join(f"- {t['title']}: {t['resolution_notes']}" for t in similar_tickets)
        prompt = (
            "Draft a suggested support response based on these similar resolved tickets.\n\n"
            f"Similar tickets:\n{context}\n\n"
            f"Current ticket: {state['title']} - {state['description']}"
        )
    else:
        prompt = (
            "No similar past tickets were found. Draft a cautious, best-effort suggested "
            "response based on general knowledge only, and make explicit that it is not "
            f"based on prior resolutions.\n\nTicket: {state['title']} - {state['description']}"
        )

    response = await _responder_llm.ainvoke(prompt)
    return {"suggested_response": response.content}


def _cosine_similarity(a: list[float], b: list[float]) -> float:
    a_arr, b_arr = np.array(a), np.array(b)
    return float(np.dot(a_arr, b_arr) / (np.linalg.norm(a_arr) * np.linalg.norm(b_arr)))
