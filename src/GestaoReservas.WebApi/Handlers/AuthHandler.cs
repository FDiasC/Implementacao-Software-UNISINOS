using System.Security.Claims;
using System.Text;
using GestaoReservas.Domain.Dtos.Auth;
using GestaoReservas.Domain.Dtos.Usuarios;
using GestaoReservas.Domain.Providers;
using GestaoReservas.WebApi.Common.Auth;
using GestaoReservas.WebApi.Common.Exceptions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace GestaoReservas.WebApi.Handlers;

public class AuthHandler : IAuthHandler
{
    private const string MensagemCredenciaisInvalidas = "E-mail ou senha inválidos.";

    private readonly IUsuarioProvider _usuarioProvider;
    private readonly JwtOptions _jwt;

    public AuthHandler(IUsuarioProvider usuarioProvider, IOptions<JwtOptions> jwt)
    {
        _usuarioProvider = usuarioProvider;
        _jwt = jwt.Value;
    }

    public async Task<TokenDto> GerarTokenAsync(LoginDto dto, CancellationToken ct)
    {
        var email = dto.Email.Trim().ToLowerInvariant();
        var usuario = await _usuarioProvider.ObterUsuarioPorEmailAsync(email, ct);

        // Mesma mensagem para e-mail inexistente, senha errada ou usuário inativo,
        // para não revelar quais e-mails estão cadastrados. A senha ainda é guardada sem hash.
        if (usuario is null || !usuario.Ativo || string.IsNullOrEmpty(usuario.Senha) || usuario.Senha != dto.Senha)
        {
            throw new NaoAutenticadoException(MensagemCredenciaisInvalidas);
        }

        var expiraEm = DateTime.UtcNow.AddMinutes(_jwt.ExpiracaoMinutos);
        var descritor = new SecurityTokenDescriptor
        {
            Issuer = _jwt.Emissor,
            Audience = _jwt.Audiencia,
            Expires = expiraEm,
            Subject = new ClaimsIdentity(
            [
                new Claim(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, usuario.Email),
                new Claim(JwtRegisteredClaimNames.Name, usuario.Nome)
            ]),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Chave)),
                SecurityAlgorithms.HmacSha256)
        };

        var token = new JsonWebTokenHandler().CreateToken(descritor);

        return new TokenDto(token, expiraEm, ParaDto(usuario.Id, usuario.Nome, usuario.Email, usuario.Ativo));
    }

    public async Task<UsuarioDto> ObterUsuarioLogadoAsync(int usuarioId, CancellationToken ct)
    {
        var usuario = await _usuarioProvider.ObterUsuarioPorIdAsync(usuarioId, ct);
        if (usuario is null || !usuario.Ativo)
        {
            throw new NaoAutenticadoException("O usuário do token não existe mais ou foi desativado.");
        }

        return ParaDto(usuario.Id, usuario.Nome, usuario.Email, usuario.Ativo);
    }

    private static UsuarioDto ParaDto(int id, string nome, string email, bool ativo) => new(id, nome, email, ativo);
}
