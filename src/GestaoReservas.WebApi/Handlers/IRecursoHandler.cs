using GestaoReservas.WebApi.Dtos.Recursos;

namespace GestaoReservas.WebApi.Handlers;


public interface IRecursoHandler
{
    
    Task<List<RecursoDto>> ListarAsync(bool apenasAtivos, CancellationToken ct);


    Task<RecursoDto> ObterPorIdAsync(int id, CancellationToken ct);

    
    Task<RecursoDto> CriarAsync(CriarRecursoDto dto, CancellationToken ct);

    
    Task<RecursoDto> AtualizarAsync(int id, AtualizarRecursoDto dto, CancellationToken ct);

    
    Task DesativarAsync(int id, CancellationToken ct);
}