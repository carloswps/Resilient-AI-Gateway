# Architecture Decision Records

## ADR-001: Minimal APIs vs Controllers

**Decisão**: Utilizar ASP.NET Core Minimal APIs.

**Motivo**: Menor overhead de reflexão, startup mais rápido, código mais conciso. Para um proxy com 2 endpoints bem definidos, Controllers não adicionam valor estrutural.

---

## ADR-002: Polly v2 (ResiliencePipeline) vs v1 (PolicyWrap)

**Decisão**: Utilizar a API v2 do Polly 8.x (`ResiliencePipelineBuilder`).

**Motivo**: API mais composable, testável e alinhada com `IHttpClientFactory`. A API v1 está em modo de manutenção.

---

## ADR-003: OpenAI-compatible API vs Hugging Face native API

**Decisão**: Utilizar o endpoint `chat/completions` do Hugging Face router (`router.huggingface.co/v1`) em vez da API nativa (`api-inference.huggingface.co/models/{id}`).

**Motivo**: O router da HF suporta a mesma interface da OpenAI, permitindo compatibilidade com qualquer cliente OpenAI. Também fornece roteamento inteligente entre providers.

---

## ADR-004: MongoDB para logs de auditoria

**Decisão**: MongoDB para armazenamento de logs e auditoria.

**Motivo**: Documentos de log são heterogéneos. MongoDB oferece schema flexível, índices TTL nativos para expiração automática e alta taxa de escrita.

---

## ADR-005: System.Threading.Channels para log assíncrono

**Decisão**: `Channel<T>` bounded (capacidade 1000, `DropOldest`) para desacoplar escrita de logs da requisição HTTP.

**Motivo**: Garante que a latência de escrita no MongoDB nunca impacta o tempo de resposta. O canal bounded com `DropOldest` previne consumo ilimitado de memória em picos.

---

## ADR-006: Nginx como Edge Proxy

**Decisão**: Nginx como proxy de borda, em contentor separado, com configuração montada por volume.

**Motivo**: Configuração declarativa matura para rate limiting, compressão e security headers. Não há necessidade de imagem personalizada — a configuração é montada como volume read-only.

---

## ADR-007: TCP healthcheck vs HTTP healthcheck

**Decisão**: TCP port check (`exec 3<>/dev/tcp/localhost/8080`) para o healthcheck da API no Docker.

**Motivo**: A imagem `aspnet:10.0` não inclui `curl` nem `wget`. TCP check é suficiente para verificar que o processo está a escutar, sem modificar o Dockerfile.

---

## ADR-008: DotNetEnv para variáveis de ambiente

**Decisão**: Biblioteca `DotNetEnv` para carregar `.env` em desenvolvimento.

**Motivo**: O .NET clássico lê `appsettings.json` por padrão mas não carrega `.env` automaticamente. `DotNetEnv` preenche essa lacuna mantendo compatibilidade com Docker Compose (que usa `env_file`).
