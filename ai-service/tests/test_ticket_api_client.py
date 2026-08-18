import httpx

from app.clients import ticket_api_client


def _client_with_handler(handler):
    return httpx.AsyncClient(transport=httpx.MockTransport(handler))


async def test_fetch_resolved_tickets_filters_out_tickets_without_resolution_notes(monkeypatch):
    def handler(request):
        return httpx.Response(
            200,
            json=[
                {"title": "Screen broken", "description": "Won't turn on", "resolutionNotes": "Replaced cable."},
                {"title": "No notes", "description": "Still open-ish", "resolutionNotes": None},
                {"title": "Missing key", "description": "No resolutionNotes key at all"},
            ],
        )

    monkeypatch.setattr(ticket_api_client, "_client", _client_with_handler(handler))

    result = await ticket_api_client.fetch_resolved_tickets()

    assert result == [{"title": "Screen broken", "description": "Won't turn on", "resolution_notes": "Replaced cable."}]


async def test_fetch_resolved_tickets_returns_empty_list_on_http_error(monkeypatch):
    def handler(request):
        return httpx.Response(500)

    monkeypatch.setattr(ticket_api_client, "_client", _client_with_handler(handler))

    result = await ticket_api_client.fetch_resolved_tickets()

    assert result == []
