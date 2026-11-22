using System.ComponentModel.DataAnnotations;

namespace GestaoReservas.Domain.Dtos.Auth;

/// <summary>Credenciais para solicitar um token de acesso.</summary>
public record LoginDto(
    [Required(ErrorMessage = "O campo {0} é obrigatório.")]
    [EmailAddress(ErrorMessage = "O campo {0} não é um e-mail válido.")]
    string Email,
    [Required(ErrorMessage = "O campo {0} é obrigatório.")]
    string Senha);
