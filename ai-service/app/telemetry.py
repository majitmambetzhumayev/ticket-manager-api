from fastapi import FastAPI
from opentelemetry import trace
from opentelemetry.exporter.otlp.proto.grpc.trace_exporter import OTLPSpanExporter
from opentelemetry.instrumentation.fastapi import FastAPIInstrumentor
from opentelemetry.instrumentation.httpx import HTTPXClientInstrumentor
from opentelemetry.sdk.resources import Resource
from opentelemetry.sdk.trace import TracerProvider
from opentelemetry.sdk.trace.export import BatchSpanProcessor

SERVICE_NAME = "ai-service"


def setup_telemetry(app: FastAPI) -> None:
    resource = Resource.create({"service.name": SERVICE_NAME})
    provider = TracerProvider(resource=resource)
    provider.add_span_processor(BatchSpanProcessor(OTLPSpanExporter()))
    trace.set_tracer_provider(provider)

    FastAPIInstrumentor.instrument_app(app)
    # Instruments every httpx client process-wide, including the one in
    # ticket_api_client.py: outgoing calls to the .NET API get a span and
    # propagate the current trace via the W3C traceparent header, so a
    # request that starts in .NET and calls into this service stays one
    # single distributed trace instead of two disconnected ones.
    HTTPXClientInstrumentor().instrument()
