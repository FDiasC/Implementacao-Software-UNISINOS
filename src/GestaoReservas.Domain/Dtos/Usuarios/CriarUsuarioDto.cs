using System.ComponentModel.DataAnnotations;

namespace GestaoReservas.Domain.Dtos.Usuarios;

/// <summary>Dados para cadastrar um usuário.</summary>
/// <param name="Nome">Nome completo (até 150 caracteres).</param>
/// <param name="Email">E-mail válido e único (até 200 caracteres).</param>
/// <param name="Senha">Senha do usuário (de 4 a 100 caracteres). Armazenada sem criptografia.</param>
public record CriarUsuarioDto(
    [Required, MaxLength(150)] string Nome,
    [Required, EmailAddress, MaxLength(200)] string Email,
    [Required, StringLength(100, MinimumLength = 4)] string Senha);
