using GestaoReservas.Domain.Entities;

namespace GestaoReservas.Domain.Providers;

public interface ICategoriaProvider
{
    Task<List<Categoria>> ListarAsync(bool apenasAtivas, CancellationToken ct);

    Task<Categoria?> ObterPorIdAsync(int id, CancellationToken ct);

    Task<bool> PossuiVinculosAsync(int categoriaId, bool apenasAtivos, CancellationToken ct);

    Task AdicionarAsync(Categoria categoria, CancellationToken ct);

    Task SalvarAlteracoesAsync(CancellationToken ct);
}
