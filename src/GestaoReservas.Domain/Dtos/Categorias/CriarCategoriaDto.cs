using System.ComponentModel.DataAnnotations;
using GestaoReservas.Domain.Enums;

namespace GestaoReservas.Domain.Dtos.Categorias;

/// <summary>Dados para cadastrar uma categoria.</summary>
/// <param name="Nome">Nome da categoria (até 100 caracteres).</param>
/// <param name="Tipo">Local ou Recurso.</param>
public record CriarCategoriaDto(
    [Required(ErrorMessage = "O campo {0} é obrigatório.")]
    [MaxLength(100, ErrorMessage = "O campo {0} deve ter no máximo {1} caracteres.")]
    string Nome,
    [EnumDataType(typeof(TipoCategoria), ErrorMessage = "O campo {0} deve ser \"Local\" ou \"Recurso\".")]
    TipoCategoria Tipo);
