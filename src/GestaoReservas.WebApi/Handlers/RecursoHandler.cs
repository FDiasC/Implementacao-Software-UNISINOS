using GestaoReservas.WebApi.Common.Exceptions;
using GestaoReservas.WebApi.Dtos.Recursos;
using GestaoReservas.Domain.Providers;
using GestaoReservas.Domain.Entities;
using GestaoReservas.Domain.Enums;

namespace GestaoReservas.WebApi.Handlers;

public class RecursoHandler : IRecursoHandler
{
    private readonly IRecursoProvider _recursoProvider;
    private readonly ICategoriaProvider _categoriaProvider;

    public RecursoHandler(IRecursoProvider recursoProvider, ICategoriaProvider categoriaProvider)
    {
        _recursoProvider = recursoProvider;
        _categoriaProvider = categoriaProvider;
    }

    public async Task<List<RecursoDto>> ListarAsync(bool apenasAtivos, CancellationToken ct)
    {
        var recursos = await _recursoProvider.ListarAsync(apenasAtivos, ct);
        return recursos.Select(ParaDto).ToList();
    }

    public async Task<RecursoDto> ObterPorIdAsync(int id, CancellationToken ct)
    {
        var recurso = await _recursoProvider.ObterPorIdAsync(id, ct)
            ?? throw new EntidadeNaoEncontradaException(nameof(Recurso), id);

        return ParaDto(recurso);
    }

    public async Task<RecursoDto> CriarAsync(CriarRecursoDto dto, CancellationToken ct)
    {
        var categoria = await ObterCategoriaValidaAsync(dto.CategoriaId, ct);

        var recurso = new Recurso
        {
            Descricao = dto.Descricao.Trim(),
            NumeroPatrimonio = dto.NumeroPatrimonio.Trim(),
            DiasMinimosReserva = dto.DiasMinimosReserva,
            DiasMaximosReserva = dto.DiasMaximosReserva,
            CategoriaId = categoria.Id,
            Categoria = categoria
        };

        await _recursoProvider.AdicionarAsync(recurso, ct);
        await _recursoProvider.SalvarAlteracoesAsync(ct);

        return ParaDto(recurso);
    }

    public async Task<RecursoDto> AtualizarAsync(int id, AtualizarRecursoDto dto, CancellationToken ct)
    {
        var recurso = await _recursoProvider.ObterPorIdAsync(id, ct)
            ?? throw new EntidadeNaoEncontradaException(nameof(Recurso), id);

        var categoria = await ObterCategoriaValidaAsync(dto.CategoriaId, ct);

        recurso.Descricao = dto.Descricao.Trim();
        recurso.NumeroPatrimonio = dto.NumeroPatrimonio.Trim();
        recurso.DiasMinimosReserva = dto.DiasMinimosReserva;
        recurso.DiasMaximosReserva = dto.DiasMaximosReserva;
        recurso.CategoriaId = categoria.Id;
        recurso.Categoria = categoria;

        await _recursoProvider.SalvarAlteracoesAsync(ct);

        return ParaDto(recurso);
    }

    public async Task DesativarAsync(int id, CancellationToken ct)
    {
        var recurso = await _recursoProvider.ObterPorIdAsync(id, ct)
            ?? throw new EntidadeNaoEncontradaException(nameof(Recurso), id);

        if (await _recursoProvider.PossuiReservasAtivasAsync(id, ct))
        {
            throw new RegraDeNegocioException(
                $"O recurso '{recurso.NumeroPatrimonio}' não pode ser desativado enquanto houver reservas ativas vinculadas a ele.");
        }

        recurso.Desativar();

        await _recursoProvider.SalvarAlteracoesAsync(ct);
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

    private static RecursoDto ParaDto(Recurso recurso) => new(
    recurso.Id,
    recurso.Descricao,
    recurso.NumeroPatrimonio,
    recurso.DiasMinimosReserva,
    recurso.DiasMaximosReserva,
    recurso.CategoriaId,
    recurso.Categoria.Nome,
    recurso.Ativo);
}