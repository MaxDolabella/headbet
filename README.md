<div align="center">
  <img src="src/HeadBet.Blazor/wwwroot/images/logo.png" alt="HeadBet" width="320" />

  <p><strong>Bolão de futebol em .NET 10 + Blazor Server</strong></p>
</div>

---

HeadBet é uma aplicação de bolão de futebol. Crie um bolão, convide a galera, importe os
jogos de um torneio real e dispute um ranking pontuado a cada rodada — quem mais acerta os
placares leva.

> 🤖 **Meu primeiro app em Blazor**, desenvolvido com a ajuda do **Claude** (Anthropic).

## Funcionalidades

- **Bolões** — públicos ou por convite, gratuitos ou pagos, com aprovação automática ou manual de membros.
- **Importação de torneios** — um wizard busca competições, times e jogos da API pública
  [football-data.org](https://www.football-data.org/) e popula o bolão automaticamente.
- **Palpites** — cada participante chuta o placar dos jogos antes de começarem.
- **Pontuação flexível** — cada bolão define quantos pontos vale acertar placar exato, vencedor +
  diferença de gols, só o vencedor, etc.
- **Ranking e prêmios** — classificação com desempate por tipo de acerto e distribuição de
  prêmios por posição (percentual sobre o arrecadado ou valor fixo).
- **Chat em tempo real** — mural geral do bolão e comentários por jogo (na tela de palpite e nos
  detalhes da partida), atualizados ao vivo. Só membros postam; admins moderam.
- **Contas e segurança** — cadastro, login por cookie, recuperação de senha por e-mail, e papéis
  de usuário/administrador.

## Como funciona a pontuação

Ao final de cada jogo, o palpite de cada participante é comparado com o placar real e recebe
a melhor categoria de acerto aplicável:

| Acerto | Descrição |
|---|---|
| **Placar exato** | Cravou mandante e visitante |
| **Vencedor + diferença** | Acertou o resultado e a diferença de gols (ou o empate) |
| **Vencedor + gols do perdedor** | Acertou o resultado e quantos gols o perdedor fez |
| **Apenas o vencedor** | Acertou só quem ganhou |
| **Consolação** | Errou o resultado |
| **Não palpitou** | Deixou o jogo em branco |

Os pontos de cada categoria são configuráveis por bolão. No ranking, empates em pontos são
desfeitos pela quantidade de acertos mais valiosos (placar exato primeiro).

## Stack

- **.NET 10** — C# com nullable reference types
- **Blazor Server** (Interactive Server) + **MudBlazor 9** (tema dark)
- **Entity Framework Core 10** + **SQLite**
- **football-data.org** para dados de competições, times e jogos
- Mediator, Repository/UnitOfWork, AutoMapper e FluentValidation (via pacotes Headsoft)

## Arquitetura

A solution (formato `.slnx`) tem dois projetos:

```
HeadBet.slnx
src/
├── HeadBet.Core/      Class library, sem dependência de ASP.NET
│   ├── Domain/            Entities, Enums, Interfaces (+ Repositories), Roles
│   ├── Application/       Commands, Queries, DTOs, Profiles (AutoMapper), Models (ViewModels)
│   ├── Infrastructure/    EF Core (Data), Scoring, Tournament import, Http, Email, Identity, ...
│   └── Migrations/        EF Core migrations
└── HeadBet.Blazor/    ASP.NET Core Blazor Server (startup, referencia o Core)
    ├── Components/       Pages (por contexto), Layout, Shared
    ├── Infrastructure/   Auth por cookie, theming, inicialização do banco
    └── wwwroot/          Estáticos (logo, css, js)
```

**Padrões e bibliotecas:**

- **Mediator** — Commands e Queries trafegam por um `IBus`; cada handler é resolvido por
  contexto. Um Command carrega seu próprio validator e handler no mesmo arquivo.
- **Repository + Unit of Work** sobre EF Core, com repositórios descobertos por DI.
- **AutoMapper** para Entity ↔ ViewModel e **FluentValidation** para validação de entrada.
- **Autenticação por cookie** própria (sem Identity UI), com papéis de usuário e administrador.
  As chaves de Data Protection são persistidas em disco para sobreviver a reinícios do app.
- **Cultura `pt-BR`** fixada; datas são gravadas em UTC e convertidas para BRT (UTC−3) na borda
  de apresentação.
- **E-mail assíncrono** — operações que disparam e-mail (ex.: recuperação de senha) enfileiram a
  mensagem e um serviço em background a envia por SMTP, sem bloquear a requisição.

**Organização de pastas:** o padrão é *horizontal slicing* (agrupar por tipo técnico —
`Entities/`, `Enums/`, `DTOs/`, etc.). A exceção é `Commands/` e `Queries/`, organizados por
contexto de domínio (`Pools/`, `Users/`, `Matches/`, ...), já que geram muitos arquivos.

> A importação de torneios usada em produção é o **wizard** (football-data.org). Há também um
> fluxo experimental de setup assistido por IA no repositório, atualmente **pausado**.

## Rodando localmente

### Pré-requisitos

- [.NET 10 SDK](https://dotnet.microsoft.com/)
- Uma API key gratuita do [football-data.org](https://www.football-data.org/client/register)
- Acesso ao feed NuGet privado `HeadSoft` (configurado em `nuget.config`)

### Configuração

`appsettings.json` é git-ignored e precisa ser criado localmente em `src/HeadBet.Blazor/`:

```json
{
  "ConnectionStrings": {
    "AppDbContext": "Data Source=headbet.db"
  },
  "FootballData": {
    "ApiKey": "SUA_API_KEY_AQUI"
  },
  "Seeder": {
    "Admin": {
      "Name": "Admin",
      "Email": "admin@exemplo.com",
      "Password": "trocar-esta-senha"
    }
  },
  "Email": {
    "Host": "smtp.exemplo.com",
    "Port": 587,
    "EnableSsl": true,
    "User": "",
    "Password": "",
    "From": "no-reply@exemplo.com",
    "FromName": "HeadBet"
  }
}
```

### Build e execução

```bash
# Build
dotnet build HeadBet.slnx

# Rodar (migrations e seed do admin são aplicados automaticamente no startup)
dotnet run --project src/HeadBet.Blazor
```

Acesse a URL exibida no console e entre com as credenciais do admin definidas no seeder.

### Migrations

```bash
dotnet ef migrations add <Name> --project src/HeadBet.Core --startup-project src/HeadBet.Blazor
dotnet ef database update --project src/HeadBet.Core --startup-project src/HeadBet.Blazor
```

---

<div align="center">
  <sub>HeadBet — feito com ⚽, .NET e 🤖 Claude</sub>
</div>
