using System.ComponentModel.DataAnnotations;
using GestaoReservas.Domain.Enums;

namespace GestaoReservas.WebApi.Dtos.Categorias;

/// <summary>Dados para cadastrar uma categoria.</summary>
/// <param name="Nome">Nome da categoria (até 100 caracteres).</param>
/// <param name="Tipo">Local ou Recurso.</param>
public record CriarCategoriaDto(
    [Required, MaxLength(100)] string Nome,
    [EnumDataType(typeof(TipoCategoria))] TipoCategoria Tipo);
