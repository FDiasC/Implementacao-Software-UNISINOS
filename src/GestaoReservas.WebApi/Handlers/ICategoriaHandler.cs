using GestaoReservas.Domain.Dtos.Categorias;

namespace GestaoReservas.WebApi.Handlers;

/// <summary>Regras de negócio e orquestração do cadastro de categorias.</summary>
public interface ICategoriaHandler
{
    Task<List<CategoriaDto>> ListarAsync(bool apenasAtivas, CancellationToken ct);

    Task<CategoriaDto> ObterPorIdAsync(int id, CancellationToken ct);

    Task<CategoriaDto> CriarAsync(CriarCategoriaDto dto, CancellationToken ct);

    Task<CategoriaDto> AtualizarAsync(int id, AtualizarCategoriaDto dto, CancellationToken ct);

    Task DesativarAsync(int id, CancellationToken ct);
}
