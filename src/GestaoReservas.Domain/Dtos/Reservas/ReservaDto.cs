namespace GestaoReservas.Domain.Dtos.Reservas;

/// <summary>Dados de uma reserva retornados pela API.</summary>
/// <param name="LocalId">Local da reserva; nulo quando a reserva contém apenas recursos.</param>
public record ReservaDto(
    int Id,
    int UsuarioId,
    int? LocalId,
    IEnumerable<int> RecursosId,
    DateOnly DataInicial,
    TimeOnly HoraInicial,
    DateOnly DataFinal,
    TimeOnly HoraFinal,
    bool Ativo);
