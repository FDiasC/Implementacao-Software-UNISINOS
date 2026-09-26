using GestaoReservas.Domain.Dtos.Reservas;

namespace GestaoReservas.WebApi.Handlers;

/// <summary>Regras de negócio e orquestração do cadastro de reservas.</summary>
public interface IReservaHandler
{
    /// <summary>Lista as reservas; por padrão somente as ativas.</summary>
    Task<List<ReservaDto>> ListarAsync(bool apenasAtivos, CancellationToken ct);

    /// <summary>Obtém uma reserva pelo id. Lança <c>EntidadeNaoEncontradaException</c> se não existir.</summary>
    Task<ReservaDto> ObterPorIdAsync(int id, CancellationToken ct);

    /// <summary>Valida escopo, período, local, recursos e conflitos de horário e cadastra a reserva.</summary>
    Task<ReservaDto> CriarAsync(CriarReservaDto dto, CancellationToken ct);

    /// <summary>Aplica as mesmas validações da criação a uma reserva ativa existente.</summary>
    Task<ReservaDto> AtualizarAsync(int id, AtualizarReservaDto dto, CancellationToken ct);

    /// <summary>Cancelamento lógico: marca a reserva como inativa, liberando local e recursos.</summary>
    Task DesativarAsync(int id, CancellationToken ct);
}
