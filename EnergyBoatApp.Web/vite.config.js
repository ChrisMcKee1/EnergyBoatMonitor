import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// https://vitejs.dev/config/
export default defineConfig({
  plugins: [react()],
  optimizeDeps: {
    include: [
      '@opentelemetry/api',
      '@opentelemetry/core',
      '@opentelemetry/resources',
      '@opentelemetry/sdk-trace-web',
      '@opentelemetry/sdk-trace-base',
      '@opentelemetry/context-zone',
      '@opentelemetry/instrumentation',
      '@opentelemetry/instrumentation-document-load',
      '@opentelemetry/instrumentation-fetch',
      '@opentelemetry/instrumentation-xml-http-request',
      '@opentelemetry/exporter-trace-otlp-proto',
      '@opentelemetry/semantic-conventions'
    ]
  },
  server: {
    port: parseInt(process.env.PORT || '5173'),
    proxy: {
      '/api': {
        target: process.env.services__apiservice__https__0 || 
                process.env.services__apiservice__http__0 || 
                'https://localhost:7585',
        changeOrigin: true,
        secure: false,
      }
    }
  }
})
