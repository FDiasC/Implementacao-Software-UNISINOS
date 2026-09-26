using GestaoReservas.WebApi.Common.Exceptions;
using GestaoReservas.Domain.Dtos.Usuarios;
using GestaoReservas.Domain.Providers;
using GestaoReservas.Domain.Entities;

namespace GestaoReservas.WebApi.Handlers;

public class UsuarioHandler : IUsuarioHandler
{
    private readonly IUsuarioProvider _provider;

    public UsuarioHandler(IUsuarioProvider provider)
    {
        _provider = provider;
    }

    public async Task<List<UsuarioDto>> ListarAsync(bool apenasAtivos, CancellationToken ct)
    {
        var usuarios = await _provider.ListarUsuariosAsync(apenasAtivos, ct);
        return usuarios.Select(ParaDto).ToList();
    }

    public async Task<UsuarioDto> ObterPorIdAsync(int id, CancellationToken ct)
    {
        var usuario = await _provider.ObterUsuarioPorIdAsync(id, ct)
            ?? throw new EntidadeNaoEncontradaException(nameof(Usuario), id);

        return ParaDto(usuario);
    }

    public async Task<UsuarioDto> CriarAsync(CriarUsuarioDto dto, CancellationToken ct)
    {
        var email = NormalizarEmail(dto.Email);

        if (await _provider.ExisteComEmailAsync(email, idParaIgnorar: null, ct))
        {
            throw new RegraDeNegocioException($"Já existe um usuário cadastrado com o e-mail '{email}'.");
        }

        var usuario = new Usuario
        {
            Nome = dto.Nome.Trim(),
            Email = email,
            Senha = dto.Senha
        };

        await _provider.AdicionarUsuarioAsync(usuario, ct);
        await _provider.SalvarAlteracoesAsync(ct);

        return ParaDto(usuario);
    }

    public async Task<UsuarioDto> AtualizarAsync(int id, AtualizarUsuarioDto dto, CancellationToken ct)
    {
        var usuario = await _provider.ObterUsuarioPorIdAsync(id, ct)
            ?? throw new EntidadeNaoEncontradaException(nameof(Usuario), id);

        var email = NormalizarEmail(dto.Email);

        if (await _provider.ExisteComEmailAsync(email, idParaIgnorar: id, ct))
        {
            throw new RegraDeNegocioException($"Já existe um usuário cadastrado com o e-mail '{email}'.");
        }

        usuario.Nome = dto.Nome.Trim();
        usuario.Email = email;

        if (!string.IsNullOrEmpty(dto.Senha))
        {
            usuario.Senha = dto.Senha;
        }

        await _provider.SalvarAlteracoesAsync(ct);

        return ParaDto(usuario);
    }

    public async Task DesativarAsync(int id, CancellationToken ct)
    {
        var usuario = await _provider.ObterUsuarioPorIdAsync(id, ct)
            ?? throw new EntidadeNaoEncontradaException(nameof(Usuario), id);

        usuario.Desativar();

        await _provider.SalvarAlteracoesAsync(ct);
    }

    private static string NormalizarEmail(string email) => email.Trim().ToLowerInvariant();

    private static UsuarioDto ParaDto(Usuario usuario) =>
        new(usuario.Id, usuario.Nome, usuario.Email, usuario.Ativo);
}
