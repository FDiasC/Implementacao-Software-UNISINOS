using GestaoReservas.Domain.Dtos.Recursos;
using GestaoReservas.WebApi.Handlers;
using Microsoft.AspNetCore.Mvc;

namespace GestaoReservas.WebApi.Controllers;

/// <summary>Cadastro de recursos.</summary>
[ApiController]
[Route("api/recursos")]
[Produces("application/json")]
public class RecursosController : ControllerBase
{
    private readonly IRecursoHandler _handler;

    public RecursosController(IRecursoHandler handler)
    {
        _handler = handler;
    }

    /// <summary>Lista recursos.</summary>
    /// <param name="apenasAtivos">Quando true (padrão), retorna somente registros ativos.</param>
    /// <param name="ct">Token de cancelamento da requisição.</param>
    /// <response code="200">Lista de recursos.</response>
    [HttpGet]
    [ProducesResponseType(typeof(List<RecursoDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<RecursoDto>>> Listar([FromQuery] bool apenasAtivos = true, CancellationToken ct = default) =>
        Ok(await _handler.ListarAsync(apenasAtivos, ct));

    /// <summary>Obtém um registro pelo id (inclusive se estiver inativo).</summary>
    /// <response code="200">Registro encontrado.</response>
    /// <response code="404">Registro não encontrado.</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(RecursoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RecursoDto>> ObterPorId(int id, CancellationToken ct) =>
        Ok(await _handler.ObterPorIdAsync(id, ct));

    /// <summary>Cadastra um novo registro.</summary>
    /// <response code="201">Registro criado; o cabeçalho Location aponta para o recurso.</response>
    /// <response code="400">Dados inválidos ou regra de negócio violada.</response>
    /// <response code="409">Conflito com um registro já existente.</response>
    [HttpPost]
    [ProducesResponseType(typeof(RecursoDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<RecursoDto>> Criar([FromBody] CriarRecursoDto dto, CancellationToken ct)
    {
        var criado = await _handler.CriarAsync(dto, ct);
        return CreatedAtAction(nameof(ObterPorId), new { id = criado.Id }, criado);
    }

    /// <summary>Atualiza os dados de um registro existente.</summary>
    /// <response code="200">Registro atualizado.</response>
    /// <response code="400">Dados inválidos ou regra de negócio violada.</response>
    /// <response code="404">Registro não encontrado.</response>
    /// <response code="409">Conflito com um registro já existente.</response>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(RecursoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<RecursoDto>> Atualizar(int id, [FromBody] AtualizarRecursoDto dto, CancellationToken ct) =>
        Ok(await _handler.AtualizarAsync(id, dto, ct));

    /// <summary>Exclusão lógica: marca o registro como inativo (Ativo = false).</summary>
    /// <response code="204">Registro desativado.</response>
    /// <response code="400">Uma regra de negócio impede a desativação.</response>
    /// <response code="404">Registro não encontrado.</response>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Desativar(int id, CancellationToken ct)
    {
        await _handler.DesativarAsync(id, ct);
        return NoContent();
    }
}
