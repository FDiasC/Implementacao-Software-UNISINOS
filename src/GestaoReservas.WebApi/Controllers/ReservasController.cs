using GestaoReservas.Domain.Dtos.Reservas;
using GestaoReservas.WebApi.Handlers;
using Microsoft.AspNetCore.Mvc;

namespace GestaoReservas.WebApi.Controllers;

/// <summary>Cadastro de reservas de locais e/ou recursos.</summary>
[ApiController]
[Route("api/reservas")]
[Produces("application/json")]
public class ReservasController : ControllerBase
{
    private readonly IReservaHandler _handler;

    public ReservasController(IReservaHandler handler)
    {
        _handler = handler;
    }

    /// <summary>Lista reservas.</summary>
    /// <param name="apenasAtivos">Quando true (padrão), retorna somente reservas ativas.</param>
    /// <param name="ct">Token de cancelamento da requisição.</param>
    /// <response code="200">Lista de reservas.</response>
    [HttpGet]
    [ProducesResponseType(typeof(List<ReservaDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ReservaDto>>> Listar([FromQuery] bool apenasAtivos = true, CancellationToken ct = default) =>
        Ok(await _handler.ListarAsync(apenasAtivos, ct));

    /// <summary>Obtém uma reserva pelo id (inclusive se estiver inativa).</summary>
    /// <response code="200">Reserva encontrada.</response>
    /// <response code="404">Reserva não encontrada.</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ReservaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReservaDto>> ObterPorId(int id, CancellationToken ct) =>
        Ok(await _handler.ObterPorIdAsync(id, ct));

    /// <summary>Cadastra uma nova reserva.</summary>
    /// <response code="201">Reserva criada; o cabeçalho Location aponta para ela.</response>
    /// <response code="400">Dados inválidos ou regra de negócio violada (escopo, período, PermiteRecursos, janela de dias do recurso).</response>
    /// <response code="404">Usuário, local ou recurso não encontrado.</response>
    /// <response code="409">Local ou recurso já reservado no período.</response>
    [HttpPost]
    [ProducesResponseType(typeof(ReservaDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ReservaDto>> Criar([FromBody] CriarReservaDto dto, CancellationToken ct)
    {
        var criada = await _handler.CriarAsync(dto, ct);
        return CreatedAtAction(nameof(ObterPorId), new { id = criada.Id }, criada);
    }

    /// <summary>Atualiza local, período e recursos de uma reserva ativa.</summary>
    /// <response code="200">Reserva atualizada.</response>
    /// <response code="400">Dados inválidos, reserva inativa ou regra de negócio violada.</response>
    /// <response code="404">Reserva, local ou recurso não encontrado.</response>
    /// <response code="409">Local ou recurso já reservado no período.</response>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ReservaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ReservaDto>> Atualizar(int id, [FromBody] AtualizarReservaDto dto, CancellationToken ct) =>
        Ok(await _handler.AtualizarAsync(id, dto, ct));

    /// <summary>Cancelamento lógico: marca a reserva como inativa (Ativo = false).</summary>
    /// <response code="204">Reserva desativada.</response>
    /// <response code="404">Reserva não encontrada.</response>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Desativar(int id, CancellationToken ct)
    {
        await _handler.DesativarAsync(id, ct);
        return NoContent();
    }
}
