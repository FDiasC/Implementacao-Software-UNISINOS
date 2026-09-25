using GestaoReservas.WebApi.Dtos.Categorias;

namespace GestaoReservas.WebApi.Handlers;

/// <summary>Regras de negócio e orquestração do cadastro de categorias.</summary>
public interface ICategoriaHandler
{
    /// <summary>Lista os registros; por padrão somente os ativos.</summary>
    Task<List<CategoriaDto>> ListarAsync(bool apenasAtivas, CancellationToken ct);

    /// <summary>Obtém um registro pelo id. Lança <c>EntidadeNaoEncontradaException</c> se não existir.</summary>
    Task<CategoriaDto> ObterPorIdAsync(int id, CancellationToken ct);

    /// <summary>Valida as regras de negócio e cadastra um novo registro.</summary>
    Task<CategoriaDto> CriarAsync(CriarCategoriaDto dto, CancellationToken ct);

    /// <summary>Valida as regras de negócio e atualiza um registro existente.</summary>
    Task<CategoriaDto> AtualizarAsync(int id, AtualizarCategoriaDto dto, CancellationToken ct);

    /// <summary>Exclusão lógica: marca o registro como inativo, se nenhuma regra impedir.</summary>
    Task DesativarAsync(int id, CancellationToken ct);
}
