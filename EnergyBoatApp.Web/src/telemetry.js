// OpenTelemetry browser telemetry initialization for .NET Aspire
// Based on Microsoft's BrowserTelemetry playground sample
// https://github.com/dotnet/aspire/tree/main/playground/BrowserTelemetry

import { WebTracerProvider } from '@opentelemetry/sdk-trace-web';
import { ConsoleSpanExporter, SimpleSpanProcessor } from '@opentelemetry/sdk-trace-base';
import { OTLPTraceExporter } from '@opentelemetry/exporter-trace-otlp-proto';
import { ZoneContextManager } from '@opentelemetry/context-zone';
import { registerInstrumentations } from '@opentelemetry/instrumentation';
import { resourceFromAttributes } from '@opentelemetry/resources';
import { ATTR_SERVICE_NAME } from '@opentelemetry/semantic-conventions';
import { DocumentLoadInstrumentation } from '@opentelemetry/instrumentation-document-load';
import { FetchInstrumentation } from '@opentelemetry/instrumentation-fetch';
import { XMLHttpRequestInstrumentation } from '@opentelemetry/instrumentation-xml-http-request';
import { W3CTraceContextPropagator } from '@opentelemetry/core';

/**
 * Parse delimited key=value pairs from environment variables
 * Format: "key1=value1,key2=value2"
 */
function parseDelimitedValues(str) {
  if (!str) return {};
  
  const pairs = str.split(',');
  const result = {};
  
  pairs.forEach(pair => {
    const [key, value] = pair.split('=');
    if (key && value) {
      result[key.trim()] = value.trim();
    }
  });
  
  return result;
}

/**
 * Extract parent span context from W3C traceparent format
 * Format: "00-{trace-id}-{parent-span-id}-{trace-flags}"
 * @param {string} traceParent - W3C traceparent string
 * @returns {Object|null} - Extracted context or null if invalid
 */
function parseTraceParent(traceParent) {
  if (!traceParent) return null;
  
  const parts = traceParent.split('-');
  if (parts.length !== 4) {
    console.warn('⚠️ OpenTelemetry: Invalid traceparent format:', traceParent);
    return null;
  }
  
  return {
    version: parts[0],
    traceId: parts[1],
    parentSpanId: parts[2],
    traceFlags: parts[3],
  };
}

/**
 * Initialize OpenTelemetry for browser telemetry to Aspire dashboard
 * Implementation matches Microsoft's official BrowserTelemetry playground sample
 */
export async function initTelemetry() {
  try {
    console.log('🔍 OpenTelemetry: Starting initialization...');
    
    // Fetch OTLP configuration from API service
    console.log('🔍 OpenTelemetry: Fetching config from /api/telemetry/config');
    const response = await fetch('/api/telemetry/config');
    if (!response.ok) {
      console.error('❌ OpenTelemetry: Unable to fetch telemetry config from API', response.status, response.statusText);
      return null;
    }

    const config = await response.json();
    console.log('📋 OpenTelemetry: Received config:', config);
    
    // Parse trace parent from server to enable distributed tracing
    const traceParentContext = parseTraceParent(config.traceParent);
    if (traceParentContext) {
      console.log('🔗 OpenTelemetry: Trace parent from server:', {
        traceId: traceParentContext.traceId,
        parentSpanId: traceParentContext.parentSpanId,
      });
    }
    
    // If OTLP endpoint is not configured, skip telemetry initialization
    if (!config.otlpEndpoint) {
      console.error('❌ OpenTelemetry: OTLP endpoint not configured');
      return null;
    }

    console.log(`✅ OpenTelemetry: Initializing browser telemetry with proxy endpoint: ${config.otlpEndpoint}`);

    // No headers needed when using the proxy endpoint
    const headers = {};
    console.log('📋 OpenTelemetry: Using CORS proxy - no authentication headers needed');

    // Parse resource attributes and set service name to 'browser' (like Microsoft sample)
    const attributes = parseDelimitedValues(config.resourceAttributes);
    attributes[ATTR_SERVICE_NAME] = 'browser';
    console.log('📋 OpenTelemetry: Resource attributes:', attributes);

    // Create resource with service information (v2.x uses resourceFromAttributes)
    const resource = resourceFromAttributes(attributes);

    // Configure OTLP exporter to use the proxy endpoint
    // The proxy endpoint handles CORS and forwards to the actual OTLP collector
    const otlpUrl = config.otlpEndpoint;  // Already includes /api/telemetry/traces
    const otlpOptions = {
      url: otlpUrl,
      headers: headers,
    };
    console.log('📋 OpenTelemetry: OTLP proxy URL:', otlpUrl);

    // Create span processors (v2.x API change: pass as array in config)
    console.log('🔧 OpenTelemetry: Creating span processors');
    const spanProcessors = [
      // Console exporter for debugging
      new SimpleSpanProcessor(new ConsoleSpanExporter()),
      // OTLP exporter for Aspire dashboard
      new SimpleSpanProcessor(new OTLPTraceExporter(otlpOptions))
    ];

    // Create and configure tracer provider (v2.x: spanProcessors in config)
    console.log('🔧 OpenTelemetry: Creating WebTracerProvider with span processors');
    const provider = new WebTracerProvider({
      resource: resource,
      spanProcessors: spanProcessors,
    });

    // Register provider with ZoneContextManager
    // ZoneContextManager is recommended for browser apps to support async operations
    console.log('🔧 OpenTelemetry: Registering provider with ZoneContextManager');
    provider.register({
      contextManager: new ZoneContextManager(),
      // Use W3C Trace Context propagator for distributed tracing
      propagator: new W3CTraceContextPropagator(),
    });

    // If we have a trace parent from server, set it in the document
    // This allows browser spans to be children of the server span
    if (traceParentContext) {
      console.log('🔗 OpenTelemetry: Setting up distributed trace linking');
      
      // Create a meta tag with traceparent for DocumentLoadInstrumentation
      let metaTag = document.querySelector('meta[name="traceparent"]');
      if (!metaTag) {
        metaTag = document.createElement('meta');
        metaTag.name = 'traceparent';
        document.head.appendChild(metaTag);
      }
      metaTag.content = config.traceParent;
      console.log('✅ OpenTelemetry: Added traceparent meta tag for distributed tracing');
    }

  // Register instrumentations (document load + fetch/XHR for API calls)
  console.log('🔧 OpenTelemetry: Registering instrumentations');
  registerInstrumentations({
    instrumentations: [
      new DocumentLoadInstrumentation(),
      new FetchInstrumentation({
        propagateTraceHeaderCorsUrls: [
          /localhost:.*\/api/,
          /localhost:7585/,
          /localhost:\d+/,
        ],
        clearTimingResources: true,
      }),
      new XMLHttpRequestInstrumentation({
        propagateTraceHeaderCorsUrls: [
          /localhost:.*\/api/,
          /localhost:7585/,
          /localhost:\d+/,
        ],
      }),
    ],
  });

  console.log('✅ OpenTelemetry: Initialization complete!');
  console.log('📊 OpenTelemetry: Traces will be visible in:');
  console.log('   1. This browser console (via ConsoleSpanExporter)');
  console.log(`   2. Aspire dashboard at ${otlpUrl.replace('/v1/traces', '')}`);

  return provider;

  } catch (error) {
    console.error('❌ OpenTelemetry: Failed to initialize telemetry', error);
    return null;
  }
}
