# Implementacao-Software-UNISINOS
Sistema de gestão de reservas de locais e recursos, desenvolvido para a cadeira de Implementação de Software da UNISINOS. É composto por:

- **API REST** em .NET 10 (C#) com Entity Framework Core e PostgreSQL, protegida por **token JWT**;
- **Frontend web** em React + TypeScript (Vite), com telas de login e cadastro completo de todas as entidades.

Documentação complementar:

| Documento | Conteúdo |
|---|---|
| [docs/arquitetura.md](docs/arquitetura.md) | Camadas, fluxo de uma requisição, autenticação, organização do frontend e decisões técnicas. |
| [docs/dicionario-dados.md](docs/dicionario-dados.md) | Tabelas, colunas, tipos e restrições do banco. |

## 1. Regras de negócio e de domínio
1. **Escopo da reserva**: uma `Reserva` deve conter pelo menos um `Local` OU pelo menos um `Recurso` (nunca nenhum dos dois). Essa regra cruza duas tabelas (`Reserva` e `ReservaRecurso`) e não pode ser expressa como uma *constraint* de coluna única; é representada no domínio pela propriedade calculada `Reserva.TemEscopoValido` e é validada no `ReservaHandler` (ver seção 2.1).
2. **`PermiteRecursos`**: quando `false`, o `Local` vinculado a uma reserva não pode ter recursos associados na mesma reserva. Regra de aplicação, validada no `ReservaHandler`.
3. **Item físico único**: `Recurso.NumeroPatrimonio` é único no sistema (índice único). O `RecursoHandler` checa a duplicidade antes de gravar (409 com mensagem clara). Não existe atributo de quantidade em `Recurso` nem em `ReservaRecurso`.
4. **Janela de dias do recurso**: `DiasMinimosReserva` e `DiasMaximosReserva` definem os limites de duração (em dias) permitidos para reservar aquele recurso; a duração efetiva é obtida do cruzamento de `Reserva.DataInicial/HoraInicial` com `Reserva.DataFinal/HoraFinal`. Validada no `ReservaHandler` (ver seção 2.1).
5. **Exclusão lógica (soft delete)**: todas as entidades possuem a flag `Ativo` (booleana). Nenhum registro é fisicamente excluído pela aplicação; "excluir" significa `Ativo = false`.
   - Exceção de modelagem: `ReservaRecurso` é uma tabela de junção pura (sem atributos próprios de negócio) — removê-la representa apenas a desvinculação de um recurso a uma reserva, não a exclusão de uma entidade. Por isso não recebeu a flag `Ativo`.
6. **Status da reserva**: o campo "Ativo/Inativo" da `Reserva` mencionado nas regras de negócio é o mesmo campo `Ativo` do requisito global de soft delete (não há um enum de status separado). No frontend, uma reserva ativa ainda é exibida como *Agendada*, *Em andamento* ou *Encerrada* conforme a data/hora atual — é só apresentação, calculada a partir do período.
7. **Senha do usuário**: `Usuario.Senha` é gravada em texto puro, sem hash, por ser uma fase inicial. É usada para validar o login (`POST /api/auth/token`, ver seção 2.2). Obrigatória no cadastro, opcional na atualização (omitida = mantém a atual) e nunca devolvida pelos endpoints. Trocar por hash antes de uso real.
8. **Acesso autenticado**: todas as rotas exigem um token JWT válido, exceto o login e o cadastro de usuário. Usuário desativado não consegue fazer login. Não há perfis/papéis: qualquer usuário autenticado pode manter todos os cadastros (ver limitações em [docs/arquitetura.md](docs/arquitetura.md#7-limitações-conhecidas-e-próximos-passos)).

## 2. Entidades identificadas
| Entidade | Descrição |
|---|---|
| **Usuario** | Pessoa que realiza uma reserva (usuário unificado do sistema) e que se autentica na API. |
| **Categoria** | Classificação reutilizável, aplicável a `Local` ou a `Recurso` (campo `Tipo` diferencia). |
| **Local** | Espaço físico reservável (sala, auditório, etc.). |
| **Recurso** | Item físico único e identificado por Número de Patrimônio (sem conceito de quantidade/estoque). |
| **Reserva** | Registro de uso feito por um `Usuario`, podendo conter um `Local`, `Recurso`(s), ou ambos. |
| **ReservaRecurso** | Entidade associativa da relação N:N entre `Reserva` e `Recurso`. |

Os cadastros de Usuários, Categorias, Locais e Recursos seguem o mesmo padrão de rotas: `GET /api/{entidade}` (lista, por padrão só ativos), `GET /api/{entidade}/{id}`, `POST`, `PUT /{id}` e `DELETE /{id}` (exclusão lógica). A lista completa, com exemplos, está no Swagger (seção 3.4).

### 2.1 Fluxo de reservas (`/api/reservas`)

| Método | Rota | Descrição | Respostas |
|---|---|---|---|
| GET | `/api/reservas?apenasAtivos=true` | Lista reservas (por padrão só as ativas), ordenadas por data/hora inicial. | 200, 401 |
| GET | `/api/reservas/{id}` | Obtém uma reserva, mesmo inativa. | 200, 401, 404 |
| POST | `/api/reservas` | Cria uma reserva. | 201, 400, 401, 404, 409 |
| PUT | `/api/reservas/{id}` | Atualiza local, período e recursos de uma reserva **ativa**. O usuário dono não muda. | 200, 400, 401, 404, 409 |
| DELETE | `/api/reservas/{id}` | Cancelamento lógico (`Ativo = false`), liberando local e recursos. | 204, 401, 404 |

Corpo do POST/PUT (datas em `yyyy-MM-dd`, horas em `HH:mm:ss`; `UsuarioId` só existe no POST):

```json
{
  "usuarioId": 1,
  "localId": 2,
  "dataInicial": "2026-10-10",
  "horaInicial": "08:00:00",
  "dataFinal": "2026-10-10",
  "horaFinal": "12:00:00",
  "recursosId": [3, 4]
}
```

Validações executadas em `ReservaHandler` (na criação e na atualização), nesta ordem:

1. **Usuário** (só na criação): deve existir (404) e estar ativo (400).
2. **Período**: o término deve ser posterior ao início (400). O início não pode estar no passado; na atualização essa checagem só vale se o início foi alterado, para não bloquear a edição de reservas em andamento.
3. **Escopo**: é obrigatório informar `localId` e/ou ao menos um recurso (400). Ids de recurso repetidos são desconsiderados.
4. **Local** (se informado): deve existir (404) e estar ativo (400). Se houver recursos, o local precisa ter `PermiteRecursos = true` (400).
5. **Recursos**: cada um deve existir (404) e estar ativo (400). A duração da reserva em dias, **arredondada para cima** (1 dia e 2 horas = 2 dias), deve estar entre `DiasMinimosReserva` e `DiasMaximosReserva` do recurso (400).
6. **Conflito de horário**: o local e cada recurso não podem estar em outra reserva **ativa** cujo intervalo se sobreponha (409). Intervalos que apenas se encostam (término = início da outra) não conflitam. Na atualização, a própria reserva é ignorada.

Outras regras:
- Reserva inativa não pode ser atualizada (400); pode ser consultada.
- Cancelar uma reserva não é bloqueado por nenhuma regra.
- Os recursos da atualização **substituem** a lista atual (vínculos removidos deixam de existir na tabela `ReservaRecursos`).
- Como um `Local` ou `Recurso` só pode ser desativado sem reservas ativas vinculadas (`PossuiReservasAtivasAsync`), cancelar a reserva é o caminho para liberá-los.
- Erros seguem `application/problem+json` (`ProblemDetails`); o 409 usa `ConflitoException`, tratada em `ExceptionHandlingMiddleware`.
- O frontend repete as checagens de período, escopo, `PermiteRecursos`, janela de dias e conflito de horário **apenas como aviso visual** antes do envio; a validação definitiva continua sendo a da API.

### 2.2 Autenticação (`/api/auth`)

A API usa **JWT Bearer**. O cliente troca e-mail e senha por um token e o envia no cabeçalho `Authorization: Bearer {token}` das demais requisições.

| Método | Rota | Autenticação | Descrição | Respostas |
|---|---|---|---|---|
| POST | `/api/auth/token` | pública | Valida e-mail/senha e devolve o token, a data de expiração e o usuário. | 200, 400, 401 |
| GET | `/api/auth/me` | token | Dados do usuário dono do token (usado pelo frontend ao reabrir a página). | 200, 401 |
| POST | `/api/usuarios` | pública | Cadastro de usuário — fica aberto para permitir criar a primeira conta e o auto cadastro. | 201, 400, 409 |
| demais rotas | — | token | Política global (`FallbackPolicy`) exige usuário autenticado. | 401 sem token válido |

Login:

```json
POST /api/auth/token
{ "email": "maria@exemplo.com", "senha": "1234" }
```

```json
200 OK
{
  "token": "eyJhbGciOiJIUzI1NiIs...",
  "expiraEm": "2026-11-20T03:00:00Z",
  "usuario": { "id": 1, "nome": "Maria Silva", "email": "maria@exemplo.com", "ativo": true }
}
```

Regras:
- E-mail inexistente, senha errada e usuário inativo devolvem **a mesma** mensagem (`401 E-mail ou senha inválidos.`), para não revelar quais e-mails existem.
- O token é assinado com HMAC-SHA256 e carrega `sub` (id), `email` e `name` do usuário. Validade padrão de 8 horas (`Jwt:ExpiracaoMinutos`).
- Token ausente, inválido ou expirado → `401` em `ProblemDetails` (`"Sessão expirada. Faça login novamente."` quando expirou).
- Configuração na seção `Jwt` do `appsettings` (ver seção 3.3).

## 3. Como executar a API

### 3.1 Pré-requisitos

| Ferramenta | Versão | Observação |
|---|---|---|
| [.NET SDK](https://dotnet.microsoft.com/download/dotnet/10.0) | 10.0 | Conferir com `dotnet --list-sdks`. |
| [PostgreSQL](https://www.postgresql.org/download/) | 13 ou superior | Precisa estar rodando. O usuário usado precisa ter permissão para criar banco (o `postgres` padrão tem). |
| [Node.js](https://nodejs.org/) | 20.19+ ou 22.12+ | Só para o frontend (seção 4). Versões 20.x anteriores ainda compilam, mas o Vite exibe aviso. |
| Postman (ou similar) | qualquer | Opcional, para testar os endpoints. O arquivo [GestaoReservas.WebApi.http](src/GestaoReservas.WebApi/GestaoReservas.WebApi.http) também serve (VS / VS Code com REST Client). |

### 3.2 Estrutura da solução

| Projeto | Responsabilidade |
|---|---|
| `GestaoReservas.Domain` | Entidades, enums, DTOs (`Dtos/`) e interfaces dos providers (`I*Provider`). |
| `GestaoReservas.Infrastructure` | `AppDbContext`, mapeamento Fluent API, implementação dos providers e migrations. |
| `GestaoReservas.WebApi` | Controllers, handlers (regras de negócio), autenticação JWT, tratamento de erros e `Program.cs`. |
| `GestaoReservas.Web` | Frontend React + TypeScript (Vite). Ver seção 4. Não faz parte da `.slnx` (é um projeto Node). |

### 3.3 Configurar a conexão com o PostgreSQL e o JWT

O `appsettings.json` contém a conexão **sem a senha**, porque esse arquivo vai para o repositório:

```
Host=localhost;Port=5432;Database=gestao_reservas;Username=postgres
```

Informar a própria connection string completa (com senha) no **User Secrets** do .NET. Os segredos ficam só na máquina da pessoa (`%APPDATA%\Microsoft\UserSecrets\` no Windows) e não são commitados. Na raiz do repositório, rodar o comando abaixo trocando `SUA_SENHA`:

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=gestao_reservas;Username=postgres;Password=SUA_SENHA" --project src/GestaoReservas.WebApi
```

Se o seu PostgreSQL usa outra porta, outro usuário ou outro host, ajuste esses valores no mesmo comando. O valor do User Secrets substitui por completo o do `appsettings.json`.

Para conferir o que está salvo:

```bash
dotnet user-secrets list --project src/GestaoReservas.WebApi
```

**Chave do JWT**: em desenvolvimento, `appsettings.Development.json` já traz uma chave (`Jwt:Chave`) para a API subir sem configuração extra. Ela é pública (está no repositório), então qualquer ambiente fora da máquina de desenvolvimento precisa definir a própria chave, com pelo menos 32 caracteres — a API se recusa a iniciar com uma chave menor:

```bash
dotnet user-secrets set "Jwt:Chave" "uma-chave-longa-e-aleatoria-com-32-ou-mais-caracteres" --project src/GestaoReservas.WebApi
```

Outras chaves da seção `Jwt` (em `appsettings.json`): `Emissor`, `Audiencia` e `ExpiracaoMinutos` (padrão 480). A seção `Cors:Origens` lista as origens que podem chamar a API pelo navegador (padrão `http://localhost:3000`, o frontend).

### 3.4 Rodar

```bash
dotnet run --project src/GestaoReservas.WebApi --launch-profile http
```

A API sobe em `http://localhost:5001` e o navegador abre direto no Swagger (`/swagger`). Na primeira execução, as migrations são aplicadas automaticamente: o banco `gestao_reservas` e todas as tabelas são criadas, então não é preciso criar nada manualmente no PostgreSQL.

Para testar rotas protegidas pelo Swagger: crie uma conta em `POST /api/usuarios`, faça login em `POST /api/auth/token`, copie o `token` da resposta, clique em **Authorize** e cole o token. Ele fica salvo entre recarregamentos da página.

Os dados podem ser consultados pelo pgAdmin ou pelo `psql` (ex.: `SELECT * FROM "Usuarios";`). Os nomes das tabelas e colunas estão em PascalCase e precisam de aspas duplas no SQL.

### 3.5 Criar novas migrations (só quando o modelo mudar)

Quando alguém alterar uma entidade ou uma configuração do Fluent API, precisa gerar uma nova migration e commitá-la junto com a alteração:

```bash
dotnet tool install --global dotnet-ef
dotnet ef migrations add NomeDaMudanca --project src/GestaoReservas.Infrastructure --startup-project src/GestaoReservas.WebApi -o Data/Migrations
```

A autenticação não alterou o modelo: o login usa a coluna `Senha` que já existia.

### 3.6 Problemas comuns

| Erro | Causa provável |
|---|---|
| `28P01: password authentication failed` | Senha errada no User Secrets. Rode de novo o comando da seção 3.3. |
| `Failed to connect to 127.0.0.1:5432` | O serviço do PostgreSQL não está rodando ou usa outra porta. |
| `address already in use` na porta 5001 | Outra instância da API ainda está aberta. Termine o processo e tente novamente. |
| `42501: permission denied to create database` | O usuário configurado não pode criar bancos. Use o `postgres` ou crie o banco `gestao_reservas` manualmente. |
| API não inicia: `Configure 'Jwt:Chave'...` | Rodando fora de `Development` sem chave JWT. Defina `Jwt:Chave` (seção 3.3). |
| `401` em todas as rotas | Falta o cabeçalho `Authorization: Bearer {token}`, ou o token expirou. Faça login de novo. |
| Login com usuário antigo sempre falha | Usuários criados antes da coluna `Senha` ficaram com senha vazia. Defina uma senha editando o usuário (`PUT /api/usuarios/{id}`) com outra conta. |

## 4. Frontend web (`GestaoReservas.Web`)

Aplicação React 19 + TypeScript criada com Vite. Estilização própria em CSS puro (sem framework), com tema claro/escuro e layout responsivo. Detalhes de organização em [docs/arquitetura.md](docs/arquitetura.md#5-frontend).

### 4.1 Rodar

Com a API rodando (seção 3.4):

```bash
cd src/GestaoReservas.Web
npm install
npm run dev
```

Abre em `http://localhost:3000` (porta fixa, porque é a origem liberada no CORS da API). Para apontar para outra API, copie `.env.example` para `.env.local` e ajuste `VITE_API_URL` (padrão `http://localhost:5001/api`).

Outros scripts: `npm run build` (gera `dist/` após checagem de tipos), `npm run lint` e `npm run preview` (serve o `dist/` na porta 3000).

### 4.2 Telas

| Tela | O que faz |
|---|---|
| **Acesso** | Entrar ou criar conta. A sessão fica salva no navegador até expirar ou a pessoa clicar em *Sair*. |
| **Início** | Indicadores (reservas vigentes, em andamento, locais e recursos ativos) e as próximas reservas do usuário logado. |
| **Reservas** | Lista com busca, filtro por situação, "Somente minhas" e "Mostrar canceladas". Nova reserva / edição em modal, com aviso de duração considerada, recursos fora da janela de dias e local/recurso **ocupado no período** antes de enviar. Cancelamento com confirmação. A reserva é criada em nome do usuário logado. |
| **Locais**, **Recursos**, **Categorias**, **Usuários** | Listagem com busca e filtros, cadastro e edição em modal, desativação com confirmação e opção de exibir inativos. |

Mensagens de erro da API (`ProblemDetails`) aparecem dentro do formulário, que continua aberto para correção; sucessos aparecem como aviso temporário no canto da tela.
