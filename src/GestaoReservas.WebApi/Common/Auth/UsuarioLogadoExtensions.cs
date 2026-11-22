using System.Security.Claims;

namespace GestaoReservas.WebApi.Common.Auth;

public static class UsuarioLogadoExtensions
{
    /// <summary>Id do usuário autenticado, lido da claim "sub" do token.</summary>
    public static int ObterUsuarioId(this ClaimsPrincipal principal)
    {
        var valor = principal.FindFirstValue("sub") ?? principal.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(valor, out var id)
            ? id
            : throw new InvalidOperationException("O token não contém o id do usuário.");
    }
}
