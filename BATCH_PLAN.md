# Gold-standard completion batches

Every batch must preserve the portal completion contract: a full guide, a unique runnable sample, focused tests, official references, generated page updates, browser checks, a clean dependency audit, and one reviewable pull request.

## Status

- Complete on `main`: .NET, C#, Dependency Injection, Source Generators, ASP.NET Core, Entity Framework Core, Blazor, Minimal APIs, Dapper
- Batch 1 ready for review: Authentication & Authorization, API Security & OWASP, Secrets Management

## Planned batches

1. Security foundations: Authentication & Authorization, API Security & OWASP, Secrets Management
2. HTTP contracts: API Design Best Practices, OpenAPI & Scalar, Webhook Patterns
3. Real-time and RPC: gRPC, SignalR, GraphQL in .NET
4. Data and caching: Redis & Distributed Caching, Multi-Tenancy Patterns, Performance
5. Application structure: Clean Architecture, Vertical Slice Architecture, Modular Monolith
6. Domain design: Domain-Driven Design, CQRS & MediatR, Design Patterns
7. Messaging foundations: Event-Driven Messaging, RabbitMQ, Apache Kafka
8. Distributed workflows: Saga Pattern, Event Sourcing, Background & Hosted Services
9. Reliability: Resilience & Rate Limiting, Health Checks, Observability & OpenTelemetry
10. Quality engineering: Testing, Architecture Testing, Testcontainers for .NET
11. Delivery: Docker, Kubernetes, CI/CD for .NET
12. Infrastructure as code: Terraform, Infrastructure as Code (Bicep), Azure Resource Manager
13. Azure application platform: Azure for .NET, Azure Container Apps, Azure Functions & Serverless
14. Azure integration: Azure API Management, Azure Logic Apps, Azure Event Hubs
15. Azure data and secrets: Azure Data Factory & ETL, Azure Key Vault & Secrets, Feature Flags
16. Cloud architecture: Microservices, .NET Aspire, Orleans
17. AI application stack: Semantic Kernel, Microsoft.Extensions.AI, Model Context Protocol
18. ML and clients: ML.NET, .NET MAUI, Backend for Frontend
19. Architecture practice: Solution Architecture Fundamentals, Architecture Decision Records, Legacy Modernization
20. Platform architecture: Internal Developer Platforms, Cloud Adoption Framework, AWS for .NET
21. Azure Well-Architected core: Azure Well-Architected Framework, WAF: Reliability, WAF: Security
22. Azure Well-Architected efficiency: WAF: Cost Optimization, WAF: Operational Excellence, WAF: Performance Efficiency
23. Edge and operations: YARP — Reverse Proxy, Structured Logging, Interview Questions

## Batch exit criteria

- All batch topics have `contentStatus: complete` and at least 20 realistic reading minutes.
- Each topic references a different sample with `status: complete`, local README, run command, and test command.
- Release builds and tests pass with warnings treated as errors.
- Direct and transitive dependencies report no known vulnerabilities.
- `scripts/verify.mjs`, generated pages, desktop/mobile browser checks, and live smoke tests pass.
- The batch is committed, pushed, and opened as a draft pull request against `main`.
