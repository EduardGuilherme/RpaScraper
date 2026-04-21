# RpaScraper 🤖

> Sistema automatizado de captura e disponibilização de dados externos, construído em **.NET 8** com arquitetura limpa, resiliência integrada e conteinerização via Docker.

---

## Visão Geral da Solução

O sistema é composto por dois serviços independentes que compartilham um repositório em memória via injeção de dependência (no modo standalone) ou comunicação via rede (modo Docker):

```
┌─────────────────────────────────────────────┐
│              Docker Network (rpa-net)        │
│                                             │
│  ┌──────────────┐       ┌────────────────┐  │
│  │  rpa-worker  │       │   rpa-api      │  │
│  │ (Background  │──────▶│ (ASP.NET Core  │  │
│  │  Service)    │  mem  │  Web API)      │  │
│  └──────────────┘       └───────┬────────┘  │
│                                 │ :8080      │
└─────────────────────────────────┼───────────┘
                                  │
                            Client / Browser
                         http://localhost:5000
```

### Componentes

| Projeto | Tipo | Responsabilidade |
|---|---|---|
| `Rpa.Domain` | Class Library | Entidades, interfaces, regras de negócio |
| `Rpa.Infrastructure` | Class Library | Repositório em memória, scraper HTTP, políticas de resiliência |
| `Rpa.Worker` | Worker Service | Background service que executa o scraper periodicamente |
| `Rpa.Api` | ASP.NET Core Web API | Expõe os dados coletados via endpoints REST + Swagger |

### Fonte de Dados

O scraper consome a **API pública do Hacker News via Algolia** (`hn.algolia.com/api/v1`), que oferece um contrato estável baseado em JSON — eliminando fragilidade de parsers HTML contra mudanças de layout. Os 30 itens mais relevantes da página principal são capturados a cada ciclo.

---

## Como Rodar o Projeto

### Pré-requisitos

- [Docker](https://docs.docker.com/get-docker/) ≥ 24
- [Docker Compose](https://docs.docker.com/compose/) ≥ 2.20

### 1. Clonar o repositório

```bash
git clone https://github.com/EduardGuilherme/RpaScraper.git
cd RpaScraper
```

### 2. Subir todos os serviços

```bash
docker compose up --build
```

> Na primeira execução o Docker irá baixar a imagem base do .NET 8 SDK (~200 MB) e compilar os projetos. Nas execuções seguintes o cache de camadas acelera o processo.

### 3. Acessar

| Recurso | URL |
|---|---|
| Swagger UI | http://localhost:5000/swagger |
| Health Check | http://localhost:5000/health |
| Todos os itens | http://localhost:5000/api/news |
| Item por ID | http://localhost:5000/api/news/{id} |
| Por fonte | http://localhost:5000/api/news/source/Hacker%20News |
| Estatísticas | http://localhost:5000/api/news/stats |

### 4. Parar os serviços

```bash
docker compose down
```

### Variáveis de Ambiente

| Variável | Padrão | Descrição |
|---|---|---|
| `Worker__IntervalMinutes` | `15` | Intervalo entre raspagens (minutos) |
| `ASPNETCORE_ENVIRONMENT` | `Production` | Ambiente do ASP.NET Core |

---

## Decisões Arquiteturais

### Clean Architecture em camadas
A separação `Domain → Infrastructure → Application` garante que as regras de negócio (entidades, interfaces) nunca dependem de detalhes de infraestrutura. Trocar a fonte de dados ou persistência requer apenas uma nova implementação de `IScraper` ou `INewsRepository`, sem tocar no domínio.

### Repositório em Memória 
Para o escopo do desafio, optei por um repositório thread-safe em memória (`ConcurrentDictionary<Guid, NewsItem>`). Ele é registrado como **Singleton** e injetado tanto no Worker quanto na API, permitindo que ambos compartilhem o mesmo estado dentro de um processo — ou seja, no modo standalone (sem Docker) funciona perfeitamente. Em produção real, seria substituído por Redis ou PostgreSQL (ver "O que melhoraria").

### API pública em vez de scraping de HTML bruto
Usar a API Algolia do Hacker News ao invés de parsear HTML diretamente elimina a principal fonte de fragilidade em RPAs: mudanças de layout quebrarem os seletores CSS/XPath. O contrato JSON é versionado e estável.

### Resiliência com `Microsoft.Extensions.Http.Resilience` (Polly v8)
Todas as chamadas HTTP do scraper são envolvidas por:
- **Retry** com back-off exponencial + jitter (até 3 tentativas)
- **Circuit Breaker** que abre após falhas consecutivas (evita amplificar problemas em cascata)
- **Timeout** por tentativa (30 s)

### Sobrevivência de falhas no Worker
O `ScraperWorker` captura todas as exceções dentro do ciclo (`try/catch`) e registra o erro sem deixar o host encerrar. Na próxima iteração o robô tenta novamente automaticamente. `OperationCanceledException` (shutdown gracioso) é tratada separadamente.

### Dockerfiles multi-stage
Cada `Dockerfile` usa dois estágios:
1. **`sdk`**: compila e publica o binário
2. **`aspnet`** (runtime apenas): imagem final muito menor (~200 MB vs ~900 MB do SDK)

Além disso, os arquivos `.csproj` são copiados antes do código-fonte para aproveitar o **cache de camadas do Docker** — `dotnet restore` só é re-executado quando as dependências mudam.

### Usuário não-root nos containers
Os containers rodam com um usuário sem privilégios (`appuser`), seguindo a boa prática de segurança para workloads conteinerizados.

---

## O que Melhoraria com Mais Tempo

1. **Persistência real com PostgreSQL + EF Core**  
   Substituir o repositório em memória por um banco de dados relacional garante durabilidade dos dados entre reinicializações e permite consultas mais ricas (paginação, full-text search).

2. **Comunicação entre serviços via message broker**  
   Com Redis Pub/Sub ou RabbitMQ, o Worker publicaria eventos e a API consumiria, desacoplando completamente os dois processos sem depender de estado compartilhado em memória.

3. **Múltiplos scrapers com registro automático**  
   Varredura automática de implementações de `IScraper` via reflection para registrar todas as fontes sem alterar a configuração de DI.

4. **Testes automatizados**  
   - Testes unitários para `ScraperWorker` (com mocks de `IScraper` e `INewsRepository`)
   - Testes de integração para os endpoints da API com `WebApplicationFactory`
   - Testes de contrato para garantir que mudanças na API não quebram clientes

5. **Observabilidade com OpenTelemetry**  
   Métricas (contagem de itens raspados, latência de ciclo), traces distribuídos e logs estruturados exportados para Jaeger/Grafana.

6. **Autenticação na API**  
   JWT Bearer ou API Key para proteger os endpoints em ambientes não-internos.

7. **Paginação e filtros avançados**  
   Parâmetros `page`, `pageSize`, `from`, `to` nos endpoints de listagem para suportar grandes volumes de dados.

---

## Estrutura do Projeto

```
RpaScraper/
├── docker-compose.yml
├── RpaScraper.sln
└── src/
    ├── Rpa.Api/
    │   ├── Controllers/NewsController.cs
    │   ├── Middlewares/GlobalExceptionMiddleware.cs
    │   ├── Dockerfile
    │   └── Program.cs
    ├── Rpa.Domain/
    │   ├── Entities/NewsItem.cs
    │   └── Interfaces/
    │       ├── INewsRepository.cs
    │       └── IScraper.cs
    ├── Rpa.Infrastructure/
    │   ├── Persistence/InMemoryNewsRepository.cs
    │   ├── Resilience/ResiliencePolicies.cs
    │   ├── Scrapers/HackerNewsScraper.cs
    │   └── DependencyInjection.cs
    └── Rpa.Worker/
        ├── Configuration/WorkerOptions.cs
        ├── Services/ScraperWorker.cs
        ├── Dockerfile
        └── Program.cs
```
