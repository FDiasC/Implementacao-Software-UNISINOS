using System.ComponentModel.DataAnnotations;

namespace GestaoReservas.Domain.Dtos.Usuarios;

/// <summary>Dados para atualizar um usuário existente.</summary>
/// <param name="Nome">Nome completo (até 150 caracteres).</param>
/// <param name="Email">E-mail válido e único (até 200 caracteres).</param>
/// <param name="Senha">Nova senha (de 4 a 100 caracteres). Opcional: quando omitida, a senha atual é mantida.</param>
public record AtualizarUsuarioDto(
    [Required, MaxLength(150)] string Nome,
    [Required, EmailAddress, MaxLength(200)] string Email,
    [StringLength(100, MinimumLength = 4)] string? Senha = null);
