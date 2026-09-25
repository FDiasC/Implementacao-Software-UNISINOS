using GestaoReservas.Domain.Entities;

namespace GestaoReservas.Domain.Providers;

public interface IUsuarioProvider
{
    Task<List<Usuario>> ListarAsync(bool apenasAtivos, CancellationToken ct);

    Task<Usuario?> ObterPorIdAsync(int id, CancellationToken ct);

    Task<bool> ExisteComEmailAsync(string email, int? idParaIgnorar, CancellationToken ct);

    Task AdicionarAsync(Usuario usuario, CancellationToken ct);

    Task SalvarAlteracoesAsync(CancellationToken ct);
}
