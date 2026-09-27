using System.ComponentModel.DataAnnotations;

namespace GestaoReservas.Domain.Dtos.Recursos;

/// <summary>Dados para cadastrar um recurso.</summary>
/// <param name="NumeroPatrimonio">Número de patrimônio, único no sistema (até 50 caracteres).</param>
/// <param name="Descricao">Descrição do recurso (até 200 caracteres).</param>
/// <param name="DiasMinimosReserva">Duração mínima, em dias, de uma reserva do recurso (mínimo 1).</param>
/// <param name="DiasMaximosReserva">Duração máxima, em dias, de uma reserva do recurso (mínimo 1; não pode ser menor que o mínimo).</param>
/// <param name="CategoriaId">Categoria existente, ativa e do tipo Recurso.</param>
public record CriarRecursoDto(
    [Required(ErrorMessage = "O campo {0} é obrigatório.")]
    [MaxLength(50, ErrorMessage = "O campo {0} deve ter no máximo {1} caracteres.")]
    string NumeroPatrimonio,
    [Required(ErrorMessage = "O campo {0} é obrigatório.")]
    [MaxLength(200, ErrorMessage = "O campo {0} deve ter no máximo {1} caracteres.")]
    string Descricao,
    [Range(1, int.MaxValue, ErrorMessage = "O campo {0} deve ser maior ou igual a {1}.")]
    int DiasMinimosReserva,
    [Range(1, int.MaxValue, ErrorMessage = "O campo {0} deve ser maior ou igual a {1}.")]
    int DiasMaximosReserva,
    [Range(1, int.MaxValue, ErrorMessage = "O campo {0} deve ser um id válido.")]
    int CategoriaId);
