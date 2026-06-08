# CLAUDE.md

Guidance for Claude Code (claude.ai/code) when working in this repository.

## Project Overview

HeadBet é uma aplicação de **bolão de futebol** em .NET 10. Usuários criam bolões,
convidam participantes, palpitam nos placares dos jogos de um torneio e disputam um
ranking pontuado. A solution usa o formato `.slnx` (`HeadBet.slnx`) e tem dois projetos:

- **HeadBet.Core** — class library (Domain, Application, Infrastructure). Sem dependência de ASP.NET.
- **HeadBet.Blazor** — ASP.NET Core Blazor Server (MudBlazor, dark theme). Referencia o Core e é o projeto de startup.

Os jogos, times e competições são importados da API pública **football-data.org** através
do **wizard de setup** (`setup-tournament-wizard`), que é o fluxo oficial de cadastro de torneios.

> **Nota:** o repositório também contém um fluxo **experimental** de setup de torneio assistido
> por IA (`Microsoft.Agents.AI`, providers Anthropic/OpenAI — pastas `Infrastructure/AI`,
> entidades `AiModel`/`ChatThread`/`UserApiKey`, páginas `AiModels`/`ApiKeys`). Ele está
> **pausado** e não é o caminho suportado: o wizard é o fluxo oficial. O `TournamentImporter`
> é compartilhado pelos dois.

## Build and Run Commands

```bash
# Build da solution
dotnet build HeadBet.slnx

# Rodar o app Blazor
dotnet run --project src/HeadBet.Blazor

# EF Core migrations (Core hospeda DbContext + Migrations; Blazor é o startup project)
dotnet ef migrations add <Name> --project src/HeadBet.Core --startup-project src/HeadBet.Blazor
dotnet ef database update --project src/HeadBet.Core --startup-project src/HeadBet.Blazor
```

Migrations são aplicadas automaticamente no startup (`app.InitializeAsync()` →
`DatabaseInitializer`, que faz `Migrate()` + seed). O banco é **SQLite** (`headbet.db`,
connection string `AppDbContext` no `appsettings.json`).

## Solution Structure

```
HeadBet.slnx                          Solution (formato XML .slnx, não o .sln legado)
src/
├── HeadBet.Core/                     Class library (sem ASP.NET)
└── HeadBet.Blazor/                   Blazor Server (startup, referencia Core)
```

### HeadBet.Core

```
HeadBet.Core/
├── CoreAssemblyMarker.cs             Marker para DI scanning (handlers, profiles, validators)
├── GlobalUsings.cs                   Configuration, DependencyInjection, Logging
├── DependencyInjection.cs            services.AddCore(config) — registra tudo do Core
├── Domain/
│   ├── Entities/                     14 entidades (Pool, PoolMember, Match, Bet, User, ...)
│   ├── Enums/                        ScoreType, MatchStatus, PoolMemberStatus, PrizeMode, ...
│   ├── Interfaces/                   Serviços de domínio + Repositories/
│   └── Roles.cs                      Constantes de role: USER, ADMIN
├── Application/
│   ├── Commands/                     Subpastas por contexto; namespace flat
│   ├── Queries/                      Subpastas por contexto; namespace flat
│   ├── DTOs/                         Flat (FootballData responses, chaves, results)
│   ├── Profiles/                     AutoMapper (Entity ↔ ViewModel)
│   └── Models/                       ViewModels usadas por Commands/Queries (flat)
├── Infrastructure/
│   ├── Data/                         AppDbContext, AppUnitOfWork, Configurations/, Repositories/, Seeders/
│   ├── AI/                           AgentFactory (Microsoft.Agents.AI) — experimental, pausado
│   ├── Email/                        SmtpEmailSender + EmailQueue + EmailBackgroundService
│   ├── Http/                         FootballDataClient (football-data.org)
│   ├── Identity/                     PasswordHasher (sem HttpContext)
│   ├── Localization/                 NotificationTranslator (keys → títulos pt-BR)
│   ├── Scoring/                      MatchScoringService, PoolRankingCalculator
│   ├── Tournament/                   TournamentImporter (importa competição → times + jogos)
│   ├── SettingsProvider.cs           Lê/grava AppSetting (JSON) no banco
│   └── InviteCodeGenerator.cs
├── Extensions/                       DateTimeExtensions (ToBrt)
└── Migrations/                       EF Core migrations
```

### HeadBet.Blazor

```
HeadBet.Blazor/
├── Program.cs                        AddCore + cookie auth + MudBlazor + Razor Components (InteractiveServer)
├── Components/
│   ├── App.razor, Routes.razor, RedirectToLogin.razor
│   ├── _Imports.razor
│   ├── Pages/                        Por contexto: Account, Pools, Matches, Bets, Ranking,
│   │                                 Users, Teams, Guide, AiModels, ApiKeys
│   ├── Layout/                       MainLayout, NavMenu, UserMenu
│   └── Shared/                       Componentes reutilizáveis (dialogs, badges, cards, PageHeader)
├── Infrastructure/
│   ├── Identity/BlazorUserContext.cs        IUserContext a partir do ClaimsPrincipal
│   ├── Identity/SignInService.cs            Login/logout (cookie auth, endpoints minimal API)
│   ├── Identity/PasswordEndpoints.cs        Endpoints de senha (change/forgot/reset, form post)
│   ├── Data/DatabaseInitializer.cs          Migrate + seed no startup
│   ├── Theming/AppTheme.cs                  Tema MudBlazor (dark)
│   └── BusExtensions.cs / SnackbarExtensions.cs
└── wwwroot/
    ├── app.css, app.js
    └── images/                       logo.png, favicon.png
```

## Architecture

- **Mediator pattern** via `Headsoft.Messaging` (`IBus`, `CommandBase`, `QueryBase`).
- **Repository + UnitOfWork** via `Headsoft.Core` / `Headsoft.Core.Data` (auto-discovery de repositórios).
- **AutoMapper** para Entity ↔ ViewModel; **FluentValidation** para validação (auto-discovery do assembly).
- **Cookie authentication** própria (cookie `HeadBet.Blazor.Auth`, 7 dias, sliding). Data Protection
  persistido em disco (`DataProtection-Keys/`) para sobreviver a recycles de app pool.
- **Cultura** fixada em `pt-BR`.
- **E-mail**: handlers enfileiram na `EmailQueue` (instantâneo) e um `EmailBackgroundService`
  drena e envia via SMTP. Config na seção `Email` do `appsettings.json`.

### Organização de pastas — regra arquitetural

- **Horizontal Slicing** (por tipo técnico) é o padrão: `Entities/`, `Enums/`, `Interfaces/`,
  `DTOs/`, `Profiles/`, `Models/` são flat.
- **Vertical Slicing** (por contexto de domínio) aplica-se apenas em `Commands/` e `Queries/`,
  que geram muitos arquivos por entidade. Cada contexto tem sua subpasta (`Pools/`, `Users/`, etc.).
- **Namespaces permanecem flat** (`HeadBet.Core.Application.Commands`,
  `HeadBet.Core.Application.Queries`) independente da subpasta física.
- **Command = Command + Validator + Handler** no mesmo arquivo `.cs`. Não existe pasta `Validators/`.

## Scoring (Infrastructure/Scoring)

Cada bolão define seus pontos por `ScoreType` (`PoolScoringRule`). Ao finalizar um jogo, o
`MatchScoringService` recomputa o `MatchUserScore` de cada membro ativo comparando o palpite
(`Bet`) com o placar real. Ordem de aplicação da regra (do mais específico ao menos):

| ScoreType | Quando aplica |
|---|---|
| `ExactScore` | Placar exato (mandante e visitante) |
| `WinnerAndWinnerGoals` | Mesmo vencedor (não-empate) **e** mesmo nº de gols do vencedor |
| `WinnerAndDifference` | Mesmo resultado **e** (empate, ou mesma diferença de gols) |
| `WinnerAndLoserGoals` | Mesmo resultado **e** mesmo nº de gols do perdedor |
| `WinnerOnly` | Acertou só o vencedor |
| `Consolation` | Errou o resultado |
| `NoBet` | Não palpitou (ou jogo cancelado/sem placar → 0 pontos) |

`PoolRankingCalculator` ordena o ranking por pontos (desc) com desempate pela contagem de
acertos de cada `ScoreType` (ExactScore primeiro), depois nome. Distribui prêmios por posição
conforme o `PrizeMode`: `Percentage` (percentual sobre o arrecadado — `CollectedAmount`, ou a
estimativa `EntryFee × membros ativos`) ou `Fixed` (valor fixo em R$). Posições empatadas
dividem o prêmio: empate em N posições ocupa os N slots seguintes (ex.: 2 no 1º → posições
1, 1, 3) e o prêmio do grupo é a **soma dos slots ocupados dividida igualmente** entre os empatados.

> **`CollectedAmount` é um campo manual** — só o admin grava/atualiza. **Nunca** é recalculado
> automaticamente por entrada/saída de participantes. O ranking percentual usa `CollectedAmount`
> como base quando preenchido; só cai na estimativa `EntryFee × membros ativos` quando ele é nulo.

## DateTime — UTC + BRT

- **Entities**: sempre `DateTime` (nunca `DateTimeOffset`). SQLite não suporta `DateTimeOffset` em `ORDER BY`.
- **Gravar**: sempre UTC (`DateTime.UtcNow`).
- **ViewModels**: datas já chegam em BRT (UTC-3) — a View nunca chama `.ToLocalTime()`.
- **Conversão**: `DateTimeExtensions.ToBrt()` (`Extensions/DateTimeExtensions.cs`) no AutoMapper profile ou no handler.

## Headsoft Packages (feed privado)

| Package | Uso |
|---|---|
| Headsoft.Core | `Entity<T>`, `OperationResult`, `IRepository`, `IUnitOfWork` |
| Headsoft.Messaging | `IBus`, `CommandBase`, `QueryBase` (Mediator) |
| Headsoft.Core.Data | `RepositoryBase`, `UnitOfWorkBase`, EF Core extensions |
| Headsoft.Core.Web | `ApiControllerBase`, utilitários web |

Namespaces-chave: `Headsoft.Core.Entities`, `Headsoft.Core.Interfaces.Repositories`,
`Headsoft.Core.Interfaces.Data`, `Headsoft.Messaging.Abstractions`,
`Headsoft.Messaging.Abstractions.Commands`, `Headsoft.Messaging.Abstractions.Queries`,
`Headsoft.Messaging.Extensions`.

Para padrões de uso dos pacotes Headsoft, use o código existente do repositório como referência
(entidades, EntityConfig, repositories, commands/queries, páginas Blazor).

## Key Details

- Target framework: **net10.0** (requer .NET 10 SDK). Nullable + implicit usings habilitados.
- UI: **MudBlazor 9** (dark theme), render mode **InteractiveServer**.
- `nuget.config` mapeia `HeadSoft.*` para um feed NuGet privado (Azure DevOps); os demais pacotes
  vêm do nuget.org. **A restauração exige acesso a esse feed** — sem ele o build não compila.
- `appsettings.json` e `appsettings.*.json` são git-ignored — **crie localmente** (precisa de
  connection string, seed do admin, `FootballData:ApiKey`, e seção `Email`).
- Local .NET tool: `ilspycmd` (decompiler IL, via `dotnet-tools.json`).
- `OperationResult` (tipo de retorno dos handlers) — os construtores são obsoletos. Use a
  factory `Result`: `Result.Success()`, `Result.Success(data)`, `Result.Warning(msg, details)`,
  `Result.Error(msg, details)` e as variantes genéricas `Result.Success<T>`, `Result.Warning<T>`,
  `Result.Error<T>`. Para propagar as notificações de um `OperationResult` já existente em outro
  tipo, use `result.Cast<T>()`.

## Project Rules (`.claude/rules/`)

- **build-on-finish**: após qualquer alteração em código (`.cs`, `.csproj`, `.slnx`), rode
  `dotnet build HeadBet.slnx` como passo final. A tarefa só termina com build em 0 erros.
- **const-naming**: todo `const` em C# usa `UPPER_SNAKE_CASE`, independente de visibilidade
  (sobrepõe a convenção PascalCase do .NET).
- **no-git**: nunca execute comandos git (commit, push, add, stash, etc.). O versionamento é
  responsabilidade do desenvolvedor.
- **commit-message-on-finish**: ao terminar uma tarefa que mexe em código, finalize a resposta
  com uma linha curta de commit (Conventional Commits, em pt-BR) num bloco de código para o dev
  copiar. Só gera o texto — não roda git (respeita a `no-git`).
