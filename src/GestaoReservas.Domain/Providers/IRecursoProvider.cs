using GestaoReservas.Domain.Entities;

namespace GestaoReservas.Domain.Providers;

public interface IRecursoProvider
{
    Task<List<Recurso>> ListarRecursosAsync(bool apenasAtivos, CancellationToken ct);

    Task<Recurso?> ObterRecursoPorIdAsync(int id, CancellationToken ct);

    Task<bool> PossuiReservasAtivasAsync(int recursoId, CancellationToken ct);

    Task<bool> ExisteComNumeroPatrimonioAsync(string numeroPatrimonio, int? idParaIgnorar, CancellationToken ct);
    
    Task AdicionarRecursoAsync(Recurso recurso, CancellationToken ct);

    Task SalvarAlteracoesAsync(CancellationToken ct);
}