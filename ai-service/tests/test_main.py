from unittest.mock import AsyncMock

from fastapi.testclient import TestClient

from app.main import app, ticket_graph

client = TestClient(app)


def _stub_graph(monkeypatch, result):
    monkeypatch.setattr(ticket_graph, "ainvoke", AsyncMock(return_value=result))


def test_classify_maps_graph_result_and_marks_grounded_when_similar_tickets_found(monkeypatch):
    _stub_graph(
        monkeypatch,
        {
            "priority": "High",
            "category": "Technical",
            "suggested_response": "Try restarting the device.",
            "similar_tickets": [{"title": "Screen broken", "description": "...", "resolution_notes": "..."}],
        },
    )

    response = client.post("/classify", json={"title": "Screen broken", "description": "Won't turn on"})

    assert response.status_code == 200
    assert response.json() == {
        "priority": "High",
        "category": "Technical",
        "suggested_response": "Try restarting the device.",
        "grounded_in_history": True,
    }


def test_classify_marks_ungrounded_when_no_similar_tickets_found(monkeypatch):
    _stub_graph(
        monkeypatch,
        {
            "priority": "Low",
            "category": "General",
            "suggested_response": None,
            "similar_tickets": [],
        },
    )

    response = client.post("/classify", json={"title": "Minor issue", "description": "Not urgent"})

    assert response.status_code == 200
    assert response.json()["grounded_in_history"] is False


def test_health_returns_ok():
    response = client.get("/health")

    assert response.status_code == 200
    assert response.json() == {"status": "ok"}
