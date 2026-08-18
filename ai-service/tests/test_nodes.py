import math

import pytest

from app.graph import nodes
from app.graph.nodes import _cosine_similarity, retrieve_similar, should_retrieve


class TestCosineSimilarity:
    def test_identical_vectors_returns_one(self):
        assert _cosine_similarity([1.0, 2.0, 3.0], [1.0, 2.0, 3.0]) == pytest.approx(1.0)

    def test_orthogonal_vectors_returns_zero(self):
        assert _cosine_similarity([1.0, 0.0], [0.0, 1.0]) == pytest.approx(0.0)

    def test_opposite_vectors_returns_minus_one(self):
        assert _cosine_similarity([1.0, 0.0], [-1.0, 0.0]) == pytest.approx(-1.0)


class TestShouldRetrieve:
    @pytest.mark.parametrize("priority", ["High", "Critical"])
    def test_urgent_priority_retrieves_similar(self, priority):
        assert should_retrieve({"priority": priority}) == "retrieve_similar"

    @pytest.mark.parametrize("priority", ["Low", "Medium"])
    def test_non_urgent_priority_ends(self, priority):
        assert should_retrieve({"priority": priority}) == "__end__"


def _unit_vector_with_similarity(x: float) -> list[float]:
    """A 2D unit vector whose cosine similarity to [1.0, 0.0] is exactly x."""
    return [x, math.sqrt(1 - x**2)]


class _FakeEmbeddings:
    """Stands in for OpenAIEmbeddings, which rejects arbitrary attribute patching (it's a pydantic model)."""

    def __init__(self, doc_vectors):
        self._doc_vectors = doc_vectors

    async def aembed_query(self, text):
        return [1.0, 0.0]

    async def aembed_documents(self, texts):
        return self._doc_vectors


class TestRetrieveSimilar:
    async def test_keeps_top_matches_above_threshold_in_similarity_order(self, monkeypatch):
        # Scores: 0.95, 0.80, 0.60 clear the SIMILARITY_THRESHOLD (0.55); 0.56 also
        # clears it but must be dropped by the MAX_SIMILAR_TICKETS (3) cap.
        scores = (0.95, 0.80, 0.60, 0.56)
        tickets = [{"title": f"Ticket {s}", "description": "d", "resolution_notes": "r"} for s in scores]
        vectors = [_unit_vector_with_similarity(s) for s in scores]

        async def fake_fetch():
            return tickets

        monkeypatch.setattr(nodes, "fetch_resolved_tickets", fake_fetch)
        monkeypatch.setattr(nodes, "_embeddings", _FakeEmbeddings(vectors))

        result = await retrieve_similar({"title": "Query", "description": "d"})

        assert result["similar_tickets"] == tickets[:3]

    async def test_excludes_matches_below_threshold_even_with_room_left_under_the_cap(self, monkeypatch):
        included = {"title": "Above threshold", "description": "d", "resolution_notes": "r"}
        excluded = {"title": "Below threshold", "description": "d", "resolution_notes": "r"}

        async def fake_fetch():
            return [included, excluded]

        monkeypatch.setattr(nodes, "fetch_resolved_tickets", fake_fetch)
        monkeypatch.setattr(
            nodes, "_embeddings", _FakeEmbeddings([_unit_vector_with_similarity(0.9), _unit_vector_with_similarity(0.3)])
        )

        result = await retrieve_similar({"title": "Query", "description": "d"})

        assert result["similar_tickets"] == [included]

    async def test_returns_empty_list_when_no_resolved_tickets_exist(self, monkeypatch):
        async def fake_fetch():
            return []

        monkeypatch.setattr(nodes, "fetch_resolved_tickets", fake_fetch)

        result = await retrieve_similar({"title": "Query", "description": "d"})

        assert result["similar_tickets"] == []
