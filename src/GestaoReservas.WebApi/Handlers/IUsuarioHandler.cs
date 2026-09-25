using GestaoReservas.WebApi.Dtos.Usuarios;

namespace GestaoReservas.WebApi.Handlers;

/// <summary>Regras de negócio e orquestração do cadastro de usuários.</summary>
public interface IUsuarioHandler
{
    /// <summary>Lista os registros; por padrão somente os ativos.</summary>
    Task<List<UsuarioDto>> ListarAsync(bool apenasAtivos, CancellationToken ct);

    /// <summary>Obtém um registro pelo id. Lança <c>EntidadeNaoEncontradaException</c> se não existir.</summary>
    Task<UsuarioDto> ObterPorIdAsync(int id, CancellationToken ct);

    /// <summary>Valida as regras de negócio e cadastra um novo registro.</summary>
    Task<UsuarioDto> CriarAsync(CriarUsuarioDto dto, CancellationToken ct);

    /// <summary>Valida as regras de negócio e atualiza um registro existente.</summary>
    Task<UsuarioDto> AtualizarAsync(int id, AtualizarUsuarioDto dto, CancellationToken ct);

    /// <summary>Exclusão lógica: marca o registro como inativo, se nenhuma regra impedir.</summary>
    Task DesativarAsync(int id, CancellationToken ct);
}
