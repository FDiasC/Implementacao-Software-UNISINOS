using GestaoReservas.Domain.Dtos.Recursos;

namespace GestaoReservas.WebApi.Handlers;

/// <summary>Regras de negócio e orquestração do cadastro de recursos.</summary>
public interface IRecursoHandler
{
    /// <summary>Lista os registros; por padrão somente os ativos.</summary>
    Task<List<RecursoDto>> ListarAsync(bool apenasAtivos, CancellationToken ct);

    /// <summary>Obtém um registro pelo id. Lança <c>EntidadeNaoEncontradaException</c> se não existir.</summary>
    Task<RecursoDto> ObterPorIdAsync(int id, CancellationToken ct);

    /// <summary>Valida as regras de negócio e cadastra um novo registro.</summary>
    Task<RecursoDto> CriarAsync(CriarRecursoDto dto, CancellationToken ct);

    /// <summary>Valida as regras de negócio e atualiza um registro existente.</summary>
    Task<RecursoDto> AtualizarAsync(int id, AtualizarRecursoDto dto, CancellationToken ct);

    /// <summary>Exclusão lógica: marca o registro como inativo, se nenhuma regra impedir.</summary>
    Task DesativarAsync(int id, CancellationToken ct);
}
