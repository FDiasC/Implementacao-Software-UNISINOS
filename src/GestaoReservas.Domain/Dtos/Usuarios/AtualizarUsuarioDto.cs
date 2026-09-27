using System.ComponentModel.DataAnnotations;

namespace GestaoReservas.Domain.Dtos.Usuarios;

/// <summary>Dados para atualizar um usuário existente.</summary>
/// <param name="Nome">Nome completo (até 150 caracteres).</param>
/// <param name="Email">E-mail válido e único (até 200 caracteres).</param>
/// <param name="Senha">Nova senha (de 4 a 100 caracteres). Opcional: quando omitida, a senha atual é mantida.</param>
public record AtualizarUsuarioDto(
    [Required(ErrorMessage = "O campo {0} é obrigatório.")]
    [MaxLength(150, ErrorMessage = "O campo {0} deve ter no máximo {1} caracteres.")]
    string Nome,
    [Required(ErrorMessage = "O campo {0} é obrigatório.")]
    [EmailAddress(ErrorMessage = "O campo {0} não é um e-mail válido.")]
    [MaxLength(200, ErrorMessage = "O campo {0} deve ter no máximo {1} caracteres.")]
    string Email,
    [StringLength(100, MinimumLength = 4, ErrorMessage = "O campo {0} deve ter entre {2} e {1} caracteres.")]
    string? Senha = null);
