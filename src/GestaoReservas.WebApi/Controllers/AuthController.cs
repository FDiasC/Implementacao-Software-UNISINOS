using GestaoReservas.Domain.Dtos.Auth;
using GestaoReservas.Domain.Dtos.Usuarios;
using GestaoReservas.WebApi.Common.Auth;
using GestaoReservas.WebApi.Handlers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestaoReservas.WebApi.Controllers;

/// <summary>Autenticação via token JWT.</summary>
[ApiController]
[Route("api/auth")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IAuthHandler _handler;

    public AuthController(IAuthHandler handler)
    {
        _handler = handler;
    }

    /// <summary>Valida e-mail e senha e devolve um token JWT para as demais rotas.</summary>
    /// <response code="200">Credenciais válidas; token emitido.</response>
    /// <response code="400">Dados inválidos (campos obrigatórios, e-mail mal formatado).</response>
    /// <response code="401">E-mail ou senha inválidos, ou usuário inativo.</response>
    [HttpPost("token")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(TokenDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<TokenDto>> GerarToken([FromBody] LoginDto dto, CancellationToken ct) =>
        Ok(await _handler.GerarTokenAsync(dto, ct));

    /// <summary>Retorna o usuário dono do token enviado.</summary>
    /// <response code="200">Usuário autenticado.</response>
    /// <response code="401">Token ausente, inválido, expirado ou de usuário desativado.</response>
    [HttpGet("me")]
    [ProducesResponseType(typeof(UsuarioDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<UsuarioDto>> ObterUsuarioLogado(CancellationToken ct) =>
        Ok(await _handler.ObterUsuarioLogadoAsync(User.ObterUsuarioId(), ct));
}
