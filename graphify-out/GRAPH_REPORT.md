# Graph Report - .  (2026-05-12)

## Corpus Check
- Corpus is ~4,775 words - fits in a single context window. You may not need a graph.

## Summary
- 77 nodes · 77 edges · 21 communities (14 shown, 7 thin omitted)
- Extraction: 75% EXTRACTED · 21% INFERRED · 4% AMBIGUOUS · INFERRED: 16 edges (avg confidence: 0.78)
- Token cost: 0 input · 0 output

## Community Hubs (Navigation)
- [[_COMMUNITY_Architecture & ADRs|Architecture & ADRs]]
- [[_COMMUNITY_Project Metadata & Agents|Project Metadata & Agents]]
- [[_COMMUNITY_Core Application|Core Application]]
- [[_COMMUNITY_Configuration Options|Configuration Options]]
- [[_COMMUNITY_HuggingFace Service Implementation|HuggingFace Service Implementation]]
- [[_COMMUNITY_Resilience & Response Models|Resilience & Response Models]]
- [[_COMMUNITY_HuggingFace Interface|HuggingFace Interface]]
- [[_COMMUNITY_Resilience Pipeline Factory|Resilience Pipeline Factory]]
- [[_COMMUNITY_Inference Request Model|Inference Request Model]]
- [[_COMMUNITY_HuggingFace Response Model|HuggingFace Response Model]]
- [[_COMMUNITY_Inference Response Model|Inference Response Model]]
- [[_COMMUNITY_HuggingFace Request Model|HuggingFace Request Model]]
- [[_COMMUNITY_Shared Contracts|Shared Contracts]]

## God Nodes (most connected - your core abstractions)
1. `Guia de Desenvolvimento` - 17 edges
2. `Resilient-AI-Gateway Assembly` - 7 edges
3. `HuggingFaceClient` - 5 edges
4. `Program` - 5 edges
5. `Polly Resilience Pipeline` - 5 edges
6. `HuggingFaceClient` - 4 edges
7. `ResilienceOptions` - 4 edges
8. `HuggingFaceRequest` - 4 edges
9. `ASP.NET Core 10 API` - 4 edges
10. `Early Scaffold State` - 4 edges

## Surprising Connections (you probably didn't know these)
- `Guia de Desenvolvimento` --semantically_similar_to--> `Early Scaffold State`  [INFERRED] [semantically similar]
  Resilient-AI-Gateway/Guia_Desenvolvimento.md → AGENTS.md
- `resilient-ai-gateway Docker Service` --references--> `Resilient-AI-Gateway Assembly`  [INFERRED]
  compose.yaml → Resilient-AI-Gateway/obj/Debug/net10.0/Resilient-AI-Gateway.AssemblyInfo.cs
- `Early Scaffold State` --references--> `Resilient-AI-Gateway.Shared Assembly`  [AMBIGUOUS]
  AGENTS.md → Resilient-AI-Gateway.Shared/obj/Debug/net10.0/Resilient-AI-Gateway.Shared.AssemblyInfo.cs
- `ResilienceOptions` --semantically_similar_to--> `ModelFallbackList`  [INFERRED] [semantically similar]
  Resilient-AI-Gateway/Configuration/ResilienceOptions.cs → Resilient-AI-Gateway.Shared/Contracts/ModelFallbackList.cs
- `Early Scaffold State` --conceptually_related_to--> `Resilient-AI-Gateway Assembly`  [INFERRED]
  AGENTS.md → Resilient-AI-Gateway/obj/Debug/net10.0/Resilient-AI-Gateway.AssemblyInfo.cs

## Hyperedges (group relationships)
- **DI Registration** — program_program, i_hugging_face_client_i_hugging_face_client, hugging_face_client_hugging_face_client [EXTRACTED 1.00]
- **Configuration Binding** — program_program, gateway_options_gateway_options, hugging_face_options_hugging_face_options, resilience_options_resilience_options [EXTRACTED 1.00]
- **Fallback Strategy** — resilience_options_resilience_options, model_fallback_list_model_fallback_list, inference_response_inference_response [INFERRED 0.75]
- **Four-Layer Architecture** — guia_nginx_edge_proxy, guia_aspnetcore10_api, guia_polly_resilience_pipeline, guia_mongodb_async_persistence [INFERRED 0.80]
- **Design vs Reality Gap** — guia_desenvolvimento, agents_early_scaffold, assembly_resilient_ai_gateway [INFERRED 0.75]
- **Architectural Decision Records** — guia_adr_001, guia_adr_002, guia_adr_003, guia_adr_004, guia_adr_005 [EXTRACTED 1.00]

## Communities (21 total, 7 thin omitted)

### Community 0 - "Architecture & ADRs"
Cohesion: 0.18
Nodes (17): Guia as Vision Not Implementation, ADR-001: Minimal APIs vs Controllers, ADR-002: Polly v2 vs v1, ADR-003: MongoDB vs SQL, ADR-004: System.Threading.Channels, ADR-005: Nginx vs YARP, API Key Authentication, ASP.NET Core 10 API (+9 more)

### Community 1 - "Project Metadata & Agents"
Cohesion: 0.28
Nodes (8): Early Scaffold State, Resilient-AI-Gateway Assembly, Resilient-AI-Gateway.Shared Assembly, Shared Assembly, resilient-ai-gateway Docker Service, Microsoft.OpenApi, Polly.Core, Polly.Extensions

### Community 2 - "Core Application"
Cohesion: 0.39
Nodes (8): GatewayOptions, HuggingFaceClient, HuggingFaceOptions, HuggingFaceRequest, IHuggingFaceClient, InferenceParameters, InferenceRequest, Program

### Community 3 - "Configuration Options"
Cohesion: 0.29
Nodes (4): GatewayOptions, HuggingFaceOptions, ResilienceOptions, string

### Community 4 - "HuggingFace Service Implementation"
Cohesion: 0.33
Nodes (4): HttpClient, IHuggingFaceClient, JsonSerializerOptions, HuggingFaceClient

### Community 5 - "Resilience & Response Models"
Cohesion: 0.5
Nodes (5): HuggingFaceResponse, InferenceResponse, ModelFallbackList, ResilienceOptions, ResiliencePipelineFactory

## Ambiguous Edges - Review These
- `Resilient-AI-Gateway.Shared Assembly` → `Shared Assembly`  [AMBIGUOUS]
  Resilient-AI-Gateway.Shared/obj/Debug/net10.0/Resilient-AI-Gateway.Shared.AssemblyInfo.cs · relation: semantically_similar_to
- `Resilient-AI-Gateway.Shared Assembly` → `Early Scaffold State`  [AMBIGUOUS]
  AGENTS.md · relation: references
- `Polly.Core` → `Early Scaffold State`  [AMBIGUOUS]
  AGENTS.md · relation: references

## Knowledge Gaps
- **15 isolated node(s):** `HttpClient`, `JsonSerializerOptions`, `InferenceRequest`, `InferenceParameters`, `HuggingFaceResponse` (+10 more)
  These have ≤1 connection - possible missing edges or undocumented components.
- **7 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **What is the exact relationship between `Resilient-AI-Gateway.Shared Assembly` and `Shared Assembly`?**
  _Edge tagged AMBIGUOUS (relation: semantically_similar_to) - confidence is low._
- **What is the exact relationship between `Resilient-AI-Gateway.Shared Assembly` and `Early Scaffold State`?**
  _Edge tagged AMBIGUOUS (relation: references) - confidence is low._
- **What is the exact relationship between `Polly.Core` and `Early Scaffold State`?**
  _Edge tagged AMBIGUOUS (relation: references) - confidence is low._
- **Why does `Guia de Desenvolvimento` connect `Architecture & ADRs` to `Project Metadata & Agents`?**
  _High betweenness centrality (0.079) - this node is a cross-community bridge._
- **Why does `Early Scaffold State` connect `Project Metadata & Agents` to `Architecture & ADRs`?**
  _High betweenness centrality (0.022) - this node is a cross-community bridge._
- **Why does `Resilient-AI-Gateway Assembly` connect `Project Metadata & Agents` to `Architecture & ADRs`?**
  _High betweenness centrality (0.016) - this node is a cross-community bridge._
- **Are the 2 inferred relationships involving `Guia de Desenvolvimento` (e.g. with `Guia as Vision Not Implementation` and `Early Scaffold State`) actually correct?**
  _`Guia de Desenvolvimento` has 2 INFERRED edges - model-reasoned connections that need verification._