using GestaoReservas.WebApi.Common.Exceptions;
using GestaoReservas.WebApi.Dtos.Locais;
using GestaoReservas.Domain.Providers;
using GestaoReservas.Domain.Entities;
using GestaoReservas.Domain.Enums;

namespace GestaoReservas.WebApi.Handlers;

public class LocalHandler : ILocalHandler
{
    private readonly ILocalProvider _localProvider;
    private readonly ICategoriaProvider _categoriaProvider;

    public LocalHandler(ILocalProvider localProvider, ICategoriaProvider categoriaProvider)
    {
        _localProvider = localProvider;
        _categoriaProvider = categoriaProvider;
    }

    public async Task<List<LocalDto>> ListarAsync(bool apenasAtivos, CancellationToken ct)
    {
        var locais = await _localProvider.ListarAsync(apenasAtivos, ct);
        return locais.Select(ParaDto).ToList();
    }

    public async Task<LocalDto> ObterPorIdAsync(int id, CancellationToken ct)
    {
        var local = await _localProvider.ObterPorIdAsync(id, ct)
            ?? throw new EntidadeNaoEncontradaException(nameof(Local), id);

        return ParaDto(local);
    }

    public async Task<LocalDto> CriarAsync(CriarLocalDto dto, CancellationToken ct)
    {
        var categoria = await ObterCategoriaValidaAsync(dto.CategoriaId, ct);

        var local = new Local
        {
            Sala = dto.Sala.Trim(),
            Predio = dto.Predio.Trim(),
            Capacidade = dto.Capacidade,
            Andar = dto.Andar,
            PermiteRecursos = dto.PermiteRecursos,
            CategoriaId = categoria.Id,
            Categoria = categoria
        };

        await _localProvider.AdicionarAsync(local, ct);
        await _localProvider.SalvarAlteracoesAsync(ct);

        return ParaDto(local);
    }

    public async Task<LocalDto> AtualizarAsync(int id, AtualizarLocalDto dto, CancellationToken ct)
    {
        var local = await _localProvider.ObterPorIdAsync(id, ct)
            ?? throw new EntidadeNaoEncontradaException(nameof(Local), id);

        var categoria = await ObterCategoriaValidaAsync(dto.CategoriaId, ct);

        local.Sala = dto.Sala.Trim();
        local.Predio = dto.Predio.Trim();
        local.Capacidade = dto.Capacidade;
        local.Andar = dto.Andar;
        local.PermiteRecursos = dto.PermiteRecursos;
        local.CategoriaId = categoria.Id;
        local.Categoria = categoria;

        await _localProvider.SalvarAlteracoesAsync(ct);

        return ParaDto(local);
    }

    public async Task DesativarAsync(int id, CancellationToken ct)
    {
        var local = await _localProvider.ObterPorIdAsync(id, ct)
            ?? throw new EntidadeNaoEncontradaException(nameof(Local), id);

        if (await _localProvider.PossuiReservasAtivasAsync(id, ct))
        {
            throw new RegraDeNegocioException(
                $"O local '{local.Sala}' não pode ser desativado enquanto houver reservas ativas vinculadas a ele.");
        }

        local.Desativar();

        await _localProvider.SalvarAlteracoesAsync(ct);
    }

    private async Task<Categoria> ObterCategoriaValidaAsync(int categoriaId, CancellationToken ct)
    {
        var categoria = await _categoriaProvider.ObterPorIdAsync(categoriaId, ct)
            ?? throw new EntidadeNaoEncontradaException(nameof(Categoria), categoriaId);

        if (categoria.Tipo != TipoCategoria.Local)
        {
            throw new RegraDeNegocioException(
                $"A categoria '{categoria.Nome}' não é do tipo '{TipoCategoria.Local}' e não pode ser usada em um Local.");
        }

        if (!categoria.Ativo)
        {
            throw new RegraDeNegocioException($"A categoria '{categoria.Nome}' está inativa.");
        }

        return categoria;
    }

    private static LocalDto ParaDto(Local local) => new(
        local.Id,
        local.Sala,
        local.Predio,
        local.Capacidade,
        local.Andar,
        local.PermiteRecursos,
        local.CategoriaId,
        local.Categoria.Nome,
        local.Ativo);
}
