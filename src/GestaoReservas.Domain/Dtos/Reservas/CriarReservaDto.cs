namespace GestaoReservas.Domain.Dtos.Reservas;

/// <summary>Dados para cadastrar uma reserva.</summary>
/// <param name="UsuarioId">Usuário existente e ativo que faz a reserva.</param>
/// <param name="LocalId">Local existente e ativo. Opcional quando a reserva tiver ao menos um recurso.</param>
/// <param name="DataInicial">Data de início (não pode estar no passado).</param>
/// <param name="HoraInicial">Hora de início.</param>
/// <param name="DataFinal">Data de término.</param>
/// <param name="HoraFinal">Hora de término (o término deve ser posterior ao início).</param>
/// <param name="RecursosId">Ids dos recursos reservados. Opcional quando a reserva tiver um local; exige que o local permita recursos.</param>
public record CriarReservaDto(
    int UsuarioId,
    int? LocalId,
    DateOnly DataInicial,
    TimeOnly HoraInicial,
    DateOnly DataFinal,
    TimeOnly HoraFinal,
    IEnumerable<int>? RecursosId);
