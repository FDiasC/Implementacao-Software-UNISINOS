using GestaoReservas.Domain.Dtos.Usuarios;

namespace GestaoReservas.Domain.Dtos.Auth;

/// <summary>Token JWT emitido no login.</summary>
/// <param name="Token">Token a ser enviado no cabeçalho <c>Authorization: Bearer {token}</c>.</param>
/// <param name="ExpiraEm">Data/hora (UTC) em que o token deixa de ser aceito.</param>
public record TokenDto(string Token, DateTime ExpiraEm, UsuarioDto Usuario);
