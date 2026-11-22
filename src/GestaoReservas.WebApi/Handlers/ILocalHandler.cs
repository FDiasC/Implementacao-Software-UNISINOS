using GestaoReservas.Domain.Dtos.Locais;

namespace GestaoReservas.WebApi.Handlers;

/// <summary>Regras de negócio e orquestração do cadastro de locais.</summary>
public interface ILocalHandler
{
    Task<List<LocalDto>> ListarAsync(bool apenasAtivos, CancellationToken ct);

    Task<LocalDto> ObterPorIdAsync(int id, CancellationToken ct);

    Task<LocalDto> CriarAsync(CriarLocalDto dto, CancellationToken ct);

    Task<LocalDto> AtualizarAsync(int id, AtualizarLocalDto dto, CancellationToken ct);

    Task DesativarAsync(int id, CancellationToken ct);
}
