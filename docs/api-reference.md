# API Reference

Base URL: `http://localhost` (via Nginx) or `http://localhost:8080` (diretamente)

## POST /api/v1/inference

Envia uma requisição de inferência com failover automático entre modelos.

### Headers

| Header | Obrigatório | Descrição |
|---|---|---|
| `Content-Type` | Sim | `application/json` |
| `X-Gateway-Key` | Sim | Chave de API do cliente |

### Request Body

```json
{
  "model": "gpt2",
  "inputs": "Texto de entrada",
  "parameters": {
    "max_new_tokens": 100,
    "temperature": 0.8,
    "top_p": 0.95
  }
}
```

| Campo | Tipo | Obrigatório | Descrição |
|---|---|---|---|
| `model` | string | Sim | ID do modelo no Hugging Face |
| `inputs` | string | Sim | Texto de entrada |
| `parameters.max_new_tokens` | int | Não | Máximo de tokens a gerar |
| `parameters.temperature` | float | Não | Temperatura da amostragem (0-2) |
| `parameters.top_p` | float | Não | Nucleus sampling |

### Response 200 OK

```json
{
  "request_id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "model_used": "gpt2",
  "fallback_activated": false,
  "generated_text": "Texto gerado pelo modelo...",
  "latency_ms": 923
}
```

### Response 503 Service Unavailable

```json
{
  "request_id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "error": "AllModelsUnavailable",
  "message": "Todos os modelos configurados estão indisponíveis no momento.",
  "retry_after_seconds": 30,
  "latency_ms": 45000
}
```

### Response 401 Unauthorized

```json
{
  "error": "Unauthorized",
  "message": "Missing or invalid API key"
}
```

---

## GET /health

Liveness check. Retorna 200 se o processo está vivo.

### Response 200

```json
{
  "status": "Healthy",
  "timestamp": "2025-05-15T12:00:00Z"
}
```

---

## GET /health/ready

Readiness check. Retorna 200 quando todas as dependências estão disponíveis.

### Response 200

```json
{
  "status": "Healthy",
  "timestamp": "2025-05-15T12:00:00Z",
  "checks": {
    "database": "Healthy",
    "huggingface_connectivity": "Healthy"
  }
}
```

---

## Fluxo de Resiliência

O pipeline Polly executa na seguinte ordem:

1. **Timeout global** (configurável, default 60s)
2. **Retry** (3 tentativas, backoff exponencial com jitter)
   - Gatilhos: `429`, `503`, `504`, `HttpRequestException`, `TaskCanceledException`
3. **Fallback** (iteração sobre modelos configurados)
   - Se todos os fallbacks falharem: `503 AllModelsUnavailable`
