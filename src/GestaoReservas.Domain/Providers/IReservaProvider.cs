using GestaoReservas.Domain.Entities;

namespace GestaoReservas.Domain.Providers;

public interface IReservaProvider
{
    Task<List<Reserva>> ListarReservasAsync(bool apenasAtivos, CancellationToken ct);

    Task<Reserva?> ObterReservaPorIdAsync(int id, CancellationToken ct);

    /// <summary>
    /// Reservas ativas cujas datas tocam o intervalo [dataInicial, dataFinal] (pré-filtro por data;
    /// o cruzamento exato com as horas é feito pelo handler). Inclui os recursos vinculados.
    /// </summary>
    Task<List<Reserva>> ListarAtivasNoPeriodoAsync(DateOnly dataInicial, DateOnly dataFinal, int? idParaIgnorar, CancellationToken ct);

    Task AdicionarReservaAsync(Reserva reserva, CancellationToken ct);

    Task SalvarAlteracoesAsync(CancellationToken ct);
}
