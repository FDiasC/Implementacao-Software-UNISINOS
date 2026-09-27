using System.ComponentModel.DataAnnotations;

namespace GestaoReservas.Domain.Dtos.Usuarios;

/// <summary>Dados para cadastrar um usuário.</summary>
/// <param name="Nome">Nome completo (até 150 caracteres).</param>
/// <param name="Email">E-mail válido e único (até 200 caracteres).</param>
/// <param name="Senha">Senha do usuário (de 4 a 100 caracteres). Armazenada sem criptografia.</param>
public record CriarUsuarioDto(
    [Required(ErrorMessage = "O campo {0} é obrigatório.")]
    [MaxLength(150, ErrorMessage = "O campo {0} deve ter no máximo {1} caracteres.")]
    string Nome,
    [Required(ErrorMessage = "O campo {0} é obrigatório.")]
    [EmailAddress(ErrorMessage = "O campo {0} não é um e-mail válido.")]
    [MaxLength(200, ErrorMessage = "O campo {0} deve ter no máximo {1} caracteres.")]
    string Email,
    [Required(ErrorMessage = "O campo {0} é obrigatório.")]
    [StringLength(100, MinimumLength = 4, ErrorMessage = "O campo {0} deve ter entre {2} e {1} caracteres.")]
    string Senha);
