using System.ComponentModel.DataAnnotations;

namespace GestaoReservas.Domain.Dtos.Auth;

/// <summary>Credenciais para solicitar um token de acesso.</summary>
/// <param name="Email">E-mail do usuário cadastrado.</param>
/// <param name="Senha">Senha do usuário.</param>
public record LoginDto(
    [Required(ErrorMessage = "O campo {0} é obrigatório.")]
    [EmailAddress(ErrorMessage = "O campo {0} não é um e-mail válido.")]
    string Email,
    [Required(ErrorMessage = "O campo {0} é obrigatório.")]
    string Senha);
