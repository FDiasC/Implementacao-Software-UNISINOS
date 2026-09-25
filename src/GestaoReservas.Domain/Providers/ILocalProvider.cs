using GestaoReservas.Domain.Entities;

namespace GestaoReservas.Domain.Providers;

public interface ILocalProvider
{
    Task<List<Local>> ListarAsync(bool apenasAtivos, CancellationToken ct);

    Task<Local?> ObterPorIdAsync(int id, CancellationToken ct);

    Task<bool> PossuiReservasAtivasAsync(int localId, CancellationToken ct);

    Task AdicionarAsync(Local local, CancellationToken ct);

    Task SalvarAlteracoesAsync(CancellationToken ct);
}
