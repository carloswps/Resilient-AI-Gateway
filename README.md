# Resilient-AI-Gateway

Proxy reverso inteligente para Hugging Face Inference API com alta disponibilidade, failover automático entre modelos e auditoria completa.

## Stack

| Componente | Tecnologia |
|---|---|
| Backend | ASP.NET Core 10 (Minimal APIs) |
| Edge Proxy | Nginx 1.27-alpine |
| Resiliência | Polly 8.x (Retry + Timeout + Fallback) |
| Logs | MongoDB 7.x (escrita assíncrona via Channel) |
| Documentação API | Scalar + OpenAPI |

## Pré-requisitos

- Docker 27.x + Docker Compose v2
- .NET SDK 10.0 (desenvolvimento local)
- Hugging Face API token (grátis em huggingface.co/settings/tokens)

## Início rápido

```bash
# 1. Configurar ambiente
cp .env.example .env
# Editar .env com HF_API_TOKEN real

# 2. Subir tudo
docker compose up --build -d

# 3. Verificar saúde
curl http://localhost/health

# 4. Testar inferência
curl -X POST http://localhost/api/v1/inference \
  -H "Content-Type: application/json" \
  -H "X-Gateway-Key: key-client-a" \
  -d '{
    "model": "gpt2",
    "inputs": "Hello world",
    "parameters": {
      "max_new_tokens": 50,
      "temperature": 0.7
    }
  }'
```

## Endpoints

| Método | Rota | Descrição |
|---|---|---|
| `POST` | `/api/v1/inference` | Inferência com failover automático |
| `GET` | `/health` | Liveness check |
| `GET` | `/health/ready` | Readiness check |

## Arquitetura

```
Cliente → Nginx (:80) → API (.NET :8080) → Hugging Face API
                            ↕
                         MongoDB (logs assíncronos)
```

- Nginx: rate limiting, security headers, proxy reverso
- Polly: timeout global → retry (3x exp backoff + jitter) → fallback entre modelos
- Logging: `Channel<T>` bounded + `BackgroundService` com batch inserts (max 100 docs / 1s flush)

## Variáveis de ambiente

Ver `.env.example` para todas as variáveis disponíveis.

## Desenvolvimento local

```bash
dotnet run --project Resilient-AI-Gateway/Resilient-AI-Gateway.csproj
# Serves on http://localhost:5243
```
