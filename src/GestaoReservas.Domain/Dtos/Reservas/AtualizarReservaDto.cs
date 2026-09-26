namespace GestaoReservas.Domain.Dtos.Reservas;

/// <summary>Dados para atualizar uma reserva ativa. O usuário dono da reserva não pode ser alterado.</summary>
/// <param name="LocalId">Local existente e ativo. Opcional quando a reserva tiver ao menos um recurso.</param>
/// <param name="DataInicial">Data de início.</param>
/// <param name="HoraInicial">Hora de início.</param>
/// <param name="DataFinal">Data de término.</param>
/// <param name="HoraFinal">Hora de término (o término deve ser posterior ao início).</param>
/// <param name="RecursosId">Ids dos recursos reservados; substitui a lista atual.</param>
public record AtualizarReservaDto(
    int? LocalId,
    DateOnly DataInicial,
    TimeOnly HoraInicial,
    DateOnly DataFinal,
    TimeOnly HoraFinal,
    IEnumerable<int>? RecursosId);
