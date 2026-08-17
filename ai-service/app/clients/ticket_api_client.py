import logging
import os

import httpx

from app.graph.state import SimilarTicket

logger = logging.getLogger(__name__)

TICKET_API_BASE_URL = os.environ.get("TICKET_API_BASE_URL", "http://api:8080")
REQUEST_TIMEOUT_SECONDS = 12.0

# Shared across requests so repeated calls reuse pooled connections to the
# .NET API instead of paying a fresh TCP handshake every time.
_client = httpx.AsyncClient(timeout=REQUEST_TIMEOUT_SECONDS)


async def fetch_resolved_tickets() -> list[SimilarTicket]:
    """
    Fetches resolved tickets from the .NET API for similarity retrieval.

    Never raises: on timeout, network error, or unexpected response shape,
    logs and returns an empty list. retrieve_similar treats an empty list as
    "no history available" — the graph must stay honest about grounding, so
    a failed fetch here degrades to "no similar tickets", never to a
    fabricated one.
    """
    url = f"{TICKET_API_BASE_URL}/api/tickets"
    try:
        response = await _client.get(url, params={"status": "Resolved"})
        response.raise_for_status()
    except httpx.HTTPError:
        logger.exception("Failed to fetch resolved tickets from %s", url)
        return []

    tickets = response.json()
    return [
        SimilarTicket(
            title=t["title"],
            description=t["description"],
            resolution_notes=t.get("resolutionNotes") or "",
        )
        for t in tickets
        if t.get("resolutionNotes")
    ]
