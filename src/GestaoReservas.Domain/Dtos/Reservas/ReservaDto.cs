namespace GestaoReservas.Domain.Dtos.Reservas;

/// <summary>Dados de uma reserva retornados pela API.</summary>
/// <param name="Id">Identificador da reserva.</param>
/// <param name="UsuarioId">Usuário dono da reserva.</param>
/// <param name="LocalId">Local da reserva; nulo quando a reserva contém apenas recursos.</param>
/// <param name="RecursosId">Ids dos recursos associados à reserva.</param>
/// <param name="DataInicial">Data de início.</param>
/// <param name="HoraInicial">Hora de início.</param>
/// <param name="DataFinal">Data de término.</param>
/// <param name="HoraFinal">Hora de término.</param>
/// <param name="Ativo">Indica se a reserva está ativa (false = cancelada/excluída logicamente).</param>
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
