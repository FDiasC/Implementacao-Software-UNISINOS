using GestaoReservas.Domain.Entities;

namespace GestaoReservas.Domain.Providers;

public interface IReservaProvider
{
    Task<List<Reserva>> ListarReservasAsync(bool apenasAtivos, CancellationToken ct);

    Task<Reserva?> ObterReservaPorIdAsync(int id, CancellationToken ct);

    /// <summary>
    /// Reservas ativas nas datas informadas, com seus recursos.
    /// A sobreposição exata de data e hora é verificada no ReservaHandler.
    /// </summary>
    Task<List<Reserva>> ListarAtivasNoPeriodoAsync(DateOnly dataInicial, DateOnly dataFinal, int? idParaIgnorar, CancellationToken ct);

    Task AdicionarReservaAsync(Reserva reserva, CancellationToken ct);

    Task SalvarAlteracoesAsync(CancellationToken ct);
}
