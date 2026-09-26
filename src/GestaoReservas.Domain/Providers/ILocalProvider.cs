using GestaoReservas.Domain.Entities;

namespace GestaoReservas.Domain.Providers;

public interface ILocalProvider
{
    Task<List<Local>> ListarLocaisAsync(bool apenasAtivos, CancellationToken ct);

    Task<Local?> ObterLocalPorIdAsync(int id, CancellationToken ct);

    Task<bool> PossuiReservasAtivasAsync(int localId, CancellationToken ct);

    Task AdicionarLocalAsync(Local local, CancellationToken ct);

    Task SalvarAlteracoesAsync(CancellationToken ct);
}
