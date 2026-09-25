using GestaoReservas.WebApi.Dtos.Locais;
using GestaoReservas.WebApi.Handlers;
using Microsoft.AspNetCore.Mvc;

namespace GestaoReservas.WebApi.Controllers;

/// <summary>Cadastro de locais.</summary>
[ApiController]
[Route("api/locais")]
[Produces("application/json")]
public class LocaisController : ControllerBase
{
    private readonly ILocalHandler _handler;

    public LocaisController(ILocalHandler handler)
    {
        _handler = handler;
    }

    /// <summary>Lista locais.</summary>
    /// <param name="apenasAtivos">Quando true (padrão), retorna somente registros ativos.</param>
    /// <param name="ct">Token de cancelamento da requisição.</param>
    /// <response code="200">Lista de locais.</response>
    [HttpGet]
    [ProducesResponseType(typeof(List<LocalDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<LocalDto>>> Listar([FromQuery] bool apenasAtivos = true, CancellationToken ct = default) =>
        Ok(await _handler.ListarAsync(apenasAtivos, ct));

    /// <summary>Obtém um registro pelo id (inclusive se estiver inativo).</summary>
    /// <response code="200">Registro encontrado.</response>
    /// <response code="404">Registro não encontrado.</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(LocalDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LocalDto>> ObterPorId(int id, CancellationToken ct) =>
        Ok(await _handler.ObterPorIdAsync(id, ct));

    /// <summary>Cadastra um novo registro.</summary>
    /// <response code="201">Registro criado; o cabeçalho Location aponta para o recurso.</response>
    /// <response code="400">Dados inválidos ou regra de negócio violada.</response>
    /// <response code="409">Conflito com um registro já existente.</response>
    [HttpPost]
    [ProducesResponseType(typeof(LocalDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<LocalDto>> Criar([FromBody] CriarLocalDto dto, CancellationToken ct)
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
    [ProducesResponseType(typeof(LocalDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<LocalDto>> Atualizar(int id, [FromBody] AtualizarLocalDto dto, CancellationToken ct) =>
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
