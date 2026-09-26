using GestaoReservas.Domain.Entities;

namespace GestaoReservas.Domain.Providers;

public interface IUsuarioProvider
{
    Task<List<Usuario>> ListarUsuariosAsync(bool apenasAtivos, CancellationToken ct);

    Task<Usuario?> ObterUsuarioPorIdAsync(int id, CancellationToken ct);

    Task<bool> ExisteComEmailAsync(string email, int? idParaIgnorar, CancellationToken ct);

    Task AdicionarUsuarioAsync(Usuario usuario, CancellationToken ct);

    Task SalvarAlteracoesAsync(CancellationToken ct);
}
