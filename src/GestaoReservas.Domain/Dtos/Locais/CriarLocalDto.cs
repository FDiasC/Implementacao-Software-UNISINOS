using System.ComponentModel.DataAnnotations;

namespace GestaoReservas.Domain.Dtos.Locais;

/// <summary>Dados para cadastrar um local.</summary>
/// <param name="Sala">Identificação da sala (até 100 caracteres).</param>
/// <param name="Predio">Prédio onde a sala fica (até 100 caracteres).</param>
/// <param name="Capacidade">Número máximo de pessoas (mínimo 1).</param>
/// <param name="Andar">Andar do prédio (0 ou mais).</param>
/// <param name="PermiteRecursos">Se reservas deste local podem incluir recursos.</param>
/// <param name="CategoriaId">Categoria existente, ativa e do tipo Local.</param>
public record CriarLocalDto(
    [Required(ErrorMessage = "O campo {0} é obrigatório.")]
    [MaxLength(100, ErrorMessage = "O campo {0} deve ter no máximo {1} caracteres.")]
    string Sala,
    [Required(ErrorMessage = "O campo {0} é obrigatório.")]
    [MaxLength(100, ErrorMessage = "O campo {0} deve ter no máximo {1} caracteres.")]
    string Predio,
    [Range(1, int.MaxValue, ErrorMessage = "O campo {0} deve ser maior ou igual a {1}.")]
    int Capacidade,
    [Range(0, int.MaxValue, ErrorMessage = "O campo {0} deve ser maior ou igual a {1}.")]
    int Andar,
    bool PermiteRecursos,
    [Range(1, int.MaxValue, ErrorMessage = "O campo {0} deve ser um id válido.")]
    int CategoriaId);
