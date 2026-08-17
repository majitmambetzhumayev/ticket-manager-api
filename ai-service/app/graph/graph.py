from langgraph.graph import END, START, StateGraph

from app.graph.nodes import classify, retrieve_similar, should_retrieve, suggest_response
from app.graph.state import GraphState

_workflow = StateGraph(GraphState)

_workflow.add_node("classify", classify)
_workflow.add_node("retrieve_similar", retrieve_similar)
_workflow.add_node("suggest_response", suggest_response)

_workflow.add_edge(START, "classify")
_workflow.add_conditional_edges(
    "classify",
    should_retrieve,
    {"retrieve_similar": "retrieve_similar", "__end__": END},
)
_workflow.add_edge("retrieve_similar", "suggest_response")
_workflow.add_edge("suggest_response", END)

ticket_graph = _workflow.compile()
