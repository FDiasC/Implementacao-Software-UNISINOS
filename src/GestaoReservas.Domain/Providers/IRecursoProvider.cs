using GestaoReservas.Domain.Entities;

namespace GestaoReservas.Domain.Providers;

public interface IRecursoProvider
{
    Task<List<Recurso>> ListarAsync(bool apenasAtivos, CancellationToken ct);

    Task<Recurso?> ObterPorIdAsync(int id, CancellationToken ct);

    Task<bool> PossuiReservasAtivasAsync(int recursoId, CancellationToken ct);
    
    Task AdicionarAsync(Recurso recurso, CancellationToken ct);

    Task SalvarAlteracoesAsync(CancellationToken ct);
}