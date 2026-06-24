# Design — Log de palpites (`BetLog`)

**Data:** 2026-06-22
**Status:** Aprovado (design)
**Escopo:** Registrar (logar) toda criação e atualização de palpite numa tabela de auditoria append-only, **e** uma tela mínima só para superadmin (`Roles.ADMIN`) visualizar o log filtrando por usuário e/ou bolão.

## Objetivo

Manter um histórico imutável de quando cada usuário criou ou alterou um palpite, com placar antigo e novo. Útil para auditar disputas ("eu não palpitei isso"), entender comportamento e alimentar futuras telas (estatísticas).

## Decisões de design (acordadas)

| Decisão | Escolha | Motivo |
|---|---|---|
| Persistência | **Mesma transação** do palpite (atômico) | Volume de palpites é baixo; auditoria não pode perder registro silenciosamente. |
| Formato | **Tabela `BetLog` específica e tipada** | Simples, fácil de consultar; só palpites por enquanto. |
| Metadados de origem | **Não capturar IP/User-Agent** | Core é livre de ASP.NET (sem `HttpContext`); plumbing não justificado (YAGNI). |

## Modelo de dados

### Entidade `BetLog : Entity<Guid>` (`Domain/Entities/BetLog.cs`)

Append-only. **Sem propriedades de navegação** — um registro de auditoria não deve ser removido por cascade nem depender da existência das linhas relacionadas; as consultas usam os ids diretamente.

| Campo | Tipo | Nota |
|---|---|---|
| `MatchId` | `Guid` | |
| `UserId` | `Guid` | |
| `PoolId` | `Guid` | denormalizado para consultar por bolão sem join |
| `Action` | `BetLogAction` | `Created = 1`, `Updated = 2` |
| `OldHomeScore` | `int?` | null quando `Created` |
| `OldAwayScore` | `int?` | null quando `Created` |
| `NewHomeScore` | `int` | |
| `NewAwayScore` | `int` | |
| `CreatedAt` | `DateTime` | UTC (`DateTime.UtcNow`) |

### Enum `BetLogAction` (`Domain/Enums/BetLogAction.cs`)

`Created = 1`, `Updated = 2`. Sem `Deleted`: o handler atual não permite limpar palpite (exige ambos os placares).

### `BetLogConfig` (`Infrastructure/Data/Configurations/BetLogConfig.cs`)

- `internal sealed`, `ToTable("BetLog")`, chave `Id`.
- Todos os campos `IsRequired()`, exceto `OldHomeScore`/`OldAwayScore` (`IsRequired(false)`).
- Índices **não-únicos** em `(PoolId)` e `(MatchId, UserId)` para consulta. Append-only → nenhum índice único.
- Sem `HasOne(...)` — sem FK/cascade, deliberado.

### Registro no `AppDbContext`

`DbSet<BetLog>` + configuração aplicada (auto-discovery de `IEntityTypeConfiguration`, conforme o resto do projeto).

## Repositório

- `IBetLogRepository : IRepository<BetLog>` (`Domain/Interfaces/Repositories/`).
- `BetLogRepository : RepositoryBase<BetLog>` (`Infrastructure/Data/Repositories/`), auto-descoberto, igual aos demais.

## Fluxo (alteração no `SaveBetCommandHandler`)

Injetar `IBetLogRepository`. Antes de sobrescrever o palpite existente, capturar os valores antigos. Regra de quando logar:

- **Palpite novo** (`existingBet is null`) → adiciona `Bet` + `BetLog` com `Action = Created`, `Old* = null`, `New* =` placar informado.
- **Atualização com mudança real** (`old != new`) → muta `Bet` + adiciona `BetLog` com `Action = Updated`, `Old*` = anterior, `New*` = novo.
- **Re-save idêntico** (`old == new`) → não loga (e o EF não gera UPDATE, pois nada mudou).

O `betLogRepository.AddAsync(...)` ocorre **antes** do `uow.SaveChangesAsync()` já existente, garantindo palpite + log no mesmo commit (atômico). O `scoringService.RecomputeForUserBetAsync(...)` permanece após o save, sem alteração.

## UI de visualização (só superadmin)

Tela mínima de auditoria, no padrão das demais páginas admin (`SqlConsole`).

- **Rota:** `/admin/bet-logs`, com `@attribute [Authorize(Roles = Roles.ADMIN)]`.
- **Link no menu:** dentro da seção "Administração" do `NavMenu` (`<AuthorizeView Roles="@Roles.ADMIN">`).
- **Filtros (ambos opcionais):** dropdown de **Bolão** e dropdown de **Usuário**. Sem filtro → mostra tudo (com cap, abaixo).
- **Colunas:** Data/hora (BRT), Bolão, Jogo (`Mandante x Visitante`), Usuário, Ação (`Criado`/`Atualizado`), Palpite (`— → 2x1` no Created; `2x1 → 3x1` no Updated).
- **Ordenação/limite:** `CreatedAt` desc; cap de 500 linhas, com legenda avisando quando o limite é atingido (volume real é baixo; sem paginação para manter simples).

### Query `GetBetLogsQuery(Guid? PoolId, Guid? UserId)` → `List<BetLogItemViewModel>`

Como a `BetLog` não tem navegações, o handler resolve os nomes por lookup: carrega os logs filtrados (com cap), depois mapeia ids → nomes via repositórios (`User`, `Pool`, `Match` + `Team` para montar `Mandante x Visitante`). Datas convertidas para BRT (`ToBrt()`).

### `BetLogItemViewModel` (`Models/`)

`CreatedAt` (BRT), `PoolName`, `MatchLabel` (`Home x Away`), `UserName`, `Action` (`BetLogAction`), `OldHomeScore`/`OldAwayScore` (int?), `NewHomeScore`/`NewAwayScore` (int).

## Migration

`dotnet ef migrations add AddBetLog --project src/HeadBet.Core --startup-project src/HeadBet.Blazor`. Aplicada automaticamente no startup (`DatabaseInitializer` → `Migrate()`).

## Convenções respeitadas

- DateTime sempre UTC ao gravar; entidades usam `DateTime` (nunca `DateTimeOffset`).
- `OperationResult` via factory `Result`.
- `const` em `UPPER_SNAKE_CASE` (se algum surgir).
- `BetLogConfig` `internal sealed`.

## Verificação

Não há projeto de testes no repositório. Validação:

1. `dotnet build HeadBet.slnx` em 0 erros.
2. Checagem manual via script descartável consultando `headbet.db`:
   - Salvar palpite novo → 1 linha `Created` (Old* nulos).
   - Atualizar para placar diferente → 1 linha `Updated` (Old* = anterior).
   - Re-salvar placar idêntico → nenhuma linha nova.

## Fora de escopo (fila futura)

- Paginação/exportação da tela de log (volume baixo dispensa por ora).
- Logar outras entidades (extrair `AuditLog` genérico) — só se a necessidade aparecer.
