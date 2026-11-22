using GestaoReservas.Domain.Dtos.Usuarios;

namespace GestaoReservas.WebApi.Handlers;

/// <summary>Regras de negócio e orquestração do cadastro de usuários.</summary>
public interface IUsuarioHandler
{
    Task<List<UsuarioDto>> ListarAsync(bool apenasAtivos, CancellationToken ct);

    Task<UsuarioDto> ObterPorIdAsync(int id, CancellationToken ct);

    Task<UsuarioDto> CriarAsync(CriarUsuarioDto dto, CancellationToken ct);

    Task<UsuarioDto> AtualizarAsync(int id, AtualizarUsuarioDto dto, CancellationToken ct);

    Task DesativarAsync(int id, CancellationToken ct);
}
