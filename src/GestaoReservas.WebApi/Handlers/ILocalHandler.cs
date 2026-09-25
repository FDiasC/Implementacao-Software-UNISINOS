using GestaoReservas.WebApi.Dtos.Locais;

namespace GestaoReservas.WebApi.Handlers;

/// <summary>Regras de negócio e orquestração do cadastro de locais.</summary>
public interface ILocalHandler
{
    /// <summary>Lista os registros; por padrão somente os ativos.</summary>
    Task<List<LocalDto>> ListarAsync(bool apenasAtivos, CancellationToken ct);

    /// <summary>Obtém um registro pelo id. Lança <c>EntidadeNaoEncontradaException</c> se não existir.</summary>
    Task<LocalDto> ObterPorIdAsync(int id, CancellationToken ct);

    /// <summary>Valida as regras de negócio e cadastra um novo registro.</summary>
    Task<LocalDto> CriarAsync(CriarLocalDto dto, CancellationToken ct);

    /// <summary>Valida as regras de negócio e atualiza um registro existente.</summary>
    Task<LocalDto> AtualizarAsync(int id, AtualizarLocalDto dto, CancellationToken ct);

    /// <summary>Exclusão lógica: marca o registro como inativo, se nenhuma regra impedir.</summary>
    Task DesativarAsync(int id, CancellationToken ct);
}
