# Chat de bolão — design

**Data:** 2026-06-08 · **Status:** aprovado (auto) · **Branch:** feat/implementacao-chat

## Objetivo

Mensagens curtas entre membros de um bolão, em duas telas:

- **Mural do bolão** — em `Pools/Details` (`/pools/{id}`), só para membros.
- **Comentários por jogo** — em `Bets/Index` (`/pools/{poolId}/bets`), um painel expansível por card de jogo.

Tudo isolado **por bolão**: só membros ativos veem e postam. Atualização em **tempo real**.

## Decisões

- **Tempo real:** broadcaster em memória (singleton + pub/sub por contexto). Blazor Server já
  mantém um circuito por usuário; não precisa de SignalR. Handler salva no banco e publica;
  componentes inscritos no contexto recebem e dão `StateHasChanged`.
- **Uma tabela** `ChatMessage` com discriminador `Scope` (`PoolGeneral` | `Match`).
- **Permissões:** membro ativo cria e lê. Sem edição, sem auto-delete. **Apaga** quem é admin do
  bolão (`PoolMemberRole.Admin`) ou admin do app (role `ADMIN`). Delete é soft (`IsDeleted`).

## Modelo de dados — `ChatMessage : Entity<Guid>`

| Campo | Tipo | Papel |
|---|---|---|
| `PoolId` | Guid | sempre amarrado a um bolão |
| `Scope` | `ChatScope` | discriminador (`PoolGeneral`/`Match`) |
| `MatchId` | Guid? | preenchido só quando `Scope = Match` |
| `UserId` | Guid | autor |
| `Text` | string (máx. 500) | a mensagem |
| `CreatedAt` | DateTime (UTC) | data/hora |
| `IsDeleted` | bool | soft delete |
| `DeletedAt` / `DeletedByUserId` | DateTime? / Guid? | trilha do admin |

Navegações: `Pool`, `Match?`, `User`. Índice em `(PoolId, Scope, MatchId, CreatedAt)`.

## Backend (Core)

- `IChatMessageRepository : IRepository<ChatMessage>` + `ChatMessageRepository`.
- `ChatMessageConfig` (`ToTable("ChatMessage")`, índice, navegações).
- `IChatBroadcaster` (Domain) + `ChatBroadcaster` (Infrastructure, singleton).
  Chave de contexto: `match:{matchId}` ou `pool:{poolId}` (`ChatContextKeys.For(...)`).
- `ChatMessageViewModel` (Id, UserId, UserName, Text, CreatedAt em BRT) + `ChatProfile`.
- `PostChatMessageCommand(PoolId, Scope, MatchId?, Text)` — valida membership/limite/coerência,
  salva e publica.
- `DeleteChatMessageCommand(MessageId)` — admin do bolão/app, soft delete + publica.
- `ListChatMessagesQuery(PoolId, Scope, MatchId?)` — últimas 50, cronológica, ignora apagadas.

## UI (Blazor)

- `Shared/ChatPanel.razor` — reutilizável (`PoolId`, `Scope`, `MatchId?`, `CanPost`, `CanModerate`).
  Carrega na inicialização, inscreve no broadcaster em `OnAfterRenderAsync(firstRender)` (só
  interativo, evita duplicar no prerender), desinscreve no `Dispose`. Input com Enter, lixeira
  só pra moderador.
- `Pools/Details`: um `ChatPanel` geral, renderizado só se `IsMember`.
- `Bets/Index`: `MudExpansionPanel` por card de jogo que monta o `ChatPanel` daquele jogo só ao
  expandir (não carrega os 10 de uma vez).
