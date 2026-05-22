# Resilient-AI-Gateway

**Proxy reverso inteligente para Hugging Face Inference API** com alta disponibilidade, failover automático entre modelos, auditoria completa em MongoDB e edge proxy com rate limiting.

Arquitetura de microsserviços orquestrada com Docker Compose, combinando **ASP.NET Core 10**, **Polly 8.x**, **Nginx**, e **MongoDB 7** para um gateway de inferência LLM resiliente e observável.

---

## Índice

- [Stack Tecnológica](#stack-tecnológica)
- [Arquitetura](#arquitetura)
- [Pré-requisitos](#pré-requisitos)
- [Início Rápido](#início-rápido)
- [Endpoints da API](#endpoints-da-api)
- [Pipeline de Resiliência (Polly)](#pipeline-de-resiliência-polly)
- [Modelos de Dados](#modelos-de-dados)
- [Logging e Auditoria (MongoDB)](#logging-e-auditoria-mongodb)
- [Estrutura do Projeto](#estrutura-do-projeto)
- [Configuração](#configuração)
  - [appsettings.json](#appsettingsjson)
  - [Variáveis de Ambiente](#variáveis-de-ambiente)
- [Nginx — Edge Proxy](#nginx--edge-proxy)
- [Desenvolvimento Local](#desenvolvimento-local)
- [Docker](#docker)
- [Segurança](#segurança)
- [Resolução de Problemas](#resolução-de-problemas)

---

## Stack Tecnológica

| Componente | Tecnologia | Versão |
|---|---|---|
| **Backend** | ASP.NET Core (Minimal APIs) | 10.0 |
| **Linguagem** | C# | 13 |
| **Edge Proxy** | Nginx | 1.27-alpine |
| **Resiliência** | Polly (Retry + Timeout + Fallback) | 8.6.6 |
| **Logs** | MongoDB | 7.x |
| **Documentação API** | Scalar + OpenAPI | 2.14.11 |
| **Cliente MongoDB** | MongoDB.Driver | 3.8.1 |
| **Projeto Partilhado** | Class Library .NET 10 | — |
| **Containerização** | Docker + Docker Compose v2 | — |

NuGet packages adicionais:

- `DotNetEnv` 3.2.0 — carregamento de `.env` em desenvolvimento
- `Polly.Core` 8.6.6 — pipeline de resiliência
- `Polly.Extensions` 8.6.6 — logging integrado na pipeline
- `Scalar.AspNetCore` 2.14.11 — UI interativa OpenAPI
- `Microsoft.AspNetCore.OpenApi` 10.0.3 — suporte OpenAPI

---

## Arquitetura

```
                    ┌──────────────────────────────────────────────────────────────┐
                    │                      Docker Compose                          │
                    │                                                              │
                    │  ┌──────┐    ┌──────────────┐    ┌──────────────────────┐   │
                    │  │       │    │              │    │                      │   │
  ┌───────┐         │  │ Nginx│───▶│   API .NET   │───▶│  Hugging Face API    │   │
  │Cliente│────────▶│  │:80   │    │  :8080       │    │  router.hf.co/v1     │   │
  └───────┘         │  │       │    │              │    │                      │   │
   HTTP/1.1         │  └──────┘    └──────┬───────┘    └──────────────────────┘   │
                    │                     │                                        │
                    │                     ▼                                        │
                    │              ┌──────────────┐                               │
                    │              │   MongoDB 7  │                               │
                    │              │  :27017      │                               │
                    │              │              │                               │
                    │              │ request_logs │                               │
                    │              │ error_events │                               │
                    │              │ model_metrics│                               │
                    │              └──────────────┘                               │
                    └──────────────────────────────────────────────────────────────┘
```

### Fluxo de uma requisição

1. **Cliente** → envia `POST /api/v1/inference` para Nginx (`:80`)
2. **Nginx** → aplica rate limiting (30 req/min), adiciona security headers, faz proxy reverso para API .NET (`:8080`)
3. **ApiKeyAuthMiddleware** → valida cabeçalho `X-Gateway-Key`
4. **RequestTimingMiddleware** → mede latência total, loga warnings se >500ms
5. **GatewayService** → constrói pipeline Polly e executa:
   - **Timeout** global (60s)
   - **Retry** (3x, backoff exponencial com jitter)
   - **Fallback** para modelos alternativos
6. **HuggingFaceClient** → chama `chat/completions` na Hugging Face Router API
7. **MongoRequestLogger** (`BackgroundService`) → escreve log em batch no MongoDB (max 100 docs / 1s)
8. **Resposta** → volta pelo mesmo caminho até ao cliente

---

## Pré-requisitos

| Ferramenta | Versão Mínima | Obter |
|---|---|---|
| Docker | 27.x | [docker.com](https://docs.docker.com/engine/install/) |
| Docker Compose | v2 (plugin) | Incluído no Docker Desktop |
| .NET SDK | 10.0 (preview/RC) | [dotnet.microsoft.com](https://dotnet.microsoft.com/download/dotnet/10.0) |
| Git | — | — |

> **Nota**: O .NET 10 está em preview/RC. Verifica a instalação com `dotnet --list-sdks`.

Também precisas de um **Hugging Face API token** gratuito:
1. Cria conta em [huggingface.co](https://huggingface.co)
2. Gera token em [huggingface.co/settings/tokens](https://huggingface.co/settings/tokens)
3. Escolhe um token com acesso de leitura (ou "read")

---

## Início Rápido

```bash
# 1. Clonar o repositório
git clone <repo-url>
cd Resilient-AI-Gateway

# 2. Configurar ambiente (criar .env a partir do exemplo)
cp .env.example .env
# Aguardar para editar .env

# 3. Editar .env com o teu token da Hugging Face
#    HF_API_TOKEN=hf_xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

# 4. Subir tudo (build + start em background)
docker compose up --build -d

# 5. Verificar que os 3 containers estão saudáveis
docker compose ps

# 6. Testar health check
curl http://localhost/health

# 7. Testar inferência
curl -X POST http://localhost/api/v1/inference \
  -H "Content-Type: application/json" \
  -H "X-Gateway-Key: key-client-a" \
  -d '{
    "model": "deepseek-ai/DeepSeek-V4-Pro",
    "inputs": "What is the meaning of life?",
    "parameters": {
      "max_new_tokens": 100,
      "temperature": 0.7,
      "top_p": 0.9
    }
  }'

# 8. Parar tudo
docker compose down

# 9. Para destruir também o volume MongoDB (apaga logs)
docker compose down -v
```

---

## Endpoints da API

### `POST /api/v1/inference`

Endpoint principal de inferência. Aceita um prompt e encaminha para a Hugging Face com resiliência e failover automático.

**Headers:**

| Header | Obrigatório | Descrição |
|---|---|---|
| `Content-Type` | ✅ | `application/json` |
| `X-Gateway-Key` | ✅ | API Key de autenticação |

**Body (JSON):**

```json
{
  "model": "deepseek-ai/DeepSeek-V4-Pro",
  "inputs": "Explain quantum computing in simple terms",
  "parameters": {
    "max_new_tokens": 200,
    "temperature": 0.7,
    "top_p": 0.9
  }
}
```

| Campo | Tipo | Obrigatório | Descrição |
|---|---|---|---|
| `model` | `string` | ✅ | ID do modelo na Hugging Face (ex: `deepseek-ai/DeepSeek-V4-Pro`) |
| `inputs` | `string` | ✅ | Prompt de texto para o modelo |
| `parameters.max_new_tokens` | `int?` | ❌ | Máximo de tokens a gerar (default: definido pelo modelo) |
| `parameters.temperature` | `float?` | ❌ | Controla criatividade (0.0–1.0+). Valores mais altos = mais aleatório |
| `parameters.top_p` | `float?` | ❌ | Nucleus sampling (0.0–1.0) |

**Resposta 200 (Sucesso):**

```json
{
  "request_id": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "model_used": "deepseek-ai/DeepSeek-V4-Pro",
  "generated_text": "Quantum computing harnesses quantum mechanics...",
  "latency_ms": 3420,
  "error": null,
  "message": null,
  "retry_after_seconds": null
}
```

**Resposta 503 (Todos os modelos indisponíveis):**

```json
{
  "request_id": "b2c3d4e5-f6a7-8901-bcde-f12345678901",
  "model_used": "deepseek-ai/DeepSeek-V4-Pro",
  "error": "AllModelsUnavailable",
  "message": "Todos os modelos configurados estão indisponíveis no momento.",
  "retry_after_seconds": 30,
  "latency_ms": 15420
}
```

**Resposta 500 (Erro interno):**

```json
{
  "request_id": "c3d4e5f6-a7b8-9012-cdef-123456789012",
  "model_used": "deepseek-ai/DeepSeek-V4-Pro",
  "error": "InternalError",
  "message": "Ocorreu um erro interno ao processar a requisição.",
  "latency_ms": 5000
}
```

---

### `GET /health`

Liveness check — indica se a aplicação está viva e a responder.

```json
{
  "status": "Healthy",
  "timestamp": "2025-05-22T10:30:00Z"
}
```

---

### `GET /health/ready`

Readiness check — indica se a aplicação está pronta para receber tráfego.

```json
{
  "status": "Healthy",
  "timestamp": "2025-05-22T10:30:00Z",
  "checks": {
    "database": "Healthy",
    "huggingface_connectivity": "Healthy"
  }
}
```

> **Nota:** Atualmente as verificações internas (DB ping, conectividade HF) retornam "Healthy" sem validação real. Está marcado como `TODO` no código.

---

## Pipeline de Resiliência (Polly)

A pipeline é construída em `ResiliencePipelineFactory.Create()` com três camadas sequenciais:

```
┌──────────────────────────────────────────────────────────────┐
│                    1. Timeout (60s)                          │
│  Cancela automaticamente se a operação exceder 60 segundos  │
└──────────────────────────────────┬───────────────────────────┘
                                   ▼
┌──────────────────────────────────────────────────────────────┐
│              2. Retry (até 3 tentativas)                     │
│                                                              │
│  ┌──────────────┐    ┌──────────────┐    ┌──────────────┐   │
│  │ Tentativa #1  │───▶│ Tentativa #2  │───▶│ Tentativa #3  │   │
│  │ Delay: 200ms  │    │ Delay: 400ms  │    │ Delay: 800ms  │   │
│  └──────────────┘    └──────────────┘    └──────────────┘   │
│                                                              │
│  Backoff: Exponencial com jitter (aleatorização)             │
│  Gatilhos: 429, 503, 504, HttpRequestException,              │
│            TaskCanceledException                             │
└──────────────────────────────────┬───────────────────────────┘
                                   ▼
┌──────────────────────────────────────────────────────────────┐
│              3. Fallback (modelos alternativos)               │
│                                                              │
│  Se todas as retries falharem, tenta cada modelo da lista    │
│  de fallback até um responder com sucesso.                   │
│                                                              │
│  FallbackModels (configurável em appsettings.json):          │
│    1. deepseek-ai/DeepSeek-V4-Pro   (primário)               │
│    2. Qwen/Qwen2.5-7B-Instruct-1M   (fallback #1)           │
│                                                              │
│  Se TODOS falharem → AllModelsUnavailableException → 503     │
└──────────────────────────────────────────────────────────────┘
```

### Comportamento detalhado

| Condição | Ação |
|---|---|
| `200 OK` | Devolve resposta imediatamente |
| `429 Too Many Requests` | Retry com backoff exponencial |
| `503 Service Unavailable` | Retry com backoff exponencial |
| `504 Gateway Timeout` | Retry com backoff exponencial |
| `HttpRequestException` | Retry com backoff exponencial |
| `TaskCanceledException` (timeout) | Retry com backoff exponencial |
| Todas as retries esgotadas | Fallback para modelo alternativo |
| Todos os modelos falham | `AllModelsUnavailableException` → HTTP 503 |
| Timeout global (60s) | Cancela e propaga exceção |

---

## Modelos de Dados

### InferenceRequest / InferenceResponse (API pública)

**Request** — o que o cliente envia:
```csharp
public class InferenceRequest
{
    string Model;                          // ID do modelo (ex: "gpt2")
    string Inputs;                         // Prompt de texto
    InferenceParameters? Parameters;       // Parâmetros opcionais
}

public class InferenceParameters
{
    int? MaxNewTokens;                     // Máximo de tokens a gerar
    double? Temperature;                   // Criatividade (0.0–1.0+)
    double? TopP;                          // Nucleus sampling
}
```

**Response** — o que o cliente recebe:
```csharp
public class InferenceResponse
{
    string RequestId;                      // UUID único da requisição
    string ModelUsed;                      // Modelo que realmente serviu
    bool FallbackActivated;                // Se houve fallback
    string? GeneratedText;                 // Texto gerado
    long LatencyMs;                        // Latência total (ms)
    string? Error;                         // Código de erro (null se sucesso)
    string? Message;                       // Mensagem de erro
    int? RetryAfterSeconds;                // Sugestão de retry (ex: 30)
}
```

### ChatCompletionRequest / ChatCompletionResponse (formato Hugging Face / OpenAI-compatible)

Formato usado internamente para comunicar com a Hugging Face Router API (compatível com OpenAI Chat Completions):

```json
{
  "model": "deepseek-ai/DeepSeek-V4-Pro",
  "messages": [{"role": "user", "content": "Hello"}],
  "max_tokens": 100,
  "temperature": 0.7,
  "top_p": 0.9,
  "stream": false
}
```

### RequestLogDocument (documento MongoDB)

Cada requisição origina um documento na coleção `request_logs`:

```json
{
  "request_id": "uuid",
  "timestamp": "2025-05-22T10:30:00Z",
  "client_id": "key-client-a",
  "endpoint": "/api/v1/inference",
  "http_method": "POST",
  "requested_model": "deepseek-ai/DeepSeek-V4-Pro",
  "model_used": "deepseek-ai/DeepSeek-V4-Pro",
  "fallback_activated": false,
  "retry_attempts": 0,
  "status_code": 200,
  "latency_ms": 3420,
  "hf_latency_ms": null,
  "payload_size_bytes": 145,
  "response_size_bytes": 1024,
  "error": null
}
```

---

## Logging e Auditoria (MongoDB)

### Arquitetura de logging

O logging é **assíncrono e não bloqueante**, implementado com `System.Threading.Channels`:

```
GatewayService ──Log()──▶ Channel<T> (bounded, 1000)
                              │
                              ▼
                   MongoRequestLogger (BackgroundService)
                              │
                   ┌──────────┴──────────┐
                   │  Batch de até 100   │
                   │  ou flush a cada 1s │
                   └──────────┬──────────┘
                              ▼
                      MongoDB (request_logs)
```

### Características

- **Channel com capacidade 1000** — se excedido, descarta os documentos mais antigos (`DropOldest`)
- **SingleReader** — o `BackgroundService` é o único consumidor
- **Batch inserts** — maximiza eficiência de escrita no MongoDB (máx. 100 docs ou 1 segundo)
- **TTL index** — logs expiram automaticamente após 1 hora (`expireAfterSeconds: 3600`) para estudo de tendências sem acumular dados eternamente

### Coleções MongoDB

O script de inicialização (`mongo/init-mongo.js`) cria automaticamente:

| Coleção | Índices | TTL | Finalidade |
|---|---|---|---|
| `request_logs` | `timestamp`, `model_used+status_code`, `client_id+timestamp`, `request_id` (unique) | 1h | Log detalhado de cada requisição |
| `error_events` | `timestamp`, `error_type`, `model_id+timestamp` | 48h | Eventos de erro para análise |
| `model_metrics` | `model_id+date` (unique) | — | Métricas agregadas por modelo/dia |

---

## Estrutura do Projeto

```
Resilient-AI-Gateway/
├── .env.example                          # Template de variáveis de ambiente
├── .gitignore
├── README.md                             # (este ficheiro)
├── compose.yaml                          # Orquestração Docker Compose
├── Resilient-AI-Gateway.sln              # Solução .NET
│
├── Resilient-AI-Gateway/                 # 📦 Projeto principal (ASP.NET Core)
│   ├── Program.cs                        # Entrypoint, DI, middleware pipeline
│   ├── Resilient-AI-Gateway.csproj       # Dependências e configuração de build
│   ├── Resilient-AI-Gateway.http         # Ficheiro .http para testes no Rider/VS
│   ├── Dockerfile                        # Multi-stage build (base → build → publish → final)
│   ├── appsettings.json                  # Configuração principal
│   ├── appsettings.Development.json      # Configuração de desenvolvimento
│   │
│   ├── Configuration/                    # 🛠️ Opções fortemente tipadas (IOptions)
│   │   ├── GatewayOptions.cs             #   ApiKeys[]
│   │   ├── HuggingFaceOptions.cs         #   ApiToken, BaseUrl
│   │   ├── MongoDbOptions.cs             #   ConnectionString, DatabaseName
│   │   └── ResilienceOptions.cs          #   Timeout, Retry, FallbackModels
│   │
│   ├── Endpoints/                        # 🚪 Minimal API endpoints
│   │   ├── InferenceEndpoints.cs         #   POST /api/v1/inference
│   │   └── HealthEndpoints.cs            #   GET /health, GET /health/ready
│   │
│   ├── Middleware/                       # 🔧 Middleware pipeline
│   │   ├── ApiKeyAuthMiddleware.cs       #   Autenticação X-Gateway-Key
│   │   └── RequestTimingMiddleware.cs    #   Medição de latência + slow request warning
│   │
│   ├── Models/                           # 📄 DTOs e modelos
│   │   ├── InferenceRequest.cs           #   Pedido público da API
│   │   ├── InferenceResponse.cs          #   Resposta pública da API
│   │   ├── ChatCompletionRequest.cs      #   Formato OpenAI-compatible (HF)
│   │   ├── ChatCompletionResponse.cs     #   Resposta da HF
│   │   ├── HuggingFaceRequest.cs         #   Formato legacy HF (não usado ativamente)
│   │   └── HuggingFaceResponse.cs        #   Resposta legacy HF
│   │
│   ├── Services/                         # ⚙️ Lógica de negócio
│   │   ├── IGatewayService.cs            #   Interface do gateway
│   │   ├── GatewayService.cs             #   Orquestrador principal com pipeline Polly
│   │   ├── IHuggingFaceClient.cs         #   Interface do cliente HTTP
│   │   └── HuggingFaceClient.cs          #   Chamadas HTTP à API HF
│   │
│   ├── Resilience/                       # 💪 Pipeline Polly
│   │   ├── ResiliencePipelineFactory.cs  #   Fábrica: Timeout → Retry → Fallback
│   │   └── ResilienceContextExtensions.cs#   Context Polly para transporte de request
│   │
│   ├── Logging/                          # 📝 Logging assíncrono
│   │   ├── IRequestLogger.cs             #   Interface
│   │   ├── RequestLogger.cs              #   Implementação (escreve no Channel)
│   │   ├── LoggingChannel.cs             #   Channel<T> bounded (1000, DropOldest)
│   │   ├── MongoRequestLogger.cs         #   BackgroundService (batch inserts)
│   │   └── RequestLogDocument.cs         #   Documento MongoDB
│   │
│   ├── Exceptions/                       # ⚠️ Exceções customizadas
│   │   ├── AllModelsUnavailableException.cs
│   │   └── InvalidApiKeyException.cs
│   │
│   ├── nginx/                            # 🌐 Configuração Nginx
│   │   ├── nginx.conf                    #   Rate limiting, proxy reverso, security headers
│   │   └── ssl/                          #   Certificados SSL (não versionados)
│   │       └── .gitkeep
│   │
│   └── mongo/                            # 🗄️ Scripts MongoDB
│       └── init-mongo.js                 #   Init: coleções, índices, TTLs
│
└── Resilient-AI-Gateway.Shared/          # 🔗 Projeto partilhado (contracts)
    ├── Resilient-AI-Gateway.Shared.csproj
    └── Contracts/
        └── ModelFallbackList.cs          #   Contrato de fallback models
```

---

## Configuração

### appsettings.json

Ficheiro de configuração principal em `Resilient-AI-Gateway/appsettings.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",

  "HuggingFace": {
    "ApiToken": "",
    "BaseUrl": "https://router.huggingface.co/v1"
  },

  "Gateway": {
    "ApiKeys": [
      "key-client-a",
      "key-client-b"
    ]
  },

  "Resilience": {
    "GlobalTimeoutSeconds": 60,
    "MaxRetryAttempts": 3,
    "BaseDelayMs": 200,
    "FallbackModels": [
      "deepseek-ai/DeepSeek-V4-Pro",
      "Qwen/Qwen2.5-7B-Instruct-1M"
    ]
  },

  "MongoDb": {
    "ConnectionString": "mongodb://localhost:27017",
    "DatabaseName": "resilient_gateway"
  }
}
```

### Secções de Configuração

| Secção | Chave | Tipo | Default | Descrição |
|---|---|---|---|---|
| `HuggingFace` | `ApiToken` | `string` | `""` | Token de API da Hugging Face |
| `HuggingFace` | `BaseUrl` | `string` | `https://router.huggingface.co/v1` | Base URL da API (HF Router ou TGI endpoint) |
| `Gateway` | `ApiKeys` | `string[]` | `["key-client-a", "key-client-b"]` | Lista de API Keys válidas |
| `Resilience` | `GlobalTimeoutSeconds` | `int` | `60` | Timeout máximo global (segundos) |
| `Resilience` | `MaxRetryAttempts` | `int` | `3` | Número máximo de retries |
| `Resilience` | `BaseDelayMs` | `int` | `200` | Delay base do backoff exponencial (ms) |
| `Resilience` | `FallbackModels` | `string[]` | `[DeepSeek-V4, Qwen2.5-7B]` | Modelos alternativos para fallback |
| `MongoDb` | `ConnectionString` | `string` | `mongodb://localhost:27017` | Connection string MongoDB |
| `MongoDb` | `DatabaseName` | `string` | `resilient_gateway` | Nome da base de dados |

### Variáveis de Ambiente

As variáveis de ambiente podem **sobrescrever** qualquer valor do `appsettings.json` usando a convenção `__` (double underscore) como separador de secções:

```bash
# Obrigatória
HuggingFace__ApiToken=hf_xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

# Opcionais (com valores default)
HuggingFace__BaseUrl=https://router.huggingface.co/v1
Gateway__ApiKeys__0=key-client-a
Gateway__ApiKeys__1=key-client-b
Resilience__GlobalTimeoutSeconds=60
Resilience__MaxRetryAttempts=3
Resilience__FallbackModels__0=deepseek-ai/DeepSeek-V4-Pro
Resilience__FallbackModels__1=Qwen/Qwen2.5-7B-Instruct-1M
MongoDb__ConnectionString=mongodb://mongo:27017
MongoDb__DatabaseName=resilient_gateway
```

Ficheiro `.env.example` incluído no repositório — copia para `.env` e preenche:

```bash
cp .env.example .env
```

> **Nota:** O `.env` está no `.gitignore` e **nunca deve ser versionado** (contém tokens secretos). Em Docker Compose, o ficheiro `.env` é carregado automaticamente.

---

## Nginx — Edge Proxy

### Configuração (`nginx/nginx.conf`)

O Nginx funciona como **edge proxy**, recebendo tráfego na porta `80` e encaminhando para a API .NET na porta `8080`.

**Funcionalidades:**

| Funcionalidade | Detalhe |
|---|---|
| **Rate Limiting** | 30 requisições por minuto por IP, com burst de 10 |
| **Proxy Reverso** | `proxy_pass` para `http://api_backend` (upstream com keepalive 32 conexões) |
| **Timeouts** | connect: 5s, send: 65s, read: 65s |
| **Security Headers** | `X-Frame-Options: DENY`, `X-Content-Type-Options: nosniff`, `X-XSS-Protection`, `Referrer-Policy: no-referrer` |
| **Gzip** | Compressão de JSON e texto para respostas ≥1000 bytes |
| **Health check bypass** | Rota `/health` com `access_log off` (não polui logs) |
| **HTTP/1.1 keepalive** | Conexões persistentes com o backend |
| **SSL** | Pasta `ssl/` disponível para certificados (HTTPS não configurado por default) |

### Mapa de rotas Nginx

| Path | Ação |
|---|---|
| `/api/` | Proxy para API .NET + rate limiting |
| `/health` | Proxy para API .NET (sem rate limiting, sem access log) |

---

## Desenvolvimento Local

Sem Docker, apenas a API .NET:

```bash
# 1. Garantir que o .NET 10 SDK está instalado
dotnet --list-sdks

# 2. Restaurar dependências
dotnet restore

# 3. Build
dotnet build

# 4. Executar (serve em http://localhost:5243)
dotnet run --project Resilient-AI-Gateway/Resilient-AI-Gateway.csproj
```

> ⚠️ **Atenção**: Em desenvolvimento local sem Docker, o MongoDB precisa de estar a correr em `localhost:27017` para o logging funcionar. Alternativamente, define `MongoDb__ConnectionString` para um valor vazio ou comentar o `MongoRequestLogger` no `Program.cs`.

### Testar com o ficheiro .http

O projeto inclui um ficheiro `Resilient-AI-Gateway.http` compatível com JetBrains Rider e VS Code (REST Client). Abre-o no IDE e clica em "Send Request" para cada cenário:

- Health Check
- Readiness Check
- Inference — Success
- Inference — Unauthorized (missing API key)
- Inference — Unauthorized (invalid API key)

---

## Docker

### Comandos úteis

```bash
# Build e start (modo detached)
docker compose up --build -d

# Ver logs em tempo real
docker compose logs -f

# Ver logs de um serviço específico
docker compose logs -f resilient-ai-gateway
docker compose logs -f nginx
docker compose logs -f mongo

# Parar serviços
docker compose down

# Parar + apagar volumes (destrói dados MongoDB)
docker compose down -v

# Reconstruir sem cache
docker compose build --no-cache

# Escalar (apenas API, se necessário)
docker compose up -d --scale resilient-ai-gateway=2
```

### Serviços Docker Compose

| Serviço | Container | Imagem | Portas | Depende de |
|---|---|---|---|---|
| `resilient-ai-gateway` | — | build local | `8080:8080` | `mongo` (health) |
| `nginx` | `gateway_nginx` | `nginx:1.27-alpine` | `80:80` | `resilient-ai-gateway` (health) |
| `mongo` | `gateway_mongo` | `mongo:7` | `27017:27017` | — |

### Healthchecks

Todos os serviços têm health checks configurados:

- **MongoDB**: `mongosh --eval "db.adminCommand('ping')"` (a cada 10s, 5 retries)
- **API .NET**: `dotnet --info` (a cada 30s, 3 retries, start period 15s)
- **Nginx**: depende do health da API .NET

---

## Segurança

| Medida | Implementação |
|---|---|
| **Autenticação** | `ApiKeyAuthMiddleware` valida cabeçalho `X-Gateway-Key` contra lista de keys configurada |
| **Rotas públicas** | `/health`, `/openapi/v1.json`, `/scalar/v1/api-docs` não exigem autenticação |
| **Security headers** | Adicionados pelo Nginx: `X-Frame-Options`, `X-Content-Type-Options`, `X-XSS-Protection`, `Referrer-Policy` |
| **Rate limiting** | Nginx limita a 30 req/min por IP com burst de 10 |
| **Secrets** | `.env` no `.gitignore`; tokens em variáveis de ambiente ou `appsettings.json` não versionado |
| **SSL/TLS** | Pasta `nginx/ssl/` preparada para certificados (não configurado por default) |
| **Container isolation** | Rede `gateway-net` privada do Docker, apenas Nginx expõe porta pública |

### API Keys

Por omissão, duas API Keys estão pré-configuradas no `appsettings.json`:

- `key-client-a`
- `key-client-b`

Para produção, **deves alterar estas keys** para valores seguros (idealmente via variáveis de ambiente ou segredo externo).

---

## Resolução de Problemas

### Erro: `Unable to find a matching version of package 'xxx'`

O .NET 10 está em preview. Alguns packages podem não ter versões oficiais para `net10.0`. Tenta:

```bash
dotnet restore
```

Se falhar, usa as imagens Docker que já contêm o SDK correto.

### Erro: `Cannot find .NET SDK 10.0`

Verifica a instalação:

```bash
dotnet --list-sdks
```

Se não aparecer o 10.0, descarrega em [dotnet.microsoft.com](https://dotnet.microsoft.com/download/dotnet/10.0).

### Erro: Connection refused ao MongoDB

Certifica-te que o MongoDB está a correr:

```bash
# Com Docker
docker compose ps

# Ver logs
docker compose logs mongo
```

### Erro: `401 Unauthorized` da Hugging Face

O token da Hugging Face pode estar inválido ou sem permissões. Verifica:

1. Que o `.env` tem o token correto: `HuggingFace__ApiToken=hf_xxxx`
2. Que o token é válido em huggingface.co/settings/tokens
3. Em Docker, os logs: `docker compose logs resilient-ai-gateway`

### Respostas lentas (slow requests)

O `RequestTimingMiddleware` loga warnings para requisições que demoram mais de 500ms:

```
warn: Slow request: POST /api/v1/inference took 3420ms
```

Isto é esperado para inferência de LLMs. Ajusta o limiar no código se necessário.

---