# Implementacao-Software-UNISINOS
Sistema de gestão de reservas de locais e recursos, desenvolvido com API REST em .NET 10 (C#) com Entity Framework Core e PostgreSQL, para a cadeira de Implementação de Software da UNISINOS.

## 1. Regras de negócio e de domínio
1. **Escopo da reserva**: uma `Reserva` deve conter pelo menos um `Local` OU pelo menos um `Recurso` (nunca nenhum dos dois). Essa regra cruza duas tabelas (`Reserva` e `ReservaRecurso`) e não pode ser expressa como uma *constraint* de coluna única; é representada no domínio pela propriedade calculada `Reserva.TemEscopoValido` e é validada no `ReservaHandler` (ver seção 2.1).
2. **`PermiteRecursos`**: quando `false`, o `Local` vinculado a uma reserva não pode ter recursos associados na mesma reserva. Regra de aplicação, validada no `ReservaHandler`.
3. **Item físico único**: `Recurso.NumeroPatrimonio` é único no sistema (índice único). O `RecursoHandler` checa a duplicidade antes de gravar (409 com mensagem clara). Lacunas nos ids são normais: no PostgreSQL a sequência de identity não é revertida quando um INSERT falha (ex.: corrida entre duas requisições). Não existe atributo de quantidade em `Recurso` nem em `ReservaRecurso`.
4. **Janela de dias do recurso**: `DiasMinimosReserva` e `DiasMaximosReserva` definem os limites de duração (em dias) permitidos para reservar aquele recurso; a duração efetiva é obtida do cruzamento de `Reserva.DataInicial/HoraInicial` com `Reserva.DataFinal/HoraFinal`. Validada no `ReservaHandler` (ver seção 2.1).
5. **Exclusão lógica (soft delete)**: todas as entidades possuem a flag `Ativo` (booleana). Nenhum registro é fisicamente excluído pela aplicação; "excluir" significa `Ativo = false`.
   - Exceção de modelagem: `ReservaRecurso` é uma tabela de junção pura (sem atributos próprios de negócio) — removê-la representa apenas a desvinculação de um recurso a uma reserva, não a exclusão de uma entidade. Por isso não recebeu a flag `Ativo`.
6. **Status da reserva**: o campo "Ativo/Inativo" da `Reserva` mencionado nas regras de negócio é o mesmo campo `Ativo` do requisito global de soft delete (não há um enum de status separado).
7. **Senha do usuário**: `Usuario.Senha` é gravada em texto puro, sem hash, por ser uma fase inicial (o objetivo é permitir a validação de login na futura interface). Obrigatória no cadastro, opcional na atualização (omitida = mantém a atual) e nunca devolvida pelos endpoints. Trocar por hash antes de uso real.

## 2. Entidades identificadas
| Entidade | Descrição |
|---|---|
| **Usuario** | Pessoa que realiza uma reserva (usuário unificado do sistema). |
| **Categoria** | Classificação reutilizável, aplicável a `Local` ou a `Recurso` (campo `Tipo` diferencia). |
| **Local** | Espaço físico reservável (sala, auditório, etc.). |
| **Recurso** | Item físico único e identificado por Número de Patrimônio (sem conceito de quantidade/estoque). |
| **Reserva** | Registro de uso feito por um `Usuario`, podendo conter um `Local`, `Recurso`(s), ou ambos. |
| **ReservaRecurso** | Entidade associativa da relação N:N entre `Reserva` e `Recurso`. |


### 2.1 Fluxo de reservas (`/api/reservas`)

| Método | Rota | Descrição | Respostas |
|---|---|---|---|
| GET | `/api/reservas?apenasAtivos=true` | Lista reservas (por padrão só as ativas), ordenadas por data/hora inicial. | 200 |
| GET | `/api/reservas/{id}` | Obtém uma reserva, mesmo inativa. | 200, 404 |
| POST | `/api/reservas` | Cria uma reserva. | 201, 400, 404, 409 |
| PUT | `/api/reservas/{id}` | Atualiza local, período e recursos de uma reserva **ativa**. O usuário dono não muda. | 200, 400, 404, 409 |
| DELETE | `/api/reservas/{id}` | Cancelamento lógico (`Ativo = false`), liberando local e recursos. | 204, 404 |

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

## 3. Como executar a API

### 3.1 Pré-requisitos

| Ferramenta | Versão | Observação |
|---|---|---|
| [.NET SDK](https://dotnet.microsoft.com/download/dotnet/10.0) | 10.0 | Conferir com `dotnet --list-sdks`. |
| [PostgreSQL](https://www.postgresql.org/download/) | 13 ou superior | Precisa estar rodando. O usuário usado precisa ter permissão para criar banco (o `postgres` padrão tem). |
| Postman (ou similar) | qualquer | Opcional, para testar os endpoints. |

### 3.2 Estrutura da solução

| Projeto | Responsabilidade |
|---|---|
| `GestaoReservas.Domain` | Entidades, enums, DTOs (`Dtos/`) e interfaces dos providers (`I*Provider`). |
| `GestaoReservas.Infrastructure` | `AppDbContext`, mapeamento Fluent API, implementação dos providers e migrations. |
| `GestaoReservas.WebApi` | Controllers, handlers (regras de negócio), tratamento de erros e `Program.cs`. |

### 3.3 Configurar a conexão com o PostgreSQL

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

### 3.4 Rodar

```bash
dotnet run --project src/GestaoReservas.WebApi --launch-profile http
```

A API sobe em `http://localhost:3000` e o navegador abre direto no Swagger (`/swagger`). Na primeira execução, as migrations são aplicadas automaticamente: o banco `gestao_reservas` e todas as tabelas são criadas, então não é preciso criar nada manualmente no PostgreSQL.

Os dados podem ser consultados pelo pgAdmin ou pelo `psql` (ex.: `SELECT * FROM "Usuarios";`). Os nomes das tabelas e colunas estão em PascalCase e precisam de aspas duplas no SQL.

### 3.5 Criar novas migrations (só quando o modelo mudar)

Quando alguém alterar uma entidade ou uma configuração do Fluent API, precisa gerar uma nova migration e commitá-la junto com a alteração:

```bash
dotnet tool install --global dotnet-ef
dotnet ef migrations add NomeDaMudanca --project src/GestaoReservas.Infrastructure --startup-project src/GestaoReservas.WebApi -o Data/Migrations
```

### 3.6 Problemas comuns

| Erro | Causa provável |
|---|---|
| `28P01: password authentication failed` | Senha errada no User Secrets. Rode de novo o comando da seção 3.3. |
| `Failed to connect to 127.0.0.1:5432` | O serviço do PostgreSQL não está rodando ou usa outra porta. |
| `address already in use` na porta 3000 | Outra instância da API ainda está aberta. Termine o processo e tente novamente. |
| `42501: permission denied to create database` | O usuário configurado não pode criar bancos. Use o `postgres` ou crie o banco `gestao_reservas` manualmente. |
