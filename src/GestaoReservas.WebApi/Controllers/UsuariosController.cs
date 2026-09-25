using GestaoReservas.WebApi.Dtos.Usuarios;
using GestaoReservas.WebApi.Handlers;
using Microsoft.AspNetCore.Mvc;

namespace GestaoReservas.WebApi.Controllers;

/// <summary>Cadastro de usuários.</summary>
[ApiController]
[Route("api/usuarios")]
[Produces("application/json")]
public class UsuariosController : ControllerBase
{
    private readonly IUsuarioHandler _handler;

    public UsuariosController(IUsuarioHandler handler)
    {
        _handler = handler;
    }

    /// <summary>Lista usuários.</summary>
    /// <param name="apenasAtivos">Quando true (padrão), retorna somente registros ativos.</param>
    /// <param name="ct">Token de cancelamento da requisição.</param>
    /// <response code="200">Lista de usuários.</response>
    [HttpGet]
    [ProducesResponseType(typeof(List<UsuarioDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<UsuarioDto>>> Listar([FromQuery] bool apenasAtivos = true, CancellationToken ct = default) =>
        Ok(await _handler.ListarAsync(apenasAtivos, ct));

    /// <summary>Obtém um registro pelo id (inclusive se estiver inativo).</summary>
    /// <response code="200">Registro encontrado.</response>
    /// <response code="404">Registro não encontrado.</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(UsuarioDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UsuarioDto>> ObterPorId(int id, CancellationToken ct) =>
        Ok(await _handler.ObterPorIdAsync(id, ct));

    /// <summary>Cadastra um novo registro.</summary>
    /// <response code="201">Registro criado; o cabeçalho Location aponta para o recurso.</response>
    /// <response code="400">Dados inválidos ou regra de negócio violada.</response>
    /// <response code="409">Conflito com um registro já existente.</response>
    [HttpPost]
    [ProducesResponseType(typeof(UsuarioDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UsuarioDto>> Criar([FromBody] CriarUsuarioDto dto, CancellationToken ct)
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
    [ProducesResponseType(typeof(UsuarioDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UsuarioDto>> Atualizar(int id, [FromBody] AtualizarUsuarioDto dto, CancellationToken ct) =>
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
