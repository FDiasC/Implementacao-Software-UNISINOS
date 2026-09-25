using GestaoReservas.WebApi.Common.Exceptions;
using GestaoReservas.WebApi.Dtos.Categorias;
using GestaoReservas.Domain.Providers;
using GestaoReservas.Domain.Entities;

namespace GestaoReservas.WebApi.Handlers;

public class CategoriaHandler : ICategoriaHandler
{
    private readonly ICategoriaProvider _provider;

    public CategoriaHandler(ICategoriaProvider provider)
    {
        _provider = provider;
    }

    public async Task<List<CategoriaDto>> ListarAsync(bool apenasAtivas, CancellationToken ct)
    {
        var categorias = await _provider.ListarAsync(apenasAtivas, ct);
        return categorias.Select(ParaDto).ToList();
    }

    public async Task<CategoriaDto> ObterPorIdAsync(int id, CancellationToken ct)
    {
        var categoria = await _provider.ObterPorIdAsync(id, ct)
            ?? throw new EntidadeNaoEncontradaException(nameof(Categoria), id);

        return ParaDto(categoria);
    }

    public async Task<CategoriaDto> CriarAsync(CriarCategoriaDto dto, CancellationToken ct)
    {
        var categoria = new Categoria
        {
            Nome = dto.Nome.Trim(),
            Tipo = dto.Tipo
        };

        await _provider.AdicionarAsync(categoria, ct);
        await _provider.SalvarAlteracoesAsync(ct);

        return ParaDto(categoria);
    }

    public async Task<CategoriaDto> AtualizarAsync(int id, AtualizarCategoriaDto dto, CancellationToken ct)
    {
        var categoria = await _provider.ObterPorIdAsync(id, ct)
            ?? throw new EntidadeNaoEncontradaException(nameof(Categoria), id);

        if (dto.Tipo != categoria.Tipo && await _provider.PossuiVinculosAsync(id, apenasAtivos: false, ct))
        {
            throw new RegraDeNegocioException(
                $"O tipo da categoria '{categoria.Nome}' não pode ser alterado enquanto houver locais ou recursos vinculados a ela.");
        }

        categoria.Nome = dto.Nome.Trim();
        categoria.Tipo = dto.Tipo;

        await _provider.SalvarAlteracoesAsync(ct);

        return ParaDto(categoria);
    }

    public async Task DesativarAsync(int id, CancellationToken ct)
    {
        var categoria = await _provider.ObterPorIdAsync(id, ct)
            ?? throw new EntidadeNaoEncontradaException(nameof(Categoria), id);

        if (await _provider.PossuiVinculosAsync(id, apenasAtivos: true, ct))
        {
            throw new RegraDeNegocioException(
                $"A categoria '{categoria.Nome}' não pode ser desativada enquanto houver locais ou recursos ativos vinculados a ela.");
        }

        categoria.Desativar();

        await _provider.SalvarAlteracoesAsync(ct);
    }

    private static CategoriaDto ParaDto(Categoria categoria) =>
        new(categoria.Id, categoria.Nome, categoria.Tipo.ToString(), categoria.Ativo);
}
