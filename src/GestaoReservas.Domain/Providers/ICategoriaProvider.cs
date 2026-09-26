using GestaoReservas.Domain.Entities;

namespace GestaoReservas.Domain.Providers;

public interface ICategoriaProvider
{
    Task<List<Categoria>> ListarCategoriasAsync(bool apenasAtivas, CancellationToken ct);

    Task<Categoria?> ObterCategoriaPorIdAsync(int id, CancellationToken ct);

    Task<bool> PossuiVinculosAsync(int categoriaId, bool apenasAtivos, CancellationToken ct);

    Task AdicionarCategoriaAsync(Categoria categoria, CancellationToken ct);

    Task SalvarAlteracoesAsync(CancellationToken ct);
}
