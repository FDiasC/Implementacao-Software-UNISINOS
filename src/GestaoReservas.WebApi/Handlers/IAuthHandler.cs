using GestaoReservas.Domain.Dtos.Auth;
using GestaoReservas.Domain.Dtos.Usuarios;

namespace GestaoReservas.WebApi.Handlers;

/// <summary>Autenticação: validação de credenciais e emissão do token JWT.</summary>
public interface IAuthHandler
{
    /// <summary>Valida e-mail e senha e emite um token. Lança <c>NaoAutenticadoException</c> se forem inválidos.</summary>
    Task<TokenDto> GerarTokenAsync(LoginDto dto, CancellationToken ct);

    /// <summary>Obtém os dados do usuário autenticado. Lança <c>NaoAutenticadoException</c> se ele não estiver mais ativo.</summary>
    Task<UsuarioDto> ObterUsuarioLogadoAsync(int usuarioId, CancellationToken ct);
}
