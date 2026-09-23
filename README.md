# Implementacao-Software-UNISINOS
Projeto de desenvolvimento de um aplicativo WEB para a cadeira de Implementação de Software da UNISINOS.

## 1. Regras de negócio e de domínio
1. **Escopo da reserva**: uma `Reserva` deve conter pelo menos um `Local` OU pelo menos um `Recurso` (nunca nenhum dos dois). Essa regra cruza duas tabelas (`Reserva` e `ReservaRecurso`) e não pode ser expressa como uma *constraint* de coluna única; é representada no domínio pela propriedade calculada `Reserva.TemEscopoValido` e será validada na camada de serviço/aplicação.
2. **`PermiteRecursos`**: quando `false`, o `Local` vinculado a uma reserva não pode ter recursos associados na mesma reserva. Regra também de aplicação (Etapa 3), não expressável via *constraint* simples.
3. **Item físico único**: `Recurso.NumeroPatrimonio` é único no sistema (índice único). Não existe atributo de quantidade em `Recurso` nem em `ReservaRecurso`.
4. **Janela de dias do recurso**: `DiasMinimosReserva` e `DiasMaximosReserva` definem os limites de duração (em dias) permitidos para reservar aquele recurso; a duração efetiva é obtida do cruzamento de `Reserva.DataInicial/HoraInicial` com `Reserva.DataFinal/HoraFinal`. Validação de negócio na Etapa 3.
5. **Exclusão lógica (soft delete)**: todas as entidades possuem a flag `Ativo` (booleana). Nenhum registro é fisicamente excluído pela aplicação; "excluir" significa `Ativo = false`.
   - Exceção de modelagem: `ReservaRecurso` é uma tabela de junção pura (sem atributos próprios de negócio) — removê-la representa apenas a desvinculação de um recurso a uma reserva, não a exclusão de uma entidade. Por isso não recebeu a flag `Ativo`.
6. **Status da reserva**: o campo "Ativo/Inativo" da `Reserva` mencionado nas regras de negócio é o mesmo campo `Ativo` do requisito global de soft delete (não há um enum de status separado).

## 2. Entidades identificadas
| Entidade | Descrição |
|---|---|
| **Usuario** | Pessoa que realiza uma reserva (usuário unificado do sistema). |
| **Categoria** | Classificação reutilizável, aplicável a `Local` ou a `Recurso` (campo `Tipo` diferencia). |
| **Local** | Espaço físico reservável (sala, auditório, etc.). |
| **Recurso** | Item físico único e identificado por Número de Patrimônio (sem conceito de quantidade/estoque). |
| **Reserva** | Registro de uso feito por um `Usuario`, podendo conter um `Local`, `Recurso`(s), ou ambos. |
| **ReservaRecurso** | Entidade associativa da relação N:N entre `Reserva` e `Recurso`. |
